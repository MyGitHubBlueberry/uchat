using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLibrary.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace uchat.ViewModels;

public partial class GroupChatViewModel : ObservableObject, IChatItemViewModel
{
    private readonly GroupChat _groupChat;
    private readonly int _currentUserId;

    public GroupChat GroupChat => _groupChat;
    
    public int ChatId => _groupChat.id;
    
    public bool IsGroupChat => true;

    public string DisplayName => _groupChat.name;

    public string DisplayInitial => DisplayName.Length > 0 
        ? DisplayName[0].ToString() 
        : "#";

    [ObservableProperty] private string _lastMessagePreview = "No messages yet";
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isOwner;
    
    public bool CanLeave => !IsOwner;
    
    public ICommand? LeaveGroupCommand { get; set; }
    public ICommand? DeleteGroupCommand { get; set; }
    public ICommand? OpenGroupSettingsCommand { get; set; }
    
    public GroupChatViewModel(GroupChat groupChat, int currentUserId)
    {
        _groupChat = groupChat;
        _currentUserId = currentUserId;
        _isOwner = groupChat.owner.Id == currentUserId;
    }

    public void UpdateLastMessage(string messageContent)
    {
        LastMessagePreview = messageContent.Length > 50 
            ? messageContent[..50] + "..." 
            : messageContent;
    }
}
