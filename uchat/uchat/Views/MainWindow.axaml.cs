using Avalonia.Controls;
using Avalonia.Input;
using uchat.ViewModels;

namespace uchat.Views;

public partial class MainWindow : UserControl
{
    public MainWindow()
    {
        InitializeComponent();
        PointerPressed += OnControlPointerPressed;
        
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
    
    private void OnControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var element = e.Source as Control;
        if (element is not TextBox)
        {
            TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
        }
    }

    private void ScrollToBottom()
    {
        MessageScrollViewer?.ScrollToEnd();
    }
}
