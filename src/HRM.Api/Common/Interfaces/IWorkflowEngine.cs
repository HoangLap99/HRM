using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces;

/// <summary>
/// Engine xử lý vòng đời quy trình phê duyệt chuẩn cho mọi thực thể kế thừa AudiQuyTrinh,
/// thay thế các hàm static dynamic trong dungchung.cs cũ.
/// </summary>
public interface IWorkflowEngine
{
    Task<MessageReturn> InitializeWorkflowAsync<TEntity>(
        TEntity entity,
        string? maQuyTrinh = null,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> BeforeSaveAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> AfterSaveAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> BeforeForwardAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        long idDuAn = 0,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> AfterForwardAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> BeforeRejectAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> AfterRejectAsync<TEntity>(
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<MessageReturn> LoadWorkflowStateAsync<TEntity>(
        TEntity? entity,
        bool isXemQuyTrinh = false,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh;

    Task<bool> SyncWorkflowAsync(string idTable, CancellationToken ct = default);

    Task<MessageReturn> DeleteWorkflowAsync(string idTable, CancellationToken ct = default);
}
