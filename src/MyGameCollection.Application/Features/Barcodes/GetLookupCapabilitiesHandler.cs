using MediatR;
using MyGameCollection.Application.Abstractions.Features.Barcodes;
using MyGameCollection.Application.Features.Barcodes.Common;

namespace MyGameCollection.Application.Features.Barcodes;

internal sealed class GetLookupCapabilitiesHandler(GameMetadataEnricher enricher) : IRequestHandler<GetLookupCapabilitiesQuery, LookupCapabilities>
{
    public Task<LookupCapabilities> Handle(GetLookupCapabilitiesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(new LookupCapabilities(enricher.IsEnabled));
}
