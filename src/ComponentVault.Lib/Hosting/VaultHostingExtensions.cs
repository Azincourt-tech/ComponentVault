using ComponentVault.Lib.Controls;
using LiveChartsCore.SkiaSharpView.Maui;
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

        RemoveNativeChrome();
        return builder;
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
            if (view is BorderlessTimePicker) Strip(handler.PlatformView);
        });
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
