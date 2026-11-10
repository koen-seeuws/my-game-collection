using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Domain.Entities;

public class Game : CollectionItem
{
    public Game() { }
    public Game(Guid id) : base(id) { }

    public override ItemType Type => ItemType.Game;

    public string? Publisher { get; set; }
    public string? Developer { get; set; }
    public string? Genre { get; set; }
    public int? ReleaseYear { get; set; }
}
