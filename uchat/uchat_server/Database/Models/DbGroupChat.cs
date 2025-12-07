namespace uchat_server.Models
{
    public class DbGroupChat : DbChat
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        
        public int OwnerId { get; set; }
        public required DbUser Owner { get; set; }
    }
}
