using System;
using System.Collections.Generic;
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
    
    public ObservableCollection<ChatViewModel> Chats { get; } = new();
    public ObservableCollection<MessageViewModel> Messages { get; } = new(); 
    public ObservableCollection<User> SearchResults { get; } = new();
    public ObservableCollection<string> AttachedImages { get; } = new();
    
    [ObservableProperty] private ChatViewModel? _selectedChat;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _shouldScrollToBottom;
    [ObservableProperty] private string _searchText = string.Empty;

    public string SelectedChatName => SelectedChat?.DisplayName ?? "Select a chat";
    
    public MainWindowViewModel(IServerClient serverClient, IUserSession userSession)
    {
        _serverClient = serverClient;
        _userSession = userSession;
        _userName = _userSession.CurrentUser?.Name ?? string.Empty;

        Messages.CollectionChanged += OnMessagesCollectionChanged;
        _serverClient.OnMessageReceived += OnMessageReceived;
        
        OpenProfileCommand = new RelayCommand(OpenProfile);

        _ = InitializeAsync();
    }
    
    public event EventHandler? ProfileRequested;

    private int _currentPage = 0;
    private const int PageSize = 50;
    private int? _currentChatId;

    partial void OnSelectedChatChanged(ChatViewModel? oldValue, ChatViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
        }
        
        if (newValue != null)
        {
            newValue.IsSelected = true;
        }
        
        OnPropertyChanged(nameof(SelectedChatName));
    }

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
            await SelectChatCommand.ExecuteAsync(Chats[0]);
        }
    }
    
    [RelayCommand]
    private async Task Send()
    {
        if ((string.IsNullOrWhiteSpace(MessageText) && AttachedImages.Count == 0) || SelectedChat == null) return;

        var chatId = SelectedChat.Chat.id;

        if (chatId == -1)
        {
            try
            {
                chatId = await _serverClient.CreateChat(_userSession.CurrentUser!.Id, SelectedChat.Chat.userTo.Id);
            }
            catch (Exception ex) when (ex.Message.Contains("Chat already exists"))
            {
                await LoadChats();
                
                var existingChat = Chats.FirstOrDefault(c => 
                    (c.Chat.userFrom.Id == _userSession.CurrentUser?.Id && c.Chat.userTo.Id == SelectedChat.Chat.userTo.Id) ||
                    (c.Chat.userTo.Id == _userSession.CurrentUser?.Id && c.Chat.userFrom.Id == SelectedChat.Chat.userTo.Id));

                if (existingChat != null)
                {
                    RemoveLocalChats();
                    SelectedChat = existingChat;
                    chatId = existingChat.Chat.id;
                    
                    var memberIds = new List<int> { SelectedChat.Chat.userFrom.Id, SelectedChat.Chat.userTo.Id };
                    await _serverClient.JoinChatGroup(chatId, memberIds);
                    _currentChatId = chatId;
                }
                else
                {
                    throw;
                }
            }
            
            if (chatId != SelectedChat.Chat.id)
            {
                var realChat = new Chat(
                    id: chatId,
                    userFrom: SelectedChat.Chat.userFrom,
                    userTo: SelectedChat.Chat.userTo,
                    muted: false,
                    blocked: false
                );

                var index = Chats.IndexOf(SelectedChat);
                Chats.RemoveAt(index);
                var newChatViewModel = new ChatViewModel(realChat, _userSession.CurrentUser.Id);
                Chats.Insert(index, newChatViewModel);
                SelectedChat = newChatViewModel;

                var memberIds = new List<int> { SelectedChat.Chat.userFrom.Id, SelectedChat.Chat.userTo.Id };
                await _serverClient.JoinChatGroup(chatId, memberIds);
                _currentChatId = chatId;
            }
        }

        var msg = new Message
        {
            Content = MessageText ?? string.Empty,
            SenderName = UserName,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

        MessageText = "";

        await _serverClient.SendMessage(msg, chatId, AttachedImages.Count > 0 ? AttachedImages.ToList() : null);
        
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
            Chats.Add(new ChatViewModel(chat, _userSession.CurrentUser!.Id));
        }
    }
    
    [RelayCommand]
    private async Task GetChatHistory()
    {
        if (SelectedChat == null) return;
        Messages.Clear();

        _currentPage = 1;

        var history = await _serverClient.GetMessages(SelectedChat.Chat.id, _currentPage, PageSize);

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

        var older = await _serverClient.GetMessages(SelectedChat.Chat.id, _currentPage, PageSize);

        for (int i = older.Count - 1; i >= 0; i--)
        {
            Messages.Insert(0, CreateMessageViewModel(older[i]));
        }
    }
    
    private void OnMessageReceived(Message msg)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var chatViewModel = Chats.FirstOrDefault(c => c.Chat.id == msg.ChatId);
            
            if (chatViewModel != null)
            {
                chatViewModel.UpdateLastMessage(msg.Content);
            }
        
            if (SelectedChat != null && msg.ChatId == SelectedChat.Chat.id)
            {
                Messages.Add(CreateMessageViewModel(msg));
            }
            else
            {
                Console.WriteLine($"Received message for chat {msg.ChatId}, but current chat is {SelectedChat?.Chat.id}");
            }
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
            if (user.Id != _userSession.CurrentUser?.Id)
            {
                SearchResults.Add(user);
            }
        }
    }

    [RelayCommand]
    private async Task SelectUser(User user)
    {
        var existingChat = Chats.FirstOrDefault(c => 
            (c.Chat.userFrom.Id == _userSession.CurrentUser?.Id && c.Chat.userTo.Id == user.Id) ||
            (c.Chat.userTo.Id == _userSession.CurrentUser?.Id && c.Chat.userFrom.Id == user.Id));

        if (existingChat != null)
        {
            SelectedChat = existingChat;
        }
        else
        {
            RemoveLocalChats();

            var localChat = new Chat(
                id: -1,
                userFrom: _userSession.CurrentUser!,
                userTo: user,
                muted: false,
                blocked: false
            );

            var chatViewModel = new ChatViewModel(localChat, _userSession.CurrentUser.Id);
            Chats.Insert(0, chatViewModel);
            SelectedChat = chatViewModel;
            Messages.Clear();
        }

        SearchText = string.Empty;
        SearchResults.Clear();
    }

    [RelayCommand]
    private async Task SelectChat(ChatViewModel chatViewModel)
    {
        if (_currentChatId.HasValue)
        {
            await _serverClient.LeaveChatGroup(_currentChatId.Value);
            _currentChatId = null;
        }

        if (chatViewModel.Chat.id == -1)
        {
            SelectedChat = chatViewModel;
            Messages.Clear();
        }
        else
        {
            RemoveLocalChats();
            SelectedChat = chatViewModel;
            await GetChatHistory();
            
            var memberIds = new List<int> { chatViewModel.Chat.userFrom.Id, chatViewModel.Chat.userTo.Id };
            await _serverClient.JoinChatGroup(chatViewModel.Chat.id, memberIds);
            _currentChatId = chatViewModel.Chat.id;
        }
    }

    private void RemoveLocalChats()
    {
        for (int i = Chats.Count - 1; i >= 0; i--)
        {
            if (Chats[i].Chat.id == -1)
            {
                Chats.RemoveAt(i);
            }
        }
    }
}
