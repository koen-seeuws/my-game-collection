namespace MyGameCollection.Application.Abstractions.Features.Photos;

public sealed record SetPrimaryPhotoCommand(Guid ItemId, Guid PhotoId) : IRequest;
