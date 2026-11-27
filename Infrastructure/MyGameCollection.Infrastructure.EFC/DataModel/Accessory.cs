using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Infrastructure.EFC.DataModel;

public class Accessory : CollectionItem
{
    public required string Name { get; set; }
    public AccessoryType Type { get; set; }
    public string? Platform { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Colour { get; set; }
    public int Quantity { get; set; } = 1;
}
