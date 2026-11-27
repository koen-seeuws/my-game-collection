using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Tests.UnitTests;

internal static class GameCollectionBuilder
{
    public const string Owner = "collector@example.com";

    public static GameCollection Empty() => new() { AggregateKey = Owner };

    public static GameCollection WithGames(params Game[] games) => new() { AggregateKey = Owner, Games = [..games] };

    public static GameCollection WithConsoles(params GameConsole[] consoles) => new() { AggregateKey = Owner, Consoles = [..consoles] };

    public static Game Game(string title = "Super Mario World", CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Key = Guid.NewGuid(),
        Title = title,
        Platform = "Super Nintendo",
        Status = status
    };

    public static GameConsole Console(string name = "Super Nintendo", CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Key = Guid.NewGuid(),
        Name = name,
        Manufacturer = "Nintendo",
        Status = status
    };
}
