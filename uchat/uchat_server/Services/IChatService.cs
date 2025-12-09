using SharedLibrary.Models;
using uchat_server.Models;

namespace uchat_server.Services
{
    public interface IChatService
    {
        Task<int> CreateChatAsync(int sourceUserId, int targetUserId);
        Task<int> CreateGroupChatAsync(GroupChatCreateRequest groupChat);
        Task<Chat> GetChatByIdAsync(int chatId, int userId);
        Task<GroupChat> GetGroupChatByIdAsync(int chatId, int userId);
        Task<(List<Chat>, List<GroupChat>)> GetUserChatsAsync(int userId);
        Task<bool> DeleteChatAsync(int chatId);

        //Task AddChatMemberAsync(int chatId, int userId);
        //Task<bool> RemoveChatMemberAsync(int chatId, int userId);
        //Task<List<DbChatMember>> GetChatMembersAsync(int chatId);
        //Task<bool> IsMemberOfChatAsync(int chatId, int userId);
        //Task<bool> IsOwnerOfChatAsync(int chatId, int userId);
    }
}
