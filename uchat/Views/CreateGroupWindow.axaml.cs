using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace uchat.Views;

public partial class CreateGroupWindow : Window
{
    public CreateGroupWindow()
    {
        InitializeComponent();
        PointerPressed += OnWindowPointerPressed;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var element = e.Source as Control;
        if (element is not TextBox)
        {
            this.FocusManager?.ClearFocus();
        }
    }
}
