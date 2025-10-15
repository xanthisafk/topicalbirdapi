using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TopicalBirdAPI.Data.DTO.PaginationDTO;

namespace TopicalBirdAPI.Helpers
{
    public static class PaginationHelper
    {
        // TSource is Entity
        // TResult is DTO
        public static async Task<PagedResult<TResult>> PaginateAsync<TSource, TResult>(
            IQueryable<TSource> query,
            Expression<Func<TSource, TResult>> selector,
            int pageNo = 1,
            int limit = 20) where TSource : class
        {
            pageNo = (pageNo < 1) ? 1 : pageNo;
            limit = (limit < 1 || limit > 50) ? 20 : limit;

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            pageNo = (pageNo > totalPages && totalPages > 0) ? totalPages : pageNo;

            int skipCount = (pageNo - 1) * limit;

            var items = await query
                .Skip(skipCount)
                .Take(limit)
                .Select(selector)
                .ToListAsync();


            var pagination = new PaginationMetadata
            {
                PageNumber = pageNo,
                Limit = limit,
                TotalItems = totalCount,
                TotalPages = totalPages
            };

            return new PagedResult<TResult>
            {
                Pagination = pagination,
                Items = items
            };
        }
    }
}