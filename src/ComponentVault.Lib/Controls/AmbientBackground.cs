using ComponentVault.Lib.Theming;
using Microsoft.Maui.Layouts;

namespace ComponentVault.Lib.Controls;

// snippet:control.ambient
/// <summary>
/// Fundo ambiente: manchas de luz (gradientes radiais) sobre o fundo da página, que derivam
/// devagar. É o que dá leitura de vidro às superfícies translúcidas por cima, sem desfoque.
/// As manchas vêm da paleta (AmbientBlue/Teal/Rose): só o tema Vidro as acende.
/// </summary>
public class AmbientBackground : AbsoluteLayout
{
    private const string DriftAnimation = "AmbientDrift";

    private readonly (Grid View, string BrushKey, double PhaseX, double PhaseY)[] _blobs;

    public AmbientBackground()
    {
        InputTransparent = true;
        CascadeInputTransparent = true;
        this.SetThemeColor(BackgroundColorProperty, "Ground");

        // Posição e tamanho proporcionais à página: x, y, largura, altura.
        _blobs =
        [
            (Blob(new Rect(-0.2, -0.25, 0.9, 0.8)), "AmbientBlue", 0.0, 1.3),
            (Blob(new Rect(1.2, 0.2, 0.75, 0.7)), "AmbientTeal", 2.1, 0.4),
            (Blob(new Rect(0.3, 1.25, 0.95, 0.75)), "AmbientRose", 4.2, 2.8),
        ];

        Loaded += (_, _) =>
        {
            ThemeColors.PaletteChanged += OnPaletteChanged;
            Paint();
            StartDrift();
        };
        Unloaded += (_, _) =>
        {
            ThemeColors.PaletteChanged -= OnPaletteChanged;
            this.AbortAnimation(DriftAnimation);
        };
    }

    private Grid Blob(Rect bounds)
    {
        // Grid aceita pincel no Background; BoxView pinta só a Color e ignora o degradê.
        var blob = new Grid { InputTransparent = true };
        SetLayoutFlags((IView)blob, AbsoluteLayoutFlags.All);
        SetLayoutBounds((IView)blob, bounds);
        Add(blob);
        return blob;
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Paint();

    // Um Brush só pode ter um "pai" no MAUI: cada mancha recebe a SUA cópia do pincel da paleta,
    // senão a segunda tela que usar o mesmo pincel fica sem nada.
    private void Paint()
    {
        foreach (var (view, key, _, _) in _blobs)
        {
            view.Background = Application.Current?.Resources.TryGetValue(key, out var value) == true && value is GradientBrush source
                ? Clone(source)
                : null;
        }
    }

    private static RadialGradientBrush Clone(GradientBrush source)
    {
        var copy = new RadialGradientBrush
        {
            Center = (source as RadialGradientBrush)?.Center ?? new Point(0.5, 0.5),
            Radius = (source as RadialGradientBrush)?.Radius ?? 0.5,
        };
        foreach (var stop in source.GradientStops) copy.GradientStops.Add(new GradientStop(stop.Color, stop.Offset));
        return copy;
    }

    // Deriva lenta (ciclo de 24s) para o fundo parecer vivo sem disputar atenção com o conteúdo.
    private void StartDrift()
    {
        this.AbortAnimation(DriftAnimation);
        var drift = new Animation(t =>
        {
            var angle = t * Math.PI * 2;
            // Translation move só o desenho: não dispara layout a cada quadro.
            foreach (var (view, _, px, py) in _blobs)
            {
                view.TranslationX = Math.Sin(angle + px) * Width * 0.06;
                view.TranslationY = Math.Cos(angle + py) * Height * 0.05;
            }
        });
        drift.Commit(this, DriftAnimation, rate: 32, length: 24000, easing: Easing.Linear, repeat: () => true);
    }
}
// /snippet:control.ambient
