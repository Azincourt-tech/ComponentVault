# ComponentVault

Vitrine viva de componentes **.NET MAUI** (.NET 10). Cada componente aparece em níveis:

| Nível | O que mostra |
|---|---|
| **Base MAUI** | O controle cru, sem nenhum estilo — o que a plataforma entrega. |
| **Estilizado** | O mesmo controle levado ao limite com os tokens do design system. |
| **Custom** | Desenhado à mão (Microsoft.Maui.Graphics, SkiaSharp, handlers). |
| **Biblioteca** | Construído com libs do ecossistema (UraniumUI, CommunityToolkit...). |

Cada componente tem **Preview** interativo, **Lado a lado** (todos os níveis juntos), **Docs** (propriedades, quando usar, limitações) e **Código** com realce de sintaxe e botão de copiar.

## Aparência

Três temas trocáveis ao vivo, escolhidos na Home ou pelo botão **Tema** (que alterna Claro → Escuro → Vidro):

- **Claro** e **Escuro**: superfícies sólidas, tipografia Plus Jakarta Sans e JetBrains Mono para código.
- **Vidro**: superfícies translúcidas sobre um fundo ambiente com manchas de luz que derivam devagar (`AmbientBackground`), e bordas em degradê.

As cores vêm de três paletas (`LightPalette`, `DarkPalette`, `GlassPalette`) com as mesmas chaves, consumidas por `{DynamicResource}`. O `VaultThemeManager` troca o dicionário ativo. Um teste garante que as três paletas têm as mesmas chaves e que toda chave usada no XAML existe.

O movimento fica em propriedades anexadas (`Motion.Entrance`, `Motion.Delay`, `Motion.Press`), e o preview faz uma transição a cada troca de nível.

## Estrutura

```
src/
  ComponentVault.Core   net10.0 puro: modelos, serviços, ViewModels (sem MAUI)
  ComponentVault.Lib    design system (tokens + estilos), controles e Views de preview
  ComponentVault.App    app MAUI: Shell, DI, páginas
tests/
  ComponentVault.Tests  xUnit v3: serviços, ViewModels e consistência catálogo × código
```

- **Snippets nunca divergem do código**: a Lib embute o próprio fonte como recurso; o app extrai as regiões marcadas com `<!-- snippet:id -->` / `// snippet:id`.
- **Adicionar um componente** = criar a View de preview com `[ShowcasePreview("id")]` + uma entrada em `ComponentCatalog`. Um teste falha se um snippet ou preview citado não existir.
- **Estilos são sempre nomeados** (nunca implícitos), para a variante Base continuar crua.

## Rodando

Requisitos: SDK .NET 10 e workload MAUI (Android + Windows).

```powershell
dotnet test --project tests/ComponentVault.Tests
dotnet build src/ComponentVault.App -f net10.0-windows10.0.19041.0 -t:Run
```

No Visual Studio, abra `ComponentVault.slnx` e rode o perfil **Windows Machine** ou um emulador Android.

As fontes ficam em `src/ComponentVault.App/Resources/Fonts` (`MauiFont`); fontes embutidas na Lib não carregavam no Windows.

Variáveis de ambiente para desenvolvimento e capturas:

| Variável | Efeito |
|---|---|
| `COMPONENTVAULT_START` | Abre o app numa rota do Shell, ex.: `//catalog` ou `component?id=switch` |
| `COMPONENTVAULT_THEME` | Força `Light`, `Dark` ou `Glass` nesta execução, sem salvar a preferência |

No Windows, uma exceção não tratada é registrada em `%LOCALAPPDATA%\ComponentVault\crash.log`.

O que ainda falta está em [ROADMAP.md](ROADMAP.md).

## Problemas conhecidos

- **Windows + leitor de tela:** com MAUI 10.0.130 e Windows App SDK 1.8 (WinUI 3.1.8), qualquer página com `ScrollView` derruba o app (access violation em `Microsoft.UI.Xaml.dll`) quando a UI Automation percorre a árvore, como o Narrador faz. Reproduz num app MAUI vazio com um `ScrollView` e um `Label`, inclusive com o Windows App SDK self-contained; é bug do framework, não do ComponentVault. Acompanhar as próximas versões do MAUI e do Windows App SDK.

## Decisões (ADRs)

As decisões do PRD (CommunityToolkit.Mvvm, Lib separada, SQLite para preferências, Shell, `.resx`) valem, com dois acréscimos:

- **ADR-006 — Core sem MAUI.** ViewModels e serviços ficam num projeto `net10.0`. O compilador garante que nenhuma ViewModel referencie `Microsoft.Maui.*`, e os testes rodam sem workload.
- **ADR-007 — Aviso de cópia dentro do app.** No Windows, o Snackbar do CommunityToolkit vira notificação do sistema; o aviso "copiado" é um componente próprio na tela.
