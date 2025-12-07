namespace uchat_server.Models
{
    public class DbGroupChat : DbChat
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        
        public int OwnerId { get; set; }
        public DbUser Owner { get; set; } = null!;
    }

}
