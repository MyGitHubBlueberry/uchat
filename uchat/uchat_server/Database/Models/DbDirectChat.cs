using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbDirectChat : DbChat
    {
        // UserAId < UserBId
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        
        public DbUser UserA { get; set; } = null!;
        public DbUser UserB { get; set; } = null!;
    }

}
