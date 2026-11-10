namespace MyGameCollection.Infrastructure.Metadata;

public sealed class RawgOptions
{
    public const string SectionName = "Rawg";

    /// <summary>Free API key from https://rawg.io/apidocs. Leave empty to disable game metadata lookups.</summary>
    public string? ApiKey { get; set; }
}
