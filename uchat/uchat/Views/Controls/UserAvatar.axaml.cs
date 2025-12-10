using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace uchat.Views.Controls;

public partial class UserAvatar : UserControl
{
    public static readonly StyledProperty<string> InitialProperty =
        AvaloniaProperty.Register<UserAvatar, string>(nameof(Initial), "?");

    public static readonly StyledProperty<double> AvatarSizeProperty =
        AvaloniaProperty.Register<UserAvatar, double>(nameof(AvatarSize), 40);

    public static readonly StyledProperty<CornerRadius> AvatarCornerRadiusProperty =
        AvaloniaProperty.Register<UserAvatar, CornerRadius>(nameof(AvatarCornerRadius), new CornerRadius(20));

    public static readonly StyledProperty<IBrush> AvatarBackgroundProperty =
        AvaloniaProperty.Register<UserAvatar, IBrush>(nameof(AvatarBackground), Brush.Parse("#5865F2"));

    public static readonly StyledProperty<IBrush> AvatarForegroundProperty =
        AvaloniaProperty.Register<UserAvatar, IBrush>(nameof(AvatarForeground), Brushes.White);

    public static readonly StyledProperty<FontWeight> AvatarFontWeightProperty =
        AvaloniaProperty.Register<UserAvatar, FontWeight>(nameof(AvatarFontWeight), FontWeight.SemiBold);

    public static readonly StyledProperty<double> AvatarFontSizeProperty =
        AvaloniaProperty.Register<UserAvatar, double>(nameof(AvatarFontSize), 14);

    public string Initial
    {
        get => GetValue(InitialProperty);
        set => SetValue(InitialProperty, value);
    }

    public double AvatarSize
    {
        get => GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public CornerRadius AvatarCornerRadius
    {
        get => GetValue(AvatarCornerRadiusProperty);
        set => SetValue(AvatarCornerRadiusProperty, value);
    }

    public IBrush AvatarBackground
    {
        get => GetValue(AvatarBackgroundProperty);
        set => SetValue(AvatarBackgroundProperty, value);
    }

    public IBrush AvatarForeground
    {
        get => GetValue(AvatarForegroundProperty);
        set => SetValue(AvatarForegroundProperty, value);
    }

    public FontWeight AvatarFontWeight
    {
        get => GetValue(AvatarFontWeightProperty);
        set => SetValue(AvatarFontWeightProperty, value);
    }

    public double AvatarFontSize
    {
        get => GetValue(AvatarFontSizeProperty);
        set => SetValue(AvatarFontSizeProperty, value);
    }

    public UserAvatar()
    {
        InitializeComponent();
    }
}
