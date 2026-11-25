namespace HRM.Api.Common;

/// <summary>
/// Contract cho các entity lưu vết thời gian và người tạo/cập nhật.
/// </summary>
public interface IAuditableEntity
{
    DateTime Created { get; set; }
    string? CreatedBy { get; set; }
    string? CreatedByName { get; set; }
    DateTime Modified { get; set; }
    string? ModifiedBy { get; set; }
    string? ModifiedByName { get; set; }
}

/// <summary>
/// Contract cho các entity sử dụng khóa chính GUID dạng chuỗi có tên cột IdGuid.
/// </summary>
public interface IGuidKeyEntity
{
    string IdGuid { get; set; }
}

/// <summary>
/// Contract cho các entity sử dụng khóa chính GUID dạng chuỗi có tên cột Id.
/// </summary>
public interface IStringIdEntity
{
    string Id { get; set; }
}

/// <summary>
/// Contract chuẩn cho các thực thể tham gia quy trình phê duyệt SmartEOS.
/// </summary>
public interface IWorkflowEntity : IAuditableEntity, IGuidKeyEntity
{
    string? SoQuyTrinh { get; set; }
    string? NoiDung { get; set; }
    string? GhiChu { get; set; }
    string? IdTrangThai { get; set; }
    string? TenTrangThai { get; set; }
    long? IdDuAn { get; set; }
    bool isKetThuc { get; set; }
    string? eAction { get; set; }
    string? LyDoKhongDuyet { get; set; }
    string? IdCongViec { get; set; }
    float? ThoiGianT { get; set; }
    float? ThoiGianGiamTru { get; set; }
    string? LyDoGiamTru { get; set; }
    string? IdTrangThaiThamGia { get; set; }
    string? IdTrangThaiTruocKhiDoi { get; set; }
    List<string>? listIdTrangThaiKhongDuyetBuocCho { get; set; }
    string? IdTrangThaiTruocKhiChuyenBuoc { get; set; }
    List<HRM_QT_TrangThaiTiepTheo> listTrangThaiTiepTheo { get; set; }
    List<HRM_QT_UserTiepTheo> listUserTiepTheo { get; set; }
    List<QuyTrinhKhoiTao>? listKhoiTao { get; set; }
    bool isDisableTrangThaiTiepTheo { get; set; }
    int? LoaiTrangThai { get; set; }
    string? NoiDungKhac { get; set; }
}
