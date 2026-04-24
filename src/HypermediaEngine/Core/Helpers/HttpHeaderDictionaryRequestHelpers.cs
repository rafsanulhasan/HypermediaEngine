using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HypermediaEngine.Helpers;

public static class HttpHeaderDictionaryRequestHelpers
{
    extension(IHeaderDictionary? header)
    {
        public StringValues GetOrDefault(string key, StringValues defaultValue)
        {
            if (header is null)
            {
                return defaultValue;
            }

            if (header.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}
