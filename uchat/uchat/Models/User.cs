using System.Collections.Generic;
using System;

namespace uchat.Models;

public record User (
        int id,
        string name,
        string? image,
        List<int> friends,
        List<Chat> chats
);
