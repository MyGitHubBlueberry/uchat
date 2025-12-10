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
        var chatContent = this.FindControl<ChatContent>("ChatContent");
        chatContent?.FindControl<ScrollViewer>("MessageScrollViewer")?.ScrollToEnd();
    }
    
    private async void OnProfileRequested(object? sender, System.EventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window mainWindow) return;

        var serviceProvider = App.Services;
        if (serviceProvider == null) return;

        var serverClient = serviceProvider.GetRequiredService<IServerClient>();
        var userSession = serviceProvider.GetRequiredService<IUserSession>();
        var navigationService = serviceProvider.GetRequiredService<INavigationService>();

        var profileWindow = new ProfileWindow();
        var profileViewModel = new ProfileViewModel(serverClient, userSession, () => profileWindow.Close(), navigationService);
        profileWindow.DataContext = profileViewModel;

        await profileWindow.ShowDialog(mainWindow);
    }
}
