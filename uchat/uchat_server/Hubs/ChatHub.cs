using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Models;
using System.Collections.Concurrent;

namespace uchat_server.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<int, List<string>> _connectedUsers = new();

    private static readonly ConcurrentDictionary<string, int> _connectionToUser = new();

    public async Task SubscribeUser(int userId)
    {
        var connectionId = Context.ConnectionId;

        _connectedUsers.AddOrUpdate(
            userId,
            new List<string> { connectionId },
            (key, existingList) =>
            {
                lock (existingList)
                {
                    if (!existingList.Contains(connectionId))
                    {
                        existingList.Add(connectionId);
                    }
                }
                return existingList;
            });

        _connectionToUser.TryAdd(connectionId, userId);
        
        await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

        Console.WriteLine($"User {userId} subscribed");
    }

    public async Task JoinChat(string chatId, List<int> chatMemberIds)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);

        var tasks = new List<Task>();

        foreach (var memberId in chatMemberIds)
        {
            if (_connectedUsers.TryGetValue(memberId, out var connectionIds))
            {
                List<string> connectionIdsCopy;
                lock (connectionIds)
                {
                    connectionIdsCopy = new List<string>(connectionIds);
                }

                foreach (var connId in connectionIdsCopy)
                {
                    tasks.Add(Groups.AddToGroupAsync(connId, chatId));
                }
            }
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"Client with connection {Context.ConnectionId} joined chat {chatId}");
    }

    public async Task SendMessageToChat(Message message)
    {
        await Clients.Group(message.ChatId.ToString()).SendAsync("ReceiveMessage", message);
        Console.WriteLine($"Client {Context.ConnectionId} sent message to chat {message.ChatId.ToString()}: {message}");
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        Console.WriteLine($"Client {Context.ConnectionId} left chat {chatId}");
    }

    public async Task EditMessage(Message message)
    {
        await Clients.Group(message.ChatId.ToString()).SendAsync("MessageEdited", message);
        Console.WriteLine($"Message {message.Id} edited in chat {message.ChatId}");
    }

    public async Task DeleteMessage(int messageId, int chatId)
    {
        await Clients.Group(chatId.ToString()).SendAsync("MessageDeleted", messageId);
        Console.WriteLine($"Message {messageId} deleted from chat {chatId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        if (_connectionToUser.TryRemove(connectionId, out var userId))
        {
            if (_connectedUsers.TryGetValue(userId, out var connectionIds))
            {
                lock (connectionIds)
                {
                    connectionIds.Remove(connectionId);

                    if (connectionIds.Count == 0)
                    {
                        _connectedUsers.TryRemove(userId, out _);
                    }
                }
            }
        }

        Console.WriteLine($"Connection {connectionId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}
