using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace MyGameCollection.Core.Domain;

/// <summary>
/// The secret part of an invite link: <c>{invite key}.{secret}</c>. The key finds the invite, the secret proves the
/// link is genuine, and only a hash of the secret is stored, so the stored data can't be turned back into links.
/// </summary>
public static class InviteToken
{
    public static string Format(Guid inviteKey, string secret) => $"{inviteKey:N}.{secret}";

    public static bool TryParse(string? token, out Guid inviteKey, [NotNullWhen(true)] out string? secret)
    {
        inviteKey = Guid.Empty;
        secret = null;

        var separator = token?.IndexOf('.') ?? -1;
        if (separator < 0 || !Guid.TryParseExact(token.AsSpan(0, separator), "N", out inviteKey))
            return false;

        secret = token![(separator + 1)..];
        return secret.Length > 0;
    }

    /// <summary>A new random (256 bit, URL-safe) secret and its hash.</summary>
    internal static (string Secret, string Hash) NewSecret()
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (secret, Hash(secret));
    }

    internal static string Hash(string secret) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

    /// <summary>Compares in constant time, so response times don't reveal how much of a guess was right.</summary>
    internal static bool Matches(string secret, string hash) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Hash(secret)), Encoding.ASCII.GetBytes(hash));
}
