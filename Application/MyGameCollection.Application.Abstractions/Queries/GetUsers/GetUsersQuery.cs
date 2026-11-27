namespace MyGameCollection.Application.Abstractions.Queries.GetUsers;

/// <summary>All accounts, by last and first name (administrators only).</summary>
public record GetUsersQuery : IQuery<IReadOnlyList<UserDto>>;

public record UserDto(string FirstName, string LastName, string Email, bool IsAdmin, DateTime CreatedAt);
