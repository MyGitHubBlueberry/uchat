namespace SharedLibrary.Models;

public record Chat(
        int id,
        User userFrom,
        User userTo,
        bool muted,
        bool blocked,
        string? lastMessagePreview = null
    );

