using ComponentVault.App.Views.Catalog;
using ComponentVault.App.Views.Detail;
using ComponentVault.Core.Navigation;

namespace ComponentVault.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas empilhadas sobre a Home (ADR-004). Parâmetros chegam via IQueryAttributable.
        Routing.RegisterRoute(Routes.Catalog, typeof(CatalogPage));
        Routing.RegisterRoute(Routes.ComponentDetail, typeof(ComponentDetailPage));
    }
}
