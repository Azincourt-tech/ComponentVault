using CommunityToolkit.Maui;
using ComponentVault.App.Services;
using ComponentVault.App.Views.Catalog;
using ComponentVault.App.Views.Detail;
using ComponentVault.App.Views.Home;
using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Services.Highlighting;
using ComponentVault.Core.Services.Metadata;
using ComponentVault.Core.Services.Preferences;
using ComponentVault.Core.Services.Snippets;
using ComponentVault.Core.Services.Theming;
using ComponentVault.Core.ViewModels;
using ComponentVault.Lib.Hosting;
using Microsoft.Extensions.Logging;

namespace ComponentVault.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseVaultDesignSystem();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var services = builder.Services;

        // Core: serviços sem dependência de MAUI
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IComponentMetadataService, ComponentMetadataService>();
        services.AddSingleton<ISnippetSource, EmbeddedSnippetSource>();
        services.AddSingleton<ISnippetRepository, SnippetRepository>();
        services.AddSingleton<ISyntaxHighlighter, ColorCodeHighlighter>();
        services.AddSingleton<IPreferenceStore, SqlitePreferenceStore>();
        services.AddSingleton<IThemeService, ThemeService>();

        // Implementações MAUI das abstrações do Core
        services.AddSingleton<IAppPaths, MauiAppPaths>();
        services.AddSingleton<IThemeApplier, MauiThemeApplier>();
        services.AddSingleton<IClipboardService, MauiClipboardService>();
        services.AddSingleton<INavigationService, ShellNavigationService>();

        // ViewModels e páginas. O catálogo é singleton para manter busca, filtro e scroll ao voltar.
        services.AddSingleton<AppearanceViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddSingleton<CatalogViewModel>();
        services.AddTransient<ComponentDetailViewModel>();

        services.AddSingleton<AppShell>();
        services.AddTransient<HomePage>();
        services.AddSingleton<CatalogPage>();
        services.AddTransient<ComponentDetailPage>();

        return builder.Build();
    }
}
