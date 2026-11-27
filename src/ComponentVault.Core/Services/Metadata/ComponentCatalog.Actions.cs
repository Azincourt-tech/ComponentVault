using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Actions() =>
    [
        new()
        {
            Id = "button",
            Name = "Button",
            Icon = "CursorClick24",
            Category = ComponentCategory.Actions,
            Summary = "Dispara uma ação com toque ou clique.",
            Tags = ["botão", "ação", "cta", "material", "carregando"],
            AddedOn = new DateOnly(2026, 11, 10),
            Docs = new ComponentDocs
            {
                Overview = "O Button é o controle de ação mais básico do MAUI. Sem estilo ele herda o visual da plataforma; com o design system ganha hierarquia clara (um primário por tela), estados de toque e altura mínima de 44px.",
                Properties =
                [
                    new("Text", "Rótulo: verbo + objeto (\"Salvar rascunho\", não \"OK\")."),
                    new("Command / CommandParameter", "Ação executada no MVVM. Prefira ao evento Clicked."),
                    new("ImageSource + ContentLayout", "Ícone ao lado do texto e sua posição."),
                    new("IsEnabled", "Quando o Command expõe CanExecute, é controlado por ele."),
                ],
                UseCases =
                [
                    "Ação principal de uma tela: um único botão primário.",
                    "Ações alternativas no secundário; ações leves no terciário (só texto).",
                ],
                Limitations =
                [
                    "O Button nativo não tem estado de carregamento: é preciso compor com ActivityIndicator.",
                ],
            },
            Variants =
            [
                Base("button.base", "Button sem nenhum estilo: o que o MAUI entrega de fábrica em cada plataforma."),
                new()
                {
                    Id = "button.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Hierarquia primário, secundário, terciário e destrutivo, com ícones, tamanhos e carregamento.",
                    Highlights =
                    [
                        "Primário em tinta e um só por tela; a cor de destaque fica para ações de criação.",
                        "Carregando troca o rótulo para \"Salvando…\" e ignora um segundo toque, evitando envio duplicado.",
                        "Toque encolhe para 98,5% e desce 1px: resposta imediata, sem bounce.",
                        "Botão só de ícone tem 44×44 e descrição para leitor de tela.",
                    ],
                    Snippets = [Xaml("Uso", "button.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.button")],
                },
                new()
                {
                    Id = "button.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Segurar para confirmar: o preenchimento cruza o botão enquanto se segura, para ações destrutivas.",
                    Highlights =
                    [
                        "Substitui o diálogo \"Tem certeza?\": a confirmação está no próprio gesto, sem tela a mais.",
                        "Soltar antes do fim desfaz o preenchimento; nada acontece por engano.",
                        "Depois de confirmar, o texto diz o que aconteceu e aparece a opção de desfazer.",
                        "Funciona com dedo e com mouse; o resultado é anunciado ao leitor de tela.",
                    ],
                    Snippets = [Xaml("Uso", "button.custom"), CSharp("Controle (HoldButton.cs)", "control.holdbutton")],
                },
                new()
                {
                    Id = "button.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "Botões Material Design 3 via StyleClass, sem escrever estilos à mão.",
                    Highlights =
                    [
                        "As cinco ênfases do Material 3 (filled, tonal, elevated, outlined, text) com uma linha cada.",
                        "Estilos mesclados só nesta View, para não vazar para o resto do app.",
                    ],
                    Snippets = [Xaml("Uso", "button.uranium")],
                },
                new()
                {
                    Id = "button.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfButton com degradê, modo alternar, contorno tracejado e conteúdo livre.",
                    Highlights =
                    [
                        "Fundo aceita qualquer Brush: degradê da paleta sem desenhar nada.",
                        "IsCheckable faz o mesmo botão ligar e desligar, com o texto dizendo o estado.",
                        "DashArray no contorno vira área de soltar arquivos, com ícone acima do texto.",
                        "Content aceita qualquer layout: ícone, título e detalhe dentro do botão.",
                    ],
                    Snippets = [Xaml("Uso", "button.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "chips",
            Name = "Chips",
            Icon = "Tag24",
            Category = ComponentCategory.Actions,
            Summary = "Opções compactas para escolher, filtrar ou listar tags.",
            Tags = ["chip", "tag", "filtro", "seleção", "pílula"],
            AddedOn = new DateOnly(2026, 11, 24),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem chips. Eles são montados com Border, Label e TapGestureRecognizer sobre uma coleção; o estado selecionado vem da ViewModel.",
                Properties =
                [
                    new("BindableLayout.ItemsSource", "Coleção de chips (FlexLayout com Wrap quebra a linha)."),
                    new("IsSelected (no item)", "Estado de cada chip, ligado por DataTrigger."),
                    new("TapGestureRecognizer", "Alterna ou remove o chip."),
                ],
                UseCases =
                [
                    "Filtros rápidos acima de uma lista.",
                    "Tags digitadas pelo usuário num formulário.",
                ],
                Limitations =
                [
                    "Com muitos itens, prefira um Picker ou uma lista: chips são para até ~8 opções.",
                ],
            },
            Variants =
            [
                new()
                {
                    Id = "chips.base",
                    Title = "Base MAUI",
                    Tier = VariantTier.Base,
                    Origin = ComponentOrigin.Native,
                    Summary = "Sem chips no MAUI: RadioButton para escolher um e CheckBox para filtrar vários.",
                    Highlights =
                    [
                        "Funciona e é acessível, mas ocupa mais espaço e não lembra um chip.",
                        "O CheckBox não tem rótulo: o texto vai num Label ao lado.",
                        "No Windows o CheckBox reserva ~120px de largura, então o rótulo fica longe da caixa.",
                    ],
                    Snippets = [Xaml("Escolha", "chips.base"), Xaml("Filtro", "chips.base.filter")],
                },
                new()
                {
                    Id = "chips.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Chips de escolha, de filtro e de entrada, com 36px de altura.",
                    Highlights =
                    [
                        "Escolha tem sempre exatamente um ativo; filtro aceita vários.",
                        "Filtro ativo ganha um check, para o estado não depender só da cor.",
                        "O resumo abaixo diz em texto o que está sendo filtrado.",
                        "Tags removíveis têm um alvo de toque próprio para o X.",
                    ],
                    Snippets = [Xaml("Uso", "chips.styled")],
                },
                new()
                {
                    Id = "chips.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Escolha com uma pílula que desliza até o chip tocado; filtro em que o check entra com animação.",
                    Highlights =
                    [
                        "Uma pílula só para a escolha: o olho acompanha a mudança, em vez de ver dois chips trocarem de cor.",
                        "No filtro, o chip abre espaço e o check cresce dentro dele; ao ligar, um pulo de 6%.",
                        "O estado continua legível sem cor: check no filtro, texto em destaque na escolha.",
                        "O deslize é o mesmo SlidingIndicator da barra de navegação e das abas do app.",
                    ],
                    Snippets =
                    [
                        Xaml("Escolha", "chips.custom"),
                        Xaml("Filtro", "chips.custom.filter"),
                        CSharp("ChoiceChipGroup (ChipControls.cs)", "control.choicechips"),
                        CSharp("FilterChip (ChipControls.cs)", "control.filterchip"),
                    ],
                },
                new()
                {
                    Id = "chips.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfChipGroup nos tipos escolha, filtro e entrada, com as cores do design system.",
                    Highlights =
                    [
                        "ChipType troca o comportamento: Choice (um), Filter (vários, com check), Input (com remover).",
                        "Um estilo só para os três grupos: altura de 36px, raio de pílula e fonte do design system.",
                        "Escolha preenchida na cor de destaque; filtro no tom leve, para os dois não competirem.",
                    ],
                    Snippets = [Xaml("Uso", "chips.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "fab",
            Name = "FAB",
            Icon = "Add24",
            Category = ComponentCategory.Actions,
            Summary = "Botão flutuante com a ação principal da tela, sozinho ou abrindo um leque de ações.",
            Tags = ["fab", "botão flutuante", "speed dial", "leque", "criar", "ação principal"],
            AddedOn = new DateOnly(2026, 11, 28),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem botão flutuante. O Base usa um Button redondo no canto do Grid; o Estilizado é um FAB estendido que encolhe ao rolar; e o Custom abre as ações em leque a partir do próprio botão. Nenhuma lib livre com .NET 10 traz um FAB pronto.",
                Properties =
                [
                    new("HorizontalOptions / VerticalOptions = End", "Prende o botão no canto do Grid, por cima do conteúdo."),
                    new("Actions", "Ações do leque, com ícone e rótulo (SpeedDial)."),
                    new("ActionInvoked (evento)", "Disparado ao tocar numa ação do leque."),
                ],
                UseCases = ["A ação principal de uma lista: nova nota, novo e-mail, novo item.", "Até quatro formas de criar algo, num leque."],
                Limitations = ["Um FAB por tela. Ações secundárias ficam na barra ou num menu."],
            },
            Variants =
            [
                Base("fab.base", "Button redondo no canto do Grid, abrindo a lista de ações nativa.",
                    "O + é texto e muda com a fonte da plataforma.",
                    "As opções abrem num diálogo genérico, longe do botão."),
                new()
                {
                    Id = "fab.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "FAB estendido com ícone e rótulo, que encolhe até o círculo ao rolar a lista.",
                    Highlights =
                    [
                        "O rótulo diz a ação (\"Nova nota\") enquanto a lista está no topo.",
                        "Rolar para baixo recolhe o rótulo; rolar para cima estende de novo.",
                        "Cantos de 18 e a única sombra do app, para flutuar sobre o conteúdo.",
                    ],
                    Snippets = [Xaml("Uso", "fab.styled")],
                },
                new()
                {
                    Id = "fab.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "SpeedDial: o + vira × e as ações saem do botão num quarto de círculo, com mola.",
                    Highlights =
                    [
                        "As ações aparecem junto do dedo, em vez de num diálogo no meio da tela.",
                        "Saem uma depois da outra (40ms) e voltam na ordem inversa.",
                        "O fundo escurece; tocar fora ou numa ação fecha.",
                        "Cada ação tem rótulo, também para o leitor de tela.",
                    ],
                    Snippets = [Xaml("Uso", "fab.custom"), CSharp("Controle (SpeedDial.cs)", "control.speeddial")],
                },
            ],
        },
    ];
}
