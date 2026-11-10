namespace MyGameCollection.Application.Abstractions.Features.Platforms;

public sealed record UpdatePlatformCommand(Guid Id, PlatformEditModel Platform) : IRequest;
