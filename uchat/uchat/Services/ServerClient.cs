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
    private readonly string _serverUrl = "http://localhost:5000";
    
    public ServerClient()
    {
        _httpClient = new HttpClient();

        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/chatHub")
            .Build();

        _connection.StartAsync().Wait();

        _connection.InvokeAsync("JoinChat", "1"); 
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

        return result;
    }

    public Task<User> GetUserInfo(int chatId, int messageId)
    {
        throw new NotImplementedException();
    }

    public Task<Chat[]> GetChats()
    {
        throw new NotImplementedException();
    }

    public void RegisterNotificationCallback(Action<Message> onMessageReceived)
    {
        _connection.On<Message>("ReceiveMessage", (msg) =>
        {
            onMessageReceived?.Invoke(msg);
        });
    }
}