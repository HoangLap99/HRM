#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
using OfficeOpenXml.FormulaParsing.ExpressionGraph.FunctionCompilers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.BieuDo
{

    public class BieuDoBienDongNhanSuTheoNam
    {
        public int Nam { get; set; }

        public int SoLuongNghiViec { get; set; }

        public int SoLuongNhanVienMoi { get; set; }

    }

    public class FuncBieuDoNhanVien:FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncBieuDoNhanVien(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBieuDoNhanVien> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        


        public dynamic GetBieuDoBienDongNhanSuTuNamDenNam(int TuNam, int DenNam,long IdCongTyEOS, ApplicationDbContext db)
        {
            var lstNhanSuNghiViecTuNamDenNam = (from a in db.HRM_NhanVien
                                                where (a.NgayNghiViec != null && a.NgayNghiViec.Value.Year >= TuNam && a.NgayNghiViec.Value.Year <= DenNam) && (IdCongTyEOS==0 || a.IdCongTyEOS== IdCongTyEOS)
                                                select new
                                                {
                                                    Nam = a.NgayNghiViec.Value.Year,
                                                    IdNhanVien = a.IdGuid,
                                                }).Distinct().GroupBy(x => x.Nam).Select(x => new { Nam = x.Key, SoLuongNghi = x.Count() }).ToList();


            var lstNhanVienKyHopDongMoi = (from a in db.HRM_NhanVien
                                           join b in db.HRM_HopDong on a.IdGuid equals b.IdGuid
                                           where b.NgayHieuLuc.Year >= TuNam && b.NgayHieuLuc.Year <= DenNam && (IdCongTyEOS == 0 || a.IdCongTyEOS == IdCongTyEOS)
                                           && b.isPhuLuc==false
                                           select new
                                           {
                                               Nam = b.NgayKy.Year,
                                               IdNhanVien = b.IdNhanVien,
                                           }).Distinct().GroupBy(x => x.Nam).Select(x => new { Nam = x.Key, SoLuongMoi = x.Count() }).ToList();

            List<BieuDoBienDongNhanSuTheoNam> lstReturn = new List<BieuDoBienDongNhanSuTheoNam>();

            for (int Nam = TuNam; Nam <= DenNam; Nam++)
            {
                BieuDoBienDongNhanSuTheoNam BieuDo = new BieuDoBienDongNhanSuTheoNam();
                BieuDo.Nam = Nam;

                var itemNghiViec = lstNhanSuNghiViecTuNamDenNam.Where(x => x.Nam == Nam).FirstOrDefault();
                if (itemNghiViec != null)
                    BieuDo.SoLuongNghiViec = itemNghiViec.SoLuongNghi;

                var itemNhanSuMoi = lstNhanVienKyHopDongMoi.Where(x => x.Nam == Nam).FirstOrDefault();
                if (itemNhanSuMoi != null)
                    BieuDo.SoLuongNhanVienMoi = itemNhanSuMoi.SoLuongMoi;

                lstReturn.Add(BieuDo);
            }

            return lstReturn;

        }

        public class BieuDoBienDongNhanSuTheoThangQuyNam
        {

            //public int Nam { get; set; }
            //public int Thang { get; set; }
            //public int Quy { get; set; }
            //public int NuaNam { get; set; } // 0,1
            public int SoLuongNghiViec { get; set; }

            public int SoLuongNhanVienMoi { get; set; }

        }

        //public dynamic GetBieuDoBienDongNhanSuTuNgayDenNgay(double TuNgayUnix, double DenNgayUnix, string IdBoPhan, string idCurrentUser = "", ApplicationDbContext db = null)
        //{
        //    var queryNghiViec = (from Link in db.HRM_Link_NhanVien_BoPhan_ChucVu
        //                             //  where Link.NgayKetThuc.Year >= TuNam && Link.NgayKetThuc.Year <= DenNam
        //                         select new
        //                         {
        //                             IdBoPhan = Link.IdBoPhanEOS,
        //                             IdNhanVien = Link.IdNhanVien,
        //                             NgayKetThuc = Link.NgayKetThuc,
        //                         });


        //    var queryHopDongMoi = (from hd in db.HRM_HopDong
        //                           select new
        //                           {
        //                               IdBoPhan = hd.IdBoPhanEOS,
        //                               IdNhanVien = hd.IdNhanVien,
        //                               NgayKy = hd.NgayKy
        //                           });

        //    if (TuNgayUnix > 0)
        //    {
        //        DateTime TuNgay = dungchung.UnixTimeStampToDateTime(TuNgayUnix).Date;
        //        queryNghiViec = queryNghiViec.Where(x => x.NgayKetThuc >= TuNgay);
        //        queryHopDongMoi = queryHopDongMoi.Where(x => x.NgayKy >= TuNgay);
        //    }

        //    if (DenNgayUnix > 0)
        //    {
        //        DateTime DenNgay = dungchung.UnixTimeStampToDateTime(DenNgayUnix).Date.AddDays(1);
        //        queryNghiViec = queryNghiViec.Where(x => x.NgayKetThuc < DenNgay);
        //        queryHopDongMoi = queryHopDongMoi.Where(x => x.NgayKy < DenNgay);
        //    }

            

        //    var SoNhanVienNghi = queryNghiViec.Count();
        //    var SoNhanVienMoi = queryHopDongMoi.Count();


        //    BieuDoBienDongNhanSuTheoThangQuyNam BieuDo = new BieuDoBienDongNhanSuTheoThangQuyNam();
        //    BieuDo.SoLuongNghiViec = SoNhanVienNghi;
        //    BieuDo.SoLuongNhanVienMoi = SoNhanVienMoi;

        //    return BieuDo;

        //}

        public class BieuDoNhanSuTheoTrinhDoChuyenMon
        {
            public string IddmTrinhDo { get; set; }

            public string TrinhDo { get; set; }

            public int SoNguoi { get; set; }

            public BieuDoNhanSuTheoTrinhDoChuyenMon() { }

            public BieuDoNhanSuTheoTrinhDoChuyenMon(HRM_dmTrinhDoDaoTao TrinhDo)
            {
                this.IddmTrinhDo = TrinhDo.IdGuid;
                this.TrinhDo = TrinhDo.Ten;
            }
        }
        
        public dynamic GetBieuDoNhanSuTheoTrinhDoChuyenMon(long IdBoPhan, ApplicationDbContext db)
        {
            var ListBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
            List<long> listIdBoPhand = DequyPhongBan(IdBoPhan, ListBoPhanAll);
            string TrangThaiHieuLucHopDong = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var query = (from nv in db.HRM_NhanVien
                         join hd in db.HRM_HopDong
                         on nv.IdGuid equals hd.IdNhanVien
                         where hd.TrangThai == TrangThaiHieuLucHopDong && listIdBoPhand.Contains(hd.IdBoPhanEOS) && hd.isPhuLuc==false
                         select new
                         {
                             IddmTrinhDo = nv.IddmTrinhDo,
                             IdBoPhan = hd.IdBoPhanEOS,
                             IdNhanVien = hd.IdNhanVien
                         });

           

            var lstNhanVienViTriChuyenMon = query.Select(x => new { IddmTrinhDo = x.IddmTrinhDo, IdNhanVien = x.IdNhanVien }).Distinct().ToList();

            var lstReturn = new List<BieuDoNhanSuTheoTrinhDoChuyenMon>();

            var lstViTriChuyenMon = db.HRM_dmTrinhDoDaoTao.ToList();

            foreach (var item in lstViTriChuyenMon)
            {
                BieuDoNhanSuTheoTrinhDoChuyenMon itemReturn = new BieuDoNhanSuTheoTrinhDoChuyenMon(item);
                itemReturn.SoNguoi = lstNhanVienViTriChuyenMon.Where(x => x.IddmTrinhDo == item.IdGuid).Count();
                lstReturn.Add(itemReturn);
            }

            return lstReturn;


        }

        public dynamic GetBieuDoNhanSuTheoThanhPhan(long IdCongTyEOS, ApplicationDbContext db)
        {
            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var lstNhanVienTheoThanhPhan = (from nv in db.HRM_NhanVien
                                            join hd in db.HRM_HopDong
                                            on nv.IdGuid equals hd.IdNhanVien
                                            where hd.TrangThai == TrangThaiDangThucHien && hd.IdCongTyEOS== IdCongTyEOS && hd.isPhuLuc == false
                                            select new
                                            {
                                                GioiTinh = nv.IdGioiTinh,
                                                NhanVien = nv.IdGuid
                                            }).Distinct().GroupBy(x => x.GioiTinh).Select(x => new
                                            {
                                                GioiTinh = x.Key,
                                                SoLuong = x.Count()
                                            }).ToList();


            return lstNhanVienTheoThanhPhan;
        }
        public dynamic GetBieuDoNhanSuTheoDiaPhuong(long IdCongTyEOS, ApplicationDbContext db)
        {
            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var lstNhanVienTheoThanhPhan = (from nv in db.HRM_NhanVien
                                            join hd in db.HRM_HopDong
                                            on nv.IdGuid equals hd.IdNhanVien
                                            where hd.TrangThai == TrangThaiDangThucHien && hd.IdCongTyEOS == IdCongTyEOS && hd.isPhuLuc == false
                                            select new
                                            {
                                                IdTinhThanhPho = nv.HoKhauThuongTruIdTinhThanhPho,
                                                NhanVien = nv.IdGuid
                                            }).Distinct().GroupBy(x => x.IdTinhThanhPho).Select(x => new
                                            {
                                                IdTinhThanhPho = x.Key,
                                                SoLuong = x.Count()
                                            }).ToList();
            var listDanhMucTinh = db.HRM_dmTinh.ToList();
            List<NhanSuTheoTinhTrangViewModel> listReturn = new List<NhanSuTheoTinhTrangViewModel>();
            foreach (var item in lstNhanVienTheoThanhPhan)
            {
                NhanSuTheoTinhTrangViewModel ns = new NhanSuTheoTinhTrangViewModel();
                ns.Iddm = item.IdTinhThanhPho;
                ns.Ten = listDanhMucTinh.Where(r => r.IdGuid == item.IdTinhThanhPho).Select(r => r.Ten).FirstOrDefault();
                ns.SoLuong = item.SoLuong;
                listReturn.Add(ns);
            }
            return listReturn;

           
        }

        public dynamic GetBieuDoNhanSuTheoLoaiHopDong(long IdCongTyEOS, ApplicationDbContext db)
        {
            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var lstNhanVienTheoThanhPhan = (from nv in db.HRM_NhanVien
                                            join hd in db.HRM_HopDong
                                            on nv.IdGuid equals hd.IdNhanVien
                                            where hd.TrangThai == TrangThaiDangThucHien && hd.IdCongTyEOS == IdCongTyEOS && hd.isPhuLuc == false
                                            select new
                                            {
                                                IdLoaiHopDong = hd.IdLoaiHopDong,
                                                NhanVien = nv.IdGuid
                                            }).Distinct().GroupBy(x => x.IdLoaiHopDong).Select(x => new
                                            {
                                                IdLoaiHopDong = x.Key,
                                                SoLuong = x.Count()
                                            }).ToList();
            var listThoiHanHopDong = db.HRM_dmLoaiHopDong.ToList();
            List<NhanSuTheoTinhTrangViewModel> listReturn = new List<NhanSuTheoTinhTrangViewModel>();
            foreach (var item in lstNhanVienTheoThanhPhan)
            {
                NhanSuTheoTinhTrangViewModel ns = new NhanSuTheoTinhTrangViewModel();
                ns.Iddm = item.IdLoaiHopDong;
                ns.Ten = listThoiHanHopDong.Where(r => r.IdGuid == item.IdLoaiHopDong).Select(r => r.Ten).FirstOrDefault();
                ns.SoLuong = item.SoLuong;
                listReturn.Add(ns);
            }
            return listReturn;


        }
        public dynamic GetBieuDoNhanSuTheoTinhTrangHonNhan(long IdCongTyEOS, ApplicationDbContext db)
        {
            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var lstNhanVienTheoThanhPhan = (from nv in db.HRM_NhanVien
                                            join hd in db.HRM_HopDong
                                            on nv.IdGuid equals hd.IdNhanVien
                                            where hd.TrangThai == TrangThaiDangThucHien && hd.IdCongTyEOS == IdCongTyEOS && hd.isPhuLuc == false
                                            select new
                                            {
                                                GioiTinh = nv.IdTinhTrangHonNhan,
                                                NhanVien = nv.IdGuid
                                            }).Distinct().GroupBy(x => x.GioiTinh).Select(x => new
                                            {
                                                IdTinhTrangHonNhan = x.Key,
                                                SoLuong = x.Count()
                                            }).ToList();

            var listDanhMucTinhTrangHonNhan = db.HRM_dmTinhTrangHonNhan.ToList();
            List<NhanSuTheoTinhTrangViewModel> listReturn = new List<NhanSuTheoTinhTrangViewModel>();
            foreach (var item in lstNhanVienTheoThanhPhan)
            {
                NhanSuTheoTinhTrangViewModel ns = new NhanSuTheoTinhTrangViewModel();
                ns.Iddm = item.IdTinhTrangHonNhan;
                ns.Ten = listDanhMucTinhTrangHonNhan.Where(r => r.IdGuid == item.IdTinhTrangHonNhan).Select(r => r.Ten).FirstOrDefault();
                ns.SoLuong = item.SoLuong;
                listReturn.Add(ns);
            }
            return listReturn;
        }
      

   

        public dynamic GetBieuDoNhanSuTheoDoTuoi(long IdCongTyEOS, ApplicationDbContext db = null)
        {

    
            int NamHienTai = DateTime.Now.Year;

            string TrangThaiDangThucHien = DefineData.eTrangTHaiHopDong.HieuLuc.ToString();
            var lstNhanVienTheoDoTuoi = (from nv in db.HRM_NhanVien
                                            join hd in db.HRM_HopDong
                                            on nv.IdGuid equals hd.IdNhanVien
                                         where hd.TrangThai == TrangThaiDangThucHien && hd.IdCongTyEOS == IdCongTyEOS && hd.isPhuLuc == false
                                         select new
                                            {
                                                NgaySinh = nv.NgaySinh,
                                                NhanVien = nv.IdGuid
                                            }).ToList();

            DateTime MaxNgaySinh = lstNhanVienTheoDoTuoi.Select(r => r.NgaySinh).Max();
            DateTime MinNgaySinh = lstNhanVienTheoDoTuoi.Select(r => r.NgaySinh).Min();
          
         

            List<DoTuoiViewModel> lstDoTuoi = new List<DoTuoiViewModel>();

            for (int i = MinNgaySinh.Year; i < (MaxNgaySinh.Year+1); i++)
            {
                DoTuoiViewModel doTuoi = new DoTuoiViewModel();
                doTuoi.Tuoi = NamHienTai - i;
                doTuoi.SoLuong = lstNhanVienTheoDoTuoi.Where(r => r.NgaySinh.Year == i).Count();
                lstDoTuoi.Add(doTuoi);
            }
            return lstDoTuoi;
        }
    }
    public class NhanSuTheoTinhTrangViewModel
    {
        public int SoLuong { set; get; }
        public string Ten { set; get; }
        public string Iddm { set; get; }
    }
    public class DoTuoiViewModel
    {
        public int Tuoi { get; set; }    

        public int SoLuong { get; set; }

       
    }
}