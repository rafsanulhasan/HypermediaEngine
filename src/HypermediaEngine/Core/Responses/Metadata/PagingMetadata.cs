using Ardalis.SmartEnum.SystemTextJson;

using HypermediaEngine.Requests.Paging;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Responses.Metadata;

public sealed record class PagingMetadata
{
    public static readonly PagingMetadata Default = new()
    {
        CurrentPage = 1,
        PageSize = 10,
        Style = PagingStyles.Offset,
    };

    public int TotalCount { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasNext { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? HasPrevious { get; init; }
    public bool IsFirst { get; init; }
    public int PageSize { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPages { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CurrentPage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ServedCursor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RequestedCursor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CursorField {  get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(SmartEnumNameConverter<PagingStyles, int>))]
    public PagingStyles? Style { get; init; }
}

