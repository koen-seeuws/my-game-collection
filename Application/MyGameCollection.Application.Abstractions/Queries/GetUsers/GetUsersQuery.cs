namespace MyGameCollection.Application.Abstractions.Queries.GetUsers;

/// <summary>All accounts, by user name (administrators only).</summary>
public record GetUsersQuery : IQuery<IReadOnlyList<UserDto>>;

public record UserDto(string UserName, string? Email, bool IsAdmin, DateTime CreatedAt);
