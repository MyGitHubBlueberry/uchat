using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    public static readonly StyledProperty<ICommand?> EnterCommandProperty =
        AvaloniaProperty.Register<StyledTextBox, ICommand?>(nameof(EnterCommand));

    public static readonly StyledProperty<ICommand?> PasteCommandProperty =
        AvaloniaProperty.Register<StyledTextBox, ICommand?>(nameof(PasteCommand));

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

    public ICommand? EnterCommand
    {
        get => GetValue(EnterCommandProperty);
        set => SetValue(EnterCommandProperty, value);
    }

    public ICommand? PasteCommand
    {
        get => GetValue(PasteCommandProperty);
        set => SetValue(PasteCommandProperty, value);
    }

    public StyledTextBox()
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
            AcceptsReturn = false,
            TextWrapping = TextWrapping.NoWrap
        };

        SetInputControl(inputBox);

        inputBox.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.TextProperty)
            {
                Text = inputBox.Text ?? string.Empty;
            }
        };

        inputBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter && EnterCommand?.CanExecute(null) == true)
            {
                EnterCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
            {
                if (PasteCommand?.CanExecute(null) == true)
                {
                    PasteCommand.Execute(null);
                    // Mark as handled to prevent default paste behavior alongside custom command
                    e.Handled = true;
                }
            }
        };

        UpdateProperties();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
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

        InputControl.Text = Text;
        InputControl.Watermark = Watermark;
        InputControl.PasswordChar = PasswordChar ?? default;
        InputControl.CaretBrush = Brushes.White;
    }
}
