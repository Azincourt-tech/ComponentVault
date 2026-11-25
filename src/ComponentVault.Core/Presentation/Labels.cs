using ComponentVault.Core.Models;

namespace ComponentVault.Core.Presentation;

/// <summary>Textos exibidos para os enums do domínio. Ponto único para a futura migração para .resx (RF11).</summary>
public static class Labels
{
    public static string Origin(ComponentOrigin origin) => origin switch
    {
        ComponentOrigin.Native => "Nativo",
        ComponentOrigin.Custom => "Custom",
        ComponentOrigin.FreeLibrary => "Lib Gratuita",
        ComponentOrigin.Syncfusion => "Syncfusion",
        _ => origin.ToString(),
    };

    public static string Tier(VariantTier tier) => tier switch
    {
        VariantTier.Base => "Base MAUI",
        VariantTier.Styled => "Estilizado",
        VariantTier.Custom => "Custom",
        VariantTier.Library => "Biblioteca",
        _ => tier.ToString(),
    };

    public static string Category(ComponentCategory category) => category switch
    {
        ComponentCategory.Actions => "Ações",
        ComponentCategory.Inputs => "Entradas",
        ComponentCategory.Selection => "Seleção",
        ComponentCategory.Feedback => "Feedback",
        ComponentCategory.Display => "Exibição",
        ComponentCategory.Containers => "Contêineres",
        ComponentCategory.Navigation => "Navegação",
        ComponentCategory.Data => "Dados",
        _ => category.ToString(),
    };

    public static string Theme(AppThemePreference theme) => theme switch
    {
        AppThemePreference.System => "Sistema",
        AppThemePreference.Light => "Claro",
        AppThemePreference.Dark => "Escuro",
        AppThemePreference.Glass => "Vidro",
        _ => theme.ToString(),
    };

    public static string Plural(int count, string singular, string plural) => count == 1 ? singular : plural;

    public static string Count(int count, string singular, string plural) =>
        $"{count} {Plural(count, singular, plural)}";
}
