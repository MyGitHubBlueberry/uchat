namespace uchat_server.Models
{
    public class RegisterUserRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
