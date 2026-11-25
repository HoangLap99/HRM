#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.BieuDo
{
    public class FuncBieuDoTuyenDung : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncBieuDoTuyenDung(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncBieuDoTuyenDung> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        //public class BieuDoTuyenDungTuNamDenNam
        //{
        //    public int Nam { get; set; }

        //    public int SoLuongDeNghi { get; set; }

        //    public int SoLuongTuyenDuoc { get; set; }
        //}
        //public List<BieuDoTuyenDungTuNamDenNam> GetBieuDoTuyenDungTuNamDenNam(int TuNam, int DenNam, string idCurrentUser = "", ApplicationDbContext db = null)
        //{
        //    byte TrangThaiPheDuyetDeNghiTuyenDung = (byte)DefineData.eTrangThaiDeNghiTuyenDungTongHop.GiamDocDaDuyet;
        //    var lstDeNghi = (from th in db.HRM_TuyenDung_DeNghi_TongHop
        //                     join dn in db.HRM_TuyenDung_DeNghi
        //                       on th.IdGuid equals dn.IdDeNghiTuyenDungTongHop
        //                     join ct in db.HRM_TuyenDung_DeNghi_ChiTiet
        //                       on dn.IdGuid equals ct.IdTuyenDungDeNghi
        //                       where th.Nam >= TuNam && th.Nam <= DenNam
        //                       select new {
        //                           Nam = th.Nam,
        //                           SoLuongCanTuyen = ct.BoSung + ct.ThayThe,
        //                       } ).GroupBy(x=>x.Nam).Select(x=> new {
        //                           Nam = x.Key,
        //                           SoLuong = x.Sum(y=>y.SoLuongCanTuyen)
        //                       }).ToList();

        //    string TrangThaiUngVien = DefineData.eTrangThaiUngVien.DuocTiepNhan.ToString();

        //    var lstThucTe = (from th in db.HRM_TuyenDung_QuyTrinh
        //                     join link in db.HRM_Link_TuyenDungQuyTrinh_UngVien
        //                       on th.IdGuid equals link.IdTuyenDungPheDuyet
        //                       join uv in db.HRM_TuyenDung_UngVien
        //                       on link.IdTuyenDungUngVien equals uv.IdGuid
        //                       where //th.TrangThai == TrangThaiPheDuyet 
        //                       link.TrangThai == TrangThaiUngVien
        //                       && th.Nam >= TuNam && th.Nam <= DenNam
        //                       group th by th.Nam into grp1
        //                       select new
        //                       {
        //                           Nam = grp1.Key,
        //                           SoLuong = grp1.Count()
        //                       }).ToList();


        //    List<BieuDoTuyenDungTuNamDenNam> lstReturn = new List<BieuDoTuyenDungTuNamDenNam>();
        //    for(int Nam = TuNam; Nam <= DenNam; Nam ++)
        //    {
        //        BieuDoTuyenDungTuNamDenNam itemReturn = new BieuDoTuyenDungTuNamDenNam();
        //        itemReturn.Nam = Nam;
        //        var itemDeNghi = lstDeNghi.FirstOrDefault(x=>x.Nam == Nam);
        //        if (itemDeNghi != null)
        //            itemReturn.SoLuongDeNghi = itemDeNghi.SoLuong;

        //        var itemThucTe = lstThucTe.FirstOrDefault(x => x.Nam == Nam);
        //        if (itemThucTe != null)
        //            itemReturn.SoLuongDeNghi = itemThucTe.SoLuong;

        //        lstReturn.Add(itemReturn);
        //    }

        //    return lstReturn;
           
        //}

        //public class BieuDoTuyenDungChiTietNam
        //{
        //    public int Thang { get; set; }

        //    public int SoLuongHoSo { get; set; }

        //    public int SoLuongTuyenDuoc { get; set; }

        //    public List<BieuDoTuyenDungChiTietNamTheoViTriChuyenMon> lstTheoChucDanh { get; set; }
        //}

        //public class BieuDoTuyenDungChiTietNamTheoViTriChuyenMon
        //{
        //    public string IdViTriChuyenMon { get; set; }

        //    public string ViTriChuyenMon { get; set; }

        //    public int SoLuong { get; set; }
        //}

        //public List<BieuDoTuyenDungChiTietNam> GetBieuDoTuyenDungChiTietNam(int Nam, string idCurrentUser = "", ApplicationDbContext db = null)
        //{


        //    var lstThucTe = (from th in db.HRM_TuyenDung_QuyTrinh
        //                     join link in db.HRM_Link_TuyenDungQuyTrinh_UngVien
        //                     on th.IdGuid equals link.IdTuyenDungPheDuyet
        //                     join uv in db.HRM_TuyenDung_UngVien
        //                     on link.IdTuyenDungUngVien equals uv.IdGuid
        //                     join ctdn in db.HRM_TuyenDung_DeNghi_ChiTiet
        //                     on link.IdTuyenDungDeNghiChiTiet equals ctdn.IdGuid
        //                     join vtcm in db.HRM_DM_ChucVu
        //                     on ctdn.IddmChucDanh equals vtcm.IdGuid
        //                     where th.Nam == Nam
        //                     select new
        //                     {
        //                         ThangQuyTrinhTuyenDung = th.Created.Month,
        //                         ThangTuyenUngVien = link.Modified.Month,
        //                         TrangThaiUngVien = uv.TrangThai,
        //                         IddmChucDanh = vtcm.IdGuid,
        //                         ViTriChuyenMon = vtcm.Ten,
        //                     }).ToList();


        //    string TrangThaiUngVien = DefineData.eTrangThaiUngVien.DuocTiepNhan.ToString();
        //    List<BieuDoTuyenDungChiTietNam> lstReturn = new List<BieuDoTuyenDungChiTietNam>();

        //    for (int i = 1; i <=12; i++)
        //    {
        //        BieuDoTuyenDungChiTietNam BieuDoChiTiet = new BieuDoTuyenDungChiTietNam();
        //        BieuDoChiTiet.Thang = i;
        //        BieuDoChiTiet.SoLuongHoSo = lstThucTe.Where(x => x.ThangQuyTrinhTuyenDung == i).Count();

        //        var lstTuyenDuoc = lstThucTe.Where(x => x.ThangTuyenUngVien == i && x.TrangThaiUngVien == TrangThaiUngVien).ToList();
        //        BieuDoChiTiet.SoLuongTuyenDuoc = lstTuyenDuoc.Count();

        //        BieuDoChiTiet.lstTheoChucDanh = lstTuyenDuoc.GroupBy(x => new { IddmChucDanh = x.IddmChucDanh, ViTriChuyenMon = x.ViTriChuyenMon })
        //            .Select(x => new BieuDoTuyenDungChiTietNamTheoViTriChuyenMon
        //            {
        //                IdViTriChuyenMon = x.Key.IddmChucDanh,
        //                ViTriChuyenMon = x.Key.ViTriChuyenMon,
        //                SoLuong = x.Count()
        //            }).ToList();

        //        lstReturn.Add(BieuDoChiTiet);
        //    }

        //    return lstReturn;

        //}

    }
}