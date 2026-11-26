using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Containers() =>
    [
        new()
        {
            Id = "glass",
            Name = "Glass",
            Icon = "Sparkle24",
            Category = ComponentCategory.Containers,
            Summary = "Superfícies de vidro translúcido sobre luz difusa, no estilo Liquid Glass.",
            Tags = ["vidro", "liquid glass", "translúcido", "blur", "apple", "tema"],
            AddedOn = new DateOnly(2026, 11, 25),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem desfoque de fundo multiplataforma. O efeito de vidro sai de três camadas: algo vivo atrás (gradientes radiais), uma superfície branca translúcida e uma aresta com brilho no topo. É a mesma técnica do tema Vidro do app.",
                Properties =
                [
                    new("BackgroundColor (#2EFFFFFF)", "Branco com ~18% de opacidade: deixa a cor de trás atravessar."),
                    new("Stroke (LinearGradientBrush)", "Aresta clara em cima, quase invisível embaixo: simula a luz batendo no vidro."),
                    new("Shadow", "Sombra larga e difusa para descolar o painel do fundo."),
                    new("RadialGradientBrush", "Manchas de luz que dão contraste e profundidade ao vidro."),
                ],
                UseCases =
                [
                    "Camadas flutuantes sobre conteúdo rico: players, barras de ferramentas, cards de destaque.",
                    "Temas de marca onde a interface deve parecer leve e luminosa.",
                ],
                Limitations =
                [
                    "Sem desfoque real, o vidro depende de um fundo suave; sobre texto ou imagem detalhada, a leitura cai.",
                    "Texto sobre vidro precisa de contraste alto (branco sobre fundo escuro).",
                ],
            },
            Variants =
            [
                new()
                {
                    Id = "glass.base",
                    Title = "Base MAUI",
                    Tier = VariantTier.Base,
                    Origin = ComponentOrigin.Native,
                    Summary = "Border com fundo translúcido, sem cena atrás e sem aresta de luz: um cinza lavado.",
                    Highlights =
                    [
                        "Só transparência não faz vidro; no tema Claro o painel quase some.",
                        "O MAUI não tem desfoque de fundo multiplataforma.",
                    ],
                    Snippets = [Xaml("Uso", "glass.base")],
                },
                new()
                {
                    Id = "glass.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Cards do design system só com tokens: sólidos no Claro e no Escuro, vidro no tema Vidro.",
                    Highlights =
                    [
                        "Nenhuma cor fixa: Surface e CardStroke vêm da paleta ativa.",
                        "No tema Vidro, Surface vira branco translúcido e CardStroke vira a aresta de luz.",
                        "O fundo vivo é o AmbientBackground do app, não uma cena própria do componente.",
                        "É assim que o app inteiro vira vidro: troca de paleta, não de marcação.",
                    ],
                    Snippets = [Xaml("Uso", "glass.styled")],
                },
                new()
                {
                    Id = "glass.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Player, indicador e barra flutuante em vidro, sobre uma cena de luz.",
                    Highlights =
                    [
                        "Aresta com degradê (branco 60% → 4%): a luz parece vir de cima, como no Liquid Glass.",
                        "Cápsulas de 44px na barra flutuante: alvo de toque confortável sem pesar o vidro.",
                        "Texto sempre branco sobre a cena escura; metadado a 75% de opacidade, nunca abaixo.",
                        "A mesma receita alimenta o tema Vidro do app, com a luz ambiente derivando devagar.",
                    ],
                    Snippets =
                    [
                        Xaml("Uso", "glass.custom"),
                        CSharp("Luz ambiente (AmbientBackground.cs)", "control.ambient"),
                    ],
                },
            ],
        },
        new()
        {
            Id = "card",
            Name = "Card",
            Icon = "Board24",
            Category = ComponentCategory.Containers,
            Summary = "Agrupa conteúdo relacionado numa superfície.",
            Tags = ["border", "container", "superfície", "expander", "toolkit", "accordion"],
            AddedOn = new DateOnly(2026, 11, 20),
            Docs = new ComponentDocs
            {
                Overview = "No MAUI o card é montado com Border (o Frame está obsoleto desde o .NET 9). O design system padroniza raio de 16, linha fina no lugar de sombra e espaçamentos.",
                Properties =
                [
                    new("StrokeShape", "Forma da borda, ex.: RoundRectangle 16."),
                    new("Stroke / StrokeThickness", "Cor e espessura do contorno."),
                    new("Padding", "Espaço interno do conteúdo."),
                ],
                UseCases = ["Resumos, itens de lista ricos e painéis agrupados."],
                Limitations = ["Border aceita um único filho: use um layout dentro dele."],
            },
            Variants =
            [
                Base("card.base", "Border com os valores padrão."),
                new()
                {
                    Id = "card.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Card de resumo, card com lista e card clicável do design system.",
                    Highlights =
                    [
                        "Linha fina de 1px no lugar de sombra: separa sem pesar, nos dois temas.",
                        "Uma única ação por card, no botão secundário; o primário fica para a tela.",
                        "Linhas de lista com 56px de altura mínima e a linha inteira como alvo de toque.",
                        "O card clicável responde ao hover subindo 2px e escurecendo a borda.",
                    ],
                    Snippets = [Xaml("Uso", "card.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.card")],
                },
                new()
                {
                    Id = "card.expander",
                    Title = "Toolkit Expander",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "CommunityToolkit.Maui",
                    Summary = "Seções recolhíveis dentro de um card, com o Expander do CommunityToolkit.",
                    Highlights =
                    [
                        "Cabeçalho inteiro clicável, com 52px de altura mínima.",
                        "A seta gira 180° para mostrar o estado, além do conteúdo aparecer.",
                        "Resumo à vista no cabeçalho (ex.: \"Cartão final 4021\") sem precisar abrir.",
                    ],
                    Snippets = [Xaml("Uso", "card.expander")],
                },
            ],
        },
        new()
        {
            Id = "list",
            Name = "List",
            Icon = "AppsList24",
            Category = ComponentCategory.Containers,
            Summary = "Lista de itens com separadores, ícones e ações por linha.",
            Tags = ["collectionview", "lista", "swipe", "configurações", "excluir", "desfazer"],
            AddedOn = new DateOnly(2026, 11, 22),
            Docs = new ComponentDocs
            {
                Overview = "A CollectionView é a lista virtualizada do MAUI. Para listas curtas e estáticas, como configurações, um BindableLayout dentro de um card é mais simples e dá controle total do visual.",
                Properties =
                [
                    new("ItemsSource / ItemTemplate", "Dados e o modelo visual de cada item."),
                    new("SelectionMode", "None, Single ou Multiple."),
                    new("SwipeView.RightItems", "Ações reveladas ao deslizar a linha."),
                    new("FlyoutBase.ContextFlyout", "Menu de clique direito no Windows."),
                ],
                UseCases = ["Caixas de entrada, configurações e resultados de busca."],
                Limitations =
                [
                    "Deslizar não é descoberto com mouse: ofereça também menu de contexto.",
                    "BindableLayout não virtualiza; para centenas de itens use CollectionView.",
                ],
            },
            Variants =
            [
                Base("list.base", "CollectionView de strings com o visual padrão."),
                new()
                {
                    Id = "list.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Lista de configurações agrupada e caixa de entrada com excluir e desfazer.",
                    Highlights =
                    [
                        "Ícone, título, descrição e valor atual na linha: dá para decidir sem abrir.",
                        "Separadores recuados até o texto, alinhando a leitura.",
                        "Excluir por deslize ou clique direito, sem diálogo, e sempre com \"Desfazer\".",
                    ],
                    Snippets = [Xaml("Uso", "list.styled"), CSharp("Excluir e desfazer", "list.styled.code")],
                },
            ],
        },
        new()
        {
            Id = "carousel",
            Name = "Carousel",
            Icon = "SlideMultiple24",
            Category = ComponentCategory.Containers,
            Summary = "Conteúdo em cartões horizontais, um de cada vez.",
            Tags = ["carouselview", "indicador", "slides", "onboarding", "paginação"],
            AddedOn = new DateOnly(2026, 11, 23),
            Docs = new ComponentDocs
            {
                Overview = "A CarouselView mostra itens um a um com rolagem horizontal e se liga a uma IndicatorView. Use para conteúdo de mesmo peso em que a ordem importa, como um passo a passo.",
                Properties =
                [
                    new("PeekAreaInsets", "Quanto do próximo item fica visível na borda."),
                    new("Position / PositionChanged", "Índice atual e o evento de troca."),
                    new("Loop", "Volta ao início no fim. Desligue em sequências com começo e fim."),
                    new("IndicatorView", "Indicador de posição ligado ao carrossel."),
                ],
                UseCases = ["Onboarding e destaques de mesmo peso."],
                Limitations =
                [
                    "Conteúdo escondido é pouco visto: nunca coloque ali algo essencial.",
                    "No Windows sem toque, o arrasto não é óbvio: inclua setas.",
                ],
            },
            Variants =
            [
                Base("carousel.base", "CarouselView com IndicatorView padrão."),
                new()
                {
                    Id = "carousel.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Cartões com espiada do próximo, setas, \"2 de 4\" e indicador que se estica.",
                    Highlights =
                    [
                        "A borda do próximo cartão aparece: fica claro que há mais conteúdo.",
                        "Setas e posição em texto funcionam com mouse, teclado e leitor de tela.",
                        "Nunca avança sozinho e não dá a volta: o usuário controla o ritmo.",
                        "O ponto atual vira uma barra em 200ms, sem bounce.",
                    ],
                    Snippets = [Xaml("Uso", "carousel.styled"), CSharp("Indicador e navegação", "carousel.styled.code")],
                },
            ],
        },
    ];
}
