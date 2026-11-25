using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

/// <summary>
/// Entradas do catálogo, divididas por categoria em arquivos parciais (ComponentCatalog.*.cs).
/// Cada variante aponta para uma View de preview com o mesmo id (atributo [ShowcasePreview("id")]
/// na Lib) e para snippets marcados no código real (um teste garante as duas coisas).
/// </summary>
internal static partial class ComponentCatalog
{
    public static IReadOnlyList<ComponentItem> Items { get; } =
    [
        .. Actions(),
        .. Inputs(),
        .. Selection(),
        .. Feedback(),
        .. Display(),
        .. Containers(),
        .. Navigation(),
        .. Data(),
    ];

    private static CodeSnippetRef Xaml(string title, string id) => new(title, id, CodeLanguage.Xaml);

    private static CodeSnippetRef CSharp(string title, string id) => new(title, id, CodeLanguage.CSharp);

    private static ComponentVariant Base(string id, string summary, params string[] highlights) => new()
    {
        Id = id,
        Title = "Base MAUI",
        Tier = VariantTier.Base,
        Origin = ComponentOrigin.Native,
        Summary = summary,
        Highlights = highlights,
        Snippets = [Xaml("Uso", id)],
    };
}
