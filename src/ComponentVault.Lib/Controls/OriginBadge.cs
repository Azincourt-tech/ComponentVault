using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Controls;

public enum BadgeKind
{
    Native,
    Custom,
    FreeLibrary,
    Syncfusion,
    New,
}

// snippet:control.badge
/// <summary>
/// Etiqueta neutra com um ponto colorido indicando a origem. Só o ponto tem cor: a interface
/// continua neutra e a cor vira uma marca pequena, legível nos dois temas.
/// </summary>
public class OriginBadge : Border
{
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(BadgeKind), typeof(OriginBadge), BadgeKind.Native,
        propertyChanged: (b, _, _) => ((OriginBadge)b).Refresh());

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(OriginBadge), null,
        propertyChanged: (b, _, _) => ((OriginBadge)b).Refresh());

    public static readonly BindableProperty IsCompactProperty = BindableProperty.Create(
        nameof(IsCompact), typeof(bool), typeof(OriginBadge), false,
        propertyChanged: (b, _, _) => ((OriginBadge)b).Refresh());

    private readonly Ellipse _dot = new() { VerticalOptions = LayoutOptions.Center };
    private readonly Label _label = new() { FontFamily = "JakartaMedium", VerticalOptions = LayoutOptions.Center };

    public OriginBadge()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 6 };
        this.SetThemeColor(BackgroundColorProperty, "SurfaceAlt");
        _label.SetThemeColor(Label.TextColorProperty, "InkSecondary");
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        Content = new HorizontalStackLayout { Spacing = 5, Children = { _dot, _label } };
        Refresh();
    }

    public BadgeKind Kind
    {
        get => (BadgeKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Substitui o texto padrão da origem.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public static string DefaultText(BadgeKind kind) => kind switch
    {
        BadgeKind.Native => "Nativo",
        BadgeKind.Custom => "Custom",
        BadgeKind.FreeLibrary => "Lib Gratuita",
        BadgeKind.Syncfusion => "Syncfusion",
        BadgeKind.New => "Novo",
        _ => kind.ToString(),
    };

    private void Refresh()
    {
        // A cor do ponto vem do token Origin{Kind}.
        _dot.SetThemeBrush(Shape.FillProperty, $"Origin{Kind}");

        var text = Text ?? DefaultText(Kind);
        _label.Text = text;
        _label.FontSize = 13;
        _dot.WidthRequest = _dot.HeightRequest = 7;
        Padding = IsCompact ? new Thickness(7, 2) : new Thickness(9, 4);
        SemanticProperties.SetDescription(this, $"Origem: {text}");
    }
}
// /snippet:control.badge
