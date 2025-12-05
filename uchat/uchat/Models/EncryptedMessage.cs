namespace uchat.Models
{
    public record EncryptedMessage(
            byte[] cipheredText,
            byte[] iv
        );
}
