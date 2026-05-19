using Asp.Versioning;

using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HypermediaEngine.Responses.Handlers;

#pragma warning disable MA0048 // File name must match type name
internal sealed class CollectionApiVersionMetadataHandler<T>(
    IHttpContextAccessor contextAccessor,
    IHypermediaCollectionBuilder<T> builder
) : AbstractCollectionMetadataHandler<T>(builder)
    where T : notnull
{
    public override IHypermediaCollectionBuilder<T> Handle(IEnumerable<T> result, ListResponseMetadata? metadata = null)
    {
        HttpContext httpContext = contextAccessor.HttpContext 
                               ?? throw new InvalidOperationException("HttpContext not available");
        metadata ??= Builder.Metadata
                  ?? new ListResponseMetadata(EntityTag.Empty);
        IApiVersionReader? reader = httpContext.RequestServices.GetService<IApiVersionReader>();
        if (reader is null)
        {
            return Builder.WithMetadata(metadata);
        }

        string? apiVersion = reader
            .Read(httpContext.Request)
            .Distinct(StringComparer.Ordinal)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            Builder = Builder.WithMetadata(metadata);
        }

        metadata = metadata with
        {
            ApiVersion = apiVersion,
        };
        Builder = Builder.WithMetadata(metadata);
        return Builder;
    }
}
#pragma warning restore MA0048 // File name must match type name
