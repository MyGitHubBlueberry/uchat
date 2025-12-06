using Microsoft.AspNetCore.SignalR;

namespace uchat_server.Hubs;

public class ChatHub : Hub
{
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        Console.WriteLine($"Client {Context.ConnectionId} joined chat {chatId}");
    }
}