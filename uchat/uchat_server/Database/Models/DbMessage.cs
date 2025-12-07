namespace uchat_server.Models
{
    public class DbMessage
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public DateTime? TimeEdited { get; set; }
        public int ChatId { get; set; }
        public DbChat Chat { get; set; } = null!;
        public int SenderId { get; set; }
        public DbUser Sender { get; set; } = null!;

        public List<DbAttachment> Attachments { get; set; } = new();
    }
}
