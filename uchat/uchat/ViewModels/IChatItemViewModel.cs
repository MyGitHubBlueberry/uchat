namespace uchat.ViewModels;

public interface IChatItemViewModel
{
    int ChatId { get; }
    string DisplayName { get; }
    string DisplayInitial { get; }
    string LastMessagePreview { get; set; }
    bool IsSelected { get; set; }
    bool IsGroupChat { get; }
    void UpdateLastMessage(string messageContent);
}
