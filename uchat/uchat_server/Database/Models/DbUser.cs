using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class DbUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public string PasswordHash { get; set; }
        
        public List<DbUserChat> ChatSettings { get; set; }
    }
}
