using System.Security.Cryptography;
using System.IO;
using uchat.Models;

namespace uchat.Extensions
{
	public static class StringEncryptionMessage
	{
        public static EncryptedMessage Encrypt(this string simpletext, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(simpletext);
                        }

                        return new EncryptedMessage
                        {
                            CipheredText = memoryStream.ToArray(),
                            iv = aes.IV
                        };
                        
                    }
                }
            }
        }

        public static string Decrypt(this EncryptedMessage message, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = message.iv; 

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(message.CipheredText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd(); 
                        }
                    }
                }
            }
        }
    }
}
