using HRM.Api.Common;
using Microsoft.AspNetCore.Identity;

namespace HRM.Api.Identity;

public class ApplicationUser : IdentityUser, IAuditableEntity
{
    public string? TenNhanVien { get; set; }
    public string? MaNhanVien { get; set; }
    public string? CMND { get; set; }
    public string? DienThoai { get; set; }
    public string? LinkAnhDaiDien { get; set; }
    public string? LinkAnhBanLamViec { get; set; }
    public bool IsLocked { get; set; }

    public string? MaCoSo { get; set; }
    public long idCoSo { get; set; }
    public string? idCoSoGUI { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime Modified { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public bool isThuNgan { get; set; }
    public bool isQuanLy { get; set; }
    public bool isPhucVu { get; set; }
    public bool isBepBar { get; set; }
}
