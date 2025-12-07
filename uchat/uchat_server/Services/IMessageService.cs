using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public interface IMessageService
    {
        Task<List<DbMessage>> GetChatMessagesAsync(int chatId, int skip = 0, int take = 50);
    }
}
