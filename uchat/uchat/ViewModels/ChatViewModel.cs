using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public partial class ChatViewModel(Chat chat, int currentUserId) : ObservableObject
{
    private readonly Chat _chat = chat;
    private readonly int _currentUserId = currentUserId;

    public Chat Chat => _chat;

    public string DisplayName => _chat.userFrom.Id == _currentUserId 
        ? _chat.userTo.Name 
        : _chat.userFrom.Name;

    public string DisplayInitial => DisplayName.Length > 0 
        ? DisplayName[0].ToString() 
        : "?";

    [ObservableProperty] private string _lastMessagePreview = chat.lastMessagePreview ?? "No messages yet";

    [ObservableProperty] private bool _isSelected;

    public void UpdateLastMessage(string messageContent)
    {
        LastMessagePreview = messageContent.Length > 50 
            ? messageContent[..50] + "..." 
            : messageContent;
    }
}
