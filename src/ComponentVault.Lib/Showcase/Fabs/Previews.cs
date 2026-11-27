using MauiIcons.Fluent;

namespace ComponentVault.Lib.Showcase.Fabs;

[ShowcasePreview("fab.base")]
public partial class FabBasePreview : ContentView
{
    public FabBasePreview() => InitializeComponent();

    private async void OnFabClicked(object? sender, EventArgs e)
    {
        if (Popups.PageLocator.FindPage(this) is not { } page) return;
        var choice = await page.DisplayActionSheetAsync("Criar", "Cancelar", null, "Nota", "Foto", "Lembrete");
        ResultLabel.Text = choice is null or "Cancelar" ? "Nada criado" : $"Criar: {choice}";
    }
}

[ShowcasePreview("fab.styled")]
public partial class FabStyledPreview : ContentView
{
    private double _lastY;
    private bool _extended = true;

    public FabStyledPreview() => InitializeComponent();

    private void OnFabTapped(object? sender, TappedEventArgs e) => ResultLabel.Text = "Nova nota criada.";

    // Rolar para baixo recolhe o rótulo; rolar para cima (ou voltar ao topo) estende de novo.
    private void OnScrolled(object? sender, ScrolledEventArgs e)
    {
        var down = e.ScrollY > _lastY + 4;
        var up = e.ScrollY < _lastY - 4 || e.ScrollY <= 0;
        if (down || up) _lastY = e.ScrollY;
        if (down && _extended) SetExtended(false);
        else if (up && !_extended) SetExtended(true);
    }

    private async void SetExtended(bool extended)
    {
        _extended = extended;
        if (extended)
        {
            Fab.Padding = new Thickness(18, 0);
            FabLabel.IsVisible = true;
            await FabLabel.FadeToAsync(1, 140);
        }
        else
        {
            await FabLabel.FadeToAsync(0, 100);
            if (_extended) return;
            FabLabel.IsVisible = false;
            Fab.Padding = new Thickness(17, 0); // 17 + ícone de 22 + 17 = círculo de 56
        }
    }
}

[ShowcasePreview("fab.custom")]
public partial class FabCustomPreview : ContentView
{
    public FabCustomPreview()
    {
        InitializeComponent();
        Dial.Actions =
        [
            new SpeedDialAction { Icon = FluentIcons.Alert20, Label = "Lembrete" },
            new SpeedDialAction { Icon = FluentIcons.Camera20, Label = "Foto" },
            new SpeedDialAction { Icon = FluentIcons.Note20, Label = "Nota" },
        ];

        // Abre sozinho na primeira vez, para o preview já mostrar o leque.
        var shown = false;
        Loaded += (_, _) =>
        {
            if (shown) return;
            shown = true;
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), Dial.Open);
        };
    }

    private void OnActionInvoked(object? sender, SpeedDialAction action) => ResultLabel.Text = $"Criar: {action.Label}";
}
