using ComponentVault.Lib.Theming;

namespace ComponentVault.Lib.Showcase.Editors;

[ShowcasePreview("editor.base")]
public partial class EditorBasePreview : ContentView
{
    public EditorBasePreview() => InitializeComponent();
}

[ShowcasePreview("editor.styled")]
public partial class EditorStyledPreview : ContentView
{
    public EditorStyledPreview()
    {
        InitializeComponent();
        UpdateCounter(0);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e) => UpdateCounter(e.NewTextValue?.Length ?? 0);

    private void UpdateCounter(int length)
    {
        var max = DescriptionEditor.MaxLength;
        CounterLabel.Text = $"{length}/{max}";

        var (text, fill) = length >= max ? ("DangerText", "DangerTint")
            : length >= max * 0.9 ? ("WarningText", "WarningTint")
            : ("InkTertiary", "SurfaceAlt");
        CounterLabel.SetThemeColor(Label.TextColorProperty, text);
        CounterPill.SetThemeColor(BackgroundColorProperty, fill);
        SemanticProperties.SetDescription(CounterLabel, $"{max - length} caracteres restantes");
    }

    // Foco igual ao do VaultTextField: o campo clareia e ganha o anel de 2px.
    private void OnFocusChanged(object? sender, FocusEventArgs e) => FieldFrames.Apply(EditorFrame, e.IsFocused);

    private void OnBoldTapped(object? sender, TappedEventArgs e) => Wrap("**", "**");

    private void OnItalicTapped(object? sender, TappedEventArgs e) => Wrap("_", "_");

    private void OnListTapped(object? sender, TappedEventArgs e) => Wrap(LineStart() ? "- " : "\n- ", string.Empty);

    private void OnLinkTapped(object? sender, TappedEventArgs e) => Wrap("[", "](https://)");

    private bool LineStart()
    {
        var text = DescriptionEditor.Text ?? string.Empty;
        var at = Math.Clamp(DescriptionEditor.CursorPosition, 0, text.Length);
        return at == 0 || text[at - 1] == '\n';
    }

    // Envolve a seleção (ou insere no cursor) e devolve o foco com o cursor dentro das marcas.
    private void Wrap(string prefix, string suffix)
    {
        var text = DescriptionEditor.Text ?? string.Empty;
        var start = Math.Clamp(DescriptionEditor.CursorPosition, 0, text.Length);
        var length = Math.Clamp(DescriptionEditor.SelectionLength, 0, text.Length - start);
        var result = text[..start] + prefix + text.Substring(start, length) + suffix + text[(start + length)..];
        if (result.Length > DescriptionEditor.MaxLength) return;

        DescriptionEditor.Text = result;
        DescriptionEditor.Focus();
        DescriptionEditor.CursorPosition = start + prefix.Length + length;
        DescriptionEditor.SelectionLength = 0;
    }
}

[ShowcasePreview("editor.custom")]
public partial class EditorCustomPreview : ContentView
{
    private const string Sample = """
        # Notas da versão 2.4
        O **modo Apresentar** agora abre em tela cheia no Windows. Use as setas para trocar o nível e `Esc` para sair.

        ## O que mudou
        - Fundo de _galáxia_ animado nos três temas
        - Campos preenchidos com anel de foco
        - Dez controles Custom novos

        > Feito com tokens: muda de tema sem recarregar.

        ```
        dotnet build -c Release
        ```
        Detalhes no [repositório](https://github.com).
        """;

    public EditorCustomPreview()
    {
        InitializeComponent();
        Source.Text = Sample;
    }

    private void OnSourceChanged(object? sender, TextChangedEventArgs e)
    {
        var text = e.NewTextValue ?? string.Empty;
        Rendered.Markdown = text;
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Count(w => w.Any(char.IsLetterOrDigit));
        StatsLabel.Text = $"{words} palavras · {Math.Max(1, (int)Math.Ceiling(words / 200d))} min";
    }

    // Troca com fade: a área que sai some em 120ms e a que entra sobe 6px enquanto aparece.
    private async void OnTabChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Chips.ChoiceChipGroup.SelectedIndex) || WritePane is null) return;
        var (show, hide) = Tabs.SelectedIndex == 0 ? ((View)WritePane, (View)PreviewPane) : (PreviewPane, WritePane);
        if (show.IsVisible && show.Opacity >= 1) return;

        await hide.FadeToAsync(0, 120);
        hide.IsVisible = false;
        show.Opacity = 0;
        show.TranslationY = 6;
        show.IsVisible = true;
        await Task.WhenAll(show.FadeToAsync(1, 160), show.TranslateToAsync(0, 0, 160, Easing.CubicOut));
        if (show == WritePane) Source.Focus();
    }
}
