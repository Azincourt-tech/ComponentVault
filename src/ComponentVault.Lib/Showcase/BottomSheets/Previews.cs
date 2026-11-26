using ComponentVault.Lib.Theming;
using Syncfusion.Maui.Toolkit.BottomSheet;

namespace ComponentVault.Lib.Showcase.BottomSheets;

[ShowcasePreview("bottomsheet.base")]
public partial class BottomSheetBasePreview : ContentView
{
    public BottomSheetBasePreview() => InitializeComponent();

    private void OnToggleClicked(object? sender, EventArgs e) => Sheet.IsVisible = !Sheet.IsVisible;
}

[ShowcasePreview("bottomsheet.styled")]
public partial class BottomSheetStyledPreview : ContentView
{
    public BottomSheetStyledPreview()
    {
        InitializeComponent();
        // Abre sozinha ao carregar, para o preview já mostrar a folha.
        Loaded += (_, _) => Open();
    }

    private void OnOpenClicked(object? sender, EventArgs e) => Open();

    private void OnCloseTapped(object? sender, TappedEventArgs e) => Close();

    private void OnCloseClicked(object? sender, EventArgs e) => Close();

    private async void Open()
    {
        if (Sheet.IsVisible) return;
        Scrim.IsVisible = Sheet.IsVisible = true;
        await Task.Yield(); // mede a folha antes de posicioná-la logo abaixo da borda
        Sheet.TranslationY = Math.Max(Sheet.Height, 240);
        await Task.WhenAll(Scrim.FadeToAsync(0.4, 220), Sheet.TranslateToAsync(0, 0, 260, Easing.CubicOut));
    }

    private async void Close()
    {
        if (!Sheet.IsVisible) return;
        await Task.WhenAll(Scrim.FadeToAsync(0, 180), Sheet.TranslateToAsync(0, Math.Max(Sheet.Height, 240), 200, Easing.CubicIn));
        Scrim.IsVisible = Sheet.IsVisible = false;
    }
}

[ShowcasePreview("bottomsheet.custom")]
public partial class BottomSheetCustomPreview : ContentView
{
    public BottomSheetCustomPreview()
    {
        InitializeComponent();
        OnStopChanged(this, EventArgs.Empty);
    }

    private void OnStopChanged(object? sender, EventArgs e)
    {
        if (StopLabel is null) return;
        StopLabel.Text = Sheet.Stop switch
        {
            SheetStop.Peek => "Recolhido: só o resumo. Arraste o cabeçalho para cima.",
            SheetStop.Half => "Meia altura: resumo, começo da lista e o mapa ainda visível.",
            _ => "Cheio: a lista inteira; toque no fundo escuro para voltar à metade.",
        };
    }
}

[ShowcasePreview("bottomsheet.syncfusion")]
public partial class BottomSheetSyncfusionPreview : ContentView
{
    public BottomSheetSyncfusionPreview()
    {
        InitializeComponent();
        ApplyColors();
        Loaded += (_, _) =>
        {
            ThemeColors.PaletteChanged += OnPaletteChanged;
            Sheet.IsOpen = true;
        };
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
    }

    private void OnOpenClicked(object? sender, EventArgs e)
    {
        Sheet.State = BottomSheetState.HalfExpanded;
        Sheet.IsOpen = true;
    }

    private void OnStateChanged(object? sender, StateChangedEventArgs e) => StateLabel.Text = e.NewState switch
    {
        BottomSheetState.FullExpanded => "Cheio",
        BottomSheetState.HalfExpanded => "Meia altura",
        BottomSheetState.Collapsed => "Recolhido",
        _ => "Fechado: toque em Ver rota para abrir",
    };

    // Background e GrabberBackground são Brush: o DynamicResource de cor não serve, então vão pelo código.
    private void ApplyColors()
    {
        Sheet.Background = new SolidColorBrush(ThemeColors.Resolve("SurfaceRaised", Colors.White));
        Sheet.GrabberBackground = new SolidColorBrush(ThemeColors.Resolve("HairlineStrong", Colors.Gray));
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => ApplyColors();
}
