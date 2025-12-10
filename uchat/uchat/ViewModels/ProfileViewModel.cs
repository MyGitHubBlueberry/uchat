using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using uchat.Commands;
using uchat.Services;

namespace uchat.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly IServerClient _serverClient;
    private readonly IUserSession _userSession;
    private readonly INavigationService _navigationService;
    private readonly Action _closeAction;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string? _profileImageUrl;
    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public ICommand UploadPictureCommand { get; }
    public ICommand RemovePictureCommand { get; }
    public ICommand UpdatePasswordCommand { get; }
    public ICommand DeleteAccountCommand { get; }
    public ICommand LogoutAccountCommand { get; }

    public ProfileViewModel(IServerClient serverClient, 
        IUserSession userSession, 
        Action closeAction,
        INavigationService navigationService)
    {
        _navigationService = navigationService;
        _serverClient = serverClient;
        _userSession = userSession;
        _closeAction = closeAction;

        Username = _userSession.CurrentUser?.Name ?? "Unknown";
        ProfileImageUrl = _userSession.CurrentUser?.Image;

        UploadPictureCommand = new RelayCommand(UploadPictureAsync);
        RemovePictureCommand = new RelayCommand(RemovePictureAsync);
        UpdatePasswordCommand = new RelayCommand(UpdatePasswordAsync);
        DeleteAccountCommand = new RelayCommand(DeleteAccountAsync);
        LogoutAccountCommand = new RelayCommand(LogoutAccountAsync);

    }
    private async Task LogoutAccountAsync()
    {
        if (_userSession.CurrentUser == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var result = await _serverClient.LogoutAccount(_userSession.CurrentUser.Id);

            if (result)
            {
                _userSession.Clear();
                _closeAction.Invoke();
                _navigationService.NavigateTo<LoginWindowViewModel>();
            }
        }
        catch (Exception ex)
        {
            StatusMessage =  ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UploadPictureAsync()
    {
        if (_userSession.CurrentUser == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Profile Picture",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var filePath = files[0].Path.LocalPath;
                await _serverClient.UploadProfilePicture(_userSession.CurrentUser.Id, filePath);
                
                ProfileImageUrl = filePath;
                if (_userSession.CurrentUser != null)
                {
                    _userSession.CurrentUser.Image = filePath;
                }
                
                StatusMessage = "Profile picture updated successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RemovePictureAsync()
    {
        if (_userSession.CurrentUser == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            await _serverClient.RemoveProfilePicture(_userSession.CurrentUser.Id);
            
            ProfileImageUrl = null;
            if (_userSession.CurrentUser != null)
            {
                _userSession.CurrentUser.Image = null;
            }
            
            StatusMessage = "Profile picture removed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task UpdatePasswordAsync()
    {
        if (_userSession.CurrentUser == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                StatusMessage = "New password cannot be empty";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                StatusMessage = "Passwords do not match";
                return;
            }

            var currentPwd = string.IsNullOrWhiteSpace(CurrentPassword) ? null : CurrentPassword;
            await _serverClient.UpdatePassword(_userSession.CurrentUser.Id, currentPwd, NewPassword);
            
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            
            StatusMessage = "Password updated successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteAccountAsync()
    {
        if (_userSession.CurrentUser == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var result = await _serverClient.DeleteAccount(_userSession.CurrentUser.Id);

            if (result)
            {
                _userSession.Clear();
                _closeAction.Invoke();
                _navigationService.NavigateTo<RegistrationWindowViewModel>();
            }

        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
