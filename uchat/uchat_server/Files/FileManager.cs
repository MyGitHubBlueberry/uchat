namespace uchat_server.Files;

public static class FileManager
{
    public static readonly long AvatarSizeLimit = 10 * 1024 * 1024;
    public static readonly long FileSizeLimit = 100 * 1024 * 1024;

    public static async Task<string> SaveAvatar(IFormFile file, string saveFolder)
    {
        if (Path.GetExtension(file.FileName).Trim()
                is not (".png" or ".jpeg" or ".jpg" or ".gif"))
            throw new InvalidDataException($"This file format is not supported as avatar decoratoin.");
        if (file.Length > AvatarSizeLimit)
            throw new InvalidDataException("File size is too large.");

        return await Save(file, saveFolder);
    }

    public static async Task<string> Save(IFormFile file, string saveFolder)
    {
        string folder = AppendRootFolder(saveFolder);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        if (file.Length > FileSizeLimit)
            throw new InvalidDataException("File size is too large.");

        string extention = Path.GetExtension(file.FileName).Trim();
        string uniqueFileName = Guid.NewGuid().ToString() + extention;
        string path = Path.Combine(folder, uniqueFileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return uniqueFileName;
    }

    public static bool Delete(string folder, string file)
    {
        file = Path.Combine(AppendRootFolder(folder), file);
        if (File.Exists(file))
        {
            File.Delete(file);
            return true;
        }
        return false;
    }

    static string AppendRootFolder(string saveFolder)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", saveFolder);
    }
}
