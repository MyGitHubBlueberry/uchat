using uchat;

namespace uchat_server.Models;

public record NotificationModel(int id, string title, string icon, string text, DateTime time);
