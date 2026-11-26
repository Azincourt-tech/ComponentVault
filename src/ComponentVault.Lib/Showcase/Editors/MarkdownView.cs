using System.Text.RegularExpressions;
using ComponentVault.Lib.Theming;
using Microsoft.Maui.Controls.Shapes;

namespace ComponentVault.Lib.Showcase.Editors;

// snippet:control.markdownview
/// <summary>
/// Renderizador de Markdown feito com Labels e Spans, sem WebView: títulos (#, ##), listas
/// (- e 1.), citação (&gt;), bloco de código (```), e no meio do texto **negrito**, _itálico_,
/// `código` e [links](url). Cobre o que se escreve numa descrição; tabelas e imagens ficam de fora.
/// Usa só tokens do design system, então acompanha os três temas.
/// </summary>
public partial class MarkdownView : ContentView
{
    public static readonly BindableProperty MarkdownProperty = BindableProperty.Create(
        nameof(Markdown), typeof(string), typeof(MarkdownView), string.Empty,
        propertyChanged: (b, _, _) => ((MarkdownView)b).Render());

    private readonly VerticalStackLayout _blocks = new() { Spacing = 10 };

    public MarkdownView() => Content = _blocks;

    public string Markdown { get => (string)GetValue(MarkdownProperty); set => SetValue(MarkdownProperty, value); }

    private void Render()
    {
        _blocks.Clear();
        var lines = (Markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var paragraph = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                Flush();
                var code = new List<string>();
                while (++i < lines.Length && !lines[i].Trim().StartsWith("```", StringComparison.Ordinal)) code.Add(lines[i]);
                _blocks.Add(CodeBlock(string.Join('\n', code)));
            }
            else if (trimmed.Length == 0)
            {
                Flush();
            }
            else if (HeadingRegex().Match(trimmed) is { Success: true } heading)
            {
                Flush();
                _blocks.Add(Heading(heading.Groups[2].Value, heading.Groups[1].Value.Length));
            }
            else if (ListRegex().Match(trimmed) is { Success: true } item)
            {
                Flush();
                var marker = item.Groups[1].Value is "-" or "*" or "+" ? "•" : item.Groups[1].Value;
                _blocks.Add(ListItem(marker, item.Groups[2].Value));
            }
            else if (trimmed.StartsWith('>'))
            {
                Flush();
                _blocks.Add(Quote(trimmed.TrimStart('>', ' ')));
            }
            else
            {
                paragraph.Add(trimmed);
            }
        }
        Flush();

        // Linhas seguidas formam um parágrafo, como no Markdown: a quebra simples vira espaço.
        void Flush()
        {
            if (paragraph.Count == 0) return;
            _blocks.Add(Paragraph(string.Join(' ', paragraph), "Ink", 15));
            paragraph.Clear();
        }
    }

    private static Label Paragraph(string text, string colorKey, double size)
    {
        var label = new Label { FontFamily = "JakartaRegular", FontSize = size, LineHeight = 1.4, FormattedText = Inline(text, colorKey, size) };
        label.SetThemeColor(Label.TextColorProperty, colorKey);
        return label;
    }

    private static Label Heading(string text, int level)
    {
        var label = Paragraph(text, "Ink", level == 1 ? 22 : 18);
        label.FontFamily = "JakartaBold";
        label.LineHeight = 1.15;
        label.Margin = new Thickness(0, level == 1 ? 4 : 2, 0, 0);
        SemanticProperties.SetHeadingLevel(label, level == 1 ? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.Level2);
        return label;
    }

    private static Grid ListItem(string marker, string text)
    {
        var bullet = new Label { Text = marker, FontFamily = "JakartaSemiBold", FontSize = 15, LineHeight = 1.4, MinimumWidthRequest = 14 };
        bullet.SetThemeColor(Label.TextColorProperty, "AccentText");
        var grid = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 10, Padding = new Thickness(4, 0, 0, 0) };
        grid.Add(bullet, 0);
        grid.Add(Paragraph(text, "Ink", 15), 1);
        return grid;
    }

    private static Grid Quote(string text)
    {
        var bar = new BoxView { WidthRequest = 3, CornerRadius = 1.5 };
        bar.SetThemeColor(BoxView.ColorProperty, "Accent");
        var grid = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 12 };
        grid.Add(bar, 0);
        grid.Add(Paragraph(text, "InkSecondary", 15), 1);
        return grid;
    }

    private static Border CodeBlock(string code)
    {
        var label = new Label { Text = code, FontFamily = "JetBrainsMono", FontSize = 13, LineHeight = 1.35 };
        label.SetThemeColor(Label.TextColorProperty, "SyntaxPlain");
        var border = new Border
        {
            Padding = new Thickness(14, 12),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Content = label,
        };
        border.SetThemeColor(BackgroundColorProperty, "SurfaceAlt");
        border.SetThemeBrush(Border.StrokeProperty, "Hairline");
        return border;
    }

    // Trechos do meio da linha: cada casamento vira um Span com o seu estilo.
    private static FormattedString Inline(string text, string colorKey, double size)
    {
        var result = new FormattedString();
        var last = 0;
        foreach (Match match in InlineRegex().Matches(text))
        {
            if (match.Index > last) result.Spans.Add(Plain(text[last..match.Index]));
            Span span;
            if (match.Groups["bold"].Success) span = new Span { Text = match.Groups["bold"].Value, FontFamily = "JakartaBold" };
            else if (match.Groups["italic"].Success) span = new Span { Text = match.Groups["italic"].Value, FontAttributes = FontAttributes.Italic };
            else if (match.Groups["code"].Success)
            {
                span = new Span { Text = $" {match.Groups["code"].Value} ", FontFamily = "JetBrainsMono", FontSize = size - 2 };
                span.SetThemeColor(Span.BackgroundColorProperty, "SurfaceAlt");
            }
            else
            {
                span = new Span { Text = match.Groups["link"].Value, TextDecorations = TextDecorations.Underline, FontFamily = "JakartaSemiBold" };
                span.SetThemeColor(Span.TextColorProperty, "AccentText");
                if (Uri.TryCreate(match.Groups["url"].Value, UriKind.Absolute, out var uri))
                {
                    var tap = new TapGestureRecognizer();
                    tap.Tapped += async (_, _) => await Launcher.Default.OpenAsync(uri);
                    span.GestureRecognizers.Add(tap);
                }
                result.Spans.Add(span);
                last = match.Index + match.Length;
                continue;
            }
            span.SetThemeColor(Span.TextColorProperty, colorKey);
            result.Spans.Add(span);
            last = match.Index + match.Length;
        }
        if (last < text.Length) result.Spans.Add(Plain(text[last..]));
        return result;

        Span Plain(string value)
        {
            var span = new Span { Text = value };
            span.SetThemeColor(Span.TextColorProperty, colorKey);
            return span;
        }
    }

    [GeneratedRegex(@"^(#{1,2})\s+(.+)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^([-*+]|\d+\.)\s+(.+)$")]
    private static partial Regex ListRegex();

    [GeneratedRegex(@"\*\*(?<bold>.+?)\*\*|(?<![\w*])[_*](?<italic>[^_*]+?)[_*](?![\w*])|`(?<code>[^`]+)`|\[(?<link>[^\]]+)\]\((?<url>[^)\s]+)\)")]
    private static partial Regex InlineRegex();
}
// /snippet:control.markdownview
