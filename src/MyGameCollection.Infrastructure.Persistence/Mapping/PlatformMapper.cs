using MyGameCollection.Domain.Entities;
using MyGameCollection.Infrastructure.Persistence.Models;

namespace MyGameCollection.Infrastructure.Persistence.Mapping;

internal static class PlatformMapper
{
    public static Platform ToDomain(PlatformRecord record)
    {
        var platform = new Platform(record.Id, record.Name, record.Manufacturer, record.ReleaseYear, record.Aliases);
        platform.RestoreAudit(record.CreatedAt, record.UpdatedAt);
        return platform;
    }

    public static PlatformRecord ToRecord(Platform platform) => Apply(platform, new PlatformRecord { Id = platform.Id });

    public static PlatformRecord Apply(Platform platform, PlatformRecord record)
    {
        record.Name = platform.Name;
        record.Manufacturer = platform.Manufacturer;
        record.ReleaseYear = platform.ReleaseYear;
        record.Aliases = platform.Aliases.ToList();
        return record;
    }
}
