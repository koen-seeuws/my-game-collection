namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>How complete a collection item is, in collector terms.</summary>
public enum Completeness
{
    /// <summary>New, still in its original shrink wrap.</summary>
    Sealed,

    /// <summary>Complete in box: box, manual and the item itself.</summary>
    CompleteInBox,

    /// <summary>Boxed, but missing the manual (or other inserts).</summary>
    Boxed,

    /// <summary>Just the cartridge, disc or console.</summary>
    Loose
}
