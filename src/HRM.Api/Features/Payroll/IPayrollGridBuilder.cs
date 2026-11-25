using HRM.Api.Common.Interfaces;

namespace HRM.Api.Features.Payroll;

/// <summary>
/// Service dựng dữ liệu lưới bảng lương động theo cấu hình header (thay thế chuỗi else-if thủ công bằng ánh xạ O(1)).
/// </summary>
public interface IPayrollGridBuilder
{
    List<clsBangTinhLuong_NhanVien> BuildStandardGrid(
        List<clsBangTinhLuong_Header> headers,
        List<HRM_QuyTrinhTinhLuong_NhanVien> data,
        List<HRM_NhanVien> employees,
        List<HRM_HopDong> contracts,
        IHrmDbContext db);

    List<clsBangTinhLuong_NhanVien> BuildPhtGrid(
        List<clsBangTinhLuong_Header> headers,
        List<HRM_QuyTrinhTinhLuong_NhanVien> data,
        List<HRM_NhanVien> employees,
        List<HRM_HopDong> contracts,
        IHrmDbContext db);
}