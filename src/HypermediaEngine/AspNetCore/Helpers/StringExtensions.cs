using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

using System.Text;

namespace HypermediaEngine.Helpers;

public static class UriHelpers
{
    extension(string url)
    {
        public string RemoveParameters(params ICollection<string> paramsToBeRemoved)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Split off fragment (#something)
            int fragmentIndex = url.IndexOf('#', StringComparison.OrdinalIgnoreCase);
            string fragment = fragmentIndex >= 0 ? url[fragmentIndex..] : string.Empty;
            string baseUrl = fragmentIndex >= 0 ? url[..fragmentIndex] : url;

            // Parse query parameters
            Uri uri = new(baseUrl, UriKind.RelativeOrAbsolute);
            return uri.RemoveParameters(fragment, paramsToBeRemoved);
        }
    }

    extension (Uri uri)
    {
        public string RemoveParameters(string fragment, params ICollection<string> paramsToBeRemoved)
        {
            string path = uri.GetLeftPart(UriPartial.Path);
            Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(uri.Query);

            // Rebuild query without page and pageSize
            Dictionary<string, StringValues> filtered = query
                .Where(kvp => !paramsToBeRemoved.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);
            StringBuilder sb = new();
            sb
                .Append(fragment.Length > 0
                    ? QueryHelpers.AddQueryString(path, filtered)
                    : path)
                .Append(fragment);

            return sb.ToString();
        }
    }
}