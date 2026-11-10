using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Domain.Entities;

/// <summary>A piece of console hardware (named GameConsole to avoid clashing with System.Console).</summary>
public class GameConsole : CollectionItem
{
    public GameConsole() { }
    public GameConsole(Guid id) : base(id) { }

    public override ItemType Type => ItemType.Console;

    /// <summary>Hardware model / revision, e.g. "SCPH-1002" or "Model 2".</summary>
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    /// <summary>Colour or special edition, e.g. "Crystal Red".</summary>
    public string? Edition { get; set; }
}
