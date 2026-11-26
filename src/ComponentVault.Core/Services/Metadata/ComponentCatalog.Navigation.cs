using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Navigation() =>
    [
        new()
        {
            Id = "tabs",
            Name = "Tabs",
            Icon = "Tabs24",
            Category = ComponentCategory.Navigation,
            Summary = "Alterna entre visões irmãs do mesmo contexto.",
            Tags = ["abas", "tabs", "segmentado", "sharpnado", "navegação"],
            AddedOn = new DateOnly(2026, 11, 24),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI só oferece abas no nível do Shell (TabBar). Abas dentro da página são montadas à mão ou com bibliotecas como Sharpnado.Tabs.",
                Properties =
                [
                    new("SelectedIndex", "Aba ativa; ligue ao conteúdo (ViewSwitcher) nos dois sentidos."),
                    new("TabType", "Fixed (largura igual) ou Scrollable (muitas abas)."),
                    new("IsSegmented", "Vira um controle segmentado com contorno."),
                ],
                UseCases = ["Visões de um mesmo objeto (visão geral, atividade, membros).", "Filtros de período (dia, semana, mês)."],
                Limitations = ["Mais de 4 abas fixas quebram no telefone: use rolagem ou outra navegação."],
            },
            Variants =
            [
                new()
                {
                    Id = "tabs.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Abas fora do Shell com RadioButton e ControlTemplate: texto e sublinhado na cor de destaque.",
                    Highlights =
                    [
                        "Escolha única, teclado e leitor de tela vêm do grupo de RadioButton.",
                        "Abas do tamanho do texto, alinhadas à esquerda, sobre uma linha fina.",
                        "Só o painel da aba marcada fica visível.",
                        "O sublinhado troca sem deslizar: o deslize é do Custom.",
                    ],
                    Snippets = [Xaml("Uso", "tabs.styled")],
                },
                new()
                {
                    Id = "tabs.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Abas de largura igual com sublinhado que desliza entre elas.",
                    Highlights =
                    [
                        "Um único sublinhado que desliza em 200ms: o olho acompanha a troca.",
                        "Aba ativa em tinta e as demais em cinza legível, não só pela cor do sublinhado.",
                        "O leitor de tela anuncia qual aba está selecionada.",
                    ],
                    Snippets = [Xaml("Uso", "tabs.custom"), CSharp("Seleção e animação", "tabs.custom.code")],
                },
                new()
                {
                    Id = "tabs.sharpnado",
                    Title = "Sharpnado.Tabs",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "Sharpnado.Tabs.Maui",
                    Summary = "TabHostView com abas sublinhadas, ViewSwitcher e controle segmentado.",
                    Highlights =
                    [
                        "TabHostView e ViewSwitcher compartilham o SelectedIndex: zero código de ligação.",
                        "O mesmo componente vira controle segmentado com IsSegmented.",
                        "Cores do design system aplicadas por propriedade, inclusive no tema escuro.",
                    ],
                    Snippets = [Xaml("Uso", "tabs.sharpnado")],
                },
                new()
                {
                    Id = "tabs.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfTabView com indicador deslizante e conteúdo que troca com transição e deslize.",
                    Highlights =
                    [
                        "Abas e conteúdo no mesmo controle: não precisa de um ViewSwitcher à parte.",
                        "Deslizar o dedo troca de aba no celular (EnableSwiping).",
                        "Barra no topo, embaixo ou nas laterais, com rolagem quando as abas não cabem.",
                    ],
                    Snippets = [Xaml("Uso", "tabs.syncfusion")],
                },
                new()
                {
                    Id = "tabs.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "TabView Material com conteúdo por aba e troca animada.",
                    Highlights =
                    [
                        "Cada TabItem tem título e conteúdo no próprio XAML, sem ViewSwitcher.",
                        "Cabeçalhos dividem a largura; a troca de aba é animada (UseAnimation).",
                        "Barra no topo, embaixo ou nas laterais (TabPlacement).",
                    ],
                    Snippets = [Xaml("Uso", "tabs.uranium")],
                },
            ],
        },
        new()
        {
            Id = "popup",
            Name = "Popup",
            Icon = "Window24",
            Category = ComponentCategory.Navigation,
            Summary = "Diálogo modal para confirmar ou escolher.",
            Tags = ["dialog", "modal", "alerta", "confirmação", "toolkit", "action sheet"],
            AddedOn = new DateOnly(2026, 11, 25),
            Docs = new ComponentDocs
            {
                Overview = "Para avisos simples, o diálogo nativo da página basta. Quando o diálogo precisa de identidade visual ou de conteúdo próprio, o Popup do CommunityToolkit.Maui aceita qualquer View e devolve um resultado.",
                Properties =
                [
                    new("DisplayAlertAsync", "Alerta ou confirmação nativa; devolve bool."),
                    new("DisplayActionSheetAsync", "Lista de ações nativa; devolve o texto escolhido."),
                    new("Popup<T>.CloseAsync(resultado)", "Fecha o popup do Toolkit devolvendo um valor."),
                    new("PopupOptions", "Forma, sombra, cor do fundo e se fecha ao tocar fora."),
                ],
                UseCases = ["Confirmar ações destrutivas.", "Escolher uma ação entre poucas."],
                Limitations = ["Diálogo interrompe o fluxo: para ações reversíveis prefira \"Desfazer\"."],
            },
            Variants =
            [
                Base("popup.base", "Alerta, confirmação e lista de ações nativas da página."),
                new()
                {
                    Id = "popup.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Diálogo de confirmação montado com Border e Button, por cima do conteúdo.",
                    Highlights =
                    [
                        "Fundo que escurece e cartão que surge com escala de 0,96 a 1.",
                        "Ícone e cor de perigo para a ação destrutiva, que fica à direita.",
                        "O texto diz o que acontece e o que dá para recuperar (30 dias na lixeira).",
                        "Tocar fora cancela.",
                    ],
                    Snippets = [Xaml("Uso", "popup.styled")],
                },
                new()
                {
                    Id = "popup.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Folha inferior (bottom sheet) com opções de compartilhar, numa moldura de celular.",
                    Highlights =
                    [
                        "O fundo escurece e a folha sobe da borda de baixo, ao alcance do polegar.",
                        "Fecha tocando fora, arrastando a alça para baixo ou escolhendo uma opção.",
                        "O contexto continua visível atrás, diferente de um diálogo no meio da tela.",
                    ],
                    Snippets = [Xaml("Uso", "popup.custom"), CSharp("Controle (BottomSheet.cs)", "control.bottomsheet")],
                },
                new()
                {
                    Id = "popup.toolkit",
                    Title = "Toolkit Popup",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "CommunityToolkit.Maui",
                    Summary = "Diálogo de confirmação próprio sobre o Popup<bool> do CommunityToolkit.",
                    Highlights =
                    [
                        "O título diz a ação e o texto diz o que se perde (18 tarefas, 4 membros).",
                        "O botão destrutivo repete o verbo (\"Excluir projeto\"), nunca \"OK\".",
                        "Botões de mesma largura, cancelar à esquerda; tocar fora cancela.",
                    ],
                    Snippets =
                    [
                        Xaml("Uso", "popup.toolkit"),
                        CSharp("Abrir e ler o resultado", "popup.toolkit.code"),
                        CSharp("Diálogo (ConfirmDialog.cs)", "control.confirmdialog"),
                    ],
                },
                new()
                {
                    Id = "popup.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfPopup de confirmação destrutiva com fundo desfocado e zoom curto.",
                    Highlights =
                    [
                        "Cabeçalho, mensagem e rodapé com dois botões, sem montar o layout.",
                        "Excluir em vermelho, cancelar neutro; tocar fora conta como cancelar.",
                        "Fundo desfocado (OverlayMode Blur) e seis animações de entrada.",
                    ],
                    Snippets = [Xaml("Uso", "popup.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "bottomsheet",
            Name = "Bottom sheet",
            Icon = "PanelBottom20",
            Category = ComponentCategory.Navigation,
            Summary = "Painel que sobe da borda de baixo, com paradas em meia altura e cheio.",
            Tags = ["bottom sheet", "painel", "folha", "gaveta", "mapa", "arrastar", "drawer"],
            AddedOn = new DateOnly(2026, 11, 28),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem bottom sheet. O Base usa um Border no fim do Grid; o Estilizado acrescenta tokens, fundo escurecido e animação; o Custom tem três paradas arrastáveis, como nos apps de mapa; e o SfBottomSheet do Syncfusion Toolkit traz tudo pronto.",
                Properties =
                [
                    new("Stop / State", "Parada atual: recolhido, meia altura ou cheio."),
                    new("HalfExpandedRatio", "Fração da altura na parada do meio (SfBottomSheet)."),
                    new("Header", "Parte sempre visível e área de arrasto (SnapSheet)."),
                    new("CollapseOnOverlayTap", "Tocar no fundo escurecido fecha ou recolhe."),
                ],
                UseCases = ["Detalhes de um ponto no mapa sem sair dele.", "Filtros e ações rápidas que o polegar alcança."],
                Limitations = ["Com uma lista rolável dentro, deixe o arrasto só no cabeçalho para os dois gestos não brigarem."],
            },
            Variants =
            [
                Base("bottomsheet.base", "Border no fim do Grid, mostrado e escondido por IsVisible.",
                    "Surge do nada, sem animação nem fundo escurecido.",
                    "Não fecha tocando fora nem arrastando.",
                    "Cores fixas que não acompanham o tema."),
                new()
                {
                    Id = "bottomsheet.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "O mesmo Border com tokens, alça, fundo que escurece e a folha subindo da borda.",
                    Highlights =
                    [
                        "Cantos de cima arredondados, alça e linhas finas do design system.",
                        "Sobe em 260ms com TranslateTo; o fundo escurece junto.",
                        "Tocar fora fecha; Fechar e Iniciar dividem a largura.",
                        "Não arrasta: as paradas ficam para o Custom.",
                    ],
                    Snippets = [Xaml("Uso", "bottomsheet.styled")],
                },
                new()
                {
                    Id = "bottomsheet.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "SnapSheet: três paradas (recolhido, metade e cheio) arrastando o cabeçalho, como nos apps de mapa.",
                    Highlights =
                    [
                        "O painel acompanha o dedo e, ao soltar, vai para a parada seguinte na direção do arrasto.",
                        "Tocar no cabeçalho sobe uma parada; cheio, tocar no fundo volta à metade.",
                        "O fundo só escurece entre a metade e a cheia: na metade, o mapa continua usável.",
                        "A lista rola normalmente; o arrasto fica no cabeçalho.",
                    ],
                    Snippets = [Xaml("Uso", "bottomsheet.custom"), CSharp("Controle (SnapSheet.cs)", "control.snapsheet")],
                },
                new()
                {
                    Id = "bottomsheet.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfBottomSheet com meia altura, cheio, alça e evento de mudança de estado.",
                    Highlights =
                    [
                        "Content é a tela por baixo e BottomSheetContent é o painel.",
                        "HalfExpandedRatio e FullExpandedRatio definem as paradas.",
                        "Fundo escurecido que fecha ao tocar (CollapseOnOverlayTap).",
                        "Background e a cor da alça são pincéis: as cores do tema entram pelo código.",
                    ],
                    Snippets = [Xaml("Uso", "bottomsheet.syncfusion")],
                },
            ],
        },
    ];
}
