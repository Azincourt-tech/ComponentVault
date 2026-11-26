using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Selection() =>
    [
        new()
        {
            Id = "switch",
            Name = "Switch",
            Icon = "ToggleRight24",
            Category = ComponentCategory.Selection,
            Summary = "Liga e desliga uma opção com efeito imediato.",
            Tags = ["toggle", "alternador", "liga", "desliga", "graphics", "configuração"],
            AddedOn = new DateOnly(2026, 11, 18),
            Docs = new ComponentDocs
            {
                Overview = "O Switch representa uma escolha binária aplicada na hora. O MAUI só deixa trocar as cores; para controlar forma, tamanho e animação, a variante custom desenha o controle inteiro com Microsoft.Maui.Graphics.",
                Properties =
                [
                    new("IsToggled (TwoWay)", "Estado atual."),
                    new("OnColor / ThumbColor", "Cores da trilha ligada e do botão."),
                    new("Toggled (evento)", "Disparado a cada mudança; no MVVM prefira o binding."),
                ],
                UseCases = ["Preferências com efeito imediato (tema, notificações).", "Não use quando a escolha precisa de confirmação: aí cabe CheckBox + botão."],
                Limitations = ["A cor da trilha desligada não é exposta pelo Switch nativo.", "Tamanho e animação seguem a plataforma."],
            },
            Variants =
            [
                Base("switch.base", "Switch sem estilo, com o rótulo montado à parte."),
                new()
                {
                    Id = "switch.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Linhas de configuração com título e descrição; a linha inteira alterna.",
                    Highlights =
                    [
                        "A linha inteira é o alvo de toque, não só os 40px do controle.",
                        "Título diz o quê; descrição diz a consequência.",
                        "Opção indisponível mostra o motivo em texto, não só esmaecida.",
                    ],
                    Snippets = [Xaml("Uso", "switch.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.controls")],
                },
                new()
                {
                    Id = "switch.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "VaultToggle desenhado com GraphicsView: trilha, sombra e animação próprias.",
                    Highlights =
                    [
                        "Mesmo desenho no Android e no Windows, com animação de 180ms sem bounce.",
                        "A trilha desligada usa o token do tema e muda junto com claro/escuro.",
                        "Estado anunciado em texto (\"Ligado\"/\"Desligado\") para leitores de tela.",
                    ],
                    Snippets = [Xaml("Uso", "switch.custom"), CSharp("Desenho (VaultToggle.cs)", "control.toggle")],
                },
            ],
        },
        new()
        {
            Id = "checkbox",
            Name = "CheckBox",
            Icon = "CheckboxChecked24",
            Category = ComponentCategory.Selection,
            Summary = "Marca uma ou várias opções independentes.",
            Tags = ["caixa de seleção", "múltipla escolha", "selecionar tudo", "formulário", "uranium"],
            AddedOn = new DateOnly(2026, 11, 19),
            Docs = new ComponentDocs
            {
                Overview = "O CheckBox do MAUI é só a caixa: não tem texto, não tem estado indeterminado e o rótulo ao lado não responde ao toque. As variantes mostram como resolver isso compondo ou usando o UraniumUI.",
                Properties =
                [
                    new("IsChecked (TwoWay)", "Estado da caixa."),
                    new("Color", "Cor da caixa marcada."),
                    new("CheckedChanged (evento)", "Disparado a cada mudança."),
                ],
                UseCases = ["Escolhas múltiplas e independentes (notificações, filtros).", "Aceite de termos, sempre com texto clicável."],
                Limitations = ["Não existe estado indeterminado nativo para \"selecionar tudo\".", "A área de toque é só a caixa."],
            },
            Variants =
            [
                Base("checkbox.base", "CheckBox nativo com Label ao lado (o texto não marca a caixa)."),
                new()
                {
                    Id = "checkbox.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Lista com selecionar tudo em três estados, linhas inteiras clicáveis e descrição.",
                    Highlights =
                    [
                        "Selecionar tudo em três estados: vazio, traço (alguns) e marcado (todos).",
                        "Contador \"2 de 3\" deixa o estado explícito sem depender da caixa.",
                        "Toque na linha inteira marca; a caixa sozinha seria um alvo de 20px.",
                        "Item bloqueado explica por que não pode ser desmarcado.",
                    ],
                    Snippets = [Xaml("Uso", "checkbox.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.controls")],
                },
                new()
                {
                    Id = "checkbox.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Check desenhado: a caixa se enche do centro e o traço do check é desenhado de ponta a ponta.",
                    Highlights =
                    [
                        "Animação de 220ms que mostra a ação acontecendo, não só o estado final.",
                        "Estado indeterminado (um traço) no \"Todas\" quando só parte está marcada.",
                        "A linha inteira é o alvo de toque; o contorno forte deixa a caixa vazia visível.",
                    ],
                    Snippets = [Xaml("Uso", "checkbox.custom"), CSharp("Controle (AnimatedCheck.cs)", "control.animatedcheck")],
                },
                new()
                {
                    Id = "checkbox.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "CheckBox Material com texto clicável e três estilos de caixa.",
                    Highlights =
                    [
                        "Texto faz parte do controle: tocar no rótulo marca a caixa.",
                        "Tipos Regular, Filled e Material sem estilo próprio.",
                        "Suporta validação dentro de formulários do UraniumUI.",
                    ],
                    Snippets = [Xaml("Uso", "checkbox.uranium")],
                },
            ],
        },
        new()
        {
            Id = "radio",
            Name = "RadioButton",
            Icon = "RadioButton24",
            Category = ComponentCategory.Selection,
            Summary = "Escolhe exatamente uma opção de um grupo.",
            Tags = ["opção", "escolha única", "plano", "grupo", "card", "uranium"],
            AddedOn = new DateOnly(2026, 11, 21),
            Docs = new ComponentDocs
            {
                Overview = "O RadioButton agrupa opções mutuamente exclusivas pelo GroupName. Trocando o ControlTemplate, o mesmo controle vira um card de opção sem perder grupo, teclado e acessibilidade.",
                Properties =
                [
                    new("GroupName", "Opções com o mesmo nome se excluem."),
                    new("Value", "Valor associado à opção, útil no binding."),
                    new("Content", "Texto ou, com ControlTemplate, qualquer View."),
                    new("ControlTemplate", "Substitui o visual inteiro mantendo o comportamento."),
                ],
                UseCases = ["Escolha de plano, frete ou forma de pagamento.", "Até ~5 opções visíveis; mais que isso, use Picker."],
                Limitations = ["Sem ControlTemplate, o círculo e o texto seguem a plataforma."],
            },
            Variants =
            [
                Base("radio.base", "RadioButton nativo com Content em texto e GroupName."),
                new()
                {
                    Id = "radio.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Cards de opção via ControlTemplate: título, preço e descrição, com seleção clara.",
                    Highlights =
                    [
                        "O card inteiro é o alvo de toque e continua sendo um RadioButton de verdade.",
                        "Seleção dupla: borda de 2px na cor de destaque e fundo suave, não só o círculo.",
                        "Preço alinhado à direita para comparar opções num relance.",
                        "Linha de status confirma a escolha em texto.",
                    ],
                    Snippets = [Xaml("Uso e ControlTemplate", "radio.styled")],
                },
                new()
                {
                    Id = "radio.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Opções de entrega em cards, com o ponto do marcador crescendo com mola ao escolher.",
                    Highlights =
                    [
                        "O card inteiro é tocável; o escolhido ganha contorno e fundo no tom do destaque.",
                        "O ponto entra com mola e sai direto: a escolha chama atenção, a saída não.",
                        "Um resumo em frase confirma prazo e preço da opção escolhida.",
                    ],
                    Snippets = [Xaml("Uso", "radio.custom"), CSharp("Controle (AnimatedRadio.cs)", "control.animatedradio")],
                },
                new()
                {
                    Id = "radio.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "RadioButtonGroupView: o grupo expõe SelectedItem e SelectedIndex.",
                    Highlights =
                    [
                        "Contêiner de grupo no lugar de GroupName espalhado pelos itens.",
                        "SelectedItem/SelectedIndex prontos para binding no MVVM.",
                        "Validação de \"escolha obrigatória\" integrada.",
                    ],
                    Snippets = [Xaml("Uso", "radio.uranium")],
                },
            ],
        },
        new()
        {
            Id = "slider",
            Name = "Slider",
            Icon = "Options24",
            Category = ComponentCategory.Selection,
            Summary = "Escolhe um valor (ou uma faixa) arrastando.",
            Tags = ["controle deslizante", "faixa", "range", "volume", "preço", "graphics"],
            AddedOn = new DateOnly(2026, 11, 22),
            Docs = new ComponentDocs
            {
                Overview = "O Slider escolhe um valor contínuo entre Minimum e Maximum. Para valores legíveis é preciso encaixar em passos e mostrar o valor; para faixas (mínimo e máximo) o MAUI não tem controle, e a variante custom desenha um.",
                Properties =
                [
                    new("Minimum / Maximum / Value", "Limites e valor atual (double)."),
                    new("MinimumTrackColor / MaximumTrackColor / ThumbColor", "Cores da trilha e do polegar."),
                    new("ValueChanged (evento)", "Útil para encaixar o valor em passos."),
                ],
                UseCases = ["Ajustes com prévia imediata (volume, tamanho do texto).", "Filtros de faixa, como preço mínimo e máximo."],
                Limitations = ["Sem passo nativo: o valor é contínuo.", "Sem slider de faixa no MAUI."],
            },
            Variants =
            [
                Base("slider.base", "Slider nativo com valor contínuo e sem rótulos."),
                new()
                {
                    Id = "slider.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Rótulo e valor na mesma linha, limites embaixo, passo e prévia ao vivo.",
                    Highlights =
                    [
                        "Valor atual sempre visível ao lado do rótulo, com unidade.",
                        "Encaixe em passos: nada de \"40,37%\".",
                        "Prévia imediata do efeito (o texto de exemplo muda de tamanho).",
                    ],
                    Snippets = [Xaml("Uso", "slider.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.controls")],
                },
                new()
                {
                    Id = "slider.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "RangeSlider com dois polegares desenhado em GraphicsView.",
                    Highlights =
                    [
                        "O polegar mais perto do toque é o que se move; os dois nunca se cruzam.",
                        "Halo no polegar ativo mostra qual está sendo arrastado.",
                        "Faixa escrita por extenso (\"R$ 120 – R$ 680\") acima do controle.",
                    ],
                    Snippets = [Xaml("Uso", "slider.custom"), CSharp("Controle (RangeSlider.cs)", "control.rangeslider")],
                },
            ],
        },
        new()
        {
            Id = "segmented",
            Name = "Segmented control",
            Icon = "Tabs24",
            Category = ComponentCategory.Selection,
            Summary = "Alterna entre 2 a 5 visões do mesmo conteúdo.",
            Tags = ["segmentado", "abas", "período", "filtro", "sharpnado", "toggle group"],
            AddedOn = new DateOnly(2026, 11, 23),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem controle segmentado. A variante custom é montada com Border e Label e um indicador animado; a de biblioteca usa as abas segmentadas do Sharpnado.Tabs com ViewSwitcher.",
                Properties =
                [
                    new("Items (SegmentedControl)", "Textos dos segmentos."),
                    new("SelectedIndex (TwoWay)", "Segmento escolhido."),
                    new("IsSegmented / SegmentedOutlineColor (Sharpnado)", "Liga o modo segmentado do TabHostView."),
                ],
                UseCases = ["Trocar a visão de um mesmo dado (Dia, Semana, Mês).", "Alternar modo de exibição (Lista, Grade)."],
                Limitations = ["Mais de 5 opções ou textos longos: use abas roláveis ou Picker."],
            },
            Variants =
            [
                new()
                {
                    Id = "segmented.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Trilha neutra com indicador que desliza até a opção escolhida.",
                    Highlights =
                    [
                        "Indicador desliza em 200ms (ease-out, sem bounce) em vez de piscar.",
                        "Segmentos de largura igual e altura mínima de 36px.",
                        "Cada segmento anuncia \"2 de 4, selecionado\" ao leitor de tela.",
                    ],
                    Snippets = [Xaml("Uso", "segmented.custom"), CSharp("Controle (SegmentedControl.cs)", "control.segmented")],
                },
                new()
                {
                    Id = "segmented.sharpnado",
                    Title = "Sharpnado.Tabs",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "Sharpnado.Tabs.Maui",
                    Summary = "TabHostView segmentado + ViewSwitcher trocando o conteúdo.",
                    Highlights =
                    [
                        "Separadores e contorno prontos, sem desenhar nada.",
                        "ViewSwitcher mantém só a visão escolhida visível.",
                        "Seleção em tinta (preto/branco) segue o primário do design system.",
                    ],
                    Snippets = [Xaml("Uso", "segmented.sharpnado")],
                },
                new()
                {
                    Id = "segmented.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfSegmentedControl com pílula preenchida e, na mesma lib, a versão sublinhada.",
                    Highlights =
                    [
                        "Pílula animada, sublinhado ou borda: só muda o SelectionIndicatorPlacement.",
                        "Rola sozinho quando há mais segmentos do que cabem (VisibleSegmentsCount).",
                        "Navegação por teclado e realce de foco já vêm prontos.",
                        "Toolkit open source (MIT): não pede chave, diferente da linha paga do Syncfusion.",
                    ],
                    Snippets = [Xaml("Uso", "segmented.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "rating",
            Name = "Rating",
            Icon = "Star24",
            Category = ComponentCategory.Selection,
            Summary = "Avaliação por estrelas, interativa ou só leitura.",
            Tags = ["estrelas", "avaliação", "nota", "review", "graphics"],
            AddedOn = new DateOnly(2026, 11, 24),
            Docs = new ComponentDocs
            {
                Overview = "Não há controle de avaliação no MAUI. O RatingView desenha as estrelas com PathF e recorta a parte preenchida pela fração do valor, servindo tanto para votar quanto para exibir médias.",
                Properties =
                [
                    new("Value (TwoWay)", "Nota atual; aceita frações na exibição."),
                    new("AllowHalf", "Permite votar em meia estrela."),
                    new("IsReadOnly", "Só exibição (médias)."),
                    new("StarSize / Maximum", "Tamanho e quantidade de estrelas."),
                ],
                UseCases = ["Avaliar um item depois de usar.", "Exibir a média com a contagem de avaliações ao lado."],
                Limitations = ["Estrelas sozinhas são ambíguas: mostre o significado da nota em texto."],
            },
            Variants =
            [
                new()
                {
                    Id = "rating.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "RatingView desenhado com Graphics: toque, meia estrela, prévia no hover e só leitura.",
                    Highlights =
                    [
                        "Significado da nota em texto (\"Muito bom\") logo abaixo das estrelas.",
                        "Prévia ao passar o mouse no Windows antes de confirmar o clique.",
                        "Médias fracionadas (4,3) aparecem exatas, não arredondadas.",
                    ],
                    Snippets = [Xaml("Uso", "rating.custom"), CSharp("Controle (RatingView.cs)", "control.rating")],
                },
            ],
        },
    ];
}
