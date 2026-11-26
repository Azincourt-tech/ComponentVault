using ComponentVault.Lib.Theming;
using Microsoft.Maui.Layouts;

namespace ComponentVault.Lib.Controls;

// snippet:control.ambient
/// <summary>
/// Fundo ambiente em forma de galáxia: manchas de luz (gradientes radiais) que fluem como líquido,
/// como uma nebulosa, e um campo de estrelas que pisca e gira devagar. Cada mancha percorre a sua
/// curva, se deforma e gira, e as cores se misturam onde se cruzam. É o que dá leitura de vidro às
/// superfícies translúcidas por cima, sem desfoque.
/// As manchas vêm da paleta (AmbientBlue/Teal/Rose). O Vidro tem o movimento mais amplo; no Claro e
/// no Escuro ele é mais contido, para não disputar atenção com o conteúdo. As estrelas só aparecem
/// nos temas escuros.
/// </summary>
public class AmbientBackground : AbsoluteLayout
{
    private const string FlowAnimation = "AmbientFlow";
    // Ciclo completo. As frequências de cada mancha são inteiras, então o fim encontra o começo sem salto.
    private const uint CycleMs = 42000;

    private readonly Blob[] _blobs;
    private readonly GraphicsView _stars;
    private readonly StarField _field = new();
    private double _intensity = 1;

    /// <param name="View">A mancha.</param>
    /// <param name="BrushKey">Chave do pincel radial na paleta.</param>
    /// <param name="Fx">Voltas no eixo X por ciclo.</param>
    /// <param name="Fy">Voltas no eixo Y por ciclo.</param>
    /// <param name="Phase">Defasagem, para as manchas não andarem juntas.</param>
    /// <param name="Spin">Sentido e velocidade do giro (voltas por ciclo).</param>
    private sealed record Blob(Grid View, string BrushKey, int Fx, int Fy, double Phase, int Spin);

    public AmbientBackground()
    {
        InputTransparent = true;
        CascadeInputTransparent = true;
        this.SetThemeColor(BackgroundColorProperty, "Ground");

        // Posição e tamanho proporcionais à página: x, y, largura, altura.
        _blobs =
        [
            new(Spot(new Rect(-0.2, -0.25, 0.9, 0.8)), "AmbientBlue", 1, 2, 0.0, 1),
            new(Spot(new Rect(1.2, 0.2, 0.75, 0.7)), "AmbientTeal", 2, 1, 2.1, -1),
            new(Spot(new Rect(0.3, 1.25, 0.95, 0.75)), "AmbientRose", 1, 3, 4.2, 1),
            // Quarta mancha, menor, que atravessa o meio e mistura as outras.
            new(Spot(new Rect(0.55, 0.45, 0.6, 0.55)), "AmbientTeal", 3, 2, 1.0, -1),
        ];

        // As estrelas ficam por cima da nebulosa. O quadro é maior que a página (diagonal),
        // para o giro não deixar cantos vazios.
        _stars = new GraphicsView { Drawable = _field, InputTransparent = true, BackgroundColor = Colors.Transparent };
        SetLayoutFlags((IView)_stars, AbsoluteLayoutFlags.All);
        SetLayoutBounds((IView)_stars, new Rect(0.5, 0.5, 1.6, 1.6));
        Add(_stars);

        Loaded += (_, _) =>
        {
            ThemeColors.PaletteChanged += OnPaletteChanged;
            Paint();
            StartFlow();
        };
        Unloaded += (_, _) =>
        {
            ThemeColors.PaletteChanged -= OnPaletteChanged;
            this.AbortAnimation(FlowAnimation);
        };
    }

    private Grid Spot(Rect bounds)
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
    // senão a segunda tela (ou a segunda mancha) que usar o mesmo pincel fica sem nada.
    private void Paint()
    {
        var palette = VaultThemeManager.Current;
        _intensity = palette == VaultPalette.Glass ? 1.0 : 0.65;
        _field.Brightness = palette switch
        {
            VaultPalette.Glass => 1.0,
            VaultPalette.Dark => 0.7,
            _ => 0.0,
        };
        _stars.IsVisible = _field.Brightness > 0;
        _stars.Invalidate();

        foreach (var blob in _blobs)
        {
            blob.View.Background = Application.Current?.Resources.TryGetValue(blob.BrushKey, out var value) == true && value is GradientBrush source
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

    private void StartFlow()
    {
        this.AbortAnimation(FlowAnimation);
        var frame = 0;
        var flow = new Animation(t =>
        {
            var angle = t * Math.PI * 2;
            var k = _intensity;
            // Translation, Scale e Rotation mexem só no desenho: nenhum quadro dispara layout.
            foreach (var b in _blobs)
            {
                var p = b.Phase;
                b.View.TranslationX = Math.Sin(angle * b.Fx + p) * Width * 0.16 * k;
                b.View.TranslationY = Math.Cos(angle * b.Fy + p * 1.3) * Height * 0.14 * k;
                // Escalas fora de fase em X e Y: a mancha "respira" e se alonga como uma gota.
                b.View.ScaleX = 1 + Math.Sin(angle * 2 + p) * 0.22 * k;
                b.View.ScaleY = 1 + Math.Cos(angle * 3 + p * 0.7) * 0.18 * k;
                b.View.Rotation = (t * 360 * b.Spin + p * 57.3) % 360;
            }

            if (_stars.IsVisible)
            {
                // A galáxia dá 1/8 de volta por ciclo: perceptível só para quem repara.
                _stars.Rotation = t * 45;
                _field.Time = t;
                // O brilho das estrelas muda devagar: redesenhar a cada 2 quadros basta.
                if (++frame % 2 == 0) _stars.Invalidate();
            }
        });
        flow.Commit(this, FlowAnimation, rate: 16, length: CycleMs, easing: Easing.Linear, repeat: () => true);
    }

    /// <summary>Estrelas em posições fixas (semente constante), cada uma piscando no seu ritmo.</summary>
    private sealed class StarField : IDrawable
    {
        private readonly (float X, float Y, float Size, float Phase, int Speed, bool Warm)[] _stars;

        public double Time { get; set; }
        public double Brightness { get; set; } = 1;

        public StarField()
        {
            var random = new Random(7);
            _stars = new (float, float, float, float, int, bool)[140];
            for (var i = 0; i < _stars.Length; i++)
            {
                // Poucas estrelas grandes, muitas pequenas.
                var size = (float)(0.5 + Math.Pow(random.NextDouble(), 3) * 1.3);
                _stars[i] = ((float)random.NextDouble(), (float)random.NextDouble(), size,
                    (float)(random.NextDouble() * Math.PI * 2), random.Next(2, 7), random.NextDouble() < 0.3);
            }
        }

        public void Draw(ICanvas canvas, RectF rect)
        {
            if (Brightness <= 0) return;
            var angle = Time * Math.PI * 2;
            // Tons da própria paleta: a maioria quase branca, algumas puxando para o âmbar.
            var white = Color.FromArgb("#FFF6EC");
            var warm = Color.FromArgb("#FFBA08");
            foreach (var (x, y, size, phase, speed, isWarm) in _stars)
            {
                var twinkle = 0.35 + 0.65 * (0.5 + 0.5 * Math.Sin(angle * speed + phase));
                // Só o ponto, sem halo: um halo translúcido sobre o fundo escuro vira um disco cinza.
                var alpha = (float)Math.Min(1, twinkle * Brightness * (0.55 + size / 3));
                canvas.FillColor = (isWarm ? warm : white).WithAlpha(alpha);
                canvas.FillCircle(rect.X + x * rect.Width, rect.Y + y * rect.Height, size);
            }
        }
    }
}
// /snippet:control.ambient
