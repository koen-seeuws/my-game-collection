using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class Game : CollectionItem
{
    public required string Title { get; set; }
    public required string Platform { get; set; }
    public string? Edition { get; set; }
    public GameFormat Format { get; set; }
}
