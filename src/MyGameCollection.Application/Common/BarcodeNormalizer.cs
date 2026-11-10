namespace MyGameCollection.Application.Common;

public static class BarcodeNormalizer
{
    /// <summary>
    /// Strips spaces/dashes and converts 12-digit UPC-A codes to their 13-digit EAN form (leading 0),
    /// so the same product matches whether a scanner reports it as UPC-A or EAN-13. Returns null for empty input.
    /// </summary>
    public static string? Normalize(string? barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        var cleaned = new string(barcode.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (cleaned.Length == 0) return null;
        if (cleaned.Length == 12 && cleaned.All(char.IsAsciiDigit)) cleaned = "0" + cleaned;
        return cleaned;
    }
}
