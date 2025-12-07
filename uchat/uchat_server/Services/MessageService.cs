using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;

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
                .Include(m => m.Sender)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
