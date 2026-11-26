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
- [ ] **Componentes de biblioteca em execução**, em cada tema:
  - [ ] SwipeView
  - [ ] Carousel
  - [ ] Tabs (Sharpnado)
  - [ ] Gráficos (LiveCharts): os eixos e as séries seguem a paleta?
  - [ ] Popup (CommunityToolkit)
  - [ ] Controles UraniumUI
- [ ] **Interação real no Windows.** As capturas automatizadas só abrem rotas. Clicar à mão e conferir:
  - [ ] o seletor "Aparência"
  - [ ] o botão Tema
  - [ ] a troca de nível com a transição
  - [ ] a aba Lado a lado
  - [ ] o copiar código
  - [ ] a busca e os filtros
- [x] ~~Barra flutuante com cliques reais~~ (testada pelo dono no Windows em 26/11/2026).
- [ ] **Barra flutuante no celular:** a largura (só a seção ativa mostra o nome) e a margem sobre a barra de gestos do Android.
- [ ] **Foco automático na busca** ao abrir o Catálogo no Windows. Decidir se fica; no celular abriria o teclado sem pedir.
- [ ] **Tema Vidro em todas as telas** além de Home, Catálogo e Detalhe do Switch: o componente Glass, a aba Código (cores de sintaxe) e os popups.

## 2. Bugs conhecidos

- [x] ~~Título da janela ilegível no tema Claro~~ (barra de título com as cores da paleta).
- [x] ~~Seletor de tema fora de sincronia com `COMPONENTVAULT_THEME`~~ (`IThemeService.Preview`).
- [ ] **Catálogo às vezes abre rolado** (visto uma vez antes da navegação empilhada). Agora a página é nova a cada abertura; confirmar se ainda acontece.
- [x] ~~Código morto do botão Tema antigo~~ (removido da Home, do Catálogo e do seletor de aparência).
- [ ] **ScrollView + UI Automation derruba o app no Windows** (bug do framework, descrito no README). Testar de novo a cada versão do MAUI e do Windows App SDK.

## 3. Componentes: completar os níveis

Componentes que ainda têm um nível só:

| Componente | Tem | Falta avaliar |
|---|---|---|
| Alert | Estilizado | Custom (com ícone e ação), Biblioteca |
| Chips | Estilizado | Base (`RadioButton`/`CheckBox` com template), Custom |
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
- [ ] **Barra que se recolhe ao rolar** para baixo e volta ao rolar para cima (celular).
- [ ] **Transição entre páginas**: o card do catálogo "cresce" até o cabeçalho do detalhe (shared element aproximado com Translation e Scale).
- [ ] **Entrada em cascata** dos itens do catálogo ao rolar e ao filtrar.
- [ ] **Modo Apresentar**: tela cheia com o componente centralizado e troca de nível por gesto ou teclado.
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
