using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace uchat.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PointerPressed += OnWindowPointerPressed;
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
