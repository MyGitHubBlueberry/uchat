using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SharedLibrary.Models;
using uchat.Services;

namespace uchat.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    private readonly IUserSession _userSession;
    
    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<MessageViewModel> Messages { get; } = new(); 
    
    [ObservableProperty] private Chat? _selectedChat;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _shouldScrollToBottom;
    
    public MainWindowViewModel(IServerClient serverClient, IUserSession userSession)
    {
        _serverClient = serverClient;
        _userSession = userSession;
        // TODO: remove mock or make as default username (as it on reddit btw ?)
        _userName = _userSession.CurrentUser?.Name;

        _serverClient.RegisterNotificationCallback(OnMessageReceived);

        Messages.CollectionChanged += OnMessagesCollectionChanged;

        _ = InitializeAsync();
    }

    private int _currentPage = 0;
    private const int PageSize = 50;

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ShouldScrollToBottom = true;
        }
    }

    private MessageViewModel CreateMessageViewModel(Message msg)
    {
        return new MessageViewModel(msg, UserName);
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

        MessageText = "";
        
        await _serverClient.SendMessage(msg, SelectedChat.id);
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

        _currentPage = 1;

        var history = await _serverClient.GetMessages(SelectedChat.id, _currentPage, PageSize);

        foreach (var msg in history)
        {
            Messages.Add(CreateMessageViewModel(msg));
        }

        ShouldScrollToBottom = true;
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        if (SelectedChat == null) return;

        _currentPage++;

        var older = await _serverClient.GetMessages(SelectedChat.id, _currentPage, PageSize);

        for (int i = older.Count - 1; i >= 0; i--)
        {
            Messages.Insert(0, CreateMessageViewModel(older[i]));
        }
    }
    
    private void OnMessageReceived(Message msg)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Messages.Add(CreateMessageViewModel(msg));
        });
    }
}
