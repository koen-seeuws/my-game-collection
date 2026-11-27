using Microsoft.AspNetCore.Identity;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>
/// An account that can sign in. Its <see cref="IdentityUser.UserName"/> is also the owner of the user's game
/// collection, so user names cannot be changed.
/// </summary>
public sealed class AppUser : IdentityUser
{
    public DateTime CreatedAt { get; set; }
}
