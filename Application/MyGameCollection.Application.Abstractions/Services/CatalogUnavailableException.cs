namespace MyGameCollection.Application.Abstractions.Services;

/// <summary>
/// An external catalog (IGDB, the barcode database) could not be reached, is not configured, or refused the request.
/// </summary>
/// <remarks>The message is meant for the user: it says what went wrong without technical detail.</remarks>
public sealed class CatalogUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
