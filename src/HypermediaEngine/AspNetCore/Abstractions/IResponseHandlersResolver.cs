namespace HypermediaEngine.Abstractions;

public interface IResponseHandlersResolver<T>
    where T : notnull
{
    ValueTask<IResponseHandler> ResolveHandler(object response);
}
