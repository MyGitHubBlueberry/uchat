using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using uchat.Services;
using uchat.ViewModels;

namespace uchat.Views;

public partial class AppWindow : Window
{
    private readonly INavigationService _navigationService;

    public AppWindow()
    {
        InitializeComponent();

        if (App.Services == null)
        {
            throw new InvalidOperationException("Services not initialized");
        }

        _navigationService = App.Services.GetRequiredService<INavigationService>();

        _navigationService.Navigated += OnNavigated;

        var loaderViewModel = App.Services.GetRequiredService<LoaderViewModel>();
        ContentArea.Content = loaderViewModel;
    }

    private void OnNavigated(object? sender, object? viewModel)
    {
        ContentArea.Content = viewModel;

        UpdateWindowProperties(viewModel);
    }

    private void UpdateWindowProperties(object? viewModel)
    {
        switch (viewModel)
        {
            case LoaderViewModel:
                Title = "UCHAT - Loading...";
                CanResize = false;
                Width = 300;
                Height = 300;
                break;

            case LoginWindowViewModel:
                Title = "UCHAT - Login";
                CanResize = false;
                Width = 600;
                Height = 500;
                break;

            case RegistrationWindowViewModel:
                Title = "UCHAT - Register";
                CanResize = false;
                Width = 700;
                Height = 550;
                break;

            case MainWindowViewModel:
                Title = "UCHAT";
                CanResize = true;
                MinWidth = 600;
                MinHeight = 400;
                Width = 1000;
                Height = 600;
                break;
        }
    }
}
