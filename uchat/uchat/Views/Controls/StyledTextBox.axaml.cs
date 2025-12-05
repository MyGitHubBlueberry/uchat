using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace uchat.Views.Controls;

public partial class StyledTextBox : StyledInputBase
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StyledTextBox, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<StyledTextBox, string>(nameof(Watermark));

    public static readonly StyledProperty<char?> PasswordCharProperty =
        AvaloniaProperty.Register<StyledTextBox, char?>(nameof(PasswordChar));

    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<StyledTextBox, bool>(nameof(AcceptsReturn));

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<StyledTextBox, TextWrapping>(nameof(TextWrapping));

    public static readonly StyledProperty<double> MinHeightValueProperty =
        AvaloniaProperty.Register<StyledTextBox, double>(nameof(MinHeightValue), double.NaN);

    public static readonly StyledProperty<double> MaxHeightValueProperty =
        AvaloniaProperty.Register<StyledTextBox, double>(nameof(MaxHeightValue), double.NaN);

    private TextBox? _inputBox;

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
        InitializeInputBox();
    }

    private void InitializeInputBox()
    {
        _inputBox = new TextBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White
        };

        SetInputControl(_inputBox);
        UpdateProperties();

        _inputBox.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                Text = _inputBox.Text ?? string.Empty;
            }
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == WatermarkProperty ||
            change.Property == PasswordCharProperty ||
            change.Property == AcceptsReturnProperty ||
            change.Property == TextWrappingProperty ||
            change.Property == MinHeightValueProperty ||
            change.Property == MaxHeightValueProperty)
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
        if (_inputBox == null) return;

        _inputBox.Text = Text;
        _inputBox.Watermark = Watermark;
        _inputBox.PasswordChar = PasswordChar ?? default;
        _inputBox.CaretBrush = CaretColor;
        _inputBox.Padding = PaddingValue;
        _inputBox.AcceptsReturn = AcceptsReturn;
        _inputBox.TextWrapping = TextWrapping;

        if (!double.IsNaN(MinHeightValue))
            _inputBox.MinHeight = MinHeightValue;
        
        if (!double.IsNaN(MaxHeightValue))
            _inputBox.MaxHeight = MaxHeightValue;
    }
}
