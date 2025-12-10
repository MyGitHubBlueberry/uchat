using Avalonia.Controls;
using Avalonia.Interactivity;
using SharedLibrary.Models;
using uchat.ViewModels;

namespace uchat.Views;

public partial class ChatSidebar : UserControl
{
    public ChatSidebar()
    {
        InitializeComponent();
    }

    private async void OnChatTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: ChatViewModel chat } &&
            DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SelectChatCommand.ExecuteAsync(chat);
        }
    }

    private async void OnUserResultTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: User user } &&
            DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SelectUserCommand.ExecuteAsync(user);
        }
    }
}
