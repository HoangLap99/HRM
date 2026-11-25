using HRM.Api.Common.Interfaces;
using HRM.Api.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HRM.Api.Persistence.Interceptors;

/// <summary>
/// EF Core Interceptor tự động điền thông tin Audit (Created/Modified/IdGuid), chuẩn hóa DateTime cho SQL Server,
/// và loại bỏ truy vấn GetDatabaseValues() gây N+1 khi cập nhật bản ghi.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserContext _currentUserContext;

    public AuditSaveChangesInterceptor(ICurrentUserContext currentUserContext)
    {
        _currentUserContext = currentUserContext;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditAndSanitize(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSanitize(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditAndSanitize(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTime.Now;
        var userId = _currentUserContext.IsAuthenticated ? _currentUserContext.UserId : "SYSTEM";
        var fullName = _currentUserContext.IsAuthenticated ? _currentUserContext.FullName : "SYSTEM";

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            if (entry.Entity is Common.Entity domainEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    domainEntity.OnBeforeInsert();
                }
                else
                {
                    domainEntity.OnBeforeUpdate();
                }
            }

            if (entry.State == EntityState.Added)
            {
                HandleAddedEntry(entry, now, userId, fullName);
            }
            else if (entry.State == EntityState.Modified)
            {
                HandleModifiedEntry(entry, now, userId, fullName);
            }

            ClampSqlDates(entry);
        }
    }

    private static void HandleAddedEntry(EntityEntry entry, DateTime now, string userId, string fullName)
    {
        if (entry.Entity is IGuidKeyEntity guidEntity && string.IsNullOrWhiteSpace(guidEntity.IdGuid))
        {
            guidEntity.IdGuid = Guid.NewGuid().ToString();
        }

        if (entry.Entity is IStringIdEntity stringIdEntity && string.IsNullOrWhiteSpace(stringIdEntity.Id))
        {
            stringIdEntity.Id = Guid.NewGuid().ToString();
        }

        if (entry.Entity is IAuditableEntity auditable)
        {
            auditable.Created = now;
            auditable.Modified = now;
            auditable.CreatedBy = string.IsNullOrWhiteSpace(auditable.CreatedBy) ? userId : auditable.CreatedBy;
            auditable.ModifiedBy = userId;
            auditable.CreatedByName = string.IsNullOrWhiteSpace(auditable.CreatedByName) ? fullName : auditable.CreatedByName;
            auditable.ModifiedByName = fullName;
        }
    }

    private static void HandleModifiedEntry(EntityEntry entry, DateTime now, string userId, string fullName)
    {
        if (entry.Entity is IAuditableEntity auditable)
        {
            auditable.Modified = now;
            auditable.ModifiedBy = userId;
            auditable.ModifiedByName = fullName;

            // Tối ưu: không truy vấn lại DB bằng GetDatabaseValues(), chỉ khóa không ghi đè cột Created*
            MarkPropertyUnmodified(entry, nameof(IAuditableEntity.Created));
            MarkPropertyUnmodified(entry, nameof(IAuditableEntity.CreatedBy));
            MarkPropertyUnmodified(entry, nameof(IAuditableEntity.CreatedByName));
        }
    }

    private static void MarkPropertyUnmodified(EntityEntry entry, string propertyName)
    {
        var prop = entry.Metadata.FindProperty(propertyName);
        if (prop is not null)
        {
            entry.Property(propertyName).IsModified = false;
        }
    }

    private static void ClampSqlDates(EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            var clrType = property.Metadata.ClrType;
            if (clrType == typeof(DateTime) && property.CurrentValue is DateTime dtValue)
            {
                if (dtValue < DateTimeHelper.MinSqlDate)
                {
                    property.CurrentValue = DateTimeHelper.MinSqlDate;
                }
            }
            else if (clrType == typeof(DateTime?) && property.CurrentValue is DateTime nullableDt)
            {
                if (nullableDt < DateTimeHelper.MinSqlDate)
                {
                    property.CurrentValue = DateTimeHelper.MinSqlDate;
                }
            }
        }
    }
}
