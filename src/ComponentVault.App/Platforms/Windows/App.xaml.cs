namespace ComponentVault.App.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();

        // Exceções não tratadas do WinUI chegam como 0xc000027b, sem detalhe no log de eventos.
        // Gravamos a exceção gerenciada num arquivo legível para conseguir diagnosticar.
        UnhandledException += (_, e) => WriteCrashLog(e.Exception);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private static void WriteCrashLog(Exception exception)
    {
        try
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ComponentVault");
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "crash.log"),
                $"[{DateTime.Now:O}] {exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Sem disco ou sem permissão: não há o que fazer, e não vale mascarar a exceção original.
        }
    }
}
