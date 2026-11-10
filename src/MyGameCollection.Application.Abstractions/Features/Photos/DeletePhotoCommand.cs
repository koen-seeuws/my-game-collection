namespace MyGameCollection.Application.Abstractions.Features.Photos;

public sealed record DeletePhotoCommand(Guid ItemId, Guid PhotoId) : IRequest;
