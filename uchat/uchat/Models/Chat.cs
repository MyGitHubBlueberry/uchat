using System.Collections.Generic;

namespace uchat.Models;

public record Chat(
        int id,
        User userFrom,
        User userTo,
        bool muted,
        bool blocked
    );

public record GroupChat(
        int id,
        User owner,
        string name,
        bool muted,
        List<User> participants,
        string? picture,
        string? description
    );

