using Ardalis.GuardClauses;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Requests.Paging;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

using System.Globalization;
using System.Text;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class CollectionPageEndpointLinkHandler<T>(
    ILinkGenerationService linkGenerationService,
    IHttpContextAccessor contextAccessor
) : AbstractCollectionLinkHandler<T>(contextAccessor) where T : notnull
{
    internal PagingMetadata? Metadata { get; private set; }
    internal HypermediaLink? SelfLink { get; private set; }

    public override IHypermediaCollectionBuilder<T> Handle(IEnumerable<HateoasLinkAttribute> _)
    {
        Guard.Against.Null(HttpContext, message: "HttpContext is null");
        Guard.Against.Null(Builder, message: "Builder is null");
        Metadata ??= Builder.Metadata?.Paging;
        linkGenerationService
            .GenerateSelf<IEnumerable<T>>(title: null)
            .IfSucc(link =>
            {
                SelfLink = link with
                {
                    Type = $"ListOf{typeof(T).Name}",
                };
                Builder = Builder!.WithSelfLink(SelfLink);
            });
        Guard.Against.Null(Metadata, "Metadata must be set before handling links for a collection page endpoint.");
        Guard.Against.Null(SelfLink, "SelfLink must be set before handling links for a collection page endpoint.");
        Uri selfUri = new(SelfLink.Href, UriKind.Absolute);

        return Metadata.Style?.Name switch
        {
            PagingStyles.OffsetKey => HandleOffsetPaging(selfUri),
            PagingStyles.CursorKey => HandleCursorPaging(selfUri),
            _ => Builder,
        };
    }

    private IHypermediaCollectionBuilder<T> HandleOffsetPaging(Uri selfUri)
    {
        if (SelfLink is null)
        {
            return Builder!;
        }
        int currentPage = Metadata!.CurrentPage ?? 1;
        string selfUrl = selfUri.ToString().RemoveParameters("page", "pageSize");
        StringBuilder pageLinkBuilder = new(selfUrl);
        if (Metadata.TotalPages > 0)
        {
            for (var i = 1; i <= Metadata.TotalPages; i++)
            {
                if (i == currentPage)
                {
                    continue;
                }
                string pagedUrl = pageLinkBuilder.ToString();
                pagedUrl = QueryHelpers.AddQueryString(
                    pagedUrl,
                    new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "page", i.ToString(CultureInfo.InvariantCulture) },
                        { "pageSize", Metadata.PageSize.ToString(CultureInfo.InvariantCulture) },
                    });
                HypermediaLink link = SelfLink with
                {
                    Href = pagedUrl,
                    Relationship = LinkRelations.Collection,
                    Title = $"Page {i}",
                };
                Builder = Builder!.WithPageLink(link);
                pageLinkBuilder = new(selfUrl);
            }
        }

        if (Metadata.HasPrevious.HasValue && Metadata.HasPrevious.Value)
        {
            string prevLink = QueryHelpers.AddQueryString(
                selfUrl,
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    { "page", (currentPage - 1).ToString(CultureInfo.InvariantCulture)  },
                    { "pageSize", Metadata.PageSize.ToString(CultureInfo.InvariantCulture) },
                });
            Builder!.WithPageLink(SelfLink with
            {
                Href = prevLink,
                Relationship = LinkRelations.Previous,
                Title = "Previous Link",
            });
        }

        if (Metadata.HasNext.HasValue && Metadata.HasNext.Value)
        {
            string nextLink = QueryHelpers.AddQueryString(
                selfUrl,
                new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                {
                    { "page", (currentPage + 1).ToString(CultureInfo.InvariantCulture)  },
                    { "pageSize", Metadata.PageSize.ToString(CultureInfo.InvariantCulture) },
                });

            Builder = Builder!.WithPageLink(SelfLink with
            {
                Href = nextLink,
                Relationship = LinkRelations.Next,
                Title = "Next Link",
            });
        }

        return Builder!;
    }

    private IHypermediaCollectionBuilder<T> HandleCursorPaging(Uri selfUri)
    {
        linkGenerationService
            .GenerateSelf<IEnumerable<T>>(title: null)
            .IfSucc(link =>
            {
                SelfLink = link;
                Builder = Builder!.WithSelfLink(link);
            });
        if (string.IsNullOrEmpty(Metadata!.Cursor)
         || SelfLink is null or { Relationship: null })
        {
            return Builder!;
        }
        string nextCursor = Metadata.Cursor;
        string nextLink = selfUri.ToString().RemoveParameters("cursor", "limit");
        nextLink = QueryHelpers.AddQueryString(
            nextLink,
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                { "limit", Metadata.PageSize.ToString("{0}", CultureInfo.InvariantCulture) },
                { "cursor", nextCursor },
            });
        Builder = Builder!.WithPageLink(SelfLink with
        {
            Href = nextLink,
            Relationship = LinkRelations.Next,
            Title = "Next Link"
        });

        return Builder!;
    }

    internal CollectionPageEndpointLinkHandler<T> WithMetadata(PagingMetadata metadata)
    {
        Metadata = metadata;
        return this;
    }
}
