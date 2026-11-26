namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>The video standard / market a game or console was released for.</summary>
/// <remarks>
/// Part of the shared domain vocabulary: the application contracts and the UI use these values too,
/// which is why they live next to the other domain abstractions.
/// </remarks>
public enum Region
{
    Pal,
    NtscU,
    NtscJ,
    RegionFree,
    Other
}
