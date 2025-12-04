using uchat_server.Models;

namespace uchat_server.Services;

public interface IPushNotificationService
{
    Task Notify(NotificationModel message);
}