using SharedLibrary.Models;

namespace uchat_server;

public class FakeDatabase
{
    public static List<Message> Messages { get; } = new();
    
    public void SaveMessage(Message msg)
    {
        msg.Id = Messages.Count + 1; 
        Messages.Add(msg);
        
        Console.WriteLine($"[DB] Saved mes from {msg.SenderName}: {msg.Content}");
    }

    public List<Message> GetMessagesByChat(int chatId)
    {
        return Messages.Where(m => m.ChatId == chatId).ToList();
    }
}