using System;

namespace uchat.Models;

public record Message(int id, string text, DateTime timeSent, int userId, int chatId);
