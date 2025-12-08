using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Models;
using Microsoft.Extensions.Configuration;
using uchat.Services;
using System.IO;

namespace uchat;

public class ServerClient : IServerClient
{
    private readonly HubConnection _connection;
    private readonly HttpClient _httpClient;
    private readonly IUserSession _userSession;
    private readonly string _serverUrl;
    
    public ServerClient(IConfiguration configuration, IUserSession userSession)
    {
        _userSession = userSession;
        _serverUrl = configuration.GetValue<string>("App:ServerUrl") ?? "http://localhost:5248";
        
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
    
    public async Task UserRegistration(string username, string password)
    {
        // TODO: Implement actual server registration when UserController is ready
        // For now, simulate a successful registration
        await Task.Delay(500);
        
        Console.WriteLine($"Mock registration: {username}");
    }

    public async Task<bool> UserLogin(string username, string password)
    {
        // TODO: Implement actual server authentication when UserController is ready
        // For now, simulate successful login
        await Task.Delay(500);

        var mockUser = new User()
        {
            Name = username,
            Image = null,
            Friends = [],
            Chats = [],
            GroupChats = []
        };
        
        _userSession.CurrentUser = mockUser;
        _userSession.AuthToken = $"mock_token_{Guid.NewGuid()}";
        
        Console.WriteLine($"Mock login successful: {username}");
        return true;
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

    public void RegisterNotificationCallback(Action<Message> onMessageReceived)
    {
        _connection.On<Message>("ReceiveMessage", (msg) =>
        {
            onMessageReceived?.Invoke(msg);
        });
    }

    public async Task UploadProfilePicture(int userId, string filePath)
    {
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        var streamContent = new StreamContent(fileStream);
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        string contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", Path.GetFileName(filePath));

        var response = await _httpClient.PostAsync($"{_serverUrl}/api/user/picture/{userId}", form);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error uploading profile picture");
        }
    }

    public async Task RemoveProfilePicture(int userId)
    {
        var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/user/picture/{userId}");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error removing profile picture");
        }
    }

    public async Task UpdatePassword(int userId, string? currentPassword, string newPassword)
    {
        var request = new 
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        };

        var response = await _httpClient.PutAsJsonAsync($"{_serverUrl}/api/user/password/{userId}", request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error updating password");
        }
    }

    public async Task DeleteAccount(int userId)
    {
        var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/user/{userId}");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error deleting account");
        }
    }
}
