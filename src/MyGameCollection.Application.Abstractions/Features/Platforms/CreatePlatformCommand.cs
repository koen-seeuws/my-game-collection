namespace MyGameCollection.Application.Abstractions.Features.Platforms;

public sealed record CreatePlatformCommand(PlatformEditModel Platform) : IRequest<Guid>;
