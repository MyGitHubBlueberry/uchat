namespace uchat.Models
{
    public class EncryptedMessage
    {
        public byte[] CipheredText { get; set; }
        public byte[] iv { get; set; }
    }
}
