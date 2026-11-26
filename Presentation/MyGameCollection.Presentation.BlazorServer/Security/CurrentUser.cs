namespace MyGameCollection.Presentation.BlazorServer.Security;

/// <summary>
/// The user signed in on the current circuit (one per browser tab).
/// A stand-in until real authentication is added: the user simply picks a name.
/// </summary>
public sealed class CurrentUser
{
    public string? UserName { get; private set; }

    public bool IsSignedIn => UserName is not null;

    /// <summary>Raised when the user signs in, switches user or signs out.</summary>
    public event Action? Changed;

    public void SignIn(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        UserName = userName.Trim();
        Changed?.Invoke();
    }

    public void SignOut()
    {
        UserName = null;
        Changed?.Invoke();
    }
}
