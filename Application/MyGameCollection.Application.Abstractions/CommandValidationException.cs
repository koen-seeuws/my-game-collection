namespace MyGameCollection.Application.Abstractions;

/// <summary>A command was rejected before it was handled because its input is invalid.</summary>
/// <remarks>An <see cref="ArgumentException"/>, so callers can treat it like the domain's own input errors.</remarks>
public sealed class CommandValidationException : ArgumentException
{
    public CommandValidationException(IReadOnlyList<string> errors)
        : base(string.Join(" ", errors))
    {
        Errors = errors;
    }

    /// <summary>One user-facing message per problem.</summary>
    public IReadOnlyList<string> Errors { get; }
}
