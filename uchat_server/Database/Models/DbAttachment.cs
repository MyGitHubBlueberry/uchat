namespace uchat_server.Database.Models;

public class DbAttachment
{
    public int Id { get; set; }
    
    public int MessageId { get; set; }
    public DbMessage Message { get; set; } = null!;

    public string Url { get; set; } = null!;
}
