namespace MyGameCollection.Application.Abstractions.Commands.RedeemInvite;

/// <summary>Creates an account with an invite link, which can then no longer be used. Needs no signed-in user.</summary>
public record RedeemInviteCommand : ICommandInterfaces<RedeemInviteResultDto>
{
    public const int MaxNameLength = 100;

    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    /// <summary>Signs the user in; must be unique.</summary>
    public required string Email { get; init; }

    public required string Token { get; init; }
    public required string Password { get; init; }
}

/// <summary>The created account's id, or why no account was created (e.g. the email is already taken).</summary>
public record RedeemInviteResultDto(string? UserId, IReadOnlyList<string> Errors)
{
    public bool Succeeded => UserId is not null;

    public static RedeemInviteResultDto Success(string userId) => new(userId, []);
    public static RedeemInviteResultDto Failure(params string[] errors) => new(null, errors);
}
