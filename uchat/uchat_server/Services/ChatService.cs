using SharedLibrary.Extensions;
using SharedLibrary.Models;
using System.Security.Cryptography;
using uchat_server.Database;
using uchat_server.Database.Models;
using Microsoft.EntityFrameworkCore;
using uchat_server.Models;
using uchat_server.Files;

namespace uchat_server.Services;

public class ChatService(AppDbContext context, IConfiguration configuration, IUserService userService, IMessageService messageService) : IChatService
{

    private readonly byte[] masterKey = Convert.FromBase64String(configuration["MasterKey"]
            ?? throw new Exception("MasterKey is missing in config")
            );

    private readonly string avatarFolder = Path.Combine("GroupChat", "Avatars");

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
            OwnerId = groupChat.ownerId,
            Owner = owner,
        };

        if (groupChat.picture is not null)
            await UploadAvatarAsync(dbChat, groupChat.picture);

        var dbUsers = await Task.WhenAll(
                groupChat.participants.Select(async id =>
                    await context.Users.FindAsync(id)
                    ?? throw new InvalidOperationException("Can't add user that doesn't exist")));

        dbChat.Members.AddRange(
                dbUsers
                .Select(u => new DbChatMember
                {
                    UserId = u.Id,
                    User = u,
                    ChatId = dbChat.Id,
                    Chat = dbChat,
                    IsAdmin = u.Id == owner.Id,
                }));

        await context.Chats.AddAsync(dbChat);
        await context.SaveChangesAsync();

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
            .ThenInclude(m => m.LastMessage)
            .FirstAsync(c => c.Id == chatId)
            ?? throw new InvalidDataException("Can't get chat that doesn't exist");
        if (dbChat.OwnerId != null)
            throw new InvalidOperationException("This chat is group chat");
        var member = dbChat.Members.First(m => m.UserId == userId)
            ?? throw new InvalidDataException("User is not in this chat");
        var source = await userService.GetUserByIdAsync(userId);
        var target = await userService.GetUserByIdAsync(dbChat
                .Members
                .Where(m => m.UserId != userId)
                .First()
                .UserId);

        string? lastMessagePreview = null;
        if (member.LastMessage != null)
        {
            var decryptedContent = await messageService.DecryptMessageAsync(member.LastMessage, chatId);
            lastMessagePreview = decryptedContent.Length > 50 
                ? decryptedContent[..50] + "..." 
                : decryptedContent;
        }

        return new Chat(dbChat.Id, source, target, member.IsMuted, member.IsBlocked, lastMessagePreview);
    }

    private async Task<(List<User> users, User owner)> GetGroupChatUsersAsync(DbChat dbChat)
    {
        var memberIds = dbChat.Members
            .Select(m => m.UserId)
            .Append(dbChat.OwnerId!.Value)
            .Distinct()
            .ToList();
        
        var users = await userService.GetUsersByIdsAsync(memberIds);
        var owner = users.FirstOrDefault(user => user.Id == dbChat.OwnerId.Value)
            ?? throw new InvalidDataException("Owner not found in members");
        
        return (users, owner);
    }

    public async Task<GroupChat> GetGroupChatByIdAsync(int chatId, int userId)
    {
        DbChat dbChat = await context
            .Chats
            .Include(c => c.Members)
            .Where(c => c.Id == chatId)
            .FirstAsync()
            ?? throw new InvalidDataException("Chat not found");
        var member = dbChat.Members.First(m => m.UserId == userId)
            ?? throw new InvalidDataException("User is not in the chat");
        if (dbChat.OwnerId == null)
            throw new InvalidOperationException("This is not a group chat");

        var (users, owner) = await GetGroupChatUsersAsync(dbChat);

        return new GroupChat(
                dbChat.Id,
                owner,
                dbChat.Title,
                member.IsMuted,
                users,
                dbChat.ImageUrl,
                dbChat.Description
                );
    }

    public async Task<Dictionary<int, GroupChat>> GetGroupChatForAllMembersAsync(int chatId)
    {
        DbChat dbChat = await context
            .Chats
            .Include(c => c.Members)
            .Where(chat => chat.Id == chatId)
            .FirstOrDefaultAsync()
            ?? throw new InvalidDataException("Chat not found");

        if (dbChat.OwnerId == null)
            throw new InvalidOperationException("This is not a group chat");

        var (users, owner) = await GetGroupChatUsersAsync(dbChat);

        var result = new Dictionary<int, GroupChat>();
        foreach (var member in dbChat.Members)
        {
            result[member.UserId] = new GroupChat(
                dbChat.Id,
                owner,
                dbChat.Title,
                member.IsMuted,
                users,
                dbChat.ImageUrl,
                dbChat.Description
            );
        }

        return result;
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

    public async Task AddChatMemberAsync(int chatId, int userId)
    {
        var chat = await context.Chats
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == chatId)
            ?? throw new Exception("Chat not found");

        // Only Group Chats can have new members
        if (chat.OwnerId == null)
        {
            throw new InvalidOperationException("Cannot add members to a Direct Chat.");
        }

        var user = await context.Users.FindAsync(userId)
            ?? throw new Exception("User not found");
        if (chat.Members.Any(m => m.UserId == userId))
        {
            throw new InvalidOperationException("User is already a member of this chat.");
        }
        var newMember = new DbChatMember
        {
            ChatId = chatId,
            UserId = userId,
            IsAdmin = false,
            IsMuted = false,
            IsBlocked = false
        };

        context.ChatMembers.Add(newMember);
        await context.SaveChangesAsync();
    }

    public async Task<bool> RemoveChatMemberAsync(int chatId, int userId)
    {
        var memberToRemove = await context.ChatMembers
            .FirstOrDefaultAsync(m => m.ChatId == chatId && m.UserId == userId);

        if (memberToRemove == null) return false;

        var chat = await context.Chats
            .Include(c => c.Members) 
            .FirstOrDefaultAsync(c => c.Id == chatId);
        
        if (chat == null) return false;

        if (chat.OwnerId == userId)
        {
            var successor = chat.Members
                .Where(m => m.UserId != userId)
                .OrderBy(m => m.UserId) 
                .FirstOrDefault();

            if (successor == null)
            {
                // --- SCENARIO 1: LAST PERSON LEFT ---
                // No successor found. Delete everything.
                
                context.ChatMembers.Remove(memberToRemove);
                context.Chats.Remove(chat);
            }
            else
            {
                // --- SCENARIO 2: TRANSFER OWNERSHIP ---
                chat.OwnerId = successor.UserId;
                successor.IsAdmin = true; 
                context.ChatMembers.Remove(memberToRemove);
            }
        }
        else
        {
            // --- SCENARIO 3: NORMAL MEMBER LEAVING ---
            context.ChatMembers.Remove(memberToRemove);
        }

        await context.SaveChangesAsync();
        return true;
    }

    public async Task UploadAvatarAsync(int chatId, IFormFile file)
    {
        DbChat chat = context.Chats.Find(chatId)
            ?? throw new InvalidDataException("Chat not exist");
        if (chat.OwnerId is null)
            throw new InvalidOperationException("Can't change avator for regular chats");
        await UploadAvatarAsync(chat, file);
    }

    private async Task UploadAvatarAsync(DbChat chat, IFormFile file)
    {
        if (chat.ImageUrl is not null)
            await RemoveGroupChatAvatarAsync(chat);
        chat.ImageUrl = await FileManager.SaveAvatar(file, Path.Combine("GroupChat", "Avatars"));
        await context.SaveChangesAsync();
    }

    public async Task<bool> RemoveGroupChatAvatarAsync(int chatId)
    {
        DbChat chat = context.Chats.Find(chatId)
            ?? throw new InvalidDataException("Chat not exist");
        if (chat.OwnerId is null)
            throw new InvalidOperationException("Can't change avator for regular chats");
        return await RemoveGroupChatAvatarAsync(chat);
    }

    public async Task<bool> RemoveGroupChatAvatarAsync(DbChat chat)
    {
        string? url = chat.ImageUrl;
        chat.ImageUrl = null;
        await context.SaveChangesAsync();
        if (url is null) return false;
        return FileManager.Delete(avatarFolder, url);
    }

    public async Task<List<ChatMemberDto>> GetChatMembersAsync(int chatId)
    {
        bool exists = await context.Chats.AnyAsync(c => c.Id == chatId);
        if (!exists) throw new Exception("Chat not found");

        return await context.ChatMembers
            .Where(m => m.ChatId == chatId)
            .Select(m => new ChatMemberDto
            {
                UserId = m.UserId,
                UserName = m.User.Name, 
                ImageUrl = m.User.ImageUrl,
                IsAdmin = m.IsAdmin,
                IsMuted = m.IsMuted
            })
            .ToListAsync();
    }

    public async Task<bool> IsMemberOfChatAsync(int chatId, int userId)
    {
        return await context.ChatMembers
            .AnyAsync(m => m.ChatId == chatId && m.UserId == userId);
    }

    public async Task<bool> IsOwnerOfChatAsync(int chatId, int userId)
    {
        return await context.Chats
            .AnyAsync(c => c.Id == chatId && c.OwnerId == userId);
    }
}
