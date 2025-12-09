using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using SharedLibrary.Models;
using SharedLibrary.Extensions;

namespace uchat_server.Services
{
    public class MessageService(AppDbContext db, IConfiguration configuration) : IMessageService
    {

        private readonly byte[] _masterKey = Convert.FromBase64String(configuration["MasterKey"]
                                             ?? throw new Exception("MasterKey is missing in config")
        );
        public async Task<List<DbMessage>> GetChatMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var skip = (pageNumber - 1) * pageSize;

            return await db.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.TimeSent)
                .Include(m => m.Attachments)
                .Include(m => m.Sender).ThenInclude(sm => sm.User)
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
                .OrderByDescending(m => m.TimeSent)
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
            if (dbChat is null) {
                throw new Exception("Can't send message in chat, that doesn't exist.");
            }

            DbUser user = await db.Users
                .Where(u => u.Name == msg.SenderName)
                .FirstAsync();

            DbChatMember? sender = await db.ChatMembers.FindAsync(user.Id, msg.ChatId);
            if (sender is null) {
                sender = new DbChatMember {
                    UserId = user.Id,
                    User = user,
                    ChatId = msg.ChatId,
                    Chat = dbChat
                };
                await db.ChatMembers.AddAsync(sender);
            }

            (byte[] text, byte[] iv) = msg.Content.Encrypt(_masterKey);
            DbMessage dbMsg = new DbMessage() {
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
                
                string folder = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("wwwroot", "Attachments"));
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + fileInfo.Extension.Trim();
                    string diskPath = Path.Combine(folder, uniqueFileName);

                    using (var stream = new FileStream(diskPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    dbMsg.Attachments.Add(new DbAttachment
                    {
                        Url = Path.Combine("Attachments", uniqueFileName),
                        Message = dbMsg
                    });
                }
            }

            await db.Messages.AddAsync(dbMsg);
            await db.SaveChangesAsync();
        }
    }
}
