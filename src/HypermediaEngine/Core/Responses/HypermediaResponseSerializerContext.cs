using System.Text.Json.Serialization;

namespace HypermediaEngine.Responses;

[JsonSerializable(typeof(HypermediaObjectResponse<>))]
[JsonSerializable(typeof(HypermediaCollectionResponse<>))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class HypermediaResponseSerializerContext : JsonSerializerContext
{

}
