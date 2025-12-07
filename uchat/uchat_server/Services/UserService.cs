using Microsoft.EntityFrameworkCore;
using uchat_server.Database;
using uchat_server.Database.Models;
using uchat_server.Models;

namespace uchat_server.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Name == username);
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

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

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
            var user = await _context.Users
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
            var dbUser = await _context.Users.FindAsync(userId);

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

            var dbUsers = await _context.Users
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
    }
}
