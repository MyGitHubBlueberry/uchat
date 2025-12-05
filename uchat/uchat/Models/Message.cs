using System;

namespace uchat.Models;

public record Message(
        int id,
        User user,
        int chatId,
        EncryptedMessage encriptedText,
        DateTime timeSent
    );
