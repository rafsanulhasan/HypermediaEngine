using HypermediaEngine.Requests.Paging;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using System.Globalization;

namespace HypermediaEngine.Helpers;

public static class HttpContextHelpers
{
    extension(HttpContext? httpContext)
    {
        public StringValues GetRequestHeaderOrDefault(string key, StringValues defaultValue)
        {
            return httpContext?.Request.Headers.GetOrDefault(key, defaultValue)
                ?? defaultValue;
        }

        public OptionalResult<OffsetOrCursorPaging> GetPagingFromRequestQueryParams()
        {
            if (httpContext is null)
            {
                return new OptionalResult<OffsetOrCursorPaging>(
                    new ArgumentNullException(nameof(httpContext)));
            }

            if (httpContext.Request.Query is not IQueryCollection query)
            {
                return new OptionalResult<OffsetOrCursorPaging>(
                    Option<OffsetOrCursorPaging>.None);
            }

            if (query.TryGetValue(nameof(OffsetPaging.PageSize), out StringValues pageSizeStr)
             && query.TryGetValue(nameof(OffsetPaging.Page), out StringValues currentPageStr))
            {
                if (StringValues.IsNullOrEmpty(pageSizeStr)
                 || !int.TryParse(pageSizeStr, CultureInfo.InvariantCulture, out int pageSize))
                {
                    return new OptionalResult<OffsetOrCursorPaging>(
                        new InvalidDataException($"The value of {nameof(OffsetPaging.PageSize)} should be valid non-negetive number"));
                }

                if (StringValues.IsNullOrEmpty(currentPageStr)
                 || !int.TryParse(currentPageStr, CultureInfo.InvariantCulture, out int currentPage))
                {
                    return new OptionalResult<OffsetOrCursorPaging>(
                        new InvalidDataException($"The value of {nameof(OffsetPaging.Page)} should be valid non-negetive number"));
                }

                OffsetOrCursorPaging op = new(new OffsetPaging(currentPage, pageSize));
                return OptionalResult<OffsetOrCursorPaging>.Some(op);
            }

            if (query.TryGetValue(nameof(CursorPaging.Limit), out StringValues limitStr)
             && query.TryGetValue(nameof(CursorPaging.Cursor), out StringValues cursorStr))
            {
                if (StringValues.IsNullOrEmpty(limitStr)
                 || !int.TryParse(limitStr, CultureInfo.InvariantCulture, out int limit))
                {
                    return new OptionalResult<OffsetOrCursorPaging>(
                        new InvalidDataException($"The value of {nameof(CursorPaging.Limit)} should be valid non-negetive number"));
                }

                if (StringValues.IsNullOrEmpty(cursorStr))
                {
                    return new OptionalResult<OffsetOrCursorPaging>(
                        new InvalidDataException($"The value of {nameof(CursorPaging.Cursor)} should not be empty"));
                }

                OffsetOrCursorPaging cp = new(new CursorPaging(cursorStr, limit));
                return OptionalResult<OffsetOrCursorPaging>.Some(cp);
            }

            return new OptionalResult<OffsetOrCursorPaging>(
                    Option<OffsetOrCursorPaging>.None);
        }
    }
}
