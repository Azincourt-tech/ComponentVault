using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Display() =>
    [
        new()
        {
            Id = "badge",
            Name = "Badge",
            Icon = "Tag24",
            Category = ComponentCategory.Display,
            Summary = "Rótulo curto de origem, status ou contagem.",
            Tags = ["badge", "chip", "tag", "status", "origem", "contador"],
            AddedOn = new DateOnly(2026, 11, 25),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI não tem um controle de badge. A versão base mostra como montá-lo com Border e Label; a custom encapsula isso no OriginBadge (o mesmo usado pelo app) e cobre status e contadores.",
                Properties =
                [
                    new("Kind (OriginBadge)", "Nativo, Custom, Lib Gratuita, Syncfusion ou Novo: define texto e cor do ponto."),
                    new("IsCompact (OriginBadge)", "Versão menor para listas."),
                    new("Text (OriginBadge)", "Substitui o texto padrão (ex.: \"v2\")."),
                ],
                UseCases = ["Status de item, origem e contagem de não lidos."],
                Limitations = ["Texto longo quebra o formato: mantenha em uma ou duas palavras."],
            },
            Variants =
            [
                Base("badge.base", "Border + Label montados à mão, com cores fixas."),
                new()
                {
                    Id = "badge.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "OriginBadge neutro com ponto colorido, status em tons suaves e contadores sobre ícones.",
                    Highlights =
                    [
                        "Só o ponto tem cor: a interface continua neutra.",
                        "Status com fundo suave e texto escuro da mesma família, nunca branco sobre cor forte.",
                        "Contador cresce com o número (99+) e tem anel da cor da superfície.",
                        "Cada ícone com contador descreve o número para leitores de tela.",
                    ],
                    Snippets = [Xaml("Uso", "badge.custom"), CSharp("Controle (OriginBadge.cs)", "control.badge")],
                },
            ],
        },
        new()
        {
            Id = "avatar",
            Name = "Avatar",
            Icon = "PersonCircle24",
            Category = ComponentCategory.Display,
            Summary = "Representa uma pessoa com foto ou iniciais.",
            Tags = ["avatar", "foto", "perfil", "iniciais", "usuário", "toolkit"],
            AddedOn = new DateOnly(2026, 11, 19),
            Docs = new ComponentDocs
            {
                Overview = "Sem biblioteca, o avatar é um Border arredondado com Label. O AvatarView do CommunityToolkit resolve imagem com fallback para iniciais, borda e raio.",
                Properties =
                [
                    new("Text", "Iniciais exibidas quando não há imagem."),
                    new("ImageSource", "Foto; se falhar, as iniciais aparecem."),
                    new("CornerRadius", "Metade do tamanho para um círculo."),
                    new("BorderColor / BorderWidth", "Anel; da cor da superfície em grupos."),
                ],
                UseCases = ["Listas de pessoas, comentários, autoria e presença."],
                Limitations = ["O tamanho e o raio precisam ser definidos juntos; não há presets de tamanho."],
            },
            Variants =
            [
                Base("avatar.base", "Border circular com Label, cores fixas."),
                new()
                {
                    Id = "avatar.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Anel de stories no degradê da paleta e status desenhado na borda do avatar.",
                    Highlights =
                    [
                        "Anel novo em degradê, visto em cinza fino: dá para saber o que é novidade de relance.",
                        "Status com forma própria (traço no ocupado, lua no ausente), não só cor.",
                        "O ponto de status é recortado do avatar por um contorno na cor da superfície.",
                    ],
                    Snippets = [Xaml("Uso", "avatar.custom"), CSharp("Controle (StoryAvatar.cs)", "control.storyavatar")],
                },
                new()
                {
                    Id = "avatar.toolkit",
                    Title = "Toolkit AvatarView",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "CommunityToolkit.Maui",
                    Summary = "Tamanhos 32 a 72, grupo sobreposto com \"+3\" e linha de lista com presença.",
                    Highlights =
                    [
                        "Iniciais em fundo neutro: nada de cor aleatória por pessoa.",
                        "No grupo, um anel da cor da superfície separa um avatar do outro.",
                        "O grupo descreve quem está nele para leitores de tela.",
                        "Presença com ponto verde e texto (\"online\"), não só cor.",
                    ],
                    Snippets = [Xaml("Uso", "avatar.toolkit")],
                },
            ],
        },
        new()
        {
            Id = "icons",
            Name = "Ícones",
            Icon = "Sparkle24",
            Category = ComponentCategory.Display,
            Summary = "Ícones consistentes em qualquer tamanho e cor.",
            Tags = ["ícone", "icons", "fluent", "fonte", "glifo", "mauiicons"],
            AddedOn = new DateOnly(2026, 11, 20),
            Docs = new ComponentDocs
            {
                Overview = "O MAUI renderiza ícones por FontImageSource, mas não traz nenhum conjunto. O MauiIcons empacota o Fluent (e outros) com um controle e extensões XAML.",
                Properties =
                [
                    new("Icon", "Valor do enum, ex.: FluentIcons.Home24."),
                    new("IconSize / IconColor", "Tamanho em pontos e cor (aceita AppThemeResource)."),
                    new("IconSuffix", "Texto ao lado do ícone."),
                ],
                UseCases = ["Navegação, ações e estados em toda a interface."],
                Limitations = ["O conjunto Fluent desta versão só traz o estilo Regular (sem variantes preenchidas)."],
            },
            Variants =
            [
                Base("icons.base", "FontImageSource com glifos Unicode da fonte do sistema."),
                new()
                {
                    Id = "icons.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "Ícones desenhados que se transformam: menu em X, play em pausa e curtir com explosão.",
                    Highlights =
                    [
                        "A forma muda no lugar (320ms), em vez de trocar um glifo pelo outro.",
                        "Cada desenho interpola os próprios pontos com Microsoft.Maui.Graphics.",
                        "O coração pula e solta seis pontos só ao curtir; descurtir é discreto.",
                        "O leitor de tela ouve a ação do próximo toque (\"Pausar\", \"Fechar menu\").",
                    ],
                    Snippets = [Xaml("Uso", "icons.custom"), CSharp("Controle (AnimatedIcon.cs)", "control.animatedicon")],
                },
                new()
                {
                    Id = "icons.mauiicons",
                    Title = "MauiIcons Fluent",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "AathifMahir.Maui.MauiIcons.Fluent",
                    Summary = "Tamanhos, cores de estado, ícone com texto e uma galeria do conjunto Fluent.",
                    Highlights =
                    [
                        "Desenho idêntico em todas as plataformas, independente da fonte do sistema.",
                        "Cor pela tinta do tema; o destaque fica para o estado ativo.",
                        "Grade de tamanhos 16/20/24/32 para alinhar com o texto.",
                    ],
                    Snippets = [Xaml("Uso", "icons.mauiicons")],
                },
            ],
        },
    ];
}
