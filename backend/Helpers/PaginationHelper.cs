using Microsoft.EntityFrameworkCore;

namespace backend.Helpers;

public static class PaginationHelper
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    public static async Task<PagedResult<T>> PaginateAsync<T>(
        IQueryable<T> query,
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;

        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}