using System.Threading.Tasks;
using uchat.Models;

namespace uchat;

public interface IUser
{
    Task<bool> AddContact(User user);
    Task<bool> RemoveContact(User user);
    Task CreateChat(User user);
    Task DeleteChat(int id);
    Task LeaveGroupChat(int id);
    Task DeleteGroupChat(int id);
}
