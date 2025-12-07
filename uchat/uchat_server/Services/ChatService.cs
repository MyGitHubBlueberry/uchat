using System;
using System.Security.Cryptography;
using SharedLibrary;
using uchat_server.Models;
using uchat_server.Database;
using SharedLibrary.Models;
using SharedLibrary.Extensions;


namespace uchat_server.Services
{
    public class ChatService(AppDbContext context)
    {

        public int CreateChatRoom(string chatName)
        {
            // key to the room
            byte[] rawChatKey = Aes.Create().Key;

            string keyAsString = Convert.ToBase64String(rawChatKey);

            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(ServerSecrets.MasterKey);

            var newChat = new DbChat
            {
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv
            };

            context.Chats.Add(newChat);
            context.SaveChanges();

            return newChat.Id;

        }

        public byte[] GetChatKey(int chatId)
        {
            var chat = context.Chats.Find(chatId);
            if (chat == null) throw new Exception("Chat not found");

            var encryptedPackage = new EncryptedMessage
            (
                chat.EncryptedKey,
                chat.KeyIV
            );

            string keyAsString = encryptedPackage.Decrypt(ServerSecrets.MasterKey);

            return Convert.FromBase64String(keyAsString);
        }
    }
}
