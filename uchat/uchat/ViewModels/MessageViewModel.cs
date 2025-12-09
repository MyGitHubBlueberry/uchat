using System;
using System.Collections.Generic;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public class MessageViewModel
{
    private readonly Message _message;
    
    public MessageViewModel(Message message, string currentUserName)
    {
        _message = message ?? throw new ArgumentNullException(nameof(message));
        IsFromCurrentUser = _message.SenderName == currentUserName;
    }
    
    public bool IsFromCurrentUser { get; }
    
    public string Content => _message.Content;
    
    public string SenderName => _message.SenderName;
    
    public DateTime Timestamp => _message.Timestamp;
    
    public int Id => _message.Id;
    
    public int ChatId => _message.ChatId;
    
    public List<Attachment>? Attachments => _message.Attachments;
}
