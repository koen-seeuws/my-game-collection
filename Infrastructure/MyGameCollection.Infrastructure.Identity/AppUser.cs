using Microsoft.AspNetCore.Identity;

namespace MyGameCollection.Infrastructure.Identity;

/// <summary>
/// An account that signs in with its (unique) email address, which also serves as Identity's
/// <see cref="IdentityUser.UserName"/>. Its <see cref="IdentityUser{TKey}.Id"/> owns the user's game collection, so
/// the collection stays put should the email address ever change.
/// </summary>
public sealed class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
