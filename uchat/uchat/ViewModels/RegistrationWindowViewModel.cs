using System;
using System.Threading.Tasks;
using System.Windows.Input;
using uchat.Services;
using uchat.Commands;

namespace uchat.ViewModels;

public class RegistrationWindowViewModel : ViewModelBase, IClearNavigationStack
{
    private readonly INavigationService _navigationService;
    private readonly IServerClient _serverClient;
    private readonly IUserSession _userSession;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public RegistrationWindowViewModel(
        INavigationService navigationService,
        IServerClient serverClient,
        IUserSession userSession)
    {
        _navigationService = navigationService;
        _serverClient = serverClient;
        _userSession = userSession;

        RegisterCommand = new RelayCommand(RegisterAsync);
        NavigateToLoginCommand = new RelayCommand(() => _navigationService.NavigateTo<LoginWindowViewModel>());
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
            ErrorMessage = string.Empty;
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            ErrorMessage = string.Empty;
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
            ErrorMessage = string.Empty;
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand NavigateToLoginCommand { get; }

    private async Task RegisterAsync()
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

        if(!PasswordValidation(Password, ConfirmPassword)) return;

        try
        {
            var user = await _serverClient.UserRegistration(Username, Password);
            
            _userSession.CurrentUser = user;
            _navigationService.NavigateTo<MainWindowViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private bool PasswordValidation(string Password, string SecondPassword)
    {
        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters";
            return false;
        }
        if (Password.Length > 50)
        {
            ErrorMessage = "Password is too long";
            return false;
        }

        bool hasDigit = false;
        bool hasLetter = false;

        foreach (char c in Password)
        {
            if (char.IsDigit(c)) hasDigit = true;
            if (char.IsLetter(c)) hasLetter = true;
            if (hasDigit && hasLetter) break;
        }

        if (!hasDigit || !hasLetter)
        {
            ErrorMessage = "Password must contain at least one letter and one digit";
            return false;
        }

        if (Password != SecondPassword)
        {
            ErrorMessage = "Passwords do not match";
            return false;
        }

        return true;
    }
}

