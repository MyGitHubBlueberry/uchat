using System.Security.Cryptography;
using uchat_server.Database;
using SharedLibrary.Models;
using SharedLibrary.Extensions;
using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public class ChatService(AppDbContext context) : IChatService
    {
        private async Task<bool> ChatExistsAsync(int chatId)
        {
            return await context.FindAsync(typeof(DbChat), chatId) is not null;
        }

        public async Task<int> CreateChatRoomAsync(string chatName)
        {
            byte[] rawChatKey = Aes.Create().Key;

            string keyAsString = Convert.ToBase64String(rawChatKey);

            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(ServerSecrets.MasterKey);

            var newChat = new DbChat
            {
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Title = chatName,
            };

            await context.Chats.AddAsync(newChat);
            context.SaveChanges();

            return newChat.Id;
        }

        public async Task<bool> DeleteChatAsync(int chatId)
        {
            DbChat? chat = await context.Chats.FindAsync(chatId);
            if (chat is not null) {
                context.Chats.Remove(chat);
                return true;
            } 
            return false;
        }
        
        // ADDED SECRET KEY PARAMETER
        public async Task<DbChat> GetChatByIdAsync(int chatId, byte[] key)
        {
            // VALIDATE KEY WITH GetChatKeyAsync(), THEN RETURN CHAT IF VALID, OTHWERSISE THROW EXCEPTION 403
            DbChat? chat = await context.Chats.FindAsync(chatId);
            if (chat is not null && ValidateChatKeyAsync(chat, key)) {
                return chat;
            }
            throw new Exception(System.Net.HttpStatusCode.Forbidden.ToString());
        }

        private bool ValidateChatKeyAsync(DbChat chat, byte[] key)
        {
            if (chat is null) {
                throw new Exception("Chat not found");
            }

            var encryptedPackage = new EncryptedMessage
            (
                chat.EncryptedKey,
                chat.KeyIV
            );

            string keyAsString = encryptedPackage.Decrypt(ServerSecrets.MasterKey);

            return Convert.FromBase64String(keyAsString) == key;
        }

        public async Task<List<DbChat>> GetUserChatsAsync(int userId)
        {
            DbUser? user = await context.Users.FindAsync(userId);

            if (user is null) {
                throw new Exception("User doesn't exist");
            }

            if (user.Chats is List<DbChatMember> chats) {
                return chats
                    .DefaultIfEmpty()
                    .Where(c => c is not null)
                    .Select(c => c.Chat)
                    .ToList();
            }

            return new List<DbChat>();
        }
    }
}
