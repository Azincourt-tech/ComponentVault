using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Buttons;

// snippet:control.holdbutton
/// <summary>
/// Botão de "segurar para confirmar": para ações destrutivas, em vez de um diálogo "Tem certeza?".
/// Enquanto o dedo (ou o mouse) fica pressionado, um preenchimento cruza o botão; soltar antes
/// desfaz o preenchimento, e chegar ao fim confirma. O texto diz o que fazer e o que aconteceu.
/// </summary>
public class HoldButton : ContentView, IDrawable
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(HoldButton), "Segure para excluir",
        propertyChanged: (b, _, _) => ((HoldButton)b).UpdateLabel());

    public static readonly BindableProperty DoneTextProperty = BindableProperty.Create(
        nameof(DoneText), typeof(string), typeof(HoldButton), "Excluído");

    public static readonly BindableProperty HoldDurationProperty = BindableProperty.Create(
        nameof(HoldDuration), typeof(uint), typeof(HoldButton), 1200u);

    private const string FillAnimation = "HoldFill";
    private readonly GraphicsView _canvas;
    private readonly Label _label;
    private double _progress;
    private bool _done;

    public event EventHandler? Confirmed;

    public HoldButton()
    {
        _canvas = new GraphicsView { Drawable = this, BackgroundColor = Colors.Transparent };
        _canvas.StartInteraction += (_, _) => Press();
        _canvas.EndInteraction += (_, _) => Release();
        _canvas.CancelInteraction += (_, _) => Release();

        _label = new Label
        {
            FontFamily = "JakartaSemiBold",
            FontSize = 15,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true,
        };

        HeightRequest = 52;
        Content = new Grid { Children = { _canvas, _label } };
        SemanticProperties.SetHint(this, "Toque e segure para confirmar");
        UpdateLabel();

        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) =>
        {
            ThemeColors.PaletteChanged -= OnPaletteChanged;
            this.AbortAnimation(FillAnimation);
        };
    }

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    public string DoneText { get => (string)GetValue(DoneTextProperty); set => SetValue(DoneTextProperty, value); }

    /// <summary>Quanto tempo segurar até confirmar, em milissegundos.</summary>
    public uint HoldDuration { get => (uint)GetValue(HoldDurationProperty); set => SetValue(HoldDurationProperty, value); }

    /// <summary>Volta ao estado inicial (para usar de novo).</summary>
    public void Reset()
    {
        _done = false;
        _progress = 0;
        UpdateLabel();
        _canvas.Invalidate();
    }

    // Segurando: o preenchimento corre do ponto atual até o fim, no tempo que falta.
    private void Press()
    {
        if (_done) return;
        this.AbortAnimation(FillAnimation);
        var remaining = (uint)Math.Max(1, HoldDuration * (1 - _progress));
        this.Animate(FillAnimation, v => { _progress = v; _canvas.Invalidate(); }, _progress, 1, length: remaining, easing: Easing.Linear,
            finished: (_, cancelled) => { if (!cancelled && _progress >= 0.999) Complete(); });
    }

    // Soltou antes do fim: o preenchimento volta rápido, sem confirmar nada.
    private void Release()
    {
        if (_done) return;
        this.AbortAnimation(FillAnimation);
        this.Animate(FillAnimation, v => { _progress = v; _canvas.Invalidate(); }, _progress, 0, length: 260, easing: Easing.CubicOut);
    }

    private void Complete()
    {
        _done = true;
        _progress = 1;
        UpdateLabel();
        _canvas.Invalidate();
        SemanticScreenReader.Announce(DoneText);
        Confirmed?.Invoke(this, EventArgs.Empty);
        Pulse();
    }

    // Um "pulo" curto confirma o gesto também para quem não está lendo o texto.
    private async void Pulse()
    {
        await this.ScaleToAsync(1.03, 90, Easing.CubicOut);
        await this.ScaleToAsync(1, 120, Easing.CubicIn);
    }

    private void UpdateLabel()
    {
        _label.Text = _done ? $"✓  {DoneText}" : Text;
        if (_done) _label.TextColor = Colors.White; // sobre o preenchimento sólido de perigo
        else _label.SetThemeColor(Label.TextColorProperty, "DangerText");
        SemanticProperties.SetDescription(this, _label.Text);
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        var radius = rect.Height / 2;
        var danger = ThemeColors.Resolve("Danger", Colors.Red);

        // Trilha: tom de perigo bem leve, com contorno fino.
        canvas.FillColor = ThemeColors.Resolve("DangerTint", Colors.MistyRose);
        canvas.FillRoundedRectangle(rect, radius);
        canvas.StrokeColor = danger.WithAlpha(0.35f);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect.Inflate(-0.5f, -0.5f), radius);

        if (_progress <= 0.001) return;

        // Preenchimento: da esquerda para a direita, recortado pela forma do botão.
        canvas.SaveState();
        var path = new PathF();
        path.AppendRoundedRectangle(rect, radius);
        canvas.ClipPath(path);
        canvas.FillColor = _done ? danger : danger.WithAlpha(0.28f);
        canvas.FillRectangle(rect.X, rect.Y, (float)(rect.Width * _progress), rect.Height);
        canvas.RestoreState();
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateLabel();
        _canvas.Invalidate();
    }
}
// /snippet:control.holdbutton
