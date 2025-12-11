namespace uchat_server.Database.Models;

public class DbUserRelation
{
    public int Id { get; set; }
    public int SourceUserId { get; set; }
    public DbUser SourceUser { get; set; } = null!;
    public int TargetUserId { get; set; }
    public DbUser TargetUser { get; set; } = null!;

    public bool IsBlocked { get; set; } = false;
    public bool IsFriend { get; set; } = false;
}
