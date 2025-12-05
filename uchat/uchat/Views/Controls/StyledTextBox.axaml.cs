using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace uchat.Views.Controls;

public partial class StyledTextBox : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StyledTextBox, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<StyledTextBox, string>(nameof(Watermark));

    public static readonly StyledProperty<char?> PasswordCharProperty =
        AvaloniaProperty.Register<StyledTextBox, char?>(nameof(PasswordChar));

    public static readonly StyledProperty<IBrush?> BackgroundColorProperty =
        AvaloniaProperty.Register<StyledTextBox, IBrush?>(nameof(BackgroundColor), Brush.Parse("#202225"));

    public static readonly StyledProperty<IBrush?> CaretColorProperty =
        AvaloniaProperty.Register<StyledTextBox, IBrush?>(nameof(CaretColor), Brush.Parse("#EB459E"));

    public static readonly StyledProperty<double> CornerRadiusValueProperty =
        AvaloniaProperty.Register<StyledTextBox, double>(nameof(CornerRadiusValue), 25.0);

    public static readonly StyledProperty<Thickness> PaddingValueProperty =
        AvaloniaProperty.Register<StyledTextBox, Thickness>(nameof(PaddingValue), new Thickness(15, 12));

    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<StyledTextBox, bool>(nameof(AcceptsReturn));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<StyledTextBox, TextWrapping>(nameof(TextWrapping));

    public static readonly StyledProperty<double> MinHeightValueProperty =
        AvaloniaProperty.Register<StyledTextBox, double>(nameof(MinHeightValue), double.NaN);

    public static readonly StyledProperty<double> MaxHeightValueProperty =
        AvaloniaProperty.Register<StyledTextBox, double>(nameof(MaxHeightValue), double.NaN);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public char? PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public IBrush? BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public IBrush? CaretColor
    {
        get => GetValue(CaretColorProperty);
        set => SetValue(CaretColorProperty, value);
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

    public bool AcceptsReturn
    {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public double MinHeightValue
    {
        get => GetValue(MinHeightValueProperty);
        set => SetValue(MinHeightValueProperty, value);
    }

    public double MaxHeightValue
    {
        get => GetValue(MaxHeightValueProperty);
        set => SetValue(MaxHeightValueProperty, value);
    }

    public StyledTextBox()
    {
        InitializeComponent();
        UpdateProperties();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == WatermarkProperty ||
            change.Property == PasswordCharProperty ||
            change.Property == BackgroundColorProperty ||
            change.Property == CaretColorProperty ||
            change.Property == CornerRadiusValueProperty ||
            change.Property == PaddingValueProperty ||
            change.Property == AcceptsReturnProperty ||
            change.Property == TextWrappingProperty ||
            change.Property == MinHeightValueProperty ||
            change.Property == MaxHeightValueProperty)
        {
            UpdateProperties();
        }
    }

    private void UpdateProperties()
    {
        if (InputBox == null || Container == null) return;

        InputBox.Text = Text;
        InputBox.Watermark = Watermark;
        InputBox.PasswordChar = PasswordChar ?? default;
        InputBox.CaretBrush = CaretColor;
        InputBox.Padding = PaddingValue;
        InputBox.AcceptsReturn = AcceptsReturn;
        InputBox.TextWrapping = TextWrapping;

        if (!double.IsNaN(MinHeightValue))
            InputBox.MinHeight = MinHeightValue;
        
        if (!double.IsNaN(MaxHeightValue))
            InputBox.MaxHeight = MaxHeightValue;

        Container.Background = BackgroundColor;
        Container.CornerRadius = new CornerRadius(CornerRadiusValue);

        InputBox.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                Text = InputBox.Text ?? string.Empty;
            }
        };
    }
}
