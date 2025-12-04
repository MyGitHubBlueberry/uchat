using System;
using System.Threading.Tasks;
using uchat.Models;

namespace uchat;

public interface IServerClient
{
    Task UserRegistration(string username, string password); //TODO: posible more arguments
    Task<bool> UserLogin(string  username, string password);
    Task SendMessage(string message, int chatId);
    Task EditMessage(string newMessage, int chatId, int messageId);
    Task DeleteMessage(int chatId, int messageId);
    Task GetMessages(int chatId, int messageId); // TODO: get models type of message
    Task<User> GetUserInfo(int chatId, int messageId);
    Task GetChats();
    void RegisterNotificationCallback(Action<Message> msg);
}