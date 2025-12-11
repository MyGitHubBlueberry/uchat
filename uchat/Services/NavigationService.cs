using System;
using System.Collections.Generic;

namespace uchat.Services;

public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly Stack<object> _navigationStack = new();
    private object? _currentViewModel;

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            Navigated?.Invoke(this, _currentViewModel);
        }
    }

    public bool CanGoBack => _navigationStack.Count > 0;

    public event EventHandler<object?>? Navigated;

    public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : class
    {
        if (_serviceProvider.GetService(typeof(TViewModel)) is not TViewModel viewModel)
        {
            throw new InvalidOperationException(
                $"Unable to resolve ViewModel of type {typeof(TViewModel).Name}. " +
                "Ensure it is registered in the DI container.");
        }

        if (parameter != null && viewModel is INavigationAware aware)
        {
            aware.OnNavigatedTo(parameter);
        }

        if (viewModel is IClearNavigationStack)
        {
            _navigationStack.Clear();
        }
        else if (CurrentViewModel != null)
        {
            _navigationStack.Push(CurrentViewModel);
        }

        CurrentViewModel = viewModel;
    }

    public void GoBack()
    {
        if (!CanGoBack)
        {
            return;
        }

        CurrentViewModel = _navigationStack.Pop();
    }
}

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
}

public interface IClearNavigationStack
{
}
