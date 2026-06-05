namespace SynergyFx.HypermediaEngine.Abstractions;

public interface IResponseHandler : IDisposable
{
    ValueTask<object?> HandleResponseAsync(object? response);
}
