using ComponentVault.Lib.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp;
using MauiIcons.Fluent;
using Microsoft.Maui.Handlers;
using Sharpnado.Tabs;
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
            .UseSharpnadoTabs(loggerEnable: false);

        // A LiveCharts desenha texto com SkiaSharp e não enxerga as fontes do MAUI: sem isto,
        // eixos e dicas saem na fonte do sistema e destoam do resto do app.
        if (LoadTypeface("PlusJakartaSans-Regular.ttf") is { } typeface)
        {
            LiveCharts.Configure(config => config.HasTextSettings(new TextSettings { DefaultTypeface = typeface }));
        }

        RemoveNativeChrome();
        return builder;
    }

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
        "CalendarDatePickerBackground", "CalendarDatePickerBackgroundPointerOver", "CalendarDatePickerBackgroundPressed",
        "CalendarDatePickerBorderBrush", "CalendarDatePickerBorderBrushPointerOver", "CalendarDatePickerBorderBrushPressed",
        "TimePickerButtonBackground", "TimePickerButtonBackgroundPointerOver", "TimePickerButtonBackgroundPressed",
        "TimePickerButtonBorderBrush", "TimePickerButtonBorderBrushPointerOver", "TimePickerButtonBorderBrushPressed",
    ];

    private static void Strip(Microsoft.UI.Xaml.Controls.Control view)
    {
        var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
        view.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        view.Background = transparent;
        foreach (var key in TransparentKeys) view.Resources[key] = transparent;
    }
#else
    private static void Strip(object view)
    {
    }
#endif
}
