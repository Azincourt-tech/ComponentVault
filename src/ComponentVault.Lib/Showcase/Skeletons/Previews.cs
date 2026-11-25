namespace ComponentVault.Lib.Showcase.Skeletons;

[ShowcasePreview("skeleton.custom")]
public partial class SkeletonCustomPreview : ContentView
{
    private const string PulseAnimation = "SkeletonPulse";
    private bool _loaded;

    public SkeletonCustomPreview()
    {
        InitializeComponent();
        Loaded += (_, _) => { if (!_loaded) StartPulse(); };
        Unloaded += (_, _) => StopPulse();
    }

    // Pulso lento (2,8s por ciclo, ida e volta suave): sinaliza "carregando" sem chamar atenção.
    private void StartPulse()
    {
        StopPulse();
        var pulse = new Animation
        {
            { 0, 0.5, new Animation(v => Skeleton.Opacity = v, 1, 0.45, Easing.SinInOut) },
            { 0.5, 1, new Animation(v => Skeleton.Opacity = v, 0.45, 1, Easing.SinInOut) },
        };
        pulse.Commit(this, PulseAnimation, length: 2800, repeat: () => !_loaded);
    }

    private void StopPulse()
    {
        this.AbortAnimation(PulseAnimation);
        Skeleton.Opacity = 1;
    }

    private async void OnToggleClicked(object? sender, EventArgs e)
    {
        _loaded = !_loaded;
        ToggleButton.Text = _loaded ? "Recarregar" : "Mostrar conteúdo";

        if (_loaded)
        {
            StopPulse();
            Skeleton.IsVisible = false;
            LoadedContent.IsVisible = true;
            await LoadedContent.FadeToAsync(1, 200, Easing.CubicOut);
            StatusLabel.Text = "Conteúdo carregado no lugar exato dos placeholders.";
        }
        else
        {
            LoadedContent.Opacity = 0;
            LoadedContent.IsVisible = false;
            Skeleton.IsVisible = true;
            StatusLabel.Text = "Carregando com placeholders que imitam o layout final.";
            StartPulse();
        }
    }
}
