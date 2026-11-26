using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.RadioButtons;

// snippet:control.animatedradio
/// <summary>
/// Marcador de opção única desenhado com Microsoft.Maui.Graphics. Ao escolher, o anel ganha a cor
/// de destaque e o ponto interno cresce com uma mola curta; ao sair, ele encolhe. A lógica do grupo
/// (só um escolhido) fica com quem usa: aqui é só o desenho e o estado.
/// </summary>
public class AnimatedRadio : GraphicsView, IDrawable
{
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(AnimatedRadio), false,
        propertyChanged: (b, _, _) => ((AnimatedRadio)b).OnSelectedChanged());

    private const string AnimationName = "AnimatedRadio";
    private double _shown;

    public AnimatedRadio()
    {
        Drawable = this;
        WidthRequest = HeightRequest = 26;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true; // o toque é da linha inteira, não só do círculo
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnPaletteChanged; this.AbortAnimation(AnimationName); };
    }

    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    private void OnSelectedChanged()
    {
        var target = IsSelected ? 1d : 0d;
        this.AbortAnimation(AnimationName);
        if (Handler is null) { _shown = target; Invalidate(); return; }
        // Entrar com mola (passa um pouco do tamanho e volta); sair direto, sem chamar atenção.
        this.Animate(AnimationName, v => { _shown = v; Invalidate(); }, _shown, target,
            length: IsSelected ? 320u : 160u, easing: IsSelected ? Easing.SpringOut : Easing.CubicIn);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        const float size = 22;
        var center = rect.Center;
        var accent = ThemeColors.Resolve("Accent", Colors.OrangeRed);
        var t = (float)_shown;

        // Anel: neutro desligado, destaque ligado (a cor acompanha o ponto).
        var stroke = 1.5f + t * 0.5f;
        canvas.StrokeSize = stroke;
        canvas.StrokeColor = t > 0.5f ? accent : ThemeColors.Resolve("HairlineStrong", Colors.Gray);
        canvas.FillColor = ThemeColors.Resolve("Surface", Colors.White);
        canvas.FillCircle(center, size / 2);
        canvas.DrawCircle(center, size / 2 - stroke / 2 - 0.25f);

        if (t <= 0.001f) return;

        // Ponto interno: até 45% do raio, podendo passar um pouco no pico da mola.
        canvas.FillColor = accent;
        canvas.FillCircle(center, size / 2 * 0.45f * t);
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.animatedradio
