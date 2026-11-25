using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces.SmartEos;

/// <summary>
/// Giao tiếp lấy dữ liệu cơ cấu tổ chức, bộ phận, dự án, chức danh và nhân sự từ SmartEOS.
/// </summary>
public interface ISmartEosOrganizationClient
{
    Task<string?> CallGetAsync(string relativeUrl, string? jwt = null, IReadOnlyList<clsCookie>? cookies = null, CancellationToken ct = default);
    Task<string?> CallPostAsync(string relativeUrl, object? payload, string? jwt = null, IReadOnlyList<clsCookie>? cookies = null, CancellationToken ct = default);

    Task<List<BoPhan>> GetDanhSachBoPhanAsync(bool useCache = true, CancellationToken ct = default);
    Task<List<BoPhan>> GetDanhSachCongTyAsync(string? idUser = null, bool useCache = true, CancellationToken ct = default);
    Task<List<BoPhan>> GetDanhSachDuAnAsync(bool useCache = true, CancellationToken ct = default);
    Task<List<ChucVu>> GetDanhSachChucVuAsync(bool useCache = true, CancellationToken ct = default);
    Task<List<ChucDanh>> GetDanhSachChucDanhAsync(bool useCache = true, CancellationToken ct = default);
    Task<List<clsUser>> GetDanhSachUserByIdsAsync(IReadOnlyCollection<string> userIds, CancellationToken ct = default);
    Task<List<Link_DanhSachDuAn_NhanSu>> GetListUserNhanSuDuAnTheoThoiGianAsync(long idDuAn, double tuNgayUnix, double denNgayUnix, CancellationToken ct = default);
}
