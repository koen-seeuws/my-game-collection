using MudBlazor;

namespace MyGameCollection.Web.Services;

public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6a3de8",
            Secondary = "#00a2b8",
            Tertiary = "#e8457c",
            AppbarBackground = "#2a1f4a",
            AppbarText = "#ffffff",
            Background = "#f6f4fb",
            DrawerBackground = "#ffffff",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#9d7bff",
            Secondary = "#33e1f5",
            Tertiary = "#ff6b9d",
            AppbarBackground = "#1a1530",
            AppbarText = "#ffffff",
            Background = "#110e1f",
            Surface = "#1c1832",
            DrawerBackground = "#16122a",
            DrawerText = "rgba(255,255,255,0.80)",
            TextPrimary = "rgba(255,255,255,0.90)",
            TextSecondary = "rgba(255,255,255,0.62)",
            LinesDefault = "rgba(255,255,255,0.10)",
            TableLines = "rgba(255,255,255,0.08)",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"]
            }
        }
    };
}
