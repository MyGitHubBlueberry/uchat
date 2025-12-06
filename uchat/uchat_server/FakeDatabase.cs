using SharedLibrary.Models;

namespace uchat_server;

public class FakeDatabase
{
    private static List<User> Users { get; } = new();
    private static  Dictionary<int, List<Message>> Messages { get; } = new();

    private static List<Chat> Chats { get; } = new();

    public void SaveMessage(Message msg)
    {
        msg.Id = new Random().Next(1, 1000000);
        
        if (!Messages.ContainsKey(msg.ChatId))
        {
            Messages[msg.ChatId] = new List<Message>();
        }
        
        Messages[msg.ChatId].Add(msg);
        
        Console.WriteLine($"[DB] Saved mes from {msg.SenderName} in chat {msg.ChatId}: {msg.Content}");
    }

    public List<Message> GetMessagesByChat(int chatId)
    {
        return Messages[chatId];
    }

    public void CreateUser(User user)
    {
        user.Id = Users.Count + 1;
        
        Users.Add(user);
        Console.WriteLine($"User created {user.Id}");
    }

    public List<User> GetUsers()
    {
        return Users;
    }
}