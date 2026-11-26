using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.BottomSheets;

/// <summary>
/// Mapa de mentira para os exemplos de bottom sheet: quarteirões, um parque, ruas e a rota em
/// destaque, desenhados com Microsoft.Maui.Graphics e as cores do tema. Só cenário; não é snippet.
/// </summary>
public class MapBackdrop : GraphicsView, IDrawable
{
    public MapBackdrop()
    {
        Drawable = this;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        float w = rect.Width, h = rect.Height;
        canvas.FillColor = ThemeColors.Resolve("SurfaceAlt", Colors.LightGray);
        canvas.FillRectangle(rect);

        // Parque.
        canvas.FillColor = ThemeColors.Resolve("SuccessTint", Colors.Honeydew);
        canvas.FillRoundedRectangle(w * 0.58f, h * 0.08f, w * 0.34f, h * 0.22f, 14);

        // Ruas largas e estreitas.
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeColor = ThemeColors.Resolve("Surface", Colors.White);
        canvas.StrokeSize = 12;
        canvas.DrawLine(0, h * 0.36f, w, h * 0.30f);
        canvas.DrawLine(w * 0.30f, 0, w * 0.42f, h);
        canvas.StrokeSize = 6;
        canvas.DrawLine(0, h * 0.62f, w, h * 0.70f);
        canvas.DrawLine(w * 0.78f, 0, w * 0.70f, h);
        canvas.DrawLine(0, h * 0.14f, w * 0.5f, h * 0.18f);

        // Rota: do ponto de saída (embaixo à esquerda) ao destino (em cima à direita).
        var route = new PathF(w * 0.12f, h * 0.64f);
        route.LineTo(w * 0.385f, h * 0.665f);
        route.LineTo(w * 0.355f, h * 0.335f);
        route.LineTo(w * 0.74f, h * 0.31f);
        route.LineTo(w * 0.77f, h * 0.16f);
        var accent = ThemeColors.Resolve("Accent", Colors.OrangeRed);
        canvas.StrokeColor = accent;
        canvas.StrokeSize = 5;
        canvas.StrokeLineJoin = LineJoin.Round;
        canvas.DrawPath(route);

        // Saída (anel) e destino (pino cheio).
        canvas.FillColor = Colors.White;
        canvas.FillCircle(w * 0.12f, h * 0.64f, 8);
        canvas.StrokeSize = 4;
        canvas.DrawCircle(w * 0.12f, h * 0.64f, 8);
        canvas.FillColor = accent;
        canvas.FillCircle(w * 0.77f, h * 0.16f, 11);
        canvas.FillColor = Colors.White;
        canvas.FillCircle(w * 0.77f, h * 0.16f, 4);
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();
}
