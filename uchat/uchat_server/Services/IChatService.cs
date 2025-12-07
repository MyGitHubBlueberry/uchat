using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public interface IChatService
    {
        Task<int> CreateChatRoomAsync(string chatName);
        Task<DbChat?> GetChatByIdAsync(int chatId, bool includeMembers = false, bool includeMessages = false);
        Task<List<DbChat>> GetUserChatsAsync(int userId);
        Task<bool> ChatExistsAsync(int chatId);
        Task<bool> DeleteChatAsync(int chatId);
        Task<byte[]> GetChatKeyAsync(int chatId);

        //Task AddChatMemberAsync(int chatId, int userId);
        //Task<bool> RemoveChatMemberAsync(int chatId, int userId);
        //Task<List<DbChatMember>> GetChatMembersAsync(int chatId);
        //Task<bool> IsMemberOfChatAsync(int chatId, int userId);
        //Task<bool> IsOwnerOfChatAsync(int chatId, int userId);
    }
}
