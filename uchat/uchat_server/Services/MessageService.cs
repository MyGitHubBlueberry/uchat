using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using SharedLibrary.Models;
using SharedLibrary.Extensions;
using uchat_server.Files;

namespace uchat_server.Services
{
    public class MessageService(AppDbContext db, IConfiguration configuration) : IMessageService
    {
        private readonly byte[] _masterKey = Convert.FromBase64String(configuration["MasterKey"]
                                             ?? throw new Exception("MasterKey is missing in config")
        );

        private readonly string _attachmentFolder = "Attachments";

        private async Task<byte[]> GetChatKeyAsync(int chatId)
        {

            var chat = await db.Chats.FindAsync(chatId);
            if (chat == null) throw new Exception("Chat not found");

            var keyPackage = new EncryptedMessage(chat.EncryptedKey, chat.KeyIV);

            string decryptedKey = keyPackage.Decrypt(_masterKey);

            return Convert.FromBase64String(decryptedKey);
        }

        public async Task<List<DbMessage>> GetChatMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var skip = (pageNumber - 1) * pageSize;

            return await db.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.TimeSent)
            .ThenBy(m => m.Id)
            .Include(m => m.Attachments)
            .Include(m => m.Sender)
            .ThenInclude(sm => sm.User)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        }

        public async Task<List<Message>> GetChatMessagesDtoAsync(int chatId, int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var skip = (pageNumber - 1) * pageSize;

            byte[] chatKey = await GetChatKeyAsync(chatId);

            return await db.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.TimeSent)
                .Include(m => m.Sender).ThenInclude(s => s.User)
                .Include(m => m.Attachments)
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new Message
                {
                    Id = m.Id,
                    Content = new EncryptedMessage(
                                    m.CipheredText,
                                    m.Iv
                               ).Decrypt(chatKey),
                    ChatId = m.ChatId,
                    SenderName = m.Sender != null && m.Sender.User != null ? m.Sender.User.Name : string.Empty,
                    Timestamp = m.TimeSent,
                    LastEdited = m.TimeEdited,
                    IsEdited = m.TimeEdited.HasValue,
                    Attachments = m.Attachments != null ? m.Attachments.Select(a => new Attachment
                    {
                        Id = a.Id,
                        Url = a.Url
                    }).ToList() : null
                })
                .ToListAsync();
        }

        public async Task SaveMessageAsync(Message msg, List<IFormFile>? files = null)
        {
            DbChat? dbChat = await db.Chats.FindAsync(msg.ChatId);
            if (dbChat is null)
            {
                throw new Exception($"Can't send message in chat with id '{msg.ChatId}', that doesn't exist.");
            }

            byte[] chatKey = await GetChatKeyAsync(msg.ChatId);

            DbUser user = await db.Users
                .Where(u => u.Name == msg.SenderName)
                .FirstAsync();

            DbChatMember? sender = await db.ChatMembers.FindAsync(user.Id, msg.ChatId);
            if (sender is null)
            {
                sender = new DbChatMember
                {
                    UserId = user.Id,
                    User = user,
                    ChatId = msg.ChatId,
                    Chat = dbChat
                };
                await db.ChatMembers.AddAsync(sender);
            }

            (byte[] text, byte[] iv) = msg.Content.Encrypt(chatKey);
            DbMessage dbMsg = new()
            {
                CipheredText = text,
                Iv = iv,
                TimeSent = msg.Timestamp,
                ChatId = msg.ChatId,
                Chat = dbChat,
                SenderId = user.Id,
                Sender = sender,
            };

            if (files != null && files.Count > 0)
            {
                dbMsg.Attachments = new List<DbAttachment>();

                foreach (var file in files)
                {
                    string uniqueFileName = await FileManager.Save(file, _attachmentFolder);

                    dbMsg.Attachments.Add(new DbAttachment
                    {
                        Url = uniqueFileName,
                        Message = dbMsg
                    });
                }
            }

            await db.Messages.AddAsync(dbMsg);
            await db.SaveChangesAsync();

            msg.Id = dbMsg.Id;
            
            if (dbMsg.Attachments != null && dbMsg.Attachments.Count > 0)
            {
                msg.Attachments = [.. dbMsg.Attachments.Select(a => new Attachment
                {
                    Id = a.Id,
                    Url = a.Url
                })];
            }

            var chatMembers = await db.ChatMembers
                .Where(m => m.ChatId == msg.ChatId)
                .ToListAsync();

            foreach (var member in chatMembers)
            {
                member.LastMessageId = dbMsg.Id;
            }

            await db.SaveChangesAsync();
        }

        public async Task<string> DecryptMessageAsync(DbMessage message, int chatId)
        {
            var chat = await db.Chats.FindAsync(chatId);
            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            byte[] chatKey = await GetChatKeyAsync(chatId);

            var encryptedMessage = new EncryptedMessage(message.CipheredText, message.Iv);
            return encryptedMessage.Decrypt(chatKey);
        }
        
        public async Task<bool> RemoveAttachmentsAsync(int messageId, params int[]? idxes)
        {
            List<DbAttachment> attachments = await db.Attachments
                .Where(a => a.MessageId == messageId)
                .OrderBy(a => a.Id)
                .ToListAsync();
            if (attachments.Count == 0)
                return false;
            if (idxes is null || idxes.Length == 0)
            {
                var urls = attachments.Select(a => a.Url);
                foreach (var url in urls)
                    FileManager.Delete(_attachmentFolder, url);
            }
            else
            {
                attachments = idxes
                    .Select(idx => attachments.ElementAtOrDefault(idx))
                    .OfType<DbAttachment>()
                    .ToList();
                if (attachments.Count == 0)
                    return false;
                foreach (DbAttachment attachment in attachments)
                    FileManager.Delete(_attachmentFolder, attachment.Url);
            }
            db.Attachments.RemoveRange(attachments);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task AddAttachmentsAsync(int messageId, params IFormFile[] files)
        {
            var message = await db.Messages.FindAsync(messageId)
                ?? throw new InvalidDataException("Message not found");
            if (files == null || files.Length == 0)
                return;
            if (message.Attachments is null)
                message.Attachments = new List<DbAttachment>();
            var urls = await Task.WhenAll(files
                .Select(async file =>
                    await FileManager
                    .Save(file, _attachmentFolder)));
            message.Attachments
                .AddRange(urls
                    .Select(url => new DbAttachment
                        {
                            Message = message,
                            Url = url
                        }));
            await db.SaveChangesAsync();
        }

        public async Task<Message> EditMessageAsync(int messageId, string newContent)
        {
            var dbMessage = await db.Messages
                .Include(m => m.Sender)
                .ThenInclude(s => s.User)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == messageId)
                ?? throw new InvalidDataException("Message not found");

            byte[] chatKey = await GetChatKeyAsync(dbMessage.ChatId);

            (byte[] encrypted, byte[] iv) = newContent.Encrypt(chatKey);
            dbMessage.CipheredText = encrypted;
            dbMessage.Iv = iv;
            dbMessage.TimeEdited = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return new Message
            {
                Id = dbMessage.Id,
                Content = newContent,
                ChatId = dbMessage.ChatId,
                SenderName = dbMessage.Sender.User.Name,
                Timestamp = dbMessage.TimeSent,
                LastEdited = dbMessage.TimeEdited,
                IsEdited = true,
                Attachments = dbMessage.Attachments?.Select(a => new Attachment
                {
                    Id = a.Id,
                    Url = a.Url
                }).ToList()
            };
        }

        public async Task<int> DeleteMessageAsync(int messageId)
        {
            var dbMessage = await db.Messages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == messageId)
                ?? throw new InvalidDataException("Message not found");

            int chatId = dbMessage.ChatId;

            if (dbMessage.Attachments != null && dbMessage.Attachments.Count > 0)
            {
                foreach (var attachment in dbMessage.Attachments)
                {
                    FileManager.Delete(_attachmentFolder, attachment.Url);
                }
            }

            db.Messages.Remove(dbMessage);
            await db.SaveChangesAsync();
            
            return chatId;
        }

        public async Task DeleteAllMessagesInChatAsync(int chatId)
        {
            var messages = await db.Messages
                .Where(m => m.ChatId == chatId)
                .Include(m => m.Attachments)
                .ToListAsync();

            if (messages.Count == 0) return;

            foreach (var msg in messages)
            {
                if (msg.Attachments != null && msg.Attachments.Count > 0)
                {
                    foreach (var attachment in msg.Attachments)
                    {
                        FileManager.Delete(_attachmentFolder, attachment.Url);
                    }
                }
            }

            db.Messages.RemoveRange(messages);
            await db.SaveChangesAsync();
        }
    }
}
