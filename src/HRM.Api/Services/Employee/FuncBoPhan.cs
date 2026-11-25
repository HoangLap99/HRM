#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services
{
    public class FuncBoPhan
    {
        public CallWebService _CallWebService { get; set; }
        private readonly HrmOptions HrmOptions;
        private string Host => HrmOptions.Host ?? "0";

        public FuncBoPhan(CallWebService callWebService, IOptions<HrmOptions> options)
        {
            _CallWebService = callWebService;
            HrmOptions = options.Value;
        }

        public dynamic GetDanhSachNguoiDaiDienTheoCongTy(long IdCongTyEOS, ApplicationDbContext db)
        {
            //List<NguoiDaiDien> listUser = _CallWebService.GetDanhSachNguoiDaiDienTheoCongTy(db, IdCongTyEOS);
            //if (listUser != null && listUser.Count > 0)
            //{
                var BoPhan = _CallWebService.GetDanhSachBoPhan(db).FirstOrDefault(a => a.MaBoPhan == "BLD");
                //var listId = listUser.Select(a => a.IdNguoiDaiDien).Distinct().ToList();
                List<NguoiDaiDienCongTyViewModel> lstNhanVien = (from a in db.HRM_NhanVien
                                                                 where a.IdBoPhanEOS== BoPhan.Id/*&& listId.Contains(a.IdNhanVienEOS)*/ && (Host == "ICT" || (Host != "ICT" && a.TrangThai == DefineData.eTrangThaiNhanVien.DangLamViec.ToString()))
                                                                 //join b in db.HRM_dmQuocTich on a.IdQuocTich equals b.IdGuid
                                                                 orderby a.MaNhanVien
                                                                 select new NguoiDaiDienCongTyViewModel
                                                                 {
                                                                     Ma=a.MaNhanVien,
                                                                     IdNguoiDaiDien = a.IdGuid,
                                                                     TenNguoiDaiDien = a.TenDayDu,
                                                                     IddmQuocTich = a.IdQuocTich,
                                                                     //TenQuocTich = b.Ten,
                                                                     IddmChucVu = a.IdChucVu,
                                                                     //TenChucVu = c.TenChucVu,
                                                                     IdCongTy = a.IdCongTyEOS,
                                                                 }).ToList();
                var listChucVu = _CallWebService.GetDanhSachChucVu(db);
                var listQuocTich = db.HRM_dmQuocTich.ToList();
                foreach (var item in lstNhanVien)
                {
                    var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IddmChucVu);
                    if (chucVu != null)
                        item.TenChucVu = chucVu.TenChucVu;
                    item.TenQuocTich = listQuocTich.Where(a => a.IdGuid == item.IddmQuocTich).Select(a => a.Ten).FirstOrDefault();
                }
                return lstNhanVien;
            //}
            //return listUser;
        }

        //public dynamic GetThongTinCongTyByNhanVien(string IdUser, ApplicationDbContext db)
        //{
        //    long IdLoaiBoPhanPkid = smdb.LoaiBoPhans.Where(r => r.MaLoaiBoPhan == "CONGTY").Select(r => r.Id).FirstOrDefault();
        //    dynamic result = new DynamicObject();
        //    long IdBoPhan = (from a in smdb.NhanViens
        //                     join b in smdb.Link_NhanVien_BoPhan_ChucVu on a.Id equals b.NhanVienPkid
        //                     where a.UserPkid == IdUser && b.KiemNhiem == false
        //                     select b.BoPhanPkid).FirstOrDefault();
        //    var lstBoPhanAll = _CallWebService.GetDanhSachBoPhan(db);
        //    List<long> listBoPhanId = new List<long>();
        //    GetDanhSachPhongBanCha(IdBoPhan, lstBoPhanAll, listBoPhanId);


        //    List<BoPhan> lstBoPhan = lstBoPhanAll.Where(r => listBoPhanId.Contains(r.Id)).ToList();
        //    result.listBoPhan = lstBoPhan;
        //    BoPhan BoPhan = lstBoPhan.FirstOrDefault(x => x.LoaiBoPhanPkid == IdLoaiBoPhanPkid);
        //    var lstChucDanh = _CallWebService.GetDanhSachChucDanh(db);
        //    if (BoPhan != null)
        //    {
        //        result.CongTy = BoPhan;
        //        Models.SmartEOEModel.NhanVien NhanViensmdb = smdb.NhanViens.FirstOrDefault(x => x.Id == BoPhan.idNguoiPhuTrach);
        //        if (NhanViensmdb != null)
        //        {
        //            HRM_NhanVien NhanVien = db.HRM_NhanVien.FirstOrDefault(x => x.IdNhanVienEOS == NhanViensmdb.Id);
        //            var HRM_Link_NhanVien_BoPhan_ChucVus = db.HRM_Link_NhanVien_BoPhan_ChucVu.FirstOrDefault(r => r.IdNhanVien == NhanVien.IdGuid);
        //            if(HRM_Link_NhanVien_BoPhan_ChucVus!=null)
        //            {
        //                NhanVien.TenChucDanh = lstChucDanh.Where(r => r.Id == HRM_Link_NhanVien_BoPhan_ChucVus.IdChucDanhEOS).Select(r => r.Ten).FirstOrDefault();
        //            }    

        //            result.NhanVien = NhanVien;
        //        }

        //    }
        //    return result;
        //}
        public void GetDanhSachPhongBanCha(long id, List<BoPhan> BoPhans, List<long> tempListBoPhansId)
        {
            var listBoPhansId = BoPhans.Where(item => item.Id == id).Select(item => item.BoPhanChaPkid).ToList();
            if (listBoPhansId != null && listBoPhansId.Count > 0)
            {
                tempListBoPhansId.AddRange(listBoPhansId);
                foreach (var item in listBoPhansId)
                {
                    GetDanhSachPhongBanCha(item, BoPhans, tempListBoPhansId);
                }
            }
        }

        #region Connect SmartEOS


        //public long UpdateBoPhanSmartEOS(HRM.Api.Models.SmartEOEModel.BoPhan BoPhan, string IdCurrentUser)
        //{
        //    long IdBoPhan = 0;
        //    if (BoPhan.Id == 0)
        //    {
        //        BoPhan.Created = DateTime.Now;
        //        BoPhan.Modified = DateTime.Now;
        //        BoPhan.ModifiedBy = IdCurrentUser;
        //        BoPhan.CreatedBy = IdCurrentUser;

        //        smdb.BoPhans.Add(BoPhan);
        //    }
        //    else
        //    {
        //        BoPhan.Modified = DateTime.Now;
        //        BoPhan.ModifiedBy = IdCurrentUser;
        //        smdb.Entry(BoPhan).State = EntityState.Modified;

        //    }

        //    try
        //    {
        //        smdb.SaveChanges();
        //        IdBoPhan = BoPhan.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("UpdateBoPhanSmartEOS data: {0}, Loi: ", JsonConvert.SerializeObject(BoPhan), ex.Message);
        //        IdBoPhan = 0;
        //    }

        //    return IdBoPhan;
        //}

        //public bool DeleteBoPhanSmartEOS(List<long> lstIdBoPhan)
        //{
        //    var BoPhans = smdb.BoPhans.Where(x => lstIdBoPhan.Contains(x.Id)).ToList();

        //    smdb.BoPhans.RemoveRange(BoPhans);

        //    try
        //    {
        //        smdb.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("DeleteBoPhanSmartEOS {0}", ex.Message);
        //        return false;
        //    }
        //}

        #endregion
    }
}
