using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using SharedLibrary.Models;

namespace uchat_server.Services
{
    public class MessageService(AppDbContext db) : IMessageService
    {
        private readonly AppDbContext _db = db;

        public async Task<List<DbMessage>> GetChatMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var skip = (pageNumber - 1) * pageSize;

            return await _db.Messages
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

            return await _db.Messages
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.TimeSent)
                .Include(m => m.Sender).ThenInclude(s => s.User)
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new Message
                {
                    Id = m.Id,
                    Content = m.Text,
                    ChatId = m.ChatId,
                    SenderName = m.Sender != null && m.Sender.User != null ? m.Sender.User.Name : string.Empty,
                    Timestamp = m.TimeSent
                })
                .ToListAsync();
        }

        public Task SaveMessageAsync(Message msg)
        {
            msg.Id = Random.Shared.Next(1, 1000000);
            return Task.CompletedTask;
        }
    }
}
