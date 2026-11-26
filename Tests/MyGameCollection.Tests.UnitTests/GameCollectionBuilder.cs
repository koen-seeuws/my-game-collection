using MyGameCollection.Core.Domain;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Tests.UnitTests;

internal static class GameCollectionBuilder
{
    public const string Owner = "collector@example.com";

    public static GameCollection Empty() => new() { AggregateKey = Owner };

    public static GameCollection WithGames(params Game[] games) => new() { AggregateKey = Owner, Games = [..games] };

    public static GameCollection WithConsoles(params GameConsole[] consoles) => new() { AggregateKey = Owner, Consoles = [..consoles] };

    public static GameDetails GameDetails(string title = "Super Mario World", string platform = "Super Nintendo",
        CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Title = title,
        Platform = platform,
        Status = status
    };

    public static Game Game(string title = "Super Mario World", CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Key = Guid.NewGuid(),
        Details = GameDetails(title, status: status)
    };

    public static ConsoleDetails ConsoleDetails(string name = "Super Nintendo", string manufacturer = "Nintendo",
        CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Name = name,
        Manufacturer = manufacturer,
        Status = status
    };

    public static GameConsole Console(string name = "Super Nintendo", CollectionStatus status = CollectionStatus.Owned) => new()
    {
        Key = Guid.NewGuid(),
        Details = ConsoleDetails(name, status: status)
    };
}
