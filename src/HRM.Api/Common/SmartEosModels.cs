using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.Api.Common;

public class BoPhan
{
    public long Id { get; set; }
    public string? TenBoPhan { get; set; } = "";
    public string? MaBoPhan { get; set; } = "";
    public string? DienThoai { get; set; } = "";
    public string? IddmLoaiDuAn { get; set; } = "";
    public string? Fax { get; set; }
    public string? Email { get; set; } = "";
    public long LoaiBoPhanPkid { get; set; }
    public long BoPhanChaPkid { get; set; }
    public int? ThuTu { get; set; }
    public bool TrangThai { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string? ModifiedBy { get; set; }
    public long idNguoiPhuTrach { get; set; }
    public long idNguoiQuanLyTrucTiep { get; set; }
    public int Order { get; set; }
    public long idDuAn { get; set; }
    public string? TenVietTat { get; set; }
    public string? MaIPOSKeToan { get; set; }
    public bool isDoiTac { get; set; }
    public long IdKhachHang { get; set; }
    public string? DiaChi { get; set; }
    public string? MaSoThue { get; set; }
    public string? IddmCaLamViec { get; set; }
    public double SoNgayLamViec { get; set; }
    public List<long>? lstIdParent { get; set; }
    public bool isNhaMay { get; set; }
    public string? CachTinhNgoaiGio { get; set; }
    public DuAn_Item? itemThongTinDuAn { get; set; }
    public List<string> ListCT { get; set; } = new();
}

public class HRM_DuAn
{
    public long Id { get; set; }
    public string? SoPhieu { get; set; }
    public string? SoQuyTrinh => SoPhieu;
    public string? TenDuAn { get; set; }
    public string? MaDuAn { get; set; }
    public string? IddmChuDauTu { get; set; }
    public double? TongMucDauTu { get; set; }
    public string? idUserQuanLyDuAn { get; set; }
    public string? IdTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? TendmChuDauTu { get; set; }
    public string? IddmLoaiDuAn { get; set; }
    public bool isNhaMay { get; set; } = true;
    public string? DiaChi { get; set; }
    public string? GhiChu { get; set; }
    public string? TenNguoiTao { get; set; }
    public string? IddmCaLamViec { get; set; }
    public string? CreatedByName { get; set; }
    public DuAn_Item? itemThongTinDuAn { get; set; }
}

public class DuAn_Item
{
    public List<DuAn_CaLamViec>? listCaLamViec { get; set; }
    public List<DuAn_CongDem>? listPhuCapLamDem { get; set; }
    public List<DuAn_ChuyenCan>? listHoTroChuyenCan { get; set; }
}

public class DuAn_CongDem
{
    public long IdBoPhan { get; set; }
    public long IdChucVu { get; set; }
    public double MucHoTro { get; set; }
}

public class DuAn_CaLamViec
{
    public string? IddmCaLamViec { get; set; }
}

public class DuAn_ChuyenCan
{
    public string? XepHangDanhGia { get; set; }
    public double CongToiDa { get; set; }
    public double MucHoTro { get; set; }
    public double CongToiThieu { get; set; }
    public List<DuAn_ChuyenCan_DieuKien>? listDieuKienNghi { get; set; }
}

public class DuAn_ChuyenCan_DieuKien
{
    public string? LoaiNghi { get; set; }
    public double SoNgayNghi { get; set; }
}

public class PhongBan
{
    public long Id { get; set; }
    public string? TenBoPhan { get; set; }
    public string? MaBoPhan { get; set; }
    public string? MaIPOSKeToan { get; set; }
    public string? DienThoai { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public long LoaiBoPhanPkid { get; set; }
    public long BoPhanChaPkid { get; set; }
    public bool TrangThai { get; set; }
    public long idNguoiPhuTrach { get; set; }
    public long idNguoiQuanLyTrucTiep { get; set; }
    public int Order { get; set; }
    public bool isDoiTac { get; set; }
    public long IdKhachHang { get; set; }
    public long idDuAn { get; set; }
    public PhongBan? BoMon { get; set; }
}

public class ChucVu
{
    public long Id { get; set; }
    public string? TenChucVu { get; set; }
    public string? MaChucVu { get; set; }
    public long LoaiChucVuPkid { get; set; }
    public bool isQuanLy { get; set; }
}

public class ChucDanh
{
    public long Id { get; set; }
    public string? Ten { get; set; }
    public string? Ma { get; set; }
    public bool isQuanLy { get; set; }
}

public class NguoiDaiDien
{
    public long IdNguoiDaiDien { get; set; }
    public string? TenNguoiDaiDien { get; set; }
    public string? TenChucVu { get; set; }
    public long IddmChucVu { get; set; }
    public long IdCongTy { get; set; }
}

public class DanhMuc
{
    public string? Id { get; set; }
    public string? Ma { get; set; }
    public string? Ten { get; set; }
}

public class ClsDanhMuc
{
    public long Id { get; set; }
    public string? Ten { get; set; }
}

public class HDFile
{
    public long idDuAn { get; set; }
    public long IdItem { get; set; }
    public string? Name { get; set; }
    public string? NameLocal { get; set; }
    public string? Url { get; set; }
    public string? Size { get; set; }
    public bool isDeleted { get; set; }
}

public class UploadFileLenFolder
{
    public long idDuAn { get; set; }
    public long idFolder { get; set; }
    public List<HDFile>? listFileUpload { get; set; }
}

public class clsButton
{
    public bool Cho { get; set; }
    public bool ChuyenTiep { get; set; }
    public bool Ghi { get; set; }
    public bool Khac { get; set; }
    public bool KhoiTao { get; set; }
    public bool KhongDuyet { get; set; }
    public bool PhanCong { get; set; }
    public bool Xoa { get; set; }
}

public class TrangThaiQuyTrinh
{
    public string? IdTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public int LoaiTrangThai { get; set; }
    public string? IdQuytrinh { get; set; }
    public string? IdQuytrinhLienQuan { get; set; }
    public string? eActionLienQuan { get; set; }
    public string? eAction { get; set; }
    public bool SuaDuLieu { get; set; }

    [NotMapped]
    public List<QuyTrinhKhoiTao>? listKhoiTao { get; set; }
}

public class clsGetTrangThai
{
    public string? IdUser { get; set; }
    public string? eAction { get; set; }
    public string? SoQuyTrinh { get; set; }
    public long IdDuAn { get; set; }
    public string? NoiDung { get; set; }
    public string? IdTable { get; set; }
    public string? IdTrangThai { get; set; }
    public float ThoiGianT { get; set; }
    public float ThoiGianGiamTru { get; set; }
    public string? LyDoGiamTru { get; set; }
    public List<string> listIdUser { get; set; } = new();
    public string? IdTrangThaiTiepTheo { get; set; }
    public List<string>? listIdTrangThaiKhongDuyetBuocCho { get; set; }
    public string? IdTrangThaiTruocKhiDoi { get; set; }

    public clsGetTrangThai()
    {
    }

    public clsGetTrangThai(string? idUser, ChuyenTrangThai entity)
    {
        IdUser = idUser;
        eAction = entity.eAction;
        IdTable = entity.IdGuid;
        IdTrangThai = entity.IdTrangThai;
    }

    public clsGetTrangThai(string? idUser, IWorkflowEntity entity)
    {
        IdUser = idUser;
        eAction = entity.eAction;
        IdTable = entity.IdGuid;
        IdTrangThai = entity.IdTrangThai;
        listIdTrangThaiKhongDuyetBuocCho = entity.listIdTrangThaiKhongDuyetBuocCho;
        NoiDung = !string.IsNullOrEmpty(entity.NoiDungKhac) ? entity.NoiDungKhac : entity.NoiDung;
        SoQuyTrinh = entity.SoQuyTrinh;
        IdDuAn = entity.IdDuAn ?? 0;
        ThoiGianT = entity.ThoiGianT ?? 0;
        ThoiGianGiamTru = entity.ThoiGianGiamTru ?? 0;
        LyDoGiamTru = entity.LyDoGiamTru;
        IdTrangThaiTruocKhiDoi = entity.IdTrangThaiTruocKhiDoi;
        listIdUser = entity.listUserTiepTheo
            .Where(x => x.isChon && !string.IsNullOrEmpty(x.IdUser))
            .Select(x => x.IdUser!)
            .ToList();

        var trangThaiTiepTheo = entity.listTrangThaiTiepTheo.FirstOrDefault(x => x.isChon);
        if (trangThaiTiepTheo is not null)
        {
            IdTrangThaiTiepTheo = trangThaiTiepTheo.IdTrangThai;
        }
    }

    public clsGetTrangThai(string? idUser, string? eAction, string? idTable, string? soQuyTrinh, string? noiDung, long idDuAn = 0)
    {
        IdUser = idUser;
        this.eAction = eAction;
        IdTable = idTable;
        NoiDung = noiDung;
        SoQuyTrinh = soQuyTrinh;
        IdDuAn = idDuAn;
    }
}

public class clsHuyQuyTrinh
{
    public string? IdUser { get; set; }
    public List<string> listIdTable { get; set; } = new();
}

public class LM_Modify
{
    public string? TrangThai { get; set; }
    public string? eTable { get; set; }
    public string? IdQuyTrinh { get; set; }
    public string? DataTruoc { get; set; }
    public string? DataSau { get; set; }
    public long IdDuAn { get; set; }
    public string? IdUser { get; set; }
    public string? UserName { get; set; }
    public string? NoiDung { get; set; }
    public string? TenChucNang { get; set; }
}

public class HRMFaceModel
{
    public DateTime NgayDauKy { get; set; }
    public DateTime NgayCuoiKy { get; set; }
    public List<long> listIdNhanVien { get; set; } = new();
}

public class HRMFaceResponseModel
{
    public string? IdUser { get; set; }
    public string? IdFcaeNhanVien { get; set; }
    public long IdNhanVienEOS { get; set; }
    public DateTime NgayChamCong { get; set; }
    public DateTime ThoiGianChamCong { get; set; }
}

public class Link_DanhSachDuAn_NhanSu
{
    public long Id { get; set; }
    public string? IdUser { get; set; }
    public long IdDuAn { get; set; }
    public long IddmChucVu { get; set; }
    public long IddmBoPhan { get; set; }
    public long IddmGiaiDoanDuAn { get; set; }
    public DateTime NgayBatDau { get; set; }
    public string? IddmCoCau { get; set; }
    public double NgayBatDauUnix { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public double NgayKetThucUnix { get; set; }
    public string? HinhThucHopDong { get; set; }
    public string? GhiChu { get; set; }
    public string? LinkAnhDaiDien { get; set; }
    public bool isDieuDong { get; set; }
    public bool isNghiViec { get; set; }
    public bool isQuanLy { get; set; }
    public bool isXoa { get; set; }
    public string? TenNhanVien { get; set; }
    public string? Ten { get; set; }
    public string? MaNhanVien { get; set; }
}
