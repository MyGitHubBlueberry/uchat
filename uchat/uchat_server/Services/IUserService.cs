using uchat_server.Models;

namespace uchat_server.Services
{
    public interface IUserService
    {
        Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> UserExistsAsync(string username);
        Task<SharedLibrary.Models.User> GetUserByIdAsync(int userId);
        Task<List<SharedLibrary.Models.User>> GetUsersByIdsAsync(IEnumerable<int> userIds);
        Task<List<SharedLibrary.Models.User>> GetUserByNameAsync(string partialName);
        Task UploadProfilePicture(int userId, IFormFile file);
        Task<bool> RemoveProfilePicture(int userId);
        Task UpdatePasswordAsync(int userId, UpdatePasswordRequest request);
        Task DeleteUserAsync(int userId);
        Task BlockUserAsync(int userId, int targetUserId);
        Task UnblockUserAsync(int userId, int targetUserId);
    }
}
