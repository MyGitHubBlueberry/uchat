using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharedLibrary.Models;

namespace uchat;

public interface IServerClient
{
    Task UserRegistration(string username, string password); //making
    Task<bool> UserLogin(string  username, string password); //making
    Task SendMessage(Message message, int chatId);
    Task EditMessage(string newMessage, int chatId, int messageId);
    Task DeleteMessage(int chatId, int messageId);
    Task<List<Message>> GetMessages(int chatId, int pageNumber = 1, int pageSize = 50);
    Task<User> GetUserInfo(int chatId, int messageId);
    Task<Chat[]> GetChats();
}
