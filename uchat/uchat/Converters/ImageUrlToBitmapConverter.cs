using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using uchat.Services;

namespace uchat.Converters;

public class ImageUrlToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string url && !string.IsNullOrWhiteSpace(url))
        {
            return LoadImageAsync(url);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private async Task<Bitmap?> LoadImageAsync(string url)
    {
        try
        {
            var fullUrl = ServerConfig.GetAttachmentUrl(url);

            var response = await ServerConfig.HttpClient.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image from {url}: {ex.Message}");
            return null;
        }
    }
}
