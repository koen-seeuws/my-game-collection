namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class GameCollection
{
    public int Id { get; set; }

    /// <summary>The domain key of the aggregate (unique).</summary>
    public required string Owner { get; set; }

    public List<Game> Games { get; set; } = [];
    public List<GameConsole> Consoles { get; set; } = [];
}
