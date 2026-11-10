namespace MyGameCollection.Application.Abstractions.Features.Platforms;

public sealed record GetPlatformsQuery : IRequest<IReadOnlyList<PlatformDto>>;
