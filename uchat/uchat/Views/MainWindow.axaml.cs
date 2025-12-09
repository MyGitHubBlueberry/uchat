using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using uchat.Services;
using uchat.ViewModels;

namespace uchat.Views;

public partial class MainWindow : FocusDetachableUserControl
{
    public MainWindow()
    {
        InitializeComponent();
        
        DataContextChanged += (s, e) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MainWindowViewModel.ShouldScrollToBottom) && 
                        viewModel.ShouldScrollToBottom)
                    {
                        ScrollToBottom();
                        viewModel.ShouldScrollToBottom = false;
                    }
                };
                
                viewModel.ProfileRequested += OnProfileRequested;
            }
        };
    }

    private void ScrollToBottom()
    {
        MessageScrollViewer?.ScrollToEnd();
    }
    
    private async void OnProfileRequested(object? sender, System.EventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window mainWindow) return;

        var serviceProvider = App.Services;
        if (serviceProvider == null) return;

        var serverClient = serviceProvider.GetRequiredService<IServerClient>();
        var userSession = serviceProvider.GetRequiredService<IUserSession>();
        
        var profileViewModel = new ProfileViewModel(serverClient, userSession, null);
        var profileWindow = new ProfileWindow(profileViewModel);
        
        await profileWindow.ShowDialog(mainWindow);
    }
}
