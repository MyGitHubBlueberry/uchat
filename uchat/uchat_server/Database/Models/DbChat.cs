namespace uchat_server.Models
{
    public class DbChat
    {
        public int Id { get; set; }
        public List<DbMessage> Messages { get; set; } = null!;
    }
}
