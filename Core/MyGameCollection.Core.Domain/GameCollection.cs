using System.Collections.Immutable;
using MyGameCollection.Core.Domain.Abstractions;

namespace MyGameCollection.Core.Domain;

/// <summary>Everything one collector owns or wants: their games, consoles and accessories.</summary>
public record GameCollection : IAggregate<string>
{
    public required string AggregateKey { get; init; } // Filled in with the owner.
    public ImmutableList<Game> Games { get; set; } = [];
    public ImmutableList<GameConsole> Consoles { get; set; } = [];
    public ImmutableList<Accessory> Accessories { get; set; } = [];
}
