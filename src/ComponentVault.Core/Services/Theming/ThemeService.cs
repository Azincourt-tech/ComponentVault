using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Preferences;

namespace ComponentVault.Core.Services.Theming;

public interface IThemeService
{
    AppThemePreference Current { get; }

    event EventHandler<AppThemePreference>? ThemeChanged;

    /// <summary>Lê a preferência salva e aplica. Chamado uma vez na subida do app.</summary>
    Task InitializeAsync();

    Task SetThemeAsync(AppThemePreference preference);

    /// <summary>Avança para o próximo tema: Claro → Escuro → Vidro → Claro.</summary>
    Task ToggleAsync();
}

public sealed class ThemeService(IPreferenceStore store, IThemeApplier applier) : IThemeService
{
    internal const string PreferenceKey = "theme";

    public AppThemePreference Current { get; private set; } = AppThemePreference.System;

    public event EventHandler<AppThemePreference>? ThemeChanged;

    public async Task InitializeAsync()
    {
        var saved = await store.GetAsync(PreferenceKey).ConfigureAwait(false);
        Current = Enum.TryParse<AppThemePreference>(saved, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : AppThemePreference.System;
        applier.Apply(Current);
        ThemeChanged?.Invoke(this, Current);
    }

    public async Task SetThemeAsync(AppThemePreference preference)
    {
        if (preference == Current) return;

        // Aplica antes de persistir: a troca visual não espera o disco (critério de < 200ms).
        Current = preference;
        applier.Apply(preference);
        ThemeChanged?.Invoke(this, preference);
        await store.SetAsync(PreferenceKey, preference.ToString()).ConfigureAwait(false);
    }

    public Task ToggleAsync()
    {
        // "Sistema" parte do tema visível no momento.
        var visible = Current == AppThemePreference.System
            ? applier.IsDarkActive ? AppThemePreference.Dark : AppThemePreference.Light
            : Current;

        return SetThemeAsync(visible switch
        {
            AppThemePreference.Light => AppThemePreference.Dark,
            AppThemePreference.Dark => AppThemePreference.Glass,
            _ => AppThemePreference.Light,
        });
    }
}
