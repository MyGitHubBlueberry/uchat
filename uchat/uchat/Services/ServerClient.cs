using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using uchat.Models;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Models;

namespace uchat;

public class ServerClient : IServerClient
{
    private readonly HubConnection _connection;
    private readonly HttpClient _httpClient;
    // FIXME: MAKE IT CONFIGURABLE FROM BINARY!!!
    // WHILE ON WINDOWS IT IS 5000 DEFAULT, ON MACOS IT IS 5248
    // fix it ASAP so no more need to change it manually :)
    private readonly string _serverUrl = "http://localhost:5248";
    
    public ServerClient()
    {
        _httpClient = new HttpClient();

        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/chatHub")
            .WithAutomaticReconnect()
            .Build();

        _ = Task.Run(async () =>
        {
            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("JoinChat", "1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to server: {ex.Message}");
            }
        });
    }
    
    public Task UserRegistration(string username, string password)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UserLogin(string username, string password)
    {
        throw new NotImplementedException();
    }

    public async Task SendMessage(Message mes, int chatId)
    {
        mes.ChatId = chatId;
        
        var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/message", mes);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error sending message");
        }
    }

    public Task EditMessage(string newMessage, int chatId, int messageId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMessage(int chatId, int messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Message>> GetMessages(int chatId)
    {
        var response = _httpClient.GetAsync($"{_serverUrl}/api/message/{chatId}");
        
        if (!response.IsCompleted)
        {
            throw new Exception("Error sending message");
        }

        var result =  await response.Result.Content.ReadFromJsonAsync<List<Message>>();

        return result ?? [];
    }

    public Task<User> GetUserInfo(int chatId, int messageId)
    {
        throw new NotImplementedException();
    }

    public async Task<Chat[]> GetChats()
    {
        var mockChat = new Chat(
            id: 1,
            userFrom: new User(
                name: "You",
                image: null,
                friends: [],
                chats: [],
                groupChats: []
            ),
            userTo: new User(
                name: "Other User",
                image: null,
                friends: [],
                chats: [],
                groupChats: []
            ),
            muted: false,
            blocked: false
        );
        
        await Task.Delay(50);
        
        return [mockChat];
    }

    public void RegisterNotificationCallback(Action<Message> onMessageReceived)
    {
        _connection.On<Message>("ReceiveMessage", (msg) =>
        {
            onMessageReceived?.Invoke(msg);
        });
    }
}
