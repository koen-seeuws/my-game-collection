using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

/// <summary>The columns games and consoles have in common: they belong to a collection directly.</summary>
public abstract class CollectionItem : ItemRow
{
    public Region Region { get; set; }

    public int GameCollectionId { get; set; }
    public GameCollection GameCollection { get; set; } = null!;
}
