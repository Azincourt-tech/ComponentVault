using System.Reflection;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Lib.Showcase;
using ComponentVault.Lib.Theming;

namespace ComponentVault.App.Services;

public sealed class MauiAppPaths : IAppPaths
{
    public string AppDataDirectory => FileSystem.Current.AppDataDirectory;
}

public sealed class MauiClipboardService : IClipboardService
{
    public Task SetTextAsync(string text) => Clipboard.Default.SetTextAsync(text);
}

/// <summary>Traduz a preferência do usuário para a paleta do design system (Claro, Escuro, Vidro).</summary>
public sealed class MauiThemeApplier : IThemeApplier
{
    private bool _followSystem;
    private bool _subscribed;

    public bool IsDarkActive => VaultThemeManager.Current != VaultPalette.Light;

    public void Apply(AppThemePreference preference)
    {
        // A leitura da preferência termina fora da UI thread; a troca de tema precisa voltar para ela.
        // Usa o Dispatcher da Application (capturado na UI thread): no Windows, MainThread.BeginInvoke
        // chamado de uma thread de fundo durante a abertura falha com "Unable to find main thread".
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || !dispatcher.IsDispatchRequired)
        {
            ApplyOnMainThread(preference);
        }
        else
        {
            dispatcher.Dispatch(() => ApplyOnMainThread(preference));
        }
    }

    private void ApplyOnMainThread(AppThemePreference preference)
    {
        if (Application.Current is not { } app) return;

        _followSystem = preference == AppThemePreference.System;
        var palette = preference switch
        {
            AppThemePreference.Light => VaultPalette.Light,
            AppThemePreference.Dark => VaultPalette.Dark,
            AppThemePreference.Glass => VaultPalette.Glass,
            _ => app.PlatformAppTheme == AppTheme.Dark ? VaultPalette.Dark : VaultPalette.Light,
        };
        VaultThemeManager.Apply(palette, _followSystem);

        if (!_subscribed)
        {
            app.RequestedThemeChanged += OnSystemThemeChanged;
            _subscribed = true;
        }
    }

    // Só importa no modo "Sistema": o SO trocou de claro para escuro (ou o contrário).
    private void OnSystemThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (!_followSystem) return;
        var palette = e.RequestedTheme == AppTheme.Dark ? VaultPalette.Dark : VaultPalette.Light;
        if (palette != VaultThemeManager.Current) VaultThemeManager.Apply(palette, followSystem: true);
    }
}

public sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        parameters is null
            ? Shell.Current.GoToAsync(route)
            : Shell.Current.GoToAsync(route, new ShellNavigationQueryParameters(parameters));

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}

/// <summary>
/// Lê o código-fonte que a Lib embute como recurso ("snippets/..."), de onde saem os snippets
/// exibidos. Assim o texto na aba Código é sempre o do componente compilado.
/// </summary>
public sealed class EmbeddedSnippetSource : ISnippetSource
{
    private const string Prefix = "snippets/";

    // Os arquivos entram com ".src" no fim: o XamlC descarta, no Release, todo recurso terminado
    // em .xaml (ver ComponentVault.Lib.csproj).
    private const string Suffix = ".src";

    public IEnumerable<SnippetFile> GetFiles()
    {
        var assembly = typeof(PreviewRegistry).Assembly;
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.StartsWith(Prefix, StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null) continue;

            using var reader = new StreamReader(stream);
            var path = name[Prefix.Length..];
            if (path.EndsWith(Suffix, StringComparison.Ordinal)) path = path[..^Suffix.Length];
            yield return new SnippetFile(path, reader.ReadToEnd());
        }
    }
}
