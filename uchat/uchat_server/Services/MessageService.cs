using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using SharedLibrary.Models;
using SharedLibrary.Extensions;

namespace uchat_server.Services
{
    public class MessageService(AppDbContext db) : IMessageService
    {
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
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new Message
                {
                    Id = m.Id,
                    Content = new EncryptedMessage(
                                    m.CipheredText,
                                    m.Iv
                               ).Decrypt(ServerSecrets.MasterKey),
                    ChatId = m.ChatId,
                    SenderName = m.Sender != null && m.Sender.User != null ? m.Sender.User.Name : string.Empty,
                    Timestamp = m.TimeSent
                })
                .ToListAsync();
        }

        public async Task SaveMessageAsync(Message msg)
        {
            DbChat? dbChat = await db.Chats.FindAsync(msg.ChatId);
            if (dbChat is null) {
                // TODO: maybe create new chat instead
                throw new Exception("Can't send message in chat, that doesn't exist.");
            }

            // Throws exeption if user doesn't exist
            DbUser user = await db.Users
                .Where(u => u.Name == msg.SenderName)
                .FirstAsync();

            DbChatMember? sender = await db.ChatMembers.FindAsync(new {user.Id, msg.ChatId});
            if (sender is null) {
                throw new Exception("Chat doesn't exist");
            }

            (byte[] text, byte[] iv) = msg.Content.Encrypt(ServerSecrets.MasterKey);
            DbMessage dbMsg = new DbMessage() {
                CipheredText = text,
                Iv = iv,
                TimeSent = msg.Timestamp,
                ChatId = msg.ChatId,
                Chat = dbChat,
                SenderId = user.Id,
                Sender = sender,
            };
            
            await db.Messages.AddAsync(dbMsg);
        }
    }
}
