namespace SharedLibrary.Models
{
    public record EncryptedMessage(
            byte[] cipheredText,
            byte[] iv
        );
}
