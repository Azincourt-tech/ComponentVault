namespace ComponentVault.App.Controls;

/// <summary>
/// Faz a página passar por baixo da barra de título do Windows, para a luz do fundo animado ir
/// até o topo da janela. O MAUI reserva a altura da barra com uma margem no <c>ContentGrid</c> do
/// Shell; aqui a margem é zerada e o espaço volta só para o conteúdo, pelos recursos
/// <c>TitleBarInset</c> (padding do Grid da página) e <c>TitleBarBleed</c> (margem negativa do
/// fundo). Em tela cheia a barra some e os dois viram zero.
/// </summary>
public static class TitleBarOverlay
{
    public const double Height = 40;

    /// <summary>Liga ou desliga o espaço da barra nas páginas (ex.: tela cheia no modo Apresentar).</summary>
    public static void SetVisible(bool visible)
    {
        if (Application.Current?.Resources is not { } resources) return;
        var inset = visible ? Height : 0;
        resources["TitleBarInset"] = new Thickness(0, inset, 0, 0);
        resources["TitleBarBleed"] = new Thickness(0, -inset, 0, 0);
    }

#if WINDOWS
    public static void Attach(Window window)
    {
        // A árvore nativa só existe depois que a página raiz carrega; antes disso o Content é nulo.
        var hooked = false;
        window.Page!.Loaded += (_, _) =>
        {
            if (hooked || window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native) return;
            if (native.Content is not Microsoft.UI.Xaml.FrameworkElement root) return;
            hooked = true;
            root.LayoutUpdated += (_, _) => Apply(root);
            Apply(root);
        };
    }

    private static Microsoft.UI.Xaml.FrameworkElement? _content;
    private static Microsoft.UI.Xaml.FrameworkElement? _bar;

    // Procura o ContentGrid do Shell (margem de cima = altura da barra) e o contêiner do título.
    // Roda a cada passada de layout, porque o MAUI reaplica a margem; a busca na árvore só
    // acontece enquanto os dois não foram achados (ou saíram da árvore).
    private static void Apply(Microsoft.UI.Xaml.FrameworkElement root)
    {
        // Há mais de um "ContentGrid" na árvore (o do painel lateral tem só margem à direita): o da
        // página fica dentro do ContentRoot.
        if (_content?.XamlRoot is null) _content = Find(root, "ContentRoot") is { } area ? Find(area, "ContentGrid") : null;
        if (_bar?.XamlRoot is null) _bar = Find(root, "AppTitleBarContainer");

        if (_content is { } content && content.Margin.Top != 0)
        {
            content.Margin = new Microsoft.UI.Xaml.Thickness(0);
            // A grade do Shell tem um cinza translúcido de fundo; a página já pinta o seu.
            if (content is Microsoft.UI.Xaml.Controls.Panel panel) panel.Background = null;
        }
        if (_bar is { } bar && Microsoft.UI.Xaml.Controls.Canvas.GetZIndex(bar) < 1)
        {
            Microsoft.UI.Xaml.Controls.Canvas.SetZIndex(bar, 1);
        }
    }

    private static Microsoft.UI.Xaml.FrameworkElement? Find(Microsoft.UI.Xaml.DependencyObject node, string name)
    {
        if (node is Microsoft.UI.Xaml.FrameworkElement element && element.Name == name) return element;
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++)
        {
            if (Find(Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(node, i), name) is { } found) return found;
        }
        return null;
    }
#endif
}
