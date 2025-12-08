using Microsoft.Extensions.Configuration;
using SharedLibrary.Extensions;
using SharedLibrary.Models;
using System.Security.Cryptography;
using uchat_server.Database;
using uchat_server.Database.Models;

namespace uchat_server.Services
{
    public class ChatService(AppDbContext context, IConfiguration configuration) : IChatService
    {

        private readonly byte[] _masterKey = Convert.FromBase64String(configuration["MasterKey"]
                                             ?? throw new Exception("MasterKey is missing in config")
        );
        private async Task<bool> ChatExistsAsync(int chatId)
        {
            return await context.FindAsync(typeof(DbChat), chatId) is not null;
        }

        public async Task<int> CreateChatRoomAsync(string chatName)
        {
            byte[] rawChatKey = Aes.Create().Key;

            string keyAsString = Convert.ToBase64String(rawChatKey);

            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(_masterKey);

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
                context.SaveChanges();
                return true;
            } 
            return false;
        }
        
        public async Task<DbChat> GetChatByIdAsync(int chatId, int userId)
        {
            var members = await context.ChatMembers.FindAsync(userId, chatId);
            if (members is not null) {
                return members.Chat;
            }
            throw new Exception("User doesn't have chat acess");
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
