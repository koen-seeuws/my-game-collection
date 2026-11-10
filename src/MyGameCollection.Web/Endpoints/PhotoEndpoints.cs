using Microsoft.Net.Http.Headers;
using MediatR;
using MyGameCollection.Application.Abstractions.Features.Photos;

namespace MyGameCollection.Web.Endpoints;

public static class PhotoEndpoints
{
    public static IEndpointRouteBuilder MapPhotoEndpoints(this IEndpointRouteBuilder app)
    {
        // Photo files never change for a given id, so they can be cached aggressively by the browser.
        app.MapGet("/photos/{id:guid}/{size}", async (Guid id, string size, ISender sender, HttpContext http, CancellationToken ct) =>
        {
            if (size is not ("full" or "thumb")) return Results.NotFound();

            var file = await sender.Send(new GetPhotoFileQuery(id, Thumbnail: size == "thumb"), ct);
            if (file is null) return Results.NotFound();

            http.Response.Headers[HeaderNames.CacheControl] = "private, max-age=31536000, immutable";
            return Results.Stream(file.Content, file.ContentType, lastModified: file.LastModified, enableRangeProcessing: true);
        });

        return app;
    }
}
