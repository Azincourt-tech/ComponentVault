namespace ComponentVault.Core.Models;

/// <summary>
/// Um componente do catálogo (ex.: Button). Agrupa as variantes que mostram o mesmo
/// controle em níveis diferentes: base, estilizado, custom e via biblioteca.
/// </summary>
public sealed record ComponentItem
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required ComponentCategory Category { get; init; }

    public required string Summary { get; init; }

    /// <summary>
    /// Nome do ícone Fluent (enum FluentIcons do MauiIcons, ex.: "Cube24") usado na miniatura.
    /// Nulo ou inválido cai no placeholder padrão.
    /// </summary>
    public string? Icon { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>Data em que entrou no catálogo; alimenta "Destaques recentes" (RF09) e o badge Novo.</summary>
    public required DateOnly AddedOn { get; init; }

    public required ComponentDocs Docs { get; init; }

    public required IReadOnlyList<ComponentVariant> Variants { get; init; }

    /// <summary>Origens distintas presentes nas variantes, na ordem do enum.</summary>
    public IReadOnlyList<ComponentOrigin> Origins =>
        Variants.Select(v => v.Origin).Distinct().Order().ToArray();
}

public sealed record ComponentVariant
{
    /// <summary>Identificador único global (ex.: "button.styled"). Também é a chave do preview.</summary>
    public required string Id { get; init; }

    public required string Title { get; init; }

    public required VariantTier Tier { get; init; }

    public required ComponentOrigin Origin { get; init; }

    public required string Summary { get; init; }

    /// <summary>Pacote NuGet usado, quando a variante depende de biblioteca.</summary>
    public string? PackageId { get; init; }

    /// <summary>
    /// Decisões de design e usabilidade que este nível resolve (ex.: "Estado de carregamento evita
    /// envio duplicado"). Aparecem abaixo do preview.
    /// </summary>
    public IReadOnlyList<string> Highlights { get; init; } = [];

    /// <summary>Snippets exibidos na aba Código, na ordem.</summary>
    public IReadOnlyList<CodeSnippetRef> Snippets { get; init; } = [];
}

/// <summary>
/// Referência a um trecho marcado no código-fonte real (<c>snippet:id</c>).
/// O texto não é duplicado aqui: vem do arquivo, então o snippet nunca diverge do componente.
/// </summary>
public sealed record CodeSnippetRef(string Title, string SnippetId, CodeLanguage Language);

public sealed record ComponentDocs
{
    public required string Overview { get; init; }

    public IReadOnlyList<DocProperty> Properties { get; init; } = [];

    public IReadOnlyList<string> UseCases { get; init; } = [];

    public IReadOnlyList<string> Limitations { get; init; } = [];
}

public sealed record DocProperty(string Name, string Description);
