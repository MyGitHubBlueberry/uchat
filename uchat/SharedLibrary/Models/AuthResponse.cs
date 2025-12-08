namespace SharedLibrary.Models
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Token { get; set; }
        public string? ImageUrl { get; set; }
    }
}
