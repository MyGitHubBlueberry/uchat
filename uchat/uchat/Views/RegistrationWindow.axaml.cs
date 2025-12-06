using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace uchat.Views;

public partial class RegistrationWindow : UserControl
{
    public event EventHandler? NavigateToLogin;

    public RegistrationWindow()
    {
        InitializeComponent();
    }

    private void OnLoginLinkPressed(object? sender, PointerPressedEventArgs e)
    {
        NavigateToLogin?.Invoke(this, EventArgs.Empty);
    }
}
