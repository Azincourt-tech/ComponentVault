# AGENTS.md

Contexto para quem (pessoa ou agente de IA) for continuar este trabalho em outra máquina. Leia isto, o [README.md](README.md) e o [ROADMAP.md](ROADMAP.md) antes de mexer no código.

## O projeto

O ComponentVault é uma vitrine de componentes .NET MAUI (.NET 10) para Android e Windows. Cada componente aparece em até quatro níveis:

1. **Base MAUI**: o controle cru.
2. **Estilizado**: o mesmo controle levado ao limite com os tokens.
3. **Custom**: desenhado à mão.
4. **Biblioteca**: feito com libs gratuitas do ecossistema.

O objetivo do dono é um app que chame atenção e dê vontade de explorar. A direção pedida é "ousada", com animações e transições, longe de interface genérica com cara de IA.

Os documentos de origem (PRD v2 e o design system inicial em HTML) ficaram fora do repositório, na máquina original. O README e o ROADMAP já trazem o que importa deles.

## Ambiente

- SDK .NET 10 (10.0.302 ou mais novo, ver `global.json`) com o workload MAUI. No Windows: Visual Studio com "Desenvolvimento .NET Multiplataforma" e o Android SDK instalado pelo instalador.
- Pacotes centralizados em `Directory.Packages.props` (Central Package Management). Para atualizar uma versão, mude lá, não no `.csproj`.
- Os testes usam o Microsoft.Testing.Platform (configurado no `global.json`):

```powershell
dotnet test --project tests/ComponentVault.Tests
dotnet build src/ComponentVault.App -c Release -f net10.0-windows10.0.19041.0
dotnet build src/ComponentVault.App -f net10.0-windows10.0.19041.0 -t:Run
```

- O build Android ocupa vários GB. Na máquina original não coube (disco cheio), então **o Android nunca rodou**. É a primeira coisa a validar numa máquina com espaço.
- Antes de recompilar o app Windows, feche-o (`Get-Process ComponentVault.App | Stop-Process`). Com ele aberto, o build falha por arquivo em uso.

## Git

- Trabalhe na branch `develop`. A `main` só tem o commit inicial.
- **Identidade dos commits:** autor "Willian Almeida", com o mesmo e-mail pessoal dos commits existentes (`git log -1 --format='%an <%ae>'`). Configure com `git config --local user.name` e `user.email` antes do primeiro commit. Nunca use o e-mail corporativo.
- Mensagens de commit em pt-BR. **Sem `Co-Authored-By` e sem citar assistente de IA.**
- Só commite quando o dono pedir.

## Arquitetura

```
src/ComponentVault.Core   net10.0 puro: modelos, serviços, ViewModels. Não referencia MAUI (ADR-006).
src/ComponentVault.Lib    design system (tokens, paletas, estilos), controles e previews.
src/ComponentVault.App    Shell, DI, páginas, serviços que dependem de MAUI.
tests/ComponentVault.Tests xUnit v3.
```

- **Catálogo:** `Core/Services/ComponentCatalog*.cs` (um arquivo parcial por categoria). Cada variante aponta para um preview (`[ShowcasePreview("id")]` em `Lib/Showcase/<Pasta>/Previews.cs`) e para snippets.
- **Snippets:** são extraídos do próprio código. A Lib embute os `.xaml`/`.cs` como recurso, e as regiões ficam entre `<!-- snippet:id -->` / `<!-- /snippet:id -->` ou `// snippet:id` / `// /snippet:id`. O `CatalogConsistencyTests` falha se um snippet ou preview citado não existir.
- **Adicionar um componente:**
  1. Crie a pasta em `Lib/Showcase/<NomeNoPlural>`. Use o plural para não colidir com o nome do tipo (ex.: `Buttons`, não `Button`).
  2. Crie as Views com os marcadores de snippet e o `Previews.cs` com os atributos.
  3. Adicione a entrada no `ComponentCatalog.<Categoria>.cs`.
  4. Rode os testes.
- **Navegação:** a Home é a única raiz do Shell. O Catálogo (`catalog`) e o Detalhe (`component`) são empilhados. As barras nativas do Shell ficam escondidas, e a navegação visível é a `NavDock` (`App/Controls/NavDock.cs`). Ela tem duas formas, conforme a largura:
  - **Média e expandida:** cápsula no topo, com voltar, seções com nome e os 3 temas.
  - **Compacta:** barra inferior em largura total, com Início, Catálogo e Tema. Tocar em Tema troca a barra pelo seletor de temas. No Detalhe, o voltar fica no topo da página.

  Página nova entra com `<controls:NavDock Grid.Row="{adaptive:OnWidth Compact=2, Default=0}" Section="..." />` num Grid `Auto,*,Auto`.
- **Responsivo pela largura, não pelo aparelho:**
  - `Lib/Theming/Adaptive.cs` define as faixas: compacta (até 600 px), média (até 1000 px) e expandida. O App atualiza a faixa quando a janela muda de tamanho.
  - No XAML, use `{adaptive:OnWidth Compact=..., Default=...}`, que muda ao vivo. Não use `OnIdiom`.
  - No código, use `Adaptive.Current.IsCompact` e o evento `ClassChanged`. Use `DeviceInfo.Idiom` só para o que depende de toque, como o alvo de 44 px.
  - Para testar, abra a janela num tamanho com `COMPONENTVAULT_SIZE=390x844` (ou `768x1024`, `1280x800`).
- **ViewModels** usam CommunityToolkit.Mvvm com campos `[ObservableProperty]`. A navegação passa por `INavigationService`, o tema por `IThemeService`/`IThemeApplier`: nada de `Microsoft.Maui.*` no Core.

## Design system

- **Três temas:** Claro "Pérola", Escuro "Titânio" e Vidro.
  - **Claro e Escuro são metalizados.** Os cards usam um degradê de cima para baixo (`CardFill`) e uma aresta com brilho em cima e sombra embaixo (`CardStroke`): bronze claro no Claro, metal noturno no Escuro. A luz ambiente é de pôr do sol: suave no Claro (açafrão, ocre e âmbar) e de brasa no Escuro (Oxblood, Cayenne e açafrão).
  - **Vidro:** superfícies translúcidas sobre luzes Red Ochre, Deep Saffron e Oxblood. As chaves `AmbientBlue/Teal/Rose` mantêm o nome antigo, mas as cores são as da paleta.
  - No Escuro, `SurfaceAlt` precisa ser mais claro que o topo do `CardFill`, senão selos e chips somem dentro dos cards.
  - As cores trocáveis ficam em `Lib/Resources/Theme/LightPalette.xaml`, `DarkPalette.xaml` e `GlassPalette.xaml`. As três têm exatamente as mesmas chaves, e o `ResourceKeyTests` garante isso.
  - Use sempre `{DynamicResource Chave}` para essas cores.
- **Campos "preenchidos":** o estilo `FieldFrame` usa `FieldFill` (tom próprio) e `FieldStroke` (contorno quase invisível), com raio 14. No foco e na validação, chame `FieldFrames.Apply(frame, focused, stateKey)` (`Lib/Theming/FieldFrames.cs`): o campo clareia e ganha o anel de 2px. Não pinte a borda à mão. O `VaultTextField` tem ícone à esquerda (acende no foco) e mostrar/ocultar senha.
  - As cores fixas ficam em `Colors.xaml`, com `StaticResource`. Elas incluem Accent, Success, Danger, Origin* e Tier*.
- **Paleta da marca: "Fiery Red Sunset"** (`#03071E` `#370617` `#6A040F` `#9D0208` `#D00000` `#DC2F02` `#E85D04` `#F48C06` `#FAA307` `#FFBA08`), listada no topo do `Colors.xaml`.
  - O destaque é o Cayenne `#E85D04`, com hover em Red Ochre `#DC2F02`. Ele fica de propósito longe do azul nativo do Windows e do Android, para o nível Estilizado se distinguir do Base. Aparece no botão principal, na pílula da navegação, no filtro e na aba ativos, nos switches, sliders e progresso, e no foco.
  - O fundo do Escuro e do Vidro é o Ink Black `#03071E`. Texto em destaque: Oxblood no Claro, Orange no Escuro e Amber Flame no Vidro.
  - Ícone e splash usam o degradê Amber Flame → Cayenne → Oxblood.
  - Atenção: branco sobre `#E85D04` dá cerca de 3,3:1, abaixo do AA para texto pequeno.
- **Cor por nível,** em todo ponto e selo de nível. Use `OriginBadge Kind="Base|Styled|Custom|Library"` ou os conversores `TierToBrush` e `TierToBadgeKind`:
  - Base: aço `#8B93A1`;
  - Estilizado: Cayenne `#E85D04`;
  - Custom: verde-água `#14A394` (fica frio de propósito, para não se confundir com os níveis quentes);
  - Biblioteca: Amber Flame `#FFBA08`.
  - Em código, use `ThemeColors.SetThemeColor` / `SetThemeBrush`.
- **Troca de tema:** o `VaultThemeManager.Apply(palette)` troca o dicionário e dispara `ThemeColors.PaletteChanged`.
- **Estilos sempre nomeados** (`Style="{StaticResource PrimaryButton}"`), nunca implícitos, para a variante Base continuar crua.
- **Tipografia:** Plus Jakarta Sans (`FontRegular`/`Medium`/`SemiBold`/`Bold`) e JetBrains Mono (`FontMono`). Os arquivos ficam em `App/Resources/Fonts` como `MauiFont`.
- **Transição de página (Hero):** a linha tocável recebe `controls:Hero.Source="True"`, e o ícone e o título, na linha e no destino, recebem `controls:Hero.Key="thumb"` / `"title"`. A página de destino chama `Hero.PlayInto(...)` no `Loaded`. Na volta, o destino chama `Hero.CaptureReturn(...)` no `OnDisappearing`, e a origem chama `Hero.PlayReturn(Root)` no `OnAppearing`, passando o Grid raiz, onde as cópias voam sem o recorte da lista. As posições vêm da janela nativa (`TransformToVisual` no Windows, `GetLocationInWindow` no Android).
- **Cascata em listas:** `controls:Cascade.Item="True"` na raiz do template do item, e `Cascade.Begin()` na página quando a lista deve entrar em sequência (abrir, trocar filtro). Itens que surgem fora dessa janela de 700ms entram parados.
- **Movimento:** use `Motion.Entrance`, `Motion.Delay` e `Motion.Press` (propriedades anexadas em `Lib/Theming/Motion.cs`). O `AmbientBackground` é o fundo de galáxia dos três temas:
  - quatro manchas de luz fluem como líquido, em curvas de Lissajous com frequências inteiras num ciclo de 42s, mais escala e giro;
  - um campo de estrelas pisca e gira devagar, só no Escuro e no Vidro;
  - tudo se move só por `Translation`, `Scale` e `Rotation`, sem disparar layout;
  - o movimento é amplo no Vidro e contido no Claro e no Escuro.
- **Regras de gosto** (adaptadas da taste-skill, Leonxlnx/taste-skill):
  - interface majoritariamente neutra, com uma só cor de destaque (o Cayenne da paleta), usada com intenção nos pontos de ação e estado; o dono pediu mais presença de cor e o acabamento metálico nos temas Claro e Escuro;
  - tinta quase preta;
  - linhas finas no lugar de sombras;
  - corpo 16px;
  - rótulos de 13px, peso 500, em sentence case (nada de maiúsculas espaçadas);
  - sem violeta;
  - um botão primário por tela;
  - movimento curto (press de 90ms, escala 0,985).
- **Evitar** o padrão "pill + título enorme + CTA + cards de estatística", que o dono achou com cara de IA.
- **Referências visuais:** Mobbin, ScreensDesign, Appshots, Loadmore, o estilo de vidro da Apple e apps populares de Android e iOS. **Não usar o Grial UI Kit** (é pago e foi descartado).
- **Libs:** só gratuitas. Já estão no projeto UraniumUI, CommunityToolkit.Maui, MauiIcons (Fluent), LiveCharts, Sharpnado.Tabs e SkiaSharp.

## Armadilhas já encontradas

- **No MAUI, um `Brush` só pode ter um pai.** Pincel de paleta compartilhado entre várias views some na segunda tela. Clone o pincel por instância (veja `AmbientBackground.Clone`).
- **`BoxView` ignora `Background` em degradê.** Use `Grid` ou `Border`.
- **Na Lib, o XamlC em Release não resolve os namespaces `http://` das libs.** Use `clr-namespace:...;assembly=...`. No Sharpnado, o assembly se chama `Maui.Tabs`.
- **Atualização de tema a partir de thread de fundo:** use `Application.Current.Dispatcher.Dispatch`. O `MainThread.BeginInvokeOnMainThread` falha na abertura do Windows com "Unable to find main thread".
- **Abas do Shell escondidas (`TabBarIsVisible=False`) não trocam de conteúdo no Windows:** o Shell muda de rota, mas a tela fica na aba anterior. Por isso não há `TabBar`, só páginas empilhadas.
- **`SizeChanged` de um layout dispara antes de os filhos serem posicionados.** Para ler `X`/`Width` dos filhos, adie com `Dispatcher.Dispatch`. Para pílula ou sublinhado que desliza até o item selecionado, use `Lib/Controls/SlidingIndicator.cs`, que já trata isso (usado na barra, nas abas, no seletor de nível e nos chips Custom).
- **Numa rota com várias páginas (`catalog/component`), o Shell insere as de baixo na pilha só depois que a de cima carrega.** O `CurrentState` também pode estar atrasado no `Loaded`. Confira de novo no `Shell.Navigated` (veja `NavDock.UpdateSectionFromStack`).
- **`FlexLayout` não conta `Margin` dos filhos ao medir** e corta os itens embaixo. Dê o espaço com um `Grid Padding="0,0,8,8"` em volta de cada item. Percentual em `FlexLayout.Basis` não funciona no Windows: calcule a largura no code-behind (veja a aba Lado a lado).
- **Larguras responsivas, nunca fixas.**
  - **`MaximumWidthRequest` engana:** com `HorizontalOptions="Fill"` ele centraliza o elemento; com `"Start"` o elemento encolhe até o conteúdo.
  - **`WidthRequest` fixo estoura** em telas e cards estreitos. O mesmo preview aparece no card da aba Lado a lado, que é mais estreito.
  - **Previews ocupam a largura toda do card**, como os Base. Não limite a largura do bloco raiz nem dos blocos de dentro: um bloco estreito deixa um vazio só à direita, e centralizado ele fica flutuando no meio. O dono achou as duas formas estranhas.
  - **Use `theming:Fit.MaxWidth="N"`** (`Lib/Theming/Fit.cs`) só para o que é pequeno por natureza (campo de data, campo de tags, tile). O elemento ocupa o espaço até N, encolhe quando falta espaço e fica à esquerda.
  - **Itens soltos estreitos** (um tile, um card pequeno) viram grade com colunas `*` (`OnWidth`), em vez de largura fixa.
  - **Para itens lado a lado**, use um Grid com colunas `*` e `OnIdiom` (uma coluna no celular), e não larguras fixas.
- **No Release, o XamlC descarta todo recurso embutido que termina em `.xaml`.** Os fontes dos snippets entram por um alvo no `ComponentVault.Lib.csproj`, com o sufixo `.src`, que o `EmbeddedSnippetSource` tira. Os testes leem do disco e não pegam esse caso: confira a aba Código num build Release.
- **Controles nativos do Windows dentro dos campos do design system:**
  - O `Strip` (em `VaultHostingExtensions`) zera o espaço interno e deixa transparente.
  - O `TimePicker` precisa de cerca de 242 px de largura; espremido, corta os dígitos.
  - O `CalendarDatePicker` lança `COMException` com algumas chaves de cor, por isso cada chave fica num try/catch, e o ícone nativo some pelo nome no template.
  - No `Picker`, o `Title` vira cabeçalho no Windows e é trocado pelo texto de exemplo.
- **O estilo `Body` tem `LineHeight` 1,45:** numa linha única ao lado de um valor ou ícone, ele desce alguns pixels. Use `LineHeight="1"` e `VerticalOptions="Center"`.
- **Voltar do Shell duplicado:** nas páginas empilhadas, use `Shell.BackButtonBehavior IsVisible="False"`. O voltar é o da `NavDock`.
- **Shell:** resolva o `AppShell` em `CreateWindow`, não no construtor do `App`, porque os recursos do `App.xaml` ainda não existem lá. Use `Title=""` nas páginas para não duplicar o título na barra.
- **`CollectionView` com `MaximumWidthRequest` corta itens no Windows.** A margem é calculada no `SizeChanged` da página (veja `CatalogPage.xaml.cs`).
- **`CharacterSpacing` muito negativo cola as palavras** na Plus Jakarta. O limite seguro é -0.4.
- **Windows + UI Automation/Narrador derruba o app em páginas com `ScrollView`.** É bug do framework (ver README). Por isso, não automatize cliques via UIA.
- **Syncfusion Toolkit** (`Syncfusion.Maui.Toolkit`, MIT, registrado com `ConfigureSyncfusionToolkit()`):
  - No XAML da Lib, use `clr-namespace:Syncfusion.Maui.Toolkit.<Área>;assembly=Syncfusion.Maui.Toolkit`.
  - Várias propriedades documentadas são internas (`HoveredBackground`, `FocusedStroke` e `SeparatorColor` do OTP) ou fogem da convenção (`SelectedSegmentTextColor`). Algumas o XamlC barra; outras só quebram ao abrir a tela ("Attempt to access field ...Property failed"). Antes de usar uma propriedade, confira por reflexão se o setter é público.
  - Cores de foco, seleção e hover vêm das chaves globais `Primary*` (`PrimaryBackground`, `PrimaryBorderFocused`...), sobrescritas no `Colors.xaml` com o Cayenne. Sem isso, a lib sai no roxo do Material 3.
  - No `SfChip`, `IsSelected` não é definível no XAML: selecione pelo grupo (`SelectedItem`) no código.
  - O `SfSegmentedControl` usa largura fixa por segmento e se centraliza. Para ocupar a linha, use `VisibleSegmentsCount` igual ao número de itens; mudar o `SegmentWidth` pelo código não pega.
  - Ele traz um WebView2 que pede o `WinRT.Runtime` 2.3.1. Por isso o `Directory.Build.props` fixa o `WindowsSdkPackageVersion` em 10.0.19041.86; sem isso, o build avisa (MSB3277).
  - Origem no catálogo: `ComponentOrigin.Syncfusion`, nível `Library`, `PackageId = "Syncfusion.Maui.Toolkit"`.

## Verificação visual

Para capturar uma tela sem interagir, abra o exe com estas variáveis:

- `COMPONENTVAULT_START`: uma rota do Shell (`catalog`, `component?id=switch`, `catalog/component?id=switch`; `&variant=switch.styled` escolhe o nível e `&present=true` abre no modo Apresentar);
- `COMPONENTVAULT_THEME`: `Light`, `Dark` ou `Glass`.

Depois capture a janela (ex.: `PrintWindow`). As exceções não tratadas vão para `%LOCALAPPDATA%\ComponentVault\crash.log`. `&tab=compare|docs|code` abre o Detalhe direto numa aba.

Para uma revisão geral, capture todas as variantes no modo Apresentar e as páginas, uma por vez. Apague as capturas antigas antes: se o app cair numa tela, a imagem antiga fica no lugar e parece que nada mudou. E se uma tela derrubar o app, o processo travado contamina as capturas seguintes.

## Próximos passos

Siga o [ROADMAP.md](ROADMAP.md), nesta ordem: validar o Android e os componentes de biblioteca, corrigir os bugs conhecidos, completar os níveis e depois os componentes novos e a direção de movimento.
