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
        if (sender is Control { DataContext: IChatItemViewModel chat } &&
            DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.SelectChatCommand.ExecuteAsync(chat);
        }
    }

    private void OnUserResultTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: User user } &&
            DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectUserCommand.Execute(user);
        }
    }
}
