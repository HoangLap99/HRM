#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Features.Attendance;
using HRM.Api.Features.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using HRM.Api.Services.NhanVien;
using HRM.Api.Entity;
using HRM.Api.Entity.DinhBien;
using HRM.Api.Helper;
using HRM.Api.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static HRM.Api.Services.FuncBase;
using static HRM.Api.Services.NhanVien.FuncKeHoachLamViec;
using static HRM.Api.Entity.CodeName;
using static HRM.Api.Helper.HelperConstant;
using static HRM.Api.Helper.HelperConstant.LuongHopDong;
namespace HRM.Api.Services
{

        public class FuncChamCong : FuncBase
    {
        private readonly FuncKeHoachLamViec _funcKeHoach;
        private readonly IAttendancePairingService _attendancePairingService;
        private readonly IPayrollGridBuilder _payrollGridBuilder;
        private readonly ITaxCalculationService _taxCalculationService;
        private string Host => HrmOptions.Host;

        public FuncChamCong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncChamCong> logger,
            FuncKeHoachLamViec funcKeHoach,
            IAttendancePairingService attendancePairingService,
            IPayrollGridBuilder payrollGridBuilder,
            ITaxCalculationService taxCalculationService)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcKeHoach = funcKeHoach;
            _attendancePairingService = attendancePairingService;
            _payrollGridBuilder = payrollGridBuilder;
            _taxCalculationService = taxCalculationService;
        }
        public dynamic GetDanhSachMaChamCongHangNgay(dynamic dyn, string IdCurrentUser, ApplicationDbContext aContext)
        {

            List<StringCodeName> result = new List<StringCodeName>();

            StringCodeName DL = new StringCodeName(DefineData.eMaNgayChamCong.DL.ToString(), "Äi lÃ m");
            StringCodeName VM = new StringCodeName(DefineData.eMaNgayChamCong.VM.ToString(), "Váº¯ng máº·t");
            StringCodeName CT = new StringCodeName(DefineData.eMaNgayChamCong.CT.ToString(), "Äi cÃ´ng tÃ¡c");
            StringCodeName NL = new StringCodeName(DefineData.eMaNgayChamCong.NL.ToString(), "Nghá»‰ lá»…");
            StringCodeName HL = new StringCodeName(DefineData.eMaNgayChamCong.HL.ToString(), "Nghá»‰ bÃ¹ lá»…");
            result.Add(DL);
            result.Add(VM);
            result.Add(CT);
            result.Add(NL);
            result.Add(HL);
            //CÃ¡c loáº¡i nghá»‰ phÃ©p

            var lstHinhThucNghi = aContext.HRM_dmHinhThucNghi.AsNoTracking().ToList();

            foreach (var item in lstHinhThucNghi)
            {
                StringCodeName code = new StringCodeName(item.Ma, item.Ten);
                result.Add(code);
            }


            return result;
        }
        public dynamic GetDanhSachBangChamCongTheoKy(HRM_ChamCong_Detail newitem, ApplicationDbContext db)
        {

            //List<HRM_HopDong> listHopdong = db.HRM_HopDong.ToList();
            //foreach (var item in listHopdong)
            //{
            //    HRM_Luong_HopDong itemAdd = new HRM_Luong_HopDong();
            //    itemAdd.IdGuid = Guid.NewGuid().ToString();
            //    itemAdd.IdHopDong = item.IdGuid;
            //    itemAdd.IdNhanVien = item.IdNhanVien;
            //    itemAdd.isTinhLuong = true;
            //    itemAdd.KieuLuong = 0;
            //    itemAdd.IddmHinhThucTraLuong = "c7b3b5e0-eaa4-447a-b6a2-48334d94c7e2";
            //    itemAdd.LuongThucTe = 15000000;
            //    itemAdd.LuongBaoHiem = 10000000;
            //    itemAdd.NgayBatDau = DateTime.Now;
            //    itemAdd.NgayKetThuc = DateTime.Now.AddYears(10);
            //    itemAdd.TyLeHuongLuong = 100;
            //    db.HRM_Luong_HopDong.Add(itemAdd);
            //    db.SaveChanges();

            //}
            string IdDanhMucDotChamCongThang = newitem.IdDanhMucDotChamCongThang;
            long IdCongTyEOS = newitem.IdCongTyEOS;

            int Thang = newitem.Thang;
            int Nam = newitem.Nam;

            if (newitem.TuNgayUnix > 0)
            {
                newitem.TuNgay = UnixTimeStampToDateTime(newitem.TuNgayUnix);
                newitem.DenNgay = UnixTimeStampToDateTime(newitem.DenNgayUnix);
                Thang = newitem.TuNgay.Month;
                Nam = newitem.TuNgay.Year;
            }
            //var lstBoPhanAll = smdb.BoPhans.AsNoTracking().ToList();
            //var lstCaiDatGioLamViec = smdb.CaiDatGioLamViecs.AsNoTracking().ToList();
            //var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe" || q.Ma == "NgayNghiBu").ToList();
            HRM_dmLichLamViec LichLamViec = db.HRM_dmLichLamViec.Where(x => x.isApDung == true).FirstOrDefault();
            if (LichLamViec != null)
                LichLamViec.listNgay = db.HRM_dmLichLamViec_Ngay.Where(x => x.IdLichLamViec == LichLamViec.IdGuid).ToList();
            HRM_dmLichNghiLe LichNghiLe = db.HRM_dmLichNghiLe.Where(x => x.isApDung == true).FirstOrDefault();
            if (LichNghiLe != null)
                LichNghiLe.listNgay = db.HRM_dmLichNghiLe_Ngay.Where(x => x.IdLichNghiLe == LichLamViec.IdGuid).ToList();
            var lstIdBoPhanChamCong = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.AsNoTracking().Where(q => q.IdUserNhanVien == db.IdUser).Select(q => q.IdBoPhanEOS).ToList();
            var listBoPhanChamCong = newitem.lstChamCongBoPhan;
            if (lstIdBoPhanChamCong.Any() == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t khÃ´ng thÃ nh cÃ´ng");
            List<long> lstIdBoPhan = listBoPhanChamCong.Select(r => r.IdBoPhanEOS).ToList();
            var lstBoPhanTemp = new List<BoPhan>();

            var lstDanhMucDotChamCong = db.HRM_dmDotChamCongThang.AsNoTracking().ToList();
            var lstNgayChamCong = dungchung.GetDanhSachNgayCongTrongThang(Nam, Thang, IdDanhMucDotChamCongThang, lstDanhMucDotChamCong, LichLamViec, LichNghiLe);
            lstNgayChamCong = lstNgayChamCong.OrderBy(q => q.NgayCong).ToList();
            var ngayDauKy = lstNgayChamCong.FirstOrDefault().NgayCong;
            var ngayCuoiKy = lstNgayChamCong.LastOrDefault().NgayCong;

            DateTime date = new DateTime(Nam, Thang, 1);
            //   var lstIdBoPhanChild = lstBoPhanTemp.Where(q => q.lstIdParent.Join(lstIdBoPhanChamCong, r1 => r1, r2 => r2, (r1, r2) => new { r1 }).Any() || lstIdBoPhanChamCong.Contains(q.Id)).Select(q => q.Id).ToList();

            var lstAllHopDong = (from a in db.HRM_HopDong
                                 join b in db.HRM_Luong_HopDong on a.IdGuid equals b.IdHopDong
                                 where lstIdBoPhan.Contains(a.IdBoPhanEOS) && (IdCongTyEOS == 0 || (IdCongTyEOS > 0 && a.IdCongTyEOS == IdCongTyEOS))
                                 && a.TrangThai != DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString() && a.TrangThai != DefineData.eTrangTHaiHopDong.ChuaKy.ToString()
                                 && ((a.NgayHieuLuc.Month == Thang && a.NgayHieuLuc.Year == Nam) || date > a.NgayHieuLuc)
                                 && a.isPhuLuc == false && b.isTinhLuong == true
                                 select a).ToList();
            if (lstAllHopDong.Count() == 0)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t khÃ´ng thÃ nh cÃ´ng do khÃ´ng cÃ³ nhÃ¢n viÃªn nÃ o trong Ä‘á»£t cháº¥m cÃ´ng");
            }
            List<string> listIDNhanVien = lstAllHopDong.Select(r => r.IdNhanVien).ToList();
            var toDictNhanVien = db.HRM_NhanVien.Where(r => listIDNhanVien.Contains(r.IdGuid)).AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var listNhanVien = toDictNhanVien.Select(r => r.Value).ToList();
            var listIdEOS = listNhanVien.Select(r => r.IdNhanVienEOS).ToList();
            //var toDictNhanVienEOS_IdUser = smdb.NhanViens.AsNoTracking().Where(x => listIdEOS.Contains(x.Id) && !string.IsNullOrEmpty(x.UserPkid)).ToDictionary(x => x.Id, x => x.UserPkid);
            var lstAllHopDongHieuLucTrongThang = lstAllHopDong.Where(q => q.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() || q.NgayHetHan > ngayDauKy).ToList();
            var lstAllNhanVienOfHopDong = lstAllHopDongHieuLucTrongThang.Select(q => q.IdNhanVien).Distinct().ToList();
            var lstAllNhanVienOfQuyetDinh = new List<string>();
            HRMFaceModel faceModel = new HRMFaceModel
            {
                listIdNhanVien = listIdEOS,
                NgayDauKy = ngayDauKy.Date,
                NgayCuoiKy = ngayCuoiKy.Date,
            };
            List<HRMFaceReponeModel> listChamCongFace = GetChamCongFace(faceModel);

            newitem.lstChamCongBoPhan = new List<HRM_ChamCong_BoPhan>();

            newitem.lstChamCongNhanVien = new List<HRM_ChamCong_NhanVien_BoPhan>();
            var lstChamCongThoiGianAll = new List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan>();

            //Láº¥y há»£p Ä‘á»“ng
            //Láº¥y theo quyáº¿t Ä‘á»‹nh Ä‘i cÃ´ng tÃ¡c
            //Láº¥y cÃ¡c quy trÃ¬nh nghá»‰

            ////Nghá»‰ phÃ©p
            //var mapKyHieuNghiPhep = smdb.DanhMucHinhThucNghis.AsNoTracking().ToDictionary(q => q.Id, q => q.Ma);

            //var lsQuyTrinhXinNghiPhep = smdb.QT_XinNghiPhep_ChiTietNgayNghi.AsNoTracking().Where(q => (q.NgayBatDau >= ngayDauKy && q.NgayBatDau <= ngayCuoiKy) || (q.NgayKetThuc >= ngayDauKy && q.NgayKetThuc <= ngayCuoiKy) || (q.NgayBatDau <= ngayDauKy && q.NgayKetThuc >= ngayCuoiKy))
            //    .Join(smdb.QT_QuyTrinhXinNghiPhep.Where(r => r.IdNguoiDeNghi != null).AsNoTracking()
            //    //.Where(q=> lstIdBoPhanChild.Contains(q.IdBoPhan) 
            //    ////&& q.IdTrangThai == ""
            //    //)
            //    , r1 => r1.IdQuyTrinh, r2 => r2.Id, (r1, r2) => new { ngaynghi = r1, qtnghi = r2 }).ToList()
            //    .Select(q => { var ngaynghi = new EntityQT_XinNghiPhep_ChiTietNgayNghi(q.ngaynghi); var qtchamcong = new EntityQT_QuyTrinhXinNghiPhep(q.qtnghi); ngaynghi.IdNguoiDeNghi = qtchamcong.IdNguoiDeNghi; return ngaynghi; }).ToList();

            //IdNguoiDeNghi => nhÃ¢n viÃªn => hrm nhÃ¢n viÃªn
            //var mapUserNgayNghi = lsQuyTrinhXinNghiPhep.GroupBy(q => q.IdNguoiDeNghi).ToDictionary(q => q.Key, q => q.ToList());


            ////Äi cÃ´ng tÃ¡c
            //var lstDiCongTac = smdb.HRM_QT_KeHoachDiCongTac_UserThamGia.AsNoTracking().ToList();
            //var mapUserDiCongTac = lstDiCongTac.GroupBy(q => q.IdUser).ToDictionary(q => q.Key, q => q.ToList());

            //Convert IdUser sang IdNhanVien

            foreach (var idNhanVien in lstAllNhanVienOfHopDong)
            {
                var lstHopDongOfNhanVien = lstAllHopDong.Where(q => q.IdNhanVien == idNhanVien).ToList();
                var itemNhanVienHRM = idNhanVien.GetValueFromDic(toDictNhanVien);
                List<HRMFaceReponeModel> listChamCongFaceNhanVien = listChamCongFace.Where(r => r.IdNhanVienEOS == itemNhanVienHRM.IdNhanVienEOS).ToList();
                if (itemNhanVienHRM == null)
                    continue;
                long IdNhanVienEOS = itemNhanVienHRM.IdNhanVienEOS;

                //string idUserNhanVien = toDictNhanVienEOS_IdUser.ContainsKey(IdNhanVienEOS) ? toDictNhanVienEOS_IdUser[IdNhanVienEOS] : "";

                ////Check nghi phep
                //var lstNghiPhepNgay = idUserNhanVien.IsNotEmpty() && mapUserNgayNghi.ContainsKey(idUserNhanVien) ? mapUserNgayNghi[idUserNhanVien] : new List<EntityQT_XinNghiPhep_ChiTietNgayNghi>();
                //var lstDiCongTacOfNhanVien = idUserNhanVien.IsNotEmpty() && mapUserDiCongTac.ContainsKey(idUserNhanVien) ? mapUserDiCongTac[idUserNhanVien] : new List<Models.SmartEOEModel.HRM_QT_KeHoachDiCongTac_UserThamGia>();

                foreach (var itemHopDongCoHieuLucTrongThoiGianChamCong in lstHopDongOfNhanVien)
                {
                    var ngayDauKyTheoHopDong = ngayDauKy;
                    if (itemHopDongCoHieuLucTrongThoiGianChamCong.NgayHieuLuc > ngayDauKyTheoHopDong)
                        ngayDauKyTheoHopDong = itemHopDongCoHieuLucTrongThoiGianChamCong.NgayHieuLuc.Date;

                    var ngayCuoiKyTheoHopDong = ngayCuoiKy;
                    if (itemHopDongCoHieuLucTrongThoiGianChamCong.TrangThai != DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && ngayCuoiKyTheoHopDong > itemHopDongCoHieuLucTrongThoiGianChamCong.NgayHetHan)
                    {
                        ngayCuoiKyTheoHopDong = itemHopDongCoHieuLucTrongThoiGianChamCong.NgayHetHan.Value.Date;
                    }
                    var idBoPhanEOS = itemHopDongCoHieuLucTrongThoiGianChamCong.IdBoPhanEOS;
                    var itemChamCong_BoPhan = newitem.lstChamCongBoPhan.Where(q => q.IdBoPhanEOS == idBoPhanEOS).FirstOrDefault();

                    HRM_ChamCong_NhanVien_BoPhan newchamcong_nhanvien = new HRM_ChamCong_NhanVien_BoPhan();
                    newchamcong_nhanvien.IdGuid = Guid.NewGuid().ToString();
                    newchamcong_nhanvien.IdChamCongDetail = newitem.IdGuid;
                    newchamcong_nhanvien.IdChamCongBoPhan = itemChamCong_BoPhan == null ? "" : itemChamCong_BoPhan.IdGuid;
                    newchamcong_nhanvien.IdNhanVien = idNhanVien;
                    newchamcong_nhanvien.IdBoPhanEOS = idBoPhanEOS;
                    newchamcong_nhanvien.idChamCongCongTy = "";
                    newchamcong_nhanvien.lstChamCongThoiGian = new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                    newitem.lstChamCongNhanVien.Add(newchamcong_nhanvien);

                    foreach (var itemNgayChamCong in lstNgayChamCong)
                    {
                        List<HRMFaceReponeModel> listChamCongFaceNgay = listChamCongFaceNhanVien.Where(r => r.NgayChamCong == itemNgayChamCong.NgayCong).ToList();
                        var ngayCongEnd = itemNgayChamCong.NgayCong.AddDays(1).AddMilliseconds(-1);
                        var ngayCong12h = itemNgayChamCong.NgayCong.AddHours(12);

                        ////Äi cÃ´ng tÃ¡c
                        //var lstItemDiCongTacTrongNgayCurr = lstDiCongTacOfNhanVien.Where(q => (q.NgayBatDau >= itemNgayChamCong.NgayCong && q.NgayBatDau <= ngayCongEnd) || (q.NgayKetThuc >= itemNgayChamCong.NgayCong && q.NgayKetThuc <= ngayCongEnd) || (q.NgayBatDau <= itemNgayChamCong.NgayCong && q.NgayKetThuc >= ngayCongEnd)).ToList();
                        //var itemCongTacCaNgay = lstItemDiCongTacTrongNgayCurr.Where(q => (q.NgayBatDau <= ngayCong12h) || (q.NgayKetThuc >= ngayCong12h)).FirstOrDefault();
                        //var itemCongTacSang = lstItemDiCongTacTrongNgayCurr.Where(q => q.NgayKetThuc <= ngayCong12h && q.NgayKetThuc >= itemNgayChamCong.NgayCong).FirstOrDefault();
                        //var itemCongTacChieu = lstItemDiCongTacTrongNgayCurr.Where(q => q.NgayBatDau >= ngayCong12h && q.NgayBatDau <= ngayCongEnd).FirstOrDefault();

                        ////Nghá»‰ phÃ©p
                        //var lstItemNgayNghiTrongNgayCurr = lstNghiPhepNgay.Where(q => (q.NgayBatDau >= itemNgayChamCong.NgayCong && q.NgayBatDau <= ngayCongEnd) || (q.NgayKetThuc >= itemNgayChamCong.NgayCong && q.NgayKetThuc <= ngayCongEnd) || (q.NgayBatDau <= itemNgayChamCong.NgayCong && q.NgayKetThuc >= ngayCongEnd)).ToList();

                        //var itemNghiCaNgay = lstItemNgayNghiTrongNgayCurr.Where(q => q.IdLoaiNghiPhep == idLoaiNghiPhep_CANGAY).FirstOrDefault();
                        //var itemNghiSang = lstItemNgayNghiTrongNgayCurr.Where(q => q.IdLoaiNghiPhep == idLoaiNghiPhep_SANG).FirstOrDefault();
                        //var itemNghiChieu = lstItemNgayNghiTrongNgayCurr.Where(q => q.IdLoaiNghiPhep == idLoaiNghiPhep_CHIEU).FirstOrDefault();

                        HRM_ChamCong_NhanVien_ThoiGian_BoPhan newthoigian = new HRM_ChamCong_NhanVien_ThoiGian_BoPhan(itemNgayChamCong, idNhanVien, newitem.IdGuid, newchamcong_nhanvien.IdGuid);
                        //if (itemNgayChamCong.isNghiLe)
                        //{
                        //    newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.NL.ToString();
                        //    newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.NL.ToString();

                        //}
                        //else if (itemNgayChamCong.isNghiBuNgayLe)
                        //{
                        //    newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.HL.ToString();
                        //    newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.HL.ToString();

                        //}
                        //else if (listChamCongFaceNgay.Count > 1)
                        //{
                        //    DateTime MinDate = listChamCongFaceNgay.Min(r => r.ThoiGianChamCong);
                        //    DateTime MaxDate = listChamCongFaceNgay.Min(r => r.ThoiGianChamCong);
                        //    if (MinDate.Hour < 10 && MaxDate.Hour > 15)
                        //    {
                        //        newthoigian.MaChamCongSang = "F";
                        //        newthoigian.MaChamCongChieu = "F";
                        //    }
                        //    else if (MinDate.Hour > 12 && MaxDate.Hour > 15)
                        //    {
                        //        newthoigian.MaChamCongChieu = "F";
                        //    }
                        //    else if (MinDate.Hour < 10 && MaxDate.Hour < 12)
                        //    {
                        //        newthoigian.MaChamCongSang = "F";
                        //    }
                        //    else
                        //    {
                        //        newthoigian.MaChamCongSang = "F";
                        //        newthoigian.MaChamCongChieu = "F";

                        //    }
                        //}
                        //else
                        //{
                        //    newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.DL.ToString();
                        //    newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.DL.ToString();
                        //}
                        if (itemNgayChamCong.NgayCong < ngayDauKyTheoHopDong || itemNgayChamCong.NgayCong > ngayCuoiKyTheoHopDong)
                        {
                            newthoigian.IdChamCongBoPhan = "";
                            //newthoigian.isHoTroCongTacXa = false;
                            //newthoigian.SoGioSang = 0;
                            //newthoigian.SoGioChieu = 0;
                            //newthoigian.isCheckCa1 = false;
                            //newthoigian.isCheckCa2 = false;
                            newthoigian.isDisable = true;
                            //newthoigian.MaChamCongSang = "";
                            //newthoigian.MaChamCongChieu = "";
                            newthoigian.idHopDong = "";
                            continue;//KhÃ´ng lÆ°u ngÃ y ngoÃ i há»£p Ä‘á»“ng
                        }
                        else
                        {
                            newthoigian.IdChamCongBoPhan = itemChamCong_BoPhan == null ? "" : itemChamCong_BoPhan.IdGuid;
                            newthoigian.IdBoPhanEOS = idBoPhanEOS;
                            newthoigian.IdCongTyEOS = IdCongTyEOS;


                            //if (!isLamThu7 && itemNgayChamCong.NgayCong.DayOfWeek == DayOfWeek.Saturday)
                            //{
                            //    newthoigian.SoGioSang = 0;
                            //    newthoigian.SoGioChieu = 0;
                            //    newthoigian.isCheckCa1 = false;
                            //    newthoigian.isCheckCa2 = false;
                            //    newthoigian.isDisable = true;
                            //    newthoigian.MaChamCongSang = "";
                            //    newthoigian.MaChamCongChieu = "";
                            //    if (itemNgayChamCong.isNghiLe)
                            //    {
                            //        newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.NL.ToString();
                            //        newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.NL.ToString();

                            //    }
                            //    //else if (itemNgayChamCong.isNghiBuNgayLe)
                            //    //{
                            //    //    newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.HL.ToString();
                            //    //    newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.HL.ToString();

                            //    //}
                            //}
                            //if (!isLamChuNhat && itemNgayChamCong.NgayCong.DayOfWeek == DayOfWeek.Sunday)
                            //{
                            //    newthoigian.SoGioSang = 0;
                            //    newthoigian.SoGioChieu = 0;
                            //    newthoigian.isCheckCa1 = false;
                            //    newthoigian.isCheckCa2 = false;
                            //    newthoigian.isDisable = true;
                            //    newthoigian.MaChamCongSang = "";
                            //    newthoigian.MaChamCongChieu = "";
                            //    if (itemNgayChamCong.isNghiLe)
                            //    {
                            //        newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.NL.ToString();
                            //        newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.NL.ToString();

                            //    }
                            //    //else if (itemNgayChamCong.isNghiBuNgayLe)
                            //    //{
                            //    //    newthoigian.MaChamCongSang = DefineData.eMaNgayChamCong.HL.ToString();
                            //    //    newthoigian.MaChamCongChieu = DefineData.eMaNgayChamCong.HL.ToString();

                            //    //}
                            //}
                            newthoigian.idHopDong = itemHopDongCoHieuLucTrongThoiGianChamCong.IdGuid;
                        }


                        //if (!newthoigian.isDisable && newthoigian.MaChamCongSang == DefineData.eMaNgayChamCong.DL.ToString() && (itemNghiCaNgay != null || itemNghiSang != null))
                        //{
                        //    var idHinhThucNghiCur = itemNghiCaNgay != null ? itemNghiCaNgay.IdHinhThucNghi : itemNghiSang.IdHinhThucNghi;

                        //    newthoigian.MaChamCongSang = !string.IsNullOrEmpty(idHinhThucNghiCur) && mapKyHieuNghiPhep.ContainsKey(idHinhThucNghiCur) ? mapKyHieuNghiPhep[idHinhThucNghiCur] : "";
                        //}
                        //if (!newthoigian.isDisable && newthoigian.MaChamCongChieu == DefineData.eMaNgayChamCong.DL.ToString() && (itemNghiCaNgay != null || itemNghiChieu != null))
                        //{
                        //    var idHinhThucNghiCur = itemNghiCaNgay != null ? itemNghiCaNgay.IdHinhThucNghi : itemNghiChieu.IdHinhThucNghi;

                        //    newthoigian.MaChamCongChieu = !string.IsNullOrEmpty(idHinhThucNghiCur) && mapKyHieuNghiPhep.ContainsKey(idHinhThucNghiCur) ? mapKyHieuNghiPhep[idHinhThucNghiCur] : "";
                        //}


                        //if (!newthoigian.isDisable && newthoigian.MaChamCongSang == DefineData.eMaNgayChamCong.DL.ToString() && (itemCongTacCaNgay != null || itemCongTacSang != null))
                        //{
                        //    newthoigian.MaChamCongSang = "CT";
                        //}
                        //if (!newthoigian.isDisable && newthoigian.MaChamCongChieu == DefineData.eMaNgayChamCong.DL.ToString() && (itemCongTacCaNgay != null || itemCongTacChieu != null))
                        //{
                        //    newthoigian.MaChamCongChieu = "CT";
                        //}

                        lstChamCongThoiGianAll.Add(newthoigian);
                    }
                }

            }

            //var lstDotChamCong = db.HRM_dmDotChamCongThang.AsNoTracking().ToList();
            newitem.Created = newitem.Modified = DateTime.Now;

            var result = new EntityHRM_ChamCong_Detail(newitem);
            result.GetDanhSachNgayCongTrongThang(lstDanhMucDotChamCong, LichLamViec, LichNghiLe);
            var lstChamCong_BoPhan = newitem.lstChamCongBoPhan;
            var lstChamCongNhanVienAll = newitem.lstChamCongNhanVien;
            var lstIdNhanVien = lstChamCongNhanVienAll.Select(q => q.IdNhanVien).Distinct().ToList();
            var mapChamCongNhanVienThoiGianAll = lstChamCongThoiGianAll.Select(q => new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(q)).GroupBy(q => q.IdNhanVien).ToDictionary(q => q.Key); ;// db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.AsNoTracking().Where(q => q.IdChamCongDetail == IdGuid).ToList().Select(q => new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(q)).GroupBy(q => q.IdNhanVien).ToDictionary(q => q.Key);
                                                                                                                                                                                              //var lstBoPhan = smdb.BoPhans.ToList();

            var lstChamCongBP = lstChamCong_BoPhan;
            List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongBoPhan = new List<EntityHRM_ChamCong_NhanVien_BoPhan>();
            foreach (var item in lstChamCongBP)
            {
                //var BoPhan = lstBoPhan.FirstOrDefault(r => r.Id == item.IdBoPhanEOS);
                EntityHRM_ChamCong_NhanVien_BoPhan cbbp = new EntityHRM_ChamCong_NhanVien_BoPhan
                {
                    idChamCongCongTy = item.idChamCongCongTy,
                    IdChamCongDetail = item.IdChamCongDetail,
                    IdBoPhanEOS = item.IdBoPhanEOS,
                    IdChamCongBoPhan = item.IdGuid,

                };
                //if (BoPhan != null)
                //    cbbp.TenPhongBan = BoPhan.TenBoPhan;
                lstChamCongBoPhan.Add(cbbp);
            }
            result.lstChamCongBoPhan = lstChamCongBoPhan;


            List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien = new List<EntityHRM_ChamCong_NhanVien_BoPhan>();
            foreach (var idNhanVien in lstIdNhanVien)
            {
                var itemChamCongNhanVien = new EntityHRM_ChamCong_NhanVien_BoPhan();
                itemChamCongNhanVien.lstChamCongThoiGian = new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                itemChamCongNhanVien.IdNhanVien = idNhanVien;
                long IdBoPhan = lstChamCongNhanVienAll.Where(r => r.IdNhanVien == idNhanVien).Select(r => r.IdBoPhanEOS).FirstOrDefault();
                itemChamCongNhanVien.IdBoPhanEOS = IdBoPhan;
                //var BoPhanEOS = lstBoPhan.FirstOrDefault(r => r.Id == IdBoPhan);
                //if (BoPhanEOS != null)
                //    itemChamCongNhanVien.TenPhongBan = BoPhanEOS.TenBoPhan;

                itemChamCongNhanVien.MaNhanVien = itemChamCongNhanVien.IdNhanVien.GetValueFromDic(toDictNhanVien, x => x.MaNhanVien);
                itemChamCongNhanVien.TenNhanVien = idNhanVien.GetValueFromDic(toDictNhanVien, x => x.TenDayDu);

                var lstChamCongThoiGian = mapChamCongNhanVienThoiGianAll.ContainsKey(idNhanVien) ? mapChamCongNhanVienThoiGianAll[idNhanVien]

                    .OrderBy(q => q.NgayCong).ToList() : new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();

                foreach (var itemThoiGian in result.lstNgayChamCongTrongKy)
                {
                    var itemChamCongThoiGian = lstChamCongThoiGian.FirstOrDefault(q => q.CheckExist(itemThoiGian));

                    if (itemChamCongThoiGian == null)
                    {
                        itemChamCongThoiGian = new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(itemThoiGian);
                    }
                    else

                        itemChamCongNhanVien.lstChamCongThoiGian.Add(itemChamCongThoiGian);
                }
                lstChamCongNhanVien.Add(itemChamCongNhanVien);
            }
            result.lstChamCongNhanVien = lstChamCongNhanVien.OrderBy(r => r.TenNhanVien).ThenBy(r => r.TenPhongBan).ToList();

            return result;

        }


        public EntityHRM_ChamCong_Detail GetItemChamCongDetail(string Id, string idCurrentUser, ApplicationDbContext db = null)
        {
            #region data
            //{IdGuid:9, CallFromFunc:0}
            #endregion
            string IdGuid = Id;
            //string CallFromFunc = dyn.CallFromFunc;

            ////Test import cham cong
            //if(CallFromFunc != "1")
            //{
            //    dynamic sdyn = new DynamicObject();
            //    sdyn.idChamCongDetail = IdGuid;
            //    return ImportChamCongTuMayChamCong(sdyn, idCurrentUser, db);
            //}




            var toDictNhanVien = db.HRM_NhanVien.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            //var toDictBoPhan = smdb.BoPhans.AsNoTracking().ToList().ToDict(x => x.Id);
            //var lstCaiDatGioLamViec = smdb.CaiDatGioLamViecs.AsNoTracking().ToList();
            //var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe" || q.Ma == "NgayNghiBu").ToList();

            var result1 = db.HRM_ChamCong_Detail.AsNoTracking().FirstOrDefault(q => q.IdGuid == IdGuid);
            var result = new EntityHRM_ChamCong_Detail(result1);
            var lstDanhMucDotChamCong = db.HRM_dmDotChamCongThang.AsNoTracking().ToList();
            HRM_dmLichLamViec LichLamViec = db.HRM_dmLichLamViec.Where(x => x.isApDung == true).FirstOrDefault();
            if (LichLamViec != null)
                LichLamViec.listNgay = db.HRM_dmLichLamViec_Ngay.Where(x => x.IdLichLamViec == LichLamViec.IdGuid).ToList();
            HRM_dmLichNghiLe LichNghiLe = db.HRM_dmLichNghiLe.Where(x => x.isApDung == true).FirstOrDefault();
            if (LichNghiLe != null)
                LichNghiLe.listNgay = db.HRM_dmLichNghiLe_Ngay.Where(x => x.IdLichNghiLe == LichLamViec.IdGuid).ToList();
            result.GetDanhSachNgayCongTrongThang(lstDanhMucDotChamCong, LichLamViec, LichNghiLe);

            var lstChamCong_BoPhan = db.HRM_ChamCong_BoPhan.AsNoTracking().Where(q => q.IdChamCongDetail == IdGuid).ToList();
            var lstChamCongNhanVienAll = db.HRM_ChamCong_NhanVien_BoPhan.AsNoTracking().Where(q => q.IdChamCongDetail == IdGuid).ToList();
            var lstIdNhanVien = lstChamCongNhanVienAll.Where(q => q.IdChamCongDetail == IdGuid).Select(q => q.IdNhanVien).Distinct().ToList();
            var mapChamCongNhanVienThoiGianAll = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.AsNoTracking().Where(q => q.IdChamCongDetail == IdGuid).ToList().Select(q => new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(q)).GroupBy(q => q.IdNhanVien).ToDictionary(q => q.Key);
            var lstBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            //   List<EntityHRM_ChamCong_NhanVien_BoPhan>  lstChamCongBoPhan = new List<EntityHRM_ChamCong_NhanVien_BoPhan>(); //lstChamCong_BoPhan;
            //result.lstChamCongNhanVien = lstChamCongNhanVienAll;
            var lstChamCongBP = db.HRM_ChamCong_BoPhan.Where(r => r.IdChamCongDetail == IdGuid).ToList();
            List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongBoPhan = new List<EntityHRM_ChamCong_NhanVien_BoPhan>();
            foreach (var item in lstChamCongBP)
            {
                var BoPhan = lstBoPhan.FirstOrDefault(r => r.Id == item.IdBoPhanEOS);
                EntityHRM_ChamCong_NhanVien_BoPhan cbbp = new EntityHRM_ChamCong_NhanVien_BoPhan
                {
                    idChamCongCongTy = item.idChamCongCongTy,
                    IdChamCongDetail = item.IdChamCongDetail,
                    IdBoPhanEOS = item.IdBoPhanEOS,
                    IdChamCongBoPhan = item.IdGuid,

                };
                if (BoPhan != null)
                    cbbp.TenPhongBan = BoPhan.TenBoPhan;
                lstChamCongBoPhan.Add(cbbp);
            }
            result.lstChamCongBoPhan = lstChamCongBoPhan;


            List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien = new List<EntityHRM_ChamCong_NhanVien_BoPhan>();
            foreach (var idNhanVien in lstIdNhanVien)
            {
                var itemChamCongNhanVien = new EntityHRM_ChamCong_NhanVien_BoPhan();
                itemChamCongNhanVien.lstChamCongThoiGian = new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                itemChamCongNhanVien.IdNhanVien = idNhanVien;
                long IdBoPhan = lstChamCongNhanVienAll.Where(r => r.IdNhanVien == idNhanVien).Select(r => r.IdBoPhanEOS).FirstOrDefault();
                itemChamCongNhanVien.IdBoPhanEOS = IdBoPhan;
                var BoPhanEOS = lstBoPhan.FirstOrDefault(r => r.Id == IdBoPhan);
                if (BoPhanEOS != null)
                    itemChamCongNhanVien.TenPhongBan = BoPhanEOS.TenBoPhan;

                itemChamCongNhanVien.MaNhanVien = itemChamCongNhanVien.IdNhanVien.GetValueFromDic(toDictNhanVien, x => x.MaNhanVien);
                itemChamCongNhanVien.TenNhanVien = idNhanVien.GetValueFromDic(toDictNhanVien, x => x.TenDayDu);
                var lstChamCongThoiGian = mapChamCongNhanVienThoiGianAll.ContainsKey(idNhanVien) ? mapChamCongNhanVienThoiGianAll[idNhanVien]
                //.Select(q=>new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(q))
                .OrderBy(q => q.NgayCong).ToList() : new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();//itemChamCongNhanVien.IdGuid.GetValueFromDic(mapChamCongNhanVienThoiGianAll);

                foreach (var itemThoiGian in result.lstNgayChamCongTrongKy)
                {
                    var itemChamCongThoiGian = lstChamCongThoiGian.FirstOrDefault(q => q.CheckExist(itemThoiGian));

                    if (itemChamCongThoiGian == null)
                    {
                        itemChamCongThoiGian = new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan(itemThoiGian);
                    }
                    else

                        itemChamCongNhanVien.lstChamCongThoiGian.Add(itemChamCongThoiGian);
                }
                lstChamCongNhanVien.Add(itemChamCongNhanVien);
            }
            result.lstChamCongNhanVien = lstChamCongNhanVien.OrderBy(r => r.TenNhanVien).ThenBy(r => r.TenPhongBan).ToList();
            //result.lstChamCongNhanVien = result.lstChamCongNhanVien.Take(1).ToList();
            //foreach (var itemChamCongNhanVien in lstChamCongNhanVienAll)
            //{
            //    itemChamCongNhanVien.lstChamCongThoiGian = itemChamCongNhanVien.lstChamCongThoiGian.Take(1).ToList(); 
            //}
            return result;
        }
        public dynamic DeleteBangChamCongDetail(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_ChamCong_Detail chamCong = db.HRM_ChamCong_Detail.Find(Id);

                    List<HRM_ChamCong_BoPhan> lstChamCongBoPhan = db.HRM_ChamCong_BoPhan.Where(r => r.IdChamCongDetail == Id).ToList();

                    db.HRM_ChamCong_BoPhan.RemoveRange(lstChamCongBoPhan);
                    List<HRM_ChamCong_NhanVien_BoPhan> lstChamCongNhanVien = db.HRM_ChamCong_NhanVien_BoPhan.Where(r => r.IdChamCongDetail == Id).ToList();
                    db.HRM_ChamCong_NhanVien_BoPhan.RemoveRange(lstChamCongNhanVien);
                    db.HRM_ChamCong_Detail.Remove(chamCong);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "XÃ³a báº£ng cháº¥m cÃ´ng thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½ " + ex.Message);
                }
            }
        }

        public string FuncUpdateBangChamCong(HRM_ChamCong_Detail model, ApplicationDbContext db)
        {

            try
            {
                var listChamCongBoPhan = model.lstChamCongBoPhan;
                if (model.lstChamCongBoPhan.Count == 0 || model.lstChamCongBoPhan == null)
                    return "Vui lÃ²ng chá»n bá»™ pháº­n cháº¥m cÃ´ng";
                if (model.lstChamCongNhanVien.Count == 0 || model.lstChamCongNhanVien == null)
                    return "Vui lÃ²ng chá»n bá»™ pháº­n cÃ³ nhÃ¢n viÃªn Ä‘á»ƒ cháº¥m cÃ´ng";
                List<long> lstIdBoPhan = listChamCongBoPhan.Select(r => r.IdBoPhanEOS).ToList();

                var listBoPhanDaChamCongs = (from a in db.HRM_ChamCong_Detail
                                             join b in db.HRM_ChamCong_BoPhan on a.IdGuid equals b.IdChamCongDetail
                                             where lstIdBoPhan.Contains(b.IdBoPhanEOS) && a.IdDanhMucDotChamCongThang == model.IdDanhMucDotChamCongThang
                                             && a.Nam == model.Nam && a.Thang == model.Thang && a.IdGuid != model.IdGuid
                                             select b).ToList();

                if (listBoPhanDaChamCongs.Count() > 0)
                {
                    var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, lstIdBoPhan);
                    string TenBoPhans = "";
                    foreach (var item in listBoPhanDaChamCongs)
                    {
                        var BoPhans = listBoPhan.Where(r => r.Id == item.IdBoPhanEOS).FirstOrDefault();
                        if (BoPhans != null)
                            TenBoPhans = TenBoPhans + BoPhans.TenBoPhan + "; ";

                    }
                    string res = "ÄÃ£ tá»“n táº¡i báº£ng cháº¥m cÃ´ng cá»§a bá»™ pháº­n Ä‘Ã£ chá»n " + TenBoPhans;
                    return res;
                }

                if (string.IsNullOrEmpty(model.IdGuid))
                {

                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_ChamCong_Detail.Add(model);

                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    var lstItemChamCongNhanVienThoiGianCu = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.Where(q => q.IdChamCongDetail == model.IdGuid).ToList();
                    db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.RemoveRange(lstItemChamCongNhanVienThoiGianCu);

                    var lstChamCongNhanVien = db.HRM_ChamCong_NhanVien_BoPhan.Where(q => q.IdChamCongDetail == model.IdGuid).ToList();
                    db.HRM_ChamCong_NhanVien_BoPhan.RemoveRange(lstChamCongNhanVien);
                    var lstChamCongBoPhanOld = db.HRM_ChamCong_BoPhan.Where(q => q.IdChamCongDetail == model.IdGuid).ToList();
                    db.HRM_ChamCong_BoPhan.RemoveRange(lstChamCongBoPhanOld);
                }
                foreach (var item in model.lstChamCongBoPhan)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.Nam = model.Nam;
                    item.Thang = model.Thang;
                    item.IdDanhMucDotChamCongThang = model.IdDanhMucDotChamCongThang;
                    item.IdChamCongDetail = model.IdGuid;
                }
                db.HRM_ChamCong_BoPhan.AddRange(model.lstChamCongBoPhan);

                List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> listNhanVienChamCongBoPhanthoiGian = new List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                foreach (var item in model.lstChamCongNhanVien)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdChamCongDetail = model.IdGuid;
                    List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> listitem = item.lstChamCongThoiGian.Select(q1 => new HRM_ChamCong_NhanVien_ThoiGian_BoPhan(q1)).ToList();
                    foreach (var itemTG in listitem)
                    {
                        itemTG.IdGuid = Guid.NewGuid().ToString();
                        itemTG.IdChamCongDetail = model.IdGuid;
                        itemTG.IdChamCongBoPhan = item.IdGuid;
                    }
                    listNhanVienChamCongBoPhanthoiGian.AddRange(listitem);
                }
                db.HRM_ChamCong_NhanVien_BoPhan.AddRange(model.lstChamCongNhanVien);

                db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.AddRange(listNhanVienChamCongBoPhanthoiGian);

                db.SaveChanges();
                return "0";


            }
            catch (Exception ex)
            {
                string ms = "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½" + ex.Message;
                return ms;
            }


        }



        public dynamic GetDanhSachBangChamCongTongHop(dynamic data, ApplicationDbContext db)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            int CurrentPage = itemTimKiem.CurrentPage ?? 1;
            int PageSize = itemTimKiem.PageSize ?? 15;

            IQueryable<HRM_ChamCong_Detail> listChamCongDaDuyet = from a in db.HRM_ChamCong_Detail
                                                                   where (itemTimKiem.IdCongTyEOS == 0 || a.IdCongTyEOS == itemTimKiem.IdCongTyEOS)
                                                                 && a.Thang == itemTimKiem.Thang && a.Nam == itemTimKiem.Nam && a.isKetThuc == true
                                                                   select a;
            int count = listChamCongDaDuyet.Count();
            var listItem = listChamCongDaDuyet.OrderByDescending(q => q.Modified).Skip(PageSize * (CurrentPage - 1)).Take(PageSize).ToList();
            List<string> listId = listItem.Select(r => r.IdGuid).ToList();
            List<HRM_ChamCong_BoPhan> lstChamCongBoPhans = db.HRM_ChamCong_BoPhan.Where(r => listId.Contains(r.IdChamCongDetail)).ToList();
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);


            foreach (var item in listItem)
            {
                List<HRM_ChamCong_BoPhan> lstChamCongBoPhan = lstChamCongBoPhans.Where(r => r.IdChamCongDetail == item.IdGuid).ToList();
                foreach (var itemBP in lstChamCongBoPhan)
                {
                    BoPhan boPhan = BoPhans.Where(r => r.Id == itemBP.IdBoPhanEOS).FirstOrDefault();
                    if (boPhan == null)
                        continue;
                    if (item.TenBoPhans == null || item.TenBoPhans == "")
                        item.TenBoPhans = boPhan.TenBoPhan;
                    else
                        item.TenBoPhans = item.TenBoPhans + "; " + boPhan.TenBoPhan;
                }

            }
            dynamic dnm = new DynamicObject();
            dnm.paging = new PagingInfor(CurrentPage, count, (int)count / PageSize + (count % PageSize > 0 ? 1 : 0));
            dnm.items = listItem;
            return dnm;
        }
        public void GetGioLamViec(List<HRM_TheoDoiLichLamViecNhanVien> listLichLamViec, HRM_NhanVien NhanVienFind, DateTime? ngayCong)
        {
            List<HRM_TheoDoiLichLamViecNhanVien> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == NhanVienFind.IdGuid && x.Ngay == ngayCong.Value);


            foreach (HRM_TheoDoiLichLamViecNhanVien itemTheoDoi in listLichLamViecFind)
            {

            }
        }


        public dynamic ImportChamCongTuExcel(string FileName, string Id, ApplicationDbContext db = null)
        {
            #region data
            //{idChamCongDetail:"", file:{HDFile}}
            #endregion
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                    string idChamCongDetail = Id;
                    var toDictNhanVien = db.HRM_NhanVien.AsNoTracking().ToList().ToDict(x => x.IdGuid);
                    //var toDictBoPhan = smdb.BoPhans.AsNoTracking().ToList().ToDict(x => x.Id);
                    var lstDotChamCong = db.HRM_dmDotChamCongThang.AsNoTracking().ToList();
                    var lstItemChamCongNhanVienThoiGianCu = db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.Where(q => q.IdChamCongDetail == idChamCongDetail).ToList();
                    List<string> listIdNhanVienDuocChamCong = lstItemChamCongNhanVienThoiGianCu.Select(r => r.IdNhanVien).ToList();



                    //var lstCaiDatGioLamViec = smdb.CaiDatGioLamViecs.AsNoTracking().ToList();
                    //var lstCaiDatNghiLe = smdb.CaiDatHeThongs.AsNoTracking().Where(q => q.Ma == "NgayNghiLe" || q.Ma == "NgayNghiBu").ToList();

                    //var itemChamCongDetail = db.HRM_ChamCong_Detail.AsNoTracking().FirstOrDefault(q => q.IdGuid == idChamCongDetail);
                    //itemChamCongDetail.GetDanhSachNgayCongTrongThang(lstDotChamCong, lstCaiDatNghiLe);

                    //HDFile file = JsonConvert.DeserializeObject<HDFile>(dyn.file.ToString());
                    string pathTemplate = PathUploads + FileName;
                    using Stream s = File.OpenRead(pathTemplate);
                    using ExcelPackage package = new ExcelPackage(s);
                    s.Close();
                    var sheet1 = package.Workbook.Worksheets[1];
                    int row = 5;
                    int maxrow = sheet1.Dimension.Rows;
                    int maxCol = sheet1.Dimension.Columns;
                    int colVao = 7;

                    //Check maxCol
                    for (var nCol = colVao; nCol < maxCol; nCol++)
                    {
                        string sTongGioText = GetCell(sheet1, 4, nCol);
                        if (sTongGioText.IsEmpty())
                        {
                            maxCol = nCol - 1;
                            break;
                        }
                    }
                    List<string> listMaNhanVien = new List<string>();
                    List<EntityHRM_ChamCong_NhanVien_BoPhan> lstChamCongImport = new List<EntityHRM_ChamCong_NhanVien_BoPhan>();
                    DateTime dtBegin = DateTime.Now.AddYears(10);
                    DateTime dtEnd = DateTime.Now.AddYears(-10);
                    for (int i = row; i <= maxrow; i++)
                    {
                        DateTime? ngayCong = dungchung.GetCellToDateTime(sheet1, i, 1);
                        string sMaNhanVien = GetCell(sheet1, i, 3);
                        if (sMaNhanVien.IsEmpty() || ngayCong == null)
                            continue;
                        listMaNhanVien.Add(sMaNhanVien);
                        if (dtBegin > ngayCong)
                            dtBegin = ngayCong.Value;
                        if (dtEnd < ngayCong)
                            dtEnd = ngayCong.Value;
                    }
                    listMaNhanVien = listMaNhanVien.Distinct().ToList();
                    List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listMaNhanVien.Contains(x.MaNhanVien)).ToList();
                    List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                    dtBegin = new DateTime(dtBegin.Year, dtBegin.Month, dtBegin.Day, 0, 0, 0);
                    dtEnd = new DateTime(dtEnd.Year, dtEnd.Month, dtEnd.Day, 0, 0, 0).AddDays(1);
                    List<HRM_TheoDoiLichLamViecNhanVien> listLichLamViec = db.HRM_TheoDoiLichLamViecNhanVien.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                                    && x.Ngay >= dtBegin && x.Ngay < dtEnd).ToList();
                    List<HRM_dmLichLamViec> dmLichLamViecs = db.HRM_dmLichLamViec.Where(a => a.isApDung == true).ToList();
                    List<HRM_dmLichLamViec_Ngay> dmLichLamViecNgays = db.HRM_dmLichLamViec_Ngay.ToList();
                    List<HRM_dmLichLamViec_Gio> dmLichLamViecGios = db.HRM_dmLichLamViec_Gio.ToList();
                    for (int i = row; i <= maxrow; i++)
                    {
                        DateTime? ngayCong = dungchung.GetCellToDateTime(sheet1, i, 1);
                        string sMaNhanVien = dungchung.GetCell(sheet1, i, 3);
                        if (sMaNhanVien.IsEmpty() || ngayCong == null)
                            continue;
                        HRM_NhanVien NhanVienFind = listNhanVien.Find(x => x.MaNhanVien == sMaNhanVien);
                        if (NhanVienFind == null)
                            continue;
                        ngayCong = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);

                        var itemChamCongImport = lstChamCongImport.FirstOrDefault(q => q.MaNhanVien == sMaNhanVien);
                        if (itemChamCongImport == null)
                        {
                            itemChamCongImport = new EntityHRM_ChamCong_NhanVien_BoPhan();
                            itemChamCongImport.MaNhanVien = sMaNhanVien;
                            itemChamCongImport.lstChamCongThoiGian = new List<EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                            lstChamCongImport.Add(itemChamCongImport);
                        }
                        string sGioVaoDau = "";
                        string sGioRaCuoi = "";
                        for (var nCol = colVao; nCol < maxCol; nCol++)
                        {
                            string sColText = GetCell(sheet1, i, nCol);
                            if (!sColText.IsEmpty())
                            {
                                if (sGioVaoDau.IsEmpty())
                                    sGioVaoDau = sColText;
                                else
                                    sGioRaCuoi = sColText;
                            }
                        }
                        if (sGioVaoDau.IsNotEmpty() && sGioRaCuoi.IsNotEmpty())
                        {
                            double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sGioVaoDau);
                            double dGioRaCuoi = dungchung.ConvertGioPhutSangGio(sGioRaCuoi);
                            double dTongGioLam = dGioRaCuoi - dGioVaoDau;

                            int gio = 0, phut = 0;
                            dungchung.ConvertGioPhut(sGioVaoDau, ref gio, ref phut);
                            DateTime dtGioVaoDau = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, gio, phut, 0);
                            //LÃ m theo concept xÃ¡c Ä‘á»‹nh lÃ m sÃ¡ng chiá»u cá»§a ca lÃ m viá»‡c
                            if (dTongGioLam > 0)
                            {
                                EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan itemThoiGian = new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan();
                                itemThoiGian.Ngay = ngayCong.Value.Day;
                                itemThoiGian.Thang = ngayCong.Value.Month;
                                itemThoiGian.SoGioLamViec = dTongGioLam;
                                var dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Loai == DefineData.eKyHieuChamCong.DL.ToString());
                                itemThoiGian.IddmChamCong = dmKyHieuChamCong.IdGuid;
                                //itemThoiGian.MaChamCongSang = DefineData.eMaNgayChamCong.DL.ToString();
                                //itemThoiGian.SoGioSang = 4;
                                //if (dTongGioLam > 6)
                                //{
                                //    itemThoiGian.MaChamCongChieu = DefineData.eMaNgayChamCong.DL.ToString();
                                //    itemThoiGian.SoGioChieu = 4;
                                //}
                                itemChamCongImport.lstChamCongThoiGian.Add(itemThoiGian);
                                List<HRM_TheoDoiLichLamViecNhanVien> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == NhanVienFind.IdGuid && x.Ngay == ngayCong);
                                if (listLichLamViecFind.Count == 1)
                                {
                                    string IdLichLamViec = listLichLamViecFind.FirstOrDefault().IdLichLamViec;
                                    string IdLichLamViecNgay = "";
                                    HRM_dmLichLamViec_Ngay dmLichLamViecNgay = dmLichLamViecNgays.Find(x => x.IdLichLamViec == IdLichLamViec && x.Thu == (int)ngayCong.Value.DayOfWeek);
                                    if (dmLichLamViecNgay != null)
                                        IdLichLamViecNgay = dmLichLamViecNgay.IdGuid;
                                    HRM_dmLichLamViec_Gio dmLichLamViecGio = dmLichLamViecGios.Find(x => x.IdLichLamViec == IdLichLamViec && x.IdLichLamViecNgay == IdLichLamViecNgay && x.TuGio.Year != 1790 && x.Ca == 1);
                                    if (dmLichLamViecGio == null)
                                        continue;
                                    DateTime dtGioVaoDauLich = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, dmLichLamViecGio.TuGio.Hour, dmLichLamViecGio.TuGio.Minute, 0);
                                    if (dtGioVaoDau > dtGioVaoDauLich)
                                    {
                                        dmKyHieuChamCong = dmKyHieuChamCongs.Find(x => x.Loai == DefineData.eKyHieuChamCong.M.ToString());
                                        if (dmKyHieuChamCong != null)
                                        {
                                            EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan itemThoiGianK = new EntityHRM_ChamCong_NhanVien_ThoiGian_BoPhan();
                                            itemThoiGianK.Ngay = ngayCong.Value.Day;
                                            itemThoiGianK.Thang = ngayCong.Value.Month;
                                            var SoGioKhac = (dtGioVaoDau - dtGioVaoDauLich);
                                            itemThoiGianK.SoGioKhac = SoGioKhac.TotalSeconds / 3600;
                                            itemThoiGian.IddmChamCong = dmKyHieuChamCong.IdGuid;
                                            //itemThoiGian.MaChamCongSang = DefineData.eMaNgayChamCong.DL.ToString();
                                            //itemThoiGian.SoGioSang = 4;
                                            //if (dTongGioLam > 6)
                                            //{
                                            //    itemThoiGian.MaChamCongChieu = DefineData.eMaNgayChamCong.DL.ToString();
                                            //    itemThoiGian.SoGioChieu = 4;
                                            //}
                                            itemChamCongImport.lstChamCongThoiGian.Add(itemThoiGian);
                                        }

                                    }
                                }
                                else
                                {
                                    foreach (HRM_TheoDoiLichLamViecNhanVien itemTheoDoi in listLichLamViecFind)
                                    {
                                    }

                                }

                            }


                        }
                    }
                    List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> listNhanVienChamCongImport = new List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan>();
                    var lstChamCongThoiGian = lstChamCongImport.SelectMany(r => r.lstChamCongThoiGian).ToList();
                    foreach (var item in lstChamCongThoiGian)
                    {
                        HRM_ChamCong_NhanVien_ThoiGian_BoPhan nhanvien = new HRM_ChamCong_NhanVien_ThoiGian_BoPhan(item);
                        listNhanVienChamCongImport.Add(nhanvien);
                    }
                    List<HRM_ChamCong_NhanVien_ThoiGian_BoPhan> listNhanVienChamCongNew = (from a in lstItemChamCongNhanVienThoiGianCu
                                                                                           join b in listNhanVienChamCongImport on a.IdNhanVien equals b.IdNhanVien
                                                                                           select b).ToList();
                    foreach (var item in listNhanVienChamCongNew)
                    {
                        HRM_ChamCong_NhanVien_ThoiGian_BoPhan nhanVienBP = lstItemChamCongNhanVienThoiGianCu.FirstOrDefault(r => r.IdNhanVien == item.IdNhanVien && item.Ngay == r.Ngay && r.Thang == item.Thang);
                        if (nhanVienBP == null)
                            continue;
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.idChamCongCongTy = nhanVienBP.idChamCongCongTy;
                        item.IdCongTyEOS = nhanVienBP.IdCongTyEOS;
                        item.IdChamCongDetail = nhanVienBP.IdChamCongDetail;
                        item.idHopDong = nhanVienBP.idHopDong ?? "";
                        item.IdChamCongNhanVienBoPhan = nhanVienBP.IdChamCongNhanVienBoPhan ?? "";
                        item.Nam = nhanVienBP.Nam;

                    }
                    db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.RemoveRange(lstItemChamCongNhanVienThoiGianCu);
                    db.HRM_ChamCong_NhanVien_ThoiGian_BoPhan.AddRange(listNhanVienChamCongNew);
                    db.SaveChanges();
                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thành công file chấm công");
                }

                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + ex.ToString());
                }
            }
        }

        public dynamic ExportMauFileChamCong()
        {
            try
            {
                string sFile = "MauChamCong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);

                var filenamegui = "Bangchamcong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
            }
        }
        #region Quy trÃ¬nh cháº¥m cÃ´ng bá»™ pháº­n
        public dynamic GetNextMaChamCong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCode(db.HRM_ChamCong_Detail.Select(x => x.SoPhieu), DefineData.MaChamCong));
        public dynamic GetDanhSachChamCongDetailQuyTrinh(EntityGetDanhSachChamCongDetailRequest model, ApplicationDbContext db)
        {
            // TimKiem itemTimKiem = JsonConvert.DeserializeObject<TimKiem>(dyn.ToString());
            try
            {
                string KeyWord = model.KeyWord;
                int nCurrentPage = model.CurrentPage;
                int PageSize = model.NumperPage;
                int nTabTrangThai = model.TabTrangThai;
                string IdDotChamCong = model.IdDotChamCong;
                IQueryable<HRM_ChamCong_Detail> query = db.HRM_ChamCong_Detail;
                if (!string.IsNullOrEmpty(KeyWord))
                    query = query.Where(dm => dm.SoPhieu.Contains(KeyWord));
                if (model.Thang > 0)
                    query = query.Where(dm => dm.Thang == model.Thang);
                if (model.Nam > 0)
                    query = query.Where(dm => dm.Nam == model.Nam);
                if (model.IdCongTyEOS > 0)
                    query = query.Where(dm => dm.IdCongTyEOS == model.IdCongTyEOS);
                if (!string.IsNullOrEmpty(IdDotChamCong))
                    query = query.Where(dm => dm.IdDanhMucDotChamCongThang == IdDotChamCong);

                List<HRM_ChamCong_Detail> listItem = GetPagedWorkflowList(query, db, nTabTrangThai, nCurrentPage, nPageSize, out PagingInfor paging);
                if (nTabTrangThai == -1)
                    return paging.TotalItem;
                var dotChamCongById = db.HRM_dmDotChamCongThang.AsNoTracking().ToDictionary(r => r.IdGuid, r => r.Ten);
                var detailIds = listItem.Select(x => x.IdGuid).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                var boPhanByDetail = detailIds.Count > 0
                    ? db.HRM_ChamCong_BoPhan.AsNoTracking().Where(r => detailIds.Contains(r.IdChamCongDetail)).ToLookup(r => r.IdChamCongDetail)
                    : Enumerable.Empty<HRM_ChamCong_BoPhan>().ToLookup(r => r.IdChamCongDetail);
                foreach (var item in listItem)
                {
                    item.lstChamCongBoPhan = boPhanByDetail[item.IdGuid].ToList();
                    item.TenDanhMucDotChamCong = !string.IsNullOrEmpty(item.IdDanhMucDotChamCongThang) && dotChamCongById.TryGetValue(item.IdDanhMucDotChamCongThang, out var tenDot) ? tenDot : null;
                }
                //foreach (var item in listItem)
                //{
                //    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                //    item.NgayHieuLucUnix = DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                //    item.TuNgayUnix = DateTimeToUnixTimeStamp(item.TuNgay);
                //    item.DenNgayUnix = DateTimeToUnixTimeStamp(item.DenNgay);

                //}
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = listItem;
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra --" + e.Message);
            }
            ////Kiá»ƒm tra theo id ngÆ°á»i táº¡o
            //var result = new EntityResponse<HRM_ChamCong_Detail>();
            //var lstresultall = aContext.HRM_ChamCong_Detail.AsNoTracking().Where(q => q.IdCongTyEOS == aRequest.IdCongTyEOS && (string.IsNullOrEmpty(aRequest.IdDotChamCong) || aRequest.IdDotChamCong == q.IdDanhMucDotChamCongThang) && (aRequest.Nam == 0 || aRequest.Nam == q.Nam) && (aRequest.Thang == 0 || aRequest.Thang == q.Thang))
            //    .Join(aContext.HRM_QuyTrinhXuLy.AsNoTracking().Where(q => q.IdUser == IdCurrentUser && q.isDaXuLy == aRequest.TabTrangThai - 1), r1 => r1.IdGuid, r2 => r2.IdTable, (r1, r2) => r1)
            //    .ToList();
            //int count = lstresultall.Count();
            //var lstresult = lstresultall.Skip(aRequest.NumperPage * (aRequest.CurrentPage - 1)).Take(aRequest.NumperPage).ToList();
            //result.paging = new PagingInfor(aRequest.CurrentPage, count, (int)count / aRequest.NumperPage + (count % aRequest.NumperPage > 0 ? 1 : 0));
            //result.items = lstresult;
            //return result;
        }
        public dynamic SetQuyTrinhChamCongDetail(HRM_ChamCong_Detail item, ApplicationDbContext db = null)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng danh sÃ¡ch khÃ´ng cÃ³ lÆ°Æ¡ng");
                    string check = FuncUpdateBangChamCong(item, db);
                    if (check != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);

                    }

                    if (string.IsNullOrEmpty(item.IdTrangThai))
                    {
                        // DI: use inherited _CallWebService
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(db, DefineData.eAction.QUYTRINHCHAMCONG.ToString(), item.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {
                            dbContextTransaction.Rollback();
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "KhÃ´ng tÃ¬m tháº¥y tráº¡ng thÃ¡i dá»¯ liá»‡u khi chuyá»ƒn");
                        }

                        item.IdTrangThai = trangThai.IdTrangThai;
                        item.TenTrangThai = trangThai.TenTrangThai;
                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }

                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng", item.IdGuid);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh khÃ´ng cÃ´ng");
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhChamCongDetail(HRM_ChamCong_Detail item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng danh sÃ¡ch khÃ´ng cÃ³ lÆ°Æ¡ng");
                    string check = FuncUpdateBangChamCong(item, db);
                    if (check != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);

                    }
                    var trangThai = GetTrangThaiChuyenTiep(db, DefineData.eAction.QUYTRINHCHAMCONG.ToString(), item.IdTrangThai, item.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "KhÃ´ng tÃ¬m tháº¥y tráº¡ng thÃ¡i dá»¯ liá»‡u tuyá»ƒn dá»¥ng khi chuyá»ƒn");
                    }

                    item.IdTrangThai = trangThai.IdTrangThai;
                    item.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    db.SaveChanges();



                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhChamCongDetail(HRM_ChamCong_Detail item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string check = FuncUpdateBangChamCong(item, db);
                    if (check != "0")
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check);

                    }

                    var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHCHAMCONG.ToString(), item.IdTrangThai, item.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "KhÃ´ng tÃ¬m tháº¥y tráº¡ng thÃ¡i dá»¯ liá»‡u tuyá»ƒn dá»¥ng khi chuyá»ƒn");
                    }

                    item.IdTrangThai = trangThai.IdTrangThai;
                    item.TenTrangThai = trangThai.TenTrangThai;
                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        #endregion
        #region Quy trÃ¬nh cháº¥m cÃ´ng bá»™ pháº­n New
        public dynamic GetNextMaBangChamCong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                DefineData.MaChamCong,
                db.HRM_BangChamCong.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListBangChamCong(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_BangChamCong> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_BangChamCong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_BangChamCong
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0 && TimKiem.DenNgay > 0)
                {
                    result = result.Where(a => (a.TuNgay <= dtTuNgay && a.DenNgay >= dtTuNgay) || (a.TuNgay >= dtTuNgay && a.TuNgay <= dtDenNgay)
                    || (a.DenNgay <= dtDenNgay && a.DenNgay >= dtTuNgay));
                }
                else
                {
                    if (TimKiem.TuNgay > 0)
                        result = result.Where(a => (dtTuNgay >= a.TuNgay && dtTuNgay <= a.DenNgay));
                    if (TimKiem.DenNgay > 0)
                        result = result.Where(a => (dtDenNgay >= a.TuNgay && dtDenNgay <= a.DenNgay));
                }
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTyEOS > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTyEOS);
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                //List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                foreach (HRM_BangChamCong item in itemList)
                {
                    item.DateTimeToUnix();
                    BoPhan duAn = listCongTy.Find(x => x.Id == item.IdCongTyEOS);
                    if (duAn != null)
                        item.TenCongty = duAn.TenBoPhan;



                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic SetBangChamCong(HRM_BangChamCong item, ApplicationDbContext db = null)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    var QuyTrinhdb = db.HRM_BangChamCong.AsNoTracking().FirstOrDefault(a => a.IdGuid == item.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, item, err))
                        return err;

                    bool check = UpdateBangChamCong(item, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng");
                    }
                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, item, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng", item);
                }
                catch (Exception)
                {

                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh khÃ´ng cÃ´ng");
                }
            }
        }
        public dynamic GetBangChamCong(string Id, ApplicationDbContext db)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                HRM_BangChamCong QuyTrinh = db.HRM_BangChamCong.FirstOrDefault(a => a.IdGuid == Id);
                if (!base.GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
                QuyTrinh.DateTimeToUnix();
                QuyTrinh.listNhanVien = new List<HRM_BangChamCong_NhanVien>();
                QuyTrinh.listBoPhan = new List<HRM_BangChamCong_NhanVienViewModel>();
                QuyTrinh.listNhanVien = db.HRM_BangChamCong_NhanVien.Where(a => a.IdBangChamCong == Id).OrderBy(a => a.Created).ToList();
                var listNhanVien = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => a.IdBangChamCong == Id).ToList();
                if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                {
                    var tangCas = db.HRM_BangChamCongNhanVien_TangCa.Where(a => a.IdBangChamCong == QuyTrinh.IdGuid).ToList();
                    var SoGioLamViecs = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => a.IdBangChamCong == QuyTrinh.IdGuid).ToList();


                    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var NhanViens = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    List<long> IdNhanVienEOS = NhanViens.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    foreach (var itemAdd in QuyTrinh.listNhanVien)
                    {
                        var item = NhanViens.FirstOrDefault(a => a.IdGuid == itemAdd.IdNhanVien);
                        itemAdd.NgayVaoLam = item.NgayBatDauLamViec;
                        if (tangCas != null && tangCas.Count > 0)
                            itemAdd.listTangCa = tangCas.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).OrderBy(a => a.HeSo).ToList();
                        if (SoGioLamViecs != null && SoGioLamViecs.Count > 0)
                            itemAdd.listGioTieuChuan = SoGioLamViecs.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).OrderBy(a => a.GioTieuChuan).ToList();
                        itemAdd.listNgay = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                        DateTime time = QuyTrinh.TuNgay.Value;
                        var listNgay = listNhanVien.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).OrderBy(a => a.NgayChamCong).ToList();
                        while (time <= QuyTrinh.DenNgay)
                        {

                            var Ngay = listNgay.FirstOrDefault(a => a.NgayChamCong == time);
                            if (Ngay != null)
                            {
                                Ngay.DateTimeToUnix();
                                Ngay.isFake = false;
                                itemAdd.listNgay.Add(Ngay);
                            }
                            else
                            {
                                HRM_BangChamCong_NhanVien_ThoiGian Add = new HRM_BangChamCong_NhanVien_ThoiGian();
                                Add.isFake = true;
                                Add.NgayChamCong = time;
                                Add.Thang = time.Month;
                                Add.Ngay = time.Day;
                                Add.Nam = time.Year;
                                itemAdd.listNgay.Add(Add);
                            }
                            time = time.AddDays(1);
                        }
                        if (item != null)
                        {
                            itemAdd.TenNhanVien = item.TenDayDu;
                            itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;

                            itemAdd.MaNhanVien = item.MaNhanVien;

                            var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                            if (chucVu != null)
                                itemAdd.TenChucVu = chucVu.TenChucVu;
                            var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                            if (ChucDanh != null)
                                itemAdd.TenChucDanh = ChucDanh.Ten;

                            var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                            if (BoPhan != null)
                                itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                    }
                    if (QuyTrinh.listNhanVien.Count > 0)
                    {
                        List<string> listBP = QuyTrinh.listNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                        QuyTrinh.listBoPhan = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                        foreach (var item in QuyTrinh.listBoPhan)
                        {        //item.listNhanVien = QuyTrinh.listNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.NgayVaoLam).ToList();
                            if (Host == "ICT")
                                item.listNhanVien = QuyTrinh.listNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                            else
                            {
                                var listNhanVienbp = QuyTrinh.listNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                                listBP = listNhanVienbp.Select(a => a.TenChucDanh).Distinct().ToList();
                                item.lischucDanh = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                                foreach (var ChucDanh in item.lischucDanh)
                                {
                                    ChucDanh.listNhanVien = listNhanVienbp.Where(a => a.TenChucDanh == ChucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                                }
                            }
                        }
                    }
                }
                QuyTrinh.listNhanVien = QuyTrinh.listNhanVien.OrderBy(a => a.TenBoPhan).ThenBy(a => a.TenNhanVien).ToList();
                return QuyTrinh;
            }
            catch (Exception ex)
            {
                string ms = "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½" + ex.Message;
                return ms;
            }
        }
        public bool UpdateBangChamCong(HRM_BangChamCong model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {

            try
            {
                model.UnixToDateTime();
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    model.IdDuAn = model.IdCongTyEOS;
                    model.eAction = DefineData.eAction.QUYTRINHCHAMCONG.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return false;
                    db.HRM_BangChamCong.Add(model);

                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                db.SaveChanges();
                var listNhanVienOld = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => a.IdBangChamCong == model.IdGuid).ToList();
                if (listNhanVienOld != null && listNhanVienOld.Count > 0)
                    db.HRM_BangChamCong_NhanVien_ThoiGian.RemoveRange(listNhanVienOld);
                db.SaveChanges();
                var listNVOld = db.HRM_BangChamCong_NhanVien.Where(a => a.IdBangChamCong == model.IdGuid).ToList();
                if (listNVOld != null && listNVOld.Count > 0)
                    db.HRM_BangChamCong_NhanVien.RemoveRange(listNVOld);
                db.SaveChanges();
                var tangCaOld = db.HRM_BangChamCongNhanVien_TangCa.Where(a => a.IdBangChamCong == model.IdGuid).ToList();
                if (tangCaOld != null && tangCaOld.Count > 0)
                    db.HRM_BangChamCongNhanVien_TangCa.RemoveRange(tangCaOld);
                db.SaveChanges();
                var SoGioLamViecOld = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => a.IdBangChamCong == model.IdGuid).ToList();
                if (SoGioLamViecOld != null && SoGioLamViecOld.Count > 0)
                    db.HRM_BangChamCongNhanVien_SoGioLamViec.RemoveRange(SoGioLamViecOld);
                db.SaveChanges();
                if (model.listBoPhan != null && model.listBoPhan.Count > 0)
                    model.listNhanVien = model.listBoPhan.SelectMany(a => a.listNhanVien).ToList();
                if (model.listNhanVien == null || model.listNhanVien.Count() == 0)
                {
                    if (model.listBoPhan != null && model.listBoPhan.Count > 0)
                    {

                        if (Host == "ICT")
                            model.listNhanVien = model.listBoPhan.SelectMany(a => a.listNhanVien).ToList();
                        else
                            model.listNhanVien = model.listBoPhan.SelectMany(a => a.lischucDanh.SelectMany(b => b.listNhanVien)).ToList();

                    }

                }
                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {
                    List<HRM_BangChamCong_NhanVien_ThoiGian> listAdd = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                    List<HRM_BangChamCongNhanVien_TangCa> listtangCaAdd = new List<HRM_BangChamCongNhanVien_TangCa>();
                    List<HRM_BangChamCongNhanVien_SoGioLamViec> listGioAdd = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                    foreach (var item in model.listNhanVien)
                    {
                        if (item.listNgay != null && item.listNgay.Count > 0)
                        {
                            foreach (var Ngay in item.listNgay.Where(a => a.isFake == false))
                            {
                                Ngay.UnixToDateTime();
                                Ngay.IdGuid = Guid.NewGuid().ToString();
                                Ngay.IdBangChamCong = model.IdGuid;
                                Ngay.IdNhanVien = item.IdNhanVien;
                                Ngay.isKetThuc = model.isKetThuc;
                                Ngay.IdCongTyEOS = model.IdCongTyEOS;
                                listAdd.Add(Ngay);
                            }

                        }
                        if (item.listGioTieuChuan != null && item.listGioTieuChuan.Count > 0)
                        {
                            foreach (var Ngay in item.listGioTieuChuan)
                            {
                                Ngay.IdGuid = Guid.NewGuid().ToString();
                                Ngay.IdBangChamCong = model.IdGuid;
                                Ngay.IdNhanVien = item.IdNhanVien;
                                Ngay.TuNgay = model.TuNgay ?? DateTime.Now;
                                Ngay.DenNgay = model.DenNgay ?? DateTime.Now;
                                Ngay.isKetThuc = model.isKetThuc;
                                listGioAdd.Add(Ngay);
                            }

                        }
                        if (item.listTangCa != null && item.listTangCa.Count > 0)
                        {
                            foreach (var Ngay in item.listTangCa)
                            {
                                Ngay.IdGuid = Guid.NewGuid().ToString();
                                Ngay.IdBangChamCong = model.IdGuid;
                                Ngay.IdNhanVien = item.IdNhanVien;
                                Ngay.TuNgay = model.TuNgay ?? DateTime.Now;
                                Ngay.DenNgay = model.DenNgay ?? DateTime.Now;
                                Ngay.isKetThuc = model.isKetThuc;
                                listtangCaAdd.Add(Ngay);
                            }

                        }

                        //item.IdNhanVien = item.listNgay.FirstOrDefault().IdNhanVien;
                        item.TuNgay = model.TuNgay ?? DateTime.Now;
                        item.DenNgay = model.DenNgay ?? DateTime.Now;
                        item.IdCongTyEOS = model.IdCongTyEOS;
                        item.isKetThuc = model.isKetThuc;
                        item.IdBangChamCong = model.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                    }
                    db.HRM_BangChamCong_NhanVien.AddRange(model.listNhanVien);
                    db.SaveChanges();
                    if (listAdd != null && listAdd.Count > 0)
                        db.HRM_BangChamCong_NhanVien_ThoiGian.AddRange(listAdd);
                    db.SaveChanges();
                    if (listGioAdd != null && listGioAdd.Count > 0)
                        db.HRM_BangChamCongNhanVien_SoGioLamViec.AddRange(listGioAdd);
                    db.SaveChanges();
                    if (listtangCaAdd != null && listtangCaAdd.Count > 0)
                        db.HRM_BangChamCongNhanVien_TangCa.AddRange(listtangCaAdd);
                }
                db.SaveChanges();
                if (!base.SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;

                return true;


            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }


        }
        public dynamic ChuyenTiepBangChamCong(HRM_BangChamCong item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    HRM_BangChamCong pcuDatabase = db.HRM_BangChamCong.AsNoTracking().Where(x => x.IdGuid == item.IdGuid).FirstOrDefault();
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, item, err, item.IdCongTyEOS))
                        return err;
                    bool Error = UpdateBangChamCong(item, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng");
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, item, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic KhongDuyetBangChamCong(HRM_BangChamCong item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    HRM_BangChamCong pcuDatabase = db.HRM_BangChamCong.AsNoTracking().Where(x => x.IdGuid == item.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, item, err))
                        return err;

                    bool Error = UpdateBangChamCong(item, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng");
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, item))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic DeleteBangChamCong(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_BangChamCong chamCong = db.HRM_BangChamCong.Find(Id);
                    var listNhanVienOld = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => a.IdBangChamCong == Id).ToList();
                    if (listNhanVienOld != null && listNhanVienOld.Count > 0)
                        db.HRM_BangChamCong_NhanVien_ThoiGian.RemoveRange(listNhanVienOld);
                    var tangCaOld = db.HRM_BangChamCongNhanVien_TangCa.Where(a => a.IdBangChamCong == Id).ToList();
                    if (tangCaOld != null && tangCaOld.Count > 0)
                        db.HRM_BangChamCongNhanVien_TangCa.RemoveRange(tangCaOld);
                    var SoGioLamViecOld = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => a.IdBangChamCong == Id).ToList();
                    if (SoGioLamViecOld != null && SoGioLamViecOld.Count > 0)
                        db.HRM_BangChamCongNhanVien_SoGioLamViec.RemoveRange(SoGioLamViecOld);

                    db.HRM_BangChamCong.Remove(chamCong);
                    db.SaveChanges();
                    _CallWebService.AddModify(db, DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHCHAMCONG.ToString()), DefineData.eModify.Xoa.ToString(), nameof(HRM_BangChamCong), chamCong.NoiDung, Id, "", Id, 0, db.currentUser.Id, db.currentUser.TenNhanVien);

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "XÃ³a báº£ng cháº¥m cÃ´ng thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½ " + ex.Message);
                }
            }
        }
        public dynamic ImportBangChamCong(string FileName, double TuNgay, double DenNgay, ApplicationDbContext db = null, long IdCongTy = 0)
        {
            try
            {
                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgay);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgay);
                string pathTemplate = PathUploads + FileName;
                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);

                var sheet1 = package.Workbook.Worksheets.First();
                int row = 4;
                int maxrow = sheet1.Dimension.Rows;
                int maxCol = sheet1.Dimension.Columns;
                int colVao = 7;
                List<string> listMaNhanVien = new List<string>();
                DateTime dtBegin = DateTime.Now.AddYears(10);
                DateTime dtEnd = DateTime.Now.AddYears(-10);
                for (int i = row; i <= sheet1.Cells.End.Row; i++)
                {
                    DateTime? ngayCong = dungchung.GetCellToDateTime(sheet1, i, 1);
                    string sMaNhanVien = GetCell(sheet1, i, 3);
                    if (sMaNhanVien.IsEmpty() || ngayCong == null)
                        break;
                    listMaNhanVien.Add(sMaNhanVien);
                    if (dtBegin > ngayCong)
                        dtBegin = ngayCong.Value;
                    if (dtEnd < ngayCong)
                        dtEnd = ngayCong.Value;
                }
                listMaNhanVien = listMaNhanVien.Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listMaNhanVien.Contains(x.MaNhanVien)).ToList();
                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_CheckInOut> listCheckInOutCheck = (from a in db.HRM_CheckInOut
                                                            where listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay >= dTuNgay && a.Ngay <= dDenNgay
                                                            && (IdCongTy == 0 || a.IdCongTy == IdCongTy)
                                                            select a
                                                               ).ToList();
                List<HRM_CheckInOut> listCheckInOuts = new List<HRM_CheckInOut>();
                List<HRM_CheckInOut> listCheckInOutDelete = new List<HRM_CheckInOut>();
                for (int i = row; i <= sheet1.Cells.End.Row; i++)
                {

                    DateTime? ngayCong = dungchung.GetCellToDateTime(sheet1, i, 1);
                    string sMaNhanVien = dungchung.GetCell(sheet1, i, 3);
                    if (sMaNhanVien.IsEmpty() || ngayCong == null)
                        break;
                    HRM_NhanVien NhanVienFind = listNhanVien.Find(x => x.MaNhanVien == sMaNhanVien);
                    if (NhanVienFind == null)
                        continue;

                    ngayCong = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);
                    string Loai = "I";
                    for (var nCol = colVao; nCol <= maxCol; nCol++)
                    {
                        HRM_CheckInOut CheckInOut = new HRM_CheckInOut();
                        CheckInOut.Id = Guid.NewGuid().ToString();
                        CheckInOut.IdNhanVien = NhanVienFind.IdGuid;
                        CheckInOut.IdBoPhan = NhanVienFind.IdBoPhanEOS;
                        CheckInOut.IdCongTy = NhanVienFind.IdCongTyEOS;
                        CheckInOut.Loai = Loai;
                        CheckInOut.Ngay = ngayCong ?? dtBegin;
                        string sColText = GetCell(sheet1, i, nCol);
                        if (string.IsNullOrEmpty(sColText))
                            break;
                        try
                        {
                            double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                            int gioVao = 0, phutVao = 0;
                            dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                            CheckInOut.Gio = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, gioVao, phutVao, 0);
                            if (Loai == "O")
                            {
                                DateTime time = ngayCong.Value.AddDays(1);
                                CheckInOut.Gio = new DateTime(time.Year, time.Month, time.Day, gioVao, phutVao, 0);

                            }
                        }
                        catch
                        {
                            int a = i;
                        }
                        if (Loai == "I")
                            Loai = "O";
                        else
                            Loai = "I";
                        listCheckInOuts.Add(CheckInOut);

                    }
                    if (listCheckInOutCheck != null && listCheckInOutCheck.Count > 0)
                    {
                        var delete = listCheckInOutCheck.Where(a => a.IdNhanVien == NhanVienFind.IdGuid && a.Ngay == ngayCong).ToList();
                        if (delete != null)
                            listCheckInOutDelete.AddRange(delete);
                    }

                }
                if (listCheckInOuts.Count > 0)
                {
                    db.HRM_CheckInOut.AddRange(listCheckInOuts);
                    if (listCheckInOutDelete != null && listCheckInOutDelete.Count > 0)
                    {
                        listCheckInOutDelete = listCheckInOutDelete.Distinct().ToList();
                        db.HRM_CheckInOut.RemoveRange(listCheckInOutDelete);
                    }
                    db.SaveChanges();

                }

                s.Close();
                List<HRM_BangChamCong_NhanVienViewModel> listBoPhan = new List<HRM_BangChamCong_NhanVienViewModel>();
                List<HRM_BangChamCong_NhanVien> listNhanViencc = new List<HRM_BangChamCong_NhanVien>();
                if (Host == "ICT")
                    listNhanViencc = GetChamCongFromMayChamCongICT(IdCongTy, TuNgay, DenNgay, db, false);
                else
                    listNhanViencc = GetChamCongFromMayChamCongPHT(IdCongTy, TuNgay, DenNgay, db);
                if (listNhanViencc.Count > 0)
                {
                    List<string> listBP = listNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                    listBoPhan = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                    foreach (var item in listBoPhan)
                        item.listNhanVien = listNhanViencc.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.NgayVaoLam).ToList();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", listBoPhan);
                }
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listNhanViencc);
            }

            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + ex.ToString());
            }

        }
        public dynamic ImportBangChamCongICT(string FileName, double TuNgay, double DenNgay, ApplicationDbContext db = null, long IdCongTy = 0)
        {
            try
            {
                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgay);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgay);
                string pathTemplate = PathUploads + FileName;
                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);
                var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongTy, TuNgay, DenNgay, db);
                List<string> listIdUser = new List<string>();
                if (listUserDuAn != null && listUserDuAn.Count > 0)
                    listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();
                var sheet1 = package.Workbook.Worksheets.First();
                int row = 3;
                int maxrow = sheet1.Dimension.Rows;
                int maxCol = sheet1.Dimension.Columns;
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listIdUser.Contains(x.IdUser)).ToList();

                List<string> listMaCheck = new List<string>();
                List<HRM_CheckInOut> listCheckInOuts = new List<HRM_CheckInOut>();
                List<HRM_CheckInOut> listCheckInOutDelete = new List<HRM_CheckInOut>();
                for (int i = row; i <= sheet1.Cells.End.Row; i++)
                {

                    DateTime? ngayCong = dungchung.GetCellToDateTime(sheet1, i, 2);

                    string sMaNhanVien = dungchung.GetCell(sheet1, i, 3);
                    if (sMaNhanVien.IsEmpty() || ngayCong == null)
                        break;
                    if (ngayCong < dTuNgay || ngayCong > dDenNgay)
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "NgÃ y nháº­p khÃ´ng thuá»™c khoáº£ng thá»i gian chá»n dÃ²ng: " + i);
                    }
                    HRM_NhanVien NhanVienFind = listNhanVien.Find(x => x.SoGen == sMaNhanVien || x.MaNhanVien == sMaNhanVien);
                    if (NhanVienFind == null)
                    {
                        listMaCheck.Add(sMaNhanVien);
                        continue;
                    }
                    DateTime? TuGio = null;
                    try
                    {
                        DateTime? Gio = dungchung.GetCellToDateTime(sheet1, i, 5);
                        if (Gio != null)
                        {
                            HRM_CheckInOut CheckInOut = new HRM_CheckInOut();
                            CheckInOut.Id = Guid.NewGuid().ToString();
                            CheckInOut.IdNhanVien = NhanVienFind.IdGuid;
                            CheckInOut.IdBoPhan = NhanVienFind.IdBoPhanEOS;
                            CheckInOut.IdCongTy = IdCongTy == 0 ? NhanVienFind.IdCongTyEOS : IdCongTy;
                            CheckInOut.Ngay = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);
                            CheckInOut.Loai = DefineData.eKyHieuChamCong.I.ToString();
                            CheckInOut.Gio = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                            TuGio = CheckInOut.Gio;
                            listCheckInOuts.Add(CheckInOut);
                        }
                    }
                    catch
                    {
                        HRM_CheckInOut CheckInOut = new HRM_CheckInOut();
                        CheckInOut.Id = Guid.NewGuid().ToString();
                        CheckInOut.IdNhanVien = NhanVienFind.IdGuid;
                        CheckInOut.IdBoPhan = NhanVienFind.IdBoPhanEOS;
                        CheckInOut.IdCongTy = IdCongTy == 0 ? NhanVienFind.IdCongTyEOS : IdCongTy;
                        CheckInOut.Ngay = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);
                        CheckInOut.Loai = DefineData.eKyHieuChamCong.I.ToString();
                        string sColText = GetCell(sheet1, i, 5);
                        if (string.IsNullOrEmpty(sColText))
                            break;
                        listCheckInOuts.Add(CheckInOut);
                        double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                        int gioVao = 0, phutVao = 0;
                        dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                        CheckInOut.Gio = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, gioVao, phutVao, 0);

                        listCheckInOuts.Add(CheckInOut);
                    }
                    try
                    {
                        DateTime? Gio = dungchung.GetCellToDateTime(sheet1, i, 6);
                        if (Gio != null)
                        {
                            HRM_CheckInOut CheckInOut = new HRM_CheckInOut();
                            CheckInOut.Id = Guid.NewGuid().ToString();
                            CheckInOut.IdNhanVien = NhanVienFind.IdGuid;
                            CheckInOut.IdBoPhan = NhanVienFind.IdBoPhanEOS;
                            CheckInOut.IdCongTy = IdCongTy == 0 ? NhanVienFind.IdCongTyEOS : IdCongTy;
                            CheckInOut.Ngay = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);
                            CheckInOut.Loai = DefineData.eKyHieuChamCong.O.ToString();
                            CheckInOut.Gio = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, Gio.Value.Hour, Gio.Value.Minute, 0);
                            if (TuGio != null && TuGio > CheckInOut.Gio)
                                CheckInOut.Gio = CheckInOut.Gio.AddDays(1);
                            listCheckInOuts.Add(CheckInOut);
                        }
                    }
                    catch
                    {
                        HRM_CheckInOut CheckInOut = new HRM_CheckInOut();
                        CheckInOut.Id = Guid.NewGuid().ToString();
                        CheckInOut.IdNhanVien = NhanVienFind.IdGuid;
                        CheckInOut.IdBoPhan = NhanVienFind.IdBoPhanEOS;
                        CheckInOut.IdCongTy = IdCongTy == 0 ? NhanVienFind.IdCongTyEOS : IdCongTy;
                        CheckInOut.Ngay = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, 0, 0, 0);
                        CheckInOut.Loai = DefineData.eKyHieuChamCong.O.ToString();
                        string sColText = GetCell(sheet1, i, 6);
                        if (string.IsNullOrEmpty(sColText))
                            break;
                        listCheckInOuts.Add(CheckInOut);
                        double dGioVaoDau = dungchung.ConvertGioPhutSangGio(sColText);
                        int gioVao = 0, phutVao = 0;
                        dungchung.ConvertGioPhut(sColText, ref gioVao, ref phutVao);
                        CheckInOut.Gio = new DateTime(ngayCong.Value.Year, ngayCong.Value.Month, ngayCong.Value.Day, gioVao, phutVao, 0);
                        if (TuGio != null && TuGio > CheckInOut.Gio)
                            CheckInOut.Gio = CheckInOut.Gio.AddDays(1);
                        listCheckInOuts.Add(CheckInOut);
                    }
                }
                listCheckInOuts = listCheckInOuts.OrderBy(a => a.Gio).ToList();
                //foreach (var item in listCheckInOuts)
                //{
                //    if (item.Loai == DefineData.eKyHieuChamCong.I.ToString())
                //        continue;
                //    var In = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.I.ToString() && a.Gio <= item.Gio && a.IdNhanVien == item.IdNhanVien).OrderByDescending(a => a.Gio).FirstOrDefault();
                //    item.Ngay = In.Ngay;
                //}
                if (listMaCheck != null && listMaCheck.Count > 0)
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "NhÃ¢n viÃªn khÃ´ng thuá»™c dá»± Ã¡n: " + string.Join(", ", listMaCheck.Distinct().ToList()));
                List<string> listIdNhanVien = listCheckInOuts.Select(x => x.IdNhanVien).ToList();
                List<HRM_CheckInOut> listCheckInOutCheck = (from a in db.HRM_CheckInOut
                                                            where listIdNhanVien.Contains(a.IdNhanVien) && a.Ngay >= dTuNgay && a.Ngay <= dDenNgay
                                                            && (IdCongTy == 0 || a.IdCongTy == IdCongTy)
                                                            select a
                                                               ).ToList();
                if (listCheckInOutCheck != null && listCheckInOutCheck.Count > 0)
                {
                    db.HRM_CheckInOut.RemoveRange(listCheckInOutCheck);
                }
                if (listCheckInOuts.Count > 0)
                {
                    db.HRM_CheckInOut.AddRange(listCheckInOuts);

                }
                db.SaveChanges();
                s.Close();
                _CallWebService.AddModify(db, DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHCHAMCONG.ToString()), DefineData.eModify.Import.ToString(), nameof(HRM_BangChamCong), DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHCHAMCONG.ToString()), "", "", FileName, 0, db.currentUser.Id, db.currentUser.TenNhanVien);

                List<HRM_BangChamCong_NhanVienViewModel> listBoPhan = new List<HRM_BangChamCong_NhanVienViewModel>();
                List<HRM_BangChamCong_NhanVien> listNhanViencc = new List<HRM_BangChamCong_NhanVien>();
                if (Host == "ICT")
                    listNhanViencc = GetChamCongFromMayChamCongICT(IdCongTy, TuNgay, DenNgay, db, false);
                else
                    listNhanViencc = GetChamCongFromMayChamCongPHT(IdCongTy, TuNgay, DenNgay, db);
                if (listNhanViencc.Count > 0)
                {
                    List<string> listBP = listNhanViencc.Select(a => a.TenBoPhan).Distinct().ToList();
                    listBoPhan = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                    foreach (var item in listBoPhan)
                        item.listNhanVien = listNhanViencc.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.NgayVaoLam).ToList();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listBoPhan);
                }
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listNhanViencc);
                //return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listResult.OrderBy(a => a.isDieuDong).ToList());
            }

            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + ex.ToString());
            }

        }
        public dynamic GetChamCongFromMayChamCong(long IdCongTy, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db, bool isWeb = false, bool isAll = false)
        {

            List<HRM_BangChamCong_NhanVienViewModel> listBoPhan = new List<HRM_BangChamCong_NhanVienViewModel>();
            List<HRM_BangChamCong_NhanVien> listNhanVien = new List<HRM_BangChamCong_NhanVien>();
            if (Host == "ICT")
                listNhanVien = GetChamCongFromMayChamCongICT(IdCongTy, TuNgayUnix, DenNgayUnix, db, isAll, isWeb);
            else
                listNhanVien = GetChamCongFromMayChamCongPHT(IdCongTy, TuNgayUnix, DenNgayUnix, db);
            if (listNhanVien.Count > 0 && isWeb == true)
            {
                List<string> listBP = listNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                listBoPhan = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                foreach (var item in listBoPhan)
                {
                    if (Host == "ICT")
                        item.listNhanVien = listNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                    else
                    {
                        var listNhanVienbp = listNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                        listBP = listNhanVienbp.Select(a => a.TenChucVu).Distinct().ToList();
                        item.lischucDanh = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                        foreach (var ChucDanh in item.lischucDanh)
                        {
                            ChucDanh.listNhanVien = listNhanVienbp.Where(a => a.TenChucVu == ChucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList();
                        }
                    }


                }
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", listBoPhan);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", listNhanVien);
        }
        public List<HRM_BangChamCong_NhanVien> GetChamCongFromMayChamCongICT(long IdCongTy, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db, bool isAll = false, bool isWeb = false, List<string> listIdUser = null)
        {
            List<HRM_BangChamCong_NhanVien> listResult = new List<HRM_BangChamCong_NhanVien>();
            try
            {
                string Host = HrmOptions.Host;
                var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                var ChucVuGS = ChucVus.Where(a => a.MaChucVu == "GS").FirstOrDefault();
                var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongTy, TuNgayUnix, DenNgayUnix, db);
                //List<string> listIdUser = new List<string>();
                if (listUserDuAn != null && listUserDuAn.Count > 0 && listIdUser == null)
                    listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();
                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
                if (ChucVuGS == null)
                    ChucVuGS = new ChucVu();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                if (Host == "ICT")
                    listNhanVien = db.HRM_NhanVien.AsNoTracking().Where(x => ((isAll == true && x.IdChucVu != ChucVuGS.Id) || isAll == false) && (!string.IsNullOrEmpty(x.IdUser) && listIdUser.Contains(x.IdUser))).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.AsNoTracking().Where(x => listIdUser.Contains(x.IdUser)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut.AsNoTracking()
                                                       where listIdNhanVien.Contains(x.IdNhanVien)/* && x.Loai == DefineData.eKyHieuChamCong.I.ToString()*/
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                       select x
                                                       //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                       //select g.OrderBy(a => a.Gio).ToList()/*.FirstOrDefault()*/
                                                       ).ToList();
                //listCheckInOut.AddRange((from x in db.HRM_CheckInOut.AsNoTracking()
                //                         where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                //                                       && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                //                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                //                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.AsNoTracking().ToList();
                //List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                //                                                                      && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();


                var listLoaiNghi = _CallWebService.GetDanhMucLoaiNghiPhep(db);
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    foreach (var item in listXinNghiPhep)
                    {
                        var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (hinhThucNghi != null)
                            item.HinhThucNghi = hinhThucNghi.Ma;

                        var MaLoaiNghi = listLoaiNghi.FirstOrDefault(a => a.Id == item.IdLoaiNghiPhep);
                        if (MaLoaiNghi != null)
                            item.MaLoaiNghi = MaLoaiNghi.Ma;

                    }
                }
                var listCaLamViecs = db.HRM_dmLichLamViec.AsNoTracking().ToList();



                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                var funcKeHoach = _funcKeHoach;
                List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
                List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
                var DuAn = _CallWebService.GetQuyTrinhDuAn(IdCongTy, db);
                if (DuAn == null)
                    DuAn = new BoPhan();
                DateTime dateNow = DateTime.Now;
                dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);
                if (DuAn.MaBoPhan == "VPCT")
                {
                    SoPhutDuocDiMuon = 0;
                    SoPhutDuocVeSom = 0;
                }
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "0156")
                        item.MaNhanVien = "0156";
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    var ChucVu = ChucVus.FirstOrDefault(a => a.Id == item.IdChucVu);
                    var NhanSuDA = listUserDuAn.FirstOrDefault(a => a.IdUser == item.IdUser);
                    DateTime dt = dTuNgay;
                    if (listHeSo != null && listHeSo.Count > 0)
                    {
                        foreach (var heso in listHeSo)
                        {
                            HRM_BangChamCongNhanVien_TangCa hesoAdd = new HRM_BangChamCongNhanVien_TangCa();
                            hesoAdd.HeSo = heso;
                            hesoAdd.IdNhanVien = item.IdGuid;
                            var tangca = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.HeSoTangCa == heso && a.Loai == 1).ToList();
                            if (tangca != null && tangca.Count > 0)
                            {
                                //hesoAdd.TongSoGio = tangca.Sum(a => (a.GioRa - a.GioVao).TotalHours);
                                //hesoAdd.Cong = tangca.Sum(a => ((a.GioRa - a.GioVao).TotalHours) / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                                hesoAdd.IddmHeSoTangCa = tangca.FirstOrDefault().IddmHeSoTangCa;
                            }
                            listDataTangCa.Add(hesoAdd);
                        }
                    }
                    if (listCaTieuChuan != null && listCaTieuChuan.Count > 0)
                    {
                        foreach (var Id in listCaTieuChuan)
                        {
                            HRM_BangChamCongNhanVien_SoGioLamViec hesoAdd = new HRM_BangChamCongNhanVien_SoGioLamViec();
                            var caLamViec = new HRM_dmLichLamViec();
                            try
                            {
                                string idCalamViec = listLichLamViec.FirstOrDefault(a => a.GioTieuChuan == Id && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec)).IddmCaLamViec;
                                caLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == idCalamViec);

                            }
                            catch
                            {
                                caLamViec = listCaLamViecs.FirstOrDefault(a => a.SoGioTrongNgay == Id);
                            }
                            if (caLamViec != null)
                            {
                                hesoAdd.IddmCaLamViec = caLamViec.IdGuid;
                                hesoAdd.IdNhanVien = item.IdGuid;
                                hesoAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                                hesoAdd.CongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                                hesoAdd.TongSoGio = 0;
                                listDataTieuChuan.Add(hesoAdd);
                            }
                        }
                    }

                    while (dt <= dDenNgay)
                    {
                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.Where(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt).OrderBy(a => a.GioVao).ToList();
                        DateTime? Check = null;
                        if (listLichLamViecFind != null)
                            Check = listLichLamViecFind.OrderByDescending(a => a.Ngay).Select(a => a.GioRa).FirstOrDefault();
                        if (Check != null)
                            Check = Check.Value.Date;
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt && x.IdCongTy == IdCongTy);
                        List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && (x.Ngay == dt || (Check != null && x.Ngay == Check))).OrderBy(a => a.Gio).ToList();
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);
                        if (dt >= dateNow)
                        {
                            dt = dDenNgay.AddDays(1);
                            continue;
                        }
                        if (listCheckInOutFind != null && listCheckInOutFind.Count > 1)
                        {
                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                            string IdCaLamViec = listLichLamViecFind.Where(a => a.IdCongTy == IdCongTy).Select(a => a.IddmCaLamViec).FirstOrDefault();
                            var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);

                            List<clsDiMuonVeSom> listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, DuAn.MaBoPhan, NghiPheps, false, IdCongTy, CaLamViec);
                            //List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                            if (listLichLamViecFind.Count >= 1)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                bool TangCaDem = false;
                                foreach (var KeHoach in listLichLamViecFind)
                                {

                                    if (KeHoach.IdCongTy != IdCongTy && (isAll == true || isWeb == true))
                                        continue;

                                    double SoGioLamViec = KeHoach.TongSoGioGiao - (KeHoach.SoGioNghi ?? 0);
                                    if ((itemAdd.SoGioTieuChuan == null || itemAdd.SoGioTieuChuan == 0) && KeHoach.GioTieuChuan != null)
                                        itemAdd.SoGioTieuChuan = KeHoach.GioTieuChuan;
                                    if (KeHoach.Loai == 0)
                                    {
                                        //if (KeHoach.isCaGay == true)
                                        //    itemAdd.SoGioLamViecCaGay += SoGioLamViec;
                                        //else


                                        itemAdd.SoGioLamViec += SoGioLamViec;
                                        if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == KeHoach.GioTieuChuan);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;

                                        }
                                    }
                                    else
                                    {
                                        itemAdd.IddmHeSoTangCa = KeHoach.IddmHeSoTangCa;

                                        itemAdd.SoGioLamViecTangCa += SoGioLamViec;
                                        if (KeHoach.GioVao.Hour >= 21)
                                        {
                                            TangCaDem = true;

                                        }
                                        if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                        {
                                            var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == KeHoach.IddmHeSoTangCa);
                                            if (tangCa != null)
                                                tangCa.TongSoGio += SoGioLamViec;
                                        }
                                    }

                                }
                                List<string> listKyHieu = new List<string>();
                                if (listSomMuon != null && listSomMuon.Count > 0)
                                    foreach (var SomMuon in listSomMuon)
                                    {
                                        //itemAdd.GioLamViec += "; ";
                                        double Gio = SomMuon.SoPhut / 60.0;
                                        if (SomMuon.Loai == 0)
                                        {
                                            //if (SomMuon.isCaGay == true)
                                            //    itemAdd.SoGioLamViecCaGay -= Gio;
                                            //else
                                            itemAdd.SoGioLamViec -= Gio;
                                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0)
                                            {
                                                var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == itemAdd.SoGioTieuChuan);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio -= Gio;
                                                if (tangCa != null && tangCa.TongSoGio < 0)
                                                    tangCa.TongSoGio = 0;
                                            }
                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa -= Gio;
                                            if (listDataTangCa != null && listDataTangCa.Count > 0)
                                            {
                                                var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == itemAdd.IddmHeSoTangCa);
                                                if (tangCa != null)
                                                {
                                                    tangCa.TongSoGio -= Gio;
                                                    if (tangCa.TongSoGio < 0.1)
                                                        tangCa.TongSoGio = 0;
                                                }
                                            }
                                        }
                                        listKyHieu.Add(SomMuon.KyHieuChamCong);
                                    }
                                if (CaLamViec != null && CaLamViec.LoaiNgayNghi == 4 && (int)dt.DayOfWeek == 6 && itemAdd.SoGioLamViec > 4)// Ä‘á»‘i vá»›i lÃ m thá»© 7 chá»‰ tÃ­nh 4 tiáº¿ng cÃ²n láº¡i lÃ  tÄƒng ca
                                {

                                    var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == itemAdd.SoGioTieuChuan);
                                    if (tangCa != null)
                                        tangCa.TongSoGio = tangCa.TongSoGio - (itemAdd.SoGioLamViec ?? 4) + 4;
                                    itemAdd.SoGioLamViec = 4;
                                }
                                if (itemAdd.SoGioLamViec < 0)
                                    itemAdd.SoGioLamViec = 0;
                                double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                string KyHieu = "";
                                if (listKyHieu != null && listKyHieu.Count > 0)
                                    KyHieu += string.Join("", listKyHieu) + ";";
                                if (GioLamViec > 0)
                                    KyHieu += Math.Round(GioLamViec, 2);
                                if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                {
                                    itemAdd.isCaDem = true;
                                }
                                if (itemAdd.SoGioLamViecTangCa > 0)
                                {
                                    if (TangCaDem == false)
                                    {
                                        KyHieu += "(" + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + ")";
                                    }
                                    else
                                    {
                                        if (listLichLamViecFind.Count > 1)
                                            KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                        else if (listLichLamViecFind.Count == 1)
                                        {
                                            if (GioLamViec > 0)
                                                KyHieu += "(" + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä)";
                                            else
                                                KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                        }
                                    }
                                }
                                itemAdd.GioLamViec = KyHieu;
                                if (NhanSuDA.isDieuDong == false)
                                {
                                    var checkIn = listCheckInOutFind.FirstOrDefault(a => a.IdCongTy != IdCongTy);
                                    if (checkIn != null)
                                        itemAdd.GioLamViec = DefineData.eKyHieuChamCong.XTC.ToString();
                                }
                                listData.Add(itemAdd);
                            }
                        }
                        else if (NghiPheps.Count > 0)
                        {
                            //if (listLichLamViecFind == null)
                            //    listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.FirstOrDefault(x => x.Ma == NghiPheps.FirstOrDefault().HinhThucNghi);
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                //double SoGioLamViec = 0;
                                //foreach (var KeHoach in listLichLamViecFind)
                                //{
                                //    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                //}

                                itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                double SoGioNghi = NghiPheps.Sum(a => a.ThoiGianNghi);
                                if (SoGioNghi == 0.5)
                                    itemAdd.GioLamViec += "/2;NTD";

                                listData.Add(itemAdd);
                            }
                        }
                        else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.FirstOrDefault(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                }
                                itemAdd.GioLamViec = NghiPhep.Ma;
                                listData.Add(itemAdd);
                            }
                        }
                        dt = dt.AddDays(1);
                    }

                }
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                var listTangCa = db.HRM_TongHopXacNhanNgoaiGio.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TuNgay <= dTuNgay && a.DenNgay <= dDenNgay).OrderByDescending(a => a.Modified).ToList();

                if (listData != null && listData.Count > 0)
                {
                    var LoaiNhanVien = db.HRM_dmLoaiNhanVien.FirstOrDefault(a => a.Ma == "BT");


                    List<long> IdNhanVienEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => ((a.TuNgay.Month <= dTuNgay.Month) || a.DenNgay.Month >= dTuNgay.Month || a.TuNgay.Month <= dDenNgay.Month || a.DenNgay.Month >= dDenNgay.Month)).ToList();
                    var listIdHinhThucNghi = listHinhThucNghi.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).Select(a => a.Id).ToList();
                    foreach (var item in listNhanVien)
                    {
                        if (item.MaNhanVien == "0061")
                            item.MaNhanVien = "0061";
                        {
                            var check = listData.Where(a => a.IdNhanVien == item.IdGuid).FirstOrDefault();
                            if (check == null)
                                continue;
                        }
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        HRM_BangChamCong_NhanVien itemAdd = new HRM_BangChamCong_NhanVien();
                        itemAdd.NgayVaoLam = item.NgayBatDauLamViec;
                        var user = listUserDuAn.FirstOrDefault(a => a.IdUser == item.IdUser);
                        itemAdd.TongSoCong200 = 0;
                        itemAdd.TongSoCong = 0;
                        itemAdd.isDieuDong = user.isDieuDong;
                        itemAdd.LinkAnhDaiDien = user.LinkAnhDaiDien;
                        //itemAdd.isTruyThu = !itemAdd.isDieuDong;
                        itemAdd.TongSoCong150 = 0;
                        //itemAdd.TongSoCong300 = 0;
                        var CongPhep = listXinNghiPhep.Where(x => x.IdNhanVien == item.IdGuid && listIdHinhThucNghi.Contains(x.IdHinhThucNghi)).ToList();
                        if (CongPhep != null && CongPhep.Count > 0)
                            itemAdd.TongCongPhep = CongPhep.Sum(a => a.ThoiGianNghi);
                        itemAdd.listNgay = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;

                        }
                        var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (chucVu != null)
                            itemAdd.TenChucVu = chucVu.TenChucVu;

                        DateTime time = dTuNgay;
                        var listNgay = listData.Where(a => a.IdNhanVien == item.IdGuid && a.IdCongTyEOS == IdCongTy).OrderBy(a => a.NgayChamCong).ToList();
                        double? CongLe = 0;
                        double? CongLeKDL = 0;

                        var listTangCaHT = listTangCa.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        if (listTangCaHT != null && listTangCaHT.Count > 0)
                        {
                            foreach (var TC in listTangCaHT)
                            {
                                if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                                {
                                    var TangCa = itemAdd.listTangCa.FirstOrDefault(a => a.HeSo == TC.HeSo);
                                    if (TangCa != null)
                                    {
                                        TangCa.TongSoGio += TC.SoGio;
                                        TangCa.Cong += (TC.SoGio / TC.GioTieuChuan);
                                    }
                                    else
                                        itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                        {
                                            IdNhanVien = item.IdGuid,
                                            TongSoGio = TC.SoGio,
                                            Cong = (TC.SoGio / TC.GioTieuChuan),
                                            CongTieuChuan = TC.CongTieuChuan,
                                            HeSo = TC.HeSo
                                        });
                                }
                                else
                                {
                                    itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                    itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                    {
                                        IdNhanVien = item.IdGuid,
                                        TongSoGio = TC.SoGio,
                                        Cong = (TC.SoGio / TC.GioTieuChuan),
                                        CongTieuChuan = TC.CongTieuChuan,
                                        HeSo = TC.HeSo
                                    });

                                }
                                if (itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)
                                {
                                    var GTC = itemAdd.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == TC.GioTieuChuan);
                                    if (GTC != null)
                                    {
                                        GTC.TongSoGio -= TC.SoGio;

                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((DuAn.CachTinhNgoaiGio == "TheoNgay") || (DuAn.CachTinhNgoaiGio == "TheoHopDongLaoDong" && HopDong.LoaiLuong == 1))
                            {
                                if (listDataTangCa != null && listDataTangCa.Count > 0)
                                    itemAdd.listTangCa = listDataTangCa.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.HeSo).ToList();
                                if (listDataTieuChuan != null && listDataTieuChuan.Count > 0)
                                    itemAdd.listGioTieuChuan = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.GioTieuChuan).ToList();
                            }
                            else
                            {

                                {
                                    if (listDataTangCa != null && listDataTangCa.Count > 0)
                                        itemAdd.listTangCa = listDataTangCa.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.HeSo).ToList();
                                    if (listDataTieuChuan != null && listDataTieuChuan.Count > 0)
                                        itemAdd.listGioTieuChuan = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.GioTieuChuan).ToList();
                                }
                                if (chucVu != null && (chucVu.MaChucVu == "CN" || chucVu.MaChucVu == "GS") && HopDong != null && HopDong.LoaiLuong == 1 && itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)

                                //if ()
                                {
                                    double tongSoGio = itemAdd.listGioTieuChuan.Sum(a => a.TongSoGio);
                                    var Gio = itemAdd.listGioTieuChuan.Where(a => a.TongSoGio > 0).FirstOrDefault();
                                    if (Gio != null)
                                    {
                                        double GioTC = Gio.GioTieuChuan * Gio.CongTieuChuan;
                                        if (tongSoGio > GioTC)
                                        {
                                            //itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                            //itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                            //{
                                            //    IdNhanVien = item.IdGuid,
                                            //    TongSoGio = tongSoGio - GioTC,
                                            //    Cong = ((tongSoGio - GioTC) / Gio.GioTieuChuan),
                                            //    CongTieuChuan = Gio.CongTieuChuan,
                                            //    HeSo = 150
                                            //});

                                            {
                                                double GioChuyenTC = tongSoGio - GioTC;
                                                if (itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)
                                                {
                                                    var GTC = itemAdd.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == Gio.GioTieuChuan);
                                                    GTC.TongSoGio -= GioChuyenTC;
                                                }
                                                double heso = 150;
                                                if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                                                {
                                                    var TC = itemAdd.listTangCa.FirstOrDefault(a => a.HeSo == heso);
                                                    if (TC != null)
                                                    {
                                                        TC.TongSoGio += GioChuyenTC;
                                                        TC.Cong += GioChuyenTC / (Gio.GioTieuChuan);
                                                    }
                                                    else
                                                    {
                                                        itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                                        {
                                                            IdNhanVien = item.IdGuid,
                                                            TongSoGio = GioChuyenTC,
                                                            Cong = ((GioChuyenTC) / (Gio.GioTieuChuan)),
                                                            CongTieuChuan = Gio.CongTieuChuan,
                                                            HeSo = heso
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                                    itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                                    {
                                                        IdNhanVien = item.IdGuid,
                                                        TongSoGio = GioChuyenTC,
                                                        Cong = ((GioChuyenTC) / (Gio.GioTieuChuan)),
                                                        CongTieuChuan = Gio.CongTieuChuan,
                                                        HeSo = heso
                                                    });
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        {
                            while (time <= dDenNgay)
                            {

                                var Ngay = listNgay.FirstOrDefault(a => a.NgayChamCong == time);

                                HRM_ThoiGianLamViecKeHoach listLichLamViecFind = listLichLamViec.Where(x => x.IdNhanVien == item.IdGuid && x.Ngay == time && !string.IsNullOrEmpty(x.IddmCaLamViec)).FirstOrDefault();

                                var checkLe = listLichNghiChiTiet.FirstOrDefault(a => a.TuNgay <= time && a.DenNgay >= time);
                                if (Ngay != null)
                                {

                                    Ngay.Nam = time.Year;
                                    Ngay.Thang = time.Month;
                                    Ngay.Ngay = time.Day;
                                    if (checkLe != null)
                                    {
                                        CongLe += ((Ngay.SoGioLamViecTangCa ?? 0) + (Ngay.SoGioLamViec ?? 0)) / (Ngay.SoGioTieuChuan ?? 8);

                                        Ngay.isNgayLe = true;
                                        Ngay.Color = "#FF0000";
                                        if (Ngay.SoGioLamViec == 0)
                                            Ngay.GioLamViec = DefineData.eKyHieuChamCong.NL.ToString();
                                        var GTC = itemAdd.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == listLichLamViecFind.GioTieuChuan);
                                        if (GTC != null)
                                        {
                                            GTC.TongSoGio -= (Ngay.SoGioLamViec ?? 0);

                                        }
                                        CongLeKDL++;
                                    }
                                    else
                                    if ((int)time.DayOfWeek == 0)
                                    {
                                        itemAdd.TongSoCong200 += (Ngay.SoGioLamViecTangCa ?? 0);
                                        Ngay.Color = "#FF0000";
                                    }
                                    else if ((int)time.DayOfWeek == 6)
                                        Ngay.Color = "#FFCC33";
                                    itemAdd.listNgay.Add(Ngay);

                                }
                                else
                                {
                                    HRM_BangChamCong_NhanVien_ThoiGian Add = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    Add.Nam = time.Year;
                                    Add.Thang = time.Month;
                                    Add.Ngay = time.Day;
                                    Add.isFake = true;
                                    Add.NgayChamCong = time;
                                    if ((int)time.DayOfWeek == 0)
                                    {
                                        Add.Color = "#FF0000";
                                    }
                                    else if ((int)time.DayOfWeek == 6)
                                        Add.Color = "#FFCC33";
                                    if (checkLe != null)
                                    {
                                        CongLeKDL++;
                                        Add.isNgayLe = true;
                                        Add.GioLamViec = DefineData.eKyHieuChamCong.NL.ToString();
                                        Add.Color = "#FF0000";
                                    }

                                    itemAdd.listNgay.Add(Add);


                                }
                                time = time.AddDays(1);
                            }
                        }
                        itemAdd.TongSoCong150 = listNgay.Sum(a => a.SoGioLamViecTangCa) - itemAdd.TongSoCong200;
                        var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                        if (CaLamViec == null)
                            CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };
                        itemAdd.TongSoCongDem = (listNgay.Where(a => a.isCaDem == true).Sum(a => a.SoGioLamViec)) / CaLamViec.SoGioTrongNgay;
                        itemAdd.TongSoCong = (double)itemAdd.listGioTieuChuan.Sum(a => a.TongSoGio / a.GioTieuChuan) + CongLeKDL;
                        //if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                        //{
                        //    itemAdd.TongSoCong -= itemAdd.listTangCa.Sum(a => a.TongSoGio);
                        //}
                        //itemAdd.TongSoCong = itemAdd.TongSoCong / CaLamViec.SoGioTrongNgay;
                        itemAdd.TongSoCong300 = Math.Round(CongLe ?? 0, 2).ToString();
                        itemAdd.TongSoCong = Math.Round(itemAdd.TongSoCong ?? 0, 2);
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                        itemAdd.IdCongTyEOS = IdCongTy;
                        if (BoPhan != null)
                        {
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        itemAdd.MaNhanVien = item.MaNhanVien;

                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            itemAdd.TenChucDanh = ChucDanh.Ten;

                        listResult.Add(itemAdd);
                    }
                    var listHeSoTangCa = listResult.Where(a => a.listTangCa != null && a.listTangCa.Count > 0).SelectMany(a => a.listTangCa.Select(x => x.HeSo)).Distinct().ToList();
                    if (listHeSoTangCa != null && listHeSoTangCa.Count > 0)
                    {
                        foreach (var item in listResult)
                        {
                            foreach (var heso in listHeSoTangCa)
                            {
                                if (item.listTangCa != null && item.listTangCa.Count > 0)
                                {
                                    var Tc = item.listTangCa.FirstOrDefault(a => a.HeSo == heso);
                                    if (Tc == null)
                                        item.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                        {
                                            IdNhanVien = item.IdGuid,
                                            TongSoGio = 0,
                                            Cong = 0,
                                            CongTieuChuan = 0,
                                            HeSo = heso
                                        });
                                }
                                else
                                {
                                    item.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                    item.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                    {
                                        IdNhanVien = item.IdGuid,
                                        TongSoGio = 0,
                                        Cong = 0,
                                        CongTieuChuan = 0,
                                        HeSo = heso
                                    });
                                }
                            }
                        }
                    }
                }

                return listResult.ToList();
                //return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listResult.OrderBy(a => a.isDieuDong).ToList());
            }

            catch (Exception ex)
            {
                return listResult;
                //return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + ex.ToString());
            }

        }
        public List<HRM_BangChamCong_NhanVien> GetChamCongFromMayChamCongPHT(long IdCongTy, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db, List<string> listIdUser = null)
        {
            List<HRM_BangChamCong_NhanVien> listResult = new List<HRM_BangChamCong_NhanVien>();
            try
            {

                //string Host = HrmOptions.Host;
                var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongTy, TuNgayUnix, DenNgayUnix, db);
                //List<string> listIdUser = new List<string>();
                if (listUserDuAn != null && listUserDuAn.Count > 0 && listIdUser == null)
                    listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();
                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                listNhanVien = db.HRM_NhanVien.AsNoTracking().Where(x => listIdUser.Contains(x.IdUser)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongTy
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                listIdNhanVien = listLichLamViec.Select(x => x.IdNhanVien).Distinct().ToList();
                //List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut.AsNoTracking()
                //                                       where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                //                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                //                                       group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                //                                       select g.OrderBy(a => a.Gio).FirstOrDefault()).ToList();
                //listCheckInOut.AddRange((from x in db.HRM_CheckInOut.AsNoTracking()
                //                         where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                //                                       && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                //                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                //                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

                List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut.AsNoTracking()
                                                       where listIdNhanVien.Contains(x.IdNhanVien)/* && x.Loai == DefineData.eKyHieuChamCong.I.ToString()*/
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                       select x
                                                     //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                     //select g.OrderBy(a => a.Gio).ToList()/*.FirstOrDefault()*/
                                                     ).ToList();
                //List<HRM_CheckInOut> listCheckInOut = (from x in listCheckInOutdata
                //                                       where x.Loai == DefineData.eKyHieuChamCong.I.ToString()
                //                                       group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                //                                       select g.OrderBy(a => a.Gio).FirstOrDefault()).ToList();
                //listCheckInOut.AddRange((from x in listCheckInOutdata
                //                         where x.Loai == DefineData.eKyHieuChamCong.O.ToString()
                //                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                //                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());

                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.AsNoTracking().ToList();
                //List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                //                                                                      && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongTy
                                                                              && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();

                var listLoaiNghi = _CallWebService.GetDanhMucLoaiNghiPhep(db);
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    foreach (var item in listXinNghiPhep)
                    {
                        var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (hinhThucNghi != null)
                            item.HinhThucNghi = hinhThucNghi.Ma;

                        var MaLoaiNghi = listLoaiNghi.FirstOrDefault(a => a.Id == item.IdLoaiNghiPhep);
                        if (MaLoaiNghi != null)
                            item.MaLoaiNghi = MaLoaiNghi.Ma;

                    }
                }
                var listCaLamViecs = db.HRM_dmLichLamViec.AsNoTracking().ToList();

                double TongT7 = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Saturday);
                double TongCN = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Sunday);
                int TongSoNgay = (dDenNgay - dTuNgay).Days + 1;
                double TongSoCongKhac = (((int)(TongSoNgay / 6)) + (int)((TongSoNgay % 6) / 4)) * 4 + (TongSoNgay % 6) % 4;
                foreach (var item in listCaLamViecs)
                {
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongT7 - TongCN);
                    if (item.LoaiNgayNghi == 2)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongCN);
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongSoCongKhac);
                }
                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                var funcKeHoach = _funcKeHoach;
                List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
                List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
                var DuAn = _CallWebService.GetQuyTrinhDuAn(IdCongTy, db);
                if (DuAn == null)
                    DuAn = new BoPhan();
                DateTime dateNow = DateTime.Now;
                dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);
                if (DuAn.MaBoPhan == "VPCT")
                {
                    SoPhutDuocDiMuon = 0;
                    SoPhutDuocVeSom = 0;
                }
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "026304003878")
                        item.MaNhanVien = item.MaNhanVien;
                    var ChucVu = ChucVus.FirstOrDefault(a => a.Id == item.IdChucVu);
                    var NhanSuDA = listUserDuAn.FirstOrDefault(a => a.IdUser == item.IdUser);
                    DateTime dt = dTuNgay;

                    while (dt <= dDenNgay)
                    {
                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                        DateTime Check = dt;
                        if (listLichLamViecFind != null)
                            Check = listLichLamViecFind.OrderByDescending(a => a.Ngay).Select(a => a.Ngay).FirstOrDefault();
                        List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && (x.Ngay == dt || x.Ngay == Check)).OrderBy(a => a.Gio).ToList();
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);
                        if (dt >= dateNow)
                        {
                            dt = dt.AddDays(1);
                            continue;
                        }
                        if (listCheckInOutFind != null && listCheckInOutFind.Count > 1)
                        {
                            if (listLichLamViecFind == null)
                                listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();
                            string IdCaLamViec = listLichLamViecFind.Where(a => a.Loai == 0 && a.IdCongTy == IdCongTy && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();
                            var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);
                            if (CaLamViec == null)
                                CaLamViec = new HRM_dmLichLamViec { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };
                            List<clsDiMuonVeSom> listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, DuAn.MaBoPhan, NghiPheps, false, IdCongTy);
                            //List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                            if (listLichLamViecFind.Count >= 1)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.SoGioTieuChuan = CaLamViec.SoGioTrongNgay ?? 8;

                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                //bool TangCaDem = false;
                                foreach (var KeHoach in listLichLamViecFind)
                                {

                                    if (KeHoach.IdCongTy != IdCongTy)
                                        continue;
                                    double SoGioLamViec = (KeHoach.TongSoGioGiao - (KeHoach.SoGioNghi ?? 0));
                                    if ((int)dt.DayOfWeek == 6 && CaLamViec != null && CaLamViec.LoaiNgayNghi == 4)
                                        SoGioLamViec = CaLamViec.SoGioTrongNgay ?? 8;
                                    if (KeHoach.Loai == 0)
                                    {
                                        if (KeHoach.isCaGay == true)
                                            itemAdd.SoGioLamViecCaGay += SoGioLamViec;
                                        else
                                            itemAdd.SoGioLamViec += SoGioLamViec;

                                    }
                                    else
                                    {
                                        itemAdd.SoGioLamViecTangCa += SoGioLamViec;

                                    }

                                }
                                List<string> listKyHieu = new List<string>();
                                if (listSomMuon != null && listSomMuon.Count > 0)
                                    foreach (var SomMuon in listSomMuon)
                                    {
                                        //itemAdd.GioLamViec += "; ";
                                        double Gio = SomMuon.SoPhut / 60.0;
                                        if (SomMuon.Loai == 0)
                                        {
                                            if (SomMuon.isCaGay == true)
                                                itemAdd.SoGioLamViecCaGay -= Gio;
                                            else
                                                itemAdd.SoGioLamViec -= Gio;

                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa -= Gio;

                                        }
                                        listKyHieu.Add(SomMuon.KyHieuChamCong);
                                    }

                                if (CaLamViec != null && ((itemAdd.SoGioLamViec) > CaLamViec.SoGioTrongNgay))
                                {
                                    itemAdd.SoGioLamViecTangCa += itemAdd.SoGioLamViec - CaLamViec.SoGioTrongNgay;
                                    itemAdd.SoGioLamViec = CaLamViec.SoGioTrongNgay;
                                }
                                if (CaLamViec != null && ((itemAdd.SoGioLamViecCaGay) > CaLamViec.SoGioTrongNgay))
                                {
                                    itemAdd.SoGioLamViecTangCa += itemAdd.SoGioLamViecCaGay - CaLamViec.SoGioTrongNgay;
                                    itemAdd.SoGioLamViecCaGay = CaLamViec.SoGioTrongNgay;
                                }

                                if (itemAdd.SoGioLamViec < 0)
                                    itemAdd.SoGioLamViec = 0;
                                if (itemAdd.SoGioLamViecCaGay < 0)
                                    itemAdd.SoGioLamViecCaGay = 0;
                                double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                string KyHieu = "";
                                if (listKyHieu != null && listKyHieu.Count > 0)
                                    KyHieu += string.Join("", listKyHieu);
                                if (itemAdd.SoGioLamViecCaGay > 0)
                                    KyHieu += Math.Round(itemAdd.SoGioLamViecCaGay ?? 0, 2) + "P";
                                if (itemAdd.SoGioLamViec > 0)
                                    KyHieu += Math.Round(itemAdd.SoGioLamViec ?? 0, 2) + "F";

                                if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                {
                                    itemAdd.isCaDem = true;
                                }
                                if (itemAdd.SoGioLamViecTangCa > 0)
                                {
                                    //if (TangCaDem == false)
                                    //{
                                    KyHieu += "(" + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + ")";
                                    //}
                                    //else
                                    //{
                                    //    if (listLichLamViecFind.Count > 1)
                                    //        KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                    //    else if (listLichLamViecFind.Count == 1)
                                    //    {
                                    //        if (GioLamViec > 0)
                                    //            KyHieu += "TC" + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                    //        else
                                    //            KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                    //    }
                                    //}
                                }
                                itemAdd.GioLamViec = KyHieu;
                                //if (NhanSuDA.isDieuDong == false)
                                //{
                                //    //var checkIn = listCheckInOutFind.FirstOrDefault(a => a.IdCongTy != IdCongTy);
                                //    //if (checkIn != null)
                                //    //{
                                //    //    itemAdd.GioLamViec = DefineData.eKyHieuChamCong.XTC.ToString();

                                //    //}
                                //}
                                //else
                                //{
                                //    var checkIn = listCheckInOutFind.FirstOrDefault(a => a.IdCongTy != IdCongTy);
                                //    if (checkIn != null)
                                //    {
                                //        itemAdd.SoGioLamViec = 0;

                                //    }
                                //}
                                if (itemAdd.SoGioLamViec < 0)
                                    itemAdd.SoGioLamViec = 0;

                                if (itemAdd.SoGioLamViecCaGay < 0)
                                    itemAdd.SoGioLamViecCaGay = 0;

                                listData.Add(itemAdd);
                            }
                        }
                        else if (NghiPheps.Count > 0)
                        {
                            //if (listLichLamViecFind == null)
                            //    listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.FirstOrDefault(x => string.Join(",", NghiPheps.Select(a => a.HinhThucNghi).Distinct().ToList()).Contains(x.Ma))/* == NghiPheps.FirstOrDefault().HinhThucNghi)*/;
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                //double SoGioLamViec = 0;
                                //foreach (var KeHoach in listLichLamViecFind)
                                //{
                                //    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                //}
                                itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                double SoGioNghi = NghiPheps.Sum(a => a.ThoiGianNghi);
                                if (SoGioNghi == 0.5)
                                    itemAdd.GioLamViec += "/2;NTD";
                                listData.Add(itemAdd);
                            }
                        }
                        else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                        {
                            HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                            if (NghiPhep != null)
                            {
                                var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                itemAdd.SoGioLamViec = 0;
                                itemAdd.SoGioLamViecCaGay = 0;
                                itemAdd.SoGioLamViecTangCa = 0;
                                itemAdd.NgayChamCong = dt;
                                itemAdd.IdCongTyEOS = IdCongTy;
                                itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                double SoGioLamViec = 0;
                                foreach (var KeHoach in listLichLamViecFind)
                                {
                                    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                }
                                itemAdd.GioLamViec = NghiPhep.Ma;
                                listData.Add(itemAdd);
                            }
                        }
                        dt = dt.AddDays(1);
                    }

                }
                var listHopDong = (from a in db.HRM_HopDong
                                   where listIdNhanVien.Contains(a.IdNhanVien) && a.isKetThuc == true
                                   group a by a.IdNhanVien into g
                                   select g.OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault()).ToList();
                var listTangCa = db.HRM_TongHopXacNhanNgoaiGio.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TuNgay <= dTuNgay && a.DenNgay <= dDenNgay).OrderByDescending(a => a.Modified).ToList();
                //List<HRM_BangChamCong_NhanVien> listResult = new List<HRM_BangChamCong_NhanVien>();
                if (listData != null && listData.Count > 0)
                {

                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    List<long> IdNhanVienEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongTy && (a.Nam == dTuNgay.Year || a.Nam == dDenNgay.Year)).ToList();
                    List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                    //var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => ((a.TuNgay.Month <= dTuNgay.Month) || a.DenNgay.Month >= dTuNgay.Month || a.TuNgay.Month <= dDenNgay.Month || a.DenNgay.Month >= dDenNgay.Month)).ToList();
                    var listIdHinhThucNghi = listHinhThucNghi.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).Select(a => a.Id).ToList();
                    var LoaiHopDongs = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.Ma == "HÄVV");

                    foreach (var item in listNhanVien)
                    {
                        if (item.MaNhanVien == "026304003878")
                            item.MaNhanVien = item.MaNhanVien;
                        {
                            var check = listData.Where(a => a.IdNhanVien == item.IdGuid).FirstOrDefault();
                            if (check == null)
                                continue;
                        }

                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        HRM_BangChamCong_NhanVien itemAdd = new HRM_BangChamCong_NhanVien();
                        itemAdd.NgayVaoLam = item.NgayBatDauLamViec;
                        var user = listUserDuAn.FirstOrDefault(a => a.IdUser == item.IdUser);
                        itemAdd.TongSoCong200 = 0;
                        itemAdd.TongSoCong = 0;
                        itemAdd.isDieuDong = user.isDieuDong;
                        itemAdd.LinkAnhDaiDien = user.LinkAnhDaiDien;
                        itemAdd.IdCongTyEOS = IdCongTy;
                        //itemAdd.isTruyThu = !itemAdd.isDieuDong;
                        itemAdd.TongSoCong150 = 0;
                        //itemAdd.TongSoCong300 = 0;
                        itemAdd.TongSoCong = 0;
                        itemAdd.TongCongPhep = listXinNghiPhep.Where(x => x.IdNhanVien == item.IdGuid && listIdHinhThucNghi.Contains(x.IdHinhThucNghi)).Sum(a => a.ThoiGianNghi);



                        itemAdd.listNgay = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                        DateTime time = dTuNgay;
                        var listNgay = listData.Where(a => a.IdNhanVien == item.IdGuid && a.IdCongTyEOS == IdCongTy).OrderBy(a => a.NgayChamCong).ToList();

                        var listTangCaHT = listTangCa.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGioTangCa = listNgay.Sum(a => a.SoGioLamViecTangCa ?? 0);

                        if (HopDong != null && HopDong.LoaiLuong != 3)
                            itemAdd.TongSoCongTangCa = listNgay.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));/*CaLamViec.SoGioTrongNgay*/
                        else
                            itemAdd.TongSoCongTangCa = itemAdd.SoGioTangCa;
                        double? CLCaGay = 0;
                        double? CongLe = 0;
                        double? CongLeP = 0;
                        double? CLCaThang = 0;
                        {
                            while (time <= dDenNgay)
                            {
                                var checkLe = listLichNghiChiTiet.FirstOrDefault(a => a.TuNgay <= time && a.DenNgay >= time);

                                var Ngay = listNgay.FirstOrDefault(a => a.NgayChamCong == time);
                                if (Ngay != null)
                                {
                                    if (Ngay.SoGioTieuChuan == 0 || Ngay.SoGioTieuChuan == null)
                                        Ngay.SoGioTieuChuan = 8;
                                    double TongSoCong = (Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0);

                                    if (HopDong != null && HopDong.LoaiLuong != 3)
                                    {
                                        itemAdd.TongSoCong += TongSoCong / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                    }
                                    else
                                        itemAdd.TongSoCong += TongSoCong;
                                    if (checkLe != null)
                                    {

                                        Ngay.isNgayLe = true;
                                        Ngay.Color = "#FF0000";
                                        if (Ngay.SoGioLamViec == 0 && Ngay.SoGioLamViecCaGay == 0)
                                        {
                                            //List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == time);
                                            //if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                            //{
                                            //    //Add.SoGioLamViec = 
                                            //    if (HopDong == null || (HopDong != null && HopDong.LoaiLuong != 3))
                                            //    {
                                            //        itemAdd.TongSoCong300 += 1;
                                            //    }

                                            //}
                                            Ngay.GioLamViec = DefineData.eKyHieuChamCong.NL.ToString();
                                        }
                                        else
                                        {
                                            if (HopDong == null || (HopDong != null && HopDong.LoaiLuong != 3))
                                            {
                                                CongLe += ((Ngay.SoGioLamViecTangCa ?? 0) + (Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0)) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                                CongLeP += ((Ngay.SoGioLamViecCaGay ?? 0)) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                                itemAdd.TongSoCong = itemAdd.TongSoCong - ((Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0)) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                                itemAdd.TongSoCongTangCa = itemAdd.TongSoCongTangCa - (Ngay.SoGioLamViecTangCa ?? 0) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                                CLCaGay = CLCaGay + (Ngay.SoGioLamViecCaGay ?? 0) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                                CLCaThang = CLCaThang + (Ngay.SoGioLamViec ?? 0) / (Ngay.SoGioTieuChuan == 0 ? 8 : Ngay.SoGioTieuChuan);
                                            }

                                            else
                                            {
                                                CongLe += ((Ngay.SoGioLamViecTangCa ?? 0) + (Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0));
                                                CongLeP += ((Ngay.SoGioLamViecCaGay ?? 0));
                                                itemAdd.TongSoCong = itemAdd.TongSoCong - ((Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0));
                                                itemAdd.TongSoCongTangCa = itemAdd.TongSoCongTangCa - (Ngay.SoGioLamViecTangCa ?? 0);
                                                CLCaGay = CLCaGay + (Ngay.SoGioLamViecCaGay ?? 0);
                                                CLCaThang = CLCaThang + (Ngay.SoGioLamViec ?? 0);
                                            }
                                        }

                                    }
                                    else
                                    if ((int)time.DayOfWeek == 0)
                                    {
                                        //itemAdd.TongSoCong200 += (Ngay.SoGioLamViecTangCa ?? 0);
                                        //Ngay.isNgayLe = true;
                                    }
                                    Ngay.Nam = time.Year;
                                    Ngay.Thang = time.Month;
                                    Ngay.Ngay = time.Day;
                                    itemAdd.listNgay.Add(Ngay);
                                }
                                else
                                {
                                    List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == time);
                                    HRM_BangChamCong_NhanVien_ThoiGian Add = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    Add.isFake = true;
                                    Add.NgayChamCong = time;
                                    Add.Nam = time.Year;
                                    Add.Thang = time.Month;
                                    Add.Ngay = time.Day;
                                    if (checkLe != null)
                                    {
                                        if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                        {
                                            //Add.SoGioLamViec = 
                                            //if (HopDong == null || (HopDong != null && HopDong.IdLoaiHopDong != LoaiHopDongs.IdGuid))
                                            //    itemAdd.TongSoCong300 += 1;

                                        }
                                        Add.isNgayLe = true;
                                        Add.GioLamViec = DefineData.eKyHieuChamCong.NL.ToString();
                                        Add.Color = "#FF0000";
                                    }
                                    itemAdd.listNgay.Add(Add);
                                }
                                time = time.AddDays(1);
                            }
                        }
                        if (CongLe > 0)
                        {
                            itemAdd.TongSoCong300 = Math.Round(CongLe ?? 0, 2).ToString();
                            if (CongLe > 0)
                                itemAdd.TongSoCong300 += "(" + Math.Round(CongLeP ?? 0, 2).ToString() + "p)";
                        }
                        //itemAdd.TongSoCong150 = listNgay.Sum(a => a.SoGioLamViecTangCa) - itemAdd.TongSoCong200 - Cong300;
                        var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                        if (CaLamViec == null)
                            CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };
                        itemAdd.SoCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 24;
                        itemAdd.SoGioTrongNgay = CaLamViec.SoGioTrongNgay ?? 8;
                        //itemAdd.TongSoCongDem = (listNgay.Where(a => a.isCaDem == true).Sum(a => a.SoGioLamViec)) / CaLamViec.SoGioTrongNgay;
                        double? TCaGay = 0;

                        if (HopDong == null || (HopDong != null && HopDong.LoaiLuong != 3))
                        {
                            TCaGay = listNgay.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan == 0 ? 8 : a.SoGioTieuChuan)) - (CLCaGay ?? 0);
                        }
                        else
                        {
                            TCaGay = listNgay.Sum(a => (a.SoGioLamViecCaGay ?? 0)) - (CLCaGay ?? 0);
                        }
                        itemAdd.TongSoCongChu = Math.Round(itemAdd.TongSoCong ?? 0, 2).ToString();
                        if (TCaGay > 0)
                            itemAdd.TongSoCongChu = itemAdd.TongSoCongChu + "(" + Math.Round(TCaGay ?? 0, 2) + "P)";
                        itemAdd.TongSoCong = Math.Round(itemAdd.TongSoCong ?? 0, 2);
                        //itemAdd.TongSoCong300 = Math.Round((itemAdd.TongSoCong300 ?? 0), 2);
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;

                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;

                        }
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (chucVu != null)
                            itemAdd.TenChucVu = chucVu.TenChucVu;
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            itemAdd.TenChucDanh = ChucDanh.Ten;

                        listResult.Add(itemAdd);
                    }
                }

                return listResult;
                //return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listResult.OrderBy(a => a.isDieuDong).ToList());
            }

            catch (Exception ex)
            {
                return listResult;
                //return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + ex.ToString());
            }

        }
        public dynamic GetThoiGianChamCong(TimKiemChung timkiem,
            ApplicationDbContext db)
        {
            int nCurrentPage = timkiem.CurrentPage ?? 0;
            int PageSize = timkiem.PageSize ?? 10;
            var ChucVus = _CallWebService.GetDanhSachChucVu(db);
            var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(timkiem.IdCongTy, timkiem.TuNgayUnix, timkiem.DenNgayUnix, db);
            List<string> listIdUser = new List<string>();
            if (listUserDuAn != null && listUserDuAn.Count > 0)
                listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();
            DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(timkiem.TuNgayUnix);
            DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(timkiem.DenNgayUnix);
            List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
            var listNhanViens = db.HRM_NhanVien.Where(x => listIdUser.Contains(x.IdUser));
            if (timkiem.IdChucVu > 0)
                listNhanViens = listNhanViens.Where(a => a.IdChucVu == timkiem.IdChucVu);
            if (timkiem.IdBoPhan > 0)
                listNhanViens = listNhanViens.Where(a => a.IdBoPhanEOS == timkiem.IdBoPhan);
            if (timkiem.IddmChucDanh > 0)
                listNhanViens = listNhanViens.Where(a => a.IdChucDanh == timkiem.IddmChucDanh);
            if (!string.IsNullOrEmpty(timkiem.KeyWord))
                listNhanViens = listNhanViens.Where(a => a.MaNhanVien.ToLower().Contains(timkiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(timkiem.KeyWord.ToLower().Trim()));
            listNhanVien = listNhanViens.ToList();
            var listdmLoaiNhanVien = db.HRM_dmLoaiNhanVien.ToList();
            List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
            List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy != 0
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            listIdNhanVien = listThoiGianLamViec.Select(x => x.IdNhanVien).ToList();
            listNhanVien = listNhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            int nSum = listNhanVien.Count;
            PagingInfor paging = new PagingInfor();
            if (nCurrentPage > 0)
            {
                int nTotalPage = nSum / PageSize;
                paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                listNhanVien = listNhanVien.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            }
            listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
            DateTime datecheck = dDenNgay.AddDays(1);
            List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut
                                                   where listIdNhanVien.Contains(x.IdNhanVien) /*&& x.Loai == DefineData.eKyHieuChamCong.I.ToString()*/
                                                                 && x.Ngay >= dTuNgay && x.Ngay <= datecheck
                                                   //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                   //select g.OrderBy(a => a.Gio).FirstOrDefault()
                                                   select x

                                                   ).ToList();
            //listCheckInOut.AddRange((from x in db.HRM_CheckInOut
            //                         where listIdNhanVien.Contains(x.IdNhanVien) && x.Loai == DefineData.eKyHieuChamCong.O.ToString()
            //                                       && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
            //                         group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
            //                         select g.OrderByDescending(a => a.Gio).FirstOrDefault()).ToList());


            List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                          && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
            //List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
            //                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
            List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                                                                                  && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
            List<HRM_TheoDoiXinNghiPhep> listNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                          && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();
            var listLoaiNghi = _CallWebService.GetDanhMucLoaiNghiPhep(db);
            var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
            var listDuAn = _CallWebService.GetDanhSachDuAn(db);
            if (listNghiPhep != null && listNghiPhep.Count > 0)
            {
                foreach (var item in listNghiPhep)
                {
                    var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                    if (hinhThucNghi != null)
                        item.HinhThucNghi = hinhThucNghi.Ma;

                    var MaLoaiNghi = listLoaiNghi.FirstOrDefault(a => a.Id == item.IdLoaiNghiPhep);
                    if (MaLoaiNghi != null)
                        item.MaLoaiNghi = MaLoaiNghi.Ma;

                }
            }
            var listCa = db.HRM_dmLichLamViec.ToList();
            listCheckInOut = listCheckInOut.OrderBy(a => a.Gio).ToList();
            List<HRM_BangChamCong_NhanVienView> listResultNV = new List<HRM_BangChamCong_NhanVienView>();
            List<HRM_BangChamCong_NhanVien_Ngay> listResult = new List<HRM_BangChamCong_NhanVien_Ngay>();
            List<HRM_ThoiGianLamViecKeHoach> listInOutCD = new List<HRM_ThoiGianLamViecKeHoach>();
            int nCount = listCheckInOut.Count;
            double SoGioLamViecTT = 0;
            //string VeSom = HrmOptions.SoPhutSom.ToString();
            //double SoPhutDuocVeSom = double.Parse(VeSom);

            //string DiMuon = HrmOptions.SoPhutMuon.ToString();

            var listDuAn_NhanVien = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
            List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
            var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
            if (Host == "PHT")
                listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == timkiem.IdCongTy && (a.Nam == dTuNgay.Year || a.Nam == dDenNgay.Year)).ToList();
            List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
            var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
            //double SoPhutDuocDiMuon = double.Parse(DiMuon);
            for (int i = 0; i < nCount; i++)
            {
                var itemIn = listCheckInOut[i];
                if (itemIn.Loai == DefineData.eKyHieuChamCong.O.ToString())
                    continue;
                HRM_ThoiGianLamViecKeHoach InOutCD = new HRM_ThoiGianLamViecKeHoach();
                InOutCD.IdCongTy = itemIn.IdCongTy;
                InOutCD.Ngay = itemIn.Ngay;
                InOutCD.IdNhanVien = itemIn.IdNhanVien;
                DateTime ngayCheck = itemIn.Ngay;
                var LLV = listThoiGianLamViec.FirstOrDefault(a => a.Ngay == ngayCheck && a.IdNhanVien == itemIn.IdNhanVien);
                if (LLV != null)
                    ngayCheck = LLV.GioRa.Date;
             
                var listCheckInOuts = listCheckInOut.Where(a => a.IdNhanVien == itemIn.IdNhanVien && a.Gio >= itemIn.Gio /*&& a.IdCongTy == itemIn.IdCongTy*/ && a.Ngay >= itemIn.Ngay && a.Ngay <= ngayCheck).ToList();

                int Count = listCheckInOuts.Count;
                InOutCD.GioVao = itemIn.Gio;
                if (Count >= 2)
                {
                    var itemOut = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString()).FirstOrDefault();
                    if (itemOut != null)
                    {
                        InOutCD.GioRa = itemOut.Gio;
                        InOutCD.IdCongTyVe = itemOut.IdCongTy;
                    }
                }
                if (i + 2 < nCount && InOutCD.GioRa != null && InOutCD.GioRa.Year > 2024)
                {
                    var itemVaoTiep = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.I.ToString() && a.Gio > InOutCD.GioRa).OrderBy(a => a.Gio).FirstOrDefault();
                    if (itemVaoTiep != null)
                    {
                        InOutCD.GioVaoTiep = itemVaoTiep.Gio;
                        var itemOut = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > InOutCD.GioVao && a.Gio <= InOutCD.GioVaoTiep).OrderByDescending(a => a.Gio).FirstOrDefault();
                        if (itemOut != null)
                            InOutCD.GioRa = itemOut.Gio;
                    }
                    else
                    {
                        var itemOut = listCheckInOuts.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > InOutCD.GioVao).OrderByDescending(a => a.Gio).FirstOrDefault();
                        if (itemOut != null)
                            InOutCD.GioRa = itemOut.Gio;
                    }
                    SoGioLamViecTT += (InOutCD.GioRa - InOutCD.GioVao).TotalMinutes;

                }
                else
                    SoGioLamViecTT += (InOutCD.GioRa - InOutCD.GioVao).TotalMinutes;
                listInOutCD.Add(InOutCD);
            }

            foreach (var item in listThoiGianLamViec)
            {

                var listInOutCDs = listInOutCD.Where(a => a.IdNhanVien == item.IdNhanVien && a.Ngay >= item.Ngay && ((a.GioVao <= item.GioRa && a.GioRa >= item.GioVao) || (a.GioVao < item.GioRa && (a.GioRa == null || a.GioRa.Year != a.GioVao.Year)) || (a.GioRa > item.GioVao && (a.GioVao == null || a.GioRa.Year != a.GioVao.Year)))).OrderBy(a => a.GioVao).ToList();
                if (listInOutCDs != null)
                {
                    foreach (var itemInOut in listInOutCDs/*.Where(a => a.IdCongTy == item.IdCongTy)*/.ToList())
                    {
                        if (itemInOut.GioRa.Year > 2020 && itemInOut.GioRa < item.GioVao)
                            continue;
                        if (itemInOut.GioVao > item.GioRa && item.GioVao > item.GioRa)
                            continue;
                        if (item.GioVaoTT == null || item.GioVaoTT > itemInOut.GioVao)
                        {
                            //if (item.GioVao >= itemInOut.GioVao && item.GioRa > itemInOut.GioVao)
                            //    item.GioVaoTT = itemInOut.GioVao;
                            //else if (item.GioVao <= itemInOut.GioVao && item.GioRa > itemInOut.GioVao)
                            //{
                            item.GioVaoTT = itemInOut.GioVao;
                            //}
                        }
                        if (itemInOut.GioRa.Year > 2020 && (item.GioRaTT == null || (item.GioRaTT <= itemInOut.GioRa && item.GioRa >= itemInOut.GioVao)))
                            item.GioRaTT = itemInOut.GioRa;
                        if (itemInOut.GioVaoTiep != null)
                            item.GioVaoTiep = itemInOut.GioVaoTiep;
                    }
                }

            }
            var DuAn = listDuAn.FirstOrDefault(a => a.Id == timkiem.IdCongTy);
            DateTime today = DateTime.Today;
            foreach (var NV in listNhanVien)
            {
                NV.TenChucVu = ChucVus.Where(a => a.Id == NV.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                HRM_BangChamCong_NhanVienView NhanVien = new HRM_BangChamCong_NhanVienView();
                NhanVien.listNgay = new List<HRM_BangChamCong_NhanVien_Ngay>();
                NhanVien.MaNhanVien = NV.MaNhanVien;
                var listIdCTY = listDuAn_NhanVien.Where(a => a.IdQuyTrinh == NV.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
                if (listIdCTY != null && listIdCTY.Count > 0)
                {
                    var listCty = listDuAn.Where(a => listIdCTY.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                    NV.TenCongTy = string.Join("; ", listCty);
                }
                NhanVien.TenNhanVien = NV.TenDayDu;
                listIdLichNghi = listLichNghi.Where(a => a.IdBoPhan == NV.IdBoPhanEOS).Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiets = listLichNghiChiTiet.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                var LichLamViecs = listThoiGianLamViec.Where(a => a.IdNhanVien == NV.IdGuid).OrderBy(a => a.Ngay).ToList();
                var ListCheckIns = listInOutCD.Where(a => a.IdNhanVien == NV.IdGuid).ToList();
                List<DateTime> listDate = listCheckInOut.Where(a => a.IdNhanVien == NV.IdGuid).Select(a => a.Ngay).Distinct().ToList();
                string LoaiNhanVien = listdmLoaiNhanVien.Where(a => a.IdGuid == NV.IdLoaiNhanVien).Select(a => a.Ten).FirstOrDefault();
                foreach (var date in listDate)
                {
                    int heso = 1;

                    if (date > today)
                        break;
                    var LamViecNgay = LichLamViecs.Where(a => a.Ngay == date).OrderBy(a => a.GioVao).ToList();
                    double TGianLam = LamViecNgay.Sum(a => a.TongSoGioGiao);
                    var ListCheckIn = ListCheckIns.Where(a => a.IdNhanVien == NV.IdGuid && a.Ngay == date).ToList();
                    var NgayLe = listLichNghiChiTiets.FirstOrDefault(a => a.TuNgay <= date && a.DenNgay >= date);
                    double NghiPhep = 0;
                    //if (listNghiPhep != null && listNghiPhep.Count > 0)
                    //    NghiPhep = listNghiPhep.Where(a => a.IdNhanVien == NV.IdGuid && a.NgayNghi == date).Sum(a => TGianLam * a.ThoiGianNghi);
                    int i = 0;
                    if (ListCheckIn == null || ListCheckIn.Count == 0)
                    {
                        var checkIn = listCheckInOut.Where(a => a.IdNhanVien == NV.IdGuid && a.Ngay == date).ToList();
                        if (checkIn == null || checkIn.Count == 0)
                            continue;
                        else
                        {
                            DateTime ngayCheck = date;
                            var LLV = listThoiGianLamViec.FirstOrDefault(a => a.Ngay == date && a.IdNhanVien == NV.IdGuid);

                            ListCheckIn = new List<HRM_ThoiGianLamViecKeHoach>();
                            HRM_ThoiGianLamViecKeHoach InOutCD = new HRM_ThoiGianLamViecKeHoach();
                            var itemOut = checkIn.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Ngay == date).OrderByDescending(a => a.Gio).FirstOrDefault();
                            if (LLV != null)
                            {
                                ngayCheck = LLV.GioRa.Date;
                                var itemOutCheck = checkIn.Where(a => a.Loai == DefineData.eKyHieuChamCong.O.ToString() && a.Gio > LLV.GioVao && (a.Ngay == date || a.Ngay == ngayCheck)).OrderByDescending(a => a.Gio).FirstOrDefault();
                                if (itemOutCheck != null)
                                    itemOut = itemOutCheck;
                            }
                            if (itemOut != null)
                            {
                                InOutCD.GioRa = itemOut.Gio;
                                InOutCD.IdCongTyVe = itemOut.IdCongTy;
                                ListCheckIn.Add(InOutCD);
                            }
                        }
                    }
                    if (ListCheckIn.Count == 1 && LamViecNgay.Count > 1)
                    {
                        if ((ListCheckIn == null || ListCheckIn.Count == 0))
                        {
                            if (listNghiPhep != null && listNghiPhep.Count > 0)
                                NghiPhep = listNghiPhep.Where(a => a.IdNhanVien == NV.IdGuid && a.NgayNghi == date).Sum(a => TGianLam * a.ThoiGianNghi);
                            if (NghiPhep > 0)
                            {
                                HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                                itemAdd.MaNhanVien = NV.MaNhanVien;
                                itemAdd.TenNhanVien = NV.TenDayDu;
                                itemAdd.TenLoaiNhanVien = LoaiNhanVien;
                                itemAdd.Ngay = date;
                                itemAdd.TenCongTy = DuAn.TenBoPhan;
                                itemAdd.TenChucVu = NV.TenChucVu;
                                var listIdCa = LamViecNgay.Select(a => a.IddmCaLamViec).Distinct().ToList();
                                var CaLamViecs = listCa.Where(a => listIdCa.Contains(a.IdGuid)).ToList();
                                if (CaLamViecs == null)
                                    CaLamViecs = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).ToList();
                                itemAdd.CaLamViec = string.Join(", ", CaLamViecs.Select(a => a.Ten));

                                //if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                //    CaLamViec.SoGioTrongNgay = 8;
                                itemAdd.ThoiGianLamViec = string.Join(", ", LamViecNgay.Select(KH => KH.GioVao.ToString("HH:mm") + " - " + KH.GioRa.ToString("HH:mm")).ToList());
                                var CaLamViec = listCa.Where(a => a.IdGuid == LamViecNgay.FirstOrDefault().IddmCaLamViec).FirstOrDefault();
                                double soGioTrongNgay = (CaLamViec != null && CaLamViec.SoGioTrongNgay > 0) ? CaLamViec.SoGioTrongNgay.Value : 8;
                                itemAdd.CongPhep += (double)(NghiPhep / soGioTrongNgay);
                                listResult.Add(itemAdd);
                                NhanVien.listNgay.Add(itemAdd);
                            }
                        }
                        else
                        {
                            HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                            itemAdd.MaNhanVien = NV.MaNhanVien;
                            itemAdd.TenNhanVien = NV.TenDayDu;
                            itemAdd.TenLoaiNhanVien = LoaiNhanVien;
                            itemAdd.Ngay = date;
                            itemAdd.TenCongTy = NV.TenCongTy;
                            itemAdd.TenChucVu = NV.TenChucVu;
                            var listIdCa = LamViecNgay.Select(a => a.IddmCaLamViec).Distinct().ToList();
                            var CaLamViecs = listCa.Where(a => listIdCa.Contains(a.IdGuid)).ToList();
                            if (CaLamViecs == null)
                                CaLamViecs = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).ToList();
                            itemAdd.CaLamViec = string.Join(", ", CaLamViecs.Select(a => a.Ten));
                            //if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                            //    CaLamViec.SoGioTrongNgay = 8;
                            itemAdd.ThoiGianLamViec = string.Join(", ", LamViecNgay.Select(KH => KH.GioVao.ToString("HH:mm") + " - " + KH.GioRa.ToString("HH:mm")).ToList());
                            //if ((ListCheckIn.Count - 1) < i)
                            //    continue;
                            //var checkIn = ListCheckIn[i];
                            var checkIn = ListCheckIn.FirstOrDefault();
                            //var checkIn = ListCheckIn.Where(a=>a.GioVao<=kh).FirstOrDefault();
                            if (checkIn != null)
                            {
                                if (checkIn.GioVao != null && checkIn.GioVao.Year > 2020)
                                    itemAdd.TuGio = checkIn.GioVao;
                                itemAdd.NoiChamTuGio = listDuAn.Where(a => a.Id == checkIn.IdCongTy).Select(a => a.TenBoPhan).FirstOrDefault();
                                if (checkIn.GioRa != null && checkIn.GioRa.Year > 2020)
                                {
                                    itemAdd.DenGio = checkIn.GioRa;
                                    itemAdd.NoiChamDenGio = listDuAn.Where(a => a.Id == checkIn.IdCongTyVe).Select(a => a.TenBoPhan).FirstOrDefault();

                                }
                                itemAdd.SoGioQuyDoi = 0;
                                itemAdd.SoGioVeSom = "";
                                itemAdd.SoGioDiMuon = "";
                                var CaLamViec = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).FirstOrDefault();

                                foreach (var KH in LamViecNgay)
                                {
                                    if ((itemAdd.TuGio != null && itemAdd.TuGio >= KH.GioRa) || (itemAdd.DenGio != null && itemAdd.DenGio <= KH.GioVao))
                                        continue;
                                    double SoGioQuyDoi = 0;
                                    CaLamViec = listCa.Where(a => a.IdGuid == KH.IddmCaLamViec).FirstOrDefault();

                                    if (CaLamViec == null)
                                        CaLamViec = new HRM_dmLichLamViec();

                                    if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                        CaLamViec.SoGioTrongNgay = 8;
                                    if (itemAdd.DenGio != null && itemAdd.TuGio != null)
                                    {
                                        SoGioQuyDoi = KH.TongSoGioGiao;
                                        if (Host == "PHT" && KH.Loai != 1 && (int)date.DayOfWeek == 6 && CaLamViec != null && CaLamViec.LoaiNgayNghi == 4)
                                            SoGioQuyDoi = CaLamViec.SoGioTrongNgay ?? 8;
                                        SoGioQuyDoi = SoGioQuyDoi - (KH.SoGioNghi ?? 0);

                                    }
                                    if (itemAdd.DenGio != null && itemAdd.DenGio < KH.GioRa && itemAdd.DenGio > KH.GioVao)
                                    {

                                        if (listNghiPhep != null && listNghiPhep.Count > 0)
                                            NghiPhep = listNghiPhep.Where(a => a.IdNhanVien == NV.IdGuid && a.NgayNghi == date && a.MaLoaiNghi == "CHIEU").Sum(a => TGianLam * a.ThoiGianNghi);
                                        if (SoGioQuyDoi > 0 && NghiPhep > 0)
                                        {
                                            itemAdd.CongPhep += (double)(NghiPhep / CaLamViec.SoGioTrongNgay);
                                            SoGioQuyDoi -= NghiPhep;
                                        }
                                        if (itemAdd.DenGio < KH.GioRa)
                                        {
                                            if (SoGioQuyDoi > 0 && itemAdd.DenGio.Value.AddHours(NghiPhep) < KH.GioRa)
                                            {
                                                TimeSpan times = (KH.GioRa - itemAdd.DenGio.Value.AddHours(NghiPhep));

                                                itemAdd.SoGioVeSom += times.Hours + ":" + times.Minutes;
                                                SoGioQuyDoi -= times.TotalHours;

                                            }
                                        }
                                        else
                                        {

                                            if (SoGioQuyDoi > 0 && KH.GioVao.AddHours(NghiPhep) < KH.GioRa)
                                            {
                                                TimeSpan times = (KH.GioRa - KH.GioVao.AddHours(NghiPhep));

                                                itemAdd.SoGioVeSom += times.Hours + ":" + times.Minutes;
                                                SoGioQuyDoi -= times.TotalHours;

                                            }
                                        }
                                    }
                                    if (itemAdd.TuGio != null && itemAdd.TuGio > KH.GioVao && itemAdd.TuGio < KH.GioRa)
                                    {
                                        if (listNghiPhep != null && listNghiPhep.Count > 0)
                                            NghiPhep = listNghiPhep.Where(a => a.IdNhanVien == NV.IdGuid && a.NgayNghi == date && a.MaLoaiNghi == "SANG").Sum(a => TGianLam * a.ThoiGianNghi);
                                        if (SoGioQuyDoi > 0 && NghiPhep > 0)
                                        {

                                            itemAdd.CongPhep += (double)(NghiPhep / CaLamViec.SoGioTrongNgay);
                                            SoGioQuyDoi -= NghiPhep;
                                        }
                                        if (SoGioQuyDoi > 0 && KH.GioVao.AddHours(NghiPhep) < itemAdd.TuGio.Value)
                                        {
                                            TimeSpan times = (itemAdd.TuGio.Value - KH.GioVao.AddHours(NghiPhep));

                                            itemAdd.SoGioDiMuon += times.Hours + ":" + times.Minutes;
                                            SoGioQuyDoi -= times.TotalHours;

                                        }

                                    }
                                    if (SoGioQuyDoi < 0)
                                        SoGioQuyDoi = 0;
                                    itemAdd.SoGioQuyDoi += SoGioQuyDoi;
                                    double SoCongQuyDoi = (double)(SoGioQuyDoi / CaLamViec.SoGioTrongNgay);

                                    if (Host == "PHT" && CaLamViec != null && /*CaLamViec.isCaGay == false &&*/ SoCongQuyDoi > 1 && KH.Loai != 1) // cÃ´ng chÃ­nh >1 thÃ¬ tÃ­nh vÃ o tÄƒng ca
                                    {
                                        itemAdd.CongTangCa += SoCongQuyDoi - 1;
                                        itemAdd.SoGioQuyDoi = CaLamViec.SoGioTrongNgay ?? 0;
                                        SoCongQuyDoi = 1;
                                    }
                                    itemAdd.SoCongQuyDoi += SoCongQuyDoi;

                                    if (KH.Loai == 1)
                                    {
                                        if (NgayLe != null)
                                            itemAdd.CongLe += SoCongQuyDoi * NgayLe.TyLeLuong / 100;
                                        else
                                            itemAdd.CongTangCa += SoCongQuyDoi;
                                    }
                                    else
                                    {
                                        if (NgayLe != null)
                                            itemAdd.CongLe += SoCongQuyDoi * NgayLe.TyLeLuong / 100;

                                        if (CaLamViec.isCaGay == null || CaLamViec.isCaGay == false)
                                            itemAdd.CongF += SoCongQuyDoi;
                                        else
                                            itemAdd.CongP += SoCongQuyDoi;
                                    }
                                }

                            }
                            if (itemAdd.DenGio != null && itemAdd.DenGio.Value.Year < 2000)
                                itemAdd.DenGio = null;
                            if (itemAdd.TuGio != null && itemAdd.TuGio.Value.Year < 2000)
                                itemAdd.TuGio = null;
                            i++;
                            listResult.Add(itemAdd);
                            NhanVien.listNgay.Add(itemAdd);
                        }
                    }
                    else if (LamViecNgay != null && LamViecNgay.Count > 0)
                    {
                        if (ListCheckIn != null && ListCheckIn.Count > 0)
                        {
                            foreach (var KH in LamViecNgay)
                            {
                                HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                                itemAdd.MaNhanVien = NV.MaNhanVien;
                                itemAdd.TenNhanVien = NV.TenDayDu;
                                itemAdd.TenLoaiNhanVien = LoaiNhanVien;
                                itemAdd.Ngay = date;
                                itemAdd.TenCongTy = NV.TenCongTy;
                                itemAdd.TenChucVu = NV.TenChucVu;
                                var CaLamViec = listCa.Where(a => a.IdGuid == KH.IddmCaLamViec).FirstOrDefault();
                                if (CaLamViec == null)
                                    CaLamViec = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).FirstOrDefault();
                                if (CaLamViec != null)
                                    itemAdd.CaLamViec = CaLamViec.Ten;
                                else
                                    CaLamViec = new HRM_dmLichLamViec();

                                if (CaLamViec == null || CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                    CaLamViec.SoGioTrongNgay = 8;

                                itemAdd.ThoiGianLamViec = KH.GioVao.ToString("HH:mm") + " - " + KH.GioRa.ToString("HH:mm");
                                //if ((ListCheckIn.Count - 1) < i)
                                //    continue;
                                //var checkIn = ListCheckIn[i];
                                var checkIn = ListCheckIn.Where(a => (a.GioVao <= KH.GioRa && a.GioRa >= KH.GioVao) || (a.GioVao < KH.GioRa && a.GioRa > KH.GioVao) || (a.GioVao < KH.GioVao && a.GioRa > KH.GioVao) || (a.GioVao < KH.GioRa && a.GioRa > KH.GioRa) || (a.GioRa.Year != KH.GioRa.Year || a.GioVao.Year != KH.GioVao.Year)).OrderByDescending(a => a.GioVao).FirstOrDefault();
                                if (checkIn != null)
                                {
                                    itemAdd.TuGio = KH.GioVaoTT;
                                    itemAdd.DenGio = KH.GioRaTT;
                                    if (itemAdd.DenGio != null && itemAdd.DenGio.Value.Year < 2000)
                                        itemAdd.DenGio = null;
                                    if (itemAdd.TuGio != null && itemAdd.TuGio.Value.Year < 2000)
                                        itemAdd.TuGio = null;

                                    if (itemAdd.TuGio == null && checkIn.GioVao != null && checkIn.GioVao.Year > 2020)
                                        itemAdd.TuGio = checkIn.GioVao;
                                    itemAdd.NoiChamTuGio = listDuAn.Where(a => a.Id == checkIn.IdCongTy).Select(a => a.TenBoPhan).FirstOrDefault();
                                    if (checkIn.GioRa != null && checkIn.GioRa.Year > 2020)
                                    {
                                        if (itemAdd.DenGio == null)
                                            itemAdd.DenGio = checkIn.GioRa;
                                        itemAdd.NoiChamDenGio = listDuAn.Where(a => a.Id == checkIn.IdCongTyVe).Select(a => a.TenBoPhan).FirstOrDefault();
                                    }
                                    else
                                    {
                                        itemAdd.SoGioQuyDoi = 0;
                                    }

                                    if (itemAdd.DenGio != null && itemAdd.TuGio != null)
                                    {
                                        itemAdd.SoGioQuyDoi = KH.TongSoGioGiao;
                                        if (Host == "PHT" && KH.Loai != 1 && (int)date.DayOfWeek == 6 && CaLamViec != null && CaLamViec.LoaiNgayNghi == 4)
                                            itemAdd.SoGioQuyDoi = CaLamViec.SoGioTrongNgay ?? 8;
                                        itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - (KH.SoGioNghi ?? 0);
                                    }
                                    if (itemAdd.SoGioQuyDoi > 0 && itemAdd.DenGio != null && itemAdd.DenGio < KH.GioRa && itemAdd.DenGio.Value.Year == KH.GioRa.Year)
                                    {

                                        TimeSpan times = (KH.GioRa - itemAdd.DenGio.Value);

                                        itemAdd.SoGioVeSom = times.Hours + ":" + times.Minutes;
                                        itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - times.TotalHours;
                                    }
                                    if (itemAdd.SoGioQuyDoi > 0 && itemAdd.TuGio != null && itemAdd.TuGio > KH.GioVao && itemAdd.TuGio.Value.Year == KH.GioVao.Year)
                                    {
                                        TimeSpan times = (itemAdd.TuGio.Value - KH.GioVao);

                                        itemAdd.SoGioDiMuon = times.Hours + ":" + times.Minutes;
                                        itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - times.TotalHours;

                                    }

                                    if (KH.GioVaoTiep < KH.GioRa)
                                    {
                                        var itemInOuts = ListCheckIn.Where(a => (a.GioVao < KH.GioRa && a.GioRa > KH.GioVao) || (a.GioVao < KH.GioVao && a.GioRa > KH.GioVao) || (a.GioVao < KH.GioRa && a.GioRa > KH.GioRa)).OrderBy(a => a.GioVao).ToList();

                                        //Ra ngoÃ i giá»¯a giá»
                                        foreach (var itemInOut in itemInOuts)
                                        {
                                            if (itemInOut.GioVaoTiep == null)
                                                continue;
                                            if (itemInOut.GioVaoTiep >= KH.GioRa)
                                                continue;
                                            if (itemInOut.GioRa >= KH.GioVao && itemInOut.GioRa <= KH.GioRa)
                                            {
                                                double SoGio = (itemInOut.GioVaoTiep.Value - itemInOut.GioRa).TotalHours;
                                                if (SoGio > 0)
                                                {
                                                    itemAdd.SoGioQuyDoi = itemAdd.SoGioQuyDoi - SoGio;
                                                }
                                            }
                                        }
                                    }

                                    itemAdd.SoCongQuyDoi = (double)(itemAdd.SoGioQuyDoi / CaLamViec.SoGioTrongNgay);
                                    if (itemAdd.SoCongQuyDoi < 0)
                                    {
                                        itemAdd.SoCongQuyDoi = 0;
                                        itemAdd.SoGioQuyDoi = 0;
                                    }
                                }

                                if (NghiPhep > 0)
                                {
                                    if (KH.TongSoGioGiao >= NghiPhep)
                                        itemAdd.CongPhep = (double)(NghiPhep / CaLamViec.SoGioTrongNgay);
                                    else
                                    {
                                        itemAdd.CongPhep = KH.TongSoGioGiao;
                                        NghiPhep = NghiPhep - KH.TongSoGioGiao;
                                    }
                                }
                                if (Host == "ICT" && CaLamViec != null /*&& CaLamViec.isCaGay == false */&& itemAdd.SoCongQuyDoi > 1 && KH.Loai != 1 && NgayLe == null) // cÃ´ng chÃ­nh >1 thÃ¬ tÃ­nh vÃ o tÄƒng ca
                                {
                                    itemAdd.CongTangCa += itemAdd.SoCongQuyDoi - 1;
                                    itemAdd.SoCongQuyDoi = 1;
                                    itemAdd.SoGioQuyDoi = CaLamViec.SoGioTrongNgay ?? 0;
                                }
                                if (KH.Loai == 1)
                                {
                                    if (NgayLe != null)
                                        itemAdd.CongLe = itemAdd.SoCongQuyDoi * NgayLe.TyLeLuong / 100;
                                    else
                                        itemAdd.CongTangCa = itemAdd.SoCongQuyDoi;
                                }
                                else
                                {
                                    if (NgayLe != null)
                                        itemAdd.CongLe += itemAdd.SoCongQuyDoi * NgayLe.TyLeLuong / 100;
                                    else
                                    if (CaLamViec.isCaGay == null || CaLamViec.isCaGay == false)
                                        itemAdd.CongF = itemAdd.SoCongQuyDoi;
                                    else
                                        itemAdd.CongP = itemAdd.SoCongQuyDoi;
                                }
                                i++;
                                listResult.Add(itemAdd);
                                NhanVien.listNgay.Add(itemAdd);
                            }
                        }
                        else
                        {
                            if (NgayLe != null)
                            {
                                HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                                itemAdd.MaNhanVien = NV.MaNhanVien;
                                itemAdd.TenNhanVien = NV.TenDayDu;
                                itemAdd.TenLoaiNhanVien = LoaiNhanVien;
                                itemAdd.Ngay = date;
                                itemAdd.TenCongTy = DuAn.TenBoPhan;
                                itemAdd.TenChucVu = NV.TenChucVu;
                                var listIdCa = LamViecNgay.Select(a => a.IddmCaLamViec).Distinct().ToList();
                                var CaLamViecs = listCa.Where(a => listIdCa.Contains(a.IdGuid)).ToList();
                                if (CaLamViecs == null)
                                    CaLamViecs = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).ToList();
                                itemAdd.CaLamViec = string.Join(", ", CaLamViecs.Select(a => a.Ten));

                                //if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                //    CaLamViec.SoGioTrongNgay = 8;
                                itemAdd.ThoiGianLamViec = string.Join(", ", LamViecNgay.Select(KH => KH.GioVao.ToString("HH:mm") + " - " + KH.GioRa.ToString("HH:mm")).ToList());
                                var CaLamViec = listCa.Where(a => a.IdGuid == LamViecNgay.FirstOrDefault().IddmCaLamViec).FirstOrDefault();
                                itemAdd.CongLe += 1;
                            }
                            else
                            {
                                if (listNghiPhep != null && listNghiPhep.Count > 0)
                                    NghiPhep = listNghiPhep.Where(a => a.IdNhanVien == NV.IdGuid && a.NgayNghi == date).Sum(a => TGianLam * a.ThoiGianNghi);
                                if (NghiPhep > 0)
                                {
                                    HRM_BangChamCong_NhanVien_Ngay itemAdd = new HRM_BangChamCong_NhanVien_Ngay();
                                    itemAdd.MaNhanVien = NV.MaNhanVien;
                                    itemAdd.TenNhanVien = NV.TenDayDu;
                                    itemAdd.TenLoaiNhanVien = LoaiNhanVien;
                                    itemAdd.Ngay = date;
                                    itemAdd.TenCongTy = DuAn.TenBoPhan;
                                    itemAdd.TenChucVu = NV.TenChucVu;
                                    var listIdCa = LamViecNgay.Select(a => a.IddmCaLamViec).Distinct().ToList();
                                    var CaLamViecs = listCa.Where(a => listIdCa.Contains(a.IdGuid)).ToList();
                                    if (CaLamViecs == null)
                                        CaLamViecs = listCa.Where(a => a.IdGuid == NV.IdCaLamViec).ToList();
                                    itemAdd.CaLamViec = string.Join(", ", CaLamViecs.Select(a => a.Ten));

                                    //if (CaLamViec.SoGioTrongNgay == null || CaLamViec.SoGioTrongNgay == 0)
                                    //    CaLamViec.SoGioTrongNgay = 8;
                                    itemAdd.ThoiGianLamViec = string.Join(", ", LamViecNgay.Select(KH => KH.GioVao.ToString("HH:mm") + " - " + KH.GioRa.ToString("HH:mm")).ToList());
                                    var CaLamViec = listCa.Where(a => a.IdGuid == LamViecNgay.FirstOrDefault().IddmCaLamViec).FirstOrDefault();
                                    double soGioTrongNgay = (CaLamViec != null && CaLamViec.SoGioTrongNgay > 0) ? CaLamViec.SoGioTrongNgay.Value : 8;
                                    itemAdd.CongPhep += (double)(NghiPhep / soGioTrongNgay);
                                    listResult.Add(itemAdd);
                                    NhanVien.listNgay.Add(itemAdd);
                                }
                            }

                        }
                    }

                }
                listResultNV.Add(NhanVien);
            }
            if (nCurrentPage > 0)
            {
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = listResultNV;
                return dnm;
            }
            else
                return listResult;
        }
        public dynamic ExportTheoDoiChamCong(ApplicationDbContext db, TimKiemChung timkiem)
        {
            try
            {
                string sFile = "BBC.xlsx";
                if (Host == "PHT")
                    sFile = "BBCPHT.xlsx";
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "TheoDoiChamCong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                timkiem.CurrentPage = 0;
                List<HRM_BangChamCong_NhanVien_Ngay> listData = GetThoiGianChamCong(timkiem, db);
                if (listData != null && listData.Count > 0)
                {
                    int nCol = 1;
                    int STT = 1;
                    int nRow = 2;
                    foreach (var item in listData)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.Ngay.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenLoaiNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenCongTy;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.CaLamViec;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThoiGianLamViec;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TuGio == null ? "" : item.TuGio.Value.ToString("HH:mm");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NoiChamTuGio;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.DenGio == null ? "" : item.DenGio.Value.ToString("HH:mm");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NoiChamDenGio;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoGioQuyDoi;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoCongQuyDoi;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;
                        if (Host == "PHT")
                        {
                            sheet1.Cells[nRow, nCol].Value = item.CongF;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.CongP;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            nCol++;



                        }
                        sheet1.Cells[nRow, nCol].Value = item.CongTangCa;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;
                        if (Host == "PHT")
                        {
                            sheet1.Cells[nRow, nCol].Value = item.CongPhep;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.CongLe;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            nCol++;



                        }
                        sheet1.Cells[nRow, nCol].Value = item.SoGioDiMuon;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoGioVeSom;
                        nRow++;
                        STT++;
                    }
                    nRow--;
                    DrawTableExcel(sheet1, 1, nRow, 1, nCol);
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.Font.Size = 12;
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[2, 4, nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra " + ex.ToString());
            }
        }
        public dynamic ExportBangChamCong(ApplicationDbContext db, List<HRM_BangChamCong_NhanVien> listNhanVien)
        {
            try
            {
                string sFile = "BangChamCongNhanVien.xlsx";
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "BangChamCongNhanVienThang.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                if (listNhanVien != null && listNhanVien.Count > 0)
                {

                    int nColHeader = 8;
                    int STT = 1;
                    int nRow = 4;
                    var data = listNhanVien.FirstOrDefault();
                    var NgayBatDau = data.listNgay.FirstOrDefault();
                    var NgayKT = data.listNgay.LastOrDefault();
                    var listNgay = data.listNgay;
                    long IdDuAn = data.IdCongTyEOS;
                    var DuAn = _CallWebService.GetDanhSachDuAn(db).FirstOrDefault(a => a.Id == IdDuAn);
                    foreach (var item in listNgay)
                    {
                        DateTime time = new DateTime(item.Nam, item.Thang, item.Ngay, 0, 0, 0);
                        sheet1.Cells[nRow, nColHeader].Value = item.Ngay + "/" + item.Thang + " - " + QuyDoiThuVT((int)time.DayOfWeek);
                        sheet1.Column(nColHeader).Width = 20;
                        nColHeader++;
                    }
                    nColHeader--;
                    sheet1.Cells[1, 1].Value = "Báº¢NG CHáº¤M CÃ”NG " + DuAn.TenBoPhan.ToUpper();
                    sheet1.Cells[1, 1, 1, nColHeader].Merge = true;
                    sheet1.Cells[2, 1].Value = NgayBatDau.Ngay + "/" + NgayBatDau.Thang + "/" + NgayBatDau.Nam + " - " + NgayKT.Ngay + "/" + NgayKT.Thang + "/" + NgayKT.Nam;
                    sheet1.Cells[2, 1, 2, nColHeader].Merge = true;

                    nRow++;
                    int nCol = 1;
                    var listBoPhan = listNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                    foreach (var Bp in listBoPhan)
                    {
                        var listNhanViens = listNhanVien.Where(a => a.TenBoPhan == Bp).ToList();
                        sheet1.Cells[nRow, 1].Value = Bp;
                        sheet1.Cells[nRow, 1, nRow, 3].Merge = true;
                        sheet1.Cells[nRow, 1, nRow, 10].Style.Font.Bold = true;
                        sheet1.Cells[nRow, 1, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        nCol = 4;
                        if (Host == "ICT")
                            sheet1.Cells[nRow, nCol].Value = listNhanViens.Sum(a => a.TongSoCong);
                        else if (Host == "PHT")
                            sheet1.Cells[nRow, nCol].Value = listNhanViens.Sum(a => a.TongSoCong) + "(" + Math.Round(listNhanViens.Sum(a => a.listNgay.Sum(b => (b.SoGioLamViecCaGay ?? 0) / (b.SoGioTieuChuan ?? 8))), 2) + "P)";
                        if (Host == "ICT")
                        {
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = listNhanViens.Sum(a => a.TongSoCongTangCa);
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = listNhanViens.Sum(a => a.TongCongPhep);
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                            nCol++;
                            if (Host == "ICT")
                                sheet1.Cells[nRow, nCol].Value = listNhanViens.Sum(a => double.Parse(a.TongSoCong300));
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                            nCol++;
                            foreach (var item in data.listNgay)
                            {
                                sheet1.Cells[nRow, nCol].Value = Math.Round(listNhanViens.Sum(a => a.listNgay.Where(x => x.Ngay == item.Ngay && x.Thang == item.Thang).Sum(b => ((b.SoGioLamViec ?? 0) + (b.SoGioLamViecCaGay ?? 0)))), 2);
                                nCol++;
                            }

                        }
                        nRow++;

                        foreach (var item in listNhanViens)
                        {
                            nCol = 1;
                            sheet1.Cells[nRow, nCol].Value = STT;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                            nCol++;
                            if (Host == "ICT")
                                sheet1.Cells[nRow, nCol].Value = item.TongSoCong;
                            else
                                sheet1.Cells[nRow, nCol].Value = item.TongSoCongChu;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongSoCongTangCa;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongCongPhep;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongSoCong300;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;


                            nCol++;
                            foreach (var Ngay in item.listNgay)
                            {
                                sheet1.Cells[nRow, nCol].Value = Ngay.GioLamViec;
                                nCol++;
                            }
                            nRow++;
                            STT++;
                        }
                    }
                    nRow--;
                    DrawTableExcel(sheet1, 4, nRow, 1, nColHeader);
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.Font.Size = 12;
                    sheet1.Cells.Style.WrapText = true;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[5, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }

        public dynamic GetChamCongForGiamSat(double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db, List<string> listIdUser = null)
        {
            try
            {

                List<HRM_BangChamCong_NhanVien> listResult = GetChamCongForGiamSat_ChiTiet(TuNgayUnix, DenNgayUnix, db, listIdUser);
                List<HRM_BangChamCong_NhanVienViewModel> listBoPhan = new List<HRM_BangChamCong_NhanVienViewModel>();
                if (listResult.Count > 0)
                {
                    List<string> listBP = listResult.Select(a => a.TenBoPhan).Distinct().ToList();
                    listBoPhan = listBP.Select(a => new HRM_BangChamCong_NhanVienViewModel() { TenBoPhan = a, listNhanVien = new List<HRM_BangChamCong_NhanVien>() }).OrderBy(a => a.TenBoPhan).ToList();
                    foreach (var item in listBoPhan)
                        item.listNhanVien = listResult.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();
                }

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import thÃ nh cÃ´ng file cháº¥m cÃ´ng", listBoPhan);
            }

            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + ex.ToString());
            }

        }
        public List<HRM_BangChamCong_NhanVien> GetChamCongForGiamSat_ChiTiet(double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db, List<string> listIdUser = null)
        {
            List<HRM_BangChamCong_NhanVien> listResult = new List<HRM_BangChamCong_NhanVien>();

            try
            {
                var chucVuGS = _CallWebService.GetDanhSachChucVu(db).Where(a => a.MaChucVu == "GS").FirstOrDefault();
                //var listUserDuAn = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongTy, TuNgayUnix, DenNgayUnix, db);
                //List<string> listIdUser = new List<string>();
                //if (listUserDuAn != null && listUserDuAn.Count > 0)
                //    listIdUser = listUserDuAn.Where(a => !string.IsNullOrEmpty(a.IdUser)).Select(a => a.IdUser).Distinct().ToList();

                DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix);
                DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix);
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => x.IdChucVu == chucVuGS.Id).OrderByDescending(a => a.NgayBatDauLamViec).ToList();
                if (listIdUser != null && listIdUser.Count > 0)
                    listNhanVien = listNhanVien.Where(x => listIdUser.Contains(x.IdUser)).ToList();
                listIdUser = listNhanVien.Select(a => a.IdUser).Distinct().ToList();
                var listUserDuAn = _CallWebService.GetDanhSachUserTheoListIdUser(db, listIdUser);
                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_CheckInOut> listCheckInOut = (from x in db.HRM_CheckInOut.AsNoTracking()
                                                       where listIdNhanVien.Contains(x.IdNhanVien)/* && x.Loai == DefineData.eKyHieuChamCong.I.ToString()*/
                                                                     && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay
                                                       select x
                                                       //group x by new { x.IdCongTy, x.IdNhanVien, x.Loai, x.Gio.Year, x.Gio.Month, x.Gio.Day } into g
                                                       //select g.OrderBy(a => a.Gio).ToList()/*.FirstOrDefault()*/
                                                       ).ToList();
                listIdNhanVien = listCheckInOut.Select(x => x.IdNhanVien).ToList();

                listNhanVien = listNhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).ToList();

                var listIdCongTy = new List<long>();
                if (listLichLamViec != null && listLichLamViec.Count > 0)
                    listIdCongTy = listLichLamViec.Select(a => a.IdCongTy).Distinct().ToList();
                List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom = db.HRM_TheoDoiDiMuonVeSom.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_TheoDoiLamThemGio> listLamThemGio = db.HRM_TheoDoiLamThemGio.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                               && x.NgayDangKy >= dTuNgay && x.NgayDangKy <= dDenNgay).ToList();
                List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs = db.HRM_dmKyHieuChamCong.ToList();
                List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecNV = db.HRM_KeHoachLamViecTuan_NhanVien_ThoiGian.Where(x => x.isTheoDoi == true && listIdNhanVien.Contains(x.IdNhanVien)
                                                                                      && x.Ngay >= dTuNgay && x.Ngay <= dDenNgay).ToList();
                List<HRM_TheoDoiXinNghiPhep> listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                                                                              && x.NgayNghi >= dTuNgay && x.NgayNghi <= dDenNgay).ToList();
                var listHinhThucNghi = _CallWebService.GetListDanhMucHinhThucNghi(db);
                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                {
                    foreach (var item in listXinNghiPhep)
                    {
                        var hinhThucNghi = listHinhThucNghi.FirstOrDefault(a => a.Id == item.IdHinhThucNghi);
                        if (hinhThucNghi != null)
                            item.HinhThucNghi = hinhThucNghi.Ma;
                    }
                }
                var listCaLamViecs = db.HRM_dmLichLamViec.ToList();

                double TongT7 = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Saturday);
                double TongCN = Enumerable.Range(0, (dDenNgay - dTuNgay).Days + 1).Select(offset => dTuNgay.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Sunday);
                int TongSoNgay = (dDenNgay - dTuNgay).Days + 1;
                double TongSoCongKhac = (((int)(TongSoNgay / 6)) + (int)((TongSoNgay % 6) / 4)) * 4 + (TongSoNgay % 6) % 4;
                foreach (var item in listCaLamViecs)
                {
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongT7 - TongCN);
                    if (item.LoaiNgayNghi == 2)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongCN);
                    if (item.LoaiNgayNghi == 1)
                        item.SoNgayCongTieuChuan = (int)(TongSoNgay - TongSoCongKhac);
                }


                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                List<HRM_BangChamCongNhanVien_TangCa> listDataTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                List<HRM_BangChamCongNhanVien_SoGioLamViec> listDataTieuChuan = new List<HRM_BangChamCongNhanVien_SoGioLamViec>();
                var funcKeHoach = _funcKeHoach;
                List<double> listHeSo = listLichLamViec.Where(a => a.Loai == 1).Select(a => a.HeSoTangCa).Distinct().ToList();
                List<double> listCaTieuChuan = listLichLamViec.Select(a => a.GioTieuChuan).Distinct().ToList();
                string VeSom = HrmOptions.SoPhutSom.ToString();
                double SoPhutDuocVeSom = double.Parse(VeSom);
                string DiMuon = HrmOptions.SoPhutMuon.ToString();
                double SoPhutDuocDiMuon = double.Parse(DiMuon);

                DateTime dateNow = DateTime.Now;
                dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 0, 0, 0);
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "0249")
                        item.MaNhanVien = "0249";
                    var NhanSuDA = listUserDuAn.FirstOrDefault(a => a.Id == item.IdUser);
                    DateTime dt = dTuNgay;
                    if (listHeSo != null && listHeSo.Count > 0)
                    {
                        foreach (var heso in listHeSo)
                        {
                            HRM_BangChamCongNhanVien_TangCa hesoAdd = new HRM_BangChamCongNhanVien_TangCa();
                            hesoAdd.HeSo = heso;
                            hesoAdd.IdNhanVien = item.IdGuid;
                            var tangca = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.HeSoTangCa == heso && a.Loai == 1).ToList();
                            if (tangca != null && tangca.Count > 0)
                            {
                                hesoAdd.TongSoGio = 0;
                                //hesoAdd.TongSoGio = tangca.Sum(a => (a.GioRa - a.GioVao).TotalHours);
                                hesoAdd.Cong = 0;
                                //hesoAdd.Cong = tangca.Sum(a => ((a.GioRa - a.GioVao).TotalHours) / (a.GioTieuChuan == 0 ? 8 : a.GioTieuChuan));
                                hesoAdd.IddmHeSoTangCa = tangca.FirstOrDefault().IddmHeSoTangCa;
                            }
                            listDataTangCa.Add(hesoAdd);
                        }
                    }
                    if (listCaTieuChuan != null && listCaTieuChuan.Count > 0)
                    {
                        foreach (var Id in listCaTieuChuan)
                        {
                            string IdCaLamViec = listLichLamViec.Where(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == Id && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();

                            HRM_BangChamCongNhanVien_SoGioLamViec hesoAdd = new HRM_BangChamCongNhanVien_SoGioLamViec();
                            var caLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);
                            if (caLamViec != null)
                            {
                                hesoAdd.IddmCaLamViec = caLamViec.IdGuid;
                                hesoAdd.IdNhanVien = item.IdGuid;
                                hesoAdd.GioTieuChuan = caLamViec.SoGioTrongNgay ?? 8;
                                hesoAdd.CongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 26;
                                hesoAdd.TongSoGio = 0;
                                listDataTieuChuan.Add(hesoAdd);
                            }
                        }
                    }

                    while (dt <= dDenNgay)
                    {
                        List<HRM_TheoDoiXinNghiPhep> NghiPheps = listXinNghiPhep.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayNghi == dt);
                        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSoms = listDiMuonVeSom.FindAll(x => x.IdNhanVien == item.IdGuid && x.NgayDangKy == dt);
                        if (dt >= dateNow)
                        {
                            dt = dt.AddDays(1);
                            continue;
                        }
                        foreach (long IdCongTy in listIdCongTy)
                        {
                            List<HRM_CheckInOut> listCheckInOutFind = listCheckInOut.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt /*&& x.IdCongTy == IdCongTy*/).OrderBy(a => a.Ngay).ToList();
                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt && x.IdCongTy == IdCongTy);

                            if (listCheckInOutFind != null && listCheckInOutFind.Count > 1)
                            {
                                //if (listCheckInOutFind.LastOrDefault().Loai == "I")
                                //{
                                //    DateTime dta = dt.AddDays(1);
                                //    listCheckInOutFind.Add(listCheckInOut.FindAll(x => x.Loai == "O" && x.IdNhanVien == item.IdGuid && x.Ngay == dta).OrderBy(a => a.Ngay).FirstOrDefault());
                                //}

                                if (listLichLamViecFind == null)
                                    continue;
                                string IdCaLamViec = listLichLamViecFind.Where(a => a.IdCongTy == IdCongTy).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == IdCaLamViec);

                                List<clsDiMuonVeSom> listSomMuon = funcKeHoach.TinhSomMuon(db, item.IdGuid, dmKyHieuChamCongs, listDiMuonVeSoms, listLichLamViecFind, listCheckInOutFind, dt, SoPhutDuocDiMuon, SoPhutDuocVeSom, "", NghiPheps, false, IdCongTy);
                                List<HRM_KeHoachLamViecTuan_NhanVien_ThoiGian> listLichLamViecFindNV = listLichLamViecNV.FindAll(x => x.IdNhanVien == item.IdGuid && x.Ngay == dt);
                                if (listLichLamViecFind.Count >= 1)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    bool TangCaDem = false;
                                    foreach (var KeHoach in listLichLamViecFind)
                                    {

                                        if (KeHoach.IdCongTy != IdCongTy)
                                            continue;
                                        double SoGioLamViec = KeHoach.TongSoGioGiao - (KeHoach.SoGioNghi ?? 0);
                                        if (KeHoach.Loai == 0)
                                        {
                                            itemAdd.SoGioLamViec += SoGioLamViec;
                                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == KeHoach.GioTieuChuan);
                                                if (tangCa != null)
                                                    tangCa.TongSoGio += SoGioLamViec;

                                            }
                                        }
                                        else
                                        {
                                            itemAdd.SoGioLamViecTangCa += SoGioLamViec;
                                            if (KeHoach.GioVao.Hour >= 21)
                                            {
                                                TangCaDem = true;

                                            }
                                            if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || KeHoach.IdCongTy == IdCongTy))
                                            {
                                                var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == KeHoach.IddmHeSoTangCa);
                                                if (tangCa != null)
                                                {
                                                    tangCa.TongSoGio += SoGioLamViec;
                                                    tangCa.Cong += SoGioLamViec / (tangCa.GioTieuChuan == 0 ? 8 : tangCa.GioTieuChuan);
                                                }
                                            }
                                        }

                                    }
                                    List<string> listKyHieu = new List<string>();
                                    if (listSomMuon != null && listSomMuon.Count > 0)
                                        foreach (var SomMuon in listSomMuon)
                                        {
                                            //itemAdd.GioLamViec += "; ";
                                            double Gio = SomMuon.SoPhut / 60.0;
                                            if (SomMuon.Loai == 0)
                                            {
                                                //if (SomMuon.isCaGay == true)
                                                //    itemAdd.SoGioLamViecCaGay -= Gio;
                                                //else
                                                itemAdd.SoGioLamViec -= Gio;
                                                if (listDataTieuChuan != null && listDataTieuChuan.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                                {
                                                    var tangCa = listDataTieuChuan.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.GioTieuChuan == SomMuon.GioTieuChuan);
                                                    if (tangCa != null)
                                                        tangCa.TongSoGio -= Gio;
                                                    if (tangCa.TongSoGio < 0)
                                                        tangCa.TongSoGio = 0;
                                                }
                                            }
                                            else
                                            {
                                                itemAdd.SoGioLamViecTangCa -= Gio;
                                                if (listDataTangCa != null && listDataTangCa.Count > 0 && (IdCongTy == 0 || SomMuon.IdCongTy == IdCongTy))
                                                {
                                                    var tangCa = listDataTangCa.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IddmHeSoTangCa == SomMuon.IddmHeSoTangCa);
                                                    if (tangCa != null)
                                                        tangCa.TongSoGio -= Gio;
                                                }
                                            }
                                            listKyHieu.Add(SomMuon.KyHieuChamCong);
                                        }
                                    double GioLamViec = (double)(itemAdd.SoGioLamViecCaGay + itemAdd.SoGioLamViec);
                                    string KyHieu = "";
                                    if (listKyHieu != null && listKyHieu.Count > 0)
                                        KyHieu += string.Join("", listKyHieu);
                                    if (GioLamViec > 0)
                                        KyHieu += Math.Round(GioLamViec, 2);
                                    if (CaLamViec != null && CaLamViec.TuGio.Hour >= 21)
                                    {
                                        itemAdd.isCaDem = true;
                                    }
                                    if (itemAdd.SoGioLamViecTangCa > 0)
                                    {
                                        if (TangCaDem == false)
                                        {
                                            KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "";
                                        }
                                        else
                                        {
                                            if (listLichLamViecFind.Count > 1)
                                                KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                            else if (listLichLamViecFind.Count == 1)
                                            {
                                                if (GioLamViec > 0)
                                                    KyHieu += "." + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                                else
                                                    KyHieu += DefineData.eKyHieuChamCong.T.ToString() + Math.Round(itemAdd.SoGioLamViecTangCa ?? 0, 2) + "Ä";
                                            }
                                        }
                                    }
                                    itemAdd.GioLamViec = KyHieu;

                                    listData.Add(itemAdd);
                                }
                            }
                            else if (NghiPheps.Count > 0)
                            {
                                //if (listLichLamViecFind == null)
                                //    listLichLamViecFind = new List<HRM_ThoiGianLamViecKeHoach>();


                                HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == NghiPheps.FirstOrDefault().HinhThucNghi);
                                if (NghiPhep != null)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    //double SoGioLamViec = 0;
                                    //foreach (var KeHoach in listLichLamViecFind)
                                    //{
                                    //    SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;

                                    //}
                                    itemAdd.GioLamViec = NghiPhep.Ma /*+ SoGioLamViec*/;
                                    listData.Add(itemAdd);
                                }
                            }
                            else if ((int)dt.DayOfWeek != 0 && listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                            {
                                HRM_dmKyHieuChamCong NghiPhep = dmKyHieuChamCongs.Find(x => x.Ma == DefineData.eKyHieuChamCong.NTD.ToString());
                                if (NghiPhep != null)
                                {
                                    var itemAdd = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    itemAdd.IdNhanVien = item.IdGuid;
                                    itemAdd.MaNhanVien = item.MaNhanVien;
                                    itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;
                                    itemAdd.SoGioLamViec = 0;
                                    itemAdd.SoGioLamViecCaGay = 0;
                                    itemAdd.SoGioLamViecTangCa = 0;
                                    itemAdd.NgayChamCong = dt;
                                    itemAdd.IdCongTyEOS = IdCongTy;
                                    itemAdd.NgayChamCongUnix = dungchung.DateTimeToUnixTimeStamp(dt);
                                    double SoGioLamViec = 0;
                                    foreach (var KeHoach in listLichLamViecFind)
                                    {
                                        SoGioLamViec += (KeHoach.GioRa - KeHoach.GioVao).TotalHours;
                                    }
                                    itemAdd.GioLamViec = NghiPhep.Ma;
                                    listData.Add(itemAdd);
                                }
                            }
                        }
                        dt = dt.AddDays(1);
                    }

                }
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                var listTangCa = db.HRM_TongHopXacNhanNgoaiGio.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TuNgay <= dTuNgay && a.DenNgay <= dDenNgay).OrderByDescending(a => a.Modified).ToList();
                if (listData != null && listData.Count > 0)
                {

                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    List<long> IdNhanVienEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => ((a.TuNgay.Month <= dTuNgay.Month) || a.DenNgay.Month >= dTuNgay.Month || a.TuNgay.Month <= dDenNgay.Month || a.DenNgay.Month >= dDenNgay.Month)).ToList();
                    var listIdHinhThucNghi = listHinhThucNghi.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString()).Select(a => a.Id).ToList();
                    foreach (var item in listNhanVien)
                    {
                        {
                            var check = listData.Where(a => a.IdNhanVien == item.IdGuid).FirstOrDefault();
                            if (check == null)
                                continue;
                        }
                        if (item.MaNhanVien == "0249")
                            item.MaNhanVien = "0249";
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        HRM_BangChamCong_NhanVien itemAdd = new HRM_BangChamCong_NhanVien();
                        itemAdd.TongSoCong200 = 0;
                        itemAdd.TongSoCong = 0;
                        itemAdd.isDieuDong = false;
                        //itemAdd.isTruyThu = !itemAdd.isDieuDong;
                        itemAdd.TongSoCong150 = 0;
                        //itemAdd.TongSoCong300 = 0;
                        itemAdd.TongCongPhep = listXinNghiPhep.Where(x => x.IdNhanVien == item.IdGuid && listIdHinhThucNghi.Contains(x.IdHinhThucNghi)).Sum(a => a.ThoiGianNghi);
                        double? CongLe = 0;


                        itemAdd.listNgay = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
                        DateTime time = dTuNgay;
                        var listNgay = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayChamCong).ToList();
                        double Cong300 = 0;
                        var listTangCaHT = listTangCa.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        if (listTangCaHT != null && listTangCaHT.Count > 0)
                        {
                            foreach (var TC in listTangCaHT)
                            {
                                if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                                {
                                    var TangCa = itemAdd.listTangCa.FirstOrDefault(a => a.HeSo == TC.HeSo);
                                    if (TangCa != null)
                                    {
                                        TangCa.TongSoGio += TC.SoGio;
                                        TangCa.Cong += (TC.SoGio / TC.GioTieuChuan);
                                    }
                                    else
                                        itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                        {
                                            IdNhanVien = item.IdGuid,
                                            TongSoGio = TC.SoGio,
                                            Cong = (TC.SoGio / TC.GioTieuChuan),
                                            CongTieuChuan = TC.CongTieuChuan,
                                            HeSo = TC.HeSo
                                        });
                                }
                                else
                                {
                                    itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                    itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                    {
                                        IdNhanVien = item.IdGuid,
                                        TongSoGio = TC.SoGio,
                                        Cong = (TC.SoGio / TC.GioTieuChuan),
                                        CongTieuChuan = TC.CongTieuChuan,
                                        HeSo = TC.HeSo
                                    });

                                }
                                if (itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)
                                {
                                    var GTC = itemAdd.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == TC.GioTieuChuan);
                                    if (GTC != null)
                                    {
                                        GTC.TongSoGio -= TC.SoGio;

                                    }
                                }
                            }
                        }
                        else
                        {

                            //if (/*(DuAn.CachTinhNgoaiGio =="TheoNgay") || */(/*DuAn.CachTinhNgoaiGio == "TheoHopDongLaoDong" &&*/ HopDong.LoaiLuong == 1))
                            //{
                            if (listDataTangCa != null && listDataTangCa.Count > 0)
                                itemAdd.listTangCa = listDataTangCa.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.HeSo).ToList();
                            if (listDataTieuChuan != null && listDataTieuChuan.Count > 0)
                                itemAdd.listGioTieuChuan = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.GioTieuChuan).ToList();
                            if (HopDong != null && HopDong.LoaiLuong == 1 && itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)

                            //if ()
                            {
                                double tongSoGio = itemAdd.listGioTieuChuan.Sum(a => a.TongSoGio);
                                var Gio = itemAdd.listGioTieuChuan.Where(a => a.TongSoGio > 0).FirstOrDefault();
                                if (Gio != null)
                                {
                                    double GioTC = Gio.GioTieuChuan * Gio.CongTieuChuan;
                                    if (tongSoGio > GioTC)
                                    {
                                        //itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                        //itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                        //{
                                        //    IdNhanVien = item.IdGuid,
                                        //    TongSoGio = tongSoGio - GioTC,
                                        //    Cong = ((tongSoGio - GioTC) / Gio.GioTieuChuan),
                                        //    CongTieuChuan = Gio.CongTieuChuan,
                                        //    HeSo = 150
                                        //});

                                        {
                                            double GioChuyenTC = tongSoGio - GioTC;
                                            if (itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)
                                            {
                                                var GTC = itemAdd.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == Gio.GioTieuChuan);
                                                GTC.TongSoGio -= GioChuyenTC;
                                            }
                                            double heso = 150;
                                            if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                                            {
                                                var TC = itemAdd.listTangCa.FirstOrDefault(a => a.HeSo == heso);
                                                if (TC != null)
                                                {
                                                    TC.TongSoGio += GioChuyenTC;
                                                    TC.Cong += GioChuyenTC / (Gio.GioTieuChuan);
                                                }
                                                else
                                                {
                                                    itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                                    {
                                                        IdNhanVien = item.IdGuid,
                                                        TongSoGio = GioChuyenTC,
                                                        Cong = ((GioChuyenTC) / (Gio.GioTieuChuan)),
                                                        CongTieuChuan = Gio.CongTieuChuan,
                                                        HeSo = heso
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                                itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                                {
                                                    IdNhanVien = item.IdGuid,
                                                    TongSoGio = GioChuyenTC,
                                                    Cong = ((GioChuyenTC) / (Gio.GioTieuChuan)),
                                                    CongTieuChuan = Gio.CongTieuChuan,
                                                    HeSo = heso
                                                });
                                            }

                                        }
                                    }
                                }
                            }
                            //}
                            //else
                            //{
                            //    if (itemAdd.listGioTieuChuan != null && itemAdd.listGioTieuChuan.Count > 0)
                            //    {
                            //        double tongSoGio = itemAdd.listGioTieuChuan.Sum(a => a.TongSoGio);
                            //        var Gio = itemAdd.listGioTieuChuan.FirstOrDefault();
                            //        double GioTC = Gio.GioTieuChuan * Gio.CongTieuChuan;
                            //        if (tongSoGio > GioTC)
                            //        {
                            //            itemAdd.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                            //            itemAdd.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                            //            {
                            //                IdNhanVien = item.IdGuid,
                            //                TongSoGio = tongSoGio - GioTC,
                            //                Cong = ((tongSoGio - GioTC) / Gio.GioTieuChuan),
                            //                CongTieuChuan = Gio.CongTieuChuan,
                            //                HeSo = 150
                            //            });
                            //        }
                            //    }
                            //    else
                            //    {
                            //        if (listDataTangCa != null && listDataTangCa.Count > 0)
                            //            itemAdd.listTangCa = listDataTangCa.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.HeSo).ToList();
                            //        if (listDataTieuChuan != null && listDataTieuChuan.Count > 0)
                            //            itemAdd.listGioTieuChuan = listDataTieuChuan.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.GioTieuChuan).ToList();
                            //    }
                            //}
                        }
                        {
                            while (time <= dDenNgay)
                            {
                                var Ngay = listNgay.FirstOrDefault(a => a.NgayChamCong == time);

                                if (Ngay != null)
                                {
                                    Ngay.Ngay = time.Day;
                                    var checkLe = listLichNghiChiTiet.FirstOrDefault(a => a.TuNgay <= time && a.DenNgay >= time);
                                    if (checkLe != null)
                                    {
                                        CongLe += (Ngay.SoGioLamViecTangCa ?? 0) + (Ngay.SoGioLamViec ?? 0);
                                        Cong300 += (Ngay.SoGioLamViecTangCa ?? 0);
                                        Ngay.isNgayLe = true;
                                    }
                                    else
                                    if ((int)time.DayOfWeek == 0)
                                    {
                                        itemAdd.TongSoCong200 += (Ngay.SoGioLamViecTangCa ?? 0);
                                        Ngay.isNgayLe = true;
                                    }
                                    itemAdd.listNgay.Add(Ngay);
                                }
                                else
                                {
                                    HRM_BangChamCong_NhanVien_ThoiGian Add = new HRM_BangChamCong_NhanVien_ThoiGian();
                                    Add.isFake = true;
                                    Add.NgayChamCong = time;
                                    itemAdd.listNgay.Add(Add);
                                }
                                time = time.AddDays(1);
                            }
                        }
                        itemAdd.TongSoCong150 = listNgay.Sum(a => a.SoGioLamViecTangCa) - itemAdd.TongSoCong200 - Cong300;
                        var CaLamViec = listCaLamViecs.FirstOrDefault(a => a.IdGuid == item.IdCaLamViec);
                        if (CaLamViec == null)
                            CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };
                        itemAdd.TongSoCongDem = (listNgay.Where(a => a.isCaDem == true).Sum(a => a.SoGioLamViec)) / CaLamViec.SoGioTrongNgay;
                        itemAdd.TongSoCong = (listNgay.Sum(a => a.SoGioLamViec));
                        if (itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                        {
                            itemAdd.TongSoCong -= itemAdd.listTangCa.Sum(a => a.TongSoGio);
                        }
                        itemAdd.TongSoCong = itemAdd.TongSoCong / CaLamViec.SoGioTrongNgay;
                        itemAdd.TongSoCong300 = Math.Round(CongLe ?? 0, 2).ToString();
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.IdBoPhanEOS = item.IdBoPhanEOS;

                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                            if (BoPhan.MaBoPhan != "CN" && itemAdd.listTangCa != null && itemAdd.listTangCa.Count > 0)
                                itemAdd.TongSoCong += (itemAdd.listTangCa.Where(a => a.HeSo == 100).Sum(a => a.TongSoGio)) / CaLamViec.SoGioTrongNgay;
                        }
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (chucVu != null)
                            itemAdd.TenChucVu = chucVu.TenChucVu;
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                            itemAdd.TenChucDanh = ChucDanh.Ten;

                        listResult.Add(itemAdd);
                    }
                }
                var listHeSoTangCa = listResult.Where(a => a.listTangCa != null && a.listTangCa.Count > 0).SelectMany(a => a.listTangCa.Select(x => x.HeSo)).Distinct().ToList();
                if (listHeSoTangCa != null && listHeSoTangCa.Count > 0)
                {
                    foreach (var item in listResult)
                    {
                        foreach (var heso in listHeSoTangCa)
                        {
                            if (item.listTangCa != null && item.listTangCa.Count > 0)
                            {
                                var Tc = item.listTangCa.FirstOrDefault(a => a.HeSo == heso);
                                if (Tc == null)
                                    item.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                    {
                                        IdNhanVien = item.IdGuid,
                                        TongSoGio = 0,
                                        Cong = 0,
                                        CongTieuChuan = 0,
                                        HeSo = heso
                                    });
                            }
                            else
                            {
                                item.listTangCa = new List<HRM_BangChamCongNhanVien_TangCa>();
                                item.listTangCa.Add(new HRM_BangChamCongNhanVien_TangCa
                                {
                                    IdNhanVien = item.IdGuid,
                                    TongSoGio = 0,
                                    Cong = 0,
                                    CongTieuChuan = 0,
                                    HeSo = heso
                                });
                            }
                        }
                    }
                }
                return listResult.ToList();
            }

            catch (Exception ex)
            {
                return listResult;
            }

        }

        public dynamic GetTongHopLichChamCong(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                ThoiDoiLichLamViecNhanVienViewModel result = new ThoiDoiLichLamViecNhanVienViewModel();
                if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                {
                    listNhanVien = db.HRM_NhanVien.Where(b => b.TenDayDu.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower())).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "KhÃ´ng tá»“n táº¡i nhÃ¢n viÃªn");
                }
                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = (from a in db.HRM_BangChamCong_NhanVien_ThoiGian
                                                                     where a.isKetThuc == true
                                                                      && (string.IsNullOrEmpty(TimKiem.KeyWord) || (!string.IsNullOrEmpty(TimKiem.KeyWord) && listIdNhanVien.Contains(a.IdNhanVien)))
                                                                      && (TimKiem.IdCongTyEOS == 0 || (TimKiem.IdCongTyEOS == a.IdCongTyEOS))
                                                                      && (TimKiem.IdBoPhanEOS == 0 || (TimKiem.IdBoPhanEOS == a.IdBoPhanEOS))

                                                                     select a
                                                                      ).ToList();
                if (TimKiem.TuNgay > 0)
                    listData = listData.Where(a => dtTuNgay <= a.NgayChamCong).ToList();
                if (TimKiem.DenNgay > 0)
                    listData = listData.Where(a => dtDenNgay >= a.NgayChamCong).ToList();

                if (listData != null && listData.Count > 0)
                {

                    result.listLabel = new List<ThoiDoiLichLamViecNhanVien_Label>();
                    DateTime time = dtTuNgay;
                    while (time <= dtDenNgay)
                    {
                        ThoiDoiLichLamViecNhanVien_Label label = new ThoiDoiLichLamViecNhanVien_Label();
                        label.Day = time.Day;
                        label.label = time.Day.ToString();
                        int thu = DefineData.QuyDoiThuSangSo(time.DayOfWeek.ToString());
                        label.Thu = DefineData.QuyDoiThuVT(thu);
                        result.listLabel.Add(label);
                        time = time.AddDays(1);
                    }
                    if (listData != null && listData.Count > 0)
                    {

                        result.listNhanVien = new List<HRM_KeHoachLamViecTuan_NhanVienViewModel>();
                        if (listNhanVien == null || listNhanVien.Count == 0)
                        {
                            listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                            listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                        }
                        List<long> listIdNVEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                        var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                        var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                        var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                        //List<Link_NhanVien_BoPhan_ChucDanh> listChucDanh = callWeb.GetDanhSachChucDanhTheoListIdNhanVien(db, listIdNVEOS);
                        foreach (var item in listIdNhanVien)
                        {
                            List<HRM_BangChamCong_NhanVien_ThoiGian> nhanviens = listData.Where(a => a.IdNhanVien == item).OrderBy(a => a.Ngay).ToList();

                            HRM_KeHoachLamViecTuan_NhanVienViewModel NhanVienDay = new HRM_KeHoachLamViecTuan_NhanVienViewModel();
                            var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item);
                            if (NV != null)
                            {
                                NhanVienDay.MaNhanVien = NV.MaNhanVien;
                                NhanVienDay.TenNhanVien = NV.TenDayDu;
                                NhanVienDay.IdNhanVien = item;
                                if (listChucVu != null && listChucVu.Count > 0)
                                {
                                    var chucVu = listChucVu.FirstOrDefault(a => a.Id == NV.IdChucVu);
                                    if (chucVu != null)
                                        NhanVienDay.ChucVu = chucVu.TenChucVu;
                                }
                                if (listBoPhan != null && listBoPhan.Count > 0)
                                {
                                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NV.IdBoPhanEOS);
                                    if (BoPhan != null)
                                        NhanVienDay.TenBoPhan = BoPhan.TenBoPhan;
                                }

                            }
                            NhanVienDay.listWeek = new List<HRM_KeHoachLamViecTuan_ChiTietViewModel>();
                            time = dtTuNgay;
                            while (time <= dtDenNgay)
                            {
                                var nhanvien = nhanviens.Where(a => a.NgayChamCong == time).OrderByDescending(a => a.Modified).FirstOrDefault();
                                if (nhanvien != null)
                                {
                                    HRM_KeHoachLamViecTuan_ChiTietViewModel day = new HRM_KeHoachLamViecTuan_ChiTietViewModel(nhanvien);

                                    NhanVienDay.listWeek.Add(day);
                                }
                                else
                                {
                                    HRM_KeHoachLamViecTuan_ChiTietViewModel day = new HRM_KeHoachLamViecTuan_ChiTietViewModel();
                                    NhanVienDay.listWeek.Add(day);
                                }
                                time = time.AddDays(1);
                            }

                            result.listNhanVien.Add(NhanVienDay);
                        }
                        result.listNhanVien = result.listNhanVien.OrderBy(a => a.MaNhanVien).ToList();
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetTongHopLichChamCongTheoCong(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                List<HRM_TheoDoiNgayCong> result = new List<HRM_TheoDoiNgayCong>();
                if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                {
                    listNhanVien = db.HRM_NhanVien.Where(b => b.TenDayDu.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.Trim().ToLower())).ToList();
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    else
                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "KhÃ´ng tá»“n táº¡i nhÃ¢n viÃªn");
                }
                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = (from a in db.HRM_BangChamCong_NhanVien_ThoiGian
                                                                     where a.isKetThuc == true
                                                                      && (string.IsNullOrEmpty(TimKiem.KeyWord) || (!string.IsNullOrEmpty(TimKiem.KeyWord) && listIdNhanVien.Contains(a.IdNhanVien)))
                                                                      && (TimKiem.IdCongTyEOS == 0 || (TimKiem.IdCongTyEOS == a.IdCongTyEOS))
                                                                      && (TimKiem.IdBoPhanEOS == 0 || (TimKiem.IdBoPhanEOS == a.IdBoPhanEOS))
                                                                     group a by new { a.IdCongTyEOS, a.IdNhanVien, a.NgayChamCong } into g
                                                                     select g.OrderByDescending(a => a.Modified).FirstOrDefault()

                                                                      ).ToList();
                if (TimKiem.TuNgay > 0)
                    listData = listData.Where(a => dtTuNgay <= a.NgayChamCong).ToList();
                if (TimKiem.DenNgay > 0)
                    listData = listData.Where(a => dtDenNgay >= a.NgayChamCong).ToList();

                if (listData != null && listData.Count > 0)
                {
                    if (listNhanVien == null || listNhanVien.Count == 0)
                    {
                        listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                        listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    }
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    var listIdHinhThuc = dmHinhThucNghis.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.N100.ToString() || a.Ma == DefineData.eKyHieuChamCong.N50.ToString() || a.Ma == "NBL").Select(a => a.Id).ToList();
                    List<string> list = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => list.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    var listNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => list.Contains(a.IdNhanVien) && listIdHinhThuc.Contains(a.IdHinhThucNghi) && a.NgayNghi >= dtTuNgay && a.NgayNghi <= dtDenNgay).ToList();
                    List<long> listIdBoPhan = listData.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                    List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TheoDoiNgayCong itemAdd = new HRM_TheoDoiNgayCong();
                        var data = listData.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                        double SoGioMotNgay = 8.0;
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);

                            if (CaLamViec != null)
                            {
                                SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 8.0;
                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCa = "P";
                                else
                                    itemAdd.LoaiCa = "F";

                            }
                        }

                        var congTy = listCongTy.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                        double TongCong = 0;

                        itemAdd.CoSo = listCongTy.Where(a => a.Id == NhanVien.IdCongTyEOS).Select(a => a.TenBoPhan).FirstOrDefault();

                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;

                        itemAdd.TenNhanVien = NhanVien.TenDayDu;

                        itemAdd.TenBoPhan = listBoPhan.Where(a => a.Id == NhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault(); ;
                        itemAdd.listNgay = new List<HRM_TheoDoiNgayCong_Ngay>();
                        DateTime time = dtTuNgay;
                        while (time <= dtDenNgay)
                        {
                            var Ngay = data.FirstOrDefault(a => a.NgayChamCong == time);
                            if (Ngay != null)
                            {
                                if (Ngay.SoGioTieuChuan == 0)
                                    Ngay.SoGioTieuChuan = 8;
                                var ngayadd = new HRM_TheoDoiNgayCong_Ngay();

                                ngayadd.Ngay = time;
                                ngayadd.CaChinh = (double)(((Ngay.SoGioLamViec ?? 0) + (Ngay.SoGioLamViecCaGay ?? 0)) / (Ngay.SoGioTieuChuan ?? 8));
                                ngayadd.TangCa = (double)((Ngay.SoGioLamViecTangCa ?? 0) / (Ngay.SoGioTieuChuan ?? 8));
                                itemAdd.listNgay.Add(ngayadd);
                            }
                            else
                            {
                                itemAdd.listNgay.Add(new HRM_TheoDoiNgayCong_Ngay
                                {
                                    Ngay = time,

                                });
                            }
                            time = time.AddDays(1);
                        }

                        itemAdd.CaGayP = (double)(data.Sum(a => (a.SoGioLamViecCaGay ?? 0) / a.SoGioTieuChuan));
                        itemAdd.CaThangF = (double)(data.Sum(a => (a.SoGioLamViec ?? 0) / a.SoGioTieuChuan));
                        itemAdd.TangCa = (double)(data.Sum(a => (a.SoGioLamViecTangCa ?? 0) / a.SoGioTieuChuan));
                        if (listNghiPhep != null && listNghiPhep.Count > 0)
                        {
                            var nghiPheps = listNghiPhep.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                            if (nghiPheps != null && nghiPheps.Count > 0)
                            {
                                double nghiBuLe = 0;
                                if (Nghibu != null)
                                {
                                    nghiBuLe = nghiPheps.Where(a => a.IdHinhThucNghi == Nghibu.Id).Sum(a => a.ThoiGianNghi);
                                    itemAdd.NghiBuLe = nghiBuLe;

                                }
                                itemAdd.Phep = nghiPheps.Sum(a => a.ThoiGianNghi) - nghiBuLe;
                            }

                        }

                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == NhanVien.IdBoPhanEOS);
                        var NghiLeChiTiets = new List<HRM_dmLichNghiLe_Ngay>();
                        double Le_Tet = 0;
                        if (NghiLe != null)

                            NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe &&
                           ((a.TuNgay.Month <= dtTuNgay.Month
                           && a.DenNgay.Month >= dtTuNgay.Month)
                           || (a.TuNgay.Month <= dtDenNgay.Month
                           && (a.DenNgay.Month >= dtDenNgay.Month || a.DenNgay.Month >= dtTuNgay.Month)))).ToList();

                        if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                        {
                            List<int> Ngays = new List<int>();
                            foreach (var ngay in NghiLeChiTiets)
                            {
                                DateTime day = ngay.TuNgay;
                                while (day <= ngay.DenNgay)
                                {
                                    if (day >= dtTuNgay && day <= dtDenNgay)
                                        Ngays.Add(day.Day);
                                    day = day.AddDays(1);
                                }
                            }

                            if (data != null && data.Count > 0)
                            {


                                Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    if (ngay.TyLeLuongLamThem > 1)
                                    {
                                        DateTime day = ngay.TuNgay;
                                        while (day <= ngay.DenNgay)
                                        {
                                            if (Ngays.Contains(day.Day))
                                            {
                                                double TyLe = ngay.TyLeLuong / 100;
                                                Le_Tet++;

                                            }
                                            day = day.AddDays(1);
                                        }
                                    }
                                }
                            }
                        }
                        itemAdd.Le = Le_Tet;
                        TongCong += Le_Tet;
                        itemAdd.TongCong = itemAdd.Le + itemAdd.NghiBuLe + itemAdd.Phep + itemAdd.TangCa + itemAdd.CaThangF + itemAdd.CaGayP;
                        result.Add(itemAdd);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra " + ex.ToString());
            }
        }
        public dynamic ExportTongHopBangChamCong(ApplicationDbContext db, dynamic dyn)
        {

            try
            {
                List<HRM_TheoDoiNgayCong> data = JsonConvert.DeserializeObject(dyn.ToString(), typeof(List<HRM_TheoDoiNgayCong>)) as List<HRM_TheoDoiNgayCong>;
                string sFile = "TongHopBangChamCong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "TongHopBangChamCong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                #region header
                // header
                int nRow = 3;
                int nCol = 7;
                int STT = 1;
                if (data != null && data.Count > 0)
                {
                    DateTime time = data.FirstOrDefault().listNgay.FirstOrDefault().Ngay;
                    DateTime DenNgay = data.FirstOrDefault().listNgay.OrderByDescending(a => a.Ngay).FirstOrDefault().Ngay;
                    while (time <= DenNgay)
                    {
                        sheet1.Cells[nRow, nCol, nRow, nCol + 1].Merge = true;
                        sheet1.Cells[nRow, nCol].Value = time.ToString("dd/MM/yyyy");
                        sheet1.Cells[nRow + 1, nCol].Value = "ChÃ­nh";
                        sheet1.Cells[nRow + 1, nCol + 1].Value = "TÄƒng ca";
                        nCol++;
                        nCol++;
                        time = time.AddDays(1);
                    }
                }
                sheet1.Cells[nRow, nCol, nRow, nCol + 1].Merge = true;
                sheet1.Cells[nRow + 1, nCol].Value = "Ca GÃ£y(P)";
                sheet1.Cells[nRow + 1, nCol + 1].Value = "Ca Tháº³ng (F)";
                nCol++;
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "TÄƒng ca";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Nghá»‰ bÃ¹ lá»…";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "PhÃ©p";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Lá»…";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng cÃ´ng";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "KÃ½ xÃ¡c nháº­n";
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[1, 1].Value = "Báº¢NG CHáº¤M CÃ”NG";
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                nRow = 5;
                if (data != null && data.Count > 0)
                {

                    foreach (var item in data)
                    {

                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.CoSo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LoaiCa;
                        nCol++;
                        if (item.listNgay != null && item.listNgay.Count > 0)
                        {
                            item.listNgay = item.listNgay.OrderBy(a => a.Ngay).ToList();
                            foreach (var Ngay in item.listNgay)
                            {
                                sheet1.Cells[nRow, nCol].Value = Ngay.CaChinh;
                                nCol++;
                                sheet1.Cells[nRow, nCol].Value = Ngay.TangCa;
                                nCol++;
                            }

                            sheet1.Cells[nRow, nCol].Value = item.CaGayP;
                            nCol++;

                            sheet1.Cells[nRow, nCol].Value = item.CaThangF;
                            nCol++;

                            sheet1.Cells[nRow, nCol].Value = item.TangCa;
                            nCol++;

                        }

                        sheet1.Cells[nRow, nCol].Value = item.NghiBuLe;


                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.Phep;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.Le;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongCong;

                        nRow++;
                        STT++;

                    }
                }
                // tá»•ng
                if (nRow >= 5)
                {
                    sheet1.Cells[nRow, 1, nRow, 5].Merge = true;
                    sheet1.Cells[nRow, 1, nRow, 5].Style.Font.Bold = true;
                    sheet1.Cells[nRow, 1].Value = "Tá»”NG";
                    for (int i = 7; i <= nCol; i++)
                    {
                        string NameBD = "";
                        string NameKT = "";
                        try
                        {
                            NameBD = sheet1.Cells[5, i].Address;
                            NameKT = sheet1.Cells[nRow - 1, i].Address;
                            sheet1.Cells[nRow, i].Formula = "=SUM(" + NameBD + ":" + NameKT + ")";
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                sheet1.Cells[5, 7, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong3;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 4)
                {
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol + 1);
                    sheet1.Cells[5, 3, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                // chá»¯ kÃ½
                nRow += 4;
                sheet1.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                sheet1.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet1.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, nCol - 10, nRow - 1, nCol - 2].Merge = true;
                sheet1.Cells[nRow - 1, nCol - 8].Value = "HÃ  Ná»™i,ngÃ y " + dt.Day + " thÃ¡ng " + dt.Month + " nÄƒm " + dt.Year;
                sheet1.Cells[nRow, nCol - 8, nRow, nCol - 4].Merge = true;
                sheet1.Cells[nRow, nCol - 8].Value = "NgÆ°á»i láº­p";
                sheet1.Cells[nRow + 1, nCol - 8, nRow + 1, nCol - 4].Merge = true;
                sheet1.Cells[nRow + 1, nCol - 8].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetTongHopLichChamCongTheoCongTangCa(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, double TuNgay, double DenNgay, string KeyWord)
        {

            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(TuNgay);
                DateTime dtDenNgay = UnixTimeStampToDateTime(DenNgay);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                List<HRM_TheoDoiNgayCongTangCa> result = new List<HRM_TheoDoiNgayCongTangCa>();

                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = (from a in db.HRM_BangChamCong_NhanVien_ThoiGian
                                                                     where a.isKetThuc == true
                                                                     select a
                                                                      ).ToList();
                if (listIdNhanVien != null && listIdNhanVien.Count > 0)
                {
                    listData = listData.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                }
                if (IdCongTy > 0)
                    listData = listData.Where(a => IdCongTy == a.IdCongTyEOS).ToList();
                if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                    listData = listData.Where(a => listIdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
                if (TuNgay > 0)
                    listData = listData.Where(a => dtTuNgay <= a.NgayChamCong).ToList();

                if (DenNgay > 0)
                    listData = listData.Where(a => dtDenNgay >= a.NgayChamCong).ToList();

                if (listData != null && listData.Count > 0)
                {
                    listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    if (!string.IsNullOrEmpty(KeyWord))
                    {
                        listNhanVien = listNhanVien.Where(b => b.TenDayDu.ToLower().Contains(KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    }
                    if (IdCongTy > 0)
                    {
                        var _listBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => IdCongTy == x.IdBoPhan).ToList();
                        var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                        listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                    }
                    if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                    {
                        listNhanVien = listNhanVien.Where(b => listIdBoPhan.Contains(b.IdBoPhanEOS)).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();

                    }
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    List<string> list = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => list.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    var listNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => list.Contains(a.IdNhanVien) && a.NgayNghi >= dtTuNgay && a.NgayNghi <= dtDenNgay).ToList();
                    listIdBoPhan = listData.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                    List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TheoDoiNgayCongTangCa itemAdd = new HRM_TheoDoiNgayCongTangCa();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                        var data = listData.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                        double SoGioMotNgay = 8.0;
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                            if (CaLamViec != null)
                            {
                                SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 8.0;
                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCa = "P";
                                else
                                    itemAdd.LoaiCa = "F";
                            }
                        }
                        //var congTy = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);

                        //itemAdd.CoSo = congTy.TenBoPhan;

                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;

                        itemAdd.TenNhanVien = NhanVien.TenDayDu;
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.listNgay = new List<HRM_TheoDoiNgayCongTangCa_Ngay>();
                        for (DateTime time = dtTuNgay; time <= dtDenNgay; time = time.AddDays(1))
                        {
                            var Ngay = data.FirstOrDefault(a => a.NgayChamCong == time);
                            if (Ngay != null)
                            {
                                itemAdd.listNgay.Add(new HRM_TheoDoiNgayCongTangCa_Ngay
                                {
                                    Ngay = time,
                                    SoGioTangCa = (Ngay.SoGioLamViecTangCa ?? 0),
                                    CongTangCa = (double)((Ngay.SoGioLamViecTangCa ?? 0) / SoGioMotNgay),
                                });
                            }
                            else
                            {
                                itemAdd.listNgay.Add(new HRM_TheoDoiNgayCongTangCa_Ngay
                                {
                                    Ngay = time,

                                });
                            }
                        }

                        itemAdd.TongCongTangCa = itemAdd.listNgay.Sum(a => a.CongTangCa ?? 0);
                        itemAdd.TongGioTangCa = itemAdd.listNgay.Sum(a => a.SoGioTangCa ?? 0);

                        result.Add(itemAdd);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetTongHopLichChamCongTheoCongTangCaById(ApplicationDbContext db, string Id, DateTime dtTuNgay, DateTime dtDenNgay)
        {
            try
            {
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                List<HRM_TheoDoiNgayCongTangCa> result = new List<HRM_TheoDoiNgayCongTangCa>();

                List<HRM_BangChamCong_NhanVien_ThoiGian> listData = (from a in db.HRM_BangChamCong_NhanVien_ThoiGian
                                                                     where a.IdBangChamCong == Id
                                                                     select a
                                                                      ).ToList();
                if (listData != null && listData.Count > 0)
                {
                    listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    List<string> list = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => list.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    var listNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => list.Contains(a.IdNhanVien) && a.NgayNghi >= dtTuNgay && a.NgayNghi <= dtDenNgay).ToList();
                    List<long> listIdBoPhan = listData.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                    List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                    var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                    //var list = _CallWebService.GetDanhSachBoPhan(db);
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TheoDoiNgayCongTangCa itemAdd = new HRM_TheoDoiNgayCongTangCa();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                        var data = listData.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                        double SoGioMotNgay = 8.0;
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                            if (CaLamViec != null)
                            {
                                SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 8.0;
                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCa = "P";
                                else
                                    itemAdd.LoaiCa = "F";
                            }
                        }
                        //var congTy = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);

                        //itemAdd.CoSo = congTy.TenBoPhan;

                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;

                        itemAdd.TenNhanVien = NhanVien.TenDayDu;
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.listNgay = new List<HRM_TheoDoiNgayCongTangCa_Ngay>();
                        for (DateTime time = dtTuNgay; time <= dtDenNgay; time = time.AddDays(1))
                        {
                            var Ngay = data.FirstOrDefault(a => a.NgayChamCong == time);
                            if (Ngay != null)
                            {
                                itemAdd.listNgay.Add(new HRM_TheoDoiNgayCongTangCa_Ngay
                                {
                                    Ngay = time,
                                    SoGioTangCa = (Ngay.SoGioLamViecTangCa ?? 0),
                                    CongTangCa = (double)((Ngay.SoGioLamViecTangCa ?? 0) / SoGioMotNgay),
                                });
                            }
                            else
                            {
                                itemAdd.listNgay.Add(new HRM_TheoDoiNgayCongTangCa_Ngay
                                {
                                    Ngay = time,

                                });
                            }
                        }

                        itemAdd.TongCongTangCa = itemAdd.listNgay.Sum(a => a.CongTangCa ?? 0);
                        itemAdd.TongGioTangCa = itemAdd.listNgay.Sum(a => a.SoGioTangCa ?? 0);

                        result.Add(itemAdd);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetTongHopLichChamCongTheoCongTangCaCaNhan(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, double TuNgay, double DenNgay, string KeyWord)
        {

            try
            {
                DateTime dtTuNgay = UnixTimeStampToDateTime(TuNgay);
                DateTime dtDenNgay = UnixTimeStampToDateTime(DenNgay);
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                HRM_TheoDoiNgayCongTangCaCaNhan result = new HRM_TheoDoiNgayCongTangCaCaNhan();

                var listKeHoach = (from a in db.HRM_ThoiGianLamViecKeHoach
                                   where a.Loai == 1
                                   && dtTuNgay <= a.Ngay && dtDenNgay >= a.Ngay
                                   select a
                                 ).ToList();
                var listChamCong = (from a in db.HRM_CheckInOut
                                    where dtTuNgay <= a.Ngay && dtDenNgay >= a.Ngay
                                    select a
                                    ).ToList();
                if (listIdNhanVien != null && listIdNhanVien.Count > 0)
                {
                    listChamCong = listChamCong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    listKeHoach = listKeHoach.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();

                }
                if (listKeHoach != null && listKeHoach.Count > 0)
                {
                    listIdNhanVien = listKeHoach.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    if (!string.IsNullOrEmpty(KeyWord))
                    {
                        listNhanVien = listNhanVien.Where(b => b.TenDayDu.ToLower().Contains(KeyWord.Trim().ToLower()) || b.MaNhanVien.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                    }
                    if (IdCongTy > 0)
                    {
                        var _listBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => IdCongTy == x.IdBoPhan).ToList();
                        var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
                        listNhanVien = listNhanVien.Where(x => _listIdNhanVien.Contains(x.IdGuid)).ToList();
                        //listNhanVien = listNhanVien.Where(b => b.IdCongTyEOS == IdCongTy).ToList();
                        //if (listNhanVien != null && listNhanVien.Count > 0)
                        //    listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList()
                    }
                    if (listIdBoPhan != null && listIdBoPhan.Count > 0)
                    {
                        listNhanVien = listNhanVien.Where(b => listIdBoPhan.Contains(b.IdBoPhanEOS)).ToList();
                        if (listNhanVien != null && listNhanVien.Count > 0)
                            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();

                    }
                    var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    result.listNhanVien = new List<HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien>();

                    result.Label = 1;

                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien itemAdd = new HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien();
                        var data = listKeHoach.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                        //var congTy = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                        //itemAdd.CoSo = congTy.TenBoPhan;
                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;
                        itemAdd.TenNhanVien = NhanVien.TenDayDu;
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.listNgay = new List<HRM_TheoDoiNgayCongTangCaCaNhan_Ngay>();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);

                            if (CaLamViec != null)
                            {

                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCa = "P";
                                else
                                    itemAdd.LoaiCa = "F";

                            }
                        }
                        for (DateTime time = dtTuNgay; time <= dtDenNgay; time = time.AddDays(1))
                        {
                            HRM_TheoDoiNgayCongTangCaCaNhan_Ngay NgayAdd = new HRM_TheoDoiNgayCongTangCaCaNhan_Ngay();
                            NgayAdd.Ngay = time;
                            NgayAdd.SoGio = 0;
                            NgayAdd.listChiTiet = new List<HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao>();
                            var ChamCong = listChamCong.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.Ngay == time).OrderBy(a => a.Gio).ToList();
                            var Ngay = data.Where(a => a.Ngay == time).OrderBy(a => a.GioVao).ToList();
                            if (Ngay != null && Ngay.Count > 0)
                            {
                                if (result.Label < Ngay.Count)
                                    result.Label = Ngay.Count;
                                NgayAdd.LyDo = Ngay.FirstOrDefault().LyDo;
                                DateTime? GioRaTruoc = null;
                                int STT = 0;
                                foreach (var item in Ngay)
                                {
                                    HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao itemRaVao = new HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao();
                                    // giá» vÃ o
                                    var GioVao = ChamCong.FirstOrDefault(a => a.Loai == "I" && a.Gio < item.GioRa && (a.Gio >= item.GioVao || (a.Gio < item.GioVao && (GioRaTruoc == null || (GioRaTruoc != null && a.Gio >= GioRaTruoc)))));
                                    if (GioVao != null)
                                    {
                                        if (GioVao.Gio >= item.GioVao)
                                            itemRaVao.GioVao = GioVao.Gio;
                                        else
                                            itemRaVao.GioVao = item.GioVao;
                                    }

                                    var GioRa = ChamCong.FirstOrDefault(a => a.Loai != "I" && a.Gio > item.GioVao && a.Gio > item.GioRa);
                                    if (GioRa != null)
                                    {
                                        itemRaVao.GioRa = item.GioRa;
                                    }
                                    GioRa = ChamCong.Where(a => a.Loai != "I" && a.Gio > item.GioVao && a.Gio <= item.GioRa).OrderByDescending(a => a.Gio).FirstOrDefault();
                                    if (GioRa != null)
                                    {
                                        itemRaVao.GioRa = GioRa.Gio;
                                    }

                                    if (itemRaVao.GioRa != null && itemRaVao.GioVao == null)
                                        itemRaVao.GioVao = item.GioVao;
                                    NgayAdd.listChiTiet.Add(itemRaVao);
                                    GioRaTruoc = item.GioRa;
                                    STT++;
                                    if (itemRaVao.GioVao != null && itemRaVao.GioRa != null)
                                    {
                                        TimeSpan Gio = (DateTime)itemRaVao.GioRa - (DateTime)itemRaVao.GioVao;
                                        NgayAdd.SoGio += Gio.TotalHours;
                                    }
                                }
                                itemAdd.listNgay.Add(NgayAdd);
                            }
                        }
                        result.listNhanVien.Add(itemAdd);
                    }
                    foreach (var item in result.listNhanVien)
                    {
                        foreach (var ngay in item.listNgay)
                        {
                            while (ngay.listChiTiet.Count < result.Label)
                            {
                                ngay.listChiTiet.Add(new HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao());

                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra " + ex.ToString());
            }
        }
        public dynamic GetTongHopLichChamCongTheoCongTangCaCaNhanById(ApplicationDbContext db, string Id, DateTime dtTuNgay, DateTime dtDenNgay)
        {

            try
            {
                List<string> listIdNhanVien = new List<string>();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                HRM_TheoDoiNgayCongTangCaCaNhan result = new HRM_TheoDoiNgayCongTangCaCaNhan();

                var listKeHoach = (from a in db.HRM_ThoiGianLamViecKeHoach
                                   where a.Loai == 1
                                   && dtTuNgay <= a.Ngay && dtDenNgay >= a.Ngay
                                   select a
                                 ).ToList();
                var listChamCong = (from a in db.HRM_CheckInOut
                                    where dtTuNgay <= a.Ngay && dtDenNgay >= a.Ngay
                                    select a
                                    ).ToList();
                if (listIdNhanVien != null && listIdNhanVien.Count > 0)
                {
                    listChamCong = listChamCong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                    listKeHoach = listKeHoach.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();

                }
                if (listKeHoach != null && listKeHoach.Count > 0)
                {
                    listIdNhanVien = listKeHoach.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                    var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
                    var listCa = db.HRM_dmLichLamViec.ToList();
                    var dmHinhThucNghis = _CallWebService.GetListDanhMucHinhThucNghi(db);
                    var Nghibu = dmHinhThucNghis.FirstOrDefault(a => a.Ma == "NBL");
                    var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                    result.listNhanVien = new List<HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien>();

                    result.Label = 1;
                    foreach (var NhanVien in listNhanVien)
                    {
                        HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien itemAdd = new HRM_TheoDoiNgayCongTangCaCaNhan_NhanVien();
                        var data = listKeHoach.Where(a => a.IdNhanVien == NhanVien.IdGuid).ToList();
                        var congTy = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                        if (congTy != null)
                            itemAdd.CoSo = congTy.TenBoPhan;
                        itemAdd.MaNhanVien = NhanVien.MaNhanVien;
                        itemAdd.TenNhanVien = NhanVien.TenDayDu;
                        if (BoPhan != null)
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        itemAdd.listNgay = new List<HRM_TheoDoiNgayCongTangCaCaNhan_Ngay>();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == NhanVien.IdGuid);
                        if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                        {
                            var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);

                            if (CaLamViec != null)
                            {

                                if (CaLamViec.isCaGay == true)
                                    itemAdd.LoaiCa = "P";
                                else
                                    itemAdd.LoaiCa = "F";

                            }
                        }
                        for (DateTime time = dtTuNgay; time <= dtDenNgay; time = time.AddDays(1))
                        {
                            HRM_TheoDoiNgayCongTangCaCaNhan_Ngay NgayAdd = new HRM_TheoDoiNgayCongTangCaCaNhan_Ngay();
                            NgayAdd.Ngay = time;
                            NgayAdd.SoGio = 0;
                            NgayAdd.listChiTiet = new List<HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao>();
                            var ChamCong = listChamCong.Where(a => a.IdNhanVien == NhanVien.IdGuid && a.Ngay == time).OrderBy(a => a.Gio).ToList();
                            var Ngay = data.Where(a => a.Ngay == time).OrderBy(a => a.GioVao).ToList();
                            if (Ngay != null && Ngay.Count > 0)
                            {
                                if (result.Label < Ngay.Count)
                                    result.Label = Ngay.Count;
                                NgayAdd.LyDo = Ngay.FirstOrDefault().LyDo;
                                DateTime? GioRaTruoc = null;
                                int STT = 0;
                                foreach (var item in Ngay)
                                {
                                    HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao itemRaVao = new HRM_TheoDoiNgayCongTangCaCaNhan_GioRaVao();
                                    // giá» vÃ o
                                    var GioVao = ChamCong.FirstOrDefault(a => a.Loai == "I" && a.Gio < item.GioRa && (a.Gio >= item.GioVao || (a.Gio < item.GioVao && (GioRaTruoc == null || (GioRaTruoc != null && a.Gio >= GioRaTruoc)))));
                                    if (GioVao != null)
                                    {
                                        if (GioVao.Gio >= item.GioVao)
                                            itemRaVao.GioVao = GioVao.Gio;
                                        else
                                            itemRaVao.GioVao = item.GioVao;
                                    }

                                    var GioRa = ChamCong.FirstOrDefault(a => a.Loai != "I" && a.Gio > item.GioVao && a.Gio > item.GioRa);
                                    if (GioRa != null)
                                    {
                                        itemRaVao.GioRa = item.GioRa;
                                    }
                                    GioRa = ChamCong.Where(a => a.Loai != "I" && a.Gio > item.GioVao && a.Gio <= item.GioRa).OrderByDescending(a => a.Gio).FirstOrDefault();
                                    if (GioRa != null)
                                    {
                                        itemRaVao.GioRa = GioRa.Gio;
                                    }

                                    if (itemRaVao.GioRa != null && itemRaVao.GioVao == null)
                                        itemRaVao.GioVao = item.GioVao;
                                    NgayAdd.listChiTiet.Add(itemRaVao);
                                    GioRaTruoc = item.GioRa;
                                    STT++;
                                    if (itemRaVao.GioVao != null && itemRaVao.GioRa != null)
                                    {
                                        TimeSpan Gio = (DateTime)itemRaVao.GioRa - (DateTime)itemRaVao.GioVao;
                                        NgayAdd.SoGio += Gio.TotalHours;
                                    }
                                }
                                itemAdd.listNgay.Add(NgayAdd);
                            }
                        }
                        result.listNhanVien.Add(itemAdd);
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic ExportTongHopCongTangCa(ApplicationDbContext db, long IdCongTy, List<long> listIdBoPhan, double TuNgayUnix, double DenNgayUnix, string keyWord)
        {

            try
            {
                List<HRM_TheoDoiNgayCongTangCa> data = GetTongHopLichChamCongTheoCongTangCa(db, IdCongTy, listIdBoPhan, TuNgayUnix, DenNgayUnix, keyWord);
                DateTime dtTuNgay = UnixTimeStampToDateTime(TuNgayUnix);
                DateTime dtDenNgay = UnixTimeStampToDateTime(DenNgayUnix);
                string sFile = "TongHopCongTangCa.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = sFile;
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Tá»•ng há»£p tÄƒng ca"];
                ExcelWorksheet sheet2 = package.Workbook.Worksheets["CÃ´ng tÄƒng ca cÃ¡ nhÃ¢n"];

                #region header
                // header
                int nRow = 3;
                int nCol = 7;
                int STT = 1;

                DateTime time = dtTuNgay;
                while (time <= dtDenNgay)
                {
                    sheet1.Cells[nRow, nCol, nRow, nCol + 1].Merge = true;
                    sheet1.Cells[nRow, nCol].Value = time.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow + 1, nCol].Value = "Sá»‘ giá» tÄƒng Ca";
                    sheet1.Cells[nRow + 1, nCol + 1].Value = "CÃ´ng tÄƒng ca";
                    nCol++;
                    nCol++;
                    time = time.AddDays(1);
                }

                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng giá» tÄƒng ca";

                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng cÃ´ng tÄƒng ca";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "KÃ½ xÃ¡c nháº­n";
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[1, 1].Value = "Tá»”NG Há»¢P TÄ‚NG CA";
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                nRow = 5;
                if (data != null && data.Count > 0)
                {

                    foreach (var item in data)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.CoSo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LoaiCa;
                        nCol++;

                        if (item.listNgay != null && item.listNgay.Count > 0)
                        {
                            item.listNgay = item.listNgay.OrderBy(a => a.Ngay).ToList();
                            foreach (var Ngay in item.listNgay)
                            {
                                sheet1.Cells[nRow, nCol].Value = Ngay.SoGioTangCa;
                                nCol++;
                                sheet1.Cells[nRow, nCol].Value = Ngay.CongTangCa;
                                nCol++;
                            }

                            sheet1.Cells[nRow, nCol].Value = item.TongGioTangCa;
                            nCol++;

                            sheet1.Cells[nRow, nCol].Value = item.TongCongTangCa;
                            nCol++;

                        }
                        nRow++;
                        STT++;
                    }
                }
                // tá»•ng
                if (nRow >= 5)
                {
                    sheet1.Cells[nRow, 1, nRow, 5].Merge = true;
                    sheet1.Cells[nRow, 1, nRow, 5].Style.Font.Bold = true;
                    sheet1.Cells[nRow, 1].Value = "Tá»”NG";
                    for (int i = 6; i <= nCol; i++)
                    {
                        string NameBD = sheet1.Cells[5, i].Address;
                        string NameKT = sheet1.Cells[nRow - 1, i].Address;
                        sheet1.Cells[nRow, i].Formula = "=SUM(" + NameBD + ":" + NameKT + ")";
                    }
                }
                sheet1.Cells[5, 7, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                if (nRow > 4)
                {
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[5, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                    sheet1.Cells[5, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                // chá»¯ kÃ½
                nRow += 4;
                sheet1.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                sheet1.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet1.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                //sheet1.Cells[nRow - 1, nCol - 10, nRow - 1, nCol - 2].Merge = true;
                //sheet1.Cells[nRow - 1, nCol - 8].Value = "HÃ  Ná»™i,ngÃ y " + dt.Day + " thÃ¡ng " + dt.Month + " nÄƒm " + dt.Year;
                //sheet1.Cells[nRow, nCol - 8, nRow, nCol - 4].Merge = true;
                //sheet1.Cells[nRow, nCol - 8].Value = "NgÆ°á»i láº­p";
                //sheet1.Cells[nRow + 1, nCol - 8, nRow + 1, nCol - 4].Merge = true;
                //sheet1.Cells[nRow + 1, nCol - 8].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;

                /// cÃ´ng tÄƒng ca cÃ¡ nhÃ¢n
                HRM_TheoDoiNgayCongTangCaCaNhan dataCongCaNhan = GetTongHopLichChamCongTheoCongTangCaCaNhan(db, IdCongTy, listIdBoPhan, TuNgayUnix, DenNgayUnix, keyWord);
                nRow = 3;
                nCol = 7;
                // header
                if (dataCongCaNhan != null)
                {
                    if (dataCongCaNhan.Label == 0)
                        dataCongCaNhan.Label = 1;

                    sheet2.Cells[nRow, nCol, nRow, nCol + dataCongCaNhan.Label * 2 - 1].Merge = true;
                    sheet2.Cells[nRow, nCol].Value = "Giá» tÄƒng ca";
                    string CheckInOut = "I";
                    for (int i = 1; i <= dataCongCaNhan.Label * 2; i++)
                    {
                        if (CheckInOut == "I")
                        {
                            sheet2.Cells[nRow + 1, nCol].Value = "Giá» vÃ o";
                            CheckInOut = "O";
                        }
                        else
                        {
                            sheet2.Cells[nRow + 1, nCol].Value = "Giá» ra";
                            CheckInOut = "I";
                        }

                        nCol++;
                    }
                    sheet2.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                    sheet2.Cells[nRow, nCol].Value = "Sá»‘ giá» tÄƒng ca";
                    nCol++;
                    sheet2.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                    sheet2.Cells[nRow, nCol].Value = "LÃ½ do";
                }
                sheet2.Cells[1, 1, 1, nCol].Merge = true;
                sheet2.Cells[1, 1].Value = "CÃ”NG TÄ‚NG CA CÃ NHÃ‚N";
                sheet2.Cells[1, 1].Style.Font.Bold = true;
                sheet2.Cells[1, 1].Style.Font.Size = 18;
                nRow = 5;
                if (dataCongCaNhan.listNhanVien != null)
                {

                    STT = 1;
                    foreach (var item in dataCongCaNhan.listNhanVien)
                    {
                        nCol = 1;
                        int Dem = item.listNgay.Count;
                        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                        sheet2.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                        sheet2.Cells[nRow, nCol].Value = item.CoSo;
                        nCol++;
                        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                        sheet2.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                        sheet2.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                        sheet2.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;

                        int i = 0;
                        foreach (var Ngay in item.listNgay)
                        {
                            double TongGio = 0;
                            int Col = nCol;
                            sheet2.Cells[nRow + i, Col].Value = Ngay.Ngay.ToString("dd/MM/yyyy");
                            Col++;

                            foreach (var Cong in Ngay.listChiTiet.OrderBy(a => a.GioVao))
                            {
                                if (Cong.GioVao != null)
                                    sheet2.Cells[nRow + i, Col].Value = Cong.GioVao.Value.ToString("HH:mm");
                                Col++;
                                if (Cong.GioRa != null)
                                    sheet2.Cells[nRow + i, Col].Value = Cong.GioRa.Value.ToString("HH:mm");
                                Col++;
                                if (Cong.GioVao != null && Cong.GioRa != null)
                                {
                                    TimeSpan Gio = (DateTime)Cong.GioRa - (DateTime)Cong.GioVao;
                                    TongGio += Gio.TotalHours;
                                }
                            }
                            Col = nCol + 1 + dataCongCaNhan.Label * 2;
                            sheet2.Cells[nRow + i, Col].Value = TongGio;
                            sheet2.Cells[nRow + i, Col].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            Col++;
                            sheet2.Cells[nRow + i, Col].Value = Ngay.LyDo;
                            i++;
                        }



                        nRow = nRow + Dem;
                        STT++;
                    }
                    nCol = 8 + dataCongCaNhan.Label * 2;
                    // tá»•ng
                    sheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet2.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    if (nRow > 4)
                    {
                        nRow--;
                        dungchung.DrawTableExcel(sheet2, 3, nRow, 1, nCol);
                        sheet2.Cells[5, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet2.Cells[5, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    }
                }
                // chá»¯ kÃ½
                nRow += 4;
                sheet2.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet2.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                sheet2.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet2.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";
                sheet2.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet2.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet2.Cells.Style.Font.Name = "Times New Roman";
                sheet2.Cells.Style.WrapText = true;

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic ExportTongHopCongTangCa_TheoQuyTrinhChamCong(ApplicationDbContext db, string Id)
        {

            try
            {
                var BangChamCong = db.HRM_BangChamCong.FirstOrDefault(a => a.IdGuid == Id);
                List<HRM_TheoDoiNgayCongTangCa> data = GetTongHopLichChamCongTheoCongTangCaById(db, Id, BangChamCong.TuNgay.Value, BangChamCong.DenNgay.Value);
                DateTime dtTuNgay = new DateTime(BangChamCong.TuNgay.Value.Year, BangChamCong.TuNgay.Value.Month, BangChamCong.TuNgay.Value.Day, 0, 0, 0);
                DateTime dtDenNgay = new DateTime(BangChamCong.DenNgay.Value.Year, BangChamCong.DenNgay.Value.Month, BangChamCong.DenNgay.Value.Day, 0, 0, 0);
                string sFile = "TongHopCongTangCa.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = sFile;
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Tá»•ng há»£p tÄƒng ca"];
                ExcelWorksheet sheet2 = package.Workbook.Worksheets["CÃ´ng tÄƒng ca cÃ¡ nhÃ¢n"];
                package.Workbook.Worksheets.Delete(sheet2);
                #region header
                // header
                int nRow = 3;
                int nCol = 7;
                int STT = 1;

                DateTime time = dtTuNgay;
                while (time <= dtDenNgay)
                {
                    sheet1.Cells[nRow, nCol, nRow, nCol + 1].Merge = true;
                    sheet1.Cells[nRow, nCol].Value = time.ToString("dd/MM/yyyy");
                    sheet1.Cells[nRow + 1, nCol].Value = "Sá»‘ giá» tÄƒng Ca";
                    sheet1.Cells[nRow + 1, nCol + 1].Value = "CÃ´ng tÄƒng ca";
                    nCol++;
                    nCol++;
                    time = time.AddDays(1);
                }

                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng giá» tÄƒng ca";

                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng cÃ´ng tÄƒng ca";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "KÃ½ xÃ¡c nháº­n";
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[1, 1].Value = "Tá»”NG Há»¢P TÄ‚NG CA";
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                nRow = 5;
                if (data != null && data.Count > 0)
                {

                    foreach (var item in data)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.CoSo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LoaiCa;
                        nCol++;

                        if (item.listNgay != null && item.listNgay.Count > 0)
                        {
                            item.listNgay = item.listNgay.OrderBy(a => a.Ngay).ToList();
                            foreach (var Ngay in item.listNgay)
                            {
                                sheet1.Cells[nRow, nCol].Value = Ngay.SoGioTangCa;
                                nCol++;
                                sheet1.Cells[nRow, nCol].Value = Ngay.CongTangCa;
                                nCol++;
                            }

                            sheet1.Cells[nRow, nCol].Value = item.TongGioTangCa;
                            nCol++;

                            sheet1.Cells[nRow, nCol].Value = item.TongCongTangCa;

                            nCol++;
                        }

                        nRow++;
                        STT++;

                    }
                }
                // tá»•ng
                if (nRow >= 5)
                {
                    sheet1.Cells[nRow, 1, nRow, 5].Merge = true;
                    sheet1.Cells[nRow, 1, nRow, 5].Style.Font.Bold = true;
                    sheet1.Cells[nRow, 1].Value = "Tá»”NG";
                    for (int i = 6; i <= nCol; i++)
                    {
                        string NameBD = sheet1.Cells[5, i].Address;
                        string NameKT = sheet1.Cells[nRow - 1, i].Address;
                        sheet1.Cells[nRow, i].FormulaR1C1 = "SUM(" + NameBD + ":" + NameKT + ")";
                    }
                }
                sheet1.Cells[5, 7, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 4)
                {
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                    sheet1.Cells[5, 3, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                // chá»¯ kÃ½
                nRow += 4;
                sheet1.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                sheet1.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet1.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                //sheet1.Cells[nRow - 1, nCol - 10, nRow - 1, nCol - 2].Merge = true;
                //sheet1.Cells[nRow - 1, nCol - 8].Value = "HÃ  Ná»™i,ngÃ y " + dt.Day + " thÃ¡ng " + dt.Month + " nÄƒm " + dt.Year;
                //sheet1.Cells[nRow, nCol - 8, nRow, nCol - 4].Merge = true;
                //sheet1.Cells[nRow, nCol - 8].Value = "NgÆ°á»i láº­p";
                //sheet1.Cells[nRow + 1, nCol - 8, nRow + 1, nCol - 4].Merge = true;
                //sheet1.Cells[nRow + 1, nCol - 8].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                #region
                /// cÃ´ng tÄƒng ca cÃ¡ nhÃ¢n

                //HRM_TheoDoiNgayCongTangCaCaNhan dataCongCaNhan = GetTongHopLichChamCongTheoCongTangCaCaNhanById(db, Id, BangChamCong.TuNgay.Value, BangChamCong.DenNgay.Value);
                //nRow = 3;
                //nCol = 8;
                //// header

                //if (dataCongCaNhan != null)
                //{
                //    if (dataCongCaNhan.Label == 0)
                //        dataCongCaNhan.Label = 1;
                //    sheet2.Cells[nRow, nCol, nRow, nCol + dataCongCaNhan.Label * 2 - 1].Merge = true;
                //    sheet2.Cells[nRow, nCol].Value = "Giá» tÄƒng ca";
                //    string CheckInOut = "I";
                //    for (int i = 1; i <= dataCongCaNhan.Label * 2; i++)
                //    {
                //        if (CheckInOut == "I")
                //        {
                //            sheet2.Cells[nRow + 1, nCol].Value = "Giá» vÃ o";
                //            CheckInOut = "O";
                //        }
                //        else
                //        {
                //            sheet2.Cells[nRow + 1, nCol].Value = "Giá» ra";
                //            CheckInOut = "I";
                //        }

                //        nCol++;
                //    }
                //    sheet2.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                //    sheet2.Cells[nRow, nCol].Value = "Sá»‘ giá» tÄƒng ca";
                //    nCol++;
                //    sheet2.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                //    sheet2.Cells[nRow, nCol].Value = "LÃ½ do";
                //}
                //nRow = 5;

                //STT = 1;
                //if (dataCongCaNhan.listNhanVien != null)
                //{
                //    foreach (var item in dataCongCaNhan.listNhanVien)
                //    {
                //        nCol = 1;
                //        int Dem = item.listNgay.Count;
                //        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                //        sheet2.Cells[nRow, nCol].Value = STT;
                //        nCol++;
                //        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                //        sheet2.Cells[nRow, nCol].Value = item.CoSo;
                //        nCol++;
                //        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                //        sheet2.Cells[nRow, nCol].Value = item.MaNhanVien;
                //        nCol++;
                //        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                //        sheet2.Cells[nRow, nCol].Value = item.TenNhanVien;
                //        nCol++;
                //        sheet2.Cells[nRow, nCol, nRow + Dem - 1, nCol].Merge = true;
                //        sheet2.Cells[nRow, nCol].Value = item.TenBoPhan;
                //        nCol++;

                //        int i = 0;
                //        foreach (var Ngay in item.listNgay)
                //        {
                //            double TongGio = 0;
                //            int Col = nCol;
                //            sheet2.Cells[nRow + i, Col].Value = Ngay.Ngay;
                //            Col++;
                //            Col++;

                //            foreach (var Cong in Ngay.listChiTiet.OrderBy(a => a.GioVao))
                //            {
                //                if (Cong.GioVao != null)
                //                    sheet2.Cells[nRow + i, Col].Value = Cong.GioVao.Value.ToString("HH:mm");
                //                Col++;
                //                if (Cong.GioRa != null)
                //                    sheet2.Cells[nRow + i, Col].Value = Cong.GioRa.Value.ToString("HH:mm");
                //                Col++;
                //                if (Cong.GioVao != null && Cong.GioRa != null)
                //                {
                //                    TimeSpan Gio = (DateTime)Cong.GioRa - (DateTime)Cong.GioVao;
                //                    TongGio += Gio.TotalHours;
                //                }
                //            }
                //            Col = nCol + dataCongCaNhan.Label * 2;
                //            sheet2.Cells[nRow + i, Col].Value = TongGio;
                //            Col++;
                //            sheet2.Cells[nRow + i, Col].Value = Ngay.LyDo;
                //            i++;
                //        }



                //        nRow = nRow + Dem;
                //        STT++;
                //    }
                //    nCol = 9 + dataCongCaNhan.Label * 2;
                //    // tá»•ng
                //    sheet2.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                //    sheet2.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                //    if (nRow > 4)
                //    {
                //        nRow--;
                //        dungchung.DrawTableExcel(sheet2, 3, nRow, 1, nCol);
                //        sheet2.Cells[5, 2, nRow, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                //        sheet2.Cells[5, 4, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                //    }
                //}
                //// chá»¯ kÃ½
                //nRow += 4;
                //sheet2.Cells[nRow, 3, nRow, 6].Merge = true;
                //sheet2.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                //sheet2.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                //sheet2.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";
                //sheet2.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                //sheet2.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                //sheet2.Cells.Style.Font.Name = "Times New Roman";
                //sheet2.Cells.Style.WrapText = true;
                #endregion
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        #endregion
        #region Quy trÃ¬nh TÃ­nh LÆ°Æ¡ng
        public dynamic GetNextMaQuyTrinhTinhLuong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "QTTL",
                db.HRM_QuyTrinhTinhLuong.OrderByDescending(a => a.Created).AsNoTracking().Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListQuyTrinhTinhLuong(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_QuyTrinhTinhLuong> result;
                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_QuyTrinhTinhLuong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_QuyTrinhTinhLuong
                             orderby a.Created descending
                             select a;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                if (TimKiem.IdCongTy > 0)
                    result = result.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_QuyTrinhTinhLuong" && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }

                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                foreach (HRM_QuyTrinhTinhLuong item in itemList)
                {
                    BoPhan CongTy = listCongTy.Find(x => x.Id == item.IdCongTyEOS);
                    if (CongTy != null)
                        item.TenCongty = CongTy.TenBoPhan;
                    if (item.IdBoPhan != null)
                    {
                        item.listIdBoPhan = new List<long>();
                        if (item.IdBoPhan.Contains(","))
                        {
                            string[] list = item.IdBoPhan.Split(',');
                            foreach (var bp in list)
                                item.listIdBoPhan.Add(long.Parse(bp));
                        }
                        else item.listIdBoPhan.Add(long.Parse(item.IdBoPhan));
                    }
                    if (item.listIdBoPhan != null)
                    {
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));
                    }

                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic SetQuyTrinhTinhLuong(HRM_QuyTrinhTinhLuong item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // DI: use inherited _CallWebService
                    {

                        #region check bá»™ pháº­n Ä‘Ã£ tÃ­nh lÆ°Æ¡ng
                        var list = (from a in db.HRM_QuyTrinhTinhLuong
                                    join b in db.HRM_QuyTrinhTinhLuong_ChamCong on a.IdGuid equals b.IdBangTinhLuong
                                    //join c in db.HRM_QuyTrinhBoPhan_BoPhan on a.IdGuid equals c.IdQuyTrinh
                                    where item.listIdKyChamCong.Contains(b.IdBangChamCong) && a.IdGuid != item.IdGuid
                                    select a.IdGuid
                                           ).ToList();
                        if (list != null && list.Count > 0)
                        {

                            var listIdBoPhanCheck = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => list.Contains(a.IdQuyTrinh)).Select(a => a.IdBoPhan).ToList();
                            if ((listIdBoPhanCheck == null || listIdBoPhanCheck.Count == 0))
                            {

                                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Ká»³ Cháº¥m cÃ´ng Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³");
                            }
                            else if (item.listIdBoPhan.Count == 0)
                            {
                                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhanCheck);
                                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Ká»³ Cháº¥m cÃ´ng Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³: " + string.Join(", ", listBoPhan.Select(a => a.TenBoPhan).ToList()));
                            }
                            else
                            {
                                List<long> listTrung = listIdBoPhanCheck.Intersect(item.listIdBoPhan).ToList();
                                if (listTrung != null && listTrung.Count > 0)
                                {
                                    var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listTrung);
                                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Bá»™ pháº­n Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³: " + string.Join(", ", listBoPhan.Select(a => a.TenBoPhan).ToList()));
                                }
                            }

                        }

                        #endregion
                    }
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");

                    //if (item.lstLuongChiTiet == null || item.lstLuongChiTiet.Count == 0)
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng danh sÃ¡ch khÃ´ng cÃ³ lÆ°Æ¡ng");
                    var QuyTrinhdb = db.HRM_QuyTrinhTinhLuong.AsNoTracking().FirstOrDefault(a => a.IdGuid == item.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, item, err))
                        return err;

                    bool check = UpdateQuyTrinhTinhLuong(item, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check == false)
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, item, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();

                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng", GetQuyTrinhTinhLuong(item.IdGuid, db));
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh khÃ´ng cÃ´ng " + ex.ToString());
                }
            }
        }
        public dynamic GetQuyTrinhTinhLuong(string Id, ApplicationDbContext db)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                HRM_QuyTrinhTinhLuong QuyTrinh = db.HRM_QuyTrinhTinhLuong.FirstOrDefault(a => a.IdGuid == Id);
                QuyTrinh.listBoPhan = new List<clsBangTinhLuong_NhanVienView>();
                if (!base.GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
                if (QuyTrinh.TuNgay != null)
                    QuyTrinh.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.TuNgay);
                if (QuyTrinh.DenNgay != null)
                    QuyTrinh.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.DenNgay);

                if (QuyTrinh.IdBoPhan != null)
                {
                    QuyTrinh.listIdBoPhan = new List<long>();
                    if (QuyTrinh.IdBoPhan.Contains(","))
                    {
                        string[] list = QuyTrinh.IdBoPhan.Split(',');
                        foreach (var bp in list)
                            QuyTrinh.listIdBoPhan.Add(long.Parse(bp));
                    }
                    else QuyTrinh.listIdBoPhan.Add(long.Parse(QuyTrinh.IdBoPhan));
                }
                if (!string.IsNullOrEmpty(QuyTrinh.IdKyChamCong))
                {
                    QuyTrinh.listIdKyChamCong = new List<string>();
                    if (QuyTrinh.IdKyChamCong.Contains(","))
                    {
                        string[] list = QuyTrinh.IdKyChamCong.Split(',');
                        foreach (var bp in list)
                            QuyTrinh.listIdKyChamCong.Add(bp);
                    }
                    else QuyTrinh.listIdKyChamCong.Add(QuyTrinh.IdKyChamCong);
                }

                QuyTrinh.listNhanVien = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
                QuyTrinh.listNhanVien = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.Created).ToList();
                var listdmPhucap = db.HRM_dmPhuCap.ToList();
                var listPhuCap = db.HRM_QuyTrinhTinhLuong_PhuCap.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.Created).ToList();
                foreach (var item in listPhuCap)
                    item.Ma = listdmPhucap.Where(a => a.IdGuid == item.IddmPhuCap).Select(a => a.Ma).FirstOrDefault();
                var ListBaoHiemNgoai = db.HRM_QuyTrinhTinhLuong_BaoHiemNgoai.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.Created).ToList();
                var ListTangCa = db.HRM_QuyTrinhTinhLuong_TangCa.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.Created).ToList();
                var ListGTC = db.HRM_QuyTrinhTinhLuong_SoGioLamViec.Where(a => a.IdQuyTrinh == Id).OrderBy(a => a.Created).ToList();
                if (QuyTrinh.listNhanVien != null)
                {
                    List<string> listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).ToList();
                    var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                    List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).ToList();
                    var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();
                    var listboPhan = _CallWebService.GetDanhSachBoPhan(db);
                    var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                    var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                    var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                    var ListNganHang = db.HRM_dmNganHang.ToList();
                    var ListLoaiHD = db.HRM_dmLoaiHopDong.ToList();
                    foreach (var item in QuyTrinh.listNhanVien)
                    {
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                        if (HopDong != null)
                        {
                            item.TenLoaiHopDong = ListLoaiHD.Where(a => a.IdGuid == HopDong.IdLoaiHopDong).Select(a => a.Ten).FirstOrDefault();
                        }
                        item.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        item.listBaoHiemNgoai = ListBaoHiemNgoai.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Created).ToList();
                        item.listTangCa = ListTangCa.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Created).ToList();
                        item.listGioTieuChuan = ListGTC.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Created).ToList();
                        item.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == item.IdNhanVien).OrderBy(a => a.Created).ToList();
                        var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                        if (NhanVien != null)
                        {

                            item.SoTaiKhoan = NhanVien.TaiKhoanNganHang;
                            if (!string.IsNullOrEmpty(NhanVien.TenNganHang))
                            {
                                var NganHang = ListNganHang.FirstOrDefault(a => a.IdGuid == NhanVien.TenNganHang);
                                if (NganHang != null)
                                {
                                    item.NganHang = NganHang.Ten;
                                    if (!string.IsNullOrEmpty(NganHang.IdNganHangNguon))
                                        item.NganHangNguon = ListNganHang.Where(a => a.IdGuid == NganHang.IdNganHangNguon).Select(a => a.Ten).FirstOrDefault();
                                }


                            }
                            item.MaSoThue = NhanVien.MaSoThueTNCN;
                            item.MaNhanVien = NhanVien.MaNhanVien;
                            item.TenNhanVien = NhanVien.TenDayDu;
                            if (NhanVien.NgayNghiViec != null && NhanVien.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                item.NgayNghiViec = NhanVien.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                            if (NhanVien.NgayBatDauLamViec != null && NhanVien.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                item.NgayVaoLam = NhanVien.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");

                            var boPhan = listboPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                            if (boPhan != null)
                                item.TenBoPhan = boPhan.TenBoPhan;
                            var ChucVu = listChucVu.FirstOrDefault(a => a.Id == NhanVien.IdChucVu);
                            if (ChucVu != null)
                                item.TenChucVu = ChucVu.TenChucVu;
                            var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh);
                            if (ChucDanh != null)
                                item.TenChucDanh = ChucDanh.Ten;
                            var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == NhanVien.IdGuid).Select(a => a.IdBoPhan).ToList();
                            var CoSo = listDuAn.Where(a => listIdCoSo.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                            if (CoSo != null)
                                item.MaCoSo = string.Join(",", CoSo);
                        }
                    }
                    //QuyTrinh.listHeader = new List<clsBangTinhLuong_Header>();
                    if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                    {
                        QuyTrinh.listNhanVien = QuyTrinh.listNhanVien.OrderBy(a => a.TenBoPhan).ThenBy(a => a.NgayVaoLam).ToList();
                        QuyTrinh = GetListHeaderNew(QuyTrinh, QuyTrinh.listNhanVien.FirstOrDefault().listTangCa, QuyTrinh.listNhanVien.FirstOrDefault().listGioTieuChuan, QuyTrinh.IdCongTyEOS, db, QuyTrinh.listIdBoPhan);
                        if (Host == "ICT")
                            QuyTrinh.listGiaTriNhanVien = GetListGiatri(QuyTrinh.listHeader_Data, QuyTrinh.listNhanVien, listNhanVien, listHopDong, db);
                        else
                            QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, QuyTrinh.listNhanVien, listNhanVien, listHopDong, db);
                    }
                    List<string> listBP = QuyTrinh.listGiaTriNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                    QuyTrinh.listBoPhan = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a }).Distinct().ToList();
                    foreach (var item in QuyTrinh.listBoPhan)
                    {
                        if (string.IsNullOrEmpty(item.TenBoPhan))
                        {
                            //item.listNhanVien = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();
                            item.listGiaTri = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).FirstOrDefault().listGiaTri;
                            item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();
                            item.TenBoPhan = "Tá»”NG";

                        }
                        else
                        {
                            var listNhanViendl = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();

                            if (Host == "ICT")
                            {
                                item.listNhanVien = new List<clsBangTinhLuong_NhanVien>(); new List<clsBangTinhLuong_GiaTri>();
                                clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                                Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                foreach (var header in QuyTrinh.listHeader_Data)
                                {
                                    var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                    var listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                    if (header.Ma == "HoTro")
                                        listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                    if (header.Ma == "CongGioTieuChuan")
                                        listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                    if (listCount.Count > 0)
                                        itemAdd.GiaTri = listCount.Sum(a => a);
                                    //itemAdd.Ma = "TONG";
                                    Add.listGiaTri.Add(itemAdd);
                                }
                                item.listNhanVien.Add(Add);
                                item.listNhanVien.AddRange(QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList());
                            }
                            else
                            {
                                int SapXep = listboPhan.FirstOrDefault(a => a.TenBoPhan == item.TenBoPhan).ThuTu ?? 0;
                                item.SapXep = SapXep;
                                listBP = listNhanViendl.Select(a => a.TenChucDanh).Distinct().ToList();
                                {
                                    item.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                    foreach (var header in QuyTrinh.listHeader_Data)
                                    {
                                        var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                        var listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "HoTro")
                                            listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "CongGioTieuChuan")
                                            listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                        if (listCount.Count > 0)
                                            itemAdd.GiaTri = listCount.Sum(a => a);
                                        //itemAdd.Ma = "TONG";
                                        item.listGiaTri.Add(itemAdd);
                                    }
                                }
                                item.lischucDanh = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a, listNhanVien = new List<clsBangTinhLuong_NhanVien>() }).Distinct().ToList();
                                foreach (var chucDanh in item.lischucDanh)
                                {
                                    var listNhanVienCD = listNhanViendl.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).ToList();

                                    clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                                    Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                    foreach (var header in QuyTrinh.listHeader_Data)
                                    {
                                        var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                        var listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "HoTro")
                                            listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "CongGioTieuChuan")
                                            listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                        if (listCount.Count > 0)
                                            itemAdd.GiaTri = listCount.Sum(a => a);
                                        //itemAdd.Ma = "TONG";
                                        Add.listGiaTri.Add(itemAdd);
                                    }
                                    chucDanh.listNhanVien.Add(Add);
                                    chucDanh.listNhanVien.AddRange(listNhanViendl.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList());
                                }

                            }
                        }
                    }
                    QuyTrinh.listBoPhan = QuyTrinh.listBoPhan.OrderBy(a => a.SapXep).ToList();
                }
                return QuyTrinh;
            }
            catch (Exception ex)
            {
                string ms = "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½" + ex.Message;
                return ms;
            }
        }
        public bool UpdateQuyTrinhTinhLuong(HRM_QuyTrinhTinhLuong model, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {

            try
            {

                if (model.listIdBoPhan != null && model.listIdBoPhan.Count > 0)
                    model.IdBoPhan = string.Join(",", model.listIdBoPhan);
                if (model.listIdKyChamCong != null && model.listIdKyChamCong.Count > 0)
                    model.IdKyChamCong = string.Join(",", model.listIdKyChamCong);
                if (model.TuNgayUnix > 0)
                    model.TuNgay = dungchung.UnixTimeStampToDateTime(model.TuNgayUnix);
                if (model.DenNgayUnix > 0)
                    model.DenNgay = dungchung.UnixTimeStampToDateTime(model.DenNgayUnix);
                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    model.IdDuAn = model.IdCongTyEOS;
                    if (string.IsNullOrEmpty(model.eAction))
                        model.eAction = DefineData.eAction.QUYTRINHTINHLUONG.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, model, err))
                        return false;
                    db.HRM_QuyTrinhTinhLuong.Add(model);

                }
                else
                {
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                var listNhanVienOld = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                if (listNhanVienOld != null && listNhanVienOld.Count > 0)
                    db.HRM_QuyTrinhTinhLuong_NhanVien.RemoveRange(listNhanVienOld);
                var listPhuCapOld = db.HRM_QuyTrinhTinhLuong_PhuCap.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                if (listPhuCapOld != null && listPhuCapOld.Count > 0)
                    db.HRM_QuyTrinhTinhLuong_PhuCap.RemoveRange(listPhuCapOld);
                var listBaoHiemNgoaiOld = db.HRM_QuyTrinhTinhLuong_BaoHiemNgoai.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                if (listBaoHiemNgoaiOld != null && listBaoHiemNgoaiOld.Count > 0)
                    db.HRM_QuyTrinhTinhLuong_BaoHiemNgoai.RemoveRange(listBaoHiemNgoaiOld);
                var listSoGiaLamViecOld = db.HRM_QuyTrinhTinhLuong_SoGioLamViec.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                if (listSoGiaLamViecOld != null && listSoGiaLamViecOld.Count > 0)
                    db.HRM_QuyTrinhTinhLuong_SoGioLamViec.RemoveRange(listSoGiaLamViecOld);
                var listTangCaOld = db.HRM_QuyTrinhTinhLuong_TangCa.Where(a => a.IdQuyTrinh == model.IdGuid).ToList();
                if (listTangCaOld != null && listTangCaOld.Count > 0)
                    db.HRM_QuyTrinhTinhLuong_TangCa.RemoveRange(listTangCaOld);

                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                var ListBangChamCong = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(r => r.IdBangTinhLuong == model.IdGuid).ToList();
                db.HRM_QuyTrinhTinhLuong_ChamCong.RemoveRange(ListBangChamCong);

                if (model.listIdBoPhan != null && model.listIdBoPhan.Count > 0)
                {
                    foreach (var item in model.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = model.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_QuyTrinhTinhLuong);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                if (model.listIdKyChamCong != null && model.listIdKyChamCong.Count > 0)
                {
                    foreach (var item in model.listIdKyChamCong)
                    {
                        HRM_QuyTrinhTinhLuong_ChamCong cc = new HRM_QuyTrinhTinhLuong_ChamCong();
                        cc.IdGuid = Guid.NewGuid().ToString();
                        cc.IdBangChamCong = item;
                        cc.IdBangTinhLuong = model.IdGuid;
                        db.HRM_QuyTrinhTinhLuong_ChamCong.Add(cc);
                    }
                }

                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {

                    if (Host == "ICT")
                    {
                        if (model.listBoPhan != null && model.listBoPhan.Count > 0)
                            model.listGiaTriNhanVien = model.listBoPhan.SelectMany(a => a.listNhanVien).ToList();
                    }
                    else
                    {
                        if (model.listBoPhan != null && model.listBoPhan.Count > 0)
                            model.listGiaTriNhanVien = model.listBoPhan.Where(a => a.lischucDanh != null).SelectMany(b => b.lischucDanh.Where(a => a.listNhanVien != null).SelectMany(a => a.listNhanVien)).ToList();
                    }
                    List<HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu> listADD = new List<HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu>();
                    string IdNhanVien = "";
                    foreach (var item in model.listNhanVien)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdQuyTrinh = model.IdGuid;
                        item.IdCongTyEOS = model.IdCongTyEOS;
                        item.isKetThuc = model.isKetThuc;
                        if (IdNhanVien != item.IdNhanVien)
                        {
                            var GiaTri = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TruyHoan");
                            var GiaTriTC = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "ThuCoc");
                            var GiaTriTU = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TruyThu");
                            if (GiaTriTU != null)
                            {
                                item.TamUng = GiaTriTU.GiaTri ?? 0;
                            }

                            var HoTroKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroKhac");
                            if (HoTroKhac != null)
                            {
                                item.HoTroKhac = HoTroKhac.GiaTri ?? 0;
                            }
                            var HoTroNghiViec = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroNghiViec");
                            if (HoTroNghiViec != null)
                            {
                                item.HoTroNghiViec = HoTroNghiViec.GiaTri ?? 0;
                            }
                            var DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "DCKhac");
                            if (DCKhac != null)
                            {
                                item.DCKhac = DCKhac.GiaTri ?? 0;
                            }
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TienBienChe");
                            if (DCKhac != null)
                            {
                                item.TienBienChe = DCKhac.GiaTri ?? 0;
                            }
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "CongNghiKeHoach");
                            if (DCKhac != null)
                            {
                                item.CongNghiKeHoach = DCKhac.GiaTri ?? 0;
                                item.LuongNghiKeHoach = 4410000 / 26 * item.CongNghiKeHoach;
                            }
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroPhongSach");
                            if (DCKhac != null)
                            {
                                item.HoTroPhongSach = DCKhac.GiaTri ?? 0;
                            }
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TA_DonGia");
                            if (DCKhac != null)
                                item.TA_DonGia = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TA_SoCong");
                            if (DCKhac != null)
                                item.TA_SoCong = DCKhac.GiaTri ?? 0;
                            item.TA_ThanhTien = (item.TA_DonGia ?? 0) * (item.TA_SoCong ?? 0);

                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroCapPhatBHLD");
                            if (DCKhac != null)
                                item.HoTroCapPhatBHLD = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "CongOm");
                            if (DCKhac != null)
                                item.CongOm = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TK_XuatAn");
                            if (DCKhac != null)
                                item.TK_XuatAn = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TK_DonGia");
                            if (DCKhac != null)
                                item.TK_DonGia = DCKhac.GiaTri ?? 0;
                            item.TK_TruAnCa = (item.TK_DonGia ?? 0) * (item.TK_XuatAn ?? 0);
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "GiamDanhGia");
                            if (DCKhac != null)
                                item.GiamDanhGia = DCKhac.GiaTri ?? 0;

                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LiXiTrucTet");
                            if (DCKhac != null)
                                item.LiXiTrucTet = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LuongDanhGia");
                            if (DCKhac != null)
                                item.LuongDanhGia = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroBHYT");
                            if (DCKhac != null)
                                item.HoTroBHYT = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "HoTroNhatCuaRoi");
                            if (DCKhac != null)
                                item.HoTroNhatCuaRoi = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "GiamTruDoViPham");
                            if (DCKhac != null)
                                item.GiamTruDoViPham = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "ThanhToanBHCN");
                            if (DCKhac != null)
                                item.ThanhToanBHCN = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TruTienBHCN");
                            if (DCKhac != null)
                                item.TruTienBHCN = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "CongNghiKeHoach");
                            if (DCKhac != null)
                                item.CongNghiKeHoach = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LuongNghiKeHoach");
                            if (DCKhac != null)
                                item.LuongNghiKeHoach = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LuongDem");
                            if (DCKhac != null)
                                item.LuongTheoCongDem = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "CongDem");
                            if (DCKhac != null)
                                item.CongDem = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LuongLe");
                            if (DCKhac != null)
                                item.LuongLe = DCKhac.GiaTri ?? 0;
                            DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "LeTet");
                            if (DCKhac != null)
                                item.Le_Tet = DCKhac.GiaTri ?? 0;

                            // PHT
                            {
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TruyThuBHXH");
                                if (DCKhac != null)
                                {
                                    item.TruyThu = DCKhac.GiaTri ?? 0;
                                }
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "TraTienDP");
                                if (DCKhac != null)
                                {
                                    item.TraTienDongPhuc = DCKhac.GiaTri ?? 0;
                                }
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "ThueTNCN");
                                if (DCKhac != null)
                                {
                                    item.ThueTNCNTheoLuyTien = DCKhac.GiaTri ?? 0;
                                }
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "ThuDongPhuc");
                                if (DCKhac != null)
                                {
                                    item.DongPhuc = DCKhac.GiaTri ?? 0;
                                }
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "PhuCapKhac");
                                if (DCKhac != null)
                                {
                                    item.PhuCapKhongTheoCongChinh = DCKhac.GiaTri ?? 0;
                                }

                            }
                            //Giang bá»• sung ngÃ y 27/06/2025
                            {
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "DanhGia");
                                if (DCKhac != null)
                                    item.LuongDanhGia = DCKhac.GiaTri ?? 0;
                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "CongDem");
                                if (DCKhac != null)
                                    //Giang sá»­a ngÃ y 27/06/2025 cÃ´ng Ä‘Ãªm lÃ  tiá»n ko pháº£i sá»‘ ngÃ y cÃ´ng Ä‘em
                                    //item.NgayCongDem = DCKhac.GiaTri ?? 0;
                                    item.CongDem = DCKhac.GiaTri ?? 0;

                                DCKhac = model.listGiaTriNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien).listGiaTri.FirstOrDefault(a => a.Ma == "Thuong");
                                if (DCKhac != null)
                                    item.Thuong = DCKhac.GiaTri ?? 0;
                                item.LuongTuNhap = item.DanhGia + item.CongDem + item.Thuong;
                            }
                            //
                            if (GiaTriTC != null)
                            {
                                item.DongPhuc = GiaTriTC.GiaTri;
                            }

                            if (GiaTri != null)
                            {
                                item.TienTruyHoan = GiaTri.GiaTri ?? 0;
                                //item.TienThucNhan += item.TienTruyHoan;
                            }
                        }
                        else
                        {
                            IdNhanVien = item.IdNhanVien;
                        }
                        if (item.listPhuCap != null && item.listPhuCap.Count > 0)
                        {
                            foreach (var pc in item.listPhuCap)
                            {
                                pc.IdGuid = Guid.NewGuid().ToString();
                                pc.IdNhanVien = item.IdNhanVien;
                                pc.IdQuyTrinh = model.IdGuid;

                            }
                            db.HRM_QuyTrinhTinhLuong_PhuCap.AddRange(item.listPhuCap);

                        }
                        if (item.listBaoHiemNgoai != null && item.listBaoHiemNgoai.Count > 0)
                        {
                            foreach (var pc in item.listBaoHiemNgoai)
                            {
                                pc.IdGuid = Guid.NewGuid().ToString();
                                pc.IdNhanVien = item.IdNhanVien;
                                pc.IdQuyTrinh = model.IdGuid;

                            }
                            db.HRM_QuyTrinhTinhLuong_BaoHiemNgoai.AddRange(item.listBaoHiemNgoai);
                        }
                        if (item.listGioTieuChuan != null && item.listGioTieuChuan.Count > 0)
                        {
                            foreach (var pc in item.listGioTieuChuan)
                            {
                                pc.IdGuid = Guid.NewGuid().ToString();
                                pc.IdNhanVien = item.IdNhanVien;
                                pc.IdQuyTrinh = model.IdGuid;

                            }
                            db.HRM_QuyTrinhTinhLuong_SoGioLamViec.AddRange(item.listGioTieuChuan);
                        }
                        if (item.listTangCa != null && item.listTangCa.Count > 0)
                        {
                            foreach (var pc in item.listTangCa)
                            {
                                pc.IdGuid = Guid.NewGuid().ToString();
                                pc.IdNhanVien = item.IdNhanVien;
                                pc.IdQuyTrinh = model.IdGuid;

                            }
                            db.HRM_QuyTrinhTinhLuong_TangCa.AddRange(item.listTangCa);
                        }

                        item.CacKhoanCongTyThanhToan = item.listGioTieuChuan.Sum(a => a.Luong) + item.listPhuCap.Sum(a => a.SoTienPhuCap) + (item.LuongDanhGia ?? 0) + (item.LuongLe) + (item.LuongLeKDL) + (item.LuongTheoCongDem) + (item.Thuong ?? 0) + (item.LuongNghiKeHoach ?? 0) + (item.LuongPhep) + (item.Luong50 ?? 0) + (item.Luong100 ?? 0) + (item.TA_ThanhTien ?? 0) + (item.HoTroCapPhatBHLD ?? 0) + (item.HoTroBHYT ?? 0) + (item.HoTroNhatCuaRoi ?? 0) - (item.GiamTruDoViPham ?? 0);

                        item.TongLuong = item.LuongTheoCong + item.LuongTangCa + item.LuongPhep + item.LuongNghiBu + item.LuongLe + item.LuongLeKDL + item.PhuCapCongChinh + item.PhuCapKhongTheoCongChinh + item.DCKhac + item.HoTroKhac + item.HoTroNghiViec;
                        if (item.MucDongBaoHiemXaHoi == 0 && item.isCamKet == false)
                            item.KhauTruTaiNguon = item.TongLuong * 0.1;
                        item.TongGiamThueTNCN = item.ThueTNCNTheoLuyTien + item.KhauTruTaiNguon;
                        if (item.TongGiamThueTNCN < 50000)
                            item.TongGiamThueTNCN = 0;
                        item.TienThucNhan = item.TongLuong - item.ThueTNCNTheoLuyTien - item.TongNguoiNLD - (item.TongGiamThueTNCN ?? 0) - (item.TongGiamThueTNCN ?? 0);
                        if (item.listTangCa != null && item.listTangCa.Count > 0)
                            item.CacKhoanCongTyThanhToan += item.listTangCa.Sum(a => a.Luong);
                        if (model.isKetThuc == true)
                        {
                            double TienThucNhan = item.TongLuong + (item.ThuongLuong ?? 0) - (item.TienCongDoan ?? 0) - item.ThueTNCNTheoLuyTien - (item.Phat ?? 0) - item.TongNguoiNLD - (item.DongPhuc ?? 0) + (item.TraTienDongPhuc ?? 0) - item.TruyThu;
                            if (TienThucNhan < 0)
                            {
                                HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu ItemAdd = new HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu();
                                ItemAdd.IdNhanVien = item.IdNhanVien;
                                ItemAdd.IdGuid = Guid.NewGuid().ToString();
                                ItemAdd.SoTien = 0 - TienThucNhan;
                                listADD.Add(ItemAdd);
                            }
                        }
                    }
                    if (model.isKetThuc == true)
                    {
                        var listIdNhanVien = model.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                        var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid) && a.IsDaThuCoc == false).ToList();
                        foreach (var item in listNhanVien)
                        {
                            var check = model.listNhanVien.Where(a => a.IdNhanVien == item.IdGuid).FirstOrDefault();
                            item.ThuCoc = item.ThuCoc - check.DongPhuc;
                            if (item.ThuCoc == 0)
                                item.IsDaThuCoc = true;
                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        var listTruyThu = db.HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                        if (listTruyThu != null && listTruyThu.Count > 0)
                            db.HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu.RemoveRange(listTruyThu);
                        if (listADD != null && listADD.Count > 0)
                            db.HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu.AddRange(listADD);
                    }
                    db.HRM_QuyTrinhTinhLuong_NhanVien.AddRange(model.listNhanVien);
                }
                if (!base.SetQuyTrinh(db, _CallWebService, model, eLoaiHam, err))
                    return false;
                db.SaveChanges();
                return true;


            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }


        }
        public dynamic ChuyenTiepQuyTrinhTinhLuong(HRM_QuyTrinhTinhLuong item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    {
                        #region check bá»™ pháº­n Ä‘Ã£ tÃ­nh lÆ°Æ¡ng
                        var list = (from a in db.HRM_QuyTrinhTinhLuong
                                    join b in db.HRM_QuyTrinhTinhLuong_ChamCong on a.IdGuid equals b.IdBangTinhLuong
                                    //join c in db.HRM_QuyTrinhBoPhan_BoPhan on a.IdGuid equals c.IdQuyTrinh
                                    where item.listIdKyChamCong.Contains(b.IdBangChamCong) && a.IdGuid != item.IdGuid
                                    select a.IdGuid
                                           ).ToList();
                        if (list != null && list.Count > 0)
                        {

                            var listIdBoPhanCheck = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => list.Contains(a.IdQuyTrinh)).Select(a => a.IdBoPhan).ToList();
                            if ((listIdBoPhanCheck == null || listIdBoPhanCheck.Count == 0))
                            {

                                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Ká»³ Cháº¥m cÃ´ng Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³");
                            }
                            else if (item.listIdBoPhan.Count == 0)
                            {
                                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhanCheck);
                                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Ká»³ Cháº¥m cÃ´ng Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³: " + string.Join(", ", listBoPhan.Select(a => a.TenBoPhan).ToList()));
                            }
                            else
                            {
                                List<long> listTrung = listIdBoPhanCheck.Intersect(item.listIdBoPhan).ToList();
                                if (listTrung != null && listTrung.Count > 0)
                                {
                                    var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listTrung);
                                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Bá»™ pháº­n Ä‘Ã£ Ä‘Æ°á»£c táº¡o báº£ng lÆ°Æ¡ng trÆ°á»›c Ä‘Ã³: " + string.Join(", ", listBoPhan.Select(a => a.TenBoPhan).ToList()));
                                }
                            }

                        }

                        #endregion
                    }
                    HRM_QuyTrinhTinhLuong pcuDatabase = db.HRM_QuyTrinhTinhLuong.AsNoTracking().Where(x => x.IdGuid == item.IdGuid).FirstOrDefault();
                    item.NoiDung = item.TenBangLuong;
                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, item, err, item.IdCongTyEOS))
                        return err;
                    bool Error = UpdateQuyTrinhTinhLuong(item, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng");
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, item, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {

                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic KhongDuyetQuyTrinhTinhLuong(HRM_QuyTrinhTinhLuong item, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    HRM_QuyTrinhTinhLuong pcuDatabase = db.HRM_QuyTrinhTinhLuong.AsNoTracking().Where(x => x.IdGuid == item.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, item, err))
                        return err;

                    bool Error = UpdateQuyTrinhTinhLuong(item, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error == false)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cáº­p nháº­t dá»¯ liá»‡u khÃ´ng thÃ nh cÃ´ng");
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, item))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic DeleteQuyTrinhTinhLuong(string Id, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_QuyTrinhTinhLuong chamCong = db.HRM_QuyTrinhTinhLuong.FirstOrDefault(a => a.IdGuid == Id);
                    var listNhanVienOld = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => a.IdQuyTrinh == Id).ToList();
                    if (listNhanVienOld != null && listNhanVienOld.Count > 0)
                        db.HRM_QuyTrinhTinhLuong_NhanVien.RemoveRange(listNhanVienOld);
                    var listPhuCapOld = db.HRM_QuyTrinhTinhLuong_PhuCap.Where(a => a.IdQuyTrinh == Id).ToList();
                    if (listPhuCapOld != null && listPhuCapOld.Count > 0)
                        db.HRM_QuyTrinhTinhLuong_PhuCap.RemoveRange(listPhuCapOld);
                    db.HRM_QuyTrinhTinhLuong.Remove(chamCong);
                    var ListBangChamCong = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(r => r.IdBangTinhLuong == Id).ToList();
                    db.HRM_QuyTrinhTinhLuong_ChamCong.RemoveRange(ListBangChamCong);
                    db.SaveChanges();
                    _CallWebService.AddModify(db, DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHTINHLUONG.ToString()), DefineData.eModify.Xoa.ToString(), nameof(HRM_QuyTrinhTinhLuong), chamCong.NoiDung, Id, "", Id, 0, db.currentUser.Id, db.currentUser.TenNhanVien);

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "XÃ³a báº£ng tÃ­nh lÆ°Æ¡ng thÃ nh cÃ´ng");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra trong quÃ¡ trÃ¬nh xá»­ lÃ½ " + ex.Message);
                }
            }
        }
        public dynamic GetTinhLuongNhanVienTheoBoPhan(long IdBoPhan, int Thang, int Nam, ApplicationDbContext db)
        {
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            DateTime NgayBD = new DateTime(Nam, Thang, 1);
            DateTime NgayKT = NgayBD.AddMonths(1).AddDays(-1);

            var listData = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => a.IdBoPhanEOS == IdBoPhan && a.NgayChamCong.Value.Year == Nam && a.NgayChamCong.Value.Month == Thang).ToList();
            if (listData != null && listData.Count > 0)
            {
                List<string> listIdNhanVien = listData.Select(x => x.IdGuid).ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).ToList();
                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                List<long> IdNhanVienEOS = listNhanVien.Select(a => a.IdNhanVienEOS).Distinct().ToList();
                List<Link_NhanVien_BoPhan_ChucVu> listChucVu = _CallWebService.GetDanhSachChucVuTheoListIdNhanVien(db, IdNhanVienEOS);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Created).ToList();
                List<string> listIdCaLamViec = listHopDong.Select(a => a.IdHinhThucLamViec).Distinct().ToList();

                var caLamViecs = db.HRM_dmLichLamViec.Where(a => listIdCaLamViec.Contains(a.IdGuid)).ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderByDescending(a => a.Created).ToList();
                var listPhuCap = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong) && a.NgayBatDau <= NgayBD && a.NgayKetThuc >= NgayKT).ToList();
                var listPhuCapBaoHiem = db.HRM_HopDongPhuCapBaoHiem.Where(a => listIdHopDong.Contains(a.IdHopDong) && a.NgayBatDau <= NgayBD && a.NgayKetThuc >= NgayKT).ToList();
                var listLuong = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                var listLuongBaoHiem = db.HRM_LuongBaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();

                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi.Month == Thang && a.NgayNghi.Year == Nam).ToList();

                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    var datas = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                    if (datas != null && datas.Count > 0)
                    {
                        HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                        itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                        {
                            itemAdd.MaCoSo = CongTy.MaBoPhan;
                        }
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        itemAdd.PhuCapCongChinh = 0;
                        if (HopDong != null)
                        {
                            var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                            if (loaiHopDong != null)
                            {
                                itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                            }
                            var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdHinhThucLamViec);
                            if (CaLamViec != null)
                            {
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                itemAdd.CaLamViec = CaLamViec.Ten;
                                itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                            }

                            var Luong = listLuong.FirstOrDefault(a => a.IdHopDong == HopDong.IdGuid);
                            if (Luong != null)
                            {
                                itemAdd.LuongCoBan = Luong.LuongBaoHiem;
                                itemAdd.LuongDongBHXH = Luong.LuongBaoHiem;

                            }
                            var LuongBH = listLuongBaoHiem.FirstOrDefault(a => a.IdHopDong == HopDong.IdGuid);
                            if (Luong != null)
                            {
                                itemAdd.LuongDongBHXH = Luong.LuongBaoHiem;
                            }
                            // tÃ­nh Cong
                            itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / itemAdd.SoGioMotNgay);
                            itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / itemAdd.SoGioMotNgay);
                            // phá»¥ cáº¥p
                            var PhuCaps = listPhuCap.Where(a => a.IdHopDong == HopDong.IdGuid).ToList();
                            double TongSoCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay;
                            foreach (var phucap in listPhuCapdb)
                            {
                                HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                                itemPC.IdNhanVien = item.IdGuid;
                                if (PhuCaps != null && PhuCaps.Count > 0)
                                {
                                    var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                    if (pc != null)
                                    {
                                        itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                        itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                        if (itemAdd.NgayCongTieuChuan > 0)
                                            itemPC.SoTienPhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien * 100;
                                        if (pc.LoaiPhuCap == 5 && TongSoCong > 15)
                                        {
                                            itemPC.SoTienPhuCap = pc.SoTien;
                                        }
                                        if (phucap.isTinhThueTNCN == false)
                                            itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + itemPC.SoTienPhuCap;
                                    }
                                }
                                itemAdd.listPhuCap.Add(itemPC);

                            }
                        }

                        // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                        double Cong1Ngay = Math.Round(itemAdd.LuongCoBan / itemAdd.NgayCongTieuChuan, 3);

                        itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                        if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                        {
                            itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid).Count();
                            itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                        }
                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                        var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdGuid && (a.TuNgay.Month <= Thang || a.DenNgay.Month >= Thang)).ToList();
                        if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                        {
                            List<int> Ngays = new List<int>();
                            foreach (var ngay in NghiLeChiTiets)
                            {
                                DateTime time = ngay.TuNgay;
                                while (time <= ngay.DenNgay)
                                {
                                    if (time.Month == Thang)
                                        Ngays.Add(time.Day);
                                    time = time.AddDays(1);
                                }
                            }
                            var data = datas.Where(a => Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                            if (data != null && data.Count > 0)
                            {
                                itemAdd.Le_Tet = data.Count;
                                itemAdd.LuongLe = 0;
                                Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    if (ngay.TyLeLuongLamThem > 1)
                                    {
                                        DateTime time = ngay.TuNgay;
                                        while (time <= ngay.DenNgay)
                                        {
                                            if (Ngays.Contains(time.Day))
                                                itemAdd.LuongLe = itemAdd.LuongLe + Cong1Ngay * (ngay.TyLeLuongLamThem - 1);
                                        }
                                    }
                                }
                            }
                        }

                        // tÄƒng ca
                        itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / itemAdd.SoGioMotNgay); ;
                        itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;

                        itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe;
                        // báº£o hiá»ƒm xÃ£ há»™i
                        itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                        itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                        itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                        itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                        itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                        itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                        itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;


                        // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                        var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGiamTruBanThan = 11000000;
                        itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SpNguoiDangKyNPT * 4400000;
                        itemAdd.ThuNhapMienThue = itemAdd.PhuCapCongChinh + itemAdd.TongNguoiNLD;
                        if (itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                            itemAdd.ThuNhapTinhThueTNCN = itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                        else
                            itemAdd.ThuNhapTinhThueTNCN = 0;
                        itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                        itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD;
                        listResult.Add(itemAdd);
                    }
                }
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                listResult.Add(Add);
            }
            return listResult;
        }
        public dynamic GetTinhLuongNhanVienTheoKyChamCong(long IdCongty, List<long> IdBoPhan, List<string> IdKyChamCong, int LoaiLuong, string eAction, ApplicationDbContext db)
        {
            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            if (Host == "ICT")
                QuyTrinh = GetTinhLuongNhanVienTheoKyChamCongICT(IdCongty, IdBoPhan, IdKyChamCong, LoaiLuong, db, eAction);
            else
                QuyTrinh = GetTinhLuongNhanVienTheoKyChamCongPHT(IdCongty, IdBoPhan, IdKyChamCong, LoaiLuong, db);
            List<string> listBP = QuyTrinh.listGiaTriNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
            QuyTrinh.listBoPhan = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a }).Distinct().ToList();
            var listboPhan = _CallWebService.GetDanhSachBoPhan(db);
            foreach (var item in QuyTrinh.listBoPhan)
            {
                if (string.IsNullOrEmpty(item.TenBoPhan))
                {
                    //item.listNhanVien = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();
                    item.listGiaTri = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).FirstOrDefault().listGiaTri;
                    item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();
                    item.TenBoPhan = "Tá»”NG";

                }
                else
                {

                    var listNhanVien = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();
                    if (Host == "ICT")
                    {
                        item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();

                        clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                        Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                        foreach (var header in QuyTrinh.listHeader_Data)
                        {
                            var itemAdd = new clsBangTinhLuong_GiaTri(header);
                            var listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                            if (header.Ma == "HoTro")
                                listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                            if (header.Ma == "CongGioTieuChuan")
                                listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                            if (listCount.Count > 0)
                                itemAdd.GiaTri = listCount.Sum(a => a);
                            //itemAdd.Ma = "TONG";
                            Add.listGiaTri.Add(itemAdd);
                        }
                        item.listNhanVien.Add(Add);
                        item.listNhanVien.AddRange(QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList());
                    }
                    else
                    {
                        int SapXep = listboPhan.FirstOrDefault(a => a.TenBoPhan == item.TenBoPhan).ThuTu ?? 0;
                        item.SapXep = SapXep;
                        listBP = listNhanVien.Select(a => a.TenChucDanh).Distinct().ToList();
                        {
                            item.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                            foreach (var header in QuyTrinh.listHeader_Data)
                            {
                                var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                var listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "HoTro")
                                    listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "CongGioTieuChuan")
                                    listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                if (listCount.Count > 0)
                                    itemAdd.GiaTri = listCount.Sum(a => a);
                                //itemAdd.Ma = "TONG";
                                item.listGiaTri.Add(itemAdd);
                            }
                        }
                        item.lischucDanh = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a, listNhanVien = new List<clsBangTinhLuong_NhanVien>() }).Distinct().ToList();
                        foreach (var chucDanh in item.lischucDanh)
                        {
                            var listNhanVienCD = listNhanVien.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).ToList();

                            clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                            Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                            foreach (var header in QuyTrinh.listHeader_Data)
                            {
                                var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                var listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "HoTro")
                                    listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "CongGioTieuChuan")
                                    listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                if (listCount.Count > 0)
                                    itemAdd.GiaTri = listCount.Sum(a => a);
                                //itemAdd.Ma = "TONG";
                                Add.listGiaTri.Add(itemAdd);
                            }
                            chucDanh.listNhanVien.Add(Add);
                            chucDanh.listNhanVien.AddRange(listNhanVien.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList());
                        }

                    }
                }
            }
            QuyTrinh.listBoPhan = QuyTrinh.listBoPhan.OrderBy(a => a.SapXep).ToList();
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", QuyTrinh);

        }
        public HRM_QuyTrinhTinhLuong GetTinhLuongNhanVienTheoKyChamCongICT(long IdCongty, List<long> IdBoPhan, List<string> IdKyChamCong, int LoaiLuong, ApplicationDbContext db, string eAction)
        {

            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            var BangChamCong = db.HRM_BangChamCong.Where(a => IdKyChamCong.Contains(a.IdGuid)).ToList();

            DateTime NgayBD = BangChamCong.OrderBy(a => a.TuNgay).FirstOrDefault().TuNgay.Value;
            NgayBD = new DateTime(NgayBD.Year, NgayBD.Month, NgayBD.Day, 0, 0, 0);
            DateTime NgayKT = BangChamCong.OrderByDescending(a => a.DenNgay).FirstOrDefault().DenNgay.Value;
            NgayKT = new DateTime(NgayKT.Year, NgayKT.Month, NgayKT.Day, 0, 0, 0);
            var listDataTG = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
            var listData = (from a in db.HRM_BangChamCong_NhanVien
                            where IdKyChamCong.Contains(a.IdBangChamCong)
                            group a by new { a.IdNhanVien, a.IdCongTyEOS } into g
                            select g.FirstOrDefault()
                           ).ToList();
            var ThongTinDuAn = _CallWebService.GetThongTinCaLamViecDuAn(IdCongty, db);

            if (listData != null && listData.Count > 0)
            {
                List<string> listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
                IdKyChamCong = listData.Select(a => a.IdBangChamCong).Distinct().ToList();
                var tangCas = db.HRM_BangChamCongNhanVien_TangCa.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                var SoGioLamViecs = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                var SoGioLamViecKhongTrongKy = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && !IdKyChamCong.Contains(a.IdBangChamCong) && a.TuNgay >= NgayBD && a.DenNgay <= NgayKT).ToList();

                var listUser = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(IdCongty, DateTimeToUnixTimeStamp(NgayBD), DateTimeToUnixTimeStamp(NgayKT), db);

                var listThuong = db.HRM_QuyTrinh_PhucLoi_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isKetThuc == true && a.isTinhLuong == true && NgayBD <= a.Ngay && NgayKT >= a.Ngay).ToList();
                //var listPhat = db.HRM_Luong_KhenThuongKyLuat.Where(a => a.Loai == 1 && listIdNhanVien.Contains(a.IdNhanVien) && a.IdCongTy == IdCongty && NgayBD <= a.KyLuongThucHien && NgayKT >= a.KyLuongThucHien).ToList();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                //Giang sá»­a láº¥y nhÃ¢n viÃªn theo Bo pháº­n 14/04/2025
                var ChucVus = _CallWebService.GetDanhSachChucVu(db);

                if (IdBoPhan.Count > 0)
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid) && IdBoPhan.Contains(x.IdBoPhanEOS)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                var ChucVuCheck = new ChucVu();
                if (eAction == DefineData.eAction.QUYTRINHTINHLUONGCONGNHAN.ToString())
                {
                    ChucVuCheck = ChucVus.Where(a => a.MaChucVu == "CN").FirstOrDefault();
                    listNhanVien = db.HRM_NhanVien.Where(x => x.IdChucVu == ChucVuCheck.Id).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                }
                else
                if (eAction == DefineData.eAction.QUYTRINHTINHLUONGGIAMSAT.ToString())
                {
                    ChucVuCheck = ChucVus.Where(a => a.MaChucVu == "GS").FirstOrDefault();
                    string MaCongTySamSung = HrmOptions.MaCongTySamSung;
                    List<string> listma = MaCongTySamSung.Split(',').ToList();
                    var listDuAn = _CallWebService.GetDanhSachDuAn(db).Where(a => listma.Contains(a.MaBoPhan)).Select(a => a.Id).ToList();

                    var listIds = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh) && !listDuAn.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).ToList();
                    listNhanVien = db.HRM_NhanVien.Where(x => x.IdChucVu == ChucVuCheck.Id && listIds.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                }
                else
                if (eAction == DefineData.eAction.QUYTRINHTINHLUONGGIAMSATSAMSUNG.ToString())
                {
                    ChucVuCheck = ChucVus.Where(a => a.MaChucVu == "GS").FirstOrDefault();
                    string MaCongTySamSung = HrmOptions.MaCongTySamSung;
                    List<string> listma = MaCongTySamSung.Split(',').ToList();
                    var listDuAn = _CallWebService.GetDanhSachDuAn(db).Where(a => listma.Contains(a.MaBoPhan)).Select(a => a.Id).ToList();
                    var listIds = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh) && listDuAn.Contains(a.IdBoPhan)).Select(a => a.IdQuyTrinh).ToList();
                    listNhanVien = db.HRM_NhanVien.Where(x => x.IdChucVu == ChucVuCheck.Id && listIds.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                }
                else
                if (eAction == DefineData.eAction.QUYTRINHTINHLUONGNHANVIEN.ToString())
                {
                    ChucVuCheck = ChucVus.Where(a => a.MaChucVu == "CN").FirstOrDefault();
                    var ChucVuGS = ChucVus.Where(a => a.MaChucVu == "GS").FirstOrDefault();
                    listNhanVien = db.HRM_NhanVien.Where(x => x.IdChucVu != ChucVuCheck.Id && x.IdChucVu != ChucVuGS.Id).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                }


                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                List<long> listIdChucVu = listNhanVien.Select(a => a.IdChucVu).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Created).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listBaoHiemNgoai = db.HRM_BaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong) && a.isNgoai == true).ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc != null && a.NgayBatDauHieuLuc <= NgayBD && (a.NgayHetHieuLuc == null || (a.NgayHetHieuLuc.Value.Year != 1790 && a.NgayHetHieuLuc >= NgayKT))).OrderByDescending(a => a.Created).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayBD && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayKT)).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongty
                                                      && x.Ngay >= NgayBD && x.Ngay <= NgayKT).ToList();
                var listId = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
                if (listId.Count() <= listIdNhanVien.Count())
                {
                    var listLuongHetHan = (from a in db.HRM_Luong_NhanVien
                                           where listIdNhanVien.Contains(a.IdNhanVien) && !listId.Contains(a.IdNhanVien)
                                           group a by a.IdNhanVien into g
                                           select g.OrderByDescending(b => b.NgayBatDauHieuLuc).FirstOrDefault()
                                         ).ToList();
                    if (listLuongHetHan != null && listLuongHetHan.Count > 0)
                        listLuong.AddRange(listLuongHetHan);
                }
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var LoaiPheps = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.N100.ToString() || a.Ma == DefineData.eKyHieuChamCong.N50.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).ToList();
                List<string> IdLoaiPhep = LoaiPheps.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT /*&& a.IdCongTy == IdCongty*/ && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();
                var listNgayLamViec = new List<HRM_TheoDoiLichLamViecNhanVien>();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listTruyThu = db.HRM_QuyTrinhTinhLuong_TheoDoiLuongTruyThu.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                //var /*listDanhGia*/ = db.HRM_QuyTrinhDanhGia_NhanVien.Where(a => a.isKetThuc == true && a.Thang == NgayKT.Month && a.Nam == NgayKT.Year && listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                //var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                var listNoiCapBH = db.HRM_dmNoiCapBaoHiem.OrderBy(a => a.Created).ToList();
                //List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiBP = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongty && (a.Nam == NgayBD.Year || a.Nam == NgayKT.Year)).ToList();
                List<string> listIdLichNghi = listLichNghiBP.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghi = db.HRM_dmLichNghiLe.Where(a => listIdLichNghi.Contains(a.IdGuid) || (a.IdChucVu != null && listIdChucVu.Any(x => a.IdChucVu.Contains(x.ToString())))).OrderByDescending(a => a.Created).ToList();
                listIdLichNghi.AddRange(listLichNghi.Select(a => a.IdGuid).Distinct().ToList());
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                var ListNganHang = db.HRM_dmNganHang.ToList();
                DateTime time = DateTime.Now;
                time = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
                double TongT7 = Enumerable.Range(0, (NgayKT - NgayBD).Days + 1).Select(offset => NgayBD.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Saturday);
                double TongCN = Enumerable.Range(0, (NgayKT - NgayBD).Days + 1).Select(offset => NgayBD.AddDays(offset)).Count(date => date.DayOfWeek == DayOfWeek.Sunday);
                int TongSoNgay = (NgayKT - NgayBD).Days + 1;
                double TongSoCongKhac = (((int)(TongSoNgay / 6)) + (int)((TongSoNgay % 6) / 4)) * 4 + (TongSoNgay % 6) % 4;
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "18349")
                        item.MaNhanVien = "18349";
                    var dataTGs = listDataTG.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayChamCong).ToList();
                    var datas = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.Created).ToList();
                    if (datas != null && datas.Count > 0)
                    {
                        HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                        itemAdd.SoTaiKhoan = item.TaiKhoanNganHang;
                        if (!string.IsNullOrEmpty(item.TenNganHang))
                        {
                            var NganHang = ListNganHang.FirstOrDefault(a => a.IdGuid == item.TenNganHang);
                            if (NganHang != null)
                            {
                                itemAdd.NganHang = NganHang.Ten;
                                if (!string.IsNullOrEmpty(NganHang.IdNganHangNguon))
                                    itemAdd.NganHangNguon = ListNganHang.Where(a => a.IdGuid == NganHang.IdNganHangNguon).Select(a => a.Ten).FirstOrDefault();
                            }
                        }
                        // tÃ­nh Cong
                        itemAdd.NgayCong = datas.Sum(a => (a.TongSoCong ?? 0));
                        itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        itemAdd.IdNhanVien = item.IdGuid;
                        var IdUserDieuDong = listUser.FirstOrDefault(a => a.IdUser == item.IdUser);
                        if (IdUserDieuDong != null)
                            itemAdd.isDieuDong = IdUserDieuDong.isDieuDong;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        // thu cá»c
                        if (item.IsDaThuCoc == false)
                            itemAdd.DongPhuc = item.ThuCoc;
                        // thÆ°á»Ÿng
                        if (listThuong != null && listThuong.Count > 0)
                        {
                            //itemAdd.Thuong = listThuong.Where(a => a.IdNhanVien == item.IdGuid && a.isTinhVaoLuong == false).Sum(a => a.SoTien);
                            itemAdd.ThuongLuong = listThuong.Where(a => a.IdNhanVien == item.IdGuid/* && a.isTinhVaoLuong == true*/).Sum(a => a.SoTien * a.SoLuong);
                        }

                        if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                        {
                            itemAdd.MaCoSo = CongTy.TenBoPhan;
                        }
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                        {
                            itemAdd.TenChucDanh = ChucDanh.Ten;
                        }
                        var ChucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                        if (ChucVu != null)
                        {
                            itemAdd.TenChucVu = ChucVu.TenChucVu;
                            if (ChucVu.MaChucVu.Contains("CN"))
                                itemAdd.SoTienPCDiLai = 50000;
                            else
                                itemAdd.SoTienPCDiLai = 100000;
                        }

                        var Luong = listLuong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        if (Luong != null)
                        {
                            itemAdd.LoaiLuong = Luong.LoaiLuong;
                            itemAdd.LuongCoBan = Luong.Luong;

                            if (item.isLuongHuu == false)
                                itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                        }
                        //var DanhGia = listDanhGia.FirstOrDefault(a => a.IdNhanVien == item.IdGuid); Anh Duy báº£o Ä‘á»•i ngÃ y 21/7/2025
                        //if (DanhGia != null)
                        //{
                        //    var SoTienDG = ThongTinDuAn.itemThongTinDuAn.listHoTroChuyenCan.FirstOrDefault(a => a.XepHangDanhGia == DanhGia.KetQuaThucTe);
                        //    if (SoTienDG != null)
                        //        itemAdd.LuongDanhGia = SoTienDG.MucHoTro;
                        //}
                        //Pháº¡t
                        //if (listPhat != null && listPhat.Count > 0)
                        //{
                        //    var Phats = listPhat.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        //    if (Phats != null && Phats.Count > 0)
                        //    {
                        //        itemAdd.Phat = 0;
                        //        foreach (var phat in Phats)
                        //        {
                        //            if (phat.TyLe != null && phat.TyLe > 0)
                        //                itemAdd.Phat += itemAdd.LuongCoBan * phat.TyLe;
                        //            else
                        //                itemAdd.Phat += (phat.SoTien);
                        //        }
                        //    }
                        //}
                        itemAdd.listGioTieuChuan = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        itemAdd.NgayCongTieuChuan = 26;
                        itemAdd.SoGioMotNgay = 8;
                        if (SoGioLamViecs != null && SoGioLamViecs.Count > 0)
                        {
                            double NgayCongTheoThang = TongSoNgay;
                            itemAdd.TienCongDoan = item.PhiCongDoan;
                            itemAdd.SoGioMotNgay = SoGioLamViecs.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).OrderByDescending(a => a.TongSoGio).FirstOrDefault().GioTieuChuan;
                            var ca = caLamViecs.FirstOrDefault(a => a.IdGuid == SoGioLamViecs.Where(x => x.IdNhanVien == itemAdd.IdNhanVien).OrderByDescending(x => x.TongSoGio).FirstOrDefault().IddmCaLamViec);

                            if (ca.LoaiNgayNghi == 1)
                                NgayCongTheoThang = (double)(TongSoNgay - TongT7 - TongCN);
                            else if (ca.LoaiNgayNghi == 2 || ca.LoaiNgayNghi == 5)
                                NgayCongTheoThang = (double)(TongSoNgay - TongCN);
                            else if (ca.LoaiNgayNghi == 3)
                                NgayCongTheoThang = (double)(TongSoNgay - TongSoCongKhac);
                            else if (ca.LoaiNgayNghi == 4)
                                NgayCongTheoThang = (double)(TongSoNgay - TongCN - (TongT7 / 2));

                            itemAdd.NgayCongTieuChuan = NgayCongTheoThang;
                            var listCaLamViec = new List<HRM_dmLichLamViec>();
                            if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                            {
                                listCaLamViec = caLamViecs.Where(a => HopDong.IdThoiGianLamViec.Contains(a.IdGuid)).ToList();

                            }
                            if (eAction == DefineData.eAction.QUYTRINHTINHLUONGGIAMSATSAMSUNG.ToString())
                            {
                                var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                                if (CaLamViec == null)
                                {
                                    if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                        CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                    if (CaLamViec == null)
                                    {
                                        string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec) && a.Ngay >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.Ngay <= Luong.NgayHetHieuLuc))).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                        CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                    }
                                }
                                if (CaLamViec == null)
                                    CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                                {
                                    itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                }
                            }
                            itemAdd.listGioTieuChuan = SoGioLamViecs.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).Select(a => new HRM_QuyTrinhTinhLuong_SoGioLamViec
                            {
                                IdNhanVien = itemAdd.IdNhanVien,
                                IddmCaLamViec = a.IddmCaLamViec,
                                GioTieuChuan = a.GioTieuChuan,
                                CongTieuChuan = itemAdd.NgayCongTieuChuan,
                                TongSoGio = a.TongSoGio,

                            }).ToList();
                            foreach (var Gio in itemAdd.listGioTieuChuan)
                            {
                                Gio.Luong = (((itemAdd.LuongCoBan) / (Gio.CongTieuChuan)) * (Gio.TongSoGio / Gio.GioTieuChuan));
                                if (itemAdd.LoaiLuong == 2)
                                {
                                    Gio.Luong = (itemAdd.LuongCoBan * (Gio.TongSoGio / Gio.GioTieuChuan));
                                }
                            }
                        }
                        itemAdd.listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                        if (tangCas != null && tangCas.Count > 0)
                        {
                            itemAdd.listTangCa = tangCas.Where(a => a.IdNhanVien == itemAdd.IdNhanVien).Select(a => new HRM_QuyTrinhTinhLuong_TangCa
                            {
                                IdNhanVien = itemAdd.IdNhanVien,
                                HeSo = a.HeSo,
                                IddmHeSoTangCa = a.IddmHeSoTangCa,
                                Cong = a.Cong,
                                CongTieuChuan = a.CongTieuChuan,
                                TongSoGio = a.TongSoGio,
                                Luong = itemAdd.LoaiLuong == 1 ? (((itemAdd.LuongCoBan) / (itemAdd.NgayCongTieuChuan)) * a.Cong) * (a.HeSo / 100) : ((itemAdd.LuongCoBan * a.Cong) * (a.HeSo / 100))
                            }).ToList();

                        }


                        itemAdd.PhuCapCongChinh = 0;
                        itemAdd.PhuCapKhongTheoCongChinh = 0;
                        if (HopDong != null)
                        {
                            var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                            if (loaiHopDong != null)
                            {
                                itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                            }
                            //var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);



                        }

                        itemAdd.listBaoHiemNgoai = new List<HRM_QuyTrinhTinhLuong_BaoHiemNgoai>();
                        var BaoHiemNgoai = new HRM_BaoHiem_HopDong();
                        if (HopDong != null)
                            BaoHiemNgoai = listBaoHiemNgoai.FirstOrDefault(a => a.IdHopDong == HopDong.IdGuid);
                        var User = listUser.FirstOrDefault(a => a.IdUser == item.IdUser);
                        foreach (var BHN in listNoiCapBH)
                        {
                            HRM_QuyTrinhTinhLuong_BaoHiemNgoai itemBH = new HRM_QuyTrinhTinhLuong_BaoHiemNgoai();
                            itemBH.IdNoiCap = BHN.IdGuid;
                            if (BaoHiemNgoai != null && BaoHiemNgoai.NoiCapSoBaoHiem == BHN.IdGuid && User != null && User.isDieuDong == false)
                                itemBH.SoTien = BaoHiemNgoai.MucDongBaoHiem;
                            itemAdd.listBaoHiemNgoai.Add(itemBH);

                        }
                        // phá»¥ cáº¥p
                        var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.Created).ToList();
                        double TongSoCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay;
                        double PCDem = 0;

                        foreach (var phucap in listPhuCapdb)
                        {
                            HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                            itemPC.IdNhanVien = item.IdGuid;
                            itemPC.Ma = phucap.Ma;
                            if (phucap.Ma == "DANHGIA")
                            {
                                var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                if (pc != null)
                                {
                                    itemAdd.LuongDanhGia = pc.SoTien;
                                }
                                continue;
                            }
                            else
                              if (phucap.Ma == "HTCDP,NM")
                            {
                                var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                if (pc != null)
                                {
                                    itemAdd.HoTroCapPhatBHLD = pc.SoTien;
                                }
                                continue;
                            }
                            else

                            if (phucap.Ma == "LAMDEM" || phucap.Ma == "CHUYENCAN")
                            {
                                if (phucap.Ma == "LAMDEM")
                                {
                                    itemPC.IddmPhuCap = phucap.IdGuid;
                                    if (ThongTinDuAn.itemThongTinDuAn != null && ThongTinDuAn.itemThongTinDuAn.listPhuCapLamDem != null && ThongTinDuAn.itemThongTinDuAn.listPhuCapLamDem.Count > 0)
                                        itemPC.SoTienPhuCap = ThongTinDuAn.itemThongTinDuAn.listPhuCapLamDem.Where(a => a.IdBoPhan == item.IdBoPhanEOS && a.IdChucVu == item.IdChucVu).Select(a => a.MucHoTro).FirstOrDefault();
                                    PCDem = itemPC.SoTienPhuCap;
                                }
                            }
                            //else if (phucap.Ma == "THAMNIEN")
                            //{
                            //    itemPC.IddmPhuCap = phucap.IdGuid;
                            //    PCThamNien = itemPC.SoTienPhuCap;
                            //}
                            else

                            if (PhuCaps != null && PhuCaps.Count > 0)
                            {
                                var pc = PhuCaps.Where(a => a.IddmLoaiPhuCap == phucap.IdGuid).OrderByDescending(a => a.Created).FirstOrDefault();
                                if (pc != null)
                                {
                                    itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                    itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                    double PhuCap = 0;

                                    if (itemAdd.listGioTieuChuan.Count > 0 && pc.LoaiPhuCap == 1)
                                    {
                                        //PhuCapHienThi = pc.SoTien; /*itemAdd.listGioTieuChuan.Sum(a => pc.SoTien); *///Giang sá»­a, náº¿u theo thÃ¡ng Ä‘á»ƒ nguyÃªn phá»¥ cáº¥p
                                        PhuCap = pc.SoTien; /*itemAdd.listGioTieuChuan.Sum(a => (((pc.SoTien) / (a.CongTieuChuan == 0 ? 26 : a.CongTieuChuan)) * (a.TongSoGio / a.GioTieuChuan)));*/
                                    }
                                    //PhuCap = itemAdd.listGioTieuChuan.Sum(a => (((pc.SoTien) / (a.CongTieuChuan == 0 ? 26 : a.CongTieuChuan)) * (a.TongSoGio / a.GioTieuChuan)));
                                    else if (pc.LoaiPhuCap == 2)
                                    {
                                        PhuCap = itemAdd.listGioTieuChuan.Sum(a => (a.TongSoGio / a.GioTieuChuan)) / (itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                    }
                                    else if (pc.LoaiPhuCap == 3)
                                    {
                                        PhuCap = (TongSoCong * itemAdd.SoGioMotNgay) * pc.SoTien;
                                    }
                                    itemPC.SoTienPhuCap = PhuCap;
                                    itemPC.SoTienPhuCapHienThi = pc.SoTien;
                                    itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;

                                }
                            }
                            itemAdd.listPhuCap.Add(itemPC);
                        }
                        double Cong1Ngay = 0;
                        double Cong1NgayKDL = 0;
                        if (itemAdd.NgayCongTieuChuan > 0)
                        {
                            Cong1Ngay = (double)((itemAdd.LuongCoBan) / itemAdd.NgayCongTieuChuan);
                            Cong1NgayKDL = (double)((itemAdd.LuongCoBan + itemAdd.listPhuCap.Sum(a => a.SoTienPhuCapHienThi)) / itemAdd.NgayCongTieuChuan);
                        }
                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdChucVu != null && a.IdChucVu.Contains(item.IdChucVu.ToString()));
                        var NghiLeBP = listLichNghiBP.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                        if (NghiLe != null || NghiLeBP != null)
                        {

                            string id = NghiLe != null ? NghiLe.IdGuid : NghiLeBP.IdLichNghiLe;
                            var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == id &&
                            ((a.TuNgay.Month <= NgayBD.Month)
                            || a.DenNgay.Month >= NgayBD.Month
                            || a.TuNgay.Month <= NgayKT.Month
                            || a.DenNgay.Month >= NgayKT.Month)).ToList();
                            if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                            {
                                List<int> Ngays = new List<int>();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    time = ngay.TuNgay;
                                    while (time <= ngay.DenNgay)
                                    {
                                        if (time >= NgayBD && time <= NgayKT)
                                            Ngays.Add(time.Day);
                                        time = time.AddDays(1);
                                    }
                                }
                                // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng

                                var data = dataTGs.Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0) && Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    itemAdd.Le_Tet = 0;
                                    itemAdd.Le_TetKDL = 0;
                                    itemAdd.LuongLe = 0;
                                    itemAdd.LuongLeKDL = 0;
                                    Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                    foreach (var ngay in NghiLeChiTiets)
                                    {
                                        if (ngay.TyLeLuong > 1)
                                        {
                                            time = ngay.TuNgay;
                                            while (time <= ngay.DenNgay)
                                            {
                                                if (Ngays.Contains(time.Day))
                                                {
                                                    var ngayC = data.FirstOrDefault(a => a.NgayChamCong.Value.Day == time.Day);
                                                    double SoGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                                                    double TongCongN = SoGLV / (ngayC.SoGioTieuChuan ?? 8);
                                                    double TyLe = ngay.TyLeLuong / 100;

                                                    if (itemAdd.LoaiLuong == 3)
                                                    {
                                                        TyLe = 2;
                                                        itemAdd.Le_Tet += SoGLV;
                                                        itemAdd.LuongLe = itemAdd.LuongLe + SoGLV * itemAdd.LuongCoBan * TyLe;
                                                        itemAdd.Le_TetKDL += SoGLV;
                                                        itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + SoGLV * itemAdd.LuongCoBan;
                                                    }
                                                    else
                                                    {
                                                        itemAdd.Le_Tet += TongCongN;
                                                        itemAdd.LuongLe = itemAdd.LuongLe + Cong1Ngay * TongCongN * (TyLe);
                                                        itemAdd.Le_TetKDL += TongCongN;
                                                        itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + Cong1NgayKDL * TongCongN;
                                                    }
                                                    //itemAdd.NgayCong = itemAdd.NgayCong - (ngayC.SoGioLamViec ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                    //itemAdd.NgayCongCaGay = itemAdd.NgayCongCaGay - (ngayC.SoGioLamViecCaGay ?? 0) / (ngayC.SoGioTieuChuan ?? 8);

                                                    //itemAdd.NgayTangCa = itemAdd.NgayTangCa - (ngayC.SoGioLamViecTangCa ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                }
                                                time = time.AddDays(1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    int listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day)).Select(a => a.Ngay.Day).Distinct().Count();
                                    if (listLichLamViecFind > 0)
                                    {
                                        if (itemAdd.LoaiLuong == 3)
                                        {
                                            //itemAdd.Le_Tet += itemAdd.SoGioMotNgay;
                                            //itemAdd.LuongLe = itemAdd.LuongLe + itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;
                                        }
                                        else
                                        {
                                            itemAdd.Le_TetKDL += listLichLamViecFind;
                                            itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + Cong1NgayKDL * listLichLamViecFind;
                                        }


                                    }
                                }



                            }
                        }
                        if (itemAdd.Le_TetKDL < 0.1)
                        {
                            itemAdd.Le_TetKDL = 0;
                            itemAdd.LuongLeKDL = 0;
                        }
                        //if (itemAdd.NgayCongTieuChuan > 0)
                        //{
                        //    Cong1Ngay = (double)((itemAdd.LuongCoBan) / itemAdd.NgayCongTieuChuan);
                        //}
                        double Cong1NgayT7 = (double)((itemAdd.LuongCoBan + itemAdd.PhuCapCongChinh) / itemAdd.NgayCongTieuChuan);
                        //Giang sá»­a ngÃ y 27/06/2025, lÆ°Æ¡ng khÃ´ng tÃ­nh phá»¥ cÃ¢p
                        double Cong1NgayT7KhongPhuCap = (double)((itemAdd.LuongCoBan) / itemAdd.NgayCongTieuChuan);
                        itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                        if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                        {
                            // || a.Ma == DefineData.eKyHieuChamCong.N100.ToString()|| a.Ma == DefineData.eKyHieuChamCong.N50.ToString()
                            IdLoaiPhep = LoaiPheps.Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString()).Select(a => a.Id).Distinct().ToList();
                            itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && IdLoaiPhep.Contains(a.IdHinhThucNghi)).Sum(a => a.ThoiGianNghi);
                            itemAdd.Cong100 = 0;
                            itemAdd.Cong50 = 0;

                            IdLoaiPhep = LoaiPheps.Where(a => a.Ma == DefineData.eKyHieuChamCong.N100.ToString() || a.Ma == DefineData.eKyHieuChamCong.N50.ToString()).Select(a => a.Id).Distinct().ToList();
                            var nghiPheps = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();
                            if (nghiPheps != null && nghiPheps.Count > 0)
                                foreach (var np in nghiPheps)
                                {
                                    //Giang sá»­a ngÃ y 27/06/2025 láº¥y sai id hÃ¬nh thá»©c nghá»‰
                                    //if (np.IdLoaiNghiPhep == LoaiPheps.FirstOrDefault(a => a.Ma == DefineData.eKyHieuChamCong.N100.ToString()).Id)
                                    if (np.IdHinhThucNghi == LoaiPheps.FirstOrDefault(a => a.Ma == DefineData.eKyHieuChamCong.N100.ToString()).Id)
                                        itemAdd.Cong100 += np.ThoiGianNghi;
                                    else
                                        itemAdd.Cong50 += np.ThoiGianNghi * 0.5;
                                }

                            itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                            //Giang sá»­a ngÃ y 27/06/2025 ko tÃ­nh phá»¥ cáº¥p
                            //itemAdd.Luong50 = itemAdd.Cong50 * Cong1NgayT7;
                            //itemAdd.Luong100 = itemAdd.Cong100 * Cong1NgayT7;
                            itemAdd.Luong50 = itemAdd.Cong50 * Cong1NgayT7KhongPhuCap;
                            itemAdd.Luong100 = itemAdd.Cong100 * Cong1NgayT7KhongPhuCap;

                        }
                        itemAdd.NgayCong = datas.Sum(a => (a.TongSoCong ?? 0));
                        // tÄƒng ca
                        if (itemAdd.SoGioMotNgay > 0)
                        {
                            itemAdd.NgayTangCa = datas.Sum(a => (a.TongSoCong150 ?? 0) / itemAdd.SoGioMotNgay);
                            //itemAdd.Le_Tet = datas.Sum(a => (a.TongSoCong300 ?? 0)) / (itemAdd.SoGioMotNgay);
                            itemAdd.Ngay200 = datas.Sum(a => (a.TongSoCong200 ?? 0)) / (itemAdd.SoGioMotNgay);

                        }
                        if (item.NgayBatDauLamViec == null)
                            item.NgayBatDauLamViec = DateTime.Today;
                        int Nam = (((time.Year - item.NgayBatDauLamViec.Value.Year) * 12) + time.Month - item.NgayBatDauLamViec.Value.Month + 1) / 12;
                        itemAdd.LuongThamNien = (itemAdd.SoTienPCDiLai ?? 0) * Nam;
                        itemAdd.NgayCongDem = datas.Sum(a => (a.TongSoCongDem ?? 0));
                        itemAdd.LuongTheoCongDem = itemAdd.NgayCongDem * PCDem;
                        itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay * 1.5;
                        //itemAdd.LuongLe = itemAdd.Le_Tet + Cong1Ngay;
                        itemAdd.Luong200 = itemAdd.Ngay200 + Cong1Ngay * 3;

                        // báº£o hiá»ƒm xÃ£ há»™i
                        if (itemAdd.isDieuDong == false)
                        {
                            double TongCong = itemAdd.listGioTieuChuan.Sum(a => a.TongSoGio / a.GioTieuChuan);
                            if (SoGioLamViecKhongTrongKy != null)
                                TongCong += SoGioLamViecKhongTrongKy.Sum(a => a.TongSoGio / a.GioTieuChuan);

                            if (((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= NgayBD && item.ThangTruyThuBHXH <= NgayBD) || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= NgayBD && item.ThangTruyThuBHYT <= NgayBD)))
                            {
                                itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
                            }
                            if (TongCong < 14)
                            {
                                itemAdd.TruyThu = itemAdd.LuongDongBHXH * 0.32;
                            }
                            else
                            {
                                itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                                itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                                itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                                itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                                itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                                itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                                itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                            }
                            var TruyThu = listTruyThu.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                            if (TruyThu != null)
                                itemAdd.TruyThu += TruyThu.SoTien;
                        }
                        itemAdd.TongLuong = itemAdd.listGioTieuChuan.Sum(a => a.Luong) + itemAdd.listTangCa.Sum(a => a.Luong) + itemAdd.LuongPhep + itemAdd.LuongLe + itemAdd.LuongLeKDL + (itemAdd.Luong50 ?? 0) + (itemAdd.Luong100 ?? 0) + (itemAdd.listPhuCap.Where(a => a.Ma != "LAMDEM").Sum(a => a.SoTienPhuCap)) + (itemAdd.LuongTheoCongDem) /*+ (itemAdd.LuongThamNien)*/;

                        // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                        var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGiamTruBanThan = 11000000;
                        itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SpNguoiDangKyNPT * 4400000;
                        itemAdd.ThuNhapMienThue = itemAdd.PhuCapCongChinh + itemAdd.TongNguoiNLD;
                        if (itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue - itemAdd.SoGiamTruBanThan > 0)
                            itemAdd.ThuNhapTinhThueTNCN = itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue - itemAdd.SoGiamTruBanThan;
                        else
                            itemAdd.ThuNhapTinhThueTNCN = 0;
                        itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                        itemAdd.KhauTruTaiNguon = 0;
                        itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;

                        itemAdd.TienThucNhan = itemAdd.TongLuong + (itemAdd.ThuongLuong ?? 0) - (itemAdd.TienCongDoan ?? 0) /*- itemAdd.ThueTNCNTheoLuyTien*/ - (itemAdd.Phat ?? 0) - itemAdd.TongNguoiNLD - itemAdd.TruyThu;
                        //if (itemAdd.listBaoHiemNgoai != null && itemAdd.listBaoHiemNgoai.Count > 0)
                        //    itemAdd.TienThucNhan -= itemAdd.listBaoHiemNgoai.Sum(a => a.SoTien);
                        if (itemAdd.TienThucNhan < 0)
                            itemAdd.TienThucNhan = 0;
                        //itemAdd.CacKhoanCongTyThanhToan = itemAdd.listGioTieuChuan.Sum(a => a.Luong) + itemAdd.listPhuCap.Sum(a => a.SoTienPhuCap) + (itemAdd.LuongDanhGia ?? 0) + (itemAdd.Thuong ?? 0) + (itemAdd.LuongPhep) + (itemAdd.Luong100 ?? 0) + (itemAdd.Luong50 ?? 0) + (itemAdd.TA_ThanhTien ?? 0) + (itemAdd.HoTroCapPhatBHLD ?? 0);
                        itemAdd.CacKhoanCongTyThanhToan = itemAdd.listGioTieuChuan.Sum(a => a.Luong) + itemAdd.listPhuCap.Sum(a => a.SoTienPhuCap) + (itemAdd.LuongDanhGia ?? 0) + (itemAdd.LuongLe) + (itemAdd.LuongLeKDL) + (itemAdd.LuongTheoCongDem) + (itemAdd.Thuong ?? 0) + (itemAdd.LuongNghiKeHoach ?? 0) + (itemAdd.LuongPhep) + (itemAdd.Luong50 ?? 0) + (itemAdd.Luong100 ?? 0) + (itemAdd.TA_ThanhTien ?? 0) + (itemAdd.HoTroCapPhatBHLD ?? 0) + (itemAdd.HoTroBHYT ?? 0) + (itemAdd.HoTroNhatCuaRoi ?? 0) - (itemAdd.GiamTruDoViPham ?? 0);

                        listResult.Add(itemAdd);
                    }
                }

                QuyTrinh = GetListHeaderNew(QuyTrinh, listResult.FirstOrDefault().listTangCa, listResult.FirstOrDefault().listGioTieuChuan, IdCongty, db, IdBoPhan);
                if (LoaiLuong == 0)
                    QuyTrinh.listGiaTriNhanVien = GetListGiatri(QuyTrinh.listHeader_Data, listResult.OrderBy(a => a.NgayVaoLam).ToList(), listNhanVien, listHopDong, db);
                else
                    QuyTrinh.listGiaTriNhanVien = GetListGiatri(QuyTrinh.listHeader_Data, listResult.Where(a => a.LoaiLuong == LoaiLuong).OrderBy(a => a.NgayVaoLam).ToList(), listNhanVien, listHopDong, db);
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                Add.listBaoHiemNgoai = new List<HRM_QuyTrinhTinhLuong_BaoHiemNgoai>();
                Add.listGioTieuChuan = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                Add.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                Add.listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();

                listResult.Add(Add);
            }
            QuyTrinh.listNhanVien = listResult;


            return QuyTrinh;
        }
        public HRM_QuyTrinhTinhLuong GetTinhLuongNhanVienTheoKyChamCongPHTOLD(long IdCongty, List<long> IdBoPhan, List<string> IdKyChamCong, int LoaiLuong, ApplicationDbContext db)
        {

            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            var BangChamCong = db.HRM_BangChamCong.Where(a => IdKyChamCong.Contains(a.IdGuid)).ToList();

            DateTime NgayBD = BangChamCong.OrderBy(a => a.TuNgay).FirstOrDefault().TuNgay.Value;
            NgayBD = new DateTime(NgayBD.Year, NgayBD.Month, NgayBD.Day, 0, 0, 0);
            DateTime NgayKT = BangChamCong.OrderByDescending(a => a.DenNgay).FirstOrDefault().DenNgay.Value;
            NgayKT = new DateTime(NgayKT.Year, NgayKT.Month, NgayKT.Day, 0, 0, 0);
            var listData = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
            if (IdBoPhan != null && IdBoPhan.Count > 0)
                listData = listData.Where(a => IdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            if (listData != null && listData.Count > 0)
            {
                IdKyChamCong = listData.Select(a => a.IdBangChamCong).Distinct().ToList();
                var tangCas = db.HRM_BangChamCongNhanVien_TangCa.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                var SoGioLamViecs = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                List<string> listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                if (IdBoPhan.Count > 0)
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid) && IdBoPhan.Contains(x.IdBoPhanEOS)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                List<long> listIdChucVu = listNhanVien.Select(a => a.IdChucVu).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                var parttime = LoaiHopDongs.FirstOrDefault(a => a.Ma == "HÄVV");
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                var listId = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
                listLuong.AddRange((from a in db.HRM_Luong_NhanVien
                                    where listIdNhanVien.Contains(a.IdNhanVien) && !listId.Contains(a.IdNhanVien) && a.NgayHetHieuLuc != null && a.NgayHetHieuLuc.Value.Year > 2000 && a.NgayHetHieuLuc <= NgayBD
                                    group a by a.IdNhanVien into g
                                    select g.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault()).ToList());
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var LoaiPheps = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).ToList();
                List<string> IdLoaiPhep = LoaiPheps.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.IdCongTy == IdCongty && listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT /*&& a.IdCongTy == IdCongty*/ && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();

                //var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongty && (a.Nam == NgayBD.Year || a.Nam == NgayKT.Year)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe) && a.TuNgay <= NgayKT && a.DenNgay >= NgayBD).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongty
                                                                   && x.Ngay >= NgayBD && x.Ngay <= NgayKT).ToList();
                var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();

                Parallel.ForEach(listData, item => item.isNgayLe = false);
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "033307005154")
                        item.MaNhanVien = "033307005154";
                    var datas = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayChamCong).ToList();
                    if (datas != null && datas.Count > 0)
                    {
                        HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                        itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        itemAdd.PhuCapCongChinh = 0;
                        itemAdd.PhuCapKhongTheoCongChinh = 0;
                        var listLuongNV = listLuong.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                        if (listLuongNV != null && listLuongNV.Count > 0)
                        {
                            var Luong = listLuongNV.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                            itemAdd.LoaiLuong = Luong.LoaiLuong;

                            itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                            itemAdd.LuongCoBan = Luong.Luong;
                            itemAdd.NgayApDung = Luong.NgayBatDauHieuLuc;
                            if (listLuongNV.Count > 1)
                                itemAdd.LuongCoBan = listLuongNV.Sum(a => a.Luong) / listLuongNV.Count;

                        }
                        else continue;
                        if (HopDong != null)
                        {
                            var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                            if (loaiHopDong != null)
                            {
                                itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                            }
                            var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                            if (CaLamViec == null)
                            {
                                if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                    CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                if (CaLamViec == null)
                                {
                                    string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                    CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                }
                            }
                            if (CaLamViec == null)
                                CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                            {
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                            }

                            if (itemAdd.LoaiLuong == 3)
                            {
                                itemAdd.SoGioMotNgay = 1;
                                itemAdd.NgayCongTieuChuan = 1;
                            }



                        }
                        // tÃ­nh Cong
                        if (itemAdd.SoGioMotNgay > 0)
                        {
                            double NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                            double NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                            itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                            itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                            itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));
                            if (itemAdd.LoaiLuong == 3)
                            {
                                itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                            }
                        }
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                        var CoSo = listDuAn.Where(a => listIdCoSo.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                        if (CoSo != null)
                            itemAdd.MaCoSo = string.Join(",", CoSo);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                        {
                            itemAdd.TenChucDanh = ChucDanh.Ten;
                        }



                        // phá»¥ cáº¥p
                        var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        //double TongSoCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay;
                        foreach (var phucap in listPhuCapdb)
                        {
                            HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                            itemPC.IdNhanVien = item.IdGuid;
                            if (PhuCaps != null && PhuCaps.Count > 0)
                            {
                                var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                if (pc != null)
                                {
                                    double NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    double NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));

                                    if (itemAdd.LoaiLuong == 3)
                                    {
                                        NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0));
                                        NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0));

                                    }
                                    double TongSoCong = NgayCong + NgayCongCaGay;
                                    itemPC.Ma = phucap.Ma;
                                    itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                    itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                    double PhuCap = 0;
                                    itemPC.SoTienPhuCap = pc.SoTien;
                                    if (itemAdd.NgayCongTieuChuan > 0)
                                        PhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                    if (phucap.Ma == "PCK-KCT") /// Ä‘á»‘i vá»›i phá»¥ cáº¥p khÃ´ng theo ca chÃ­nh
                                        itemAdd.PhuCapKhongTheoCongChinh = itemAdd.PhuCapKhongTheoCongChinh + pc.SoTien;
                                    else
                                    if (phucap.Ma == "PC.CG" && pc.LoaiPhuCap != 5) /// Ä‘á»‘i vá»›i ca gÃ£y  nhÃ¢n viÃªn sale
                                    {

                                        PhuCap = ((NgayCongCaGay) / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;

                                    }
                                    else
                                    if (pc.LoaiPhuCap == 5)
                                    {
                                        if (phucap.Ma == "PC.NN")  // phá»¥ cáº¥p ngoáº¡i ngá»¯
                                        {
                                            if (TongSoCong >= 15)
                                            {
                                                PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                            }
                                            else
                                            {
                                                //PhuCap = (itemAdd.NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                        }
                                        else if (phucap.Ma == "PC.CG") // phá»¥ cáº¥p ca gÃ£y
                                        {
                                            if (itemAdd.NgayCongCaGay >= 15)
                                            {
                                                PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                            else
                                            {
                                                PhuCap = (NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (TongSoCong >= itemAdd.NgayCongTieuChuan || pc.LoaiPhuCap == 1)
                                            PhuCap = pc.SoTien;
                                        itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                    }

                                }
                            }
                            itemAdd.listPhuCap.Add(itemPC);
                        }



                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);

                        if (NghiLe != null)
                        {
                            var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe).Distinct().ToList();
                            if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                            {
                                List<int> Ngays = new List<int>();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    DateTime time = ngay.TuNgay;
                                    while (time <= ngay.DenNgay)
                                    {
                                        if (time >= NgayBD && time <= NgayKT)
                                            Ngays.Add(time.Day);
                                        time = time.AddDays(1);
                                    }
                                }

                                var data = datas.Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0) && Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    itemAdd.Le_Tet = 0;
                                    itemAdd.Le_TetKDL = 0;
                                    itemAdd.LuongLe = 0;
                                    itemAdd.LuongLeKDL = 0;
                                    Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                    foreach (var ngay in NghiLeChiTiets)
                                    {
                                        if (ngay.TyLeLuong > 1)
                                        {
                                            DateTime time = ngay.TuNgay;
                                            while (time <= ngay.DenNgay)
                                            {
                                                var Luong = listLuongNV.FirstOrDefault(a => a.NgayBatDauHieuLuc <= time && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc.Value.Year > 2000 || time <= a.NgayHetHieuLuc));
                                                if (Luong != null)
                                                {
                                                    double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                    if (Luong.LoaiLuong == 3)
                                                        NgayCong = Luong.Luong;
                                                    if (Ngays.Contains(time.Day))
                                                    {


                                                        var ngayC = data.FirstOrDefault(a => a.NgayChamCong.Value.Day == time.Day);
                                                        double SoGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                                                        double TongCongN = SoGLV / (ngayC.SoGioTieuChuan ?? 8);
                                                        double TyLe = ngay.TyLeLuong / 100;

                                                        if (itemAdd.LoaiLuong == 3)
                                                        {
                                                            TyLe = 2;
                                                            itemAdd.Le_Tet += SoGLV;
                                                            itemAdd.LuongLe = itemAdd.LuongLe + SoGLV * itemAdd.LuongCoBan * TyLe;


                                                        }
                                                        else
                                                        {

                                                            itemAdd.Le_Tet += TongCongN;
                                                            itemAdd.LuongLe = itemAdd.LuongLe + NgayCong * TongCongN * (TyLe);
                                                            if (TongCongN < 1)
                                                            {

                                                                itemAdd.Le_TetKDL += 1 - TongCongN;
                                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong * (1 - TongCongN);

                                                            }
                                                        }
                                                        itemAdd.NgayCong = itemAdd.NgayCong - (ngayC.SoGioLamViec ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                        itemAdd.NgayCongCaGay = itemAdd.NgayCongCaGay - (ngayC.SoGioLamViecCaGay ?? 0) / (ngayC.SoGioTieuChuan ?? 8);

                                                        itemAdd.NgayTangCa = itemAdd.NgayTangCa - (ngayC.SoGioLamViecTangCa ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                        ngayC.isNgayLe = true;
                                                    }
                                                    else
                                                    {
                                                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                                        if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                                        {
                                                            if (itemAdd.LoaiLuong != 3)
                                                            {

                                                                itemAdd.Le_TetKDL += 1;
                                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                            }

                                                        }
                                                    }
                                                }
                                                time = time.AddDays(1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                    if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                    {
                                        var lNgays = listLichLamViecFind.Select(a => a.Ngay).Distinct().ToList();
                                        foreach (var itemLV in lNgays)
                                        {
                                            if (itemAdd.LoaiLuong != 3)
                                            {
                                                var Luong = listLuongNV.FirstOrDefault(a => a.NgayBatDauHieuLuc <= itemLV && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc.Value.Year > 2000 || itemLV <= a.NgayHetHieuLuc));
                                                double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                itemAdd.Le_TetKDL += 1;
                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                            }

                                        }
                                    }
                                }



                            }
                        }

                        // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                        double Cong1Ngay = 0;
                        itemAdd.LuongTheoCong = 0;
                        if (listLuongNV.Count > 1)
                        {
                            itemAdd.NgayTangCa = 0;
                            itemAdd.LuongTangCa = 0;
                            itemAdd.NghiPhep = 0;
                            itemAdd.LuongPhep = 0;
                            itemAdd.LuongTheoCong = 0;
                            foreach (var Luong in listLuongNV)
                            {
                                double NgayTangCa = 0;
                                double LuongTangCa = 0;
                                double NghiPhep = 0;
                                double LuongPhep = 0;
                                double LuongTheoCong = 0;

                                if (itemAdd.NgayCongTieuChuan > 0)
                                    Cong1Ngay = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                if (Luong.LoaiLuong == 3)
                                    Cong1Ngay = Luong.Luong;
                                var data = datas.Where(a => a.isNgayLe == false && Luong.NgayBatDauHieuLuc <= a.NgayChamCong && (Luong.NgayHetHieuLuc == null || a.NgayChamCong <= Luong.NgayHetHieuLuc)).ToList();
                                // tÄƒng ca
                                {
                                    NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0) / 8);
                                    if (Luong.LoaiLuong == 3)
                                        NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                    LuongTangCa = NgayTangCa * Cong1Ngay;
                                    if (Luong.LoaiLuong == 3)
                                        LuongTangCa = NgayTangCa * Luong.Luong;
                                }

                                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                                {
                                    NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && Luong.NgayBatDauHieuLuc <= a.NgayNghi && (Luong.NgayHetHieuLuc == null || a.NgayNghi <= Luong.NgayHetHieuLuc)).Sum(a => a.ThoiGianNghi);
                                    LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                    if (Luong.LoaiLuong == 3)
                                        LuongPhep = NghiPhep * itemAdd.SoGioMotNgay * Luong.Luong;

                                }
                                double NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                double NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                                if (Luong.LoaiLuong == 3)
                                {
                                    NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0));
                                    NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                }
                                LuongTheoCong = (NgayCong + NgayCongCaGay) * Cong1Ngay;
                                if (Luong.LoaiLuong == 3)
                                    LuongTheoCong = (NgayCong + NgayCongCaGay) * Luong.Luong;


                                itemAdd.NgayTangCa += NgayTangCa;
                                itemAdd.LuongTangCa += LuongTangCa;
                                itemAdd.NghiPhep += NghiPhep;
                                itemAdd.LuongPhep += LuongPhep;
                                itemAdd.LuongTheoCong += LuongTheoCong;

                            }
                        }
                        else
                        {

                            if (itemAdd.NgayCongTieuChuan > 0)
                            {
                                Cong1Ngay = (double)((itemAdd.LuongCoBan) / itemAdd.NgayCongTieuChuan);
                            }

                            double NgayTangCa = 0;
                            double LuongTangCa = 0;
                            double NghiPhep = 0;
                            double LuongPhep = 0;
                            double LuongTheoCong = 0;


                            var data = datas.Where(a => a.isNgayLe == false && itemAdd.NgayApDung <= a.NgayChamCong).ToList();
                            // tÄƒng ca
                            {
                                NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0) / 8);
                                if (itemAdd.LoaiLuong == 3)
                                    NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                LuongTangCa = NgayTangCa * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    LuongTangCa = NgayTangCa * itemAdd.LuongCoBan;
                            }

                            if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                            {
                                NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && itemAdd.NgayApDung <= a.NgayNghi).Sum(a => a.ThoiGianNghi);
                                LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    LuongPhep = NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                            }
                            double NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                            double NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                            if (itemAdd.LoaiLuong == 3)
                            {
                                NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0));
                                NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                            }
                            LuongTheoCong = (NgayCong + NgayCongCaGay) * Cong1Ngay;
                            if (itemAdd.LoaiLuong == 3)
                                LuongTheoCong = (NgayCong + NgayCongCaGay) * itemAdd.LuongCoBan;


                            //itemAdd.NgayTangCa += NgayTangCa;
                            itemAdd.LuongTangCa += LuongTangCa;
                            //itemAdd.NghiPhep += NghiPhep;
                            itemAdd.LuongPhep += LuongPhep;
                            itemAdd.LuongTheoCong += LuongTheoCong;

                            #region Old
                            //if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                            //{
                            //    itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                            //    itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                            //    if (itemAdd.LoaiLuong == 3)
                            //        itemAdd.LuongPhep = itemAdd.NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                            //}

                            //itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                            //if (itemAdd.LoaiLuong == 3)
                            //    itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * itemAdd.LuongCoBan;

                            //itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;
                            //if (itemAdd.LoaiLuong == 3)
                            //    itemAdd.LuongTangCa = itemAdd.NgayTangCa * itemAdd.LuongCoBan;
                            #endregion
                        }

                        if (itemAdd.NgayTangCa < 0)
                            itemAdd.NgayTangCa = 0;
                        if (itemAdd.LuongTangCa < 0)
                            itemAdd.LuongTangCa = 0;

                        if (itemAdd.NgayCong < 0)
                            itemAdd.NgayCong = 0;
                        if (itemAdd.NgayCongCaGay < 0)
                            itemAdd.NgayCongCaGay = 0;
                        if (itemAdd.Le_TetKDL < 0.1)
                        {
                            itemAdd.Le_TetKDL = 0;
                            itemAdd.LuongLeKDL = 0;
                        }

                        itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.LuongLeKDL + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;
                        double TongCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay + itemAdd.NghiPhep + itemAdd.Le_Tet + itemAdd.NghiBu + itemAdd.NgayTangCa;
                        if (((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= NgayBD && item.ThangTruyThuBHXH <= NgayBD) || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= NgayBD && item.ThangTruyThuBHYT <= NgayBD)))
                        {
                            itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
                        }
                        if (TongCong > 14)
                        //{
                        //    itemAdd.TruyThu = itemAdd.LuongDongBHXH * 0.32;
                        //}
                        //else
                        {
                            itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                            itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                            itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                            itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                            itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                            itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                            itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                        }

                        // báº£o hiá»ƒm xÃ£ há»™i
                        //itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                        //itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                        //itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                        //itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        //itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                        //itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                        //itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                        //itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        //itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;


                        // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                        var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGiamTruBanThan = 11000000;
                        itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
                        itemAdd.ThuNhapMienThue = itemAdd.LuongLe + itemAdd.LuongLeKDL + +itemAdd.TongNguoiNLD;
                        if (itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                            itemAdd.ThuNhapTinhThueTNCN = itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                        else
                            itemAdd.ThuNhapTinhThueTNCN = 0;
                        itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                        // cáº¥p phÃ¡t tÃ i sáº£n
                        if (listCapPhatTaiSan != null)
                        {
                            var listTaiSan = listCapPhatTaiSan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                            if (listTaiSan != null && listTaiSan.Count > 0)
                            {
                                var capPhat = listTaiSan.Where(a => a.Loai == 1).ToList();
                                if (capPhat != null && capPhat.Count > 0)
                                    itemAdd.DongPhuc = capPhat.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
                                var ThuHoi = listTaiSan.Where(a => a.Loai == 1).ToList();
                                if (ThuHoi != null && ThuHoi.Count > 0)
                                    itemAdd.TraTienDongPhuc = ThuHoi.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));

                            }
                        }
                        itemAdd.KhauTruTaiNguon = 0;
                        itemAdd.isCamKet = item.isCamKet;
                        if (itemAdd.MucDongBaoHiemXaHoi == 0 && item.isCamKet == false)
                            itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;
                        itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
                        if (itemAdd.TongGiamThueTNCN < 50000)
                            itemAdd.TongGiamThueTNCN = 0;
                        itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD - (itemAdd.TongGiamThueTNCN ?? 0) - (itemAdd.TongGiamThueTNCN ?? 0);
                        listResult.Add(itemAdd);
                    }
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listResult.FirstOrDefault().listTangCa, listResult.FirstOrDefault().listGioTieuChuan, IdCongty, db, IdBoPhan);
                QuyTrinh.listNhanVien = listResult.OrderBy(a => a.NgayVaoLam).ToList();
                if (LoaiLuong == 0)
                    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult, listNhanVien, listHopDong, db);
                else
                    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult.Where(a => a.LoaiLuong == LoaiLuong).ToList(), listNhanVien, listHopDong, db);
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                listResult.Add(Add);
            }
            return QuyTrinh;
        }
        public HRM_QuyTrinhTinhLuong GetTinhLuongNhanVienTheoKyChamCongPHT(long IdCongty, List<long> IdBoPhan, List<string> IdKyChamCong, int LoaiLuong, ApplicationDbContext db)
        {

            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            var BangChamCong = db.HRM_BangChamCong.Where(a => IdKyChamCong.Contains(a.IdGuid)).ToList();

            DateTime NgayBD = BangChamCong.OrderBy(a => a.TuNgay).FirstOrDefault().TuNgay.Value;
            NgayBD = new DateTime(NgayBD.Year, NgayBD.Month, NgayBD.Day, 0, 0, 0);
            DateTime NgayKT = BangChamCong.OrderByDescending(a => a.DenNgay).FirstOrDefault().DenNgay.Value;
            NgayKT = new DateTime(NgayKT.Year, NgayKT.Month, NgayKT.Day, 0, 0, 0);
            var listData = db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
            if (IdBoPhan != null && IdBoPhan.Count > 0)
                listData = listData.Where(a => IdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            if (listData != null && listData.Count > 0)
            {
                IdKyChamCong = listData.Select(a => a.IdBangChamCong).Distinct().ToList();
                var tangCas = db.HRM_BangChamCongNhanVien_TangCa.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                var SoGioLamViecs = db.HRM_BangChamCongNhanVien_SoGioLamViec.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
                List<string> listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();
                if (IdBoPhan.Count > 0)
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid) && IdBoPhan.Contains(x.IdBoPhanEOS)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                List<long> listIdChucVu = listNhanVien.Select(a => a.IdChucVu).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                var parttime = LoaiHopDongs.FirstOrDefault(a => a.Ma == "HÄVV");
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                var listId = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
                listLuong.AddRange((from a in db.HRM_Luong_NhanVien
                                    where listIdNhanVien.Contains(a.IdNhanVien) && !listId.Contains(a.IdNhanVien) && a.NgayHetHieuLuc != null && a.NgayHetHieuLuc.Value.Year > 2000 && a.NgayHetHieuLuc <= NgayBD
                                    group a by a.IdNhanVien into g
                                    select g.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault()).ToList());
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var LoaiPheps = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).ToList();
                List<string> IdLoaiPhep = LoaiPheps.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.IdCongTy == IdCongty && listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT /*&& a.IdCongTy == IdCongty*/ && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();

                //var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongty && (a.Nam == NgayBD.Year || a.Nam == NgayKT.Year)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe) && a.TuNgay <= NgayKT && a.DenNgay >= NgayBD).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongty
                                                                   && x.Ngay >= NgayBD && x.Ngay <= NgayKT).ToList();
                var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();

                Parallel.ForEach(listData, item => item.isNgayLe = false);
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "056304012695")
                        item.MaNhanVien = "056304012695";
                    List<HRM_QuyTrinhTinhLuong_NhanVien> listLuongCheck = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
                    var listdatas = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();

                    var listLuongNV = listLuong.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayBatDauHieuLuc).ToList();
                    if (listLuongNV != null && listLuongNV.Count > 0)
                    {
                        int STT = 1;
                        bool isDaTinhBHXH = false;
                        foreach (var Luong in listLuongNV)
                        {
                            HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                            //var Luong = listLuongNV.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                            itemAdd.LoaiLuong = Luong.LoaiLuong;

                            itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                            itemAdd.LuongCoBan = Luong.Luong;
                            itemAdd.NgayApDung = Luong.NgayBatDauHieuLuc;

                            var datas = listdatas.Where(a => a.NgayChamCong >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayChamCong <= Luong.NgayHetHieuLuc))).OrderBy(a => a.NgayChamCong).ToList();
                            if (datas != null && datas.Count > 0)
                            {

                                itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                                var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IdGuid == Luong.IdNguonNhapLuong);
                                itemAdd.PhuCapCongChinh = 0;
                                itemAdd.PhuCapKhongTheoCongChinh = 0;
                                if (HopDong != null)
                                {
                                    var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                                    if (loaiHopDong != null)
                                    {
                                        itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                        itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                                    }
                                    var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                                    if (CaLamViec == null)
                                    {
                                        if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                            CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                        if (CaLamViec == null)
                                        {
                                            string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec) && a.Ngay >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.Ngay <= Luong.NgayHetHieuLuc))).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                            CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                        }
                                    }
                                    if (CaLamViec == null)
                                        CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                                    {
                                        itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                        itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                        itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                                    }
                                }
                                else
                                {
                                    var CaLamViec = new HRM_dmLichLamViec();

                                    if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                        CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                    else
                                    {
                                        string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                        CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                    }

                                    if (CaLamViec == null)
                                        CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                                    {
                                        itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                        itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                        itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                                    }
                                }
                                if (itemAdd.LoaiLuong == 3)
                                {
                                    itemAdd.SoGioMotNgay = 1;
                                    itemAdd.NgayCongTieuChuan = 1;
                                }

                                // tÃ­nh Cong
                                if (itemAdd.SoGioMotNgay > 0)
                                {
                                    double NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                    double NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                    itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    if (itemAdd.LoaiLuong == 3)
                                    {
                                        itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                        itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                        itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                    }
                                }
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.TenNhanVien = item.TenDayDu;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                    itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                                if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                    itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                                var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                                var CoSo = listDuAn.Where(a => listIdCoSo.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                                if (CoSo != null)
                                    itemAdd.MaCoSo = string.Join(",", CoSo);
                                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                                if (BoPhan != null)
                                {
                                    itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                                }
                                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                                if (ChucDanh != null)
                                {
                                    itemAdd.TenChucDanh = ChucDanh.Ten;
                                }
                                var ChucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                                if (ChucVu != null)
                                {
                                    itemAdd.TenChucVu = ChucVu.TenChucVu;
                                }

                                // phá»¥ cáº¥p
                                var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid && a.IdLuong_NhanVien == Luong.IdGuid).ToList();
                                foreach (var phucap in listPhuCapdb)
                                {
                                    HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                                    itemPC.IdNhanVien = item.IdGuid;
                                    if (PhuCaps != null && PhuCaps.Count > 0)
                                    {
                                        var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                        if (pc != null)
                                        {
                                            double NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                            double NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));

                                            if (itemAdd.LoaiLuong == 3)
                                            {
                                                NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0));
                                                NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0));

                                            }
                                            double TongSoCong = NgayCong + NgayCongCaGay;
                                            itemPC.Ma = phucap.Ma;
                                            itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                            itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                            double PhuCap = 0;
                                            itemPC.SoTienPhuCap = pc.SoTien;
                                            if (itemAdd.NgayCongTieuChuan > 0)
                                                PhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                            if (phucap.Ma == "PCK-KCT") /// Ä‘á»‘i vá»›i phá»¥ cáº¥p khÃ´ng theo ca chÃ­nh
                                                itemAdd.PhuCapKhongTheoCongChinh = itemAdd.PhuCapKhongTheoCongChinh + pc.SoTien;
                                            else
                                            if (phucap.Ma == "PC.CG" && pc.LoaiPhuCap != 5) /// Ä‘á»‘i vá»›i ca gÃ£y  nhÃ¢n viÃªn sale
                                            {

                                                PhuCap = ((NgayCongCaGay) / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;

                                            }
                                            else
                                            if (pc.LoaiPhuCap == 5)
                                            {
                                                if (phucap.Ma == "PC.NN")  // phá»¥ cáº¥p ngoáº¡i ngá»¯
                                                {
                                                    if (TongSoCong >= 15)
                                                    {
                                                        PhuCap = pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                                    }
                                                    else
                                                    {
                                                        //PhuCap = (itemAdd.NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                }
                                                else if (phucap.Ma == "PC.CG") // phá»¥ cáº¥p ca gÃ£y
                                                {
                                                    if (itemAdd.NgayCongCaGay >= 15)
                                                    {
                                                        PhuCap = pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                    else
                                                    {
                                                        PhuCap = (NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                if (TongSoCong >= itemAdd.NgayCongTieuChuan || pc.LoaiPhuCap == 1)
                                                    PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                            }

                                        }
                                    }
                                    itemAdd.listPhuCap.Add(itemPC);
                                }
                                var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                                if (NghiLe != null)
                                {
                                    var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe).Distinct().ToList();
                                    if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                                    {
                                        List<int> Ngays = new List<int>();
                                        foreach (var ngay in NghiLeChiTiets)
                                        {
                                            DateTime time = ngay.TuNgay;
                                            while (time <= ngay.DenNgay)
                                            {
                                                if (time >= NgayBD && time <= NgayKT)
                                                    Ngays.Add(time.Day);
                                                time = time.AddDays(1);
                                            }
                                        }

                                        var data = datas.Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0) && Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                                        if (data != null && data.Count > 0)
                                        {
                                            itemAdd.Le_Tet = 0;
                                            itemAdd.Le_TetKDL = 0;
                                            itemAdd.LuongLe = 0;
                                            itemAdd.LuongLeKDL = 0;
                                            Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                            foreach (var ngay in NghiLeChiTiets)
                                            {
                                                if (ngay.TyLeLuong > 1)
                                                {
                                                    DateTime time = ngay.TuNgay;
                                                    while (time <= ngay.DenNgay)
                                                    {
                                                        double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                        if (Luong.LoaiLuong == 3)
                                                            NgayCong = Luong.Luong;
                                                        if (Ngays.Contains(time.Day))
                                                        {


                                                            var ngayC = data.FirstOrDefault(a => a.NgayChamCong.Value.Day == time.Day);
                                                            double SoGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                                                            double TongCongN = SoGLV / (ngayC.SoGioTieuChuan ?? 8);
                                                            double TyLe = ngay.TyLeLuong / 100;

                                                            if (itemAdd.LoaiLuong == 3)
                                                            {
                                                                TyLe = 2;
                                                                itemAdd.Le_Tet += SoGLV;
                                                                itemAdd.LuongLe = itemAdd.LuongLe + SoGLV * itemAdd.LuongCoBan * TyLe;


                                                            }
                                                            else
                                                            {

                                                                itemAdd.Le_Tet += TongCongN;
                                                                itemAdd.LuongLe = itemAdd.LuongLe + NgayCong * TongCongN * (TyLe);
                                                                if (TongCongN < 1)
                                                                {

                                                                    itemAdd.Le_TetKDL += 1 - TongCongN;
                                                                    itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong * (1 - TongCongN);

                                                                }
                                                            }
                                                            itemAdd.NgayCong = itemAdd.NgayCong - (ngayC.SoGioLamViec ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                            itemAdd.NgayCongCaGay = itemAdd.NgayCongCaGay - (ngayC.SoGioLamViecCaGay ?? 0) / (ngayC.SoGioTieuChuan ?? 8);

                                                            itemAdd.NgayTangCa = itemAdd.NgayTangCa - (ngayC.SoGioLamViecTangCa ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                            ngayC.isNgayLe = true;
                                                        }
                                                        else
                                                        {
                                                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                                            if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                                            {
                                                                if (itemAdd.LoaiLuong != 3)
                                                                {

                                                                    itemAdd.Le_TetKDL += 1;
                                                                    itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                                }

                                                            }
                                                        }

                                                        time = time.AddDays(1);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                            if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                            {
                                                var lNgays = listLichLamViecFind.Select(a => a.Ngay).Distinct().ToList();
                                                foreach (var itemLV in lNgays)
                                                {
                                                    if (itemAdd.LoaiLuong != 3)
                                                    {
                                                        double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                        itemAdd.Le_TetKDL += 1;
                                                        itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                    }

                                                }
                                            }
                                        }



                                    }
                                }
                                // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                                double Cong1Ngay = itemAdd.LuongCoBan / itemAdd.NgayCongTieuChuan;
                                itemAdd.LuongTheoCong = 0;
                                #region Old
                                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                                {
                                    itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && a.NgayNghi >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayNghi <= Luong.NgayHetHieuLuc))).Sum(a => a.ThoiGianNghi);
                                    itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                    if (itemAdd.LoaiLuong == 3)
                                        itemAdd.LuongPhep = itemAdd.NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                                }
                                itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * itemAdd.LuongCoBan;

                                itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    itemAdd.LuongTangCa = itemAdd.NgayTangCa * itemAdd.LuongCoBan;
                                #endregion
                                if (itemAdd.NgayTangCa < 0)
                                    itemAdd.NgayTangCa = 0;
                                if (itemAdd.LuongTangCa < 0)
                                    itemAdd.LuongTangCa = 0;

                                if (itemAdd.NgayCong < 0)
                                    itemAdd.NgayCong = 0;
                                if (itemAdd.NgayCongCaGay < 0)
                                    itemAdd.NgayCongCaGay = 0;
                                if (itemAdd.Le_TetKDL < 0.1)
                                {
                                    itemAdd.Le_TetKDL = 0;
                                    itemAdd.LuongLeKDL = 0;
                                }
                                itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.LuongLeKDL + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;
                                double TongCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay + itemAdd.NghiPhep + itemAdd.Le_Tet + itemAdd.NghiBu + itemAdd.NgayTangCa;
                                if (((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= NgayBD && item.ThangTruyThuBHXH <= NgayBD) || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= NgayBD && item.ThangTruyThuBHYT <= NgayBD)))
                                {
                                    itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
                                }
                                if (Luong.LuongHachToan > 0 && Luong.LoaiLuong == 1)
                                {
                                    if (isDaTinhBHXH == false)
                                    {
                                        DateTime TuNgay = listLuongNV.Where(a => a.LoaiLuong == 1 && a.LuongHachToan > 0).OrderBy(a => a.NgayBatDauHieuLuc).Select(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                                        DateTime? DenNgay = listLuongNV.Where(a => a.LoaiLuong == 1 && a.LuongHachToan > 0).OrderByDescending(a => a.NgayBatDauHieuLuc).Select(a => a.NgayHetHieuLuc).FirstOrDefault();

                                        if (listdatas.Where(a => a.NgayChamCong >= TuNgay && (DenNgay == null || (DenNgay.Value.Year > 2000 && DenNgay >= a.NgayChamCong))).Sum(a => (a.SoGioLamViec ?? 0) + (a.SoGioLamViecCaGay ?? 0) + (a.SoGioLamViecTangCa ?? 0)) / (itemAdd.SoGioMotNgay == 1 ? 8 : itemAdd.SoGioMotNgay) > 14)
                                        {
                                            itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                                            itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                                            itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                                            itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;
                                            itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                                            itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                                            itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                                        }
                                        isDaTinhBHXH = true;
                                    }
                                }
                                if (STT == listLuongNV.Count())
                                {
                                    listLuongCheck.Add(itemAdd);
                                    // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                                    var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                                    itemAdd.SoGiamTruBanThan = 11000000;
                                    itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                                    itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
                                    itemAdd.ThuNhapMienThue = listLuongCheck.Sum(a => a.LuongLe + a.LuongLeKDL + +a.TongNguoiNLD);
                                    if (listLuongCheck.Sum(a => a.TongLuong) - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                                        itemAdd.ThuNhapTinhThueTNCN = listLuongCheck.Sum(a => a.TongLuong) - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                                    else
                                        itemAdd.ThuNhapTinhThueTNCN = 0;
                                    itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                                    // cáº¥p phÃ¡t tÃ i sáº£n
                                    if (listCapPhatTaiSan != null)
                                    {
                                        var listTaiSan = listCapPhatTaiSan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                                        if (listTaiSan != null && listTaiSan.Count > 0)
                                        {
                                            var capPhat = listTaiSan.Where(a => a.Loai == 1).ToList();
                                            if (capPhat != null && capPhat.Count > 0)
                                                itemAdd.DongPhuc = capPhat.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
                                            var ThuHoi = listTaiSan.Where(a => a.Loai == 1).ToList();
                                            if (ThuHoi != null && ThuHoi.Count > 0)
                                                itemAdd.TraTienDongPhuc = ThuHoi.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));

                                        }
                                    }
                                    itemAdd.KhauTruTaiNguon = 0;
                                    itemAdd.isCamKet = item.isCamKet;
                                    if (listLuongCheck.Sum(a => a.MucDongBaoHiemXaHoi) == 0 && item.isCamKet == false)
                                        itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;
                                    itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
                                    if (itemAdd.TongGiamThueTNCN < 50000)
                                        itemAdd.TongGiamThueTNCN = 0;
                                }
                                else
                                    listLuongCheck.Add(itemAdd);
                                STT++;
                                itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD - (itemAdd.TongGiamThueTNCN ?? 0) - (itemAdd.TongGiamThueTNCN ?? 0);
                                listResult.Add(itemAdd);
                            }
                        }
                    }
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listResult.FirstOrDefault().listTangCa, listResult.FirstOrDefault().listGioTieuChuan, IdCongty, db, IdBoPhan);
                QuyTrinh.listNhanVien = listResult.OrderBy(a => a.NgayVaoLam).ToList();
                if (LoaiLuong == 0)
                    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult, listNhanVien, listHopDong, db);
                else
                    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult.Where(a => a.LoaiLuong == LoaiLuong).ToList(), listNhanVien, listHopDong, db);
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                listResult.Add(Add);
            }
            return QuyTrinh;
        }

        public dynamic GetTinhLuongNhanVienTheoChamCong(long IdCongTy, List<long> IdBoPhan, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {
            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            //if (Host == "ICT")
            //    QuyTrinh = GetTinhLuongNhanVienTheoKyChamCongICT(IdCongty, IdBoPhan, IdKyChamCong, LoaiLuong, db);
            //else
            QuyTrinh = GetTinhLuongNhanVienTheoChamCongPHTNew(IdCongTy, IdBoPhan, TuNgayUnix, DenNgayUnix, db);
            if (QuyTrinh != null && QuyTrinh.listGiaTriNhanVien != null)
            {
                List<string> listBP = QuyTrinh.listGiaTriNhanVien.Select(a => a.TenBoPhan).Distinct().ToList();
                QuyTrinh.listBoPhan = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a }).Distinct().ToList();
                foreach (var item in QuyTrinh.listBoPhan)
                {
                    if (string.IsNullOrEmpty(item.TenBoPhan))
                    {
                        //item.listNhanVien = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();
                        item.listGiaTri = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).FirstOrDefault().listGiaTri;
                        item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();
                        item.TenBoPhan = "Tá»”NG";

                    }
                    else
                    {
                        var listNhanVien = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan).ToList();

                        {
                            item.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                            foreach (var header in QuyTrinh.listHeader_Data)
                            {
                                var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                var listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "HoTro")
                                    listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "CongGioTieuChuan")
                                    listCount = listNhanVien.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                if (listCount.Count > 0)
                                    itemAdd.GiaTri = listCount.Sum(a => a);
                                //itemAdd.Ma = "TONG";
                                item.listGiaTri.Add(itemAdd);
                            }
                        }
                        listBP = listNhanVien.Select(a => a.TenChucDanh).Distinct().ToList();

                        item.lischucDanh = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a, listNhanVien = new List<clsBangTinhLuong_NhanVien>() }).Distinct().ToList();
                        foreach (var chucDanh in item.lischucDanh)
                        {
                            var listNhanVienCD = listNhanVien.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).ToList();

                            clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                            Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                            foreach (var header in QuyTrinh.listHeader_Data)
                            {
                                var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                var listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "HoTro")
                                    listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                if (header.Ma == "CongGioTieuChuan")
                                    listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                if (listCount.Count > 0)
                                    itemAdd.GiaTri = listCount.Sum(a => a);
                                //itemAdd.Ma = "TONG";
                                Add.listGiaTri.Add(itemAdd);
                            }
                            chucDanh.listNhanVien.Add(Add);
                            chucDanh.listNhanVien.AddRange(listNhanVien.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList());
                        }

                    }
                }

            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", QuyTrinh);

        }
        public HRM_QuyTrinhTinhLuong GetTinhLuongNhanVienTheoChamCongPHT(long IdCongTy, List<long> IdBoPhan, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {

            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            DateTime NgayBD = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime NgayKT = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            var listnhanvien = GetChamCongFromMayChamCongPHT(IdCongTy, TuNgayUnix, DenNgayUnix, db);
            if (listnhanvien != null && listnhanvien.Count > 0)
            {
                var listData = listnhanvien.SelectMany(a => a.listNgay).ToList();
                List<string> listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();

                if (IdBoPhan.Count > 0)
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid) && IdBoPhan.Contains(x.IdBoPhanEOS)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                List<long> listIdChucVu = listNhanVien.Select(a => a.IdChucVu).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                var parttime = LoaiHopDongs.FirstOrDefault(a => a.Ma == "HÄVV");
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                var listId = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
                listLuong.AddRange((from a in db.HRM_Luong_NhanVien
                                    where listIdNhanVien.Contains(a.IdNhanVien) && !listId.Contains(a.IdNhanVien) && a.NgayHetHieuLuc != null && a.NgayHetHieuLuc.Value.Year > 2000 && a.NgayHetHieuLuc <= NgayBD
                                    group a by a.IdNhanVien into g
                                    select g.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault()).ToList());
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var LoaiPheps = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).ToList();
                List<string> IdLoaiPhep = LoaiPheps.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.IdCongTy == IdCongTy && listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT /*&& a.IdCongTy == IdCongty*/ && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();

                //var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongTy && (a.Nam == NgayBD.Year || a.Nam == NgayKT.Year)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe) && a.TuNgay <= NgayKT && a.DenNgay >= NgayBD).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongTy
                                                                   && x.Ngay >= NgayBD && x.Ngay <= NgayKT).ToList();
                var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();

                Parallel.ForEach(listData, item => item.isNgayLe = false);
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "033307005154")
                        item.MaNhanVien = "033307005154";
                    var datas = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayChamCong).ToList();
                    if (datas != null && datas.Count > 0)
                    {
                        HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                        itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        itemAdd.PhuCapCongChinh = 0;
                        itemAdd.PhuCapKhongTheoCongChinh = 0;
                        var listLuongNV = listLuong.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                        if (listLuongNV != null && listLuongNV.Count > 0)
                        {
                            var Luong = listLuongNV.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                            itemAdd.LoaiLuong = Luong.LoaiLuong;

                            itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                            itemAdd.LuongCoBan = Luong.Luong;
                            itemAdd.NgayApDung = Luong.NgayBatDauHieuLuc;
                            if (listLuongNV.Count > 1)
                                itemAdd.LuongCoBan = listLuongNV.Sum(a => a.Luong) / listLuongNV.Count;

                        }
                        else continue;
                        if (HopDong != null)
                        {
                            var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                            if (loaiHopDong != null)
                            {
                                itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                            }
                            var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                            if (CaLamViec == null)
                            {
                                if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                    CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                if (CaLamViec == null)
                                {
                                    string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                    CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                }
                            }
                            if (CaLamViec == null)
                                CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                            {
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                            }

                            if (itemAdd.LoaiLuong == 3)
                            {
                                itemAdd.SoGioMotNgay = 1;
                                itemAdd.NgayCongTieuChuan = 1;
                            }



                        }
                        // tÃ­nh Cong
                        if (itemAdd.SoGioMotNgay > 0)
                        {
                            double NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                            double NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                            itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                            itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                            itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));
                            if (itemAdd.LoaiLuong == 3)
                            {
                                itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                            }
                        }
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                        var CoSo = listDuAn.Where(a => listIdCoSo.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                        if (CoSo != null)
                            itemAdd.MaCoSo = string.Join(",", CoSo);
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                        {
                            itemAdd.TenChucDanh = ChucDanh.Ten;
                        }



                        // phá»¥ cáº¥p
                        var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        //double TongSoCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay;
                        foreach (var phucap in listPhuCapdb)
                        {
                            HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                            itemPC.IdNhanVien = item.IdGuid;
                            if (PhuCaps != null && PhuCaps.Count > 0)
                            {
                                var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                if (pc != null)
                                {
                                    double NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    double NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));

                                    if (itemAdd.LoaiLuong == 3)
                                    {
                                        NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0));
                                        NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0));

                                    }
                                    double TongSoCong = NgayCong + NgayCongCaGay;
                                    itemPC.Ma = phucap.Ma;
                                    itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                    itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                    double PhuCap = 0;
                                    itemPC.SoTienPhuCap = pc.SoTien;
                                    if (itemAdd.NgayCongTieuChuan > 0)
                                        PhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                    if (phucap.Ma == "PCK-KCT") /// Ä‘á»‘i vá»›i phá»¥ cáº¥p khÃ´ng theo ca chÃ­nh
                                        itemAdd.PhuCapKhongTheoCongChinh = itemAdd.PhuCapKhongTheoCongChinh + pc.SoTien;
                                    else
                                    if (phucap.Ma == "PC.CG" && pc.LoaiPhuCap != 5) /// Ä‘á»‘i vá»›i ca gÃ£y  nhÃ¢n viÃªn sale
                                    {

                                        PhuCap = ((NgayCongCaGay) / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;

                                    }
                                    else
                                    if (pc.LoaiPhuCap == 5)
                                    {
                                        if (phucap.Ma == "PC.NN")  // phá»¥ cáº¥p ngoáº¡i ngá»¯
                                        {
                                            if (TongSoCong >= 15)
                                            {
                                                PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                            }
                                            else
                                            {
                                                //PhuCap = (itemAdd.NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                        }
                                        else if (phucap.Ma == "PC.CG") // phá»¥ cáº¥p ca gÃ£y
                                        {
                                            if (itemAdd.NgayCongCaGay >= 15)
                                            {
                                                PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                            else
                                            {
                                                PhuCap = (NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        if (TongSoCong >= itemAdd.NgayCongTieuChuan || pc.LoaiPhuCap == 1)
                                            PhuCap = pc.SoTien;
                                        itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                    }

                                }
                            }
                            itemAdd.listPhuCap.Add(itemPC);
                        }
                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                        if (NghiLe != null)
                        {
                            var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe).Distinct().ToList();
                            if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                            {
                                List<int> Ngays = new List<int>();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    DateTime time = ngay.TuNgay;
                                    while (time <= ngay.DenNgay)
                                    {
                                        if (time >= NgayBD && time <= NgayKT)
                                            Ngays.Add(time.Day);
                                        time = time.AddDays(1);
                                    }
                                }

                                var data = datas.Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0) && Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                                if (data != null && data.Count > 0)
                                {
                                    itemAdd.Le_Tet = 0;
                                    itemAdd.Le_TetKDL = 0;
                                    itemAdd.LuongLe = 0;
                                    itemAdd.LuongLeKDL = 0;
                                    Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                    foreach (var ngay in NghiLeChiTiets)
                                    {
                                        if (ngay.TyLeLuong > 1)
                                        {
                                            DateTime time = ngay.TuNgay;
                                            while (time <= ngay.DenNgay)
                                            {
                                                var Luong = listLuongNV.FirstOrDefault(a => a.NgayBatDauHieuLuc <= time && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc.Value.Year > 2000 || time <= a.NgayHetHieuLuc));
                                                if (Luong != null)
                                                {
                                                    double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                    if (Luong.LoaiLuong == 3)
                                                        NgayCong = Luong.Luong;
                                                    if (Ngays.Contains(time.Day))
                                                    {


                                                        var ngayC = data.FirstOrDefault(a => a.NgayChamCong.Value.Day == time.Day);
                                                        double SoGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                                                        double TongCongN = SoGLV / (ngayC.SoGioTieuChuan ?? 8);
                                                        double TyLe = ngay.TyLeLuong / 100;

                                                        if (itemAdd.LoaiLuong == 3)
                                                        {
                                                            TyLe = 2;
                                                            itemAdd.Le_Tet += SoGLV;
                                                            itemAdd.LuongLe = itemAdd.LuongLe + SoGLV * itemAdd.LuongCoBan * TyLe;


                                                        }
                                                        else
                                                        {

                                                            itemAdd.Le_Tet += TongCongN;
                                                            itemAdd.LuongLe = itemAdd.LuongLe + NgayCong * TongCongN * (TyLe);
                                                            if (TongCongN < 1)
                                                            {

                                                                itemAdd.Le_TetKDL += 1 - TongCongN;
                                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong * (1 - TongCongN);

                                                            }
                                                        }
                                                        itemAdd.NgayCong = itemAdd.NgayCong - (ngayC.SoGioLamViec ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                        itemAdd.NgayCongCaGay = itemAdd.NgayCongCaGay - (ngayC.SoGioLamViecCaGay ?? 0) / (ngayC.SoGioTieuChuan ?? 8);

                                                        itemAdd.NgayTangCa = itemAdd.NgayTangCa - (ngayC.SoGioLamViecTangCa ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                        ngayC.isNgayLe = true;
                                                    }
                                                    else
                                                    {
                                                        List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                                        if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                                        {
                                                            if (itemAdd.LoaiLuong != 3)
                                                            {

                                                                itemAdd.Le_TetKDL += 1;
                                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                            }

                                                        }
                                                    }
                                                }
                                                time = time.AddDays(1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                    if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                    {
                                        var lNgays = listLichLamViecFind.Select(a => a.Ngay).Distinct().ToList();
                                        foreach (var itemLV in lNgays)
                                        {
                                            if (itemAdd.LoaiLuong != 3)
                                            {
                                                var Luong = listLuongNV.FirstOrDefault(a => a.NgayBatDauHieuLuc <= itemLV && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc.Value.Year > 2000 || itemLV <= a.NgayHetHieuLuc));
                                                double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                itemAdd.Le_TetKDL += 1;
                                                itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                            }

                                        }
                                    }
                                }



                            }
                        }

                        // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                        double Cong1Ngay = 0;
                        itemAdd.LuongTheoCong = 0;
                        if (listLuongNV.Count > 1)
                        {
                            itemAdd.NgayTangCa = 0;
                            itemAdd.LuongTangCa = 0;
                            itemAdd.NghiPhep = 0;
                            itemAdd.LuongPhep = 0;
                            itemAdd.LuongTheoCong = 0;
                            foreach (var Luong in listLuongNV)
                            {
                                double NgayTangCa = 0;
                                double LuongTangCa = 0;
                                double NghiPhep = 0;
                                double LuongPhep = 0;
                                double LuongTheoCong = 0;

                                if (itemAdd.NgayCongTieuChuan > 0)
                                    Cong1Ngay = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                if (Luong.LoaiLuong == 3)
                                    Cong1Ngay = Luong.Luong;
                                var data = datas.Where(a => a.isNgayLe == false && Luong.NgayBatDauHieuLuc <= a.NgayChamCong && (Luong.NgayHetHieuLuc == null || a.NgayChamCong <= Luong.NgayHetHieuLuc)).ToList();
                                // tÄƒng ca
                                {
                                    NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0) / 8);
                                    if (Luong.LoaiLuong == 3)
                                        NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                    LuongTangCa = NgayTangCa * Cong1Ngay;
                                    if (Luong.LoaiLuong == 3)
                                        LuongTangCa = NgayTangCa * Luong.Luong;
                                }

                                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                                {
                                    NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && Luong.NgayBatDauHieuLuc <= a.NgayNghi && (Luong.NgayHetHieuLuc == null || a.NgayNghi <= Luong.NgayHetHieuLuc)).Sum(a => a.ThoiGianNghi);
                                    LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                    if (Luong.LoaiLuong == 3)
                                        LuongPhep = NghiPhep * itemAdd.SoGioMotNgay * Luong.Luong;

                                }
                                double NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                double NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                                if (Luong.LoaiLuong == 3)
                                {
                                    NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0));
                                    NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                }
                                LuongTheoCong = (NgayCong + NgayCongCaGay) * Cong1Ngay;
                                if (Luong.LoaiLuong == 3)
                                    LuongTheoCong = (NgayCong + NgayCongCaGay) * Luong.Luong;


                                itemAdd.NgayTangCa += NgayTangCa;
                                itemAdd.LuongTangCa += LuongTangCa;
                                itemAdd.NghiPhep += NghiPhep;
                                itemAdd.LuongPhep += LuongPhep;
                                itemAdd.LuongTheoCong += LuongTheoCong;

                            }
                        }
                        else
                        {

                            if (itemAdd.NgayCongTieuChuan > 0)
                            {
                                Cong1Ngay = (double)((itemAdd.LuongCoBan) / itemAdd.NgayCongTieuChuan);
                            }

                            double NgayTangCa = 0;
                            double LuongTangCa = 0;
                            double NghiPhep = 0;
                            double LuongPhep = 0;
                            double LuongTheoCong = 0;


                            var data = datas.Where(a => a.isNgayLe == false && itemAdd.NgayApDung <= a.NgayChamCong).ToList();
                            // tÄƒng ca
                            {
                                NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0) / 8);
                                if (itemAdd.LoaiLuong == 3)
                                    NgayTangCa = data.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                LuongTangCa = NgayTangCa * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    LuongTangCa = NgayTangCa * itemAdd.LuongCoBan;
                            }

                            if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                            {
                                NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && itemAdd.NgayApDung <= a.NgayNghi).Sum(a => a.ThoiGianNghi);
                                LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    LuongPhep = NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                            }
                            double NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                            double NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                            if (itemAdd.LoaiLuong == 3)
                            {
                                NgayCong = data.Sum(a => (a.SoGioLamViec ?? 0));
                                NgayCongCaGay = data.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                            }
                            LuongTheoCong = (NgayCong + NgayCongCaGay) * Cong1Ngay;
                            if (itemAdd.LoaiLuong == 3)
                                LuongTheoCong = (NgayCong + NgayCongCaGay) * itemAdd.LuongCoBan;


                            //itemAdd.NgayTangCa += NgayTangCa;
                            itemAdd.LuongTangCa += LuongTangCa;
                            //itemAdd.NghiPhep += NghiPhep;
                            itemAdd.LuongPhep += LuongPhep;
                            itemAdd.LuongTheoCong += LuongTheoCong;

                            #region Old
                            //if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                            //{
                            //    itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                            //    itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                            //    if (itemAdd.LoaiLuong == 3)
                            //        itemAdd.LuongPhep = itemAdd.NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                            //}

                            //itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                            //if (itemAdd.LoaiLuong == 3)
                            //    itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * itemAdd.LuongCoBan;

                            //itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;
                            //if (itemAdd.LoaiLuong == 3)
                            //    itemAdd.LuongTangCa = itemAdd.NgayTangCa * itemAdd.LuongCoBan;
                            #endregion
                        }

                        if (itemAdd.NgayTangCa < 0)
                            itemAdd.NgayTangCa = 0;
                        if (itemAdd.LuongTangCa < 0)
                            itemAdd.LuongTangCa = 0;

                        if (itemAdd.NgayCong < 0)
                            itemAdd.NgayCong = 0;
                        if (itemAdd.NgayCongCaGay < 0)
                            itemAdd.NgayCongCaGay = 0;
                        if (itemAdd.Le_TetKDL < 0.1)
                        {
                            itemAdd.Le_TetKDL = 0;
                            itemAdd.LuongLeKDL = 0;
                        }

                        itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.LuongLeKDL + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;
                        double TongCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay + itemAdd.NghiPhep + itemAdd.Le_Tet + itemAdd.NghiBu + itemAdd.NgayTangCa;
                        if (((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= NgayBD && item.ThangTruyThuBHXH <= NgayBD) || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= NgayBD && item.ThangTruyThuBHYT <= NgayBD)))
                        {
                            itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
                        }
                        if (TongCong > 14)
                        //{
                        //    itemAdd.TruyThu = itemAdd.LuongDongBHXH * 0.32;
                        //}
                        //else
                        {
                            itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                            itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                            itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                            itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                            itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                            itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                            itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                        }

                        // báº£o hiá»ƒm xÃ£ há»™i
                        //itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                        //itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                        //itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                        //itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        //itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                        //itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                        //itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                        //itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                        //itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;


                        // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                        var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGiamTruBanThan = 11000000;
                        itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
                        itemAdd.ThuNhapMienThue = itemAdd.LuongLe + itemAdd.LuongLeKDL + +itemAdd.TongNguoiNLD;
                        if (itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                            itemAdd.ThuNhapTinhThueTNCN = itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                        else
                            itemAdd.ThuNhapTinhThueTNCN = 0;
                        itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                        // cáº¥p phÃ¡t tÃ i sáº£n
                        if (listCapPhatTaiSan != null)
                        {
                            var listTaiSan = listCapPhatTaiSan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                            if (listTaiSan != null && listTaiSan.Count > 0)
                            {
                                var capPhat = listTaiSan.Where(a => a.Loai == 1).ToList();
                                if (capPhat != null && capPhat.Count > 0)
                                    itemAdd.DongPhuc = capPhat.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
                                var ThuHoi = listTaiSan.Where(a => a.Loai == 1).ToList();
                                if (ThuHoi != null && ThuHoi.Count > 0)
                                    itemAdd.TraTienDongPhuc = ThuHoi.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));

                            }
                        }
                        itemAdd.KhauTruTaiNguon = 0;
                        itemAdd.isCamKet = item.isCamKet;
                        if (itemAdd.MucDongBaoHiemXaHoi == 0 && item.isCamKet == false)
                            itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;
                        itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
                        if (itemAdd.TongGiamThueTNCN < 50000)
                            itemAdd.TongGiamThueTNCN = 0;
                        itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD - (itemAdd.TongGiamThueTNCN ?? 0) - (itemAdd.TongGiamThueTNCN ?? 0);
                        listResult.Add(itemAdd);
                    }
                }
                List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuans = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                if (listResult.Count > 0)
                {
                    listTangCa = listResult.FirstOrDefault().listTangCa;
                    listGioTieuChuans = listResult.FirstOrDefault().listGioTieuChuan;
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listTangCa, listGioTieuChuans, IdCongTy, db, IdBoPhan);
                QuyTrinh.listNhanVien = listResult.OrderBy(a => a.NgayVaoLam).ToList();
                //if (LoaiLuong == 0)
                QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult, listNhanVien, listHopDong, db);
                //else
                //    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult.Where(a => a.LoaiLuong == LoaiLuong).ToList(), listNhanVien, listHopDong, db);
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuans = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                if (listResult.Count > 0)
                {
                    listTangCa = listResult.FirstOrDefault().listTangCa;
                    listGioTieuChuans = listResult.FirstOrDefault().listGioTieuChuan;
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listTangCa, listGioTieuChuans, IdCongTy, db, IdBoPhan);
                listResult.Add(Add);
            }
            return QuyTrinh;
        }
        public HRM_QuyTrinhTinhLuong GetTinhLuongNhanVienTheoChamCongPHTNew(long IdCongTy, List<long> IdBoPhan, double TuNgayUnix, double DenNgayUnix, ApplicationDbContext db)
        {

            HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            DateTime NgayBD = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
            DateTime NgayKT = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date;
            var listnhanvien = GetChamCongFromMayChamCongPHT(IdCongTy, TuNgayUnix, DenNgayUnix, db);
            if (listnhanvien != null && listnhanvien.Count > 0)
            {
                var listData = listnhanvien.SelectMany(a => a.listNgay).ToList();
                List<string> listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = new List<HRM_NhanVien>();

                if (IdBoPhan.Count > 0)
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid) && IdBoPhan.Contains(x.IdBoPhanEOS)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();
                else
                    listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).OrderBy(a => a.NgayBatDauLamViec).ThenBy(a => a.TenDayDu).ToList();

                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                List<long> listIdChucVu = listNhanVien.Select(a => a.IdChucVu).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                var parttime = LoaiHopDongs.FirstOrDefault(a => a.Ma == "HÄVV");
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayKT && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayBD)).OrderByDescending(a => a.NgayBatDauHieuLuc).ToList();
                var listId = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
                listLuong.AddRange((from a in db.HRM_Luong_NhanVien
                                    where listIdNhanVien.Contains(a.IdNhanVien) && !listId.Contains(a.IdNhanVien) && a.NgayHetHieuLuc != null && a.NgayHetHieuLuc.Value.Year > 2000 && a.NgayHetHieuLuc <= NgayBD
                                    group a by a.IdNhanVien into g
                                    select g.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault()).ToList());
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var LoaiPheps = _CallWebService.GetListDanhMucHinhThucNghi(db).Where(a => a.Ma == DefineData.eKyHieuChamCong.PN.ToString() || a.Ma == DefineData.eKyHieuChamCong.NP.ToString() || a.Ma == DefineData.eKyHieuChamCong.NC.ToString() || a.Ma == DefineData.eKyHieuChamCong.NTL.ToString()).ToList();
                List<string> IdLoaiPhep = LoaiPheps.Select(a => a.Id).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => a.IdCongTy == IdCongTy && listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT /*&& a.IdCongTy == IdCongty*/ && IdLoaiPhep.Contains(a.IdHinhThucNghi)).ToList();

                //var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan) && a.IdCongTy == IdCongTy && (a.Nam == NgayBD.Year || a.Nam == NgayKT.Year)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe) && a.TuNgay <= NgayKT && a.DenNgay >= NgayBD).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                List<HRM_ThoiGianLamViecKeHoach> listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdNhanVien) && x.IdCongTy == IdCongTy
                                                                   && x.Ngay >= NgayBD && x.Ngay <= NgayKT).ToList();
                var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();

                Parallel.ForEach(listData, item => item.isNgayLe = false);
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    if (item.MaNhanVien == "001304028349")
                        item.MaNhanVien = "001304028349";
                    List<HRM_QuyTrinhTinhLuong_NhanVien> listLuongCheck = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
                    var listdatas = listData.Where(a => a.IdNhanVien == item.IdGuid).ToList();

                    var listLuongNV = listLuong.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.NgayBatDauHieuLuc).ToList();
                    if (listLuongNV != null && listLuongNV.Count > 0)
                    {
                        int STT = 1;
                        bool isDaTinhBHXH = false;
                        foreach (var Luong in listLuongNV)
                        {
                            HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                            //var Luong = listLuongNV.OrderByDescending(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                            itemAdd.LoaiLuong = Luong.LoaiLuong;

                            itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                            itemAdd.LuongCoBan = Luong.Luong;
                            itemAdd.NgayApDung = Luong.NgayBatDauHieuLuc;

                            var datas = listdatas.Where(a => a.NgayChamCong >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayChamCong <= Luong.NgayHetHieuLuc))).OrderBy(a => a.NgayChamCong).ToList();
                            if (datas != null && datas.Count > 0)
                            {

                                itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                                var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid && a.IdGuid == Luong.IdNguonNhapLuong);
                                itemAdd.PhuCapCongChinh = 0;
                                itemAdd.PhuCapKhongTheoCongChinh = 0;
                                if (HopDong != null)
                                {
                                    var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                                    if (loaiHopDong != null)
                                    {
                                        itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                        itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                                    }
                                    var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                                    if (CaLamViec == null)
                                    {
                                        if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                            CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                        if (CaLamViec == null)
                                        {
                                            string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec) && a.Ngay >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.Ngay <= Luong.NgayHetHieuLuc))).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                            CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                        }
                                    }
                                    if (CaLamViec == null)
                                        CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                                    {
                                        itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                        itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                        itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                                    }
                                }
                                else
                                {
                                    var CaLamViec = new HRM_dmLichLamViec();

                                    if (!string.IsNullOrEmpty(item.IdCaLamViec))
                                        CaLamViec = caLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
                                    else
                                    {
                                        string Id = listLichLamViec.Where(a => a.Loai == 0 && a.IdNhanVien == item.IdGuid && !string.IsNullOrEmpty(a.IddmCaLamViec)).Select(a => a.IddmCaLamViec).FirstOrDefault();
                                        CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == Id);
                                    }

                                    if (CaLamViec == null)
                                        CaLamViec = new HRM_dmLichLamViec() { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };

                                    {
                                        itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                        itemAdd.CaLamViec = CaLamViec.Ten ?? "";
                                        itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                                    }
                                }
                                if (itemAdd.LoaiLuong == 3)
                                {
                                    itemAdd.SoGioMotNgay = 1;
                                    itemAdd.NgayCongTieuChuan = 1;
                                }

                                // tÃ­nh Cong
                                if (itemAdd.SoGioMotNgay > 0)
                                {
                                    double NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                    double NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                    itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));
                                    if (itemAdd.LoaiLuong == 3)
                                    {
                                        itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                        itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                        itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0));
                                    }
                                }
                                itemAdd.IdNhanVien = item.IdGuid;
                                itemAdd.TenNhanVien = item.TenDayDu;
                                itemAdd.MaNhanVien = item.MaNhanVien;
                                if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                    itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                                if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                    itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                                var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == item.IdGuid).Select(a => a.IdBoPhan).ToList();
                                var CoSo = listDuAn.Where(a => listIdCoSo.Contains(a.Id)).Select(a => a.TenBoPhan).ToList();
                                if (CoSo != null)
                                    itemAdd.MaCoSo = string.Join(",", CoSo);
                                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                                if (BoPhan != null)
                                {
                                    itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                                }
                                var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                                if (ChucDanh != null)
                                {
                                    itemAdd.TenChucDanh = ChucDanh.Ten;
                                }
                                var ChucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                                if (ChucVu != null)
                                {
                                    itemAdd.TenChucVu = ChucVu.TenChucVu;
                                }

                                // phá»¥ cáº¥p
                                var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid && a.IdLuong_NhanVien == Luong.IdGuid).ToList();
                                foreach (var phucap in listPhuCapdb)
                                {
                                    HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                                    itemPC.IdNhanVien = item.IdGuid;
                                    if (PhuCaps != null && PhuCaps.Count > 0)
                                    {
                                        var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                        if (pc != null)
                                        {
                                            double NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                                            double NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));

                                            if (itemAdd.LoaiLuong == 3)
                                            {
                                                NgayCong = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViec ?? 0));
                                                NgayCongCaGay = datas.Where(a => a.NgayChamCong >= pc.NgayBatDauHieuLuc).Sum(a => (a.SoGioLamViecCaGay ?? 0));

                                            }
                                            double TongSoCong = NgayCong + NgayCongCaGay;
                                            itemPC.Ma = phucap.Ma;
                                            itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                            itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                            double PhuCap = 0;
                                            itemPC.SoTienPhuCap = pc.SoTien;
                                            if (itemAdd.NgayCongTieuChuan > 0)
                                                PhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                            if (phucap.Ma == "PCK-KCT") /// Ä‘á»‘i vá»›i phá»¥ cáº¥p khÃ´ng theo ca chÃ­nh
                                                itemAdd.PhuCapKhongTheoCongChinh = itemAdd.PhuCapKhongTheoCongChinh + pc.SoTien;
                                            else
                                            if (phucap.Ma == "PC.CG" && pc.LoaiPhuCap != 5) /// Ä‘á»‘i vá»›i ca gÃ£y  nhÃ¢n viÃªn sale
                                            {

                                                PhuCap = ((NgayCongCaGay) / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;

                                            }
                                            else
                                            if (pc.LoaiPhuCap == 5)
                                            {
                                                if (phucap.Ma == "PC.NN")  // phá»¥ cáº¥p ngoáº¡i ngá»¯
                                                {
                                                    if (TongSoCong >= 15)
                                                    {
                                                        PhuCap = pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                                    }
                                                    else
                                                    {
                                                        //PhuCap = (itemAdd.NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                }
                                                else if (phucap.Ma == "PC.CG") // phá»¥ cáº¥p ca gÃ£y
                                                {
                                                    if (itemAdd.NgayCongCaGay >= 15)
                                                    {
                                                        PhuCap = pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                    else
                                                    {
                                                        PhuCap = (NgayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                if (TongSoCong >= itemAdd.NgayCongTieuChuan || pc.LoaiPhuCap == 1)
                                                    PhuCap = pc.SoTien;
                                                itemAdd.PhuCapCongChinh = (itemAdd.PhuCapCongChinh) + PhuCap;
                                            }

                                        }
                                    }
                                    itemAdd.listPhuCap.Add(itemPC);
                                }
                                var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                                if (NghiLe != null)
                                {
                                    var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe).Distinct().ToList();
                                    if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                                    {
                                        List<int> Ngays = new List<int>();
                                        foreach (var ngay in NghiLeChiTiets)
                                        {
                                            DateTime time = ngay.TuNgay;
                                            while (time <= ngay.DenNgay)
                                            {
                                                if (time >= NgayBD && time <= NgayKT)
                                                    Ngays.Add(time.Day);
                                                time = time.AddDays(1);
                                            }
                                        }

                                        var data = datas.Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0) && Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                                        if (data != null && data.Count > 0)
                                        {
                                            itemAdd.Le_Tet = 0;
                                            itemAdd.Le_TetKDL = 0;
                                            itemAdd.LuongLe = 0;
                                            itemAdd.LuongLeKDL = 0;
                                            Ngays = data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                            foreach (var ngay in NghiLeChiTiets)
                                            {
                                                if (ngay.TyLeLuong > 1)
                                                {
                                                    DateTime time = ngay.TuNgay;
                                                    while (time <= ngay.DenNgay)
                                                    {
                                                        double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                        if (Luong.LoaiLuong == 3)
                                                            NgayCong = Luong.Luong;
                                                        if (Ngays.Contains(time.Day))
                                                        {


                                                            var ngayC = data.FirstOrDefault(a => a.NgayChamCong.Value.Day == time.Day);
                                                            double SoGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                                                            double TongCongN = SoGLV / (ngayC.SoGioTieuChuan ?? 8);
                                                            double TyLe = ngay.TyLeLuong / 100;

                                                            if (itemAdd.LoaiLuong == 3)
                                                            {
                                                                TyLe = 2;
                                                                itemAdd.Le_Tet += SoGLV;
                                                                itemAdd.LuongLe = itemAdd.LuongLe + SoGLV * itemAdd.LuongCoBan * TyLe;


                                                            }
                                                            else
                                                            {

                                                                itemAdd.Le_Tet += TongCongN;
                                                                itemAdd.LuongLe = itemAdd.LuongLe + NgayCong * TongCongN * (TyLe);
                                                                if (TongCongN < 1)
                                                                {

                                                                    itemAdd.Le_TetKDL += 1 - TongCongN;
                                                                    itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong * (1 - TongCongN);

                                                                }
                                                            }
                                                            itemAdd.NgayCong = itemAdd.NgayCong - (ngayC.SoGioLamViec ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                            itemAdd.NgayCongCaGay = itemAdd.NgayCongCaGay - (ngayC.SoGioLamViecCaGay ?? 0) / (ngayC.SoGioTieuChuan ?? 8);

                                                            itemAdd.NgayTangCa = itemAdd.NgayTangCa - (ngayC.SoGioLamViecTangCa ?? 0) / (ngayC.SoGioTieuChuan ?? 8);
                                                            ngayC.isNgayLe = true;
                                                        }
                                                        else
                                                        {
                                                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                                            if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                                            {
                                                                if (itemAdd.LoaiLuong != 3)
                                                                {

                                                                    itemAdd.Le_TetKDL += 1;
                                                                    itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                                }

                                                            }
                                                        }

                                                        time = time.AddDays(1);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<HRM_ThoiGianLamViecKeHoach> listLichLamViecFind = listLichLamViec.FindAll(x => x.IdNhanVien == item.IdGuid && Ngays.Contains(x.Ngay.Day));
                                            if (listLichLamViecFind != null && listLichLamViecFind.Count > 0)
                                            {
                                                var lNgays = listLichLamViecFind.Select(a => a.Ngay).Distinct().ToList();
                                                foreach (var itemLV in lNgays)
                                                {
                                                    if (itemAdd.LoaiLuong != 3)
                                                    {
                                                        double NgayCong = (double)(Luong.Luong / itemAdd.NgayCongTieuChuan);
                                                        itemAdd.Le_TetKDL += 1;
                                                        itemAdd.LuongLeKDL = itemAdd.LuongLeKDL + NgayCong;
                                                    }

                                                }
                                            }
                                        }



                                    }
                                }
                                // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                                double Cong1Ngay = itemAdd.LuongCoBan / itemAdd.NgayCongTieuChuan;
                                itemAdd.LuongTheoCong = 0;
                                #region Old
                                if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                                {
                                    itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid && a.NgayNghi >= Luong.NgayBatDauHieuLuc && (Luong.NgayHetHieuLuc == null || (Luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayNghi <= Luong.NgayHetHieuLuc))).Sum(a => a.ThoiGianNghi);
                                    itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                                    if (itemAdd.LoaiLuong == 3)
                                        itemAdd.LuongPhep = itemAdd.NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan;

                                }
                                itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    itemAdd.LuongTheoCong = (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * itemAdd.LuongCoBan;

                                itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;
                                if (itemAdd.LoaiLuong == 3)
                                    itemAdd.LuongTangCa = itemAdd.NgayTangCa * itemAdd.LuongCoBan;
                                #endregion
                                if (itemAdd.NgayTangCa < 0)
                                    itemAdd.NgayTangCa = 0;
                                if (itemAdd.LuongTangCa < 0)
                                    itemAdd.LuongTangCa = 0;

                                if (itemAdd.NgayCong < 0)
                                    itemAdd.NgayCong = 0;
                                if (itemAdd.NgayCongCaGay < 0)
                                    itemAdd.NgayCongCaGay = 0;
                                if (itemAdd.Le_TetKDL < 0.1)
                                {
                                    itemAdd.Le_TetKDL = 0;
                                    itemAdd.LuongLeKDL = 0;
                                }
                                itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.LuongLeKDL + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;
                                double TongCong = itemAdd.NgayCong + itemAdd.NgayCongCaGay + itemAdd.NghiPhep + itemAdd.Le_Tet + itemAdd.NghiBu + itemAdd.NgayTangCa;
                                if (((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= NgayBD && item.ThangTruyThuBHXH <= NgayBD) || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= NgayBD && item.ThangTruyThuBHYT <= NgayBD)))
                                {
                                    itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
                                }
                                if (Luong.LuongHachToan > 0 && Luong.LoaiLuong == 1)
                                {
                                    if (isDaTinhBHXH == false)
                                    {
                                        DateTime TuNgay = listLuongNV.Where(a => a.LoaiLuong == 1 && a.LuongHachToan > 0).OrderBy(a => a.NgayBatDauHieuLuc).Select(a => a.NgayBatDauHieuLuc).FirstOrDefault();
                                        DateTime? DenNgay = listLuongNV.Where(a => a.LoaiLuong == 1 && a.LuongHachToan > 0).OrderByDescending(a => a.NgayBatDauHieuLuc).Select(a => a.NgayHetHieuLuc).FirstOrDefault();

                                        if (listdatas.Where(a => a.NgayChamCong >= TuNgay && (DenNgay == null || (DenNgay.Value.Year > 2000 && DenNgay >= a.NgayChamCong))).Sum(a => (a.SoGioLamViec ?? 0) + (a.SoGioLamViecCaGay ?? 0) + (a.SoGioLamViecTangCa ?? 0)) / (itemAdd.SoGioMotNgay == 1 ? 8 : itemAdd.SoGioMotNgay) > 14)
                                        {
                                            itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                                            itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                                            itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                                            itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;
                                            itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                                            itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                                            itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                                            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                                        }
                                        isDaTinhBHXH = true;
                                    }
                                }

                                if (STT == listLuongNV.Count())
                                {
                                    listLuongCheck.Add(itemAdd);
                                    // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                                    var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                                    itemAdd.SoGiamTruBanThan = 11000000;
                                    itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                                    itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
                                    itemAdd.ThuNhapMienThue = listLuongCheck.Sum(a => a.LuongLe + a.LuongLeKDL + +a.TongNguoiNLD);
                                    if (listLuongCheck.Sum(a => a.TongLuong) - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                                        itemAdd.ThuNhapTinhThueTNCN = listLuongCheck.Sum(a => a.TongLuong) - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                                    else
                                        itemAdd.ThuNhapTinhThueTNCN = 0;
                                    itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                                    // cáº¥p phÃ¡t tÃ i sáº£n
                                    if (listCapPhatTaiSan != null)
                                    {
                                        var listTaiSan = listCapPhatTaiSan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                                        if (listTaiSan != null && listTaiSan.Count > 0)
                                        {
                                            var capPhat = listTaiSan.Where(a => a.Loai == 1).ToList();
                                            if (capPhat != null && capPhat.Count > 0)
                                                itemAdd.DongPhuc = capPhat.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
                                            var ThuHoi = listTaiSan.Where(a => a.Loai == 1).ToList();
                                            if (ThuHoi != null && ThuHoi.Count > 0)
                                                itemAdd.TraTienDongPhuc = ThuHoi.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));

                                        }
                                    }
                                    itemAdd.KhauTruTaiNguon = 0;
                                    itemAdd.isCamKet = item.isCamKet;
                                    if (listLuongCheck.Sum(a => a.MucDongBaoHiemXaHoi) == 0 && item.isCamKet == false)
                                        itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;
                                    itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
                                    if (itemAdd.TongGiamThueTNCN < 50000)
                                        itemAdd.TongGiamThueTNCN = 0;
                                }
                                else
                                    listLuongCheck.Add(itemAdd);
                                STT++;
                                itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD - (itemAdd.TongGiamThueTNCN ?? 0) - (itemAdd.TongGiamThueTNCN ?? 0);
                                listResult.Add(itemAdd);
                            }
                        }
                    }
                }
                List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuans = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                if (listResult.Count > 0)
                {
                    listTangCa = listResult.FirstOrDefault().listTangCa;
                    listGioTieuChuans = listResult.FirstOrDefault().listGioTieuChuan;
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listTangCa, listGioTieuChuans, IdCongTy, db, IdBoPhan);
                QuyTrinh.listNhanVien = listResult.OrderBy(a => a.NgayVaoLam).ToList();
                //if (LoaiLuong == 0)
                QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult, listNhanVien, listHopDong, db);
                //else
                //    QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, listResult.Where(a => a.LoaiLuong == LoaiLuong).ToList(), listNhanVien, listHopDong, db);
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuans = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                if (listResult.Count > 0)
                {
                    listTangCa = listResult.FirstOrDefault().listTangCa;
                    listGioTieuChuans = listResult.FirstOrDefault().listGioTieuChuan;
                }
                QuyTrinh = GetListHeaderNew(QuyTrinh, listTangCa, listGioTieuChuans, IdCongTy, db, IdBoPhan);
                listResult.Add(Add);
            }
            return QuyTrinh;
        }

        public dynamic GetTinhLuongNhanVienTheoKyExcel(string FileName, double TuNgay, double DenNgay, ApplicationDbContext db)
        {

            //DateTime dTuNgay = dungchung.UnixTimeStampToDateTime(TuNgay);
            //DateTime dDenNgay = dungchung.UnixTimeStampToDateTime(DenNgay);
            string pathTemplate = PathUploads + FileName;
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets.First();


            List<HRM_QuyTrinhTinhLuong_NhanVien> listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
            //var BangChamCong = db.HRM_BangChamCong.Where(a => IdKyChamCong.Contains(a.IdGuid)).ToList();
            DateTime NgayBD = dungchung.UnixTimeStampToDateTime(TuNgay);
            DateTime NgayKT = dungchung.UnixTimeStampToDateTime(DenNgay);
            var listData = new List<HRM_BangChamCong_NhanVien_ThoiGian>();
            for (int row = 4; row <= sheet1.Cells.End.Row; row++)
            {
                HRM_BangChamCong_NhanVien_ThoiGian data = new HRM_BangChamCong_NhanVien_ThoiGian();
                data.MaNhanVien = dungchung.GetCell(sheet1, row, 2);
                if (string.IsNullOrEmpty(data.MaNhanVien))
                    break;
                data.SoGioLamViec = dungchung.GetCellToDouble(sheet1, row, 6);
                data.SoGioLamViecTangCa = dungchung.GetCellToDouble(sheet1, row, 7);
                listData.Add(data);
            }

            //db.HRM_BangChamCong_NhanVien_ThoiGian.Where(a => IdKyChamCong.Contains(a.IdBangChamCong)).ToList();
            //if (IdBoPhan != null && IdBoPhan.Count > 0)
            //    listData = listData.Where(a => IdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            if (listData != null && listData.Count > 0)
            {
                List<string> listMa = listData.Select(a => a.MaNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listMa.Contains(x.MaNhanVien)).ToList();
                if (listMa.Count > listNhanVien.Count)
                {
                    var listmaCheck = listNhanVien.Select(a => a.MaNhanVien).ToList();
                    var listMaOut = listMa.Where(a => !listmaCheck.Contains(a)).ToList();
                    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "MÃ£ nhÃ¢n viÃªn khÃ´ng Ä‘Ãºng " + string.Join(", ", listMaOut));
                }

                List<string> listIdNhanVien = listNhanVien.Select(x => x.IdGuid).Distinct().ToList();
                //List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).ToList();
                List<long> listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                listIdBoPhan.AddRange(listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList());
                var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan);
                var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Created).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var caLamViecs = db.HRM_dmLichLamViec.ToList();
                var LoaiHopDongs = db.HRM_dmLoaiHopDong.ToList();
                List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listPhuCapdb = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
                var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayBatDauHieuLuc <= NgayBD && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= NgayKT)).ToList();
                var listLuong = db.HRM_Luong_NhanVien.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                var listNguoiThan = db.HRM_NhanVienQuanHeGiaDinh.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true).ToList();
                var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.NgayNghi >= NgayBD && a.NgayNghi <= NgayKT).ToList();
                var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TinhLuong == true && ((a.NgayCapPhat != null && a.NgayCapPhat >= NgayBD && a.NgayCapPhat <= NgayKT) || (a.NgayThuHoi != null && a.NgayThuHoi >= NgayBD && a.NgayThuHoi <= NgayKT))).ToList();
                var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.Where(a => listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                List<string> listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
                var listLichNghiChiTiet = db.HRM_dmLichNghiLe_Ngay.Where(a => listIdLichNghi.Contains(a.IdLichNghiLe)).ToList();
                List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.OrderByDescending(a => a.STT).ToList();
                // tÃ­nh lÆ°Æ¡ng
                foreach (var item in listNhanVien)
                {
                    var datas = listData.Where(a => a.MaNhanVien == item.MaNhanVien).OrderBy(a => a.NgayChamCong).ToList();
                    if (datas != null && datas.Count > 0)
                    {
                        HRM_QuyTrinhTinhLuong_NhanVien itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien();
                        itemAdd.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                        itemAdd.IdNhanVien = item.IdGuid;
                        itemAdd.TenNhanVien = item.TenDayDu;
                        itemAdd.MaNhanVien = item.MaNhanVien;
                        if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                            itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                        if (CongTy != null)
                        {
                            itemAdd.MaCoSo = CongTy.TenBoPhan;
                        }
                        var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                        if (BoPhan != null)
                        {
                            itemAdd.MaBoPhan = BoPhan.MaBoPhan;
                            itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                        }
                        var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                        if (ChucDanh != null)
                        {
                            itemAdd.TenChucDanh = ChucDanh.Ten;
                        }
                        var Luong = listLuong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        if (Luong != null)
                        {
                            itemAdd.LuongCoBan = Luong.Luong;
                            itemAdd.LuongDongBHXH = Luong.LuongHachToan;
                        }

                        var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                        itemAdd.PhuCapCongChinh = 0;
                        itemAdd.PhuCapKhongTheoCongChinh = 0;
                        if (HopDong != null)
                        {
                            var loaiHopDong = LoaiHopDongs.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                            if (loaiHopDong != null)
                            {
                                itemAdd.IdLoaiHopDong = HopDong.IdLoaiHopDong;
                                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
                            }
                            var CaLamViec = caLamViecs.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                            if (CaLamViec != null)
                            {
                                itemAdd.NgayCongTieuChuan = CaLamViec.SoNgayCongTieuChuan ?? 1;
                                itemAdd.CaLamViec = CaLamViec.Ten;
                                itemAdd.SoGioMotNgay = CaLamViec.SoGioTrongNgay ?? 0;

                            }
                            // tÃ­nh Cong
                            if (itemAdd.SoGioMotNgay > 0)
                            {
                                double NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0));
                                //double NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0));
                                itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / itemAdd.SoGioMotNgay);
                                //itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / itemAdd.SoGioMotNgay);
                            }

                        }
                        // phá»¥ cáº¥p
                        var PhuCaps = listPhuCap.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        double TongSoCong = itemAdd.NgayCong /*+ itemAdd.NgayCongCaGay*/;
                        foreach (var phucap in listPhuCapdb)
                        {
                            HRM_QuyTrinhTinhLuong_PhuCap itemPC = new HRM_QuyTrinhTinhLuong_PhuCap();
                            itemPC.IdNhanVien = item.IdGuid;
                            if (PhuCaps != null && PhuCaps.Count > 0)
                            {
                                var pc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == phucap.IdGuid);
                                if (pc != null)
                                {
                                    itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                                    itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                                    double PhuCap = 0;
                                    itemPC.SoTienPhuCap = pc.SoTien;
                                    if (itemAdd.NgayCongTieuChuan > 0)
                                        PhuCap = (TongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                                    if (pc.LoaiPhuCap == 5 && TongSoCong > 15)
                                    {
                                        PhuCap = pc.SoTien;
                                    }
                                    if (phucap.Ma != "KHAC")
                                        itemAdd.PhuCapCongChinh = itemAdd.PhuCapCongChinh + PhuCap;
                                    else
                                        itemAdd.PhuCapKhongTheoCongChinh += pc.SoTien;
                                }
                            }
                            itemAdd.listPhuCap.Add(itemPC);
                        }
                        if (itemAdd.listGioTieuChuan.Count > 0)
                        {
                            itemAdd.NgayCongTieuChuan = itemAdd.listGioTieuChuan.OrderByDescending(a => a.TongSoGio).FirstOrDefault().CongTieuChuan;
                            itemAdd.SoGioMotNgay = itemAdd.listGioTieuChuan.OrderByDescending(a => a.TongSoGio).FirstOrDefault().GioTieuChuan;
                        }
                        else
                        {
                            itemAdd.NgayCongTieuChuan = 26;
                            itemAdd.SoGioMotNgay = 8;
                        }
                        // ngÃ y cÃ´ng vÃ  LÆ°Æ¡ng
                        double Cong1Ngay = 0;
                        if (itemAdd.NgayCongTieuChuan > 0)
                            Cong1Ngay = (double)(itemAdd.LuongCoBan / itemAdd.NgayCongTieuChuan);

                        itemAdd.LuongTheoCong = (itemAdd.NgayCong /*+ itemAdd.NgayCongCaGay*/) * Cong1Ngay;
                        if (listXinNghiPhep != null && listXinNghiPhep.Count > 0)
                        {
                            itemAdd.NghiPhep = listXinNghiPhep.Where(a => a.IdNhanVien == item.IdGuid).Sum(a => a.ThoiGianNghi);
                            itemAdd.LuongPhep = itemAdd.NghiPhep * Cong1Ngay;
                        }
                        var NghiLe = listLichNghi.FirstOrDefault(a => a.IdBoPhan == item.IdBoPhanEOS);
                        var NghiLeChiTiets = listLichNghiChiTiet.Where(a => a.IdLichNghiLe == NghiLe.IdLichNghiLe &&
                        ((a.TuNgay.Month <= NgayBD.Month)
                        || a.DenNgay.Month >= NgayBD.Month
                        || a.TuNgay.Month <= NgayKT.Month
                        || a.DenNgay.Month >= NgayKT.Month)).ToList();
                        if (NghiLeChiTiets != null && NghiLeChiTiets.Count > 0)
                        {
                            List<int> Ngays = new List<int>();
                            foreach (var ngay in NghiLeChiTiets)
                            {
                                DateTime time = ngay.TuNgay;
                                while (time <= ngay.DenNgay)
                                {
                                    if (time >= NgayBD && time <= NgayKT)
                                        Ngays.Add(time.Day);
                                    time = time.AddDays(1);
                                }
                            }
                            //var data = datas.Where(a => Ngays.Contains(a.NgayChamCong.Value.Day)).ToList();
                            if (Ngays.Contains(NgayBD.Day) || Ngays.Contains(NgayKT.Day))
                            {
                                itemAdd.Le_Tet = 0;
                                itemAdd.LuongLe = 0;
                                Ngays = new List<int>();
                                Ngays.Add(NgayBD.Day);
                                Ngays.Add(NgayKT.Day);
                                //data.Select(a => a.NgayChamCong.Value.Day).Distinct().ToList();
                                foreach (var ngay in NghiLeChiTiets)
                                {
                                    if (ngay.TyLeLuong > 1)
                                    {
                                        DateTime time = ngay.TuNgay;
                                        while (time <= ngay.DenNgay)
                                        {
                                            if (Ngays.Contains(time.Day))
                                            {
                                                double TyLe = ngay.TyLeLuong / 100;
                                                itemAdd.Le_Tet += TyLe - 1;
                                                itemAdd.LuongLe = itemAdd.LuongLe + Cong1Ngay * (TyLe - 1);
                                            }
                                            time = time.AddDays(1);
                                        }
                                    }
                                }
                            }
                        }

                        // tÄƒng ca
                        if (itemAdd.SoGioMotNgay > 0)
                            itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / itemAdd.SoGioMotNgay); ;
                        itemAdd.LuongTangCa = itemAdd.NgayTangCa * Cong1Ngay;

                        itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;

                        // báº£o hiá»ƒm xÃ£ há»™i
                        if (item.isLuongHuu == false)
                        {
                            itemAdd.MucDongBaoHiemXaHoi = itemAdd.LuongDongBHXH;
                            itemAdd.SDLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 17.5 / 100;
                            itemAdd.SDLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 3 / 100;
                            itemAdd.SDLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;

                            itemAdd.NLDBHXH = itemAdd.MucDongBaoHiemXaHoi * 8 / 100;
                            itemAdd.NLDBHYT = itemAdd.MucDongBaoHiemXaHoi * 1.5 / 100;
                            itemAdd.NLDBHTN = itemAdd.MucDongBaoHiemXaHoi * 1 / 100;
                            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
                        }

                        // thuáº¿ thu nháº­p cÃ¡ nhÃ¢n
                        var QuanHes = listNguoiThan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        itemAdd.SoGiamTruBanThan = 11000000;
                        itemAdd.SpNguoiDangKyNPT = QuanHes.Count;
                        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
                        itemAdd.ThuNhapMienThue = itemAdd.PhuCapCongChinh + itemAdd.TongNguoiNLD;
                        if (itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue > 0)
                            itemAdd.ThuNhapTinhThueTNCN = itemAdd.TongLuong - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
                        else
                            itemAdd.ThuNhapTinhThueTNCN = 0;
                        itemAdd.ThueTNCNTheoLuyTien = TinhThue(itemAdd.ThuNhapTinhThueTNCN, listCauHinh);
                        // cáº¥p phÃ¡t tÃ i sáº£n
                        //if (listCapPhatTaiSan != null)
                        //{
                        //    var listTaiSan = listCapPhatTaiSan.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        //    if (listTaiSan != null && listTaiSan.Count > 0)
                        //    {
                        //        var capPhat = listTaiSan.Where(a => a.Loai == 1).ToList();
                        //        if (capPhat != null && capPhat.Count > 0)
                        //            itemAdd.DongPhuc = capPhat.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
                        //        var ThuHoi = listTaiSan.Where(a => a.Loai == 1).ToList();
                        //        if (ThuHoi != null && ThuHoi.Count > 0)
                        //            itemAdd.TraTienDongPhuc = ThuHoi.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));

                        //    }
                        //}
                        itemAdd.KhauTruTaiNguon = 0;
                        if (itemAdd.MucDongBaoHiemXaHoi > 0 && itemAdd.TongLuong >= 20000000)
                            itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;
                        itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
                        itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD /*- (itemAdd.DongPhuc ?? 0) + (itemAdd.TraTienDongPhuc ?? 0)*/;
                        listResult.Add(itemAdd);
                    }
                }
            }
            else
            {
                HRM_QuyTrinhTinhLuong_NhanVien Add = new HRM_QuyTrinhTinhLuong_NhanVien();
                listResult.Add(Add);
            }
            return listResult;
        }
        public dynamic GetDanhSachBangChamCong(long IdCongTy, ApplicationDbContext db)
        {
            DateTime time = DateTime.Today.AddMonths(-2);
            time = new DateTime(time.Year, time.Month, 1, 0, 0, 0);
            var listResult = db.HRM_BangChamCong.Where(a => time <= a.TuNgay && a.IdCongTyEOS == IdCongTy && a.isKetThuc == true).ToList();

            #region check bá»™ pháº­n Ä‘Ã£ tÃ­nh lÆ°Æ¡ng
            var list = (from a in db.HRM_QuyTrinhTinhLuong
                        join b in db.HRM_QuyTrinhTinhLuong_ChamCong on a.IdGuid equals b.IdBangTinhLuong
                        select b
                               ).ToList();
            if (list != null && list.Count > 0)
            {
                var listIdBCC = listResult.Select(a => a.IdGuid).Distinct().ToList();

                var listBPGoc = db.HRM_BangChamCong_NhanVien.Where(a => listIdBCC.Contains(a.IdBangChamCong) && a.isKetThuc == true).Select(a => new { a.IdBoPhanEOS, a.IdBangChamCong }).Distinct().ToList();
                var listId = list.Select(a => a.IdBangTinhLuong).Distinct().ToList();
                var listBoPhanCheck = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listId.Contains(a.IdQuyTrinh)).ToList();
                var listIdCheck = new List<string>();
                foreach (var item in listResult)
                {
                    item.DateTimeToUnix();
                    var listIdBPG = listBPGoc.Select(a => a.IdBoPhanEOS).Distinct().ToList();
                    var QTLuong = list.Where(a => a.IdBangChamCong == item.IdGuid).ToList();
                    if (QTLuong != null && QTLuong.Count > 0)
                    {
                        listId = list.Select(a => a.IdBangTinhLuong).Distinct().ToList();
                        var listBoPhanCheckChild = listBoPhanCheck.Where(a => listId.Contains(a.IdQuyTrinh)).Select(a => a.IdBoPhan).Distinct().ToList();
                        if (listBoPhanCheckChild == null && listBoPhanCheckChild.Count == 0)
                            listIdCheck.Add(item.IdGuid);
                        var listIDBp = listIdBPG.Where(a => !listBoPhanCheckChild.Contains(a)).ToList();
                        if (listIDBp == null && listIDBp.Count == 0)
                            listIdCheck.Add(item.IdGuid);
                    }

                }
                if (listIdCheck.Count > 0)
                    listResult = listResult.Where(a => !listIdCheck.Contains(a.IdGuid)).ToList();
            }
            else
                foreach (var item in listResult)
                    item.DateTimeToUnix();
            #endregion

            return listResult;
        }
        public dynamic ExportBangChamCong(ApplicationDbContext db)
        {

            try
            {
                //HRM_QuyTrinhTinhLuong data = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_QuyTrinhTinhLuong)) as HRM_QuyTrinhTinhLuong;
                string sFile = "BangChamCongNhanVien.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "BangChamCongNhanVien.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic ExportBangLuongThangNew(ApplicationDbContext db, HRM_QuyTrinhTinhLuong QuyTrinh)
        {

            try
            {
                string sFile = "BangLuongThang.xlsx";
                if (Host == "PHT")
                    sFile = "BangLuongThangPHT.xlsx";
                int nTCol = QuyTrinh.listHeader.FirstOrDefault().listheader.Sum(a => a.colspan ?? 1) + 1;

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "BangLuongThang.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                sheet1.Cells[1, 3, 1, nTCol].Merge = true;
                if (!string.IsNullOrEmpty(QuyTrinh.TenBangLuong))
                    sheet1.Cells[1, 3].Value = QuyTrinh.TenBangLuong.ToUpper();
                int nRow = 3;
                int nCol = 1;
                #region Header
                sheet1.Cells[nRow, nCol].Value = "STT";
                int Cap = QuyTrinh.listHeader.OrderByDescending(a => a.Cap).FirstOrDefault().Cap;
                sheet1.Cells[nRow, nCol, nRow + Cap - 1, nCol].Merge = true;
                nCol++;
                int Col = 2;
                foreach (var header in QuyTrinh.listHeader)
                {
                    Col = 2;
                    foreach (var item in header.listheader)
                    {
                        int colspan = item.colspan ?? 1;
                        int Rowspan = item.rowspan ?? 1;
                        //try
                        //{
                        if (colspan > 1 || Rowspan > 1)
                        {
                            bool merge = sheet1.Cells[nRow, Col].Merge;
                            if (merge == true)
                            {
                                Col++;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                Col = ExportHeader_ChiTietHeader(sheet1, nRow, Col, Rowspan, colspan, item.name, item.DoRong);

                            }
                            else
                            {
                                sheet1.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].Merge = true;
                                sheet1.Cells[nRow, Col].Value = item.name;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                //sheet1.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].AutoFitColumns();
                                Col += colspan;
                            }
                        }
                        else
                        {
                            bool merge = sheet1.Cells[nRow, Col].Merge;
                            if (merge == true)
                            {
                                Col++;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                Col = ExportHeader_ChiTietHeader(sheet1, nRow, Col, Rowspan, colspan, item.name, item.DoRong);
                            }
                            else
                            {
                                sheet1.Cells[nRow, Col].Value = item.name;
                                sheet1.Column(Col).Width = item.DoRong / 8;
                                //sheet1.Cells[nRow, Col].AutoFitColumns();
                                Col += colspan;
                            }
                        }

                    }
                    nRow++;


                }


                sheet1.Cells[3, 1, nRow + 1, nTCol].Style.Font.Bold = true;
                sheet1.Cells[nRow, nCol, nRow + 1, nTCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                //nRow++;
                int RowBegin = nRow;
                #endregion
                #region Body
                if (Host == "ICT")
                {
                    int STT = 1;
                    if (QuyTrinh.listBoPhan != null && QuyTrinh.listBoPhan.Count > 0)
                    {
                        int STTBp = 1;
                        foreach (var Bp in QuyTrinh.listBoPhan)
                        {

                            nCol = 2;
                            if (Bp.listGiaTri != null && Bp.listGiaTri.Count > 0)
                            {
                                foreach (var GiaTri in Bp.listGiaTri)
                                {

                                    if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                    {
                                        sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                        if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                        {
                                            sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;

                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                        }
                                        else if (GiaTri.Ma == "QueQuan")
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                        else
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                    }
                                    else if (GiaTri.GiaTri != null)
                                    {
                                        sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                        if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                        else
                                        {
                                            if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                        }


                                    }
                                    nCol++;
                                }
                                nRow++;
                            }
                            nCol = 1;
                            if (Bp.listNhanVien != null && Bp.listNhanVien.Count > 0)
                            {

                                foreach (var item in Bp.listNhanVien)
                                {
                                    //if (string.IsNullOrEmpty(item.IdNhanVien))
                                    //    continue;
                                    HRM_QuyTrinhTinhLuong_NhanVien nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                    if (QuyTrinh.listNhanVien != null)
                                        nhanVien = QuyTrinh.listNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                                    if (nhanVien == null)
                                        nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                    double tyle = 0;
                                    if (nhanVien.LuongLe > 0)
                                    {
                                        double Cong1Ngay = nhanVien.LuongCoBan / nhanVien.NgayCongTieuChuan;
                                        tyle = nhanVien.LuongLe / (nhanVien.Le_Tet * (Cong1Ngay));
                                    }
                                    var listPhuCap = new List<string>();
                                    var listPhuCapTC = new List<string>();
                                    var listCong = new List<string>();
                                    var listTru = new List<string>();
                                    string LuongCoBan = "";
                                    string LuongDongBHXH = "";
                                    string LuongBHXH = "";

                                    string ThanhToanBHCN = "";
                                    string TruTienBHCN = "";
                                    string CongDem = "";
                                    string CongLe = "";
                                    string CongDoan = "";
                                    string CongOm = "";
                                    string LuongDem = "";

                                    string CongPhep = "";
                                    string LuongPhep = "";

                                    string TongLuongCB = "";

                                    string TongKhauTru = "";
                                    string CacKhoanCongTyThanhToan = "";

                                    string GiamTruDoViPham = "";

                                    string SoTienConLai = "";
                                    string SoCongThucTe = "";
                                    List<string> lstCacKhoanCongTyThanhToan = new List<string>();

                                    nCol = 1;
                                    if (!string.IsNullOrEmpty(item.TenBoPhan))
                                        sheet1.Cells[nRow, nCol].Value = STT;
                                    else
                                        sheet1.Cells[nRow, nCol].Value = dungchung.QuyDoiLaMa(STTBp);

                                    foreach (var GiaTri in item.listGiaTri)
                                    {

                                        nCol++;
                                        if (GiaTri.Ma == "TenNhanVien")
                                        {
                                            if (string.IsNullOrEmpty(item.TenBoPhan))
                                            {
                                                sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                                sheet1.Row(nRow).Style.Font.Bold = true;
                                                sheet1.Column(nCol).Width = 25;
                                                STT = 1;
                                            }

                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                        }

                                        if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                        {
                                            sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                            if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                            {
                                                if (string.IsNullOrEmpty(item.TenBoPhan))
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                                    sheet1.Row(nRow).Style.Font.Bold = true;
                                                    STT = 1;

                                                }
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                            }
                                            else if (GiaTri.Ma == "QueQuan")
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                            else
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                        }
                                        else
                                        if (GiaTri.GiaTri != null)
                                        {
                                            sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                            {
                                                if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                    sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                            }
                                            if (!string.IsNullOrEmpty(nhanVien.TenNhanVien) && Host == "ICT")
                                            {
                                                if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                else
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                                if (GiaTri.Ma == "LuongCoBan")
                                                    LuongCoBan = sheet1.Cells[nRow, nCol].Address;

                                                else if (GiaTri.Ma == "LeTet")
                                                    CongLe = sheet1.Cells[nRow, nCol].Address;
                                                else if (GiaTri.Ma == "LuongNgoaiGios")
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);

                                                else if (GiaTri.Ma == "LuongLe")
                                                {
                                                    sheet1.Cells[nRow, nCol].Formula = "=(" + LuongCoBan + "/" + nhanVien.NgayCongTieuChuan + ")*" + CongLe + "*" + tyle;
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else
                                                if (GiaTri.Ma == "TruyHoan")
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "Thuong")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);

                                                }
                                                else if (GiaTri.Ma == "TienBienChe")
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "LuongNghiKeHoach")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "HoTroPhongSach")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "LiXiTrucTet")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "TA_ThanhTien")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "HoTroCapPhatBHLD")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "CongOm")
                                                {
                                                    CongOm = sheet1.Cells[nRow, nCol].Address;
                                                }
                                                else if (GiaTri.Ma == "ThanhToanBHCN")
                                                {
                                                    ThanhToanBHCN = sheet1.Cells[nRow, nCol].Address;
                                                }

                                                else if (GiaTri.Ma == "SoCongThucTe")
                                                {
                                                    SoCongThucTe = sheet1.Cells[nRow, nCol].Address;
                                                }

                                                else if (GiaTri.Ma == "DanhGia")
                                                {
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }

                                                else if (GiaTri.Ma == "TienTruyHoan")
                                                    listCong.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "CongDoan")
                                                {

                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                    CongDoan = sheet1.Cells[nRow, nCol].Address;
                                                }

                                                else
                                               if (GiaTri.Ma == "TruyThu")
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "TK_TruAnCa")
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "GiamDanhGia")
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "TruTienBHCN")
                                                {
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                    TruTienBHCN = sheet1.Cells[nRow, nCol].Address;
                                                }
                                                else if (GiaTri.Ma == "DongPhuc")
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "ThuCoc")
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);

                                                else if (GiaTri.Ma == "TongKhauTru")
                                                {
                                                    TongKhauTru = sheet1.Cells[nRow, nCol].Address;
                                                    sheet1.Cells[nRow, nCol].Formula = "=" + listTru.Join("+");
                                                }
                                                else
                                                 if (GiaTri.Ma == "ThucNhan")
                                                {
                                                    if (string.IsNullOrEmpty(SoTienConLai))
                                                        sheet1.Cells[nRow, nCol].Formula = "=" + CacKhoanCongTyThanhToan + "-" + TongKhauTru;
                                                    else
                                                        sheet1.Cells[nRow, nCol].Formula = "=" + SoTienConLai + "-" + CongDoan;

                                                }
                                                else
                                                 if (GiaTri.Ma == "SoTienConLai")
                                                {
                                                    SoTienConLai = sheet1.Cells[nRow, nCol].Address;
                                                    sheet1.Cells[nRow, nCol].Formula = "=" + CacKhoanCongTyThanhToan + "-" + TruTienBHCN + "+" + ThanhToanBHCN + "-" + LuongBHXH;


                                                }

                                                else
                                                 if (GiaTri.Ma == "TongLuongCB")
                                                {
                                                    TongLuongCB = sheet1.Cells[nRow, nCol].Address;
                                                    if (listPhuCap.Count > 0)
                                                        sheet1.Cells[nRow, nCol].Formula = "=" + LuongCoBan + "+" + listPhuCap.Join("+");
                                                    else
                                                        sheet1.Cells[nRow, nCol].Formula = "=" + LuongCoBan;
                                                }


                                                else

                                                if (GiaTri.Ma == "LuongDongBHXH")
                                                    LuongDongBHXH = sheet1.Cells[nRow, nCol].Address;
                                                else
                                                if (GiaTri.Ma == "LuongBHXH")
                                                {
                                                    sheet1.Cells[nRow, nCol].Formula = "=(10.5*" + LuongDongBHXH + ")/100";
                                                    listTru.Add(sheet1.Cells[nRow, nCol].Address);
                                                    LuongBHXH = sheet1.Cells[nRow, nCol].Address;
                                                }
                                                else
                                                if (GiaTri.Ma == "TongLuong")
                                                {
                                                    if (nhanVien.LoaiLuong == 1)
                                                    {
                                                        if (listPhuCap.Count > 0)
                                                            sheet1.Cells[nRow, nCol].Formula = "=(" + LuongCoBan + "/" + (nhanVien.NgayCongTieuChuan) + ")*" + SoCongThucTe + "+" + string.Join("+", listPhuCapTC);
                                                        else
                                                            sheet1.Cells[nRow, nCol].Formula = "=(" + LuongCoBan + "/" + (nhanVien.NgayCongTieuChuan) + ")*" + SoCongThucTe;
                                                    }
                                                    else
                                                    {
                                                        if (listPhuCap.Count > 0)
                                                            sheet1.Cells[nRow, nCol].Formula = "=" + LuongCoBan + "*" + SoCongThucTe + "+" + string.Join("+", listPhuCapTC);
                                                        else
                                                            sheet1.Cells[nRow, nCol].Formula = "=" + LuongCoBan + "*" + SoCongThucTe;

                                                    }
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);

                                                }
                                                else if (GiaTri.Ma == "Luong50")
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "Luong100")
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "TA_ThanhTien")
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                else if (GiaTri.Ma == "GiamTruDoViPham")
                                                    GiamTruDoViPham = sheet1.Cells[nRow, nCol].Address;
                                                else if (GiaTri.Ma == "HoTroNhatCuaRoi")
                                                {
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);

                                                }
                                                else if (GiaTri.Ma == "HoTroBHYT")
                                                {
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);

                                                }
                                                else if (GiaTri.Ma == "CacKhoanCongTyThanhToan")
                                                {
                                                    CacKhoanCongTyThanhToan = sheet1.Cells[nRow, nCol].Address;
                                                    //sheet1.Cells[nRow, nCol].Formula = "=" + lstCacKhoanCongTyThanhToan.Join("+");
                                                    string text = "=" + lstCacKhoanCongTyThanhToan.Join("+");
                                                    if (!string.IsNullOrEmpty(GiamTruDoViPham))
                                                        text = text + "-" + GiamTruDoViPham;
                                                    sheet1.Cells[nRow, nCol].Formula = text;
                                                }


                                                else
                                                    if (GiaTri.Ma == "CongPhep")
                                                    CongPhep = sheet1.Cells[nRow, nCol].Address;
                                                else if (GiaTri.Ma == "CongDem")
                                                    CongDem = sheet1.Cells[nRow, nCol].Address;
                                                else
                                                    if (GiaTri.Ma == "CongGioTieuChuan")
                                                {
                                                    var Cong = nhanVien.listGioTieuChuan.FirstOrDefault(a => Math.Round(a.GioTieuChuan, 0) == Math.Round(GiaTri.heso, 0));
                                                    if (Cong != null)
                                                        Cong.DiaChi = sheet1.Cells[nRow, nCol].Address;
                                                }
                                                else if (GiaTri.Ma == "CongTangCa")
                                                {
                                                    var CongTC = nhanVien.listTangCa.FirstOrDefault(a => Math.Round(a.HeSo, 0) == Math.Round(GiaTri.heso, 0));
                                                    if (CongTC != null)
                                                        CongTC.DiaChi = sheet1.Cells[nRow, nCol].Address;
                                                }



                                                else
                                                if (GiaTri.Ma == "LuongPhep")
                                                {
                                                    sheet1.Cells[nRow, nCol].Formula = "=(" + LuongCoBan + "/" + nhanVien.NgayCongTieuChuan + ")*" + CongPhep;
                                                    LuongPhep = sheet1.Cells[nRow, nCol].Address;
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else if (GiaTri.Ma == "LuongDem")
                                                {
                                                    //sheet1.Cells[nRow, nCol].Formula = "=(" + CongDem + "*" + PhuCapDem + ")";
                                                    LuongDem = sheet1.Cells[nRow, nCol].Address;
                                                    lstCacKhoanCongTyThanhToan.Add(sheet1.Cells[nRow, nCol].Address);
                                                }
                                                else
                                                    if (GiaTri.Ma == "LuongGioTieuChuan")
                                                {
                                                    var Cong = nhanVien.listGioTieuChuan.FirstOrDefault(a => a.GioTieuChuan == GiaTri.heso);
                                                    if (Cong != null)
                                                    {
                                                        sheet1.Cells[nRow, nCol].Formula = "=(" + LuongCoBan + "/" + Cong.CongTieuChuan + ")*" + Cong.DiaChi;
                                                        Cong.DiaChiLuong = sheet1.Cells[nRow, nCol].Address;
                                                    }
                                                }
                                                else if (GiaTri.Ma == "LuongTangCa")
                                                {
                                                    var CongTC = nhanVien.listTangCa.FirstOrDefault(a => a.HeSo == GiaTri.heso);
                                                    if (CongTC != null)
                                                    {
                                                        string text = "";
                                                        double CTC = (CongTC.CongTieuChuan == 0 ? 26 : CongTC.CongTieuChuan);
                                                        if (nhanVien.LoaiLuong == 1)
                                                            text = "(((" + LuongCoBan + ") / (" + CTC + ") * " + CongTC.DiaChi + ") * (" + CongTC.HeSo + "/ 100)";
                                                        else
                                                            text = "((" + LuongCoBan + "*" + CongTC.Cong + ") * (" + CongTC.HeSo + "/ 100))";
                                                        sheet1.Cells[nRow, nCol].Formula = "=" + text;
                                                        CongTC.DiaChiLuong = sheet1.Cells[nRow, nCol].Address;
                                                    }
                                                }



                                                else if (GiaTri.Ma == "HoTro")
                                                {
                                                    string diaChi = sheet1.Cells[nRow, nCol].Address;
                                                    double SoCong = nhanVien.listGioTieuChuan.Sum(a => a.TongSoGio / a.GioTieuChuan) + nhanVien.Le_TetKDL;
                                                    if (GiaTri.LoaiPhuCap == 2)
                                                    {
                                                        listPhuCapTC.Add("(" + diaChi + "/" + (nhanVien.NgayCongTieuChuan) + ")*" + SoCong);
                                                    }
                                                    else
                                                    {
                                                        listPhuCapTC.Add(diaChi);
                                                    }
                                                    listPhuCap.Add(diaChi);
                                                }
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(item.TenBoPhan))
                                        STT++;


                                    nRow++;
                                }
                                STTBp++;
                            }
                        }

                    }
                }
                else
                {
                    int STT = 1;
                    if (QuyTrinh.listBoPhan != null && QuyTrinh.listBoPhan.Count > 0)
                    {
                        int STTBp = 1;
                        foreach (var Bp in QuyTrinh.listBoPhan)
                        {

                            nCol = 2;
                            if (Bp.listGiaTri != null && Bp.listGiaTri.Count > 0)
                            {
                                foreach (var GiaTri in Bp.listGiaTri)
                                {

                                    if (GiaTri.GiaTri != null)
                                    {
                                        sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                        if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                        else
                                        {
                                            if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                        }


                                    }
                                    else
                                    {

                                        if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                            sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                        if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                        {
                                            sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                            sheet1.Row(nRow).Style.Font.Bold = true;
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                        }
                                        else if (GiaTri.Ma == "QueQuan")
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                        else
                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                    }
                                    nCol++;
                                }
                                nRow++;
                            }
                            nCol = 1;
                            if (Bp.lischucDanh != null && Bp.lischucDanh.Count > 0)
                            {
                                foreach (var chucVu in Bp.lischucDanh)
                                {
                                    if (chucVu.listGiaTri != null && chucVu.listGiaTri.Count > 0)
                                    {
                                        foreach (var GiaTri in chucVu.listGiaTri)
                                        {

                                            if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                            {
                                                sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                                if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = chucVu.TenBoPhan;

                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                }
                                                else if (GiaTri.Ma == "QueQuan")
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                else
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                            }
                                            else if (GiaTri.GiaTri != null)
                                            {
                                                sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                                if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                else
                                                {
                                                    if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                        sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                }


                                            }
                                            nCol++;
                                        }
                                        nRow++;
                                    }
                                    if (chucVu.listNhanVien != null && chucVu.listNhanVien.Count > 0)
                                    {

                                        foreach (var item in chucVu.listNhanVien)
                                        {
                                            //if (string.IsNullOrEmpty(item.IdNhanVien))
                                            //    continue;
                                            HRM_QuyTrinhTinhLuong_NhanVien nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                            if (QuyTrinh.listNhanVien != null)
                                                nhanVien = QuyTrinh.listNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                                            if (nhanVien == null)
                                                nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                            double tyle = 0;
                                            if (nhanVien.LuongLe > 0)
                                            {
                                                double Cong1Ngay = nhanVien.LuongCoBan / nhanVien.NgayCongTieuChuan;
                                                tyle = nhanVien.LuongLe / (nhanVien.Le_Tet * (Cong1Ngay));
                                            }

                                            nCol = 1;
                                            if (!string.IsNullOrEmpty(item.TenBoPhan))
                                                sheet1.Cells[nRow, nCol].Value = STT;
                                            else
                                                sheet1.Cells[nRow, nCol].Value = dungchung.QuyDoiLaMa(STTBp);

                                            foreach (var GiaTri in item.listGiaTri)
                                            {

                                                nCol++;
                                                if (GiaTri.Ma == "TenNhanVien")
                                                {
                                                    if (string.IsNullOrEmpty(item.TenBoPhan))
                                                    {
                                                        sheet1.Cells[nRow, nCol].Value = chucVu.TenBoPhan;
                                                        sheet1.Row(nRow).Style.Font.Bold = true;
                                                        sheet1.Column(nCol).Width = 25;
                                                        STT = 1;
                                                    }

                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                }

                                                if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                                    if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                                    {
                                                        if (string.IsNullOrEmpty(item.TenBoPhan))
                                                        {
                                                            sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                                            sheet1.Row(nRow).Style.Font.Bold = true;
                                                            STT = 1;

                                                        }
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                    }
                                                    else if (GiaTri.Ma == "QueQuan")
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                    else
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                                }
                                                else
                                                if (GiaTri.GiaTri != null)
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                                    {
                                                        if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                            sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                    }

                                                }
                                            }
                                            if (!string.IsNullOrEmpty(item.TenBoPhan))
                                                STT++;


                                            nRow++;
                                        }
                                        STTBp++;
                                    }
                                }
                            }
                            nCol = 1;

                        }

                    }
                }
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nTCol);

                nRow += 2;
                sheet1.Cells[nRow, 1, nRow, 4].Merge = true;
                sheet1.Cells[nRow, 1].Value = "PHÃŠ DUYá»†T";
                sheet1.Cells[nRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 1].Style.Font.Bold = true;
                sheet1.Cells[nRow, 8, nRow, 11].Merge = true;
                sheet1.Cells[nRow, 8].Value = "Káº¾ TOÃN TRÆ¯á»žNG";
                sheet1.Cells[nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 8].Style.Font.Bold = true;
                sheet1.Cells[nRow - 1, 16, nRow - 1, 20].Merge = true;
                DateTime time = DateTime.Today;
                sheet1.Cells[nRow - 1, 16].Value = "NgÃ y " + time.Day + " thÃ¡ng " + time.Month + " nÄƒm " + time.Year;
                sheet1.Cells[nRow - 1, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 16, nRow, 20].Merge = true;
                sheet1.Cells[nRow, 16].Value = "PHÃ’NG NHÃ‚N Sá»°";
                sheet1.Cells[nRow, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 16].Style.Font.Bold = true;



                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic ExportBangLuongThangNewTongHop(ApplicationDbContext db, HRM_QuyTrinhTinhLuong QuyTrinh)
        {

            try
            {
                string sFile = "BangLuongThangPHT.xlsx";
                int nTCol = QuyTrinh.listHeader.FirstOrDefault().listheader.Sum(a => a.colspan ?? 1) + 1;

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "BangLuongThang.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                sheet1.Cells[1, 3, 1, nTCol].Merge = true;
                if (!string.IsNullOrEmpty(QuyTrinh.TenBangLuong))
                    sheet1.Cells[1, 3].Value = QuyTrinh.TenBangLuong.ToUpper();
                int nRow = 3;
                int nCol = 1;
                #region Header
                sheet1.Cells[nRow, nCol].Value = "STT";
                int Cap = QuyTrinh.listHeader.OrderByDescending(a => a.Cap).FirstOrDefault().Cap;
                sheet1.Cells[nRow, nCol, nRow + Cap - 1, nCol].Merge = true;
                nCol++;
                int Col = 2;
                foreach (var header in QuyTrinh.listHeader)
                {
                    Col = 2;
                    foreach (var item in header.listheader)
                    {
                        int colspan = item.colspan ?? 1;
                        int Rowspan = item.rowspan ?? 1;
                        //try
                        //{
                        if (colspan > 1 || Rowspan > 1)
                        {
                            bool merge = sheet1.Cells[nRow, Col].Merge;
                            if (merge == true)
                            {
                                Col++;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                Col = ExportHeader_ChiTietHeader(sheet1, nRow, Col, Rowspan, colspan, item.name, item.DoRong);

                            }
                            else
                            {
                                sheet1.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].Merge = true;
                                sheet1.Cells[nRow, Col].Value = item.name;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                //sheet1.Cells[nRow, Col, nRow + Rowspan - 1, Col + colspan - 1].AutoFitColumns();
                                Col += colspan;
                            }
                        }
                        else
                        {
                            bool merge = sheet1.Cells[nRow, Col].Merge;
                            if (merge == true)
                            {
                                Col++;
                                if (colspan == 1)
                                    sheet1.Column(Col).Width = item.DoRong / 8;
                                Col = ExportHeader_ChiTietHeader(sheet1, nRow, Col, Rowspan, colspan, item.name, item.DoRong);
                            }
                            else
                            {
                                sheet1.Cells[nRow, Col].Value = item.name;
                                sheet1.Column(Col).Width = item.DoRong / 8;
                                //sheet1.Cells[nRow, Col].AutoFitColumns();
                                Col += colspan;
                            }
                        }

                    }
                    nRow++;


                }


                sheet1.Cells[3, 1, nRow + 1, nTCol].Style.Font.Bold = true;
                sheet1.Cells[nRow, nCol, nRow + 1, nTCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                //nRow++;
                int RowBegin = nRow;
                #endregion
                #region Body

                {
                    var lisCoSo = QuyTrinh.listGiaTriNhanVien.Select(a => a.TenCongTy).Distinct().ToList();

                    int STT = 1;
                    if (QuyTrinh.listBoPhan != null && QuyTrinh.listBoPhan.Count > 0)
                    {


                        foreach (var congTy in lisCoSo)
                        {
                            {

                                var listNhanViendl = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenCongTy == congTy).ToList();
                                {
                                    var listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                    foreach (var header in QuyTrinh.listHeader_Data)
                                    {
                                        var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                        var listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "HoTro")
                                            listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                        if (header.Ma == "CongGioTieuChuan")
                                            listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                        if (listCount.Count > 0)
                                            itemAdd.GiaTri = listCount.Sum(a => a);
                                        listGiaTri.Add(itemAdd);
                                    }
                                    nCol = 2;
                                    if (listGiaTri != null && listGiaTri.Count > 0)
                                    {
                                        foreach (var GiaTri in listGiaTri)
                                        {

                                            if (GiaTri.GiaTri != null)
                                            {
                                                sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                                sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                                if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                else
                                                {
                                                    if (GiaTri.GiaTri > 1000)
                                                        sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                }


                                            }
                                            else
                                            {

                                                if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                                    sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                                if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = congTy;
                                                    sheet1.Row(nRow).Style.Font.Bold = true;
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                }
                                                else if (GiaTri.Ma == "QueQuan")
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                else
                                                    sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                            }
                                            nCol++;
                                        }
                                        nRow++;
                                    }



                                }

                            }
                            int STTBp = 1;
                            var listBoPhan = QuyTrinh.listBoPhan.Where(a => a.TenCongTy == congTy).ToList();
                            foreach (var Bp in listBoPhan)
                            {

                                nCol = 2;
                                if (Bp.listGiaTri != null && Bp.listGiaTri.Count > 0)
                                {
                                    foreach (var GiaTri in Bp.listGiaTri)
                                    {

                                        if (GiaTri.GiaTri != null)
                                        {
                                            sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                            if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                            else
                                            {
                                                if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                    sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                            }


                                        }
                                        else
                                        {

                                            if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                                sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                            if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                            {
                                                sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                                sheet1.Row(nRow).Style.Font.Bold = true;
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                            }
                                            else if (GiaTri.Ma == "QueQuan")
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                            else
                                                sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                        }
                                        nCol++;
                                    }
                                    nRow++;
                                }
                                nCol = 1;
                                if (Bp.lischucDanh != null && Bp.lischucDanh.Count > 0)
                                {
                                    foreach (var chucVu in Bp.lischucDanh)
                                    {
                                        if (chucVu.listGiaTri != null && chucVu.listGiaTri.Count > 0)
                                        {
                                            foreach (var GiaTri in chucVu.listGiaTri)
                                            {

                                                if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                                    if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                                    {
                                                        sheet1.Cells[nRow, nCol].Value = chucVu.TenBoPhan;

                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                    }
                                                    else if (GiaTri.Ma == "QueQuan")
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                    else
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                                }
                                                else if (GiaTri.GiaTri != null)
                                                {
                                                    sheet1.Cells[nRow, nCol].Value = GiaTri.GiaTri;
                                                    sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                                                    if (GiaTri.Ma.Contains("Cong") || (GiaTri.Ma.Length > 4 && GiaTri.Ma.Substring(0, 4) == "Cong"))
                                                    {
                                                        if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                            sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                    }
                                                    else
                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;


                                                }
                                                nCol++;
                                            }
                                            nRow++;
                                        }
                                        if (chucVu.listNhanVien != null && chucVu.listNhanVien.Count > 0)
                                        {

                                            foreach (var item in chucVu.listNhanVien)
                                            {
                                                //if (string.IsNullOrEmpty(item.IdNhanVien))
                                                //    continue;
                                                HRM_QuyTrinhTinhLuong_NhanVien nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                                if (QuyTrinh.listNhanVien != null)
                                                    nhanVien = QuyTrinh.listNhanVien.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                                                if (nhanVien == null)
                                                    nhanVien = new HRM_QuyTrinhTinhLuong_NhanVien();
                                                double tyle = 0;
                                                if (nhanVien.LuongLe > 0)
                                                {
                                                    double Cong1Ngay = nhanVien.LuongCoBan / nhanVien.NgayCongTieuChuan;
                                                    tyle = nhanVien.LuongLe / (nhanVien.Le_Tet * (Cong1Ngay));
                                                }

                                                nCol = 1;
                                                if (!string.IsNullOrEmpty(item.TenBoPhan))
                                                    sheet1.Cells[nRow, nCol].Value = STT;
                                                else
                                                    sheet1.Cells[nRow, nCol].Value = dungchung.QuyDoiLaMa(STTBp);

                                                foreach (var GiaTri in item.listGiaTri)
                                                {

                                                    nCol++;
                                                    if (GiaTri.Ma == "TenNhanVien")
                                                    {
                                                        if (string.IsNullOrEmpty(item.TenBoPhan))
                                                        {
                                                            sheet1.Cells[nRow, nCol].Value = chucVu.TenBoPhan;
                                                            sheet1.Row(nRow).Style.Font.Bold = true;
                                                            sheet1.Column(nCol).Width = 25;
                                                            STT = 1;
                                                        }

                                                        sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                    }

                                                    if (!string.IsNullOrEmpty(GiaTri.NoiDung))
                                                    {
                                                        sheet1.Cells[nRow, nCol].Value = GiaTri.NoiDung;
                                                        if (GiaTri.Ma == "HoVaTen" || GiaTri.Ma == "TenNhanVien")
                                                        {
                                                            if (string.IsNullOrEmpty(item.TenBoPhan))
                                                            {
                                                                sheet1.Cells[nRow, nCol].Value = Bp.TenBoPhan;
                                                                sheet1.Row(nRow).Style.Font.Bold = true;
                                                                STT = 1;

                                                            }
                                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                        }
                                                        else if (GiaTri.Ma == "QueQuan")
                                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                                        else
                                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                                                    }
                                                    else
                                                    if (GiaTri.GiaTri != null)
                                                    {
                                                        {
                                                            if (Host == "PHT" && GiaTri.GiaTri > 1000)
                                                                sheet1.Cells[nRow, nCol].Value = Math.Round((GiaTri.GiaTri ?? 0) / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
                                                            sheet1.Cells[nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                                        }
                                                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;

                                                    }
                                                }
                                                if (!string.IsNullOrEmpty(item.TenBoPhan))
                                                    STT++;


                                                nRow++;
                                            }
                                            STTBp++;
                                        }
                                    }
                                }
                                nCol = 1;

                            }
                        }
                    }
                }
                #endregion
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nTCol);

                nRow += 2;
                sheet1.Cells[nRow, 1, nRow, 4].Merge = true;
                sheet1.Cells[nRow, 1].Value = "PHÃŠ DUYá»†T";
                sheet1.Cells[nRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 1].Style.Font.Bold = true;
                sheet1.Cells[nRow, 8, nRow, 11].Merge = true;
                sheet1.Cells[nRow, 8].Value = "Káº¾ TOÃN TRÆ¯á»žNG";
                sheet1.Cells[nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 8].Style.Font.Bold = true;
                sheet1.Cells[nRow - 1, 16, nRow - 1, 20].Merge = true;
                DateTime time = DateTime.Today;
                sheet1.Cells[nRow - 1, 16].Value = "NgÃ y " + time.Day + " thÃ¡ng " + time.Month + " nÄƒm " + time.Year;
                sheet1.Cells[nRow - 1, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 16, nRow, 20].Merge = true;
                sheet1.Cells[nRow, 16].Value = "PHÃ’NG NHÃ‚N Sá»°";
                sheet1.Cells[nRow, 16].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[nRow, 16].Style.Font.Bold = true;



                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }

        public dynamic ExportBangLuongThangTongHop(ApplicationDbContext db, TimKiemBangLuong timKiem)
        {

            try
            {
                HRM_QuyTrinhTinhLuong QuyTrinh = new HRM_QuyTrinhTinhLuong();
                DateTime TuNgay = UnixTimeStampToDateTime(timKiem.TuNgay);
                DateTime DenNgay = UnixTimeStampToDateTime(timKiem.DenNgay);
                var listidChamChamCong = db.HRM_BangChamCong.Where(a => a.isKetThuc == true && a.TuNgay <= DenNgay && a.DenNgay >= TuNgay).Select(a => a.IdGuid).ToList();
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                var VPCT = listDuAn.FirstOrDefault(a => a.MaBoPhan == "VPCT");
                var listTangCa = new List<HRM_QuyTrinhTinhLuong_TangCa>();
                var listGioTieuChuan = new List<HRM_QuyTrinhTinhLuong_SoGioLamViec>();
                QuyTrinh = GetListHeaderNew(QuyTrinh, listTangCa, listGioTieuChuan, VPCT.Id, db, QuyTrinh.listIdBoPhan);
                if (listidChamChamCong != null && listidChamChamCong.Count > 0)
                {
                    var listIdBangLuong = db.HRM_QuyTrinhTinhLuong.Where(a => a.isKetThuc == true && listidChamChamCong.Any(b => a.IdKyChamCong.Contains(b))).Select(a => a.IdGuid).ToList();
                    if (listIdBangLuong != null && listIdBangLuong.Count > 0)
                    {
                        QuyTrinh.listNhanVien = (from a in db.HRM_QuyTrinhTinhLuong_NhanVien
                                                 where listIdBangLuong.Contains(a.IdQuyTrinh)
                                                 group a by new { a.IdCongTyEOS, a.IdNhanVien } into g
                                                 select g.OrderByDescending(x => x.Modified).FirstOrDefault()).ToList();
                        var listIdNhanVien = QuyTrinh.listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                        var listId = QuyTrinh.listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                        var listPhuCap = db.HRM_QuyTrinhTinhLuong_PhuCap.Where(a => listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                        var listdmPhucap = db.HRM_dmPhuCap.ToList();
                        foreach (var item in listPhuCap)
                            item.Ma = listdmPhucap.Where(a => a.IdGuid == item.IddmPhuCap).Select(a => a.Ma).FirstOrDefault();
                        if (QuyTrinh.listNhanVien != null)
                        {
                            var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).OrderByDescending(a => a.NgayHieuLuc).ToList();
                            List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(x => listIdNhanVien.Contains(x.IdGuid)).ToList();
                            var ListQT_BP = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => listIdNhanVien.Contains(x.IdQuyTrinh)).ToList();
                            var listboPhan = _CallWebService.GetDanhSachBoPhan(db);

                            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                            var ListNganHang = db.HRM_dmNganHang.ToList();
                            var ListLoaiHD = db.HRM_dmLoaiHopDong.ToList();
                            foreach (var item in QuyTrinh.listNhanVien)
                            {
                                var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                                if (HopDong != null)
                                {
                                    item.TenLoaiHopDong = ListLoaiHD.Where(a => a.IdGuid == HopDong.IdLoaiHopDong).Select(a => a.Ten).FirstOrDefault();
                                }
                                item.listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>();
                                item.listPhuCap = listPhuCap.Where(a => a.IdNhanVien == item.IdNhanVien && a.IdQuyTrinh == item.IdQuyTrinh).OrderBy(a => a.Created).ToList();
                                var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                                if (NhanVien != null)
                                {

                                    item.SoTaiKhoan = NhanVien.TaiKhoanNganHang;
                                    if (!string.IsNullOrEmpty(NhanVien.TenNganHang))
                                    {
                                        var NganHang = ListNganHang.FirstOrDefault(a => a.IdGuid == NhanVien.TenNganHang);
                                        if (NganHang != null)
                                        {
                                            item.NganHang = NganHang.Ten;
                                            if (!string.IsNullOrEmpty(NganHang.IdNganHangNguon))
                                                item.NganHangNguon = ListNganHang.Where(a => a.IdGuid == NganHang.IdNganHangNguon).Select(a => a.Ten).FirstOrDefault();
                                        }


                                    }
                                    item.MaSoThue = NhanVien.MaSoThueTNCN;
                                    item.MaNhanVien = NhanVien.MaNhanVien;
                                    item.TenNhanVien = NhanVien.TenDayDu;
                                    if (NhanVien.NgayNghiViec != null && NhanVien.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                        item.NgayNghiViec = NhanVien.NgayNghiViec.Value.ToString("dd/MM/yyyy");
                                    if (NhanVien.NgayBatDauLamViec != null && NhanVien.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                                        item.NgayVaoLam = NhanVien.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");

                                    var boPhan = listboPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                                    if (boPhan != null)
                                    {
                                        item.TenBoPhan = boPhan.TenBoPhan;
                                        item.SapXepBoPhan = boPhan.ThuTu ?? 0;
                                    }
                                    var ChucVu = listChucVu.FirstOrDefault(a => a.Id == NhanVien.IdChucVu);
                                    if (ChucVu != null)
                                        item.TenChucVu = ChucVu.TenChucVu;
                                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Id == NhanVien.IdChucDanh);
                                    if (ChucDanh != null)
                                        item.TenChucDanh = ChucDanh.Ten;
                                    //var listIdCoSo = ListQT_BP.Where(a => a.IdQuyTrinh == NhanVien.IdGuid).Select(a => a.IdBoPhan).ToList();
                                    var CoSo = listDuAn.Where(a => item.IdCongTyEOS == a.Id).Select(a => a.TenBoPhan).ToList();
                                    if (CoSo != null)
                                        item.MaCoSo = string.Join(",", CoSo);
                                }
                            }
                            if (QuyTrinh.listNhanVien != null && QuyTrinh.listNhanVien.Count > 0)
                            {
                                QuyTrinh.listNhanVien = QuyTrinh.listNhanVien.OrderBy(a => a.TenBoPhan).ThenBy(a => a.NgayVaoLam).ToList();
                                QuyTrinh.listGiaTriNhanVien = GetListGiatriPHT(QuyTrinh.listHeader_Data, QuyTrinh.listNhanVien, listNhanVien, listHopDong, db);
                            }
                            var listIdCongTy = QuyTrinh.listNhanVien.Select(a => a.IdCongTyEOS).Distinct().ToList();
                            QuyTrinh.listBoPhan = new List<clsBangTinhLuong_NhanVienView>();
                            {
                                var item = new clsBangTinhLuong_NhanVienView();
                                item.listGiaTri = QuyTrinh.listGiaTriNhanVien.Where(a => a.TenBoPhan == item.TenBoPhan && string.IsNullOrEmpty(a.TenBoPhan)).FirstOrDefault().listGiaTri;
                                item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();
                                item.TenBoPhan = "Tá»”NG";
                                QuyTrinh.listBoPhan.Add(item);
                            }
                            foreach (var IdCongTy in listIdCongTy)
                            {
                                List<string> listBP = QuyTrinh.listGiaTriNhanVien.Where(a => a.IdCongTy == IdCongTy).Select(a => a.TenBoPhan).Distinct().ToList();
                                string TenCongTy = QuyTrinh.listGiaTriNhanVien.Where(a => a.IdCongTy == IdCongTy).Select(a => a.TenCongTy).FirstOrDefault();

                                var listBoPhan = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a, IdCongTy = IdCongTy, TenCongTy = TenCongTy }).Distinct().ToList();

                                foreach (var item in listBoPhan)
                                {
                                    if (string.IsNullOrEmpty(item.TenBoPhan))
                                    {
                                        item.listGiaTri = QuyTrinh.listGiaTriNhanVien.Where(a => a.IdCongTy == IdCongTy && a.TenBoPhan == item.TenBoPhan).FirstOrDefault().listGiaTri;
                                        item.listNhanVien = new List<clsBangTinhLuong_NhanVien>();
                                        item.TenBoPhan = "Tá»”NG";

                                    }
                                    else
                                    {
                                        int SapXep = listboPhan.FirstOrDefault(a => a.TenBoPhan == item.TenBoPhan).ThuTu ?? 0;
                                        item.SapXep = SapXep;
                                        var listNhanViendl = QuyTrinh.listGiaTriNhanVien.Where(a => a.IdCongTy == IdCongTy && a.TenBoPhan == item.TenBoPhan).ToList();
                                        {
                                            listBP = listNhanViendl.Select(a => a.TenChucDanh).Distinct().ToList();
                                            {
                                                item.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                                foreach (var header in QuyTrinh.listHeader_Data)
                                                {
                                                    var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                                    var listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                                    if (header.Ma == "HoTro")
                                                        listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                                    if (header.Ma == "CongGioTieuChuan")
                                                        listCount = listNhanViendl.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                                    if (listCount.Count > 0)
                                                        itemAdd.GiaTri = listCount.Sum(a => a);
                                                    item.listGiaTri.Add(itemAdd);
                                                }
                                            }
                                            item.lischucDanh = listBP.Select(a => new clsBangTinhLuong_NhanVienView { TenBoPhan = a, listNhanVien = new List<clsBangTinhLuong_NhanVien>() }).Distinct().ToList();
                                            foreach (var chucDanh in item.lischucDanh)
                                            {
                                                var listNhanVienCD = listNhanViendl.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).ToList();

                                                clsBangTinhLuong_NhanVien Add = new clsBangTinhLuong_NhanVien();
                                                Add.listGiaTri = new List<clsBangTinhLuong_GiaTri>();
                                                foreach (var header in QuyTrinh.listHeader_Data)
                                                {
                                                    var itemAdd = new clsBangTinhLuong_GiaTri(header);
                                                    var listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma).Select(x => x.GiaTri ?? 0)).ToList();
                                                    if (header.Ma == "HoTro")
                                                        listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.IddmLabel == header.IddmLabel).Select(x => x.GiaTri ?? 0)).ToList();
                                                    if (header.Ma == "CongGioTieuChuan")
                                                        listCount = listNhanVienCD.SelectMany(a => a.listGiaTri.Where(b => b.GiaTri != null && b.Ma == header.Ma && b.Name == header.name).Select(x => x.GiaTri ?? 0)).ToList();

                                                    if (listCount.Count > 0)
                                                        itemAdd.GiaTri = listCount.Sum(a => a);
                                                    Add.listGiaTri.Add(itemAdd);
                                                }
                                                chucDanh.listNhanVien.Add(Add);
                                                chucDanh.listNhanVien.AddRange(listNhanViendl.Where(a => a.TenChucDanh == chucDanh.TenBoPhan).OrderBy(a => a.isDieuDong).ThenBy(a => a.NgayVaoLam).ToList());
                                            }

                                        }
                                    }
                                }
                                QuyTrinh.listBoPhan.AddRange(listBoPhan.OrderBy(a => a.SapXep).ToList());
                            }
                        }
                    }
                }
                return ExportBangLuongThangNewTongHop(db, QuyTrinh);
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }

        public int ExportHeader_ChiTietHeader(ExcelWorksheet Sheet1, int nRow, int nCol, int Rowspan, int colspan, string Name, double DoRong)
        {
            //try
            //{
            if (colspan > 1 || Rowspan > 1)
            {
                bool merge = Sheet1.Cells[nRow, nCol].Merge;
                if (merge == true)
                {
                    nCol++;
                    nCol = ExportHeader_ChiTietHeader(Sheet1, nRow, nCol, Rowspan, colspan, Name, DoRong);
                }
                else
                {
                    Sheet1.Cells[nRow, nCol, nRow + Rowspan - 1, nCol + colspan - 1].Merge = true;
                    Sheet1.Cells[nRow, nCol].Value = Name;
                    //Sheet1.Cells[nRow, nCol, nRow + Rowspan - 1, nCol + colspan - 1].AutoFitColumns();
                    if (colspan == 1)
                        Sheet1.Column(nCol).Width = DoRong / 8;
                    nCol += colspan;
                }
            }
            else
            {
                bool merge = Sheet1.Cells[nRow, nCol].Merge;
                if (merge == true)
                {
                    nCol++;
                    nCol = ExportHeader_ChiTietHeader(Sheet1, nRow, nCol, Rowspan, colspan, Name, DoRong);
                    return nCol;
                }
                else
                {
                    Sheet1.Cells[nRow, nCol].Value = Name;
                    if (colspan == 1)
                        Sheet1.Column(nCol).Width = DoRong / 8;
                    //Sheet1.Cells[nRow, nCol].AutoFitColumns();
                    nCol += colspan;
                }
            }

            return nCol;
        }
        public dynamic ExportTongHopLuongThang(ApplicationDbContext db, dynamic dyn)
        {
            try
            {
                #region láº¥y data
                TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
                var listcongty = _CallWebService.GetDanhSachCongTy(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).OrderBy(a => a.MaBoPhan).ToList();
                DateTime timeStart = new DateTime(TimKiem.Nam ?? 0, TimKiem.Thang ?? 0, 1, 0, 0, 0);
                DateTime timeEnd = timeStart.AddMonths(1).AddDays(-1);
                var bangChamCong = db.HRM_BangChamCong.Where(a => ((a.TuNgay >= timeStart && a.TuNgay <= timeEnd) || (a.TuNgay < timeStart && a.DenNgay >= timeStart && a.DenNgay <= timeEnd)) && a.isKetThuc == true).ToList();
                List<HRM_TongHopLuongThang> listNhanVien = new List<HRM_TongHopLuongThang>();
                if (bangChamCong != null)
                {
                    List<string> listIdBangChamCong = bangChamCong.Select(a => a.IdGuid).Distinct().ToList();
                    List<string> listIdBangLuong = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(a => listIdBangChamCong.Contains(a.IdBangChamCong)).Select(a => a.IdBangTinhLuong).Distinct().ToList();
                    var QuyTrinhtinhLuong = db.HRM_QuyTrinhTinhLuong.Where(a => listIdBangLuong.Contains(a.IdGuid) && a.isKetThuc == true).ToList();
                    listIdBangLuong = QuyTrinhtinhLuong.Select(a => a.IdGuid).Distinct().ToList();
                    var listData = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                    var listNganHang = db.HRM_dmNganHang.ToList();
                    if (listData != null && listData.Count > 0)
                    {
                        var listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                        listNhanVien = (from a in db.HRM_NhanVien
                                        where listIdNhanVien.Contains(a.IdGuid)
                                        select new HRM_TongHopLuongThang
                                        {
                                            IdNhanVien = a.IdGuid,
                                            MaNhanVien = a.MaNhanVien,
                                            TenNhanVien = a.TenDayDu,
                                            NgaySinh = a.NgaySinh,
                                            IdCongTy = a.IdCongTyEOS,
                                            SoTaiKhoan = a.TaiKhoanNganHang,
                                            IdNganHang = a.TenNganHang,
                                        }
                                         ).ToList();
                        if (TimKiem.IdCongTy > 0)
                            listNhanVien = listNhanVien.Where(a => a.IdCongTy == TimKiem.IdCongTy).ToList();

                        if (!string.IsNullOrEmpty(TimKiem.IddmNganHangNguon))
                        {
                            var listIdNganHang = listNganHang.Where(a => a.IdNganHangNguon == TimKiem.IddmNganHangNguon).Select(a => a.IdGuid).ToList();
                            listNhanVien = listNhanVien.Where(a => listIdNganHang.Contains(a.IdNganHang)).ToList();
                        }
                        listIdNhanVien = listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                        var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
                        var listNoiCap = db.HRM_dmNoiCap.ToList();
                        var listDataCong = db.HRM_QuyTrinhTinhLuong_SoGioLamViec.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                        var listDataCongTangCa = db.HRM_QuyTrinhTinhLuong_TangCa.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                        listIdBangLuong = listDataCong.Select(a => a.IdQuyTrinh).Distinct().ToList();
                        QuyTrinhtinhLuong = QuyTrinhtinhLuong.Where(a => listIdBangLuong.Contains(a.IdGuid)).ToList();

                        foreach (var item in listNhanVien)
                        {
                            var TKNganhang = listNganHang.FirstOrDefault(a => a.IdGuid == item.IdNganHang);
                            if (TKNganhang != null)
                            {
                                item.NganHang = TKNganhang.Ten;
                                item.MaBank = TKNganhang.MaBank;
                                item.ChiNhanh = TKNganhang.ChiNhanh;
                                item.TKNguon = listNganHang.Where(a => a.IdGuid == TKNganhang.IdNganHangNguon).Select(a => a.Ten).FirstOrDefault();
                            }
                            var giayTo = listGiayTo.FirstOrDefault(a => a.IdNhanVien == item.IdNhanVien);
                            if (giayTo != null)
                            {
                                item.CCCD = giayTo.So;
                                item.NgayCap = giayTo.NgayCap;
                                item.NoiCap = listNoiCap.Where(a => a.IdGuid == giayTo.NoiCap).Select(a => a.Ten).FirstOrDefault();
                            }
                            var listDaTaNv = listData.Where(a => a.IdNhanVien == item.IdNhanVien).ToList();
                            var listIdCongTY = listData.Select(a => a.IdCongTyEOS).Distinct().ToList();
                            var listcongtyChild = listcongty.Where(a => listIdCongTY.Contains(a.Id)).ToList();
                            item.listChiTiet = new List<HRM_TongHopLuongThang_ChiTiet>();
                            foreach (var Cty in listcongtyChild)
                            {
                                var listDaTaCty = listDaTaNv.Where(a => a.IdCongTyEOS == Cty.Id).ToList();
                                listIdBangLuong = listDaTaCty.Select(a => a.IdQuyTrinh).Distinct().ToList();
                                HRM_TongHopLuongThang_ChiTiet itemAdd = new HRM_TongHopLuongThang_ChiTiet();
                                itemAdd.TenCongTy = Cty.TenBoPhan;
                                itemAdd.Cong = listDataCong.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.TongSoGio / a.GioTieuChuan);
                                itemAdd.CongTangCa = listDataCongTangCa.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.Cong);
                                itemAdd.Luong = listDaTaCty.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.TienThucNhan + (a.TienTruyHoan ?? 0));
                                item.listChiTiet.Add(itemAdd);
                            }
                            item.TongLuong = item.listChiTiet.Sum(a => a.Luong);
                        }
                    }
                }


                #endregion

                DateTime dt = DateTime.Now;
                var dmNganHang = db.HRM_dmNganHang.FirstOrDefault(a => a.IdGuid == TimKiem.IddmNganHangNguon);
                if (dmNganHang.Ma == "VPbank")
                {
                    string sFile = "BangLuongTongHopVPBank.xlsx";

                    string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                    var filenamegui = "BangLuongTongHopVPBank.xlsx";
                    string sFileNameCopy = PathUploads + filenamegui;
                    File.Copy(sFileName, sFileNameCopy, true);
                    using Stream s = File.OpenRead(sFileName);
                    using ExcelPackage package = new ExcelPackage(s);
                    ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                    int nRow = 2;
                    int nCol = 1;
                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        foreach (var item in listNhanVien)
                        {
                            nCol = 1;
                            sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.SoTaiKhoan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.ChiNhanh;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.NganHang;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.MaBank;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = "VNÄ";
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongLuong;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            nRow++;

                        }
                        nRow--;
                        DrawTableExcel(sheet1, 1, nRow, 1, nCol);
                        sheet1.Cells.Style.Font.Name = "Times New Roman";
                        sheet1.Cells.Style.Font.Size = 12;
                        sheet1.Cells.Style.WrapText = true;
                        sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet1.Cells[2, 1, nRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet1.Cells[2, 3, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet1.Cells[2, nCol, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }
                    s.Close();
                    byte[] bytee = package.GetAsByteArray();
                    File.WriteAllBytes(sFileNameCopy, bytee);
                    dynamic dnm = new DynamicObject();
                    dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                    + "&path=" + Base64Encode(sFileNameCopy);
                    return dnm;
                }
                else if (dmNganHang.Ma == "VietTinBank")
                {
                    string sFile = "BangLuongTongHopVietTinBank.xlsx";

                    string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                    var filenamegui = "BangLuongTongHopVietTinBank.xlsx";
                    string sFileNameCopy = PathUploads + filenamegui;
                    File.Copy(sFileName, sFileNameCopy, true);
                    using Stream s = File.OpenRead(sFileName);
                    using ExcelPackage package = new ExcelPackage(s);
                    ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                    sheet1.Cells[3, 1].Value = "NgÃ y chi lÆ°Æ¡ng: " + dt.ToString("dd/MM/yyyy");
                    int nRow = 5;
                    int nCol = 1;
                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        int STT = 1;
                        foreach (var item in listNhanVien)
                        {
                            nCol = 1;
                            sheet1.Cells[nRow, nCol].Value = STT;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = "Luong T" + TimKiem.Thang + " nam " + TimKiem.Nam;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongLuong;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.SoTaiKhoan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.NganHang;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.ChiNhanh;
                            nRow++;
                            STT++;
                        }
                        nRow--;
                        DrawTableExcel(sheet1, 1, nRow, 1, nCol);
                        sheet1.Cells.Style.Font.Name = "Times New Roman";
                        sheet1.Cells.Style.Font.Size = 12;
                        sheet1.Cells.Style.WrapText = true;
                        sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet1.Cells[2, 3, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet1.Cells[2, 7, nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet1.Cells[2, 5, nRow, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }
                }
                else if (dmNganHang.Ma == "Vietcombank")
                {
                    string sFile = "BangLuongTongHopVietcombank.xlsx";

                    string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                    var filenamegui = "BangLuongTongHopVietcombank.xlsx";
                    string sFileNameCopy = PathUploads + filenamegui;
                    File.Copy(sFileName, sFileNameCopy, true);
                    using Stream s = File.OpenRead(sFileName);
                    using ExcelPackage package = new ExcelPackage(s);
                    ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                    sheet1.Cells[3, 1].Value = "NgÃ y chi lÆ°Æ¡ng: " + dt.ToString("dd/MM/yyyy");
                    int nRow = 5;
                    int nCol = 1;
                    if (listNhanVien != null && listNhanVien.Count > 0)
                    {
                        int STT = 1;
                        foreach (var item in listNhanVien)
                        {
                            nCol = 1;
                            sheet1.Cells[nRow, nCol].Value = STT;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.SoTaiKhoan;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.CCCD;
                            nCol++;
                            if (item.NgayCap != null)
                                sheet1.Cells[nRow, nCol].Value = item.NgayCap.Value.ToString("dd/MM/yyyy");
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.NoiCap;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.NganHang;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = item.TongLuong;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = "VND";
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = "chuyen khoan";
                            nRow++;
                            STT++;
                        }
                        nRow--;
                        DrawTableExcel(sheet1, 1, nRow, 1, nCol);
                        sheet1.Cells.Style.Font.Name = "Times New Roman";
                        sheet1.Cells.Style.Font.Size = 12;
                        sheet1.Cells.Style.WrapText = true;
                        sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        sheet1.Cells[2, 7, nRow, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                        sheet1.Cells[2, 9, nRow, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }

                    s.Close();
                    byte[] bytee = package.GetAsByteArray();
                    File.WriteAllBytes(sFileNameCopy, bytee);
                    dynamic dnm = new DynamicObject();
                    dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                    + "&path=" + Base64Encode(sFileNameCopy);
                    return dnm;
                }
                return "";
            }
            catch (Exception ex)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic ExportTongHopBangLuong(ApplicationDbContext db, dynamic dyn)
        {

            try
            {
                HRM_QuyTrinhTinhLuong data = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_QuyTrinhTinhLuong)) as HRM_QuyTrinhTinhLuong;
                string sFile = "Luong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "Luong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Chi tiáº¿t"];
                ExcelWorksheet sheet2 = package.Workbook.Worksheets["Quy trÃ¬nh"];
                // sheet quy trÃ¬nh
                var _callWeb = _CallWebService;
                var listBoPhan = _callWeb.GetDanhSachBoPhan(db);

                if (data != null && !string.IsNullOrEmpty(data.IdGuid))
                {
                    var KyChamCong = db.HRM_BangChamCong.Where(a => data.listIdKyChamCong.Contains(a.IdGuid)).ToList();
                    sheet2.Cells[3, 2].Value = data.TenBangLuong;
                    var CongTy = listBoPhan.Where(a => a.Id == data.IdCongTyEOS).FirstOrDefault();
                    if (CongTy != null)
                        sheet2.Cells[3, 3].Value = CongTy.TenBoPhan;
                    if (data.listIdBoPhan != null)
                    {
                        var BoPhans = listBoPhan.Where(a => data.listIdBoPhan.Contains(a.Id)).ToList();
                        if (BoPhans != null && BoPhans.Count > 0)
                            sheet2.Cells[3, 4].Value = string.Join(",", BoPhans.Select(a => a.TenBoPhan).ToList());
                    }
                    sheet2.Cells[3, 5].Value = string.Join(",", KyChamCong.Select(a => a.NoiDung).ToList());
                }
                #region header
                // header
                int nRow = 3;
                int nCol = 8;
                int STT = 1;
                var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).OrderByDescending(a => a.Created).ToList();
                if (listPhuCap != null && listPhuCap.Count > 0)
                {
                    sheet1.Cells[nRow, nCol, nRow, nCol + listPhuCap.Count - 1].Merge = true;
                    sheet1.Cells[nRow, nCol].Value = "CÃ¡c khoáº£n phá»¥ cáº¥p há»— trá»£";
                    foreach (var item in listPhuCap)
                    {
                        sheet1.Cells[nRow + 1, nCol].Value = item.Ten;
                        nCol++;
                    }
                }
                sheet1.Cells[nRow, nCol, nRow, nCol + 5].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "CÃ´ng";
                sheet1.Cells[nRow + 1, nCol].Value = "Ca F";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Ca P";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "TÄƒng ca";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Lá»…, táº¿t";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "PhÃ©p";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Nghá»‰ bÃ¹";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow, nCol + 4].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "LÆ°Æ¡ng";
                sheet1.Cells[nRow + 1, nCol].Value = "LÆ°Æ¡ng cÆ¡ báº£n";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "LÆ°Æ¡ng tÄƒng ca";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "LÆ°Æ¡ng lá»…";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "LÆ°Æ¡ng phÃ©p";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "LÆ°Æ¡ng nghá»‰ bÃ¹";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow, nCol + 1].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Phá»¥ cáº¥p";
                sheet1.Cells[nRow + 1, nCol].Value = "Theo cÃ´ng chÃ­nh";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "KhÃ´ng theo cÃ´ng chÃ­nh";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng lÆ°Æ¡ng";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Äá»“ng phá»¥c";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "VÃ© xe";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "KhÃ¡c";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow, nCol + 9].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Thuáº¿ thu nháº­p cÃ¡ nhÃ¢n";
                sheet1.Cells[nRow + 1, nCol].Value = "Loáº¡i HÄLÄ";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Cam káº¿t C02/CK- TNCN";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Sá»‘ giáº£m trá»« cho báº£n thÃ¢n";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Sá»‘ ngÆ°á»i Ä‘Äƒng kÃ½ NPT";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Tá»•ng tiá»n giáº£m trá»« gia cáº£nh";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Thu nháº­p miá»…n thuáº¿ (Ä‘iá»‡n thoáº¡i, BHXHâ€¦)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Thu nháº­p tÃ­nh thuáº¿ TNCN";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Thuáº¿ TNCN theo lÅ©y tiáº¿n";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Kháº¥u trá»« 10% táº¡i nguá»“n";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Tá»•ng giáº£m trá»« thuáº¿ TNCN";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Má»©c Ä‘Ã³ng BHXH";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow, nCol + 3].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "BHXH NgÆ°á»i SDLÄ";
                sheet1.Cells[nRow + 1, nCol].Value = "BHXH (17.5%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "BHYT (3%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "BHTN (1%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Tá»•ng ngÆ°á»i SDLÄ ( 21.5%)";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow, nCol + 3].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "BHXH NgÆ°á»i LÄ";
                sheet1.Cells[nRow + 1, nCol].Value = "BHXH (8%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "BHYT (1.5%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "BHTN (1%)";
                nCol++;
                sheet1.Cells[nRow + 1, nCol].Value = "Tá»•ng ngÆ°á»i SDLÄ ( 10.5%)";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Truy thu BHXH, táº¡m á»©ng lÆ°Æ¡ng, trá»« 10% lÆ°Æ¡ng, khÃ¡c";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tráº£ tiá»n Ä‘á»“ng phá»¥c";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Thá»±c nháº­n";

                #endregion
                dungchung.DrawTableExcel(sheet1, 3, 4, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[1, 1].Value = "Báº¢NG LÆ¯Æ NG";
                sheet1.Cells[1, 1].Style.Font.Bold = true;
                nRow = 5;
                if (data != null && !string.IsNullOrEmpty(data.IdGuid))
                {

                    foreach (var item in data.listNhanVien)
                    {

                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenChucVu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NgayCongTieuChuan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongCoBan;
                        nCol++;
                        foreach (var PC in item.listPhuCap)
                        {
                            sheet1.Cells[nRow, nCol].Value = PC.SoTienPhuCap;
                            nCol++;
                        }
                        sheet1.Cells[nRow, nCol].Value = item.NgayCong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NgayCongCaGay;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NgayTangCa;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.Le_Tet;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NghiPhep;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NghiBu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongCoBan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongTangCa;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongLe;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongPhep;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LuongNghiBu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.PhuCapCongChinh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.PhuCapKhongTheoCongChinh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongLuong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.DongPhuc;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.XeVe;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.Khac;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LoaiHopDong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.CamKeC02;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoGiamTruBanThan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SpNguoiDangKyNPT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongTienGiamTruGiaCanh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThuNhapMienThue;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThuNhapTinhThueTNCN;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThueTNCNTheoLuyTien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.KhauTruTaiNguon;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongGiamThueTNCN;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MucDongBaoHiemXaHoi;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SDLDBHXH;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SDLDBHYT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SDLDBHTN;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongNguoiSDLD;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NLDBHXH;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NLDBHYT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.NLDBHTN;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TongNguoiNLD;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TruyThu;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TraTienDongPhuc;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TienThucNhan;


                        nRow++;
                        STT++;

                    }
                }
                // tá»•ng
                if (nRow >= 4)
                {
                    sheet1.Cells[nRow, 1, nRow, 7].Merge = true;
                    sheet1.Cells[nRow, 1, nRow, 7].Style.Font.Bold = true;
                    sheet1.Cells[nRow, 1].Value = "Tá»”NG";
                    for (int i = 8; i <= nCol; i++)
                    {
                        string NameBD = sheet1.Cells[4, i].Address;
                        string NameKT = sheet1.Cells[nRow - 1, i].Address;
                        sheet1.Cells[nRow, i].Formula = "=SUM(" + NameBD + ":" + NameKT + ")";
                    }
                }
                sheet1.Cells[5, 7, nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 4)
                {
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, nCol);
                    sheet1.Cells[5, 3, nRow, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }

                // chá»¯ kÃ½
                nRow += 4;
                sheet1.Cells[nRow, 3, nRow, 6].Merge = true;
                sheet1.Cells[nRow, 3].Value = "Quáº£n lÃ½ cÆ¡ sá»Ÿ";
                sheet1.Cells[nRow + 1, 3, nRow + 1, 6].Merge = true;
                sheet1.Cells[nRow + 1, 3].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, nCol - 10, nRow - 1, nCol - 2].Merge = true;
                sheet1.Cells[nRow - 1, nCol - 8].Value = "HÃ  Ná»™i,ngÃ y " + dt.Day + " thÃ¡ng " + dt.Month + " nÄƒm " + dt.Year;
                sheet1.Cells[nRow, nCol - 8, nRow, nCol - 4].Merge = true;
                sheet1.Cells[nRow, nCol - 8].Value = "NgÆ°á»i láº­p";
                sheet1.Cells[nRow + 1, nCol - 8, nRow + 1, nCol - 4].Merge = true;
                sheet1.Cells[nRow + 1, nCol - 8].Value = "(KÃ½, ghi rÃµ há» tÃªn)";

                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[nRow - 1, 1, nRow + 1, nCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.WrapText = true;
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception)

            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetTongHopLuongThang(ApplicationDbContext db, dynamic dyn)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = TimKiem.CurrentPage ?? 0;
            int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
            int PageSize = TimKiem.PageSize ?? 25;

            var listcongty = _CallWebService.GetDanhSachCongTy(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).OrderBy(a => a.MaBoPhan).ToList();
            DateTime timeStart = new DateTime(TimKiem.Nam ?? 0, TimKiem.Thang ?? 0, 1, 0, 0, 0);
            DateTime timeEnd = timeStart.AddMonths(1).AddDays(-1);
            var listNhanVienUser = _CallWebService.GetListUserNhanSuDuAnTheoThoiGian(TimKiem.IdCongTy ?? 0, dungchung.DateTimeToUnixTimeStamp(timeStart), dungchung.DateTimeToUnixTimeStamp(timeEnd), db);
            List<string> listIdUser = listNhanVienUser.Select(a => a.IdUser).Distinct().ToList();
            var bangChamCong = db.HRM_BangChamCong.Where(a => ((a.TuNgay >= timeStart && a.TuNgay <= timeEnd) || (a.TuNgay < timeStart && a.DenNgay >= timeStart && a.DenNgay <= timeEnd)) && a.isKetThuc == true).ToList();
            //if (TimKiem.IdCongTy > 0)
            //    bangChamCong = bangChamCong.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy).ToList();

            List<HRM_TongHopLuongThang> listNhanVien = new List<HRM_TongHopLuongThang>();
            PagingInfor paging = new PagingInfor();
            if (bangChamCong != null)
            {
                List<string> listIdBangChamCong = bangChamCong.Select(a => a.IdGuid).Distinct().ToList();
                List<string> listIdBangLuong = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(a => listIdBangChamCong.Contains(a.IdBangChamCong)).Select(a => a.IdBangTinhLuong).Distinct().ToList();

                var QuyTrinhtinhLuong = db.HRM_QuyTrinhTinhLuong.Where(a => listIdBangLuong.Contains(a.IdGuid) && a.isKetThuc == true).ToList();
                listIdBangLuong = QuyTrinhtinhLuong.Select(a => a.IdGuid).Distinct().ToList();
                var listData = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                var listNganHang = db.HRM_dmNganHang.ToList();
                if (listData != null && listData.Count > 0)
                {
                    var listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    listNhanVien = (from a in db.HRM_NhanVien
                                    where listIdNhanVien.Contains(a.IdGuid) && listIdUser.Contains(a.IdUser)
                                    select new HRM_TongHopLuongThang
                                    {
                                        IdNhanVien = a.IdGuid,
                                        MaNhanVien = a.MaNhanVien,
                                        TenNhanVien = a.TenDayDu,
                                        NgaySinh = a.NgaySinh,
                                        IdCongTy = a.IdCongTyEOS,
                                        SoTaiKhoan = a.TaiKhoanNganHang,
                                        IdNganHang = a.TenNganHang,
                                    }
                                     ).ToList();


                    if (!string.IsNullOrEmpty(TimKiem.IddmNganHangNguon))
                    {
                        var listIdNganHang = listNganHang.Where(a => a.IdNganHangNguon == TimKiem.IddmNganHangNguon).Select(a => a.IdGuid).ToList();
                        listNhanVien = listNhanVien.Where(a => listIdNganHang.Contains(a.IdNganHang)).ToList();
                    }
                    int nSum = listNhanVien.Count;
                    int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                    paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                    listNhanVien = listNhanVien.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                    listIdNhanVien = listNhanVien.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listDataCong = db.HRM_QuyTrinhTinhLuong_SoGioLamViec.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                    var listDataCongTangCa = db.HRM_QuyTrinhTinhLuong_TangCa.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                    listIdBangLuong = listDataCong.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    QuyTrinhtinhLuong = QuyTrinhtinhLuong.Where(a => listIdBangLuong.Contains(a.IdGuid)).ToList();

                    foreach (var item in listNhanVien)
                    {
                        var TKNganhang = listNganHang.FirstOrDefault(a => a.IdGuid == item.IdNganHang);
                        if (TKNganhang != null)
                        {
                            item.NganHang = TKNganhang.Ten;
                            item.MaBank = TKNganhang.MaBank;
                            item.TKNguon = listNganHang.Where(a => a.IdGuid == TKNganhang.IdNganHangNguon).Select(a => a.Ten).FirstOrDefault();
                        }
                        var listDaTaNv = listData.Where(a => a.IdNhanVien == item.IdNhanVien).ToList();
                        var listIdCongTY = listData.Select(a => a.IdCongTyEOS).Distinct().ToList();
                        var listcongtyChild = listcongty.Where(a => listIdCongTY.Contains(a.Id)).ToList();
                        item.listChiTiet = new List<HRM_TongHopLuongThang_ChiTiet>();
                        foreach (var Cty in listcongtyChild)
                        {
                            var listDaTaCty = listDaTaNv.Where(a => a.IdCongTyEOS == Cty.Id).ToList();
                            listIdBangLuong = listDaTaCty.Select(a => a.IdQuyTrinh).Distinct().ToList();
                            HRM_TongHopLuongThang_ChiTiet itemAdd = new HRM_TongHopLuongThang_ChiTiet();
                            itemAdd.TenCongTy = Cty.TenBoPhan;
                            itemAdd.Cong = listDataCong.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.TongSoGio / a.GioTieuChuan);
                            itemAdd.CongTangCa = listDataCongTangCa.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.Cong);
                            itemAdd.Luong = listDaTaCty.Where(a => a.IdNhanVien == item.IdNhanVien && listIdBangLuong.Contains(a.IdQuyTrinh)).Sum(a => a.TienThucNhan + (a.TienTruyHoan ?? 0));
                            item.listChiTiet.Add(itemAdd);
                        }
                        item.TongLuong = item.listChiTiet.Sum(a => a.Luong);
                    }
                }
            }


            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = listNhanVien;
            return dnm;
        }
        public List<HRM_TongHopLuong> GetTongHopLuongCoSo(ApplicationDbContext db, int Thang, int Nam, string KeyWord)
        {
            //var listKiemNhiem = _CallWebService.GetDanhSachCongTy(db);
            //List<long> listId = listKiemNhiem.Select(a => a.Id).Distinct().ToList();
            List<HRM_TongHopLuong> listresult = new List<HRM_TongHopLuong>();
            var listcongty = _CallWebService.GetDanhSachCongTy(db)/*.Where(a => listId.Contains(a.Id))*/.ToList(); ;
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).Where(a => a.LoaiBoPhanPkid == 1).OrderBy(a => a.MaBoPhan).ToList();
            if (!string.IsNullOrEmpty(KeyWord))
            {
                listBoPhan = listBoPhan.Where(a => a.MaBoPhan.ToLower().Contains(KeyWord.Trim().ToLower()) || a.TenBoPhan.ToLower().Contains(KeyWord.Trim().ToLower())).ToList();
            }
            List<string> listMaBoPhan = listBoPhan.Select(a => a.MaBoPhan.Substring(2, 3)).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()).ToList();
            var bangChamCong = db.HRM_BangChamCong.Where(a => (a.TuNgay.Value.Year == Nam && a.TuNgay.Value.Month == Thang) || (a.DenNgay.Value.Year == Nam && a.DenNgay.Value.Month == Thang) && a.isKetThuc == true).ToList();
            if (bangChamCong != null)
            {
                List<string> listIdBangChamCong = bangChamCong.Select(a => a.IdGuid).Distinct().ToList();
                List<string> listIdBangLuong = db.HRM_QuyTrinhTinhLuong_ChamCong.Where(a => listIdBangChamCong.Contains(a.IdBangChamCong)).Select(a => a.IdBangTinhLuong).Distinct().ToList();

                var QuyTrinhtinhLuong = db.HRM_QuyTrinhTinhLuong.Where(a => listIdBangLuong.Contains(a.IdGuid) && a.isKetThuc == true).ToList();
                listIdBangLuong = QuyTrinhtinhLuong.Select(a => a.IdGuid).Distinct().ToList();
                var listData = db.HRM_QuyTrinhTinhLuong_NhanVien.Where(a => listIdBangLuong.Contains(a.IdQuyTrinh)).ToList();
                double TongLuong = listData.Sum(a => a.TienThucNhan);
                foreach (var ma in listMaBoPhan)
                {


                    HRM_TongHopLuong itemAdd = new HRM_TongHopLuong();
                    itemAdd.MaBoPhan = ma.Substring(1, 2);
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.MaBoPhan.Contains(ma));

                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;
                    // vÄƒn phÃ²ng cÃ´ng ty
                    var BoPhanvp = listBoPhan.FirstOrDefault(a => a.BoPhanChaPkid == 8 && a.MaBoPhan.Contains(ma));
                    if (BoPhanvp != null)
                    {
                        var NhanVienvp = listNhanVien.Where(a => a.IdCongTyEOS == 8 && a.IdBoPhanEOS == BoPhanvp.Id).ToList();
                        List<string> listIdNV = NhanVienvp.Select(a => a.IdGuid).Distinct().ToList();
                        var datavp = listData.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();
                        if (datavp != null && datavp.Count > 0)
                        {
                            itemAdd.SoLuongNhanSuVPCT = datavp.Select(a => a.IdNhanVien).Distinct().Count();
                            itemAdd.SoTienVPCT = datavp.Sum(a => a.TienThucNhan);

                        }
                    }
                    itemAdd.listCongty = new List<HRM_TongHopLuong_CoSo>();
                    itemAdd.TongCong = 0;
                    foreach (var item in listcongty.Where(a => a.Id != 8))
                    {
                        HRM_TongHopLuong_CoSo coso = new HRM_TongHopLuong_CoSo();
                        var BoPhancs = listBoPhan.FirstOrDefault(a => a.BoPhanChaPkid == item.Id && a.MaBoPhan.Contains(ma));
                        if (BoPhancs != null)
                        {
                            var NhanViencs = listNhanVien.Where(a => a.IdCongTyEOS == item.Id).ToList();
                            List<string> listIdNV = NhanViencs.Select(a => a.IdGuid).Distinct().ToList();
                            var datacs = listData.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();

                            double Tong = 1;
                            if (datacs != null && datacs.Count > 0)
                            {
                                Tong = datacs.Sum(a => a.TienThucNhan);

                            }
                            NhanViencs = NhanViencs.Where(a => a.IdBoPhanEOS == BoPhancs.Id).ToList();
                            listIdNV = NhanViencs.Select(a => a.IdGuid).Distinct().ToList();
                            var data = listData.Where(a => listIdNV.Contains(a.IdNhanVien)).ToList();


                            if (data != null && data.Count > 0)
                            {
                                coso.SoLuongNhanSu = data.Select(a => a.IdNhanVien).Distinct().Count();
                                coso.SoTien = data.Sum(a => a.TienThucNhan);
                                coso.Cong = data.Sum(a => a.NgayCong + a.NgayCongCaGay + a.NgayTangCa + a.Le_Tet + a.NghiPhep + a.NghiBu);
                                if (Tong > 0)
                                    coso.TyLe = coso.SoTien * 100 / Tong;
                                itemAdd.TongCong += (coso.SoTien ?? 0);
                            }
                        }
                        itemAdd.listCongty.Add(coso);
                        if (TongLuong > 0)
                            itemAdd.TyLeLuong = itemAdd.TongCong * 100 / TongLuong;
                    }
                    listresult.Add(itemAdd);
                }


            }
            return listresult;
        }
        public dynamic ExportTongHopLuongCoSo(ApplicationDbContext db, int Thang, int Nam, string KeyWord)
        {
            try
            {
                string sFile = "TongHopTheoDoiBangLuongCoSo.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);

                var filenamegui = "TongHopTheoDoiBangLuongCoSo.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                List<HRM_TongHopLuong> listData = GetTongHopLuongCoSo(db, Thang, Nam, KeyWord);
                var listcongty = _CallWebService.GetDanhSachCongTy(db);
                // header
                int nRow = 4;
                int nCol = 6;
                foreach (var item in listcongty.Where(a => a.Id != 8))
                {
                    sheet1.Cells[nRow, nCol, nRow, nCol + 4].Merge = true;
                    sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                    sheet1.Cells[nRow + 1, nCol].Value = "SL nhÃ¢n sá»±";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "CÃ´ng";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Sá»‘ tiá»n";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Tá»· lá»‡/tá»•ng";
                    nCol++;
                    sheet1.Cells[nRow + 1, nCol].Value = "Tá»· lá»‡/DT cÆ¡ sÆ¡";
                    nCol++;

                }
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»•ng cá»™ng";
                nCol++;
                sheet1.Cells[nRow, nCol, nRow + 1, nCol].Merge = true;
                sheet1.Cells[nRow, nCol].Value = "Tá»· lá»‡ lÆ°Æ¡ng BP/Tá»•ng lÆ°Æ¡ng";

                dungchung.DrawTableExcel(sheet1, 4, 5, 1, nCol);
                sheet1.Cells[1, 1, 1, nCol].Merge = true;
                sheet1.Cells[2, 1, 2, nCol].Merge = true;
                sheet1.Cells[2, 1].Value = "ThÃ¡ng " + Thang + "/" + Nam;
                sheet1.Cells[2, 1].Style.Font.Bold = true;
                if (listData.Count > 0)
                {
                    nRow = 6;
                    int STT = 1;
                    foreach (var item in listData)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = STT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenBoPhan;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoLuongNhanSuVPCT;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.SoTienVPCT;
                        nCol++;
                        foreach (var cty in item.listCongty)
                        {
                            sheet1.Cells[nRow, nCol].Value = cty.SoLuongNhanSu;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = cty.Cong;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = cty.SoTien;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = cty.TyLe;
                            sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                            nCol++;
                            sheet1.Cells[nRow, nCol].Value = cty.TyLeDoanhThuCoSo;
                            nCol++;

                        }
                        sheet1.Cells[nRow, nCol].Value = item.TongCong;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TyLeLuong;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;

                        STT++;
                        nRow++;
                    }
                    nRow--;
                    dungchung.DrawTableExcel(sheet1, 4, nRow, 1, 7 + listcongty.Where(a => a.Id != 8).Count() * 5);

                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[6, 3, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                }
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
            }
        }
        public List<clsBangTinhLuong_Header> GetListHeader(List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa, List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuan, long IdCongTy, ApplicationDbContext db, List<long> IdBoPhan = null)
        {
            List<clsBangTinhLuong_Header> listResult = new List<clsBangTinhLuong_Header>();
            var BoPhan_QT = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBangLuong_CONGTY" && a.IdBoPhan == IdCongTy).ToList();
            List<string> listIds = BoPhan_QT.Select(a => a.IdQuyTrinh).Distinct().ToList();
            var list = db.HRM_dmThietLapBangLuong.Where(a => listIds.Contains(a.IdGuid)).ToList();
            listIds = list.Select(a => a.IdGuid).Distinct().ToList();

            if (IdBoPhan != null && IdBoPhan.Count > 0)
            {
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBangLuong" && IdBoPhan.Contains(a.IdBoPhan)).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                list = list.Where(a => listIds.Contains(a.IdGuid)).ToList();

            }
            var listLabel = db.HRM_dmThietLapBangLuong_ChiTiet.Where(a => listIds.Contains(a.IdDanhMuc)).OrderBy(a => a.SapXep).ToList();
            var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            var listNoiCap = db.HRM_dmNoiCapBaoHiem.ToList();

            var listDanhMuc = db.HRM_dmLabelThietLapBangLuong.OrderBy(a => a.SapXep).ToList();
            foreach (var item in listDanhMuc)
            {
                //HRM_dmThietLapBangLuong_ChiTiet itemAdd = new HRM_dmThietLapBangLuong_ChiTiet(item);
                if (listLabel != null && listLabel.Count > 0)
                {
                    var check = listLabel.FirstOrDefault(a => a.IddmLabel == item.IdGuid);
                    if (check != null)
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header(item);
                        listResult.Add(itemAdd);
                    }
                }
                else
                {
                    clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header(item);
                    listResult.Add(itemAdd);
                }

            }
            List<string> listCheck = new List<string>();
            foreach (var item in listResult)
            {
                item.listheader = new List<clsBangTinhLuong_Header>();
                if (item.Ma == "HoTro")
                {
                    var listId = listPhuCap.Select(a => a.IdGuid).ToList();
                    int SL = listLabel.Where(a => listId.Contains(a.IddmLabel)).Count();
                    if (SL == 0)
                    {

                        listCheck.Add(item.Ma);
                        continue;
                    }
                    foreach (var PC in listPhuCap)
                    {
                        if (listLabel != null && listLabel.Count > 0)
                        {
                            var listdtPhuCap = listLabel.FirstOrDefault(a => PC.IdGuid == a.IddmLabel);
                            if (listdtPhuCap != null)
                            {
                                clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                                {
                                    Ma = PC.Ma,
                                    name = PC.Ten,
                                    IddmLabel = PC.IdGuid,
                                    rowspan = 1,
                                    colspan = 1,
                                    STT = PC.SapXep,
                                };
                                item.listheader.Add(itemAdd);
                            }


                        }
                        else
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = PC.Ma,
                                name = PC.Ten,
                                IddmLabel = PC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = PC.SapXep,
                            };
                            item.listheader.Add(itemAdd);
                        }
                    }
                    item.listheader = item.listheader.OrderBy(a => a.STT).ToList();
                }
                else if (item.Ma == "Cong")
                {
                    int STT = 1;
                    if (listGioTieuChuan != null && listGioTieuChuan.Count > 0)
                    {

                        foreach (var GTC in listGioTieuChuan)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "CongGioTieuChuan",
                                name = GTC.GioTieuChuan.ToString(),
                                IddmLabel = GTC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = STT,
                            };
                            item.listheader.Add(itemAdd);
                            STT++;
                        }
                    }
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                        {
                            Ma = "CongDem",
                            name = "ÄÃªm",
                            IddmLabel = "",
                            rowspan = 1,
                            colspan = 1,
                            STT = STT,
                        };
                        STT++;
                        item.listheader.Add(itemAdd);
                    }
                    if (listTangCa != null && listTangCa.Count > 0)
                    {

                        foreach (var GTC in listTangCa)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "CongTangCa",
                                name = GTC.HeSo.ToString(),
                                IddmLabel = GTC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = STT,
                            };
                            item.listheader.Add(itemAdd);
                            STT++;
                        }
                    }
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                        {
                            Ma = "CongPhep",
                            name = "PhÃ©p",
                            IddmLabel = "",
                            rowspan = 1,
                            colspan = 1,
                            STT = STT,
                        };
                        STT++;
                        item.listheader.Add(itemAdd);
                    }


                }
                else if (item.Ma == "Luong")
                {
                    int STT = 1;
                    if (listGioTieuChuan != null && listGioTieuChuan.Count > 0)
                    {

                        foreach (var GTC in listGioTieuChuan)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "LuongGioTieuChuan",
                                name = GTC.GioTieuChuan.ToString(),
                                IddmLabel = GTC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = STT,
                            };
                            item.listheader.Add(itemAdd);
                            STT++;
                        }
                    }
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                        {
                            Ma = "LuongDem",
                            name = "ÄÃªm",
                            IddmLabel = "",
                            rowspan = 1,
                            colspan = 1,
                            STT = STT,
                        };
                        STT++;
                        item.listheader.Add(itemAdd);
                    }
                    if (listTangCa != null && listTangCa.Count > 0)
                    {

                        foreach (var GTC in listTangCa)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "LuongTangCa",
                                name = GTC.HeSo.ToString(),
                                IddmLabel = GTC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = STT,
                            };
                            item.listheader.Add(itemAdd);
                            STT++;
                        }
                    }
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                        {
                            Ma = "LuongPhep",
                            name = "PhÃ©p",
                            IddmLabel = "",
                            rowspan = 1,
                            colspan = 1,
                            STT = STT,
                        };
                        STT++;
                        item.listheader.Add(itemAdd);
                    }


                }

                else if (item.Ma == "BaoHiemNgoai")
                {
                    int STT = 1;
                    if (listNoiCap != null && listNoiCap.Count > 0)
                    {
                        foreach (var GTC in listNoiCap)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = GTC.Ma.ToString(),
                                name = GTC.Ten.ToString(),
                                IddmLabel = GTC.IdGuid,
                                rowspan = 1,
                                colspan = 1,
                                STT = STT,
                            };
                            item.listheader.Add(itemAdd);
                            STT++;
                        }
                    }
                    else
                        listCheck.Add(item.Ma);
                }
                item.colspan = item.listheader.Count;
                if (item.colspan == 0)
                    item.colspan = 1;
            }
            if (listCheck != null && listCheck.Count > 0)
                listResult = listResult.Where(a => !listCheck.Contains(a.Ma)).OrderBy(a => a.STT).ToList();
            return listResult;
        }
        public HRM_QuyTrinhTinhLuong GetListHeaderNew(HRM_QuyTrinhTinhLuong QuyTrinh, List<HRM_QuyTrinhTinhLuong_TangCa> listTangCa, List<HRM_QuyTrinhTinhLuong_SoGioLamViec> listGioTieuChuan, long IdCongTy, ApplicationDbContext db, List<long> IdBoPhan = null)
        {
            QuyTrinh.listHeader = new List<clsBangTinhLuongHeader>();
            QuyTrinh.listHeader_Data = new List<clsBangTinhLuong_Header>();
            List<clsBangTinhLuong_Header> listResult = new List<clsBangTinhLuong_Header>();
            var BoPhan_QT = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBangLuong_CONGTY" && a.IdBoPhan == IdCongTy).ToList();
            List<string> listIds = BoPhan_QT.Select(a => a.IdQuyTrinh).Distinct().ToList();
            var list = db.HRM_dmThietLapBangLuong.Where(a => listIds.Contains(a.IdGuid)).ToList();
            listIds = list.Select(a => a.IdGuid).Distinct().ToList();
            if (IdBoPhan != null && IdBoPhan.Count > 0)
            {
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIds.Contains(a.IdQuyTrinh) && a.TableName == "HRM_dmThietLapBangLuong" && IdBoPhan.Contains(a.IdBoPhan)).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                list = list.Where(a => listIds.Contains(a.IdGuid)).ToList();

            }
            var listLabel = db.HRM_dmThietLapBangLuong_ChiTiet.Where(a => listIds.Contains(a.IdDanhMuc)).OrderBy(a => a.SapXep).ToList();
            var listPhuCap = db.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            var listNoiCap = db.HRM_dmNoiCapBaoHiem.ToList();

            var listDanhMuc = db.HRM_dmLabelThietLapBangLuong.OrderBy(a => a.SapXep).ToList();
            List<clsBangTinhLuong_Header> listAdd = new List<clsBangTinhLuong_Header>();
            //HRM_dmThietLapBangLuong_ChiTiet itemAdd = new HRM_dmThietLapBangLuong_ChiTiet(item);
            if (listLabel != null && listLabel.Count > 0)
            {

                foreach (var item in listLabel)
                {
                    var DanhMuc = listDanhMuc.FirstOrDefault(a => a.Ma == item.Ma);
                    if (DanhMuc != null)
                    {
                        item.Ten = DanhMuc.Ten;
                        item.SoThapPhan = DanhMuc.SoThapPhan;
                    }
                    else
                    {
                        var PC = listPhuCap.FirstOrDefault(a => a.IdGuid == item.IddmLabel);
                        if (PC != null)
                        {
                            item.Ten = PC.Ten;
                            item.Ma = "HoTro";

                        }
                    }
                    clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header(item);

                    listResult.Add(itemAdd);

                }
            }
            else
            {
                foreach (var item in listDanhMuc)
                {
                    clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header(item);
                    listResult.Add(itemAdd);
                }

            }

            int STT = 1;
            foreach (var item in listResult)
            {
                item.STT = STT;

                if (item.Ma == "HoTro" && (listLabel == null || listLabel.Count == 0))
                {
                    STT++;
                    foreach (var PC in listPhuCap)
                    {
                        clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                        {
                            Ma = "HoTro",
                            name = PC.Ten,
                            IddmLabel = PC.IdGuid,
                            STT = STT,
                            Id = PC.IdGuid,
                            IdParent = item.Id,
                            IdKey = item.IdKey + "," + item.IdKey + "," + PC.IdGuid,
                            DoRong = 100,
                            Cap = item.Cap + 1
                        };
                        listAdd.Add(itemAdd);
                        STT++;
                    }

                }
                else if (item.Ma == "TLCB_HT")
                {

                    if (listGioTieuChuan != null && listGioTieuChuan.Count > 0)
                    {
                        STT++;
                        foreach (var GTC in listGioTieuChuan)
                        {
                            GTC.IdGuid = Guid.NewGuid().ToString();
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "CongGioTieuChuan",
                                name = "CÃ´ng (" + GTC.GioTieuChuan.ToString() + "h)",
                                IddmLabel = GTC.IdGuid,
                                STT = STT,
                                Id = GTC.IdGuid,
                                IdParent = item.Id,
                                IdKey = item.IdKey + "," + item.IdKey + "," + GTC.IdGuid,
                                DoRong = 100,
                                SoThapPhan = 1,
                                Cap = item.Cap + 1
                            };
                            listAdd.Add(itemAdd);
                            STT++;
                        }
                    }
                }
                else if (item.Ma == "TienLuongNgoaiGio")
                {

                    if (listTangCa != null && listTangCa.Count > 0)
                    {
                        STT++;
                        foreach (var GTC in listTangCa)
                        {
                            GTC.IdGuid = Guid.NewGuid().ToString();
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "CongTangCa",
                                name = "CÃ´ng " + "(" + GTC.HeSo.ToString() + "%)",
                                IddmLabel = GTC.IdGuid,
                                Id = GTC.IdGuid,
                                IdParent = item.Id,
                                IdKey = item.IdKey + "," + item.IdKey + "," + GTC.IdGuid,
                                STT = STT,
                                DoRong = 100,
                                SoThapPhan = 1,
                                Cap = item.Cap + 1
                            };
                            listAdd.Add(itemAdd);
                            STT++;
                        }
                    }



                }
                else if (item.Ma == "BaoHiemNgoai")
                {

                    if (listNoiCap != null && listNoiCap.Count > 0)
                    {
                        STT++;
                        foreach (var GTC in listNoiCap)
                        {
                            clsBangTinhLuong_Header itemAdd = new clsBangTinhLuong_Header()
                            {
                                Ma = "BaoHiemNgoai",
                                name = GTC.Ten.ToString(),
                                IddmLabel = GTC.IdGuid,
                                Id = GTC.IdGuid,
                                IdParent = item.Id,
                                IdKey = item.IdKey + "," + item.IdKey + "," + GTC.IdGuid,
                                STT = STT,
                                DoRong = 100,
                                Cap = item.Cap + 1
                            };
                            listAdd.Add(itemAdd);
                            STT++;
                        }
                    }

                }
                STT++;
            }
            listResult.AddRange(listAdd);
            int CapHeader = listResult.OrderByDescending(a => a.Cap).Select(a => a.Cap).FirstOrDefault();
            for (int i = CapHeader; i >= 1; i--)
            {

                clsBangTinhLuongHeader Cap = new clsBangTinhLuongHeader();
                Cap.Cap = i;
                Cap.listheader = new List<clsBangTinhLuong_Header>();
                var listCapheader = listResult.Where(a => a.Cap == i).OrderBy(a => a.STT).ToList();
                foreach (var itemAdd in listCapheader)
                {
                    //var itemAdd = new clsBangTinhLuong_Header(item);
                    var child = listResult.Where(a => a.IdParent == itemAdd.Id).ToList();
                    if (child == null || child.Count == 0)
                    {
                        itemAdd.colspan = 1;
                        QuyTrinh.listHeader_Data.Add(itemAdd);
                    }
                    else
                    {
                        itemAdd.colspan = child.Sum(a => a.colspan);
                        itemAdd.DoRong = child.Sum(a => a.DoRong);
                    }


                    child = listResult.Where(a => !string.IsNullOrEmpty(a.IdKey) && a.IdKey.Contains(itemAdd.Id) && a.Id != itemAdd.Id).ToList();
                    if (child == null || child.Count == 0)
                        itemAdd.rowspan = CapHeader - i + 1;
                    else
                    {
                        itemAdd.rowspan = 1;

                    }
                    Cap.listheader.Add(itemAdd);
                }
                QuyTrinh.listHeader.Add(Cap);

            }
            QuyTrinh.listHeader_Data = QuyTrinh.listHeader_Data.OrderBy(a => a.STT).ToList();
            QuyTrinh.listHeader = QuyTrinh.listHeader.OrderBy(a => a.Cap).ToList();
            return QuyTrinh;
        }
        public List<clsBangTinhLuong_NhanVien> GetListGiatri(
            List<clsBangTinhLuong_Header> listheader,
            List<HRM_QuyTrinhTinhLuong_NhanVien> listDaTa,
            List<HRM_NhanVien> listNhanVien,
            List<HRM_HopDong> listHopDong,
            ApplicationDbContext db)
            => _payrollGridBuilder.BuildStandardGrid(listheader, listDaTa, listNhanVien, listHopDong, db);
        public List<clsBangTinhLuong_NhanVien> GetListGiatriPHT(
            List<clsBangTinhLuong_Header> listheader,
            List<HRM_QuyTrinhTinhLuong_NhanVien> listDaTa,
            List<HRM_NhanVien> listNhanVien,
            List<HRM_HopDong> listHopDong,
            ApplicationDbContext db)
            => _payrollGridBuilder.BuildPhtGrid(listheader, listDaTa, listNhanVien, listHopDong, db);
        #endregion
        public double TinhThue(double TongLuong, List<HRM_dmCauHinhThueThuNhapCaNhan> listCauHinh)
            => _taxCalculationService.CalculatePersonalIncomeTax(TongLuong, listCauHinh);
        #region Cáº¥p phÃ¡t tÃ i sáº£n
        public dynamic GetListCapPhatThuHoiTaiSan(dynamic dyn, ApplicationDbContext db)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_CapPhatThuHoiTaiSan> listQuytrinh = from a in db.HRM_CapPhatThuHoiTaiSan
                                                                    orderby a.Created descending
                                                                    select a;


                if (TimKiem.IdCongTy > 0)
                {
                    listQuytrinh = listQuytrinh.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                }
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == nameof(HRM_CapPhatThuHoiTaiSan) && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    listQuytrinh = listQuytrinh.Where(a => listId.Contains(a.IdGuid));
                }
                if (TimKiem.Loai > 0)
                {
                    listQuytrinh = listQuytrinh.Where(a => a.Loai == TimKiem.Loai);
                }
                List<string> listIdQuyTrinh = listQuytrinh.Select(a => a.IdGuid).Distinct().ToList();
                var listIdUserCreatedBy = listQuytrinh.GroupBy(q => q.CreatedBy).Select(q => q.Key).ToList();

                var listUser = _CallWebService.GetDanhSachUserTheoListIdUser(db, listIdUserCreatedBy);
                var result = (from a in db.HRM_CapPhatThuHoiTaiSan_Item
                              where listIdQuyTrinh.Contains(a.IdQuyTrinh)
                              orderby a.Created descending
                              select a).ToList();
                List<string> listIdNhanVien = result.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                {
                    listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())).ToList();
                }
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.NgayCapPhat >= dtTuNgay).ToList();
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.NgayCapPhat < dtDenNgay.AddDays(1)).ToList();
                listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                result = result.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
                var itemList = result.ToList();
                listIdQuyTrinh = itemList.Select(a => a.IdQuyTrinh).Distinct().ToList();
                List<HRM_CapPhatThuHoiTaiSan_NhanVien> listResult = new List<HRM_CapPhatThuHoiTaiSan_NhanVien>();
                foreach (var item in listIdQuyTrinh)
                {
                    var listDaTa = itemList.Where(a => a.IdQuyTrinh == item).ToList();
                    listIdNhanVien = listDaTa.Select(a => a.IdNhanVien).Distinct().ToList();
                    var QuyTrinh = listQuytrinh.FirstOrDefault(a => a.IdGuid == item);
                    foreach (var Nv in listIdNhanVien)
                    {
                        var Datas = listDaTa.Where(a => a.IdNhanVien == Nv).ToList();
                        var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == Nv);
                        var NhanVien = Datas.FirstOrDefault();
                        HRM_CapPhatThuHoiTaiSan_NhanVien itemAdd = new HRM_CapPhatThuHoiTaiSan_NhanVien();
                        itemAdd.MaNhanVien = NV.MaNhanVien;
                        itemAdd.TenNhanVien = NV.TenDayDu;
                        itemAdd.IdNhanVien = NV.IdGuid;
                        itemAdd.IdBoPhan = NV.IdBoPhanEOS;
                        itemAdd.IdCongTy = QuyTrinh.IdCongTyEOS;
                        itemAdd.GhiChu = QuyTrinh.GhiChu;
                        itemAdd.ItemUserCreatedBy = listUser.FirstOrDefault(q => q.Id == QuyTrinh.CreatedBy);
                        itemAdd.IdChucVu = NV.IdChucVu;
                        itemAdd.IdGuid = item;
                        if (NhanVien.Loai == 1)
                        {
                            itemAdd.Loai = 1;
                            itemAdd.TenLoai = "Cáº¥p phÃ¡t";
                            itemAdd.SoLuong = Datas.Sum(a => a.SoLuongCapPhat);
                            itemAdd.GiaTri = Datas.Sum(a => a.GiaTriCapPhat);
                            itemAdd.Ngay = Datas.OrderByDescending(a => a.NgayCapPhat).FirstOrDefault().NgayCapPhat;
                        }
                        if (NhanVien.Loai == 2)
                        {
                            itemAdd.Loai = 2;
                            itemAdd.TenLoai = "Thu há»“i";
                            itemAdd.SoLuong = Datas.Sum(a => a.SoLuongThuHoi);
                            itemAdd.GiaTri = Datas.Sum(a => a.GiaTriThuHoi);
                            itemAdd.Ngay = Datas.OrderByDescending(a => a.NgayThuHoi).FirstOrDefault().NgayThuHoi;
                        }
                        listResult.Add(itemAdd);
                    }

                }

                int nSum = listResult.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                listResult = listResult.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                var listCongTy = _CallWebService.GetDanhSachDuAn(db);
                List<BoPhan> BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                foreach (var item in listResult)
                {
                    var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                    BoPhan CongTy = listCongTy.Find(x => x.Id == item.IdCongTy);
                    if (CongTy != null)
                        item.TenCongTy = CongTy.TenBoPhan;
                    BoPhan BoPhan = BoPhans.Find(x => x.Id == item.IdBoPhan);
                    if (BoPhan != null)
                        item.TenBoPhan = BoPhan.TenBoPhan;
                    var ChucDanh = listChucDanh.Find(x => x.Id == NV.IdChucDanh);
                    if (ChucDanh != null)
                        item.TenChucDanh = ChucDanh.Ten;
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = listResult;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic SetCapPhatThuHoiTaiSan(HRM_CapPhatThuHoiTaiSan model, ApplicationDbContext db)
        {

            try
            {
                if (model.listIdBoPhan != null && model.listIdBoPhan.Count > 0)
                    model.IdBoPhan = string.Join(",", model.listIdBoPhan);

                if (string.IsNullOrEmpty(model.IdGuid))
                {
                    model.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_CapPhatThuHoiTaiSan.Add(model);
                }
                else
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                var ListNhanVienOld = db.HRM_CapPhatThuHoiTaiSan_Item.Where(r => r.IdQuyTrinh == model.IdGuid).ToList();
                if (ListNhanVienOld != null && ListNhanVienOld.Count > 0)
                    db.HRM_CapPhatThuHoiTaiSan_Item.RemoveRange(ListNhanVienOld);
                if (model.listIdBoPhan != null && model.listIdBoPhan.Count > 0)
                {
                    foreach (var item in model.listIdBoPhan)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = model.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_CapPhatThuHoiTaiSan);
                        db.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }
                }
                if (model.listNhanVien != null && model.listNhanVien.Count > 0)
                {
                    foreach (var nhanVien in model.listNhanVien)
                    {

                        if (nhanVien.listTaiSan != null && nhanVien.listTaiSan.Count > 0)
                        {

                            foreach (var item in nhanVien.listTaiSan)
                            {
                                if (nhanVien.DatCoc != 0)
                                {
                                    item.DatCoc = nhanVien.DatCoc / nhanVien.listTaiSan.Count;

                                }
                                item.TinhLuong = nhanVien.TinhLuong;
                                item.UnixToDatetime();
                                item.IdGuid = Guid.NewGuid().ToString();
                                item.IdQuyTrinh = model.IdGuid;
                                item.Loai = model.Loai;
                                item.IdNhanVien = nhanVien.IdNhanVien;
                            }

                            db.HRM_CapPhatThuHoiTaiSan_Item.AddRange(nhanVien.listTaiSan);
                            List<string> listIdCapPhat = nhanVien.listTaiSan.Where(a => !string.IsNullOrEmpty(a.IdCapPhat_Item)).Select(a => a.IdCapPhat_Item).ToList();
                            if (listIdCapPhat != null && listIdCapPhat.Count > 0)
                            {
                                List<HRM_CapPhatThuHoiTaiSan_Item> listResult = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => a.Loai == 1 && listIdCapPhat.Contains(a.IdGuid)).ToList();
                                if (listResult != null && listResult.Count > 0)
                                    foreach (var item in listResult)
                                    {
                                        item.SoLuongThuHoi = nhanVien.listTaiSan.FirstOrDefault(a => a.IdCapPhat_Item == item.IdGuid).SoLuongThuHoi;
                                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                    }
                            }
                        }
                    }
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xá»­ lÃ½ thÃ nh cÃ´ng", model);
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetCapPhatThuHoiTaiSan(string Id, ApplicationDbContext db)
        {

            try
            {
                var QuyTrinh = db.HRM_CapPhatThuHoiTaiSan.FirstOrDefault(a => a.IdGuid == Id);
                if (QuyTrinh.IdBoPhan != null)
                {
                    QuyTrinh.listIdBoPhan = new List<long>();
                    if (QuyTrinh.IdBoPhan.Contains(","))
                    {
                        string[] list = QuyTrinh.IdBoPhan.Split(',');
                        foreach (var bp in list)
                            QuyTrinh.listIdBoPhan.Add(long.Parse(bp));
                    }
                    else QuyTrinh.listIdBoPhan.Add(long.Parse(QuyTrinh.IdBoPhan));
                }
                QuyTrinh.listNhanVien = new List<HRM_CapPhatThuHoiTaiSan_NhanVien>();
                var ListTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.Where(r => r.IdQuyTrinh == Id).AsNoTracking().ToList();
                List<string> listIdNhanVien = ListTaiSan.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                foreach (var item in listNhanVien)
                {
                    HRM_CapPhatThuHoiTaiSan_NhanVien itemAdd = new HRM_CapPhatThuHoiTaiSan_NhanVien();
                    itemAdd.IdNhanVien = item.IdGuid;
                    itemAdd.MaNhanVien = item.MaNhanVien;
                    itemAdd.TenNhanVien = item.TenDayDu;
                    var chucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                    if (chucDanh != null)
                        itemAdd.TenChucDanh = chucDanh.Ten;
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                    if (BoPhan != null)
                        itemAdd.TenBoPhan = BoPhan.TenBoPhan;

                    itemAdd.listTaiSan = ListTaiSan.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.Created).ToList();
                    if (itemAdd.listTaiSan != null && itemAdd.listTaiSan.Count > 0)
                    {
                        itemAdd.DatCoc = itemAdd.listTaiSan.Sum(a => a.DatCoc ?? 0);
                        itemAdd.TinhLuong = itemAdd.listTaiSan.FirstOrDefault().TinhLuong;
                        foreach (var ts in itemAdd.listTaiSan)
                            ts.DateTimeToUnix();
                    }
                    QuyTrinh.listNhanVien.Add(itemAdd);
                }
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Láº¥y dá»¯ liá»‡u thÃ nh cÃ´ng", QuyTrinh);
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic DeleteCapPhatThuHoiTaiSan(string Id, ApplicationDbContext db)
        {

            try
            {
                var QuyTrinh = db.HRM_CapPhatThuHoiTaiSan.Find(Id);
                db.HRM_CapPhatThuHoiTaiSan.Remove(QuyTrinh);
                var ListBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == Id).ToList();
                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                var ListNhanVienOld = db.HRM_CapPhatThuHoiTaiSan_Item.Where(r => r.IdQuyTrinh == Id).ToList();
                if (ListNhanVienOld != null && ListNhanVienOld.Count > 0)
                    db.HRM_CapPhatThuHoiTaiSan_Item.RemoveRange(ListNhanVienOld);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "XÃ³a dá»¯ liá»‡u thÃ nh cÃ´ng", QuyTrinh);
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetDanhSanhNhanVienDaCapPhatTaiSan(long IdCongTy, List<long> listIdBoPhan, ApplicationDbContext db)
        {

            List<HRM_CapPhatThuHoiTaiSan_Item> listData = (from a in db.HRM_CapPhatThuHoiTaiSan_Item

                                                           where a.Loai == 1 && (a.SoLuongCapPhat ?? 0) > (a.SoLuongThuHoi ?? 0)

                                                           select a).ToList();

            List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
            if (IdCongTy > 0)
            {
                var listNhanSuDuAn = _CallWebService.GetListUserNhanSuDuAnCurrent(IdCongTy, db);
                List<string> listIdUser = listNhanSuDuAn.Select(a => a.IdUser).Distinct().ToList();
                listNhanVien = listNhanVien.Where(a => listIdUser.Contains(a.IdUser)).ToList();
            }
            if (listIdBoPhan.Count > 0)
            {
                listNhanVien = listNhanVien.Where(a => listIdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            }

            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
            listData = listData.Where(a => listIdNhanVien.Contains(a.IdNhanVien)).ToList();
            var listresult = new List<HRM_CapPhatThuHoiTaiSan_NhanVien>();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            foreach (var item in listNhanVien)
            {
                HRM_CapPhatThuHoiTaiSan_NhanVien itemAdd = new HRM_CapPhatThuHoiTaiSan_NhanVien();
                itemAdd.IdNhanVien = item.IdGuid;
                itemAdd.MaNhanVien = item.MaNhanVien;
                itemAdd.TenNhanVien = item.TenDayDu;
                var chucDanh = listChucDanh.FirstOrDefault(a => a.Id == item.IdChucDanh);
                if (chucDanh != null)
                    itemAdd.TenChucDanh = chucDanh.Ten;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                if (BoPhan != null)
                    itemAdd.TenBoPhan = BoPhan.TenBoPhan;

                itemAdd.listTaiSan = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderBy(a => a.Created).ToList();
                foreach (var ts in itemAdd.listTaiSan)
                {
                    ts.DateTimeToUnix();
                    ts.IdCapPhat_Item = ts.IdGuid;
                }
                listresult.Add(itemAdd);
            }
            return listresult;
        }
        public List<HRM_BaoCaoCapPhatDongPhuc> GetTongHopTheoDoiCapPhatTaiSan(ApplicationDbContext db, TimKiemBangLuong timkiem)
        {
            var listCapPhat = db.HRM_CapPhatThuHoiTaiSan.Where(a => timkiem.IdCongty == 0 || timkiem.IdCongty == a.IdCongTyEOS).ToList();
            List<string> listIdCapPhat = listCapPhat.Select(a => a.IdGuid).Distinct().ToList();
            var listDaTa = db.HRM_CapPhatThuHoiTaiSan_Item.Where(a => listIdCapPhat.Contains(a.IdQuyTrinh)).ToList();
            List<string> listIdNhanVien = listDaTa.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString() && listIdNhanVien.Contains(a.IdGuid)).ToList();
            if (timkiem.listIdBoPhan.Count != 0)
                listNhanVien = listNhanVien.Where(a => timkiem.listIdBoPhan.Contains(a.IdBoPhanEOS)).ToList();
            if (!string.IsNullOrEmpty(timkiem.Keyword))
                listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(timkiem.Keyword.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(timkiem.Keyword.ToLower().Trim())).ToList();

            listIdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
            var listHopDong = db.HRM_HopDong.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).OrderByDescending(a => a.Modified).ToList();
            var listCa = db.HRM_dmLichLamViec.ToList();
            var listTaiSandb = db.HRM_dmTrangThietBi.ToList();
            List<HRM_BaoCaoCapPhatDongPhuc> listresult = new List<HRM_BaoCaoCapPhatDongPhuc>();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            DateTime dt = DateTime.Now;
            foreach (var item in listNhanVien)
            {
                HRM_BaoCaoCapPhatDongPhuc itemAdd = new HRM_BaoCaoCapPhatDongPhuc();
                itemAdd.MaNhanVien = item.MaNhanVien;
                itemAdd.TenNhanVien = item.TenDayDu;
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                if (BoPhan != null)
                    itemAdd.BoPhan = BoPhan.TenBoPhan;
                var CongTy = listBoPhan.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                if (CongTy != null)
                    itemAdd.CoSo = CongTy.TenBoPhan;

                var HopDong = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                if (HopDong != null && !string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                {
                    var CaLamViec = listCa.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                    if (CaLamViec != null)
                    {

                        if (CaLamViec.isCaGay == true)
                            itemAdd.LoaiCong = "P";
                        else
                            itemAdd.LoaiCong = "F";

                    }
                }
                double Thang = 0;
                if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
                {
                    itemAdd.NgayBatDauLamViec = item.NgayBatDauLamViec;
                    Thang = (dt - item.NgayBatDauLamViec.Value).TotalDays / 30.5;
                }
                if (Thang > 0)
                    itemAdd.ThamNien = Math.Round(Thang, 2).ToString();
                var datas = listDaTa.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                itemAdd.listTaiSan = new List<HRM_BaoCaoCapPhatDongPhuc_TaiSan>();
                List<string> listIdTaiSan = datas.Select(a => a.IdTaiSan).ToList();
                var listTaiSan = listTaiSandb.Where(a => listIdTaiSan.Contains(a.IdGuid)).ToList();
                if (listTaiSan != null && listTaiSan.Count > 0)
                    foreach (var TaiSan in listTaiSan)
                    {
                        HRM_BaoCaoCapPhatDongPhuc_TaiSan Add = new HRM_BaoCaoCapPhatDongPhuc_TaiSan();
                        Add.TenTaiSan = TaiSan.Ten;
                        // phÃ¡t Ä‘Ã´ng phá»¥c
                        var listTSPhat = datas.Where(a => a.IdTaiSan == TaiSan.IdGuid && a.Loai == 1 && a.TinhLuong == true).ToList();
                        if (listTSPhat != null && listTSPhat.Count > 0)
                        {
                            Add.SoLuongPhatMoi = listTSPhat.Sum(a => a.SoLuongCapPhat ?? 0);
                            Add.GiaPhatMoi = listTSPhat.Sum(a => (a.GiaTriCapPhat ?? 0));
                            DateTime time = dt;
                            if (listTSPhat.Where(a => a.NgayCapPhat != null).OrderByDescending(a => a.NgayCapPhat).FirstOrDefault() != null)
                                if (listTSPhat.Where(a => a.NgayCapPhat != null).OrderByDescending(a => a.NgayCapPhat).FirstOrDefault().NgayCapPhat != null)
                                    time = listTSPhat.OrderByDescending(a => a.NgayCapPhat).FirstOrDefault().NgayCapPhat.Value;
                            Add.NgayPhatPhatMoi = time;
                            Add.NgayThuTienPhatMoi = time.Month + "/" + time.Year;

                        }
                        // phÃ¡t Bá»• sung
                        var listTSPhatBS = datas.Where(a => a.IdTaiSan == TaiSan.IdGuid && a.Loai == 1 && a.TinhLuong == false).ToList();
                        if (listTSPhatBS != null && listTSPhatBS.Count > 0)
                        {
                            Add.SoLuongBoSung = listTSPhatBS.Sum(a => a.SoLuongCapPhat ?? 0);
                            Add.GiaBoSung = listTSPhatBS.Sum(a => (a.GiaTriCapPhat ?? 0));

                            DateTime time = dt;
                            if (listTSPhatBS.Where(a => a.NgayCapPhat != null).OrderByDescending(a => a.NgayCapPhat).FirstOrDefault() != null)
                                if (listTSPhatBS.Where(a => a.NgayCapPhat != null).OrderByDescending(a => a.NgayCapPhat).FirstOrDefault().NgayCapPhat != null)
                                    time = listTSPhatBS.OrderByDescending(a => a.NgayCapPhat).FirstOrDefault().NgayCapPhat.Value;
                            Add.NgayPhatBoSung = time;
                            //Add.NgayThuTienPhatMoi = time.Month + "/" + time.Year;

                        }
                        // phÃ¡t Thu há»“i
                        var listThuHoi = datas.Where(a => a.IdTaiSan == TaiSan.IdGuid && a.Loai == 2).ToList();
                        if (listThuHoi != null && listThuHoi.Count > 0)
                        {
                            Add.SoLuongThuHoi = listThuHoi.Sum(a => a.SoLuongThuHoi ?? 0);
                            Add.GiaThuHoi = listThuHoi.Sum(a => (a.GiaTriThuHoi ?? 0));
                            DateTime time = dt;
                            if (listThuHoi.Where(a => a.NgayThuHoi != null).OrderByDescending(a => a.NgayThuHoi).FirstOrDefault() != null)
                                if (listThuHoi.Where(a => a.NgayThuHoi != null).OrderByDescending(a => a.NgayThuHoi).FirstOrDefault().NgayThuHoi != null)
                                    time = listThuHoi.OrderByDescending(a => a.NgayThuHoi).FirstOrDefault().NgayThuHoi.Value;

                            Add.NgayPhatThuHoi = time;
                            Add.NgayThuTienThuHoi = time.Month + "/" + time.Year;

                        }
                        itemAdd.listTaiSan.Add(Add);
                    }
                listresult.Add(itemAdd);
            }


            return listresult;
        }
        public dynamic ExportTongHopTheoDoiCapPhatTaiSan(ApplicationDbContext db, dynamic dyn)
        {
            try
            {
                TimKiem TimKiem = JsonConvert.DeserializeObject(dyn.ToString(), typeof(TimKiem)) as TimKiem;
                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);

                string sFile = "TheoDoiCapPhatThuHoiDongPhuc.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                if (Host == "PHT")
                    sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/" + sFile);
                var filenamegui = "TongHopTheoDoiCapPhatTaiSan.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                int Loai = 1;
                if (TimKiem.Loai == 2)
                    Loai = 2;
                string NoiDung = "Cáº¤P PHÃT";
                if (Loai == 2)
                    NoiDung = "THU Há»’I";
                var listDongPhuc = db.HRM_dmTrangThietBi.OrderBy(a => a.Ten).ToList();
                int CountCol = listDongPhuc.Count + 5;
                sheet1.Cells[1, 3].Value = "Sá»” THEO DÃ•I " + NoiDung + " Báº¢O Há»˜ LAO Äá»˜NG THÃNG " + dtDenNgay.ToString("MM/yyyy");
                sheet1.Cells[1, 3, 1, CountCol - 3].Merge = true;
                sheet1.Cells[1, CountCol - 2, 1, CountCol].Merge = true;
                sheet1.Cells[1, CountCol - 2].Value = "MÃ£ sá»‘: BM.08.VT.10";
                sheet1.Cells[1, CountCol - 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                sheet1.Cells[2, 3].Value = "Tá»« ngÃ y " + dtTuNgay.ToString("dd/MM/yyyy") + " Ä‘áº¿n ngÃ y " + dtDenNgay.ToString("dd/MM/yyyy");
                sheet1.Cells[2, 3, 2, CountCol - 3].Merge = true;
                sheet1.Cells[4, 6, 4, CountCol].Merge = true;
                int nRow = 5;
                int ncol = 6;
                foreach (var item in listDongPhuc)
                {

                    sheet1.Cells[nRow, ncol].Value = item.Ten;
                    ncol++;
                }
                dungchung.DrawTableExcel(sheet1, 4, 5, 1, CountCol);
                #region data
                IQueryable<HRM_CapPhatThuHoiTaiSan> listQuytrinh = from a in db.HRM_CapPhatThuHoiTaiSan
                                                                    orderby a.Created descending
                                                                    select a;
                if (TimKiem.IdCongTy > 0)
                {
                    listQuytrinh = listQuytrinh.Where(a => a.IdCongTyEOS == TimKiem.IdCongTy);
                }
                if (TimKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == nameof(HRM_CapPhatThuHoiTaiSan) && a.IdBoPhan == TimKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    listQuytrinh = listQuytrinh.Where(a => listId.Contains(a.IdGuid));
                }

                listQuytrinh = listQuytrinh.Where(a => a.Loai == Loai);
                List<string> listIdQuyTrinh = listQuytrinh.Select(a => a.IdGuid).Distinct().ToList();
                var listIdUserCreatedBy = listQuytrinh.GroupBy(q => q.CreatedBy).Select(q => q.Key).ToList();

                var listUser = _CallWebService.GetDanhSachUserTheoListIdUser(db, listIdUserCreatedBy);
                var result = (from a in db.HRM_CapPhatThuHoiTaiSan_Item
                              where listIdQuyTrinh.Contains(a.IdQuyTrinh)
                              orderby a.Created descending
                              select a).ToList();

                //if (!string.IsNullOrEmpty(TimKiem.KeyWord))
                //{
                //    listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())).ToList();
                //}
                result = result.Where(a => a.NgayCapPhat >= dtTuNgay && a.NgayCapPhat < dtDenNgay).ToList();


                #endregion
                if (result != null && result.Count > 0)
                {
                    List<string> listIdNhanVien = result.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listCongTy = _CallWebService.GetDanhSachCongTy(db);

                    nRow++;
                    int STT = 1;
                    foreach (var item in listNhanVien)
                    {
                        ncol = 1;
                        sheet1.Cells[nRow, ncol].Value = STT;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.TenDayDu;
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = listCongTy.Where(a => a.Id == item.IdCongTyEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                        ncol++;
                        var listData = result.Where(a => a.IdNhanVien == item.IdGuid).ToList();
                        if (Loai == 1)
                        {
                            List<string> listNgay = listData.Select(a => a.NgayCapPhat.Value.ToString("dd/MM")).Distinct().ToList();
                            sheet1.Cells[nRow, ncol].Value = string.Join(",", listNgay);
                        }
                        else
                        {
                            List<string> listNgay = listData.Select(a => a.NgayThuHoi.Value.ToString("dd/MM")).Distinct().ToList();
                            sheet1.Cells[nRow, ncol].Value = string.Join(",", listNgay);
                        }
                        ncol++;
                        sheet1.Cells[nRow, ncol].Value = item.SoGen;
                        ncol++;
                        foreach (var dp in listDongPhuc)
                        {
                            var listDp = listData.Where(a => a.IdTaiSan == dp.IdGuid).ToList();
                            if (listDp != null && listDp.Count > 0)
                            {
                                if (Loai == 1)
                                    sheet1.Cells[nRow, ncol].Value = listDp.Sum(a => a.SoLuongCapPhat);
                                else
                                    sheet1.Cells[nRow, ncol].Value = listDp.Sum(a => a.SoLuongThuHoi);

                            }
                            ncol++;
                        }
                        STT++;
                        nRow++;

                    }
                }
                nRow--;
                dungchung.DrawTableExcel(sheet1, 4, nRow, 1, CountCol);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells[4, 1, nRow, CountCol].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 5)
                {
                    sheet1.Cells[6, 2, nRow, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;

                }
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
            }
        }
        public dynamic ExportTongHopTheoDoiCapPhatTaiSanOld(ApplicationDbContext db, TimKiemBangLuong timkiem)
        {
            try
            {
                string sFile = "TongHopTheoDoiCapPhatTaiSan.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);

                var filenamegui = "TongHopTheoDoiCapPhatTaiSan.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                List<HRM_BaoCaoCapPhatDongPhuc> listresult = GetTongHopTheoDoiCapPhatTaiSan(db, timkiem);
                int nRow = 5;
                int ncol = 1;
                if (listresult.Count > 0)
                    foreach (var item in listresult)
                    {
                        ncol = 1;
                        int nCoutTS = item.listTaiSan.Count();
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        sheet1.Cells[nRow, ncol].Value = item.CoSo;
                        ncol++;
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        sheet1.Cells[nRow, ncol].Value = item.MaNhanVien;
                        ncol++;
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        sheet1.Cells[nRow, ncol].Value = item.TenNhanVien;
                        ncol++;
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        sheet1.Cells[nRow, ncol].Value = item.BoPhan;
                        ncol++;
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        if (item.NgayBatDauLamViec != null)
                            sheet1.Cells[nRow, ncol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        ncol++;
                        sheet1.Cells[nRow, ncol, nRow + nCoutTS - 1, ncol].Merge = true;
                        sheet1.Cells[nRow, ncol].Value = item.ThamNien;
                        ncol++;
                        int col = ncol;
                        foreach (var ts in item.listTaiSan)
                        {
                            col = ncol;
                            sheet1.Cells[nRow, col].Value = ts.TenTaiSan;
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.SoLuongPhatMoi;
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.GiaPhatMoi;
                            sheet1.Cells[nRow, col].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
                            col++;
                            if (ts.NgayPhatPhatMoi != null)

                                sheet1.Cells[nRow, col].Value = ts.NgayPhatPhatMoi.Value.ToString("dd/MM/yyyy");
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.NgayThuTienPhatMoi;
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.SoLuongBoSung;
                            col++;
                            sheet1.Cells[nRow, col].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
                            sheet1.Cells[nRow, col].Value = ts.GiaBoSung;
                            col++;
                            if (ts.NgayPhatBoSung != null)
                                sheet1.Cells[nRow, col].Value = ts.NgayPhatBoSung.Value.ToString("dd/MM/yyyy");
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.SoLuongThuHoi;
                            col++;
                            sheet1.Cells[nRow, col].Style.Numberformat.Format = dungchung.m_FormatSoLuong1;
                            sheet1.Cells[nRow, col].Value = ts.GiaThuHoi;
                            col++;
                            if (ts.NgayPhatThuHoi != null)

                                sheet1.Cells[nRow, col].Value = ts.NgayPhatThuHoi.Value.ToString("dd/MM/yyyy");
                            col++;
                            sheet1.Cells[nRow, col].Value = ts.NgayThuTienThuHoi;

                            nRow++;
                        }
                        ncol = col;

                    }
                nRow--;
                dungchung.DrawTableExcel(sheet1, 3, nRow, 1, ncol);
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                if (nRow > 5)
                {
                    sheet1.Cells[5, 1, nRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[5, 3, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[5, 7, nRow, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                }
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
            }
        }
        #endregion
        public void UpdateLuongNhanVien(ApplicationDbContext db)
        {
            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString()).ToList();
            List<string> listIdNV = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
            var listHopDong = db.HRM_HopDong.Where(a => a.isKetThuc == true && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && listIdNV.Contains(a.IdNhanVien)).ToList();
            List<string> listIdHD = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
            var listLuong = db.HRM_Luong_HopDong.Where(a => listIdHD.Contains(a.IdHopDong)).ToList();
            var listLuongBH = db.HRM_LuongBaoHiem_HopDong.Where(a => listIdHD.Contains(a.IdHopDong)).ToList();
            var listPhuCap = db.HRM_HopDongPhuCap.Where(a => listIdHD.Contains(a.IdHopDong)).ToList();
            List<HRM_Luong_NhanVien> listLuongAdd = new List<HRM_Luong_NhanVien>();
            List<HRM_Luong_PhuCap_NhanVien> listPCAdd = new List<HRM_Luong_PhuCap_NhanVien>();
            foreach (var item in listNhanVien)
            {
                var QuyTrinh = listHopDong.FirstOrDefault(a => a.IdNhanVien == item.IdGuid);
                if (QuyTrinh != null)
                {
                    QuyTrinh.LuongHopDong = listLuong.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                    QuyTrinh.LuongBaoHiem = listLuongBH.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                    QuyTrinh.lstPhuCap = listPhuCap.Where(a => a.IdHopDong == QuyTrinh.IdGuid).ToList();
                    HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(QuyTrinh);
                    listLuongAdd.Add(luong_NhanVien);
                    if (QuyTrinh.lstPhuCap != null)
                    {
                        foreach (var PC in QuyTrinh.lstPhuCap)
                            listPCAdd.Add(new HRM_Luong_PhuCap_NhanVien(QuyTrinh, PC, luong_NhanVien.IdGuid, nameof(HRM_HopDong), Host));

                    }
                }

            }
            db.HRM_Luong_NhanVien.AddRange(listLuongAdd);
            db.HRM_Luong_PhuCap_NhanVien.AddRange(listPCAdd);
            db.SaveChanges();
        }
        #region diá»…n giáº£i cháº¥m cÃ´ng
        public dynamic GetDienGiaiChamCong(ApplicationDbContext db, string Id)
        {
            try
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                HRM_DienGiaiChamCong QuyTrinh = db.HRM_DienGiaiChamCong.FirstOrDefault(x => x.IdGuid == Id);
                QuyTrinh.ThoiGianUnix = dungchung.DateTimeToUnixTimeStamp(QuyTrinh.ThoiGian);
                var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == QuyTrinh.IdUser);
                QuyTrinh.TenNguoiTao = NhanVien.TenDayDu;
                if (!base.GetQuyTrinh(db, _CallWebService, QuyTrinh, err, true))
                    return err;
                QuyTrinh.listFileDinhKem = FileUploadController.GetListFileDinhKem(db, QuyTrinh.IdGuid, nameof(HRM_DienGiaiChamCong));
                return QuyTrinh;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }
        }
        public dynamic GetNextSoDienGiaiChamCong(ApplicationDbContext db)
            => CreateSoPhieuResult(GenerateNextCodeFromLast(
                "DGCC",
                db.HRM_DienGiaiChamCong.OrderByDescending(a => a.Created).Select(a => a.SoQuyTrinh).FirstOrDefault()));
        public dynamic GetListDienGiaiChamCong(ApplicationDbContext db, dynamic data)
        {
            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_DienGiaiChamCong> result;

                if (TimKiem.TabTrangThai != 3)

                    result = from a in db.HRM_DienGiaiChamCong
                             join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable

                             where b.isDaXuLy == (TimKiem.TabTrangThai - 1)
                             && b.IdUser == db.currentUser.Id
                             orderby a.Created descending
                             select a;
                else
                    result = from a in db.HRM_DienGiaiChamCong
                             orderby a.Created descending
                             select a;


                DateTime dtTuNgay = UnixTimeStampToDateTime(TimKiem.TuNgay ?? 0);
                DateTime dtDenNgay = UnixTimeStampToDateTime(TimKiem.DenNgay ?? 0);
                if (TimKiem.TuNgay > 0)
                    result = result.Where(a => a.Created >= dtTuNgay);
                if (TimKiem.DenNgay > 0)
                    result = result.Where(a => a.Created < dtDenNgay.AddDays(1));
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => (!string.IsNullOrEmpty(a.SoQuyTrinh) && a.SoQuyTrinh.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.CreatedByName) && a.CreatedByName.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.NoiDung) && a.NoiDung.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim())));
                if (!string.IsNullOrEmpty(TimKiem.IdUser))
                {

                    var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == TimKiem.IdUser);
                    result = result.Where(a => !string.IsNullOrEmpty(a.IdUser) && a.IdUser == NhanVien.IdUser);
                }
                if (TimKiem.IdBoPhan > 0)
                {

                    var listIdUser = db.HRM_NhanVien.Where(a => a.IdBoPhanEOS == TimKiem.IdBoPhan).Select(a => a.IdUser).ToList();
                    result = result.Where(a => !string.IsNullOrEmpty(a.IdUser) && listIdUser.Contains(a.IdUser));
                }
                List<string> listId = new List<string>();
                if (TimKiem.IdDuAn > 0)
                {
                    var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.AsNoTracking().Where(a => a.TableName == nameof(HRM_NhanVien) && a.IdBoPhan == TimKiem.IdDuAn).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdNhanVien));
                }
                if (nCurrentPage == 0)
                {
                    return result.AsNoTracking().ToList();
                }
                var itemList = Paginate(result.AsNoTracking().Distinct(), nCurrentPage, PageSize, out PagingInfor paging);
                var webService = _CallWebService;
                var listIdNv = itemList.Select(a => a.IdNhanVien).Distinct().ToList();
                List<HRM_NhanVien> listNhanVien = db.HRM_NhanVien.Where(a => listIdNv.Contains(a.IdGuid)).ToList();
                foreach (HRM_DienGiaiChamCong item in itemList)
                {
                    item.TenNguoiTao = listNhanVien.FirstOrDefault(q => q.IdGuid == item.IdNhanVien).TenDayDu;
                    if (!string.IsNullOrEmpty(item.GhiChu))
                        item.TenNguoiTao += " (" + item.GhiChu + ")";
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic SetDienGiaiChamCong(ApplicationDbContext db, HRM_DienGiaiChamCong QuyTrinh)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    //= JsonConvert.DeserializeObject<HRM_DienGiaiChamCong>(data.ToString());

                    var QuyTrinhdb = db.HRM_DienGiaiChamCong.AsNoTracking().FirstOrDefault(a => a.IdGuid == QuyTrinh.IdGuid);
                    if (!base.SetTruocHamUpdate(db, _CallWebService, QuyTrinhdb, QuyTrinh, err))
                        return err;

                    MessageReturn check = UpdateDienGiaiChamCong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.Set);
                    if (check.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, check.sError);
                    }

                    if (!base.SetSauHamUpdate(db, QuyTrinhdb, QuyTrinh, err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng", QuyTrinh.IdGuid);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
                }
            }
        }
        public dynamic UpdateDienGiaiChamCong(HRM_DienGiaiChamCong QuyTrinh, ApplicationDbContext db, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            try
            {
                //string checkHRM = ConfigurationManager.AppSettings["ISHRM"] ?? "0";
                //if (checkHRM == "1")
                //{
                //    List<HRM_DienGiaiChamCong_ChiTiet> listNgay = QuyTrinh.listUser;

                //}
                QuyTrinh.ThoiGian = dungchung.UnixTimeStampToDateTime(QuyTrinh.ThoiGianUnix);
                if (!string.IsNullOrEmpty(QuyTrinh.IdGuid))
                {


                    db.Entry(QuyTrinh).State = EntityState.Modified;
                    //dungchung.AddModifyDelete(db, DefineData.eTableName.HRM_DienGiaiChamCong, DefineData.eModify.Add, DefineData.eModify.Add, "", JsonConvert.SerializeObject(QuyTrinh));
                }
                else
                {
                    var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdUser == db.currentUser.Id);
                    QuyTrinh.IdUser = NhanVien.IdUser;
                    QuyTrinh.IdNhanVien = NhanVien.IdGuid;
                    QuyTrinh.SoQuyTrinh = GetNextSoDienGiaiChamCong(db).SoPhieu.ToString();
                    QuyTrinh.eAction = DefineData.eAction.DIENGIAICHAMCONG.ToString();
                    if (!base.KhoiTaoQuyTrinh(db, _CallWebService, QuyTrinh, err))
                        return err.Value;

                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    //dungchung.AddModifyDelete(db, DefineData.eTableName.HRM_DienGiaiChamCong, DefineData.eModify.Add, DefineData.eModify.Add, JsonConvert.SerializeObject(QuyTrinh), "");
                }

                FileUploadController.SetFileDinhKem(db, QuyTrinh.listFileDinhKem, nameof(HRM_DienGiaiChamCong).ToString(), QuyTrinh.IdGuid);
                if (!base.SetQuyTrinh(db, _CallWebService, QuyTrinh, eLoaiHam, err))
                    return err.Value;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "ThÃ nh cÃ´ng"); ;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
            }

        }
        public dynamic DeleteDienGiaiChamCong(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    HRM_DienGiaiChamCong QuyTrinh = JsonConvert.DeserializeObject<HRM_DienGiaiChamCong>(data.ToString());
                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;

                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t thÃ nh cÃ´ng", QuyTrinh);
                }
                catch (Exception)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra");
                }
            }
        }
        public dynamic ChuyenTiepDienGiaiChamCong(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    HRM_DienGiaiChamCong QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DienGiaiChamCong)) as HRM_DienGiaiChamCong;

                    HRM_DienGiaiChamCong pcuDatabase = db.HRM_DienGiaiChamCong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();

                    if (!base.ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.IdDuAn ?? 0))
                        return err;
                    MessageReturn Error = UpdateDienGiaiChamCong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.sError, QuyTrinh.IdGuid);
                    }
                    if (!base.ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    if (QuyTrinh.isKetThuc == true)
                    {
                        DateTime time = new DateTime(QuyTrinh.ThoiGian.Year, QuyTrinh.ThoiGian.Month, QuyTrinh.ThoiGian.Day, 0, 0, 0);
                        var listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.Where(a => a.IdNhanVien == QuyTrinh.IdNhanVien && a.Ngay == time).ToList();
                        List<HRM_CheckInOut> listCheckInAdd = new List<HRM_CheckInOut>();

                        if (QuyTrinh.Loai == 0)
                        {
                            HRM_CheckInOut In = new HRM_CheckInOut();
                            In.Id = Guid.NewGuid().ToString();
                            In.Loai = DefineData.eKyHieuChamCong.I.ToString();
                            In.Ngay = time;
                            In.IdUser = QuyTrinh.IdUser;
                            In.IdNhanVien = QuyTrinh.IdNhanVien;
                            In.IdCongTy = QuyTrinh.IdDuAn ?? 0;
                            In.Gio = QuyTrinh.ThoiGian;
                            listCheckInAdd.Add(In);

                        }
                        else if (QuyTrinh.Loai == 1)
                        {
                            HRM_CheckInOut Out = new HRM_CheckInOut();
                            Out.Id = Guid.NewGuid().ToString();
                            Out.Loai = DefineData.eKyHieuChamCong.O.ToString();
                            Out.Ngay = time;
                            Out.IdUser = QuyTrinh.IdUser;
                            Out.IdNhanVien = QuyTrinh.IdNhanVien;
                            Out.IdCongTy = QuyTrinh.IdDuAn ?? 0;
                            Out.Gio = QuyTrinh.ThoiGian;
                            listCheckInAdd.Add(Out);
                        }
                        else if (QuyTrinh.Loai == 2)
                        {

                            foreach (var lich in listLichLamViec)
                            {
                                time = lich.Ngay;
                                HRM_CheckInOut In = new HRM_CheckInOut();
                                In.Id = Guid.NewGuid().ToString();
                                In.Loai = DefineData.eKyHieuChamCong.I.ToString();
                                In.Ngay = time;
                                In.IdUser = QuyTrinh.IdUser;
                                In.IdNhanVien = QuyTrinh.IdNhanVien;
                                In.IdCongTy = QuyTrinh.IdDuAn ?? 0;
                                In.Gio = QuyTrinh.ThoiGian;
                                DateTime TuGio = lich.GioVao;
                                In.Gio = new DateTime(time.Year, time.Month, time.Day, TuGio.Hour, TuGio.Minute, TuGio.Second);

                                listCheckInAdd.Add(In);
                                HRM_CheckInOut Out = new HRM_CheckInOut();
                                Out.Id = Guid.NewGuid().ToString();
                                Out.Loai = DefineData.eKyHieuChamCong.O.ToString();
                                Out.Ngay = time;
                                Out.IdUser = QuyTrinh.IdUser;
                                Out.IdNhanVien = QuyTrinh.IdNhanVien;
                                Out.IdCongTy = QuyTrinh.IdDuAn ?? 0;
                                DateTime DenGio = lich.GioRa;
                                Out.Gio = new DateTime(time.Year, time.Month, time.Day, DenGio.Hour, DenGio.Minute, DenGio.Second);
                                listCheckInAdd.Add(Out);
                            }
                        }
                        db.HRM_CheckInOut.AddRange(listCheckInAdd);

                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyá»ƒn tiáº¿p thÃ nh cÃ´ng!", QuyTrinh.IdGuid);

                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
                }
            }
        }
        public dynamic KhongDuyetDienGiaiChamCong(ApplicationDbContext db, dynamic data)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cáº­p nháº­t dá»¯ liá»‡u thÃ nh cÃ´ng");
                    HRM_DienGiaiChamCong QuyTrinh = JsonConvert.DeserializeObject(data.ToString(), typeof(HRM_DienGiaiChamCong)) as HRM_DienGiaiChamCong;

                    HRM_DienGiaiChamCong pcuDatabase = db.HRM_DienGiaiChamCong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!base.KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;

                    MessageReturn Error = UpdateDienGiaiChamCong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    if (Error.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                    {
                        dbContextTransaction.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, Error.Value, QuyTrinh.IdGuid);
                    }

                    if (!base.KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }

                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyá»ƒn tiáº¿p thÃ nh cÃ´ng!", QuyTrinh.IdGuid);
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
                }
            }
        }
        public dynamic ExportDienGiaiChamCong(ApplicationDbContext db, dynamic data)
        {
            try
            {
                TimKiem Itemtimkiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
                if (Itemtimkiem.TabTrangThai == null || Itemtimkiem.TabTrangThai == 0)
                    Itemtimkiem.TabTrangThai = 2;
                string sFile = "DienGiaiChamCong.xlsx";
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                var filenamegui = "DienGiaiChamCong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                var listIdUser = new List<string>();
                Itemtimkiem.CurrentPage = 0;
                List<HRM_DienGiaiChamCong> itemlist = GetListDienGiaiChamCong(db, JsonConvert.SerializeObject(Itemtimkiem));
                //var itemlist = JsonConvert.DeserializeObject<List<HRM_DienGiaiChamCong>>(datas);
                var listIdNhanVien = itemlist.Select(a => a.IdNhanVien).Distinct().ToList();
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();

                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);

                int Stt = 1;
                int nRow = 4;
                int nCol = 1;
                foreach (var item in itemlist)
                {
                    nCol = 1;
                    var NhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);
                    sheet1.Cells[nRow, nCol].Value = Stt;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.ThoiGian.ToString("dd/MM/yyyy");
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = NhanVien.MaNhanVien;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = NhanVien.TenDayDu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listBoPhan.Where(a => a.Id == NhanVien.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = listChucVu.Where(a => a.Id == NhanVien.IdChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                    nCol++;
                    if (item.Loai == 0)
                        sheet1.Cells[nRow, nCol].Value = "Cháº¥m cÃ´ng Ä‘áº¿n";
                    else if (item.Loai == 1)
                        sheet1.Cells[nRow, nCol].Value = "Cháº¥m cÃ´ng vá»";
                    else
                        sheet1.Cells[nRow, nCol].Value = "Äi khÃ¡ch hÃ ng";

                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.GhiChu;
                    nCol++;
                    sheet1.Cells[nRow, nCol].Value = item.TenTrangThai;

                    nRow++;
                    Stt++;

                }
                sheet1.Cells.Style.Font.Name = "Times New Roman";
                sheet1.Cells.Style.Font.Size = 12;
                sheet1.Cells.Style.WrapText = true;
                sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet1.Cells[4, 4, nRow, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                DrawTableExcel(sheet1, 4, nRow, 1, nCol);


                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "CÃ³ lá»—i xáº£y ra" + e.ToString());
            }
        }
        #endregion
    }
}