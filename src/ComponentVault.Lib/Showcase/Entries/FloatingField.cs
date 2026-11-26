using ComponentVault.Lib.Controls;
using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Entries;

// snippet:control.floatingfield
/// <summary>
/// Campo com rótulo flutuante feito à mão: vazio, o rótulo ocupa o lugar do texto; ao focar ou
/// preencher, ele sobe e encolhe (160ms) e passa a ser o título do campo, sem sumir como um
/// placeholder. <see cref="ShowError"/> pinta o campo de erro e dá uma sacudida curta.
/// </summary>
public class FloatingField : ContentView
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(FloatingField), "", propertyChanged: (b, _, n) => ((FloatingField)b)._title.Text = (string)n);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(FloatingField), null, BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((FloatingField)b).OnTextChanged());

    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
        nameof(Keyboard), typeof(Keyboard), typeof(FloatingField), Keyboard.Default,
        propertyChanged: (b, _, n) => ((FloatingField)b)._input.Keyboard = (Keyboard)n);

    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        nameof(MaxLength), typeof(int), typeof(FloatingField), int.MaxValue,
        propertyChanged: (b, _, n) => ((FloatingField)b)._input.MaxLength = (int)n);

    private const string FloatAnimation = "FloatLabel";
    private readonly Border _frame;
    private readonly Label _title;
    private readonly BorderlessEntry _input;
    private readonly Label _message;
    private bool _floated;
    private bool _hasError;

    public FloatingField()
    {
        _title = new Label
        {
            FontFamily = "JakartaRegular",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center,
            AnchorX = 0,
            InputTransparent = true,
        };
        _input = new BorderlessEntry
        {
            FontFamily = "JakartaRegular",
            FontSize = 16,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 2),
            BackgroundColor = Colors.Transparent,
        };
        _input.SetThemeColor(Entry.TextColorProperty, "Ink");
        _input.SetBinding(Entry.TextProperty, new Binding(nameof(Text), BindingMode.TwoWay, source: this));
        _input.Focused += (_, _) => UpdateVisuals(animated: true);
        _input.Unfocused += (_, _) => UpdateVisuals(animated: true);

        _frame = new Border { HeightRequest = 58, Content = new Grid { Children = { _input, _title } } };
        _frame.SetDynamicResource(StyleProperty, "FieldFrame");
        // Tocar em qualquer ponto da moldura foca o campo (inclusive sobre o rótulo).
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _input.Focus();
        _frame.GestureRecognizers.Add(tap);

        _message = new Label { FontFamily = "JakartaMedium", FontSize = 13, IsVisible = false };

        Content = new VerticalStackLayout { Spacing = 6, Children = { _frame, _message } };
        UpdateVisuals(animated: false);
        Loaded += (_, _) => ThemeColors.PaletteChanged += OnPaletteChanged;
        Unloaded += (_, _) => ThemeColors.PaletteChanged -= OnPaletteChanged;
    }

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    public string? Text { get => (string?)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    public Keyboard Keyboard { get => (Keyboard)GetValue(KeyboardProperty); set => SetValue(KeyboardProperty, value); }

    public int MaxLength { get => (int)GetValue(MaxLengthProperty); set => SetValue(MaxLengthProperty, value); }

    /// <summary>Mostra a mensagem de erro embaixo, pinta o campo e sacode de leve.</summary>
    public async void ShowError(string message)
    {
        _hasError = true;
        _message.Text = message;
        _message.IsVisible = true;
        UpdateVisuals(animated: true);
        SemanticScreenReader.Announce(message);
        // Sacudida curta (6px, 4 vezes, ~240ms): chama atenção sem depender só da cor.
        foreach (var x in new[] { -6d, 6, -4, 4, 0 }) await _frame.TranslateToAsync(x, 0, 48, Easing.Linear);
    }

    public void ClearError()
    {
        if (!_hasError) return;
        _hasError = false;
        _message.IsVisible = false;
        UpdateVisuals(animated: true);
    }

    private void OnTextChanged()
    {
        ClearError();
        UpdateVisuals(animated: _input?.IsFocused == true);
    }

    private void UpdateVisuals(bool animated)
    {
        if (_frame is null) return;
        var focused = _input.IsFocused;
        var shouldFloat = focused || !string.IsNullOrEmpty(Text);

        FieldFrames.Apply(_frame, focused, _hasError ? "Danger" : null);
        _title.SetThemeColor(Microsoft.Maui.Controls.Label.TextColorProperty,
            _hasError ? "DangerText" : focused ? "AccentText" : shouldFloat ? "InkSecondary" : "InkTertiary");
        _message.SetThemeColor(Microsoft.Maui.Controls.Label.TextColorProperty, "DangerText");
        SemanticProperties.SetDescription(_input, Label);

        if (shouldFloat == _floated && animated) return;
        _floated = shouldFloat;

        // Flutuando: sobe 11px e encolhe para 78%, ancorado à esquerda.
        var y = shouldFloat ? -11d : 0;
        var scale = shouldFloat ? 0.78 : 1;
        this.AbortAnimation(FloatAnimation);
        if (!animated || Handler is null)
        {
            _title.TranslationY = y;
            _title.Scale = scale;
            return;
        }
        var fromY = _title.TranslationY;
        var fromScale = _title.Scale;
        this.Animate(FloatAnimation, t =>
        {
            _title.TranslationY = fromY + (y - fromY) * t;
            _title.Scale = fromScale + (scale - fromScale) * t;
        }, length: 160, easing: Easing.CubicOut);
    }

    private void OnPaletteChanged(object? sender, AppThemeChangedEventArgs e) => UpdateVisuals(animated: false);
}
// /snippet:control.floatingfield
