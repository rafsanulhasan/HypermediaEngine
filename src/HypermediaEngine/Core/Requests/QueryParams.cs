using HypermediaEngine.Requests.Paging;

using Microsoft.AspNetCore.Mvc;

using OneOf.Serialization.SystemTextJson;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Requests;

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

    //[FromQuery]
    //public int? Page
    //{
    //    get => field;
    //    set
    //    {
    //        field = value;
    //        Paging ??= new OffsetPaging(value ?? 1, 10);
    //        Paging = Paging.Match(
    //            op => op with { Page = value ?? 1 },
    //            cp => new OffsetPaging(value ?? 1, cp.Limit));
    //    }
    //}

    //[FromQuery]
    //public int? PageSize
    //{
    //    get => field;
    //    set
    //    {
    //        field = value;
    //        Paging ??= new OffsetPaging(1, value ?? 10);
    //        Paging = Paging.Match<OffsetOrCursorPaging>(
    //            op => op with { PageSize = value ?? op.PageSize },
    //            cp => cp with { Limit = value ?? 10 });
    //    }
    //}

    //[FromQuery]
    //public string? Cursor
    //{
    //    get => field;
    //    set
    //    {
    //        field = value;
    //        Paging ??= new CursorPaging(value, 10);
    //        Paging = Paging.Match(
    //            _ => new CursorPaging(value, 10),
    //            cp => cp with { Cursor = value });
    //    }
    //}
}
