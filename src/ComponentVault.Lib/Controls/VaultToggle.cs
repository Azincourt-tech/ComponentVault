using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Controls;

// snippet:control.toggle
/// <summary>
/// Alternador desenhado do zero com Microsoft.Maui.Graphics: mesma aparência em todas as
/// plataformas, trilha com cor de "desligado" configurável e animação própria.
/// </summary>
public class VaultToggle : GraphicsView, IDrawable
{
    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled), typeof(bool), typeof(VaultToggle), false, BindingMode.TwoWay,
        propertyChanged: (b, _, n) => ((VaultToggle)b).OnIsToggledChanged((bool)n));

    public static readonly BindableProperty OnColorProperty = BindableProperty.Create(
        nameof(OnColor), typeof(Color), typeof(VaultToggle), Color.FromArgb("#E85D04"),
        propertyChanged: (b, _, _) => ((VaultToggle)b).Invalidate());

    public static readonly BindableProperty OffColorProperty = BindableProperty.Create(
        nameof(OffColor), typeof(Color), typeof(VaultToggle), null,
        propertyChanged: (b, _, _) => ((VaultToggle)b).Invalidate());

    private const string AnimationName = "VaultToggleThumb";
    private float _progress;

    public VaultToggle()
    {
        Drawable = this;
        WidthRequest = 52;
        HeightRequest = 32;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        EndInteraction += OnEndInteraction;
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnThemeChanged; };
        SemanticProperties.SetHint(this, "Toque para alternar");
    }

    public event EventHandler<ToggledEventArgs>? Toggled;

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public Color OnColor
    {
        get => (Color)GetValue(OnColorProperty);
        set => SetValue(OnColorProperty, value);
    }

    /// <summary>Cor da trilha desligada. Nulo usa o token HairlineStrong do tema.</summary>
    public Color? OffColor
    {
        get => (Color?)GetValue(OffColorProperty);
        set => SetValue(OffColorProperty, value);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var off = OffColor ?? ThemeColors.Resolve("HairlineStrong", Colors.LightGray);
        var track = Lerp(off, OnColor, _progress);
        var radius = rect.Height / 2;

        canvas.FillColor = IsEnabled ? track : track.WithAlpha(0.4f);
        canvas.FillRoundedRectangle(rect, radius);

        const float inset = 3;
        var diameter = rect.Height - inset * 2;
        var x = rect.X + inset + _progress * (rect.Width - diameter - inset * 2);

        canvas.SetShadow(new SizeF(0, 1), 2, Colors.Black.WithAlpha(0.18f));
        canvas.FillColor = Colors.White;
        canvas.FillEllipse(x, rect.Y + inset, diameter, diameter);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled)) Invalidate();
    }

    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        var inside = e.Touches.Length == 0 || new Rect(0, 0, Width, Height).Contains(e.Touches[0]);
        if (IsEnabled && inside) IsToggled = !IsToggled;
    }

    private void OnIsToggledChanged(bool value)
    {
        Toggled?.Invoke(this, new ToggledEventArgs(value));
        SemanticProperties.SetDescription(this, value ? "Ligado" : "Desligado");

        var target = value ? 1f : 0f;
        this.AbortAnimation(AnimationName);
        if (Handler is null)
        {
            _progress = target;
            Invalidate();
            return;
        }

        this.Animate(AnimationName, v => { _progress = (float)v; Invalidate(); },
            _progress, target, length: 180, easing: Easing.CubicOut);
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    private static Color Lerp(Color from, Color to, float t) => new(
        from.Red + (to.Red - from.Red) * t,
        from.Green + (to.Green - from.Green) * t,
        from.Blue + (to.Blue - from.Blue) * t,
        from.Alpha + (to.Alpha - from.Alpha) * t);
}
// /snippet:control.toggle
