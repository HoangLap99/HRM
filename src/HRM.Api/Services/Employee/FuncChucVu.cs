#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services
{
    public class FuncChucVu
    {
        //public EntitySingleResponse<List<HRM_DM_ChucVu>> GetDanhSachChucVu(bool aIsChucVuDuAn, ApplicationDbContext aContext)
        //{
        //    var result = new EntitySingleResponse<List<HRM_DM_ChucVu>>();

        //    if (aContext.currentUser.Id.IsEmpty())
        //    {
        //        result.Error = new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
        //    }

        //    var listModel = aContext.HRM_DM_ChucVu.Where(x => x.isChucVuDuAn == aIsChucVuDuAn).OrderByDescending(x => x.Created).ToList();

        //    result.Item = listModel;

        //    return result;
        //}

        //public EntitySingleResponse<HRM_DM_ChucVu> GetChucVu(string aId, ApplicationDbContext aContext)
        //{
        //    var result = new EntitySingleResponse<HRM_DM_ChucVu>();

        //    if (aContext.currentUser.Id.IsEmpty())
        //    {
        //        result.Error = new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
        //        return result;
        //    }

        //    if (aId.IsEmpty())
        //    {
        //        result.Error = new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id truyền nên");
        //        return result;
        //    }

        //    var model = aContext.HRM_DM_ChucVu.FirstOrDefault(x => x.IdGuid == aId);

        //    result.Item = model;

        //    return result;
        //}

        //public MessageReturn SetChucVu(HRM_DM_ChucVu aModel, ApplicationDbContext aContext)
        //{
        //    var result = new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Thêm mới chức vụ thành công");

        //    if (aContext.currentUser.Id.IsEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
        //    }

        //    if (aModel == null)
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
        //    }

        //    if (aModel.Ma.IsEmpty())
        //    {
        //        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Vui lòng nhập mã");
        //    }



        //    using (var ts = aContext.Database.BeginTransaction())
        //    {
        //        using (var tsSMart = aSMart.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                if (aModel.IdGuid.IsEmpty())
        //                {
        //                    var checkMa = aContext.HRM_DM_ChucVu.FirstOrDefault(x => x.Ma == aModel.Ma);
        //                    if (checkMa != null)
        //                    {
        //                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã đã tồn tại vui lòng nhập mã khác");
        //                    }
        //                    var chucVuEOS = new HRM.Api.Models.SmartEOEModel.ChucVu();
        //                    chucVuEOS.MaChucVu = aModel.Ma;
        //                    chucVuEOS.TenChucVu = aModel.Ten;

        //                    aSMart.ChucVus.Add(chucVuEOS);
        //                    aSMart.SaveChanges();

        //                    aModel.IdGuid = Guid.NewGuid().ToString();
        //                    aModel.IdChucVuEOS = chucVuEOS.Id;

        //                    aContext.HRM_DM_ChucVu.Add(aModel);
        //                }
        //                else
        //                {
        //                    var model = aContext.HRM_DM_ChucVu.FirstOrDefault(x => x.IdGuid == aModel.IdGuid);
        //                    if (model == null)
        //                    {
        //                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
        //                    }
        //                    if (model.Ma != aModel.Ma)
        //                    {
        //                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không được thay đổi mã");
        //                    }
        //                    var checkMaTrung = aContext.HRM_DM_ChucVu.FirstOrDefault(x => x.Ma == aModel.Ma && x.IdGuid != aModel.IdGuid);
        //                    if (checkMaTrung != null)
        //                    {
        //                        return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã đã tồn tại vui lòng nhập mã khác");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {

        //            }
        //        }
        //    }

        //    return result;
        //}

        //public MessageReturn UpdateChucVu(HRM_DM_ChucVu data, string idCurrentUser, ApplicationDbContext db = null)
        //{

        //    var checktrung = db.HRM_DM_ChucVu.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
        //    if (checktrung > 0)
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

        //    // update sang smartEOS
        //    HRM.Api.Models.SmartEOEModel.ChucVu BoPhanEOS = new HRM.Api.Models.SmartEOEModel.ChucVu();
        //    //BoPhanEOS.LoaiChucVuPkid = data.
        //    BoPhanEOS.MaChucVu = data.Ma;
        //    BoPhanEOS.TenChucVu = data.Ten;
        //    BoPhanEOS.Id = data.IdChucVuEOS;
        //    var IdChucVuEOS = UpdateChucVuSmartEOS(BoPhanEOS, idCurrentUser);
        //    if (IdChucVuEOS == 0)
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật bộ phận không thành công");


        //    if (string.IsNullOrEmpty(data.IdGuid))
        //    {
        //        data.IdChucVuEOS = IdChucVuEOS;
        //        db.HRM_DM_ChucVu.Add(data);
        //    }
        //    else
        //        db.Entry(data).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("UpdateChucVu: {0} {1}", data.IdGuid, ex.Message);
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
        //    }
        //    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        //}

        //public MessageReturn DeleteChucVu(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        //{
        //    List<HRM_DM_ChucVu> listDanhMuc = db.HRM_DM_ChucVu.Where(q => listId.Contains(q.IdGuid)).ToList();

        //    var lstIdBoPhanEOS = listDanhMuc.Select(x => x.IdChucVuEOS).ToList();

        //    bool isDeleted = DeleteChucVuSmartEOS(lstIdBoPhanEOS);
        //    if (!isDeleted)
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật không thành công");

        //    foreach (var edititem in listDanhMuc)
        //    {
        //        db.Entry(edititem).State = EntityState.Deleted;
        //    }
        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("DeleteChucVu: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
        //        return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
        //    }
        //    return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        //}

        //#region Connect SmartEOS

        //public long UpdateChucVuSmartEOS(HRM.Api.Models.SmartEOEModel.ChucVu ChucVu, string IdCurrentUser)
        //{
        //    long IdBoPhan = 0;
        //    if (ChucVu.Id == 0)
        //    {
        //        ChucVu.Created = DateTime.Now;
        //        ChucVu.Modified = DateTime.Now;
        //        ChucVu.ModifiedBy = IdCurrentUser;
        //        ChucVu.CreatedBy = IdCurrentUser;

        //        smdb.ChucVus.Add(ChucVu);
        //    }
        //    else
        //    {
        //        ChucVu.Modified = DateTime.Now;
        //        ChucVu.ModifiedBy = IdCurrentUser;
        //        smdb.Entry(ChucVu).State = EntityState.Modified;

        //    }

        //    try
        //    {
        //        smdb.SaveChanges();
        //        IdBoPhan = ChucVu.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("UpdateChucVuSmartEOS data: {0}, Loi: ", JsonConvert.SerializeObject(ChucVu), ex.Message);
        //        IdBoPhan = 0;
        //    }

        //    return IdBoPhan;
        //}

        //public bool DeleteChucVuSmartEOS(List<long> lstIdBoPhan)
        //{
        //    var BoPhans = smdb.ChucVus.Where(x => lstIdBoPhan.Contains(x.Id)).ToList();

        //    smdb.ChucVus.RemoveRange(BoPhans);

        //    try
        //    {
        //        smdb.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        DefineData.logger.ErrorFormat("DeleteChucVuSmartEOS {0}", ex.Message);
        //        return false;
        //    }
        //}

        //#endregion



    }
}