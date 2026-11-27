using System.Diagnostics.CodeAnalysis;

namespace MyGameCollection.Application.Abstractions.Services.Barcodes;

/// <summary>An EAN or UPC product barcode: 8 to 14 digits.</summary>
public readonly record struct Barcode
{
    private Barcode(string value) => Value = value;

    /// <summary>The digits, without spaces or dashes.</summary>
    public string Value { get; }

    public override string ToString() => Value;

    /// <summary>Reads a barcode as typed or scanned, ignoring spaces and dashes.</summary>
    public static bool TryParse(string? text, [NotNullWhen(true)] out Barcode? barcode)
    {
        var digits = new string((text ?? string.Empty).Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());
        barcode = digits.Length is >= 8 and <= 14 && digits.All(char.IsAsciiDigit) ? new Barcode(digits) : null;
        return barcode is not null;
    }

    /// <exception cref="ArgumentException"><paramref name="text"/> is not an EAN or UPC barcode.</exception>
    public static Barcode Parse(string? text) =>
        TryParse(text, out var barcode)
            ? barcode.Value
            : throw new ArgumentException($"'{text?.Trim()}' is not a barcode. Enter the 8 to 14 digits under the stripes.", nameof(text));
}
