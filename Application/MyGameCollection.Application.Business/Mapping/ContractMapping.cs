using MyGameCollection.Application.Abstractions.Contracts;
using MyGameCollection.Core.Domain;

namespace MyGameCollection.Application.Business.Mapping;

internal static class ContractMapping
{
    public static GameDetails ToDomain(this GameData data) => new()
    {
        Title = data.Title,
        Platform = data.Platform,
        Region = data.Region,
        Condition = data.Condition,
        Completeness = data.Completeness,
        Status = data.Status,
        ReleaseYear = data.ReleaseYear,
        PurchaseDate = data.PurchaseDate,
        PurchasePrice = data.PurchasePrice,
        Notes = data.Notes
    };

    public static ConsoleDetails ToDomain(this ConsoleData data) => new()
    {
        Name = data.Name,
        Manufacturer = data.Manufacturer,
        Model = data.Model,
        Region = data.Region,
        Condition = data.Condition,
        Completeness = data.Completeness,
        Status = data.Status,
        ReleaseYear = data.ReleaseYear,
        PurchaseDate = data.PurchaseDate,
        PurchasePrice = data.PurchasePrice,
        Notes = data.Notes
    };
}
