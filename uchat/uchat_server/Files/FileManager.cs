namespace uchat_server.Files;

public static class FileManager {

    public static async Task<string> SaveAvatar(IFormFile file, string saveFolder) {
        string folder = AppendRootFolder(saveFolder);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        FileInfo fileInfo = new FileInfo(file.FileName);
        if (fileInfo.Extension.Trim() is not (".png" or ".jpeg" or ".gif")) {
            throw new InvalidDataException($"This file format ({fileInfo.Extension.Trim()}) is not supported as avatar decoratoin.");
        }

        return await Save(file, saveFolder);
    }

    public static async Task<string> Save(IFormFile file, string saveFolder) {
        string folder = AppendRootFolder(saveFolder);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        FileInfo fileInfo = new FileInfo(file.FileName);

        string uniqueFileName = Guid.NewGuid().ToString() + fileInfo.Extension.Trim();
        string path = Path.Combine(folder, uniqueFileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return uniqueFileName;
    }

    public static bool Delete(string folder, string file) {
        file = Path.Combine(AppendRootFolder(folder), file);
        if (File.Exists(file)) {
            File.Delete(file);
            return true;
        }
        return false;
    }

    static string AppendRootFolder(string saveFolder) {
        return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", saveFolder);
    }
}
