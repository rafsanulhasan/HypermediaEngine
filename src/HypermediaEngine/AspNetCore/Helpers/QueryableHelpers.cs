using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Requests.Sorting;

using System.Linq.Dynamic.Core;

namespace HypermediaEngine.Helpers;

public static class QueryableHelpers
{
    extension<T>(IQueryable<T> queryable)
    {
        public IQueryable<T> ApplyFiltering(FilterNode? node)
        {
            if (node is null)
            {
                return queryable;
            }
            queryable = queryable.Where(node.ToString());
            return queryable;
        }

        public IQueryable<T> ApplySorting(IReadOnlyList<SortField>? sorts)
        {
            if (sorts is  null or { Count:0})
            {
                return queryable; 
            }

            foreach (SortField sort in sorts)
            {
                queryable = queryable.OrderBy("{Field} {Direction}", sort.Field, sort.Direction);
            }
            return queryable;
        }

        public IQueryable<T> ApplyPaging(OffsetOrCursorPaging? ocp)
        {
            if (ocp is null)
            {
                return queryable;
            }
            queryable = ocp.Match(
                offset => queryable.ApplyOffetPaging(offset),
                cursor => queryable.ApplyCursorPaging(cursor)
            );
            return queryable;
        }

        public IQueryable<T> ApplyOffetPaging(OffsetPaging op)
        {

            queryable = queryable.Skip((op.Page - 1) * op.PageSize).Take(op.PageSize);
            return queryable;
        }

        public IQueryable<T> ApplyCursorPaging(CursorPaging cp)
        {
            if (string.IsNullOrEmpty(cp.Cursor))
            {
                return queryable.Take(cp.Limit);
            }
            queryable = queryable.Where("Id > {Id}", cp.Cursor).Take(cp.Limit);
            return queryable;
        }
    }
}
