namespace uchat_server.Models
{
    public class DbUser
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public required string Password { get; set; }
    }
}
