namespace MyGameCollection.Infrastructure.Persistence.Models;

/// <summary>Records whose timestamps are maintained by <see cref="AppDbContext"/>.</summary>
public interface IAuditableRecord
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
