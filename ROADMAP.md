# Roadmap

O que ainda falta no ComponentVault, em ordem de prioridade. Situação em 26/11/2026: 30 componentes e 63 variantes, 3 temas, barra flutuante de navegação e tema, build Release do Windows sem avisos e 63 testes passando.

## Feito em 26/11/2026

- **Barra flutuante (`NavDock`)** no lugar das abas e do botão "Tema" do Shell. Fica no topo no desktop e embaixo no celular, e reúne voltar, Início, Catálogo e o seletor direto dos 3 temas. A pílula desliza entre as seções, inclusive ao trocar de página, e a barra entra com animação na abertura do app.
- **Navegação empilhada.** A Home é a única raiz, e o Catálogo e o Detalhe são empilhados sobre ela. Abas escondidas do Shell não trocam de conteúdo no Windows.
- **Barra de título do Windows** com as cores da paleta. Corrige o título ilegível no tema Claro.
- **Seletor de tema sempre sincronizado:** marca o tema visível, inclusive em "Sistema" e com `COMPONENTVAULT_THEME`.
- **Filtros do catálogo e seletor de nível** em cápsula, com feedback de toque, no mesmo desenho da barra.

## 1. Verificação pendente

O que foi escrito, mas ainda não rodou de ponta a ponta.

- [ ] **Android em execução.** O build Android completo não coube no disco (sobram cerca de 2 GB). Liberar espaço e rodar no emulador: fontes, os 3 temas, o `AmbientBackground` e os mappers `Borderless*` (Background nulo).
- [x] ~~Componentes de biblioteca nos 3 temas~~ (varredura por captura em 27/11/2026: UraniumUI, Sharpnado, LiveCharts, Toolkit, MauiIcons, Carousel e Glass; correções testadas pelo dono no Windows). Achados e corrigidos:
  - LiveCharts com fundo preto sólido no Escuro e no Vidro, fonte do sistema nos eixos, total errado (1.284 em vez de 1.176), rosca com fatias em preto e branco e card dentro de card. O preview foi refeito: painel único, fundo transparente, mês atual em destaque, meta tracejada, rosca em tons de azul com o total no meio e legenda em XAML. A fonte do app vale para todos os gráficos (`HasTextSettings` em `UseVaultDesignSystem`).
  - Campos da UraniumUI (Entry, Picker e DatePicker) encolhidos ao tamanho do texto de exemplo: agora 440 px no desktop e largura total no celular.
- [x] ~~UraniumUI `TimePickerField` vazio com o título no alto~~: o exemplo ganhou um horário inicial (14:30), e o par agora mostra os dois estados (data vazia, horário preenchido). As caixas cinzas que o TimePicker nativo desenhava no Windows dentro do campo foram removidas (mesma limpeza dos campos sem borda). Os campos de data e horário ficaram com 280 px no desktop, o tamanho do conteúdo, em vez dos 440 px dos campos de texto.
- [ ] **SwipeView e Popup com interação real** (deslizar a linha, abrir o popup): as capturas só mostram o estado inicial.
- [ ] **Interação real no Windows.** As capturas automatizadas só abrem rotas. Clicar à mão e conferir:
  - [ ] o seletor "Aparência"
  - [ ] o botão Tema
  - [ ] a troca de nível com a transição
  - [ ] a aba Lado a lado
  - [ ] o copiar código
  - [ ] a busca e os filtros
- [x] ~~Barra flutuante com cliques reais~~ (testada pelo dono no Windows em 26/11/2026).
- [ ] **Barra flutuante no celular:** a largura (só a seção ativa mostra o nome) e a margem sobre a barra de gestos do Android.
- [x] ~~Foco automático na busca~~ ao abrir o Catálogo: o campo recusa o foco que a plataforma dá sozinha no primeiro segundo (`VaultTextField.RefusesInitialFocus`).
- [ ] **Tema Vidro em todas as telas** além de Home, Catálogo e Detalhe do Switch: o componente Glass, a aba Código (cores de sintaxe) e os popups.

## 2. Bugs conhecidos

- [x] ~~Título da janela ilegível no tema Claro~~ (barra de título com as cores da paleta).
- [x] ~~Seletor de tema fora de sincronia com `COMPONENTVAULT_THEME`~~ (`IThemeService.Preview`).
- [x] ~~Catálogo às vezes abre rolado~~: não se repetiu em dezenas de aberturas desde a navegação empilhada (a página é nova a cada abertura).
- [x] ~~Código morto do botão Tema antigo~~ (removido da Home, do Catálogo e do seletor de aparência).
- [ ] **ScrollView + UI Automation derruba o app no Windows** (bug do framework, descrito no README). Testar de novo a cada versão do MAUI e do Windows App SDK.

## 3. Componentes: completar os níveis

Componentes que ainda têm um nível só:

| Componente | Tem | Falta avaliar |
|---|---|---|
| ~~Alert~~ | Base, Estilizado, Custom | Feito em 27/11/2026: Base (Border e Label crus) e Custom (pilha de avisos, `AlertStack`). Sem Biblioteca: não há lib gratuita de aviso em linha, e o Snackbar do Toolkit é um toast (já está no Toast). Testado pelo dono no Windows. |
| ~~Chips~~ | Base, Estilizado, Custom | Feito em 27/11/2026: Base (`RadioButton` e `CheckBox` crus) e Custom (`ChoiceChipGroup` com pílula deslizante e `FilterChip` com check animado). O `SlidingIndicator` foi para a Lib e é usado pelo app e pelo preview. Falta testar com clique. |
| Empty state | Estilizado | Custom com ilustração animada |
| Glass | Custom | Estilizado (só tokens, sem fundo ambiente) |
| Código OTP | Custom | Base (Entry por dígito, para comparar) |
| Rating | Custom | Base (botões), Biblioteca |
| Skeleton | Custom | Biblioteca (shimmer) |

## 4. Componentes novos

Candidatos, priorizando os que mais aparecem nos apps de referência (Mobbin, ScreensDesign) e as libs gratuitas do awesome-dotnet-maui:

- [ ] **Bottom sheet**: Base com `Border` animado; Biblioteca com The49.Maui.BottomSheet.
- [ ] **Expander / Accordion**: CommunityToolkit `Expander` e uma versão estilizada.
- [ ] **FAB e menu de ações**: botão flutuante que abre as ações em leque.
- [ ] **Assinatura / desenho**: CommunityToolkit `DrawingView`.
- [ ] **Calendário**: seleção de intervalo, mais rico que o DatePicker.
- [ ] **DataGrid / tabela**: ordenação e colunas fixas.
- [ ] **Range slider**: dois polegares, por exemplo para filtro de preço.
- [ ] **Animação Lottie / vetorial**: SkiaSharp.Extended.
- [ ] **Paginação e indicador de páginas** (complementa o Carousel).
- [ ] **Timeline / linha do tempo** (Custom).
- [ ] **Tooltip e menu de contexto**: `MenuFlyout` no Windows, toque longo no Android.
- [ ] **Seletor de cor** (Custom com Maui.Graphics).

## 5. Design e movimento (direção "ousada")

- [x] ~~Barra de navegação flutuante de vidro~~ (`NavDock`).
- [x] ~~Abas do detalhe com indicador deslizante~~ (`SlidingIndicator`, o mesmo da barra flutuante).
- [x] ~~Seletor de nível do detalhe com a pílula deslizante~~ (testado pelo dono no Windows; `SlidingIndicator`, que agora também acompanha itens criados depois, como os de um `BindableLayout`).
- [ ] **Barra que se recolhe ao rolar** para baixo e volta ao rolar para cima. Só faz sentido no celular, então fica para quando o Android rodar.
- [x] ~~Transição entre páginas~~: o ícone e o título da linha tocada (Catálogo ou recentes da Home) voam até o cabeçalho do Detalhe, que ganhou o ícone do componente, e o resto entra em cascata (`App/Controls/Hero.cs`). Testado pelo dono no Windows.
- [x] ~~Transição de volta~~: ao voltar do Detalhe, o ícone e o título descem do cabeçalho até a linha de onde saíram, no Catálogo ou nos recentes da Home. Voam como cópias por cima da página, porque a lista recorta o que passa fora dela. Na barra, tocar na seção de onde o Detalhe veio volta uma página. Testado pelo dono no Windows.
- [x] ~~Entrada em cascata~~ dos itens e grupos do catálogo, ao abrir a página e ao trocar o filtro (`App/Controls/Cascade.cs`). De propósito, não anima ao rolar, ao digitar na busca nem ao voltar do Detalhe. Testado pelo dono no Windows.
- [x] ~~Modo Apresentar~~: componente centralizado sobre o fundo, com nome, nível e "2 de 3" no topo e um controle flutuante embaixo (anterior, pontos dos níveis, próximo, sair). No Windows abre em tela cheia, ← e → trocam o nível e Esc sai. Deslizar para os lados fora do componente também troca. O voltar do Android sai da apresentação. Link direto com `present=true`. Teclado e cliques testados pelo dono no Windows; gesto e voltar do Android dependem do Android.
- [ ] **Tela cheia imersiva no Android** durante a apresentação (esconder barras do sistema).
- [ ] **Microinterações**: feedback háptico no Android (`HapticFeedback`) e o realce do botão copiar.
- [ ] **Fundo ambiente que reage**: as manchas puxam levemente para o ponteiro ou o toque, ou mudam de tom conforme a categoria.
- [ ] **Respeitar "reduzir movimento"** do sistema e desligar a deriva e as entradas.
- [ ] **Ícone do app e splash** com a identidade nova (hoje são os padrões do template).

## 6. Produto (PRD)

- [ ] **Favoritos e histórico** dos componentes vistos (SQLite já está no projeto).
- [ ] **Exportar snippet** como arquivo ou compartilhar (`Share`).
- [ ] **Localização** com `.resx` (pt-BR e en-US), conforme o PRD.
- [ ] **Busca por propriedade** (ex.: "CornerRadius") usando os Docs.
- [ ] **Tela "Sobre"** com as libs usadas e as licenças.

## 7. Qualidade e entrega

- [ ] **Acessibilidade**: `SemanticProperties` nos controles Custom, contraste do Vidro (texto sobre manchas claras) e ordem de foco.
- [ ] **Testes de UI** (Appium) para os fluxos principais, quando o bug de UIA do Windows for corrigido.
- [ ] **CI**: o workflow atual compila e testa. Adicionar os artefatos Windows e Android (APK) e o build com `TreatWarningsAsErrors`.
- [ ] **Desempenho**: medir a abertura do catálogo e o custo da deriva do fundo em aparelho Android modesto.
- [ ] **Primeiro commit e PR** para `develop`: identidade pessoal, mensagens em pt-BR.
