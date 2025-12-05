using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbGroupChat : DbChat
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PictureUrl { get; set; }
        
        public int OwnerId { get; set; }
        public DbUser Owner { get; set; }
    }

}
