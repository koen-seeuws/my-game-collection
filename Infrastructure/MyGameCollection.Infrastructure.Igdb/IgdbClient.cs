using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Queries;

namespace MyGameCollection.Infrastructure.Igdb;

/// <summary>Sends Apicalypse queries to the IGDB API.</summary>
internal sealed class IgdbClient(HttpClient httpClient, IgdbTokenProvider tokenProvider, IOptions<IgdbOptions> options)
{
    public const string ImageBaseUrl = "https://images.igdb.com/igdb/image/upload";

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string endpoint, string apicalypse, CancellationToken ct)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
            throw new CatalogUnavailableException(
                "IGDB lookup is not set up yet. Configure Igdb:ClientId and Igdb:ClientSecret to search the catalog.");

        try
        {
            using var response = await SendAsync(endpoint, apicalypse, forceNewToken: false, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // The cached token was revoked or expired early: get a new one and retry once.
                using var retry = await SendAsync(endpoint, apicalypse, forceNewToken: true, ct);
                return await ReadAsync<T>(retry, ct);
            }

            return await ReadAsync<T>(response, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new CatalogUnavailableException("IGDB could not be reached. Try again later.", ex);
        }
    }

    /// <summary>Quotes a value for use in an Apicalypse string literal.</summary>
    public static string Quote(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    public static string? ImageUrl(string? imageId, string size) =>
        string.IsNullOrEmpty(imageId) ? null : $"{ImageBaseUrl}/{size}/{imageId}.png";

    private async Task<HttpResponseMessage> SendAsync(string endpoint, string apicalypse, bool forceNewToken, CancellationToken ct)
    {
        var token = await tokenProvider.GetTokenAsync(forceNewToken, ct);

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(options.Value.ApiBaseUrl, endpoint))
        {
            Content = new StringContent(apicalypse, Encoding.UTF8, "text/plain")
        };
        request.Headers.Add("Client-ID", options.Value.ClientId);
        request.Headers.Authorization = new("Bearer", token);

        return await httpClient.SendAsync(request, ct);
    }

    private static async Task<IReadOnlyList<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new CatalogUnavailableException("IGDB is receiving too many requests right now. Try again in a moment.");

        if (!response.IsSuccessStatusCode)
            throw new CatalogUnavailableException($"IGDB could not handle the search (HTTP {(int)response.StatusCode}).");

        return await response.Content.ReadFromJsonAsync<List<T>>(ct) ?? [];
    }
}
