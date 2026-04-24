using Ardalis.SmartEnum;

using LanguageExt;

using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

using System.Globalization;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;

namespace HypermediaEngine.Http;

[SmartEnumStringComparer(StringComparison.Ordinal)]
public sealed class HalMediaTypeNames : SmartEnum<HalMediaTypeNames, int>
{
    private HalMediaTypeNames(string name, int value)
        : base(name, value)
    {
    }


    public static new IReadOnlyCollection<HalMediaTypeNames> List => [Application.HalJson, Application.NewlineDelimitedJson];

    public static implicit operator string(HalMediaTypeNames mediaType) => mediaType.Name;
    public static implicit operator StringValues(HalMediaTypeNames mediaType) => mediaType.Name;


    public static IOpenApiParameter CreateParameter(bool halFirst, string version)
    {
        OpenApiParameter param = new()
        {
            Name = HeaderNames.Accept.ToLower(CultureInfo.InvariantCulture),
            In = ParameterLocation.Header,
            AllowEmptyValue = true,
            Required = true,
        };
        return AppendMetadataIntoOpenApiParameter(param, halFirst, version);
    }

    public static string AppendVersionToMediaType(string mediaType, Option<string> apiVersion)
    {
        return apiVersion.Match(
            v => $"{mediaType}; v={v}",
            () => mediaType);
    }

    internal static string CreateMediaType(bool halFirst, Option<string> version)
    {
        StringBuilder dynamicMediaTypeBuilder = new();
        dynamicMediaTypeBuilder.Append(halFirst ? Application.HalJson : MediaTypeNames.Application.Json);
        version.IfSome(v => dynamicMediaTypeBuilder.AppendFormat(CultureInfo.InvariantCulture, "; v={0}", v));
        return dynamicMediaTypeBuilder.ToString();
    }

    private static OpenApiParameter AppendMetadataIntoOpenApiParameter(OpenApiParameter param, bool halFirst, string apiVersion)
    {
        string dynamicMediaType = CreateMediaType(halFirst, apiVersion);
        param.Description = $"Specifies the media types (e.g., {MediaTypeNames.Application.Json}, {Application.HalJson}) that the client is willing to receive in the response.<br/>"
                          + $"Optionally include the API version as a parameter if required (e.g., {dynamicMediaType}).";
        param.Schema = new OpenApiSchema()
        {
            Type = JsonSchemaType.String,
            Examples = CreateExamples(halFirst, apiVersion),
            Description = "Represents a media type. Optionally represnts the API version.",
            Default = CreateDefaultValue(halFirst, apiVersion),
        };
        return param;
    }

    private static JsonNode CreateDefaultValue(bool halFirst, string apiVersion)
    {
        string apiVersionDelimeter = string.IsNullOrWhiteSpace(apiVersion)
                                   ? string.Empty
                                   : $"; v={apiVersion}";
        return JsonNode.Parse(halFirst
             ? $@"""{Application.HalJson}{apiVersionDelimeter}"""
             : $@"""{MediaTypeNames.Application.Json}{apiVersionDelimeter}""")!;
    }

    private static IList<JsonNode> CreateExamples(bool halFirst, string apiVersion)
    {
        string apiVersionDelimeter = string.IsNullOrWhiteSpace(apiVersion)
                                   ? string.Empty
                                   : $"; v={apiVersion}";
        return halFirst
             ? [
                JsonNode.Parse($@"""{Application.HalJson}{apiVersionDelimeter}""")!,
                JsonNode.Parse($@"""{MediaTypeNames.Application.Json}{apiVersionDelimeter}""")!,
               ]
             : [
                JsonNode.Parse($@"""{MediaTypeNames.Application.Json}{apiVersionDelimeter}""")!,
                JsonNode.Parse($@"""{Application.HalJson}{apiVersionDelimeter}""")!,
               ];
    }

    public static class Application
    {
        public static readonly HalMediaTypeNames HalJson = new("application/hal+json", 1);
        public static readonly HalMediaTypeNames NewlineDelimitedJson = new("application/X-ndjson", 2);
    }
}
