using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    public static readonly StyledProperty<IBrush?> CaretColorProperty =
        AvaloniaProperty.Register<StyledInputBase, IBrush?>(nameof(CaretColor), Brush.Parse("#EB459E"));

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

    public IBrush? CaretColor
    {
        get => GetValue(CaretColorProperty);
        set => SetValue(CaretColorProperty, value);
    }

    protected Control? InputControl { get; set; }

    private bool _isPointerOver;
    private IBrush? _hoverBackground;

    public StyledInputBase()
    {
        InitializeComponent();
        
        PointerPressed += OnPointerPressed;
        if (Container is not null)
        {
            Container.PointerEntered += OnContainerPointerEntered;
            Container.PointerExited += OnContainerPointerExited;
        }

        UpdateStyles();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BackgroundColorProperty ||
            change.Property == CornerRadiusValueProperty ||
            change.Property == PaddingValueProperty ||
            change.Property == CaretColorProperty)
        {
            UpdateStyles();
        }
    }

    protected virtual void UpdateStyles()
    {
        if (Container == null) return;

        Container.CornerRadius = new CornerRadius(CornerRadiusValue);
        _hoverBackground = CreateHoverBrush(BackgroundColor);
        ApplyBackground();

        if (InputControl != null)
        {
            ApplyInputStyles(InputControl);
        }
    }

    protected void SetInputControl(Control control)
    {
        InputControl = control;
        if (InputPresenter != null)
        {
            InputPresenter.Content = control;
        }
        ApplyInputStyles(control);
    }

    protected virtual void ApplyInputStyles(Control control)
    {
        if (control is TextBox textBox)
        {
            textBox.Background = Brushes.Transparent;
            textBox.BorderThickness = new Thickness(0);
            textBox.Foreground = Brushes.White;
            textBox.Padding = PaddingValue;
            textBox.CaretBrush = CaretColor;
            textBox.CornerRadius = new CornerRadius(0);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var element = e.Source as Control;
        if (element is not TextBox)
        {
            TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
        }
    }

    private void OnContainerPointerEntered(object? sender, PointerEventArgs e)
    {
        _isPointerOver = true;
        ApplyBackground();
    }

    private void OnContainerPointerExited(object? sender, PointerEventArgs e)
    {
        _isPointerOver = false;
        ApplyBackground();
    }

    private void ApplyBackground()
    {
        if (Container == null) return;

        var targetBrush = _isPointerOver
            ? _hoverBackground ?? BackgroundColor
            : BackgroundColor;

        Container.Background = targetBrush;
    }

    private static IBrush? CreateHoverBrush(IBrush? baseBrush)
    {
        if (baseBrush is ISolidColorBrush solidColorBrush)
        {
            const double darkenFactor = 0.9;
            var color = solidColorBrush.Color;

            var darkenedColor = Color.FromArgb(
                color.A,
                DarkenChannel(color.R, darkenFactor),
                DarkenChannel(color.G, darkenFactor),
                DarkenChannel(color.B, darkenFactor));

            return new SolidColorBrush(darkenedColor);
        }

        return baseBrush;
    }

    private static byte DarkenChannel(byte channel, double factor)
    {
        var value = (int)Math.Round(channel * factor);
        return (byte)Math.Clamp(value, 0, 255);
    }
}
