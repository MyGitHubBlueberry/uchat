using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace uchat.Views.Controls;

public partial class StyledInputBase : UserControl
{
    public static readonly StyledProperty<IBrush?> BackgroundColorProperty =
        AvaloniaProperty.Register<StyledInputBase, IBrush?>(nameof(BackgroundColor), Brush.Parse("#202225"));

    public static readonly StyledProperty<double> CornerRadiusValueProperty =
        AvaloniaProperty.Register<StyledInputBase, double>(nameof(CornerRadiusValue), 25.0);

    public static readonly StyledProperty<Thickness> PaddingValueProperty =
        AvaloniaProperty.Register<StyledInputBase, Thickness>(nameof(PaddingValue), new Thickness(16, 16));

    public IBrush? BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public double CornerRadiusValue
    {
        get => GetValue(CornerRadiusValueProperty);
        set => SetValue(CornerRadiusValueProperty, value);
    }

    public Thickness PaddingValue
    {
        get => GetValue(PaddingValueProperty);
        set => SetValue(PaddingValueProperty, value);
    }

    protected TextBox? InputControl { get; set; }

    public StyledInputBase()
    {
        InitializeComponent();
        UpdateStyles();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BackgroundColorProperty ||
            change.Property == CornerRadiusValueProperty ||
            change.Property == PaddingValueProperty)
        {
            UpdateStyles();
        }
    }

    protected virtual void UpdateStyles()
    {
        if (Container == null) return;
        
        Container.Background = BackgroundColor;
        Container.CornerRadius = new CornerRadius(CornerRadiusValue);

        if (InputControl != null)
        {
            InputControl.Padding = PaddingValue;
        }
    }

    protected void SetInputControl(TextBox textBox)
    {
        InputControl = textBox;
        if (InputPresenter != null)
        {
            InputPresenter.Content = textBox;
        }
    }
}
