using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SharedLibrary.Models;
using uchat.Services;
using Avalonia.Platform.Storage;
using System.Linq;

namespace uchat.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    private readonly IUserSession _userSession;
    
    public ICommand OpenProfileCommand { get; }
    
    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<MessageViewModel> Messages { get; } = new(); 
    public ObservableCollection<User> SearchResults { get; } = new();
    public ObservableCollection<string> AttachedImages { get; } = new();
    
    [ObservableProperty] private Chat? _selectedChat;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _shouldScrollToBottom;
    [ObservableProperty] private string _searchText = string.Empty;
    
    public MainWindowViewModel(IServerClient serverClient, IUserSession userSession)
    {
        _serverClient = serverClient;
        _userSession = userSession;
        _userName = _userSession.CurrentUser?.Name ?? string.Empty;

        _serverClient.RegisterNotificationCallback(OnMessageReceived);

        Messages.CollectionChanged += OnMessagesCollectionChanged;
        
        OpenProfileCommand = new RelayCommand(OpenProfile);

        _ = InitializeAsync();
    }
    
    public event EventHandler? ProfileRequested;

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
        if ((string.IsNullOrWhiteSpace(MessageText) && AttachedImages.Count == 0) || SelectedChat == null) return;

        var msg = new Message
        {
            Content = MessageText ?? string.Empty,
            SenderName = UserName,
            ChatId = SelectedChat.id,
            Timestamp = DateTime.UtcNow
        };

        MessageText = "";

        await _serverClient.SendMessage(msg, SelectedChat.id, AttachedImages.Count > 0 ? AttachedImages.ToList() : null);
        
        if (AttachedImages.Count > 0)
        {
            AttachedImages.Clear();
        }
    }

    [RelayCommand]
    private async Task AttachImages()
    {
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var mainWindow = topLevel?.MainWindow;

        if (mainWindow == null) return;

        var files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Images",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp"]
                }
            }
        });

        foreach (var file in files)
        {
            if (file.TryGetLocalPath() is string path)
            {
                AttachedImages.Add(path);
            }
        }
    }

    [RelayCommand]
    private void RemoveAttachment(string path)
    {
        AttachedImages.Remove(path);
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
    
    private void OpenProfile()
    {
        ProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task SearchUsers(string partialName)
    {
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(partialName))
        {
            return;
        }

        var users = await _serverClient.SearchUsers(partialName);
        
        foreach (var user in users)
        {
            SearchResults.Add(user);
        }
    }
}
