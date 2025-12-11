namespace uchat_server.Models
{
    public class ChatMemberDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsMuted { get; set; }
    }
}
