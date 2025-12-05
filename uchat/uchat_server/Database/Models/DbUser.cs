using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uchat_server.Models
{
    public class dbUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public string PasswordHash { get; set; }
        
        public List<dbUserChat> ChatSettings { get; set; }
    }

    public class dbUserRelation
    {
        public int Id { get; set; }

        // Two fields for each user for flexibility and convenience
        
        // The one who initiated the action 
        public int SourceUserId { get; set; }
        public dbUser SourceUser { get; set; }

        // The target
        public int TargetUserId { get; set; }
        public dbUser TargetUser { get; set; }

        public bool IsBlocked { get; set; } = false;
        public bool IsFriend { get; set; } = false;
    }

    // --- CHATS ---
    public abstract class dbChat
    {
        public int Id { get; set; }

        public List<dbMessage> Messages { get; set; }
        public List<dbUserChat> UserSettings { get; set; }
    }

    public class DirectChat : dbChat
    {
        // UserAId < UserBId
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        
        public dbUser UserA { get; set; }
        public dbUser UserB { get; set; }
    }

    public class GroupChat : dbChat
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? PictureUrl { get; set; }
        
        public int OwnerId { get; set; }
        public dbUser Owner { get; set; }
    }

    // --- USER CHAT SETTINGS ---
    public class dbUserChat
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public dbUser User { get; set; }

        public int ChatId { get; set; }
        public dbChat Chat { get; set; }

        public bool IsMuted { get; set; } = false;
        
        public bool IsAdmin { get; set; } = false;
    }

    // --- MESSAGES ---
    public class dbMessage
    {
        public int Id { get; set; }
        
        public int ChatId { get; set; }
        public dbChat Chat { get; set; }

        public int SenderId { get; set; }
        public dbUser Sender { get; set; }

        public string Text { get; set; } 

        public DateTime TimeSent { get; set; } = DateTime.UtcNow;
        public DateTime? TimeEdited { get; set; }

        public List<dbAttachment> Attachments { get; set; }
    }

    // --- ATTACHMENTS ---
    public class dbAttachment
    {
        public int Id { get; set; }
        
        public int MessageId { get; set; }
        public dbMessage Message { get; set; }

        public string Url { get; set; }
    }
}
