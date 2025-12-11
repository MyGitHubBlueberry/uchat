using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace uchat.ViewModels;

public class AttachmentPreviewViewModel : IDisposable
{
    private Bitmap? _loadedBitmap;
    private bool _disposed;

    public AttachmentPreviewViewModel(string filePath)
    {
        FilePath = filePath;
        Preview = LoadPreviewAsync();
    }

    public string FilePath { get; }
    
    public Task<Bitmap?> Preview { get; }

    private async Task<Bitmap?> LoadPreviewAsync()
    {
        try
        {
            await using var stream = File.OpenRead(FilePath);
            _loadedBitmap = await Task.Run(() => Bitmap.DecodeToWidth(stream, 200));
            return _loadedBitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading preview from {FilePath}: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _loadedBitmap?.Dispose();
        _loadedBitmap = null;
        _disposed = true;
        
        GC.SuppressFinalize(this);
    }
}
