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
- **Navegação:** a Home é a única raiz do Shell. O Catálogo (`catalog`) e o Detalhe (`component`) são empilhados. As barras nativas do Shell ficam escondidas, e a navegação visível é a `NavDock` (`App/Controls/NavDock.cs`), uma barra flutuante em cada página: no topo no desktop, embaixo no celular, com voltar, seções e o seletor dos 3 temas. Página nova entra com `<controls:NavDock Grid.Row="{OnIdiom Phone=2, Default=0}" Section="..." />` num Grid `Auto,*,Auto`.
- **ViewModels** usam CommunityToolkit.Mvvm com campos `[ObservableProperty]`. A navegação passa por `INavigationService`, o tema por `IThemeService`/`IThemeApplier`: nada de `Microsoft.Maui.*` no Core.

## Design system

- **Três temas:** Claro, Escuro e Vidro.
  - As cores trocáveis ficam em `Lib/Resources/Theme/LightPalette.xaml`, `DarkPalette.xaml` e `GlassPalette.xaml`. As três têm exatamente as mesmas chaves, e o `ResourceKeyTests` garante isso.
  - Use sempre `{DynamicResource Chave}` para essas cores.
  - As cores fixas (Accent, Success, Danger, Origin*) ficam em `Colors.xaml`, com `StaticResource`.
  - Em código, use `ThemeColors.SetThemeColor` / `SetThemeBrush`.
- **Troca de tema:** o `VaultThemeManager.Apply(palette)` troca o dicionário e dispara `ThemeColors.PaletteChanged`.
- **Estilos sempre nomeados** (`Style="{StaticResource PrimaryButton}"`), nunca implícitos, para a variante Base continuar crua.
- **Tipografia:** Plus Jakarta Sans (`FontRegular`/`Medium`/`SemiBold`/`Bold`) e JetBrains Mono (`FontMono`). Os arquivos ficam em `App/Resources/Fonts` como `MauiFont`.
- **Movimento:** use `Motion.Entrance`, `Motion.Delay` e `Motion.Press` (propriedades anexadas em `Lib/Theming/Motion.cs`). O `AmbientBackground` é o fundo de luz do Vidro. Nos dois temas sólidos as manchas são transparentes.
- **Regras de gosto** (adaptadas da taste-skill, Leonxlnx/taste-skill):
  - interface 90–95% neutra, com uma só cor de destaque;
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
- **`SizeChanged` de um layout dispara antes de os filhos serem posicionados.** Para ler `X`/`Width` dos filhos, adie com `Dispatcher.Dispatch`. Para pílula ou sublinhado que desliza até o item selecionado, use `App/Controls/SlidingIndicator.cs`, que já trata isso.
- **Voltar do Shell duplicado:** nas páginas empilhadas, use `Shell.BackButtonBehavior IsVisible="False"`. O voltar é o da `NavDock`.
- **Shell:** resolva o `AppShell` em `CreateWindow`, não no construtor do `App`, porque os recursos do `App.xaml` ainda não existem lá. Use `Title=""` nas páginas para não duplicar o título na barra.
- **`CollectionView` com `MaximumWidthRequest` corta itens no Windows.** A margem é calculada no `SizeChanged` da página (veja `CatalogPage.xaml.cs`).
- **`CharacterSpacing` muito negativo cola as palavras** na Plus Jakarta. O limite seguro é -0.4.
- **Windows + UI Automation/Narrador derruba o app em páginas com `ScrollView`.** É bug do framework (ver README). Por isso, não automatize cliques via UIA.

## Verificação visual

Para capturar uma tela sem interagir, abra o exe com estas variáveis:

- `COMPONENTVAULT_START`: uma rota do Shell (`catalog`, `component?id=switch`, `catalog/component?id=switch`);
- `COMPONENTVAULT_THEME`: `Light`, `Dark` ou `Glass`.

Depois capture a janela (ex.: `PrintWindow`). As exceções não tratadas vão para `%LOCALAPPDATA%\ComponentVault\crash.log`.

## Próximos passos

Siga o [ROADMAP.md](ROADMAP.md), nesta ordem: validar o Android e os componentes de biblioteca, corrigir os bugs conhecidos, completar os níveis e depois os componentes novos e a direção de movimento.
