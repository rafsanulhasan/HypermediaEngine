using Microsoft.Extensions.Primitives;

namespace HypermediaEngine.Helpers;

public static class StringValuesHelpers
{
    extension(StringValues acceptHeader)
    {
        public string GetMediaTypeOrDefault(string defaultMediaType)
        {
            if (StringValues.IsNullOrEmpty(acceptHeader))
            {
                return defaultMediaType;
            }

            string accept = acceptHeader.ToString();
            string mediaType = accept.Split(';') is { Length: > 0 } splittedAccept
                             ? splittedAccept[0]
                             : defaultMediaType;
            return mediaType;
        }
    }
}
