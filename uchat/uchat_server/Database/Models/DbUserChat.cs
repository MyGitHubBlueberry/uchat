using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbUserChat
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public DbUser User { get; set; } = null!;

        public int ChatId { get; set; }
        public DbChat Chat { get; set; } = null!;

        public bool IsMuted { get; set; } = false;
        
        public bool IsAdmin { get; set; } = false;
    }
}
