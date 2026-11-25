using HRM.Api.Common.Extensions;
using HRM.Api.Common.Interfaces;
using HRM.Api.Features.DanhMuc;
using Microsoft.EntityFrameworkCore;

namespace HRM.Api.Services.Catalog;

/// <summary>
/// Triển khai Generic Catalog CRUD sử dụng EF Core 8 (AsNoTracking khi đọc, AuditSaveChangesInterceptor tự điền vết khi ghi).
/// </summary>
public sealed class DanhMucGenericService : IDanhMucGenericService
{
    private readonly IHrmDbContext _db;

    public DanhMucGenericService(IHrmDbContext db)
    {
        _db = db;
    }

    public Task<List<TEntity>> GetAllAsync<TEntity>(CancellationToken ct = default)
        where TEntity : Audit
        => _db.Set<TEntity>().AsNoTracking().ToListAsync(ct);

    public Task<PagedResult<TEntity>> GetPagedAsync<TEntity>(
        int currentPage,
        int pageSize,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null,
        CancellationToken ct = default)
        where TEntity : Audit
    {
        IQueryable<TEntity> query = _db.Set<TEntity>().AsNoTracking();
        if (filter is not null)
        {
            query = filter(query);
        }

        return query.ToPagedResultAsync(currentPage, pageSize, ct);
    }

    public Task<TEntity?> GetByIdAsync<TEntity>(string idGuid, CancellationToken ct = default)
        where TEntity : Audit
        => _db.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(x => x.IdGuid == idGuid, ct);

    public async Task<MessageReturn> UpsertAsync<TEntity>(
        TEntity entity,
        Func<TEntity, Task<bool>>? duplicateCodeCheck = null,
        string duplicateMessage = "Mã đã tồn tại!",
        CancellationToken ct = default)
        where TEntity : Audit
    {
        if (duplicateCodeCheck is not null && await duplicateCodeCheck(entity))
        {
            return MessageReturn.Fail(duplicateMessage, eErrorMessage.LoiCapNhat);
        }

        if (string.IsNullOrWhiteSpace(entity.IdGuid))
        {
            entity.IdGuid = Guid.NewGuid().ToString();
            _db.Set<TEntity>().Add(entity);
        }
        else
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        await _db.SaveChangesAsync(ct);
        return MessageReturn.Success("Cập nhật dữ liệu thành công!", entity);
    }

    public async Task<MessageReturn> DeleteByIdAsync<TEntity>(string idGuid, CancellationToken ct = default)
        where TEntity : Audit
    {
        var entity = await _db.Set<TEntity>().FirstOrDefaultAsync(x => x.IdGuid == idGuid, ct);
        if (entity is null)
        {
            return MessageReturn.Fail("Không tìm thấy dữ liệu cần xóa!", eErrorMessage.LoiLayDuLieu);
        }

        _db.Set<TEntity>().Remove(entity);
        await _db.SaveChangesAsync(ct);
        return MessageReturn.Success("Xóa dữ liệu thành công!");
    }

    public async Task<MessageReturn> DeleteRangeAsync<TEntity>(IEnumerable<string> idGuids, CancellationToken ct = default)
        where TEntity : Audit
    {
        var ids = idGuids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (ids.Count == 0)
        {
            return MessageReturn.Fail("Không tìm thấy dữ liệu cần xóa!", eErrorMessage.LoiLayDuLieu);
        }

        var entities = await _db.Set<TEntity>().Where(x => ids.Contains(x.IdGuid)).ToListAsync(ct);
        if (entities.Count == 0)
        {
            return MessageReturn.Fail("Không tìm thấy dữ liệu cần xóa!", eErrorMessage.LoiLayDuLieu);
        }

        _db.Set<TEntity>().RemoveRange(entities);
        await _db.SaveChangesAsync(ct);
        return MessageReturn.Success("Xóa dữ liệu thành công!");
    }
}
