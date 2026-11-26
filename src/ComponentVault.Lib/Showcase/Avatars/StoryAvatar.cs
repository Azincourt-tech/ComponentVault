using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Avatars;

public enum AvatarRing { None, New, Seen }

public enum AvatarStatus { None, Online, Busy, Away }

// snippet:control.storyavatar
/// <summary>
/// Avatar com anel de stories e ponto de status, desenhados com Microsoft.Maui.Graphics. O anel novo
/// usa o degradê da paleta (âmbar ao vinho); o visto é cinza e mais fino. O ponto de status fica na
/// borda, com um contorno na cor da superfície que o "recorta" do avatar. As iniciais são um Label,
/// para usar a fonte do app.
/// </summary>
public class StoryAvatar : ContentView, IDrawable
{
    public static readonly BindableProperty InitialsProperty = BindableProperty.Create(
        nameof(Initials), typeof(string), typeof(StoryAvatar), "", propertyChanged: (b, _, _) => ((StoryAvatar)b).Update());

    public static readonly BindableProperty RingProperty = BindableProperty.Create(
        nameof(Ring), typeof(AvatarRing), typeof(StoryAvatar), AvatarRing.None, propertyChanged: (b, _, _) => ((StoryAvatar)b).Update());

    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(AvatarStatus), typeof(StoryAvatar), AvatarStatus.None, propertyChanged: (b, _, _) => ((StoryAvatar)b).Update());

    public static readonly BindableProperty FaceColorProperty = BindableProperty.Create(
        nameof(FaceColor), typeof(Color), typeof(StoryAvatar), Color.FromArgb("#E85D04"), propertyChanged: (b, _, _) => ((StoryAvatar)b).Update());

    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(StoryAvatar), 64d, propertyChanged: (b, _, _) => ((StoryAvatar)b).Update());

    private readonly GraphicsView _canvas;
    private readonly Border _face;
    private readonly Label _initials;

    public StoryAvatar()
    {
        _initials = new Label { FontFamily = "JakartaSemiBold", TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _face = new Border { StrokeThickness = 0, Content = _initials, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _canvas = new GraphicsView { Drawable = this, BackgroundColor = Colors.Transparent, InputTransparent = true };
        // O anel fica atrás do rosto; o ponto de status é desenhado por cima, numa segunda camada.
        var status = new GraphicsView { Drawable = new StatusLayer(this), BackgroundColor = Colors.Transparent, InputTransparent = true };
        Content = new Grid { Children = { _canvas, _face, status } };
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
        Update();
    }

    public string Initials { get => (string)GetValue(InitialsProperty); set => SetValue(InitialsProperty, value); }

    public AvatarRing Ring { get => (AvatarRing)GetValue(RingProperty); set => SetValue(RingProperty, value); }

    public AvatarStatus Status { get => (AvatarStatus)GetValue(StatusProperty); set => SetValue(StatusProperty, value); }

    /// <summary>Cor de fundo das iniciais.</summary>
    public Color FaceColor { get => (Color)GetValue(FaceColorProperty); set => SetValue(FaceColorProperty, value); }

    public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    private void Update()
    {
        if (_face is null) return;
        WidthRequest = HeightRequest = Size;
        // Com anel, o rosto encolhe para caber o anel e o respiro entre os dois.
        var face = Ring == AvatarRing.None ? Size : Size - 10;
        _face.WidthRequest = _face.HeightRequest = face;
        _face.StrokeShape = new Ellipse();
        _face.BackgroundColor = FaceColor;
        _initials.Text = Initials;
        _initials.FontSize = Math.Round(face * 0.36);
        SemanticProperties.SetDescription(this, Initials + Ring switch
        {
            AvatarRing.New => ", story nova",
            AvatarRing.Seen => ", story vista",
            _ => "",
        } + Status switch
        {
            AvatarStatus.Online => ", online",
            AvatarStatus.Busy => ", ocupado",
            AvatarStatus.Away => ", ausente",
            _ => "",
        });
        _canvas.Invalidate();
        if (Content is Grid { Children: [_, _, GraphicsView statusView] }) statusView.Invalidate();
    }

    // Anel: um "donut" (dois círculos, regra par-ímpar) pintado com o degradê ou com o cinza.
    public void Draw(ICanvas canvas, RectF rect)
    {
        if (Ring == AvatarRing.None) return;
        var thickness = Ring == AvatarRing.New ? 3f : 1.5f;
        var outer = rect.Width / 2;
        var path = new PathF();
        path.AppendCircle(rect.Center, outer);
        path.AppendCircle(rect.Center, outer - thickness);

        if (Ring == AvatarRing.New)
        {
            var paint = new LinearGradientPaint
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                [
                    new PaintGradientStop(0, Color.FromArgb("#FFBA08")),
                    new PaintGradientStop(0.5f, Color.FromArgb("#E85D04")),
                    new PaintGradientStop(1, Color.FromArgb("#9D0208")),
                ],
            };
            canvas.SetFillPaint(paint, rect);
        }
        else
        {
            canvas.FillColor = ThemeColors.Resolve("HairlineStrong", Colors.Gray);
        }
        canvas.FillPath(path, WindingMode.EvenOdd);
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => Update();

    // Ponto de status no canto inferior direito, "recortado" por um contorno na cor da superfície.
    private sealed class StatusLayer(StoryAvatar owner) : IDrawable
    {
        public void Draw(ICanvas canvas, RectF rect)
        {
            var color = owner.Status switch
            {
                AvatarStatus.Online => ThemeColors.Resolve("Success", Colors.Green),
                AvatarStatus.Busy => ThemeColors.Resolve("Danger", Colors.Red),
                AvatarStatus.Away => ThemeColors.Resolve("Warning", Colors.Orange),
                _ => null,
            };
            if (color is null) return;

            var radius = Math.Max(5f, rect.Width * 0.11f);
            // Sobre a borda do círculo, a 45° (canto inferior direito).
            var offset = (rect.Width / 2 - (owner.Ring == AvatarRing.None ? 0 : 5)) * 0.7071f;
            var center = new PointF(rect.Center.X + offset, rect.Center.Y + offset);
            canvas.FillColor = ThemeColors.Resolve("Surface", Colors.White);
            canvas.FillCircle(center, radius + 2.5f);
            canvas.FillColor = color;
            canvas.FillCircle(center, radius);

            // Ausente: lua (um círculo da cor da superfície mordendo o ponto); ocupado: traço.
            if (owner.Status == AvatarStatus.Away)
            {
                canvas.FillColor = ThemeColors.Resolve("Surface", Colors.White);
                canvas.FillCircle(center.X - radius * 0.45f, center.Y - radius * 0.45f, radius * 0.62f);
            }
            else if (owner.Status == AvatarStatus.Busy)
            {
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = Math.Max(1.5f, radius * 0.32f);
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawLine(center.X - radius * 0.5f, center.Y, center.X + radius * 0.5f, center.Y);
            }
        }
    }
}
// /snippet:control.storyavatar
