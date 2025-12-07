namespace uchat_server.Models
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Token { get; set; }
        public string? ImageUrl { get; set; }
    }
}
