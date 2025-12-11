using Avalonia.Controls;
using Avalonia.Input;
using uchat.ViewModels;

namespace uchat.Views;

public partial class ProfileWindow : Window
{
    public ProfileWindow()
    {
        InitializeComponent();
        PointerPressed += OnWindowPointerPressed;
    }

    public ProfileWindow(ProfileViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var element = e.Source as Control;
        if (element is not TextBox)
        {
            FocusManager?.ClearFocus();
        }
    }
}
