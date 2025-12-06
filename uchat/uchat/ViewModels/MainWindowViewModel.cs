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
    
    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<Message> Messages { get; } = new(); 
    
    [ObservableProperty] private Chat? _selectedChat;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _userName = "User" + new Random().Next(1, 100);
    
    public MainWindowViewModel()
    {
        _serverClient = new ServerClient();

        _serverClient.RegisterNotificationCallback(OnMessageReceived);

        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        await LoadChats();
        
        if (Chats.Count > 0)
        {
            SelectedChat = Chats[0];
            await GetChatHistory();
        }
    }
    
    [RelayCommand]
    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || SelectedChat == null) return;

        var msg = new Message
        {
            Content = MessageText,
            SenderName = UserName,
            ChatId = SelectedChat.id,
            Timestamp = DateTime.UtcNow
        };

        await _serverClient.SendMessage(msg, SelectedChat.id);

        MessageText = "";
    }
    
    [RelayCommand]
    private async Task LoadChats()
    {
        Chats.Clear();
        
        var chats = await _serverClient.GetChats();
        
        foreach (var chat in chats)
        {
            Chats.Add(chat);
        }
    }
    
    [RelayCommand]
    private async Task GetChatHistory()
    {
        if (SelectedChat == null) return;
        
        Messages.Clear();
        
        var history = await _serverClient.GetMessages(SelectedChat.id);
        
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
