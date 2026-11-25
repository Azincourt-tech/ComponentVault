namespace ComponentVault.Core.Models;

/// <summary>De onde vem a implementação de uma variante (vira o badge de origem, RF07).</summary>
public enum ComponentOrigin
{
    Native,
    Custom,
    FreeLibrary,
    Syncfusion,
}

/// <summary>
/// Nível de acabamento de uma variante. É o eixo principal da vitrine:
/// o mesmo componente aparece cru, estilizado pelo design system e via biblioteca.
/// </summary>
public enum VariantTier
{
    /// <summary>Controle MAUI puro, sem nenhum estilo aplicado.</summary>
    Base,

    /// <summary>O mesmo controle extraído ao máximo com os tokens do design system.</summary>
    Styled,

    /// <summary>Implementação desenhada à mão (Microsoft.Maui.Graphics, SkiaSharp, handlers).</summary>
    Custom,

    /// <summary>Versão construída com uma biblioteca de terceiros.</summary>
    Library,
}

public enum ComponentCategory
{
    Actions,
    Inputs,
    Selection,
    Feedback,
    Display,
    Containers,
    Navigation,
    Data,
}

public enum CodeLanguage
{
    Xaml,
    CSharp,
}

public enum AppThemePreference
{
    System,
    Light,
    Dark,
    Glass,
}

public enum DetailTab
{
    Preview,

    /// <summary>Todos os níveis renderizados ao vivo, um ao lado do outro.</summary>
    Compare,
    Docs,
    Code,
}
