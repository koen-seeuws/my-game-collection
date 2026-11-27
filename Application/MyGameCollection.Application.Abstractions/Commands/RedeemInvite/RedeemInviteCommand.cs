namespace MyGameCollection.Application.Abstractions.Commands.RedeemInvite;

/// <summary>Creates an account with an invite link, which can then no longer be used. Needs no signed-in user.</summary>
public record RedeemInviteCommand : ICommandInterfaces<RedeemInviteResultDto>
{
    public required string Token { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}

/// <summary>The created account's user name, or why no account was created (e.g. the email is already taken).</summary>
public record RedeemInviteResultDto(string? UserName, IReadOnlyList<string> Errors)
{
    public bool Succeeded => UserName is not null;

    public static RedeemInviteResultDto Success(string userName) => new(userName, []);
    public static RedeemInviteResultDto Failure(params string[] errors) => new(null, errors);
}
