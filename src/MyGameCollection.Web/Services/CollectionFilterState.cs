using MyGameCollection.Application.Abstractions.ReadModels;

namespace MyGameCollection.Web.Services;

/// <summary>Remembers the collection filters for the duration of the session (circuit), so going back keeps them.</summary>
public sealed class CollectionFilterState
{
    public ItemFilter Filter { get; set; } = new() { PageSize = 24 };
    public bool GridView { get; set; } = true;
}
