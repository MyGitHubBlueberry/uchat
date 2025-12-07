namespace uchat_server.Models
{
    public class DbUserRelation
    {
        public int Id { get; set; }
        public int SourceUserId { get; set; }
        public required DbUser SourceUser { get; set; }
        public int TargetUserId { get; set; }
        public required DbUser TargetUser { get; set; }

        public bool IsBlocked { get; set; } = false;
        public bool IsFriend { get; set; } = false;
    }
}
