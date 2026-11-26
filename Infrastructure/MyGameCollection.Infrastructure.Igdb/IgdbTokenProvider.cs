using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Queries;

namespace MyGameCollection.Infrastructure.Igdb;

/// <summary>
/// Fetches and caches the app access token IGDB requires (Twitch client-credentials flow).
/// </summary>
/// <remarks>Registered as a singleton: the token is valid for about two months and is shared by all users.</remarks>
internal sealed class IgdbTokenProvider(IHttpClientFactory httpClientFactory, IOptions<IgdbOptions> options)
{
    public const string HttpClientName = "IgdbToken";

    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt;

    public async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken ct)
    {
        if (!forceRefresh && _token is { } cached && DateTimeOffset.UtcNow < _expiresAt)
            return cached;

        await _lock.WaitAsync(ct);
        try
        {
            if (!forceRefresh && _token is { } token && DateTimeOffset.UtcNow < _expiresAt)
                return token;

            var settings = options.Value;
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsync(settings.TokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId!,
                ["client_secret"] = settings.ClientSecret!,
                ["grant_type"] = "client_credentials"
            }), ct);

            if (!response.IsSuccessStatusCode)
                throw new CatalogUnavailableException(
                    $"IGDB refused the configured credentials (HTTP {(int)response.StatusCode}). Check Igdb:ClientId and Igdb:ClientSecret.");

            var reply = await response.Content.ReadFromJsonAsync<TokenReply>(ct)
                        ?? throw new CatalogUnavailableException("IGDB did not return an access token.");

            _token = reply.AccessToken;
            // Renew a little early so a token never expires mid-request.
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, reply.ExpiresIn - 300));
            return _token;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record TokenReply(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] long ExpiresIn);
}
