using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using uchat.Services;
using System.IO;

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
    public event Action<Chat>? OnNewChat;
    public event Action<GroupChat>? OnNewGroupChat;
    public event Action<Message>? OnMessageEdited;
    public event Action<int>? OnMessageDeleted;
    public event Action? OnDisconnected;

    public ServerClient(IConfiguration configuration, IUserSession userSession)
    {
        var args = Environment.GetCommandLineArgs();

        string? host = null;
        string? port = null;

        // -h <ip> -p <port>
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "-h")
            {
                host = args[i + 1];
            }
            else if (args[i] == "-p")
            {
                port = args[i + 1];
            }
        }

        host ??= Environment.GetEnvironmentVariable("UCHAT_SERVER_HOST", EnvironmentVariableTarget.User) ?? "localhost";
        port ??= Environment.GetEnvironmentVariable("UCHAT_SERVER_PORT", EnvironmentVariableTarget.User) ?? "5000";

        _serverUrl = $"http://{host}:{port}";

        _serverUrl = $"http://localhost:{port}";
        _userSession = userSession;
        
        _httpClient = new HttpClient();

        _httpClient.BaseAddress = new Uri(_serverUrl);
    }

    public async Task<User> UserLogin(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/user/login", new {username, password});

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (loginResponse == null)
        {
            throw new Exception("Invalid server response");
        }

        _currentUserId = loginResponse.UserId;

        await ConnectToHubAsync();

        var user = new User
        {
            Id = loginResponse.UserId,
            Name = loginResponse.Username,
            Image = loginResponse.ImageUrl
        };

        return user;
    }

    public async Task<User> UserRegistration(string username, string password)
    {
        try
        {
            var request = new 
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/user/register", request);

            if(!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception("Registration failed: " + errorContent);
            }

            var registerResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (registerResponse is null)
            {
                throw new Exception("Invalid server response");
            }

            _currentUserId = registerResponse.UserId;
            await ConnectToHubAsync();

            return new User
            {
                Id = registerResponse.UserId,
                Name = registerResponse.Username,
                Image = registerResponse.ImageUrl
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error during user registration: " + ex.Message);
        }
    }

    public async Task SendMessage(Message mes, int chatId, List<string>? imagePaths = null)
    {
        mes.ChatId = chatId;

        using var form = new MultipartFormDataContent();
        
        var messageJson = System.Text.Json.JsonSerializer.Serialize(mes);
        form.Add(new StringContent(messageJson), "messageJson");

        if (imagePaths != null && imagePaths.Count > 0)
        {
            foreach (var imagePath in imagePaths)
            {
                if (File.Exists(imagePath))
                {
                    var fileStream = File.OpenRead(imagePath);
                    var streamContent = new StreamContent(fileStream);
                    string extension = Path.GetExtension(imagePath).ToLowerInvariant();
                    string contentType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".bmp" => "image/bmp",
                        // TODO: REVIEW?
                        _ => "application/octet-stream"
                    };
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    form.Add(streamContent, "files", Path.GetFileName(imagePath));
                }
            }
        }

        var response = await _httpClient.PostAsync($"{_serverUrl}/api/message", form);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error sending message");
        }
    }

    public async Task EditMessage(string newMessage, int chatId, int messageId)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_serverUrl}/api/message/{messageId}", newMessage);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error editing message: {await response.Content.ReadAsStringAsync()}");
        }
    }

    public async Task DeleteMessage(int messageId)
    {
        var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/message/{messageId}");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error deleting message");
        }
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
        if (!_currentUserId.HasValue)
        {
            return [];
        }

        var response = await _httpClient.GetAsync($"{_serverUrl}/api/chat/{_currentUserId.Value}/chats");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"GetChats failed: {response.StatusCode} - {errorContent}");
            return [];
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"GetChats response: {responseContent}");

        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
        
        if (jsonDoc == null)
        {
            return [];
        }

        if (!jsonDoc.RootElement.TryGetProperty("chats", out var chatsElement))
        {
            Console.WriteLine("Response does not contain 'chats' property");
            return [];
        }

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var chats = System.Text.Json.JsonSerializer.Deserialize<Chat[]>(chatsElement.GetRawText(), options);
        return chats ?? [];
    }

    public async Task<int> CreateChat(int sourceUserId, int targetUserId)
    {
        var response = await _httpClient.PostAsync($"{_serverUrl}/api/chat/create/chat/{sourceUserId}-{targetUserId}", null);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error creating chat: {response.StatusCode} - {errorContent}");
        }

        var chatId = await response.Content.ReadFromJsonAsync<int>();
        return chatId;
    }

    public async Task JoinChatGroup(int chatId, List<int> memberIds)
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            throw new Exception("Not connected to chat hub");
        }

        await _connection.InvokeAsync("JoinChat", chatId.ToString(), memberIds);
    }

    public async Task LeaveChatGroup(int chatId)
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            return;
        }

        await _connection.InvokeAsync("LeaveChat", chatId.ToString());
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
        
        _connection.On<Chat>("NewChat", (chat) =>
        {
            OnNewChat?.Invoke(chat);
        });
        
        _connection.On<GroupChat>("NewGroupChat", (groupChat) =>
        {
            OnNewGroupChat?.Invoke(groupChat);
        });

        _connection.On<Message>("MessageEdited", (message) =>
        {
            OnMessageEdited?.Invoke(message);
        });

        _connection.On<int>("MessageDeleted", (messageId) =>
        {
            OnMessageDeleted?.Invoke(messageId);
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

    public async Task<bool> DeleteAccount(int userId)
    {
        var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/user/{userId}");
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"{response.Content}");
        }

        if (_connection != null)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.StopAsync();
                }
                await _connection.DisposeAsync();
                _connection = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting: {ex.Message}");
            }
        }

        _currentUserId = null;

        return true;
    }

    public async Task<bool> LogoutAccount(int userId)
    {
        if (_connection != null)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.StopAsync();
            }
            await _connection.DisposeAsync();
            _connection = null;
        }

        _currentUserId = null;

        return true;
    }

    public async Task<List<User>> SearchUsers(string partialName)
    {
        if (string.IsNullOrWhiteSpace(partialName))
        {
            return [];
        }

        var response = await _httpClient.GetAsync($"{_serverUrl}/api/user/search?name={Uri.EscapeDataString(partialName)}");
        
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<List<User>>();
        return result ?? [];
    }
}
