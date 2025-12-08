using SharedLibrary.Models;

namespace uchat_server.Services
{
    public interface IChatService
    {
        Task<int> CreateChatRoomAsync(string chatName);
        // TODO: ADD SECRET KEY PARAMETER
        Task<Chat> GetChatByIdAsync(int chatId, int userId);
        Task<List<Chat>> GetUserChatsAsync(int userId);
        Task<bool> DeleteChatAsync(int chatId);

        //Task AddChatMemberAsync(int chatId, int userId);
        //Task<bool> RemoveChatMemberAsync(int chatId, int userId);
        //Task<List<DbChatMember>> GetChatMembersAsync(int chatId);
        //Task<bool> IsMemberOfChatAsync(int chatId, int userId);
        //Task<bool> IsOwnerOfChatAsync(int chatId, int userId);
    }
}
