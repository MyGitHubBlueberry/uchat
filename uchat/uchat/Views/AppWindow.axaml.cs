using System;
using Avalonia.Controls;
using uchat.ViewModels;

namespace uchat.Views;

public partial class AppWindow : Window, IDisposable
{
    private string? _currentUsername;
    private LoginWindow? _currentLoginView;
    private RegistrationWindow? _currentRegistrationView;

    public AppWindow()
    {
        InitializeComponent();
        ShowLoginView();
    }

    private void ShowLoginView()
    {
        CleanupRegistrationView();
        
        _currentLoginView = new LoginWindow();
        _currentLoginView.LoginSuccessful += OnLoginSuccessful;
        _currentLoginView.NavigateToRegister += OnNavigateToRegister;
        ContentArea.Content = _currentLoginView;
        Title = "UCHAT - Login";
        CanResize = false;
    }

    private void ShowRegistrationView()
    {
        CleanupLoginView();
        
        _currentRegistrationView = new RegistrationWindow();
        _currentRegistrationView.NavigateToLogin += OnNavigateToLogin;
        ContentArea.Content = _currentRegistrationView;
        Title = "UCHAT - Register";
        CanResize = false;
    }

    private void OnNavigateToRegister(object? sender, EventArgs e)
    {
        ShowRegistrationView();
    }

    private void OnNavigateToLogin(object? sender, EventArgs e)
    {
        ShowLoginView();
    }

    private void OnLoginSuccessful(object? sender, string username)
    {
        _currentUsername = username;
        CleanupLoginView();
        ShowMainView();
    }

    private void CleanupLoginView()
    {
        if (_currentLoginView != null)
        {
            _currentLoginView.LoginSuccessful -= OnLoginSuccessful;
            _currentLoginView.NavigateToRegister -= OnNavigateToRegister;
            _currentLoginView = null;
        }
    }

    private void CleanupRegistrationView()
    {
        if (_currentRegistrationView != null)
        {
            _currentRegistrationView.NavigateToLogin -= OnNavigateToLogin;
            _currentRegistrationView = null;
        }
    }

    private void ShowMainView()
    {
        var mainView = new MainWindow
        {
            DataContext = new MainWindowViewModel()
        };
        ContentArea.Content = mainView;
        Title = "UCHAT";
        CanResize = true;
        MinWidth = 600;
        MinHeight = 400;
        Width = 1000;
        Height = 600;
    }

    public void Dispose()
    {
        CleanupLoginView();
        CleanupRegistrationView();
        GC.SuppressFinalize(this);
    }
}
