namespace ComponentVault.Core.Models;

/// <summary>Classe semântica de um trecho de código; a camada de UI mapeia cada uma para uma cor do tema.</summary>
public enum SyntaxKind
{
    Plain,
    Keyword,
    Type,
    String,
    Number,
    Comment,
    Element,
    Attribute,
    Punctuation,
}

public readonly record struct SyntaxToken(string Text, SyntaxKind Kind);
