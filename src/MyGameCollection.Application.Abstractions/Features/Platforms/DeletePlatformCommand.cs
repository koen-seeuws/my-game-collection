namespace MyGameCollection.Application.Abstractions.Features.Platforms;

public sealed record DeletePlatformCommand(Guid Id) : IRequest;
