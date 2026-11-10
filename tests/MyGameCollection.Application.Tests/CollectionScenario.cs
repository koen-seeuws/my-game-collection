using MediatR;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Features.Items;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Domain.Enums;

namespace MyGameCollection.Application.Tests;

/// <summary>Seeds a small collection through the public requests.</summary>
internal sealed record CollectionScenario(Guid Snes, Guid MegaDrive, Guid SuperMetroid, Guid Sonic)
{
    public static async Task<CollectionScenario> SeedAsync(ISender sender)
    {
        var snes = await sender.Send(new CreatePlatformCommand(new PlatformEditModel { Name = "Super Nintendo", Manufacturer = "Nintendo", Aliases = "SNES" }));
        var md = await sender.Send(new CreatePlatformCommand(new PlatformEditModel { Name = "Mega Drive", Manufacturer = "Sega", Aliases = "Genesis, Sega Genesis" }));

        var metroid = await sender.Send(new CreateItemCommand(new ItemEditModel
        {
            Title = "Super Metroid", PlatformId = snes, HasBox = true, HasManual = true,
            Barcode = "045496830137", PurchasePrice = 80, Condition = ItemCondition.VeryGood
        }));
        await sender.Send(new CreateItemCommand(new ItemEditModel { Title = "Donkey Kong Country", PlatformId = snes, Region = Region.Pal, Condition = ItemCondition.Fair }));
        var sonic = await sender.Send(new CreateItemCommand(new ItemEditModel { Title = "Sonic the Hedgehog", PlatformId = md, Status = ItemStatus.Wishlist }));
        await sender.Send(new CreateItemCommand(new ItemEditModel
        {
            Type = ItemType.Console, Title = "Super Nintendo PAL", PlatformId = snes, Model = "SNSP-001A", EstimatedValue = 90
        }));

        return new CollectionScenario(snes, md, metroid, sonic);
    }

    public static Task<PagedResult<ItemListDto>> Search(ISender sender, ItemFilter filter) => sender.Send(new SearchItemsQuery(filter));
}
