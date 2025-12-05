using System;
using Avalonia.Controls;
using uchat.ViewModels;

namespace uchat.Views;

public partial class AppWindow : Window, IDisposable
{
    private string? _currentUsername;
    private LoginWindow? _currentLoginView;

    public AppWindow()
    {
        InitializeComponent();
        ShowLoginView();
    }

    private void ShowLoginView()
    {
        _currentLoginView = new LoginWindow();
        _currentLoginView.LoginSuccessful += OnLoginSuccessful;
        ContentArea.Content = _currentLoginView;
        Title = "UCHAT - Login";
        CanResize = false;
    }

    private void OnLoginSuccessful(object? sender, string username)
    {
        _currentUsername = username;
        
        if (_currentLoginView != null)
        {
            _currentLoginView.LoginSuccessful -= OnLoginSuccessful;
            _currentLoginView = null;
        }
        
        ShowMainView();
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
        if (_currentLoginView != null)
        {
            _currentLoginView.LoginSuccessful -= OnLoginSuccessful;
            _currentLoginView = null;
        }
        GC.SuppressFinalize(this);
    }
}
