using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Lists;

/// <summary>Lado para onde a linha foi arrastada até o fim.</summary>
public enum SwipeAction
{
    /// <summary>Arrastou para a direita.</summary>
    Archive,

    /// <summary>Arrastou para a esquerda.</summary>
    Delete,
}

// snippet:control.swiperow
/// <summary>
/// Linha que se arrasta para os lados: para a direita arquiva, para a esquerda exclui. O fundo é
/// desenhado com Microsoft.Maui.Graphics: neutro enquanto o arrasto é curto e, passado um terço
/// da largura, um círculo de cor se expande a partir do ícone (220ms), avisando que soltar
/// confirma. Soltar antes volta a linha com mola; depois, ela sai pela borda e a altura se fecha.
/// </summary>
[ContentProperty(nameof(RowContent))]
public class SwipeRow : ContentView, IDrawable
{
    public static readonly BindableProperty RowContentProperty = BindableProperty.Create(
        nameof(RowContent), typeof(View), typeof(SwipeRow), propertyChanged: (b, _, n) => ((SwipeRow)b)._front.Content = (View?)n);

    private const double Threshold = 0.34;
    private const float Radius = 16;
    private const string FillAnimation = "SwipeFill";

    private readonly GraphicsView _reveal;
    private readonly Border _front;
    private readonly MauiIcon _archiveIcon;
    private readonly MauiIcon _deleteIcon;
    private double _offset;
    private double _startOffset;
    private bool? _horizontal;   // decidido nos primeiros pixels: rolar a lista não arrasta a linha
    private bool _armed;
    private float _fill;         // 0 a 1: quanto o círculo de cor já cobriu

    public event EventHandler<SwipeAction>? Swiped;

    public SwipeRow()
    {
        _reveal = new GraphicsView { Drawable = this, BackgroundColor = Colors.Transparent, InputTransparent = true };
        _archiveIcon = Icon(FluentIcons.Archive24, LayoutOptions.Start);
        _deleteIcon = Icon(FluentIcons.Delete24, LayoutOptions.End);

        _front = new Border { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = Radius } };
        _front.SetThemeColor(BackgroundColorProperty, "SurfaceRaised");
        _front.SetThemeBrush(Border.StrokeProperty, "Hairline");

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        _front.GestureRecognizers.Add(pan);

        // No Windows, o botão direito oferece as mesmas ações (mouse sem arrasto e teclado).
        var archive = new MenuFlyoutItem { Text = "Arquivar" };
        archive.Clicked += (_, _) => Perform(SwipeAction.Archive);
        var delete = new MenuFlyoutItem { Text = "Excluir" };
        delete.Clicked += (_, _) => Perform(SwipeAction.Delete);
        FlyoutBase.SetContextFlyout(_front, new MenuFlyout { archive, delete });

        Content = new Grid { Children = { _reveal, _archiveIcon, _deleteIcon, _front } };
        SemanticProperties.SetHint(this, "Deslize para a direita para arquivar ou para a esquerda para excluir");
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => { ThemeColors.PaletteChanged -= OnPaletteChanged; this.AbortAnimation(FillAnimation); };
    }

    public View? RowContent { get => (View?)GetValue(RowContentProperty); set => SetValue(RowContentProperty, value); }

    private static MauiIcon Icon(FluentIcons glyph, LayoutOptions side)
    {
        var icon = new MauiIcon
        {
            Icon = glyph,
            IconSize = 22,
            HorizontalOptions = side,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(24, 0),
            Opacity = 0,
            InputTransparent = true,
        };
        icon.SetThemeColor(MauiIcon.IconColorProperty, "InkSecondary");
        return icon;
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                this.AbortAnimation("SwipeReset");
                _startOffset = _offset;
                _horizontal = null;
                break;
            case GestureStatus.Running:
                _horizontal ??= Math.Abs(e.TotalX) > 8 ? Math.Abs(e.TotalX) > Math.Abs(e.TotalY)
                    : Math.Abs(e.TotalY) > 8 ? false : null;
                if (_horizontal == true) SetOffset(_startOffset + e.TotalX);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_armed) Commit();
                else Reset();
                break;
        }
    }

    private void SetOffset(double x)
    {
        _offset = Math.Clamp(x, -Width * 0.9, Width * 0.9);
        _front.TranslationX = _offset;

        var progress = Math.Abs(_offset) / Math.Max(1, Width * Threshold);
        var active = _offset > 0 ? _archiveIcon : _deleteIcon;
        var other = _offset > 0 ? _deleteIcon : _archiveIcon;
        other.Opacity = 0;
        active.Opacity = Math.Min(1, progress * 1.4);
        if (!_armed) active.Scale = 0.7 + (0.3 * Math.Min(1, progress));

        var armed = progress >= 1;
        if (armed != _armed)
        {
            _armed = armed;
            // Passou do ponto: o ícone fica branco e dá um pulo; o círculo cobre o fundo.
            _archiveIcon.SetThemeColor(MauiIcon.IconColorProperty, armed ? "OnAccent" : "InkSecondary");
            _deleteIcon.SetThemeColor(MauiIcon.IconColorProperty, armed ? "OnAccent" : "InkSecondary");
            if (armed) _ = Pop(active);
            var from = _fill;
            this.AbortAnimation(FillAnimation);
            this.Animate(FillAnimation, v => { _fill = (float)v; _reveal.Invalidate(); }, from, armed ? 1 : 0, length: 220, easing: Easing.CubicOut);
        }
        _reveal.Invalidate();
    }

    private static async Task Pop(View icon)
    {
        await icon.ScaleToAsync(1.25, 90, Easing.CubicOut);
        await icon.ScaleToAsync(1, 140, Easing.CubicIn);
    }

    /// <summary>Faz a ação como se o arrasto tivesse ido até o fim (menu, atalho, leitor de tela).</summary>
    public void Perform(SwipeAction action)
    {
        SetOffset((action == SwipeAction.Archive ? 1 : -1) * ((Width * Threshold) + 1));
        Commit();
    }

    /// <summary>Volta a linha para o lugar.</summary>
    public void Reset()
    {
        // O fundo acompanha a volta (em vez de sumir na hora e deixar um buraco atrás da linha).
        this.AbortAnimation(FillAnimation);
        this.Animate("SwipeReset", v => SetOffset(v), _offset, 0, length: 260, easing: Easing.CubicOut,
            finished: (_, _) => { _offset = 0; _front.TranslationX = 0; _reveal.Invalidate(); });
    }

    // Sai pela borda, a altura fecha e só então avisa: a lista remove o item sem salto.
    private async void Commit()
    {
        var action = _offset > 0 ? SwipeAction.Archive : SwipeAction.Delete;
        await _front.TranslateToAsync(Math.Sign(_offset) * Width, 0, 160, Easing.CubicIn);
        var height = Height;
        var collapse = new Animation(v => HeightRequest = v, height, 0);
        collapse.Add(0, 1, new Animation(v => Opacity = v, 1, 0));
        var done = new TaskCompletionSource();
        collapse.Commit(this, "SwipeCollapse", length: 220, easing: Easing.CubicInOut, finished: (_, _) => done.TrySetResult());
        await done.Task;
        SemanticScreenReader.Announce(action == SwipeAction.Archive ? "Arquivado" : "Excluído");
        Swiped?.Invoke(this, action);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (Math.Abs(_offset) < 0.5) return;

        var shape = new PathF();
        shape.AppendRoundedRectangle(rect, Radius);
        canvas.ClipPath(shape);
        canvas.FillColor = ThemeColors.Resolve("SurfaceAlt", Colors.LightGray);
        canvas.FillRectangle(rect);

        if (_fill <= 0.001f) return;
        var archive = _offset > 0;
        var color = ThemeColors.Resolve(archive ? "Success" : "Danger", archive ? Colors.Teal : Colors.Red);
        var cx = archive ? 24 + 11f : rect.Width - 24 - 11f;
        var far = MathF.Sqrt((rect.Width * rect.Width) + (rect.Height * rect.Height));
        canvas.FillColor = color;
        canvas.FillCircle(cx, rect.Center.Y, Math.Max(18, far * _fill));
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => _reveal.Invalidate();
}
// /snippet:control.swiperow
