using Avalonia.Controls;
using Avalonia.Interactivity;

namespace uchat.Views;

public partial class GroupSettingsWindow : Window
{
    public GroupSettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
