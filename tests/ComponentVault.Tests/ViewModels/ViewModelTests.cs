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

    private readonly ComponentMetadataService _metadata;

    public CatalogViewModelTests()
    {
        _metadata = new ComponentMetadataService(new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero)));
        _vm = new CatalogViewModel(_metadata, _navigation);
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
        // Nenhum chip de origem pode aparecer vazio: cada um tem pelo menos um componente no catálogo.
        Assert.All(_vm.Filters.Where(f => f.Origin is not null),
            f => Assert.Contains(_metadata.GetAll(), i => i.Origins.Contains(f.Origin!.Value)));
        // Origem sem variante nenhuma (ex.: "Novo" sem item recente) não vira chip.
        var vazias = Enum.GetValues<ComponentOrigin>().Where(o => !_metadata.GetAll().Any(i => i.Origins.Contains(o)));
        Assert.All(vazias, o => Assert.DoesNotContain(_vm.Filters, f => f.Origin == o));
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

    [Fact]
    public void Proximo_e_anterior_percorrem_os_niveis_em_volta()
    {
        Open("button");
        var count = _vm.Variants.Count;
        Assert.True(count > 1);
        Assert.Equal($"1 de {count}", _vm.PositionText);

        _vm.NextVariantCommand.Execute(null);
        Assert.Same(_vm.Variants[1], _vm.SelectedVariant);
        Assert.Equal($"2 de {count}", _vm.PositionText);
        Assert.Single(_vm.Variants, v => v.IsSelected);

        // Do primeiro, "anterior" vai para o último; do último, "próximo" volta ao primeiro.
        _vm.PreviousVariantCommand.Execute(null);
        _vm.PreviousVariantCommand.Execute(null);
        Assert.Same(_vm.Variants[^1], _vm.SelectedVariant);

        _vm.NextVariantCommand.Execute(null);
        Assert.Same(_vm.Variants[0], _vm.SelectedVariant);
    }

    [Fact]
    public void Link_com_present_abre_na_apresentacao()
    {
        _vm.OnNavigatedTo(new Dictionary<string, object>
        {
            [Routes.Params.ComponentId] = "button",
            [Routes.Params.Present] = "true",
        });

        Assert.True(_vm.IsPresentationMode);
        Assert.True(_vm.IsPreviewTab);
    }

    [Fact]
    public void Link_com_tab_abre_na_aba_pedida()
    {
        _vm.OnNavigatedTo(new Dictionary<string, object>
        {
            [Routes.Params.ComponentId] = "button",
            [Routes.Params.Tab] = "code",
        });

        Assert.True(_vm.IsCodeTab);
    }

    [Fact]
    public void Sair_da_apresentacao_so_sai()
    {
        Open("button");
        _vm.ExitPresentationCommand.Execute(null);
        Assert.False(_vm.IsPresentationMode);

        _vm.TogglePresentationCommand.Execute(null);
        _vm.ExitPresentationCommand.Execute(null);
        Assert.False(_vm.IsPresentationMode);
    }
}

public class HomeViewModelTests
{
    [Fact]
    public void Mostra_recentes_e_estatisticas_do_catalogo()
    {
        var metadata = new ComponentMetadataService(new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 0, 0, 0, TimeSpan.Zero)));
        var vm = new HomeViewModel(metadata, new FakeNavigation());

        Assert.Equal(HomeViewModel.RecentCount, vm.Recent.Count);
        Assert.Equal(metadata.GetAll().Count, vm.Stats[0].Value);
        Assert.Equal(metadata.GetAll().Sum(i => i.Variants.Count), vm.Stats[1].Value);
    }
}
