using System;
using System.Net.Http;

namespace uchat.Services;

public static class ServerConfig
{
    private static string? _baseUrl;
    private static readonly Lazy<HttpClient> _httpClient = new(() =>
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        return client;
    });

    public static string BaseUrl
    {
        get
        {
            if (_baseUrl == null)
            {
                throw new InvalidOperationException("ServerConfig.BaseUrl has not been initialized.");
            }
            return _baseUrl;
        }
        set => _baseUrl = value;
    }

    public static bool IsInitialized => _baseUrl != null;

    public static HttpClient HttpClient => _httpClient.Value;

    public static string GetAttachmentUrl(string fileNameOrUrl)
    {
        if (fileNameOrUrl.StartsWith("http://") || fileNameOrUrl.StartsWith("https://"))
        {
            return fileNameOrUrl;
        }

        var baseUrl = IsInitialized ? BaseUrl : "http://localhost:5000";
        return $"{baseUrl}/Attachments/{fileNameOrUrl}";
    }
}
