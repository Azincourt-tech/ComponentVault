using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.EmptyStates;

// snippet:control.searchillustration
/// <summary>
/// Ilustração de "busca sem resultado" desenhada com Microsoft.Maui.Graphics: três cartões vazios
/// flutuam em fases diferentes e uma lupa passeia sobre eles. Cores tiradas da paleta a cada
/// quadro, então acompanha a troca de tema. Movimento lento (ciclo de 5s) para não competir
/// com o texto.
/// </summary>
public class SearchIllustration : GraphicsView, IDrawable
{
    private const string LoopName = "SearchIllustrationLoop";
    private double _t;

    public SearchIllustration()
    {
        Drawable = this;
        WidthRequest = 220;
        HeightRequest = 150;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true;
        SemanticProperties.SetDescription(this, "Ilustração: lupa procurando sobre cartões vazios");

        Loaded += (_, _) => this.Animate(LoopName, v =>
        {
            _t = v;
            Invalidate();
        }, 0, 1, rate: 16, length: 5000, easing: Easing.Linear, repeat: () => true);
        Unloaded += (_, _) => this.AbortAnimation(LoopName);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var surface = ThemeColors.Resolve("SurfaceRaised", Colors.White);
        var line = ThemeColors.Resolve("HairlineStrong", Colors.LightGray);
        // Linhas de esqueleto na cor de linha forte: no Escuro, SurfaceAlt é igual à superfície do cartão.
        var muted = ThemeColors.Resolve("HairlineStrong", Colors.Gainsboro);
        var accent = ThemeColors.Resolve("Accent", Colors.RoyalBlue);
        var tint = ThemeColors.Resolve("AccentTint", Colors.AliceBlue);
        var angle = _t * Math.PI * 2;

        // Três cartões em leque, cada um flutuando 3px numa fase própria.
        var cardWidth = 120f;
        var cardHeight = 60f;
        var cx = rect.Center.X;
        for (var i = 2; i >= 0; i--)
        {
            var dx = (i - 1) * 34f;
            var dy = (i * -14f) + 50f + (float)(Math.Sin(angle + (i * 2.1)) * 3);
            var card = new RectF(cx - (cardWidth / 2) + dx, dy, cardWidth, cardHeight);

            canvas.FillColor = surface;
            canvas.FillRoundedRectangle(card, 10);
            canvas.StrokeColor = line;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(card, 10);

            // Linhas de texto vazias (esqueleto): o "conteúdo" que não foi encontrado.
            canvas.FillColor = muted;
            canvas.FillRoundedRectangle(card.X + 14, card.Y + 16, 54, 8, 4);
            canvas.FillRoundedRectangle(card.X + 14, card.Y + 32, 86, 8, 4);
        }

        // Lupa: passeia num oito suave sobre os cartões.
        var lx = cx + (float)(Math.Sin(angle) * 46);
        var ly = 62f + (float)(Math.Sin(angle * 2) * 10);
        const float radius = 22f;

        canvas.FillColor = tint;
        canvas.FillCircle(lx, ly, radius);
        canvas.StrokeColor = accent;
        canvas.StrokeSize = 4;
        canvas.DrawCircle(lx, ly, radius);

        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeSize = 6;
        var handle = radius * 0.72f;
        canvas.DrawLine(lx + handle, ly + handle, lx + handle + 16, ly + handle + 16);

        // Brilho no vidro: um arco curto no canto superior esquerdo.
        canvas.StrokeSize = 2.5f;
        canvas.StrokeColor = surface;
        canvas.DrawArc(lx - 13, ly - 13, 26, 26, 110, 160, false, false);
    }
}
// /snippet:control.searchillustration
