using HypermediaEngine.Requests;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Helpers;

public static class HttpContextHelpers
{
    extension(HttpContext? httpContext)
    {
        public async ValueTask<OptionalResult<QueryParams>> GetQueryParamsFromRequestBody()
        {
            if (httpContext is null)
            {
                return new OptionalResult<QueryParams>(
                    new ArgumentNullException(nameof(httpContext)));
            }

            QueryParams? queryParams = await httpContext
                .Request
                .ReadFromJsonAsync<QueryParams>(httpContext.RequestAborted)
                .ConfigureAwait(false);
            return queryParams is null
                 ? Option<QueryParams>.None
                 : OptionalResult<QueryParams>.Some(queryParams);
        }
    }
}
