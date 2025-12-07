namespace uchat_server.Models
{
    public class DbMessage
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public DateTime? TimeEdited { get; set; }
        public int ChatId { get; set; }
        public required DbChat Chat { get; set; }
        public int SenderId { get; set; }
        public required DbUser Sender { get; set; }

        public List<DbAttachment> Attachments { get; set; } = new();
    }
}
