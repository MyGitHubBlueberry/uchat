using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using uchat.Services;

namespace uchat;

public class ServerClient : IServerClient
{
    private HubConnection? _connection;
    private readonly HttpClient _httpClient;
    private int? _currentUserId;
    private readonly IUserSession _userSession;
    private readonly string _serverUrl;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    public int? CurrentUserId => _currentUserId;

    public event Action<Message>? OnMessageReceived;
    public event Action? OnDisconnected;

    public ServerClient(IConfiguration configuration, IUserSession userSession)
    {
        _userSession = userSession;
        _serverUrl = configuration.GetValue<string>("App:ServerUrl") ?? "http://localhost:5248";
        
        _httpClient = new HttpClient();

        _httpClient.BaseAddress = new Uri(_serverUrl);
    }

    public async Task<bool> UserLogin(string username, string password)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/user/login?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (loginResponse == null)
            {
                return false;
            }

            _currentUserId = loginResponse.Id;

            await ConnectToHubAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task UserRegistration(string username, string password)
    {
        // TODO: Implement actual server registration when UserController is ready
        // For now, simulate a successful registration
        await Task.Delay(500);

        Console.WriteLine($"Mock registration: {username}");
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
        return await GetMessages(chatId, 1, 50);
    }

    public async Task<List<Message>> GetMessages(int chatId, int pageNumber = 1, int pageSize = 50)
    {
        var url = $"{_serverUrl}/api/message/{chatId}?pageNumber={pageNumber}&pageSize={pageSize}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error fetching messages");
        }

        var result = await response.Content.ReadFromJsonAsync<List<Message>>();

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
            userFrom: new User() {
                Name = "You",
                Image = null,
                Friends = [],
                Chats = [],
                GroupChats = []
            },
            userTo: new User() {
                Name = "Other User",
                Image = null,
                Friends = [],
                Chats = [],
                GroupChats = []
            },
            muted: false,
            blocked: false
        );

        await Task.Delay(50);

        return [mockChat];
    }

    private async Task ConnectToHubAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/chatHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<Message>("ReceiveMessage", (message) =>
        {
            OnMessageReceived?.Invoke(message);
        });

        _connection.Closed += error =>
        {
            Console.WriteLine($"Connection closed: {error?.Message}");
            OnDisconnected?.Invoke();
            return Task.CompletedTask;
        };

        await _connection.StartAsync();

        if (_currentUserId.HasValue)
        {
            await _connection.InvokeAsync("SubscribeUser", _currentUserId.Value);
        }
    }
}
