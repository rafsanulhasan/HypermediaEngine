using HypermediaEngine.Requests.Paging;

using Microsoft.AspNetCore.Mvc;

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
}
