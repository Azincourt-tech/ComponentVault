using System.Globalization;
using ComponentVault.Core.Models;
using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Showcase;
using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.App.Controls;

/// <summary>Instancia a View de preview registrada para a variante (atributo [ShowcasePreview]).</summary>
public sealed class PreviewHost : ContentView
{
    public static readonly BindableProperty VariantIdProperty = BindableProperty.Create(
        nameof(VariantId), typeof(string), typeof(PreviewHost), null,
        propertyChanged: (b, _, n) => ((PreviewHost)b).Load((string?)n));

    public string? VariantId
    {
        get => (string?)GetValue(VariantIdProperty);
        set => SetValue(VariantIdProperty, value);
    }

    private int _loadVersion;

    private async void Load(string? variantId)
    {
        var version = ++_loadVersion;

        // Transição de troca de nível: sai em 120ms, entra subindo 10px em 260ms.
        if (Content is not null && Handler is not null)
        {
            await Content.FadeToAsync(0, 120, Easing.CubicIn);
            if (version != _loadVersion) return; // outra troca chegou no meio
        }

        var next = Build(variantId);
        if (next is null)
        {
            Content = null;
            return;
        }

        next.Opacity = 0;
        next.TranslationY = 10;
        Content = next;
        await Task.WhenAll(
            next.FadeToAsync(1, 260, Easing.CubicOut),
            next.TranslateToAsync(0, 0, 260, Easing.CubicOut));
    }

    private static View? Build(string? variantId)
    {
        if (variantId is null) return null;

        try
        {
            return PreviewRegistry.Create(variantId) ?? Message($"Nenhuma View registrada para '{variantId}'.");
        }
        catch (Exception ex)
        {
            // Um preview quebrado não pode derrubar a vitrine inteira: mostra o erro no lugar dele.
            var error = ex.InnerException ?? ex;
            return Message($"Falha ao montar o preview '{variantId}': {error.Message}");
        }
    }

    private static Label Message(string text) => new()
    {
        Text = text,
        Style = (Style)Application.Current!.Resources["Caption"],
    };
}

/// <summary>Código com realce de sintaxe: cada token vira um Span com a cor do tema.</summary>
public sealed class CodeView : Label
{
    public static readonly BindableProperty TokensProperty = BindableProperty.Create(
        nameof(Tokens), typeof(IReadOnlyList<SyntaxToken>), typeof(CodeView), null,
        propertyChanged: (b, _, _) => ((CodeView)b).Render());

    public static readonly BindableProperty FallbackTextProperty = BindableProperty.Create(
        nameof(FallbackText), typeof(string), typeof(CodeView), null,
        propertyChanged: (b, _, _) => ((CodeView)b).Render());

    public IReadOnlyList<SyntaxToken>? Tokens
    {
        get => (IReadOnlyList<SyntaxToken>?)GetValue(TokensProperty);
        set => SetValue(TokensProperty, value);
    }

    /// <summary>Texto puro quando não há tokens (ex.: snippet ausente).</summary>
    public string? FallbackText
    {
        get => (string?)GetValue(FallbackTextProperty);
        set => SetValue(FallbackTextProperty, value);
    }

    private void Render()
    {
        if (Tokens is not { Count: > 0 } tokens)
        {
            FormattedText = null;
            Text = FallbackText;
            return;
        }

        var formatted = new FormattedString();
        foreach (var token in tokens)
        {
            var span = new Span { Text = token.Text };
            // SetThemeColor usa AppThemeColor: a cor troca sozinha com o tema, sem re-renderizar.
            span.SetThemeColor(Span.TextColorProperty, $"Syntax{token.Kind}");
            if (token.Kind == SyntaxKind.Comment) span.FontAttributes = FontAttributes.Italic;
            formatted.Spans.Add(span);
        }

        FormattedText = formatted;
    }
}

/// <summary>
/// Miniatura do catálogo: ícone Fluent em tinta sobre SurfaceAlt. Nome ausente ou inválido cai
/// no ícone padrão, para nenhum componente quebrar o layout.
/// </summary>
public sealed class ComponentThumb : Border
{
    public const FluentIcons PlaceholderIcon = FluentIcons.Cube24;

    public static readonly BindableProperty IconNameProperty = BindableProperty.Create(
        nameof(IconName), typeof(string), typeof(ComponentThumb), null,
        propertyChanged: (b, _, _) => ((ComponentThumb)b).Refresh());

    public static readonly BindableProperty IconSizeProperty = BindableProperty.Create(
        nameof(IconSize), typeof(double), typeof(ComponentThumb), 20d,
        propertyChanged: (b, _, n) => ((ComponentThumb)b)._icon.IconSize = (double)n);

    private readonly MauiIcon _icon = new()
    {
        IconSize = 20,
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
    };

    public ComponentThumb()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        this.SetThemeColor(BackgroundColorProperty, "SurfaceAlt");
        _icon.SetThemeColor(MauiIcon.IconColorProperty, "Ink");
        Content = _icon;
        Refresh();
    }

    /// <summary>Nome do enum FluentIcons (ex.: "Cube24").</summary>
    public string? IconName
    {
        get => (string?)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    private void Refresh() =>
        _icon.Icon = Enum.TryParse<FluentIcons>(IconName, out var icon) ? icon : PlaceholderIcon;
}

/// <summary>Nome do ícone (ex.: "Sparkle24") para o enum FluentIcons do MauiIcons.</summary>
public sealed class FluentIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Enum.TryParse<FluentIcons>(value as string, out var icon) ? icon : ComponentThumb.PlaceholderIcon;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Cor do ponto de origem (token Origin{Origem}) como pincel.</summary>
public sealed class OriginToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value is ComponentOrigin origin ? $"Origin{origin}" : "OriginNative";
        return new SolidColorBrush(ThemeColors.Resolve(key, Colors.Gray));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Cor do nível (Base, Estilizado, Custom, Biblioteca): a mesma em todo o app.</summary>
public sealed class TierToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = OriginBadge.ColorKey(TierToBadgeKindConverter.KindFor(value));
        return new SolidColorBrush(ThemeColors.Resolve(key, Colors.Gray));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Nível do Core para o badge da Lib (o ponto ganha a cor do nível).</summary>
public sealed class TierToBadgeKindConverter : IValueConverter
{
    public static BadgeKind KindFor(object? value) => value switch
    {
        VariantTier.Styled => BadgeKind.Styled,
        VariantTier.Custom => BadgeKind.Custom,
        VariantTier.Library => BadgeKind.Library,
        _ => BadgeKind.Base,
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => KindFor(value);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Converte a origem do Core para o tipo de badge da Lib (mesmos nomes; a Lib não conhece o Core).</summary>
public sealed class OriginToBadgeKindConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ComponentOrigin origin && Enum.TryParse<BadgeKind>(origin.ToString(), out var kind) ? kind : BadgeKind.Native;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
