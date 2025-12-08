using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public interface IChatService
    {
        Task<int> CreateChatRoomAsync(string chatName);
        // TODO: ADD SECRET KEY PARAMETER
        Task<DbChat> GetChatByIdAsync(int chatId, byte[] key);
        Task<List<DbChat>> GetUserChatsAsync(int userId);
        Task<bool> DeleteChatAsync(int chatId);

        //Task AddChatMemberAsync(int chatId, int userId);
        //Task<bool> RemoveChatMemberAsync(int chatId, int userId);
        //Task<List<DbChatMember>> GetChatMembersAsync(int chatId);
        //Task<bool> IsMemberOfChatAsync(int chatId, int userId);
        //Task<bool> IsOwnerOfChatAsync(int chatId, int userId);
    }
}
