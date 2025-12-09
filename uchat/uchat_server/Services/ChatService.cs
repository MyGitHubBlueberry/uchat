using SharedLibrary.Extensions;
using SharedLibrary.Models;
using System.Security.Cryptography;
using uchat_server.Database;
using uchat_server.Database.Models;
using Microsoft.EntityFrameworkCore;
using uchat_server.Models;

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

        public async Task<int> CreateGroupChatAsync(GroupChatCreateRequest groupChat)
        {
            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

            DbUser owner = await context.Users.FindAsync(groupChat.ownerId)
                ?? throw new Exception("Owner user not found");

            DbChat dbChat = new DbChat
            {
                Title = groupChat.name,
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Description = groupChat.description,
                ImageUrl = groupChat.pictureUrl,
                OwnerId = groupChat.ownerId,
                Owner = owner,
            };

            // TODO: save image if exists

            if (!groupChat.participants.Contains(owner.Id))
                groupChat.participants.Add(owner.Id);

            if (groupChat.participants.Count > 1)
            {
                var dbUsers = await Task.WhenAll(groupChat.participants.Select(async id => 
                        await context.Users.FindAsync(id)
                            ?? throw new InvalidOperationException("Can't add user that doesn't exist")));

                var members = dbUsers
                    .Select(u => new DbChatMember
                            {
                            UserId = u.Id,
                            User = u,
                            ChatId = dbChat.Id,
                            Chat = dbChat,
                            }).ToList();

                members
                    .First(u => u.UserId == groupChat.ownerId)
                    .IsAdmin = true;

                var existingRelations = new HashSet<(int, int)>(
                        (await context.UserRelations
                         .Where(r => groupChat.participants.Contains(r.TargetUserId)
                             && groupChat.participants.Contains(r.SourceUserId)).ToListAsync()
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

        private List<DbUserRelation> GetMissingRelations(DbUser[] dbUsers, HashSet<(int, int)> existingPairs)
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
            var dbChat = await context.Chats
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == chatId)
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Any())
                throw new InvalidDataException("The chat is empty");
            if (!dbChat.Members.Any(m => m.UserId == userId))
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
            var members = await context.ChatMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.Chat)
                .ToListAsync();

            var groupMembers = members.Where(m => m.Chat.OwnerId != null); 
            var directMembers = members.Where(m => m.Chat.OwnerId == null);

            var groupTasks = groupMembers.Select(m => GetGroupChatByIdAsync(m.ChatId, userId));
            var directTasks = directMembers.Select(m => GetChatByIdAsync(m.ChatId, userId));

            var groupChats = await Task.WhenAll(groupTasks);
            var chats = await Task.WhenAll(directTasks);

            return (chats.ToList(), groupChats.ToList());
        }
    }
}
