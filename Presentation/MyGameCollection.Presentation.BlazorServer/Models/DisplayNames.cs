using System.Globalization;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Presentation.BlazorServer.Models;

/// <summary>Human-friendly labels for the domain vocabulary.</summary>
public static class DisplayNames
{
    public static string ToDisplayName(this Region region) => region switch
    {
        Region.Pal => "PAL",
        Region.NtscU => "NTSC-U",
        Region.NtscJ => "NTSC-J",
        Region.RegionFree => "Region free",
        _ => "Other"
    };

    public static string ToDisplayName(this ItemCondition condition) => condition switch
    {
        ItemCondition.Mint => "Mint",
        ItemCondition.NearMint => "Near mint",
        ItemCondition.VeryGood => "Very good",
        ItemCondition.Good => "Good",
        ItemCondition.Fair => "Fair",
        _ => "Poor"
    };

    public static string ToDisplayName(this Completeness completeness) => completeness switch
    {
        Completeness.Sealed => "Sealed",
        Completeness.CompleteInBox => "Complete in box",
        Completeness.Boxed => "Boxed, no manual",
        _ => "Loose"
    };

    public static string ToDisplayName(this GameFormat format) => format switch
    {
        GameFormat.Physical => "Physical",
        GameFormat.Digital => "Digital",
        _ => "Subscription"
    };

    /// <summary>What a format means, for the game form.</summary>
    public static string ToDescription(this GameFormat format) => format switch
    {
        GameFormat.Physical => "A cartridge, disc or card",
        GameFormat.Digital => "A download you bought and keep",
        _ => "Claimed through a subscription like PS Plus; playable while subscribed"
    };

    public static string ToDisplayName(this PurchaseOrigin origin) => origin switch
    {
        PurchaseOrigin.New => "New",
        _ => "Secondhand"
    };

    public static string ToDisplayName(this CollectionStatus status) => status switch
    {
        CollectionStatus.Owned => "Owned",
        _ => "Wishlist"
    };

    /// <summary>Label for any of the enums above (used by the generic <c>EnumSelect</c>).</summary>
    public static string Of<TEnum>(TEnum value) where TEnum : struct, Enum => value switch
    {
        Region r => r.ToDisplayName(),
        ItemCondition c => c.ToDisplayName(),
        Completeness c => c.ToDisplayName(),
        CollectionStatus s => s.ToDisplayName(),
        GameFormat f => f.ToDisplayName(),
        PurchaseOrigin o => o.ToDisplayName(),
        _ => value.ToString()
    };

    /// <summary>Formats an amount in the app's culture (see "Culture" in appsettings.json).</summary>
    public static string Money(decimal? amount) =>
        amount is null ? "–" : amount.Value.ToString("C", CultureInfo.CurrentCulture);

    public static string Date(DateOnly? date) =>
        date?.ToString("d", CultureInfo.CurrentCulture) ?? "–";
}
