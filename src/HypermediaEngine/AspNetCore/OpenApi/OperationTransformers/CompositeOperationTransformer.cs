using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi.OperationTransformers;

internal abstract class CompositeOperationTransformer(
    IEnumerable<IOpenApiOperationTransformer> transformers, 
    bool isParallel = false
) : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (isParallel)
        {
            Task[] tasks = [.. transformers.Select(transformer => transformer.TransformAsync(operation, context, cancellationToken))];
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return;
        }

        foreach (IOpenApiOperationTransformer transformer in transformers)
        {
            await transformer
                .TransformAsync(operation, context, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
