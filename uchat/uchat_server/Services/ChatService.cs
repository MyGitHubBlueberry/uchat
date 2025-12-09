using SharedLibrary.Extensions;
using SharedLibrary.Models;
using System.Security.Cryptography;
using uchat_server.Database;
using uchat_server.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace uchat_server.Services
{
    public class ChatService(AppDbContext context, IConfiguration configuration, IUserService userService) : IChatService
    {

        private readonly byte[] masterKey = Convert.FromBase64String(configuration["MasterKey"]
                                             ?? throw new Exception("MasterKey is missing in config")
        );

        private async Task<bool> ChatExistsAsync(int chatId)
        {
            return await context.FindAsync(typeof(DbChat), chatId) is not null;
        }

        public async Task<int> CreateChatAsync(int sourceUserId, int targetUserId)
        {
            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

            if (sourceUserId == targetUserId)
                throw new InvalidOperationException("sourceUserId and targetUserId should be different");
            DbUser dbSourceUser = await context.Users.FindAsync(sourceUserId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");
            DbUser dbTargetUser = await context.Users.FindAsync(targetUserId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");

            var chatExists = await context.Chats
                .AnyAsync(c => c.OwnerId == null
                        && c.Members.Any(m => m.UserId == sourceUserId)
                        && c.Members.Any(m => m.UserId == targetUserId));

            if (chatExists)
                throw new InvalidOperationException("Chat already exists");

            var dbChat = new DbChat
            {
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Title = dbTargetUser.Name,
            };

            await context.Chats.AddAsync(dbChat);

            if (!context.UserRelations
                .Where(r => r.SourceUserId == sourceUserId 
                    && r.TargetUserId == targetUserId)
                .Any())
            {
                var relation = new DbUserRelation
                {
                    SourceUserId = sourceUserId,
                    SourceUser = dbSourceUser,
                    TargetUserId = targetUserId,
                    TargetUser = dbTargetUser,
                };
                await context.UserRelations.AddAsync(relation);
            }

            if(!context.UserRelations
                .Where(r => r.SourceUserId == targetUserId 
                    && r.TargetUserId == sourceUserId)
                .Any())
            {
                var relation = new DbUserRelation
                {
                    SourceUserId = targetUserId,
                    SourceUser = dbTargetUser,
                    TargetUserId = sourceUserId,
                    TargetUser = dbSourceUser,
                };
                await context.UserRelations.AddAsync(relation);
            }

            var sourceMember = new DbChatMember
            {
                UserId = sourceUserId,
                User = dbSourceUser,
                ChatId = dbChat.Id,
                Chat = dbChat,
            };

            var targetMember = new DbChatMember
            {
                UserId = targetUserId,
                User = dbTargetUser,
                ChatId = dbChat.Id,
                Chat = dbChat,
            };

            await context.ChatMembers.AddRangeAsync(sourceMember, targetMember);

            context.SaveChanges();
            return dbChat.Id;
        }

        public async Task<int> CreateGroupChatAsync(GroupChat groupChat)
        {
            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);


            DbChat dbChat = new DbChat
            {
                Title = groupChat.name,
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Description = groupChat.description,
                ImageUrl = groupChat.picture,
                OwnerId = groupChat.owner.Id,
                Owner = context.Users.Find(groupChat.owner.Id),
            };

            // TODO: save image if exists

            if (groupChat.participants.Count != 0)
            {
                var userIds = groupChat.participants.Select(p => p.Id).ToList();

                var dbUsers = context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .DistinctBy(u => u.Id)
                    .ToList();

                var members = dbUsers.Select(u => new DbChatMember
                {
                    UserId = u.Id,
                    User = u,
                    ChatId = dbChat.Id,
                    Chat = dbChat,
                });

                var existingRelations = new HashSet<(int, int)>(
                        (await context.UserRelations
                            .Where(r => userIds.Contains(r.TargetUserId)
                                    && userIds.Contains(r.SourceUserId)).ToListAsync()
                            ).Select(r => (r.SourceUserId, r.TargetUserId)));

                await context.UserRelations.AddRangeAsync(
                    GetMissingRelations(dbUsers, existingRelations)
                );

                await context.ChatMembers.AddRangeAsync(members);
                dbChat.Members.AddRange(members);
            }

            await context.Chats.AddAsync(dbChat);
            context.SaveChanges();
            
            return dbChat.Id;
        }

        private List<DbUserRelation> GetMissingRelations(List<DbUser> dbUsers, HashSet<(int, int)> existingPairs)
        {
            List<DbUserRelation> newRelations = new List<DbUserRelation>();
            foreach (var source in dbUsers)
            {
                foreach (var target in dbUsers)
                {
                    if (source.Id == target.Id ||
                          existingPairs.Contains((source.Id, target.Id))) continue;

                    newRelations.Add(new DbUserRelation
                    {
                        SourceUserId = source.Id,
                        SourceUser = source,
                        TargetUserId = target.Id,
                        TargetUser = target
                    });

                    existingPairs.Add((source.Id, target.Id));
                }
            }
            return newRelations;
        }

        public async Task<bool> DeleteChatAsync(int chatId)
        {
            DbChat? chat = await context.Chats.FindAsync(chatId);
            if (chat is not null)
            {
                context.Chats.Remove(chat);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<Chat> GetChatByIdAsync(int chatId, int userId)
        {
            var dbChat = await context.Chats.FindAsync(chatId)
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Where(m => m.UserId == userId).Any())
                throw new InvalidDataException("The user is not in the chat");
            if (dbChat.OwnerId != null)
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

        public async Task<GroupChat> GetGroupChatByIdAsync(int chatId, int userId)
        {
            DbChat dbChat = await context.Chats.FindAsync(chatId)
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Where(m => m.UserId == userId).Any())
                throw new InvalidDataException("The user is not in the chat");
            if (dbChat.OwnerId != null)
                throw new InvalidOperationException("This chat is not group chat");

            User owner = await userService.GetUserByIdAsync((int)dbChat.OwnerId);

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

        public async Task<(List<Chat>, List<GroupChat>)> GetUserChatsAsync(int userId)
        {
            DbUser? user = await context.Users.FindAsync(userId)
                ?? throw new Exception("User doesn't exist");
            List<Chat> chats = new List<Chat>();
            List<GroupChat> groupChats = new List<GroupChat>();

            if (user.Chats is List<DbChatMember> dbChats)
            {
                var groupItems = dbChats.Where(c => c.Chat.OwnerId == null);
                var directItems = dbChats.Where(c => c.Chat.OwnerId != null);

                var groupTasks = groupItems.Select(c => GetGroupChatByIdAsync(c.ChatId, userId));
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
