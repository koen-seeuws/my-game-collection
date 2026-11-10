namespace MyGameCollection.Application.Abstractions.ReadModels;

public sealed record PlatformDto(Guid Id, string Name, string Manufacturer, int? ReleaseYear, IReadOnlyList<string> Aliases, int GameCount, int ConsoleCount);
