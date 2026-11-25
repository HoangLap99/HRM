using HRM.Api.Common;
using Microsoft.EntityFrameworkCore;

namespace HRM.Api.Common.Extensions;

/// <summary>
/// Extension methods phân trang an toàn kiểu cho IQueryable và IEnumerable,
/// khắc phục hoàn toàn lỗi chia cho 0 hoặc làm tròn sai ở FuncBase.getPaging cũ.
/// </summary>
public static class PaginationExtensions
{
    public const int DefaultPageSize = 25;

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int currentPage,
        int pageSize,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        int totalItems = await query.CountAsync(ct);

        if (currentPage <= 0 || pageSize <= 0)
        {
            var allItems = await query.ToListAsync(ct);
            int totalPageAll = totalItems > 0 ? 1 : 0;
            return new PagedResult<T>(allItems, new PagingInfor(currentPage, totalItems, totalPageAll));
        }

        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, new PagingInfor(currentPage, totalItems, totalPages));
    }

    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source,
        int currentPage,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var materialized = source as IList<T> ?? source.ToList();
        int totalItems = materialized.Count;

        if (currentPage <= 0 || pageSize <= 0)
        {
            int totalPageAll = totalItems > 0 ? 1 : 0;
            return new PagedResult<T>(materialized.ToList(), new PagingInfor(currentPage, totalItems, totalPageAll));
        }

        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var items = materialized
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>(items, new PagingInfor(currentPage, totalItems, totalPages));
    }
}
