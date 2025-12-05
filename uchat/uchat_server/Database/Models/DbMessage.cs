using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
        public class DbMessage
    {
        public int Id { get; set; }
        
        public int ChatId { get; set; }
        public DbChat Chat { get; set; }

        public int SenderId { get; set; }
        public DbUser Sender { get; set; }

        public string Text { get; set; } 

        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public DateTime? TimeEdited { get; set; }

        public List<DbAttachment> Attachments { get; set; }
    }
}
