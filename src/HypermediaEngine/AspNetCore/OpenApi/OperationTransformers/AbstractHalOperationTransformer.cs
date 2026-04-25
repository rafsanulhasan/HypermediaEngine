using HypermediaEngine.Helpers;
using HypermediaEngine.Http;
using HypermediaEngine.Responses;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi.OperationTransformers;

internal abstract class AbstractHalOperationTransformer
    : IOpenApiOperationTransformer
{
    public Type ConcreteType { get; protected set; }
    public Type ResponseType { get; protected set;  }
    public bool IsList { get; protected set; }
    public bool WithExample {  get; protected set; }

    public virtual string MediaType { get; private set; }
    public abstract string ContentType { get; }

    protected AbstractHalOperationTransformer(Type type, bool isList, bool withExample)
    {
        ConcreteType = type;
        ResponseType = isList
              ? typeof(HypermediaCollectionResponse<>).MakeGenericType(type)
              : typeof(HypermediaObjectResponse<>).MakeGenericType(type);
        IsList = isList;
        WithExample = withExample;
    }

    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        string docVersion = context.DocumentName.Replace("v", string.Empty, StringComparison.Ordinal);
        string apiVersion = context
            .GetImplementedApiVersions()
            .Match(
                implementedVersions
                    => implementedVersions.FirstOrDefault(iv
                        => iv.Equals(docVersion, StringComparison.Ordinal))
                    ?? string.Empty,
                () => string.Empty);
        MediaType = HalMediaTypeNames.AppendVersionToMediaType(ContentType, apiVersion);
        await TransformAsync(docVersion, apiVersion, operation, context, cancellationToken)
            .ConfigureAwait(false);
    }

    public abstract Task TransformAsync(
        string docVersion,
        string apiVersion,
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken);
}
