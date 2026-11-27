namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class GameConsole : CollectionItem
{
    public required string Name { get; set; }
    public required string Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Colour { get; set; }
}
