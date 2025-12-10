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
                               ).Decrypt(_masterKey),
                    ChatId = m.ChatId,
                    SenderName = m.Sender != null && m.Sender.User != null ? m.Sender.User.Name : string.Empty,
                    Timestamp = m.TimeSent,
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
                throw new Exception("Can't send message in chat, that doesn't exist.");
            }

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

            (byte[] text, byte[] iv) = msg.Content.Encrypt(_masterKey);
            DbMessage dbMsg = new DbMessage()
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

            var encryptedMessage = new EncryptedMessage(message.CipheredText, message.Iv);
            return encryptedMessage.Decrypt(_masterKey);
        }

        public async Task EditMessage(int messageId, string text)
        {
            var message = await db.Messages.FindAsync(messageId)
                ?? throw new InvalidDataException("Message not found");
            message.TimeEdited = DateTime.UtcNow;
            (byte[] encipted, byte[] iv) = text.Encrypt(_masterKey);
            message.Iv = iv;
            message.CipheredText = encipted;
            await db.SaveChangesAsync();
        }
        
        public async Task RemoveAttachments(int messageId, params int[] idxes)
        {
            DbMessage message = await db.Messages.FindAsync(messageId)
                ?? throw new InvalidDataException("Message not found");
            if (message.Attachments is null)
                return;
            if (idxes is null || idxes.Length == 0)
            {
                var urls = message.Attachments.Select(a => a.Url);
                message.Attachments.Clear();
                foreach (var url in urls)
                    FileManager.Delete(_attachmentFolder, url);
            }
            else
            {
                var attachments = idxes
                    .Select(idx => message.Attachments.ElementAtOrDefault(idx))
                    .OfType<DbAttachment>();
                if (attachments is null)
                    return;
                foreach (DbAttachment attachment in attachments)
                    FileManager.Delete(_attachmentFolder, attachment.Url);
            }
            await db.SaveChangesAsync();
        }

        private async Task AddAttachments(int messageId, params IFormFile[] files)
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
        }
    }
}
