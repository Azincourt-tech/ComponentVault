using ComponentVault.App.Views.Detail;
using ComponentVault.Core.Navigation;

namespace ComponentVault.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas empilhadas sobre as abas (ADR-004). Parâmetros chegam via IQueryAttributable.
        Routing.RegisterRoute(Routes.ComponentDetail, typeof(ComponentDetailPage));
    }
}
