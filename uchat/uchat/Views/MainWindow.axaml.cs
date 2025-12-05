using Avalonia.Controls;
using Avalonia.Input;

namespace uchat.Views;

public partial class MainWindow : UserControl
{
    public MainWindow()
    {
        InitializeComponent();
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
}
