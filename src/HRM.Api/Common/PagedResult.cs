namespace HRM.Api.Common;

/// <summary>
/// Wrapper phân trang an toàn kiểu (type-safe) thay thế DynamicObject(paging, items) ở code cũ.
/// </summary>
public class PagedResult<T>
{
    public PagingInfor paging { get; set; } = new();
    public List<T> items { get; set; } = new();

    public PagedResult()
    {
    }

    public PagedResult(List<T> items, PagingInfor paging)
    {
        this.items = items;
        this.paging = paging;
    }

    public PagedResult(List<T> items, int currentPage, int pageSize, int totalItems)
    {
        int totalPages = pageSize > 0
            ? (int)Math.Ceiling((double)totalItems / pageSize)
            : (totalItems > 0 ? 1 : 0);

        this.items = items;
        paging = new PagingInfor(currentPage, totalItems, totalPages);
    }
}
