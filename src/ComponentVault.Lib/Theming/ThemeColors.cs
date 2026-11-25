namespace ComponentVault.Lib.Theming;

/// <summary>
/// Acesso em C# aos tokens de cor do design system. As cores que variam por tema vivem na paleta
/// ativa (VaultThemeManager); em código usamos SetDynamicResource, que acompanha a troca ao vivo.
/// </summary>
public static class ThemeColors
{
    /// <summary>
    /// Disparado depois de trocar a paleta. Desenhos em GraphicsView/SkiaSharp (que leem cores
    /// na hora de pintar) assinam isto para se redesenhar.
    /// </summary>
    public static event EventHandler<AppThemeChangedEventArgs>? PaletteChanged;

    internal static void RaisePaletteChanged(AppTheme theme) =>
        PaletteChanged?.Invoke(null, new AppThemeChangedEventArgs(theme));

    /// <summary>Cor do token na paleta ativa.</summary>
    public static Color Resolve(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true) return fallback;

        return value switch
        {
            Color color => color,
            SolidColorBrush brush => brush.Color,
            _ => fallback,
        };
    }

    /// <summary>Liga a propriedade ao token: a cor troca sozinha quando o tema muda.</summary>
    public static void SetThemeColor(this Element target, BindableProperty property, string key) =>
        target.SetDynamicResource(property, key);

    /// <summary>Igual a SetThemeColor, para propriedades do tipo Brush (Stroke, Fill).</summary>
    public static void SetThemeBrush(this Element target, BindableProperty property, string key) =>
        target.SetDynamicResource(property, key);
}
