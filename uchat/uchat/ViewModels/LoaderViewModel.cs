using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using uchat.Services;

namespace uchat.ViewModels;

public sealed class LoaderViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IUserSession _userSession;
    private readonly IConfiguration _configuration;

    public LoaderViewModel(
        INavigationService navigationService,
        IUserSession userSession,
        IConfiguration configuration)
    {
        _navigationService = navigationService;
        _userSession = userSession;
        _configuration = configuration;

        _ = InitializeWithErrorHandlingAsync();
    }

    private async Task InitializeWithErrorHandlingAsync()
    {
        try
        {
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during initialization: {ex.Message}");
            _navigationService.NavigateTo<LoginWindowViewModel>();
        }
    }

    private async Task InitializeAsync()
    {
        var isMock = _configuration.GetValue<bool>("App:IsMock");
        var minDisplayTime = _configuration.GetValue("App:LoaderMinimumDisplayTimeMs", 3000);

        var displayTask = Task.Delay(minDisplayTime);
        var isAuthenticated = await CheckAuthenticationAsync(isMock);
        await displayTask;

        if (isAuthenticated)
        {
            _navigationService.NavigateTo<MainWindowViewModel>();
        }
        else
        {
            _navigationService.NavigateTo<LoginWindowViewModel>();
        }
    }

    private async Task<bool> CheckAuthenticationAsync(bool isMock)
    {
        await Task.Delay(50);

        if (isMock)
        {
            // FIXME: always return false for now (user not authenticated)
            return false;
        }

        return _userSession.IsAuthenticated;
    }
}
