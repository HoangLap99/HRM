namespace HRM.Api.Common;

public class BangPhanQuyen
{
    public string? MaRole { get; set; }
    public string? MaRight { get; set; }
    public string? MaAction { get; set; }
    public int GioiHan { get; set; }
    public string? idUser { get; set; }
}

public class clsCookie
{
    public string? name { get; set; }
    public string? value { get; set; }

    public clsCookie()
    {
    }

    public clsCookie(string? name, string? value)
    {
        this.name = name;
        this.value = value;
    }
}

public class clsUser
{
    public string? Id { get; set; }
    public string? TenNhanVien { get; set; }
    public string? MaNhanVien { get; set; }
    public string? LinkAnhDaiDien { get; set; }
}

/// <summary>
/// Thông tin người dùng hiện tại được đồng bộ từ SmartEOS hoặc fallback môi trường dev.
/// </summary>
public class CurrentUser
{
    public string? Id { get; set; }
    public string? idNhanSu { get; set; }
    public string? UserName { get; set; }
    public string? TenNhanVien { get; set; }
    public string? MaNhanVien { get; set; }
    public string? Email { get; set; }
    public string? DienThoai { get; set; }
    public bool IsLocked { get; set; }
    public string? ChucVu { get; set; }
    public long idChucVu { get; set; }
    public string? BoPhan { get; set; }
    public string? Cookies { get; set; }
    public long idBoPhan { get; set; }
    public string? Authorization { get; set; }
    public List<clsCookie> listCookie { get; set; } = new();

    public CurrentUser()
    {
    }

    public CurrentUser(string id)
    {
        Id = id;
    }
}
