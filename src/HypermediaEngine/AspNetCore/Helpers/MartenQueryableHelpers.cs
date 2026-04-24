using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Requests.Sorting;

using Marten.Linq;

using System.Linq.Dynamic.Core;

namespace HypermediaEngine.Helpers;

public static class MartenQueryableHelpers
{
    extension<T>(IMartenQueryable<T> martenQueryable)
    {
        public IQueryable<T> ApplyFiltering(FilterNode? node)
        {
            if (node is null)
            {
                return martenQueryable;
            }
            return martenQueryable.Where(node.ToString());
        }

        public IQueryable<T> ApplySorting(IReadOnlyList<SortField>? sorts)
        {
            if (sorts is null or { Count: 0 })
            {
                return martenQueryable;
            }

            IQueryable<T> queryable = martenQueryable;
            foreach (SortField sort in sorts)
            {
                queryable = martenQueryable.OrderBy("{Field} {Direction}", sort.Field, sort.Direction);
            }
            return queryable;
        }

        public IQueryable<T> ApplyPaging(OffsetOrCursorPaging? ocp)
        {
            if (ocp is null)
            {
                return martenQueryable;
            }
            IQueryable<T> queryable = ocp.Match(
                offset => martenQueryable.ApplyOffetPaging(offset),
                cursor => martenQueryable.ApplyCursorPaging(cursor)
            );
            return queryable;
        }

        public IQueryable<T> ApplyOffetPaging(OffsetPaging op)
        {
            IQueryable<T> queryable = martenQueryable.Skip((op.Page - 1) * op.PageSize).Take(op.PageSize);
            return queryable;
        }

        public IQueryable<T> ApplyCursorPaging(CursorPaging cp)
        {
            if (string.IsNullOrEmpty(cp.Cursor))
            {
                return martenQueryable.Take(cp.Limit);
            }
            IQueryable<T> queryable = martenQueryable.Where("Id > {Id}", cp.Cursor).Take(cp.Limit);
            return queryable;
        }
    }
}
