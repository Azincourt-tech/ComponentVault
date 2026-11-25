using ColorCode;
using ColorCode.Parsing;
using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Highlighting;

public interface ISyntaxHighlighter
{
    IReadOnlyList<SyntaxToken> Tokenize(string code, CodeLanguage language);
}

/// <summary>
/// Usa o parser do ColorCode e devolve tokens semânticos em vez de HTML,
/// para a camada MAUI montar um FormattedString com as cores do tema ativo.
/// </summary>
public sealed class ColorCodeHighlighter : ISyntaxHighlighter
{
    public IReadOnlyList<SyntaxToken> Tokenize(string code, CodeLanguage language)
    {
        if (string.IsNullOrEmpty(code)) return [];

        var collector = new TokenCollector();
        collector.Run(code, language == CodeLanguage.Xaml ? Languages.Xml : Languages.CSharp);
        return collector.Tokens;
    }

    internal static SyntaxKind Classify(string? scopeName)
    {
        if (string.IsNullOrEmpty(scopeName)) return SyntaxKind.Plain;

        // A ordem importa: "XML Attribute Value" precisa cair em String antes de Attribute.
        if (scopeName.Contains("Comment", StringComparison.Ordinal)) return SyntaxKind.Comment;
        if (scopeName.Contains("String", StringComparison.Ordinal)
            || scopeName.Contains("Attribute Value", StringComparison.Ordinal)
            || scopeName.Contains("Attribute Quotes", StringComparison.Ordinal)) return SyntaxKind.String;
        if (scopeName.Contains("Keyword", StringComparison.Ordinal)) return SyntaxKind.Keyword;
        if (scopeName.Contains("Attribute", StringComparison.Ordinal)) return SyntaxKind.Attribute;
        if (scopeName.Contains("Delimiter", StringComparison.Ordinal)
            || scopeName.Contains("Operator", StringComparison.Ordinal)) return SyntaxKind.Punctuation;
        if (scopeName.Contains("Number", StringComparison.Ordinal)) return SyntaxKind.Number;
        if (scopeName.Contains("Class Name", StringComparison.Ordinal)
            || scopeName.Contains("Type", StringComparison.Ordinal)) return SyntaxKind.Type;
        if (scopeName.Contains("Name", StringComparison.Ordinal)
            || scopeName.Contains("Tag", StringComparison.Ordinal)) return SyntaxKind.Element;
        return SyntaxKind.Plain;
    }

    private sealed class TokenCollector() : CodeColorizerBase(null, null)
    {
        public List<SyntaxToken> Tokens { get; } = [];

        public void Run(string code, ILanguage language) => languageParser.Parse(code, language, Write);

        protected override void Write(string parsedSourceCode, IList<Scope> scopes)
        {
            if (parsedSourceCode.Length == 0) return;

            // Escopos aninhados: o mais interno vence. Pinta caractere a caractere, pais antes dos filhos.
            var kinds = new SyntaxKind[parsedSourceCode.Length];
            foreach (var scope in scopes) Paint(scope, kinds);

            var start = 0;
            for (var i = 1; i <= kinds.Length; i++)
            {
                if (i == kinds.Length || kinds[i] != kinds[start])
                {
                    Append(parsedSourceCode[start..i], kinds[start]);
                    start = i;
                }
            }
        }

        private static void Paint(Scope scope, SyntaxKind[] kinds)
        {
            var kind = Classify(scope.Name);
            var end = Math.Min(scope.Index + scope.Length, kinds.Length);
            for (var i = Math.Max(scope.Index, 0); i < end; i++) kinds[i] = kind;
            foreach (var child in scope.Children) Paint(child, kinds);
        }

        private void Append(string text, SyntaxKind kind)
        {
            // Junta com o token anterior quando a classe é a mesma, para reduzir spans na UI.
            if (Tokens.Count > 0 && Tokens[^1].Kind == kind)
            {
                Tokens[^1] = Tokens[^1] with { Text = Tokens[^1].Text + text };
                return;
            }

            Tokens.Add(new SyntaxToken(text, kind));
        }
    }
}
