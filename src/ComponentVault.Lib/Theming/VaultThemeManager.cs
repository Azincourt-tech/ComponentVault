using ComponentVault.Lib.Resources.Theme;
using Syncfusion.Maui.Toolkit.Themes;

namespace ComponentVault.Lib.Theming;

public enum VaultPalette
{
    Light,
    Dark,
    Glass,
}

/// <summary>
/// Troca a paleta ativa em tempo de execução. O MAUI só conhece claro/escuro (AppThemeBinding);
/// para ter um terceiro tema, as cores vêm de um dicionário trocável consumido por DynamicResource.
/// </summary>
public static class VaultThemeManager
{
    public static VaultPalette Current { get; private set; } = VaultPalette.Light;

    /// <param name="palette">Paleta a aplicar.</param>
    /// <param name="followSystem">
    /// Verdadeiro quando o usuário escolheu "Sistema": a paleta acompanha o SO e o tema nativo
    /// fica sem preferência (Unspecified).
    /// </param>
    public static void Apply(VaultPalette palette, bool followSystem = false)
    {
        if (Application.Current is not { } app) return;

        var merged = app.Resources.MergedDictionaries;
        foreach (var old in merged.Where(IsPalette).ToArray()) merged.Remove(old);
        merged.Add(Create(palette));

        // Tema dos controles do Syncfusion Toolkit: claro no Pérola, escuro no Escuro e no Vidro (sem isto,
        // a lib usa sempre o claro e o texto some nos temas escuros). Entra no início da lista para as
        // nossas chaves (as Primary* do Colors.xaml, no Cayenne) continuarem valendo por cima das dele.
        // MergedDictionaries não tem Insert: refaz a lista com o do Syncfusion primeiro.
        var ours = merged.Where(d => d is not SyncfusionThemeResourceDictionary).ToArray();
        merged.Clear();
        merged.Add(new SyncfusionThemeResourceDictionary
        {
            VisualTheme = palette == VaultPalette.Light ? SfVisuals.MaterialLight : SfVisuals.MaterialDark,
        });
        foreach (var dictionary in ours) merged.Add(dictionary);

        // Controles nativos (Switch, Picker, barra de título) seguem claro/escuro;
        // o Vidro é um tema escuro.
        app.UserAppTheme = followSystem
            ? AppTheme.Unspecified
            : palette == VaultPalette.Light ? AppTheme.Light : AppTheme.Dark;
        Current = palette;
        ThemeColors.RaisePaletteChanged(palette == VaultPalette.Light ? AppTheme.Light : AppTheme.Dark);
    }

    private static bool IsPalette(ResourceDictionary dictionary) =>
        dictionary is LightPalette or DarkPalette or GlassPalette;

    private static ResourceDictionary Create(VaultPalette palette) => palette switch
    {
        VaultPalette.Dark => new DarkPalette(),
        VaultPalette.Glass => new GlassPalette(),
        _ => new LightPalette(),
    };
}
