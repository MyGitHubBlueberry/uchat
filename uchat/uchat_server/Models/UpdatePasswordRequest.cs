namespace uchat_server.Models
{
    public class UpdatePasswordRequest
    {
        public string? CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
