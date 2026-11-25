using ComponentVault.Lib.Theming;
using MauiIcons.Core;
using MauiIcons.Fluent;

namespace ComponentVault.Lib.Showcase.Icons;

[ShowcasePreview("icons.base")]
public partial class IconsBasePreview : ContentView
{
    public IconsBasePreview() => InitializeComponent();
}

[ShowcasePreview("icons.mauiicons")]
public partial class IconsMauiIconsPreview : ContentView
{
    private static readonly FluentIcons[] Sample =
    [
        FluentIcons.Home24, FluentIcons.Search24, FluentIcons.Settings24, FluentIcons.Person24,
        FluentIcons.Mail24, FluentIcons.Chat24, FluentIcons.CalendarLtr24, FluentIcons.Clock24,
        FluentIcons.Folder24, FluentIcons.Document24, FluentIcons.Image24, FluentIcons.Camera24,
        FluentIcons.Share24, FluentIcons.Copy24, FluentIcons.Link24, FluentIcons.Bookmark24,
        FluentIcons.Cart24, FluentIcons.Wallet24, FluentIcons.Location24, FluentIcons.Globe24,
        FluentIcons.Cloud24, FluentIcons.ShieldCheckmark24, FluentIcons.Key24, FluentIcons.Rocket24,
    ];

    public IconsMauiIconsPreview()
    {
        InitializeComponent();

        foreach (var icon in Sample)
        {
            var glyph = new MauiIcon { Icon = icon, IconSize = 24, HorizontalOptions = LayoutOptions.Center };
            glyph.SetThemeColor(MauiIcon.IconColorProperty, "Ink");

            var name = new Label
            {
                Text = icon.ToString().TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9'),
                FontFamily = "JakartaRegular",
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
            };
            name.SetThemeColor(Label.TextColorProperty, "InkTertiary");

            var tile = new Border
            {
                WidthRequest = 96,
                Padding = new Thickness(6, 14, 6, 10),
                Margin = new Thickness(0, 0, 8, 8),
                StrokeThickness = 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = new VerticalStackLayout { Spacing = 8, Children = { glyph, name } },
            };
            tile.SetThemeBrush(Border.StrokeProperty, "Hairline");
            SemanticProperties.SetDescription(tile, icon.ToString());
            Gallery.Children.Add(tile);
        }
    }
}
