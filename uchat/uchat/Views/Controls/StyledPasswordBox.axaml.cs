using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace uchat.Views.Controls;

public partial class StyledPasswordBox : StyledInputBase
{
    public static readonly StyledProperty<string> PasswordProperty =
        AvaloniaProperty.Register<StyledPasswordBox, string>(nameof(Password), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<StyledPasswordBox, string>(nameof(Watermark), "Password");

    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<StyledPasswordBox, char>(nameof(PasswordChar), '●');

    public string Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public StyledPasswordBox()
    {
        InitializeComponent();
        InitializeInputBox();
    }

    private void InitializeInputBox()
    {
        var inputBox = new TextBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = Brushes.White,
            PasswordChar = PasswordChar,
            CaretBrush = Brushes.White
        };

        SetInputControl(inputBox);

        inputBox.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                Password = inputBox.Text ?? string.Empty;
            }
        };

        UpdateProperties();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PasswordProperty ||
            change.Property == WatermarkProperty ||
            change.Property == PasswordCharProperty)
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

        InputControl.Text = Password;
        InputControl.Watermark = Watermark;
        InputControl.PasswordChar = PasswordChar;
    }
}
