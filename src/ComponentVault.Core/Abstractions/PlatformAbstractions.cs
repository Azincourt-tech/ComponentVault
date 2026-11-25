namespace ComponentVault.Core.Abstractions;

// Contratos que a camada MAUI implementa. Mantê-los aqui deixa as ViewModels
// livres de Microsoft.Maui.* e substituíveis por dublês nos testes.

public interface INavigationService
{
    Task GoToAsync(string route, IReadOnlyDictionary<string, object>? parameters = null);

    Task GoBackAsync();
}

/// <summary>ViewModels que recebem parâmetros de navegação (substitui IQueryAttributable, que é do MAUI).</summary>
public interface INavigationAware
{
    void OnNavigatedTo(IReadOnlyDictionary<string, object> parameters);
}

public interface IClipboardService
{
    Task SetTextAsync(string text);
}

/// <summary>Aplica o tema escolhido na aplicação em execução.</summary>
public interface IThemeApplier
{
    /// <summary>Tema efetivamente visível agora (resolve "System" para claro ou escuro).</summary>
    bool IsDarkActive { get; }

    void Apply(Models.AppThemePreference preference);
}

public interface IAppPaths
{
    /// <summary>Pasta gravável e privada do app (FileSystem.AppDataDirectory no MAUI).</summary>
    string AppDataDirectory { get; }
}

/// <summary>Arquivos de código-fonte de onde os snippets são extraídos.</summary>
public interface ISnippetSource
{
    IEnumerable<SnippetFile> GetFiles();
}

public sealed record SnippetFile(string Path, string Content);
