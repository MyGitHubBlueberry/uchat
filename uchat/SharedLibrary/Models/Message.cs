namespace SharedLibrary.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public int ChatId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}