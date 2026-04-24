using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HypermediaEngine.Helpers;

public static class HttpRequestHelpers
{
    extension(HttpRequest? request)
    {
        public StringValues GetHeaderOrDefault(string key, StringValues defaultValue)
        {
            return request?.Headers.GetOrDefault(key, defaultValue)
                ?? defaultValue;
        }
    }
}
