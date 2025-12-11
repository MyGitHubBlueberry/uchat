using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLibrary.Models;

namespace uchat.ViewModels;

public partial class CreateGroupViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    private readonly int _currentUserId;
    
    [ObservableProperty] private string _groupName = string.Empty;
    [ObservableProperty] private string _groupDescription = string.Empty;
    
    public ObservableCollection<SelectableUser> AvailableUsers { get; } = new();
    
    public CreateGroupViewModel(IServerClient serverClient, int currentUserId, Chat[] chats)
    {
        _serverClient = serverClient;
        _currentUserId = currentUserId;
        
        LoadUsersFromChats(chats);
    }
    
    private void LoadUsersFromChats(Chat[] chats)
    {
        var users = chats
            .SelectMany(c => new[] { c.userFrom, c.userTo })
            .Where(u => u.Id != _currentUserId)
            .DistinctBy(u => u.Id)
            .Select(u => new SelectableUser(u))
            .OrderBy(u => u.User.Name);
            
        foreach (var user in users)
        {
            AvailableUsers.Add(user);
        }
    }
    
    [RelayCommand]
    private async Task CreateGroup(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(GroupName))
        {
            return;
        }
        
        var selectedUsers = AvailableUsers.Where(u => u.IsSelected).ToList();
        
        if (selectedUsers.Count == 0)
        {
            return;
        }
        
        var participantIds = selectedUsers.Select(u => u.User.Id).ToList();
        participantIds.Add(_currentUserId);
        
        try
        {
            await _serverClient.CreateGroupChat(GroupName, _currentUserId, participantIds, GroupDescription);
            
            if (parameter is Avalonia.Controls.Window window)
            {
                window.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating group: {ex.Message}");
        }
    }
}

public partial class SelectableUser(User user) : ObservableObject
{
    public User User { get; } = user;

    [ObservableProperty] private bool _isSelected;
}
