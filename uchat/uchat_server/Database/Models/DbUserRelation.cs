using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbUserRelation
    {
        public int Id { get; set; }

        // Two fields for each user for flexibility and convenience
        
        // The one who initiated the action 
        public int SourceUserId { get; set; }
        public DbUser SourceUser { get; set; }

        // The target
        public int TargetUserId { get; set; }
        public DbUser TargetUser { get; set; }

        public bool IsBlocked { get; set; } = false;
        public bool IsFriend { get; set; } = false;
    }
}
