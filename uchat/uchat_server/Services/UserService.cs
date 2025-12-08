using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using uchat_server.Models;

namespace uchat_server.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext context;

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

            var newUser = new DbUser
            {
                Name = request.Username,
                Password = request.Password,
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
                throw new Exception("User not found.");
            }

            // Checking password with DIRECT COMPARISON for now
            if (user.Password != request.Password)
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
                return new List<SharedLibrary.Models.User>();
            }

            var dbUsers = await context.Users
                .Where(u => u.Name.StartsWith(partialName)) 
                .Take(10) // Limit to 10 results
                .ToListAsync();

            return dbUsers.Select(u => new SharedLibrary.Models.User
            {
                Id = u.Id,
                Name = u.Name,
                Image = u.ImageUrl 
            }).ToList();
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

            string folder = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("wwwroot", "Files"));

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            FileInfo fileInfo = new FileInfo(file.FileName);

            if(fileInfo.Extension.Trim() is not (".png" or ".jpeg")) {
                throw new InvalidDataException($"This file format ({fileInfo.Extension.Trim()}) is not supported");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + fileInfo.Extension.Trim();
            string diskPath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(diskPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ImageUrl = Path.Combine("Files", uniqueFileName);
            await context.SaveChangesAsync();
        }

        public async Task RemoveProfilePicture(int userId) {
            DbUser user = await context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("Can't remove avatar from user who doesn't exist");
            if (user.ImageUrl is null) return;
            string path = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("wwwroot", user.ImageUrl));
            File.Delete(path);
            user.ImageUrl = null;
            await context.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(int userId, UpdatePasswordRequest request)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            if (!(request.CurrentPassword is null) && user.Password != request.CurrentPassword)
            {
                throw new Exception("Current password is incorrect.");
            }

            user.Password = request.NewPassword;
            await context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found.");

            // Check db conf, 
            
            // Clean up avatar file if exists
            await RemoveProfilePicture(userId);

            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}
