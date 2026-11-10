using MudBlazor;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Web.Services;

/// <summary>Display texts and icons for domain enums.</summary>
public static class Labels
{
    public static string ToLabel(this Region region) => region switch
    {
        Region.Pal => "PAL",
        Region.NtscU => "NTSC-U",
        Region.NtscJ => "NTSC-J",
        Region.RegionFree => "Region free",
        Region.Other => "Other",
        _ => "Unknown"
    };

    public static string ToLabel(this ItemCondition condition) => condition switch
    {
        ItemCondition.NearMint => "Near mint",
        ItemCondition.VeryGood => "Very good",
        ItemCondition.ForParts => "For parts",
        _ => condition.ToString()
    };

    public static string ToLabel(this ItemStatus status) => status switch
    {
        ItemStatus.ForSale => "For sale",
        _ => status.ToString()
    };

    public static string ToLabel(this ItemType type) => type == ItemType.Console ? "Console" : "Game";

    public static string ToIcon(this ItemType type) =>
        type == ItemType.Console ? Icons.Material.Filled.VideogameAsset : Icons.Material.Filled.SportsEsports;

    public static Color ToColor(this ItemStatus status) => status switch
    {
        ItemStatus.Wishlist => Color.Tertiary,
        ItemStatus.ForSale => Color.Warning,
        ItemStatus.Sold => Color.Default,
        _ => Color.Success
    };

    public static Color ToColor(this ItemCondition condition) => condition switch
    {
        ItemCondition.Mint or ItemCondition.NearMint => Color.Success,
        ItemCondition.VeryGood or ItemCondition.Good => Color.Info,
        ItemCondition.Fair => Color.Warning,
        ItemCondition.Poor or ItemCondition.ForParts => Color.Error,
        _ => Color.Default
    };

    public static string Completeness(bool hasBox, bool hasManual, bool isSealed) => isSealed
        ? "Sealed"
        : (hasBox, hasManual) switch
        {
            (true, true) => "CIB",
            (true, false) => "Boxed",
            (false, true) => "Loose + manual",
            _ => "Loose"
        };

    public static string Money(decimal? value) => value is { } v ? v.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("nl-BE")) : "–";
}
