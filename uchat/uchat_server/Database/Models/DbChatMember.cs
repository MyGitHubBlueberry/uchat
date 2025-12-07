using uchat_server.Models;

namespace uchat_server.Database.Models
{
    public class DbChatMember
    {
        public int UserId { get; set; }
        public DbUser User { get; set; } = null!;

        public int ChatId { get; set; }
        public DbChat Chat { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;
        public bool IsMuted { get; set; } = false;
        public bool IsBlocked { get; set; } = false;

        public int? LastMessageId { get; set; }
        public DbMessage? LastMessage { get; set; }

    }
}
