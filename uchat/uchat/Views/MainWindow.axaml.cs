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
            }
        };
    }

    private void ScrollToBottom()
    {
        MessageScrollViewer?.ScrollToEnd();
    }
}
