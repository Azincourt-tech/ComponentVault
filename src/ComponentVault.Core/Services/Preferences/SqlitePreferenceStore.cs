using ComponentVault.Core.Abstractions;
using SQLite;

namespace ComponentVault.Core.Services.Preferences;

public interface IPreferenceStore
{
    Task<string?> GetAsync(string key);

    Task SetAsync(string key, string value);
}

/// <summary>
/// Preferências chave-valor em SQLite (ADR-003). A conexão abre na primeira chamada,
/// fora do caminho de inicialização da UI.
/// </summary>
public sealed class SqlitePreferenceStore : IPreferenceStore, IAsyncDisposable
{
    public const string DatabaseFileName = "componentvault.db3";

    private readonly string _databasePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public SqlitePreferenceStore(IAppPaths paths)
        : this(Path.Combine(paths.AppDataDirectory, DatabaseFileName))
    {
    }

    internal SqlitePreferenceStore(string databasePath) => _databasePath = databasePath;

    public async Task<string?> GetAsync(string key)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        var row = await db.FindAsync<PreferenceRow>(key).ConfigureAwait(false);
        return row?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var db = await GetConnectionAsync().ConfigureAwait(false);
        await db.InsertOrReplaceAsync(new PreferenceRow { Key = key, Value = value }).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync().ConfigureAwait(false);
        }

        _gate.Dispose();
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null) return _connection;

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_connection is null)
            {
                var connection = new SQLiteAsyncConnection(
                    _databasePath,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
                await connection.CreateTableAsync<PreferenceRow>().ConfigureAwait(false);
                _connection = connection;
            }

            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    [Table("preferences")]
    internal sealed class PreferenceRow
    {
        [PrimaryKey]
        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string Value { get; set; } = string.Empty;
    }
}
