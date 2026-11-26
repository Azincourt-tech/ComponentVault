namespace ComponentVault.Lib.Theming;

// snippet:theming.fieldframes
/// <summary>
/// Estados da moldura de campo (estilo <c>FieldFrame</c>): em repouso o campo é "preenchido"
/// (FieldFill, contorno quase invisível); no foco ele clareia (SurfaceRaised) e ganha o anel de 2px
/// na cor de destaque. Erro e sucesso vencem o foco, para o feedback não sumir ao digitar.
/// </summary>
public static class FieldFrames
{
    /// <param name="frame">A moldura do campo.</param>
    /// <param name="focused">Se o campo de dentro está com o foco.</param>
    /// <param name="stateStroke">Chave de cor do estado (ex.: "Danger", "Success"), ou nulo no estado normal.</param>
    public static void Apply(Border frame, bool focused, string? stateStroke = null)
    {
        frame.SetThemeBrush(Border.StrokeProperty, stateStroke ?? (focused ? "Accent" : "FieldStroke"));
        frame.SetThemeColor(VisualElement.BackgroundColorProperty, focused ? "SurfaceRaised" : "FieldFill");
        frame.StrokeThickness = focused || stateStroke is not null ? 2 : 1;
    }
}
// /snippet:theming.fieldframes
