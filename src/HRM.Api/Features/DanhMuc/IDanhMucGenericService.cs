﻿namespace HRM.Api.Features.DanhMuc;

/// <summary>
/// Service CRUD generic tuân thủ SOLID (SRP, OCP, DRY) cho các danh mục kế thừa Audit.
/// Loại bỏ hàng chục khối CRUD lặp lại trong FuncDanhMuc.
/// </summary>
public interface IDanhMucGenericService
{
    Task<List<TEntity>> GetAllAsync<TEntity>(CancellationToken ct = default)
        where TEntity : Audit;

    Task<PagedResult<TEntity>> GetPagedAsync<TEntity>(
        int currentPage,
        int pageSize,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null,
        CancellationToken ct = default)
        where TEntity : Audit;

    Task<TEntity?> GetByIdAsync<TEntity>(string idGuid, CancellationToken ct = default)
        where TEntity : Audit;

    Task<MessageReturn> UpsertAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task<bool>>? duplicateCodeCheck = null,
        string duplicateMessage = "Mã đã tồn tại!",
        CancellationToken ct = default)
        where TEntity : Audit;

    Task<MessageReturn> DeleteByIdAsync<TEntity>(string idGuid, CancellationToken ct = default)
        where TEntity : Audit;

    Task<MessageReturn> DeleteRangeAsync<TEntity>(IEnumerable<string> idGuids, CancellationToken ct = default)
        where TEntity : Audit;
}
