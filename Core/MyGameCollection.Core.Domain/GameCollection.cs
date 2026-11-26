using System.Collections.Immutable;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>Everything one collector owns or wants: their games and their consoles.</summary>
public record GameCollection : IAggregate<string>
{
    public required string AggregateKey { get; init; } // Filled in with the owner.
    public ImmutableList<Game> Games { get; set; } = [];
    public ImmutableList<GameConsole> Consoles { get; set; } = [];
}
