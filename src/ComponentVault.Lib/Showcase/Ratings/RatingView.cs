using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Ratings;

// snippet:control.rating
/// <summary>
/// Avaliação por estrelas desenhada com Microsoft.Maui.Graphics. Toque define o valor
/// (meia estrela com AllowHalf); no Windows, passar o mouse mostra a prévia antes do clique.
/// Com IsReadOnly vira só exibição, inclusive de valores fracionados (4,3).
/// </summary>
public class RatingView : GraphicsView, IDrawable
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(RatingView), 0d, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((RatingView)b).OnValueChanged());

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(int), typeof(RatingView), 5, propertyChanged: Resize);

    public static readonly BindableProperty StarSizeProperty = BindableProperty.Create(
        nameof(StarSize), typeof(double), typeof(RatingView), 32d, propertyChanged: Resize);

    public static readonly BindableProperty AllowHalfProperty = BindableProperty.Create(
        nameof(AllowHalf), typeof(bool), typeof(RatingView), false);

    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(RatingView), false);

    private const float Spacing = 6f;
    private double? _hover;

    public RatingView()
    {
        Drawable = this;
        HorizontalOptions = LayoutOptions.Start;
        UpdateSize();
        EndInteraction += (_, e) => { if (CanEdit && e.Touches.Length > 0) Value = ValueAt(e.Touches[0].X); };
        MoveHoverInteraction += (_, e) => { if (CanEdit && e.Touches.Length > 0) { _hover = ValueAt(e.Touches[0].X); Invalidate(); } };
        EndHoverInteraction += (_, _) => { _hover = null; Invalidate(); };
        Loaded += (_, _) => { ThemeColors.PaletteChanged += OnThemeChanged; };
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnThemeChanged; };
    }

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

    public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public double StarSize { get => (double)GetValue(StarSizeProperty); set => SetValue(StarSizeProperty, value); }

    public bool AllowHalf { get => (bool)GetValue(AllowHalfProperty); set => SetValue(AllowHalfProperty, value); }

    public bool IsReadOnly { get => (bool)GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

    private bool CanEdit => IsEnabled && !IsReadOnly;

    public void Draw(ICanvas canvas, RectF rect)
    {
        var filled = Color.FromArgb("#E5A00D");
        var empty = ThemeColors.Resolve("HairlineStrong", Colors.LightGray);
        var shown = _hover ?? Value;
        var size = (float)StarSize;

        for (var i = 0; i < Maximum; i++)
        {
            var x = i * (size + Spacing);
            var star = Star(x, 0, size);

            canvas.FillColor = empty;
            canvas.FillPath(star);

            // Parte preenchida: recorta a estrela na fração do valor (0 a 1 desta estrela).
            var fraction = (float)Math.Clamp(shown - i, 0, 1);
            if (fraction <= 0) continue;

            canvas.SaveState();
            canvas.ClipRectangle(x, 0, size * fraction, size);
            canvas.FillColor = _hover is null ? filled : filled.WithAlpha(0.7f);
            canvas.FillPath(star);
            canvas.RestoreState();
        }
    }

    private double ValueAt(float x)
    {
        var size = StarSize + Spacing;
        var index = Math.Clamp((int)(x / size), 0, Maximum - 1);
        var inStar = x - index * size;
        var half = AllowHalf && inStar < StarSize / 2;
        return index + (half ? 0.5 : 1);
    }

    private static PathF Star(float x, float y, float size)
    {
        var path = new PathF();
        var cx = x + size / 2;
        var cy = y + size / 2 + size * 0.04f;
        var outer = size / 2;
        var inner = outer * 0.48f;
        for (var p = 0; p < 10; p++)
        {
            var radius = p % 2 == 0 ? outer : inner;
            var angle = Math.PI / 5 * p - Math.PI / 2;
            var px = cx + (float)(radius * Math.Cos(angle));
            var py = cy + (float)(radius * Math.Sin(angle));
            if (p == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }

        path.Close();
        return path;
    }

    private void OnValueChanged()
    {
        SemanticProperties.SetDescription(this, $"Avaliação: {Value:0.#} de {Maximum}");
        Invalidate();
    }

    private void UpdateSize()
    {
        WidthRequest = Maximum * StarSize + (Maximum - 1) * Spacing;
        HeightRequest = StarSize;
        Invalidate();
    }

    private static void Resize(BindableObject bindable, object oldValue, object newValue) => ((RatingView)bindable).UpdateSize();

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
// /snippet:control.rating
