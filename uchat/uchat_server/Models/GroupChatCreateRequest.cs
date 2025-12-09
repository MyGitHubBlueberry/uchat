namespace uchat_server.Models;

public record GroupChatCreateRequest(
        string name,
        int ownerId,
        List<int> participants,
        bool muted,
        string? pictureUrl,
        string? description);
