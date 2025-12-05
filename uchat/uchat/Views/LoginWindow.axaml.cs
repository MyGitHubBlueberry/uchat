using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using uchat.ViewModels;

namespace uchat.Views;

public partial class LoginWindow : UserControl
{
    public event EventHandler<string>? LoginSuccessful;

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginWindowViewModel();
        PointerPressed += OnControlPointerPressed;
    }
    
    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var element = e.Source as Control;
        if (element is not TextBox)
        {
            TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
        }
    }
    
    private void OnLoginButtonClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as LoginWindowViewModel;
        if (viewModel != null && viewModel.ValidateLogin())
        {
            LoginSuccessful?.Invoke(this, viewModel.Username);
        }
    }
}
