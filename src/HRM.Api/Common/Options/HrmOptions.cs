namespace HRM.Api.Common.Options;

/// <summary>
/// Cáº¥u hÃ¬nh há»‡ thá»‘ng HRM Ã¡nh xáº¡ tá»« section "HrmSettings" trong appsettings.json.
/// </summary>
public class HrmOptions
{
    public const string SectionName = "HrmSettings";

    public string URLSmartEOS { get; set; } = string.Empty;
    public string Host { get; set; } = "PHT";
    public string MaCongTySamSung { get; set; } = string.Empty;
    public int SoPhutSom { get; set; }
    public int SoPhutMuon { get; set; }
    public string PathUploads { get; set; } = @"C:\uploader\App_Data\uploads\";
    public string PathUploadsGoc { get; set; } = @"C:\uploader\App_Data\uploads\";
    public string PathUploaded { get; set; } = @"C:\uploader\App_Data\uploaded\";
    public string MaSoThue { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public int TuoiNghiHuuNam { get; set; } = 60;
    public int TuoiNghiHuuNu { get; set; } = 55;

    /// <summary>
    /// Cho phÃ©p fallback user ADMIN trÃªn localhost khi khÃ´ng truyá»n JWT/Cookie (tÆ°Æ¡ng thÃ­ch hÃ nh vi dev cá»§a BaseController cÅ©).
    /// </summary>
    public bool EnableLocalhostDevFallback { get; set; } = true;
    public string LocalhostDefaultUserId { get; set; } = "ced42063-c4e2-4454-a043-f84f784cc405";
    public string LocalhostDefaultUserName { get; set; } = "ADMIN";

    public string LoaiBoPhanCongTy { get; set; } = string.Empty;
    public string MaLoaiNghiPhep { get; set; } = string.Empty;
    public string MaLoaiNghiBu { get; set; } = string.Empty;
    public string MaChucVuTruongDuAn { get; set; } = string.Empty;
    public float ThoiGianLamViecMotBuoi { get; set; }
    public float ThoiGianLamViecMotBuoiDuAn { get; set; }

    public string SQLConnectMayChamCong { get; set; } = string.Empty;
    public string SQLQueryTuMayChamCong { get; set; } = string.Empty;
}
