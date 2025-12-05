using System;

namespace uchat.Models;

public record Message(string text) {
    string text = text;
    DateTime timeSent = DateTime.UtcNow; 
    int id; // by db
    int chatId; // also in db to tie messages to chats
    DateTime timeLastEdited; // todo doesn't need to be in record. make it db column
}
