using ComponentVault.Core.Abstractions;
using ComponentVault.Core.Services.Snippets;
using ComponentVault.Tests.Fakes;

namespace ComponentVault.Tests.Services;

public class SnippetParserTests
{
    [Fact]
    public void Extrai_regiao_xaml_e_remove_indentacao_comum()
    {
        const string xaml = """
            <ContentView>
                <VerticalStackLayout>
                    <!-- snippet:demo -->
                    <Button Text="Ok">
                        <Button.Margin>4</Button.Margin>
                    </Button>
                    <!-- /snippet:demo -->
                </VerticalStackLayout>
            </ContentView>
            """;

        var result = SnippetParser.Parse(xaml);

        Assert.Equal("<Button Text=\"Ok\">\n    <Button.Margin>4</Button.Margin>\n</Button>", result["demo"]);
    }

    [Fact]
    public void Extrai_regiao_csharp()
    {
        const string code = """
            class A
            {
                // snippet:metodo
                void B() { }
                // /snippet:metodo
            }
            """;

        Assert.Equal("void B() { }", SnippetParser.Parse(code)["metodo"]);
    }

    [Fact]
    public void Regioes_aninhadas_nao_incluem_linhas_de_marcacao()
    {
        const string code = """
            // snippet:fora
            a
            // snippet:dentro
            b
            // /snippet:dentro
            c
            // /snippet:fora
            """;

        var result = SnippetParser.Parse(code);

        Assert.Equal("a\nb\nc", result["fora"]);
        Assert.Equal("b", result["dentro"]);
    }

    [Fact]
    public void Regiao_sem_fechamento_e_ignorada()
    {
        Assert.Empty(SnippetParser.Parse("// snippet:aberta\nx\n"));
    }

    [Fact]
    public void Normaliza_quebras_de_linha_windows()
    {
        var result = SnippetParser.Parse("// snippet:a\r\n  x\r\n  y\r\n// /snippet:a\r\n");

        Assert.Equal("x\ny", result["a"]);
    }

    [Fact]
    public void Repositorio_rejeita_id_duplicado_entre_arquivos()
    {
        var repo = new SnippetRepository(
        [
            new FakeSnippetSource(
                new SnippetFile("a.xaml", "<!-- snippet:x -->\n1\n<!-- /snippet:x -->"),
                new SnippetFile("b.xaml", "<!-- snippet:x -->\n2\n<!-- /snippet:x -->")),
        ]);

        Assert.Throws<InvalidOperationException>(() => repo.Find("x"));
    }

    [Fact]
    public void Repositorio_devolve_nulo_para_id_desconhecido()
    {
        var repo = new SnippetRepository([new FakeSnippetSource(new SnippetFile("a.cs", "// snippet:y\n1\n// /snippet:y"))]);

        Assert.Equal("1", repo.Find("y"));
        Assert.Null(repo.Find("z"));
    }
}
