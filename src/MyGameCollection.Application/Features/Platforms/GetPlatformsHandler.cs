using MediatR;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Application.Abstractions.ReadModels;
using MyGameCollection.Application.Abstractions.Repositories;

namespace MyGameCollection.Application.Features.Platforms;

internal sealed class GetPlatformsHandler(IPlatformRepository platforms) : IRequestHandler<GetPlatformsQuery, IReadOnlyList<PlatformDto>>
{
    public Task<IReadOnlyList<PlatformDto>> Handle(GetPlatformsQuery request, CancellationToken cancellationToken) =>
        platforms.GetSummariesAsync(cancellationToken);
}
