namespace uchat_server.Database.Models;

public class DbChat
{
    public int Id { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? OwnerId { get; set; }
    public DbUser? Owner { get; set; }

    public List<DbMessage> Messages { get; set; } = null!;
    public List<DbChatMember> Members { get; set; } = null!;

    public byte[] EncryptedKey { get; set; }
    public byte[] KeyIV { get; set; }

    public bool IsGroupChat => OwnerId.HasValue;
}
