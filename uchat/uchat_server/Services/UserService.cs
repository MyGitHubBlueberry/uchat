using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using uchat_server.Database;
using uchat_server.Database.Models;
using uchat_server.Files;
using uchat_server.Models;

namespace uchat_server.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;

        public UserService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await context.Users.AnyAsync(u => u.Name == username);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
        {
            if (await UserExistsAsync(request.Username))
            {
                throw new Exception("Username is already taken.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new DbUser
            {
                Name = request.Username,
                Password = passwordHash,
                ImageUrl = null 
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = newUser.Id,
                Username = newUser.Name,
                Token = GenerateUserToken(), // Change code stub for real function
                ImageUrl = newUser.ImageUrl
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Name == request.Username);

            if (user == null)
            {
                throw new Exception("User not found. Please check your username and try again.");
            }

            bool isPasswordCorrect = false;

            try
            {
                isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            } 
            catch (Exception ex) {
                throw new Exception($"Invalid password hash format. User may need to reset password. " +
                    $"Details: {ex.Message}");
            }

            // Checking password with DIRECT COMPARISON for now
            if (!isPasswordCorrect)
            {
                throw new Exception("Invalid password.");
            }

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Name,
                Token = GenerateUserToken(), // Change code stub for real function
                ImageUrl = user.ImageUrl
            };
        }

        public async Task<SharedLibrary.Models.User> GetUserByIdAsync(int userId)
        {
            var dbUser = await context.Users.FindAsync(userId);

            if (dbUser == null)
            {
                throw new Exception("User not found");
            }

            return new SharedLibrary.Models.User
            {
                Id = dbUser.Id,
                Name = dbUser.Name,
                Image = dbUser.ImageUrl
            };
        }

        public async Task<List<SharedLibrary.Models.User>> GetUserByNameAsync(string partialName)
        {
            if (string.IsNullOrWhiteSpace(partialName))
            {
                return [];
            }

            var dbUsers = await context.Users
                .Where(u => u.Name.StartsWith(partialName)) 
                .Take(10) // Limit to 10 results
                .ToListAsync();

            return [.. dbUsers.Select(u => new SharedLibrary.Models.User
            {
                Id = u.Id,
                Name = u.Name,
                Image = u.ImageUrl 
            })];
        }

        private static string GenerateUserToken()
        {
            // Simple random string for now
            return Guid.NewGuid().ToString();
        }

        public async Task UploadProfilePicture(int userId, IFormFile file)
        {
            DbUser user = await context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("Can't assign avatar to user who doesn't exist");
            await UploadProfilePicture(user, file);
        }

        private async Task UploadProfilePicture(DbUser user, IFormFile file) {
            await RemoveProfilePicture(user);
            user.ImageUrl = await FileManager.Save(file, "ProfilePictures");
            await context.SaveChangesAsync();
        }

        public async Task<bool> RemoveProfilePicture(int userId) {
            DbUser user = await context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("Can't remove avatar from user who doesn't exist");
            return await RemoveProfilePicture(user);
        }

        private async Task<bool> RemoveProfilePicture(DbUser user) {
            string? url = user.ImageUrl;
            user.ImageUrl = null;
            await context.SaveChangesAsync();
            if (url is null) return false;
            return FileManager.Delete("ProfilePictures", url);
        }

        public async Task UpdatePasswordAsync(int userId, UpdatePasswordRequest request)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");
            if (request.CurrentPassword != null)
            {
                bool isCurrentCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);

                if (!isCurrentCorrect)
                {
                    throw new Exception("Current password is incorrect.");
                }
            }
            
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            var userMessages = await context.Messages
                .Where(m => m.SenderId == userId)
                .ToListAsync();
            context.Messages.RemoveRange(userMessages);

            var ownedChats = await context.Chats
                .Where(c => c.OwnerId == userId)
                .ToListAsync();
            context.Chats.RemoveRange(ownedChats);

            var chatMemberships = await context.ChatMembers
                .Where(cm => cm.UserId == userId)
                .ToListAsync();
            context.ChatMembers.RemoveRange(chatMemberships);

            await RemoveProfilePicture(user);

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}
