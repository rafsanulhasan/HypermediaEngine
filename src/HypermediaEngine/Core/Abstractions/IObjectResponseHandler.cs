namespace SynergyFx.HypermediaEngine.Abstractions;

public interface IObjectResponseHandler<T> : IResponseHandler
    where T : notnull
{
    ValueTask<object?> HandleResponseAsync(object? response);
}
