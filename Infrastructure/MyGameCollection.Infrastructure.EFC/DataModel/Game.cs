using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class Game : CollectionItem
{
    public required string Title { get; set; }
    public required string Platform { get; set; }
    public string? Edition { get; set; }
    public GameFormat Format { get; set; }

    // Player modes (see Domain.Abstractions.PlayerModes); all false / empty when unknown.
    public bool SinglePlayer { get; set; }
    public bool Multiplayer { get; set; }
    public int? MaxLocalPlayers { get; set; }
    public int? MaxOnlinePlayers { get; set; }
    public bool CouchCoop { get; set; }
    public bool OnlineCoop { get; set; }
    public bool SplitScreen { get; set; }
}
