using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MyGameCollection.Application.Abstractions;

namespace MyGameCollection.Infrastructure.Barcodes;

/// <summary>
/// Open Products Facts (sister project of Open Food Facts): free, open data, no API key.
/// Coverage for games is limited, so it is used as a fallback.
/// </summary>
internal sealed class OpenProductsFactsProvider(HttpClient http) : IBarcodeLookupProvider
{
    public string Name => "Open Products Facts";

    public async Task<BarcodeProduct?> LookupAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync(
            $"api/v2/product/{Uri.EscapeDataString(barcode)}.json?fields=product_name,brands,categories,image_front_url,image_url",
            cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadFromJsonAsync<OpfResponse>(cancellationToken);
        if (body is not { Status: 1, Product: { } product } || string.IsNullOrWhiteSpace(product.ProductName))
            return null;

        return new BarcodeProduct(
            Name,
            product.ProductName.Trim(),
            product.Brands?.Split(',').FirstOrDefault()?.Trim(),
            product.Categories,
            null,
            product.ImageFrontUrl ?? product.ImageUrl);
    }

    private sealed record OpfResponse(
        [property: JsonPropertyName("status")] int Status,
        [property: JsonPropertyName("product")] OpfProduct? Product);

    private sealed record OpfProduct(
        [property: JsonPropertyName("product_name")] string? ProductName,
        [property: JsonPropertyName("brands")] string? Brands,
        [property: JsonPropertyName("categories")] string? Categories,
        [property: JsonPropertyName("image_front_url")] string? ImageFrontUrl,
        [property: JsonPropertyName("image_url")] string? ImageUrl);
}
