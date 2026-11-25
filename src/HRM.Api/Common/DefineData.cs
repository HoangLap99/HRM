namespace HRM.Api.Common;

/// <summary>
/// Tập trung toàn bộ hằng số nghiệp vụ, enum trạng thái, mã quy trình và hàm quy đổi dùng chung của HRM.
/// </summary>
public static class DefineData
{
    public const double DateMinUnix = 0;
    public const string MaNhanVien = "NV_";
    public const string MaQuyetDinhDieuChinhLuong = "QDDCL";
    public const string MaQuyTrinhXacNhanTangGiamBaoHiem = "XNTGBH";
    public const string MaQuyetDinhDiCongTac = "QDCT";
    public const string MaQuyetDinhBoNhiemChucVu = "QDBNCV";
    public const string MaQuyetDinhBoNhiemDieuDong = "QDBNDD";
    public const string MaQuyetDinhKhenThuong = "QDKT";
    public const string MaQuyetDinhKyLuat = "QDKL";
    public const string MaQuyetDinhThoiViec = "QDTV";
    public const string MaQuyetDinhSaThai = "QDST";
    public const string MaHopDong = "HD";
    public const string MaHopDongChuyenGia = "HDCG";
    public const string MaDeNghiTuyenDungTongHop = "DNTDTH";
    public const string MaDeNghiTuyenDung = "DNTD";
    public const string MaChamCong = "BCC";
    public const string MaPhieuBangLuong = "BL";
    public const string MaPhieuQuyetDinhNghiViec = "QDNV";
    public const string sURLSmartEOS = "URLSmartEOS";
    public const string MaKeHoachCongTac = "KHCT";
    public const string MaKetQuaCongTac = "KQCT";
    public const string MaQuyTrinhXacNhanNgoaiGio = "XNNG";
    public const string MaQuyTrinhDanhGia = "DGNV";

    #region Tuyển dụng
    public const string MaKeHoachTuyenDung = "KHTD";
    public const string MaTHDeNghiTuyenDung = "THDN";
    public const string MaTHKeHoachTuyenDung = "THKHTD";
    public const string MaKeHoachTuyenDungTongHop = "KHTDTH";
    public const string MaUngVien = "";
    public const string MaTuyenDung = "TD";
    public const string MaPheDuyetLuongUV = "PDLUV";
    public const string MaKEHOACHPHUCLOI = "KHPL";
    public const string MaQUYTRINHPHUCLOI = "QTPL";
    public const string MAQUYETTOANPHEP = "QTPN";
    #endregion

    #region Hình thức làm việc
    public const string LamViecThang = "HĐLĐ";
    public const string LamViecTheoGio = "TTTHCV";
    public const string LamViecTheoCa = "";
    #endregion

    #region Đào tạo
    public const string MaKeHoachDaoTao = "KHDT";
    public const string MaKeHoachDaoTaoTongHop = "KHDTTH";
    public const string DotDaoTao = "DDT";
    public const string HRM_ChuongTrinhDaoTao = "CTDT";
    #endregion

    public enum eModify
    {
        Them,
        Sua,
        Xoa,
        ChuyenTiep,
        KhongDuyet,
        KhongDuyetBuocCho,
        Import,
        Export,
    }

    #region QuyTrinhChung
    public enum ePheDuyetTaiLieu
    {
        KhongDuyet = 0,
        PheDuyet = 1,
        ChoDuyet = 2,
        PheDuyetPhapLy = 3,
    }

    public enum eQuyTrinh_LoaiLienQuan
    {
        ChuyenTiep = 1,
        KhongDuyet = 2,
        Cho = 3,
    }

    public enum eQuyTrinh_LoaiTrangThai
    {
        KhoiTaoNhanVien = 1,
        KetThuc = 2,
        XuLy = 3,
        XuLyDacBiet = 17,
        Khac = 4,
        Xem = 5,
        Cho = 6,
        KhoiTaoTruongBoPhan = 11,
        Huy = 12,
        UyQuyen = 13,
        PhanCong = 16,
    }

    public enum eQuyTrinh_LoaiHam
    {
        Set = 0,
        ChuyenTiep = 1,
        KhongDuyet = 2,
    }
    #endregion

    public enum eTable
    {
        HRM_QuyTrinhXacNhanTangGiamBaoHiem,
        HRM_QuyTrinhDanhGia,
        HRM_QuyTrinhXacNhanNgoaiGio,
        HRM_ChuongTrinhDaoTao,
        HRM_QuyTrinhDuAn,
        HRM_NhanVien,
        HRM_NhanVien_FileDinhKemHoSo,
        HRM_BangCap,
        HRM_ThanhVienHiepHoi,
        HRM_NgonNguNhanVien,
        HRM_ChungChiNhanVien,
        HRM_QuaTrinhCongTac,
        HRM_QuaTrinhCongTacNuocNgoai,
        HRM_DuAnNhanVien,
        HRM_QuanHeGiaDinhNhanVien,
        HRM_BoNhiemDieuDongDuAn,
        HRM_BoNhiemChucVu,
        HRM_QuyetDinhDiCongTac,
        HRM_QuyetDinhDieuChinhLuong,
        HRM_QuyetDinhKhenThuong,
        HRM_QuyetDinhKyLuat,
        HRM_QuyetDinhThoiViec,
        HRM_QuyetDinhSaThai,
        HRM_HopDong,
        HRM_PhuLucHopDong,
        HRM_HopDongChuyenGia,
        HRM_BangChamCong,
        HRM_QuyTrinhTinhLuong,
        HRM_TuyenDung_QuyTrinhPheDuyetLuong,
        HRM_TuyenDung_NhuCau,
        HRM_TuyenDung_KeHoach,
        HRM_TuyenDung_KeHoach_TongHop,
        HRM_TuyenDung_DeNghi,
        HRM_TuyenDung_DeNghi_TongHop,
        HRM_TuyenDung_DeNghi_TongHopChiTiet,
        HRM_TuyenDung_DeNghi_TongHop_Dot,
        HRM_TuyenDung_QuyTrinh,
        HRM_TuyenDung_UngVien,
        HRM_TuyenDung_NguoiPhongVan,
        HRM_TuyenDung_PheDuyet,
        HRM_TuyenDung_ThuMoiLamViec,
        HRM_TuyenDung_QuyTrinhPhongVan,
        HRM_DaoTaoTaiLieu,
        HRM_QuyTrinhCongTacPhiChiTiet,
        HRM_QuyTrinhLamNgoaiGioTongHop,
        HRM_QuyTrinhCongTacPhiTongHop,
        HRM_QuyTrinhXinNghiPhep,
        HRM_ChamCongVanPhongTongHop,
        HRM_ChamCongDuAn,
        HRM_QuyTrinhCongTacPhiDuAnChiTiet,
        HRM_DinhBien,
        HRM_dmTieuChi,
        HRM_QuyTrinhDinhBien,
        HRM_ChamCong_Detail,
        HRM_BangLuong,
        HRM_QuyetDinhNghiViec,
        HRM_QuyTrinhXuLy,
        HRM_NhatKyNhanVien,
        HRM_KeHoachPhucLoi,
        HRM_QuyTrinh_PhucLoi,
        HRM_QuyetToanPhepNam,
        HRM_KeHoachDaoTao,
        HRM_KeHoachDaoTaoTongHop,
        HRM_Dot_DaoTao,
        HRM_Dot_DaoTaoChiTiet,
        HRM_dmKyHieuChamCong,
        HRM_QuyTrinhLamThemGio,
        HRM_KeHoachCongTac,
        HRM_KetQuaCongTac
    }

    public enum eCachTinhNgoaiGio
    {
        TheoCongTieuChuan,
        TheoNgay
    }

    public enum eTrangThaiThayDoiThongTin
    {
        ChoDuyet,
        DaDuyet,
        KhongDuyet
    }

    public enum eFlagDinhKem
    {
        AnhDaiDien,
        FileDinhKem,
        FileChamCongDuAnPM
    }

    public enum eTrangThaiDotThi
    {
        ChuaThi,
        DangThi,
        KetThuc
    }

    public enum eMaChucVu
    {
        CN,
        GS,
        NV
    }

    public enum eMaChucDanh
    {
        CN,
        GS,
        NV
    }

    public enum eLyDoTangGiamBH
    {
        ThaiSan,
        KhongLuong
    }

    public enum eTTQuyTrinh : byte
    {
        ChuaChuyen = 0,
        ChoDuyet = 1,
        ChamDut = 2,
        KhongDuyet1 = 3,
        DaDuyet1 = 4,
        ChamDut1 = 5,
        KhongDuyet2 = 6,
        DaDuyet2 = 7,
        ChamDut2 = 8,
        KhongDuyet3 = 9,
        DaDuyet3 = 10,
        ChamDut3 = 11,
        KhongDuyet4 = 12,
        DaDuyet4 = 13,
        ChamDut4 = 14,
        KhongDuyet5 = 15,
        DaDuyet5 = 16,
        ChamDut5 = 17,
    }

    public enum eLoaiCauhoi
    {
        ChonABCD,
        SapXep,
        NhapText,
        KyNang
    }

    public enum eBoPhanDanhGia
    {
        HCNS,
        KETOAN
    }

    public enum eErrorMessage : int
    {
        ChuaDangNhap = 18,
        LoiCapNhat,
        LoiLayDuLieu,
        DangNhapThanhCong,
        CapNhatThanhCong = 4,
        KhongDuQuyen,
        ChuaKhoiDongQuyTrinh
    }

    public enum eLoaiCongTacPhi
    {
        TrongNuoc,
        NgoaiNuoc
    }

    public enum eLoaiTienTe
    {
        TrongNuoc,
        NgoaiNuoc
    }

    public enum edmHinhThucKyLuat
    {
        PhatTien,
    }

    public enum eTrangThaiNhanVien
    {
        DaNghiViec,
        DangLamViec,
        NghiKhongLuong,
        NghiThaiSan,
        NghỉCheDo,
        NghiOm,
        NghiBenhNgheNghiep,
        NghiTaiNanLaoDong,
        NghiHuu
    }

    public enum eTrinhDoDaoTao
    {
        TrenDaiHoc,
        DaiHoc,
        CaoDang,
        Khac
    }

    public enum eTrinhDoNgoaiNgu
    {
        XuatSac,
        TrungBinh,
        Kha,
        Kem,
        Gioi
    }

    public enum eTrinhDoChungChi
    {
        A,
        B,
        C
    }

    public enum eTrangThaiBoNhiemDieuDongDuAn
    {
        ChuaPheDuyet,
        DangThucHien,
        HoanThanh
    }

    public enum eTrangThaiBoNhiemChucVu
    {
        ChuaPheDuyet,
        DaPheDuyet,
        HetHieuLuc
    }

    public enum eTrangThaiQuyetDinhCongTac
    {
        ChuaPheDuyet,
        DaPheDuyet,
        DangThucHien,
        HoanThanh,
        HetHieuLuc,
    }

    public enum eTrangThaiQuyetDinhDieuChinhLuong
    {
        ChuaPheDuyet,
        DaPheDuyet,
        HetHieuLuc
    }

    public enum eTrangThaiQuyetDinhKhenThuong
    {
        ChuaPheDuyet,
        DaPheDuyet,
        HetHieuLuc
    }

    public enum eTrangThaiQuyetDinhKyLuat
    {
        ChuaPheDuyet,
        DaPheDuyet,
        HetHieuLuc
    }

    public enum eTrangThaiQuyetDinhThoiViec
    {
        ChuaPheDuyet,
        DaPheDuyet,
    }

    public enum eTrangThaiQuyetDinhSaThai
    {
        ChuaPheDuyet,
        DaPheDuyet,
    }

    public enum ePhamViCongTac
    {
        TrongNuoc,
        NgoaiNuoc
    }

    public enum eLoaiMucPhi
    {
        NoiQuy,
        KhoanCongTacPhi,
        KhoanKhachSan
    }

    public enum eLoaiHopDongNhanVien
    {
        DaiHan,
        DuAn
    }

    public enum eLoaiPhuLucHopDong
    {
        GiaHan,
        ThayDoiLuong,
        CaHai
    }

    public enum eTrangThaiPhuLucHopDong
    {
        ChuaKy,
        HieuLuc,
        HetHieuLuc
    }

    public enum eTrangTHaiHopDong
    {
        SapHetHan,
        DaHetHan,
        ChuaKy,
        HieuLuc,
        HetHieuLuc
    }

    public static readonly Dictionary<string, string> dicTenTrangThaiHopDong = new()
    {
        { eTrangTHaiHopDong.ChuaKy.ToString(), "Chưa ký" },
        { eTrangTHaiHopDong.HieuLuc.ToString(), "Hiệu lực" },
        { eTrangTHaiHopDong.HetHieuLuc.ToString(), "Hết hiệu lực" }
    };

    public enum eTabTrangThai
    {
        ChuaXuLy,
        DaXuLy
    }

    public enum eTrangThaiKeHoachTuyenDung
    {
        ChuaChuyenDuyet,
        DaChuyenTruongPhongNhanSu
    }

    public enum eTrangThaiKeHoachTuyenDungTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eTrangThaiKeHoachDaoTao
    {
        ChuaChuyenDuyet,
        ChoTongHop,
        DaTongHop
    }

    public enum eTrangThaiKeHoachDaoTaoTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        DaDuyet
    }

    public enum eKyHieuChamCong
    {
        M,
        S,
        N,
        DL,
        I,
        O,
        G,
        P,
        NL,
        NKL,
        NTL,
        NC,
        NP,
        PN,
        NT,
        NK,
        No,
        ND,
        NCG,
        NTS,
        BV,
        NV,
        NTD,
        Đ,
        HV,
        XTC,
        T,
        NQĐ,
        CT,
        N100,
        N50
    }

    public enum eLoaiGiaoVien
    {
        TrongCongTy,
        ThueNgoai
    }

    public enum eKetQuaDaoTao
    {
        XuatSac,
        Gioi,
        Kha,
        TrungBinh,
        Kem
    }

    public enum eDaoTaoNguonChi
    {
        QuyCongTy
    }

    public enum eTrangThaiKeHoachPhucLoi
    {
        ChuaChuyenDuyet,
        DangThucHien,
        HoanThanh
    }

    public enum eTrangThaiPhucLoi
    {
        ChuaChuyenDuyet,
        DangThucHien,
        HoanThanh
    }

    public enum eTrangThaiQuyTrinhLamNgoaiGio
    {
        ChuaChuyenDuyet,
        ChoTruongPhongDuyet,
        TruongPhongKhongDuyet,
        ChuaTongHop,
        DaTongHop
    }

    public enum eTrangThaiQuyTrinhLamNgoaiGioTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eLoaiChucDanh
    {
        ThongThuong = 0,
        DuAn = 1
    }

    public enum eLoaiCongTac
    {
        TrongNuoc = 0,
        NgoaiNuoc = 1
    }

    public enum eTinhTrangLamViec
    {
        DangLamViec,
        DaNghi,
        NghiThaiSan,
        NghiKhongLuong,
    }

    public enum eGioiTinh
    {
        Nam,
        Nu,
        KhongXacDinh
    }

    public static readonly Dictionary<string, string> dicGioiTinh = new()
    {
        { eGioiTinh.Nam.ToString(), "Nam" },
        { eGioiTinh.Nu.ToString(), "Nữ" },
        { eGioiTinh.KhongXacDinh.ToString(), "Không xác định" },
    };

    public enum eTrangThaiQuyTrinhCongTacPhi
    {
        ChuaChuyenDuyet,
        ChoTruongPhongDuyet,
        TruongPhongKhongDuyet,
        ChoPhongHanhChinhDuyet,
        PhongHanhChinhKhongDuyet,
        ChuaTongHop,
        DaTongHop
    }

    public enum eTrangThaiQuyTrinhCongTacPhiTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eTrangThaiQuyTrinhXinNghiPhep
    {
        ChuaChuyenDuyet = 0,
        ChoTruongPhongDuyet = 1,
        TruongPhongKhongDuyet = 2,
        ChoTruongPhongNhanSuDuyet = 3,
        TruongPhongNhanSuKhongDuyet = 4,
        ChoGiamDocDuyet = 5,
        GiamDocKhongDuyet = 6,
        DaDuyet = 7
    }

    public enum eTrangThaiChamCongVanPhong
    {
        ChuaChuyenDuyet,
        ChoTruongPhongDuyet,
        TruongPhongKhongDuyet,
        ChuaTongHop,
        DaTongHop
    }

    public enum eTrangThaiChamCongVanPhongTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eTrangThaiChamCongDuAn
    {
        ChuaChuyenDuyet,
        ChoPhongNhanSuDuyet,
        PhongNhanSuKhongDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eTrangThaiQuyTrinhCongTacPhiDuAn
    {
        ChuaChuyenDuyet,
        ChoTongHop,
        DaTongHop
    }

    public enum eTrangThaiQuyTrinhCongTacPhiDuAnTongHop
    {
        ChuaChuyenDuyet,
        ChoGiamDocDuyet,
        GiamDocKhongDuyet,
        GiamDocDaDuyet
    }

    public enum eLoaiSangChieu
    {
        Sang,
        Chieu
    }

    public enum eLoaiThangQuy
    {
        Thang,
        Quy,
        Nam,
        NuaNam
    }

    public enum eLoaiBangLuong
    {
        LuongVanPhong,
        LuongDuAn
    }

    public enum eAction
    {
        QUYTRINHDUAN,
        DINHBIENCOSO,
        THONGTINNHANVIEN,
        QUYTRINHHOPDONGNHANSU,
        KEHOACHTUYENDUNG,
        KEHOACHTUYENDUNGTONGHOP,
        NHUCAUTUYENDUNG,
        DENGHITUYENDUNG,
        DENGHITUYENDUNGTONGHOP,
        QUYTRINHPHEDUYETLUONGUNGVIEN,
        QUYTRINHTUYENDUNG,
        QUYTRINHQUYETDINHTHOIVIEC,
        THOIVIEC,
        PHEDUYETTUYENDUNG,
        THUMOILAMVIEC,
        KEHOACHDAOTAO,
        KEHOACHDAOTAOTONGHOP,
        KHOADAOTAO,
        XACNHANTANGGIAMBAOHIEM,
        QUYTRINHDINHBIEN,
        QUYTRINHQUYETDINHKYLUAT,
        QUYTRINHQUYETDINHSATHAI,
        QUYTRINHQUYETDINHKHENTHUONG,
        QUYTRINHDIEUCHINHLUONG,
        QUYTRINHBONHIEMCHUCVU,
        QUYTRINHBONHIEMCHUCVUDUAN,
        QUYTRINHBONHIEMDIEUDONG,
        QUYTRINHCHAMCONG,
        QUYTRINHLUONG,
        QUYTRINHTINHLUONG,
        QUYTRINHTINHLUONGNHANVIEN,
        QUYTRINHTINHLUONGCONGNHAN,
        QUYTRINHTINHLUONGGIAMSAT,
        QUYTRINHTINHLUONGGIAMSATSAMSUNG,
        QUYTRINHDIMUONVESOM,
        QUYTRINHXINNGHIPHEP,
        QUYTRINHLAMTHEMGIO,
        QUYTRINHQUENCHAMCONG,
        QUYTRINHNGHIVIEC,
        QUYTRINHKEHOACHPHUCLOI,
        QUYTRINHPHUCLOI,
        QUYTRINHQUYETTOANPHEP,
        QUYTRINHKEHOACHDAOTAO,
        QUYTRINHTONGHOPKEHOACHDAOTAO,
        QUYTRINHDOTDAOTAO,
        CHUONGTRINHDAOTAO,
        KEHOACHLAMVIECTUAN,
        XACNHANDIMUONVESOM,
        DANHGIANHANVIEN,
        KEHOACHCONGTAC,
        KETQUACONGTAC,
        XACNHANNGOAIGIO,
        HOSONHANVIEN,
        TUYENDUNGUNGVIEN,
        DIENGIAICHAMCONG,
        QUYTRINHNGHIVIECNHANVIEN,
        QUYTRINHNGHIVIECGIAMSAT,
        QUYTRINHNGHIVIECCONGNHAN,
        QUYTRINHKHENTHUONGNHANVIEN,
        QUYTRINHKHENTHUONGGIAMSAT,
        QUYTRINHKHENTHUONGCONGNHAN,
        QUYTRINHKYLUATCONGNHAN,
        QUYTRINHKYLUATGIAMSAT,
        QUYTRINHKYLUATNHANVIEN,
        DIEUCHINHLUONGNHANVIEN,
        DIEUCHINHLUONGGIAMSAT,
        DIEUCHINHLUONGCONGNHAN,
        TANGGIAMBAOHIEMNHANVIEN,
        TANGGIAMBAOHIEMCONGNHAN,
        TANGGIAMBAOHIEMGIAMSAT,
        QUYTRINHDIEUDONGNHANVIENGIAMSAT,
        QUYTRINHDIEUDONGCONGNHAN,
        QUYTRINHDIEUDONGTOTRUONG,
        BONHIEMCHUCVUNHANVIENGIAMSAT,
        BONHIEMCHUCVUCONGNHAN,
        BONHIEMCHUCVUTOTRUONG,
        QUYTRINHPHUCLOINHANVIEN,
        QUYTRINHPHUCLOICONGNHANGIAMSAT,
        KEHOACHDAOTAONHANVIEN,
        KEHOACHDAOTAOCONGNHANGIAMSAT,
        CHUONGTRINHDAOTAONHANVIEN,
        CHUONGTRINHDAOTAOCONGNHANGIAMSAT
    }

    public enum eMaNghiPhep
    {
        PN,
        HL,
        GC,
    }

    public enum eRight
    {
        THEM,
        SUA,
        XOA,
        XEM,
        CHUCNANGKHAC,
        CHUYENDUYET,
        PHEDUYET,
        GROUP,
        KHONGDUYET,
        CHAMDUT,
        CHAMDUT2,
        CHAMDUT3,
        CHAMDUT4,
        PHEDUYET2,
        KHONGDUYET2,
        PHEDUYET3,
        KHONGDUYET3,
        PHEDUYET4,
        KHONGDUYET4,
        THONGBAONODECUOI,
        PHEDUYET5,
        KHONGDUYET5,
    }

    public enum eGioiHan
    {
        NONE,
        OWNER,
        GROUP,
        ALL,
    }

    public enum eLoaiHopDongPhuLuc
    {
        HopDong,
        PhuLucThayDoiThongTin,
        PhuLucThayDoiLuong,
        PhuLucThayDoiThongTinVaLuong,
    }

    public enum eLoaiThongBao
    {
        HRM_HetHanThuViec,
        HRM_HetHanHopDong,
        HRM_KyNiemNgayVao,
        HRM_SinhNhat,
        HRM_GiayToHetHan,
        HRM_HetGiamTruGiaCanh
    }

    public enum eLoaiNhatKyNhanVien
    {
        HopDong,
        PhuLucHopDong,
        HopDongChuyenGia,
        BoNhiemDieuDongDuAn,
        BoNhiemChucVu,
        QuyetDinhDiCongTac,
        QuyetDinhDieuChinhLuong,
        QuyetDinhKhenThuong,
        QuyetDinhKyLuat,
        QuyetDinhSaThai
    }

    public enum eNguonNhapLuong
    {
        NhapTay,
        HopDong,
        PhuLucHopDong,
        QuyetDinhDieuChinh
    }

    public enum eLoaiLuongHopDong
    {
        LuongThucTe,
        LuongHachToan
    }

    public enum eLoaiValue
    {
        type_int,
        type_long,
        type_double,
        type_string,
        type_datetime,
        type_datetime_datetime
    }

    public enum eMaSetting
    {
        SoGioLamNgayThuong,
        SoGioLamThu7,
        SoGioLamChuNhat,
        NgayNghiLe,
        NgayNghiBu,
        TenCongComGuiSMS,
        ChamCongThu7,
        ChamCongChuNhat,
        ChamCongDuAnThu7,
        ThangQuyetToan,
        SoNgayCongQuyetToanPhepNam,
        MucLuongDongPhiCongDoanGioiHan,
        ThueTNCNGiamTruCaNhan,
        ThueTNCNGiamTruNguoiPhuThuoc
    }

    public enum eLoaiSetting
    {
        CaiDatGioLamTrongNgay,
        CaiDatKhoangThoiGian,
        CaiDatThoiKhoangMacDinhCuaCongViec,
        CaiDatSMS,
        SoLuongUserMax,
        VersionCurrentAPK,
        VersionCurrentIOS,
        DuongDanBackupData,
        SoNgayBackupData,
        ThoiGianBackupData,
        CaiDatBatBuocNhapThoiKhoang,
        XuatPhieuCongVanBachMai,
        CanhBaoTreCongViecTruocThoiGian,
        NhaySoCongVanTheoDuAn,
        SoCongVanCongTruoc,
    }

    public enum eMaNgayChamCong
    {
        DL,
        VM,
        KL,
        CT,
        NL,
        HL,
        P
    }

    public enum eLoaiBaoHiem
    {
        GiamDoNghiViec,
        GiamThaiSan,
        GiamKhongLuong,
        GiamBaoHiem,
        TangMoi,
        TangLaiSauThaiSan,
        TangSauNghiKhongLuong
    }

    public static string GetGioiTinh(string? gioiTinh)
        => gioiTinh is not null && dicGioiTinh.TryGetValue(gioiTinh, out var label) ? label : "";

    public static string QuyDoiThu(int i) => i switch
    {
        0 => "Chủ Nhật",
        1 => "Thứ hai",
        2 => "Thứ Ba",
        3 => "Thứ Tư",
        4 => "Thứ Năm",
        5 => "Thứ Sáu",
        6 => "Thứ Bảy",
        _ => ""
    };

    public static string QuyDoiThuVT(int i) => i switch
    {
        0 => "Chủ nhật",
        1 => "Thứ 2",
        2 => "Thứ 3",
        3 => "Thứ 4",
        4 => "Thứ 5",
        5 => "Thứ 6",
        6 => "Thứ 7",
        _ => ""
    };

    public static string QuyDoiThuTA(int i) => i switch
    {
        0 => "Sunday",
        1 => "Monday",
        2 => "Tuesday",
        3 => "Wednesday",
        4 => "Thursday",
        5 => "Friday",
        6 => "Saturday",
        _ => ""
    };

    public static int QuyDoiThuSangSo(string? i) => i switch
    {
        "Sunday" => 0,
        "Monday" => 1,
        "Tuesday" => 2,
        "Wednesday" => 3,
        "Thursday" => 4,
        "Friday" => 5,
        "Saturday" => 6,
        _ => 0
    };

    public static string QuyDoiLoaiCauHoi(string? i) => i switch
    {
        "NhapText" => "Nhập câu trả lời",
        "SapXep" => "Sắp xếp",
        "ChonABCD" => "Chọn đáp án",
        _ => ""
    };

    public static string QuyDoiTrangThaiNhanVien(string? i) => i switch
    {
        "DangLamViec" => "Đang Làm việc",
        "DaNghiViec" => "Đã nghỉ việc",
        "NghiKhongLuong" => "Nghỉ không lương",
        "NghiThaiSan" => "Nghỉ thai sản",
        "NghiCheDo" => "Nghỉ chế độ",
        "NghiNgang" => "Nghỉ ngang",
        _ => ""
    };

    public static string QuyDoiBoPhanDanhGia(string? i) => i switch
    {
        "HCNS" => "Hành chính nhân sự",
        "KETOAN" => "Kế toán",
        _ => ""
    };

    public static string QuyDoiTenQuyTrinh(string? eActionValue)
    {
        if (string.IsNullOrEmpty(eActionValue))
        {
            return "";
        }

        return eActionValue switch
        {
            nameof(eAction.CHUONGTRINHDAOTAO) => "Chương trình đào tạo",
            nameof(eAction.QUYTRINHPHEDUYETLUONGUNGVIEN) => "Quy trình phê duyệt lương",
            nameof(eAction.HOSONHANVIEN) => "Hồ sơ nhân viên",
            nameof(eAction.KEHOACHLAMVIECTUAN) => "Kế hoạch làm việc tuần",
            nameof(eAction.QUYTRINHCHAMCONG) => "Bảng chấm công",
            nameof(eAction.QUYTRINHHOPDONGNHANSU) => "Hợp đồng lao động",
            nameof(eAction.QUYTRINHNGHIVIEC) => "Quyết định nghỉ việc",
            nameof(eAction.QUYTRINHQUYETDINHKHENTHUONG) => "Quyết định khen thưởng",
            nameof(eAction.QUYTRINHQUYETDINHKYLUAT) => "Quyết định kỷ luật",
            nameof(eAction.QUYTRINHDIEUCHINHLUONG) => "Quyết định điều chỉnh lương",
            nameof(eAction.QUYTRINHBONHIEMDIEUDONG) => "Quyết định điều động",
            nameof(eAction.QUYTRINHKEHOACHPHUCLOI) => "Kế hoạch phúc lợi",
            nameof(eAction.QUYTRINHPHUCLOI) => "Quy trình phúc lợi",
            nameof(eAction.QUYTRINHTINHLUONG) => "Quy trình tính lương",
            nameof(eAction.DENGHITUYENDUNG) => "Đề nghị tuyển dụng",
            nameof(eAction.QUYTRINHDIMUONVESOM) => "Xin đi muộn về sớm",
            nameof(eAction.XACNHANDIMUONVESOM) => "Xác nhận đi muộn về sớm",
            nameof(eAction.QUYTRINHLAMTHEMGIO) => "Đăng ký làm thêm giờ",
            nameof(eAction.QUYTRINHXINNGHIPHEP) => "Đăng ký nghỉ phép",
            nameof(eAction.DANHGIANHANVIEN) => "Đánh giá nhân viên",
            nameof(eAction.XACNHANNGOAIGIO) => "Xác nhận ngoài giờ",
            nameof(eAction.TUYENDUNGUNGVIEN) => "Tuyển dụng ứng viên",
            nameof(eAction.KETQUACONGTAC) => "Kết quả công tác",
            nameof(eAction.KEHOACHCONGTAC) => "Kế hoạch công tác",
            _ => ""
        };
    }

    public static string? QuyDoiXepLoai(double? i) => i switch
    {
        null => null,
        >= 95 => "A+",
        >= 90 => "A",
        >= 85 => "B+",
        >= 80 => "B",
        >= 75 => "B-",
        >= 70 => "C",
        _ => "C-"
    };

    public static string QuyDoiHinhThucDaoTao(int i) => i switch
    {
        1 => "Lý thuyết",
        2 => "Thực hành",
        3 => "Lý thuyết + thực hành",
        4 => "Cầm tay chỉ việc",
        _ => ""
    };

    public static string GetTenLoaiNhatKyNhanVien(string? input) => input switch
    {
        nameof(eLoaiNhatKyNhanVien.HopDong) => "Hợp đồng",
        nameof(eLoaiNhatKyNhanVien.PhuLucHopDong) => "Phụ lục hợp đồng",
        nameof(eLoaiNhatKyNhanVien.HopDongChuyenGia) => "Hợp đồng chuyên gia",
        nameof(eLoaiNhatKyNhanVien.BoNhiemDieuDongDuAn) => "Bổ nhiệm điều động dự án",
        nameof(eLoaiNhatKyNhanVien.BoNhiemChucVu) => "Bổ nhiệm chức vụ",
        nameof(eLoaiNhatKyNhanVien.QuyetDinhDiCongTac) => "Quyết định đi công tác",
        nameof(eLoaiNhatKyNhanVien.QuyetDinhDieuChinhLuong) => "Quyết định điều chỉnh lương",
        nameof(eLoaiNhatKyNhanVien.QuyetDinhKhenThuong) => "Quyết định khen thưởng",
        nameof(eLoaiNhatKyNhanVien.QuyetDinhKyLuat) => "Quyết định kỷ luật",
        nameof(eLoaiNhatKyNhanVien.QuyetDinhSaThai) => "Quyết định sa thải",
        _ => ""
    };

    public static string GetLoaiTangGiamBH(string? input) => input switch
    {
        nameof(eLoaiBaoHiem.GiamDoNghiViec) => "Giảm do nghỉ việc",
        nameof(eLoaiBaoHiem.GiamThaiSan) => "Giảm thai sản",
        nameof(eLoaiBaoHiem.GiamKhongLuong) => "Giảm không lương",
        nameof(eLoaiBaoHiem.GiamBaoHiem) => "Giảm bảo hiểm",
        nameof(eLoaiBaoHiem.TangMoi) => "Tăng mới",
        nameof(eLoaiBaoHiem.TangLaiSauThaiSan) => "Tăng sau thai sản",
        nameof(eLoaiBaoHiem.TangSauNghiKhongLuong) => "Tăng sau nghỉ không lương",
        _ => ""
    };
}
