using uchat;

namespace uchat_server.Models;

public record NotificationModel {
    public string title { get; init; };
    public string icon { get; init; } = "Uchat icon"; //todo change type
    public string text { get; init; } = "text"; //todo change type
    public DateTime time { get; init; }
}
