using Avalonia.Media.Imaging;
using SharedLibrary.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using uchat.Services;

namespace uchat.ViewModels;

public class AttachmentViewModel
{
    private readonly Attachment _attachment;

    public AttachmentViewModel(Attachment attachment)
    {
        _attachment = attachment;
        Image = LoadImageAsync();
    }

    public int Id => _attachment.Id;

    public string Url => _attachment.Url;

    public Task<Bitmap?> Image { get; }

    private async Task<Bitmap?> LoadImageAsync()
    {
        try
        {
            var url = ServerConfig.GetAttachmentUrl(_attachment.Url);
            Console.WriteLine(url);
            var response = await ServerConfig.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image from {_attachment.Url}: {ex.Message}");
            return null;
        }
    }
}

