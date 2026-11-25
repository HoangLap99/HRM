#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Models;
using Newtonsoft.Json;

namespace HRM.Api.Services.ChamCong
{

    // quy trình đã chuyển qua smartEOS
        public class funcChamCongDuAn : FuncBase
    {
        public funcChamCongDuAn(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<funcChamCongDuAn> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

    //    DefineData.eAction eAction = DefineData.eAction.QUYTRINHCHAMCONGDUAN;

    //    byte TrangThaiChuaChuyenDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.ChuaChuyenDuyet;
    //    byte TrangThaiChoPhongNhanSuDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.ChoPhongNhanSuDuyet;
    //    byte TrangThaiPhongNhanSuKhongDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.PhongNhanSuKhongDuyet;
    //    byte TrangThaiChoGiamDocDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.ChoGiamDocDuyet;
    //    byte TrangThaiGiamDocKhongDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.GiamDocKhongDuyet;
    //    byte TrangThaiGiamDocDaDuyet = (byte)DefineData.eTrangThaiChamCongDuAn.GiamDocDaDuyet;


    //    public List<HRM_DuAn> GetDuAnLamTruongDuAn(string idCurrentUser, ApplicationDbContext db = null)
    //    {
    //        var itemNhanVien = db.HRM_NhanVien.FirstOrDefault(x => x.IdUser == idCurrentUser);

    //        var itemChucVuTruongDuAn = db.HRM_ChucVu.FirstOrDefault(x => x.Ma == DefineData.MaChucVuTruongDuAn);

    //        if (itemNhanVien == null || itemChucVuTruongDuAn == null)
    //            return new List<HRM_DuAn>();

    //        // lấy ra hợp đồng dự án của nhần viền
    //        string IdNhanVien = itemNhanVien.IdGuid;
    //        string IdChucVuTruongDuAn = itemChucVuTruongDuAn.IdGuid;

    //        string TrangThaiHieuLucDuAn = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
    //        var lstIdDuAn = db.HRM_HopDong.Where(x => x.IdNguoiLaoDong == IdNhanVien
    //        && x.TrangThai == TrangThaiHieuLucDuAn && string.IsNullOrEmpty(x.IdDuAn) == false && x.IdChucVu == IdChucVuTruongDuAn).Select(x => x.IdDuAn).ToList();

    //        // quyết định bổ nhiệm điều động
    //        string TrangThaiDangThucHienBoNhiemDieuDong = DefineData.eTrangThaiBoNhiemDieuDongDuAn.DangThucHien.ToString();
    //        var lstIdDuAnTheoBoNhiemDieuDong = db.HRM_BoNhiemDieuDongDuAn
    //            .Where(x => x.IdChucVu == IdChucVuTruongDuAn && x.TrangThai == TrangThaiDangThucHienBoNhiemDieuDong).Select(x => x.IdDuAn).ToList();


    //        if (lstIdDuAn == null)
    //            lstIdDuAn = new List<string>();


    //        lstIdDuAn.AddRange(lstIdDuAnTheoBoNhiemDieuDong);


    //        if (lstIdDuAn.Count > 0)
    //            return db.HRM_DuAn.Where(x => lstIdDuAn.Contains(x.IdGuid)).ToList();
    //        else
    //            return new List<HRM_DuAn>();

    //    }

    //    public List<HRM_ChamCongDuAnChiTiet> GetDanhSachNhanVienDuAn(string IdDuAn, ApplicationDbContext db = null)
    //    {
    //        // Hợp đòng Dự án
    //        string TrangThaiHieuLucDuAn = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
    //        var lstNhanVienDuAnTheoHopDong = db.HRM_HopDong.Where(x => x.IdDuAn == IdDuAn
    //       && x.TrangThai == TrangThaiHieuLucDuAn).ToList();

    //        var lstIdNhanVien = lstNhanVienDuAnTheoHopDong.Select(x => x.IdNguoiLaoDong).ToList();


    //        // Chuyên gia dự án
    //        string TrangThaiConHieuLuc = DefineData.eTrangTHaiHopDongChuyenGia.ConHieuLuc.ToString();
    //        var lstNhanVienChuyenGiaDuAn = db.HRM_HopDongChuyenGia.Where(x => x.IdDuAn == IdDuAn && x.TrangThai == TrangThaiConHieuLuc).ToList();
    //        lstIdNhanVien.AddRange(lstNhanVienChuyenGiaDuAn.Select(x => x.IdNguoiLaoDong));

    //        // bổ nhiệm điều động
    //        string TrangThaiDangThucHienBoNhiemDieuDongDuAn = DefineData.eTrangThaiBoNhiemDieuDongDuAn.DangThucHien.ToString();
    //        var lstBoNhiemDieuDongDuAn = db.HRM_BoNhiemDieuDongDuAn
    //            .Where(x => x.TrangThai == TrangThaiDangThucHienBoNhiemDieuDongDuAn && x.IdDuAn == IdDuAn).ToList();

    //        lstIdNhanVien.AddRange(lstBoNhiemDieuDongDuAn.Select(x => x.IdNhanVien));

    //        var lstNhanVien = db.HRM_NhanVien.Where(x => lstIdNhanVien.Contains(x.IdGuid)).ToList();

    //        List<HRM_ChamCongDuAnChiTiet> lstReturn = new List<HRM_ChamCongDuAnChiTiet>();
    //        foreach (var itemHopDong in lstNhanVienDuAnTheoHopDong)
    //        {
    //            HRM_ChamCongDuAnChiTiet ChiTiet = new HRM_ChamCongDuAnChiTiet(itemHopDong);
    //            lstReturn.Add(ChiTiet);
    //        }


    //        foreach (var itemHopDongChuyenGia in lstNhanVienChuyenGiaDuAn)
    //        {
    //            HRM_ChamCongDuAnChiTiet ChiTiet = new HRM_ChamCongDuAnChiTiet(itemHopDongChuyenGia);
    //            lstReturn.Add(ChiTiet);
    //        }

    //        foreach (var itemBoNhiemDieuDong in lstBoNhiemDieuDongDuAn)
    //        {
    //            HRM_ChamCongDuAnChiTiet ChiTiet = new HRM_ChamCongDuAnChiTiet(itemBoNhiemDieuDong);
    //            lstReturn.Add(ChiTiet);
    //        }

    //        return lstReturn;

    //    }


    //    public dynamic GetDanhSachChamCongDuAn(dynamic dyn, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {

    //        bool isPhongNhanSu = false;
    //        bool isGiamDoc = false;
    //        #region Quyền

    //        List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eAction.ToString(), idCurrentUser);

    //        DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET;
    //        BangPhanQuyen pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());

    //        isPhongNhanSu = pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE;

    //        eRigth = DefineData.eRigth.PHEDUYET2;
    //        pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());

    //        isGiamDoc = pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE;

    //        #endregion

    //        var query = (from cc in db.HRM_ChamCongDuAn
    //                     join da in db.HRM_DuAn
    //                     on cc.IdDuAn equals da.IdGuid
    //                     select new ChamCongDuAn
    //                     {
    //                         Created = cc.Created,
    //                         CreatedBy = cc.CreatedBy,
    //                         CreatedByName = cc.CreatedByName,
    //                         IdDuAn = cc.IdDuAn,
    //                         IdGuid = cc.IdGuid,
    //                         MaDuAn = da.Ma,
    //                         ModifiedBy = cc.ModifiedBy,
    //                         ModifiedByName = cc.ModifiedByName,
    //                         Nam = cc.Nam,
    //                         TenDuAn = da.Ten,
    //                         Thang = cc.Thang,
    //                         TrangThai = cc.TrangThai,
    //                         Modified = cc.Modified,
    //                     });

    //        string KeyWord = dyn.KeyWord;
    //        int CurrentPage = dyn.CurrentPage == null ? 1 : dyn.CurrentPage;
    //        int NumperPage = dyn.NumperPage == null ? 10 : dyn.NumperPage;
    //        string TabTrangThai = dyn.TabTrangThai;




    //        if (TabTrangThai == DefineData.eTabTrangThai.ChuaXuLy.ToString())
    //        {
    //            query = query.Where(x =>
    //                (x.CreatedBy == idCurrentUser && (x.TrangThai == TrangThaiChuaChuyenDuyet || x.TrangThai == TrangThaiPhongNhanSuKhongDuyet))
    //                || (isPhongNhanSu && (x.TrangThai == TrangThaiChoPhongNhanSuDuyet || x.TrangThai == TrangThaiGiamDocKhongDuyet))
    //                || (isGiamDoc && (x.TrangThai == TrangThaiChoGiamDocDuyet))
    //                );
    //        }
    //        else
    //        {

    //            query = query.Where(x =>
    //                (x.CreatedBy == idCurrentUser && (x.TrangThai > TrangThaiChuaChuyenDuyet && x.TrangThai != TrangThaiPhongNhanSuKhongDuyet))
    //                || (isPhongNhanSu && (x.TrangThai > TrangThaiChoPhongNhanSuDuyet && x.TrangThai != TrangThaiGiamDocKhongDuyet))
    //                || (isGiamDoc && (x.TrangThai > TrangThaiChoGiamDocDuyet))
    //                );
    //        }

    //        if (string.IsNullOrEmpty(KeyWord) == false)
    //        {
    //            KeyWord = KeyWord.ToLower();
    //            query = query.Where(x => x.MaDuAn.ToLower().Contains(KeyWord) || x.TenDuAn.ToLower().Contains(KeyWord));
    //        }

    //        if (CurrentPage == 0)
    //        {
    //            return query.OrderByDescending(q => q.Created).ToList();
    //        }
    //        else
    //        {
    //            dynamic result = new DynamicObject();

    //            int count = query.Count();
    //            var listResult = query.OrderByDescending(q => q.Created).Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();

    //            result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
    //            result.items = listResult;
    //            return result;
    //        }


    //    }

    //    public dynamic TaoChamCongDuAn(string IdDuAn, int Thang, int Nam, CurrentUser CurrentUser, ApplicationDbContext db = null)
    //    {
    //        var item = db.HRM_ChamCongDuAn.FirstOrDefault(x => x.IdDuAn == IdDuAn && x.Thang == Thang && x.Nam == Nam);
    //        if (item != null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Đã có chấm công của dự án");

    //        item = new HRM_ChamCongDuAn();
    //        item.IdDuAn = IdDuAn;
    //        item.IdNhanVienKeKhai = CurrentUser.idNhanSu;
    //        item.Thang = Thang;
    //        item.Nam = Nam;
    //        item.TrangThai = (byte)DefineData.eTrangThaiChamCongDuAn.ChuaChuyenDuyet;

    //        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

    //        try
    //        {
    //            db.SaveChanges();
    //        }
    //        catch (Exception ex)
    //        {
    //            DefineData.logger.ErrorFormat("TaoChamCongDuAn data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //        }
    //        string Id = item.IdGuid;

    //        // lấy ra cài đặt hệ thống thứ 7

    //        var lstCaiDatHeThong = db.HRM_CaiDatHeThong.ToList();
    //        var itemCaiDatNgayThu7 = lstCaiDatHeThong.FirstOrDefault(x => x.Ma == DefineData.eMaSetting.ChamCongDuAnThu7.ToString());
    //        double SoGioLamThu7 = itemCaiDatNgayThu7.ValueDouble;



    //        DateTime NgayDauThang = new DateTime(Nam, Thang, 1).Date;
    //        DateTime NgayCuoiThang = NgayDauThang.AddMonths(1);

    //        int SoNgayTrongThang = DateTime.DaysInMonth(Nam, Thang);

    //        var lstNhanVienDuAn = GetDanhSachNhanVienDuAn(IdDuAn, db);

    //        string Sang = DefineData.eLoaiSangChieu.Sang.ToString();
    //        string Chieu = DefineData.eLoaiSangChieu.Chieu.ToString();

    //        // Xử lý
    //        foreach (var itemNhanVien in lstNhanVienDuAn)
    //        {
    //            itemNhanVien.IdChamCongDuAn = Id;
    //            itemNhanVien.lstChiTietNgay = new List<HRM_ChamCongDuAnChiTietNgay>();
    //            var HopDongChuyenGia = itemNhanVien.HopDongChuyenGia;

    //            for (DateTime dTemp = NgayDauThang; dTemp < NgayCuoiThang; dTemp.AddDays(1))
    //            {
    //                HRM_ChamCongDuAnChiTietNgay ChiTietNgaySang = new HRM_ChamCongDuAnChiTietNgay(itemNhanVien, itemNhanVien.HopDong
    //                        , itemNhanVien.BoNhiemDieuDong, HopDongChuyenGia, dTemp);
    //                ChiTietNgaySang.SangChieu = Sang;

    //                HRM_ChamCongDuAnChiTietNgay ChiTietNgayChieu = new HRM_ChamCongDuAnChiTietNgay(itemNhanVien, itemNhanVien.HopDong
    //                    , itemNhanVien.BoNhiemDieuDong, HopDongChuyenGia, dTemp);
    //                ChiTietNgayChieu.SangChieu = Chieu;

    //                int Thu = (int)dTemp.DayOfWeek;
    //                if (Thu == 8)
    //                {
    //                    if (HopDongChuyenGia == null)
    //                    {
    //                        ChiTietNgaySang.SoGioLam = 0;
    //                        ChiTietNgayChieu.SoGioLam = 0;
    //                    }
    //                    else
    //                    {
    //                        itemNhanVien.NgayCongThucTe++;
    //                    }

    //                }
    //                else
    //                    if (Thu == 7)
    //                {
    //                    if (SoGioLamThu7 <= DefineData.ThoiGianLamViecMotBuoiDuAn)
    //                    {
    //                        ChiTietNgayChieu.SoGioLam = 0;
    //                        itemNhanVien.NgayCongThucTe++;
    //                    }
    //                }
    //                else
    //                {
    //                    itemNhanVien.NgayCongThucTe++;
    //                }


    //                if (ChiTietNgaySang.isHoTroCongTacXa)
    //                    itemNhanVien.NgayDuocNhanHoTro++;

    //                itemNhanVien.lstChiTietNgay.Add(ChiTietNgaySang);
    //                itemNhanVien.lstChiTietNgay.Add(ChiTietNgayChieu);
    //            }

    //        }

    //        // Add vào DB
    //        foreach (var NhanVien in lstNhanVienDuAn)
    //        {
    //            db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Added;
    //            db.SaveChanges();

    //            foreach (var itemChiTietNgay in NhanVien.lstChiTietNgay)
    //            {
    //                itemChiTietNgay.IdChamCongDuAnChiTiet = NhanVien.IdGuid;
    //                db.Entry(NhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Added;
    //            }

    //            db.SaveChanges();
    //        }


    //        return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", Id);


    //    }


    //    public dynamic GetChamCongDuAn(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {
    //        var itemChamCongDuAn = db.HRM_ChamCongDuAn.AsNoTracking().FirstOrDefault(x => x.IdGuid == Id);
    //        if (itemChamCongDuAn == null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy chấm công dự án");


    //        string TableName = DefineData.eTable.HRM_ChamCongDuAn.ToString();
    //        itemChamCongDuAn.lstFileDinhKemCu = db.HRM_FileDinhKem.Where(x => x.idItem == Id && x.TableName == TableName).ToList();

    //        var lstChamCongChiTiet = (from cc in db.HRM_ChamCongDuAnChiTiet
    //                                  join nv in db.HRM_NhanVien
    //                                  on cc.IdNhanVien equals nv.IdGuid
    //                                  where cc.IdChamCongDuAn == Id
    //                                  select new ChamCongDuAnChiTiet
    //                                  {
    //                                      Created = cc.Created,
    //                                      CreatedBy = cc.CreatedBy,
    //                                      CreatedByName = cc.CreatedByName,
    //                                      IdGuid = cc.IdGuid,
    //                                      MaNhanVien = nv.MaNhanVien,
    //                                      Modified = cc.Modified,
    //                                      ModifiedBy = cc.ModifiedBy,
    //                                      ModifiedByName = cc.ModifiedByName,
    //                                      NgayCongThucTe = cc.NgayCongThucTe,
    //                                      NgayCongTinhLuong = cc.NgayCongCoSo,
    //                                      NgayDuocNhanHoTro = cc.NgayDuocNhanHoTro,
    //                                      TenNhanVien = nv.TenDayDu
    //                                  }).ToList();
    //        var lstChamCongChiTietNgay = db.HRM_ChamCongDuAnChiTietNgay.AsNoTracking().Where(x => x.IdChamCongDuAn == Id).ToList();

    //        foreach (var itemChiTiet in lstChamCongChiTiet)
    //        {
    //            SetChamCongDuAnChiTiet(itemChiTiet, lstChamCongChiTietNgay.Where(x => x.IdChamCongDuAnChiTiet == itemChiTiet.IdGuid).ToList());
    //        }

    //        itemChamCongDuAn.lstChiTiet = lstChamCongChiTiet;

    //        return itemChamCongDuAn;

    //    }


    //    public void SetChamCongDuAnChiTiet(ChamCongDuAnChiTiet ChamCong, List<HRM_ChamCongDuAnChiTietNgay> lstChamCongVanPhongNgay)
    //    {
    //        foreach (var ChamCongNgay in lstChamCongVanPhongNgay)
    //        {
    //            string MaChamCong = ChamCongNgay.MaChamCong;
    //            if (ChamCongNgay.isHoTroCongTacXa)
    //                ChamCong.NgayDuocNhanHoTro += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiPhep)
    //                ChamCong.NghiPhep += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiBu)
    //                ChamCong.NghiBu += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiOm)
    //                ChamCong.NghiOmTheoCheDo += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiThaiSan)
    //                ChamCong.NghiThaiSan += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiKhongLuong)
    //                ChamCong.NghiKhongLuong += ChamCongNgay.NgayLuong;
    //            else
    //             if (MaChamCong == DefineData.MaChamCongNghiTheoNQLD)
    //                ChamCong.NghiViecRiengTheoNQLD += ChamCongNgay.NgayLuong;
    //        }


    //    }

    //    public dynamic GetChamCongDuAnChiTietTheoNhanVien(string Id, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {
    //        var itemChamCongDuAn = db.HRM_ChamCongDuAn.AsNoTracking().FirstOrDefault(x => x.IdGuid == Id);
    //        if (itemChamCongDuAn == null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Lỗi lấy dữ liệu");

    //        List<ChamCongDuAnChiTietNhanVien> lstChamCongThangCuaNhanVien = (from cc in db.HRM_ChamCongDuAnChiTiet
    //                                                                         join nv in db.HRM_NhanVien
    //                                                                         on cc.IdNhanVien equals nv.IdGuid
    //                                                                         where cc.IdGuid == Id
    //                                                                         select new ChamCongDuAnChiTietNhanVien
    //                                                                         {
    //                                                                             IdGuid = cc.IdGuid,
    //                                                                             TenNhanVien = nv.TenDayDu,
    //                                                                             // PhongBan = bp.TenBoPhan,
    //                                                                             MaNhanVien = nv.MaNhanVien,
    //                                                                         }).ToList();

    //        var lstChamCongChiTietNhanVienNgay = db.HRM_ChamCongDuAnChiTietNgay.AsNoTracking().Where(x => x.IdChamCongDuAn == Id).ToList();

    //        DateTime NgayDauThang = new DateTime(itemChamCongDuAn.Nam, itemChamCongDuAn.Thang, 1).Date;
    //        DateTime NgayCuoiThang = NgayDauThang.AddMonths(1);

    //        foreach (var itemNhanVien in lstChamCongThangCuaNhanVien)
    //        {
    //            itemNhanVien.lstChamCongNgay = new List<ChamCongDuAnChiTietNhanVienNgay>();
    //            for (DateTime dTemp = NgayDauThang; dTemp < NgayCuoiThang; dTemp.AddDays(1))
    //            {
    //                ChamCongDuAnChiTietNhanVienNgay itemChiTietNgay = new ChamCongDuAnChiTietNhanVienNgay(dTemp);

    //                var lstChiTietNgay = lstChamCongChiTietNhanVienNgay.Where(x => x.NgayThang == dTemp).ToList();
    //                float SoGioLam = 0;
    //                string MaChamCong = "";

    //                foreach (var ChiTietNgay in lstChiTietNgay)
    //                {
    //                    SoGioLam += ChiTietNgay.SoGioLam;
    //                    MaChamCong = ChiTietNgay.MaChamCong;
    //                }

    //                if (SoGioLam > 0)
    //                    itemChiTietNgay.KyHieuLam = SoGioLam.ToString();
    //                else
    //                    itemChiTietNgay.KyHieuLam = MaChamCong;

    //                itemNhanVien.lstChamCongNgay.Add(itemChiTietNgay);
    //            }
    //        }

    //        return lstChamCongThangCuaNhanVien;

    //    }


    //    public List<HRM_ChamCongDuAnChiTietNgay> GetChamCongDuAnChiTietNgay(string IdNhanVien, double NgayUnix, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {
    //        DateTime Ngay = dungchung.UnixTimeStampToDateTime(NgayUnix);
    //        var lstReturn = (from ct in db.HRM_ChamCongDuAnChiTiet
    //                         join ctNgay in db.HRM_ChamCongDuAnChiTietNgay
    //                         on ct.IdGuid equals ctNgay.IdChamCongDuAnChiTiet
    //                         where ct.IdNhanVien == IdNhanVien
    //                          && ctNgay.NgayThang.Day == Ngay.Day
    //                          && ctNgay.NgayThang.Month == Ngay.Month
    //                          && ctNgay.NgayThang.Year == Ngay.Year
    //                         select ctNgay).ToList();

    //        return lstReturn;
    //    }

    //    public dynamic UpdateChamCongDuAnChiTietNgay(List<HRM_ChamCongDuAnChiTietNgay> lstChamCongChiTietNgay, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {
    //        if (lstChamCongChiTietNgay.Count == 0)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không có dữ liệu để cập nhật");

    //        string IdChamCongDuAnChiTiet = "";
    //        foreach (var item in lstChamCongChiTietNgay)
    //        {
    //            if (string.IsNullOrEmpty(item.IdGuid))
    //                continue;

    //            IdChamCongDuAnChiTiet = item.IdChamCongDuAnChiTiet;

    //            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
    //        }

    //        var itemChamCong = db.HRM_ChamCongDuAnChiTiet.FirstOrDefault(x => x.IdGuid == IdChamCongDuAnChiTiet);

    //        if (itemChamCong != null)
    //        {
    //            itemChamCong.NgayCongThucTe = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => x.IdChamCongVanPhong == IdChamCongDuAnChiTiet).Sum(x => x.NgayLuong);
    //            itemChamCong.NgayDuocNhanHoTro = db.HRM_ChamCongVanPhongChiTietNgay.Where(x => x.isHoTroDuAn == true).Sum(x => x.NgayLuong);
    //        }


    //        try
    //        {
    //            db.SaveChanges();
    //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
    //        }
    //        catch (Exception ex)
    //        {
    //            DefineData.logger.ErrorFormat("UpdateChamCongChiTietNgay Error {0}", ex.Message);
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //        }

    //    }


    //    public dynamic UpdateNgayCongCoSo(dynamic data, string idCurrentUser = "", ApplicationDbContext db = null)
    //    {
    //        if (data.NgayCongCoSo == null || data.lstIdChamCongChiTiet == null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu truyền lên không đúng");

    //        float NgayCongCoSo = data.NgayCongCoSo;
    //        List<string> lstIdChamCongChiTiet = JsonConvert.DeserializeObject<List<string>>(data.lstIdChamCongChiTiet.ToString());

    //        var lstChamCongChiTiet = db.HRM_ChamCongDuAnChiTiet.Where(x => lstIdChamCongChiTiet.Contains(x.IdGuid)).ToList();

    //        foreach(var item in lstChamCongChiTiet)
    //        {
    //            item.NgayCongCoSo = NgayCongCoSo;
    //        }
    //        try
    //        {
    //            db.SaveChanges();
    //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
    //        }
    //        catch (Exception ex)
    //        {
    //            DefineData.logger.ErrorFormat("TaoChamCongDuAn Error {0}",  ex.Message);
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //        }
    //    }

    //    public dynamic KiemTraButtonChamCongDuAn(string Id, string idCurrentUser, ApplicationDbContext db = null)
    //    {
    //        bool bChuyenTiep = false;
    //        bool bKhongDuyet = false;
    //        bool bXoa = false;
    //        bool bGhi = true;


    //        // item kế hoạch chi
    //        var item = db.HRM_ChamCongDuAn.FirstOrDefault(x => x.IdGuid == Id);

    //        byte TrangThai = 0;
    //        if (item != null)
    //            TrangThai = item.TrangThai;


            
    //        if (TrangThai == TrangThaiChuaChuyenDuyet && item.CreatedBy == idCurrentUser)
    //        {
    //            bXoa = true;
    //        }


    //        if (TrangThai == TrangThaiChoPhongNhanSuDuyet || TrangThai == TrangThaiGiamDocKhongDuyet)
    //        {
    //            // Đối với node phòng hành chính

    //            List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eAction.ToString(), idCurrentUser);
    //            DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET;
    //            BangPhanQuyen pq = PhanQuyens.FirstOrDefault(x => x.MaRight == eRigth.ToString());

    //            if (pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE)
    //            {
    //                bChuyenTiep = true;
    //            }

    //            eRigth = DefineData.eRigth.KHONGDUYET;
    //            pq = PhanQuyens.FirstOrDefault(x => x.MaAction == eAction.ToString());
    //            if (pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE)
    //            {
    //                bKhongDuyet = true;
    //            }
    //        }

    //        if (TrangThai == TrangThaiChoGiamDocDuyet)
    //        {
    //            // Đối với node giám đốc

    //            List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eAction.ToString(), idCurrentUser);
    //            DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET2;
    //            BangPhanQuyen pq = PhanQuyens.FirstOrDefault(x => x.MaRight == eRigth.ToString());

    //            if (pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE)
    //            {
    //                bChuyenTiep = true;
    //            }

    //            eRigth = DefineData.eRigth.KHONGDUYET2;
    //            pq = PhanQuyens.FirstOrDefault(x => x.MaAction == eAction.ToString());
    //            if (pq != null && pq.GioiHan != (byte)DefineData.eGioiHan.NONE)
    //            {
    //                bKhongDuyet = true;
    //            }
    //        }
    //        else
    //        if(TrangThai == TrangThaiChuaChuyenDuyet || TrangThai == TrangThaiPhongNhanSuKhongDuyet)
    //        {
    //            if(item.CreatedBy == idCurrentUser)
    //            {
    //                bChuyenTiep = true;
    //            }

    //        }

    //        dynamic dnn = new DynamicObject();
    //        dnn.ChuyenTiep = bChuyenTiep;
    //        dnn.Ghi = bChuyenTiep;
    //        dnn.KhongDuyet = bKhongDuyet;
    //        dnn.Xoa = bXoa;
    //        return dnn;
    //    }

    //    public dynamic ChuyenTiepChamCongDuAn(HRM_ChamCongDuAn ChamCongDuAn, CurrentUser CurrentUser, ApplicationDbContext db = null)
    //    {
    //        string Id = ChamCongDuAn.IdGuid;

    //        string idCurrentUser = CurrentUser.Id;

    //        var KeHoachTongHop = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);
    //        if (KeHoachTongHop == null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm đề nghị tuyển dụng");

    //        byte TrangThai = KeHoachTongHop.TrangThai;


    //        List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eAction.ToString(), idCurrentUser);

    //        BangPhanQuyen pq = null;
    //        DefineData.eRigth eRigth = DefineData.eRigth.CHUYENDUYET;

    //        var lstFile = ChamCongDuAn.lstFileKyPM.Where(x => x.isDeleted != false).ToList();

    //        bool isUpdate = false;
    //        if (TrangThai == TrangThaiChuaChuyenDuyet || TrangThai == TrangThaiPhongNhanSuKhongDuyet)
    //        {
               
    //            if (lstFile.Count == 0)
    //                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Tải lên file có chứ kỹ của PM");

             
    //            if (KeHoachTongHop.CreatedBy == idCurrentUser)
    //            {
    //                KeHoachTongHop.TrangThai = TrangThaiChoPhongNhanSuDuyet;
    //                isUpdate = true;
    //                eRigth = DefineData.eRigth.PHEDUYET;
    //            }

    //        }
    //        else
    //            if (TrangThai == TrangThaiChoPhongNhanSuDuyet || TrangThai == TrangThaiGiamDocKhongDuyet)
    //        {
    //            eRigth = DefineData.eRigth.PHEDUYET;
    //            pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
    //            if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
    //            {
    //                KeHoachTongHop.TrangThai = TrangThaiChoGiamDocDuyet;
    //                isUpdate = true;
    //                eRigth = DefineData.eRigth.PHEDUYET2;
    //            }
    //        }
    //        else
    //        if(TrangThai == TrangThaiChoGiamDocDuyet)
    //        {
    //            eRigth = DefineData.eRigth.PHEDUYET2;
    //            pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
    //            if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
    //            {
    //                KeHoachTongHop.TrangThai = TrangThaiGiamDocDaDuyet;
    //                isUpdate = true;
    //                eRigth = DefineData.eRigth.PHEDUYET2;
    //            }
    //        }


    //        if (isUpdate)
    //        {
    //            try
    //            {
    //                db.SaveChanges();
    //            }
    //            catch (Exception ex)
    //            {
    //                DefineData.logger.ErrorFormat("ChuyenTiepChamCongDuAn data: {0}, Error {1}", JsonConvert.SerializeObject(KeHoachTongHop), ex.Message);
    //                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //            }

    //            if (KeHoachTongHop.TrangThai == TrangThaiChoPhongNhanSuDuyet)
    //            {
    //                // update File Đính kèm
    //                FileUploadController FileUpload = new FileUploadController();
    //                FileUpload.AddFileFromHDFile(lstFile, KeHoachTongHop.IdGuid, DefineData.eTable.HRM_ChamCongDuAn.ToString(), db
    //                    , idCurrentUser, DefineData.eFlagDinhKem.FileChamCongDuAnPM.ToString());

    //                AddNotification(CurrentUser, eAction, eRigth, "Gửi yêu cầu phê duyệt ");
    //            }
    //            else
    //            if (KeHoachTongHop.TrangThai == TrangThaiChoGiamDocDuyet)
    //            {
    //                AddNotification(CurrentUser, eAction, eRigth, "Gửi yêu cầu phê duyệt ");
    //            }

    //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp quy trình thành công!");
    //        }
    //        else
    //            return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền chuyển tiếp quy trình!");
    //    }

    //    public dynamic KhongDuyetChamCongDuAn(string Id, CurrentUser CurrentUser, ApplicationDbContext db = null)
    //    {
    //        var KeHoachTongHop = db.HRM_ChamCongVanPhongTongHop.FirstOrDefault(x => x.IdGuid == Id);
    //        if (KeHoachTongHop == null)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy quy trình công tác phí tổng hợp");

    //        byte TrangThai = KeHoachTongHop.TrangThai;

    //        List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(eAction.ToString(), CurrentUser.Id);

    //        BangPhanQuyen pq = null;
    //        DefineData.eRigth eRigth = DefineData.eRigth.KHONGDUYET;

    //        bool isUpdate = false;
    //        if (TrangThai == TrangThaiChoPhongNhanSuDuyet)
    //        {
    //            pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
    //            if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
    //            {
    //                KeHoachTongHop.TrangThai = TrangThaiPhongNhanSuKhongDuyet;
    //                isUpdate = true;
    //                eRigth = DefineData.eRigth.CHUYENDUYET;
    //            }

    //        }
    //        else
    //        if (TrangThai == TrangThaiChoGiamDocDuyet)
    //        {
    //            eRigth = DefineData.eRigth.KHONGDUYET2;
    //            pq = PhanQuyens.Find(x => x.MaRight == eRigth.ToString());
    //            if (pq.GioiHan != (int)DefineData.eGioiHan.NONE)
    //            {
    //                KeHoachTongHop.TrangThai = TrangThaiGiamDocKhongDuyet;
    //                isUpdate = true;
    //                eRigth = DefineData.eRigth.PHEDUYET;
    //            }

    //        }

    //        if (isUpdate)
    //        {
    //            try
    //            {
    //                db.SaveChanges();
    //            }
    //            catch (Exception ex)
    //            {
    //                DefineData.logger.ErrorFormat("KhongDuyetChamCongDuAn data: {0}, Error {1}", JsonConvert.SerializeObject(KeHoachTongHop), ex.Message);
    //                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //            }

    //            if (KeHoachTongHop.TrangThai == TrangThaiPhongNhanSuKhongDuyet)
    //            {
    //                List<string> lstIdUserNhanThongBao = new List<string>() { KeHoachTongHop.CreatedBy };
    //                AddNotification(CurrentUser, eAction, lstIdUserNhanThongBao, "Không duyệt yêu cầu ");
    //            }
    //            else
    //                if (KeHoachTongHop.TrangThai == TrangThaiGiamDocKhongDuyet)
    //                {
    //                    List<string> lstIdUserNhanThongBao = new List<string>() { KeHoachTongHop.CreatedBy };
    //                    AddNotification(CurrentUser, eAction, lstIdUserNhanThongBao, "Không duyệt yêu cầu ");
    //                }

    //            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp quy trình thành công!");
    //        }
    //        else
    //            return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền chuyển tiếp quy trình!");

    //    }


    //    public dynamic DuyetThayQuyTrinhChamCongDuAn(HRM_QuyTrinhCongTacPhiTongHop item, string idCurrentUser = "", ApplicationDbContext db = null
    //  )
    //    {
    //        #region Quyền
    //        string sAction = eAction.ToString();
    //        List<BangPhanQuyen> PhanQuyens = GetAllQuyenAction(sAction, idCurrentUser);
    //        DefineData.eRigth eRigth = DefineData.eRigth.PHEDUYET;
    //        BangPhanQuyen pq = PhanQuyens.FirstOrDefault(x => x.MaRight == eRigth.ToString());
    //        if (pq == null || pq.GioiHan == (int)DefineData.eGioiHan.NONE)
    //            return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền duyệt thay quy trình!");
    //        #endregion

    //        var lstFileDinhKem = item.lstFileDinhKemMoi.Where(x => x.isDeleted == false).ToList();
    //        if (lstFileDinhKem.Count == 0)
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Tải lên file duyệt thay");

    //        FileUploadController FileUpload = new FileUploadController();
    //        FileUpload.AddFileFromHDFile(lstFileDinhKem, item.IdGuid, DefineData.eTable.HRM_ChamCongDuAn.ToString(), db
    //            , idCurrentUser, DefineData.eFlagDinhKem.FileDinhKem.ToString());


    //        var itemDb = db.HRM_ChamCongDuAn.FirstOrDefault(x => x.IdGuid == item.IdGuid);
    //        if (itemDb.TrangThai == TrangThaiChoGiamDocDuyet
    //            || item.TrangThai == TrangThaiGiamDocKhongDuyet)
    //        {
    //            itemDb.TrangThai = TrangThaiGiamDocDaDuyet;
    //            try
    //            {
    //                db.SaveChanges();
    //                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
    //            }
    //            catch (Exception ex)
    //            {
    //                DefineData.logger.ErrorFormat("DuyetThayQuyTrinhCongTacPhiTongHop data: {0}, Error {1}", JsonConvert.SerializeObject(item), ex.Message);
    //                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
    //            }
    //        }
    //        else
    //        {
    //            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển duyệt trước đó");
    //        }

    //    }
    }

}