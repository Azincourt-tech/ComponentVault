using ComponentVault.Core.Models;
using ComponentVault.Core.Services.Preferences;
using ComponentVault.Core.Services.Theming;
using ComponentVault.Tests.Fakes;

namespace ComponentVault.Tests.Services;

public class ThemeServiceTests
{
    [Fact]
    public async Task Sem_preferencia_salva_usa_o_tema_do_sistema()
    {
        var applier = new FakeThemeApplier();
        var service = new ThemeService(new InMemoryPreferenceStore(), applier);

        await service.InitializeAsync();

        Assert.Equal(AppThemePreference.System, service.Current);
        Assert.Equal([AppThemePreference.System], applier.Applied);
    }

    [Fact]
    public async Task Alternar_aplica_na_hora_persiste_e_avisa()
    {
        var store = new InMemoryPreferenceStore();
        var applier = new FakeThemeApplier();
        var service = new ThemeService(store, applier);
        AppThemePreference? notified = null;
        service.ThemeChanged += (_, t) => notified = t;

        await service.ToggleAsync();

        Assert.Equal(AppThemePreference.Dark, service.Current);
        Assert.Equal(AppThemePreference.Dark, applier.Applied[^1]);
        Assert.Equal(AppThemePreference.Dark, notified);
        Assert.Equal("Dark", store.Values[ThemeService.PreferenceKey]);

        await service.ToggleAsync();

        Assert.Equal(AppThemePreference.Glass, service.Current);

        await service.ToggleAsync();

        Assert.Equal(AppThemePreference.Light, service.Current);
    }

    [Fact]
    public async Task Seletor_de_aparencia_marca_o_tema_escolhido()
    {
        var service = new ThemeService(new InMemoryPreferenceStore(), new FakeThemeApplier());
        var appearance = new Core.ViewModels.AppearanceViewModel(service);

        await appearance.SelectCommand.ExecuteAsync(appearance.Options.Single(o => o.Preference == AppThemePreference.Glass));

        Assert.Equal(AppThemePreference.Glass, service.Current);
        Assert.Single(appearance.Options, o => o.IsSelected);
        Assert.True(appearance.Options.Single(o => o.Preference == AppThemePreference.Glass).IsSelected);
    }

    [Fact]
    public async Task Em_sistema_o_seletor_marca_o_tema_que_o_SO_escolheu()
    {
        var service = new ThemeService(new InMemoryPreferenceStore(), new FakeThemeApplier { SystemIsDark = true });
        var appearance = new Core.ViewModels.AppearanceViewModel(service);

        await service.InitializeAsync();

        Assert.Equal(AppThemePreference.System, service.Current);
        Assert.Equal(AppThemePreference.Dark, service.Visible);
        Assert.Equal(AppThemePreference.Dark, appearance.Selected.Preference);
        Assert.Single(appearance.Options, o => o.IsSelected);
    }

    [Fact]
    public async Task Preview_aplica_e_avisa_sem_salvar()
    {
        var store = new InMemoryPreferenceStore();
        var applier = new FakeThemeApplier();
        var service = new ThemeService(store, applier);
        var appearance = new Core.ViewModels.AppearanceViewModel(service);
        await service.InitializeAsync();

        service.Preview(AppThemePreference.Glass);

        Assert.Equal(AppThemePreference.Glass, applier.Applied[^1]);
        Assert.Equal(AppThemePreference.Glass, appearance.Selected.Preference);
        Assert.False(store.Values.ContainsKey(ThemeService.PreferenceKey));
    }

    [Fact]
    public async Task Alternar_parte_do_tema_visivel_quando_a_preferencia_e_sistema()
    {
        var applier = new FakeThemeApplier { SystemIsDark = true };
        var service = new ThemeService(new InMemoryPreferenceStore(), applier);
        await service.InitializeAsync();

        await service.ToggleAsync();

        // Sistema escuro → visível é Escuro → o próximo do ciclo é Vidro.
        Assert.Equal(AppThemePreference.Glass, service.Current);
    }

    [Fact]
    public async Task Valor_invalido_salvo_cai_no_tema_do_sistema()
    {
        var store = new InMemoryPreferenceStore();
        store.Values[ThemeService.PreferenceKey] = "Roxo";
        var service = new ThemeService(store, new FakeThemeApplier());

        await service.InitializeAsync();

        Assert.Equal(AppThemePreference.System, service.Current);
    }

    [Fact]
    public async Task Tema_sobrevive_a_reabertura_do_app_com_sqlite_real()
    {
        using var temp = new TempDirectory();

        await using (var store = new SqlitePreferenceStore(temp))
        {
            await new ThemeService(store, new FakeThemeApplier()).SetThemeAsync(AppThemePreference.Dark);
        }

        await using (var reopened = new SqlitePreferenceStore(temp))
        {
            var applier = new FakeThemeApplier();
            var service = new ThemeService(reopened, applier);
            await service.InitializeAsync();

            Assert.Equal(AppThemePreference.Dark, service.Current);
            Assert.Equal(AppThemePreference.Dark, applier.Applied[^1]);
        }
    }
}
