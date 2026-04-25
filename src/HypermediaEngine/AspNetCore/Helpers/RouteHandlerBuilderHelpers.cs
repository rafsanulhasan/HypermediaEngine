using HypermediaEngine.Http;
using HypermediaEngine.Responses;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using System.Net.Mime;

namespace HypermediaEngine.Helpers;

internal static class RouteHandlerBuilderHelpers
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder ProducesHalCollectionOnly<T>()
            where T : notnull
        {
            builder.Produces<HypermediaCollectionResponse<T>>(
                statusCode: StatusCodes.Status200OK,
                contentType: HalMediaTypeNames.Application.HalJson);
            return builder;
        }

        public RouteHandlerBuilder ProducesHalJsonCollectionOnly<T>()
            where T : notnull
        {
            builder
                .ProducesJsonCollectionOnly<T>()
                .ProducesHalCollectionOnly<T>();
            return builder;
        }

        public RouteHandlerBuilder ProducesHalJsonObjectOnly<T>()
            where T : notnull
        {
            builder
                .ProducesJsonObjectOnly<T>()
                .ProducesHalObjectOnly<T>();
            return builder;
        }

        public RouteHandlerBuilder ProducesHalJsonOnly<T>(bool isList)
            where T : notnull
        {
            builder
                .ProducesJsonOnly<T>(isList)
                .ProducesHalOnly<T>(isList);
            return builder;
        }

        public RouteHandlerBuilder ProducesHalObjectOnly<T>()
            where T : notnull
        {
            builder.Produces<HypermediaObjectResponse<T>>(
                statusCode: StatusCodes.Status200OK,
                contentType: HalMediaTypeNames.Application.HalJson);
            return builder;
        }

        public RouteHandlerBuilder ProducesHalOnly<T>(bool isLst)
            where T : notnull
        {
            if (isLst)
            {
                builder.ProducesHalCollectionOnly<T>();
            }
            else
            {
                builder.ProducesHalObjectOnly<T>();
            }
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonCollectionOnly<T>()
            where T : notnull
        {
            builder.Produces<List<T>>(
                statusCode: StatusCodes.Status200OK,
                contentType: MediaTypeNames.Application.Json);
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonHalCollectionOnly<T>()
            where T : notnull
        {
            builder
                .ProducesHalCollectionOnly<T>()
                .ProducesJsonCollectionOnly<T>();
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonHalObjectOnly<T>()
            where T : notnull
        {
            builder
                .ProducesHalObjectOnly<T>()
                .ProducesJsonObjectOnly<T>();
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonHalOnly<T>(bool isList)
            where T : notnull
        {
            builder
                .ProducesHalOnly<T>(isList)
                .ProducesJsonOnly<T>(isList);
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonObjectOnly<T>()
            where T : notnull
        {
            builder.Produces<T>(
                statusCode: StatusCodes.Status200OK,
                contentType: MediaTypeNames.Application.Json);
            return builder;
        }

        public RouteHandlerBuilder ProducesJsonOnly<T>(bool isList)
            where T : notnull
        {
            if (isList)
            {
                builder.ProducesJsonCollectionOnly<T>();
            }
            else
            {
                builder.ProducesJsonObjectOnly<T>();
            }
            return builder;
        }
    }   
}
