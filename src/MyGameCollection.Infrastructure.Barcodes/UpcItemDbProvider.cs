using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>
/// UPCitemdb free "trial" endpoint: no API key, ~100 lookups per day per IP.
/// https://www.upcitemdb.com/api/explorer#!/lookup/get_trial_lookup
/// </summary>
internal sealed class UpcItemDbProvider(HttpClient http, ILogger<UpcItemDbProvider> logger) : IBarcodeLookupProvider
{
    public string Name => "UPCitemdb";

    public async Task<BarcodeProduct?> LookupAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync($"prod/trial/lookup?upc={Uri.EscapeDataString(barcode)}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            logger.LogWarning("UPCitemdb daily/rate limit reached");
            return null;
        }
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadFromJsonAsync<UpcResponse>(cancellationToken);
        var item = body?.Items?.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Title));
        if (item is null) return null;

        return new BarcodeProduct(
            Name,
            WebUtility.HtmlDecode(item.Title!).Trim(),
            NullIfEmpty(item.Brand),
            NullIfEmpty(item.Category),
            NullIfEmpty(item.Description),
            item.Images?.FirstOrDefault(i => i.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ?? item.Images?.FirstOrDefault());
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : WebUtility.HtmlDecode(s).Trim();

    private sealed record UpcResponse([property: JsonPropertyName("items")] List<UpcItem>? Items);

    private sealed record UpcItem(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("brand")] string? Brand,
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("images")] List<string>? Images);
}
