namespace MyGameCollection.Application.Abstractions.Features.Photos;

public sealed record AddPhotoCommand(Guid ItemId, PhotoUpload Upload) : IRequest<Guid>;
