using System.Windows.Input;

namespace uchat.ViewModels;

public interface IChatItemViewModel
{
    int ChatId { get; }
    string DisplayName { get; }
    string DisplayInitial { get; }
    string LastMessagePreview { get; set; }
    bool IsSelected { get; set; }
    bool IsGroupChat { get; }
    bool IsOwner { get; }
    bool CanLeave { get; }
    ICommand? LeaveGroupCommand { get; set; }
    ICommand? DeleteGroupCommand { get; set; }
    ICommand? OpenGroupSettingsCommand { get; set; }
    void UpdateLastMessage(string messageContent);
}
