using System;
using System.Threading.Tasks;
using System.Windows.Input;
using uchat.Commands;
using uchat.Services;

namespace uchat.ViewModels;

public class LoginWindowViewModel : ViewModelBase, IClearNavigationStack
{
    private readonly INavigationService _navigationService;
    private readonly IServerClient _serverClient;
    private readonly IUserSession _userSession;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoggingIn = false;

    public LoginWindowViewModel(
        INavigationService navigationService,
        IServerClient serverClient,
        IUserSession userSession)
    {
        _navigationService = navigationService;
        _serverClient = serverClient;
        _userSession = userSession;

        LoginCommand = new RelayCommand(LoginAsync, () => !IsLoggingIn);
        NavigateToRegisterCommand = new RelayCommand(() => _navigationService.NavigateTo<RegistrationWindowViewModel>());
    }

    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            ErrorMessage = string.Empty;
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ErrorMessage = string.Empty;
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoggingIn
    {
        get => _isLoggingIn;
        set
        {
            SetProperty(ref _isLoggingIn, value);
            (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required";
            return;
        }

        IsLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            var user = await _serverClient.UserLogin(Username, Password);

            if (user != null)
            {
                _userSession.CurrentUser = user;
                _navigationService.NavigateTo<MainWindowViewModel>();
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}
