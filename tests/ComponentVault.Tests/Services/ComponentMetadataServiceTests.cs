using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Metadata;
using Microsoft.Extensions.Time.Testing;

namespace ComponentVault.Tests.Services;

public class ComponentMetadataServiceTests
{
    private static readonly FakeTimeProvider Time = new(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));

    private readonly ComponentMetadataService _service = new(Time);

    [Fact]
    public void Catalogo_real_e_valido_e_tem_ids_unicos()
    {
        var all = _service.GetAll();

        Assert.NotEmpty(all);
        Assert.All(all, i => Assert.NotEmpty(i.Variants));
        Assert.Equal(all.Count, all.Select(i => i.Id).Distinct().Count());
    }

    [Fact]
    public void Lista_vem_ordenada_por_nome()
    {
        var names = _service.GetAll().Select(i => i.Name).ToArray();

        Assert.Equal(names.Order(StringComparer.CurrentCultureIgnoreCase), names);
    }

    [Theory]
    [InlineData("button", "button")]
    [InlineData("BOTAO", "button")]       // sem acento e sem caixa
    [InlineData("uranium", "button")]     // pela biblioteca de uma variante
    [InlineData("toggle graphics", "switch")] // todos os termos precisam casar
    public void Busca_encontra_por_nome_tag_e_variante(string query, string expectedId)
    {
        Assert.Contains(_service.Search(query), i => i.Id == expectedId);
    }

    [Fact]
    public void Busca_sem_resultado_devolve_lista_vazia()
    {
        Assert.Empty(_service.Search("nada-disso-existe"));
    }

    [Fact]
    public void Busca_vazia_devolve_tudo()
    {
        Assert.Equal(_service.GetAll().Count, _service.Search("   ").Count);
    }

    [Fact]
    public void Filtro_de_origem_mantem_componentes_com_ao_menos_uma_variante_daquela_origem()
    {
        var custom = _service.Search(null, ComponentOrigin.Custom);

        Assert.NotEmpty(custom);
        Assert.All(custom, i => Assert.Contains(i.Variants, v => v.Origin == ComponentOrigin.Custom));
    }

    [Fact]
    public void Recentes_vem_do_mais_novo_para_o_mais_antigo()
    {
        var recent = _service.GetRecent(3);

        Assert.Equal(3, recent.Count);
        Assert.Equal(recent.OrderByDescending(i => i.AddedOn).Select(i => i.Id), recent.Select(i => i.Id));
    }

    [Fact]
    public void Badge_novo_respeita_a_janela_de_dias()
    {
        var service = new ComponentMetadataService(
        [
            Item("novo", new DateOnly(2026, 11, 20)),
            Item("velho", new DateOnly(2026, 9, 1)),
        ], Time);

        Assert.True(service.IsNew(service.GetById("novo")!));
        Assert.False(service.IsNew(service.GetById("velho")!));
    }

    [Fact]
    public void Novo_componente_registrado_aparece_sem_outra_alteracao()
    {
        var service = new ComponentMetadataService([Item("slider", new DateOnly(2026, 1, 1))], Time);

        Assert.Single(service.Search("slider"));
        Assert.NotNull(service.GetById("SLIDER"));
    }

    [Fact]
    public void Ids_de_variante_duplicados_sao_rejeitados()
    {
        Assert.Throws<InvalidOperationException>(() => new ComponentMetadataService(
        [
            Item("a", new DateOnly(2026, 1, 1), "x.base"),
            Item("b", new DateOnly(2026, 1, 1), "x.base"),
        ], Time));
    }

    [Fact]
    public void Componente_sem_variante_e_rejeitado()
    {
        var empty = Item("a", new DateOnly(2026, 1, 1)) with { Variants = [] };

        Assert.Throws<InvalidOperationException>(() => new ComponentMetadataService([empty], Time));
    }

    private static ComponentItem Item(string id, DateOnly addedOn, string? variantId = null) => new()
    {
        Id = id,
        Name = id,
        Category = ComponentCategory.Inputs,
        Summary = "teste",
        AddedOn = addedOn,
        Docs = new ComponentDocs { Overview = "-" },
        Variants =
        [
            new ComponentVariant
            {
                Id = variantId ?? $"{id}.base",
                Title = "Base",
                Tier = VariantTier.Base,
                Origin = ComponentOrigin.Native,
                Summary = "-",
            },
        ],
    };
}
