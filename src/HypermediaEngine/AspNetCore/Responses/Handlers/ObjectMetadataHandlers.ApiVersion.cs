using Asp.Versioning;

using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HypermediaEngine.Responses.Handlers;

internal class ObjectApiVersionMetadataHandler<T>(
    IHttpContextAccessor contextAccessor
) : AbstractObjectMetadataHandler<T> where T : notnull
{
    public override IHypermediaObjectBuilder<T> Handle(T? result, ObjectResponseMetadata? metadata = null)
    {
        HttpContext httpContext = contextAccessor.HttpContext 
                               ?? throw new InvalidOperationException("HttpContext not available");
        if (Builder is null)
        {
            throw new InvalidOperationException("Builder not available");
        }
        metadata ??= new ObjectResponseMetadata(EntityTag.Empty);
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
