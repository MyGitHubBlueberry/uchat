namespace uchat_server.Database.Models;

public class DbUser
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public required string Password { get; set; }
    public List<DbChatMember>? Chats { get; set; }
}
