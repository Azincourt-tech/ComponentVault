using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Preferences;

namespace ComponentVault.Tests.Fakes;

internal sealed class FakeNavigation : INavigationService
{
    public List<(string Route, IReadOnlyDictionary<string, object>? Parameters)> Calls { get; } = [];

    public Task GoToAsync(string route, IReadOnlyDictionary<string, object>? parameters = null)
    {
        Calls.Add((route, parameters));
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        Calls.Add(("..", null));
        return Task.CompletedTask;
    }
}

internal sealed class FakeClipboard : IClipboardService
{
    public string? Text { get; private set; }

    public Task SetTextAsync(string text)
    {
        Text = text;
        return Task.CompletedTask;
    }
}

internal sealed class FakeThemeApplier : IThemeApplier
{
    public List<AppThemePreference> Applied { get; } = [];

    /// <summary>Simula o "System" do SO; Light/Dark explícitos vencem.</summary>
    public bool SystemIsDark { get; set; }

    public bool IsDarkActive => Applied.Count == 0 || Applied[^1] == AppThemePreference.System
        ? SystemIsDark
        : Applied[^1] != AppThemePreference.Light;

    public void Apply(AppThemePreference preference) => Applied.Add(preference);
}

internal sealed class InMemoryPreferenceStore : IPreferenceStore
{
    public Dictionary<string, string> Values { get; } = [];

    public Task<string?> GetAsync(string key) => Task.FromResult(Values.GetValueOrDefault(key));

    public Task SetAsync(string key, string value)
    {
        Values[key] = value;
        return Task.CompletedTask;
    }
}

internal sealed class FakeSnippetSource(params SnippetFile[] files) : ISnippetSource
{
    public IEnumerable<SnippetFile> GetFiles() => files;
}

internal sealed class TempDirectory : IAppPaths, IDisposable
{
    public string AppDataDirectory { get; } = Directory.CreateTempSubdirectory("cv-tests-").FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(AppDataDirectory, recursive: true);
        }
        catch (IOException)
        {
            // O SQLite pode segurar o arquivo por alguns ms; a pasta é temporária de qualquer jeito.
        }
    }
}
