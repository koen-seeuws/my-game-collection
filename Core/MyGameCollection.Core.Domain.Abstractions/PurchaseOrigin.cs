namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>Whether an item was bought new (first owner) or secondhand.</summary>
public enum PurchaseOrigin
{
    /// <summary>Bought new, from a shop.</summary>
    New,

    /// <summary>Bought used, e.g. from a flea market, a retro shop or an online marketplace.</summary>
    Secondhand
}
