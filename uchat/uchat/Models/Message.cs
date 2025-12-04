namespace uchat.Models;

public record Message {
    int id;
    string text;
    int timeSent;
    int timeLastEdited;
}
