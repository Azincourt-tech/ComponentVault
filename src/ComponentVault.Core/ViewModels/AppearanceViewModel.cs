using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComponentVault.Core.Models;
using ComponentVault.Core.Presentation;
using ComponentVault.Core.Services.Theming;

namespace ComponentVault.Core.ViewModels;

/// <summary>Seletor de aparência (Claro · Escuro · Vidro), compartilhado entre as telas.</summary>
public sealed partial class AppearanceViewModel : ObservableObject
{
    private readonly IThemeService _theme;

    public AppearanceViewModel(IThemeService theme)
    {
        _theme = theme;
        Options =
        [
            new ThemeOption(AppThemePreference.Light, Labels.Theme(AppThemePreference.Light), "WeatherSunny24"),
            new ThemeOption(AppThemePreference.Dark, Labels.Theme(AppThemePreference.Dark), "WeatherMoon24"),
            new ThemeOption(AppThemePreference.Glass, Labels.Theme(AppThemePreference.Glass), "Sparkle24"),
        ];

        theme.ThemeChanged += (_, _) => Refresh();
        Refresh();
    }

    public IReadOnlyList<ThemeOption> Options { get; }

    [RelayCommand]
    private Task SelectAsync(ThemeOption? option) =>
        option is null ? Task.CompletedTask : _theme.SetThemeAsync(option.Preference);

    [RelayCommand]
    private Task CycleAsync() => _theme.ToggleAsync();

    /// <summary>Atualiza a marcação; em "Sistema" nenhuma opção fica marcada até o usuário escolher.</summary>
    public void Refresh()
    {
        foreach (var option in Options) option.IsSelected = option.Preference == _theme.Current;
    }
}

public sealed partial class ThemeOption(AppThemePreference preference, string label, string icon) : ObservableObject
{
    public AppThemePreference Preference { get; } = preference;

    public string Label { get; } = label;

    /// <summary>Nome do ícone Fluent.</summary>
    public string Icon { get; } = icon;

    [ObservableProperty]
    private bool _isSelected;
}
