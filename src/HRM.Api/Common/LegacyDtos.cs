#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.Api.Entity;
using HRM.Api.Models;

namespace HRM.Api.Services
{
    public class ReturnData<T>
    {
        public T data { get; set; }
        public List<ReturnData<T>> children { get; set; }
    }

    public class PaginationSet<T>
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }

        public int Count => Items != null ? Items.Count() : 0;

        public int PageSize
        {
            set => TotalPages = value > 0 ? (int)Math.Ceiling((decimal)TotalCount / value) : 0;
        }

        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; }
    }

    public class Pagination
    {
        public dynamic Items { get; set; }
    }

    public class DM_LoaiLuongModel
    {
        public int Id { get; set; }
        public string LoaiLuong { get; set; }
    }

    public class clsLink_NhanVien_BoPhan_ChucVu
    {
        public long Id { get; set; }
        public long NhanVienPkid { get; set; }
        public long IdCongTy { get; set; }
        public long BoPhanPkid { get; set; }
        public long ChucVuPkid { get; set; }
        public bool KiemNhiem { get; set; }
        public bool isTruongBoPhan { get; set; }
        public DateTime NgayBatDau { get; set; }
        public double NgayBatDauUnix { get; set; }
        public double NgayKetThucUnix { get; set; }
        private DateTime? ngayKetThuc = null;
        public bool unActivated { get; set; }

        public DateTime? NgayKetThuc
        {
            get => ngayKetThuc.HasValue ? ngayKetThuc.Value : dungchung.UnixTimeStampToDateTime(0);
            set => ngayKetThuc = value;
        }

        public clsLink_NhanVien_BoPhan_ChucVu()
        {
        }

        public clsLink_NhanVien_BoPhan_ChucVu(HRM_HopDong item)
        {
            NhanVienPkid = item.IdNhanVienEOS;
            IdCongTy = item.IdCongTyEOS;
            BoPhanPkid = item.IdBoPhanEOS;
            ChucVuPkid = item.IddmChucVu;
            NgayBatDau = item.NgayHieuLuc;
            NgayKetThuc = item.NgayHetHan;
        }
    }

    public class HRMFaceReponeModel : HRMFaceResponseModel
    {
    }

    public class clsNhanVien
    {
        public long Id { get; set; }
        public long IdNhanVienEOS { get; set; }
        public int? IdParent { get; set; }
        public string IdUser { get; set; }
        public string IdNhanVien { get; set; }
        public string Ten { get; set; }
        public string TenDem { get; set; }
        public string TenDayDu { get; set; }
        public string Ho { get; set; }
        public string SoCMT { get; set; }
        public string Email { get; set; }
        public string DienThoai { get; set; }
        public string ChucVu { get; set; }
        public string TenBoPhan { get; set; }
        public string TenNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public long IddmBoPhan { get; set; }
        public long IdBoPhan { get; set; }
        public long IddmChucVu { get; set; }
        public long IddmChucDanh { get; set; }
        public string TenChucVu { get; set; }
        public string TenChucDanh { get; set; }
        public string UserPkid { get; set; }
        public string DiaChiPkid { get; set; }
        public string UserName { get; set; }
        public string CCCD { get; set; }
        public double SoPhepConLai { get; set; }
        public double ThamNien { get; set; }
        public double CongLamThemGio { get; set; }
        public double NgaySinhUnix { get; set; }
        public string GioiTinh { get; set; }
        public List<long> ListIdDuAn { get; set; }
        [NotMapped]
        public long? IdDuAnDieuDong { get; set; }
        [NotMapped]
        public long? IdDuAnCurrent { get; set; }

        public clsNhanVien Clone() => (clsNhanVien)MemberwiseClone();

        public clsNhanVien()
        {
        }

        public clsNhanVien(HRM_NhanVien nv)
        {
            Ten = nv.Ten;
            TenDem = nv.TenDem;
            Ho = nv.Ho;
            MaNhanVien = nv.MaNhanVien;
            TenDayDu = nv.TenDayDu;
            DiaChiPkid = nv.ChoOHienNayTruDiaChi;
            UserPkid = nv.IdUser;
            if (nv.listEmail != null)
                Email = nv.listEmail.Select(a => a.Email).FirstOrDefault();
            if (nv.listGiayToTuyThan != null)
                CCCD = nv.listGiayToTuyThan.Select(a => a.So).FirstOrDefault();
            if (nv.listSoDienThoai != null)
                DienThoai = nv.listSoDienThoai.Select(a => a.SoDienThoai).FirstOrDefault();
            IdNhanVienEOS = nv.IdNhanVienEOS;
            IddmBoPhan = nv.IdBoPhanEOS;
            IddmChucVu = nv.IdChucVu;
            UserName = nv.UserName;
            NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(nv.NgaySinh);
            if (nv.IdGioiTinh != null)
            {
                if (nv.IdGioiTinh.ToLower() == "nam")
                    GioiTinh = "0";
                else if (nv.IdGioiTinh.ToLower() == "nữ")
                    GioiTinh = "1";
                else
                    GioiTinh = "2";
            }
            if (nv.listIdCongTy != null)
                ListIdDuAn = nv.listIdCongTy;
            IdDuAnDieuDong = nv.IdDuAnDieuDong;
            IdDuAnCurrent = nv.IdDuAnCurrent;
        }
    }

    public class QT_ThietLapQuyTrinh
    {
        public string Id { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string GhiChu { get; set; }
        public string Link { get; set; }
        public int STT { get; set; }
        public int? Loai { get; set; }
        public bool? isHoSoTheoQuyTrinh { get; set; }
        public bool? isGhiSauKetThuc { get; set; }
        public string IdParent { get; set; }
        public bool isPhatSinh { get; set; }
        public string eAction { get; set; }
    }

    public class clsNhanSu_DuAn
    {
        public long IdDuAn { get; set; }
        public double NgayBatDauUnix { get; set; }
        public List<string> listIdUser { get; set; } = new();
    }

    public class QuyTrinhCon
    {
        public string Id { get; set; }
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string eAction { get; set; }
    }

    public class HRM_DanhMuc
    {
        public int Id { get; set; }
        public string Ten { get; set; }
    }

    public class HRM_hungGio
    {
        public string IdCaLamViec { get; set; }
        public double TuGioUnix { get; set; }
        public double DenGioUnix { get; set; }
        public double? SoGioNghi { get; set; }
    }

    public class Notification
    {
        public long Id { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string Link { get; set; }
        public string LoaiThongBao { get; set; }
        public long idItemLienQuan { get; set; }
        public string IdQuyTrinh { get; set; }
        public int TrangThai { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class Notifications
    {
        public string NguoiGui { get; set; }
        public List<string> listNguoiNhan;
        public Notification Item;
    }

    public class TimKiemBangLuong
    {
        public long IdCongTy { get; set; }
        public long IdCongty { get; set; }
        public int LoaiLuong { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string Keyword { get; set; }
        public string eAction { get; set; }
        public string IdThuVienDanhGia { get; set; }
        public double Ngay { get; set; }
        public double TuNgay { get; set; }
        public double DenNgay { get; set; }
        public List<string> listIdNhanVien { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public List<long> listIdCongTy { get; set; }
        public List<string> listIdKyChamCong { get; set; }
    }

    public class TimKiemImport
    {
        public string FileName { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public List<string> listIdNhanVien { get; set; }
        public List<HRM_QuyetDinhDieuChinhLuong_NhanVien> listData { get; set; }
    }

    public class TimKiem
    {
        public string KeyWord { get; set; }
        public string Keyword { get; set; }
        public int Loai { get; set; }
        public string LoaiCauHoi { get; set; }
        public string IddmNhomCauHoi { get; set; }
        public string IdLoaiHopDong { get; set; }
        public bool isPhuLuc { get; set; }
        public string IddmNganHangNguon { get; set; }
        public string IdLoaiKhaoSat { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }
        public int? TrangThai { get; set; }
        public double? TuNgayUnix { get; set; }
        public double? DenNgayUnix { get; set; }
        public double? TuNgay { get; set; }
        public double? DenNgay { get; set; }
        public long? IDdmCoSo { get; set; }
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public int? LoaiTrangThai { get; set; }
        public int LoaiLuong { get; set; } = 0;
        public string IdTrangThai { get; set; }
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public long? IddmChucDanh { get; set; }
        public long? IdBoPhan { get; set; }
        public long? IdChucVu { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public List<long> listIdCongTy { get; set; }
        public long? IdBoPhanEOSCu { get; set; }
        public long? IdCongTy { get; set; }
        public long? IdDuAn { get; set; }
        public string TenTrangThai { get; set; }
        public long? IdBoPhanEOS { get; set; }
        public long? IdCongTyEOS { get; set; }
        public int? TabTrangThai { get; set; }
        public string IddmDotChamCong { get; set; }
        public string CCCD { get; set; }
        public string GioiTinh { get; set; }
        public bool isCongNhan { get; set; } = false;
        public string LoaiBH { get; set; }
        public bool isKhaoSat { get; set; } = false;
        public List<string> listIddmLoaiDuAn { get; set; }
    }

    public class TimKiemChung
    {
        public string KeyWord { get; set; }
        public double TuNgayUnix { get; set; }
        public double DenNgayUnix { get; set; }
        public long IddmChucDanh { get; set; }
        public long IdBoPhan { get; set; }
        public long IdChucVu { get; set; }
        public long IdCongTy { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }
    }

    public class TimKiemKeHoachTuanImport
    {
        public string FileName { get; set; }
        public List<long> listIdBoPhan { get; set; }
        public double DenNgayUnix { get; set; }
        public double TuNgayUnix { get; set; }
    }
    public class DashboardCongNhanVien
    {
        public string IdNhanVien { get; set; }
        public string IdUser { get; set; }
        public double NgayCongTieuChuan { get; set; }
        public double SoGioMotNgay { get; set; }
        public double LuongCoBan { get; set; }
        public bool isDuGio { get; set; } = false;
        public double SoNgayNgayPhep { get; set; }
        public double DaNghi { get; set; } = 0;
        public double ConLai { get; set; }
        public double TongSoCong { get; set; }
        public bool isNgayLe { get; set; } = false;
        public List<int> listNgayLe { get; set; }
        public bool isCongNhan { get; set; } = false;
        public double TongSoCongTrongThang { get; set; }
        public string TongSoCongLeTrongThang { get; set; }
        public double TongSoGioTrongThang { get; set; }
        public double TongSoGioVuotThang { get; set; }
        public double LuongNgayCong { get; set; }
        public double LuongNgoaiGio { get; set; }
    }


    public class NguoiDaiDienCongTyViewModel
    {
        public string IdNguoiDaiDien { get; set; }
        public string TenNguoiDaiDien { get; set; }
        public string Ma { get; set; }
        public long IddmChucDanh { get; set; }
        public string TenChucDanh { get; set; }
        public long IddmChucVu { get; set; }
        public string TenChucVu { get; set; }
        public string IddmQuocTich { get; set; }
        public string TenQuocTich { get; set; }
        public long IdCongTy { get; set; }
    }
}

namespace HRM.Api.Services.NhanVien
{
    public class clsDiMuonVeSom
    {
        public int SoPhut { get; set; }
        public string IddmKyHieuChamCong { get; set; }
        public string IdNhanVien { get; set; }
        public string IddmHeSoTangCa { get; set; }
        public DateTime Ngay { get; set; }
        public int Loai { get; set; }
        public bool isCaGay { get; set; }
        public DateTime GioVao { get; set; }
        public double GioTieuChuan { get; set; }
        public long IdCongTy { get; set; }

        public clsDiMuonVeSom() { }

        public clsDiMuonVeSom(HRM_CheckInOut item)
        {
            Ngay = item.Ngay;
            IdNhanVien = item.IdNhanVien;
        }

        public clsDiMuonVeSom(HRM_ThoiGianLamViecKeHoach item)
        {
            Ngay = item.Ngay;
            IdNhanVien = item.IdNhanVien;
            GioTieuChuan = item.GioTieuChuan;
            IdCongTy = item.IdCongTy;
            IddmHeSoTangCa = item.IddmHeSoTangCa;
        }

        [NotMapped]
        public string KyHieuChamCong { get; set; }
    }

    public class clsTieuChiDanhGia_HeaderCol
    {
        public string IdThuVienDanhGiaTieuChi { get; set; }
        public string title { get; set; }
        public int? rowspan { get; set; }
        public double? DiemToiDa { get; set; }
        public int? colspan { get; set; }
        public int SoThapPhan { get; set; } = 2;
        public bool isDisable { get; set; } = false;
        public int STT { get; set; }
        public bool isHaveChild { get; set; } = false;
        public string BoPhanDanhGia { get; set; }
        public double? TyTrongQLCS { get; set; }
        [NotMapped]
        public bool? isRoot { get; set; } = false;
        [NotMapped]
        public bool? isShow { get; set; } = false;
        [NotMapped]
        public bool? isXem { get; set; } = false;
        [NotMapped]
        public bool? isCapNhat { get; set; } = false;

        public clsTieuChiDanhGia_HeaderCol() { }

        public clsTieuChiDanhGia_HeaderCol(HRM_dmNhomDanhGia dmNhomDanhGia, int? _rowspan = null, int? _colspan = null)
        {
            title = dmNhomDanhGia.Ten;
            rowspan = _rowspan;
            colspan = _colspan;
        }

        public clsTieuChiDanhGia_HeaderCol(HRM_ThuVienDanhGia_Item dmTieuChi, int? _rowspan = null, int? _colspan = null)
        {
            IdThuVienDanhGiaTieuChi = dmTieuChi.IdGuid;
            title = dmTieuChi.Ten;
            rowspan = _rowspan;
            colspan = _colspan;
            DiemToiDa = dmTieuChi.DiemToiDa;
            BoPhanDanhGia = dmTieuChi.BoPhanDanhGia;
            TyTrongQLCS = dmTieuChi.TyTrongQLCS;
            isHaveChild = false;
            isRoot = dmTieuChi.isRoot;
            isCapNhat = dmTieuChi.isCapNhat;
            isXem = dmTieuChi.isXem;
            isShow = dmTieuChi.isShow;
        }
    }

    public class clsTieuChiDanhGia_HeaderRow
    {
        public List<clsTieuChiDanhGia_HeaderCol> listCol = new();
    }

    public class clsTieuChiDanhGia_Header
    {
        public List<clsTieuChiDanhGia_HeaderRow> listCol = new();
        public List<clsTieuChiDanhGia_HeaderCol> listHienThi = new();
    }

    public class clsTieuChiDanhGia_Data_Item
    {
        public string IdThuVienTieuChiDanhGia { get; set; }
        public double? GiaTri { get; set; }
        public double? DiemToiDa { get; set; }
        public bool isDisable { get; set; } = false;
        public int SoSauDauPhay { get; set; } = 0;
        public int SapXep { get; set; }
        public string BoPhanDanhGia { get; set; }
        public double? TyTrongQLCS { get; set; }
        public string GhiChu { get; set; }
        [NotMapped]
        public bool? isRoot { get; set; } = false;
        [NotMapped]
        public bool? isShow { get; set; } = false;
        [NotMapped]
        public bool? isXem { get; set; } = false;
        [NotMapped]
        public bool? isCapNhat { get; set; } = false;

        public clsTieuChiDanhGia_Data_Item() { }

        public clsTieuChiDanhGia_Data_Item(double? _GiaTri, int _SoSauDauPhay = 2, bool _isDisable = true)
        {
            GiaTri = _GiaTri;
            SoSauDauPhay = _SoSauDauPhay;
            isDisable = _isDisable;
        }
    }

    public class clsTieuChiDanhGia_Data
    {
        public int? STT { get; set; }
        public string IdNhanVien { get; set; }
        public long IdNhanVienEOS { get; set; }
        public string TenNhanVien { get; set; }
        public string MaNhanVien { get; set; }
        public string ChucVu { get; set; }
        public string TenChucDanh { get; set; }
        public string bold { get; set; }
        public string color { get; set; }
        public List<clsTieuChiDanhGia_Data_Item> listGiaTri { get; set; } = new();

        public clsTieuChiDanhGia_Data() { }

        public clsTieuChiDanhGia_Data(HRM_NhanVien nhanVien)
        {
            IdNhanVien = nhanVien.IdGuid;
            IdNhanVienEOS = nhanVien.IdNhanVienEOS;
            TenNhanVien = nhanVien.TenDayDu;
            MaNhanVien = nhanVien.MaNhanVien;
        }

        public clsTieuChiDanhGia_Data(
            HRM_NhanVien nhanVien,
            List<clsTieuChiDanhGia_HeaderRow> listHeader,
            List<HRM_DanhGiaNhanVien_Item> listNhanVien,
            int _STT,
            string IdUser)
        {
            IdNhanVien = nhanVien.IdGuid;
            IdNhanVienEOS = nhanVien.IdNhanVienEOS;
            TenNhanVien = nhanVien.TenDayDu;
            MaNhanVien = nhanVien.MaNhanVien;
            STT = _STT;

            int Col = 0;
            foreach (var itemHeader in listHeader)
            {
                foreach (var itemCol in itemHeader.listCol)
                {
                    if (itemCol.colspan != 1) continue;
                    Col++;
                    if (Col < 4) continue;
                    if (!itemCol.isHaveChild)
                    {
                        var itemAdd = new clsTieuChiDanhGia_Data_Item();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                        {
                            var itemFind = listNhanVien.Find(x => x.IdUser == IdUser && x.IdThuVienTieuChiDanhGia == itemCol.IdThuVienDanhGiaTieuChi)
                                ?? listNhanVien.Where(x => x.IdThuVienTieuChiDanhGia == itemCol.IdThuVienDanhGiaTieuChi)
                                    .OrderByDescending(a => a.Modified)
                                    .FirstOrDefault();
                            if (itemFind != null)
                            {
                                itemAdd.GiaTri = itemFind.Diem;
                                itemAdd.DiemToiDa = itemFind.DiemToiDa;
                                itemAdd.GhiChu = itemFind.GhiChu;
                            }
                        }

                        itemAdd.DiemToiDa ??= itemCol.DiemToiDa;
                        itemAdd.SapXep = itemCol.STT;
                        itemAdd.IdThuVienTieuChiDanhGia = itemCol.IdThuVienDanhGiaTieuChi;
                        itemAdd.isDisable = itemCol.isDisable;
                        itemAdd.SoSauDauPhay = itemCol.SoThapPhan;
                        itemAdd.BoPhanDanhGia = itemCol.BoPhanDanhGia;
                        itemAdd.TyTrongQLCS = itemCol.TyTrongQLCS;
                        itemAdd.isRoot = itemCol.isRoot;
                        itemAdd.isCapNhat = itemCol.isCapNhat;
                        itemAdd.isXem = itemCol.isXem;
                        itemAdd.isShow = itemCol.isShow;
                        listGiaTri.Add(itemAdd);
                    }
                }
            }

            listGiaTri = listGiaTri.OrderBy(a => a.SapXep).ToList();
        }
    }
}

namespace HRM.Api.Services.NhanVien
{
    public class Tu_DenNgay
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
    }
}
