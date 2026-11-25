using System.Reflection;

namespace ComponentVault.Lib.Showcase;

/// <summary>
/// Marca uma View como preview de uma variante do catálogo. O id precisa ser igual ao
/// <c>ComponentVariant.Id</c> no ComponentMetadataService (um teste garante isso).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ShowcasePreviewAttribute(string variantId) : Attribute
{
    public string VariantId { get; } = variantId;
}

/// <summary>
/// Descobre as Views de preview por reflexão: criar a View com o atributo basta para registrá-la.
/// </summary>
public static class PreviewRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> Map = new(Discover);

    public static IReadOnlyCollection<string> Ids => (IReadOnlyCollection<string>)Map.Value.Keys;

    public static View? Create(string variantId) =>
        Map.Value.TryGetValue(variantId, out var type) ? (View?)Activator.CreateInstance(type) : null;

    private static IReadOnlyDictionary<string, Type> Discover() =>
        typeof(PreviewRegistry).Assembly.GetTypes()
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<ShowcasePreviewAttribute>()))
            .Where(x => x.Attribute is not null && typeof(View).IsAssignableFrom(x.Type))
            .ToDictionary(x => x.Attribute!.VariantId, x => x.Type, StringComparer.Ordinal);
}
