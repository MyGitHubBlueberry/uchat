using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbAttachment
    {
        public int Id { get; set; }
        
        public int MessageId { get; set; }
        public DbMessage Message { get; set; } = null!;

        public string Url { get; set; } = null!;
    }
}
