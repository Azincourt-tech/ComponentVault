# Roadmap

O que ainda falta no ComponentVault, em ordem de prioridade. Situação em 26/11/2026: 30 componentes e 63 variantes, 3 temas, barra flutuante de navegação e tema, build Release do Windows sem avisos e 63 testes passando.

## Feito em 26/11/2026

- **Barra flutuante (`NavDock`)** no lugar das abas e do botão "Tema" do Shell. Fica no topo no desktop e embaixo no celular, e reúne voltar, Início, Catálogo e o seletor direto dos 3 temas. A pílula desliza entre as seções, inclusive ao trocar de página, e a barra entra com animação na abertura do app.
- **Navegação empilhada.** A Home é a única raiz, e o Catálogo e o Detalhe são empilhados sobre ela. Abas escondidas do Shell não trocam de conteúdo no Windows.
- **Barra de título do Windows** com as cores da paleta. Corrige o título ilegível no tema Claro.
- **Seletor de tema sempre sincronizado:** marca o tema visível, inclusive em "Sistema" e com `COMPONENTVAULT_THEME`.
- **Filtros do catálogo e seletor de nível** em cápsula, com feedback de toque, no mesmo desenho da barra.

## Feito em 27/11/2026: revisão geral, responsivo e nova identidade

- **Revisão de alinhamento e espaçamento** de todas as variantes e páginas, por captura:
  - cerca de 30 correções, como itens cortados em `FlexLayout`, blocos centralizados por engano, campos estreitos e ícones fora do centro;
  - Lado a lado e Docs na mesma coluna do cabeçalho;
  - Catálogo alinhado com a busca.
- **Bug grave do Release:** a aba Código dizia "não encontrado" para todo snippet em XAML. O XamlC descartava os arquivos embutidos.
- **App responsivo pela largura:**
  - faixas compacta, média e expandida (`Adaptive`, `OnWidth`), no lugar de `OnIdiom`;
  - larguras "até N" (`Fit.MaxWidth`) em vez de fixas;
  - conferido em 390×844, 768×1024 e 1280×800.
- **Navegação no celular:** barra inferior em largura total (Início, Catálogo, Tema). O Tema transforma a barra no seletor de temas, e o voltar do Detalhe fica no topo.
- **Nova identidade:**
  - paleta "Fiery Red Sunset", com o Cayenne `#E85D04` como destaque, longe do azul nativo, para o Estilizado se separar do Base; o ícone e o splash usam o degradê da paleta;
  - Claro "Pérola" e Escuro com cards metalizados e luz de pôr do sol; Escuro e Vidro sobre o Ink Black `#03071E`;
  - uma cor por nível (Base aço, Estilizado Cayenne, Custom verde-água, Biblioteca Amber Flame);
  - fundo de galáxia animado: manchas de luz que fluem como líquido e estrelas que piscam, mais forte no Vidro.

## 1. Verificação pendente

- [ ] **Barra inferior com clique** (faixa compacta): troca de seção e o seletor de temas que substitui a barra.
- [ ] **Fundo animado no Android:** conferir a fluidez e o consumo de bateria das manchas e das estrelas. Se pesar, baixar a taxa de quadros ou desligar as estrelas no celular.
- [ ] **Faixa da barra de título no Windows:** a luz do fundo é cortada na altura da `TitleBar`, que tem a cor `Ground` lisa. Avaliar estender o conteúdo por baixo dela.
- [x] ~~Revisar os níveis Estilizado lado a lado com o Base~~ (27/11/2026): campos preenchidos (`FieldFrame` + `FieldFrames.Apply`), ícones e força da senha no Entry, ícone que acompanha a escolha no Picker, barra de formatação no Editor, ícones e valor em pílula no Slider, envio já em andamento no Progress, grade de cards clicáveis no Card. Os previews ocupam a largura toda do card.

O que foi escrito, mas ainda não rodou de ponta a ponta.

- [ ] **Android em execução.** O build Android completo não coube no disco (sobram cerca de 2 GB). Liberar espaço e rodar no emulador: fontes, os 3 temas, o `AmbientBackground` e os mappers `Borderless*` (Background nulo).
- [x] ~~Componentes de biblioteca nos 3 temas~~ (varredura por captura em 27/11/2026: UraniumUI, Sharpnado, LiveCharts, Toolkit, MauiIcons, Carousel e Glass; correções testadas pelo dono no Windows). Achados e corrigidos:
  - LiveCharts com fundo preto sólido no Escuro e no Vidro, fonte do sistema nos eixos, total errado (1.284 em vez de 1.176), rosca com fatias em preto e branco e card dentro de card. O preview foi refeito: painel único, fundo transparente, mês atual em destaque, meta tracejada, rosca em tons de azul com o total no meio e legenda em XAML. A fonte do app vale para todos os gráficos (`HasTextSettings` em `UseVaultDesignSystem`).
  - Campos da UraniumUI (Entry, Picker e DatePicker) encolhidos ao tamanho do texto de exemplo: agora 440 px no desktop e largura total no celular.
- [x] ~~UraniumUI `TimePickerField` vazio com o título no alto~~: o exemplo ganhou um horário inicial (14:30), e o par agora mostra os dois estados (data vazia, horário preenchido). As caixas cinzas que o TimePicker nativo desenhava no Windows dentro do campo foram removidas (mesma limpeza dos campos sem borda). Os campos de data e horário ficaram com 280 px no desktop, o tamanho do conteúdo, em vez dos 440 px dos campos de texto.
- [ ] **SwipeView e Popup com interação real** (deslizar a linha, abrir o popup): as capturas só mostram o estado inicial.
- [ ] **Gestos dos Custom novos** (segurar, virar, arrastar, rodas, folha inferior, faixa de dias, `SwipeRow`, busca que expande, ícones animados): as capturas só mostram o estado estático. Conferir à mão no Windows e no Android, principalmente o arrasto com mouse (`PanGestureRecognizer`) no Windows.
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
| ~~Empty state~~ | Base, Estilizado, Custom | Feito em 27/11/2026: Base (`CollectionView` com `EmptyView` em texto) e Custom (`SearchIllustration` em Microsoft.Maui.Graphics, com cartões flutuando e lupa passeando, mais sugestões de busca em chips). Testado pelo dono no Windows. |
| ~~Glass~~ | Base, Estilizado, Custom | Feito em 27/11/2026: Base (`Border` translúcido sem cena, um cinza lavado) e Estilizado (cards do design system só com tokens: sólidos no Claro e no Escuro, vidro no tema Vidro). Conferido por captura nos 3 temas. |
| ~~Código OTP~~ | Base, Custom, Biblioteca | Feito em 28/11/2026: Base com um `Entry` por dígito e avanço de foco, mostrando o que falha (colar, apagar, leitor de tela). |
| ~~Rating~~ | Base, Custom, Biblioteca | Feito em 28/11/2026: Base com cinco `Button` de estrela em texto e Biblioteca com o `RatingView` do CommunityToolkit.Maui (já no projeto), com estrela, coração e média só para leitura. |
| ~~Skeleton~~ | Custom, Biblioteca | Feito em 27/11/2026: Biblioteca com o `SfShimmer` do Syncfusion Toolkit (modelo pronto e esqueleto do item real). |

### Uma aba por biblioteca e um Custom onde couber

Pedido do dono em 27/11/2026: se mais de uma lib tem o componente, cada uma ganha a sua aba no seletor de nível, para mostrar todas as possibilidades. E todo componente que dá para desenhar à mão ganha o nível Custom.

**Regras para entrar uma lib:** 100% livre (MIT, Apache e afins, sem cadastro, chave nem limite de faturamento) e com build para .NET 10 no Windows e no Android. Conferido no NuGet em 27/11/2026:

| Lib | Licença | .NET 10 | Situação |
|---|---|---|---|
| Syncfusion.Maui.Toolkit 1.0.12 | MIT | sim | **Entra.** É o toolkit open source, diferente da linha paga (que pede chave). Origem "Syncfusion" no catálogo. |
| Plugin.Maui.Calendar 3.3.0 | MIT | sim | **Entra** (calendário). |
| Microcharts.Maui 2.0.0.3 | MIT | sim (TFM `net10.0`) | **Entra** (gráficos). Conferir no Windows. |
| CommunityToolkit.Maui.MediaElement 11.0.0 | MIT | sim | **Entra** quando vierem os players (a 12 já é .NET 11). |
| UraniumUI, CommunityToolkit.Maui, Sharpnado.Tabs, LiveCharts, MauiIcons | MIT | sim | Já no projeto. |
| Mopups, Maui.DataGrid, Plugin.Maui.Audio, SkiaSharp.Extended, Sharpnado.CollectionView, The49.BottomSheet | MIT | não | **Fora:** param no .NET 7, 8 ou 9. |
| HorusStudio MaterialDesignControls | MIT | sem Windows | Fora. |
| Plugin.Maui.SegmentedControl | MIT | só a 3.0 (.NET 11) tem Windows | Fora por enquanto. |
| Syncfusion (linha paga), DevExpress, Telerik | comercial ou com cadastro | | Fora: o dono escolheu só libs 100% livres. |

**Matriz** (✓ já existe, **+** a fazer):

| Componente | Base | Estilizado | Custom | Bibliotecas |
|---|---|---|---|---|
| Avatar | ✓ | ✓ | ✓ anel de stories e status | ✓ Toolkit |
| Button | ✓ | ✓ | ✓ segurar para confirmar | ✓ UraniumUI, ✓ Syncfusion `SfButton` |
| Card | ✓ | ✓ | ✓ card que vira | ✓ Toolkit Expander, ✓ Syncfusion `SfCardView`, ✓ Syncfusion `SfAccordion` |
| Carousel | ✓ | ✓ | ✓ pilha de cartões | ✓ Syncfusion `SfCarousel` |
| Chart | | ✓ | ✓ | ✓ LiveCharts, ✓ Syncfusion `SfCartesianChart`, ✓ Microcharts |
| CheckBox | ✓ | ✓ | ✓ check desenhado | ✓ UraniumUI |
| Chips | ✓ | ✓ | ✓ | ✓ Syncfusion `SfChipGroup` |
| DatePicker | ✓ | ✓ | ✓ faixa de dias | ✓ UraniumUI, ✓ Syncfusion `SfCalendar`, ✓ Plugin.Maui.Calendar |
| Editor | ✓ | ✓ | ✓ Markdown com prévia | |
| Entry | ✓ | ✓ | ✓ rótulo flutuante | ✓ UraniumUI, ✓ Syncfusion `SfTextInputLayout` |
| Icons | ✓ | ✓ | ✓ ícone animado | ✓ MauiIcons |
| List | ✓ | ✓ | ✓ linhas com deslizar desenhado | ✓ Syncfusion `SfPullToRefresh` |
| Código OTP | ✓ | | ✓ | ✓ Syncfusion `SfOtpInput` |
| Picker | ✓ | ✓ | ✓ roda (wheel) | ✓ UraniumUI, ✓ Syncfusion `SfPicker` |
| Popup | ✓ | ✓ | ✓ folha inferior | ✓ Toolkit, ✓ Syncfusion `SfPopup` |
| Progress | ✓ | ✓ | ✓ | ✓ Syncfusion linear e circular |
| Radio | ✓ | ✓ | ✓ opções desenhadas | ✓ UraniumUI |
| Rating | ✓ | | ✓ | ✓ Toolkit `RatingView` |
| SearchBar | ✓ | ✓ | ✓ busca com sugestões | ✓ UraniumUI `AutoCompleteTextField` |
| Segmented | | ✓ | ✓ | ✓ Sharpnado, ✓ Syncfusion `SfSegmentedControl` |
| Skeleton | | | ✓ | ✓ Syncfusion `SfShimmer` |
| Stepper | ✓ | ✓ | ✓ contador que rola | ✓ Syncfusion `SfNumericUpDown` |
| Tabs | | ✓ | ✓ | ✓ Sharpnado, ✓ Syncfusion `SfTabView`, ✓ UraniumUI `TabView` |
| Toast | | ✓ | ✓ | ✓ Toolkit |

Lacunas que ficam de propósito: Chart, Segmented, Skeleton, Tabs e Toast não têm Base porque o MAUI não tem esses controles (o Estilizado já mostra a montagem com controles nativos). O Skeleton não tem Estilizado porque o Custom já é feito só com `Border` e pulso de opacidade. Rating e OTP não têm Estilizado: o Base mostra o limite, e o Custom resolve.

Componentes novos que o Syncfusion Toolkit também cobre: Bottom sheet (`SfBottomSheet`), Navigation drawer (`SfNavigationDrawer`), Pull to refresh e Accordion.

**Atenção:** com quatro ou mais abas, o seletor de nível precisa caber na faixa compacta (rolar na horizontal ou quebrar em duas linhas).

## 4. Componentes novos

Candidatos, priorizando os que mais aparecem nos apps de referência (Mobbin, ScreensDesign) e as libs gratuitas do awesome-dotnet-maui:

- [x] ~~**Bottom sheet**~~: feito em 28/11/2026 com os quatro níveis: Base (`Border` com `IsVisible`), Estilizado (tokens, fundo escurecido e animação), Custom (`SnapSheet` com três paradas arrastáveis, como nos apps de mapa) e Biblioteca (`SfBottomSheet`). Falta testar o arrasto à mão.
- [ ] **Expander / Accordion**: CommunityToolkit `Expander`, `SfExpander` e `SfAccordion` do Syncfusion Toolkit, e uma versão estilizada.
- [ ] **FAB e menu de ações**: botão flutuante que abre as ações em leque.
- [ ] **Assinatura / desenho**: CommunityToolkit `DrawingView`.
- [ ] **Calendário**: seleção de intervalo, mais rico que o DatePicker. Libs: `SfCalendar` (Syncfusion Toolkit) e Plugin.Maui.Calendar.
- [ ] **DataGrid / tabela**: ordenação e colunas fixas. Sem lib livre com .NET 10 hoje (o Maui.DataGrid parou no .NET 9): Custom.
- [ ] **Range slider**: dois polegares, por exemplo para filtro de preço.
- [ ] **Animação Lottie / vetorial**: o SkiaSharp.Extended parou no .NET 9; reavaliar quando sair versão para .NET 10.
- [ ] **Paginação e indicador de páginas** (complementa o Carousel).
- [ ] **Timeline / linha do tempo** (Custom).
- [ ] **Tooltip e menu de contexto**: `MenuFlyout` no Windows, toque longo no Android.
- [ ] **Seletor de cor** (Custom com Maui.Graphics).

### Mídia e documentos

- [ ] **Player de vídeo**: Base com `MediaElement` do CommunityToolkit (pacote gratuito `CommunityToolkit.Maui.MediaElement` 11, o último com .NET 10) e os controles nativos; Custom com controles próprios por cima (play/pausa, barra de progresso com prévia, tela cheia, legendas) no visual do design system.
- [ ] **Player de áudio**: `MediaElement` sem vídeo, com capa, forma de onda desenhada em Maui.Graphics, velocidade de reprodução e mini player fixo (como o "Tocando agora" do Glass).
- [ ] **Visualizador de PDF**: não há lib gratuita nativa e multiplataforma. Avaliar `WebView` com pdf.js empacotado (offline), o visualizador do sistema (`Launcher`) como Base, e citar a opção paga (Syncfusion) só como referência.
- [ ] **Visualizador de documentos** (texto, Markdown, código): Markdown renderizado com uma lib gratuita (ex.: Markdig + `HtmlWebViewSource`, ou um renderizador nativo de Labels); prévia de arquivos por tipo com ícone e metadados.
- [ ] **Imagem**: Base com `Image` e `Aspect`; Estilizado com placeholder, carregamento progressivo e cantos do design system; Custom com zoom por pinça, arrastar e toque duplo (`PinchGestureRecognizer` e `PanGestureRecognizer`).
- [ ] **Galeria e lightbox**: grade de imagens que abre em tela cheia com deslizar entre fotos, contador e fechar arrastando para baixo (Custom); complementa o Carousel.
- [ ] **Carrossel de mídia**: além do Carousel atual, versões com imagem de fundo e texto por cima, autoplay com pausa ao tocar e indicador de progresso por slide (no estilo dos stories).
- [ ] **Seleção e envio de arquivos**: `FilePicker` e `MediaPicker` como Base; Custom com área de soltar (Windows), prévia, barra de progresso de envio e remover.
- [ ] **Câmera e leitor de código**: avaliar `CommunityToolkit.Maui.Camera` e ZXing.Net.Maui (gratuitos) para foto e QR code.

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
- [x] ~~Ícone do app e splash com a identidade nova~~ (27/11/2026): degradê Amber Flame, Cayenne e Oxblood.

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
- [ ] **PR da `develop` para a `main`**: os commits já estão na `develop` (identidade pessoal, mensagens em pt-BR); a `main` só tem o commit inicial, então o README novo ainda não aparece na página do repositório.
- [ ] **Capturas no README** quando os componentes estiverem completos: Home nos 3 temas, Detalhe com Lado a lado, Catálogo no Vidro e o celular (390 px). Guardar em `docs/images` e atualizar os selos de contagem (componentes e variantes).
