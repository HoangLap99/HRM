#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xceed.Words.NET;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using static HRM.Api.Helper.HelperConstant;
using static HRM.Api.Helper.HelperConstant.LuongHopDong;
using DocumentFormat.OpenXml;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using static HRM.Api.Services.FuncBase;
using DocumentFormat.OpenXml.VariantTypes;
namespace HRM.Api.Services.HopDong
{
    public class FuncHopDong : FuncBase
    {
        private readonly FuncDanhMuc _funcDanhMuc;
        private string Host => HrmOptions.Host ?? "0";

        public FuncHopDong(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncHopDong> logger,
            FuncDanhMuc funcDanhMuc)
            : base(callWebService, workflowEngine, options, logger)
        {
            _funcDanhMuc = funcDanhMuc;
        }
        //string PathUploads = ConfigurationManager.AppSettings["PathUploads"] ?? @"C:\uploader\App_Data\uploads\";
        private FileUploadController FileUpload = new FileUploadController();
        public dynamic GetMaHopDong(ApplicationDbContext db, string IdLoaiHopDong, string IdNhanVien, string IdHopDong, long IdChucVu = 0, long IdBoPhan = 0, long IdCongTy = 0, double NgayUnix = 0)
        {
            try
            {

                string sTT = "";
                if (Host == "ICT")
                {

                    if (IdChucVu > 0 && IdBoPhan > 0 && !string.IsNullOrEmpty(IdLoaiHopDong))
                    {
                        int Nam = DateTime.Now.Year;
                        if (NgayUnix > 0)
                        {
                            DateTime time = UnixTimeStampToDateTime(NgayUnix);
                            Nam = time.Year;
                        }
                        var ChucVu = _CallWebService.GetChucVu(db, IdChucVu);
                        var BoPhan = _CallWebService.GetBoPhan(db, IdBoPhan);
                        var LoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == IdLoaiHopDong);
                        if (LoaiHopDong.Ma == "HĐLĐ.")
                            LoaiHopDong.Ma = "HĐLĐ";
                        string MaChucVu = "";
                        if (ChucVu.MaChucVu == "GS")
                            MaChucVu = ChucVu.MaChucVu + "/";
                        else if (ChucVu.MaChucVu == "CN" || ChucVu.MaChucVu == "CTV")
                            MaChucVu = "ICT/";
                        else
                            MaChucVu = BoPhan.MaBoPhan + "/";
                        if (LoaiHopDong.Ma == "TTHV")
                            MaChucVu = "ICT/";
                        if (!string.IsNullOrEmpty(IdHopDong))
                        {
                            sTT = "/PLHĐLĐ/" + MaChucVu + Nam;
                        }
                        else
                        {
                            sTT = "/" + LoaiHopDong.Ma + "/" + MaChucVu + Nam;
                        }


                        //if (ChucVu.MaChucVu == "CN" && (LoaiHopDong.Ma == "KTHHĐ" || LoaiHopDong.Ma == "HĐLĐ"))
                        //{
                            if (!string.IsNullOrEmpty(IdNhanVien))
                            {

                                var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdNhanVien);
                                if (NhanVien != null)
                                    sTT = NhanVien.SoGen  + sTT;
                            }
                        //}
                        //else
                        {
                            var QuyTrinh = db.HRM_HopDong.Where(a =>a.IdNhanVien==IdNhanVien && !string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong.Contains(sTT)).OrderByDescending(a => a.Created).AsNoTracking().FirstOrDefault();
                            string SoQuyTrinh = "";
                            if (QuyTrinh != null)
                                SoQuyTrinh = QuyTrinh.SoHopDong;
                            int nID = 1;
                            if (!string.IsNullOrEmpty(SoQuyTrinh) && SoQuyTrinh.Contains("/"))
                                nID = int.Parse(SoQuyTrinh.Split('/')[0].ToString() ?? "1") + 1;
                            string s = nID.ToString();
                            while (s.Length < 6)
                            {
                                s = "0" + s;
                            }
                            sTT = s + "/" + sTT;
                        }
                    }

                }
                else if (Host == "PHT")
                {
                    var DonVi = _CallWebService.GetDanhSachDuAn(db).Where(a => a.Id == IdCongTy).FirstOrDefault();

                    sTT = "PHT/";
                    if (!string.IsNullOrEmpty(IdNhanVien))
                    {

                        var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdNhanVien);
                        if (NhanVien != null)
                            sTT += NhanVien.MaNhanVien;
                    }
                    if (!string.IsNullOrEmpty(IdLoaiHopDong))
                    {
                        int Nam = DateTime.Now.Year;
                        if (NgayUnix > 0)
                        {
                            DateTime time = UnixTimeStampToDateTime(NgayUnix);
                            Nam = time.Year;
                        }
                        var LoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == IdLoaiHopDong);
                        if ((LoaiHopDong.Ma == "HĐTV" || LoaiHopDong.Ma == "HĐVV") && !string.IsNullOrEmpty(IdNhanVien))
                        {
                            int LanKy = db.HRM_HopDong.Where(a => a.IdNhanVien == IdNhanVien && a.isKetThuc == true && a.IdLoaiHopDong == LoaiHopDong.IdGuid).Count() + 1;
                            sTT = sTT + "/" + LanKy;

                        }
                        sTT = sTT + "/" + Nam + "/" + LoaiHopDong.Ma;
                        if (DonVi != null)
                            sTT = sTT + "/" + DonVi.MaBoPhan;

                    }
                }
                string sSoPhieu = sTT;
                return sSoPhieu;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public List<ModelTrangThaiViewModel> GetListdmTrangThaiHopDong()
        {
            List<ModelTrangThaiViewModel> listTrangThai = new List<ModelTrangThaiViewModel>();
            ModelTrangThaiViewModel TatCa = new ModelTrangThaiViewModel
            {
                Ma = "",
                Ten = "Tất cả"
            };
            listTrangThai.Add(TatCa);
            ModelTrangThaiViewModel HieuLuc = new ModelTrangThaiViewModel
            {
                Ma = DefineData.eTrangTHaiHopDong.HieuLuc.ToString(),
                Ten = "Hiệu lực"
            };
            listTrangThai.Add(HieuLuc);
            ModelTrangThaiViewModel hethan = new ModelTrangThaiViewModel
            {
                Ma = DefineData.eTrangTHaiHopDong.DaHetHan.ToString(),
                Ten = "Đã hết hạn"
            };
            listTrangThai.Add(hethan);

            ModelTrangThaiViewModel SapHetHan = new ModelTrangThaiViewModel
            {
                Ma = DefineData.eTrangTHaiHopDong.SapHetHan.ToString(),
                Ten = "Sắp hết hạn"
            };
            listTrangThai.Add(SapHetHan);


            ModelTrangThaiViewModel HetHieuLuc = new ModelTrangThaiViewModel
            {
                Ma = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString(),
                Ten = "Hết hiệu lực"
            };
            listTrangThai.Add(HetHieuLuc);

            return listTrangThai;
        }

        public dynamic GetDanhSachQuyTrinhHopDong(TimKiem timKiem, ApplicationDbContext db)
        {
            int CurrentPage = timKiem.CurrentPage ?? 1;
            int PageSize = timKiem.PageSize ?? 20;
            int TabTrangThai = timKiem.TabTrangThai ?? 1;

            IQueryable<HRM_HopDong> result;
            if (timKiem.TabTrangThai != 3)

                result = (from a in db.HRM_HopDong

                          join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                          where b.isDaXuLy == (timKiem.TabTrangThai - 1)
                          && b.IdUser == db.currentUser.Id
                          orderby a.Created descending
                          select a);
            else
                result = from a in db.HRM_HopDong
                         orderby a.Created descending
                         select a;
            if (!string.IsNullOrEmpty(timKiem.IdLoaiHopDong))
            {
                result = result.Where(x => x.IdLoaiHopDong == timKiem.IdLoaiHopDong);
            }

            result = result.Where(x => x.isPhuLuc == timKiem.isPhuLuc);
            DateTime dtTuNgay = UnixTimeStampToDateTime(timKiem.TuNgayUnix ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(timKiem.DenNgayUnix ?? 0);
            if (timKiem.TuNgayUnix > 0)
                result = result.Where(a => a.NgayHieuLuc >= dtTuNgay || (a.NgayChuyenKhongThoiHan != null && a.NgayChuyenKhongThoiHan >= dtTuNgay));
            if (timKiem.DenNgayUnix > 0)
                result = result.Where(a => a.NgayHieuLuc < dtDenNgay.AddDays(1) || (a.NgayChuyenKhongThoiHan != null && a.NgayChuyenKhongThoiHan <= dtDenNgay));
            if (!string.IsNullOrEmpty(timKiem.TenTrangThai))
            {
                DateTime dt = DateTime.Now;
                var VoThoiHan = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.Ma == "KTH");
                if (timKiem.TenTrangThai == DefineData.eTrangTHaiHopDong.DaHetHan.ToString())
                {
                    result = result.Where(a => a.NgayHetHan != null && a.NgayHetHan < dt);
                    if (VoThoiHan != null)
                        result = result.Where(a => a.IdThoiHanHopDong != null && a.IdThoiHanHopDong != VoThoiHan.IdGuid);
                }
                else if (timKiem.TenTrangThai == DefineData.eTrangTHaiHopDong.SapHetHan.ToString())
                {
                    result = result.Where(a => a.NgayHetHan != null && dt >= a.NgayHetHan && a.NgayHetHan <= dt.AddMonths(-1));
                    if (VoThoiHan != null)
                        result = result.Where(a => a.IdThoiHanHopDong != null && a.IdThoiHanHopDong != VoThoiHan.IdGuid);
                }
                else
                    result = result.Where(a => a.TrangThai == timKiem.TenTrangThai);

            }

            result = result.Where(a => a.isPhuLuc == timKiem.isPhuLuc).Distinct();
            if (!string.IsNullOrEmpty(timKiem.KeyWord))
            {
                List<string> IdNhanVien = new List<string>();
                var listNhanVien = db.HRM_NhanVien.Where(a => a.MaNhanVien.ToLower().Contains(timKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(timKiem.KeyWord.ToLower().Trim())).ToList();
                if (listNhanVien != null && listNhanVien.Count > 0)
                {
                    IdNhanVien = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
                }
                result = result.Where(a => IdNhanVien.Contains(a.IdNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong.ToLower().Contains(timKiem.KeyWord.ToLower().Trim())));
            }
            var lstCongTyAll = _CallWebService.GetDanhSachDuAnQuanLyHoSoNhanVien(db);
            if (timKiem.listIddmLoaiDuAn != null && timKiem.listIddmLoaiDuAn.Count > 0)
                lstCongTyAll = lstCongTyAll.Where(a => !string.IsNullOrEmpty(a.IddmLoaiDuAn) && timKiem.listIddmLoaiDuAn.Contains(a.IddmLoaiDuAn)).ToList();
            List<long> listIdDuAn = lstCongTyAll.Select(x => x.Id).ToList();
            var _listBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => x.TableName == "HRM_HopDong" && listIdDuAn.Contains(x.IdBoPhan)).ToList();
            if (timKiem.IdCongTyEOS > 0)
            {
                _listBoPhan = _listBoPhan.Where(x => timKiem.IdCongTyEOS == x.IdBoPhan).ToList();

            }
            if (timKiem.listIdCongTy != null && timKiem.listIdCongTy.Count > 0)
            {
                _listBoPhan = _listBoPhan.Where(x => timKiem.listIdCongTy.Contains(x.IdBoPhan)).ToList();

            }

            var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
            result = result.Where(x => _listIdNhanVien.Contains(x.IdGuid));
            if (timKiem.IdBoPhanEOS > 0)
                result = result.Where(a => a.IdBoPhanEOS == timKiem.IdBoPhanEOS);
            if (timKiem.LoaiLuong > 0)
            {
                result = result.Where(a => a.LoaiLuong == timKiem.LoaiLuong);
            }
            //   if (timKiem.isPhuLuc > 0)
            //{



            var listItem = Paginate(result.AsNoTracking(), CurrentPage, PageSize, out PagingInfor paging);
            var listIdNhanVien = listItem.Select(q => q.IdNhanVien).ToList();
            var NhanViens = db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();

            //var listQuyTrinh_BoPhan_1 = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => listIdNhanVien.Contains(a.IdQuyTrinh)).ToList();
            var dmThoiHanHopDongs = db.HRM_dmThoiHanHopDong.ToList();
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
            DateTime DateNow = DateTime.Now;
            DateTime DateNowMonth = DateNow.AddMonths(1);
            List<string> listid = listItem.Select(a => a.IdGuid).ToList();
            List<HRM_HopDong> lstPhuLucs = db.HRM_HopDong.Where(r => listid.Contains(r.IdHopDong)).ToList();
            foreach (var item in lstPhuLucs)
            {
                var LoaiHopDong = dmLoaiHopDong.FirstOrDefault(r => r.IdGuid == item.IdLoaiHopDong);
                if (LoaiHopDong != null)
                    item.TenLoaiHopDong = LoaiHopDong.Ten;
                var ThoiHanHopDong = dmThoiHanHopDongs.FirstOrDefault(r => r.IdGuid == item.IdThoiHanHopDong);
                if (ThoiHanHopDong != null)
                {
                    item.TenThoiHanHopDong = ThoiHanHopDong.Ten;
                    if (ThoiHanHopDong.Ma != "KTH" && item.NgayHetHan != null && item.NgayHetHan <= DateNow)
                    {
                        item.TrangThai = "Hết Hạn";
                    }
                    if (ThoiHanHopDong.Ma == "KTH")
                        item.TrangThaiHetHan = 0;
                }
            }
            foreach (var item in listItem)
            {

                var NhanVien = NhanViens.FirstOrDefault(r => r.IdGuid == item.IdNhanVien);
                if (NhanVien != null)
                {
                    item.TenNhanVien = NhanVien.TenDayDu;
                    item.MaNhanVien = NhanVien.MaNhanVien;

                }
                if (item.isKetThuc == true)
                {
                    if (item.TrangThai != DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString())
                        item.isDoiTrangThai = true;
                    item.isGiaHan = true;
                }
                else
                    item.TrangThai = DefineData.eTrangTHaiHopDong.ChuaKy.ToString();


                if (item.NgayHetHan != null)
                {
                    if (DateNow > item.NgayHetHan)

                        item.TrangThaiHetHan = 2;
                    if (DateNowMonth > item.NgayHetHan && item.NgayHetHan > DateNow)
                        item.TrangThaiHetHan = 1;
                }
                else
                    item.TrangThaiHetHan = 0;
                var LoaiHopDong = dmLoaiHopDong.FirstOrDefault(r => r.IdGuid == item.IdLoaiHopDong);
                if (LoaiHopDong != null)
                    item.TenLoaiHopDong = LoaiHopDong.Ten;
                var ThoiHanHopDong = dmThoiHanHopDongs.FirstOrDefault(r => r.IdGuid == item.IdThoiHanHopDong);
                if (ThoiHanHopDong != null)
                {
                    item.TenThoiHanHopDong = ThoiHanHopDong.Ten;
                    if (ThoiHanHopDong.Ma != "KTH" && item.NgayHetHan != null && item.NgayHetHan <= DateNow)
                    {
                        item.TrangThai = "Hết Hạn";
                    }
                    if (ThoiHanHopDong.Ma == "KTH")
                        item.TrangThaiHetHan = 0;
                }
                item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
                if (lstPhuLucs != null && lstPhuLucs.Count > 0)
                {
                    List<HRM_HopDong> lstPhuLuc = lstPhuLucs.Where(r => r.IdHopDong == item.IdGuid).ToList();
                    if (lstPhuLuc != null && lstPhuLuc.Count > 0)
                    {
                        foreach (var itemPL in lstPhuLuc)
                        {
                            itemPL.TenNhanVien = item.TenNhanVien;
                            itemPL.MaNhanVien = item.MaNhanVien;
                            //itemPL.TenLoaiHopDong = item.TenLoaiHopDong;
                            //itemPL.TenThoiHanHopDong = item.TenThoiHanHopDong;


                            itemPL.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayHetHan);
                            itemPL.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayHieuLuc);
                            itemPL.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayKy);

                            if (itemPL.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString())
                                itemPL.TrangThai = "Hiệu lực";
                            if (itemPL.TrangThai == DefineData.eTrangTHaiHopDong.ChuaKy.ToString())
                                itemPL.TrangThai = "Chưa ký";
                            if (itemPL.TrangThai == DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString())
                                itemPL.TrangThai = "Hết hiệu lực";

                        }
                        item.listPhuLucHopDong = lstPhuLuc;
                    }
                }
                if (item.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString())
                    item.TrangThai = "Hiệu lực";
                if (item.TrangThai == DefineData.eTrangTHaiHopDong.ChuaKy.ToString())
                    item.TrangThai = "Chưa ký";

                if (item.TrangThai == DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString())
                    item.TrangThai = "Hết hiệu lực";

            }
            dynamic dnm = new DynamicObject();
            dnm.paging = paging;
            dnm.items = listItem;
            return dnm;
        }
        public dynamic GetDanhSachHopDong(EntityGetDanhSachHopDongRequest aRequest, ApplicationDbContext db)
        {
            try
            {
                var listDanhSachHopDong = (from a in db.HRM_HopDong
                                           join b in db.HRM_NhanVien on a.IdNhanVien equals b.IdGuid
                                           where (aRequest.isPhuLuc == false || a.isPhuLuc == aRequest.isPhuLuc) && (aRequest.KeyWord == "" || aRequest.KeyWord == null || a.SoHopDong.Contains(aRequest.KeyWord) || b.TenDayDu.Contains(aRequest.KeyWord))
                                           select a).AsEnumerable();
                if (aRequest.IdViTriChuyenMon.IsNotEmpty())
                {
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.IdViTriChuyenMon == aRequest.IdViTriChuyenMon);
                }

                if (aRequest.TuNgayUnix > 0)
                {
                    var tuNgay = UnixTimeStampToDateTime(aRequest.TuNgayUnix);
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.NgayHieuLuc >= tuNgay);
                }

                if (aRequest.DenNgayUnix > 0)
                {
                    var denNgay = UnixTimeStampToDateTime(aRequest.DenNgayUnix);
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.NgayHetHan >= denNgay);
                }

                if (!string.IsNullOrEmpty(aRequest.IdLoaiHopDong))
                {
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.IdLoaiHopDong == aRequest.IdLoaiHopDong);
                }



                if (aRequest.DienThoai.IsNotEmpty())
                {
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.DienThoai == aRequest.DienThoai);
                }

                if (aRequest.IdChucDanh.IsNotEmpty())
                {
                    //  listDanhSachHopDong = listDanhSachHopDong.Where(x => x.IdChucDanh == aRequest.IdChucDanh);
                }

                if (!string.IsNullOrEmpty(aRequest.IdThoihanHopDong))
                {
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.IdThoiHanHopDong == aRequest.IdThoihanHopDong);
                }

                if (!string.IsNullOrEmpty(aRequest.DiaDiemLamViec))
                {
                    listDanhSachHopDong = listDanhSachHopDong.Where(x => x.DiaDiemLamViec.ToLower().Contains(aRequest.DiaDiemLamViec.ToLower()));
                }
                int count = listDanhSachHopDong.Count();
                var listDanhSachHopDongs = listDanhSachHopDong.OrderByDescending(r => r.NgayHieuLuc).Skip(aRequest.NumperPage * (aRequest.CurrentPage - 1)).Take(aRequest.NumperPage).ToList();

                var listIdNhanVien = listDanhSachHopDong.Select(q => q.IdNhanVien).ToList();
                var NhanViens = db.HRM_NhanVien.Where(r => listIdNhanVien.Contains(r.IdGuid)).ToList();
                var dmThoiHanHopDongs = db.HRM_dmThoiHanHopDong.ToList();
                var dmLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                foreach (var item in listDanhSachHopDongs)
                {
                    List<HRM_HopDong> lstPhuLuc = db.HRM_HopDong.Where(r => r.IdHopDong == item.IdGuid).ToList();
                    var NhanVien = NhanViens.FirstOrDefault(r => r.IdGuid == item.IdNhanVien);
                    if (NhanVien != null)
                    {
                        item.TenNhanVien = NhanVien.TenDayDu;
                        item.MaNhanVien = NhanVien.MaNhanVien;

                    }
                    var LoaiHopDong = dmLoaiHopDong.FirstOrDefault(r => r.IdGuid == item.IdLoaiHopDong);
                    if (LoaiHopDong != null)
                        item.TenLoaiHopDong = LoaiHopDong.Ten;
                    var ThoiHanHopDong = dmThoiHanHopDongs.FirstOrDefault(r => r.IdGuid == item.IdThoiHanHopDong);
                    if (ThoiHanHopDong != null)
                        item.TenThoiHanHopDong = ThoiHanHopDong.Ten;
                    item.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHetHan);
                    item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                    item.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKy);
                    foreach (var itemPL in lstPhuLuc)
                    {
                        itemPL.TenNhanVien = item.TenNhanVien;
                        itemPL.MaNhanVien = item.MaNhanVien;
                        itemPL.TenLoaiHopDong = item.TenLoaiHopDong;
                        itemPL.TenThoiHanHopDong = item.TenThoiHanHopDong;

                        itemPL.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayHetHan);
                        itemPL.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayHieuLuc);
                        itemPL.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(itemPL.NgayKy);
                    }
                    item.listPhuLucHopDong = lstPhuLuc;
                }

                dynamic dyn = new DynamicObject();
                dyn.paging = new PagingInfor(aRequest.CurrentPage, count, count / aRequest.NumperPage + (count % aRequest.NumperPage > 0 ? 1 : 0));
                dyn.items = listDanhSachHopDongs;
                //   dyn.value = lstBoPhanAll;
                return dyn;
            }
            catch (Exception)
            {
                dynamic dyn = new DynamicObject();
                return dyn;
            }
        }

        public List<HRM_HopDong> GetDanhSachHopDongHieuLucNhanVien(List<string> lstIdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //string TrangThaiHieuLuc = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            return db.HRM_HopDong.Where(x => string.IsNullOrEmpty(x.IdHopDong) && x.isKetThuc == true && lstIdNhanVien.Contains(x.IdNhanVien)).OrderByDescending(a => a.Modified).ToList();
        }
        public HRM_HopDong GetHopDongHieuLucNhanVien(string IdNhanVien, string idCurrentUser = "", ApplicationDbContext db = null)
        {
            //string TrangThaiHieuLuc = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            return db.HRM_HopDong.Where(x => x.isKetThuc == true && x.IdNhanVien == IdNhanVien).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a=>a.Modified).FirstOrDefault();
        }

        public MessageReturn SetHopDong(HRM_HopDong aModel, ApplicationDbContext dbContext)
        {
            //1 công ty chỉ có 1 hợp đồng active: IdBoPhanEOS, IdCongTyEOS, isDeactive
            //phụ lục hợp đồng: idHopDongParent, idHopDongGoc, LoaiHopDongPhuLuc
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu thành công");
            using (var ts = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var check = dbContext.HRM_HopDong.Where(a => a.SoHopDong == aModel.SoHopDong && a.IdGuid != aModel.IdGuid).FirstOrDefault();
                    if (check != null)
                    {
                        err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                        return err;
                    }
                    var QuyTrinhdb = dbContext.HRM_HopDong.AsNoTracking().FirstOrDefault(a => a.IdGuid == aModel.IdGuid);
                    if (QuyTrinhdb != null && QuyTrinhdb.IdTrangThai != aModel.IdTrangThai)
                    {
                        ChuyenTrangThai audit = new ChuyenTrangThai()
                        {
                            IdGuid = aModel.IdGuid,
                            eAction = DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(),
                            IdTrangThai = aModel.IdTrangThai,
                        };
                        var trangThai = _CallWebService.DoiTrangThaiKhoiTao(audit, dbContext);
                    }
                    if (string.IsNullOrEmpty(aModel.SoHopDong))
                    {
                        long IdCongTy = 0;
                        if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                            IdCongTy = aModel.listIdCongTy.FirstOrDefault();
                        aModel.SoHopDong = GetMaHopDong(dbContext, aModel.IdLoaiHopDong, aModel.IdNhanVien, aModel.IdHopDong, aModel.IddmChucVu, aModel.IdBoPhanEOS, IdCongTy);
                    }

                    if (aModel.NgayHieuLucUnix == 0)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Vui lòng nhập ngày hiệu lực hợp đồng");
                    }
                    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới hợp đồng thành công");

                    //if (dbContext.currentUser.Id.IsEmpty())
                    //{
                    //    ts.Rollback();
                    //    return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
                    //}

                    if (aModel == null)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy dữ liệu");
                    }

                    var update = FuncUpdateHopDong(aModel, dbContext, err, eQuyTrinh_LoaiHam.Set);
                    if (update == false)
                    {
                        ts.Rollback();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    }






                    dbContext.SaveChanges();
                    ts.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", aModel.IdGuid);

                }
                catch (Exception ex)
                {

                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public MessageReturn SetHopDongNoTransaction(HRM_HopDong aModel, ApplicationDbContext dbContext)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
            try
            {
                //var check = dbContext.HRM_HopDong.Where(a => a.SoHopDong == aModel.SoHopDong && a.IdGuid != aModel.IdGuid).FirstOrDefault();
                //if (check != null)
                //{
                //    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                //    return err;
                //}
                var update = FuncUpdateHopDong(aModel, dbContext, err, eQuyTrinh_LoaiHam.Set);
                if (update == false)
                {

                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                }







                dbContext.SaveChanges();

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", aModel.IdGuid);

            }
            catch (Exception ex)
            {


                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }

        }
        public void HopDongCongViecToList(List<ReturnData<HRM_HopDongCongViec>> listPhanKhu, List<HRM_HopDongCongViec> ListAdd)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();

                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                {
                    Parallel.ForEach(item.children, child =>
                    {
                        child.data.IdParent = item.data.IdGuid;
                    });
                    HopDongCongViecToList(item.children, ListAdd);
                }
            }
        }

        public void HopDongCongViecToTree(List<HRM_HopDongCongViec> ListAll, ReturnData<HRM_HopDongCongViec> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_HopDongCongViec>
            {
                data = x
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    HopDongCongViecToTree(ListAll, item);
                }
            }

        }
        public bool FuncUpdateHopDong(HRM_HopDong aModel, ApplicationDbContext dbContext, MessageReturn err, eQuyTrinh_LoaiHam eLoaiHam)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới hợp đồng thành công");
            try
            {
                var check = dbContext.HRM_HopDong.Where(a => !string.IsNullOrEmpty(aModel.SoHopDong) && a.SoHopDong == aModel.SoHopDong && a.IdGuid != aModel.IdGuid).FirstOrDefault();
                if (check != null)
                {
                    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                    return false;
                }
                if (aModel.listIdCaLamViec != null && aModel.listIdCaLamViec.Count > 0)
                    aModel.IdThoiGianLamViec = string.Join(",", aModel.listIdCaLamViec);
                if (!string.IsNullOrEmpty(aModel.IdHopDong))
                    aModel.isPhuLuc = true;

                if (aModel.NgayKyUnix > 0)
                    aModel.NgayKy = UnixTimeStampToDateTime(aModel.NgayKyUnix);
                if (aModel.NgayChuyenKhongThoiHanUnix > 0)
                    aModel.NgayChuyenKhongThoiHan = UnixTimeStampToDateTime(aModel.NgayChuyenKhongThoiHanUnix);

                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                if (aModel.NgayHetHanUnix > 0)
                    aModel.NgayHetHan = UnixTimeStampToDateTime(aModel.NgayHetHanUnix);
                if (aModel.IdThoiHanHopDong != null && aModel.NgayHetHanUnix == 0)
                {
                    var dmThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.Find(aModel.IdThoiHanHopDong);
                    if (aModel.NgayHetHan == null && dmThoiHanHopDong.SoThang != 0)
                        aModel.NgayHetHan = aModel.NgayHieuLuc.AddMonths(dmThoiHanHopDong.SoThang).AddDays(-1);
                    else
                    {
                        aModel.NgayHetHan = UnixTimeStampToDateTime(0);
                    }
                }

                if (aModel.NgaySinhUnix > 0)
                    aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
                if (aModel.ChungMinhThuNgayCapUnix > 0)
                    aModel.ChungMinhThuNgayCap = UnixTimeStampToDateTime(aModel.ChungMinhThuNgayCapUnix);
                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                var listFileCreate = new List<EntityAddHRMFileDinhKemModel>();
                if (string.IsNullOrEmpty(aModel.IdGuid))
                {

                    aModel.IdGuid = Guid.NewGuid().ToString();
                    long IdCongTy = 0;
                    if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                        IdCongTy = aModel.listIdCongTy.FirstOrDefault();
                    if (string.IsNullOrEmpty(aModel.SoHopDong))
                        aModel.SoHopDong = GetMaHopDong(dbContext, aModel.IdLoaiHopDong, aModel.IdNhanVien, aModel.IdHopDong, aModel.IddmChucVu, aModel.IdBoPhanEOS, IdCongTy, aModel.NgayKyUnix);

                    aModel.TrangThai = DefineData.eTrangTHaiHopDong.ChuaKy.ToString();//Khi  quy trình hợp đồng thì coi như chưa ký
                    var trangThai = _CallWebService.GetTrangThaiKhoiTao(dbContext, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.IdGuid);
                    if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                    {

                        err = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                        return false;
                    }

                    aModel.IdTrangThai = trangThai.IdTrangThai;
                    aModel.TenTrangThai = trangThai.TenTrangThai;
                    dbContext.HRM_HopDong.Add(aModel);
                    result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới dữ liệu hợp đồng thành công");
                }
                else
                {
                    if (aModel.isKetThuc == true)
                    {
                        aModel.TrangThai = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
                        LogLichSuNhanVien(dbContext, aModel.IdNhanVien, aModel.IdGuid, "HRM_HopDong", DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.NgayKyUnix);

                    }
                    dbContext.Entry(aModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                }
                var ListBoPhan = dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == aModel.IdGuid).ToList();
                dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                    foreach (var item in aModel.listIdCongTy)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = aModel.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_HopDong);
                        dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }

                //if (aModel.lstFileDinhKem.IsNotEmpty())
                //{
                FileUploadController.SetFileDinhKem(dbContext, aModel.lstFileDinhKem, nameof(HRM_HopDong).ToString(), aModel.IdGuid);

                //foreach (var item in aModel.lstFileDinhKem)
                //{
                //    if (string.IsNullOrEmpty(item.IdGuid))
                //    {
                //        listFileCreate.Add(new EntityAddHRMFileDinhKemModel(item, aModel.IdGuid, nameof(HRM_HopDong), nameof(DefineData.eFlagDinhKem.FileDinhKem)));
                //    }
                //    else
                //    {
                //        dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //        if (item.isDeleted == true)
                //        {
                //            dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                //        }
                //    }

                //}
                //}
                //if (aModel.listFileMoTaCongViec.IsNotEmpty())
                //{
                FileUploadController.SetFileDinhKem(dbContext, aModel.listFileMoTaCongViec, nameof(HRM_HopDongCongViec).ToString(), aModel.IdGuid);
                //foreach (var item in aModel.listFileMoTaCongViec)
                //{
                //    if (string.IsNullOrEmpty(item.IdGuid))
                //    {
                //        listFileCreate.Add(new EntityAddHRMFileDinhKemModel(item, aModel.IdGuid, nameof(HRM_HopDongCongViec), nameof(DefineData.eFlagDinhKem.FileDinhKem)));
                //    }
                //    else
                //    {
                //        dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //        if (item.isDeleted == true)
                //        {
                //            dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                //        }
                //    }

                //}
                //}

                var listPhuCapOld = dbContext.HRM_HopDongPhuCap.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listPhuCapOld != null && listPhuCapOld.Count > 0)
                    dbContext.HRM_HopDongPhuCap.RemoveRange(listPhuCapOld);
                if (aModel.lstPhuCap.IsNotEmpty())
                {
                    foreach (var item in aModel.lstPhuCap)
                    {
                        if (item.NgayKetThucUnix > 0)
                            item.NgayKetThuc = UnixTimeStampToDateTime(item.NgayKetThucUnix);
                        if (item.NgayBatDauUnix > 0)
                            item.NgayBatDau = UnixTimeStampToDateTime(item.NgayBatDauUnix);
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        dbContext.HRM_HopDongPhuCap.Add(item);

                    }
                }
                var lstPhuCapBaoHiem = dbContext.HRM_HopDongPhuCapBaoHiem.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (lstPhuCapBaoHiem != null && lstPhuCapBaoHiem.Count > 0)
                    dbContext.HRM_HopDongPhuCapBaoHiem.RemoveRange(lstPhuCapBaoHiem);
                if (aModel.lstPhuCapBaoHiem.IsNotEmpty())
                {
                    foreach (var item in aModel.lstPhuCapBaoHiem)
                    {

                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        dbContext.HRM_HopDongPhuCapBaoHiem.Add(item);

                    }
                }
                var listTrangThietBi = dbContext.HRM_HopDongTaiSanBanGiao.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listTrangThietBi != null && listTrangThietBi.Count > 0)
                    dbContext.HRM_HopDongTaiSanBanGiao.RemoveRange(listTrangThietBi);
                if (aModel.listTrangThietBi != null)
                {
                    foreach (var item in aModel.listTrangThietBi)
                    {
                        if (item.NgayBanGiaoUnix > 0)
                            item.NgayBanGiao = dungchung.UnixTimeStampToDateTime(item.NgayBanGiaoUnix);
                        if (item.NgayHoanTraUnix > 0)
                            item.NgayHoanTra = dungchung.UnixTimeStampToDateTime(item.NgayHoanTraUnix);
                        item.IdHopDong = aModel.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        dbContext.HRM_HopDongTaiSanBanGiao.Add(item);
                    }
                }
                var listMoTaCongViec = dbContext.HRM_HopDongCongViec.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listMoTaCongViec != null && listMoTaCongViec.Count > 0)
                    dbContext.HRM_HopDongCongViec.RemoveRange(listMoTaCongViec);
                if (aModel.listMoTaCongViec != null)
                {
                    List<HRM_HopDongCongViec> listCV = new List<HRM_HopDongCongViec>();
                    HopDongCongViecToList(aModel.listMoTaCongViec, listCV);
                    foreach (var item in listCV)
                    {
                        item.IdHopDong = aModel.IdGuid;
                        //item.IdGuid = Guid.NewGuid().ToString();
                        //dbContext.HRM_HopDongCongViec.Add(item);
                        if (item.listFileDinhKem != null && item.listFileDinhKem.Count > 0)
                            FileUploadController.SetFileDinhKem(dbContext, item.listFileDinhKem, nameof(HRM_HopDongCongViec).ToString(), item.IdGuid);

                    }
                    dbContext.HRM_HopDongCongViec.AddRange(listCV);
                }
                var LuongHopDong = dbContext.HRM_Luong_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (LuongHopDong != null && LuongHopDong.Count > 0)
                    dbContext.HRM_Luong_HopDong.RemoveRange(LuongHopDong);
                if (aModel.LuongHopDong != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongHopDong;
                    if (item.NgayBatDauUnix > 0)
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                    if (item.NgayKetThucUnix > 0)
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_Luong_HopDong.Add(item);
                }
                var LuongBaoHiem = dbContext.HRM_LuongBaoHiem_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (LuongBaoHiem != null && LuongBaoHiem.Count > 0)
                    dbContext.HRM_LuongBaoHiem_HopDong.RemoveRange(LuongBaoHiem);
                if (aModel.LuongBaoHiem != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongBaoHiem;
                    if (item.NgayBatDauUnix > 0)
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);


                    if (item.NgayKetThucUnix > 0)
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_LuongBaoHiem_HopDong.Add(item);
                }
                var BaoHiem = dbContext.HRM_BaoHiem_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (BaoHiem != null && BaoHiem.Count > 0)
                    dbContext.HRM_BaoHiem_HopDong.RemoveRange(BaoHiem);
                if (aModel.BaoHiem != null)
                {

                    aModel.BaoHiem.IdGuid = Guid.NewGuid().ToString();
                    aModel.BaoHiem.IdHopDong = aModel.IdGuid;
                    if (aModel.BaoHiem.NgayBatDauDongBHXHUnix > 0)
                        aModel.BaoHiem.NgayBatDauDongBHXH = dungchung.UnixTimeStampToDateTime(aModel.BaoHiem.NgayBatDauDongBHXHUnix);
                    if (aModel.BaoHiem.NgayBatDauUnix > 0)
                    {
                        aModel.BaoHiem.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiem.NgayBatDauUnix);
                    }
                    if (aModel.BaoHiem.NgayKetThucUnix > 0)
                    {
                        aModel.BaoHiem.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiem.NgayKetThucUnix);
                    }
                    if (aModel.BaoHiem.NgayCapSoBaoHiemUnix > 0)
                    {
                        aModel.BaoHiem.NgayCapSoBaoHiem = UnixTimeStampToDateTime(aModel.BaoHiem.NgayCapSoBaoHiemUnix);
                    }
                    dbContext.HRM_BaoHiem_HopDong.Add(aModel.BaoHiem);
                }
                if (aModel.BaoHiemNgoai != null)
                {

                    aModel.BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();
                    aModel.BaoHiemNgoai.IdHopDong = aModel.IdGuid;
                    if (aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix > 0)
                        aModel.BaoHiemNgoai.NgayBatDauDongBHXH = dungchung.UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix);
                    if (aModel.BaoHiemNgoai.NgayBatDauUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauUnix);
                    }
                    if (aModel.BaoHiemNgoai.NgayKetThucUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayKetThucUnix);
                    }
                    if (aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayCapSoBaoHiem = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix);
                    }
                    aModel.BaoHiemNgoai.isNgoai = true;
                    dbContext.HRM_BaoHiem_HopDong.Add(aModel.BaoHiemNgoai);
                }
                if (aModel.listKhamSucKhoe != null)
                {
                    foreach (var item in aModel.listKhamSucKhoe)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        item.IdNhanVien = aModel.IdNhanVien;
                        if (item.NgayKhamUnix > 0)
                            item.NgayKham = dungchung.UnixTimeStampToDateTime(item.NgayKhamUnix);
                        if (item.NgayKetThucUnix > 0)
                            item.NgayKetThuc = UnixTimeStampToDateTime(item.NgayKetThucUnix);
                    }
                    dbContext.HRM_HopDong_KhamSucKhoe.AddRange(aModel.listKhamSucKhoe);
                }
                if (listFileCreate.IsNotEmpty())
                {
                    foreach (var item in listFileCreate)
                    {
                        if (item.ListFile.IsNotEmpty())
                        {
                            foreach (var file in item.ListFile)
                            {
                                result = SetFileDinhKem(dbContext, file, item.NameTable, item.IdTable, result);
                                if (result.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                                {

                                    return false;
                                }
                            }
                        }
                    }
                }

                if (aModel.isKetThuc == true)
                {

                }
                dbContext.SaveChanges();
                if (!SetQuyTrinh(dbContext, _CallWebService, aModel, eLoaiHam, err))
                    return err.Value;
                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }
        }
        public bool FuncUpdateHopDongBuocCuoi(HRM_HopDong aModel, ApplicationDbContext dbContext, MessageReturn err = null)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới hợp đồng thành công");
            try
            {
                var check = dbContext.HRM_HopDong.Where(a => a.SoHopDong == aModel.SoHopDong && a.IdGuid != aModel.IdGuid).FirstOrDefault();
                if (check != null)
                {
                    err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                    return false;
                }
                if (aModel.listIdCaLamViec != null && aModel.listIdCaLamViec.Count > 0)
                    aModel.IdThoiGianLamViec = string.Join(",", aModel.listIdCaLamViec);
                if (!string.IsNullOrEmpty(aModel.IdHopDong))
                    aModel.isPhuLuc = true;

                if (aModel.NgayKyUnix > 0)
                    aModel.NgayKy = UnixTimeStampToDateTime(aModel.NgayKyUnix);
                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                if (aModel.NgayHetHanUnix > 0)
                    aModel.NgayHetHan = UnixTimeStampToDateTime(aModel.NgayHetHanUnix);
                if (aModel.IdThoiHanHopDong != null && aModel.NgayHetHanUnix == 0)
                {
                    var dmThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.Find(aModel.IdThoiHanHopDong);
                    if (aModel.NgayHetHan == null && dmThoiHanHopDong.SoThang != 0)
                        aModel.NgayHetHan = aModel.NgayHieuLuc.AddMonths(dmThoiHanHopDong.SoThang).AddDays(-1);
                    else
                    {
                        aModel.NgayHetHan = UnixTimeStampToDateTime(0);
                    }
                }

                if (aModel.NgaySinhUnix > 0)
                    aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
                if (aModel.ChungMinhThuNgayCapUnix > 0)
                    aModel.ChungMinhThuNgayCap = UnixTimeStampToDateTime(aModel.ChungMinhThuNgayCapUnix);
                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                var listFileCreate = new List<EntityAddHRMFileDinhKemModel>();

                aModel.IdGuid = Guid.NewGuid().ToString();
                long IdCongTy = 0;
                if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                    IdCongTy = aModel.listIdCongTy.FirstOrDefault();
                if (string.IsNullOrEmpty(aModel.SoHopDong))
                    aModel.SoHopDong = GetMaHopDong(dbContext, aModel.IdLoaiHopDong, aModel.IdNhanVien, aModel.IdHopDong, aModel.IddmChucVu, aModel.IdBoPhanEOS, IdCongTy, aModel.NgayKyUnix);

                //aModel.TrangThai = DefineData.eTrangTHaiHopDong.ChuaKy.ToString();//Khi  quy trình hợp đồng thì coi như chưa ký
                var trangThai = _CallWebService.GetTrangThaiKhoiTaoKetThuc(dbContext, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.IdGuid, aModel.SoHopDong);
                if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                {

                    err = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy trạng thái dữ liệu khi chuyển");
                    return false;
                }
                if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc || trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.XuLyDacBiet)
                    aModel.isKetThuc = true;
                aModel.IdTrangThai = trangThai.IdTrangThai;
                aModel.TenTrangThai = trangThai.TenTrangThai;

                dbContext.HRM_HopDong.Add(aModel);


                var ListBoPhan = dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == aModel.IdGuid).ToList();
                dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                    foreach (var item in aModel.listIdCongTy)
                    {
                        HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                        bp.IdGuid = Guid.NewGuid().ToString();
                        bp.IdQuyTrinh = aModel.IdGuid;
                        bp.IdBoPhan = item;
                        bp.TableName = nameof(HRM_HopDong);
                        dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                    }

                if (aModel.lstFileDinhKem.IsNotEmpty())
                {
                    foreach (var item in aModel.lstFileDinhKem)
                    {
                        if (string.IsNullOrEmpty(item.IdGuid))
                        {
                            listFileCreate.Add(new EntityAddHRMFileDinhKemModel(item, aModel.IdGuid, nameof(HRM_HopDong), nameof(DefineData.eFlagDinhKem.FileDinhKem)));
                        }
                        else
                        {
                            dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            if (item.isDeleted == true)
                            {
                                dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                            }
                        }

                    }
                }
                var listPhuCapOld = dbContext.HRM_HopDongPhuCap.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listPhuCapOld != null && listPhuCapOld.Count > 0)
                    dbContext.HRM_HopDongPhuCap.RemoveRange(listPhuCapOld);
                if (aModel.lstPhuCap.IsNotEmpty())
                {
                    foreach (var item in aModel.lstPhuCap)
                    {

                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        dbContext.HRM_HopDongPhuCap.Add(item);

                    }
                }
                var lstPhuCapBaoHiem = dbContext.HRM_HopDongPhuCapBaoHiem.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (lstPhuCapBaoHiem != null && lstPhuCapBaoHiem.Count > 0)
                    dbContext.HRM_HopDongPhuCapBaoHiem.RemoveRange(lstPhuCapBaoHiem);
                if (aModel.lstPhuCapBaoHiem.IsNotEmpty())
                {
                    foreach (var item in aModel.lstPhuCapBaoHiem)
                    {

                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        dbContext.HRM_HopDongPhuCapBaoHiem.Add(item);

                    }
                }
                var listTrangThietBi = dbContext.HRM_HopDongTaiSanBanGiao.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listTrangThietBi != null && listTrangThietBi.Count > 0)
                    dbContext.HRM_HopDongTaiSanBanGiao.RemoveRange(listTrangThietBi);
                if (aModel.listTrangThietBi != null)
                {
                    foreach (var item in aModel.listTrangThietBi)
                    {
                        if (item.NgayBanGiaoUnix > 0)
                            item.NgayBanGiao = dungchung.UnixTimeStampToDateTime(item.NgayBanGiaoUnix);
                        if (item.NgayHoanTraUnix > 0)
                            item.NgayHoanTra = dungchung.UnixTimeStampToDateTime(item.NgayHoanTraUnix);
                        item.IdHopDong = aModel.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        dbContext.HRM_HopDongTaiSanBanGiao.Add(item);
                    }
                }
                var listMoTaCongViec = dbContext.HRM_HopDongCongViec.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listMoTaCongViec != null && listMoTaCongViec.Count > 0)
                    dbContext.HRM_HopDongCongViec.RemoveRange(listMoTaCongViec);
                if (aModel.listMoTaCongViec != null)
                {
                    List<HRM_HopDongCongViec> listCV = new List<HRM_HopDongCongViec>();
                    HopDongCongViecToList(aModel.listMoTaCongViec, listCV);
                    foreach (var item in listCV)
                    {
                        item.IdHopDong = aModel.IdGuid;
                        //item.IdGuid = Guid.NewGuid().ToString();
                        dbContext.HRM_HopDongCongViec.Add(item);
                        if (item.listFileDinhKem != null && item.listFileDinhKem.Count > 0)
                            FileUploadController.SetFileDinhKem(dbContext, item.listFileDinhKem, nameof(HRM_HopDongCongViec).ToString(), item.IdGuid);

                    }
                    dbContext.HRM_HopDongCongViec.AddRange(listCV);
                }
                var LuongHopDong = dbContext.HRM_Luong_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (LuongHopDong != null && LuongHopDong.Count > 0)
                    dbContext.HRM_Luong_HopDong.RemoveRange(LuongHopDong);
                if (aModel.LuongHopDong != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongHopDong;
                    if (item.NgayBatDauUnix > 0)
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                    if (item.NgayKetThucUnix > 0)
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_Luong_HopDong.Add(item);
                }
                var LuongBaoHiem = dbContext.HRM_LuongBaoHiem_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (LuongBaoHiem != null && LuongBaoHiem.Count > 0)
                    dbContext.HRM_LuongBaoHiem_HopDong.RemoveRange(LuongBaoHiem);
                if (aModel.LuongBaoHiem != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongBaoHiem;
                    if (item.NgayBatDauUnix > 0)
                        item.NgayBatDau = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);


                    if (item.NgayKetThucUnix > 0)
                        item.NgayKetThuc = dungchung.UnixTimeStampToDateTime(item.NgayKetThucUnix);
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_LuongBaoHiem_HopDong.Add(item);
                }
                var BaoHiem = dbContext.HRM_BaoHiem_HopDong.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (BaoHiem != null && BaoHiem.Count > 0)
                    dbContext.HRM_BaoHiem_HopDong.RemoveRange(BaoHiem);
                if (aModel.BaoHiem != null)
                {

                    aModel.BaoHiem.IdGuid = Guid.NewGuid().ToString();
                    aModel.BaoHiem.IdHopDong = aModel.IdGuid;
                    if (aModel.BaoHiem.NgayBatDauDongBHXHUnix > 0)
                        aModel.BaoHiem.NgayBatDauDongBHXH = dungchung.UnixTimeStampToDateTime(aModel.BaoHiem.NgayBatDauDongBHXHUnix);
                    if (aModel.BaoHiem.NgayBatDauUnix > 0)
                    {
                        aModel.BaoHiem.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiem.NgayBatDauUnix);
                    }
                    if (aModel.BaoHiem.NgayKetThucUnix > 0)
                    {
                        aModel.BaoHiem.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiem.NgayKetThucUnix);
                    }
                    if (aModel.BaoHiem.NgayCapSoBaoHiemUnix > 0)
                    {
                        aModel.BaoHiem.NgayCapSoBaoHiem = UnixTimeStampToDateTime(aModel.BaoHiem.NgayCapSoBaoHiemUnix);
                    }
                    dbContext.HRM_BaoHiem_HopDong.Add(aModel.BaoHiem);
                }
                if (aModel.BaoHiemNgoai != null)
                {

                    aModel.BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();
                    aModel.BaoHiemNgoai.IdHopDong = aModel.IdGuid;
                    if (aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix > 0)
                        aModel.BaoHiemNgoai.NgayBatDauDongBHXH = dungchung.UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauDongBHXHUnix);
                    if (aModel.BaoHiemNgoai.NgayBatDauUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayBatDau = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayBatDauUnix);
                    }
                    if (aModel.BaoHiemNgoai.NgayKetThucUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayKetThuc = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayKetThucUnix);
                    }
                    if (aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix > 0)
                    {
                        aModel.BaoHiemNgoai.NgayCapSoBaoHiem = UnixTimeStampToDateTime(aModel.BaoHiemNgoai.NgayCapSoBaoHiemUnix);
                    }
                    aModel.BaoHiemNgoai.isNgoai = true;
                    dbContext.HRM_BaoHiem_HopDong.Add(aModel.BaoHiemNgoai);
                }

                if (listFileCreate.IsNotEmpty())
                {
                    foreach (var item in listFileCreate)
                    {
                        if (item.ListFile.IsNotEmpty())
                        {
                            foreach (var file in item.ListFile)
                            {
                                result = SetFileDinhKem(dbContext, file, item.NameTable, item.IdTable, result);
                                if (result.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                                {

                                    return false;
                                }
                            }
                        }
                    }
                }



                if (aModel.isKetThuc == true)
                {
                    aModel.TrangThai = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
                    LogLichSuNhanVien(dbContext, aModel.IdNhanVien, aModel.IdGuid, "HRM_HopDong", DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.NgayKyUnix);
                    var listHopDongCuHieuLuc = dbContext.HRM_HopDong.Where(a => a.isKetThuc == true && a.IdNhanVien == aModel.IdNhanVien && a.IdGuid != aModel.IdGuid).ToList();
                    if (listHopDongCuHieuLuc != null && listHopDongCuHieuLuc.Count > 0 && string.IsNullOrEmpty(aModel.IdHopDong))
                    {
                        foreach (var item in listHopDongCuHieuLuc)
                        {
                            item.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                            dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }

                    }
                    var listDonViOld = dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == aModel.IdNhanVien).ToList();
                    if (listDonViOld != null && listDonViOld.Count > 0)
                        dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(listDonViOld);
                    if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                    {
                        List<HRM_QuyTrinhBoPhan_BoPhan> listDonVi = new List<HRM_QuyTrinhBoPhan_BoPhan>();

                        foreach (var Id in aModel.listIdCongTy)
                        {
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = aModel.IdNhanVien;
                            bp.IdBoPhan = Id;
                            bp.TableName = nameof(HRM_NhanVien);
                            listDonVi.Add(bp);
                        }
                        dbContext.HRM_QuyTrinhBoPhan_BoPhan.AddRange(listDonVi);
                    }


                    HRM_NhanVien nhanVien = dbContext.HRM_NhanVien.Find(aModel.IdNhanVien);
                    var LoaiHopDong = dbContext.HRM_dmLoaiHopDong.FirstOrDefault(a => (a.IdGuid == aModel.IdLoaiHopDong) && (a.Ma == "HĐLĐ" || a.Ma == "HĐLĐ." || a.Ma == "KTHHĐ" || a.Ma == "THHĐ" || a.Ma == "HĐL"));
                    if (LoaiHopDong != null)
                    {

                        if (nhanVien != null && nhanVien.NgayBatDauChinhThuc == null)
                        {
                            nhanVien.NgayBatDauChinhThuc = aModel.NgayHieuLuc;
                        }
                    }
                    if (nhanVien != null && nhanVien.NgayBatDauLamViec == null)
                    {
                        nhanVien.NgayBatDauLamViec = aModel.NgayHieuLuc;
                    }
                    nhanVien.SoNgayNghiPhepThamNien = aModel.SoNgayNghiPhepThamNien;
                    nhanVien.SoNgayNghiPhep = aModel.SoNgayNghiPhep;
                    if (aModel.BaoHiem != null)
                        nhanVien.isThamGiaBHXH = aModel.BaoHiem.isBaoHiemXaHoi;
                    if (aModel.BaoHiemNgoai != null)
                        nhanVien.GiamBaoHiem = aModel.BaoHiemNgoai.Giam;
                    if (nhanVien.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                    {
                        nhanVien.NgayQuayLai = DateTime.Now;
                        nhanVien.NgayNghiViec = null;
                        nhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                    }
                    if (aModel.IdLoaiHopDong != null && aModel.IdLoaiHopDong != "")
                    {
                        try
                        {
                            var dmLoaiHopDong = dbContext.HRM_dmLoaiHopDong.Find(aModel.IdLoaiHopDong);
                            if (nhanVien.NgayBatDauLamViec == null)
                            {
                                if (dmLoaiHopDong.Ma == "HĐLĐ" || dmLoaiHopDong.Ma == "HĐLĐ.")
                                    nhanVien.NgayBatDauChinhThuc = aModel.NgayHieuLuc;

                            }
                        }
                        catch (Exception)
                        {

                        }

                    }

                    nhanVien.NgayNghiViec = null;
                    nhanVien.SoNgayNghiPhepThamNien = aModel.SoNgayNghiPhepThamNien;
                    nhanVien.IdChucDanh = aModel.IddmChucDanh;
                    nhanVien.IdChucVu = aModel.IddmChucVu;
                    nhanVien.IdCaLamViec = aModel.IdThoiGianLamViec;
                    nhanVien.IdBoPhanEOS = aModel.IdBoPhanEOS;
                    nhanVien.PhiCongDoan = aModel.PhiCongDoan;
                    nhanVien.IdCongTyEOS = aModel.IdCongTyEOS;
                    if (aModel.MaSoThueTNCN != "" && aModel.MaSoThueTNCN != null)
                        nhanVien.MaSoThueTNCN = aModel.MaSoThueTNCN;
                    if (aModel.BaoHiem != null && aModel.BaoHiem.SoSoBaoHiem != null || aModel.BaoHiem.SoSoBaoHiem != "")
                        nhanVien.SoBaoHiemXaHoi = aModel.BaoHiem.SoSoBaoHiem;

                    dbContext.Entry(nhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    var nhanvienTKNH = dbContext.HRM_NhanVien_TaiKhoanNganHang.FirstOrDefault(r => r.idNhanVien == aModel.IdNhanVien);
                    if (aModel.LuongHopDong != null && aModel.LuongHopDong.SoTaiKhoan != null && aModel.LuongHopDong.TenNganHang != null)
                    {
                        if (nhanvienTKNH != null)
                        {
                            nhanvienTKNH.SoTaiKhoan = aModel.LuongHopDong.SoTaiKhoan;
                            nhanvienTKNH.TenNganHang = aModel.LuongHopDong.TenNganHang;
                            dbContext.Entry(nhanvienTKNH).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                        else
                            dbContext.HRM_NhanVien_TaiKhoanNganHang.Add(new HRM_NhanVien_TaiKhoanNganHang(aModel));
                    }
                    List<HRM_Luong_NhanVien> luongs = dbContext.HRM_Luong_NhanVien.Where(r => r.IdNhanVien == aModel.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= aModel.NgayHieuLuc)).OrderBy(a => a.Modified).ToList();
                    if (luongs != null)
                    {
                        foreach (var item in luongs)
                        {
                            if (aModel.NgayHieuLuc == null)
                                item.NgayHetHieuLuc = null;
                            else
                                item.NgayHetHieuLuc = aModel.NgayHieuLuc.AddDays(-1);
                            dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }

                    }
                    if (aModel.BaoHiem != null)
                    {
                        var baohiem_hopdong = aModel.BaoHiem;
                        if (baohiem_hopdong.isBaoHiemXaHoi == true)
                        {
                            double luongBHCu = 0;
                            if (luongs != null && luongs.Count > 0)
                            {
                                luongBHCu = luongs.FirstOrDefault().LuongHachToan;

                            }
                            if (luongBHCu == 0 || luongBHCu < aModel.LuongBaoHiem.LuongBaoHiem)
                            {
                                HRM_TangBaoHiem tangBaoHiem = new HRM_TangBaoHiem();
                                tangBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                tangBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                tangBaoHiem.ThoiGianDong = aModel.NgayHieuLuc;
                                tangBaoHiem.Loai = eLoaiBaoHiem.TangMoi.ToString();
                                tangBaoHiem.MucDong = aModel.LuongBaoHiem.LuongBaoHiem;
                                dbContext.HRM_TangBaoHiem.Add(tangBaoHiem);
                            }
                            else if (luongBHCu < aModel.LuongBaoHiem.LuongBaoHiem)
                            {
                                HRM_GiamBaoHiem GiamBaoHiem = new HRM_GiamBaoHiem();
                                GiamBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                GiamBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                GiamBaoHiem.NgayGiam = aModel.NgayHieuLuc;
                                GiamBaoHiem.Loai = eLoaiBaoHiem.GiamBaoHiem.ToString();
                                GiamBaoHiem.MucDong = aModel.LuongBaoHiem.LuongBaoHiem;
                                dbContext.HRM_GiamBaoHiem.Add(GiamBaoHiem);
                            }
                        }
                    }
                    HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(aModel);
                    if (nhanVien.isLuongHuu == true)
                        luong_NhanVien.LuongHachToan = 0;
                    dbContext.HRM_Luong_NhanVien.Add(luong_NhanVien);
                    if (aModel.lstPhuCap != null)
                    {
                        List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
                        foreach (var item in aModel.lstPhuCap)
                            listPhuCap.Add(new HRM_Luong_PhuCap_NhanVien(aModel, item, luong_NhanVien.IdGuid, nameof(HRM_HopDong), Host));
                        dbContext.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCap);
                    }
                    aModel.IdNhanVienEOS = nhanVien.IdNhanVienEOS;
                    clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSu(dbContext, aModel);
                }
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                return false;
            }
        }
        public dynamic GetSoNgayPhepThamNien(ApplicationDbContext db, string IdNhanVien)
        {
            var NhanVien = db.HRM_NhanVien.FirstOrDefault(a => a.IdGuid == IdNhanVien);
            int SoNgayPhep = 0;
            if (NhanVien.NgayBatDauChinhThuc != null)
            {
                var time = DateTime.Now - NhanVien.NgayBatDauChinhThuc;
                double totalDay = time.Value.TotalDays;
                SoNgayPhep = (int)time.Value.TotalDays / (365 * 5);

            }
            return SoNgayPhep;
        }
        public dynamic ChuyenTiepHopDong(ApplicationDbContext db)
        {
            var listHopDong = db.HRM_HopDong.Where(a => a.isKetThuc == true).ToList();
            var idsNv = listHopDong.Select(a => a.IdNhanVien).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var nhanVienById = db.HRM_NhanVien.AsNoTracking()
                .Where(a => idsNv.Contains(a.IdGuid))
                .ToDictionary(a => a.IdGuid);
            foreach (var item in listHopDong)
            {
                if (!string.IsNullOrEmpty(item.IdNhanVien) && nhanVienById.TryGetValue(item.IdNhanVien, out var nv))
                {
                    item.IdNhanVienEOS = nv.IdNhanVienEOS;
                    item.IdCongTyEOS = nv.IdCongTyEOS;
                    item.IdBoPhanEOS = nv.IdBoPhanEOS;
                    item.IddmChucVu = nv.IdChucVu;
                }
                clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.AddChucVuNhanSu(db, item);
            }
            db.SaveChanges();
            return "0";
        }

        public dynamic ChuyenTiepQuyTrinhHopDong(HRM_HopDong QuyTrinh, ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
                try
                {
                    var check = db.HRM_HopDong.Where(a => a.SoHopDong == QuyTrinh.SoHopDong && a.IdGuid != QuyTrinh.IdGuid).FirstOrDefault();
                    if (check != null)
                    {
                        err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                        return err;
                    }
                    HRM_NhanVien nhanVien = db.HRM_NhanVien.Find(QuyTrinh.IdNhanVien);
                    HRM_HopDong pcuDatabase = db.HRM_HopDong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!string.IsNullOrEmpty(QuyTrinh.IdTrangThai))
                    {
                        if (QuyTrinh.IdTrangThai != pcuDatabase.IdTrangThai)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Phiếu đã được chuyển trước đó!");
                    }
                    QuyTrinh.NoiDung = nhanVien.MaNhanVien + " - " + nhanVien.TenDayDu;
                    if (!ChuyenTiepTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err, QuyTrinh.listIdCongTy.FirstOrDefault()))
                        return err;
                    if (!FuncUpdateHopDong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    if (!ChuyenTiepSauHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, "", err))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    if (QuyTrinh.isKetThuc == true)
                    {

                        var listHopDongCuHieuLuc = db.HRM_HopDong.Where(a => a.isKetThuc == true && a.IdNhanVien == QuyTrinh.IdNhanVien && a.IdGuid != QuyTrinh.IdGuid).ToList();
                        if (listHopDongCuHieuLuc != null && listHopDongCuHieuLuc.Count > 0)
                        {
                            foreach (var item in listHopDongCuHieuLuc)
                            {
                                item.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }

                        }
                        if (Host != "ICT")
                        {
                            var listDonViOld = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == QuyTrinh.IdNhanVien).ToList();
                            if (listDonViOld != null && listDonViOld.Count > 0)
                                db.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(listDonViOld);
                            if (QuyTrinh.listIdCongTy != null && QuyTrinh.listIdCongTy.Count > 0)
                            {
                                List<HRM_QuyTrinhBoPhan_BoPhan> listDonVi = new List<HRM_QuyTrinhBoPhan_BoPhan>();

                                foreach (var Id in QuyTrinh.listIdCongTy)
                                {
                                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                                    bp.IdGuid = Guid.NewGuid().ToString();
                                    bp.IdQuyTrinh = QuyTrinh.IdNhanVien;
                                    bp.IdBoPhan = Id;
                                    bp.TableName = nameof(HRM_NhanVien);
                                    listDonVi.Add(bp);
                                }
                                db.HRM_QuyTrinhBoPhan_BoPhan.AddRange(listDonVi);
                            }
                            nhanVien.NgayNghiViec = null;
                            nhanVien.SoNgayNghiPhepThamNien = QuyTrinh.SoNgayNghiPhepThamNien;
                            nhanVien.IdChucDanh = QuyTrinh.IddmChucDanh;
                            nhanVien.IdChucVu = QuyTrinh.IddmChucVu;
                            nhanVien.IdCaLamViec = QuyTrinh.IdThoiGianLamViec;

                            nhanVien.IdBoPhanEOS = QuyTrinh.IdBoPhanEOS;
                            nhanVien.PhiCongDoan = QuyTrinh.PhiCongDoan;
                            nhanVien.IdCongTyEOS = QuyTrinh.IdCongTyEOS;
                            if (QuyTrinh.MaSoThueTNCN != "" && QuyTrinh.MaSoThueTNCN != null)
                                nhanVien.MaSoThueTNCN = QuyTrinh.MaSoThueTNCN;
                            if (QuyTrinh.BaoHiem.SoSoBaoHiem != null || QuyTrinh.BaoHiem.SoSoBaoHiem != "")
                                nhanVien.SoBaoHiemXaHoi = QuyTrinh.BaoHiem.SoSoBaoHiem;
                            var LoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => (a.IdGuid == QuyTrinh.IdLoaiHopDong) && (a.Ma == "HĐLĐ" || a.Ma == "HĐLĐ." || a.Ma == "KTHHĐ" || a.Ma == "THHĐ" || a.Ma == "HĐL"));
                            if (LoaiHopDong != null)
                            {

                                if (nhanVien != null && nhanVien.NgayBatDauChinhThuc == null)
                                {
                                    nhanVien.NgayBatDauChinhThuc = QuyTrinh.NgayHieuLuc;
                                }
                            }
                            if (nhanVien != null && nhanVien.NgayBatDauLamViec == null)
                            {
                                nhanVien.NgayBatDauLamViec = QuyTrinh.NgayHieuLuc;
                            }
                            nhanVien.SoNgayNghiPhepThamNien = QuyTrinh.SoNgayNghiPhepThamNien;
                            nhanVien.SoNgayNghiPhep = QuyTrinh.SoNgayNghiPhep;
                            if (QuyTrinh.BaoHiem != null)
                                nhanVien.isThamGiaBHXH = QuyTrinh.BaoHiem.isBaoHiemXaHoi;
                            if (QuyTrinh.BaoHiemNgoai != null)
                                nhanVien.GiamBaoHiem = QuyTrinh.BaoHiemNgoai.Giam;
                            if (nhanVien.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                            {
                                nhanVien.NgayQuayLai = DateTime.Now;
                                nhanVien.NgayNghiViec = null;
                                nhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                            }
                        }



                        if (QuyTrinh.BaoHiemNgoai != null)
                        {
                            var BaoHiemNgoai = QuyTrinh.BaoHiemNgoai.Clone();
                            BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();
                            BaoHiemNgoai.IdHopDong = "";
                            db.HRM_BaoHiem_HopDong.Add(BaoHiemNgoai);
                            var BHNOld = db.HRM_BaoHiem_HopDong.Where(a => a.IdNhanVien == nhanVien.IdGuid && string.IsNullOrEmpty(a.IdHopDong)).ToList();
                            if (BHNOld != null)
                                db.HRM_BaoHiem_HopDong.RemoveRange(BHNOld);

                        }
                        if (QuyTrinh.IdLoaiHopDong != null && QuyTrinh.IdLoaiHopDong != "")
                        {
                            try
                            {
                                var dmLoaiHopDong = db.HRM_dmLoaiHopDong.Find(QuyTrinh.IdLoaiHopDong);
                                if (nhanVien.NgayBatDauLamViec == null)
                                {
                                    if (dmLoaiHopDong.Ma == "HĐLĐ" || dmLoaiHopDong.Ma == "HĐLĐ.")
                                        nhanVien.NgayBatDauChinhThuc = QuyTrinh.NgayHieuLuc;

                                }
                                nhanVien.TrangThai = "DangLamViec";
                                string MaLoaiHopDong = dmLoaiHopDong.Ma;
                                var LoaiNV = db.HRM_dmLoaiNhanVien.FirstOrDefault(r => r.Ma == MaLoaiHopDong);
                                if (LoaiNV != null)
                                    nhanVien.IdLoaiNhanVien = LoaiNV.IdGuid;
                            }
                            catch (Exception)
                            {
                                dbContextTransaction.Rollback();
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi mã ở danh mục loại hợp đồng và mã ở danh loại nhân viên không giống nhau vui lòng gọi người cầu hình kiểm tra lại");
                            }

                        }


                        foreach (var item in QuyTrinh.listKhamSucKhoe)
                        {
                            nhanVien.listKhamSucKhoe.Add(new HRM_NhanVien_KhamSucKhoe(item));
                        }

                        db.Entry(nhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        var nhanvienTKNH = db.HRM_NhanVien_TaiKhoanNganHang.FirstOrDefault(r => r.idNhanVien == QuyTrinh.IdNhanVien);
                        if (QuyTrinh.LuongHopDong.SoTaiKhoan != null && QuyTrinh.LuongHopDong.TenNganHang != null)
                        {
                            if (nhanvienTKNH != null)
                            {
                                nhanvienTKNH.SoTaiKhoan = QuyTrinh.LuongHopDong.SoTaiKhoan;
                                nhanvienTKNH.TenNganHang = QuyTrinh.LuongHopDong.TenNganHang;
                                db.Entry(nhanvienTKNH).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }
                            else
                                db.HRM_NhanVien_TaiKhoanNganHang.Add(new HRM_NhanVien_TaiKhoanNganHang(QuyTrinh));
                        }
                        List<HRM_Luong_NhanVien> luongs = db.HRM_Luong_NhanVien.Where(r => r.IdNhanVien == QuyTrinh.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= QuyTrinh.NgayHieuLuc)).OrderBy(a => a.Modified).ToList();
                        if (luongs != null)
                        {
                            foreach (var item in luongs)
                            {
                                if (QuyTrinh.NgayHieuLuc == null)
                                    item.NgayHetHieuLuc = null;
                                else
                                    item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.AddDays(-1);
                                db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }

                        }
                        {
                            var baohiem_hopdong = QuyTrinh.BaoHiem;
                            if (baohiem_hopdong.isBaoHiemXaHoi == true)
                            {
                                double luongBHCu = 0;
                                if (luongs != null && luongs.Count > 0)
                                {
                                    luongBHCu = luongs.FirstOrDefault().LuongHachToan;

                                }
                                if (luongBHCu == 0 || luongBHCu < QuyTrinh.LuongBaoHiem.LuongBaoHiem)
                                {
                                    HRM_TangBaoHiem tangBaoHiem = new HRM_TangBaoHiem();
                                    tangBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                    tangBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                    tangBaoHiem.ThoiGianDong = QuyTrinh.NgayHieuLuc;
                                    tangBaoHiem.Loai = eLoaiBaoHiem.TangMoi.ToString();
                                    tangBaoHiem.MucDong = QuyTrinh.LuongBaoHiem.LuongBaoHiem;
                                    db.HRM_TangBaoHiem.Add(tangBaoHiem);
                                }
                                else if (luongBHCu < QuyTrinh.LuongBaoHiem.LuongBaoHiem)
                                {
                                    HRM_GiamBaoHiem GiamBaoHiem = new HRM_GiamBaoHiem();
                                    GiamBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                    GiamBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                    GiamBaoHiem.NgayGiam = QuyTrinh.NgayHieuLuc;
                                    GiamBaoHiem.Loai = eLoaiBaoHiem.GiamBaoHiem.ToString();
                                    GiamBaoHiem.MucDong = QuyTrinh.LuongBaoHiem.LuongBaoHiem;
                                    db.HRM_GiamBaoHiem.Add(GiamBaoHiem);
                                }
                            }
                        }
                        HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(QuyTrinh);
                        if (nhanVien.isLuongHuu == true)
                            luong_NhanVien.LuongHachToan = 0;
                        db.HRM_Luong_NhanVien.Add(luong_NhanVien);
                        if (QuyTrinh.lstPhuCap != null)
                        {
                            List<HRM_Luong_PhuCap_NhanVien> luongPCs = db.HRM_Luong_PhuCap_NhanVien.Where(r => r.IdNhanVien == QuyTrinh.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= QuyTrinh.NgayHieuLuc)).OrderBy(a => a.Modified).ToList();
                            luongPCs.ForEach(item =>
                                {
                                    item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.AddDays(-1);
                                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                });
                            List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
                            foreach (var item in QuyTrinh.lstPhuCap)
                                listPhuCap.Add(new HRM_Luong_PhuCap_NhanVien(QuyTrinh, item, luong_NhanVien.IdGuid, nameof(HRM_HopDong), Host));
                            db.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCap);
                        }
                        QuyTrinh.IdNhanVienEOS = nhanVien.IdNhanVienEOS;
                        clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSu(db, QuyTrinh);

                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }
        public dynamic ChuyenTiepQuyTrinhHopDongUpdateLuong(ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
                try
                {
                    DateTime time = new DateTime(2025, 9, 25, 0, 0, 0);
                    #region data
                    var listHopDong = (from a in db.HRM_HopDong
                                       where a.isKetThuc == true
                                       orderby a.NgayHieuLuc, a.Modified
                                       select a).ToList();
                    List<string> IdHopDongs = listHopDong.Select(a => a.IdGuid).ToList();
                    List<string> IdNhanViens = listHopDong.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listLuong = db.HRM_Luong_HopDong.Where(a => IdHopDongs.Contains(a.IdHopDong)).ToList();
                    var listLuongBH = db.HRM_LuongBaoHiem_HopDong.Where(a => IdHopDongs.Contains(a.IdHopDong)).ToList();
                    var listBH = db.HRM_BaoHiem_HopDong.Where(a => a.isNgoai == false && IdHopDongs.Contains(a.IdHopDong)).ToList();
                    var listPhuCapdb = db.HRM_HopDongPhuCap.Where(a => IdHopDongs.Contains(a.IdHopDong)).ToList();
                    #endregion
                    List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
                    List<HRM_Luong_NhanVien> listLuongadd = new List<HRM_Luong_NhanVien>();
                    foreach (var QuyTrinh in listHopDong)
                    {
                        //var QuyTrinh = listHopDong.FirstOrDefault(a => a.IdNhanVien == IdNhanVien);
                        QuyTrinh.LuongHopDong = listLuong.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                        QuyTrinh.BaoHiem = listBH.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                        QuyTrinh.LuongBaoHiem = listLuongBH.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                        QuyTrinh.lstPhuCap = listPhuCapdb.Where(a => a.IdHopDong == QuyTrinh.IdGuid).ToList();
                        HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(QuyTrinh);
                        listLuongadd.Add(luong_NhanVien);

                        if (QuyTrinh.lstPhuCap != null)
                        {
                            foreach (var item in QuyTrinh.lstPhuCap)
                                listPhuCap.Add(new HRM_Luong_PhuCap_NhanVien(QuyTrinh, item, luong_NhanVien.IdGuid, nameof(HRM_HopDong), Host));

                        }
                        var checkLuong = listLuongadd.Where(a => a.IdNhanVien == QuyTrinh.IdNhanVien && a.IdNguonNhapLuong != QuyTrinh.IdGuid && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= QuyTrinh.NgayHieuLuc)).ToList();
                        foreach (var item in checkLuong)
                        {
                            item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.AddDays(-1);
                        }
                        var checkPC = listPhuCap.Where(a => a.IdNhanVien == QuyTrinh.IdNhanVien && a.IdNguonNhapLuong != QuyTrinh.IdGuid && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= QuyTrinh.NgayHieuLuc)).ToList();
                        foreach (var item in checkPC)
                        {
                            item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.AddDays(-1);
                        }





                    }
                    db.HRM_Luong_NhanVien.AddRange(listLuongadd);
                    db.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCap);
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
        }

        public dynamic KhongDuyetQuyTrinhHopDong(HRM_HopDong QuyTrinh, ApplicationDbContext db)
        {
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var checkdb = db.HRM_HopDong.Where(a => a.SoHopDong == QuyTrinh.SoHopDong && a.IdGuid != QuyTrinh.IdGuid).FirstOrDefault();
                    if (checkdb != null)
                    {
                        err = new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng đã tồn tại");
                        return err;
                    }
                    HRM_HopDong pcuDatabase = db.HRM_HopDong.AsNoTracking().Where(x => x.IdGuid == QuyTrinh.IdGuid).FirstOrDefault();
                    if (!KhongDuyetTruocHamUpdate(db, _CallWebService, pcuDatabase, QuyTrinh, err))
                        return err;
                    if (!FuncUpdateHopDong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.ChuyenTiep))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    if (!KhongDuyetSauHamUpdate(db, QuyTrinh))
                    {
                        dbContextTransaction.Rollback();
                        return err;
                    }
                    //bool check = FuncUpdateHopDong(QuyTrinh, db, err, eQuyTrinh_LoaiHam.KhongDuyet);
                    //if (check == false)
                    //{
                    //    dbContextTransaction.Rollback();
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công");
                    //}
                    //var trangThai = _CallWebService.GetTrangThaiKhongDuyet(db, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), QuyTrinh.IdTrangThai, QuyTrinh.IdGuid);

                    //if (string.IsNullOrEmpty(trangThai.IdTrangThai))
                    //{
                    //    dbContextTransaction.Rollback();
                    //    return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, trangThai.TenTrangThai);
                    //}

                    //QuyTrinh.IdTrangThai = trangThai.IdTrangThai;
                    //QuyTrinh.TenTrangThai = trangThai.TenTrangThai;
                    //db.Entry(QuyTrinh).State = EntityState.Modified;
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật không thành công" + ex.ToString());
                }
            }
        }

        public dynamic ChuyenTrangThaiHopDong(string IdHopDong, ApplicationDbContext db)
        {
            try
            {
                string TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                HRM_HopDong hopDong = db.HRM_HopDong.Find(IdHopDong);
                if (hopDong == null) return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy hợp đồng");
                hopDong.TrangThai = TrangThai;
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());

            }
        }
        public EntitySingleResponse<HRM_HopDong> GetHopDong(string aId, ApplicationDbContext dbContext)
        {

            var result = new EntitySingleResponse<HRM_HopDong>();
            if (string.IsNullOrEmpty(dbContext.currentUser.Id))
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
                return result;
            }

            if (string.IsNullOrEmpty(aId))
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id");
                return result;
            }

            var model = dbContext.HRM_HopDong.AsNoTracking().FirstOrDefault(x => x.IdGuid == aId);
            if (model == null)
            {
                result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu hợp đồng đã bị xóa hoặc thay đổi");
                return result;
            }
            MessageReturn err = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Chuyển tiếp dữ liệu thành công");
            if (!GetQuyTrinh(dbContext, _CallWebService, model, err, true))
                return result;
            if (!string.IsNullOrEmpty(model.IdThoiGianLamViec))
                model.listIdCaLamViec = model.IdThoiGianLamViec.Split(',').ToList();

            var listTrangThietBi = dbContext.HRM_HopDongTaiSanBanGiao.Where(r => r.IdHopDong == aId).ToList();
            foreach (var item in listTrangThietBi)
            {
                item.NgayBanGiaoUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBanGiao);
                item.NgayHoanTraUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHoanTra);
            }
            model.listTrangThietBi = listTrangThietBi;
            var listMoTaCongViec = dbContext.HRM_HopDongCongViec.AsNoTracking().Where(r => r.IdHopDong == aId).OrderBy(a => a.Created).ToList();

            if (listMoTaCongViec != null && listMoTaCongViec.Count() > 0)
            {
                var filesMoTa = FileUploadController.GetListFileDinhKemByIds(dbContext, listMoTaCongViec.Select(x => x.IdGuid), nameof(HRM_HopDongCongViec));
                foreach (var item in listMoTaCongViec)
                    item.listFileDinhKem = filesMoTa[item.IdGuid].ToList();

                model.listMoTaCongViec = listMoTaCongViec.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_HopDongCongViec>
                {
                    data = a,
                    children = new List<ReturnData<HRM_HopDongCongViec>>()
                }).ToList();
                foreach (var item in model.listMoTaCongViec)
                    HopDongCongViecToTree(listMoTaCongViec, item);
            }
            if (model.NgayChuyenKhongThoiHan != null)
                model.NgayChuyenKhongThoiHanUnix = DateTimeToUnixTimeStamp(model.NgayChuyenKhongThoiHan);
            model.NgayKyUnix = DateTimeToUnixTimeStamp(model.NgayKy);
            model.NgayHieuLucUnix = DateTimeToUnixTimeStamp(model.NgayHieuLuc);
            model.NgayHetHanUnix = DateTimeToUnixTimeStamp(model.NgayHetHan);
            model.NgaySinhUnix = DateTimeToUnixTimeStamp(model.NgaySinh);
            model.ChungMinhThuNgayCapUnix = DateTimeToUnixTimeStamp(model.ChungMinhThuNgayCap);
            model.listIdCongTy = dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == model.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();
            if ((model.listIdCongTy == null || model.listIdCongTy.Count > 0) && model.IdCongTyEOS > 0)
            {
                model.listIdCongTy = new List<long>();
                model.listIdCongTy.Add(model.IdCongTyEOS);
            }
            var toDictLoaiHopDong = dbContext.HRM_dmLoaiHopDong.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictNhanVien = dbContext.HRM_NhanVien.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictQuocTich = dbContext.HRM_dmQuocTich.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictViTriChuyenMon = dbContext.HRM_dmViTriChuyenMon.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictCapBac = dbContext.HRM_dmCapBac.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            //var toGroupFileDinhKem = dbContext.HRM_FileDinhKem.ToList().ToGroup(x => x.idItem);
            var toGroupLuong = dbContext.HRM_Luong_HopDong.AsNoTracking().ToList().ToGroup(x => x.IdHopDong);
            var toDictChucVuCongViec = dbContext.HRM_dmChucVuCongViec.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(dbContext);

            model.LuongHopDong = dbContext.HRM_Luong_HopDong.AsNoTracking().FirstOrDefault(r => r.IdHopDong == model.IdGuid);
            if (model.LuongHopDong != null && model.LuongHopDong.LuongThucTe > 0 && model.LuongHopDong.LuongBaoHiem == 0)
                model.LuongHopDong.LuongBaoHiem = model.LuongHopDong.LuongThucTe;
            model.LuongBaoHiem = dbContext.HRM_LuongBaoHiem_HopDong.AsNoTracking().FirstOrDefault(r => r.IdHopDong == model.IdGuid);
            if (model.LuongBaoHiem == null)
                model.LuongBaoHiem = new HRM_LuongBaoHiem_HopDong();
            var codeChucVu = model.IdChucVuCongViec.GetValueFromDic(toDictChucVuCongViec, x => x.Ma);
            if (codeChucVu != HelperConstant.DanhMucChucVuCongViec.KHAC.Code)
            {
                model.MoTaCongViec = model.IdChucVuCongViec.GetValueFromDic(toDictChucVuCongViec, x => x.Ten);
            }

            HRM_NhanVien nhanVien = dbContext.HRM_NhanVien.AsNoTracking().FirstOrDefault(x => x.IdGuid == model.IdNhanVien);
            if (model.isPhuLuc == true)
                model.HopDongCha = dbContext.HRM_HopDong.AsNoTracking().FirstOrDefault(x => x.IdGuid == model.IdHopDong);
            if (model.IdTrangThietBi.IsNotEmpty())
            {
                var listIdTrangThietBi = model.IdTrangThietBi.Split(',').ToList();
                if (listIdTrangThietBi.IsNotEmpty())
                {
                    var listdmTrangThietBi = dbContext.HRM_dmTrangThietBi.AsNoTracking().Where(x => listIdTrangThietBi.Contains(x.IdGuid)).ToList();
                    if (listdmTrangThietBi.IsNotEmpty())
                    {
                        var listTenTrangThietBi = listdmTrangThietBi.Select(x => x.Ten).ToList();
                        if (listTenTrangThietBi.IsNotEmpty())
                        {
                            model.TrangThietBiLamViec = listTenTrangThietBi.Join(", ");
                        }
                    }
                }
            }

            if (nhanVien != null)
            {
                nhanVien.NgayCapMaSTCNUnix = dungchung.DateTimeToUnixTimeStamp(nhanVien.NgayCapMaSTCN);
                nhanVien.NgaySinhUnix = dungchung.DateTimeToUnixTimeStamp(nhanVien.NgaySinh);

                nhanVien.TenQuocTich = dbContext.HRM_dmQuocTich.AsNoTracking().Where(r => r.IdGuid == nhanVien.IdQuocTich).Select(r => r.Ten).FirstOrDefault();
                nhanVien.TenTrinhDo = dbContext.HRM_dmTrinhDoDaoTao.AsNoTracking().Where(r => r.IdGuid == nhanVien.IddmTrinhDo).Select(r => r.Ten).FirstOrDefault();
                model.TenNhanVien = nhanVien.TenDayDu;


                model.NguoiLaoDong = nhanVien;
                if (model.NguoiDaiDien != null && model.NguoiDaiDien.IdQuocTich.IsNotEmpty())
                {
                    model.NguoiDaiDienQuocTich = model.NguoiDaiDien.IdQuocTich.GetValueFromDic(toDictQuocTich);
                }
                if (model.NguoiLaoDong != null && model.NguoiLaoDong.IdQuocTich.IsNotEmpty())
                {
                    model.NguoiLaoDongQuocTich = model.NguoiLaoDong.IdQuocTich.GetValueFromDic(toDictQuocTich);
                }

            }

            model.LoaiHopDong = model.IdLoaiHopDong.GetValueFromDic(toDictLoaiHopDong);
            model.ThoiHanHopDong = model.IdThoiHanHopDong.GetValueFromDic(toDictThoiHanHopDong);
            model.NguoiDaiDien = model.IdNguoiDaiDien.GetValueFromDic(toDictNhanVien);

            model.ViTriChuyenMon = model.IdViTriChuyenMon.GetValueFromDic(toDictViTriChuyenMon);
            model.ChucVuCongViec = model.IdChucVuCongViec.GetValueFromDic(toDictChucVuCongViec);
            model.CapBac = model.IdCapBac.GetValueFromDic(toDictCapBac);
            var BoPhan = BoPhans.Find(x => x.Id == model.IdBoPhanEOS);
            if (BoPhan != null)
                model.BoPhanEOS = BoPhan;
            var CongTy = BoPhans.Find(x => x.Id == model.IdCongTyEOS);
            model.CongTyEOS = CongTy;
            model.lstFileDinhKem = FileUploadController.GetListFileDinhKem(dbContext, model.IdGuid, nameof(HRM_HopDong));
            model.listFileMoTaCongViec = FileUploadController.GetListFileDinhKem(dbContext, model.IdGuid, nameof(HRM_HopDongCongViec));
            var lstPhuCap = dbContext.HRM_HopDongPhuCap.AsNoTracking().Where(x => x.IdHopDong == aId).ToList();
            var listPhuCap = dbContext.HRM_dmPhuCap.AsNoTracking().ToList();
            foreach (var itemPC in lstPhuCap)
            {
                itemPC.NgayBatDauUnix = DateTimeToUnixTimeStamp(itemPC.NgayBatDau);
                itemPC.NgayKetThucUnix = DateTimeToUnixTimeStamp(itemPC.NgayKetThuc);
                var PhuCap = listPhuCap.FirstOrDefault(a => a.IdGuid == itemPC.IddmLoaiPhuCap);
                if (PhuCap != null)
                {
                    itemPC.TenPhuCap = PhuCap.Ten;
                    itemPC.Ten = PhuCap.Ten;
                }
            }
            model.lstPhuCap = lstPhuCap;
            var lstPhuCapBaoHiem = dbContext.HRM_HopDongPhuCapBaoHiem.AsNoTracking().Where(x => x.IdHopDong == aId).ToList();
            foreach (var itemPC in lstPhuCapBaoHiem)
            {
                itemPC.NgayBatDauUnix = DateTimeToUnixTimeStamp(itemPC.NgayBatDau);
                itemPC.NgayKetThucUnix = DateTimeToUnixTimeStamp(itemPC.NgayKetThuc);
            }
            if (lstPhuCapBaoHiem != null)
                model.lstPhuCapBaoHiem = lstPhuCapBaoHiem;
            else
                model.lstPhuCapBaoHiem = new List<HRM_HopDongPhuCapBaoHiem>();
            var BaoHiem = dbContext.HRM_BaoHiem_HopDong.AsNoTracking().FirstOrDefault(x => x.IdHopDong == aId && x.isNgoai == false);
            if (BaoHiem != null)
            {
                BaoHiem.NgayBatDauDongBHXHUnix = DateTimeToUnixTimeStamp(BaoHiem.NgayBatDauDongBHXH);
                BaoHiem.NgayBatDauUnix = DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
                BaoHiem.NgayCapSoBaoHiemUnix = DateTimeToUnixTimeStamp(BaoHiem.NgayCapSoBaoHiem);
                BaoHiem.NgayKetThucUnix = DateTimeToUnixTimeStamp(BaoHiem.NgayKetThuc);
                model.BaoHiem = BaoHiem;
            }
            var BaoHiemNgoai = dbContext.HRM_BaoHiem_HopDong.AsNoTracking().FirstOrDefault(x => x.IdHopDong == aId && x.isNgoai == true);
            if (BaoHiemNgoai != null)
            {
                BaoHiemNgoai.NgayBatDauDongBHXHUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDauDongBHXH);
                BaoHiemNgoai.NgayBatDauUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDau);
                BaoHiemNgoai.NgayCapSoBaoHiemUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayCapSoBaoHiem);
                BaoHiemNgoai.NgayKetThucUnix = DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                model.BaoHiemNgoai = BaoHiemNgoai;
            }
            var listKhamSucKhoe = dbContext.HRM_HopDong_KhamSucKhoe.AsNoTracking().AsNoTracking().Where(x => x.IdHopDong == model.IdGuid).OrderByDescending(a => a.NgayKham).ToList();
            if (listKhamSucKhoe.IsNotEmpty())
            {
                foreach (var item in listKhamSucKhoe)
                {
                    if (item.NgayKham != null)
                        item.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKham);
                    if (item.NgayKetThuc != null)
                        item.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayKetThuc);
                }
            }
            model.listKhamSucKhoe = listKhamSucKhoe;
            //  model.CheDoNghiPhep = model.IdGuid.GetValueFromDic(toDictCheDoNghiPhep);

            result.Item = model;

            return result;
        }
        public EntitySingleResponse<HRM_HopDong> GetHopDongSaoChep(string aId, bool isPhuLuc, ApplicationDbContext dbContext)
        {
            var HopDong = dbContext.HRM_HopDong.Where(a => a.IdGuid == aId).FirstOrDefault();
            var HopDongCuoi = dbContext.HRM_HopDong.Where(a => a.IdNhanVien == HopDong.IdNhanVien).OrderByDescending(a => a.isKetThuc).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
            EntitySingleResponse<HRM_HopDong> result = GetHopDong(HopDongCuoi.IdGuid, dbContext);
            var luong = dbContext.HRM_Luong_NhanVien.Where(a => a.IdNhanVien == HopDong.IdNhanVien).OrderByDescending(a => a.NgayBatDauHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
            var listPhuCap = dbContext.HRM_Luong_PhuCap_NhanVien.Where(a => a.IdNguonNhapLuong == luong.IdNguonNhapLuong).ToList();
            result.Item.LuongBaoHiem.LuongBaoHiem = luong.LuongHachToan;
            result.Item.LoaiLuong = luong.LoaiLuong;
            if (luong.NguonNhapLuong == nameof(HRM_HopDong) && result.Item.LuongHopDong.TyLeHuongLuong > 0)
            {
                result.Item.LuongHopDong.LuongBaoHiem = luong.Luong * 100 / (result.Item.LuongHopDong.TyLeHuongLuong);
                if (Host == "PHT")
                    result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
                    {
                        IddmLoaiPhuCap = a.IddmLoaiPhuCap,
                        SoTien = a.SoTien * 100 / (result.Item.LuongHopDong.TyLeHuongLuong),
                        NgayBatDau = a.NgayBatDauHieuLuc,
                        NgayKetThuc = a.NgayHetHieuLuc,
                    }).ToList();
                else
                {
                    result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
                    {
                        IddmLoaiPhuCap = a.IddmLoaiPhuCap,
                        SoTien = a.SoTien,
                        NgayBatDau = a.NgayBatDauHieuLuc,
                        NgayKetThuc = a.NgayHetHieuLuc,
                    }).ToList();
                }

            }
            else
            {
                result.Item.LuongHopDong.LuongBaoHiem = luong.Luong;
                result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
                {
                    IddmLoaiPhuCap = a.IddmLoaiPhuCap,
                    SoTien = a.SoTien,
                    NgayBatDau = a.NgayBatDauHieuLuc,
                    NgayKetThuc = a.NgayHetHieuLuc,
                }).ToList();

            }

            if (isPhuLuc == true)
            {
                result.Item.isPhuLuc = true;
                //if(HopDong.isKetThuc==true)
                //{

                //}    
                if (string.IsNullOrEmpty(result.Item.IdHopDong))
                    result.Item.IdHopDong = HopDong.IdGuid;
            }
            else
            {
                result.Item.IdHopDong = "";
            }
            result.Item.IdGuid = "";
            result.Item.isKetThuc = false;

            return result;
        }
        public EntitySingleResponse<HRM_HopDong> GetHopDongSaoChepByIdNhanVien(string IdNhanVien, bool isPhuLuc, ApplicationDbContext dbContext)
        {
            var HopDongCuoi = dbContext.HRM_HopDong.AsNoTracking().Where(a => a.IdNhanVien == IdNhanVien).OrderByDescending(a => a.isKetThuc).OrderByDescending(a => a.NgayHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
            EntitySingleResponse<HRM_HopDong> result = GetHopDong(HopDongCuoi.IdGuid, dbContext);
            //var luong = dbContext.HRM_Luong_NhanVien.AsNoTracking().Where(a => a.IdNhanVien == IdNhanVien).OrderByDescending(a => a.NgayBatDauHieuLuc).ThenByDescending(a => a.Modified).FirstOrDefault();
            //var listPhuCap = dbContext.HRM_Luong_PhuCap_NhanVien.AsNoTracking().Where(a => a.IdNguonNhapLuong == luong.IdNguonNhapLuong).ToList();
            //result.Item.LuongBaoHiem.LuongBaoHiem = luong.LuongHachToan;
            //result.Item.LoaiLuong = luong.LoaiLuong;
            //if (luong.NguonNhapLuong == nameof(HRM_HopDong) && result.Item.LuongHopDong.TyLeHuongLuong > 0)
            //{
            //    result.Item.LuongHopDong.LuongBaoHiem = luong.Luong * 100 / (result.Item.LuongHopDong.TyLeHuongLuong);
            //    if (Host == "PHT")
            //        result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
            //        {
            //            IddmLoaiPhuCap = a.IddmLoaiPhuCap,
            //            SoTien = a.SoTien * 100 / (result.Item.LuongHopDong.TyLeHuongLuong),
            //            NgayBatDau = a.NgayBatDauHieuLuc,
            //            NgayKetThuc = a.NgayHetHieuLuc,
            //        }).ToList();
            //    else
            //    {
            //        result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
            //        {
            //            IddmLoaiPhuCap = a.IddmLoaiPhuCap,
            //            SoTien = a.SoTien,
            //            NgayBatDau = a.NgayBatDauHieuLuc,
            //            NgayKetThuc = a.NgayHetHieuLuc,
            //        }).ToList();
            //    }

            //}
            //else
            //{
            //    result.Item.LuongHopDong.LuongBaoHiem = luong.Luong;
            //    result.Item.lstPhuCap = listPhuCap.Select(a => new HRM_HopDongPhuCap
            //    {
            //        IddmLoaiPhuCap = a.IddmLoaiPhuCap,
            //        SoTien = a.SoTien,
            //        NgayBatDau = a.NgayBatDauHieuLuc,
            //        NgayKetThuc = a.NgayHetHieuLuc,
            //    }).ToList();

            //}

            if (isPhuLuc == true)
            {
                result.Item.isPhuLuc = true;
                //if(HopDong.isKetThuc==true)
                //{

                //}    
                if (string.IsNullOrEmpty(result.Item.IdHopDong))
                    result.Item.IdHopDong = result.Item.IdGuid;
            }
            result.Item.IdGuid = "";
            result.Item.isKetThuc = false;

            return result;
        }

        public MessageReturn DeleteHopDong(List<string> aListId, ApplicationDbContext dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu hợp đồng thành công");
            if (string.IsNullOrEmpty(dbContext.currentUser.Id))
            {
                return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            //int QuyenView = GetQuyenXoa(DefineData.eAction.HOPDONG.ToString(), dbContext.currentUser.Id, aSmart);
            //if (QuyenView == (int)DefineData.eGioiHan.NONE)
            //{
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Không có quyền Xóa");
            //}

            if (aListId.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id để xóa");
            }

            var listHopDong = dbContext.HRM_HopDong.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listHopDong.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listErr = new List<string>();
            foreach (var item in listHopDong)
            {
                if (item.CreatedBy != dbContext.currentUser.Id)
                {
                    listErr.Add(item.IdGuid);
                }
                //if (QuyenView == (int)DefineData.eGioiHan.OWNER && item.CreatedBy != dbContext.currentUser.Id)
                //{
                //    listErr.Add(item.IdGuid);
                //}
            }
            //if (listErr.IsNotEmpty())
            //{
            //    return new MessageReturn(DefineData.eErrorMessage.KhongDuQuyen, "Bạn không có quyền xóa hợp đồng");
            //}

            dbContext.HRM_HopDong.RemoveRange(listHopDong);

            var listTepDinhKem = dbContext.HRM_FileDinhKem.Where(x => aListId.Contains(x.idItem)).ToList();
            if (listTepDinhKem.IsNotEmpty())
            {
                result = FileUpload.DeleteFiles(listTepDinhKem, dbContext, result);
                if (result.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                {
                    return result;
                }
            }

            var listPhuCap = dbContext.HRM_HopDongPhuCap.Where(x => aListId.Contains(x.IdHopDong)).ToList();
            if (listPhuCap.IsNotEmpty())
            {
                dbContext.HRM_HopDongPhuCap.RemoveRange(listPhuCap);
            }

            var listLuong = dbContext.HRM_Luong_HopDong.Where(x => aListId.Contains(x.IdHopDong)).ToList();
            if (listLuong.IsNotEmpty())
            {
                dbContext.HRM_Luong_HopDong.RemoveRange(listLuong);
            }

            var listBaoHiem = dbContext.HRM_BaoHiem_HopDong.Where(x => aListId.Contains(x.IdHopDong)).ToList();
            if (listBaoHiem.IsNotEmpty())
            {
                dbContext.HRM_BaoHiem_HopDong.RemoveRange(listBaoHiem);
            }

            using (var ts = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.SaveChanges();
                    ts.Commit();
                }
                catch (Exception ex)
                {
                    ts.Rollback();
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
                }
            }
            _CallWebService.AddModify(dbContext, DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString()), DefineData.eModify.Xoa.ToString(), DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), string.Join(",", listHopDong.Select(a => a.SoHopDong).ToList()), string.Join(",", aListId), "", "", 0, dbContext.currentUser.Id, dbContext.currentUser.TenNhanVien);

            return result;
        }

        public dynamic ExportHopDongLaoDongDaiAn(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HDongLaoDong_DaiAn.docx");
            string sFileNameCopy = PathUploads + filenamegui + ".docx";
            //        string sFileNameCopy = HostingEnvironmentCompat.MapPath("~/Upload/") + filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var ChucVuNDD = _CallWebService.GetChucVu(db, HopDong.NguoiDaiDienIdChucVu);
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyEOS);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien);
            if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            HRM_dmQuocTich QuocTichNDD = null;
            if (NguoiDaiDien != null)
                QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);

            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);

            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@Nam", HopDong.NgayKy.Year.ToString());
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            if (NguoiDaiDien != null && !string.IsNullOrEmpty(NguoiDaiDien.TenDayDu))
                doc.ReplaceText("@TenNguoiDaiDien", NguoiDaiDien.TenDayDu);
            else
                doc.ReplaceText("@TenNguoiDaiDien", "");

            if (ChucVuNDD != null && !string.IsNullOrEmpty(ChucVuNDD.TenChucVu))
                doc.ReplaceText("@ChucVuNguoiDaiDien ", ChucVuNDD.TenChucVu);
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien ", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            if (!string.IsNullOrEmpty(CongTy.DiaChi))
                doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai);
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            if (QuocTichNDD != null && !string.IsNullOrEmpty(QuocTichNDD.Ten ?? ""))
                doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "");
            else
                doc.ReplaceText("@QuocTichNDD", "");

            // Nguoi lao dong
            doc.ReplaceText("@TenNguoiLaoDong", NguoiLaoDong.TenDayDu);
            if (QuocTichNLD != null && !string.IsNullOrEmpty(QuocTichNLD.Ten))
                doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten);
            else
                doc.ReplaceText("@QuocTichNLD", "");

            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NoiSinh", NguoiLaoDong.NoiSinh ?? "");
            if (TrinhDo != null)
                doc.ReplaceText("@TrinhDo", TrinhDo.Ten);
            else
                doc.ReplaceText("@TrinhDo", "");
            if (NguoiLaoDong.HoKhauThuongTruDiaChi != null)
                doc.ReplaceText("@DiaChiThuongChu", NguoiLaoDong.HoKhauThuongTruDiaChi);
            else
                doc.ReplaceText("@DiaChiThuongChu", "");

            string SMT = GetThongTinCMT(NguoiLaoDong);

            doc.ReplaceText("@CMT", SMT);

            var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            doc.ReplaceText("@dmThoiHanHopDong", thoigianHopDong.Ten);
            string ThoiHanHopDong = "";
            if (HopDong.NgayHetHan == null)
                ThoiHanHopDong = "Từ ngày " + HopDong.NgayHieuLuc.ToString("dd/MM/yyyy");
            else
                ThoiHanHopDong = "Từ ngày " + HopDong.NgayHieuLuc.ToString("dd/MM/yyyy") + " đến hết ngày " + HopDong.NgayHetHan.Value.ToString("dd/MM/yyyy");
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@DiaDiemLamViec", HopDong.DiaDiemLamViec ?? "");
            if (ChucVuNLD != null && !string.IsNullOrEmpty(ChucVuNLD.TenChucVu))
                doc.ReplaceText("@ChuVuNLD", ChucVuNLD.TenChucVu);
            else
                doc.ReplaceText("@ChuVuNLD", "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan);
            var listCongViec = db.HRM_HopDongCongViec.Where(r => r.IdHopDong == HopDong.IdGuid).ToList();
            string CongViec = "";
            foreach (var item in listCongViec)
                CongViec = CongViec + "+ " + item.TenCongViec + "\n";
            doc.ReplaceText("@CongViec", CongViec);
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
            string LoaiLuong = "";
            if (HopDong.LoaiLuong == 1)
                LoaiLuong = "tháng";
            if (HopDong.LoaiLuong == 2)
                LoaiLuong = "ngày";
            if (HopDong.LoaiLuong == 3)
                LoaiLuong = "giờ";
            if (LuonghopDong != null)
            {
                string Luong = LuonghopDong.LuongBaoHiem.ToString("N0") + "đ/" + LoaiLuong + " (bao gồm BHXH, BHYT, BHTN,…)";
                doc.ReplaceText("@Luong", Luong);
                var HinhThucTraLuong = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
                if (HinhThucTraLuong != null)
                {
                    doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong.Ten);
                }
            }
            else
                doc.ReplaceText("@Luong", "");

            var listPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdHopDong == HopDong.IdGuid).ToList();
            string phuCap = "";
            double TongKhauTru = 0;
            if (listPhuCap.Count() > 0)
                foreach (var item in listPhuCap)
                {
                    //    if (HopDong.LoaiLuong != item.LoaiPhuCap)
                    //    {
                    //        if (item.LoaiPhuCap == 3 && HopDong.LoaiLuong == 1)
                    //            TongKhauTru = TongKhauTru + item.SoTien * 8 * 26;
                    //        if (item.LoaiPhuCap == 2 && HopDong.LoaiLuong == 1)
                    //            TongKhauTru = TongKhauTru + item.SoTien * 26;
                    //    }
                    //    else
                    //    {
                    TongKhauTru = TongKhauTru + item.SoTien;

                    //}
                }

            phuCap = string.Format("{0:0,0 VNĐ}", TongKhauTru).Replace(".", ",");
            doc.ReplaceText("@PhuCap", phuCap);
            string NgayThangHieuLuc = HopDong.NgayHieuLuc.ToString("dd/MM/yyyy") + ".";
            doc.ReplaceText("@NgayHieuLuc", NgayThangHieuLuc);
            //doc.ReplaceText("@NgayHieuLuc", NgayThangHieuLuc);
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HĐLaoDong-PHT.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }

        public bool AddUpdateHopDong(HRM_HopDong aModel, ApplicationDbContext dbContext, bool isKetThuc = true)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới hợp đồng thành công");
            try
            {

                aModel.isPhuLuc = true;

                if (aModel.IdThoiHanHopDong != null && aModel.NgayHetHanUnix == 0)
                {
                    var dmThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == aModel.IdThoiHanHopDong);
                    if (aModel.NgayHetHan == null && dmThoiHanHopDong != null && dmThoiHanHopDong.SoThang != 0)
                        aModel.NgayHetHan = aModel.NgayHieuLuc.AddMonths(dmThoiHanHopDong.SoThang).AddDays(-1);
                    else
                    {
                        aModel.NgayHetHan = null;
                    }
                }
                if (aModel.NgayKyUnix > 0)
                    aModel.NgayKy = UnixTimeStampToDateTime(aModel.NgayKyUnix);
                if (aModel.NgayChuyenKhongThoiHanUnix > 0)
                    aModel.NgayChuyenKhongThoiHan = UnixTimeStampToDateTime(aModel.NgayChuyenKhongThoiHanUnix);

                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                if (aModel.NgayHetHanUnix > 0)
                    aModel.NgayHetHan = UnixTimeStampToDateTime(aModel.NgayHetHanUnix);
                //if (aModel.IdThoiHanHopDong != null && aModel.NgayHetHanUnix == 0)
                //{
                //    var dmThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.Find(aModel.IdThoiHanHopDong);
                //    if (aModel.NgayHetHan == null && dmThoiHanHopDong.SoThang != 0)
                //        aModel.NgayHetHan = aModel.NgayHieuLuc.AddMonths(dmThoiHanHopDong.SoThang).AddDays(-1);
                //    else
                //    {
                //        aModel.NgayHetHan = UnixTimeStampToDateTime(0);
                //    }
                //}

                if (aModel.NgaySinhUnix > 0)
                    aModel.NgaySinh = UnixTimeStampToDateTime(aModel.NgaySinhUnix);
                if (aModel.ChungMinhThuNgayCapUnix > 0)
                    aModel.ChungMinhThuNgayCap = UnixTimeStampToDateTime(aModel.ChungMinhThuNgayCapUnix);
                if (aModel.NgayHieuLucUnix > 0)
                    aModel.NgayHieuLuc = UnixTimeStampToDateTime(aModel.NgayHieuLucUnix);
                long IdCongTy = 0;
                if (aModel.listIdCongTy != null && aModel.listIdCongTy.Count > 0)
                    IdCongTy = aModel.listIdCongTy.FirstOrDefault();
                try
                {
                    if (string.IsNullOrEmpty(aModel.SoHopDong))
                        aModel.SoHopDong = GetMaHopDong(dbContext, aModel.IdLoaiHopDong, aModel.IdNhanVien, aModel.IdHopDong, aModel.IddmChucVu, aModel.IdBoPhanEOS, IdCongTy, aModel.NgayKyUnix);
                }
                catch
                {

                }
                var listFileCreate = new List<EntityAddHRMFileDinhKemModel>();
                aModel.IdGuid = Guid.NewGuid().ToString();
                {

                    if (isKetThuc == false)
                    {
                        var trangThai = _CallWebService.GetTrangThaiKhoiTao(dbContext, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.IdGuid);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {

                            return false;
                        }

                        aModel.IdTrangThai = trangThai.IdTrangThai;
                        aModel.TenTrangThai = trangThai.TenTrangThai;
                        aModel.TrangThai = DefineData.eTrangTHaiHopDong.ChuaKy.ToString();//Khi  quy trình hợp đồng thì coi như chưa ký
                    }
                    else
                    {
                        var trangThai = _CallWebService.GetTrangThaiKhoiTaoKetThuc(dbContext, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aModel.IdGuid, aModel.SoHopDong);
                        if (trangThai.IdTrangThai == null || trangThai.IdTrangThai == "")
                        {

                            return false;
                        }
                        aModel.isKetThuc = true;
                        aModel.IdTrangThai = trangThai.IdTrangThai;
                        aModel.TenTrangThai = trangThai.TenTrangThai;
                        aModel.TrangThai = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();//Khi  quy trình hợp đồng thì coi như chưa ký
                        var listHopDongCuHieuLuc = dbContext.HRM_HopDong.Where(a => a.isKetThuc == true && a.IdNhanVien == aModel.IdNhanVien && a.IdGuid != aModel.IdGuid).ToList();
                        if (listHopDongCuHieuLuc != null && listHopDongCuHieuLuc.Count > 0)
                        {
                            foreach (var item in listHopDongCuHieuLuc)
                            {
                                item.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                                dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            }

                        }
                    }

                }



                dbContext.HRM_HopDong.Add(aModel);
                result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới dữ liệu hợp đồng thành công");

                if (aModel.lstFileDinhKem.IsNotEmpty())
                {
                    foreach (var itemf in aModel.lstFileDinhKem)
                    {
                        var item = itemf.Clone();
                        item.IdGuid = "";

                        listFileCreate.Add(new EntityAddHRMFileDinhKemModel(item, aModel.IdGuid, nameof(HRM_HopDong), nameof(DefineData.eFlagDinhKem.FileDinhKem)));


                    }
                }

                if (aModel.lstPhuCap.IsNotEmpty())
                {
                    foreach (var itemf in aModel.lstPhuCap)
                    {
                        var item = itemf.Clone();
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        item.IdQuyetDinh = "";
                        dbContext.HRM_HopDongPhuCap.Add(item);

                    }
                }

                if (aModel.lstPhuCapBaoHiem.IsNotEmpty())
                {
                    foreach (var itemf in aModel.lstPhuCapBaoHiem)
                    {
                        var item = itemf.Clone();
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdHopDong = aModel.IdGuid;
                        dbContext.HRM_HopDongPhuCapBaoHiem.Add(item);

                    }
                }

                if (aModel.listTrangThietBi != null)
                {
                    foreach (var itemf in aModel.listTrangThietBi)
                    {
                        var item = itemf.Clone();
                        item.IdHopDong = aModel.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        dbContext.HRM_HopDongTaiSanBanGiao.Add(item);
                    }
                }

                var listMoTaCongViec = dbContext.HRM_HopDongCongViec.Where(a => a.IdHopDong == aModel.IdGuid).ToList();
                if (listMoTaCongViec != null && listMoTaCongViec.Count > 0)
                    dbContext.HRM_HopDongCongViec.RemoveRange(listMoTaCongViec);
                if (aModel.listMoTaCongViec != null)
                {
                    List<HRM_HopDongCongViec> listCV = new List<HRM_HopDongCongViec>();
                    HopDongCongViecToList(aModel.listMoTaCongViec, listCV);
                    foreach (var item in listCV)
                    {
                        item.IdHopDong = aModel.IdGuid;
                        item.IdGuid = Guid.NewGuid().ToString();
                        dbContext.HRM_HopDongCongViec.Add(item);
                        if (item.listFileDinhKem != null && item.listFileDinhKem.Count > 0)
                            FileUploadController.SetFileDinhKem(dbContext, item.listFileDinhKem, nameof(HRM_HopDongCongViec).ToString(), item.IdGuid);

                    }
                    dbContext.HRM_HopDongCongViec.AddRange(listCV);
                }

                if (aModel.LuongHopDong != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongHopDong.Clone();
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_Luong_HopDong.Add(item);
                }

                if (aModel.LuongBaoHiem != null)
                {
                    //foreach (var item in aModel.listLuong)
                    //{
                    var item = aModel.LuongBaoHiem.Clone();

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_LuongBaoHiem_HopDong.Add(item);
                }

                if (aModel.BaoHiem != null)
                {
                    var BaoHiem = aModel.BaoHiem.Clone();
                    BaoHiem.IdGuid = Guid.NewGuid().ToString();
                    BaoHiem.IdHopDong = aModel.IdGuid;
                    dbContext.HRM_BaoHiem_HopDong.Add(BaoHiem);
                }
                if (aModel.BaoHiemNgoai != null)
                {
                    var BaoHiemNgoai = aModel.BaoHiem.Clone();
                    BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();
                    BaoHiemNgoai.IdHopDong = aModel.IdGuid;
                    BaoHiemNgoai.isNgoai = true;
                    dbContext.HRM_BaoHiem_HopDong.Add(BaoHiemNgoai);
                }
                if (listFileCreate.IsNotEmpty())
                {
                    foreach (var item in listFileCreate)
                    {
                        if (item.ListFile.IsNotEmpty())
                        {
                            foreach (var filef in item.ListFile)
                            {
                                var file = filef.Clone();
                                file.IdGuid = "";
                                result = SetFileDinhKem(dbContext, file, item.NameTable, item.IdTable, result);
                                if (result.Error != DefineData.eErrorMessage.CapNhatThanhCong)
                                {

                                    return false;
                                }
                            }
                        }
                    }
                }


                dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public dynamic ExportPhuLucHopDongLaoDongDaiAn(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/PhuLuc_DaiAn.docx");
            string sFileNameCopy = PathUploads + filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyEOS);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);

            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@Nam", HopDong.NgayKy.Year.ToString());
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu);
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien ", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai);
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "");
            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten);
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NoiSinh", NguoiLaoDong.NoiSinh ?? "");
            if (TrinhDo != null)
                doc.ReplaceText("@TrinhDo", TrinhDo.Ten);
            else
                doc.ReplaceText("@TrinhDo", "");
            doc.ReplaceText("@ThuongTruNguoiLaoDong", NguoiLaoDong.HoKhauThuongTruDiaChi);
            string SMT = GetThongTinCMT(NguoiLaoDong);

            doc.ReplaceText("@CMT", SMT);

            var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            doc.ReplaceText("@dmThoiHanHopDong", thoigianHopDong.Ten);


            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();

            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0") + "đồng";
            doc.ReplaceText("@Luong", Luong);
            var listPhuCap = db.HRM_HopDongPhuCap.Where(r => r.IdHopDong == HopDong.IdGuid).ToList();
            string phuCap = "";
            if (listPhuCap.Count() > 0)
                phuCap = listPhuCap.Sum(r => r.SoTien).ToString("N0") + "đồng";
            doc.ReplaceText("@PhuCap", phuCap);
            string NgayThangHieuLuc = HopDong.NgayHieuLuc.ToString("dd/MM/yyyy") + ".";
            doc.ReplaceText("@NgayHieuLucPhuLuc", NgayThangHieuLuc);
            doc.ReplaceText("@NgayKyPhuLuc", HopDong.NgayKy.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@SoHopDong", HopDongCha.SoHopDong);
            doc.ReplaceText("@NgayKyHopDong", HopDongCha.NgayKy.ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("PhuLuc_DaiAn.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }

        public string GetThongTinCMT(HRM_NhanVien NguoiLaoDong)
        {
            string SMT = "";
            //if (NguoiLaoDong.SoChungMinhThu != "" && NguoiLaoDong.SoChungMinhThu != null)
            //{
            //    SMT = NguoiLaoDong.SoChungMinhThu + " do " + NguoiLaoDong.ChungMinhThuNoiCap + " cấp ngày ";
            //    if (NguoiLaoDong.ChungMinhThuNgayCap != null)
            //    {
            //        DateTime dt = NguoiLaoDong.ChungMinhThuNgayCap ?? DateTime.Now;
            //        SMT += dt.ToString("dd/MM/yyyy");
            //    }
            //}
            //else
            //{
            //    SMT = NguoiLaoDong.SoCanCuoc + " do " + NguoiLaoDong.CanCuocNoiCap + " cấp ngày ";
            //    if (NguoiLaoDong.CanCuocNgayCap != null)
            //    {
            //        DateTime dt = NguoiLaoDong.CanCuocNgayCap ?? DateTime.Now;
            //        SMT += dt.ToString("dd/MM/yyyy");
            //    }
            //}
            return SMT;
        }

        public dynamic ExportTiepNhanNhanVienDaiAn(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/QuyetDinhChinhThuc.docx");
            string sFileNameCopy = PathUploads + filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyEOS);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);

            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            string SMT = GetThongTinCMT(NguoiLaoDong);
            doc.ReplaceText("@CMT", SMT);
            string TenNguoiKy = db.HRM_NhanVien.Where(r => r.IdGuid == HopDong.IdNguoiDaiDien).Select(r => r.TenDayDu).FirstOrDefault();
            if (TenNguoiKy != null)
                doc.ReplaceText("@@TenNguoiKy", TenNguoiKy);
            else
                doc.ReplaceText("@@TenNguoiKy", "");

            doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
            if (NguoiLaoDong.HoKhauThuongTruDiaChi != null)
                doc.ReplaceText("@HoKhauThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi);
            else
                doc.ReplaceText("@HoKhauThuongTru", "");
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            if (TrinhDo != null)
                doc.ReplaceText("@TrinhDo", TrinhDo.Ten);
            else
                doc.ReplaceText("@TrinhDo", "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            string ChucVuBoPhan = ChucVuNLD.TenChucVu + " " + BoPhan.TenBoPhan;
            doc.ReplaceText("@ChucVuBoPhan", ChucVuBoPhan);
            doc.ReplaceText("@BoPhanNhanVien", BoPhan.TenBoPhan);

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("QuyetDinhChinhThuc.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportThongBaoKhongKyHopDong(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ThongBaokhongKyHopDong.docx");
            string sFileNameCopy = PathUploads + filenamegui + ".docx";

            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            dynamic data = new DynamicObject();


            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@TenNhanVien", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@NoiSinh", NguoiLaoDong.HoKhauThuongTruDiaChi);
            string SMT = GetThongTinCMT(NguoiLaoDong);

            doc.ReplaceText("@CMT", SMT);
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            if (TrinhDo != null)
                doc.ReplaceText("@TrinhDo", TrinhDo.Ten);
            else
                doc.ReplaceText("@TrinhDo", "");
            string ChucVuBoPhan = "";
            var ChucVuNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            ChucVuBoPhan = ChucVuNLD.Ten + " " + BoPhan.TenBoPhan;
            doc.ReplaceText("@ChucVuBoPhan", ChucVuBoPhan);
            doc.ReplaceText("@NgayBatDau", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
            if (HopDong.NgayHetHan != null)
            {
                doc.ReplaceText("@NgayKetThuc", HopDong.NgayHetHan.Value.ToString("dd/MM/yyyy"));
                doc.ReplaceText("@NgayBanGiao", HopDong.NgayHetHan.Value.AddDays(1).ToString("dd/MM/yyyy"));
            }
            doc.ReplaceText("@Phong", BoPhan.TenBoPhan);


            doc.Save();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("ThongBaokhongKyHopDong.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

            return data;
        }
        public dynamic ExportHopDongLaoDongPHT(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HD_TNHH.docx");
            string sFileNameCopy = HostingEnvironmentCompat.MapPath("~/Upload/") + filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var ChucVuNDD = _CallWebService.GetChucDanh(db, HopDong.NguoiDaiDienIdChucDanh);
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyEOS);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);

            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            var ChucVuNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();

            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);

            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
            doc.ReplaceText("@DienThoaiCongTy", CongTy.DienThoai);
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue);

            //    doc.ReplaceText("@Nam", HopDong.NgayKy.Year.ToString());
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKyHopDong", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "");
            if (ChucVuNDD != null)
                doc.ReplaceText("@ChucVu ", ChucVuNDD.Ten);
            else
                doc.ReplaceText("@ChucVu ", "");


            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien);
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@SoSoBHXH", NguoiLaoDong.SoBaoHiemXaHoi == null ? (HopDong.BaoHiem.SoSoBaoHiem ?? "") : NguoiLaoDong.SoBaoHiemXaHoi);

            if (NguoiLaoDong.HoKhauThuongTruDiaChi != null)
                doc.ReplaceText("@NguoiLaoDongThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi);
            else
                doc.ReplaceText("@NguoiLaoDongThuongTru", "");

            if (NguoiLaoDong.ChoOHienNayTruDiaChi != null)
                doc.ReplaceText("@NguoiLaoDongoHienTai", NguoiLaoDong.ChoOHienNayTruDiaChi);
            else
                doc.ReplaceText("@NguoiLaoDongoHienTai", "");
            string SMT = GetThongTinCMT(NguoiLaoDong);

            doc.ReplaceText("@CMTND", SMT);
            doc.ReplaceText("@ChucDanh", ChucVuNLD.Ten);

            var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            doc.ReplaceText("@ThoiGianHopDong", thoigianHopDong.Ten);
            string ThoiHanHopDong = "";
            if (HopDong.NgayHetHan == null)
            {
                ThoiHanHopDong = "Từ ngày " + HopDong.NgayHieuLuc.ToString("dd/MM/yyyy");
            }
            else
                ThoiHanHopDong = "Từ ngày " + HopDong.NgayHieuLuc.ToString("dd/MM/yyyy") + " đến hết ngày " + HopDong.NgayHetHan.Value.ToString("dd/MM/yyyy");
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);

            string LoaiLuong = "";
            if (HopDong.LoaiLuong == 1)
                LoaiLuong = "tháng";
            if (HopDong.LoaiLuong == 2)
                LoaiLuong = "ngày";
            if (HopDong.LoaiLuong == 3)
                LoaiLuong = "giờ";
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0") + "đông/" + LoaiLuong;
            doc.ReplaceText("@Luong", Luong);
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploaded/Get?filename=" + Base64Encode("HD_TNHH.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

            return data;
        }
        #region Import excel hợp đồng
        public dynamic ExportMauExcelHopDong(ApplicationDbContext db)
        {
            try
            {
                string sFile = "ImportHopDongNhanVien.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now;
                var filenamegui = "ImportHopDongNhanVien_" + DateTime.Now.ToOADate() + ".xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                CallWebService callWeb = _CallWebService;
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"]; ;
                var dmTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
                var ListChucDanh = _CallWebService.GetDanhSachChucDanh(db);
                int nRow = 3;

                foreach (var item in ListChucDanh)
                {
                    sheet1.Cells[nRow, 1].Value = item.Ten;
                    sheet1.Cells[nRow, 2].Value = item.Ma;
                    nRow++;
                }

                nRow = 3;
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                foreach (var item in listChucVu)
                {
                    sheet1.Cells[nRow, 3].Value = item.TenChucVu;
                    sheet1.Cells[nRow, 4].Value = item.MaChucVu;
                    nRow++;
                }
                var loaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                nRow = 3;
                foreach (var item in loaiHopDong)
                {
                    sheet1.Cells[nRow, 5].Value = item.Ten;
                    sheet1.Cells[nRow, 6].Value = item.Ma;
                    nRow++;
                }
                var ThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
                nRow = 3;
                foreach (var item in ThoiHanHopDong)
                {
                    sheet1.Cells[nRow, 7].Value = item.Ten;
                    sheet1.Cells[nRow, 8].Value = item.Ma;
                    nRow++;
                }
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var BoPhanLD = listBoPhan.FirstOrDefault(a => a.MaBoPhan == "BLD");

                var listcongTy = _CallWebService.GetDanhSachDuAn(db);
                nRow = 3;

                foreach (var item in listcongTy)
                {
                    sheet1.Cells[nRow, 9].Value = item.TenBoPhan;
                    sheet1.Cells[nRow, 10].Value = item.MaBoPhan;
                    nRow++;

                }

                nRow = 3;
                foreach (var item in listBoPhan)
                {
                    sheet1.Cells[nRow, 11].Value = item.TenBoPhan;
                    sheet1.Cells[nRow, 12].Value = item.MaBoPhan;
                    nRow++;
                }
                var danhMuc = _funcDanhMuc;
                var listLoaiThoiGian = danhMuc.GetDanhMucLoaiLuong();
                nRow = 3;
                foreach (var item in listLoaiThoiGian)
                {
                    sheet1.Cells[nRow, 13].Value = item.LoaiLuong;
                    sheet1.Cells[nRow, 14].Value = item.Id;
                    nRow++;
                }
                var listDonViCapbaoHiem = db.HRM_dmNoiCapBaoHiem.ToList();
                nRow = 3;
                foreach (var item in listDonViCapbaoHiem)
                {
                    sheet1.Cells[nRow, 15].Value = item.Ten;
                    sheet1.Cells[nRow, 16].Value = item.Ma;
                    nRow++;
                }
                var ListLoaiPhuCap = db.HRM_dmPhuCap.ToList();
                nRow = 3;
                foreach (var item in ListLoaiPhuCap)
                {
                    sheet1.Cells[nRow, 17].Value = item.Ten;
                    sheet1.Cells[nRow, 18].Value = item.Ma;
                    nRow++;

                }
                var ListLoaiTraLuong = db.HRM_dmHinhThucTraLuong.ToList();
                nRow = 3;
                foreach (var item in ListLoaiTraLuong)
                {
                    sheet1.Cells[nRow, 19].Value = item.Ten;
                    sheet1.Cells[nRow, 20].Value = item.Ma;
                    nRow++;

                }
                nRow = 3;
                listBoPhan = _CallWebService.GetCongTyKyHopDong(db);
                foreach (var item in listBoPhan)
                {
                    sheet1.Cells[nRow, 21].Value = item.TenBoPhan;
                    sheet1.Cells[nRow, 22].Value = item.MaBoPhan;
                    nRow++;
                }

                var listTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
                nRow = 3;
                foreach (var item in listTrinhDo)
                {
                    sheet1.Cells[nRow, 23].Value = item.Ten;
                    sheet1.Cells[nRow, 24].Value = item.Ma;
                    nRow++;

                }
                var listNhanVien = db.HRM_NhanVien.Where(a => a.IdBoPhanEOS == BoPhanLD.Id).ToList();
                nRow = 3;
                foreach (var item in listNhanVien)
                {
                    sheet1.Cells[nRow, 25].Value = item.TenDayDu;
                    sheet1.Cells[nRow, 26].Value = item.MaNhanVien;
                    nRow++;

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic ExportHopDongHetHan(ApplicationDbContext db)
        {
            try
            {
                string sFile = "DanhSachNhanVienHetHanHopDong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
                DateTime dt = DateTime.Now.AddMonths(1);
                DateTime time = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
                var filenamegui = "DanhSachNhanVienHetHanHopDong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                CallWebService callWeb = _CallWebService;
                var VoThoiHan = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.Ma == "KTH");
                var listData = db.HRM_HopDong.Where(a => a.isKetThuc == true
                && a.NgayHetHan != null && a.NgayHetHan <= time
                && a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).ToList();
                if (VoThoiHan != null)
                    listData = listData.Where(a => a.IdThoiHanHopDong != null && a.IdThoiHanHopDong != VoThoiHan.IdGuid).ToList();
                var listPhuCap = db.HRM_dmPhuCap.ToList();
                int CountPC = listPhuCap.Count;
                int col = 16;
                // header

                sheet1.Cells[3, 10, 3, 16 + CountPC].Merge = true;
                sheet1.Cells[4, 16, 5, 15 + CountPC].Merge = true;
                sheet1.Cells[4, 16].Value = "Phụ cấp áp dụng trong thời gian thử việc";
                foreach (var item in listPhuCap)
                {
                    sheet1.Cells[6, col].Value = item.Ten;
                    col++;
                }
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Tổng cộng";
                col++;
                sheet1.Cells[3, col, 3, col + CountPC + 1].Merge = true;
                sheet1.Cells[3, col].Value = "Cơ sở đề xuất";
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Mức lương chính thức";
                col++;
                sheet1.Cells[4, col, 5, col + CountPC - 1].Merge = true;
                sheet1.Cells[4, col].Value = "Phụ cấp TBP đề xuất mới";
                foreach (var item in listPhuCap)
                {
                    sheet1.Cells[6, col].Value = item.Ten;
                    col++;
                }
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Tổng cộng";
                col++;
                sheet1.Cells[3, col, 6, col].Merge = true;
                sheet1.Cells[3, col].Value = "Chênh lệch";
                col++;
                sheet1.Cells[3, col, 3, col + CountPC + 6].Merge = true;
                sheet1.Cells[3, col].Value = "Đề xuất hợp đồng lao động";
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Ngày bắt đầu HĐ";
                col++;
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Ngày kết thúc HĐ";
                col++;
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Loại hợp đồng";
                col++;
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Thời hạn hợp đồng";
                col++;
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Mức lương trên HĐ đề xuất";
                col++;
                sheet1.Cells[4, col, 5, col + CountPC - 1].Merge = true;
                sheet1.Cells[4, col].Value = "Phụ cấp";
                foreach (var item in listPhuCap)
                {
                    sheet1.Cells[6, col].Value = item.Ten;
                    col++;
                }
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "Tổng";
                col++;
                sheet1.Cells[4, col, 6, col].Merge = true;
                sheet1.Cells[4, col].Value = "So sánh Lương + PC và BHXH";
                col++;
                sheet1.Cells[3, col, 6, col].Merge = true;
                sheet1.Cells[3, col].Value = "Ghi chú";

                sheet1.Cells[1, 1, 1, col].Merge = true;
                //style
                dungchung.DrawTableExcel(sheet1, 3, 6, 1, col);
                sheet1.Cells[3, 1, 6, col].Style.Font.Bold = true;

                // data
                if (listData != null && listData.Count > 0)
                {
                    var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
                    var ChucVus = _CallWebService.GetDanhSachChucVu(db);
                    var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                    var listThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
                    var listCaLamViec = db.HRM_dmLichLamViec.ToList();

                    List<string> listIdHopDong = listData.Select(a => a.IdGuid).Distinct().ToList();
                    var listPhuCaps = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                    var listLuongs = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                    List<string> listIdNhanVien = listData.Select(a => a.IdNhanVien).Distinct().ToList();
                    var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai != DefineData.eTrangThaiNhanVien.DaNghiViec.ToString() && listIdNhanVien.Contains(a.IdGuid)).ToList();
                    var listDanhGia = (from a in db.HRM_DanhGiaNhanVien_Item
                                       join b in db.HRM_DanhGiaNhanVien on a.IdDanhGiaNhanVien equals b.IdGuid
                                       where b.isKetThuc == true && listIdNhanVien.Contains(a.IdNhanVien) && a.IdThuVienTieuChiDanhGia == "TRUNGBINH"
                                       orderby a.Modified descending
                                       select a).ToList();
                    int STT = 1;
                    int nRow = 7;
                    if (listNhanVien != null && listNhanVien.Count > 0)
                        foreach (var item in listNhanVien)
                        {
                            var HopDong = listData.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                            var PhuCaps = listPhuCaps.Where(a => HopDong.IdGuid == a.IdHopDong).ToList();
                            col = 1;
                            sheet1.Cells[nRow, col].Value = STT;
                            col++;
                            var CongTy = BoPhans.FirstOrDefault(a => a.Id == item.IdCongTyEOS);
                            if (CongTy != null)
                                sheet1.Cells[nRow, col].Value = CongTy.TenBoPhan;
                            col++;
                            sheet1.Cells[nRow, col].Value = item.MaNhanVien;
                            col++;
                            sheet1.Cells[nRow, col].Value = item.TenDayDu;
                            col++;
                            var BoPhan = BoPhans.FirstOrDefault(a => a.Id == item.IdBoPhanEOS);
                            if (BoPhan != null)
                                sheet1.Cells[nRow, col].Value = BoPhan.TenBoPhan;
                            col++;
                            var ChucVu = ChucVus.FirstOrDefault(a => a.Id == item.IdChucVu);
                            if (ChucVu != null)
                                sheet1.Cells[nRow, col].Value = ChucVu.TenChucVu;
                            col++;
                            double diemDanhGia = 0;
                            if (listDanhGia != null && listDanhGia.Count > 0)
                            {
                                var DanhGia = listDanhGia.Where(a => a.IdNhanVien == item.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                                if (DanhGia != null)
                                {
                                    sheet1.Cells[nRow, col].Value = DanhGia.Diem;
                                    diemDanhGia = (double)DanhGia.Diem;
                                }
                            }
                            col++;
                            if (diemDanhGia > 0)
                                sheet1.Cells[nRow, col].Value = DefineData.QuyDoiXepLoai(diemDanhGia);
                            col++;
                            col++;
                            if (item.NgayBatDauLamViec != null)
                                sheet1.Cells[nRow, col].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                            col++;
                            if (HopDong.NgayHetHan != null)
                                sheet1.Cells[nRow, col].Value = HopDong.NgayHetHan.Value.ToString("dd/MM/yyyy");
                            col++;
                            if (HopDong.IdLoaiHopDong != null)
                            {
                                var LoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                                if (LoaiHopDong != null)
                                    sheet1.Cells[nRow, col].Value = LoaiHopDong.Ten;
                            }
                            col++;
                            if (HopDong.IdThoiHanHopDong != null)
                            {
                                var ThoiHanHopDong = listThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
                                if (ThoiHanHopDong != null)
                                    sheet1.Cells[nRow, col].Value = ThoiHanHopDong.Ten;
                            }
                            col++;
                            if (HopDong.IdThoiGianLamViec != null)
                            {
                                var calamViec = listCaLamViec.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiGianLamViec);
                                if (calamViec != null)
                                    sheet1.Cells[nRow, col].Value = calamViec.Ma;
                            }
                            col++;
                            var Luong = listLuongs.FirstOrDefault(a => a.IdHopDong == HopDong.IdGuid);
                            if (Luong != null)
                            {
                                sheet1.Cells[nRow, col].Value = Luong.LuongBaoHiem;
                            }
                            col++;
                            double SoTienPC = 0;
                            foreach (var PC in listPhuCap)
                            {
                                var itemPc = PhuCaps.FirstOrDefault(a => a.IddmLoaiPhuCap == PC.IdGuid);
                                if (itemPc != null)
                                {
                                    sheet1.Cells[nRow, col].Value = itemPc.SoTien;
                                    SoTienPC += (itemPc.SoTien);
                                }
                                col++;
                            }
                            sheet1.Cells[nRow, col].Value = SoTienPC;
                            nRow++;
                            STT++;
                        }
                    dungchung.DrawTableExcel(sheet1, 3, nRow, 1, col + listPhuCap.Count * 2 + 11);
                    sheet1.Cells[7, 15, nRow, col + listPhuCap.Count * 2 + 11].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                }

                sheet1.Cells.Style.WrapText = true;
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                dynamic dnm = new DynamicObject();
                dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                                + "&path=" + Base64Encode(sFileNameCopy);
                return dnm;
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public MessageReturn ImportExcelHopDong(string aFileName, ApplicationDbContext dbContext, bool CheckDuLieuCu = true, bool buocCuoi = false)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            int nRowError = 0;
            int nColError = 0;
            ExcelWorksheet sheetError = null;
            try
            {

                string pathTemplate = PathUploads + aFileName;
                if (File.Exists(pathTemplate) == false)
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);
                s.Close();
                #region Thông tin chung
                var sheet1 = package.Workbook.Worksheets["Thông tin chung"];
                int row = 3;
                int maxrow = sheet1.Dimension.Rows;

                var listNhanVien = dbContext.HRM_NhanVien.AsNoTracking().ToList();
                var listTrinhDoChuyenMon = dbContext.HRM_dmTrinhDoDaoTao.ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(dbContext);
                //  var listChucVu = dbContext.HRM_dmChucVu.ToList();
                var listChucVuCongViec = _CallWebService.GetDanhSachChucVu(dbContext);
                var listCapBac = dbContext.HRM_dmCapBac.ToList();
                var listLichLamViec = dbContext.HRM_dmLichLamViec.ToList();
                var listLoaiHopDong = dbContext.HRM_dmLoaiHopDong.ToList();
                var listHinhThucTraLuong = dbContext.HRM_dmHinhThucTraLuong.ToList();
                var listCtyKy = _CallWebService.GetCongTyKyHopDong(dbContext);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(dbContext);
                var listCongTy = _CallWebService.GetDanhSachDuAn(dbContext);
                List<NguoiDaiDienCongTyViewModel> lstNguoiDaiDien = new List<NguoiDaiDienCongTyViewModel>();
                var listThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.ToList();
                var listHopDong = new List<HRM_HopDong>();
                var listErr = new List<string>();

                for (int i = row; i <= maxrow; i++)
                {

                    var model = new HRM_HopDong();
                    var STT = 2;
                    model.MaNhanVien = dungchung.GetCell(sheet1, i, STT);
                    //model.IdGuid = Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(model.MaNhanVien))

                        break;

                    if (listNhanVien.IsNotEmpty())
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == model.MaNhanVien);
                        if (nhanVien == null)
                        {
                            listErr.Add(model.MaNhanVien);
                            continue;
                        }
                        model.IdNhanVien = nhanVien.IdGuid;
                    }
                    STT++;//3
                    STT++; //4
                    string maTrinhDo = dungchung.GetCell(sheet1, i, STT);
                    if (listTrinhDoChuyenMon.IsNotEmpty())
                    {
                        if (maTrinhDo.IsNotEmpty())
                        {
                            var trinhDo = listTrinhDoChuyenMon.FirstOrDefault(x => x.Ma == maTrinhDo);
                            if (trinhDo != null)
                                model.IddmTrinhDoDaoTao = trinhDo.IdGuid;
                        }
                    }
                    STT++; //5 mã chức danh
                    STT++; //5 mã chức danh
                    var maChucDanhCongViec = dungchung.GetCell(sheet1, i, STT);
                    if (listChucDanh.IsNotEmpty())
                    {
                        if (maChucDanhCongViec.IsNotEmpty())
                        {
                            var ChucDanhCongViec = listChucDanh.FirstOrDefault(x => x.Ma == maChucDanhCongViec);
                            if (ChucDanhCongViec == null)
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã chức vụ ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            else
                                model.IddmChucDanh = ChucDanhCongViec.Id;
                        }
                    }
                    //var maChucVu = dungchung.GetCell(sheet1, i, STT);
                    STT++;//6 chức vụ công việc
                    STT++;//6 chức vụ công việc
                    var maChucVuCongViec = dungchung.GetCell(sheet1, i, STT);
                    if (listChucVuCongViec.IsNotEmpty())
                    {
                        if (maChucVuCongViec.IsNotEmpty())
                        {
                            var chucVuCongViec = listChucVuCongViec.FirstOrDefault(x => x.MaChucVu == maChucVuCongViec);
                            if (chucVuCongViec == null)
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã chức vụ ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            else
                                model.IddmChucVu = chucVuCongViec.Id;
                        }
                    }
                    STT++;//7
                    STT++;//7
                    var maCapBac = dungchung.GetCell(sheet1, i, STT);

                    if (listCapBac.IsNotEmpty())
                    {
                        if (maCapBac.IsNotEmpty())
                        {
                            var capBac = listCapBac.FirstOrDefault(x => x.Ma == maCapBac);
                            if (capBac != null)
                                model.IdCapBac = capBac.IdGuid;
                        }
                    }
                    STT++;//8
                    model.DiaDiemLamViec = dungchung.GetCell(sheet1, i, STT);
                    STT++;
                    var maBoPhan = dungchung.GetCell(sheet1, i, STT);
                    if (listBoPhan.IsNotEmpty())
                    {
                        if (maBoPhan.IsNotEmpty())
                        {
                            var BoPhan = listBoPhan.FirstOrDefault(x => x.MaBoPhan == maBoPhan);
                            if (BoPhan != null)
                                model.IdBoPhanEOS = BoPhan.Id;
                        }
                    }
                    STT++;//8
                    STT++;//8
                    var maCongTy = dungchung.GetCell(sheet1, i, STT);
                    model.listIdCongTy = new List<long>();
                    if (listCongTy.IsNotEmpty())
                    {
                        if (maCongTy.IsNotEmpty())
                        {
                            List<string> listMaCT = maCongTy.Split(';').ToList();
                            var CongTys = listCongTy.Where(a => listMaCT.Contains(a.MaBoPhan)).ToList();
                            if (CongTys != null)
                                foreach (var item in CongTys)
                                {
                                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                                    bp.IdGuid = Guid.NewGuid().ToString();
                                    bp.IdQuyTrinh = model.IdGuid;
                                    bp.IdBoPhan = item.Id;
                                    bp.TableName = nameof(HRM_HopDong);
                                    dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                                    model.listIdCongTy.Add(item.Id);
                                }
                        }
                    }
                    STT++;//8
                    var MaCaLamViec = dungchung.GetCell(sheet1, i, STT);
                    if (!string.IsNullOrEmpty(MaCaLamViec))
                    {
                        List<string> list = MaCaLamViec.Split('-').ToList();
                        model.listIdCaLamViec = listLichLamViec.Where(a => list.Contains(a.Ma)).Select(a => a.IdGuid).Distinct().ToList();
                        model.IdThoiGianLamViec = string.Join(",", model.listIdCaLamViec);
                    }
                    STT++; //9
                    model.SoHopDong = dungchung.GetCell(sheet1, i, STT);
                    STT++; //9
                    model.SoHopDongGoc = dungchung.GetCell(sheet1, i, STT);
                    if (!string.IsNullOrEmpty(model.SoHopDongGoc))
                        model.isPhuLuc = true;
                    STT++; //10
                    string LoaiHopDong = dungchung.GetCell(sheet1, i, STT);
                    if (listLoaiHopDong.IsNotEmpty())
                    {
                        if (LoaiHopDong.IsNotEmpty())
                        {
                            var LHopDong = listLoaiHopDong.FirstOrDefault(x => x.Ma == LoaiHopDong);
                            if (LHopDong != null)
                                model.IdLoaiHopDong = LHopDong.IdGuid;
                        }
                    }
                    STT++; //11
                    STT++; //11
                    string ThoiHanHopDong = dungchung.GetCell(sheet1, i, STT);
                    if (listThoiHanHopDong.IsNotEmpty())
                    {
                        if (ThoiHanHopDong.IsNotEmpty())
                        {
                            var LHopDong = listThoiHanHopDong.FirstOrDefault(x => x.Ma == ThoiHanHopDong);
                            if (LHopDong != null)
                                model.IdThoiHanHopDong = LHopDong.IdGuid;
                        }
                    }
                    STT++;//12
                    STT++;//12
                    try
                    {
                        model.NgayKy = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        model.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKy);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Sheet " + sheet1.Name + " dòng  " + i + " Ngày ký");
                        //model.NgayHieuLuc = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        //model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
                    }
                    STT++;//13
                    try
                    {
                        model.NgayHieuLuc = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Sheet " + sheet1.Name + " dòng  " + i + " Ngày hiệu lực");
                        //model.NgayHieuLuc = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        //model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
                    }
                    STT++;//14
                    try
                    {
                        model.NgayHetHan = dungchung.GetCellToDateTime(sheet1, i, STT);
                        if (model.NgayHetHan != null)
                            model.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHetHan);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày hết hạn dòng số " + i + " sheet thông tin chung ");
                    }
                    STT++;//15
                    var maCongTyKy = dungchung.GetCell(sheet1, i, STT);


                    if (!string.IsNullOrEmpty(maCongTyKy))
                    {
                        var CongTy = listCtyKy.FirstOrDefault(x => x.MaBoPhan == maCongTyKy);
                        if (CongTy == null)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Công ty ký hợp đồng ở nhân viên " + model.MaNhanVien + " không tồn tại");
                        else
                        {
                            model.IdCongTyKyHopDong = CongTy.Id;

                        }
                    }
                    else
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Công ty ký hợp đồng ở nhân viên " + model.MaNhanVien + " không được để trống");
                    }

                    STT++;//16
                    STT++;//16
                    var MaNhanVienDaiDien = dungchung.GetCell(sheet1, i, STT);
                    if (listNhanVien.IsNotEmpty())
                    {
                        if (MaNhanVienDaiDien.IsNotEmpty())
                        {
                            var nhanVienDaiDien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == MaNhanVienDaiDien);
                            if (nhanVienDaiDien == null)
                            {
                                listErr.Add(model.MaNhanVien);
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Người đại diện ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            }

                            model.IdNguoiDaiDien = nhanVienDaiDien.IdGuid;
                            model.NguoiDaiDienIdQuocTich = nhanVienDaiDien.IdQuocTich;
                            model.NguoiDaiDienIdChucDanh = nhanVienDaiDien.IdChucDanh;
                        }
                    }


                    STT++;

                    STT++;
                    string QToanPhepNam = dungchung.GetCell(sheet1, i, STT);
                    if (QToanPhepNam == "CO" || QToanPhepNam == "Co" || QToanPhepNam == "co")
                    {
                        model.isQuyetToanPhepNam = true;
                    }
                    STT++;
                    model.PhiCongDoan = dungchung.GetCellToDouble(sheet1, i, STT);
                    if (model.PhiCongDoan > 0)
                    {
                        model.isThamGiaCongDoan = true;
                    }

                    STT++;
                    model.SoNgayNghiPhep = dungchung.GetCellToDouble(sheet1, i, STT);
                    STT++;
                    try
                    {
                        model.NgayChuyenKhongThoiHan = dungchung.GetCellToDateTime(sheet1, i, STT);
                        if (model.NgayChuyenKhongThoiHan != null)
                            model.NgayChuyenKhongThoiHanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayChuyenKhongThoiHan);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày chuyển không thời hạn dòng số " + i + " sheet thông tin chung ");
                    }

                    listHopDong.Add(model);
                }

                if (listErr.IsNotEmpty())
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, $"Các mã nhân viên này không có trong dữ liệu {listErr.Join(",")}");
                #endregion
                /// thông tin liên quan
                var HopDong = new HRM_HopDong();
                var Sheetluong = package.Workbook.Worksheets["Lương"];
                var SheetphuCap = package.Workbook.Worksheets["Hỗ trợ"];
                var SheetbaoHiem = package.Workbook.Worksheets["Bảo hiểm"];
                var SheetbaoHiemCN = package.Workbook.Worksheets["Bảo hiểm con người"];
                var SheetcongViecs = package.Workbook.Worksheets["Công việc"];
                var SheetthietBiBanGiaos = package.Workbook.Worksheets["Thiết bị công việc"];
                var SheetKhamSucKhoe = package.Workbook.Worksheets["Khám Sức khỏe"];

                var listLuongHopDong = new List<HRM_Luong_HopDong>();
                var listLuongBHHopDong = new List<HRM_LuongBaoHiem_HopDong>();
                for (int i = 3; i <= Sheetluong.Dimension.Rows; i++)
                {
                    var LuongHopDong = new HRM_Luong_HopDong();
                    var STT = 2;
                    LuongHopDong.SoHopDong = dungchung.GetCell(Sheetluong, i, STT);
                    LuongHopDong.MaNhanVien = dungchung.GetCell(Sheetluong, i, STT + 1);
                    if (string.IsNullOrEmpty(LuongHopDong.SoHopDong) && string.IsNullOrEmpty(LuongHopDong.MaNhanVien))
                        break;
                    STT = STT + 3;
                    LuongHopDong.MaSoThueTNCN = dungchung.GetCell(Sheetluong, i, STT);

                    STT++;// 5 tỷ lê hưởng lương

                    LuongHopDong.TyLeHuongLuong = dungchung.GetCellToDouble(Sheetluong, i, STT);
                    STT++;
                    LuongHopDong.KieuLuong = dungchung.GetCellToInt(Sheetluong, i, STT);

                    STT++;//7 Hinh thức trả lương
                    var hinhThucTraLuong = dungchung.GetCell(Sheetluong, i, STT);

                    if (hinhThucTraLuong.IsNotEmpty())
                    {
                        LuongHopDong.IddmHinhThucTraLuong = listHinhThucTraLuong.Where(a => a.Ma == hinhThucTraLuong).Select(a => a.IdGuid).FirstOrDefault();
                    }
                    STT++;
                    STT++;
                    LuongHopDong.SoTaiKhoan = dungchung.GetCell(Sheetluong, i, STT);
                    STT++;//9 Ngân hàng
                    LuongHopDong.TenNganHang = dungchung.GetCell(Sheetluong, i, STT);

                    STT++; // 10 lương 
                    LuongHopDong.LuongBaoHiem = dungchung.GetCellToDouble(Sheetluong, i, STT);
                    STT++; // 10 lương 
                    LuongHopDong.LoaiLuong = dungchung.GetCellToInt(Sheetluong, i, STT);


                    listLuongHopDong.Add(LuongHopDong);
                }
                var listDMPhuCap = dbContext.HRM_dmPhuCap.ToList();


                var lstPhuCap = new List<HRM_HopDongPhuCap>();
                for (int i = 2; i <= SheetphuCap.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongPhuCap();

                    int STT = 2;
                    model.SoHopDong = dungchung.GetCell(SheetphuCap, i, STT);
                    model.MaNhanVien = dungchung.GetCell(SheetphuCap, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;

                    STT = STT + 3;

                    var maDMPhuCap = dungchung.GetCell(SheetphuCap, i, STT);

                    if (maDMPhuCap.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
                    {
                        var dmPhuCap = listDMPhuCap.FirstOrDefault(x => x.Ma == maDMPhuCap);
                        if (dmPhuCap != null)
                        {
                            model.IddmLoaiPhuCap = dmPhuCap.IdGuid;
                        }
                    }
                    STT++;//5
                    STT++;//5
                    model.SoTien = dungchung.GetCellToDouble(SheetphuCap, i, STT);

                    STT++;//6
                    model.NgayBatDau = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
                    model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
                    STT++;//7
                    model.NgayKetThuc = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
                    model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
                    STT++;//7
                    model.LoaiPhuCap = dungchung.GetCellToInt(SheetphuCap, i, STT);
                    //chưa xác định

                    lstPhuCap.Add(model);
                }
                var lstBaoHiem = new List<HRM_BaoHiem_HopDong>();
                var lstBaoHiemNgoai = new List<HRM_BaoHiem_HopDong>();
                var listNoiCapBH = dbContext.HRM_dmNoiCapBaoHiem.ToList();
                for (int i = 3; i <= SheetbaoHiem.Dimension.Rows; i++)
                {


                    var BaoHiem = new HRM_BaoHiem_HopDong();
                    int STT = 2;
                    BaoHiem.SoHopDong = dungchung.GetCell(SheetbaoHiem, i, STT);

                    BaoHiem.MaNhanVien = dungchung.GetCell(SheetbaoHiem, i, STT + 1);
                    if (string.IsNullOrEmpty(BaoHiem.SoHopDong) && string.IsNullOrEmpty(BaoHiem.MaNhanVien))
                        break;
                    STT = STT + 3;
                    string BHXH = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHXH == "1" || BHXH.ToLower() == "true")
                        BaoHiem.isBaoHiemXaHoi = true;
                    STT++;//5
                    BaoHiem.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
                    STT++;// 6 ngày kết thúc
                    BaoHiem.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayKetThuc);
                    STT++;//7 Sô sổ bảo hiểm
                    BaoHiem.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
                    STT++;//8 ngày cấp
                    BaoHiem.NgayCapSoBaoHiem = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayCapSoBaoHiemUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayCapSoBaoHiem);
                    STT++;// 9 nơi cấp
                    BaoHiem.NoiCapSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
                    STT++;
                    string BHYT = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHYT == "1" || BHYT.ToLower() == "true")
                        BaoHiem.isBaoHiemYTe = true;
                    STT++;
                    string BHTN = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHTN == "1" || BHTN.ToLower() == "true")
                        BaoHiem.isBaoHiemThatNghiep = true;
                    STT++;
                    string PhucLoi = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (PhucLoi == "1" || PhucLoi.ToLower() == "true")
                    {
                        var HopDongBH = listHopDong.FirstOrDefault(a => a.SoHopDong == BaoHiem.SoHopDong);
                        HopDongBH.isHuongPhucLoi = true;
                    }
                    lstBaoHiem.Add(BaoHiem);
                    STT++;
                    HRM_LuongBaoHiem_HopDong LuongBH = new HRM_LuongBaoHiem_HopDong();
                    LuongBH.LuongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiem, i, STT);
                    LuongBH.SoHopDong = BaoHiem.SoHopDong;
                    LuongBH.MaNhanVien = BaoHiem.MaNhanVien;
                    listLuongBHHopDong.Add(LuongBH);

                }
                if (SheetbaoHiemCN != null)
                {
                    sheetError = SheetbaoHiemCN;
                    for (int i = 3; i <= SheetbaoHiemCN.Dimension.Rows; i++)
                    {
                        nRowError = i;
                        int STT = 2;
                        var BaoHiemNgoai = new HRM_BaoHiem_HopDong();
                        BaoHiemNgoai.SoHopDong = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetbaoHiemCN, i, STT + 1);
                        if (string.IsNullOrEmpty(BaoHiemNgoai.SoHopDong) && string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
                            break;
                        STT = STT + 3;
                        nColError = STT;
                        BaoHiemNgoai.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDau);
                        STT++;
                        nColError = STT;
                        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                        STT++;
                        BaoHiemNgoai.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        STT++;
                        string MaDonVi = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        if (!string.IsNullOrEmpty(MaDonVi))
                            BaoHiemNgoai.NoiCapSoBaoHiem = listNoiCapBH.Where(a => a.Ma == MaDonVi).Select(a => a.IdGuid).FirstOrDefault();
                        STT++;
                        STT++;
                        BaoHiemNgoai.MucDongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                        STT++;
                        BaoHiemNgoai.SoTienBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                        STT++;
                        string Giam = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        if (Giam.ToUpper() == "CÓ")
                            BaoHiemNgoai.Giam = 1;
                        else
                            BaoHiemNgoai.Giam = 2;
                        STT++;
                        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        STT++;
                        lstBaoHiemNgoai.Add(BaoHiemNgoai);
                    }
                }
                var lstKhamSucKhoe = new List<HRM_HopDong_KhamSucKhoe>();
                if (SheetKhamSucKhoe != null)
                {
                    sheetError = SheetKhamSucKhoe;
                    for (int i = 4; i <= SheetKhamSucKhoe.Dimension.Rows; i++)
                    {
                        nRowError = i;
                        var BaoHiemNgoai = new HRM_HopDong_KhamSucKhoe();
                        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetKhamSucKhoe, i, 2);
                        if (string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
                            break;
                        nColError = 4;
                        BaoHiemNgoai.NgayKham = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 4);
                        BaoHiemNgoai.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKham);
                        nColError = 5;
                        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 5);
                        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                        BaoHiemNgoai.NoiKham = dungchung.GetCell(SheetKhamSucKhoe, i, 6);
                        BaoHiemNgoai.LoaiHinhKham = dungchung.GetCell(SheetKhamSucKhoe, i, 7);
                        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetKhamSucKhoe, i, 8);
                        lstKhamSucKhoe.Add(BaoHiemNgoai);
                    }
                }
                var listMoTaCongViec = new List<HRM_HopDongCongViec>();
                for (int i = 2; i <= SheetcongViecs.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongCongViec();
                    int STT = 1;
                    model.IdGuid = Guid.NewGuid().ToString();
                    model.STT = dungchung.GetCell(SheetcongViecs, i, STT);
                    STT++;
                    model.STT_Cha = dungchung.GetCell(SheetcongViecs, i, STT);
                    STT++;

                    model.SoHopDong = dungchung.GetCell(SheetcongViecs, i, STT);

                    model.MaNhanVien = dungchung.GetCell(SheetcongViecs, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;
                    STT = STT + 3;
                    // 2 Tên công việc
                    model.TenCongViec = dungchung.GetCell(SheetcongViecs, i, STT);
                    if (string.IsNullOrEmpty(model.TenCongViec))
                        break;
                    STT++;// 3 mô tả
                    model.MoTa = dungchung.GetCell(SheetcongViecs, i, STT);
                    listMoTaCongViec.Add(model);
                }

                var listTrangThietBi = new List<HRM_HopDongTaiSanBanGiao>();
                var dmTrangThietBi = dbContext.HRM_dmTrangThietBi.ToList();
                for (int i = 2; i <= SheetthietBiBanGiaos.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongTaiSanBanGiao();
                    int STT = 2;
                    model.SoHopDong = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

                    model.MaNhanVien = dungchung.GetCell(SheetthietBiBanGiaos, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;
                    STT = STT + 3;
                    string maThietBi = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);
                    if (maThietBi.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
                    {
                        var TrangThietBi = dmTrangThietBi.FirstOrDefault(x => x.Ma == maThietBi);
                        if (TrangThietBi != null)
                        {
                            model.IddmTrangThaiThietBi = TrangThietBi.IdGuid;
                        }
                        else
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"mã trang thiết bị không tồn tại : " + maThietBi);
                        }
                    }

                    STT++;
                    STT++;//  4 số luọng
                    model.SoLuong = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);


                    STT++;// 5 giá trk
                    model.GiaTri = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);

                    STT++;// 6 mô tả
                    model.MoTa = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

                    listTrangThietBi.Add(model);
                }


                List<HRM.Api.Models.Link_NhanVien_BoPhan_ChucVuNonSQL> listAddChucVu = new List<HRM.Api.Models.Link_NhanVien_BoPhan_ChucVuNonSQL>();
                List<HRM_NhanVien> listNhanVienUpdate = new List<HRM_NhanVien>();
                List<string> listSoHopDong = listHopDong.Where(a => !string.IsNullOrEmpty(a.SoHopDong)).Select(a => a.SoHopDong).Distinct().ToList();
                var listHopDongDB = dbContext.HRM_HopDong.Where(a => listSoHopDong.Contains(a.SoHopDong)).ToList();
                if (listHopDongDB != null && listHopDongDB.Count > 0)
                {
                    listSoHopDong = listHopDongDB.Where(a => !string.IsNullOrEmpty(a.SoHopDong)).Select(a => a.SoHopDong).Distinct().ToList();
                    listHopDong = listHopDong.Where(a => !listSoHopDong.Contains(a.SoHopDong)).ToList();
                    //new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
                }
                var listLuongdb = new List<HRM_Luong_HopDong>();
                var listLuongBHdb = new List<HRM_LuongBaoHiem_HopDong>();
                var listBHdb = new List<HRM_BaoHiem_HopDong>();
                var listPhuCapdb = new List<HRM_HopDongPhuCap>();
                var listMoTaCongViecdb = new List<HRM_HopDongCongViec>();
                List<string> listSoHopDongGoc = listHopDong.Where(a => !string.IsNullOrEmpty(a.SoHopDongGoc)).Select(a => a.SoHopDongGoc).Distinct().ToList();
                var listHopDongGoc = new List<HRM_HopDong>();
                if (listSoHopDongGoc != null && listSoHopDongGoc.Count > 0)
                {
                    listHopDongGoc = dbContext.HRM_HopDong.Where(a => listSoHopDongGoc.Contains(a.SoHopDong)).ToList();
                    if (listSoHopDongGoc.Count > listHopDongGoc.Count)
                    {
                        var listSHD = listHopDongGoc.Select(a => a.SoHopDong).Distinct().ToList();
                        var listCheck = listSoHopDongGoc.Where(a => !listSHD.Contains(a)).ToList();
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Các số hợp đồng gốc không có trong dữ liệu" + listCheck.Join(", "));
                    }
                }
                if (listMoTaCongViec.Count > 0)
                {
                    foreach (var item in listMoTaCongViec)
                    {
                        var parent = listMoTaCongViec.FirstOrDefault(a => a.STT == item.STT_Cha);
                        if (parent != null)
                            item.IdParent = parent.IdGuid;
                    }
                }
                foreach (var item in listHopDong)
                {
                    item.LuongHopDong = new HRM_Luong_HopDong();
                    if (listLuongHopDong != null)
                        item.LuongHopDong = listLuongHopDong.FirstOrDefault(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong));
                    if (item.LuongHopDong != null)
                        item.LoaiLuong = item.LuongHopDong.LoaiLuong;
                    item.LuongBaoHiem = new HRM_LuongBaoHiem_HopDong();
                    if (listLuongHopDong != null)
                        item.LuongBaoHiem = listLuongBHHopDong.FirstOrDefault(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong));

                    item.BaoHiem = new HRM_BaoHiem_HopDong();
                    if (lstBaoHiem != null)
                        item.BaoHiem = lstBaoHiem.FirstOrDefault(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong));
                    item.BaoHiemNgoai = new HRM_BaoHiem_HopDong();
                    if (lstBaoHiemNgoai != null)
                        item.BaoHiemNgoai = lstBaoHiemNgoai.FirstOrDefault(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong));

                    item.lstPhuCap = new List<HRM_HopDongPhuCap>();
                    if (lstPhuCap != null)
                        item.lstPhuCap = lstPhuCap.Where(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong)).ToList();
                    item.listMoTaCongViec = new List<ReturnData<HRM_HopDongCongViec>>();
                    if (listMoTaCongViec != null)
                    {
                        item.listMoTaCongViec = listMoTaCongViec.Where(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong) && string.IsNullOrEmpty(a.IdParent))
                            .Select(a => new ReturnData<HRM_HopDongCongViec>
                            {
                                data = a,
                                children = new List<ReturnData<HRM_HopDongCongViec>>()
                            }).ToList();
                        foreach (var cv in item.listMoTaCongViec)
                            HopDongCongViecToTree(listMoTaCongViec, cv);
                    }
                    item.listTrangThietBi = new List<HRM_HopDongTaiSanBanGiao>();
                    if (listTrangThietBi != null)
                        item.listTrangThietBi = listTrangThietBi.Where(a => (string.IsNullOrEmpty(a.SoHopDong) && a.MaNhanVien == item.MaNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong == item.SoHopDong)).ToList();
                    item.listKhamSucKhoe = new List<HRM_HopDong_KhamSucKhoe>();
                    if (lstKhamSucKhoe != null)
                        item.listKhamSucKhoe = lstKhamSucKhoe.Where(a => a.MaNhanVien == item.MaNhanVien).ToList();
                    if (!string.IsNullOrEmpty(item.SoHopDongGoc))
                        item.IdHopDong = listHopDongGoc.Where(a => a.SoHopDong == item.SoHopDongGoc).Select(a => a.IdGuid).FirstOrDefault();
                    item.NgayKyUnix = DateTimeToUnixTimeStamp(item.NgayKy);
                    if (item.NgayHetHan != null)
                        item.NgayHetHanUnix = DateTimeToUnixTimeStamp(item.NgayHetHan);
                    item.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayHieuLuc);
                    if (buocCuoi == false)
                    {
                        bool err = FuncUpdateHopDong(item, dbContext, result, eQuyTrinh_LoaiHam.ChuyenTiep);

                        if (err != true)
                            return result;
                    }
                    else
                    {
                        bool err = FuncUpdateHopDongBuocCuoi(item, dbContext, result);

                        if (err != true)
                            return result;

                    }
                }
                _CallWebService.AddModify(dbContext, DefineData.QuyDoiTenQuyTrinh(DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString()), DefineData.eModify.Import.ToString(), DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), aFileName, aFileName, "", JsonConvert.SerializeObject(listHopDong), 0, dbContext.currentUser.Id, dbContext.currentUser.TenNhanVien);

                //if (buocCuoi == true)
                //{
                //    if (listNhanVienUpdate != null && listNhanVienUpdate.Count > 0)
                //    {

                //        dbContext.SaveChanges();
                //    }
                //    var dongboBoPhanNhanVien = _CallWebService.AddlistChucVuNhanSu(dbContext, listAddChucVu);
                //}
            }

            catch (Exception exp)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu bị sai định dạng: sheet " + sheetError.Name + " dòng " + nRowError + " cột " + nColError);
            }
            return result;
        }
        public MessageReturn ImportExcelHopDongFORLUONG(string aFileName, ApplicationDbContext dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            int nRowError = 0;
            int nColError = 0;
            ExcelWorksheet sheetError = null;
            try
            {

                string pathTemplate = PathUploads + aFileName;
                if (File.Exists(pathTemplate) == false)
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);
                s.Close();
                var listHinhThucTraLuong = dbContext.HRM_dmHinhThucTraLuong.ToList();


                /// thông tin liên quan
                var Sheetluong = package.Workbook.Worksheets["Bảo hiểm"];

                var listLuongHopDong = new List<HRM_LuongBaoHiem_HopDong>();

                for (int i = 3; i <= Sheetluong.Dimension.Rows; i++)
                {
                    var LuongHopDong = new HRM_LuongBaoHiem_HopDong();
                    var STT = 2;
                    LuongHopDong.SoHopDong = dungchung.GetCell(Sheetluong, i, STT);
                    LuongHopDong.MaNhanVien = dungchung.GetCell(Sheetluong, i, STT + 1);
                    if (string.IsNullOrEmpty(LuongHopDong.SoHopDong) && string.IsNullOrEmpty(LuongHopDong.MaNhanVien))
                        break;
                    LuongHopDong.LuongBaoHiem = dungchung.GetCellToDouble(Sheetluong, i, 14);
                    STT++; // 10 lương 
                    //LuongHopDong.LoaiLuong = dungchung.GetCellToInt(Sheetluong, i, STT);


                    listLuongHopDong.Add(LuongHopDong);
                }
                var listSHD = listLuongHopDong.Select(a => a.SoHopDong).Distinct().ToList();

                var listHopDong = (from a in dbContext.HRM_HopDong
                                   where listSHD.Contains(a.SoHopDong)

                                   select a
                                    ).ToList();
                var listIdHopDong = listHopDong.Select(a => a.IdGuid).ToList();
                var listLuongNhanVien = dbContext.HRM_LuongBaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                if (listLuongNhanVien != null && listLuongNhanVien.Count > 0)
                    dbContext.HRM_LuongBaoHiem_HopDong.RemoveRange(listLuongNhanVien);
                foreach (var item in listLuongHopDong)
                {

                    var HopDong = listHopDong.Where(a => a.SoHopDong == item.SoHopDong).FirstOrDefault();
                    if (HopDong == null)
                        continue;
                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdHopDong = HopDong.IdGuid;
                    item.IdNhanVien = HopDong.IdNhanVien;
                    dbContext.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Added;

                }
                dbContext.SaveChanges();
            }

            catch (Exception exp)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu bị sai định dạng: sheet " + sheetError.Name + " dòng " + nRowError + " cột " + nColError);
            }
            return result;
        }

        public dynamic ExportExcelHopDongMau(ApplicationDbContext db, TimKiem timKiem)
        {
            string sFile = "ImportHopDongNhanVien.xlsx";

            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);
            DateTime dt = DateTime.Now;
            var filenamegui = "ImportHopDongNhanVien_" + DateTime.Now.ToOADate() + ".xlsx";
            string sFileNameCopy = PathUploads + filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            CallWebService callWeb = _CallWebService;
            #region data

            //IEnumerable<HRM_HopDong> result = null;
            IQueryable<HRM_HopDong> result;
            if (timKiem.TabTrangThai != 3)

                result = (from a in db.HRM_HopDong

                          join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
                          where b.isDaXuLy == (timKiem.TabTrangThai - 1)
                          && b.IdUser == db.currentUser.Id
                          orderby a.Created descending
                          select a);
            else
                result = from a in db.HRM_HopDong
                         orderby a.Created descending
                         select a;
            //if(timKiem.TabTrangThai!=1)
            //    result=(from a in db.HRM_HopDong
            //                                   join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
            //                                   where b.isDaXuLy == (timKiem.TabTrangThai - 1)
            //                                   && b.IdUser == db.currentUser.Id
            //                                   group a by a.IdNhanVien into g
            //                                   select g.OrderByDescending(c => c.isKetThuc).ThenByDescending(a => a.NgayHieuLuc).FirstOrDefault());
            //else
            //    result = (from a in db.HRM_HopDong
            //              join b in db.HRM_QuyTrinhXuLy on a.IdGuid equals b.IdTable
            //              where b.isDaXuLy == (timKiem.TabTrangThai - 1)
            //              && b.IdUser == db.currentUser.Id

            //              select a);

            if (!string.IsNullOrEmpty(timKiem.IdLoaiHopDong))
            {
                result = result.Where(x => x.IdLoaiHopDong == timKiem.IdLoaiHopDong);
            }

            result = result.Where(x => x.isPhuLuc == timKiem.isPhuLuc);
            DateTime dtTuNgay = UnixTimeStampToDateTime(timKiem.TuNgayUnix ?? 0);
            DateTime dtDenNgay = UnixTimeStampToDateTime(timKiem.DenNgayUnix ?? 0);
            if (timKiem.TuNgayUnix > 0)
                result = result.Where(a => a.NgayHieuLuc >= dtTuNgay || (a.NgayChuyenKhongThoiHan != null && a.NgayChuyenKhongThoiHan >= dtTuNgay));
            if (timKiem.DenNgayUnix > 0)
                result = result.Where(a => a.NgayHieuLuc < dtDenNgay.AddDays(1) || (a.NgayChuyenKhongThoiHan != null && a.NgayChuyenKhongThoiHan <= dtDenNgay));
            if (!string.IsNullOrEmpty(timKiem.TenTrangThai))
            {
                var VoThoiHan = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.Ma == "KTH");
                if (timKiem.TenTrangThai == DefineData.eTrangTHaiHopDong.DaHetHan.ToString())
                {
                    result = result.Where(a => a.NgayHetHan != null && a.NgayHetHan < dt);
                    if (VoThoiHan != null)
                        result = result.Where(a => a.IdThoiHanHopDong != null && a.IdThoiHanHopDong != VoThoiHan.IdGuid);
                }
                else if (timKiem.TenTrangThai == DefineData.eTrangTHaiHopDong.SapHetHan.ToString())
                {
                    result = result.Where(a => a.NgayHetHan != null && dt >= a.NgayHetHan && a.NgayHetHan <= dt.AddMonths(-1));
                    if (VoThoiHan != null)
                        result = result.Where(a => a.IdThoiHanHopDong != null && a.IdThoiHanHopDong != VoThoiHan.IdGuid);
                }
                else
                    result = result.Where(a => a.TrangThai == timKiem.TenTrangThai);

            }

            result = result.Where(a => a.isPhuLuc == timKiem.isPhuLuc).Distinct();
            if (!string.IsNullOrEmpty(timKiem.KeyWord))
            {
                List<string> IdNhanVien = new List<string>();
                var listNhanViens = db.HRM_NhanVien.Where(a => a.MaNhanVien.ToLower().Contains(timKiem.KeyWord.ToLower().Trim()) || a.TenDayDu.ToLower().Contains(timKiem.KeyWord.ToLower().Trim())).ToList();
                if (listNhanViens != null && listNhanViens.Count > 0)
                {
                    IdNhanVien = listNhanViens.Select(a => a.IdGuid).Distinct().ToList();
                }
                result = result.Where(a => IdNhanVien.Contains(a.IdNhanVien) || (!string.IsNullOrEmpty(a.SoHopDong) && a.SoHopDong.ToLower().Contains(timKiem.KeyWord.ToLower().Trim())));
            }
            var lstCongTyAll = _CallWebService.GetDanhSachDuAnQuanLyHoSoNhanVien(db);
            if (timKiem.listIddmLoaiDuAn != null && timKiem.listIddmLoaiDuAn.Count > 0)
                lstCongTyAll = lstCongTyAll.Where(a => !string.IsNullOrEmpty(a.IddmLoaiDuAn) && timKiem.listIddmLoaiDuAn.Contains(a.IddmLoaiDuAn)).ToList();
            List<long> listIdDuAn = lstCongTyAll.Select(x => x.Id).ToList();
            var _listBoPhan = db.HRM_QuyTrinhBoPhan_BoPhan.Where(x => x.TableName == "HRM_HopDong" && listIdDuAn.Contains(x.IdBoPhan)).ToList();
            if (timKiem.IdCongTyEOS > 0)
            {
                _listBoPhan = _listBoPhan.Where(x => timKiem.IdCongTyEOS == x.IdBoPhan).ToList();

            }
            if (timKiem.listIdCongTy != null && timKiem.listIdCongTy.Count > 0)
            {
                _listBoPhan = _listBoPhan.Where(x => timKiem.listIdCongTy.Contains(x.IdBoPhan)).ToList();

            }

            var _listIdNhanVien = _listBoPhan.Select(x => x.IdQuyTrinh).Distinct().ToList();
            result = result.Where(x => _listIdNhanVien.Contains(x.IdGuid));
            if (timKiem.IdBoPhanEOS > 0)
                result = result.Where(a => a.IdBoPhanEOS == timKiem.IdBoPhanEOS);
            if (timKiem.LoaiLuong > 0)
            {
                result = result.Where(a => a.LoaiLuong == timKiem.LoaiLuong);
            }
            var listHopDong = result.ToList();
            var listIdHopDong = listHopDong.Select(a => a.IdGuid).ToList();
            var listIdHopDongG = listHopDong.Select(a => a.IdHopDong).ToList();
            var listHopDongGoc = db.HRM_HopDong.Where(a => listIdHopDongG.Contains(a.IdHopDong)).ToList();
            #endregion
            #region export
            var dmTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            var ListChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var loaiHopDong = db.HRM_dmLoaiHopDong.ToList();
            var listCapBac = db.HRM_dmCapBac.ToList();
            var ThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listCongTy = _CallWebService.GetDanhSachDuAn(db);
            var danhMuc = _funcDanhMuc;
            var listLoaiThoiGian = danhMuc.GetDanhMucLoaiLuong();
            var listDonViCapbaoHiem = db.HRM_dmNoiCapBaoHiem.ToList();
            var listCtyKy = _CallWebService.GetCongTyKyHopDong(db);
            var ListLoaiTraLuong = db.HRM_dmHinhThucTraLuong.ToList();
            var ListLoaiPhuCap = db.HRM_dmPhuCap.ToList();
            var listNoiCapBH = db.HRM_dmNoiCapBaoHiem.ToList();
            var listLichLamViec = db.HRM_dmLichLamViec.ToList();
            var BoPhanLD = listBoPhan.FirstOrDefault(a => a.MaBoPhan == "BLD");
            List<string> listIdNhanVien = listHopDong.Select(a => a.IdNhanVien).Distinct().ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => a.IdBoPhanEOS == BoPhanLD.Id || listIdNhanVien.Contains(a.IdGuid)).ToList();
            var listTrinhDo = db.HRM_dmTrinhDoDaoTao.ToList();
            #region Thông tin chung
            var sheetTTC = package.Workbook.Worksheets["Thông tin chung"];
            int row = 3;
            int maxrow = sheetTTC.Dimension.Rows;

            List<NguoiDaiDienCongTyViewModel> lstNguoiDaiDien = new List<NguoiDaiDienCongTyViewModel>();
            var listErr = new List<string>();
            int i = 3;
            foreach (var model in listHopDong)
            {
                var nhanVien = listNhanVien.FirstOrDefault(a => a.IdGuid == model.IdNhanVien);
                if (nhanVien == null)
                    continue;
                int STT = 2;
                sheetTTC.Cells[i, STT].Value = nhanVien.MaNhanVien;
                STT++;//3
                sheetTTC.Cells[i, STT].Value = nhanVien.TenDayDu;

                STT++; //4
                if (!string.IsNullOrEmpty(model.IddmTrinhDoDaoTao))
                {
                    var trinhDo = listTrinhDo.FirstOrDefault(x => x.Ma == model.IddmTrinhDoDaoTao);
                    if (trinhDo != null)
                    {
                        sheetTTC.Cells[i, STT].Value = trinhDo.Ma;
                        STT++;
                        sheetTTC.Cells[i, STT].Value = trinhDo.Ten;
                    }
                    else STT++;
                }
                else

                    STT++; //5 mã chức danh
                STT++; //5 mã chức danh
                sheetTTC.Cells[i, STT].Value = ListChucDanh.Where(a => a.Id == model.IddmChucDanh).Select(a => a.Ma).FirstOrDefault();
                STT++; //5 mã chức danh
                sheetTTC.Cells[i, STT].Value = ListChucDanh.Where(a => a.Id == model.IddmChucDanh).Select(a => a.Ten).FirstOrDefault();
                STT++; //5 mã chức danh
                sheetTTC.Cells[i, STT].Value = listChucVu.Where(a => a.Id == model.IddmChucVu).Select(a => a.MaChucVu).FirstOrDefault();
                STT++; //5 mã chức danh
                sheetTTC.Cells[i, STT].Value = listChucVu.Where(a => a.Id == model.IddmChucVu).Select(a => a.TenChucVu).FirstOrDefault();
                STT++; //5 mã chức danh
                sheetTTC.Cells[i, STT].Value = listCapBac.Where(a => a.IdGuid == model.IdCapBac).Select(a => a.Ma).FirstOrDefault();

                STT++;
                sheetTTC.Cells[i, STT].Value = model.DiaDiemLamViec;
                STT++;

                sheetTTC.Cells[i, STT].Value = listBoPhan.Where(a => a.Id == model.IdBoPhanEOS).Select(a => a.MaBoPhan).FirstOrDefault();
                STT++;
                sheetTTC.Cells[i, STT].Value = listBoPhan.Where(a => a.Id == model.IdBoPhanEOS).Select(a => a.TenBoPhan).FirstOrDefault();
                STT++;

                var maCongTy = dungchung.GetCell(sheetTTC, i, STT);
                model.listIdCongTy = _listBoPhan.Where(a => a.IdQuyTrinh == model.IdGuid).Select(a => a.IdBoPhan).ToList();
                var CongTys = listCongTy.Where(a => model.listIdCongTy.Contains(a.Id)).ToList();
                if (CongTys != null)
                    sheetTTC.Cells[i, STT].Value = string.Join(",", CongTys.Select(a => a.MaBoPhan).ToList());
                STT++;
                if (!string.IsNullOrEmpty(model.IdThoiGianLamViec))
                {
                    model.listIdCaLamViec = listLichLamViec.Where(a => model.IdThoiGianLamViec.Contains(a.IdGuid)).Select(a => a.Ma).Distinct().ToList();
                    sheetTTC.Cells[i, STT].Value = string.Join(",", model.listIdCaLamViec);
                }
                STT++;
                sheetTTC.Cells[i, STT].Value = model.SoHopDong;
                STT++;
                sheetTTC.Cells[i, STT].Value = listHopDongGoc.Where(a => a.IdGuid == model.IdHopDong).Select(a => a.SoHopDong).FirstOrDefault();

                STT++;
                sheetTTC.Cells[i, STT].Value = loaiHopDong.Where(a => a.IdGuid == model.IdLoaiHopDong).Select(a => a.Ma).FirstOrDefault();
                STT++;
                sheetTTC.Cells[i, STT].Value = loaiHopDong.Where(a => a.IdGuid == model.IdLoaiHopDong).Select(a => a.Ten).FirstOrDefault();
                STT++;
                sheetTTC.Cells[i, STT].Value = ThoiHanHopDong.Where(a => a.IdGuid == model.IdThoiHanHopDong).Select(a => a.Ma).FirstOrDefault();
                STT++;
                sheetTTC.Cells[i, STT].Value = ThoiHanHopDong.Where(a => a.IdGuid == model.IdThoiHanHopDong).Select(a => a.Ten).FirstOrDefault();
                STT++;
                if (model.NgayKy != null && model.NgayKy.Year > 1970)
                    sheetTTC.Cells[i, STT].Value = model.NgayKy.ToString("dd/MM/yyyy");
                STT++;
                if (model.NgayHieuLuc != null && model.NgayHieuLuc.Year > 1970)
                    sheetTTC.Cells[i, STT].Value = model.NgayHieuLuc.ToString("dd/MM/yyyy");
                STT++;
                if (model.NgayHetHan != null && model.NgayHetHan.Value.Year > 1970)
                    sheetTTC.Cells[i, STT].Value = model.NgayHetHan.Value.ToString("dd/MM/yyyy");

                STT++;
                sheetTTC.Cells[i, STT].Value = listCtyKy.Where(a => a.Id == model.IdCongTyKyHopDong).Select(a => a.MaBoPhan).FirstOrDefault();
                STT++;
                sheetTTC.Cells[i, STT].Value = listCtyKy.Where(a => a.Id == model.IdCongTyKyHopDong).Select(a => a.TenBoPhan).FirstOrDefault();

                STT++;//16
                sheetTTC.Cells[i, STT].Value = listNhanVien.Where(a => a.IdGuid == model.IdNguoiDaiDien).Select(a => a.MaNhanVien).FirstOrDefault();
                STT++;//16
                sheetTTC.Cells[i, STT].Value = listNhanVien.Where(a => a.IdGuid == model.IdNguoiDaiDien).Select(a => a.TenDayDu).FirstOrDefault();

                STT++;
                sheetTTC.Cells[i, STT].Value = model.isQuyetToanPhepNam == true ? "Có" : "Không";
                STT++;
                sheetTTC.Cells[i, STT].Value = model.PhiCongDoan;
                STT++;
                sheetTTC.Cells[i, STT].Value = model.SoNgayNghiPhep;
                STT++;
                if (model.NgayChuyenKhongThoiHan != null && model.NgayChuyenKhongThoiHan.Value.Year > 1970)
                    sheetTTC.Cells[i, STT].Value = model.NgayChuyenKhongThoiHan.Value.ToString("dd/MM/yyyy");
                i++;
            }
            #endregion
            /// thông tin liên quan
            var Sheetluong = package.Workbook.Worksheets["Lương"];
            var SheetphuCap = package.Workbook.Worksheets["Hỗ trợ"];
            var SheetbaoHiem = package.Workbook.Worksheets["Bảo hiểm"];
            var SheetbaoHiemCN = package.Workbook.Worksheets["Bảo hiểm con người"];
            var SheetcongViecs = package.Workbook.Worksheets["Công việc"];
            var SheetthietBiBanGiaos = package.Workbook.Worksheets["Thiết bị công việc"];
            var SheetKhamSucKhoe = package.Workbook.Worksheets["Khám Sức khỏe"];

            var listLuongHopDong = new List<HRM_Luong_HopDong>();
            var listLuongBHHopDong = new List<HRM_LuongBaoHiem_HopDong>();
            var listLuong = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
            i = 3;
            foreach (var LuongHopDong in listLuong)
            {
                var HopDong = listHopDong.Where(a => a.IdGuid == LuongHopDong.IdHopDong).FirstOrDefault();
                var NhanVien = listNhanVien.Where(a => a.IdGuid == HopDong.IdNhanVien).FirstOrDefault();
                if (NhanVien == null)
                    continue;
                var STT = 2;
                Sheetluong.Cells[i, STT].Value = HopDong.SoHopDong;
                STT++;
                Sheetluong.Cells[i, STT].Value = NhanVien.MaNhanVien;
                STT++;
                Sheetluong.Cells[i, STT].Value = NhanVien.TenDayDu;
                STT++;
                Sheetluong.Cells[i, STT].Value = NhanVien.MaSoThueTNCN;
                STT++;
                Sheetluong.Cells[i, STT].Value = LuongHopDong.TyLeHuongLuong;
                STT++;
                Sheetluong.Cells[i, STT].Value = LuongHopDong.KieuLuong;
                STT++;
                Sheetluong.Cells[i, STT].Value = ListLoaiTraLuong.Where(a => a.IdGuid == LuongHopDong.IddmHinhThucTraLuong).Select(a => a.Ma).FirstOrDefault();
                STT++;
                Sheetluong.Cells[i, STT].Value = ListLoaiTraLuong.Where(a => a.IdGuid == LuongHopDong.IddmHinhThucTraLuong).Select(a => a.Ten).FirstOrDefault();
                STT++;
                Sheetluong.Cells[i, STT].Value = LuongHopDong.SoTaiKhoan;
                STT++;
                Sheetluong.Cells[i, STT].Value = LuongHopDong.TenNganHang;
                STT++;
                Sheetluong.Cells[i, STT].Value = LuongHopDong.LuongBaoHiem;
                STT++;
                Sheetluong.Cells[i, STT].Value = HopDong.LoaiLuong;
                STT++;
                string TenLoaiLuong = "Theo Tháng";
                if (HopDong.LoaiLuong == 2)
                    TenLoaiLuong = "Theo ngày công";
                else if (HopDong.LoaiLuong == 3)
                    TenLoaiLuong = "Theo Giờ làm việc";
                Sheetluong.Cells[i, STT].Value = TenLoaiLuong;
                i++;

            }
            i = 2;
            var listPhuCap = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong) && !string.IsNullOrEmpty(a.IddmLoaiPhuCap)).ToList();
            foreach (var item in listPhuCap)
            {
                var HopDong = listHopDong.Where(a => a.IdGuid == item.IdHopDong).FirstOrDefault();
                var NhanVien = listNhanVien.Where(a => a.IdGuid == HopDong.IdNhanVien).FirstOrDefault();
                if (NhanVien == null)
                    continue;
                var PhuCap = ListLoaiPhuCap.Where(a => a.IdGuid == item.IddmLoaiPhuCap).FirstOrDefault();
                if (PhuCap == null)
                    continue;
                var STT = 2;
                SheetphuCap.Cells[i, STT].Value = HopDong.SoHopDong;
                STT++;
                SheetphuCap.Cells[i, STT].Value = NhanVien.MaNhanVien;
                STT++;
                SheetphuCap.Cells[i, STT].Value = NhanVien.TenDayDu;
                STT++;
                SheetphuCap.Cells[i, STT].Value = PhuCap.Ma;
                STT++;
                SheetphuCap.Cells[i, STT].Value = PhuCap.Ten;
                STT++;
                SheetphuCap.Cells[i, STT].Value = item.SoTien;
                STT++;
                if (item.NgayBatDau != null && item.NgayBatDau.Value.Year >= 1970)
                    SheetphuCap.Cells[i, STT].Value = item.NgayBatDau.Value.ToString("dd/MM/yyyy");
                STT++;
                if (item.NgayKetThuc != null && item.NgayKetThuc.Value.Year >= 1970)
                    SheetphuCap.Cells[i, STT].Value = item.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                STT++;
                SheetphuCap.Cells[i, STT].Value = item.LoaiPhuCap;
                STT++;
                string TenLoaiLuong = "Theo Tháng";
                if (HopDong.LoaiLuong == 2)
                    TenLoaiLuong = "Theo ngày công";
                else if (HopDong.LoaiLuong == 3)
                    TenLoaiLuong = "Theo Giờ làm việc";
                else if (HopDong.LoaiLuong == 4)
                    TenLoaiLuong = "Theo năm";

                SheetphuCap.Cells[i, STT].Value = TenLoaiLuong;
                i++;

            }
            var listBaoHiem = db.HRM_BaoHiem_HopDong.Where(a => a.isNgoai == false && listIdHopDong.Contains(a.IdHopDong)).ToList();
            var listLuongBaoHiem = db.HRM_LuongBaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
            i = 3;
            foreach (var item in listBaoHiem)
            {
                var HopDong = listHopDong.Where(a => a.IdGuid == item.IdHopDong).FirstOrDefault();
                var NhanVien = listNhanVien.Where(a => a.IdGuid == HopDong.IdNhanVien).FirstOrDefault();
                if (NhanVien == null)
                    continue;
                var luongBH = listLuongBaoHiem.FirstOrDefault(a => a.IdHopDong == item.IdHopDong);
                var STT = 2;
                SheetbaoHiem.Cells[i, STT].Value = HopDong.SoHopDong;
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = NhanVien.MaNhanVien;
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = NhanVien.TenDayDu;
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = item.isBaoHiemXaHoi == true ? "1" : "";
                STT++;
                if (item.NgayBatDau != null && item.NgayBatDau.Value.Year >= 1970)
                    SheetbaoHiem.Cells[i, STT].Value = item.NgayBatDau.Value.ToString("dd/MM/yyyy");
                STT++;
                if (item.NgayKetThuc != null && item.NgayKetThuc.Value.Year >= 1970)
                    SheetbaoHiem.Cells[i, STT].Value = item.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = item.SoSoBaoHiem;
                STT++;
                if (item.NgayCapSoBaoHiem != null && item.NgayCapSoBaoHiem.Value.Year >= 1970)
                    SheetbaoHiem.Cells[i, STT].Value = item.NgayCapSoBaoHiem.Value.ToString("dd/MM/yyyy");
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = listNoiCapBH.Where(a => a.IdGuid == item.NoiCapSoBaoHiem).Select(a => a.Ten).FirstOrDefault();
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = item.isBaoHiemYTe == true ? "1" : "";
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = item.isBaoHiemThatNghiep == true ? "1" : "";
                STT++;
                STT++;
                SheetbaoHiem.Cells[i, STT].Value = luongBH.LuongBaoHiem;
                i++;
            }
            var listBaoHiemNgoai = db.HRM_BaoHiem_HopDong.Where(a => a.isNgoai == true && listIdHopDong.Contains(a.IdHopDong)).ToList();
            i = 3;
            foreach (var item in listBaoHiemNgoai)
            {
                var HopDong = listHopDong.Where(a => a.IdGuid == item.IdHopDong).FirstOrDefault();
                var NhanVien = listNhanVien.Where(a => a.IdGuid == HopDong.IdNhanVien).FirstOrDefault();
                if (NhanVien == null)
                    continue;
                var STT = 2;
                SheetbaoHiemCN.Cells[i, STT].Value = HopDong.SoHopDong;
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = NhanVien.MaNhanVien;
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = NhanVien.TenDayDu;
                STT++;
                if (item.NgayBatDau != null && item.NgayBatDau.Value.Year >= 1970)
                    SheetbaoHiemCN.Cells[i, STT].Value = item.NgayBatDau.Value.ToString("dd/MM/yyyy");
                STT++;
                if (item.NgayKetThuc != null && item.NgayKetThuc.Value.Year >= 1970)
                    SheetbaoHiemCN.Cells[i, STT].Value = item.NgayKetThuc.Value.ToString("dd/MM/yyyy");
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = item.SoHopDong;
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = listNoiCapBH.Where(a => a.IdGuid == item.NoiCapSoBaoHiem).Select(a => a.Ma).FirstOrDefault();
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = listNoiCapBH.Where(a => a.IdGuid == item.NoiCapSoBaoHiem).Select(a => a.Ten).FirstOrDefault();
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = item.PhiBaoHiem;
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = item.SoTienBaoHiem;
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = item.Giam == 1 ? "Có" : "Không";
                STT++;
                SheetbaoHiemCN.Cells[i, STT].Value = item.GhiChu;
                i++;

            }

            //var lstPhuCap = new List<HRM_HopDongPhuCap>();
            //for (int i = 2; i <= SheetphuCap.Dimension.Rows; i++)
            //{
            //    var model = new HRM_HopDongPhuCap();

            //    int STT = 2;
            //    model.SoHopDong = dungchung.GetCell(SheetphuCap, i, STT);
            //    model.MaNhanVien = dungchung.GetCell(SheetphuCap, i, STT + 1);
            //    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
            //        break;

            //    STT = STT + 3;

            //    var maDMPhuCap = dungchung.GetCell(SheetphuCap, i, STT);

            //    if (maDMPhuCap.IsNotEmpty() && ListLoaiPhuCap.IsNotEmpty())
            //    {
            //        var dmPhuCap = ListLoaiPhuCap.FirstOrDefault(x => x.Ma == maDMPhuCap);
            //        if (dmPhuCap != null)
            //        {
            //            model.IddmLoaiPhuCap = dmPhuCap.IdGuid;
            //        }
            //    }
            //    STT++;//5
            //    STT++;//5
            //    model.SoTien = dungchung.GetCellToDouble(SheetphuCap, i, STT);

            //    STT++;//6
            //    model.NgayBatDau = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
            //    model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
            //    STT++;//7
            //    model.NgayKetThuc = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
            //    model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
            //    STT++;//7
            //    model.LoaiPhuCap = dungchung.GetCellToInt(SheetphuCap, i, STT);
            //    //chưa xác định

            //    lstPhuCap.Add(model);
            //}
            //var lstBaoHiem = new List<HRM_BaoHiem_HopDong>();
            //var lstBaoHiemNgoai = new List<HRM_BaoHiem_HopDong>();
            //var listNoiCapBH = db.HRM_dmNoiCapBaoHiem.ToList();
            //for (int i = 3; i <= SheetbaoHiem.Dimension.Rows; i++)
            //{


            //    var BaoHiem = new HRM_BaoHiem_HopDong();
            //    int STT = 2;
            //    BaoHiem.SoHopDong = dungchung.GetCell(SheetbaoHiem, i, STT);

            //    BaoHiem.MaNhanVien = dungchung.GetCell(SheetbaoHiem, i, STT + 1);
            //    if (string.IsNullOrEmpty(BaoHiem.SoHopDong) && string.IsNullOrEmpty(BaoHiem.MaNhanVien))
            //        break;
            //    STT = STT + 3;
            //    string BHXH = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    if (BHXH == "1" || BHXH.ToLower() == "true")
            //        BaoHiem.isBaoHiemXaHoi = true;
            //    STT++;//5
            //    BaoHiem.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
            //    BaoHiem.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
            //    STT++;// 6 ngày kết thúc
            //    BaoHiem.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
            //    BaoHiem.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayKetThuc);
            //    STT++;//7 Sô sổ bảo hiểm
            //    BaoHiem.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    STT++;//8 ngày cấp
            //    BaoHiem.NgayCapSoBaoHiem = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
            //    BaoHiem.NgayCapSoBaoHiemUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayCapSoBaoHiem);
            //    STT++;// 9 nơi cấp
            //    BaoHiem.NoiCapSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    STT++;
            //    string BHYT = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    if (BHYT == "1" || BHYT.ToLower() == "true")
            //        BaoHiem.isBaoHiemYTe = true;
            //    STT++;
            //    string BHTN = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    if (BHTN == "1" || BHTN.ToLower() == "true")
            //        BaoHiem.isBaoHiemThatNghiep = true;
            //    STT++;
            //    string PhucLoi = dungchung.GetCell(SheetbaoHiem, i, STT);
            //    if (PhucLoi == "1" || PhucLoi.ToLower() == "true")
            //    {
            //        var HopDongBH = listHopDong.FirstOrDefault(a => a.SoHopDong == BaoHiem.SoHopDong);
            //        HopDongBH.isHuongPhucLoi = true;
            //    }
            //    lstBaoHiem.Add(BaoHiem);
            //    STT++;
            //    HRM_LuongBaoHiem_HopDong LuongBH = new HRM_LuongBaoHiem_HopDong();
            //    LuongBH.LuongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiem, i, STT);
            //    LuongBH.SoHopDong = BaoHiem.SoHopDong;
            //    LuongBH.MaNhanVien = BaoHiem.MaNhanVien;
            //    listLuongBHHopDong.Add(LuongBH);

            //}
            //if (SheetbaoHiemCN != null)
            //{
            //    for (int i = 3; i <= SheetbaoHiemCN.Dimension.Rows; i++)
            //    {
            //        int STT = 2;
            //        var BaoHiemNgoai = new HRM_BaoHiem_HopDong();
            //        BaoHiemNgoai.SoHopDong = dungchung.GetCell(SheetbaoHiemCN, i, STT);
            //        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetbaoHiemCN, i, STT + 1);
            //        if (string.IsNullOrEmpty(BaoHiemNgoai.SoHopDong) && string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
            //            break;
            //        STT = STT + 3;
            //        BaoHiemNgoai.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
            //        BaoHiemNgoai.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayBatDau);
            //        STT++;
            //        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
            //        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
            //        STT++;
            //        BaoHiemNgoai.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiemCN, i, STT);
            //        STT++;
            //        string MaDonVi = dungchung.GetCell(SheetbaoHiemCN, i, STT);
            //        if (!string.IsNullOrEmpty(MaDonVi))
            //            BaoHiemNgoai.NoiCapSoBaoHiem = listNoiCapBH.Where(a => a.Ma == MaDonVi).Select(a => a.IdGuid).FirstOrDefault();
            //        STT++;
            //        STT++;
            //        BaoHiemNgoai.MucDongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
            //        STT++;
            //        BaoHiemNgoai.SoTienBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
            //        STT++;
            //        string Giam = dungchung.GetCell(SheetbaoHiemCN, i, STT);
            //        if (Giam.ToUpper() == "CÓ")
            //            BaoHiemNgoai.Giam = 1;
            //        else
            //            BaoHiemNgoai.Giam = 2;
            //        STT++;
            //        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetbaoHiemCN, i, STT);
            //        STT++;
            //        lstBaoHiemNgoai.Add(BaoHiemNgoai);
            //    }
            //}
            //var lstKhamSucKhoe = new List<HRM_HopDong_KhamSucKhoe>();
            //if (SheetKhamSucKhoe != null)
            //{
            //    for (int i = 4; i <= SheetKhamSucKhoe.Dimension.Rows; i++)
            //    {
            //        var BaoHiemNgoai = new HRM_HopDong_KhamSucKhoe();
            //        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetKhamSucKhoe, i, 2);
            //        if (string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
            //            break;
            //        BaoHiemNgoai.NgayKham = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 4);
            //        BaoHiemNgoai.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKham);
            //        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 5);
            //        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
            //        BaoHiemNgoai.NoiKham = dungchung.GetCell(SheetKhamSucKhoe, i, 6);
            //        BaoHiemNgoai.LoaiHinhKham = dungchung.GetCell(SheetKhamSucKhoe, i, 7);
            //        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetKhamSucKhoe, i, 8);
            //        lstKhamSucKhoe.Add(BaoHiemNgoai);
            //    }
            //}
            //var listMoTaCongViec = new List<HRM_HopDongCongViec>();
            //for (int i = 2; i <= SheetcongViecs.Dimension.Rows; i++)
            //{
            //    var model = new HRM_HopDongCongViec();
            //    int STT = 1;
            //    model.IdGuid = Guid.NewGuid().ToString();
            //    model.STT = dungchung.GetCell(SheetcongViecs, i, STT);
            //    STT++;
            //    model.STT_Cha = dungchung.GetCell(SheetcongViecs, i, STT);
            //    STT++;

            //    model.SoHopDong = dungchung.GetCell(SheetcongViecs, i, STT);

            //    model.MaNhanVien = dungchung.GetCell(SheetcongViecs, i, STT + 1);
            //    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
            //        break;
            //    STT = STT + 3;
            //    // 2 Tên công việc
            //    model.TenCongViec = dungchung.GetCell(SheetcongViecs, i, STT);
            //    if (string.IsNullOrEmpty(model.TenCongViec))
            //        break;
            //    STT++;// 3 mô tả
            //    model.MoTa = dungchung.GetCell(SheetcongViecs, i, STT);
            //    listMoTaCongViec.Add(model);
            //}

            //var listTrangThietBi = new List<HRM_HopDongTaiSanBanGiao>();
            //var dmTrangThietBi = db.HRM_dmTrangThietBi.ToList();
            //for (int i = 2; i <= SheetthietBiBanGiaos.Dimension.Rows; i++)
            //{
            //    var model = new HRM_HopDongTaiSanBanGiao();
            //    int STT = 2;
            //    model.SoHopDong = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

            //    model.MaNhanVien = dungchung.GetCell(SheetthietBiBanGiaos, i, STT + 1);
            //    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
            //        break;
            //    STT = STT + 3;
            //    string maThietBi = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);
            //    if (maThietBi.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
            //    {
            //        var TrangThietBi = dmTrangThietBi.FirstOrDefault(x => x.Ma == maThietBi);
            //        if (TrangThietBi != null)
            //        {
            //            model.IddmTrangThaiThietBi = TrangThietBi.IdGuid;
            //        }
            //        else
            //        {
            //            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"mã trang thiết bị không tồn tại : " + maThietBi);
            //        }
            //    }

            //    STT++;
            //    STT++;//  4 số luọng
            //    model.SoLuong = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);


            //    STT++;// 5 giá trk
            //    model.GiaTri = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);

            //    STT++;// 6 mô tả
            //    model.MoTa = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

            //    listTrangThietBi.Add(model);
            //}



            #endregion
            #region danh mục
            ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"]; ;

            int nRow = 3;

            foreach (var item in ListChucDanh)
            {
                sheet1.Cells[nRow, 1].Value = item.Ten;
                sheet1.Cells[nRow, 2].Value = item.Ma;
                nRow++;
            }

            nRow = 3;
            foreach (var item in listChucVu)
            {
                sheet1.Cells[nRow, 3].Value = item.TenChucVu;
                sheet1.Cells[nRow, 4].Value = item.MaChucVu;
                nRow++;
            }
            nRow = 3;
            foreach (var item in loaiHopDong)
            {
                sheet1.Cells[nRow, 5].Value = item.Ten;
                sheet1.Cells[nRow, 6].Value = item.Ma;
                nRow++;
            }
            nRow = 3;
            foreach (var item in ThoiHanHopDong)
            {
                sheet1.Cells[nRow, 7].Value = item.Ten;
                sheet1.Cells[nRow, 8].Value = item.Ma;
                nRow++;
            }

            nRow = 3;

            foreach (var item in listCongTy)
            {
                sheet1.Cells[nRow, 9].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 10].Value = item.MaBoPhan;
                nRow++;

            }

            nRow = 3;
            foreach (var item in listBoPhan)
            {
                sheet1.Cells[nRow, 11].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 12].Value = item.MaBoPhan;
                nRow++;
            }

            nRow = 3;
            foreach (var item in listLoaiThoiGian)
            {
                sheet1.Cells[nRow, 13].Value = item.LoaiLuong;
                sheet1.Cells[nRow, 14].Value = item.Id;
                nRow++;
            }
            nRow = 3;
            foreach (var item in listDonViCapbaoHiem)
            {
                sheet1.Cells[nRow, 15].Value = item.Ten;
                sheet1.Cells[nRow, 16].Value = item.Ma;
                nRow++;
            }
            nRow = 3;
            foreach (var item in ListLoaiPhuCap)
            {
                sheet1.Cells[nRow, 17].Value = item.Ten;
                sheet1.Cells[nRow, 18].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in ListLoaiTraLuong)
            {
                sheet1.Cells[nRow, 19].Value = item.Ten;
                sheet1.Cells[nRow, 20].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listBoPhan)
            {
                sheet1.Cells[nRow, 21].Value = item.TenBoPhan;
                sheet1.Cells[nRow, 22].Value = item.MaBoPhan;
                nRow++;
            }

            nRow = 3;
            foreach (var item in listTrinhDo)
            {
                sheet1.Cells[nRow, 23].Value = item.Ten;
                sheet1.Cells[nRow, 24].Value = item.Ma;
                nRow++;

            }
            nRow = 3;
            foreach (var item in listNhanVien)
            {
                sheet1.Cells[nRow, 25].Value = item.TenDayDu;
                sheet1.Cells[nRow, 26].Value = item.MaNhanVien;
                nRow++;

            }
            #endregion
            s.Close();
            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            return dnm;
        }

        public MessageReturn ImportExcelHopDongUpdate(string aFileName, ApplicationDbContext dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");
            int nRowError = 0;
            int nColError = 0;
            ExcelWorksheet sheetError = null;
            try
            {

                string pathTemplate = PathUploads + aFileName;
                if (File.Exists(pathTemplate) == false)
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);
                s.Close();
                #region Thông tin chung
                var sheet1 = package.Workbook.Worksheets["Thông tin chung"];
                int row = 3;
                int maxrow = sheet1.Dimension.Rows;

                var listNhanVien = dbContext.HRM_NhanVien.AsNoTracking().ToList();
                var listTrinhDoChuyenMon = dbContext.HRM_dmTrinhDoDaoTao.ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(dbContext);
                //  var listChucVu = dbContext.HRM_dmChucVu.ToList();
                var listChucVuCongViec = _CallWebService.GetDanhSachChucVu(dbContext);
                var listCapBac = dbContext.HRM_dmCapBac.ToList();
                var listLichLamViec = dbContext.HRM_dmLichLamViec.ToList();
                var listLoaiHopDong = dbContext.HRM_dmLoaiHopDong.ToList();
                var listHinhThucTraLuong = dbContext.HRM_dmHinhThucTraLuong.ToList();
                var listCtyKy = _CallWebService.GetCongTyKyHopDong(dbContext);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(dbContext);
                var listCongTy = _CallWebService.GetDanhSachDuAn(dbContext);
                List<NguoiDaiDienCongTyViewModel> lstNguoiDaiDien = new List<NguoiDaiDienCongTyViewModel>();
                var listThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.ToList();
                var listHopDong = new List<HRM_HopDong>();
                var listErr = new List<string>();
                var listHopDongdb = dbContext.HRM_HopDong.Where(a => a.isKetThuc == false).ToList();
                for (int i = row; i <= maxrow; i++)
                {

                    var model = new HRM_HopDong();
                    var STT = 2;
                    string SoHopDong = dungchung.GetCell(sheet1, i, 16);
                    model.MaNhanVien = dungchung.GetCell(sheet1, i, STT);
                    if (string.IsNullOrEmpty(model.MaNhanVien))

                        break;
                    if (string.IsNullOrEmpty(SoHopDong))
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không được để trống");
                    else
                    {
                        model = listHopDongdb.FirstOrDefault(a => a.SoHopDong == SoHopDong);
                        if (model == null)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không tồn tại " + SoHopDong);
                    }

                    model.MaNhanVien = dungchung.GetCell(sheet1, i, STT);
                    if (listNhanVien.IsNotEmpty())
                    {
                        var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == model.MaNhanVien);
                        if (nhanVien == null)
                        {
                            listErr.Add(model.MaNhanVien);
                            continue;
                        }
                        if (model.IdNhanVien != nhanVien.IdGuid)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng và mã nhân viên không khớp");
                        model.IdNhanVien = nhanVien.IdGuid;
                    }

                    STT++;//3
                    STT++; //4
                    string maTrinhDo = dungchung.GetCell(sheet1, i, STT);
                    if (listTrinhDoChuyenMon.IsNotEmpty())
                    {
                        if (maTrinhDo.IsNotEmpty())
                        {
                            var trinhDo = listTrinhDoChuyenMon.FirstOrDefault(x => x.Ma == maTrinhDo);
                            if (trinhDo != null)
                                model.IddmTrinhDoDaoTao = trinhDo.IdGuid;
                        }
                    }
                    STT++; //5 mã chức danh
                    STT++; //5 mã chức danh
                    var maChucDanhCongViec = dungchung.GetCell(sheet1, i, STT);
                    if (listChucDanh.IsNotEmpty())
                    {
                        if (maChucDanhCongViec.IsNotEmpty())
                        {
                            var ChucDanhCongViec = listChucDanh.FirstOrDefault(x => x.Ma == maChucDanhCongViec);
                            if (ChucDanhCongViec == null)
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã chức vụ ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            else
                                model.IddmChucDanh = ChucDanhCongViec.Id;
                        }
                    }
                    //var maChucVu = dungchung.GetCell(sheet1, i, STT);
                    STT++;//6 chức vụ công việc
                    STT++;//6 chức vụ công việc
                    var maChucVuCongViec = dungchung.GetCell(sheet1, i, STT);
                    if (listChucVuCongViec.IsNotEmpty())
                    {
                        if (maChucVuCongViec.IsNotEmpty())
                        {
                            var chucVuCongViec = listChucVuCongViec.FirstOrDefault(x => x.MaChucVu == maChucVuCongViec);
                            if (chucVuCongViec == null)
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Mã chức vụ ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            else
                                model.IddmChucVu = chucVuCongViec.Id;
                        }
                    }
                    STT++;//7
                    STT++;//7
                    var maCapBac = dungchung.GetCell(sheet1, i, STT);

                    if (listCapBac.IsNotEmpty())
                    {
                        if (maCapBac.IsNotEmpty())
                        {
                            var capBac = listCapBac.FirstOrDefault(x => x.Ma == maCapBac);
                            if (capBac != null)
                                model.IdCapBac = capBac.IdGuid;
                        }
                    }
                    STT++;//8
                    model.DiaDiemLamViec = dungchung.GetCell(sheet1, i, STT);
                    STT++;
                    var maBoPhan = dungchung.GetCell(sheet1, i, STT);
                    if (listBoPhan.IsNotEmpty())
                    {
                        if (maBoPhan.IsNotEmpty())
                        {
                            var BoPhan = listBoPhan.FirstOrDefault(x => x.MaBoPhan == maBoPhan);
                            if (BoPhan != null)
                                model.IdBoPhanEOS = BoPhan.Id;
                        }
                    }
                    STT++;//8
                    STT++;//8
                    var maCongTy = dungchung.GetCell(sheet1, i, STT);
                    model.listIdCongTy = new List<long>();
                    if (listCongTy.IsNotEmpty())
                    {
                        if (maCongTy.IsNotEmpty())
                        {
                            List<string> listMaCT = maCongTy.Split(';').ToList();
                            var CongTys = listCongTy.Where(a => listMaCT.Contains(a.MaBoPhan)).ToList();
                            if (CongTys != null)
                                foreach (var item in CongTys)
                                {
                                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                                    bp.IdGuid = Guid.NewGuid().ToString();
                                    bp.IdQuyTrinh = model.IdGuid;
                                    bp.IdBoPhan = item.Id;
                                    bp.TableName = nameof(HRM_HopDong);
                                    dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                                    model.listIdCongTy.Add(item.Id);
                                }
                        }
                    }
                    STT++;//8
                    var MaCaLamViec = dungchung.GetCell(sheet1, i, STT);
                    if (!string.IsNullOrEmpty(MaCaLamViec))
                    {
                        List<string> list = MaCaLamViec.Split('-').ToList();
                        model.listIdCaLamViec = listLichLamViec.Where(a => list.Contains(a.Ma)).Select(a => a.IdGuid).Distinct().ToList();
                        model.IdThoiGianLamViec = string.Join(",", model.listIdCaLamViec);
                    }
                    STT++; //9

                    STT++; //9
                    model.SoHopDongGoc = dungchung.GetCell(sheet1, i, STT);
                    if (!string.IsNullOrEmpty(model.SoHopDongGoc))
                        model.isPhuLuc = true;
                    STT++; //10
                    string LoaiHopDong = dungchung.GetCell(sheet1, i, STT);
                    if (listLoaiHopDong.IsNotEmpty())
                    {
                        if (LoaiHopDong.IsNotEmpty())
                        {
                            var LHopDong = listLoaiHopDong.FirstOrDefault(x => x.Ma == LoaiHopDong);
                            if (LHopDong != null)
                                model.IdLoaiHopDong = LHopDong.IdGuid;
                        }
                    }
                    STT++; //11
                    STT++; //11
                    string ThoiHanHopDong = dungchung.GetCell(sheet1, i, STT);
                    if (listThoiHanHopDong.IsNotEmpty())
                    {
                        if (ThoiHanHopDong.IsNotEmpty())
                        {
                            var LHopDong = listThoiHanHopDong.FirstOrDefault(x => x.Ma == ThoiHanHopDong);
                            if (LHopDong != null)
                                model.IdThoiHanHopDong = LHopDong.IdGuid;
                        }
                    }
                    STT++;//12
                    STT++;//12
                    try
                    {
                        model.NgayKy = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        model.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKy);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Sheet " + sheet1.Name + " dòng  " + i + " Ngày ký");

                    }
                    STT++;//13
                    try
                    {
                        model.NgayHieuLuc = dungchung.GetCellToDateTime(sheet1, i, STT) ?? DateTime.Now;
                        model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Sheet " + sheet1.Name + " dòng  " + i + " Ngày hiệu lực");
                    }
                    STT++;//14
                    try
                    {
                        model.NgayHetHan = dungchung.GetCellToDateTime(sheet1, i, STT);
                        if (model.NgayHetHan != null)
                            model.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHetHan);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày hết hạn dòng số " + i + " sheet thông tin chung ");
                    }
                    STT++;//15
                    var maCongTyKy = dungchung.GetCell(sheet1, i, STT);


                    if (!string.IsNullOrEmpty(maCongTyKy))
                    {
                        var CongTy = listCtyKy.FirstOrDefault(x => x.MaBoPhan == maCongTyKy);
                        if (CongTy == null)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Công ty ký hợp đồng ở nhân viên " + model.MaNhanVien + " không tồn tại");
                        else
                        {
                            model.IdCongTyKyHopDong = CongTy.Id;

                        }
                    }
                    else
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Công ty ký hợp đồng ở nhân viên " + model.MaNhanVien + " không được để trống");
                    }

                    STT++;//16
                    STT++;//16
                    var MaNhanVienDaiDien = dungchung.GetCell(sheet1, i, STT);
                    if (listNhanVien.IsNotEmpty())
                    {
                        if (MaNhanVienDaiDien.IsNotEmpty())
                        {
                            var nhanVienDaiDien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == MaNhanVienDaiDien);
                            if (nhanVienDaiDien == null)
                            {
                                listErr.Add(model.MaNhanVien);
                                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Người đại diện ở nhân viên " + model.MaNhanVien + " không tồn tại");
                            }

                            model.IdNguoiDaiDien = nhanVienDaiDien.IdGuid;
                            model.NguoiDaiDienIdQuocTich = nhanVienDaiDien.IdQuocTich;
                            model.NguoiDaiDienIdChucDanh = nhanVienDaiDien.IdChucDanh;
                        }
                    }
                    STT++;

                    STT++;
                    string QToanPhepNam = dungchung.GetCell(sheet1, i, STT);
                    if (QToanPhepNam == "CO" || QToanPhepNam == "Co" || QToanPhepNam == "co")
                    {
                        model.isQuyetToanPhepNam = true;
                    }
                    STT++;
                    model.PhiCongDoan = dungchung.GetCellToDouble(sheet1, i, STT);
                    if (model.PhiCongDoan > 0)
                    {
                        model.isThamGiaCongDoan = true;
                    }

                    STT++;
                    model.SoNgayNghiPhep = dungchung.GetCellToDouble(sheet1, i, STT);
                    STT++;
                    try
                    {
                        model.NgayChuyenKhongThoiHan = dungchung.GetCellToDateTime(sheet1, i, STT);
                        if (model.NgayChuyenKhongThoiHan != null)
                            model.NgayChuyenKhongThoiHanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayChuyenKhongThoiHan);
                    }
                    catch
                    {
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "lỗi ngày chuyển không thời hạn dòng số " + i + " sheet thông tin chung ");
                    }
                    dbContext.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }

                if (listErr.IsNotEmpty())
                    return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, $"Các mã nhân viên này không có trong dữ liệu {listErr.Join(",")}");
                #endregion
                /// thông tin liên quan
                var HopDong = new HRM_HopDong();
                var Sheetluong = package.Workbook.Worksheets["Lương"];
                var SheetphuCap = package.Workbook.Worksheets["Hỗ trợ"];
                var SheetbaoHiem = package.Workbook.Worksheets["Bảo hiểm"];
                var SheetbaoHiemCN = package.Workbook.Worksheets["Bảo hiểm con người"];
                var SheetcongViecs = package.Workbook.Worksheets["Công việc"];
                var SheetthietBiBanGiaos = package.Workbook.Worksheets["Thiết bị công việc"];
                var SheetKhamSucKhoe = package.Workbook.Worksheets["Khám Sức khỏe"];

                var listLuongHopDong = new List<HRM_Luong_HopDong>();
                var listLuongBHHopDong = new List<HRM_LuongBaoHiem_HopDong>();
                for (int i = 3; i <= Sheetluong.Dimension.Rows; i++)
                {
                    var LuongHopDong = new HRM_Luong_HopDong();
                    var STT = 2;
                    LuongHopDong.IdGuid = Guid.NewGuid().ToString();
                    LuongHopDong.SoHopDong = dungchung.GetCell(Sheetluong, i, STT);
                    LuongHopDong.MaNhanVien = dungchung.GetCell(Sheetluong, i, STT + 1);

                    if (string.IsNullOrEmpty(LuongHopDong.SoHopDong) && string.IsNullOrEmpty(LuongHopDong.MaNhanVien))
                        break;
                    var HopDongdb = listHopDongdb.FirstOrDefault(a => a.SoHopDong == LuongHopDong.SoHopDong);
                    if (HopDongdb == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không tồn tại ở sheet Lương: " + LuongHopDong.SoHopDong);
                    LuongHopDong.IdHopDong = HopDongdb.IdGuid;
                    STT = STT + 3;
                    LuongHopDong.MaSoThueTNCN = dungchung.GetCell(Sheetluong, i, STT);

                    STT++;// 5 tỷ lê hưởng lương

                    LuongHopDong.TyLeHuongLuong = dungchung.GetCellToDouble(Sheetluong, i, STT);
                    STT++;
                    LuongHopDong.KieuLuong = dungchung.GetCellToInt(Sheetluong, i, STT);

                    STT++;//7 Hinh thức trả lương
                    var hinhThucTraLuong = dungchung.GetCell(Sheetluong, i, STT);

                    if (hinhThucTraLuong.IsNotEmpty())
                    {
                        LuongHopDong.IddmHinhThucTraLuong = listHinhThucTraLuong.Where(a => a.Ma == hinhThucTraLuong).Select(a => a.IdGuid).FirstOrDefault();
                    }
                    STT++;
                    STT++;
                    LuongHopDong.SoTaiKhoan = dungchung.GetCell(Sheetluong, i, STT);
                    STT++;//9 Ngân hàng
                    LuongHopDong.TenNganHang = dungchung.GetCell(Sheetluong, i, STT);

                    STT++; // 10 lương 
                    LuongHopDong.LuongBaoHiem = dungchung.GetCellToDouble(Sheetluong, i, STT);
                    STT++; // 10 lương 
                    HopDongdb.LoaiLuong = dungchung.GetCellToInt(Sheetluong, i, STT);


                    listLuongHopDong.Add(LuongHopDong);
                }
                #region
                if (listLuongHopDong != null && listLuongHopDong.Count > 0)
                {
                    var ListIdHopDong = listLuongHopDong.Select(a => a.IdHopDong).Distinct().ToList();
                    var luongHDDB = dbContext.HRM_Luong_HopDong.Where(a => ListIdHopDong.Contains(a.IdHopDong)).ToList();
                    dbContext.HRM_Luong_HopDong.RemoveRange(luongHDDB);
                    dbContext.HRM_Luong_HopDong.AddRange(listLuongHopDong);
                }
                #endregion
                var listDMPhuCap = dbContext.HRM_dmPhuCap.ToList();
                var lstPhuCap = new List<HRM_HopDongPhuCap>();
                for (int i = 2; i <= SheetphuCap.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongPhuCap();

                    int STT = 2;
                    model.IdGuid = Guid.NewGuid().ToString();

                    model.SoHopDong = dungchung.GetCell(SheetphuCap, i, STT);
                    model.MaNhanVien = dungchung.GetCell(SheetphuCap, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;
                    var HopDongdb = listHopDongdb.FirstOrDefault(a => a.SoHopDong == model.SoHopDong);
                    if (HopDongdb == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không tồn tại ở sheet phụ cấp: " + model.SoHopDong);
                    model.IdHopDong = HopDongdb.IdGuid;
                    STT = STT + 3;

                    var maDMPhuCap = dungchung.GetCell(SheetphuCap, i, STT);

                    if (maDMPhuCap.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
                    {
                        var dmPhuCap = listDMPhuCap.FirstOrDefault(x => x.Ma == maDMPhuCap);
                        if (dmPhuCap != null)
                        {
                            model.IddmLoaiPhuCap = dmPhuCap.IdGuid;
                        }
                    }
                    STT++;//5
                    STT++;//5
                    model.SoTien = dungchung.GetCellToDouble(SheetphuCap, i, STT);

                    STT++;//6
                    model.NgayBatDau = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
                    model.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayBatDau);
                    STT++;//7
                    model.NgayKetThuc = dungchung.GetCellToDateTime(SheetphuCap, i, STT);
                    model.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKetThuc);
                    STT++;//7
                    model.LoaiPhuCap = dungchung.GetCellToInt(SheetphuCap, i, STT);
                    //chưa xác định

                    lstPhuCap.Add(model);
                }
                #region
                if (lstPhuCap != null && lstPhuCap.Count > 0)
                {
                    var ListIdHopDong = lstPhuCap.Select(a => a.IdHopDong).Distinct().ToList();
                    var luongHDDB = dbContext.HRM_HopDongPhuCap.Where(a => ListIdHopDong.Contains(a.IdHopDong)).ToList();
                    dbContext.HRM_HopDongPhuCap.RemoveRange(luongHDDB);
                    dbContext.HRM_HopDongPhuCap.AddRange(lstPhuCap);
                }
                #endregion
                var lstBaoHiem = new List<HRM_BaoHiem_HopDong>();
                var lstBaoHiemNgoai = new List<HRM_BaoHiem_HopDong>();
                var listNoiCapBH = dbContext.HRM_dmNoiCapBaoHiem.ToList();
                for (int i = 3; i <= SheetbaoHiem.Dimension.Rows; i++)
                {


                    var BaoHiem = new HRM_BaoHiem_HopDong();
                    int STT = 2;
                    BaoHiem.SoHopDong = dungchung.GetCell(SheetbaoHiem, i, STT);
                    BaoHiem.IdGuid = Guid.NewGuid().ToString();

                    BaoHiem.MaNhanVien = dungchung.GetCell(SheetbaoHiem, i, STT + 1);
                    if (string.IsNullOrEmpty(BaoHiem.SoHopDong) && string.IsNullOrEmpty(BaoHiem.MaNhanVien))
                        break;
                    var HopDongdb = listHopDongdb.FirstOrDefault(a => a.SoHopDong == BaoHiem.SoHopDong);
                    if (HopDongdb == null)
                        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không tồn tại ở sheet Bảo hiểm: " + BaoHiem.SoHopDong);
                    BaoHiem.IdHopDong = HopDongdb.IdGuid;
                    STT = STT + 3;
                    string BHXH = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHXH == "1" || BHXH.ToLower() == "true")
                        BaoHiem.isBaoHiemXaHoi = true;
                    STT++;//5
                    BaoHiem.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
                    STT++;// 6 ngày kết thúc
                    BaoHiem.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayKetThuc);
                    STT++;//7 Sô sổ bảo hiểm
                    BaoHiem.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
                    STT++;//8 ngày cấp
                    BaoHiem.NgayCapSoBaoHiem = dungchung.GetCellToDateTime(SheetbaoHiem, i, STT);
                    BaoHiem.NgayCapSoBaoHiemUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayCapSoBaoHiem);
                    STT++;// 9 nơi cấp
                    BaoHiem.NoiCapSoBaoHiem = dungchung.GetCell(SheetbaoHiem, i, STT);
                    STT++;
                    string BHYT = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHYT == "1" || BHYT.ToLower() == "true")
                        BaoHiem.isBaoHiemYTe = true;
                    STT++;
                    string BHTN = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (BHTN == "1" || BHTN.ToLower() == "true")
                        BaoHiem.isBaoHiemThatNghiep = true;
                    STT++;
                    string PhucLoi = dungchung.GetCell(SheetbaoHiem, i, STT);
                    if (PhucLoi == "1" || PhucLoi.ToLower() == "true")
                    {
                        var HopDongBH = listHopDong.FirstOrDefault(a => a.SoHopDong == BaoHiem.SoHopDong);
                        HopDongBH.isHuongPhucLoi = true;
                    }
                    lstBaoHiem.Add(BaoHiem);
                    STT++;
                    HRM_LuongBaoHiem_HopDong LuongBH = new HRM_LuongBaoHiem_HopDong();
                    LuongBH.IdGuid = Guid.NewGuid().ToString();

                    LuongBH.LuongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiem, i, STT);
                    LuongBH.SoHopDong = BaoHiem.SoHopDong;
                    LuongBH.IdHopDong = BaoHiem.IdHopDong;
                    LuongBH.MaNhanVien = BaoHiem.MaNhanVien;
                    listLuongBHHopDong.Add(LuongBH);

                }
                #region
                if (listLuongBHHopDong != null && listLuongBHHopDong.Count > 0)
                {
                    var ListIdHopDong = listLuongBHHopDong.Select(a => a.IdHopDong).Distinct().ToList();
                    var luongHDDB = dbContext.HRM_BaoHiem_HopDong.Where(a => a.isNgoai == false && ListIdHopDong.Contains(a.IdHopDong)).ToList();
                    dbContext.HRM_BaoHiem_HopDong.RemoveRange(luongHDDB);
                    dbContext.HRM_BaoHiem_HopDong.AddRange(lstBaoHiem);

                    var bhHD = dbContext.HRM_LuongBaoHiem_HopDong.Where(a => ListIdHopDong.Contains(a.IdHopDong)).ToList();
                    dbContext.HRM_LuongBaoHiem_HopDong.RemoveRange(bhHD);
                    dbContext.HRM_LuongBaoHiem_HopDong.AddRange(listLuongBHHopDong);
                }
                #endregion
                if (SheetbaoHiemCN != null)
                {
                    sheetError = SheetbaoHiemCN;
                    for (int i = 3; i <= SheetbaoHiemCN.Dimension.Rows; i++)
                    {
                        nRowError = i;
                        int STT = 2;
                        var BaoHiemNgoai = new HRM_BaoHiem_HopDong();
                        BaoHiemNgoai.IdGuid = Guid.NewGuid().ToString();

                        BaoHiemNgoai.SoHopDong = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetbaoHiemCN, i, STT + 1);

                        if (string.IsNullOrEmpty(BaoHiemNgoai.SoHopDong) && string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
                            break;
                        var HopDongdb = listHopDongdb.FirstOrDefault(a => a.SoHopDong == BaoHiemNgoai.SoHopDong);
                        if (HopDongdb == null)
                            return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Số hợp đồng không tồn tại ở sheet Bảo hiểm ngoài: " + BaoHiemNgoai.SoHopDong);
                        BaoHiemNgoai.IdHopDong = HopDongdb.IdGuid;
                        STT = STT + 3;
                        nColError = STT;
                        BaoHiemNgoai.NgayBatDau = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(HopDong.BaoHiem.NgayBatDau);
                        STT++;
                        nColError = STT;
                        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(HopDong.BaoHiem.NgayKetThuc);
                        STT++;
                        BaoHiemNgoai.SoSoBaoHiem = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        STT++;
                        string MaDonVi = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        if (!string.IsNullOrEmpty(MaDonVi))
                            BaoHiemNgoai.NoiCapSoBaoHiem = listNoiCapBH.Where(a => a.Ma == MaDonVi).Select(a => a.IdGuid).FirstOrDefault();
                        STT++;
                        STT++;
                        BaoHiemNgoai.MucDongBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                        STT++;
                        BaoHiemNgoai.SoTienBaoHiem = dungchung.GetCellToDouble(SheetbaoHiemCN, i, STT);
                        BaoHiemNgoai.isNgoai = true;
                        STT++;
                        string Giam = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        if (Giam.ToUpper() == "CÓ")
                            BaoHiemNgoai.Giam = 1;
                        else
                            BaoHiemNgoai.Giam = 2;
                        STT++;
                        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetbaoHiemCN, i, STT);
                        STT++;
                        lstBaoHiemNgoai.Add(BaoHiemNgoai);
                    }
                }
                #region
                if (lstBaoHiemNgoai != null && lstBaoHiemNgoai.Count > 0)
                {
                    var ListIdHopDong = lstBaoHiemNgoai.Select(a => a.IdHopDong).Distinct().ToList();
                    var luongHDDB = dbContext.HRM_BaoHiem_HopDong.Where(a => a.isNgoai == true && ListIdHopDong.Contains(a.IdHopDong)).ToList();
                    dbContext.HRM_BaoHiem_HopDong.RemoveRange(luongHDDB);
                    dbContext.HRM_BaoHiem_HopDong.AddRange(lstBaoHiemNgoai);
                }
                #endregion
                var lstKhamSucKhoe = new List<HRM_HopDong_KhamSucKhoe>();
                if (SheetKhamSucKhoe != null)
                {
                    sheetError = SheetKhamSucKhoe;
                    for (int i = 4; i <= SheetKhamSucKhoe.Dimension.Rows; i++)
                    {
                        nRowError = i;
                        var BaoHiemNgoai = new HRM_HopDong_KhamSucKhoe();
                        BaoHiemNgoai.MaNhanVien = dungchung.GetCell(SheetKhamSucKhoe, i, 2);
                        if (string.IsNullOrEmpty(BaoHiemNgoai.MaNhanVien))
                            break;
                        nColError = 4;
                        BaoHiemNgoai.NgayKham = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 4);
                        BaoHiemNgoai.NgayKhamUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKham);
                        nColError = 5;
                        BaoHiemNgoai.NgayKetThuc = dungchung.GetCellToDateTime(SheetKhamSucKhoe, i, 5);
                        BaoHiemNgoai.NgayKetThucUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiemNgoai.NgayKetThuc);
                        BaoHiemNgoai.NoiKham = dungchung.GetCell(SheetKhamSucKhoe, i, 6);
                        BaoHiemNgoai.LoaiHinhKham = dungchung.GetCell(SheetKhamSucKhoe, i, 7);
                        BaoHiemNgoai.GhiChu = dungchung.GetCell(SheetKhamSucKhoe, i, 8);
                        lstKhamSucKhoe.Add(BaoHiemNgoai);
                    }
                }
                var listMoTaCongViec = new List<HRM_HopDongCongViec>();
                for (int i = 2; i <= SheetcongViecs.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongCongViec();
                    int STT = 2;
                    model.SoHopDong = dungchung.GetCell(SheetcongViecs, i, STT);

                    model.MaNhanVien = dungchung.GetCell(SheetcongViecs, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;
                    STT = STT + 3;
                    // 2 Tên công việc
                    model.TenCongViec = dungchung.GetCell(SheetcongViecs, i, STT);
                    if (string.IsNullOrEmpty(model.TenCongViec))
                        break;
                    STT++;// 3 mô tả
                    model.MoTa = dungchung.GetCell(SheetcongViecs, i, STT);
                    listMoTaCongViec.Add(model);
                }

                var listTrangThietBi = new List<HRM_HopDongTaiSanBanGiao>();
                var dmTrangThietBi = dbContext.HRM_dmTrangThietBi.ToList();
                for (int i = 2; i <= SheetthietBiBanGiaos.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongTaiSanBanGiao();
                    int STT = 2;
                    model.SoHopDong = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

                    model.MaNhanVien = dungchung.GetCell(SheetthietBiBanGiaos, i, STT + 1);
                    if (string.IsNullOrEmpty(model.SoHopDong) && string.IsNullOrEmpty(model.MaNhanVien))
                        break;
                    STT = STT + 3;
                    string maThietBi = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);
                    if (maThietBi.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
                    {
                        var TrangThietBi = dmTrangThietBi.FirstOrDefault(x => x.Ma == maThietBi);
                        if (TrangThietBi != null)
                        {
                            model.IddmTrangThaiThietBi = TrangThietBi.IdGuid;
                        }
                        else
                        {
                            return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, $"mã trang thiết bị không tồn tại : " + maThietBi);
                        }
                    }

                    STT++;
                    STT++;//  4 số luọng
                    model.SoLuong = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);


                    STT++;// 5 giá trk
                    model.GiaTri = dungchung.GetCellToDouble(SheetthietBiBanGiaos, i, STT);

                    STT++;// 6 mô tả
                    model.MoTa = dungchung.GetCell(SheetthietBiBanGiaos, i, STT);

                    listTrangThietBi.Add(model);
                }
                dbContext.SaveChanges();
            }

            catch (Exception exp)
            {

                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu bị sai định dạng: sheet " + sheetError.Name + " dòng " + nRowError + " cột " + nColError);
            }
            return result;
        }


        public MessageReturn ImportExcelHopDongNhanSu(string aFileName, ApplicationDbContext dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate = PathUploads + aFileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets.First();
            int row = 7;
            int maxrow = sheet1.Dimension.Rows;
            DateTime dt = DateTime.Now;
            var listNhanVien = dbContext.HRM_NhanVien.AsNoTracking().ToList();
            var listTrinhDoChuyenMon = dbContext.HRM_dmTrinhDoDaoTao.ToList();
            var listCapBac = dbContext.HRM_dmCapBac.ToList();
            var listLoaiHopDong = dbContext.HRM_dmLoaiHopDong.ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(dbContext);
            List<NguoiDaiDienCongTyViewModel> lstNguoiDaiDien = new List<NguoiDaiDienCongTyViewModel>();
            var listThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.ToList();
            var listCalamViec = dbContext.HRM_dmLichLamViec.ToList();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(dbContext);
            var listChucVu = _CallWebService.GetDanhSachChucVu(dbContext);
            var CongTyKyHopDong = _CallWebService.GetCongTyKyHopDong(dbContext).FirstOrDefault();
            var listDMPhuCap = dbContext.HRM_dmPhuCap.ToList();
            var listHopDong = new List<HRM_HopDong>();
            var listErr = new List<string>();
            var lstPhuCap = new List<HRM_HopDongPhuCap>();
            var lstBaoHiem = new List<HRM_BaoHiem_HopDong>();
            var lstLuong = new List<HRM_Luong_HopDong>();
            for (int i = row; i <= maxrow; i++)
            {
                var model = new HRM_HopDong();
                var ncol = 2;
                var maNhanVien = dungchung.GetCell(sheet1, i, ncol);
                if (string.IsNullOrEmpty(maNhanVien))
                    break;
                model.IdGuid = Guid.NewGuid().ToString();
                if (CongTyKyHopDong != null)
                    model.IdCongTyKyHopDong = CongTyKyHopDong.Id;
                if (listNhanVien.IsNotEmpty())
                {
                    var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                    if (nhanVien == null)
                    {
                        listErr.Add(maNhanVien);
                        continue;
                    }
                    model.IdNhanVien = nhanVien.IdGuid;
                    model.IdBoPhanEOS = nhanVien.IdBoPhanEOS;
                    model.IdCongTyEOS = nhanVien.IdCongTyEOS;
                }
                ncol += 6;//8

                string MaCaLamViec = dungchung.GetCell(sheet1, row, ncol);
                var CaLamViec = listCalamViec.FirstOrDefault(a => a.Ma == MaCaLamViec);
                if (CaLamViec != null)
                    model.IdHinhThucLamViec = CaLamViec.IdGuid;
                ncol++;//9
                string MaChucVu = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucVu))
                {
                    var ChucVu = listChucVu.FirstOrDefault(a => a.MaChucVu == MaChucVu);
                    if (ChucVu != null)
                        model.IddmChucVu = ChucVu.Id;
                }
                ncol++;//10
                ncol++;//11
                string MaChucDanh = dungchung.GetCell(sheet1, row, ncol);
                if (!string.IsNullOrEmpty(MaChucDanh))
                {
                    var ChucDanh = listChucDanh.FirstOrDefault(a => a.Ma == MaChucDanh);
                    if (ChucDanh != null)
                        model.IddmChucDanh = ChucDanh.Id;
                }

                ncol = 74;
                var maThoiHanHopDong = dungchung.GetCell(sheet1, i, ncol);
                if (listThoiHanHopDong.IsNotEmpty())
                {
                    if (maThoiHanHopDong.IsNotEmpty())
                    {
                        var LHopDong = listThoiHanHopDong.FirstOrDefault(x => x.Ma == maThoiHanHopDong);
                        if (LHopDong != null)
                            model.IdThoiHanHopDong = LHopDong.IdGuid;
                    }
                }
                ncol++;//75
                ncol++;//76
                model.NgayHieuLuc = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                model.NgayHieuLucUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHieuLuc);
                ncol++;//77
                model.NgayHetHan = dungchung.GetCellToDateTime(sheet1, i, ncol);
                if (model.NgayHetHan != null)
                    model.NgayHetHanUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayHetHan);
                ncol++;//78
                string LoaiHopDong = dungchung.GetCell(sheet1, i, ncol);
                if (listLoaiHopDong.IsNotEmpty())
                {
                    if (LoaiHopDong.IsNotEmpty())
                    {
                        var LHopDong = listLoaiHopDong.FirstOrDefault(x => x.Ma == LoaiHopDong);
                        if (LHopDong != null)
                            model.IdLoaiHopDong = LHopDong.IdGuid;
                    }
                }
                //ncol++;//79
                //string LoaiHD = dungchung.GetCell(sheet1, i, ncol);
                //if (LoaiHD == "Không HĐ")
                //    continue;
                ncol++;//79
                ncol++;//80
                model.SoHopDong = dungchung.GetCell(sheet1, i, ncol);
                ncol++;//81
                model.NgayKy = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                if (model.NgayKy != null)
                    model.NgayKyUnix = dungchung.DateTimeToUnixTimeStamp(model.NgayKy);
                ncol += 3;//84
                var BaoHiem = new HRM_BaoHiem_HopDong();
                BaoHiem.IdGuid = Guid.NewGuid().ToString();
                BaoHiem.IdHopDong = model.IdGuid;
                BaoHiem.IdNhanVien = model.IdNhanVien;
                BaoHiem.MucDongBaoHiem = dungchung.GetCellToDouble(sheet1, i, ncol);
                ncol++;//85
                BaoHiem.NgayThayDoiMucDongBHXH = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                if (BaoHiem.NgayThayDoiMucDongBHXH != null)
                    BaoHiem.NgayThayDoiMucDongBHXHUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayThayDoiMucDongBHXH);

                ncol++;//86
                BaoHiem.NgayBatDau = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                if (BaoHiem.NgayBatDau != null)
                    BaoHiem.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
                ncol++;//87
                BaoHiem.SoSoBaoHiem = dungchung.GetCell(sheet1, i, ncol);
                lstBaoHiem.Add(BaoHiem);
                ncol++;//88
                ncol++;//89
                var LuongHopDong = new HRM_Luong_HopDong();
                LuongHopDong.IdGuid = Guid.NewGuid().ToString();
                LuongHopDong.IdHopDong = model.IdGuid;
                LuongHopDong.IdNhanVien = model.IdNhanVien;
                var hinhThucTraLuong = dungchung.GetCell(sheet1, i, ncol);

                if (hinhThucTraLuong.IsNotEmpty())
                {
                    if (hinhThucTraLuong.In(HelperConstant.LuongHopDong.HinhThucTraLuong.ListCode))
                    {

                        LuongHopDong.IddmHinhThucTraLuong = hinhThucTraLuong;
                    }
                }
                ncol++;//90
                ncol++;//91
                string Luong = dungchung.GetCell(sheet1, i, ncol);
                if (Luong != null)
                {
                    if (Luong.Contains("/h"))
                    {
                        double Luong1h = double.Parse(Luong.Split('/')[0]);
                        LuongHopDong.LuongThucTe = Luong1h * 8 * 25;
                    }
                    else
                    {
                        try
                        {
                            LuongHopDong.LuongThucTe = dungchung.GetCellToDouble(sheet1, i, ncol);
                            if (LuongHopDong.LuongThucTe < 100000)
                                LuongHopDong.LuongThucTe = LuongHopDong.LuongThucTe * 8 * 25;
                        }
                        catch
                        {
                            aFileName = "";
                        }
                    }
                }
                ncol++;//92
                LuongHopDong.NgayBatDau = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                if (LuongHopDong.NgayBatDau != null)
                    LuongHopDong.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(LuongHopDong.NgayBatDau);
                ncol++;//93

                LuongHopDong.SoTaiKhoan = dungchung.GetCell(sheet1, i, ncol);
                ncol++;//94
                LuongHopDong.TenNganHang = dungchung.GetCell(sheet1, i, ncol);
                lstLuong.Add(LuongHopDong);
                // phụ cấp
                ncol++;//95

                for (int j = 0; j < 6; j++)
                {
                    HRM_HopDongPhuCap phuCap = new HRM_HopDongPhuCap();
                    phuCap.IdGuid = Guid.NewGuid().ToString();
                    phuCap.SoTien = dungchung.GetCellToDouble(sheet1, i, ncol + j);
                    phuCap.NgayBatDau = new DateTime(2023, 1, 1);
                    if (phuCap.SoTien > 0)
                    {
                        if (j == 0)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "TRACHNHIEM").IdGuid;
                        if (j == 1)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "KIEMNHIEM").IdGuid;
                        if (j == 2)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "ANTRUA").IdGuid;
                        if (j == 3)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "XANGXE").IdGuid;
                        if (j == 4)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "NHAO").IdGuid;
                        if (j == 5)
                            phuCap.IddmLoaiPhuCap = listDMPhuCap.FirstOrDefault(a => a.Ma == "KHAC").IdGuid;
                        phuCap.IdHopDong = model.IdGuid;
                        lstPhuCap.Add(phuCap);
                    }


                }
                listHopDong.Add(model);
            }
            foreach (var item in listHopDong)
            {
                var trangThai = _CallWebService.GetTrangThaiKhoiTao(dbContext, DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), item.IdGuid);
                if (!string.IsNullOrEmpty(trangThai.IdTrangThai))
                {

                    item.IdTrangThai = trangThai.IdTrangThai;
                    item.TenTrangThai = trangThai.TenTrangThai;
                }
                //var err = SetHopDong(item, dbContext);
                //var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

                //Link_NhanVien_BoPhan_ChucVuNonSQL chucVu = new Link_NhanVien_BoPhan_ChucVuNonSQL(item, NV.IdNhanVienEOS);
                //listAddChucVu.Add(chucVu);
            }
            dbContext.HRM_HopDong.AddRange(listHopDong);
            dbContext.HRM_HopDongPhuCap.AddRange(lstPhuCap);
            dbContext.HRM_Luong_HopDong.AddRange(lstLuong);
            dbContext.HRM_BaoHiem_HopDong.AddRange(lstBaoHiem);
            dbContext.SaveChanges();
            //var dongboBoPhanNhanVien = _CallWebService.AddlistChucVuNhanSu(dbContext, listAddChucVu);
            //foreach (var item in listHopDong)
            //{
            //    var err = ChuyenTiepQuyTrinhHopDong(item, dbContext);
            //}
            return result;
        }
        public MessageReturn ImportExcelHopDongNhanSuUpdate(string aFileName, ApplicationDbContext dbContext)
        {
            var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Import dữ liệu hợp đồng thành công");

            string pathTemplate = PathUploads + aFileName;
            if (File.Exists(pathTemplate) == false)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Không tìm thấy file xlsx");
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            s.Close();
            var sheet1 = package.Workbook.Worksheets.First();
            int row = 7;
            int maxrow = sheet1.Dimension.Rows;
            DateTime dt = DateTime.Now;
            var listNhanVien = dbContext.HRM_NhanVien.ToList();
            var listHopDong = dbContext.HRM_HopDong.ToList();
            var listLuong = dbContext.HRM_Luong_HopDong.ToList();
            ////var listLoaiHopDong = dbContext.HRM_dmLoaiHopDong.ToList();
            var listHinhThucTraLuong = dbContext.HRM_dmHinhThucTraLuong.ToList();
            var listThoiHanHopDong = dbContext.HRM_dmThoiHanHopDong.ToList();
            var listCalamViec = dbContext.HRM_dmLichLamViec.ToList();
            var lstBaoHiem = new List<HRM_BaoHiem_HopDong>();
            //var listBoPhan = _CallWebService.GetDanhSachBoPhan(dbContext);
            var listHopDongUpdate = new List<HRM_HopDong>();
            //var listErr = new List<string>();
            //var lstPhuCap = new List<HRM_HopDongPhuCap>();
            var lstBaoHiembh = new List<HRM_LuongBaoHiem_HopDong>();
            var lstLuong = new List<HRM_Luong_HopDong>();
            for (int i = row; i <= maxrow; i++)
            {

                var ncol = 2;
                var maNhanVien = dungchung.GetCell(sheet1, i, ncol);

                if (string.IsNullOrEmpty(maNhanVien))
                    break;
                var nhanVien = listNhanVien.FirstOrDefault(x => x.MaNhanVien == maNhanVien);
                //nhanVien.MaNhanVien = maNhanVien;
                if (maNhanVien == "00030")
                    maNhanVien = "";
                var model = listHopDong.FirstOrDefault(a => a.IdNhanVien == nhanVien.IdGuid);
                ncol = 8;//8

                //string MaCaLamViec = dungchung.GetCell(sheet1, i, ncol);
                //var CaLamViec = listCalamViec.FirstOrDefault(a => a.Ten == MaCaLamViec);
                //if (CaLamViec != null)
                //{
                //    model.IdThoiGianLamViec = CaLamViec.IdGuid;
                //    nhanVien.IdCaLamViec = CaLamViec.IdGuid;
                //}
                //    var MaThoiHan = dungchung.GetCell(sheet1, i, 74);
                //    if (!string.IsNullOrEmpty(MaThoiHan))
                //    {
                //        var ThoiHan = listThoiHanHopDong.FirstOrDefault(a => a.Ma == MaThoiHan);
                //        if (ThoiHan != null)
                //        {
                //            model.IdThoiHanHopDong = ThoiHan.IdGuid;
                //        }
                //    }

                //ncol = 84;//84
                //var BaoHiem = new HRM_BaoHiem_HopDong();
                //BaoHiem.IdGuid = Guid.NewGuid().ToString();
                //BaoHiem.IdHopDong = model.IdGuid;
                //BaoHiem.IdNhanVien = model.IdNhanVien;
                //BaoHiem.MucDongBaoHiem = dungchung.GetCellToDouble(sheet1, i, ncol);
                //ncol++;//85
                //BaoHiem.NgayThayDoiMucDongBHXH = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                //if (BaoHiem.NgayThayDoiMucDongBHXH != null)
                //    BaoHiem.NgayThayDoiMucDongBHXHUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayThayDoiMucDongBHXH);

                //ncol++;//86
                //BaoHiem.NgayBatDau = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                //if (BaoHiem.NgayBatDau != null)
                //    BaoHiem.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(BaoHiem.NgayBatDau);
                //ncol++;//87
                //BaoHiem.SoSoBaoHiem = dungchung.GetCell(sheet1, i, ncol);
                //ncol++;

                //string ThamGiaBaoHiem = dungchung.GetCell(sheet1, i, 88); ;
                //if (ThamGiaBaoHiem.ToLower() == "có")
                //    BaoHiem.isBaoHiemXaHoi = true;
                //else if (ThamGiaBaoHiem.ToLower() == "không")
                //    BaoHiem.isBaoHiemXaHoi = false;



                //lstBaoHiem.Add(BaoHiem);
                //    //ncol = 78;//78

                //    ////ncol++;//79
                //    ////string LoaiHD = dungchung.GetCell(sheet1, i, ncol);
                //    ////if (LoaiHD == "Không HĐ")
                //    ////    continue;
                //    //ncol++;//79
                //    //ncol++;//80

                //    //ncol++;//81
                //ncol = 84;
                //var LuongBaoHiembh = new HRM_LuongBaoHiem_HopDong();
                //LuongBaoHiembh.IdGuid = Guid.NewGuid().ToString();
                //LuongBaoHiembh.IdHopDong = model.IdGuid;
                //LuongBaoHiembh.IdNhanVien = nhanVien.IdGuid;
                //LuongBaoHiembh.LuongBaoHiem = dungchung.GetCellToDouble(sheet1, i, ncol);
                //lstBaoHiembh.Add(LuongBaoHiembh);
                ncol = 89;//89
                var LuongHopDong = new HRM_Luong_HopDong();
                LuongHopDong.IdGuid = Guid.NewGuid().ToString();
                LuongHopDong.IdHopDong = model.IdGuid;
                LuongHopDong.IdNhanVien = model.IdNhanVien;
                var hinhThucTraLuong = dungchung.GetCell(sheet1, i, ncol);

                if (hinhThucTraLuong.IsNotEmpty())
                {
                    if (hinhThucTraLuong.In(HelperConstant.LuongHopDong.HinhThucTraLuong.ListCode))
                    {

                        LuongHopDong.IddmHinhThucTraLuong = hinhThucTraLuong;
                    }
                }
                ncol++;//90
                ncol++;//91
                string Luong = dungchung.GetCell(sheet1, i, ncol);
                if (Luong != null)
                {
                    if (Luong.Contains("/h"))
                    {
                        double Luong1h = double.Parse(Luong.Split('/')[0]);
                        LuongHopDong.LuongBaoHiem = Luong1h * 8 * 25;
                    }
                    else
                    {
                        try
                        {
                            LuongHopDong.LuongBaoHiem = dungchung.GetCellToDouble(sheet1, i, ncol);
                            if (LuongHopDong.LuongBaoHiem < 100000)
                                LuongHopDong.LuongBaoHiem = LuongHopDong.LuongBaoHiem * 8 * 25;
                        }
                        catch
                        {
                            aFileName = "";
                        }
                    }
                }
                ncol++;//92
                LuongHopDong.NgayBatDau = dungchung.GetCellToDateTime(sheet1, i, ncol) ?? dt;
                if (LuongHopDong.NgayBatDau != null)
                    LuongHopDong.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(LuongHopDong.NgayBatDau);
                ncol++;//93

                LuongHopDong.SoTaiKhoan = dungchung.GetCell(sheet1, i, ncol);
                ncol++;//94
                LuongHopDong.TenNganHang = dungchung.GetCell(sheet1, i, ncol);
                lstLuong.Add(LuongHopDong);

                //dbContext.Entry(LuongHopDong).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //dbContext.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //dbContext.Entry(nhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            //List<HRM.Api.Models.Link_NhanVien_BoPhan_ChucVuNonSQL> listAddChucVu = new List<HRM.Api.Models.Link_NhanVien_BoPhan_ChucVuNonSQL>();
            //List<HRM_NhanVien> listNhanVienUpdate = new List<HRM_NhanVien>();
            //foreach (var item in listHopDong)
            //{
            //    CallWebService _CallWebService = _CallWebService;
            //    var trangThai = _CallWebService.GetTrangThaiKhoiTao(dbContext, DefineData.eAction.QUYTRINHHOPDONG.ToString(), item.IdGuid);
            //    if (!string.IsNullOrEmpty(trangThai.IdTrangThai))
            //    {

            //        item.IdTrangThai = trangThai.IdTrangThai;
            //        item.TenTrangThai = trangThai.TenTrangThai;
            //    }
            //    //var err = SetHopDong(item, dbContext);
            //    //var NV = listNhanVien.FirstOrDefault(a => a.IdGuid == item.IdNhanVien);

            //    //Link_NhanVien_BoPhan_ChucVuNonSQL chucVu = new Link_NhanVien_BoPhan_ChucVuNonSQL(item, NV.IdNhanVienEOS);
            //    //listAddChucVu.Add(chucVu);
            //}
            //dbContext.HRM_HopDong.AddRange(listHopDong);

            dbContext.HRM_Luong_HopDong.AddRange(lstLuong);
            //dbContext.HRM_LuongBaoHiem_HopDong.AddRange(lstBaoHiembh);
            //dbContext.HRM_BaoHiem_HopDong.AddRange(lstBaoHiem);
            dbContext.SaveChanges();
            //var dongboBoPhanNhanVien = _CallWebService.AddlistChucVuNhanSu(dbContext, listAddChucVu);
            //foreach (var item in listHopDong)
            //{
            //    var err = ChuyenTiepQuyTrinhHopDong(item, dbContext);
            //}
            return result;
        }
        public dynamic ExportMauPhuCap(ApplicationDbContext db)
        {
            try
            {
                string sFile = "ImportPhuCap.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);

                var filenamegui = Guid.NewGuid().ToString() + ".xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets[2];
                var listLoaiPhuCap = db.HRM_dmPhuCap.ToList();
                int nRow = 3;
                int STT = 1;
                foreach (var item in listLoaiPhuCap)
                {

                    sheet1.Cells[nRow, 1].Value = STT;
                    sheet1.Cells[nRow, 2].Value = item.Ma;
                    sheet1.Cells[nRow, 3].Value = item.Ten;
                    nRow++;
                    STT++;
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
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        public dynamic ImportPhuCapNhanVien(string aFileName, ApplicationDbContext db)
        {
            try
            {
                string pathTemplate = PathUploads + aFileName;

                using Stream s = File.OpenRead(pathTemplate);
                using ExcelPackage package = new ExcelPackage(s);
                s.Close();

                var phuCap = package.Workbook.Worksheets[1];

                var listDMPhuCap = db.HRM_dmPhuCap.ToList();

                List<string> listSoHopDong = new List<string>();
                var listPhuCap = new List<HRM_HopDongPhuCap>();
                for (int i = 3; i <= phuCap.Dimension.Rows; i++)
                {
                    var model = new HRM_HopDongPhuCap();
                    model.IdGuid = Guid.NewGuid().ToString();
                    var STT = 2;
                    var SoHopDong = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();
                    if (SoHopDong == "")
                        continue;
                    model.SoHopDong = SoHopDong;
                    listSoHopDong.Add(SoHopDong);

                    STT++; //3 Tên Nhân Viên

                    STT++;//4
                    var maDMPhuCap = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();
                    if (maDMPhuCap == "")
                        continue;
                    if (maDMPhuCap.IsNotEmpty() && listDMPhuCap.IsNotEmpty())
                    {
                        var dmPhuCap = listDMPhuCap.FirstOrDefault(x => x.Ma == maDMPhuCap);
                        if (dmPhuCap != null)
                        {
                            model.IddmLoaiPhuCap = dmPhuCap.IdGuid;
                        }
                    }
                    STT++;//6
                    var soTien = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();

                    if (soTien.IsNotEmpty())
                    {
                        double n;
                        var isSoTien = double.TryParse(soTien, out n);
                        if (isSoTien == true)
                        {
                            model.SoTien = n;
                        }
                    }
                    STT++;//7
                    var thoiGianBatDau = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();

                    if (thoiGianBatDau.IsNotEmpty())
                    {
                        DateTime date;
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        var checkDate = DateTime.TryParseExact(thoiGianBatDau, new string[] { "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy-MM-dd" }, provider, DateTimeStyles.None, out date);
                        if (checkDate == true)
                        {
                            model.NgayBatDau = date;
                        }
                    }
                    else
                        model.NgayBatDau = dungchung.UnixTimeStampToDateTime(0);
                    STT++;//8
                    var thoiGianKetThuc = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();

                    if (thoiGianKetThuc.IsNotEmpty())
                    {
                        DateTime date;
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        var checkDate = DateTime.TryParseExact(thoiGianKetThuc, new string[] { "dd/MM/yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy-MM-dd" }, provider, DateTimeStyles.None, out date);
                        if (checkDate == true)
                        {
                            model.NgayKetThuc = date;
                        }
                    }
                    var loaiPhuCap = phuCap.Cells[i, STT].Value == null ? "" : phuCap.Cells[i, STT].Value.ToString();

                    STT++;//9
                    string LoaihuongPhuCap = phuCap.Cells[i, STT].Value == null ? "0" : phuCap.Cells[i, STT].Value.ToString();
                    try
                    {
                        model.LoaiPhuCap = int.Parse(LoaihuongPhuCap);
                    }
                    catch (Exception)
                    {

                        model.LoaiPhuCap = 1;
                    }
                    listPhuCap.Add(model);
                }
                var listHopDong = db.HRM_HopDong.Where(r => listSoHopDong.Contains(r.SoHopDong)).ToList();
                var listIdHopDong = listHopDong.Select(r => r.IdGuid).ToList();
                List<HRM_Luong_PhuCap_NhanVien> listPhuCapNew = new List<HRM_Luong_PhuCap_NhanVien>();
                var listluongNhanVien = db.HRM_Luong_NhanVien.Where(r => listIdHopDong.Contains(r.IdNguonNhapLuong)).ToList();
                foreach (var item in listPhuCap)
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    var hopDong = listHopDong.Where(r => r.SoHopDong == item.SoHopDong).FirstOrDefault();
                    if (hopDong == null)
                        continue;
                    if (hopDong.isKetThuc == true)
                    {
                        var luong_NhanVien = listluongNhanVien.Where(r => r.IdNguonNhapLuong == hopDong.IdGuid).FirstOrDefault();
                        HRM_Luong_PhuCap_NhanVien phucap = new HRM_Luong_PhuCap_NhanVien
                        {
                            NgayBatDauHieuLuc = item.NgayBatDau,
                            IddmLoaiPhuCap = item.IddmLoaiPhuCap,
                            IdGuid = Guid.NewGuid().ToString(),
                            IdLuong_NhanVien = luong_NhanVien.IdGuid,
                            IdNguonNhapLuong = hopDong.IdGuid,
                            LoaiPhuCap = item.LoaiPhuCap,
                            IdNhanVien = hopDong.IdNhanVien,
                            SoTien = item.SoTien,
                            NgayHetHieuLuc = item.NgayKetThuc,
                            NguonNhapLuong = "HRM_HopDong",
                        };
                        listPhuCapNew.Add(phucap);
                    }
                }
                db.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCapNew);
                db.HRM_HopDongPhuCap.AddRange(listPhuCap);
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");

            }
            catch (Exception ex)
            {

                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật không thành công " + ex.Message);
            }

        }
        #endregion
        public dynamic ChuyenTrangThaiHopDong(ApplicationDbContext db, List<string> listIdHopDong)
        {
            try
            {
                var listHopDong = db.HRM_HopDong.Where(a => listIdHopDong.Contains(a.IdGuid)).ToList();
                //var listHopDong = db.HRM_HopDong.ToList();
                listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
                var listLuong = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                var listLuongBH = db.HRM_LuongBaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                var PhuCap = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                var BaoHiem = db.HRM_BaoHiem_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
                foreach (var QuyTrinh in listHopDong)
                {
                    QuyTrinh.LuongHopDong = listLuong.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                    QuyTrinh.LuongBaoHiem = listLuongBH.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                    QuyTrinh.BaoHiem = BaoHiem.FirstOrDefault(a => a.IdHopDong == QuyTrinh.IdGuid);
                    QuyTrinh.lstPhuCap = PhuCap.Where(a => a.IdHopDong == QuyTrinh.IdGuid).ToList();
                    CallWebService callWeb = _CallWebService;
                    ChuyenTrangThai audit = new ChuyenTrangThai()
                    {
                        IdGuid = QuyTrinh.IdGuid,
                        eAction = DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(),
                        IdTrangThai = QuyTrinh.IdTrangThai,

                    };
                    TrangThaiQuyTrinh TrangThaidnm = callWeb.GetTrangThaiChuyenTiep(db, audit);
                    if (TrangThaidnm != null)
                    {
                        string IdTrangThai = TrangThaidnm.IdTrangThai;
                        string TenTrangThai = TrangThaidnm.TenTrangThai;
                        if (IdTrangThai == "" || IdTrangThai == null)
                        {
                            TenTrangThai = TrangThaidnm.TenTrangThai;
                        }
                        else
                        {
                            QuyTrinh.IdTrangThai = TrangThaidnm.IdTrangThai;
                            QuyTrinh.TenTrangThai = TrangThaidnm.TenTrangThai;
                            if (QuyTrinh.isKetThuc == false && (TrangThaidnm.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc || TrangThaidnm.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.XuLyDacBiet))
                            {
                                LogLichSuNhanVien(db, QuyTrinh.IdNhanVien, QuyTrinh.IdGuid, "HRM_HopDong", DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(), QuyTrinh.NgayKyUnix);
                                QuyTrinh.isHuongPhucLoi = true;
                                QuyTrinh.isKetThuc = true;
                                QuyTrinh.TrangThai = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
                                var listHopDongCuHieuLuc = db.HRM_HopDong.Where(a => a.isKetThuc == true && a.IdNhanVien == QuyTrinh.IdNhanVien && a.IdGuid != QuyTrinh.IdGuid).ToList();
                                if (listHopDongCuHieuLuc != null && listHopDongCuHieuLuc.Count > 0)
                                {
                                    foreach (var item in listHopDongCuHieuLuc)
                                    {
                                        item.TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                    }

                                }
                                if (Host != "ICT")
                                {
                                    HRM_NhanVien nhanVien = db.HRM_NhanVien.Find(QuyTrinh.IdNhanVien);
                                    var LoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.Ma == "HĐLĐ" || a.Ma == "KTHHĐ" || a.Ma == "THHĐ" || a.Ma == "HĐL");
                                    if (LoaiHopDong.IdGuid == QuyTrinh.IdGuid)
                                    {

                                        if (nhanVien != null && nhanVien.NgayBatDauChinhThuc == null)
                                        {
                                            nhanVien.NgayBatDauChinhThuc = QuyTrinh.NgayHieuLuc;
                                        }
                                    }
                                    if (nhanVien != null && nhanVien.NgayBatDauLamViec == null)
                                    {
                                        nhanVien.NgayBatDauLamViec = QuyTrinh.NgayHieuLuc;
                                    }
                                    nhanVien.SoNgayNghiPhepThamNien = QuyTrinh.SoNgayNghiPhepThamNien;
                                    nhanVien.SoNgayNghiPhep = QuyTrinh.SoNgayNghiPhep;
                                    if (nhanVien.TrangThai == DefineData.eTrangThaiNhanVien.DaNghiViec.ToString())
                                    {
                                        nhanVien.NgayQuayLai = DateTime.Now;
                                        nhanVien.NgayNghiViec = null;
                                        nhanVien.TrangThai = DefineData.eTrangThaiNhanVien.DangLamViec.ToString();
                                    }
                                    if (QuyTrinh.IdLoaiHopDong != null && QuyTrinh.IdLoaiHopDong != "")
                                    {
                                        try
                                        {
                                            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.Find(QuyTrinh.IdLoaiHopDong);
                                            if (nhanVien.NgayBatDauLamViec == null)
                                            {
                                                if (dmLoaiHopDong.Ma == "HĐLĐ")
                                                    nhanVien.NgayBatDauLamViec = QuyTrinh.NgayHieuLuc;

                                            }
                                            nhanVien.TrangThai = "DangLamViec";
                                            string MaLoaiHopDong = dmLoaiHopDong.Ma;
                                            var LoaiNV = db.HRM_dmLoaiNhanVien.FirstOrDefault(r => r.Ma == MaLoaiHopDong);
                                            if (LoaiNV != null)
                                                nhanVien.IdLoaiNhanVien = LoaiNV.IdGuid;
                                        }
                                        catch (Exception)
                                        {

                                        }

                                    }

                                    nhanVien.NgayNghiViec = null;
                                    nhanVien.SoNgayNghiPhepThamNien = QuyTrinh.SoNgayNghiPhepThamNien;
                                    nhanVien.IdChucDanh = QuyTrinh.IddmChucDanh;
                                    nhanVien.IdChucVu = QuyTrinh.IddmChucVu;
                                    nhanVien.IdCaLamViec = QuyTrinh.IdThoiGianLamViec;
                                    nhanVien.IdBoPhanEOS = QuyTrinh.IdBoPhanEOS;
                                    nhanVien.PhiCongDoan = QuyTrinh.PhiCongDoan;
                                    nhanVien.IdCongTyEOS = QuyTrinh.IdCongTyEOS;
                                    if (QuyTrinh.MaSoThueTNCN != "" && QuyTrinh.MaSoThueTNCN != null)
                                        nhanVien.MaSoThueTNCN = QuyTrinh.MaSoThueTNCN;
                                    if (QuyTrinh.BaoHiem != null && (QuyTrinh.BaoHiem.SoSoBaoHiem != null || QuyTrinh.BaoHiem.SoSoBaoHiem != ""))
                                        nhanVien.SoBaoHiemXaHoi = QuyTrinh.BaoHiem.SoSoBaoHiem;

                                    db.Entry(nhanVien).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                    var nhanvienTKNH = db.HRM_NhanVien_TaiKhoanNganHang.FirstOrDefault(r => r.idNhanVien == QuyTrinh.IdNhanVien);
                                    if (QuyTrinh.LuongHopDong != null && QuyTrinh.LuongHopDong.SoTaiKhoan != null && QuyTrinh.LuongHopDong.TenNganHang != null)
                                    {
                                        if (nhanvienTKNH != null)
                                        {
                                            nhanvienTKNH.SoTaiKhoan = QuyTrinh.LuongHopDong.SoTaiKhoan;
                                            nhanvienTKNH.TenNganHang = QuyTrinh.LuongHopDong.TenNganHang;
                                            db.Entry(nhanvienTKNH).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        }
                                        else
                                            db.HRM_NhanVien_TaiKhoanNganHang.Add(new HRM_NhanVien_TaiKhoanNganHang(QuyTrinh));
                                    }
                                    List<HRM_Luong_NhanVien> luongs = db.HRM_Luong_NhanVien.Where(r => r.IdNhanVien == QuyTrinh.IdNhanVien && (r.NgayHetHieuLuc == null || r.NgayHetHieuLuc >= QuyTrinh.NgayHieuLuc)).OrderBy(a => a.Modified).ToList();
                                    if (luongs != null)
                                    {
                                        foreach (var item in luongs)
                                        {
                                            if (QuyTrinh.NgayHieuLuc == null)
                                                item.NgayHetHieuLuc = null;
                                            else
                                                item.NgayHetHieuLuc = QuyTrinh.NgayHieuLuc.AddDays(-1);
                                            db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                        }

                                    }
                                    if (QuyTrinh.BaoHiem != null)
                                    {
                                        var baohiem_hopdong = QuyTrinh.BaoHiem;
                                        if (baohiem_hopdong.isBaoHiemXaHoi == true)
                                        {
                                            double luongBHCu = 0;
                                            if (luongs != null && luongs.Count > 0)
                                            {
                                                luongBHCu = luongs.FirstOrDefault().LuongHachToan;

                                            }
                                            if (luongBHCu == 0 || luongBHCu < QuyTrinh.LuongBaoHiem.LuongBaoHiem)
                                            {
                                                HRM_TangBaoHiem tangBaoHiem = new HRM_TangBaoHiem();
                                                tangBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                                tangBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                                tangBaoHiem.ThoiGianDong = QuyTrinh.NgayHieuLuc;
                                                tangBaoHiem.Loai = eLoaiBaoHiem.TangMoi.ToString();
                                                tangBaoHiem.MucDong = QuyTrinh.LuongBaoHiem.LuongBaoHiem;
                                                db.HRM_TangBaoHiem.Add(tangBaoHiem);
                                            }
                                            else if (luongBHCu < QuyTrinh.LuongBaoHiem.LuongBaoHiem)
                                            {
                                                HRM_GiamBaoHiem GiamBaoHiem = new HRM_GiamBaoHiem();
                                                GiamBaoHiem.IdGuid = Guid.NewGuid().ToString();
                                                GiamBaoHiem.IdNhanVien = nhanVien.IdGuid;
                                                GiamBaoHiem.NgayGiam = QuyTrinh.NgayHieuLuc;
                                                GiamBaoHiem.Loai = eLoaiBaoHiem.GiamBaoHiem.ToString();
                                                GiamBaoHiem.MucDong = QuyTrinh.LuongBaoHiem.LuongBaoHiem;
                                                db.HRM_GiamBaoHiem.Add(GiamBaoHiem);
                                            }
                                        }
                                    }
                                    HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(QuyTrinh);
                                    if (nhanVien.isLuongHuu == true)
                                        luong_NhanVien.LuongHachToan = 0;
                                    db.HRM_Luong_NhanVien.Add(luong_NhanVien);
                                    if (QuyTrinh.lstPhuCap != null)
                                    {
                                        List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
                                        foreach (var item in QuyTrinh.lstPhuCap)
                                            listPhuCap.Add(new HRM_Luong_PhuCap_NhanVien(QuyTrinh, item, luong_NhanVien.IdGuid, nameof(HRM_HopDong), Host));
                                        db.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCap);
                                    }
                                    QuyTrinh.IdNhanVienEOS = nhanVien.IdNhanVienEOS;
                                    clsLink_NhanVien_BoPhan_ChucVu chucvuEOS = _CallWebService.UpdateChucVuNhanSu(db, QuyTrinh);
                                }
                            }

                        }
                    }

                    db.Entry(QuyTrinh).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                db.SaveChanges();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
        public void UpdateLuong_PhuCapHopDong(ApplicationDbContext db)
        {
            var listHopDong = db.HRM_HopDong.Where(a => a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString()).ToList();
            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai == "DangLamViec").ToList();
            var listLuongHopDong = db.HRM_Luong_HopDong.ToList();
            var listLuongNhanVien = db.HRM_Luong_NhanVien.ToList();
            var listLuongBHNhanVien = db.HRM_LuongBaoHiem_HopDong.ToList();
            var listPhuCapdb = db.HRM_HopDongPhuCap.ToList();
            foreach (var NhanVien in listNhanVien)
            {

                var hopdong = listHopDong.Where(a => a.IdNhanVien == NhanVien.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                if (hopdong != null)
                {
                    hopdong.LuongHopDong = listLuongHopDong.FirstOrDefault(a => a.IdHopDong == hopdong.IdGuid);
                    hopdong.LuongBaoHiem = listLuongBHNhanVien.FirstOrDefault(a => a.IdHopDong == hopdong.IdGuid);
                    HRM_Luong_NhanVien luong_NhanVien = new HRM_Luong_NhanVien(hopdong);
                    db.HRM_Luong_NhanVien.Add(luong_NhanVien);
                    List<HRM_Luong_PhuCap_NhanVien> listPhuCap = new List<HRM_Luong_PhuCap_NhanVien>();
                    hopdong.lstPhuCap = listPhuCapdb.Where(a => a.IdHopDong == hopdong.IdGuid).ToList();
                    foreach (var item in hopdong.lstPhuCap)
                        listPhuCap.Add(new HRM_Luong_PhuCap_NhanVien()
                        {
                            NgayBatDauHieuLuc = item.NgayBatDau.Value,
                            IddmLoaiPhuCap = item.IddmLoaiPhuCap,
                            IdGuid = Guid.NewGuid().ToString(),
                            IdLuong_NhanVien = hopdong.LuongHopDong.IdGuid,
                            IdNguonNhapLuong = hopdong.IdGuid,
                            LoaiPhuCap = item.LoaiPhuCap,
                            IdNhanVien = hopdong.IdNhanVien,
                            SoTien = item.SoTien,
                            NgayHetHieuLuc = item.NgayKetThuc,
                            NguonNhapLuong = nameof(HRM_HopDong),
                            IdCongTyEOS = hopdong.IdCongTyEOS,
                        });
                    db.HRM_Luong_PhuCap_NhanVien.AddRange(listPhuCap);
                }
            }
            db.SaveChanges();
        }
        public dynamic GetdanhTaiSanBanGiaoNhanVien(string IdNhanVien, ApplicationDbContext db)
        {
            var Idhopdong = (from a in db.HRM_HopDong where a.IdNhanVien == IdNhanVien && a.TrangThai == "HieuLuc" select a.IdGuid).FirstOrDefault();


            var HopDongTaiSanBanGiaos = db.HRM_HopDongTaiSanBanGiao.Where(r => r.IdHopDong == Idhopdong).ToList();
            var dmTaiSans = db.HRM_dmTrangThietBi.ToList();
            foreach (var item in HopDongTaiSanBanGiaos)
            {
                item.NgayBanGiaoUnix = dungchung.DateTimeToUnixTimeStamp(item.NgayBanGiao);
                item.TenTaiSanBanGiao = dmTaiSans.Where(r => r.IdGuid == item.IdTaiSanBanGiao).Select(r => r.Ten).FirstOrDefault();
            }

            return HopDongTaiSanBanGiaos;
        }
        public dynamic UpdateTrangThaiHetHieuLuc(ApplicationDbContext db)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    string TrangThai = DefineData.eTrangTHaiHopDong.HetHieuLuc.ToString();
                    DateTime DateTimeNow = DateTime.Now.AddDays(1);

                    var listHopDong = (from a in db.HRM_HopDong
                                       where a.NgayHetHan < DateTimeNow && a.TrangThai != TrangThai
                                       select a).ToList();
                    foreach (var item in listHopDong)
                    {
                        item.TrangThai = TrangThai;
                    }
                    db.SaveChanges();
                    dbContextTransaction.Commit();
                    return "0";
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    string loi = "có lỗi xảy ra trong quá trình xử lý " + ex.Message;
                    return loi;
                }

            }
        }
        public dynamic UpdateChucVuChucDanhHopDong(ApplicationDbContext db)
        {
            var listHopDong = db.HRM_HopDong.Where(a => a.isKetThuc == true).Distinct().ToList();
            var idsNv = listHopDong.Select(a => a.IdNhanVien).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            var nhanVienById = db.HRM_NhanVien.AsNoTracking()
                .Where(a => idsNv.Contains(a.IdGuid))
                .ToDictionary(a => a.IdGuid);
            foreach (var item in listHopDong)
            {
                if (!string.IsNullOrEmpty(item.IdNhanVien) && nhanVienById.TryGetValue(item.IdNhanVien, out var nhanVien))
                {
                    item.IddmChucDanh = nhanVien.IdChucDanh;
                    item.IddmChucVu = nhanVien.IdChucVu;

                    db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
            }
            db.SaveChanges();
            return 0;
        }
        public List<HRM_TheoDoiHopDong_NhanVien> GetTongHopTheoDoiHopDong(ApplicationDbContext db, TimKiemBangLuong timkiem)
        {

            var listNhanVien = db.HRM_NhanVien.Where(a => a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString());
            if (timkiem.IdCongty > 0)
                listNhanVien = listNhanVien.Where(a => a.IdCongTyEOS == timkiem.IdCongty);
            if (timkiem.listIdBoPhan.Count > 0)
                listNhanVien = listNhanVien.Where(a => timkiem.listIdBoPhan.Contains(a.IdBoPhanEOS));
            if (!string.IsNullOrEmpty(timkiem.Keyword))
                listNhanVien = listNhanVien.Where(a => a.MaNhanVien.ToLower().Contains(timkiem.Keyword.Trim().ToLower()) || a.TenDayDu.ToLower().Contains(timkiem.Keyword.Trim().ToLower()));
            List<string> listIdNV = listNhanVien.Select(a => a.IdGuid).Distinct().ToList();
            var listHopDong = db.HRM_HopDong.Where(a => listIdNV.Contains(a.IdNhanVien) && a.isKetThuc == true).OrderByDescending(a => a.Modified).ToList();
            List<string> listIdHopDong = listHopDong.Select(a => a.IdGuid).Distinct().ToList();
            var listPhuCapHopDong = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
            var listLuongHopDong = db.HRM_Luong_HopDong.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();
            var ThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
            var LoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
            var listCa = db.HRM_dmLichLamViec.ToList();
            var listPhuCap = db.HRM_dmPhuCap.ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            DateTime dt = DateTime.Now;
            List<HRM_TheoDoiHopDong_NhanVien> listresult = new List<HRM_TheoDoiHopDong_NhanVien>();
            foreach (var NhanVien in listNhanVien)
            {
                HRM_TheoDoiHopDong_NhanVien itemAdd = new HRM_TheoDoiHopDong_NhanVien();
                var congTy = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdCongTyEOS);
                var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == NhanVien.IdBoPhanEOS);
                if (congTy != null)
                    itemAdd.CoSo = congTy.TenBoPhan;
                itemAdd.MaNhanVien = NhanVien.MaNhanVien;
                itemAdd.TenNhanVien = NhanVien.TenDayDu;
                double Thang = 0;
                if (NhanVien.NgayBatDauLamViec != null)
                {
                    itemAdd.NgayBatDauLamViec = NhanVien.NgayBatDauLamViec;
                    Thang = (dt - (itemAdd.NgayBatDauLamViec ?? dt)).TotalDays / 30.5;
                    itemAdd.ThamNien = Math.Round(Thang, 2).ToString();
                }
                if (BoPhan != null)
                    itemAdd.BoPhan = BoPhan.TenBoPhan;
                var HopDong = listHopDong.FirstOrDefault(a => a.TrangThai == DefineData.eTrangTHaiHopDong.HieuLuc.ToString() && a.IdNhanVien == NhanVien.IdGuid);
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

                // học việc
                var LoaiHDHocViec = LoaiHopDong.FirstOrDefault(a => a.Ma == "HĐHV");
                var HopDongHocViec = listHopDong.FirstOrDefault(a => a.IdLoaiHopDong == LoaiHDHocViec.IdGuid && a.IdNhanVien == NhanVien.IdGuid);
                if (HopDongHocViec != null)
                {
                    var thoiHanHopDong = ThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDongHocViec.IdThoiHanHopDong);
                    if (thoiHanHopDong != null)
                        itemAdd.ThoiHan = thoiHanHopDong.Ten;
                    itemAdd.LoaiHopDong = LoaiHDHocViec.Ten;
                    itemAdd.TuNgay = HopDongHocViec.NgayHieuLuc;
                    itemAdd.DenNgay = HopDongHocViec.NgayHetHan;

                }
                // thử việc
                var LoaiHDVuViec = LoaiHopDong.FirstOrDefault(a => a.Ma == "HĐTV");
                var HopDongVuViec = listHopDong.FirstOrDefault(a => a.IdLoaiHopDong == LoaiHDVuViec.IdGuid && a.IdNhanVien == NhanVien.IdGuid);
                itemAdd.HopDongVuViec = new HRM_TheoDoiHopDong_NhanVien_HopDong();
                if (HopDongVuViec != null)
                {
                    var Luong = listLuongHopDong.FirstOrDefault(a => a.IdHopDong == HopDongVuViec.IdHopDong);
                    if (Luong != null)
                        itemAdd.HopDongVuViec.Luong = Luong.LuongBaoHiem;
                    var PhuCaps = listPhuCapHopDong.Where(a => a.IdHopDong == HopDongVuViec.IdHopDong).ToList();
                    if (PhuCaps != null && PhuCaps.Count > 0)
                    {
                        List<string> listId = PhuCaps.Select(a => a.IddmLoaiPhuCap).Distinct().ToList();
                        var phucap = listPhuCap.Where(a => listId.Contains(a.IdGuid)).ToList();
                        itemAdd.HopDongVuViec.CacLoaiPhuCap = PhuCaps.Sum(a => a.SoTien);
                    }
                    itemAdd.HopDongVuViec.Ngay = HopDongVuViec.NgayHieuLuc;
                }
                var LoaiHDCHinhThuc = LoaiHopDong.Where(a => a.Ma == "HĐLĐ" || a.Ma == "HĐLĐ.KXDTH").ToList();
                var listID = LoaiHDCHinhThuc.Select(a => a.IdGuid).ToList();
                // 12 tháng
                var thoiHanHopDong12Thang = ThoiHanHopDong.FirstOrDefault(a => a.SoThang == 12);
                var HopDong12Thang = listHopDong.Where(a => listID.Contains(a.IdLoaiHopDong) && a.IdThoiHanHopDong == thoiHanHopDong12Thang.IdGuid && a.IdNhanVien == NhanVien.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                itemAdd.HopDong12Thang = new HRM_TheoDoiHopDong_NhanVien_HopDong();
                if (HopDong12Thang != null)
                {
                    var Luong = listLuongHopDong.FirstOrDefault(a => a.IdHopDong == HopDong12Thang.IdGuid);
                    if (Luong != null)
                        itemAdd.HopDong12Thang.Luong = Luong.LuongBaoHiem;
                    var PhuCaps = listPhuCapHopDong.Where(a => a.IdHopDong == HopDong12Thang.IdGuid).ToList();
                    if (PhuCaps != null && PhuCaps.Count > 0)
                    {
                        List<string> listId = PhuCaps.Select(a => a.IddmLoaiPhuCap).Distinct().ToList();
                        var phucap = listPhuCap.Where(a => listId.Contains(a.IdGuid)).ToList();
                        itemAdd.HopDong12Thang.CacLoaiPhuCap = PhuCaps.Sum(a => a.SoTien);
                    }
                    itemAdd.HopDong12Thang.Ngay = HopDong12Thang.NgayHieuLuc;
                }
                // 36 tháng
                var thoiHanHopDong36Thang = ThoiHanHopDong.FirstOrDefault(a => a.SoThang == 36);
                var HopDong36Thang = listHopDong.Where(a => listID.Contains(a.IdLoaiHopDong) && a.IdThoiHanHopDong == thoiHanHopDong36Thang.IdGuid && a.IdNhanVien == NhanVien.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                itemAdd.HopDong36Thang = new HRM_TheoDoiHopDong_NhanVien_HopDong();
                if (HopDong36Thang != null)
                {
                    var Luong = listLuongHopDong.FirstOrDefault(a => a.IdHopDong == HopDong36Thang.IdGuid);
                    if (Luong != null)
                        itemAdd.HopDong36Thang.Luong = Luong.LuongBaoHiem;
                    var PhuCaps = listPhuCapHopDong.Where(a => a.IdHopDong == HopDong36Thang.IdGuid).ToList();
                    if (PhuCaps != null && PhuCaps.Count > 0)
                    {
                        List<string> listId = PhuCaps.Select(a => a.IddmLoaiPhuCap).Distinct().ToList();
                        var phucap = listPhuCap.Where(a => listId.Contains(a.IdGuid)).ToList();
                        itemAdd.HopDong36Thang.CacLoaiPhuCap = PhuCaps.Sum(a => a.SoTien);
                    }
                    itemAdd.HopDong36Thang.Ngay = HopDong36Thang.NgayHieuLuc;
                }
                // KTH tháng
                var thoiHanHopDongKTHThang = ThoiHanHopDong.FirstOrDefault(a => a.Ma == "KTH");
                var HopDongKTHThang = listHopDong.Where(a => listID.Contains(a.IdLoaiHopDong) && a.IdThoiHanHopDong == thoiHanHopDongKTHThang.IdGuid && a.IdNhanVien == NhanVien.IdGuid).OrderByDescending(a => a.Modified).FirstOrDefault();
                itemAdd.HopDongKhongThoiHan = new HRM_TheoDoiHopDong_NhanVien_HopDong();
                if (HopDongKTHThang != null)
                {
                    var Luong = listLuongHopDong.FirstOrDefault(a => a.IdHopDong == HopDongKTHThang.IdGuid);
                    if (Luong != null)
                        itemAdd.HopDongKhongThoiHan.Luong = Luong.LuongBaoHiem;
                    var PhuCaps = listPhuCapHopDong.Where(a => a.IdHopDong == HopDongKTHThang.IdGuid).ToList();
                    if (PhuCaps != null && PhuCaps.Count > 0)
                    {
                        List<string> listId = PhuCaps.Select(a => a.IddmLoaiPhuCap).Distinct().ToList();
                        var phucap = listPhuCap.Where(a => listId.Contains(a.IdGuid)).ToList();
                        itemAdd.HopDongKhongThoiHan.CacLoaiPhuCap = PhuCaps.Sum(a => a.SoTien);
                    }
                    itemAdd.HopDongKhongThoiHan.Ngay = HopDongKTHThang.NgayHieuLuc;
                }
                listresult.Add(itemAdd);
            }

            return listresult;
        }
        public dynamic ExportTongHopTheoDoiHopDong(ApplicationDbContext db, TimKiemBangLuong timkiem)
        {
            try
            {
                string sFile = "TongHopTheoDoiHopDong.xlsx";

                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/" + sFile);

                var filenamegui = "Theodoihopdong.xlsx";
                string sFileNameCopy = PathUploads + filenamegui;
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                List<HRM_TheoDoiHopDong_NhanVien> listData = GetTongHopTheoDoiHopDong(db, timkiem);
                if (listData != null && listData.Count > 0)
                {
                    int nRow = 5;
                    int nCol = 1;
                    foreach (var item in listData)
                    {
                        nCol = 1;
                        sheet1.Cells[nRow, nCol].Value = item.CoSo;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.MaNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.TenNhanVien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.BoPhan;
                        nCol++;
                        if (item.NgayBatDauLamViec != null)
                            sheet1.Cells[nRow, nCol].Value = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThamNien;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.LoaiHopDong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.ThoiHan;
                        nCol++;
                        if (item.TuNgay != null)
                            sheet1.Cells[nRow, nCol].Value = item.TuNgay;
                        nCol++;
                        if (item.DenNgay != null)
                            sheet1.Cells[nRow, nCol].Value = item.DenNgay;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongVuViec.Luong;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongVuViec.CacLoaiPhuCap;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        nCol++;
                        if (item.HopDongVuViec.Ngay != null)
                            sheet1.Cells[nRow, nCol].Value = item.HopDongVuViec.Ngay.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongVuViec.LyDoDieuChinh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong12Thang.Luong;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;

                        nCol++;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong12Thang.CacLoaiPhuCap;
                        nCol++;
                        if (item.HopDong12Thang.Ngay != null)
                            sheet1.Cells[nRow, nCol].Value = item.HopDong12Thang.Ngay.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong12Thang.LyDoDieuChinh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong36Thang.Luong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong36Thang.CacLoaiPhuCap;
                        nCol++;
                        if (item.HopDong36Thang.Ngay != null)
                            sheet1.Cells[nRow, nCol].Value = item.HopDong36Thang.Ngay.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDong36Thang.LyDoDieuChinh;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongKhongThoiHan.Luong;
                        nCol++;
                        sheet1.Cells[nRow, nCol].Style.Numberformat.Format = dungchung.m_FormatSoLuong2;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongKhongThoiHan.CacLoaiPhuCap;
                        nCol++;
                        if (item.HopDongKhongThoiHan.Ngay != null)
                            sheet1.Cells[nRow, nCol].Value = item.HopDongKhongThoiHan.Ngay.Value.ToString("dd/MM/yyyy");
                        nCol++;
                        sheet1.Cells[nRow, nCol].Value = item.HopDongKhongThoiHan.LyDoDieuChinh;


                        nRow++;
                    }
                    DrawTableExcel(sheet1, 5, nRow, 1, nCol);
                    sheet1.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    //sheet1.Cells.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet1.Cells[5, 1, nRow, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells[5, 3, nRow, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet1.Cells.Style.Font.Name = "Times New Roman";
                    sheet1.Cells.Style.WrapText = true;

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
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra" + e.ToString());
            }
        }
        #region Báo cáo hợp đồng mới
        public dynamic ExportHopDongXacDinhThoiHan(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongXacDinhThoiHanNhanVien.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + ".docx";

            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            if (ChucVuNLD.MaChucVu == "CN")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongLaoDongICT.docx");
            //string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            File.Copy(fileName, sFileNameCopy, true);
            var listNoicap = db.HRM_dmNoiCap.ToList();
            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
            {
                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                var CalamViecs = listCaLamViec.Select(a => new
                {
                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                }).Distinct().ToList();


                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;

                // Tìm bookmark
                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                if (bookmarkStart != null)
                {
                    // Tạo một phần tử mới để chèn dữ liệu
                    var parentElement = bookmarkStart.Parent;

                    // Chèn danh sách dữ liệu vào vị trí bookmark
                    foreach (var item in CalamViecs)
                    {
                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                        Run run = new Run();
                        run.AppendChild(new Text(item.Ten));
                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                        run.AppendChild(new Text(item.MoTa));
                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                        paragraphProperties.Indentation = new Indentation()
                        {
                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                            Right = "720", // Lùi dòng phải 0.5 inch
                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                        };
                        para.Append(paragraphProperties);
                        para.AppendChild(run);
                        parentElement.InsertAfterSelf(para);
                    }


                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            if (QuocTichNDD == null)
                QuocTichNDD = new HRM_dmQuocTich();


            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);
            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
            doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
            if (CCCDNDD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCDNDD", "");
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            if (HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongXacDinhThoiHan.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongCongNhanThoiVu(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCongNhanThoiVu.docx");
            //string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            File.Copy(fileName, sFileNameCopy, true);
            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
            {
                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                var CalamViecs = listCaLamViec.Select(a => new
                {
                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                }).Distinct().ToList();


                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;

                // Tìm bookmark
                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                if (bookmarkStart != null)
                {
                    // Tạo một phần tử mới để chèn dữ liệu
                    var parentElement = bookmarkStart.Parent;

                    // Chèn danh sách dữ liệu vào vị trí bookmark
                    foreach (var item in CalamViecs)
                    {
                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                        Run run = new Run();
                        run.AppendChild(new Text(item.Ten));
                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                        run.AppendChild(new Text(item.MoTa));
                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                        paragraphProperties.Indentation = new Indentation()
                        {
                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                            Right = "720", // Lùi dòng phải 0.5 inch
                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                        };
                        para.Append(paragraphProperties);
                        para.AppendChild(run);
                        parentElement.InsertAfterSelf(para);
                    }


                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            if (QuocTichNDD == null)
                QuocTichNDD = new HRM_dmQuocTich();
            //var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);

            var doc = DocX.Load(sFileNameCopy);
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
            if (CCCDNDD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCDNDD", "");
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            if (SDT == null)
                SDT = "";
            doc.ReplaceText("@SoDienThoai", SDT ?? "");

            //var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            //doc.ReplaceText("@dmThoiHanHopDong", thoigianHopDong.Ten);
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            doc.ReplaceText("@NgayHieuLuc", ThoiHanHopDong);
            if (HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);


            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();

            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongCongNhanThoiVu.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

            return data;
        }
        public dynamic ExportHopDongLaoDongICT(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongKhongXacDinhThoiHanNhanVien.docx");
            if (Host == "PHT")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongLapDong.docx");
            //string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- HĐLĐ" + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();

            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
            if (dmLoaiHopDong == null)
            {
                dmLoaiHopDong = new HRM_dmLoaiHopDong();
            }
            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
            {
                ChucDanhNLD = new ChucDanh();
            }
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (QuocTichNDD == null)
                QuocTichNDD = new HRM_dmQuocTich();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();

            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);
            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
            {
                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                var CalamViecs = listCaLamViec.Select(a => new
                {
                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                }).Distinct().ToList();


                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;

                // Tìm bookmark
                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                if (bookmarkStart != null)
                {
                    // Tạo một phần tử mới để chèn dữ liệu
                    var parentElement = bookmarkStart.Parent;

                    // Chèn danh sách dữ liệu vào vị trí bookmark
                    foreach (var item in CalamViecs)
                    {
                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                        Run run = new Run();
                        run.AppendChild(new Text(item.Ten));
                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                        run.AppendChild(new Text(item.MoTa));
                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                        paragraphProperties.Indentation = new Indentation()
                        {
                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                            Right = "720", // Lùi dòng phải 0.5 inch
                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                        };
                        para.Append(paragraphProperties);
                        para.AppendChild(run);
                        parentElement.InsertAfterSelf(para);
                    }


                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }

            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
            doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            var dmCaLamViec = db.HRM_dmLichLamViec.FirstOrDefault(a => a.IdGuid == HopDong.IdGuid);
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDong.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongThuViec(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecNhanVien.docx");
            if (Host == "PHT")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongThuViec.docx");
            //string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- HĐTV" + ".docx";

            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            if (ChucVuNLD.MaChucVu == "GS")
            {
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecGiamSat.docx");
            }
            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien);
            if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
            if (dmLoaiHopDong == null)
            {
                dmLoaiHopDong = new HRM_dmLoaiHopDong();
            }

            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
                ChucDanhNLD = new ChucDanh();
            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);

            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
            doc.ReplaceText("@BHXH", NguoiLaoDong.SoBaoHiemXaHoi == null ? (HopDong.BaoHiem.SoSoBaoHiem ?? "") : NguoiLaoDong.SoBaoHiemXaHoi);
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "Từ ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
            doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem);
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            string HinhThucTraLuong = "";
            var HTTL = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
            if (HTTL != null)
                HinhThucTraLuong = HTTL.Ten + ", ";
            if (HopDong.LoaiLuong == 1)
                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo tháng";
            if (HopDong.LoaiLuong == 2)
                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo ngày";
            if (HopDong.LoaiLuong == 1)
                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo giờ";
            doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong);
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongThuViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongThanhLy(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThanhLy.docx");
            //string sFileNameCopy =PathUploads+ filenamegui + ".docx";
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);

            //var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);


            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);


            File.Copy(fileName, sFileNameCopy, true);

            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var doc = DocX.Load(sFileNameCopy);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");

            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));


            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.AddDays(-1).ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongHocViec(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThoaThuanHocViec.docx");
            if (Host == "PHT")
                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongHocViec.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- HĐHV" + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
                ChucDanhNLD = new ChucDanh();
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
            if (dmLoaiHopDong == null)
            {
                dmLoaiHopDong = new HRM_dmLoaiHopDong();
            }

            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);
            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
            {
                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                var CalamViecs = listCaLamViec.Select(a => new
                {
                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " /*+ a.SoNgayCongTieuChuan + " ngày/tháng."*/
                }).Distinct().ToList();


                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;

                // Tìm bookmark
                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                if (bookmarkStart != null)
                {
                    // Tạo một phần tử mới để chèn dữ liệu
                    var parentElement = bookmarkStart.Parent;

                    // Chèn danh sách dữ liệu vào vị trí bookmark
                    foreach (var item in CalamViecs)
                    {
                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                        Run run = new Run();
                        run.AppendChild(new Text(item.MoTa));
                        //run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                        //run.AppendChild(new Text(item.MoTa));
                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                        paragraphProperties.Indentation = new Indentation()
                        {
                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                            Right = "720", // Lùi dòng phải 0.5 inch
                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                        };
                        para.Append(paragraphProperties);
                        para.AppendChild(run);
                        parentElement.InsertAfterSelf(para);
                    }


                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }

            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
            doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem);
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongHocViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongKhoan(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongKhoanViec.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- HĐK" + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();

            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
                ChucDanhNLD = new ChucDanh();
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
            if (dmLoaiHopDong == null)
                dmLoaiHopDong = new HRM_dmLoaiHopDong();

            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);

            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
            {
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
            }
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongHocViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongVuViec(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongVuViec.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- HĐVV" + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();
            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
                ChucDanhNLD = new ChucDanh();
            var dmLoaiHopDong = db.HRM_dmLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
            if (dmLoaiHopDong == null)
            {
                dmLoaiHopDong = new HRM_dmLoaiHopDong();
            }

            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);
            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
            {
                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                var CalamViecs = listCaLamViec.Select(a => new
                {
                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " /*+ a.SoNgayCongTieuChuan + " ngày/tháng."*/
                }).Distinct().ToList();


                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;

                // Tìm bookmark
                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                if (bookmarkStart != null)
                {
                    // Tạo một phần tử mới để chèn dữ liệu
                    var parentElement = bookmarkStart.Parent;

                    // Chèn danh sách dữ liệu vào vị trí bookmark
                    foreach (var item in CalamViecs)
                    {
                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                        Run run = new Run();
                        run.AppendChild(new Text(item.MoTa));
                        //run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                        //run.AppendChild(new Text(item.MoTa));
                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                        paragraphProperties.Indentation = new Indentation()
                        {
                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                            Right = "720", // Lùi dòng phải 0.5 inch
                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                        };
                        para.Append(paragraphProperties);
                        para.AppendChild(run);
                        parentElement.InsertAfterSelf(para);
                    }


                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }

            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@BHXH", NguoiLaoDong.SoBaoHiemXaHoi == null ? (HopDong.BaoHiem.SoSoBaoHiem ?? "") : NguoiLaoDong.SoBaoHiemXaHoi);
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
            doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem);
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongVuViec.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }

        public dynamic ExportHopDongHopTac(string IdHopDong, ApplicationDbContext db)
        {
            var filenamegui = Guid.NewGuid().ToString();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCTV.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + ".docx";
            var listNoicap = db.HRM_dmNoiCap.ToList();
            DateTime dateTimeNow = DateTime.Now;
            var lstQuocTich = db.HRM_dmQuocTich.ToList();

            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);
            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien);
            if (NguoiDaiDien == null)
                NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            if (QuocTichNDD == null)
                QuocTichNDD = new HRM_dmQuocTich();
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
            if (ChucDanhNLD == null)
                ChucDanhNLD = new ChucDanh();
            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            //HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);


            File.Copy(fileName, sFileNameCopy, true);
            var doc = DocX.Load(sFileNameCopy);


            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (CCCDNDD != null)
            {
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                if (CCCDNDD.NgayCap != null)
                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                else
                    doc.ReplaceText("@NgayCapCCCDNDD", "");
            }
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
            if (ChucVuNDaiDien != null)
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();

            doc.ReplaceText("@SoDienThoai", SDT ?? "");

            //var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            //doc.ReplaceText("@dmThoiHanHopDong", thoigianHopDong.Ten);
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayKy", NgayKy);
            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();


            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

            doc.Save();
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }

        public void InsetCol(DocumentFormat.OpenXml.Wordprocessing.TableRow tr, double Width, int Size, string text, JustificationValues Center, bool Bold = false, string font = "Times New Roman")
        {
            DocumentFormat.OpenXml.Wordprocessing.TableCell tc = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
            if (Bold == false)
                tc.Append(new Paragraph(new ParagraphProperties(new Justification { Val = Center }, new TableCellWidth { Width = Width.ToString(), Type = TableWidthUnitValues.Dxa }), new Run(new RunProperties(new RunFonts() { Ascii = font, HighAnsi = font }, new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = (Size * 2).ToString() }), new Text($"" + text + ""))));
            else
                tc.Append(new Paragraph(new ParagraphProperties(new Justification { Val = Center }, new TableCellWidth { Width = Width.ToString(), Type = TableWidthUnitValues.Dxa }), new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = font, HighAnsi = font }, new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = (Size * 2).ToString() }), new Text($"" + text + ""))));

            tr.Append(tc);
        }
        public dynamic ExportPhuLucHopDongICT(string IdHopDong, ApplicationDbContext db)
        {
            DateTime dateTimeNow = DateTime.Now;
            var filenamegui = dateTimeNow.ToOADate();
            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/PhuLucHopDongICT.docx");
            HRM_HopDong HopDong = db.HRM_HopDong.Find(IdHopDong);
            var listDuAn = _CallWebService.GetDanhSachDuAn(db);
            var NguoiLaoDong = db.HRM_NhanVien.Find(HopDong.IdNhanVien);
            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + "- PLHĐ" + ".docx";

            var listNoicap = db.HRM_dmNoiCap.ToList();
            var lstQuocTich = db.HRM_dmQuocTich.ToList();

            var CongTy = _CallWebService.GetBoPhan(db, HopDong.IdCongTyKyHopDong ?? 0);

            var NguoiDaiDien = db.HRM_NhanVien.Find(HopDong.IdNguoiDaiDien); if (NguoiDaiDien == null)
                if (NguoiDaiDien == null)
                    NguoiDaiDien = new HRM_NhanVien();
            var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
            var CCCDNDD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNDD == null)
                CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                if (NoiCap != null)
                    CCCDNDD.TenNoiCap = NoiCap.Ten;
            }
            var QuocTichNDD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
            if (QuocTichNDD == null)
                QuocTichNDD = new HRM_dmQuocTich();

            var ChucVuNLD = _CallWebService.GetChucVu(db, HopDong.IddmChucVu);
            var CCCDNLD = db.HRM_NhanVien_GiayToTuyThan.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();
            if (CCCDNLD == null)
                CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
            else
            {
                var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                if (NoiCap != null)
                    CCCDNLD.TenNoiCap = NoiCap.Ten;
            }
            var BoPhan = _CallWebService.GetBoPhan(db, HopDong.IdBoPhanEOS);
            var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
            if (QuocTichNLD == null)
                QuocTichNLD = new HRM_dmQuocTich();
            var TrinhDo = db.HRM_dmTrinhDoDaoTao.Find(NguoiLaoDong.IddmTrinhDo);
            HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);
            if (HopDongCha == null)
                HopDongCha = new HRM_HopDong();
            var LuongCha = db.HRM_Luong_HopDong.FirstOrDefault(a => a.IdHopDong == HopDongCha.IdGuid);
            if (LuongCha == null)
                LuongCha = new HRM_Luong_HopDong();
            var listDuAn_HD = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == HopDong.IdGuid || a.IdQuyTrinh == HopDong.IdHopDong).ToList();
            var listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdGuid).Select(a => a.IdBoPhan).ToList();
            var listDuAnMoi = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
            listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdHopDong).Select(a => a.IdBoPhan).ToList();
            var listDuAnCu = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
            var ChucVuNLDCu = _CallWebService.GetChucVu(db, HopDongCha.IddmChucVu);
            string LuongCu = LuongCha.LuongBaoHiem.ToString("N0");
            var dmThoiHanHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
            File.Copy(fileName, sFileNameCopy, true);
            HRM_Luong_HopDong LuonghopDong = db.HRM_Luong_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();

            string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            {
                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                Body body = wordDocument.MainDocumentPart.Document.Body;
                BookmarkStart bookmarkTable = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "NoiDungThayDoi");
                if (bookmarkTable != null)
                {
                    // Tạo bảng mới
                    DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                    // Định nghĩa các thuộc tính của bảng
                    TableProperties tblProperties = new TableProperties(
                    new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                    ),
                    new TableJustification { Val = TableRowAlignmentValues.Center } // Căn giữa bảng
                    , new RunFonts() { Ascii = "Times New Roman" },
                     new DocumentFormat.OpenXml.Wordprocessing.TableStyle() { Val = "TableGrid" }
                    );

                    table.AppendChild(tblProperties);


                    { // header
                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        InsetCol(tr, 100, 12, "STT", JustificationValues.Center, true);
                        InsetCol(tr, 2000, 12, "Nội dung", JustificationValues.Center, true);
                        InsetCol(tr, 3500, 12, "Thông tin cũ", JustificationValues.Center, true);
                        InsetCol(tr, 3500, 12, "Thông tin mới", JustificationValues.Center, true);
                        table.Append(tr);
                    }
                    int STT = 1;
                    if (listDuAnCu != listDuAnMoi)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                          new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                        InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                        InsetCol(tr, 2000, 12, "Đơn vị quản lý", JustificationValues.Left);
                        InsetCol(tr, 3500, 12, string.Join(",", listDuAnCu), JustificationValues.Left);
                        InsetCol(tr, 3500, 12, string.Join(",", listDuAnMoi), JustificationValues.Left);
                        table.Append(tr);
                        STT++;
                    }
                    if (ChucVuNLD.TenChucVu != ChucVuNLDCu.TenChucVu)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                          new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                        InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                        InsetCol(tr, 2000, 12, "Chức vụ", JustificationValues.Left);
                        InsetCol(tr, 3500, 12, ChucVuNLDCu.TenChucVu, JustificationValues.Left);
                        InsetCol(tr, 3500, 12, ChucVuNLD.TenChucVu, JustificationValues.Left);
                        table.Append(tr);
                        STT++;
                    }
                    if (Luong != LuongCu)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                          new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                        InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                        InsetCol(tr, 2000, 12, "Mức lương", JustificationValues.Left);
                        InsetCol(tr, 3500, 12, LuongCu, JustificationValues.Left);
                        InsetCol(tr, 3500, 12, Luong, JustificationValues.Left);
                        table.Append(tr);
                        STT++;
                    }
                    if (HopDong.DiaDiemLamViec != HopDongCha.DiaDiemLamViec)
                    {

                        DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                          new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                        InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                        InsetCol(tr, 2000, 12, "Địa điểm", JustificationValues.Left);
                        InsetCol(tr, 3500, 12, HopDongCha.DiaDiemLamViec, JustificationValues.Left);
                        InsetCol(tr, 3500, 12, HopDong.DiaDiemLamViec, JustificationValues.Left);
                        table.Append(tr);
                        STT++;
                    }
                    // Thêm bảng vào tài liệu
                    bookmarkTable.Parent.InsertAfterSelf(table);

                }
                wordDocument.MainDocumentPart.Document.Save();
                wordDocument.Dispose();

            }

            var doc = DocX.Load(sFileNameCopy);
            string NgayKy = HopDongCha.NgayKy.Day + " tháng " + HopDongCha.NgayKy.Month + " năm " + HopDongCha.NgayKy.Year;
            doc.ReplaceText("@HopDongCuNgayKy", NgayKy);
            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
            doc.ReplaceText("@HopDongCu", HopDongCha.SoHopDong);
            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
            doc.ReplaceText("@NgayThang", NgayThang);
            //Nguoi dai diẹn
            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "");
            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
            if (!string.IsNullOrEmpty(CCCDNDD.So))
                doc.ReplaceText("@CCCDNDD", CCCDNDD.So);
            else
                doc.ReplaceText("@CCCDNDD", "");
            if (!string.IsNullOrEmpty(CCCDNDD.TenNoiCap))
                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap);
            else
                doc.ReplaceText("@NoiCapCCCDNDD", "");
            if (CCCDNDD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCDNDD", "");
            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "");
            if (ChucVuNDaiDien != null && !string.IsNullOrEmpty(ChucVuNDaiDien.TenChucVu))
                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu);
            else
                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
            if (!string.IsNullOrEmpty(CongTy.DiaChi))
                doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
            else
                doc.ReplaceText("@DiaChiCongTy", "");
            if (!string.IsNullOrEmpty(CongTy.DienThoai))
                doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai);
            else
                doc.ReplaceText("@SoDienThoaiCongTy", "");
            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

            // Nguoi lao dong
            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu ?? "");
            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
            if (NguoiLaoDong.NgaySinh != null)
                doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgaySinh", "");
            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
            if (CCCDNLD.NgayCap != null)
                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
            else
                doc.ReplaceText("@NgayCapCCCD", "");
            var SDT = db.HRM_NhanVien_SoDienThoai.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
            doc.ReplaceText("@SoDienThoai", SDT ?? "");

            //var thoigianHopDong = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.IdGuid == HopDong.IdThoiHanHopDong);
            //doc.ReplaceText("@dmThoiHanHopDong", thoigianHopDong.Ten);
            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;

            if (HopDong.NgayHetHan != null && dmThoiHanHopDong.Ma != "KTH")

                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);

            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
            doc.ReplaceText("@NhiemVu", "");
            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
            if (Host == "PHT")
            {
                HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                //PhanTram = LuongBH.TyLeHuongLuong.ToString();
            }
            doc.ReplaceText("@LuongChu", LuongChu);
            doc.ReplaceText("@Luong", Luong);
            doc.ReplaceText("@PhanTramLuong ", PhanTram);
            doc.Save();
            //foreach (var paragraph in doc.Paragraphs)
            //{
            //    paragraph.Font("Times New Roman");

            //}
            dynamic data = new DynamicObject();
            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("PhuLucHopDong.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);
            return data;
        }
        public dynamic ExportHopDongHieuLuc(/*EntityGetDanhSachHopDongRequest aRequest*/ List<string> listIdHopDong, ApplicationDbContext db)
        {
            try
            {
                dynamic data = new DynamicObject();

                List<string> listfile = new List<string>();
                #region
                var listDanhSachHopDong = (from a in db.HRM_HopDong

                                           where listIdHopDong.Contains(a.IdGuid)
                                           select a).AsEnumerable();

                listIdHopDong = listDanhSachHopDong.Select(a => a.IdHopDong).Distinct().ToList();
                var listHDGoc = (from a in db.HRM_HopDong
                                 where listIdHopDong.Contains(a.IdGuid)
                                 select a).AsEnumerable();
                var listLuongNhanVien = db.HRM_Luong_HopDong.Where(r => listIdHopDong.Contains(r.IdHopDong)).ToList();
                var listLuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => listIdHopDong.Contains(r.IdHopDong)).ToList();
                List<string> listIdNhanVien = listDanhSachHopDong.Select(a => a.IdNhanVien).Distinct().ToList();
                listIdNhanVien.AddRange(listDanhSachHopDong.Select(a => a.IdNguoiDaiDien).Distinct().ToList());
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var lstQuocTich = db.HRM_dmQuocTich.ToList();
                var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                var listTrinhDoDaoTao = db.HRM_dmTrinhDoDaoTao.ToList();
                var CongTys = _CallWebService.GetCongTyKyHopDong(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).ToList();
                var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
                var listThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
                var listNoicap = db.HRM_dmNoiCap.ToList();
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);

                DateTime dateTimeNow = DateTime.Now;
                #endregion
                foreach (var HopDong in listDanhSachHopDong)
                {
                    HopDong.listIdCaLamViec = new List<string>();

                    var LoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                    var filenamegui = "_" + LoaiHopDong.Ma;
                    var NguoiDaiDien = listNhanVien.FirstOrDefault(a => a.IdGuid == HopDong.IdNguoiDaiDien);
                    if (NguoiDaiDien == null)
                        NguoiDaiDien = new HRM_NhanVien();
                    var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
                    var NguoiLaoDong = listNhanVien.FirstOrDefault(a => a.IdGuid == HopDong.IdNhanVien);
                    var CCCDNDD = listGiayTo.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).FirstOrDefault();
                    var CCCDNLD = listGiayTo.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();

                    var CongTy = CongTys.FirstOrDefault(a => a.Id == HopDong.IdCongTyKyHopDong);
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == HopDong.IdBoPhanEOS);
                    var TrinhDo = listTrinhDoDaoTao.FirstOrDefault(a => a.IdGuid == NguoiLaoDong.IddmTrinhDo);
                    var SDT = listSDT.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
                    var QuocTichNDD = new HRM_dmQuocTich();
                    if (NguoiDaiDien != null)
                        lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
                    HRM_Luong_HopDong LuonghopDong = listLuongNhanVien.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                    HRM_LuongBaoHiem_HopDong LuongBH = listLuongBH.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                    var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
                    var dmThoiHanHopDong = listThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
                    if (CCCDNDD == null)
                        CCCDNDD = new HRM_NhanVien_GiayToTuyThan();
                    else
                    {
                        var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                        if (NoiCap != null)
                            CCCDNDD.TenNoiCap = NoiCap.Ten;
                    }
                    if (QuocTichNDD == null)
                        QuocTichNDD = new HRM_dmQuocTich();
                    if (QuocTichNLD == null)
                        QuocTichNLD = new HRM_dmQuocTich();
                    var ChucVuNLD = listChucVu.FirstOrDefault(a => a.Id == HopDong.IddmChucVu);
                    if (ChucVuNLD == null)
                        ChucVuNLD = new ChucVu();
                    if (CCCDNLD == null)
                        CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
                    else
                    {
                        var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                        if (NoiCap != null)
                            CCCDNLD.TenNoiCap = NoiCap.Ten;
                    }
                    if (BoPhan == null)
                        BoPhan = new BoPhan();
                    var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh);
                    if (ChucDanhNLD == null)
                        ChucDanhNLD = new ChucDanh();
                    HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);
                    if (HopDongCha == null)
                        HopDongCha = new HRM_HopDong();
                    var LuongCha = db.HRM_Luong_HopDong.FirstOrDefault(a => a.IdHopDong == HopDongCha.IdGuid);
                    if (LuongCha == null)
                        LuongCha = new HRM_Luong_HopDong();
                    var listDuAn_HD = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == HopDong.IdGuid || a.IdQuyTrinh == HopDong.IdHopDong).ToList();
                    var listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdGuid).Select(a => a.IdBoPhan).ToList();
                    var listDuAnMoi = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
                    listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdHopDong).Select(a => a.IdBoPhan).ToList();
                    var listDuAnCu = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
                    var ChucVuNLDCu = _CallWebService.GetChucVu(db, HopDongCha.IddmChucVu);
                    string LuongCu = LuongCha.LuongBaoHiem.ToString("N0");

                    string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");


                    if (!string.IsNullOrEmpty(HopDong.IdHopDong) || HopDong.isPhuLuc == true)

                    {
                        string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/PhuLucHopDongICT.docx");

                        string sFileNameCopy = PathUploads + @"\PhuLuc_" + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                        File.Copy(fileName, sFileNameCopy, true);
                        {
                            WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                            Body body = wordDocument.MainDocumentPart.Document.Body;
                            BookmarkStart bookmarkTable = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "NoiDungThayDoi");
                            if (bookmarkTable != null)
                            {
                                // Tạo bảng mới
                                DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                                // Định nghĩa các thuộc tính của bảng
                                TableProperties tblProperties = new TableProperties(
                                new TableBorders(
                                new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                                new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                                ),
                                new TableJustification { Val = TableRowAlignmentValues.Center } // Căn giữa bảng
                                , new RunFonts() { Ascii = "Times New Roman" });
                                table.AppendChild(tblProperties);

                                { // header
                                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                                    InsetCol(tr, 100, 12, "STT", JustificationValues.Center, true);
                                    InsetCol(tr, 2000, 12, "Nội dung", JustificationValues.Center, true);
                                    InsetCol(tr, 3500, 12, "Thông tin cũ", JustificationValues.Center, true);
                                    InsetCol(tr, 3500, 12, "Thông tin mới", JustificationValues.Center, true);
                                    table.Append(tr);
                                }
                                int STT = 1;
                                if (listDuAnCu != listDuAnMoi)
                                {
                                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                      new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                    InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                    InsetCol(tr, 2000, 12, "Đơn vị quản lý", JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, string.Join(",", listDuAnCu), JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, string.Join(",", listDuAnMoi), JustificationValues.Left);
                                    table.Append(tr);
                                    STT++;
                                }
                                if (ChucVuNLD.TenChucVu != ChucVuNLDCu.TenChucVu)
                                {
                                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                      new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                    InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                    InsetCol(tr, 2000, 12, "Chức vụ", JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, ChucVuNLDCu.TenChucVu, JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, ChucVuNLD.TenChucVu, JustificationValues.Left);
                                    table.Append(tr);
                                    STT++;
                                }
                                if (Luong != LuongCu)
                                {
                                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                      new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                    InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                    InsetCol(tr, 2000, 12, "Mức lương", JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, LuongCu, JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, Luong, JustificationValues.Left);
                                    table.Append(tr);
                                    STT++;
                                }
                                if (HopDong.DiaDiemLamViec != HopDongCha.DiaDiemLamViec)
                                {
                                    DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                      new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                    InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                    InsetCol(tr, 2000, 12, "Địa điểm", JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, HopDongCha.DiaDiemLamViec, JustificationValues.Left);
                                    InsetCol(tr, 3500, 12, HopDong.DiaDiemLamViec, JustificationValues.Left);
                                    table.Append(tr);
                                    STT++;
                                }
                                // Thêm bảng vào tài liệu
                                bookmarkTable.Parent.InsertAfterSelf(table);

                            }
                            wordDocument.MainDocumentPart.Document.Save();
                            wordDocument.Dispose();

                        }
                        var doc = DocX.Load(sFileNameCopy);
                        string NgayKy = HopDongCha.NgayKy.Day + " tháng " + HopDongCha.NgayKy.Month + " năm " + HopDongCha.NgayKy.Year;
                        doc.ReplaceText("@HopDongCuNgayKy", NgayKy);
                        doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
                        doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                        doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
                        doc.ReplaceText("@HopDongCu", HopDongCha.SoHopDong);
                        string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                        doc.ReplaceText("@NgayThang", NgayThang);
                        //Nguoi dai diẹn
                        doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "");
                        doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                        doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "");
                        if (!string.IsNullOrEmpty(CCCDNDD.So))
                            doc.ReplaceText("@CCCDNDD", CCCDNDD.So);
                        else
                            doc.ReplaceText("@CCCDNDD", "");
                        if (!string.IsNullOrEmpty(CCCDNDD.TenNoiCap))
                            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap);
                        else
                            doc.ReplaceText("@NoiCapCCCDNDD", "");
                        if (CCCDNDD.NgayCap != null)
                            doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                        else
                            doc.ReplaceText("@NgayCapCCCDNDD", "");
                        doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "");
                        if (ChucVuNDaiDien != null && !string.IsNullOrEmpty(ChucVuNDaiDien.TenChucVu))
                            doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu);
                        else
                            doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                        doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                        if (!string.IsNullOrEmpty(CongTy.DiaChi))
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
                        else
                            doc.ReplaceText("@DiaChiCongTy", "");
                        if (!string.IsNullOrEmpty(CongTy.DienThoai))
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai);
                        else
                            doc.ReplaceText("@SoDienThoaiCongTy", "");
                        doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                        doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                        // Nguoi lao dong
                        doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu ?? "");
                        doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                        doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                        doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                        if (NguoiLaoDong.NgaySinh != null)
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                        else
                            doc.ReplaceText("@NgaySinh", "");
                        doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                        doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                        doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                        if (CCCDNLD.NgayCap != null)
                            doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                        else
                            doc.ReplaceText("@NgayCapCCCD", "");
                        doc.ReplaceText("@SoDienThoai", SDT ?? "");
                        string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                        //string NgayKy = " " + HopDongCha.NgayKy.Day + " tháng " + HopDongCha.NgayKy.Month + " năm " + HopDongCha.NgayKy.Year;
                        //doc.ReplaceText("@HopDongCuNgayKy", NgayKy);
                        if (HopDong.NgayHetHan != null && dmThoiHanHopDong.Ma != "KTH")
                            ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                        doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);

                        doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                        doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                        doc.ReplaceText("@NhiemVu", "");
                        doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                        doc.Save();
                        listfile.Add(sFileNameCopy);
                        data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("PhuLucHopDongICT.docx")
                               + "&path=" + Base64Encode(sFileNameCopy);
                    }
                    else
                    {
                        if (LoaiHopDong.Ma == "TTHV" || LoaiHopDong.Ma == "HĐHV")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThoaThuanHocViec.docx");
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongHocViec.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";



                            File.Copy(fileName, sFileNameCopy, true);
                            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                            {
                                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).AsNoTracking().ToList();
                                var CalamViecs = listCaLamViec.Select(a => new
                                {
                                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " /*+ a.SoNgayCongTieuChuan + " ngày/tháng."*/
                                }).Distinct().ToList();


                                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                                Body body = wordDocument.MainDocumentPart.Document.Body;

                                // Tìm bookmark
                                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                                if (bookmarkStart != null)
                                {
                                    // Tạo một phần tử mới để chèn dữ liệu
                                    var parentElement = bookmarkStart.Parent;

                                    // Chèn danh sách dữ liệu vào vị trí bookmark
                                    foreach (var item in CalamViecs)
                                    {
                                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                                        Run run = new Run();
                                        run.AppendChild(new Text(item.MoTa));
                                        //run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                                        //run.AppendChild(new Text(item.MoTa));
                                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                                        paragraphProperties.Indentation = new Indentation()
                                        {
                                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                                            Right = "720", // Lùi dòng phải 0.5 inch
                                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                                        };
                                        para.Append(paragraphProperties);
                                        para.AppendChild(run);
                                        parentElement.InsertAfterSelf(para);
                                    }


                                }
                                wordDocument.MainDocumentPart.Document.Save();
                                wordDocument.Dispose();

                            }

                            var doc = DocX.Load(sFileNameCopy);


                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");

                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
                            doc.ReplaceText("@TenLoaiHopDong", LoaiHopDong.Ten);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");

                            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                            if (Host == "PHT")
                            {
                                //HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                                PhanTram = LuongBH.TyLeHuongLuong.ToString();
                            }
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.ReplaceText("@PhanTramLuong ", PhanTram);
                            doc.ReplaceText("@NgayKyPhuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
                            doc.Save();

                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }
                        else if (LoaiHopDong.Ma == "HĐTV")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecNhanVien.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                            if (ChucVuNLD.MaChucVu == "GS")
                            {
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecGiamSat.docx");

                            }
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongThuViec.docx");
                            File.Copy(fileName, sFileNameCopy, true);
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@BHXH", NguoiLaoDong.SoBaoHiemXaHoi == null ? (HopDong.BaoHiem.SoSoBaoHiem ?? "") : NguoiLaoDong.SoBaoHiemXaHoi);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");

                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
                            doc.ReplaceText("@TenLoaiHopDong", LoaiHopDong.Ten);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");

                            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                            if (Host == "PHT")
                            {
                                //HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                                PhanTram = LuongBH.TyLeHuongLuong.ToString();
                            }
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.ReplaceText("@PhanTramLuong ", PhanTram);
                            string HinhThucTraLuong = "";
                            var HTTL = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
                            if (HTTL != null)
                                HinhThucTraLuong = HTTL.Ten + ", ";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo tháng";
                            if (HopDong.LoaiLuong == 2)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo ngày";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo giờ";
                            doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong);
                            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

                            doc.Save();

                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }
                        else if (LoaiHopDong.Ma == "HĐVV")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongVuViec.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";

                            File.Copy(fileName, sFileNameCopy, true);
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");

                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
                            doc.ReplaceText("@TenLoaiHopDong", LoaiHopDong.Ten);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");

                            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                            if (Host == "PHT")
                            {
                                //HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                                PhanTram = LuongBH.TyLeHuongLuong.ToString();
                            }
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.ReplaceText("@PhanTramLuong ", PhanTram);
                            string HinhThucTraLuong = "";
                            var HTTL = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
                            if (HTTL != null)
                                HinhThucTraLuong = HTTL.Ten + ", ";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo tháng";
                            if (HopDong.LoaiLuong == 2)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo ngày";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo giờ";
                            doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong);
                            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

                            doc.Save();

                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }
                        else if (LoaiHopDong.Ma == "HĐKHOAN")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongKhoanViec.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";

                            File.Copy(fileName, sFileNameCopy, true);
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");

                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
                            doc.ReplaceText("@TenLoaiHopDong", LoaiHopDong.Ten);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");

                            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                            if (Host == "PHT")
                            {
                                //HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                                PhanTram = LuongBH.TyLeHuongLuong.ToString();
                            }
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.ReplaceText("@PhanTramLuong ", PhanTram);
                            string HinhThucTraLuong = "";
                            var HTTL = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
                            if (HTTL != null)
                                HinhThucTraLuong = HTTL.Ten + ", ";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo tháng";
                            if (HopDong.LoaiLuong == 2)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo ngày";
                            if (HopDong.LoaiLuong == 1)
                                HinhThucTraLuong = HinhThucTraLuong + "trả lương theo giờ";
                            doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong);
                            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

                            doc.Save();

                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }

                        else if (LoaiHopDong.Ma == "BBTL")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThanhLy.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                            File.Copy(fileName, sFileNameCopy, true);

                            var doc = DocX.Load(sFileNameCopy);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");

                            // Nguoi lao dong
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));


                            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.AddDays(-1).ToString("dd/MM/yyyy"));
                            doc.Save();
                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongThanhLy.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }
                        else if (LoaiHopDong.Ma == "TTHT")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCTV.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                            File.Copy(fileName, sFileNameCopy, true);
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");
                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
                            doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
                            doc.Save();
                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                                            + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }

                        else if (HopDong.LoaiLuong != 1)
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCongNhanThoiVu.docx");

                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                            File.Copy(fileName, sFileNameCopy, true);
                            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                            {
                                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                                var CalamViecs = listCaLamViec.Select(a => new
                                {
                                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                                }).Distinct().ToList();


                                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                                Body body = wordDocument.MainDocumentPart.Document.Body;

                                // Tìm bookmark
                                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                                if (bookmarkStart != null)
                                {
                                    // Tạo một phần tử mới để chèn dữ liệu
                                    var parentElement = bookmarkStart.Parent;

                                    // Chèn danh sách dữ liệu vào vị trí bookmark
                                    foreach (var item in CalamViecs)
                                    {
                                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                                        Run run = new Run();
                                        run.AppendChild(new Text(item.Ten));
                                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                                        run.AppendChild(new Text(item.MoTa));
                                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                                        paragraphProperties.Indentation = new Indentation()
                                        {
                                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                                            Right = "720", // Lùi dòng phải 0.5 inch
                                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                                        };
                                        para.Append(paragraphProperties);
                                        para.AppendChild(run);
                                        parentElement.InsertAfterSelf(para);
                                    }


                                }
                                wordDocument.MainDocumentPart.Document.Save();
                                wordDocument.Dispose();

                            }
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                            if (CCCDNDD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCDNDD", "");
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");
                            if (SDT == null)
                                SDT = "";
                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            doc.ReplaceText("@NgayHieuLuc", ThoiHanHopDong);
                            if (HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);

                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            //string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                            if (Host == "PHT")
                            {
                                //HRM_LuongBaoHiem_HopDong LuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                                Luong = LuongBH.LuongBaoHiem.ToString("N0");
                                LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + " đồng";
                                PhanTram = LuongBH.TyLeHuongLuong.ToString();
                            }
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.ReplaceText("@PhanTramLuong ", PhanTram);

                            doc.Save();
                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongCongNhanThoiVu.docx")
                                 + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);

                        }
                        else if ((Host == "ICT" && LoaiHopDong.Ma == "HĐLĐ") || LoaiHopDong.Ma == "THHĐ")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongXacDinhThoiHanNhanVien.docx");
                            if (ChucVuNLD.MaChucVu == "CN")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongLaoDongICT.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                            File.Copy(fileName, sFileNameCopy, true);
                            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                            {
                                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                                var CalamViecs = listCaLamViec.Select(a => new
                                {
                                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                                }).Distinct().ToList();


                                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                                Body body = wordDocument.MainDocumentPart.Document.Body;

                                // Tìm bookmark
                                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                                if (bookmarkStart != null)
                                {
                                    // Tạo một phần tử mới để chèn dữ liệu
                                    var parentElement = bookmarkStart.Parent;

                                    // Chèn danh sách dữ liệu vào vị trí bookmark
                                    foreach (var item in CalamViecs)
                                    {
                                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                                        Run run = new Run();
                                        run.AppendChild(new Text(item.Ten));
                                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                                        run.AppendChild(new Text(item.MoTa));
                                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                                        paragraphProperties.Indentation = new Indentation()
                                        {
                                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                                            Right = "720", // Lùi dòng phải 0.5 inch
                                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                                        };
                                        para.Append(paragraphProperties);
                                        para.AppendChild(run);
                                        parentElement.InsertAfterSelf(para);
                                    }


                                }
                                wordDocument.MainDocumentPart.Document.Save();
                                wordDocument.Dispose();

                            }
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                            if (CCCDNDD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCDNDD", "");
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");
                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            if (HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.Save();
                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongXacDinhThoiHan.docx")
                             + "&path=" + Base64Encode(sFileNameCopy);
                            listfile.Add(sFileNameCopy);
                        }

                        else if ((Host == "PHT" && LoaiHopDong.Ma == "HĐLĐ") || LoaiHopDong.Ma == "HĐLĐ." || LoaiHopDong.Ma == "HĐLĐ.KXDTH" || LoaiHopDong.Ma == "KTHHĐ")
                        {
                            string fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongKhongXacDinhThoiHanNhanVien.docx");
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongLapDong.docx");
                            string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";

                            File.Copy(fileName, sFileNameCopy, true);
                            if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                            {
                                HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                                var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).Distinct().ToList();
                                var CalamViecs = listCaLamViec.Select(a => new
                                {
                                    Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                                    MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " + a.SoNgayCongTieuChuan + " ngày/tháng."
                                }).Distinct().ToList();


                                WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                                Body body = wordDocument.MainDocumentPart.Document.Body;

                                // Tìm bookmark
                                BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                                if (bookmarkStart != null)
                                {
                                    // Tạo một phần tử mới để chèn dữ liệu
                                    var parentElement = bookmarkStart.Parent;

                                    // Chèn danh sách dữ liệu vào vị trí bookmark
                                    foreach (var item in CalamViecs)
                                    {
                                        Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                                        Run run = new Run();
                                        run.AppendChild(new Text(item.Ten));
                                        run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                                        run.AppendChild(new Text(item.MoTa));
                                        ParagraphProperties paragraphProperties = new ParagraphProperties();
                                        paragraphProperties.Indentation = new Indentation()
                                        {
                                            Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                                            Right = "720", // Lùi dòng phải 0.5 inch
                                            FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                                        };
                                        para.Append(paragraphProperties);
                                        para.AppendChild(run);
                                        parentElement.InsertAfterSelf(para);
                                    }


                                }
                                wordDocument.MainDocumentPart.Document.Save();
                                wordDocument.Dispose();

                            }
                            var doc = DocX.Load(sFileNameCopy);
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@SoHopDong", HopDong.SoHopDong ?? "");
                            string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayThang", NgayThang);
                            //Nguoi dai diẹn
                            doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "" ?? "");
                            doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "" ?? "");
                            if (CCCDNDD != null)
                            {
                                doc.ReplaceText("@CCCDNDD", CCCDNDD.So ?? "");
                                doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap ?? "");
                                if (CCCDNDD.NgayCap != null)
                                    doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                                else
                                    doc.ReplaceText("@NgayCapCCCDNDD", "");
                            }
                            doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "" ?? "");
                            if (ChucVuNDaiDien != null)
                                doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu ?? "");
                            else
                                doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                            doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan ?? "");
                            doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai ?? "");
                            doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                            doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                            // Nguoi lao dong
                            doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                            doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu);
                            doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                            doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                            doc.ReplaceText("@NgaySinh", NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                            doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                            doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                            doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                            if (CCCDNLD.NgayCap != null)
                                doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                            else
                                doc.ReplaceText("@NgayCapCCCD", "");
                            doc.ReplaceText("@SoDienThoai", SDT ?? "");
                            string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                            string NgayKy = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                            doc.ReplaceText("@NgayKy", NgayKy);
                            if (dmThoiHanHopDong != null && dmThoiHanHopDong.Ma != "KTH" && HopDong.NgayHetHan != null)
                                ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                            doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong);
                            doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten);
                            doc.ReplaceText("@TenLoaiHopDong", LoaiHopDong.Ten);
                            doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                            doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                            doc.ReplaceText("@NhiemVu", "");
                            doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");

                            string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem) + " đồng";
                            doc.ReplaceText("@LuongChu", LuongChu);
                            doc.ReplaceText("@Luong", Luong);
                            doc.Save();
                            listfile.Add(sFileNameCopy);
                            data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongLaoDongICT.docx")
                            + "&path=" + Base64Encode(sFileNameCopy);

                        }
                    }

                }

                if (listfile != null && listfile.Count > 1)
                {
                    string sFileNameZip = PathUploads + "HopDongHieuLuc.zip";
                    using (var zipStream = new FileStream(sFileNameZip, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                        {
                            foreach (var filePath in listfile)
                            {
                                var zipEntry = archive.CreateEntry(Path.GetFileName(filePath));
                                using (var entryStream = zipEntry.Open())
                                {
                                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }
                    }
                    data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongHieuLuc.Zip")
                                        + "&path=" + Base64Encode(sFileNameZip);
                }
                return data;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic ExportHopDongHieuLucNew(/*EntityGetDanhSachHopDongRequest aRequest*/ List<string> listIdHopDong, ApplicationDbContext db)
        {
            try
            {
                dynamic data = new DynamicObject();

                List<string> listfile = new List<string>();
                #region
                var listDanhSachHopDong = (from a in db.HRM_HopDong

                                           where listIdHopDong.Contains(a.IdGuid)
                                           select a).ToList();
                var listPhuCap = db.HRM_HopDongPhuCap.Where(a => listIdHopDong.Contains(a.IdHopDong)).ToList();

                var listLuongNhanVien = db.HRM_Luong_HopDong.Where(r => listIdHopDong.Contains(r.IdHopDong)).ToList();
                var listLuongBH = db.HRM_LuongBaoHiem_HopDong.Where(r => listIdHopDong.Contains(r.IdHopDong)).ToList();
                listIdHopDong = listDanhSachHopDong.Select(a => a.IdHopDong).Distinct().ToList();
                var listHDGoc = (from a in db.HRM_HopDong
                                 where listIdHopDong.Contains(a.IdGuid)
                                 select a).AsEnumerable();
                List<string> listIdNhanVien = listDanhSachHopDong.Select(a => a.IdNhanVien).Distinct().ToList();
                listIdNhanVien.AddRange(listDanhSachHopDong.Select(a => a.IdNguoiDaiDien).Distinct().ToList());
                var listNhanVien = db.HRM_NhanVien.Where(a => listIdNhanVien.Contains(a.IdGuid)).ToList();
                var lstQuocTich = db.HRM_dmQuocTich.ToList();
                var listLoaiHopDong = db.HRM_dmLoaiHopDong.ToList();
                var listTrinhDoDaoTao = db.HRM_dmTrinhDoDaoTao.ToList();
                var CongTys = _CallWebService.GetCongTyKyHopDong(db);
                var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listGiayTo = db.HRM_NhanVien_GiayToTuyThan.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).ToList();
                var listSDT = db.HRM_NhanVien_SoDienThoai.Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isApDung == true).ToList();
                var listThoiHanHopDong = db.HRM_dmThoiHanHopDong.ToList();
                var listNoicap = db.HRM_dmNoiCap.ToList();
                var listdmPhucap = db.HRM_dmPhuCap.ToList();
                foreach(var item in listPhuCap)
                {
                    item.TenPhuCap = listdmPhucap.FirstOrDefault(a => a.IdGuid == item.IddmLoaiPhuCap).Ten;
                }    
                var listDuAn = _CallWebService.GetDanhSachDuAn(db);
                DateTime dateTimeNow = DateTime.Now;
                #endregion
                foreach (var HopDong in listDanhSachHopDong)
                {
                    HopDong.listIdCaLamViec = new List<string>();

                    var LoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                    var filenamegui = "_" + LoaiHopDong.Ma;
                    var NguoiDaiDien = listNhanVien.FirstOrDefault(a => a.IdGuid == HopDong.IdNguoiDaiDien);
                    if (NguoiDaiDien == null)
                        NguoiDaiDien = new HRM_NhanVien();
                    var ChucVuNDaiDien = _CallWebService.GetChucVu(db, NguoiDaiDien.IdChucVu);
                    var NguoiLaoDong = listNhanVien.FirstOrDefault(a => a.IdGuid == HopDong.IdNhanVien);
                    var CCCDNDD = listGiayTo.Where(a => a.IdNhanVien == NguoiDaiDien.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).FirstOrDefault();
                    var CCCDNLD = listGiayTo.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.Loai != 3 && a.Loai != 4 && a.isApDung == true).OrderByDescending(a => a.NgayCap).FirstOrDefault();

                    var CongTy = CongTys.FirstOrDefault(a => a.Id == HopDong.IdCongTyKyHopDong);
                    var BoPhan = listBoPhan.FirstOrDefault(a => a.Id == HopDong.IdBoPhanEOS);
                    var TrinhDo = listTrinhDoDaoTao.FirstOrDefault(a => a.IdGuid == NguoiLaoDong.IddmTrinhDo);
                    var SDT = listSDT.Where(a => a.IdNhanVien == NguoiLaoDong.IdGuid && a.isApDung == true).Select(a => a.SoDienThoai).FirstOrDefault();
                    var QuocTichNDD = new HRM_dmQuocTich();
                    if (NguoiDaiDien != null)
                        lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiDaiDien.IdQuocTich);
                    HRM_Luong_HopDong LuonghopDong = listLuongNhanVien.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                    HRM_LuongBaoHiem_HopDong LuongBH = listLuongBH.Where(r => r.IdHopDong == HopDong.IdGuid).FirstOrDefault();
                    var QuocTichNLD = lstQuocTich.FirstOrDefault(r => r.IdGuid == NguoiLaoDong.IdQuocTich);
                    var dmThoiHanHopDong = listThoiHanHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdThoiHanHopDong);
                    if (dmThoiHanHopDong == null)
                        dmThoiHanHopDong = new HRM_dmThoiHanHopDong();
                    if (CCCDNDD == null)
                        CCCDNDD = new HRM_NhanVien_GiayToTuyThan();

                    else
                    {
                        var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNDD.NoiCap);
                        if (NoiCap != null)
                            CCCDNDD.TenNoiCap = NoiCap.Ten;
                    }
                    if (QuocTichNDD == null)
                        QuocTichNDD = new HRM_dmQuocTich();
                    if (QuocTichNLD == null)
                        QuocTichNLD = new HRM_dmQuocTich();
                    var ChucVuNLD = listChucVu.FirstOrDefault(a => a.Id == HopDong.IddmChucVu);
                    if (ChucVuNLD == null)
                        ChucVuNLD = new ChucVu();
                    if (CCCDNLD == null)
                        CCCDNLD = new HRM_NhanVien_GiayToTuyThan();
                    else
                    {
                        var NoiCap = listNoicap.FirstOrDefault(a => a.IdGuid == CCCDNLD.NoiCap);
                        if (NoiCap != null)
                            CCCDNLD.TenNoiCap = NoiCap.Ten;
                    }
                    if (BoPhan == null)
                        BoPhan = new BoPhan();
                    var ChucDanhNLD = _CallWebService.GetChucDanh(db, HopDong.IddmChucDanh == 0 ? NguoiLaoDong.IdChucDanh : HopDong.IddmChucDanh);
                    if (ChucDanhNLD == null)
                        ChucDanhNLD = new ChucDanh();
                    HRM_HopDong HopDongCha = db.HRM_HopDong.Find(HopDong.IdHopDong);
                    if (HopDongCha == null)
                        HopDongCha = new HRM_HopDong();
               
                    var listDuAn_HD = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == HopDong.IdGuid || a.IdQuyTrinh == HopDong.IdHopDong).ToList();
                    var listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdGuid).Select(a => a.IdBoPhan).ToList();
                    var listDuAnMoi = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
                    var DiaChiLamViec = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.DiaChi).ToList();
                    listId = listDuAn_HD.Where(a => a.IdQuyTrinh == HopDong.IdHopDong).Select(a => a.IdBoPhan).ToList();
                    var listDuAnCu = listDuAn.Where(a => listId.Contains(a.Id)).OrderBy(a => a.TenBoPhan).Select(a => a.TenBoPhan).ToList();
                    var ChucVuNLDCu = _CallWebService.GetChucVu(db, HopDongCha.IddmChucVu);
                    var LuongCha = db.HRM_Luong_HopDong.FirstOrDefault(a => a.IdHopDong == HopDongCha.IdGuid);
                    if (LuongCha == null)
                        LuongCha = new HRM_Luong_HopDong();
                    var LuongBHCha = db.HRM_LuongBaoHiem_HopDong.FirstOrDefault(a => a.IdHopDong == HopDongCha.IdGuid);
                    if (LuongBHCha == null)
                        LuongBHCha = new HRM_LuongBaoHiem_HopDong();
                    string LuongCu = LuongCha.LuongBaoHiem.ToString("N0");
                    string LuongBHCu = LuongBHCha.LuongBaoHiem.ToString("N0");
                    var dmLoaiHopDong = listLoaiHopDong.FirstOrDefault(a => a.IdGuid == HopDong.IdLoaiHopDong);
                    if (dmLoaiHopDong == null)
                    {
                        dmLoaiHopDong = new HRM_dmLoaiHopDong();
                    }
                    string Luong = LuonghopDong.LuongBaoHiem.ToString("N0");
                    string LngBH = LuongBH.LuongBaoHiem.ToString("N0");

                    string fileName = "";
                    string sfileName = "";
                    string sFileNameCopy = PathUploads + NguoiLaoDong.MaNhanVien + "_" + NguoiLaoDong.TenDayDu + filenamegui + ".docx";
                    if (!string.IsNullOrEmpty(HopDong.IdHopDong) || HopDong.isPhuLuc == true)

                    {
                        fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/PhuLucHopDongICT.docx");
                        sfileName = "PhuLucHopDongICT.docx";
                        if (Host == "PHT")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/PLHĐ.docx");
                            sfileName = "PhuLucHopDong.docx";
                        }    
                    }
                    else
                    {
                        if (LoaiHopDong.Ma == "TTHV" || LoaiHopDong.Ma == "HĐHV")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThoaThuanHocViec.docx");
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongHocViec.docx");
                            sfileName = "HopDongLaoDongICT.docx";
                        }
                        else if (LoaiHopDong.Ma == "HĐTV")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecNhanVien.docx");

                            if (ChucVuNLD.MaChucVu == "GS")
                            {
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThuViecGiamSat.docx");
                            }
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongThuViec.docx");
                            sfileName = "HopDongThuViec.docx";
                        }
                        else if (LoaiHopDong.Ma == "HĐVV")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongVuViec.docx");
                            sfileName = "HopDongLaoDongICT.docx";
                        }
                        else if (LoaiHopDong.Ma == "HĐKHOAN")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongKhoanViec.docx");

                            sfileName = "HopDongKhoanViec.docx";
                        }

                        else if (LoaiHopDong.Ma == "BBTL")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongThanhLy.docx");
                            sfileName = "HopDongThanhLy.docx";
                        }
                        else if (LoaiHopDong.Ma == "TTHT")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCTV.docx");
                            sfileName = "HopDongCTV.docx";
                        }

                        else if (HopDong.LoaiLuong != 1)
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongCongNhanThoiVu.docx");

                            sfileName = "HopDongCongNhanThoiVu.docx";


                        }
                        else if ((Host == "ICT" && LoaiHopDong.Ma == "HĐLĐ") || LoaiHopDong.Ma == "THHĐ")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongXacDinhThoiHanNhanVien.docx");
                            if (ChucVuNLD.MaChucVu == "CN")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongLaoDongICT.docx");
                            sfileName = "HopDongLaoDongICT.docx";
                        }

                        else if ((Host == "PHT" && LoaiHopDong.Ma == "HĐLĐ") || LoaiHopDong.Ma == "HĐLĐ." || LoaiHopDong.Ma == "HĐLĐ.KXDTH" || LoaiHopDong.Ma == "KTHHĐ")
                        {
                            fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/HopDongKhongXacDinhThoiHanNhanVien.docx");
                            if (Host == "PHT")
                                fileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/BaoCaoPHT/HopDongLapDong.docx");
                            sfileName = "HopDongLaoDongICT.docx";

                        }
                    }
                    File.Copy(fileName, sFileNameCopy, true);
                    {
                        WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                        Body body = wordDocument.MainDocumentPart.Document.Body;
                        BookmarkStart bookmarkTable = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "NoiDungThayDoi");
                        if (bookmarkTable != null)
                        {
                            
                            var listPhuCapCha = db.HRM_HopDongPhuCap.Where(a => a.IdHopDong == HopDongCha.IdGuid).ToList();
                            if (listPhuCapCha == null)
                                listPhuCapCha = new List<HRM_HopDongPhuCap>();
                            foreach (var item in listPhuCapCha)
                            {
                                item.TenPhuCap = listdmPhucap.FirstOrDefault(a => a.IdGuid == item.IddmLoaiPhuCap).Ten;
                            }
                            var listPhuCapMoi = listPhuCap.Where(a => a.IdHopDong == HopDong.IdGuid).ToList();
                            //  Tạo bảng mới
                            DocumentFormat.OpenXml.Wordprocessing.Table table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                            // Định nghĩa các thuộc tính của bảng
                            TableProperties tblProperties = new TableProperties(
                            new TableBorders(
                            new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                            new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                            ),
                            new TableJustification { Val = TableRowAlignmentValues.Center } // Căn giữa bảng
                            , new RunFonts() { Ascii = "Times New Roman" });
                            table.AppendChild(tblProperties);

                            { // header
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                                InsetCol(tr, 100, 12, "STT", JustificationValues.Center, true);
                                InsetCol(tr, 2000, 12, "Nội dung", JustificationValues.Center, true);
                                InsetCol(tr, 3500, 12, "Thông tin cũ", JustificationValues.Center, true);
                                InsetCol(tr, 3500, 12, "Thông tin mới", JustificationValues.Center, true);
                                table.Append(tr);
                            }
                            int STT = 1;
                            if (listDuAnCu != listDuAnMoi)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Đơn vị quản lý", JustificationValues.Left);
                                InsetCol(tr, 3500, 12, string.Join(",", listDuAnCu), JustificationValues.Left);
                                InsetCol(tr, 3500, 12, string.Join(",", listDuAnMoi), JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }
                            if (ChucVuNLD.TenChucVu != ChucVuNLDCu.TenChucVu)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Chức vụ", JustificationValues.Left);
                                InsetCol(tr, 3500, 12, ChucVuNLDCu.TenChucVu, JustificationValues.Left);
                                InsetCol(tr, 3500, 12, ChucVuNLD.TenChucVu, JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }
                            if (Luong != LuongCu)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Mức lương", JustificationValues.Left);
                                InsetCol(tr, 3500, 12, LuongCu, JustificationValues.Left);
                                InsetCol(tr, 3500, 12, Luong, JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }
                            if (LngBH != LuongBHCu)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Mức lương BHXH", JustificationValues.Left);
                                InsetCol(tr, 3500, 12, LuongBHCu, JustificationValues.Left);
                                InsetCol(tr, 3500, 12, LngBH, JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }

                            if (HopDong.DiaDiemLamViec != HopDongCha.DiaDiemLamViec)
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Địa điểm", JustificationValues.Left);
                                InsetCol(tr, 3500, 12, HopDongCha.DiaDiemLamViec, JustificationValues.Left);
                                InsetCol(tr, 3500, 12, HopDong.DiaDiemLamViec, JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }
                            {
                                DocumentFormat.OpenXml.Wordprocessing.TableRow tr = new DocumentFormat.OpenXml.Wordprocessing.TableRow(new TableRowProperties(
                                  new TableRowHeight { HeightType = HeightRuleValues.Auto }));// Auto-fit chiều cao hàng
                                InsetCol(tr, 100, 12, STT.ToString(), JustificationValues.Center);
                                InsetCol(tr, 2000, 12, "Phụ cấp", JustificationValues.Left);
                                InsetCol(tr, 3500, 12,string.Join(", ", listPhuCapCha.Select(a=>a.TenPhuCap)), JustificationValues.Left);
                                InsetCol(tr, 3500, 12, string.Join(", ", listPhuCapMoi.Select(a => a.TenPhuCap)), JustificationValues.Left);
                                table.Append(tr);
                                STT++;
                            }

                            // Thêm bảng vào tài liệu
                            bookmarkTable.Parent.InsertAfterSelf(table);

                        }
                        wordDocument.MainDocumentPart.Document.Save();
                        wordDocument.Dispose();

                    }
                    if (!string.IsNullOrEmpty(HopDong.IdThoiGianLamViec))
                    {
                        HopDong.listIdCaLamViec = HopDong.IdThoiGianLamViec.Split(',').ToList();
                        var listCaLamViec = db.HRM_dmLichLamViec.Where(a => HopDong.listIdCaLamViec.Contains(a.IdGuid)).AsNoTracking().ToList();
                        var CalamViecs = listCaLamViec.Select(a => new
                        {
                            Ten = a.Ma + ": từ giờ " + a.TuGio.ToString("hh:mm") + " - đến giờ " + a.DenGio.ToString("hh:mm"),
                            MoTa = "Số giờ " + a.SoGioTrongNgay + "h/ngày; " /*+ a.SoNgayCongTieuChuan + " ngày/tháng."*/
                        }).Distinct().ToList();


                        WordprocessingDocument wordDocument = WordprocessingDocument.Open(sFileNameCopy, true);
                        Body body = wordDocument.MainDocumentPart.Document.Body;

                        //Tìm bookmark
                        BookmarkStart bookmarkStart = body.Descendants<BookmarkStart>().FirstOrDefault(b => b.Name == "CaLamViec");
                        if (bookmarkStart != null)
                        {
                            //Tạo một phần tử mới để chèn dữ liệu
                            var parentElement = bookmarkStart.Parent;

                            // Chèn danh sách dữ liệu vào vị trí bookmark
                            foreach (var item in CalamViecs)
                            {
                                Paragraph para = new Paragraph(/*new Run(new Text(item.Ten + " \r\n " + item.MoTa))*/);
                                Run run = new Run();
                                run.AppendChild(new Text(item.MoTa));
                                run.AppendChild(new Break() { Type = BreakValues.TextWrapping });
                                run.AppendChild(new Text(item.MoTa));
                                ParagraphProperties paragraphProperties = new ParagraphProperties();
                                paragraphProperties.Indentation = new Indentation()
                                {
                                    Left = "720", // Lùi dòng trái 0.5 inch (1 inch = 1440 units)
                                    Right = "720", // Lùi dòng phải 0.5 inch
                                    FirstLine = "360" // Lùi dòng đầu tiên 0.5 inch
                                };
                                para.Append(paragraphProperties);
                                para.AppendChild(run);
                                parentElement.InsertAfterSelf(para);
                            }


                        }
                        wordDocument.MainDocumentPart.Document.Save();
                        wordDocument.Dispose();

                    }

                    var doc = DocX.Load(sFileNameCopy);
                    if (HopDongCha != null)
                    {
                        string NgayKy = HopDongCha.NgayKy.Day + " tháng " + HopDongCha.NgayKy.Month + " năm " + HopDongCha.NgayKy.Year;
                        doc.ReplaceText("@HopDongCuNgayKy", NgayKy);
                        doc.ReplaceText("@HopDongCu", HopDongCha.SoHopDong ?? "");
                    }
                    doc.ReplaceText("@NgayHieuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                    doc.ReplaceText("@SoHopDong", HopDong.SoHopDong);

                    string NgayThang = "ngày " + HopDong.NgayKy.Day + " tháng " + HopDong.NgayKy.Month + " năm " + HopDong.NgayKy.Year;
                    doc.ReplaceText("@NgayThang", NgayThang);
                    //Nguoi dai diẹn
                    doc.ReplaceText("@NguoiDaiDien", NguoiDaiDien.TenDayDu ?? "");
                    doc.ReplaceText("@NgaySinhNDD", NguoiDaiDien.NgaySinh == null ? "" : NguoiDaiDien.NgaySinh.ToString("dd/MM/yyyy"));
                    if (QuocTichNDD != null)
                        doc.ReplaceText("@QuocTichNDD", QuocTichNDD.Ten ?? "");
                    if (CCCDNDD != null)
                    {
                        if (!string.IsNullOrEmpty(CCCDNDD.So))
                            doc.ReplaceText("@CCCDNDD", CCCDNDD.So);
                        else
                            doc.ReplaceText("@CCCDNDD", "");
                        if (!string.IsNullOrEmpty(CCCDNDD.TenNoiCap))
                            doc.ReplaceText("@NoiCapCCCDNDD", CCCDNDD.TenNoiCap);
                        else
                            doc.ReplaceText("@NoiCapCCCDNDD", "");
                        if (CCCDNDD.NgayCap != null)
                            doc.ReplaceText("@NgayCapCCCDNDD", CCCDNDD.NgayCap.Value.ToString("dd/MM/yyyy"));
                        else
                            doc.ReplaceText("@NgayCapCCCDNDD", "");
                    }
                    doc.ReplaceText("@DiaChiNDD", NguoiDaiDien.HoKhauThuongTruDiaChi ?? "");
                    if (ChucVuNDaiDien != null && !string.IsNullOrEmpty(ChucVuNDaiDien.TenChucVu))
                        doc.ReplaceText("@ChucVuNguoiDaiDien", ChucVuNDaiDien.TenChucVu);
                    else
                        doc.ReplaceText("@ChucVuNguoiDaiDien", "");
                    doc.ReplaceText("@TenCongTy", CongTy.TenBoPhan);
                    if (!string.IsNullOrEmpty(CongTy.DiaChi))
                        doc.ReplaceText("@DiaChiCongTy", CongTy.DiaChi);
                    else
                        doc.ReplaceText("@DiaChiCongTy", "");
                    if (!string.IsNullOrEmpty(CongTy.DienThoai))
                        doc.ReplaceText("@SoDienThoaiCongTy", CongTy.DienThoai);
                    else
                        doc.ReplaceText("@SoDienThoaiCongTy", "");
                    doc.ReplaceText("@Fax", CongTy.Fax ?? "");
                    doc.ReplaceText("@MaSoThueCongTy", CongTy.MaSoThue ?? "");

                    // Nguoi lao dong
                    doc.ReplaceText("@BHXH", NguoiLaoDong.SoBaoHiemXaHoi == null ? (HopDong.BaoHiem.SoSoBaoHiem ?? "") : NguoiLaoDong.SoBaoHiemXaHoi);
                    doc.ReplaceText("@TenLoaiHopDong", dmLoaiHopDong.Ten ?? "");
                    doc.ReplaceText("@MaNhanVien", NguoiLaoDong.MaNhanVien ?? "");
                    doc.ReplaceText("@NguoiLaoDong", NguoiLaoDong.TenDayDu ?? "");
                    doc.ReplaceText("@QuocTichNLD", QuocTichNLD.Ten ?? "");
                    doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                    doc.ReplaceText("@GioiTinh", NguoiLaoDong.IdGioiTinh ?? "");
                    if (NguoiLaoDong.NgaySinh != null)
                        doc.ReplaceText("@NgaySinh", " " + NguoiLaoDong.NgaySinh.ToString("dd/MM/yyyy"));
                    else
                        doc.ReplaceText("@NgaySinh", "");
                    doc.ReplaceText("@DiaChiThuongTru", NguoiLaoDong.HoKhauThuongTruDiaChi ?? "");
                    if (CCCDNLD != null)
                    {
                        doc.ReplaceText("@CCCD", CCCDNLD.So ?? "");
                        doc.ReplaceText("@NoiCapCCCD", CCCDNLD.TenNoiCap ?? "");
                        if (CCCDNLD.NgayCap != null)
                            doc.ReplaceText("@NgayCapCCCD", CCCDNLD.NgayCap.Value.ToString("dd/MM/yyyy"));
                        else
                            doc.ReplaceText("@NgayCapCCCD", "");
                    }
                    doc.ReplaceText("@SoDienThoai", SDT ?? "");
                    string ThoiHanHopDong = "ngày " + HopDong.NgayHieuLuc.Day + " tháng " + HopDong.NgayHieuLuc.Month + " năm " + HopDong.NgayHieuLuc.Year;
                    if (Host == "PHT")
                        ThoiHanHopDong = "từ ngày " + HopDong.NgayHieuLuc.ToString("dd/MM/yyyy");
                    if (HopDong.NgayHetHan != null && dmThoiHanHopDong.Ma != "KTH")
                    {

                        if (Host == "PHT")
                            ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.ToString("dd/MM/yyyy");
                        else
                            ThoiHanHopDong += " đến ngày " + HopDong.NgayHetHan.Value.Day + " tháng " + HopDong.NgayHetHan.Value.Month + " năm " + HopDong.NgayHetHan.Value.Year;
                    }
                    doc.ReplaceText("@ThoiHanHopDong", ThoiHanHopDong ?? "");
                    doc.ReplaceText("@ThoiHan", dmThoiHanHopDong.Ten ?? "");
                    if (Host == "PHT")
                        doc.ReplaceText("@DiaChiLamViec", CongTy.DiaChi ?? "");
                    else
                        doc.ReplaceText("@DiaChiLamViec", string.Join(",", DiaChiLamViec));
                    doc.ReplaceText("@BoPhan", BoPhan.TenBoPhan ?? "");
                    doc.ReplaceText("@NhiemVu", "");
                    doc.ReplaceText("@ChucVu", ChucVuNLD.TenChucVu ?? "");
                    doc.ReplaceText("@ChucDanh", ChucDanhNLD.Ten ?? "");
                    string LuongChu = dungchung.NumberToText(LuonghopDong.LuongBaoHiem);
                    string PhanTram = LuonghopDong.TyLeHuongLuong.ToString();
                    string HinhThucTraLuong = "";
                    var HTTL = db.HRM_dmHinhThucTraLuong.FirstOrDefault(a => a.IdGuid == LuonghopDong.IddmHinhThucTraLuong);
                    if (HTTL != null)
                        HinhThucTraLuong = HTTL.Ten + ", ";
                    string HTLuong = "";
                    if (HopDong.LoaiLuong == 1)
                    {
                        HinhThucTraLuong = HinhThucTraLuong + "trả lương theo tháng";
                        HTLuong = " đồng/tháng";
                    }
                    else if (HopDong.LoaiLuong == 2)
                    {
                        HinhThucTraLuong = HinhThucTraLuong + "trả lương theo ngày";
                        HTLuong = " đồng/ngày";

                    }
                    else if (HopDong.LoaiLuong == 3)
                    {
                        HinhThucTraLuong = HinhThucTraLuong + "trả lương theo giờ";
                        HTLuong = " đồng/giờ";
                    }
                    if (Host == "PHT" && LoaiHopDong.Ma != "HĐVV" && LoaiHopDong.Ma != "HĐHV")
                    {
                        Luong = LuongBH.LuongBaoHiem.ToString("N0") + HTLuong;
                        LuongChu = dungchung.NumberToText(LuongBH.LuongBaoHiem) + HTLuong;
                        //PhanTram = LuongBH.TyLeHuongLuong.ToString();
                    }
                    else
                    {
                        Luong += HTLuong;
                        LuongChu += HTLuong;
                    }
                    doc.ReplaceText("@LuongChu", LuongChu);
                    doc.ReplaceText("@Luong", Luong);
                    doc.ReplaceText("@PhanTramLuong ", PhanTram);
                    doc.ReplaceText("@NgayKyPhuLuc", HopDong.NgayHieuLuc.ToString("dd/MM/yyyy"));

                    doc.ReplaceText("@HinhThucTraLuong", HinhThucTraLuong);
                    doc.Save();
                    listfile.Add(sFileNameCopy);
                    data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDong.docx")
                           + "&path=" + WebUtility.UrlEncode(Base64Encode(sFileNameCopy));
                }

                if (listfile != null && listfile.Count > 1)
                {
                    string sFileNameZip = PathUploads + "HopDongHieuLuc.zip";
                    using (var zipStream = new FileStream(sFileNameZip, FileMode.Create))
                    {
                        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                        {
                            foreach (var filePath in listfile)
                            {
                                var zipEntry = archive.CreateEntry(Path.GetFileName(filePath));
                                using (var entryStream = zipEntry.Open())
                                {
                                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                                    {
                                        fileStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }
                    }
                    data.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode("HopDongHieuLuc.Zip")
                                        + "&path=" + WebUtility.UrlEncode(Base64Encode(sFileNameZip));
                }
                return data;
            }
            catch (Exception ex)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }

        #endregion
    }
    public class ModelTrangThaiViewModel
    {
        public string Ma { set; get; }
        public string Ten { set; get; }
    }
}