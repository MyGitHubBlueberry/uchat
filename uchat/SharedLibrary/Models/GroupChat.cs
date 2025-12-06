namespace SharedLibrary.Models;

public record GroupChat(int id,
    User owner,
    string name,
    bool muted,
    List<User> participants,
    string? picture,
    string? description);