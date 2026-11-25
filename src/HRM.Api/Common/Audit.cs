using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.Api.Common;

public abstract class Entity
{
    public virtual void OnBeforeInsert() { }
    public virtual void OnBeforeUpdate() { }
}

public class Audit : IAuditableEntity, IGuidKeyEntity
{
    [Key]
    [MaxLength(36)]
    public string IdGuid { get; set; } = string.Empty;

    public DateTime Created { get; set; }

    [MaxLength(36)]
    public string? CreatedBy { get; set; }

    [MaxLength(500)]
    public string? CreatedByName { get; set; }

    public DateTime Modified { get; set; }

    [MaxLength(136)]
    public string? ModifiedBy { get; set; }

    [MaxLength(500)]
    public string? ModifiedByName { get; set; }

    [NotMapped]
    public clsUser? ItemUserCreatedBy { get; set; }
}

public class AuditId : IAuditableEntity, IStringIdEntity
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = string.Empty;

    public DateTime Created { get; set; }

    [MaxLength(36)]
    public string? CreatedBy { get; set; }

    [MaxLength(500)]
    public string? CreatedByName { get; set; }

    public DateTime Modified { get; set; }

    [MaxLength(36)]
    public string? ModifiedBy { get; set; }

    [MaxLength(500)]
    public string? ModifiedByName { get; set; }

    [NotMapped]
    public clsUser? ItemUserCreatedBy { get; set; }
}

public class AudiQuyTrinh : Audit, IWorkflowEntity
{
    [StringLength(255)]
    public string? SoQuyTrinh { get; set; }

    public string? NoiDung { get; set; }

    public string? GhiChu { get; set; }

    [StringLength(36)]
    public string? IdTrangThai { get; set; }

    public string? TenTrangThai { get; set; }
    public string? NguyenNhanNhanhCham { get; set; }
    public string? DeXuatKeHoach { get; set; }
    public long? IdDuAn { get; set; } = 0;
    public bool isKetThuc { get; set; }
    public string? LyDoKhongDuyet { get; set; }

    [StringLength(36)]
    public string? IdCongViec { get; set; } = null;

    public float? ThoiGianT { get; set; }
    public string? eAction { get; set; }

    [NotMapped]
    public List<HRM_QT_TrangThaiTiepTheo> listTrangThaiTiepTheo { get; set; } = new();

    [NotMapped]
    public List<HRM_QT_UserTiepTheo> listUserTiepTheo { get; set; } = new();

    [NotMapped]
    public float? ThoiGianGiamTru { get; set; }

    [NotMapped]
    public string? LyDoGiamTru { get; set; }

    [NotMapped]
    public string? IdTrangThaiThamGia { get; set; }

    [NotMapped]
    public string? IdTrangThaiTruocKhiDoi { get; set; }

    [NotMapped]
    public List<string>? listIdTrangThaiKhongDuyetBuocCho { get; set; }

    [NotMapped]
    public string? IdTrangThaiTruocKhiChuyenBuoc { get; set; }

    [NotMapped]
    public List<HRM_FileDinhKem>? listFileDinhKem { get; set; }

    [NotMapped]
    public List<QuyTrinhTrangThai>? listTrangThai { get; set; }

    [NotMapped]
    public string? eTable { get; set; }

    [NotMapped]
    public bool isKiemTraIdCongViec { get; set; } = true;

    [NotMapped]
    public string? Ma { get; set; }

    [NotMapped]
    public string? NoiDungKhac { get; set; }

    [NotMapped]
    public List<QuyTrinhKhoiTao>? listKhoiTao { get; set; }

    [NotMapped]
    public bool isDisableTrangThaiTiepTheo { get; set; } = false;

    [NotMapped]
    public int? SapXep { get; set; } = 0;

    [NotMapped]
    public int? LoaiTrangThai { get; set; }

    public AudiQuyTrinh()
    {
    }

    public AudiQuyTrinh(HRM_QuyTrinhKhoiTao item)
    {
        IdGuid = item.IdQuyTrinhGoc ?? string.Empty;
        eAction = item.eActionGoc;
        IdDuAn = item.IdDuAn;
        NoiDung = item.NoiDung;
    }

    public AudiQuyTrinh(QuyTrinhDuAn item)
    {
        IdDuAn = item.Id;
        IdGuid = item.Id.ToString();
        eTable = DefineData.eTable.HRM_QuyTrinhDuAn.ToString();
        IdTrangThai = item.IdTrangThai;
        TenTrangThai = item.TenTrangThai;
        NoiDung = item.NoiDung;
    }
}

public class HRM_QT_UserTiepTheo : AuditId
{
    [StringLength(36)]
    public string? IdTrangThai { get; set; }

    [StringLength(36)]
    public string? IdUser { get; set; }

    [StringLength(36)]
    public string? IdTable { get; set; }

    public bool isKetThuc { get; set; } = false;

    [StringLength(36)]
    public string? IdQTTrangThaiTiepTheo { get; set; }

    [NotMapped]
    public string? TenUser { get; set; }

    [NotMapped]
    public bool isChon { get; set; } = false;

    public HRM_QT_UserTiepTheo()
    {
    }

    public HRM_QT_UserTiepTheo(IWorkflowEntity item, HRM_QT_TrangThaiTiepTheo? tttt, string? idUser)
    {
        Id = Guid.NewGuid().ToString();
        IdTrangThai = item.IdTrangThai;
        IdUser = idUser;
        IdTable = item.IdGuid;
        if (tttt is not null)
        {
            IdQTTrangThaiTiepTheo = tttt.Id;
        }
    }
}

public class HRM_QT_TrangThaiTiepTheo : AuditId
{
    [StringLength(36)]
    public string? IdTrangThai { get; set; }

    [StringLength(36)]
    public string? IdTrangThaiTiepTheo { get; set; }

    [StringLength(36)]
    public string? IdTable { get; set; }

    public bool isKetThuc { get; set; } = false;

    [NotMapped]
    public string? TenTrangThai { get; set; }

    [NotMapped]
    public bool isChon { get; set; } = false;
}

public class QuyTrinhKhoiTao
{
    public string? IdQuyTrinhKhoiTao { get; set; }
    public int Ngay { get; set; }
    public string? eAction { get; set; }

    public QuyTrinhKhoiTao()
    {
    }

    public QuyTrinhKhoiTao(int ngay, string? eAction)
    {
        Ngay = ngay;
        this.eAction = eAction;
    }
}

public class QuyTrinhDuAn
{
    public long Id { get; set; }
    public long IdDuAn { get; set; }
    public string? SoPhieu { get; set; }
    public string? TenDuAn { get; set; }
    public string? MaDuAn { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public DateTime NgayBatDauKH { get; set; }
    public DateTime NgayKetThucKH { get; set; }
    public string? IdTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? idUserQuanLyDuAn { get; set; }
    public string? TenUserQuanLyDuAn { get; set; }
    public string? MaCongVan { get; set; }
    public double TongMucDauTu { get; set; }
    public double MucDauTuDuKien { get; set; }
    public string? DiaChi { get; set; }
    public string? PhamVi { get; set; }
    public long IdDanhMucTinh { get; set; }
    public string? MaVung { get; set; }
    public string? GhiChu { get; set; }
    public string? TenNguoiTao { get; set; }
    public string? IddmChuDauTu { get; set; }
    public long IdNhaCungUngSCM { get; set; }
    public string? TenNhaCungUng { get; set; }
    public string? SoQuyTrinh { get; set; }
    public string? DanhGiaQuyTrinh { get; set; }
    public string? NoiDung { get; set; }
    public double NgayBatDauUnix { get; set; }
    public double NgayKetThucUnix { get; set; }
    public double NgayBatDauKHUnix { get; set; }
    public double NgayKetThucKHUnix { get; set; }
    public string? eAction { get; set; } = DefineData.eAction.QUYTRINHDUAN.ToString();
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public bool? isKetThuc { get; set; }
    public List<HRM_FileDinhKem>? listFileDinhKem { get; set; }

    public QuyTrinhDuAn Clone() => (QuyTrinhDuAn)MemberwiseClone();
}

public class HRM_QuyTrinhKhoiTao : AuditId
{
    [StringLength(36)]
    public string? IdQuyTrinhGoc { get; set; }

    [StringLength(255)]
    public string? eActionGoc { get; set; }

    [StringLength(255)]
    public string? eLienQuan { get; set; }

    public long? IdDuAn { get; set; }

    [StringLength(255)]
    public string? NoiDung { get; set; }

    public DateTime? NgayXuLy { get; set; }

    [NotMapped]
    public string? ThoiGianXuLy { get; set; }

    public HRM_QuyTrinhKhoiTao()
    {
    }

    public HRM_QuyTrinhKhoiTao(QuyTrinhKhoiTao quyTrinhKhoiTao, IWorkflowEntity quyTrinhGoc)
    {
        Id = Guid.NewGuid().ToString();
        IdQuyTrinhGoc = quyTrinhGoc.IdGuid;
        eActionGoc = quyTrinhGoc.eAction;
        eLienQuan = quyTrinhKhoiTao.eAction;
        IdDuAn = quyTrinhGoc.IdDuAn;
        NoiDung = quyTrinhGoc.NoiDung;
        NgayXuLy = DateTime.Now.AddDays(quyTrinhKhoiTao.Ngay);
    }
}

public class ChuyenTrangThai
{
    public string? eAction { get; set; }
    public string? IdGuid { get; set; }
    public string? IdTrangThai { get; set; }
}

public class HRM_QuyTrinhLienQuan : AuditId
{
    [StringLength(36)]
    public string? IdQuyTrinhGoc { get; set; }

    [StringLength(255)]
    public string? eActionGoc { get; set; }

    [StringLength(36)]
    public string? IdQuyTrinhLienQuan { get; set; }

    [StringLength(255)]
    public string? eActionLienQuan { get; set; }

    [StringLength(1000)]
    public string? IdQuyTrinhKey { get; set; }
}

public class QuyTrinhTrangThai
{
    public string? TenTrangThai { get; set; }
    public string? MaTrangThai { get; set; }
    public string? IdTrangThai { get; set; }
    public string? TenQuyTrinh { get; set; }
    public string? MaQuyTrinh { get; set; }
    public string? IdQuyTrinh { get; set; }
    public int SapXep { get; set; }
}

public partial class HRM_FileDinhKem : Audit
{
    [MaxLength(100)]
    public string? TableName { get; set; }

    [MaxLength(200)]
    public string? idItem { get; set; }

    [MaxLength(250)]
    public string? FileName { get; set; }

    [MaxLength(250)]
    public string? FileNameGUI { get; set; }

    public string? LinkName { get; set; }

    [MaxLength(250)]
    public string? Folder { get; set; }

    [MaxLength(1000)]
    public string? Path { get; set; }

    [MaxLength(2000)]
    public string? Link { get; set; }

    [MaxLength(2000)]
    public string? FlagDinhKem { get; set; }

    [MaxLength(1000)]
    public string? TieuDe { get; set; }

    public long idFolder { get; set; }
    public long idDuAn { get; set; }
    public long idTinhChatCongViec { get; set; }
    public bool isDeleted { get; set; }
    public int isPheDuyet { get; set; }

    [NotMapped]
    public string? Name { get; set; }

    [NotMapped]
    public string? NameLocal { get; set; }

    [NotMapped]
    public string? IdUser { get; set; }

    [NotMapped]
    public string? LinkDocView { get; set; }

    public HRM_FileDinhKem Clone() => (HRM_FileDinhKem)MemberwiseClone();
}

public class HRM_QuyTrinhXuLy : AuditId
{
    [StringLength(36)]
    public string? IdThietLapQuyTrinh { get; set; }

    [StringLength(36)]
    public string? IdTrangThai { get; set; }

    [StringLength(36)]
    public string? IdTable { get; set; }

    [StringLength(500)]
    public string? TenTrangThai { get; set; }

    [StringLength(255)]
    public string? SoQuyTrinh { get; set; }

    public string? GhiChu { get; set; }
    public int isDaXuLy { get; set; }
    public bool isTrucTiepXuLy { get; set; }

    [StringLength(36)]
    public string? IdUser { get; set; }

    [StringLength(36)]
    public string? IdUserUyQuyen { get; set; }

    public double? ThoiGian { get; set; }
    public float? ThoiGianGiamTru { get; set; }
    public string? LyDoGiamTru { get; set; }
    public int ThoiGianXuLy_Ngay { get; set; }
    public int ThoiGianXuLy_Gio { get; set; }
    public int ThoiGianXuLy_Phut { get; set; }
    public int ThoiGianNhanhCham_Ngay { get; set; }
    public int ThoiGianNhanhCham_Gio { get; set; }
    public int ThoiGianNhanhCham_Phut { get; set; }
    public long IdDuAn { get; set; }
    public int? Loai { get; set; }
    public DateTime? NgayNhanViec { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public DateTime? NgayHetHan { get; set; }
    public DateTime? NgayCanhBao { get; set; }

    [NotMapped]
    public string? ThoiGianHetHan { get; set; }

    [NotMapped]
    public string? ThoiGianXuLy { get; set; }

    [NotMapped]
    public string? TenNhanVien { get; set; }

    [NotMapped]
    public bool isAdd { get; set; } = false;

    [NotMapped]
    public bool isXoa { get; set; } = false;

    [NotMapped]
    public string? eAction { get; set; }

    [NotMapped]
    public int Count { get; set; }

    [NotMapped]
    public List<string>? listIdUser { get; set; }

    public void SetQuyTrinhXuLy(HRM_QuyTrinhXuLy item)
    {
        IdThietLapQuyTrinh = item.IdThietLapQuyTrinh;
        IdTrangThai = item.IdTrangThai;
        IdTable = item.IdTable;
        TenTrangThai = item.TenTrangThai;
        SoQuyTrinh = item.SoQuyTrinh;
        GhiChu = item.GhiChu;
        isDaXuLy = item.isDaXuLy;
        isTrucTiepXuLy = item.isTrucTiepXuLy;
        IdUser = item.IdUser;
        IdUserUyQuyen = item.IdUserUyQuyen;
        ThoiGian = item.ThoiGian;
        IdDuAn = item.IdDuAn;
        Loai = item.Loai;
        NgayNhanViec = item.NgayNhanViec;
        NgayXuLy = item.NgayXuLy;
        NgayHetHan = item.NgayHetHan;
        NgayCanhBao = item.NgayCanhBao;
        LyDoGiamTru = item.LyDoGiamTru;
        ThoiGianGiamTru = item.ThoiGianGiamTru;
        ThoiGianXuLy_Ngay = item.ThoiGianXuLy_Ngay;
        ThoiGianXuLy_Gio = item.ThoiGianXuLy_Gio;
        ThoiGianXuLy_Phut = item.ThoiGianXuLy_Phut;
        ThoiGianNhanhCham_Ngay = item.ThoiGianNhanhCham_Ngay;
        ThoiGianNhanhCham_Gio = item.ThoiGianNhanhCham_Gio;
        ThoiGianNhanhCham_Phut = item.ThoiGianNhanhCham_Phut;
    }
}
