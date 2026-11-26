namespace MyGameCollection.Application.Abstractions;

/// <summary>Provides the identity of the user on whose behalf the current request is executed.</summary>
public interface IUserContext
{
    /// <summary>The name of the current user, used as the owner of their game collection.</summary>
    string UserName { get; }
}
