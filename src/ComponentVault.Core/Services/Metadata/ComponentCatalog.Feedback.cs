using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Feedback() =>
    [
        new()
        {
            Id = "progress",
            Name = "Progress",
            Icon = "Gauge24",
            Category = ComponentCategory.Feedback,
            Summary = "Mostra quanto de uma tarefa já foi feito.",
            Tags = ["progresso", "loading", "carregando", "barra", "anel", "spinner", "etapas"],
            AddedOn = new DateOnly(2026, 11, 22),
            Docs = new ComponentDocs
            {
                Overview = "ProgressBar mostra progresso determinado e ActivityIndicator, espera indeterminada. Bom progresso sempre diz o que está acontecendo, quanto falta e como cancelar.",
                Properties =
                [
                    new("Progress (ProgressBar)", "Valor de 0 a 1."),
                    new("ProgressTo(valor, ms, easing)", "Anima até o valor; use Linear para progresso real."),
                    new("IsRunning (ActivityIndicator)", "Liga e desliga a animação de espera."),
                    new("ProgressColor / Color", "Cor do preenchimento."),
                ],
                UseCases =
                [
                    "Upload, download e processamento com duração conhecida (barra com porcentagem).",
                    "Fluxos em etapas, como cadastro e checkout.",
                ],
                Limitations =
                [
                    "A ProgressBar nativa não tem rótulo, cantos arredondados nem altura configurável em todas as plataformas.",
                ],
            },
            Variants =
            [
                Base("progress.base", "ProgressBar e ActivityIndicator sem estilo."),
                new()
                {
                    Id = "progress.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Barra com arquivo, porcentagem e tempo restante; etapas; carregamento em linha com cancelar.",
                    Highlights =
                    [
                        "Diz o que está carregando, quanto já foi e quanto falta.",
                        "O botão de envio fica desabilitado durante o envio: nada de upload duplicado.",
                        "Etapas mostram onde o usuário está e o que vem depois.",
                        "Espera indeterminada sempre com uma saída (Cancelar).",
                    ],
                    Snippets = [Xaml("Uso", "progress.styled"), Xaml("Estilos (VaultTheme.xaml)", "styles.controls")],
                },
                new()
                {
                    Id = "progress.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "ProgressRing: anel desenhado com Graphics, animado até o valor, com rótulo central.",
                    Highlights =
                    [
                        "Pontas arredondadas e trilha no tom do tema, idênticas no Android e no Windows.",
                        "Transição de 600ms até o novo valor, sem passar do ponto.",
                        "O valor é anunciado para leitores de tela.",
                    ],
                    Snippets = [Xaml("Uso", "progress.custom"), CSharp("Desenho (ProgressRing.cs)", "control.progressring")],
                },
            ],
        },
        new()
        {
            Id = "skeleton",
            Name = "Skeleton",
            Icon = "Apps24",
            Category = ComponentCategory.Feedback,
            Summary = "Placeholders com a forma do conteúdo enquanto ele carrega.",
            Tags = ["skeleton", "shimmer", "placeholder", "carregando", "loading"],
            AddedOn = new DateOnly(2026, 11, 23),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem skeleton. Ele é montado com BoxView arredondados no mesmo layout do conteúdo final e um pulso lento de opacidade.",
                Properties =
                [
                    new("BoxView.CornerRadius", "Arredonda os blocos (círculos para avatar)."),
                    new("Animation + Commit(repeat)", "Pulso em loop; pare no Unloaded."),
                    new("SemanticProperties.Description", "Anuncia \"Carregando\" para leitores de tela."),
                ],
                UseCases = ["Listas e cards que carregam da rede em mais de ~300ms."],
                Limitations = ["Para esperas curtas (< 300ms), mostrar o skeleton causa mais piscada do que ajuda."],
            },
            Variants =
            [
                new()
                {
                    Id = "skeleton.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Lista e card em placeholder, com pulso de 2,8s e troca suave pelo conteúdo real.",
                    Highlights =
                    [
                        "A mesma geometria do conteúdo final: nada pula quando os dados chegam.",
                        "Pulso lento (2,8s) em vez de brilho correndo: sinaliza sem chamar atenção.",
                        "A animação para ao sair da tela; nenhum loop fica rodando escondido.",
                    ],
                    Snippets = [Xaml("Placeholders", "skeleton.custom")],
                },
            ],
        },
        new()
        {
            Id = "alert",
            Name = "Alert",
            Icon = "Info24",
            Category = ComponentCategory.Feedback,
            Summary = "Aviso em linha para informação, sucesso, atenção ou erro.",
            Tags = ["alerta", "aviso", "banner", "callout", "mensagem", "erro"],
            AddedOn = new DateOnly(2026, 11, 21),
            Docs = new ComponentDocs
            {
                Overview = "Não existe alerta em linha no MAUI (DisplayAlertAsync é um diálogo modal). O Callout junta ícone, título, texto, ação opcional e fechar, em quatro tons semânticos.",
                Properties =
                [
                    new("Kind", "Info, Success, Warning ou Danger: define ícone e tom."),
                    new("Title / Message", "Título curto e uma frase de explicação."),
                    new("ActionText + ActionClicked", "Ação opcional (\"Tentar de novo\")."),
                    new("IsDismissible", "Mostra o botão de fechar com alvo de 44px."),
                ],
                UseCases =
                [
                    "Problemas que o usuário precisa saber sem ser interrompido.",
                    "Confirmações persistentes no topo de um formulário.",
                ],
                Limitations = ["Para decisões que bloqueiam o fluxo, use um diálogo (Popup)."],
            },
            Variants =
            [
                new()
                {
                    Id = "alert.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Callout em quatro tons, com ação, fechar animado e erros que não podem ser dispensados.",
                    Highlights =
                    [
                        "A cor fica no fundo suave e no ícone; o texto segue com contraste alto.",
                        "Título diz o que houve; o texto, o que fazer.",
                        "Erros que exigem ação não têm botão de fechar.",
                        "Fechar some em 150ms e libera o espaço.",
                    ],
                    Snippets = [Xaml("Uso", "alert.styled"), CSharp("Controle (Callout.cs)", "control.callout")],
                },
            ],
        },
        new()
        {
            Id = "toast",
            Name = "Toast",
            Icon = "Chat24",
            Category = ComponentCategory.Feedback,
            Summary = "Confirmação breve que some sozinha, com ação opcional.",
            Tags = ["toast", "snackbar", "notificação", "desfazer", "feedback", "toolkit"],
            AddedOn = new DateOnly(2026, 11, 24),
            Docs = new ComponentDocs
            {
                Overview = "Toast confirma uma ação sem interromper. O do CommunityToolkit usa o aviso nativo; o InAppToast fica dentro da tela e aceita ação, como Desfazer.",
                Properties =
                [
                    new("ShowAsync(mensagem, tipo, ação)", "InAppToast: mostra, espera ~3s e esconde."),
                    new("Toast.Make(texto, duração, tamanho)", "CommunityToolkit: cria o aviso nativo."),
                    new("ToastDuration", "Short ou Long."),
                ],
                UseCases =
                [
                    "\"Salvo\", \"Copiado\", \"Enviado\".",
                    "Exclusão reversível com Desfazer, no lugar de um diálogo de confirmação.",
                ],
                Limitations =
                [
                    "No Windows o Toast do Toolkit vira notificação do sistema, fora do app.",
                    "Não use toast para erros que exigem ação: o aviso some antes de ser lido.",
                ],
            },
            Variants =
            [
                new()
                {
                    Id = "toast.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "InAppToast: entra em 200ms, some em 3s, substitui o anterior e oferece Desfazer.",
                    Highlights =
                    [
                        "Excluir é imediato e reversível: Desfazer no lugar de \"Tem certeza?\".",
                        "Um aviso novo substitui o anterior em vez de empilhar.",
                        "Anunciado para leitores de tela.",
                        "Superfície invertida (tinta) se destaca nos dois temas.",
                    ],
                    Snippets = [Xaml("Uso", "toast.custom"), CSharp("Controle (InAppToast.cs)", "control.toast")],
                },
                new()
                {
                    Id = "toast.toolkit",
                    Title = "Toolkit Toast",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "CommunityToolkit.Maui",
                    Summary = "Toast nativo de cada plataforma via CommunityToolkit, com tratamento de erro.",
                    Highlights =
                    [
                        "Uma linha de código e o visual nativo de cada sistema.",
                        "Falha tratada: se a plataforma não suportar, o app explica em vez de quebrar.",
                    ],
                    Snippets = [CSharp("Uso", "toast.toolkit")],
                },
            ],
        },
        new()
        {
            Id = "emptystate",
            Name = "Empty state",
            Icon = "DocumentSearch24",
            Category = ComponentCategory.Feedback,
            Summary = "O que mostrar quando não há nada para mostrar.",
            Tags = ["vazio", "sem resultado", "empty", "primeiro uso", "placeholder"],
            AddedOn = new DateOnly(2026, 11, 25),
            Docs = new ComponentDocs
            {
                Overview = "Um estado vazio bem feito diz o que aconteceu, por quê, e oferece uma única saída. No MAUI ele vai no EmptyView da CollectionView ou no lugar do conteúdo.",
                Properties =
                [
                    new("CollectionView.EmptyView", "Conteúdo exibido quando ItemsSource está vazio."),
                    new("Ícone em círculo neutro", "Dá forma à tela sem ilustração pesada."),
                    new("Uma ação", "Primária quando resolve o problema; secundária quando só sugere."),
                ],
                UseCases = ["Busca sem resultado, primeiro uso e listas vazias."],
                Limitations = ["Evite humor ou ilustração em erros: o usuário está tentando resolver algo."],
            },
            Variants =
            [
                new()
                {
                    Id = "emptystate.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Busca sem resultado, primeiro uso e versão compacta em linha.",
                    Highlights =
                    [
                        "O título repete o termo buscado: o usuário vê o que digitou.",
                        "Uma única ação, a que resolve (Limpar filtros).",
                        "Primeiro uso explica o benefício, não só que está vazio.",
                        "Versão em linha para listas que não podem ocupar a tela.",
                    ],
                    Snippets = [Xaml("Uso", "emptystate.styled")],
                },
            ],
        },
    ];
}
