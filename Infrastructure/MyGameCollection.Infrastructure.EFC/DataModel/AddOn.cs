using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

/// <summary>An add-on (DLC, expansion...) of a game; it goes when its game goes.</summary>
public class AddOn : ItemRow
{
    public required string Title { get; set; }
    public AddOnType Type { get; set; }
    public GameFormat Format { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
}
