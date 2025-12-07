using uchat_server.Models;

namespace uchat_server.Services
{
    public interface IUserService
    {
        Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> UserExistsAsync(string username);
        Task<SharedLibrary.Models.User> GetUserByIdAsync(int userId);
        Task<List<SharedLibrary.Models.User>> GetUserByNameAsync(string partialName);
    }
}
