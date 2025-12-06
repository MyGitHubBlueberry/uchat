using SharedLibrary.Models;

namespace uchat.Services;

public interface IUserSession
{
    User? CurrentUser { get; set; }
    string? AuthToken { get; set; }
    bool IsAuthenticated { get; }
    void Clear();
}

public class UserSession : IUserSession
{
    public User? CurrentUser { get; set; }
    public string? AuthToken { get; set; }
    public bool IsAuthenticated => CurrentUser != null && !string.IsNullOrEmpty(AuthToken);

    public void Clear()
    {
        CurrentUser = null;
        AuthToken = null;
    }
}
