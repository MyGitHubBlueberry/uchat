using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public abstract class DbChat
    {
        public int Id { get; set; }

        public List<DbMessage> Messages { get; set; } = null!;
        public List<DbUserChat> UserSettings { get; set; } = null!;
    }
}
