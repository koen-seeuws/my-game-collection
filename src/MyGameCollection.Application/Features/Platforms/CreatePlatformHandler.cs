using MediatR;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;
using MyGameCollection.Domain.Entities;

namespace MyGameCollection.Application.Features.Platforms;

internal sealed class CreatePlatformHandler(IPlatformRepository platforms) : IRequestHandler<CreatePlatformCommand, Guid>
{
    public async Task<Guid> Handle(CreatePlatformCommand request, CancellationToken cancellationToken)
    {
        var model = request.Platform;
        if (await platforms.NameExistsAsync(model.Name, null, cancellationToken))
            throw new ValidationException($"A platform named '{model.Name.Trim()}' already exists.");

        var platform = new Platform(model.Name, model.Manufacturer, model.ReleaseYear, model.GetAliases());
        await platforms.AddAsync(platform, cancellationToken);
        return platform.Id;
    }
}
