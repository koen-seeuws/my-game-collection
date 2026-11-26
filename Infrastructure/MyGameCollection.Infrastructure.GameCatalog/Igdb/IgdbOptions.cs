namespace MyGameCollection.Infrastructure.GameCatalog.Igdb;

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

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
