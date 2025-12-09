using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace uchat;

public interface IServerClient
{
    Task<User> UserRegistration(string username, string password);
    Task<User> UserLogin(string  username, string password);
    Task SendMessage(Message message, int chatId, List<string>? imagePaths = null);
    Task EditMessage(string newMessage, int chatId, int messageId);
    Task DeleteMessage(int chatId, int messageId);
    Task<List<Message>> GetMessages(int chatId, int pageNumber = 1, int pageSize = 50);
    Task<User> GetUserInfo(int chatId, int messageId);
    Task<Chat[]> GetChats();
    //void RegisterNotificationCallback(Action<Message> msg);
    Task UploadProfilePicture(int userId, string filePath);
    Task RemoveProfilePicture(int userId);
    Task UpdatePassword(int userId, string? currentPassword, string newPassword);
    Task<bool> DeleteAccount(int userId);
    Task<List<User>> SearchUsers(string partialName);
}
