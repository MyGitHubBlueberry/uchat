namespace uchat_server.Database.Models;

public class DbChat
{
    public int Id { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? OwnerId { get; set; }
    public DbUser? Owner { get; set; }

    public List<DbMessage> Messages { get; set; } = new List<DbMessage>();
    public List<DbChatMember> Members { get; set; } = new List<DbChatMember>();

    public required byte[] EncryptedKey { get; set; }
    public required byte[] KeyIV { get; set; }

    public bool IsGroupChat => OwnerId.HasValue;
}
