namespace MyGameCollection.Infrastructure.ScanDex;

/// <summary>
/// ScanDex API access: an access token from a ScanDex developer account
/// (https://scandex.gamery.app/documentation/api/).
/// </summary>
public sealed class ScanDexOptions
{
    public const string SectionName = "ScanDex";

    public string? AccessToken { get; set; }

    public Uri ApiBaseUrl { get; set; } = new("https://scandex.gamery.app/api/v2/");

    public bool IsConfigured => !string.IsNullOrWhiteSpace(AccessToken);
}
