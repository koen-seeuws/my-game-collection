namespace MyGameCollection.Application.Abstractions;

/// <summary>Provides the identity of the user on whose behalf the current request is executed.</summary>
public interface IUserContext
{
    /// <summary>The name of the current user, used as the owner of their game collection.</summary>
    string UserName { get; }

    /// <summary>Whether the current user is an administrator, who can invite people and see all accounts.</summary>
    bool IsAdmin { get; }
}

public static class UserContextExtensions
{
    /// <exception cref="UnauthorizedAccessException">The current user is not an administrator.</exception>
    public static void EnsureAdmin(this IUserContext userContext)
    {
        if (!userContext.IsAdmin)
            throw new UnauthorizedAccessException("Only administrators can manage accounts and invites.");
    }
}
