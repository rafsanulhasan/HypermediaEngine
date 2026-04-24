using HypermediaEngine.Abstractions;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Responses.Handlers;

#pragma warning disable MA0048 // File name must match type name
internal sealed class SimpleObjectResponseHandler(IHttpContextAccessor httpContextAccessor)
    : AbstractObjectResponseHandler<object?>(httpContextAccessor)
{
    public override ValueTask<object?> HandleTypedResponse(object? response)
    {
        return ValueTask.FromResult(response);
    }
}
#pragma warning restore MA0048 // File name must match type name
