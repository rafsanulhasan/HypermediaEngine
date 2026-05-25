using Ardalis.SmartEnum.SystemTextJson;

using Microsoft.AspNetCore.Mvc;

using SynergyFx.HypermediaEngine.Requests;
using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.HypermediaEngine.Requests.Paging;

using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests;

public sealed record class QueryParams
{
    public QueryParams(QueryBody? body = null, OffsetOrCursorPaging? paging = null)
    {
        Body = body;
        Paging = paging;
    }

    [FromBody]
    public QueryBody? Body { get; set; }

    //[JsonConverter(typeof(OneOfBaseJsonConverter))]
    [FromQuery]
    public OffsetOrCursorPaging? Paging { get; set; }
}

[JsonSerializable(typeof(QueryParams))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(SmartEnumNameConverter<FilterOperator, int>)])]
public sealed partial class QueryParamsSerializerContext
    : JsonSerializerContext
{
}
