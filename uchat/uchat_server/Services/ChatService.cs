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
            return await context.Chats.AnyAsync(c => c.Id == chatId);
        }

        public async Task<int> CreateChatAsync(int sourceUserId, int targetUserId)
        {
            if (sourceUserId == targetUserId)
                throw new InvalidOperationException("sourceUserId and targetUserId should be different");
            DbUser sourceUser = await context.Users.FindAsync(sourceUserId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");
            DbUser targetUser = await context.Users.FindAsync(targetUserId)
                ?? throw new InvalidOperationException("Can't create chat for user that doesn't exist");

            var chatExists = await context.Chats
                .AnyAsync(c => c.OwnerId == null
                        && c.Members.Any(m => m.UserId == sourceUserId)
                        && c.Members.Any(m => m.UserId == targetUserId));

            if (chatExists)
                throw new InvalidOperationException("Chat already exists");

            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

            var dbChat = new DbChat
            {
                EncryptedKey = secureKeyPackage.cipheredText,
                KeyIV = secureKeyPackage.iv,
                Title = targetUser.Name,
            };
            dbChat.Members.AddRange(
                    new() { UserId = sourceUserId, User = sourceUser },
                    new() { UserId = targetUserId, User = targetUser }
                    );

            await context.Chats.AddAsync(dbChat);
            await context.SaveChangesAsync();
            return dbChat.Id;
        }

        public async Task<int> CreateGroupChatAsync(GroupChatCreateRequest groupChat)
        {
            DbUser owner = await context.Users.FindAsync(groupChat.ownerId)
                ?? throw new Exception("Owner user not found");

            if (!groupChat.participants.Contains(owner.Id))
                groupChat.participants.Add(owner.Id);

            byte[] rawChatKey = Aes.Create().Key;
            string keyAsString = Convert.ToBase64String(rawChatKey);
            EncryptedMessage secureKeyPackage = keyAsString.Encrypt(masterKey);

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

            var dbUsers = await Task.WhenAll(
                    groupChat.participants.Select(async id => 
                        await context.Users.FindAsync(id)
                        ?? throw new InvalidOperationException("Can't add user that doesn't exist")));

            dbChat.Members.AddRange(
                dbUsers
                    .Select(u => new DbChatMember {
                        UserId = u.Id,
                        User = u,
                        ChatId = dbChat.Id,
                        Chat = dbChat,
                        IsAdmin = u.Id == owner.Id,
                    }));

            await context.Chats.AddAsync(dbChat);
            context.SaveChanges();

            return dbChat.Id;
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
            if (dbChat.OwnerId != null)
                throw new InvalidOperationException("This chat is group chat");
            var member = dbChat.Members.FirstOrDefault(m => m.UserId == userId)
                ?? throw new InvalidDataException("User is not in this chat");
            var source = await userService.GetUserByIdAsync(userId);
            var target = await userService.GetUserByIdAsync(dbChat
                    .Members
                    .Where(m => m.UserId != userId)
                    .First()
                    .UserId);

            return new Chat(dbChat.Id, source, target, member.IsMuted, member.IsBlocked);
        }

        public async Task<GroupChat> GetGroupChatByIdAsync(int chatId, int userId)
        {
            DbChat dbChat = await context
                .Chats
                .Include(c => c.Members)
                .Where(c => c.Id == chatId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidDataException("Can't get chat that doesn't exist");
            if (!dbChat.Members.Where(m => m.UserId == userId).Any())
                throw new InvalidDataException("The user is not in the chat");
            if (dbChat.OwnerId == null)
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
