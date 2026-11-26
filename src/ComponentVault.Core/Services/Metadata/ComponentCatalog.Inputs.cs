using ComponentVault.Core.Models;

namespace ComponentVault.Core.Services.Metadata;

internal static partial class ComponentCatalog
{
    private static IEnumerable<ComponentItem> Inputs() =>
    [
        new()
        {
            Id = "entry",
            Name = "Entry",
            Icon = "TextField24",
            Category = ComponentCategory.Inputs,
            Summary = "Campo de texto de uma linha, com rótulo, dica e validação.",
            Tags = ["input", "campo", "texto", "formulário", "validação", "senha"],
            AddedOn = new DateOnly(2026, 11, 12),
            Docs = new ComponentDocs
            {
                Overview = "O Entry captura texto de uma linha. Sozinho ele não tem rótulo, dica nem estado de erro: a versão estilizada embrulha o Entry num controle com essas peças e altura mínima de 48px.",
                Properties =
                [
                    new("Text (TwoWay)", "Valor digitado."),
                    new("Placeholder", "Exemplo de formato. Nunca substitui o rótulo."),
                    new("Keyboard", "Teclado virtual: Email, Numeric, Telephone..."),
                    new("ReturnType / ReturnCommand", "Tecla de confirmação e a ação disparada."),
                ],
                UseCases = ["Formulários de cadastro e login.", "Busca em tempo real."],
                Limitations =
                [
                    "A borda nativa difere entre Android (sublinhado) e Windows (caixa); o estilizado remove e desenha a própria.",
                    "Não existe estado de erro nativo: precisa ser modelado na ViewModel.",
                ],
            },
            Variants =
            [
                Base("entry.base", "Entry puro, com placeholder e teclados diferentes."),
                new()
                {
                    Id = "entry.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "VaultTextField num formulário real: rótulo acima, validação no envio, limpar e busca.",
                    Highlights =
                    [
                        "Rótulo sempre visível acima; o placeholder só sugere o formato.",
                        "Valida no envio, não a cada tecla; depois do erro, revalida ao digitar para ele sumir assim que corrigido.",
                        "Cada erro aparece embaixo do próprio campo, e uma linha de status resume o resultado.",
                        "Foco com anel de 2px na cor de destaque e botão de limpar que só aparece com texto.",
                    ],
                    Snippets = [Xaml("Uso", "entry.styled"), Xaml("Controle (VaultTextField.xaml)", "control.textfield")],
                },
                new()
                {
                    Id = "entry.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "TextField Material com rótulo flutuante e botão de limpar.",
                    Highlights =
                    [
                        "Rótulo flutuante: economiza altura em formulários longos.",
                        "AllowClear e AccentColor resolvidos por propriedade, sem template.",
                    ],
                    Snippets = [Xaml("Uso", "entry.uranium")],
                },
            ],
        },
        new()
        {
            Id = "editor",
            Name = "Editor",
            Icon = "TextEditStyle24",
            Category = ComponentCategory.Inputs,
            Summary = "Texto de várias linhas que cresce com o conteúdo.",
            Tags = ["multilinha", "textarea", "comentário", "contador", "limite"],
            AddedOn = new DateOnly(2026, 11, 21),
            Docs = new ComponentDocs
            {
                Overview = "O Editor é o campo de várias linhas do MAUI. Com AutoSize=\"TextChanges\" ele cresce conforme o texto; o estilizado limita essa altura e mostra quanto falta para o limite.",
                Properties =
                [
                    new("AutoSize", "TextChanges faz o Editor crescer com o conteúdo."),
                    new("MaxLength", "Limite de caracteres aceito."),
                    new("MinimumHeightRequest / MaximumHeightRequest", "Faixa de altura antes de começar a rolar."),
                ],
                UseCases = ["Descrições, comentários e observações."],
                Limitations = ["Sem altura definida, o Editor colapsa para uma linha em alguns layouts."],
            },
            Variants =
            [
                Base("editor.base", "Editor com altura fixa e com AutoSize."),
                new()
                {
                    Id = "editor.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Editor na moldura do design system, com crescimento limitado e contador de caracteres.",
                    Highlights =
                    [
                        "Começa com 3 linhas e cresce até ~8; depois rola, para o formulário não pular.",
                        "Contador fica neutro, vira aviso a 90% e erro no limite.",
                        "O leitor de tela anuncia quantos caracteres restam, não a fração.",
                    ],
                    Snippets = [Xaml("Uso", "editor.styled")],
                },
            ],
        },
        new()
        {
            Id = "searchbar",
            Name = "SearchBar",
            Icon = "Search24",
            Category = ComponentCategory.Inputs,
            Summary = "Busca com sugestões filtradas enquanto se digita.",
            Tags = ["busca", "pesquisa", "filtro", "sugestões", "autocomplete"],
            AddedOn = new DateOnly(2026, 11, 22),
            Docs = new ComponentDocs
            {
                Overview = "O SearchBar nativo traz ícone e limpar, mas a lista de sugestões fica por conta do app. O estilizado usa o VaultTextField e filtra uma lista enquanto se digita.",
                Properties =
                [
                    new("Text / TextChanged", "Termo atual; filtra a cada alteração."),
                    new("SearchButtonPressed / SearchCommand", "Confirmação explícita (tecla Enter)."),
                    new("Placeholder", "Diga o que dá para buscar."),
                ],
                UseCases = ["Encontrar um item numa lista conhecida.", "Atalho para telas e comandos."],
                Limitations = ["Em listas grandes, filtre na ViewModel com debounce para não travar a digitação."],
            },
            Variants =
            [
                Base("searchbar.base", "SearchBar nativo, buscando ao pressionar Enter."),
                new()
                {
                    Id = "searchbar.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Busca com recentes, resultados filtrados e trecho encontrado em destaque.",
                    Highlights =
                    [
                        "Sem texto, mostra as buscas recentes em vez de uma lista vazia.",
                        "O trecho que casou aparece em negrito, para conferir o resultado num relance.",
                        "Ignora maiúsculas e acentos ao comparar.",
                        "Sem resultado, repete o termo buscado e sugere o que tentar.",
                    ],
                    Snippets = [Xaml("Uso", "searchbar.styled")],
                },
            ],
        },
        new()
        {
            Id = "stepper",
            Name = "Stepper",
            Icon = "NumberSymbol24",
            Category = ComponentCategory.Inputs,
            Summary = "Ajusta um número em passos fixos, como quantidade.",
            Tags = ["quantidade", "número", "contador", "carrinho", "incremento"],
            AddedOn = new DateOnly(2026, 11, 15),
            Docs = new ComponentDocs
            {
                Overview = "O Stepper aumenta e diminui um valor em passos. O nativo não mostra o número; o estilizado junta − valor + numa só moldura, com alvos de 44px.",
                Properties =
                [
                    new("Minimum / Maximum", "Faixa permitida."),
                    new("Increment", "Tamanho do passo."),
                    new("Value (TwoWay)", "Valor atual."),
                ],
                UseCases = ["Quantidade num carrinho.", "Número de pessoas, noites, cópias."],
                Limitations = ["Para faixas grandes (ex.: 0 a 1000), um Entry numérico ou Slider é mais rápido."],
            },
            Variants =
            [
                Base("stepper.base", "Stepper nativo com o valor num Label ao lado."),
                new()
                {
                    Id = "stepper.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Controle de quantidade − valor + ligado ao subtotal de uma linha de carrinho.",
                    Highlights =
                    [
                        "O valor fica entre os botões, sempre visível.",
                        "No mínimo e no máximo o botão é desabilitado, em vez de aceitar um toque que não faz nada.",
                        "O limite está escrito no texto da linha, antes de o usuário esbarrar nele.",
                        "Botões de 44×44 com descrição para leitor de tela.",
                    ],
                    Snippets = [Xaml("Uso", "stepper.styled")],
                },
                new()
                {
                    Id = "stepper.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfNumericUpDown: o número editável entre os botões, com formato, limites e passo.",
                    Highlights =
                    [
                        "Campo e botões num controle só: dá para digitar o valor em vez de tocar várias vezes.",
                        "CustomFormat formata sem código: quantidade inteira, porcentagem, moeda.",
                        "Limites e passo (SmallChange) declarados no XAML; no limite o botão desabilita.",
                    ],
                    Snippets = [Xaml("Uso", "stepper.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "otp",
            Name = "Código OTP",
            Icon = "Password24",
            Category = ComponentCategory.Inputs,
            Summary = "Código de verificação em caixas, com colar e avanço automático.",
            Tags = ["otp", "pin", "código", "verificação", "2fa", "senha"],
            AddedOn = new DateOnly(2026, 11, 25),
            Docs = new ComponentDocs
            {
                Overview = "Não existe controle de OTP no MAUI. O OtpInput usa um único Entry invisível para a digitação e caixas só para desenhar os dígitos, então teclado, colar e leitor de tela continuam funcionando.",
                Properties =
                [
                    new("Length", "Quantidade de dígitos (padrão 6)."),
                    new("Code (TwoWay)", "Código digitado até agora."),
                    new("HasError", "Pinta todas as caixas de erro."),
                    new("Completed (evento)", "Disparado quando todos os dígitos foram preenchidos."),
                ],
                UseCases = ["Verificação em duas etapas.", "Confirmação de e-mail ou telefone."],
                Limitations = ["O preenchimento automático do SMS (Android) exige integração extra com a plataforma."],
            },
            Variants =
            [
                new()
                {
                    Id = "otp.custom",
                    Title = "Custom",
                    Tier = VariantTier.Custom,
                    Origin = ComponentOrigin.Custom,
                    Summary = "OtpInput: seis caixas, foco na próxima posição, colar o código inteiro e reenvio com espera.",
                    Highlights =
                    [
                        "Colar \"123 456\" ou \"123-456\" funciona: só os dígitos são aproveitados.",
                        "A caixa da próxima posição ganha o anel de foco; não há caixa para tocar uma a uma.",
                        "Confere sozinho ao completar, sem botão de enviar.",
                        "Reenviar fica bloqueado por 30s com contagem visível.",
                    ],
                    Snippets = [Xaml("Uso", "otp.custom"), CSharp("Controle (OtpInput.cs)", "control.otp")],
                },
                new()
                {
                    Id = "otp.syncfusion",
                    Title = "Syncfusion Toolkit",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.Syncfusion,
                    PackageId = "Syncfusion.Maui.Toolkit",
                    Summary = "SfOtpInput preenchido para código de SMS e sublinhado, mascarado, para PIN.",
                    Highlights =
                    [
                        "Avanço entre caixas, colar o código inteiro e apagar para trás já vêm prontos.",
                        "Três estilos (Outlined, Filled, Underlined) e estados de sucesso, aviso e erro.",
                        "Type=Password mascara os dígitos; Separator divide o código em grupos.",
                    ],
                    Snippets = [Xaml("Uso", "otp.syncfusion")],
                },
            ],
        },
        new()
        {
            Id = "picker",
            Name = "Picker",
            Icon = "Options24",
            Category = ComponentCategory.Inputs,
            Summary = "Escolha de uma opção numa lista.",
            Tags = ["select", "dropdown", "combobox", "lista", "opções", "menu"],
            AddedOn = new DateOnly(2026, 11, 16),
            Docs = new ComponentDocs
            {
                Overview = "O Picker abre a lista nativa de cada plataforma. O estilizado põe o Picker sem borda dentro da moldura do design system, com rótulo fixo acima e chevron indicando que é um menu.",
                Properties =
                [
                    new("ItemsSource / ItemDisplayBinding", "Itens e como exibi-los."),
                    new("SelectedIndex / SelectedItem (TwoWay)", "Escolha atual."),
                    new("Title", "Texto exibido antes da escolha."),
                ],
                UseCases = ["Escolher uma entre 5 e ~15 opções."],
                Limitations =
                [
                    "Até 4 opções, chips ou RadioButton mostram tudo sem abrir nada.",
                    "Não tem busca: listas longas pedem uma tela de seleção própria.",
                ],
            },
            Variants =
            [
                Base("picker.base", "Picker nativo com Title."),
                new()
                {
                    Id = "picker.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "BorderlessPicker na moldura de campo, com chevron, valor padrão e estado desabilitado.",
                    Highlights =
                    [
                        "O rótulo continua acima depois da escolha; o Title sozinho some.",
                        "Chevron deixa claro que é um menu, não um campo de texto.",
                        "A dica abaixo muda com a escolha e explica o efeito dela.",
                        "Desabilitado diz o motivo em vez de só esmaecer.",
                    ],
                    Snippets = [Xaml("Uso", "picker.styled")],
                },
                new()
                {
                    Id = "picker.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "PickerField Material com título flutuante e limpar.",
                    Highlights =
                    [
                        "Mesmo visual do TextField da biblioteca: formulário consistente sem esforço.",
                        "AllowClear permite voltar ao estado sem escolha.",
                    ],
                    Snippets = [Xaml("Uso", "picker.uranium")],
                },
            ],
        },
        new()
        {
            Id = "datepicker",
            Name = "DatePicker",
            Icon = "CalendarLtr24",
            Category = ComponentCategory.Inputs,
            Summary = "Seleção de data e horário.",
            Tags = ["data", "hora", "calendário", "agenda", "lembrete", "timepicker"],
            AddedOn = new DateOnly(2026, 11, 19),
            Docs = new ComponentDocs
            {
                Overview = "DatePicker e TimePicker abrem os seletores nativos. No .NET 10, Date e Time passaram a aceitar nulo; o estilizado combina os dois num formulário com ícones e um resumo em linguagem natural.",
                Properties =
                [
                    new("Date / Time", "Valor escolhido (anuláveis no .NET 10)."),
                    new("MinimumDate / MaximumDate", "Faixa permitida."),
                    new("Format", "Formato exibido, ex.: \"dd MMM yyyy\"."),
                ],
                UseCases = ["Agendamentos, lembretes e prazos."],
                Limitations = ["O visual do seletor aberto é da plataforma e não pode ser estilizado pelo MAUI."],
            },
            Variants =
            [
                Base("datepicker.base", "DatePicker e TimePicker nativos."),
                new()
                {
                    Id = "datepicker.styled",
                    Title = "Estilizado",
                    Tier = VariantTier.Styled,
                    Origin = ComponentOrigin.Native,
                    Summary = "Data e horário lado a lado com ícones, limite mínimo e resumo do agendamento.",
                    Highlights =
                    [
                        "Mês por extenso (\"28 nov 2026\") evita a confusão dia/mês.",
                        "Datas passadas ficam bloqueadas; o padrão já é amanhã às 9h.",
                        "Um resumo em frase (\"amanhã às 09:00\") confirma a escolha.",
                    ],
                    Snippets = [Xaml("Uso", "datepicker.styled")],
                },
                new()
                {
                    Id = "datepicker.uranium",
                    Title = "UraniumUI",
                    Tier = VariantTier.Library,
                    Origin = ComponentOrigin.FreeLibrary,
                    PackageId = "UraniumUI.Material",
                    Summary = "DatePickerField e TimePickerField Material, com estado vazio e limpar.",
                    Highlights =
                    [
                        "Vazio é um estado válido: bom para datas opcionais.",
                        "Título flutuante igual aos demais campos da biblioteca.",
                    ],
                    Snippets = [Xaml("Uso", "datepicker.uranium")],
                },
            ],
        },
    ];
}
