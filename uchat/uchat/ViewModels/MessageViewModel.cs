using System;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public class MessageViewModel
{
    private readonly Message _message;
    
    public MessageViewModel(Message message, string currentUserName, MainWindowViewModel hostViewModel)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
        HostViewModel = hostViewModel ?? throw new ArgumentNullException(nameof(hostViewModel));
        IsFromCurrentUser = _message.SenderName == currentUserName;
        
        AttachmentViewModels = _message.Attachments?
            .Select(a => new AttachmentViewModel(a))
            .ToList() ?? [];
    }
    
    public bool IsFromCurrentUser { get; }
    
    public MainWindowViewModel HostViewModel { get; }
    
    public string Content => _message.Content;
    
    public string SenderName => _message.SenderName;
    
    public DateTime Timestamp => _message.Timestamp;
    
    public int Id => _message.Id;
    
    public int ChatId => _message.ChatId;
    
    public List<Attachment>? Attachments => _message.Attachments;
    
    public List<AttachmentViewModel> AttachmentViewModels { get; }
    
    public bool IsEdited => _message.IsEdited;
    
    public DateTime? LastEdited => _message.LastEdited;
    
    public bool CanEdit => Id > 0 && IsFromCurrentUser;
}
