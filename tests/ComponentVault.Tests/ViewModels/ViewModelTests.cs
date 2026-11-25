using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Models;
using ComponentVault.Core.Navigation;
using ComponentVault.Core.Services.Highlighting;
using ComponentVault.Core.Services.Metadata;
using ComponentVault.Core.Services.Snippets;
using ComponentVault.Core.Services.Theming;
using ComponentVault.Core.ViewModels;
using ComponentVault.Tests.Fakes;
using Microsoft.Extensions.Time.Testing;

namespace ComponentVault.Tests.ViewModels;

public class CatalogViewModelTests
{
    private readonly FakeNavigation _navigation = new();
    private readonly CatalogViewModel _vm;

    public CatalogViewModelTests()
    {
        var metadata = new ComponentMetadataService(new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero)));
        _vm = new CatalogViewModel(metadata, _navigation, new ThemeService(new InMemoryPreferenceStore(), new FakeThemeApplier()));
    }

    [Fact]
    public void Abre_com_todos_os_componentes_e_o_chip_todos_selecionado()
    {
        Assert.NotEmpty(_vm.Results);
        Assert.True(_vm.Filters[0].IsSelected);
        Assert.Null(_vm.SelectedOrigin);
    }

    [Fact]
    public void Digitar_filtra_imediatamente()
    {
        var total = _vm.Results.Count;

        _vm.SearchText = "entr";

        Assert.Contains(_vm.Results, r => r.Id == "entry");
        Assert.True(_vm.Results.Count < total);
        Assert.Equal(Core.Presentation.Labels.Count(_vm.Results.Count, "componente", "componentes"), _vm.ResultSummary);
    }

    [Fact]
    public void Grupos_seguem_a_ordem_das_categorias_e_somam_os_resultados()
    {
        Assert.Equal(_vm.Results.Count, _vm.Groups.Sum(g => g.Count));
        var order = _vm.Groups.Select(g => g[0].Item.Category).ToArray();
        Assert.Equal(order.Order(), order);
    }

    [Fact]
    public void Busca_sem_resultado_marca_lista_vazia()
    {
        _vm.SearchText = "zzz";

        Assert.True(_vm.IsEmpty);
        Assert.Equal("Nenhum componente encontrado", _vm.ResultSummary);
    }

    [Fact]
    public void Chip_de_origem_combina_com_a_busca_e_limpar_volta_ao_inicio()
    {
        var total = _vm.Results.Count;
        var custom = _vm.Filters.Single(f => f.Origin == ComponentOrigin.Custom);

        _vm.SelectFilterCommand.Execute(custom);

        Assert.True(custom.IsSelected);
        Assert.False(_vm.Filters[0].IsSelected);
        Assert.All(_vm.Results, r => Assert.Contains(ComponentOrigin.Custom, r.Origins));

        _vm.ClearSearchCommand.Execute(null);

        Assert.Equal(total, _vm.Results.Count);
        Assert.True(_vm.Filters[0].IsSelected);
    }

    [Fact]
    public void Chips_so_mostram_origens_presentes_no_catalogo()
    {
        // Ainda não há variante Syncfusion; o chip não deve aparecer vazio.
        Assert.DoesNotContain(_vm.Filters, f => f.Origin == ComponentOrigin.Syncfusion);
    }

    [Fact]
    public async Task Abrir_componente_navega_com_o_id()
    {
        await _vm.OpenComponentCommand.ExecuteAsync(_vm.Results[0]);

        var (route, parameters) = Assert.Single(_navigation.Calls);
        Assert.Equal(Routes.ComponentDetail, route);
        Assert.Equal(_vm.Results[0].Id, parameters![Routes.Params.ComponentId]);
    }
}

public class ComponentDetailViewModelTests
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero));
    private readonly FakeClipboard _clipboard = new();
    private readonly ComponentDetailViewModel _vm;

    public ComponentDetailViewModelTests()
    {
        var snippets = new SnippetRepository(
        [
            new FakeSnippetSource(
                new SnippetFile("ButtonBase.xaml", "<!-- snippet:button.base -->\n<Button Text=\"Ok\" />\n<!-- /snippet:button.base -->")),
        ]);

        _vm = new ComponentDetailViewModel(
            new ComponentMetadataService(_time), snippets, new ColorCodeHighlighter(), _clipboard, _time);
    }

    private void Open(string id, string? variant = null)
    {
        var parameters = new Dictionary<string, object> { [Routes.Params.ComponentId] = id };
        if (variant is not null) parameters[Routes.Params.VariantId] = variant;
        _vm.OnNavigatedTo(parameters);
    }

    [Fact]
    public void Abre_na_primeira_variante_e_na_aba_preview()
    {
        Open("button");

        Assert.Equal("Button", _vm.Title);
        Assert.Equal("button.base", _vm.SelectedVariant!.Id);
        Assert.True(_vm.SelectedVariant.IsSelected);
        Assert.True(_vm.IsPreviewTab);
        Assert.Equal("<Button Text=\"Ok\" />", _vm.SnippetModels[0].Code);
        Assert.NotEmpty(_vm.SnippetModels[0].Tokens);
    }

    [Fact]
    public void Pode_abrir_direto_numa_variante()
    {
        Open("button", "button.uranium");

        Assert.Equal("button.uranium", _vm.SelectedVariant!.Id);
        Assert.Single(_vm.Variants, v => v.IsSelected);
    }

    [Fact]
    public void Trocar_variante_troca_os_snippets_e_marca_ausentes()
    {
        Open("button");

        _vm.SelectVariantCommand.Execute(_vm.Variants[1]);

        Assert.Equal("button.styled", _vm.SelectedVariant!.Id);
        Assert.All(_vm.SnippetModels, s => Assert.True(s.IsMissing)); // não estão na fonte falsa
    }

    [Fact]
    public void Componente_inexistente_vira_mensagem_de_erro()
    {
        Open("nao-existe");

        Assert.True(_vm.HasError);
        Assert.False(_vm.IsLoaded);
    }

    [Fact]
    public void Lado_a_lado_so_existe_com_mais_de_um_nivel()
    {
        Open("button");
        Assert.True(_vm.CanCompare);

        _vm.SelectTabCommand.Execute(DetailTab.Compare);
        Assert.True(_vm.IsCompareTab);
        Assert.False(_vm.IsPreviewTab);
    }

    [Fact]
    public void Abas_sao_mutuamente_exclusivas()
    {
        Open("button");

        _vm.SelectTabCommand.Execute(DetailTab.Code);

        Assert.True(_vm.IsCodeTab);
        Assert.False(_vm.IsPreviewTab);
        Assert.False(_vm.IsDocsTab);
    }

    [Fact]
    public async Task Copiar_envia_o_codigo_e_mostra_aviso_que_some_sozinho()
    {
        Open("button");

        var copy = _vm.CopySnippetCommand.ExecuteAsync(_vm.SnippetModels[0]);

        Assert.Equal("<Button Text=\"Ok\" />", _clipboard.Text);
        Assert.True(_vm.HasCopyFeedback);

        _time.Advance(ComponentDetailViewModel.CopyFeedbackDuration);
        await copy;

        Assert.False(_vm.HasCopyFeedback);
    }

    [Fact]
    public async Task Snippet_ausente_nao_vai_para_a_area_de_transferencia()
    {
        Open("button", "button.styled");

        await _vm.CopySnippetCommand.ExecuteAsync(_vm.SnippetModels[0]);

        Assert.Null(_clipboard.Text);
    }

    [Fact]
    public void Modo_apresentacao_alterna_e_volta_para_o_preview()
    {
        _vm.SelectTabCommand.Execute(DetailTab.Code);

        _vm.TogglePresentationCommand.Execute(null);
        Assert.True(_vm.IsPresentationMode);
        Assert.True(_vm.IsPreviewTab);

        _vm.TogglePresentationCommand.Execute(null);
        Assert.False(_vm.IsPresentationMode);
    }
}

public class HomeViewModelTests
{
    [Fact]
    public void Mostra_recentes_e_estatisticas_do_catalogo()
    {
        var metadata = new ComponentMetadataService(new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero)));
        var theme = new ThemeService(new InMemoryPreferenceStore(), new FakeThemeApplier());
        var vm = new HomeViewModel(metadata, new FakeNavigation(), theme, new AppearanceViewModel(theme));

        Assert.Equal(HomeViewModel.RecentCount, vm.Recent.Count);
        Assert.Equal(metadata.GetAll().Count, vm.Stats[0].Value);
        Assert.Equal(metadata.GetAll().Sum(i => i.Variants.Count), vm.Stats[1].Value);
    }
}
