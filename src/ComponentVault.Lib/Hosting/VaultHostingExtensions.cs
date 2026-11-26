using ComponentVault.Lib.Controls;
using LiveChartsCore;
using Microcharts.Maui;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp;
using MauiIcons.Fluent;
using Microsoft.Maui.Handlers;
using Sharpnado.Tabs;
using Syncfusion.Maui.Toolkit.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace ComponentVault.Lib.Hosting;

public static class VaultHostingExtensions
{
    /// <summary>
    /// Registra tudo de que a Lib precisa: fontes embutidas, bibliotecas de terceiros usadas nos
    /// previews e os ajustes de plataforma dos controles "sem borda" do design system.
    /// O app só precisa chamar isto (e UseMauiCommunityToolkit).
    /// </summary>
    public static MauiAppBuilder UseVaultDesignSystem(this MauiAppBuilder builder)
    {
        // Os arquivos .ttf entram no app como MauiFont (Resources/Fonts do App): fonte embutida
        // como recurso de biblioteca não carrega no Windows sem empacotamento.
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
            fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
            fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
            fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
            fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
        });

        builder
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseFluentMauiIcons()
            .UseSkiaSharp()
            .UseLiveCharts()
            .UseSharpnadoTabs(loggerEnable: false)
            // Syncfusion Toolkit (MIT): registra os handlers dos controles Sf*. Não pede chave de licença.
            .ConfigureSyncfusionToolkit()
            // Microcharts (MIT): gráficos simples desenhados com SkiaSharp.
            .UseMicrocharts();

        // A LiveCharts desenha texto com SkiaSharp e não enxerga as fontes do MAUI: sem isto,
        // eixos e dicas saem na fonte do sistema e destoam do resto do app.
        Typeface = LoadTypeface("PlusJakartaSans-Regular.ttf");
        if (Typeface is { } typeface)
        {
            LiveCharts.Configure(config => config.HasTextSettings(new TextSettings { DefaultTypeface = typeface }));
        }

        RemoveNativeChrome();
        return builder;
    }

    /// <summary>
    /// Plus Jakarta carregada para o SkiaSharp. As libs que desenham texto com Skia (LiveCharts,
    /// Microcharts) não enxergam as fontes do MAUI e precisam receber esta.
    /// </summary>
    public static SKTypeface? Typeface { get; private set; }

    // MauiFont vai para a pasta do app no Windows e para os assets no Android.
    private static SKTypeface? LoadTypeface(string file)
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, file);
            if (File.Exists(path)) return SKTypeface.FromFile(path);
#if ANDROID
            using var asset = Android.App.Application.Context.Assets?.Open(file);
            if (asset is null) return null;
            using var copy = new MemoryStream();
            asset.CopyTo(copy);
            copy.Position = 0;
            return SKTypeface.FromStream(copy);
#else
            return null;
#endif
        }
        catch (Exception)
        {
            // Sem a fonte, os gráficos seguem na fonte do sistema: não vale derrubar a abertura.
            return null;
        }
    }

    // Só afeta os controles Borderless*: os nativos continuam crus (é o que a variante Base mostra).
    private static void RemoveNativeChrome()
    {
        EntryHandler.Mapper.AppendToMapping("VaultBorderless", (handler, view) =>
        {
            if (view is BorderlessEntry) Strip(handler.PlatformView);
        });
        EditorHandler.Mapper.AppendToMapping("VaultBorderless", (handler, view) =>
        {
            if (view is BorderlessEditor) Strip(handler.PlatformView);
        });
        PickerHandler.Mapper.AppendToMapping("VaultBorderless", (handler, view) =>
        {
            if (view is BorderlessPicker) Strip(handler.PlatformView);
        });
#if WINDOWS
        // No Windows o Title do Picker vira cabeçalho acima de uma caixa vazia; no campo do design
        // system ele deve ser o texto de exemplo dentro da caixa, como nas outras plataformas.
        PickerHandler.Mapper.AppendToMapping(nameof(Picker.Title), (handler, view) =>
        {
            if (view is not BorderlessPicker) return;
            handler.PlatformView.Header = null;
            handler.PlatformView.PlaceholderText = view.Title ?? string.Empty;
        });
#endif
        DatePickerHandler.Mapper.AppendToMapping("VaultBorderless", (handler, view) =>
        {
            if (view is BorderlessDatePicker) Strip(handler.PlatformView);
        });
        TimePickerHandler.Mapper.AppendToMapping("VaultBorderless", (handler, view) =>
        {
            // O TimePickerField da UraniumUI usa o TimePicker nativo por dentro: no Windows ele
            // desenhava caixas cinzas atrás de hora e minuto, destoando do campo de data ao lado.
            if (view is BorderlessTimePicker || IsInside(view as Element, "TimePickerField")) Strip(handler.PlatformView);
        });
    }

    private static bool IsInside(Element? element, string typeName)
    {
        for (var current = element?.Parent; current is not null; current = current.Parent)
        {
            if (current.GetType().Name == typeName) return true;
        }
        return false;
    }

#if ANDROID
    private static void Strip(Android.Views.View view)
    {
        view.Background = null;
        view.SetPadding(0, 0, 0, 0);
    }
#elif WINDOWS
    private static readonly string[] TransparentKeys =
    [
        "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused",
        "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused",
        "ComboBoxBackground", "ComboBoxBackgroundPointerOver", "ComboBoxBackgroundPressed", "ComboBoxBackgroundFocused",
        "ComboBoxBorderBrush", "ComboBoxBorderBrushPointerOver", "ComboBoxBorderBrushPressed", "ComboBoxBorderBrushFocused",
        // Desabilitado: sem isto o ComboBox pinta um retângulo próprio dentro da moldura do design system.
        "ComboBoxBackgroundDisabled", "ComboBoxBorderBrushDisabled",
        "CalendarDatePickerBackground", "CalendarDatePickerBackgroundPointerOver", "CalendarDatePickerBackgroundPressed",
        "CalendarDatePickerBorderBrush", "CalendarDatePickerBorderBrushPointerOver", "CalendarDatePickerBorderBrushPressed",
        "TimePickerButtonBackground", "TimePickerButtonBackgroundPointerOver", "TimePickerButtonBackgroundPressed",
        "TimePickerButtonBorderBrush", "TimePickerButtonBorderBrushPointerOver", "TimePickerButtonBorderBrushPressed",
        // Chevron nativo do ComboBox: o campo do design system desenha o seu.
        "ComboBoxDropDownGlyphForeground", "ComboBoxDropDownGlyphForegroundFocused",
        "ComboBoxDropDownGlyphForegroundDisabled", "ComboBoxDropDownGlyphForegroundFocusedPressed",
        // Ícone de calendário nativo: o campo do design system já mostra o seu à esquerda.
        "CalendarDatePickerCalendarGlyphForeground", "CalendarDatePickerCalendarGlyphForegroundPointerOver",
        "CalendarDatePickerCalendarGlyphForegroundPressed", "CalendarDatePickerCalendarGlyphForegroundDisabled",
    ];

    private static void Strip(Microsoft.UI.Xaml.Controls.Control view)
    {
        var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        view.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        view.Background = transparent;
        foreach (var key in TransparentKeys)
        {
            // Alguns controles recusam certas chaves de tema depois de montados (o CalendarDatePicker
            // lançava COMException e derrubava o app). Chave recusada fica com o valor nativo.
            try
            {
                view.Resources[key] = transparent;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }
        }

        // O espaço interno nativo (~10px) empurrava o texto para dentro do campo do design system,
        // que já tem o seu. A largura mínima nativa fica: o TimePicker precisa de ~242px, e espremido
        // ele corta os dígitos ("9" e "0" em vez de 09:00). Quem usa dá a ele a coluna de que precisa.
        view.Padding = new Microsoft.UI.Xaml.Thickness(0);

        switch (view)
        {
            // O ícone de calendário nativo não obedece às chaves de cor: some pelo nome no template.
            case Microsoft.UI.Xaml.Controls.CalendarDatePicker calendar:
                calendar.Loaded += (_, _) => Collapse(calendar, "CalendarGlyph");
                break;
            // O MAUI volta a pôr o Title como cabeçalho depois do mapeamento; ao carregar, ele sai
            // e fica só o texto de exemplo dentro da caixa (ver o mapeamento de Title).
            case Microsoft.UI.Xaml.Controls.ComboBox combo:
                combo.Loaded += (_, _) => combo.Header = null;
                break;
        }
    }

    private static void Collapse(Microsoft.UI.Xaml.DependencyObject root, string name)
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is Microsoft.UI.Xaml.FrameworkElement { Name: var childName } element && childName == name)
            {
                element.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            Collapse(child, name);
        }
    }
#else
    private static void Strip(object view)
    {
    }
#endif
}
