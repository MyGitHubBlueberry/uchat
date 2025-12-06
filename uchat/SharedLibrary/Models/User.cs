namespace SharedLibrary.Models;

public class User
{
    public int Id { get; set; }
    
    public string LoginName { get; set; }
    
    public string? Image { get; set; }
    
    public List<int>? Friends  { get; set; }

    public List<Chat>? Chats  { get; set; }
    
    public List<GroupChat>? GroupChats {get; set;}
}
