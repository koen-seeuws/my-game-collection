using MediatR;
using MyGameCollection.Application.Abstractions.Features.Platforms;
using MyGameCollection.Application.Abstractions.Repositories;
using MyGameCollection.Application.Common;

namespace MyGameCollection.Application.Features.Platforms;

internal sealed class UpdatePlatformHandler(IPlatformRepository platforms) : IRequestHandler<UpdatePlatformCommand>
{
    public async Task Handle(UpdatePlatformCommand request, CancellationToken cancellationToken)
    {
        var platform = await platforms.GetByIdAsync(request.Id, cancellationToken)
                       ?? throw new NotFoundException("Platform", request.Id);

        var model = request.Platform;
        if (await platforms.NameExistsAsync(model.Name, request.Id, cancellationToken))
            throw new ValidationException($"A platform named '{model.Name.Trim()}' already exists.");

        platform.Rename(model.Name);
        platform.Manufacturer = model.Manufacturer.Trim();
        platform.ReleaseYear = model.ReleaseYear;
        platform.SetAliases(model.GetAliases());
        await platforms.UpdateAsync(platform, cancellationToken);
    }
}
