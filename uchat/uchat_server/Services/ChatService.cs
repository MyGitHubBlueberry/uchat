using SharedLibrary.Extensions;
using SharedLibrary.Models;
using System.Security.Cryptography;
using uchat_server.Database;
using uchat_server.Database.Models;

//TODO: change this to return Chat or GroupChat

namespace uchat_server.Services
{
    public class ChatService(AppDbContext context, IConfiguration configuration, IUserService userService) : IChatService
    {

        private readonly byte[] masterKey = Convert.FromBase64String(configuration["MasterKey"]
                                             ?? throw new Exception("MasterKey is missing in config")
        );

        private async Task<bool> ChatExistsAsync(int chatId) {
            return await context.FindAsync(typeof(DbChat), chatId) is not null;
        }

        public async Task<int> CreateChatAsync(int creatorId, int userId) {
            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

            DbUser dbCreator = await context.Users.FindAsync(creatorId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");
            DbUser dbUser = await context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");

            var dbChat = new DbChat {
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Title = dbUser.Name,
            };

            await context.Chats.AddAsync(dbChat);
            context.SaveChanges();

            DbUserRelation? relation = context.UserRelations
                .Where(r => r.SourceUserId == creatorId && r.TargetUserId == userId).First();

            if (relation is null) {
                relation = new DbUserRelation {
                    SourceUserId = creatorId,
                    SourceUser = dbCreator,
                    TargetUserId = userId,
                    TargetUser = dbUser,
                };
                await context.UserRelations.AddAsync(relation);
                context.SaveChanges();
            }

            return dbChat.Id;
        }

        public async Task<int> CreateGroupChatAsync(string chatName, int creatorId, string? description, string? imageUrl, params int[] userIds) {
            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

            DbUser dbUser = await context.Users.FindAsync(creatorId) 
                ?? throw new InvalidOperationException("Can't create group chat for user that doesn't exist");
            
            DbChat dbChat = new DbChat
            {
                Title = chatName,
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Description = description ?? null,
                ImageUrl = imageUrl ?? null,
                OwnerId = creatorId,
                Owner = dbUser,
            };

            //add users as members
            //add user relations
            //add chat members
            //save image

            await context.Chats.AddAsync(dbChat);
            context.SaveChanges();
        }

        public async Task<bool> DeleteChatAsync(int chatId) {
            DbChat? chat = await context.Chats.FindAsync(chatId);
            if (chat is not null) {
                context.Chats.Remove(chat);
                context.SaveChanges();
                return true;
            } 
            return false;
        }
        
        public async Task<Chat> GetChatByIdAsync(int chatId, int userId) {
            var dbChat = await context.Chats.FindAsync(chatId)
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Where(m => m.UserId == userId).Any())
                throw new InvalidDataException("The user is not in the chat");
            if (dbChat.IsGroupChat) 
                throw new InvalidOperationException("This chat is group chat");
            var source = await userService.GetUserByIdAsync(userId);
            var target = await userService.GetUserByIdAsync(dbChat
                    .Members
                    .Where(m => m.UserId != userId)
                    .First()
                    .UserId);

            var member = await context.ChatMembers.FindAsync(source.Id, chatId)
                ?? throw new InvalidOperationException("Can't find the chat");
            
            return new Chat(dbChat.Id, source, target, member.IsMuted, member.IsBlocked);
        }

        public async Task<GroupChat> GetGroupChatsByIdAsync(int chatId, int userId) {
            DbChat dbChat = await context.Chats.FindAsync(chatId)
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Where(m => m.UserId == userId).Any())
                throw new InvalidDataException("The user is not in the chat");
            if (!dbChat.IsGroupChat)
                throw new InvalidOperationException("This chat is not group chat");

            User owner = await userService.GetUserByIdAsync((int) dbChat.OwnerId);

            var member = await context.ChatMembers.FindAsync(userId, chatId)
                ?? throw new InvalidOperationException("Can't find the chat");

            return new GroupChat(
                    dbChat.Id,
                    owner,
                    dbChat.Title,
                    member.IsMuted,
                    (await Task.WhenAll(dbChat.Members.Select(async m => await userService
                        .GetUserByIdAsync(m.UserId)))).ToList(),
                    dbChat.ImageUrl,
                    dbChat.Description
                    );
        }

        public async Task<(List<Chat>, List<GroupChat>)> GetUserChatsAsync(int userId) {
            DbUser? user = await context.Users.FindAsync(userId)
                ?? throw new Exception("User doesn't exist");
            List<Chat> chats = new List<Chat>();
            List<GroupChat> groupChats = new List<GroupChat>();

            if (user.Chats is List<DbChatMember> dbChats) {
                var groupItems = dbChats.Where(c => c.Chat.IsGroupChat);
                var directItems = dbChats.Where(c => !c.Chat.IsGroupChat);

                var groupTasks = groupItems.Select(c => GetGroupChatsByIdAsync(c.ChatId, userId));
                var directTasks = directItems.Select(c => GetChatByIdAsync(c.ChatId, userId));

                var groups = await Task.WhenAll(groupTasks);
                var directs = await Task.WhenAll(directTasks);

                groupChats.AddRange(groups);
                chats.AddRange(directs);
            }

            return (chats, groupChats);
        }
    }
}
