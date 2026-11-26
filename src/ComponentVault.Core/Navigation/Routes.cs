namespace ComponentVault.Core.Navigation;

public static class Routes
{
    // A Home é a única raiz; Catálogo e Detalhe são empilhados sobre ela. Abas do Shell escondidas
    // não trocam de conteúdo no Windows, e a navegação visível é a barra flutuante do app.
    public const string Home = "//home";
    public const string Catalog = "catalog";
    public const string ComponentDetail = "component";

    /// <summary>Catálogo a partir de qualquer ponto: desempilha o que estiver por cima dele.</summary>
    public const string CatalogRoot = "//home/catalog";

    public static class Params
    {
        public const string ComponentId = "id";
        public const string VariantId = "variant";

        /// <summary>"true" abre o detalhe já no modo Apresentar.</summary>
        public const string Present = "present";

        /// <summary>Aba inicial do detalhe: preview, compare, docs ou code.</summary>
        public const string Tab = "tab";
    }
}
