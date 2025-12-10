using SharedLibrary.Models;
using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public interface IMessageService
    {
        Task<List<DbMessage>> GetChatMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 50);
        Task<List<Message>> GetChatMessagesDtoAsync(int chatId, int pageNumber = 1, int pageSize = 50);
        Task SaveMessageAsync(Message msg, List<IFormFile>? files = null);
        Task<string> DecryptMessageAsync(DbMessage message, int chatId);
        Task ChangeMessageTextAsync(int messageId, string text);
        Task<bool> RemoveAttachmentsAsync(int messageId, params int[]? idxes);
        Task AddAttachmentsAsync(int messageId, params IFormFile[] files);
    }
}
