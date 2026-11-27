namespace MyGameCollection.Infrastructure.IGDB;

/// <summary>
/// IGDB API credentials: a Twitch application's client ID and secret (https://api-docs.igdb.com/#account-creation).
/// </summary>
public sealed class IgdbOptions
{
    public const string SectionName = "Igdb";

    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public Uri ApiBaseUrl { get; set; } = new("https://api.igdb.com/v4/");
    public Uri TokenUrl { get; set; } = new("https://id.twitch.tv/oauth2/token");

    /// <summary>How many results a search returns.</summary>
    public int SearchLimit { get; set; } = 12;

    /// <summary>
    /// How long a game's live details (rating, summary, time to beat, screenshots) are kept before asking IGDB again.
    /// They change slowly, and IGDB allows only a few requests per second.
    /// </summary>
    public TimeSpan DetailsCacheDuration { get; set; } = TimeSpan.FromDays(1);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
