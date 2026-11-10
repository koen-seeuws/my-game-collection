namespace MyGameCollection.Application.Abstractions.Features.Photos;

/// <summary>Opens the stored file of a photo (full size or thumbnail). Returns null when it doesn't exist.</summary>
public sealed record GetPhotoFileQuery(Guid PhotoId, bool Thumbnail) : IRequest<PhotoFile?>;
