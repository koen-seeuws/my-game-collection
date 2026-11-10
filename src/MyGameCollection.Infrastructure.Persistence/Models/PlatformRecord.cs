namespace MyGameCollection.Infrastructure.Persistence.Models;

/// <summary>Database shape of a platform. Mapped to/from the domain <c>Platform</c> by <c>PlatformMapper</c>.</summary>
public class PlatformRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    public List<string> Aliases { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<ItemRecord> Items { get; set; } = [];
}
