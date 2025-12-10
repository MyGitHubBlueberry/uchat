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
    public ICommand OpenCreateGroupCommand { get; }
    
    public ObservableCollection<IChatItemViewModel> Chats { get; } = new();
    public ObservableCollection<MessageViewModel> Messages { get; } = new(); 
    public ObservableCollection<User> SearchResults { get; } = new();
    public ObservableCollection<string> AttachedImages { get; } = new();
    
    [ObservableProperty] private IChatItemViewModel? _selectedChat;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _shouldScrollToBottom;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private MessageViewModel? _editingMessage;
    [ObservableProperty] private string _editText = string.Empty;
    [ObservableProperty] private bool _isReconnecting;

    public string SelectedChatName => SelectedChat?.DisplayName ?? "Select a chat";
    
    public MainWindowViewModel(IServerClient serverClient, IUserSession userSession)
    {
        _serverClient = serverClient;
        _userSession = userSession;
        _userName = _userSession.CurrentUser?.Name ?? string.Empty;

        Messages.CollectionChanged += OnMessagesCollectionChanged;
        _serverClient.OnMessageReceived += OnMessageReceived;
        _serverClient.OnNewChat += OnNewChat;
        _serverClient.OnNewGroupChat += OnNewGroupChat;
        _serverClient.OnMessageEdited += OnMessageEdited;
        _serverClient.OnMessageDeleted += OnMessageDeleted;
        _serverClient.OnReconnecting += OnReconnecting;
        _serverClient.OnReconnected += OnReconnected;

        OpenProfileCommand = new RelayCommand(OpenProfile);
        OpenCreateGroupCommand = new RelayCommand(OpenCreateGroup);

        _ = InitializeAsync();
    }
    
    public event EventHandler? ProfileRequested;

    private int _currentPage = 0;
    private const int PageSize = 50;
    private int? _currentChatId;

    partial void OnSelectedChatChanged(IChatItemViewModel? oldValue, IChatItemViewModel? newValue)
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

    private void OnReconnecting()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsReconnecting = true;
        });
    }

    private void OnReconnected()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsReconnecting = false;
        });
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

        var chatId = SelectedChat.ChatId;

        if (chatId == -1 && SelectedChat is ChatViewModel chatViewModel)
        {
            try
            {
                chatId = await _serverClient.CreateChat(_userSession.CurrentUser!.Id, chatViewModel.Chat.userTo.Id);
            }
            catch (Exception ex) when (ex.Message.Contains("Chat already exists"))
            {
                await LoadChats();
                
                var existingChat = Chats.OfType<ChatViewModel>().FirstOrDefault(c => 
                    (c.Chat.userFrom.Id == _userSession.CurrentUser?.Id && c.Chat.userTo.Id == chatViewModel.Chat.userTo.Id) ||
                    (c.Chat.userTo.Id == _userSession.CurrentUser?.Id && c.Chat.userFrom.Id == chatViewModel.Chat.userTo.Id));

                if (existingChat != null)
                {
                    RemoveLocalChats();
                    SelectedChat = existingChat;
                    chatId = existingChat.ChatId;
                    
                    var memberIds = new List<int> { existingChat.Chat.userFrom.Id, existingChat.Chat.userTo.Id };
                    await _serverClient.JoinChatGroup(chatId, memberIds);
                    _currentChatId = chatId;
                }
                else
                {
                    throw;
                }
            }
            
            if (chatId != SelectedChat.ChatId)
            {
                var realChat = new Chat(
                    id: chatId,
                    userFrom: chatViewModel.Chat.userFrom,
                    userTo: chatViewModel.Chat.userTo,
                    muted: false,
                    blocked: false
                );

                var index = Chats.IndexOf(SelectedChat);
                Chats.RemoveAt(index);
                var newChatViewModel = new ChatViewModel(realChat, _userSession.CurrentUser.Id);
                Chats.Insert(index, newChatViewModel);
                SelectedChat = newChatViewModel;

                var memberIds = new List<int> { newChatViewModel.Chat.userFrom.Id, newChatViewModel.Chat.userTo.Id };
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

        await _serverClient.SendMessage(msg, chatId, AttachedImages.Count > 0 ? [.. AttachedImages] : null);
        
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
        
        var (chats, groupChats) = await _serverClient.GetChats();
        
        foreach (var chat in chats)
        {
            Chats.Add(new ChatViewModel(chat, _userSession.CurrentUser!.Id));
        }
        
        foreach (var groupChat in groupChats)
        {
            Chats.Add(new GroupChatViewModel(groupChat, _userSession.CurrentUser!.Id));
        }
    }
    
    [RelayCommand]
    private async Task GetChatHistory()
    {
        if (SelectedChat == null) return;
        Messages.Clear();

        _currentPage = 1;

        var history = await _serverClient.GetMessages(SelectedChat.ChatId, _currentPage, PageSize);

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

        var older = await _serverClient.GetMessages(SelectedChat.ChatId, _currentPage, PageSize);

        for (int i = older.Count - 1; i >= 0; i--)
        {
            Messages.Insert(0, CreateMessageViewModel(older[i]));
        }
    }
    
    private void OnMessageReceived(Message msg)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var chatViewModel = Chats.FirstOrDefault(c => c.ChatId == msg.ChatId);
            
            if (chatViewModel != null)
            {
                chatViewModel.UpdateLastMessage(msg.Content);
            }
        
            if (SelectedChat != null && msg.ChatId == SelectedChat.ChatId)
            {
                Messages.Add(CreateMessageViewModel(msg));
            }
            else
            {
                Console.WriteLine($"Received message for chat {msg.ChatId}, but current chat is {SelectedChat?.ChatId}");
            }
        });
    }
    
    private void OnNewChat(Chat chat)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!Chats.Any(c => c.ChatId == chat.id))
            {
                Chats.Add(new ChatViewModel(chat, _userSession.CurrentUser!.Id));
            }
        });
    }
    
    private void OnNewGroupChat(GroupChat groupChat)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!Chats.Any(c => c.ChatId == groupChat.id))
            {
                Chats.Add(new GroupChatViewModel(groupChat, _userSession.CurrentUser!.Id));
            }
        });
    }

    private void OnMessageEdited(Message msg)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var messageViewModel = Messages.FirstOrDefault(m => m.Id == msg.Id);
            if (messageViewModel != null)
            {
                var index = Messages.IndexOf(messageViewModel);
                Messages[index] = CreateMessageViewModel(msg);
            }
            
            var chatViewModel = Chats.FirstOrDefault(c => c.ChatId == msg.ChatId);
            if (chatViewModel != null)
            {
                var oldContent = messageViewModel?.Content ?? "";
                var oldPreview = oldContent.Length > 50 ? oldContent[..50] + "..." : oldContent;
                
                if (chatViewModel.LastMessagePreview == oldPreview)
                {
                    chatViewModel.UpdateLastMessage(msg.Content);
                }
            }
        });
    }

    private void OnMessageDeleted(int messageId)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var messageViewModel = Messages.FirstOrDefault(m => m.Id == messageId);
            if (messageViewModel != null)
            {
                var chatId = messageViewModel.ChatId;
                var deletedContent = messageViewModel.Content;
                var deletedPreview = deletedContent.Length > 50 ? deletedContent[..50] + "..." : deletedContent;
                
                Messages.Remove(messageViewModel);
                
                // Update chat preview if the deleted message preview matches current preview
                var chatViewModel = Chats.FirstOrDefault(c => c.ChatId == chatId);
                if (chatViewModel != null && chatViewModel.LastMessagePreview == deletedPreview)
                {
                    var newLastMessage = Messages
                        .Where(m => m.ChatId == chatId)
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefault();
                    
                    if (newLastMessage != null)
                    {
                        chatViewModel.UpdateLastMessage(newLastMessage.Content);
                    }
                    else
                    {
                        try
                        {
                            var messages = await _serverClient.GetMessages(chatId, 1, 1);
                            if (messages.Count > 0)
                            {
                                chatViewModel.UpdateLastMessage(messages[0].Content);
                            }
                            else
                            {
                                chatViewModel.UpdateLastMessage("No messages yet");
                            }
                        }
                        catch
                        {
                            chatViewModel.UpdateLastMessage("No messages yet");
                        }
                    }
                }
            }
        });
    }

    [RelayCommand]
    private void StartEditMessage(MessageViewModel message)
    {
        if (message.Id <= 0)
        {
            Console.WriteLine("Cannot edit message without valid ID");
            return;
        }
        
        EditingMessage = message;
        EditText = message.Content;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        EditingMessage = null;
        EditText = string.Empty;
    }

    [RelayCommand]
    private async Task SaveEdit()
    {
        if (EditingMessage == null || string.IsNullOrWhiteSpace(EditText)) return;

        try
        {
            await _serverClient.EditMessage(EditText, EditingMessage.ChatId, EditingMessage.Id);
            EditingMessage = null;
            EditText = string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error editing message: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteMessage(MessageViewModel message)
    {
        if (message.Id <= 0)
        {
            Console.WriteLine("Cannot delete message without valid ID");
            return;
        }
        
        try
        {
            await _serverClient.DeleteMessage(message.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting message: {ex.Message}");
        }
    }
    
    private void OpenProfile()
    {
        ProfileRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void OpenCreateGroup()
    {
        var chatsOnly = Chats.OfType<ChatViewModel>().Select(c => c.Chat).ToArray();
        var createGroupViewModel = new CreateGroupViewModel(_serverClient, _userSession.CurrentUser!.Id, chatsOnly);
        var createGroupWindow = new Views.CreateGroupWindow
        {
            DataContext = createGroupViewModel
        };
        
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
        var mainWindow = topLevel?.MainWindow;
        
        if (mainWindow != null)
        {
            await createGroupWindow.ShowDialog(mainWindow);
        }
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
    private void SelectUser(User user)
    {
        var existingChat = Chats.OfType<ChatViewModel>().FirstOrDefault(c => 
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
    private async Task SelectChat(IChatItemViewModel chatViewModel)
    {
        if (_currentChatId.HasValue)
        {
            await _serverClient.LeaveChatGroup(_currentChatId.Value);
            _currentChatId = null;
        }

        if (chatViewModel.ChatId == -1)
        {
            SelectedChat = chatViewModel;
            Messages.Clear();
        }
        else
        {
            RemoveLocalChats();
            SelectedChat = chatViewModel;
            await GetChatHistory();
            
            List<int> memberIds;
            if (chatViewModel is ChatViewModel chat)
            {
                memberIds = new List<int> { chat.Chat.userFrom.Id, chat.Chat.userTo.Id };
            }
            else if (chatViewModel is GroupChatViewModel groupChat)
            {
                memberIds = groupChat.GroupChat.participants.Select(p => p.Id).ToList();
            }
            else
            {
                return;
            }
            
            await _serverClient.JoinChatGroup(chatViewModel.ChatId, memberIds);
            _currentChatId = chatViewModel.ChatId;
        }
    }

    private void RemoveLocalChats()
    {
        for (int i = Chats.Count - 1; i >= 0; i--)
        {
            if (Chats[i].ChatId == -1)
            {
                Chats.RemoveAt(i);
            }
        }
    }
}
