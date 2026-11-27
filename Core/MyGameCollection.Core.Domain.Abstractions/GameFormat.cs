namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>How a game is owned: as a physical copy, as a bought download, or through a subscription.</summary>
public enum GameFormat
{
    /// <summary>A cartridge, disc or card.</summary>
    Physical,

    /// <summary>A bought digital copy, yours to keep.</summary>
    Digital,

    /// <summary>A digital copy claimed through a subscription such as PS Plus; playable only while subscribed.</summary>
    Subscription
}
