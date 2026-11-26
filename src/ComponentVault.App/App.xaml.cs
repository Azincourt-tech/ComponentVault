using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Theming;
using ComponentVault.Lib.Theming;
using Microsoft.Extensions.Logging;

namespace ComponentVault.App;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services, IThemeService theme, ILogger<App> logger)
    {
        InitializeComponent();
        _services = services;

        // Paleta inicial pelo tema do SO, antes de qualquer tela existir (sem piscar);
        // a preferência salva é aplicada logo em seguida.
        VaultThemeManager.Apply(PlatformAppTheme == AppTheme.Dark ? VaultPalette.Dark : VaultPalette.Light, followSystem: true);

        // Restaura o tema salvo sem segurar a abertura do app (I/O fora da UI thread).
        _ = RestoreThemeAsync(theme, logger);
    }

    // O Shell é resolvido aqui, e não no construtor: as páginas usam recursos do App.xaml,
    // que só existem depois do InitializeComponent acima.
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _services.GetRequiredService<AppShell>();
        var window = new Window(shell)
        {
            Title = "ComponentVault",
            MinimumWidth = 360,
            MinimumHeight = 560,
        };

#if WINDOWS
        // Barra de título com as cores da paleta: a nativa não acompanha a troca de tema em
        // execução e deixava o título branco sobre cinza claro no tema Claro.
        var titleBar = new TitleBar { Title = "ComponentVault", HeightRequest = 40 };
        titleBar.SetThemeColor(VisualElement.BackgroundColorProperty, "Ground");
        titleBar.SetThemeColor(TitleBar.ForegroundColorProperty, "InkSecondary");
        window.TitleBar = titleBar;
#endif

        // Link profundo para desenvolvimento e capturas automatizadas, ex.:
        // COMPONENTVAULT_START="catalog" ou "catalog/component?id=button&variant=button.styled".
        var start = Environment.GetEnvironmentVariable("COMPONENTVAULT_START");
        if (!string.IsNullOrWhiteSpace(start))
        {
            shell.Loaded += async (_, _) => await shell.GoToAsync(start);
        }

        return window;
    }

    private static async Task RestoreThemeAsync(IThemeService theme, ILogger logger)
    {
        try
        {
            await theme.InitializeAsync();

            // Desenvolvimento/capturas: força um tema nesta execução, sem salvar a preferência.
            // Passa pelo serviço para o seletor de tema marcar o tema certo.
            var forced = Environment.GetEnvironmentVariable("COMPONENTVAULT_THEME");
            if (Enum.TryParse<AppThemePreference>(forced, ignoreCase: true, out var preference)
                && preference != AppThemePreference.System)
            {
                theme.Preview(preference);
            }
        }
        catch (Exception ex)
        {
            // Sem preferência salva o app segue no tema do sistema; não vale derrubar a abertura.
            logger.LogWarning(ex, "Não foi possível restaurar o tema salvo.");
        }
    }
}
