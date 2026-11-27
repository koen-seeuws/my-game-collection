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

    public static string ToDisplayName(this PlayStatus status) => status switch
    {
        PlayStatus.NotStarted => "Not started",
        PlayStatus.Playing => "Playing",
        PlayStatus.OnHold => "On hold",
        PlayStatus.Dropped => "Dropped",
        PlayStatus.Completed => "Completed",
        _ => "100%"
    };

    /// <summary>An icon for each play status, for tags and filters.</summary>
    public static string ToIcon(this PlayStatus status) => status switch
    {
        PlayStatus.NotStarted => MudBlazor.Icons.Material.Outlined.HourglassEmpty,
        PlayStatus.Playing => MudBlazor.Icons.Material.Filled.PlayArrow,
        PlayStatus.OnHold => MudBlazor.Icons.Material.Filled.Pause,
        PlayStatus.Dropped => MudBlazor.Icons.Material.Filled.Block,
        PlayStatus.Completed => MudBlazor.Icons.Material.Filled.Flag,
        _ => MudBlazor.Icons.Material.Filled.EmojiEvents
    };

    /// <summary>Whether the game was played to the end (and so can have a completion date).</summary>
    public static bool IsFinished(this PlayStatus? status) =>
        status is PlayStatus.Completed or PlayStatus.HundredPercent;

    public static string ToDisplayName(this CollectionStatus status) => status switch
    {
        CollectionStatus.Owned => "Owned",
        _ => "Wishlist"
    };

    public static string ToDisplayName(this GameType type) => type switch
    {
        GameType.MainGame => "Main game",
        GameType.Remake => "Remake",
        GameType.Remaster => "Remaster",
        GameType.ExpandedGame => "Expanded edition",
        GameType.Port => "Port",
        GameType.Expansion => "Standalone expansion",
        GameType.Bundle => "Bundle",
        _ => "Other"
    };

    public static string ToDisplayName(this AddOnType type) => type switch
    {
        AddOnType.Dlc => "DLC",
        AddOnType.Expansion => "Expansion",
        AddOnType.Pack => "Pack",
        AddOnType.SeasonPass => "Season pass",
        _ => "Other"
    };

    public static string ToDisplayName(this AccessoryType type) => type switch
    {
        AccessoryType.Controller => "Controller",
        AccessoryType.Peripheral => "Peripheral",
        AccessoryType.Storage => "Memory & storage",
        AccessoryType.AudioVideo => "Audio & video",
        AccessoryType.CableOrPower => "Cable & power",
        AccessoryType.Figure => "Figure",
        _ => "Other"
    };

    /// <summary>An icon for each kind of accessory, for tags and placeholder thumbnails.</summary>
    public static string ToIcon(this AccessoryType type) => type switch
    {
        AccessoryType.Controller => MudBlazor.Icons.Material.Outlined.Gamepad,
        AccessoryType.Peripheral => MudBlazor.Icons.Material.Outlined.SettingsRemote,
        AccessoryType.Storage => MudBlazor.Icons.Material.Outlined.SdCard,
        AccessoryType.AudioVideo => MudBlazor.Icons.Material.Outlined.Headset,
        AccessoryType.CableOrPower => MudBlazor.Icons.Material.Outlined.Cable,
        AccessoryType.Figure => MudBlazor.Icons.Material.Outlined.Toys,
        _ => MudBlazor.Icons.Material.Outlined.Category
    };

    /// <summary>"1 add-on", "3 add-ons".</summary>
    public static string AddOns(int count) => count == 1 ? "1 add-on" : $"{count} add-ons";

    /// <summary>"1 accessory", "3 accessories".</summary>
    public static string Accessories(int count) => count == 1 ? "1 accessory" : $"{count} accessories";

    /// <summary>"Couch co-op · 4P": playing together on one console, for a game card.</summary>
    public static string LocalPlayersLabel(PlayerModes players) =>
        (players.CouchCoop ? "Couch co-op" : "Couch") + PlayerCount(players.MaxLocalPlayers);

    /// <summary>"Online · 8P": playing together online, for a game card.</summary>
    public static string OnlinePlayersLabel(PlayerModes players) =>
        (players.OnlineCoop ? "Online co-op" : "Online") + PlayerCount(players.MaxOnlinePlayers);

    /// <summary>"Up to 4 players on one console, co-op, split screen".</summary>
    public static string LocalPlayersDescription(PlayerModes players) =>
        string.Join(", ", new[]
        {
            players.MaxLocalPlayers is { } max ? $"Up to {max} players on one console" : "Multiplayer on one console",
            players.CouchCoop ? "co-op" : null,
            players.SplitScreen ? "split screen" : null
        }.Where(part => part is not null));

    /// <summary>"Up to 8 players online, co-op".</summary>
    public static string OnlinePlayersDescription(PlayerModes players) =>
        (players.MaxOnlinePlayers is { } max ? $"Up to {max} players online" : "Multiplayer online")
        + (players.OnlineCoop ? ", co-op" : "");

    private static string PlayerCount(int? max) => max is { } count ? $" · {count}P" : "";

    /// <summary>Label for any of the enums above (used by the generic <c>EnumSelect</c>).</summary>
    public static string Of<TEnum>(TEnum value) where TEnum : struct, Enum => value switch
    {
        Region r => r.ToDisplayName(),
        ItemCondition c => c.ToDisplayName(),
        Completeness c => c.ToDisplayName(),
        CollectionStatus s => s.ToDisplayName(),
        GameFormat f => f.ToDisplayName(),
        PurchaseOrigin o => o.ToDisplayName(),
        PlayStatus p => p.ToDisplayName(),
        GameType t => t.ToDisplayName(),
        AddOnType t => t.ToDisplayName(),
        AccessoryType t => t.ToDisplayName(),
        _ => value.ToString()
    };

    /// <summary>"45 min", or hours to the nearest half: "13 h", "12.5 h".</summary>
    public static string Hours(TimeSpan duration)
    {
        if (duration < TimeSpan.FromMinutes(45))
            return $"{Math.Max(1, (int)Math.Round(duration.TotalMinutes))} min";

        var hours = Math.Round(duration.TotalHours * 2, MidpointRounding.AwayFromZero) / 2;
        return $"{hours.ToString("0.#", CultureInfo.CurrentCulture)} h";
    }

    /// <summary>"1 rating", "2,102 ratings".</summary>
    public static string Count(int count, string singular) =>
        $"{count.ToString("N0", CultureInfo.CurrentCulture)} {(count == 1 ? singular : singular + "s")}";

    /// <summary>Formats an amount in the app's culture (see "Culture" in appsettings.json).</summary>
    public static string Money(decimal? amount) =>
        amount is null ? "–" : amount.Value.ToString("C", CultureInfo.CurrentCulture);

    public static string Date(DateOnly? date) =>
        date?.ToString("d", CultureInfo.CurrentCulture) ?? "–";
}
