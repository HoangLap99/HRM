using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces.SmartEos;

/// <summary>
/// Giao tiếp điều phối trạng thái quy trình và ghi log thay đổi sang SmartEOS.
/// </summary>
public interface ISmartEosWorkflowClient
{
    Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoAsync(AudiQuyTrinh entity, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoAsync(string eAction, string idTable, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoKhongAddQuyTrinhXuLyAsync(string eAction, string? idTrangThai = null, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> DoiTrangThaiKhoiTaoAsync(AudiQuyTrinh entity, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> GetTrangThaiChuyenTiepAsync(AudiQuyTrinh entity, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> GetTrangThaiKhongDuyetAsync(AudiQuyTrinh entity, CancellationToken ct = default);
    Task<bool> KhongDuyetBuocChoAsync(AudiQuyTrinh entity, CancellationToken ct = default);
    Task<TrangThaiQuyTrinh> SetHuyQuyTrinhAsync(IReadOnlyCollection<string> idTables, CancellationToken ct = default);
    Task<List<HRM_QT_TrangThaiTiepTheo>> GetTrangThaiChuyenTiepQuyTrinhAsync(string? idTrangThai, CancellationToken ct = default);
    Task<List<HRM_QT_UserTiepTheo>> GetTrangThaiChuyenTiepUsersAsync(long idDuAn, string? idTrangThai, CancellationToken ct = default);
    Task<List<HRM_QuyTrinhXuLy>> GetDongBoQuyTrinhXuLyAsync(string idTable, CancellationToken ct = default);
    Task<bool> SetKiemTraQuyTrinhAsync(string idTable, string? idTrangThai, bool isOpen, CancellationToken ct = default);
    Task<bool> KiemTraTrangThaiKhoiTaoAsync(string? idTrangThai, CancellationToken ct = default);
    Task<bool> XoaQuyTrinhXuLyAsync(string idTable, CancellationToken ct = default);
    Task AddModifyAsync(LM_Modify modify, CancellationToken ct = default);
    Task CreateLogAsync(string functionName, string? idUser, string? userName, CancellationToken ct = default);
}
