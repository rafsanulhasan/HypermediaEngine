namespace HypermediaEngine.Abstractions;

public interface ICollectionResponseHandler<T> : IResponseHandler
    where T : notnull
{
    ValueTask<object?> HandleResponseAsync(object? response);
}