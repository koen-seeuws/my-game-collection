namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>
/// Settings for the UPCitemdb product database. Without a key the free trial endpoint is used (100 lookups a day).
/// </summary>
public sealed class BarcodeLookupOptions
{
    public const string SectionName = "BarcodeLookup";

    public Uri UpcItemDbBaseUrl { get; set; } = new("https://api.upcitemdb.com/prod/");

    /// <summary>Optional paid-plan key; switches from the trial to the v1 endpoint.</summary>
    public string? UpcItemDbUserKey { get; set; }
}
