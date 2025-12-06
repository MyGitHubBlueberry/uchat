using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    
    public ObservableCollection<Message> Messages { get; } = new(); 
    
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private string _userName = "User" + new Random().Next(1, 100);
    
    public MainWindowViewModel()
    {
        _serverClient = new ServerClient();

        _serverClient.RegisterNotificationCallback(OnMessageReceived);

        GetChatHistory();
    }
    
    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        var msg = new Message
        {
            Content = MessageText,
            SenderName = UserName,
            ChatId = 1, 
            Timestamp = DateTime.UtcNow
        };

        await _serverClient.SendMessage(msg, 1);

        MessageText = "";
    }
    
    [RelayCommand]
    private async Task GetChatHistory()
    {
        Messages.Clear();
        
        var history = await _serverClient.GetMessages(1);
        
        foreach (var msg in history)
        {
            Messages.Add(msg);
        }
    }
    
    private void OnMessageReceived(Message msg)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Messages.Add(msg);
        });
    }
}
