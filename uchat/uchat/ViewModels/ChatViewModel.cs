using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Models;
using System.Windows.Input;

namespace uchat.ViewModels;

public partial class ChatViewModel(Chat chat, int currentUserId) : ObservableObject, IChatItemViewModel
{
    private readonly Chat _chat = chat;
    private readonly int _currentUserId = currentUserId;

    public Chat Chat => _chat;
    
    public int ChatId => _chat.id;
    
    public bool IsGroupChat => false;
    
    public bool IsOwner => false;

    public bool CanLeave => false;

    public string DisplayName => _chat.userFrom.Id == _currentUserId 
        ? _chat.userTo.Name 
        : _chat.userFrom.Name;

    public string DisplayInitial => DisplayName.Length > 0 
        ? DisplayName[0].ToString() 
        : "?";

    [ObservableProperty] private string _lastMessagePreview = chat.lastMessagePreview ?? "No messages yet";
    [ObservableProperty] private bool _isSelected;
    
    public ICommand? LeaveGroupCommand { get; set; }
    public ICommand? DeleteGroupCommand { get; set; }
    public ICommand? OpenGroupSettingsCommand { get; set; }

    public void UpdateLastMessage(string messageContent)
    {
        LastMessagePreview = messageContent.Length > 50 
            ? messageContent[..50] + "..." 
            : messageContent;
    }
}
