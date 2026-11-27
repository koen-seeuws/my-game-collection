namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>How far the owner got with playing a game.</summary>
public enum PlayStatus
{
    /// <summary>Not played yet: still in the backlog.</summary>
    NotStarted,

    /// <summary>Being played right now.</summary>
    Playing,

    /// <summary>Started, then put aside for a while, with the plan to come back to it.</summary>
    OnHold,

    /// <summary>Started, then given up on.</summary>
    Dropped,

    /// <summary>Played to the end (e.g. the credits rolled).</summary>
    Completed,

    /// <summary>Everything done: all collectibles, achievements or trophies.</summary>
    HundredPercent
}
