namespace SharedLibrary.Models;

public record User (
        string name,
        string? image,
        List<int> friends,
        List<Chat> chats,
        List<Chat> groupChats
);
