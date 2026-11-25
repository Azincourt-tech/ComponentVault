using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Highlighting;

namespace ComponentVault.Tests.Services;

public class ColorCodeHighlighterTests
{
    private readonly ColorCodeHighlighter _highlighter = new();

    [Theory]
    [InlineData("<Button Text=\"Salvar\" Style=\"{StaticResource PrimaryButton}\" />", CodeLanguage.Xaml)]
    [InlineData("<!-- comentário -->\n<Label Text=\"a\" />", CodeLanguage.Xaml)]
    [InlineData("public sealed class A { private string _b = \"c\"; } // fim", CodeLanguage.CSharp)]
    public void Tokens_reconstroem_o_codigo_original(string code, CodeLanguage language)
    {
        var tokens = _highlighter.Tokenize(code, language);

        Assert.Equal(code, string.Concat(tokens.Select(t => t.Text)));
    }

    [Fact]
    public void Xaml_destaca_elemento_atributo_e_valor()
    {
        var tokens = _highlighter.Tokenize("<Button Text=\"Salvar\" />", CodeLanguage.Xaml);

        Assert.Contains(tokens, t => t.Kind == SyntaxKind.Element && t.Text.Contains("Button"));
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.Attribute && t.Text.Contains("Text"));
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.String && t.Text.Contains("Salvar"));
    }

    [Fact]
    public void CSharp_destaca_palavra_chave_string_e_comentario()
    {
        var tokens = _highlighter.Tokenize("public string A = \"b\"; // c", CodeLanguage.CSharp);

        Assert.Contains(tokens, t => t.Kind == SyntaxKind.Keyword && t.Text.Contains("public"));
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.String && t.Text.Contains("\"b\""));
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.Comment && t.Text.Contains("// c"));
    }

    [Fact]
    public void Codigo_vazio_nao_gera_tokens()
    {
        Assert.Empty(_highlighter.Tokenize(string.Empty, CodeLanguage.CSharp));
    }

    [Theory]
    [InlineData("XML Attribute Value", SyntaxKind.String)]
    [InlineData("XML Attribute", SyntaxKind.Attribute)]
    [InlineData("XML Name", SyntaxKind.Element)]
    [InlineData("XML Delimiter", SyntaxKind.Punctuation)]
    [InlineData("XML Comment", SyntaxKind.Comment)]
    [InlineData("Keyword", SyntaxKind.Keyword)]
    [InlineData(null, SyntaxKind.Plain)]
    public void Classifica_escopos_do_colorcode(string? scope, SyntaxKind expected)
    {
        Assert.Equal(expected, ColorCodeHighlighter.Classify(scope));
    }
}
