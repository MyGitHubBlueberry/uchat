namespace SharedLibrary.Models;

public class Attachment
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public string? FileName { get; set; }
}
