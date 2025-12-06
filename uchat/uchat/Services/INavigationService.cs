using System;

namespace uchat.Services;

public interface INavigationService
{
    void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class;
    void GoBack();
    bool CanGoBack { get; }
    object? CurrentViewModel { get; }
    event EventHandler<object?>? Navigated;
}
