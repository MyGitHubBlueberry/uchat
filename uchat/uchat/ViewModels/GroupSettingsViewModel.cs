using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedLibrary.Models;
using uchat.Services;

namespace uchat.ViewModels;

public partial class GroupSettingsViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    private readonly GroupChat _groupChat;
    private readonly int _currentUserId;
    
    [ObservableProperty] private string _groupName;
    [ObservableProperty] private string _groupDescription;
    [ObservableProperty] private bool _isOwner;
    
    public ObservableCollection<GroupMemberViewModel> Members { get; } = new();
    public ObservableCollection<SelectableUser> AvailableUsers { get; } = new();
    
    public string GroupId => _groupChat.id.ToString();
    
    public GroupSettingsViewModel(IServerClient serverClient, GroupChat groupChat, int currentUserId)
    {
        _serverClient = serverClient;
        _groupChat = groupChat;
        _currentUserId = currentUserId;
        _groupName = groupChat.name;
        _groupDescription = groupChat.description ?? string.Empty;
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        IsOwner = await _serverClient.IsOwnerOfChat(_groupChat.id, _currentUserId);
        await LoadMembers();
        await LoadAvailableUsers();
    }
    
    private async Task LoadMembers()
    {
        try
        {
            var members = await _serverClient.GetChatMembers(_groupChat.id);
            Members.Clear();
            
            foreach (var member in members)
            {
                Members.Add(new GroupMemberViewModel(member, _groupChat.owner.Id, _currentUserId));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading members: {ex.Message}");
        }
    }
    
    private async Task LoadAvailableUsers()
    {
        if (!IsOwner) return;
        
        try
        {
            var (chats, _) = await _serverClient.GetChats();
            var currentMemberIds = Members.Select(m => m.UserId).ToHashSet();
            
            AvailableUsers.Clear();
            
            foreach (var chat in chats)
            {
                var otherUser = chat.userFrom.Id == _currentUserId ? chat.userTo : chat.userFrom;
                
                if (!currentMemberIds.Contains(otherUser.Id) && otherUser.Id != _currentUserId)
                {
                    AvailableUsers.Add(new SelectableUser(otherUser));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading available users: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task SaveChanges()
    {
        if (!IsOwner) return;
        
        try
        {
            await _serverClient.UpdateGroupChat(_groupChat.id, _currentUserId, GroupName, GroupDescription);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating group: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task AddSelectedMembers()
    {
        if (!IsOwner) return;
        
        var selectedUsers = AvailableUsers.Where(u => u.IsSelected).ToList();
        
        foreach (var user in selectedUsers)
        {
            try
            {
                await _serverClient.AddChatMember(_groupChat.id, user.User.Id);
                AvailableUsers.Remove(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding member {user.User.Name}: {ex.Message}");
            }
        }
        
        await LoadMembers();
    }
    
    [RelayCommand]
    private async Task RemoveMember(GroupMemberViewModel member)
    {
        if (!IsOwner || member.IsOwner) return;
        
        try
        {
            await _serverClient.RemoveChatMember(_groupChat.id, member.UserId);
            Members.Remove(member);
            await LoadAvailableUsers();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing member: {ex.Message}");
        }
    }
}

public partial class GroupMemberViewModel(ChatMemberDto member, int ownerId, int currentUserId) : ObservableObject
{
    [ObservableProperty] private int _userId = member.UserId;
    [ObservableProperty] private string _userName = member.UserName;
    [ObservableProperty] private string? _imageUrl = member.ImageUrl;
    [ObservableProperty] private bool _isOwner = member.UserId == ownerId;
    [ObservableProperty] private bool _isCurrentUser = member.UserId == currentUserId;
    
    public bool CanRemove => !IsOwner && !IsCurrentUser;
}
