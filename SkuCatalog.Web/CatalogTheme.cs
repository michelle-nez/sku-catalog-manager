using MudBlazor;

namespace SkuCatalog.Web;

/// <summary>
/// This app's own look: indigo primary, soft elevated surfaces, comfortable
/// spacing. Each app in the portfolio gets a distinct theme so they do not all
/// read as the same template with different words in it.
/// </summary>
public static class CatalogTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#3f51b5",
            Secondary = "#5c6bc0",
            Tertiary = "#7986cb",
            Info = "#3f51b5",
            Success = "#2e7d32",
            Warning = "#ed6c02",
            Error = "#c62828",
            Background = "#f4f5fa",
            Surface = "#ffffff",
            AppbarBackground = "#3f51b5",
            AppbarText = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#3c4257",
            DrawerIcon = "#5c6bc0",
            TextPrimary = "#26282f",
            TextSecondary = "#5f6c7b",
            Divider = "#e4e7ee",
            TableLines = "#eceef4"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Roboto", "Segoe UI", "sans-serif"], FontSize = "0.9rem" },
            H1 = new H1Typography { FontSize = "2rem", FontWeight = "600" },
            H5 = new H5Typography { FontSize = "1.25rem", FontWeight = "600" },
            H6 = new H6Typography { FontSize = "1.05rem", FontWeight = "600" },
            Subtitle1 = new Subtitle1Typography { FontSize = "0.95rem" },
            Button = new ButtonTypography { TextTransform = "none", FontWeight = "500" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "245px"
        }
    };
}
