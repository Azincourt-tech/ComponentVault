namespace ComponentVault.Lib.Showcase.Progress;

[ShowcasePreview("progress.base")]
public partial class ProgressBasePreview : ContentView
{
    public ProgressBasePreview() => InitializeComponent();

    private async void OnAnimateClicked(object? sender, EventArgs e)
    {
        Bar.Progress = 0;
        await Bar.ProgressTo(1, 1500, Easing.Linear);
    }
}

[ShowcasePreview("progress.styled")]
public partial class ProgressStyledPreview : ContentView
{
    private const double TotalMb = 5;
    private bool _uploading;

    public ProgressStyledPreview()
    {
        InitializeComponent();
        UploadBar.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(ProgressBar.Progress)) return;
            var p = UploadBar.Progress;
            PercentLabel.Text = $"{p:P0}";
            DetailLabel.Text = p >= 1
                ? "Concluído"
                : $"{p * TotalMb:0.0} MB de {TotalMb:0} MB · cerca de {Math.Ceiling((1 - p) * 3)} s";
        };
        Unloaded += (_, _) => UploadBar.AbortAnimation("Progress");
        // Abre já enviando: uma barra parada em 0% não mostra nada do que o estilo faz.
        Loaded += (_, _) => Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () => OnUploadClicked(null, EventArgs.Empty));
    }

    // Simulação: num app real o progresso vem do serviço de upload (IProgress<double>).
    private async void OnUploadClicked(object? sender, EventArgs e)
    {
        if (_uploading) return;

        _uploading = true;
        UploadButton.IsEnabled = false;
        UploadBar.Progress = 0;
        await UploadBar.ProgressTo(1, 3000, Easing.Linear);
        UploadButton.Text = "Enviar de novo";
        UploadButton.IsEnabled = true;
        _uploading = false;
    }
}

[ShowcasePreview("progress.custom")]
public partial class ProgressCustomPreview : ContentView
{
    public ProgressCustomPreview() => InitializeComponent();

    private void OnStorageChanged(object? sender, ValueChangedEventArgs e)
    {
        StorageRing.Progress = e.NewValue / 100;
        StorageLabel.Text = $"{e.NewValue:0}%";
    }
}

[ShowcasePreview("progress.syncfusion")]
public partial class ProgressSyncfusionPreview : ContentView
{
    public ProgressSyncfusionPreview() => InitializeComponent();
}
