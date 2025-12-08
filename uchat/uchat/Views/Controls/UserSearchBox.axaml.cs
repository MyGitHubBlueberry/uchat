using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SharedLibrary.Models;

namespace uchat.Views.Controls;

public partial class UserSearchBox : StyledInputBase
{
    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<UserSearchBox, string>(nameof(SearchText), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<UserSearchBox, string>(nameof(Watermark), "Search users...");

    public static readonly StyledProperty<ObservableCollection<User>> SearchResultsProperty =
        AvaloniaProperty.Register<UserSearchBox, ObservableCollection<User>>(nameof(SearchResults));

    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public ObservableCollection<User> SearchResults
    {
        get => GetValue(SearchResultsProperty);
        set => SetValue(SearchResultsProperty, value);
    }

    public event EventHandler<User>? UserSelected;

    private DispatcherTimer? _debounceTimer;

    public UserSearchBox()
    {
        InitializeComponent();
        InitializeSearchBox();
    }

    private void InitializeSearchBox()
    {
        var inputBox = new TextBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            AcceptsReturn = false,
            TextWrapping = TextWrapping.NoWrap
        };

        SetInputControl(inputBox);

        inputBox.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                SearchText = inputBox.Text ?? string.Empty;
                OnSearchTextChanged();
            }
        };

        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };
        _debounceTimer.Tick += async (s, e) =>
        {
            _debounceTimer?.Stop();
            await OnSearchDebounced();
        };

        UpdateProperties();
    }

    private void OnSearchTextChanged()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    private async System.Threading.Tasks.Task OnSearchDebounced()
    {
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            await viewModel.SearchUsersCommand.ExecuteAsync(SearchText);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SearchTextProperty ||
            change.Property == WatermarkProperty)
        {
            UpdateProperties();
        }
    }

    protected override void UpdateStyles()
    {
        base.UpdateStyles();
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        if (InputControl == null) return;

        InputControl.Text = SearchText;
        InputControl.Watermark = Watermark;
        InputControl.CaretBrush = Brushes.White;
    }
}
