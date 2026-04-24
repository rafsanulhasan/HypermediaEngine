using LanguageExt;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityTagCaching.Models;

internal sealed class EntityTagJsonConverter : JsonConverter<EntityTag>
{
    public override EntityTag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? etagString = reader.GetString();
        Option<EntityTag> entityTag = EntityTag.From(etagString);
        return entityTag.Match(
            etag => etag,
            () => EntityTag.Empty);
    }

    public override void Write(Utf8JsonWriter writer, EntityTag value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

