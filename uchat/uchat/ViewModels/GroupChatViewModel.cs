using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public partial class GroupChatViewModel(GroupChat groupChat, int currentUserId) : ObservableObject, IChatItemViewModel
{
    private readonly GroupChat _groupChat = groupChat;
    private readonly int _currentUserId = currentUserId;

    public GroupChat GroupChat => _groupChat;
    
    public int ChatId => _groupChat.id;
    
    public bool IsGroupChat => true;

    public string DisplayName => _groupChat.name;

    public string DisplayInitial => DisplayName.Length > 0 
        ? DisplayName[0].ToString() 
        : "#";

    [ObservableProperty] private string _lastMessagePreview = "No messages yet";

    [ObservableProperty] private bool _isSelected;

    public void UpdateLastMessage(string messageContent)
    {
        LastMessagePreview = messageContent.Length > 50 
            ? messageContent[..50] + "..." 
            : messageContent;
    }
}
