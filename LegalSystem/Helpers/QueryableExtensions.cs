using LegalSystem.DTOs;
using System.Linq.Expressions;

namespace LegalSystem.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IQueryObject queryObj, Dictionary<string, Expression<Func<T, object>>> columnsMap)
        {
            if (columnsMap != null)
            {
                if (!string.IsNullOrWhiteSpace(queryObj.SortBy))
                {
                    var columnsForSort = queryObj.SortBy.ToLower().Split(',');

                    for (int i = 0; i < columnsForSort.Length; i++)
                    {
                        var columnName = columnsForSort[i];
                        var column = columnsMap.Where(p => p.Key.ToLower().Equals(columnName));
                        if (column.Any())
                        {
                            if (i == 0)
                            {
                                if (queryObj.IsAscending)
                                {
                                    query = query.OrderBy(column.First().Value);
                                }
                                else
                                {
                                    query = query.OrderByDescending(column.First().Value);
                                }
                            }
                            else
                            {
                                if (queryObj.IsAscending)
                                {
                                    query = ((IOrderedQueryable<T>)query).ThenBy(column.First().Value);
                                }
                                else
                                {
                                    query = ((IOrderedQueryable<T>)query).ThenByDescending(column.First().Value);
                                }
                            }
                        }
                    }
                }
            }
            return query;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, IQueryObject queryObj)
        {
            if (queryObj.Index <= 0)
                queryObj.Index = 1;

            if (queryObj.Size <= 0)
                queryObj.Size = 10;

            return query.Skip((queryObj.Index - 1) * queryObj.Size).Take(queryObj.Size);
        }
    }
}
