using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyGameCollection.Application.Abstractions.Services;

namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>Finds a product's title by its barcode in the UPCitemdb database (https://www.upcitemdb.com/api/explorer).</summary>
internal sealed class UpcItemDbClient(HttpClient httpClient, IOptions<BarcodeLookupOptions> options)
{
    /// <summary>The product title, or <c>null</c> when the barcode is unknown.</summary>
    public async Task<string?> FindProductNameAsync(string barcode, CancellationToken ct)
    {
        var settings = options.Value;
        var hasKey = !string.IsNullOrWhiteSpace(settings.UpcItemDbUserKey);

        using var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(settings.UpcItemDbBaseUrl, $"{(hasKey ? "v1" : "trial")}/lookup?upc={Uri.EscapeDataString(barcode)}"));
        if (hasKey)
        {
            request.Headers.Add("user_key", settings.UpcItemDbUserKey);
            request.Headers.Add("key_type", "3scale");
        }

        try
        {
            using var response = await httpClient.SendAsync(request, ct);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound or HttpStatusCode.BadRequest:
                    return null;
                case HttpStatusCode.TooManyRequests:
                    throw new CatalogUnavailableException(
                        "The daily limit of free barcode lookups has been reached. Search by name instead, or try again tomorrow.");
                case var status when !response.IsSuccessStatusCode:
                    throw new CatalogUnavailableException($"The barcode database could not handle the lookup (HTTP {(int)status}).");
            }

            var reply = await response.Content.ReadFromJsonAsync<LookupReply>(ct);
            return reply?.Items?
                .Select(item => item.Title?.Trim())
                .FirstOrDefault(title => !string.IsNullOrEmpty(title));
        }
        catch (HttpRequestException ex)
        {
            throw new CatalogUnavailableException("The barcode database could not be reached. Try again later.", ex);
        }
    }

    private sealed record LookupReply([property: JsonPropertyName("items")] IReadOnlyList<LookupItem>? Items);

    private sealed record LookupItem([property: JsonPropertyName("title")] string? Title);
}
