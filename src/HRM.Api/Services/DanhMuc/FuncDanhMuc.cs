#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Features.DanhMuc;
using HRM.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
//using HRM.Api.Models.SmartEOEModel;
using HRM.Api.Helper;
using HRM.Api.Entity;
using System.Net;
using OfficeOpenXml;
using System.IO;
using System.Data;
using System.Threading.Tasks;
namespace HRM.Api.Services
{
    public class FuncDanhMuc : FuncBase
    {
        protected readonly IDanhMucGenericService CatalogService;
        private string Host => HrmOptions.Host;

        public FuncDanhMuc(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IDanhMucGenericService catalogService,
            IOptions<HrmOptions> options,
            ILogger<FuncDanhMuc> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
            CatalogService = catalogService;
        }
        public List<ClsDanhMuc> GetListLoaiXinDiMuonVeSom()
        {
            List<ClsDanhMuc> list = new List<ClsDanhMuc>();
            list.Add(new ClsDanhMuc
            {
                Id = 1,
                Ten = "Đi muộn"
            });
            list.Add(new ClsDanhMuc
            {
                Id = 2,
                Ten = "Về sớm"
            });
            list.Add(new ClsDanhMuc
            {
                Id = 3,
                Ten = "Ra ngoài cá nhân trong giờ"
            });

            return list;

        }
        public List<DM_LoaiLuongModel> GetDanhMucLoaiLuong()
        {
            List<DM_LoaiLuongModel> dM_LoaiLuongs = new List<DM_LoaiLuongModel>();
            DM_LoaiLuongModel luongTheoThang = new DM_LoaiLuongModel
            {
                Id = 1,
                LoaiLuong = "Theo tháng"
            };
            dM_LoaiLuongs.Add(luongTheoThang);
            DM_LoaiLuongModel luongTheoNgay = new DM_LoaiLuongModel
            {
                Id = 2,
                LoaiLuong = "Theo ngày công"
            };
            dM_LoaiLuongs.Add(luongTheoNgay);
            DM_LoaiLuongModel luongTheoGio = new DM_LoaiLuongModel
            {
                Id = 3,
                LoaiLuong = "Theo giờ làm việc"
            };
            dM_LoaiLuongs.Add(luongTheoGio);

            return dM_LoaiLuongs;
        }
        public dynamic GetDanhMucLoaiPhuCap()
        {
            List<DM_LoaiLuongModel> dM_LoaiLuongs = new List<DM_LoaiLuongModel>();
            DM_LoaiLuongModel luongTheoThang = new DM_LoaiLuongModel
            {
                Id = 1,
                LoaiLuong = "Theo tháng"
            };
            dM_LoaiLuongs.Add(luongTheoThang);
            DM_LoaiLuongModel luongTheoNgay = new DM_LoaiLuongModel
            {
                Id = 2,
                LoaiLuong = "Theo ngày công"
            };
            dM_LoaiLuongs.Add(luongTheoNgay);
            DM_LoaiLuongModel luongTheoGio = new DM_LoaiLuongModel
            {
                Id = 3,
                LoaiLuong = "Theo giờ làm việc"
            };
            dM_LoaiLuongs.Add(luongTheoGio);
            DM_LoaiLuongModel luongTheoNam = new DM_LoaiLuongModel
            {
                Id = 4,
                LoaiLuong = "Theo năm"
            };
            dM_LoaiLuongs.Add(luongTheoNam);
            if (Host == "PHT")
            {
                DM_LoaiLuongModel luongTheoCaGay = new DM_LoaiLuongModel
                {
                    Id = 5,
                    LoaiLuong = "(>=15 công ->full, <15 c theo T Tế)"
                };
                dM_LoaiLuongs.Add(luongTheoCaGay);
            }
            return dM_LoaiLuongs;
        }
        public dynamic GetDanhMucLoaiTrangThai()
        {
            List<DanhMuc> dM_LoaiLuongs = new List<DanhMuc>();
            DanhMuc luongTheoThem = new DanhMuc
            {
                Ma = DefineData.eModify.Them.ToString(),
                Ten = "Thêm"
            };
            dM_LoaiLuongs.Add(luongTheoThem);
            DanhMuc luongTheoSua = new DanhMuc
            {
                Ma = DefineData.eModify.Sua.ToString(),
                Ten = "Sữa"
            };
            dM_LoaiLuongs.Add(luongTheoSua);
            DanhMuc luongTheoXoa = new DanhMuc
            {
                Ma = DefineData.eModify.Xoa.ToString(),
                Ten = "Xóa"
            };
            dM_LoaiLuongs.Add(luongTheoXoa);
            DanhMuc luongTheoChuyenTiep = new DanhMuc
            {
                Ma = DefineData.eModify.ChuyenTiep.ToString(),
                Ten = "Chuyển tiếp"
            };
            dM_LoaiLuongs.Add(luongTheoChuyenTiep);
            DanhMuc luongTheoKhongDuyet = new DanhMuc
            {
                Ma = DefineData.eModify.KhongDuyet.ToString(),
                Ten = "Không duyệt"
            };
            dM_LoaiLuongs.Add(luongTheoKhongDuyet);
            DanhMuc luongTheoKhongDuyetBuocCho = new DanhMuc
            {
                Ma = DefineData.eModify.KhongDuyetBuocCho.ToString(),
                Ten = "Không duyệt bước chờ"
            };
            dM_LoaiLuongs.Add(luongTheoKhongDuyetBuocCho);
            DanhMuc Import = new DanhMuc
            {
                Ma = DefineData.eModify.Import.ToString(),
                Ten = "Import"
            };
            dM_LoaiLuongs.Add(Import);



            return dM_LoaiLuongs;
        }
        public dynamic GetDanhMucLoaiChucNang()
        {
            List<DanhMuc> dM_LoaiLuongs = new List<DanhMuc>();


            List<string> listMa = new List<string>
            {
                DefineData.eAction.HOSONHANVIEN.ToString(),
                DefineData.eAction.CHUONGTRINHDAOTAO.ToString(),
            DefineData.eAction.QUYTRINHPHEDUYETLUONGUNGVIEN.ToString(),
            ////DefineData.eAction.KEHOACHLAMVIECTUAN.ToString(),
            DefineData.eAction.QUYTRINHCHAMCONG.ToString(),
            DefineData.eAction.QUYTRINHHOPDONGNHANSU.ToString(),
            DefineData.eAction.QUYTRINHNGHIVIEC.ToString(),
            DefineData.eAction.QUYTRINHQUYETDINHKHENTHUONG.ToString(),
            DefineData.eAction.QUYTRINHQUYETDINHKYLUAT.ToString(),
            DefineData.eAction.QUYTRINHDIEUCHINHLUONG.ToString(),
            DefineData.eAction.QUYTRINHBONHIEMDIEUDONG.ToString(),
            DefineData.eAction.QUYTRINHKEHOACHPHUCLOI.ToString(),
            DefineData.eAction.QUYTRINHPHUCLOI.ToString(),
            DefineData.eAction.QUYTRINHTINHLUONG.ToString(),
            DefineData.eAction.DENGHITUYENDUNG.ToString(),
            DefineData.eAction.QUYTRINHDIMUONVESOM.ToString(),
            DefineData.eAction.XACNHANDIMUONVESOM.ToString(),
            DefineData.eAction.QUYTRINHLAMTHEMGIO.ToString(),
            DefineData.eAction.QUYTRINHXINNGHIPHEP.ToString(),
            DefineData.eAction.DANHGIANHANVIEN.ToString(),
            DefineData.eAction.XACNHANNGOAIGIO.ToString(),
            DefineData.eAction.KEHOACHCONGTAC.ToString(),
            DefineData.eAction.KETQUACONGTAC.ToString(),

        };
            foreach (var item in listMa)
            {
                dM_LoaiLuongs.Add(new DanhMuc
                {
                    Ma = item,
                    Ten = DefineData.QuyDoiTenQuyTrinh(item)
                });
            }
            return dM_LoaiLuongs;
        }

        public ErrorMessage SmartDongBoQuyTrinhXuLy(ApplicationDbContext db, List<HRM_QuyTrinhXuLy> listQuyTrinh)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                //List<HRM_QuyTrinhXuLy> listQuyTrinh = JsonConvert.DeserializeObject(dnm.ToString(), typeof(List<HRM_QuyTrinhXuLy>)) as List<HRM_QuyTrinhXuLy>;
                if (listQuyTrinh.Count() > 0)
                {
                    HRM_QuyTrinhXuLy QuyTrinh = listQuyTrinh.FirstOrDefault();
                    if (QuyTrinh == null)
                    {
                        return err;
                    }
                    HRM_QuyTrinhXuLy itemDB = db.HRM_QuyTrinhXuLy.AsNoTracking().Where(x => x.IdThietLapQuyTrinh == QuyTrinh.IdThietLapQuyTrinh).FirstOrDefault();
                    if (itemDB == null)
                        return err;
                    List<HRM_QuyTrinhXuLy> listQuyTrinhDB = db.HRM_QuyTrinhXuLy.AsNoTracking().Where(x => x.IdThietLapQuyTrinh == QuyTrinh.IdThietLapQuyTrinh
                                                    && x.IdTrangThai == QuyTrinh.IdTrangThai).ToList();
                    foreach (HRM_QuyTrinhXuLy item in listQuyTrinh)
                    {
                        if (item.GhiChu == null)
                            item.GhiChu = "";
                        if (item.IdTable == null)
                            item.IdTable = "";
                        if (item.IdTrangThai == null)
                            item.IdTrangThai = "";
                        if (item.TenTrangThai == null)
                            item.TenTrangThai = "";
                        HRM_QuyTrinhXuLy itemFind = listQuyTrinhDB.Find(x => x.Id == item.Id);
                        if (itemFind == null)
                            db.HRM_QuyTrinhXuLy.Add(item);
                        else
                            db.Entry(item).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                err.SetLoiVang(e.ToString());
            }
            return err;

        }
        #region mức đống thuế thu nhập cá nhân
        public List<HRM_dmCauHinhThueThuNhapCaNhan> GetDanhSachCauHinhThueThuNhapCaNhan(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmCauHinhThueThuNhapCaNhan>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmCauHinhThueThuNhapCaNhan.OrderBy(x => x.STT).ToList();
            return result;
        }

        public MessageReturn UpdatedmCauHinhThueThuNhapCaNhan(HRM_dmCauHinhThueThuNhapCaNhan data, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmCauHinhThueThuNhapCaNhan.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmCauHinhThueThuNhapCaNhan.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucQuocTich: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeletedmCauHinhThueThuNhapCaNhan(List<string> listId, ApplicationDbContext db = null)
        {
            List<HRM_dmCauHinhThueThuNhapCaNhan> listDanhMuc = db.HRM_dmCauHinhThueThuNhapCaNhan.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DelDanhMucQuocTich: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion
        #region Quốc tịch

        public List<HRM_dmQuocTich> GetDanhSachQuocTich(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmQuocTich>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmQuocTich.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucQuocTich(HRM_dmQuocTich data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmQuocTich.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmQuocTich.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucQuocTich: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucQuocTich(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmQuocTich> listDanhMuc = db.HRM_dmQuocTich.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DelDanhMucQuocTich: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Dân tộc

        public List<HRM_dmDanToc> GetDanhSachDanToc(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmDanToc>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmDanToc.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucDanToc(HRM_dmDanToc data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmDanToc.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmDanToc.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucDanToc: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucDanToc(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmDanToc> listDanhMuc = db.HRM_dmDanToc.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucDanToc: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region tình trạng hôn nhân

        public List<HRM_dmTinhTrangHonNhan> GetDanhSachTinhTrangHonNhan(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmTinhTrangHonNhan>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmTinhTrangHonNhan.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucTinhTrangHonNhan(HRM_dmTinhTrangHonNhan data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmTinhTrangHonNhan.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTinhTrangHonNhan.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucQuocTich: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucTinhTrangHonNhan(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmTinhTrangHonNhan> listDanhMuc = db.HRM_dmTinhTrangHonNhan.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DelDanhMucQuocTich: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Loại nhân viên

        public List<HRM_dmLoaiNhanVien> GetDanhSachLoaiNhanVien(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmLoaiNhanVien>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmLoaiNhanVien.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucLoaiNhanVien(HRM_dmLoaiNhanVien data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmLoaiNhanVien.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmLoaiNhanVien.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucLoaiNhanVien: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucLoaiNhanVien(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmLoaiNhanVien> listDanhMuc = db.HRM_dmLoaiNhanVien.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucLoaiNhanVien: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Chức danh



        #endregion

        #region Nhóm chuyên môn

        public List<HRM_dmNhomChuyenMon> GetDanhSachNhomChuyenMon(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmNhomChuyenMon>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmNhomChuyenMon.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucNhomChuyenMon(HRM_dmNhomChuyenMon data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmNhomChuyenMon.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmNhomChuyenMon.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNhomChuyenMon: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucNhomChuyenMon(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmNhomChuyenMon> listDanhMuc = db.HRM_dmNhomChuyenMon.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucNhomChuyenMon: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Vị trí chuyên môn

        public List<HRM_dmViTriChuyenMon> GetDanhSachViTriChuyenMon(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmViTriChuyenMon>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmViTriChuyenMon.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public HRM_dmViTriChuyenMon GetViTriChuyenMon(string IdViTriChuyenMon, ApplicationDbContext aContext = null)
        {
            if (aContext.currentUser.Id.IsEmpty())
            {
                return null;
            }
            if (IdViTriChuyenMon.IsEmpty())
            {
                return null;
            }

            var result = aContext.HRM_dmViTriChuyenMon.FirstOrDefault(x => x.IdGuid == IdViTriChuyenMon);
            if (result.IdNhomChuyenMon.IsNotEmpty() && result != null)
            {
                var toDictNhomChuyenMon = aContext.HRM_dmNhomChuyenMon.ToList().ToDict(x => x.IdGuid);
                result.NhomChuyenMon = result.IdNhomChuyenMon.GetValueFromDic(toDictNhomChuyenMon);
            }

            return result;
        }

        public MessageReturn UpdateDanhMucChuyenMon(HRM_dmViTriChuyenMon data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmViTriChuyenMon.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmViTriChuyenMon.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNhomChuyenMon: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucViTriChuyenMon(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmViTriChuyenMon> listDanhMuc = db.HRM_dmViTriChuyenMon.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucViTriChuyenMon: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Cấp bậc

        public List<HRM_dmCapBac> GetDanhSachCapBac(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmCapBac>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmCapBac.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucCapBac(HRM_dmCapBac data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmCapBac.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmCapBac.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucCapBac: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucCapBac(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmCapBac> listDanhMuc = db.HRM_dmCapBac.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucCapBac: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Quan hệ

        public List<HRM_dmQuanHeGiaDinh> GetDanhSachQuanHeGiaDinh(ApplicationDbContext db = null)
        {
            var result = new List<HRM_dmQuanHeGiaDinh>();
            if (db.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = db.HRM_dmQuanHeGiaDinh.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucQuanHeGiaDinh(HRM_dmQuanHeGiaDinh data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmQuanHeGiaDinh.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmQuanHeGiaDinh.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucQuanHeGiaDinh: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucQuanHeGiaDinh(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmQuanHeGiaDinh> listDanhMuc = db.HRM_dmQuanHeGiaDinh.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucQuanHeGiaDinh: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region thời gian làm việc

        public List<HRM_dmThoiGianLamViec> GetDanhSachThoiGianLamViec(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmThoiGianLamViec>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmThoiGianLamViec.OrderBy(x => x.SapXep).ToList();
            if (result.IsEmpty())
            {
                return result;
            }

            foreach (var item in result)
            {
                item.TenCaLamViec = item.Ma + ":" + item.TuGio.ToString("hh:mm") + " - " + item.DenGio.ToString("hh:mm");
            }

            return result;
        }

        public List<HRM_dmHinhThucLamViec> GetDanhSachHinhThucLamViec(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmHinhThucLamViec>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmHinhThucLamViec.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhHinhThucLamViec(HRM_dmHinhThucLamViec data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmHinhThucLamViec.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmHinhThucLamViec.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucThoiGianLamViec: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucHinhThucLamViec(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmHinhThucLamViec> listDanhMuc = db.HRM_dmHinhThucLamViec.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucThoiGianLamViec: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public HRM_dmThoiGianLamViec GetThoiGianLamViec(string aIdViTriChuyenMon, ApplicationDbContext aContext = null)
        {
            if (aContext.currentUser.Id.IsEmpty())
            {
                return null;
            }

            if (aIdViTriChuyenMon.IsEmpty())
            {
                return null;
            }

            var result = aContext.HRM_dmThoiGianLamViec.FirstOrDefault(x => x.IdGuid == aIdViTriChuyenMon);
            if (result.IdChucDanh.IsNotEmpty() && result != null)
            {
                //var toDictChucDanh = aContext.HRM_dmChucDanh.ToList().ToDict(x => x.IdGuid);
                //result.ChucDanh = result.IdChucDanh.GetValueFromDic(toDictChucDanh);
            }
            return result;
        }

        public MessageReturn UpdateDanhMucThoiGianLamViec(HRM_dmThoiGianLamViec data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmThoiGianLamViec.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            data.TuGio = dungchung.UnixTimeStampToDateTime(data.TuGioUnix);
            data.DenGio = dungchung.UnixTimeStampToDateTime(data.DenGioUnix);

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmThoiGianLamViec.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucThoiGianLamViec: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucThoiGianLamViec(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmThoiGianLamViec> listDanhMuc = db.HRM_dmThoiGianLamViec.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucThoiGianLamViec: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Hệ số nghỉ bù

        public List<HRM_dmHeSoNghiBu> GetDanhSachHeSoNghiBu(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmHeSoNghiBu>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmHeSoNghiBu.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucHeSoNghiBu(HRM_dmHeSoNghiBu data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmHeSoNghiBu.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmHeSoNghiBu.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucHeSoNghiBu: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucHeSoNghiBu(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmHeSoNghiBu> listDanhMuc = db.HRM_dmHeSoNghiBu.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucHeSoNghiBu: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Ăn ca

        public List<HRM_dmAnCa> GetDanhSachAnCa(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmAnCa>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmAnCa.OrderBy(x => x.SapXep).ToList();
            return result;

        }
        public dynamic GetListdmAnCa(dynamic data, ApplicationDbContext db = null)
        {

            TimKiem TimKiem = JsonConvert.DeserializeObject(data.ToString(), typeof(TimKiem)) as TimKiem;
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int nTabTrangThai = TimKiem.TabTrangThai ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_dmAnCa> result = from a in db.HRM_dmAnCa
                                                 orderby a.Created descending
                                                 select a;
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.Ma.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.Ten.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                dynamic dnm = new DynamicObject();
                foreach (var item in itemList)
                {
                    if (item.IdChucDanh != null)
                    {
                        item.listIdChucDanh = new List<long>();
                        if (item.IdChucDanh.Contains(","))
                        {
                            string[] listId = item.IdChucDanh.Split(',');
                            foreach (var a in listId)
                                item.listIdChucDanh.Add(long.Parse(a));
                        }
                        else
                            item.listIdChucDanh.Add(long.Parse(item.IdChucDanh));
                    }
                }
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Có lỗi xảy ra");
            }
        }

        public HRM_dmAnCa GetAnCa(string aIdViTriChuyenMon, ApplicationDbContext aContext = null)
        {
            if (aContext.currentUser.Id.IsEmpty())
            {
                return null;
            }
            if (aIdViTriChuyenMon.IsEmpty())
            {
                return null;
            }
            var result = aContext.HRM_dmAnCa.FirstOrDefault(x => x.IdGuid == aIdViTriChuyenMon);
            if (result != null)
            {
                if (result.IdLoaiNhanVien.IsNotEmpty())
                {
                    var toDictLoaiNhanVien = aContext.HRM_dmLoaiNhanVien.ToList().ToDict(x => x.IdGuid);
                    result.LoaiNhanVien = result.IdLoaiNhanVien.GetValueFromDic(toDictLoaiNhanVien);
                }
                if (result.IdChucDanh.IsNotEmpty() && result.IdChucDanh.Contains(","))
                {
                    string[] list = result.IdChucDanh.Split(',');
                    foreach (string a in list)
                        result.listIdChucDanh.Add(long.Parse(a));

                }
                else
                    result.listIdChucDanh.Add(long.Parse(result.IdChucDanh));
            }
            return result;
        }

        public MessageReturn UpdateDanhMucAnCa(HRM_dmAnCa data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmAnCa.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");
            if (data.listIdChucDanh != null && data.listIdChucDanh.Count > 0)
                data.IdChucDanh = string.Join(",", data.listIdChucDanh);
            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmAnCa.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucAnCa: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucAnCa(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmAnCa> listDanhMuc = db.HRM_dmAnCa.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucAnCa: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Khu vực

        public List<HRM_dmKhuVuc> GetDanhSachKhuVuc(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmKhuVuc>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmKhuVuc.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucKhuVuc(HRM_dmKhuVuc data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmKhuVuc.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmKhuVuc.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucKhuVuc: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucKhuVuc(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmKhuVuc> listDanhMuc = db.HRM_dmKhuVuc.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucKhuVuc: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Khu vực địa điểm

        public List<HRM_dmKhuVucDiaDiem> GetDanhSachKhuVucDiaDiem(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmKhuVucDiaDiem>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmKhuVucDiaDiem.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucKhuVucDiaDiem(HRM_dmKhuVucDiaDiem data, string idCurrentUser, ApplicationDbContext db = null)
        {

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmKhuVucDiaDiem.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucKhuVucDiaDiem: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucKhuVucDiaDiem(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmKhuVucDiaDiem> listDanhMuc = db.HRM_dmKhuVucDiaDiem.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucKhuVucDiaDiem: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Công tác phí

        public List<HRM_dmCongTacPhi> GetDanhSachCongTacPhi(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmCongTacPhi>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmCongTacPhi.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public dynamic GetListdmCongTacPhi(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmCongTacPhi
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listdmLoaiChiPhi = db.HRM_dmLoaiChiPhiCongTac.ToList();
            foreach (var item in listItem)
            {
                var loaiChiPhi = listdmLoaiChiPhi.FirstOrDefault(a => a.IdGuid == item.IddmLoaiChiPhiCongTac);
                if (loaiChiPhi != null)
                    item.TendmLoaiChiPhiCongTac = loaiChiPhi.Ten;
            }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }

        public MessageReturn UpdateDanhMucCongTacPhi(HRM_dmCongTacPhi data, string idCurrentUser, ApplicationDbContext db = null)
        {
            var checktrung = db.HRM_dmCongTacPhi.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmCongTacPhi.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucCongTacPhi: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucCongTacPhi(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmCongTacPhi> listDanhMuc = db.HRM_dmCongTacPhi.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucCongTacPhi: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion



        #region Địa điểm làm việc

        public List<HRM_dmDiaDiemLamViec> GetDanhSachDiaDiemLamViec(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmDiaDiemLamViec>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmDiaDiemLamViec.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDiaDiemLamViec(HRM_dmDiaDiemLamViec data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmDiaDiemLamViec.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmDiaDiemLamViec.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDiaDiemLamViec: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDiaDiemLamViec(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmDiaDiemLamViec> listDanhMuc = db.HRM_dmDiaDiemLamViec.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDiaDiemLamViec: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Tiền tệ

        public List<HRM_dmThietLapNgayNghi> GetDanhSachNgayNghiTrongThang(DateTime TuNgay, DateTime DenNgay, ApplicationDbContext db = null)
        {
            return db.HRM_dmThietLapNgayNghi.Where(x => x.TuNgay >= TuNgay && x.TuNgay < DenNgay).OrderBy(x => x.SapXep).ToList();
        }

        #endregion

        #region Danh mục thiết lập ngày nghỉ

        public List<HRM_dmThietLapNgayNghi> GetDanhSachDanhMucThietLapNgayNghi(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmThietLapNgayNghi>();

            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            result = aContext.HRM_dmThietLapNgayNghi.OrderBy(x => x.SapXep).ToList();

            return result;
        }

        public MessageReturn SetDanhMucThietLapNgayNghi(HRM_dmThietLapNgayNghi aModel, ApplicationDbContext aContext = null)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục thiết lập ngày nghỉ");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.Ma.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Vui lòng nhập mã thiết lập ngày nghỉ");
            }

            var toDictId = aContext.HRM_dmThietLapNgayNghi.ToList().ToDict(x => x.IdGuid);
            var toDictMa = aContext.HRM_dmThietLapNgayNghi.ToList().ToDict(x => x.Ma);

            if (aModel.IdGuid.IsEmpty())
            {
                if (toDictMa.ContainsKey(aModel.Ma))
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Mã bị trùng. Vui lòng nhập mã khác");
                }
                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmThietLapNgayNghi.Add(aModel);
            }
            else
            {
                if (toDictId.ContainsKey(aModel.IdGuid))
                {
                    if (toDictId[aModel.IdGuid].Ma != aModel.Ma)
                    {
                        return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không được thay đổi mã");
                    }
                }
                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật danh mục thiết lập ngày nghỉ thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn DeleteDanhMucThietLapNgayNghi(List<string> aListId, ApplicationDbContext aContext = null)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu danh mục thiết lập ngày nghỉ thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id truyền nên");
            }

            var listModel = aContext.HRM_dmThietLapNgayNghi.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listModel.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            aContext.HRM_dmThietLapNgayNghi.RemoveRange(listModel);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #region Tiền tệ

        public List<HRM_dmTienTe> GetDanhSachTienTe(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmTienTe>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmTienTe.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucTienTe(HRM_dmTienTe data, string idCurrentUser, ApplicationDbContext db = null)
        {
            var checktrung = db.HRM_dmTienTe.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTienTe.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucTienTe: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucTienTe(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmTienTe> listDanhMuc = db.HRM_dmTienTe.Where(q => listId.Contains(q.IdGuid)).OrderBy(x => x.SapXep).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucTienTe: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion



        #region trình độ đào tạo

        public List<HRM_dmTrinhDoDaoTao> GetDanhSachTrinhDoDaoTao(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmTrinhDoDaoTao>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmTrinhDoDaoTao.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucTrinhDoDaoTao(HRM_dmTrinhDoDaoTao data, string idCurrentUser, ApplicationDbContext db = null)
        {
            var checktrung = db.HRM_dmTrinhDoDaoTao.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTrinhDoDaoTao.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucTrinhDoDaoTao: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucTrinhDoDaoTao(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmTrinhDoDaoTao> listDanhMuc = db.HRM_dmTrinhDoDaoTao.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucTrinhDoDaoTao: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Ngôn ngữ

        public List<HRM_dmNgonNgu> GetDanhSachNgonNgu(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmNgonNgu>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmNgonNgu.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucNgonNgu(HRM_dmNgonNgu data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmNgonNgu.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmNgonNgu.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNgonNgu: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucNgonNgu(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmNgonNgu> listDanhMuc = db.HRM_dmNgonNgu.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucNgonNgu: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục nguồn tuyển dụng

        public List<HRM_dmNguonTuyenDung> GetDanhSachNguonTuyenDung(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmNguonTuyenDung>();

            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            result = aContext.HRM_dmNguonTuyenDung.OrderBy(x => x.SapXep).ToList();

            return result;
        }

        public MessageReturn SetDanhMucNguonTuyenDung(HRM_dmNguonTuyenDung aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục nguồn tuyển dụng");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.Ma.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Vui lòng nhập mã nguồn tuyển dụng");
            }


            var toDictMa = aContext.HRM_dmNguonTuyenDung.FirstOrDefault(x => x.Ma == aModel.Ma && aModel.IdGuid != x.IdGuid);
            if (toDictMa != null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu mã bị trùng. Vui lòng nhập mã khác");
            }
            if (aModel.IdGuid.IsEmpty())
            {

                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmNguonTuyenDung.Add(aModel);
            }
            else
            {

                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu danh mục nguồn tuyển dụng thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn DeleteDanhMucNguonTuyenDung(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu danh mục nguồn tuyển dụng  thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id truyền nên");
            }

            var listModel = aContext.HRM_dmNguonTuyenDung.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listModel.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            aContext.HRM_dmNguonTuyenDung.RemoveRange(listModel);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #region Xếp loại

        public List<HRM_dmXepLoai> GetDanhSachXepLoai(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmXepLoai>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmXepLoai.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucXepLoai(HRM_dmXepLoai data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmXepLoai.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmXepLoai.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucXepLoai: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucXepLoai(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmXepLoai> listDanhMuc = db.HRM_dmXepLoai.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucXepLoai: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Loại hợp đồng

        public List<HRM_dmLoaiHopDong> GetDanhSachLoaiHopDong(ApplicationDbContext aContext = null, bool isPhuLuc = false)
        {
            var result = new List<HRM_dmLoaiHopDong>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmLoaiHopDong.Where(a => a.isPhuLuc == isPhuLuc).OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucLoaiHopDong(HRM_dmLoaiHopDong data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmLoaiHopDong.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmLoaiHopDong.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucLoaiHopDong: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucLoaiHopDong(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmLoaiHopDong> listDanhMuc = db.HRM_dmLoaiHopDong.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucLoaiHopDong: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Thời hạn hợp đồng

        public List<HRM_dmThoiHanHopDong> GetDanhSachThoiHanHopDong(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmThoiHanHopDong>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmThoiHanHopDong.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucThoiHanHopDong(HRM_dmThoiHanHopDong data, string idCurrentUser, ApplicationDbContext db = null)
        {
            HRM_dmThoiHanHopDong danhMuc = db.HRM_dmThoiHanHopDong.FirstOrDefault(r => r.Ma == data.Ma && r.IdGuid != data.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Mã đã tồn tại");
            }
            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmThoiHanHopDong.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucThoiHanHopDong: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucThoiHanHopDong(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmThoiHanHopDong> listDanhMuc = db.HRM_dmThoiHanHopDong.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucThoiHanHopDong: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion



        #region Hình thức trả lương

        public List<HRM_dmHinhThucTraLuong> GetDanhSachHinhThucTraLuong(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmHinhThucTraLuong>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmHinhThucTraLuong.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucHinhThucTraLuong(HRM_dmHinhThucTraLuong data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmHinhThucTraLuong.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmHinhThucTraLuong.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucHinhThucTraLuong: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucHinhThucTraLuong(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmHinhThucTraLuong> listDanhMuc = db.HRM_dmHinhThucTraLuong.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucHinhThucTraLuong: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Nhóm chứng chỉ

        public List<HRM_dmNhomChungChi> GetDanhSachNhomChungChi(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmNhomChungChi>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmNhomChungChi.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucNhomChungChi(HRM_dmNhomChungChi data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmNhomChungChi.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmNhomChungChi.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNhomChungChi: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucNhomChungChi(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmNhomChungChi> listDanhMuc = db.HRM_dmNhomChungChi.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucNhomChungChi: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion



        #region Cài đặt hệ thống

        public List<HRM_dmCaiDatHeThong> GetDanhSachCaiDatHeThong(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmCaiDatHeThong>();

            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            result = aContext.HRM_dmCaiDatHeThong.ToList();

            return result;
        }

        public MessageReturn SetDanhMucCaiDatHeThong(HRM_dmCaiDatHeThong aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới cài đặt hệ thống thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.Ma.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Vui lòng nhập dữ liệu mã cài đặt hệ thống");
            }

            var toDictId = aContext.HRM_dmCaiDatHeThong.ToList().ToDict(x => x.IdGuid);
            var toDictMa = aContext.HRM_dmCaiDatHeThong.ToList().ToDict(x => x.Ma);

            if (aModel.IdGuid.IsEmpty())
            {
                aModel.IdGuid = Guid.NewGuid().ToString();
                if (toDictMa.ContainsKey(aModel.Ma))
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu thêm mới có mã đã tồn tại trong hệ thống.Vui lòng chọn mã khác");
                }
                aContext.HRM_dmCaiDatHeThong.Add(aModel);
            }
            else
            {
                if (!toDictId.ContainsKey(aModel.Ma))
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
                }

                if (toDictId[aModel.IdGuid].Ma != aModel.Ma)
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không được thay đổi mã");
                }

                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu cài đặt hệ thống thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn DeleteCaiDatHeThong(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu danh mục cài đặt hệ thống thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id truyền nên");
            }

            var listDelete = aContext.HRM_dmCaiDatHeThong.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listDelete.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            aContext.HRM_dmCaiDatHeThong.RemoveRange(listDelete);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #region Tỉnh

        public EntityResponse<HRM_dmTinh> GetDanhSachTinh(string KeyWord, int CurrentPage = 0, int NumperPage = 25, ApplicationDbContext aContext = null)
        {
            var result = new EntityResponse<HRM_dmTinh>();
            var itemlist = aContext.HRM_dmTinh.OrderBy(x => x.SapXep).ToList();
            if (KeyWord.IsNotEmpty())
            {
                KeyWord = KeyWord.ToLower();
                itemlist = itemlist.Where(x => x.Ma.ToLower().Contains(KeyWord.ToLower().Trim()) ||
                x.Ten.ToLower().Contains(KeyWord.ToLower().Trim()) ||
               (!string.IsNullOrEmpty(x.GhiChu) && x.GhiChu.ToLower().Contains(KeyWord.ToLower().Trim()))).ToList();
            }
            if (CurrentPage == 0)
            {
                result.items = itemlist;
                return result;
            }
            var count = itemlist.Count();
            result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
            result.items = itemlist.Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucTinh(HRM_dmTinh data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmTinh.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTinh.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucTinh: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucTinh(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmTinh> listDanhMuc = db.HRM_dmTinh.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucTinh: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        public ErrorMessage ExportDanhMucTinh(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhMucTinh.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);

                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("DanhMucTinh.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("DanhMucTinh " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportDanhMucTinh(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                String[] ColumnNames = new String[] { "STT", "Mã", "Tên", "Ghi chú" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "TỈNH/ THÀNH PHỐ"))
                {
                    //err.SetLoi("File không đúng định dạng!" + err.);
                    return err;
                }
                if (!dungchung.KiemTraMaTrungExcel(sheet1, err))
                {
                    return err;
                }
                var ListDanhMuc = _dbContext.HRM_dmTinh.ToList();
                var ListAdd = new List<HRM_dmTinh>();
                var listMa = new List<string>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_dmTinh dm = new HRM_dmTinh();
                    dm.Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(dm.Ma))
                        break;
                    var check = ListDanhMuc.FirstOrDefault(a => a.Ma == dm.Ma);
                    if (check != null)
                        listMa.Add(dm.Ma);
                    dm.IdGuid = Guid.NewGuid().ToString();
                    dm.Ten = dungchung.GetCell(sheet1, row, 3);
                    dm.GhiChu = dungchung.GetCell(sheet1, row, 4);
                    ListAdd.Add(dm);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("Mã tỉnh đã tồn tại " + string.Join("", listMa));
                    return err;
                }

                _dbContext.HRM_dmTinh.AddRange(ListAdd);
                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThuVienDanhGia " + e.ToString());
            }
            return err;
        }

        #endregion

        #region Quận huyện

        public EntityResponse<DanhSachQuanHuyen> GetDanhSachQuanHuyen(string KeyWord, string idTinh = "", int CurrentPage = 0, int NumperPage = 25, ApplicationDbContext aContext = null)
        {
            var result = new EntityResponse<DanhSachQuanHuyen>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            var toDictTinh = aContext.HRM_dmTinh.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var listHuyen = aContext.HRM_dmQuanHuyen.AsNoTracking().Where(q => string.IsNullOrEmpty(idTinh) || q.IdTinh == idTinh).ToList();
            if (listHuyen.IsEmpty())
            {
                return result;
            }
            var listDanhSachHuyen = new List<DanhSachQuanHuyen>();
            foreach (var item in listHuyen.OrderBy(x => x.Ma))
            {
                var model = new DanhSachQuanHuyen();
                model.IdGuid = item.IdGuid;
                model.GhiChu = item.GhiChu;
                model.Ten = item.Ten;
                model.Tinh = item.IdTinh.GetValueFromDic(toDictTinh, x => x.Ten);
                model.Ma = item.Ma;
                model.IdTinh = item.IdTinh;
                listDanhSachHuyen.Add(model);
            }

            if (KeyWord.IsNotEmpty())
            {
                KeyWord = KeyWord.ToLower();
                //listDanhSachHuyen = listDanhSachHuyen.Where(x => x.Ma.ToLower().Contains(KeyWord) ||
                //x.Ten.ToLower().Contains(KeyWord) ||
                //x.Tinh.ToLower().Contains(KeyWord) ||
                //x.GhiChu.ToLower().Contains(KeyWord)).ToList();
                listDanhSachHuyen = listDanhSachHuyen.Where(x => x.Ma.ToLower().Contains(KeyWord.ToLower().Trim()) ||
                x.Ten.ToLower().Contains(KeyWord.ToLower().Trim()) ||
              (!string.IsNullOrEmpty(x.GhiChu) && x.GhiChu.ToLower().Contains(KeyWord.ToLower().Trim()))).ToList();
            }

            if (listDanhSachHuyen.IsEmpty())
            {
                return result;
            }

            if (CurrentPage == 0)
            {
                result.items = listDanhSachHuyen;
                return result;
            }

            var count = listDanhSachHuyen.Count();
            result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
            result.items = listDanhSachHuyen.Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
            return result;
        }

        public HRM_dmQuanHuyen GetDanhMucQuanHuyen(string Id, ApplicationDbContext aContext = null)
        {
            if (aContext.currentUser.Id.IsEmpty())
            {
                return new HRM_dmQuanHuyen();
            }
            var result = aContext.HRM_dmQuanHuyen.AsNoTracking().FirstOrDefault(x => x.IdGuid == Id);
            return result;
        }

        public MessageReturn UpdateDanhMucQuanHuyen(HRM_dmQuanHuyen data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmQuanHuyen.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");
            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmQuanHuyen.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucQuanHuyen: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucQuanHuyen(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmQuanHuyen> listDanhMuc = db.HRM_dmQuanHuyen.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucQuanHuyen: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        public ErrorMessage ExportDanhMucQuanHuyen(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhMucQuanHuyen.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);
                var ListDanhMuc = _dbContext.HRM_dmTinh.ToList();
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"];
                int nRow = 3;
                foreach (var item in ListDanhMuc)
                {
                    sheet1.Cells[nRow, 2].Value = item.Ma;
                    sheet1.Cells[nRow, 3].Value = item.Ten;
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("DanhMucQuanHuyen.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("DanhMucQuanHuyen " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportDanhMucQuanHuyen(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                String[] ColumnNames = new String[] { "STT", "Mã", "Tên", "Mã tỉnh", "Tên tỉnh", "Ghi chú" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "QUẬN/HUYỆN"))
                {
                    //err.SetLoi("File không đúng định dạng!" + err.);
                    return err;
                }
                if (!dungchung.KiemTraMaTrungExcel(sheet1, err))
                {
                    return err;
                }
                var ListTinh = _dbContext.HRM_dmTinh.ToList();
                var ListDanhMuc = _dbContext.HRM_dmQuanHuyen.ToList();
                var ListAdd = new List<HRM_dmQuanHuyen>();
                var listMa = new List<string>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_dmQuanHuyen dm = new HRM_dmQuanHuyen();
                    dm.Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(dm.Ma))
                        break;
                    var check = ListDanhMuc.FirstOrDefault(a => a.Ma == dm.Ma);
                    if (check != null)
                        listMa.Add(dm.Ma);
                    dm.IdGuid = Guid.NewGuid().ToString();
                    dm.Ten = dungchung.GetCell(sheet1, row, 3);
                    string MaTinh = dungchung.GetCell(sheet1, row, 4);
                    dm.IdTinh = ListTinh.Where(a => a.Ma == MaTinh).Select(a => a.IdGuid).FirstOrDefault();
                    dm.GhiChu = dungchung.GetCell(sheet1, row, 6);
                    ListAdd.Add(dm);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("Mã Quận/Huyện đã tồn tại " + string.Join("", listMa));
                    return err;
                }

                _dbContext.HRM_dmQuanHuyen.AddRange(ListAdd);
                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThuVienDanhGia " + e.ToString());
            }
            return err;
        }

        #endregion

        #region Phường xã

        public EntityResponse<DanhSachPhuongXa> GetDanhSachPhuongXa(string KeyWord, string IdQuanHuyen = "", int CurrentPage = 0, int NumperPage = 25, ApplicationDbContext aContext = null)
        {
            var result = new EntityResponse<DanhSachPhuongXa>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            var toDictTinh = aContext.HRM_dmTinh.AsNoTracking().ToList().ToDict(x => x.IdGuid);
            var toDictHuyen = aContext.HRM_dmQuanHuyen.AsNoTracking().ToList().ToDict(x => x.IdGuid);

            var listXa = aContext.HRM_dmPhuongXa.AsNoTracking().Where(q => string.IsNullOrEmpty(IdQuanHuyen) || q.IdQuanHuyen == IdQuanHuyen).ToList();
            if (listXa.IsEmpty())
            {
                return result;
            }

            var listDanhSachPhuongXa = new List<DanhSachPhuongXa>();
            foreach (var item in listXa)
            {
                var model = new DanhSachPhuongXa();
                model.IdGuid = item.IdGuid;
                model.Ma = item.Ma;
                model.Ten = item.Ten;
                model.GhiChu = item.GhiChu;
                model.IdQuanHuyen = item.IdQuanHuyen;
                model.QuanHuyen = model.IdQuanHuyen.GetValueFromDic(toDictHuyen, x => x.Ten);
                model.IdTinh = model.IdQuanHuyen.GetValueFromDic(toDictHuyen, x => x.IdTinh);
                model.Tinh = model.IdTinh.GetValueFromDic(toDictTinh, x => x.Ten);
                listDanhSachPhuongXa.Add(model);
            }

            if (KeyWord.IsNotEmpty())
            {
                KeyWord = KeyWord.ToLower();
                listDanhSachPhuongXa = listDanhSachPhuongXa.Where(x => x.Ma.ToLower().Contains(KeyWord.ToLower().Trim()) ||
                x.Ten.ToLower().Contains(KeyWord.ToLower().Trim()) ||
               (!string.IsNullOrEmpty(x.GhiChu) && x.GhiChu.ToLower().Contains(KeyWord.ToLower().Trim()))).ToList();
            }

            if (listDanhSachPhuongXa.IsEmpty())
            {
                return result;
            }


            if (CurrentPage == 0)
            {
                result.items = listDanhSachPhuongXa;
                return result;
            }

            var count = listDanhSachPhuongXa.Count();
            result.paging = new PagingInfor(CurrentPage, count, (int)count / NumperPage + (count % NumperPage > 0 ? 1 : 0));
            result.items = listDanhSachPhuongXa.Skip(NumperPage * (CurrentPage - 1)).Take(NumperPage).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucPhuongXa(HRM_dmPhuongXa data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmPhuongXa.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmPhuongXa.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucPhuongXa: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucPhuongXa(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmPhuongXa> listDanhMuc = db.HRM_dmPhuongXa.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucPhuongXa: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        public ErrorMessage ExportDanhMucPhuongXa(ApplicationDbContext _dbContext)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhMucPhuongXa.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);
                var ListDanhMuc = _dbContext.HRM_dmTinh.ToList();
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh Mục"];
                int nRow = 3;
                foreach (var item in ListDanhMuc)
                {
                    sheet1.Cells[nRow, 2].Value = item.Ma;
                    sheet1.Cells[nRow, 3].Value = item.Ten;
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("DanhMucPhuongXa.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("DanhMucPhuongXa " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportDanhMucPhuongXa(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();

                var listColumnName = new List<string>();
                String[] ColumnNames = new String[] { "STT", "Mã", "Tên", "Mã Quận/Huyện", "Tên Quận/Huyện", "Ghi chú" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "Phường/Xã"))
                {
                    //err.SetLoi("File không đúng định dạng!" + err.);
                    return err;
                }
                if (!dungchung.KiemTraMaTrungExcel(sheet1, err))
                {
                    return err;
                }
                var ListTinh = _dbContext.HRM_dmQuanHuyen.ToList();
                var ListDanhMuc = _dbContext.HRM_dmPhuongXa.ToList();
                var ListAdd = new List<HRM_dmPhuongXa>();
                var listMa = new List<string>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_dmPhuongXa dm = new HRM_dmPhuongXa();
                    dm.Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(dm.Ma))
                        break;
                    var check = ListDanhMuc.FirstOrDefault(a => a.Ma == dm.Ma);
                    if (check != null)
                        listMa.Add(dm.Ma);
                    dm.IdGuid = Guid.NewGuid().ToString();
                    dm.Ten = dungchung.GetCell(sheet1, row, 3);
                    string MaTinh = dungchung.GetCell(sheet1, row, 4);
                    dm.IdQuanHuyen = ListTinh.Where(a => a.Ma == MaTinh).Select(a => a.IdGuid).FirstOrDefault();
                    dm.GhiChu = dungchung.GetCell(sheet1, row, 6);
                    ListAdd.Add(dm);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("Mã Phường/Xã đã tồn tại " + string.Join("", listMa));
                    return err;
                }

                _dbContext.HRM_dmPhuongXa.AddRange(ListAdd);
                _dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThuVienDanhGia " + e.ToString());
            }
            return err;
        }

        #endregion


        #region Loại tính thuế chuyên gia

        public List<HRM_dmLoaiTinhThueChuyenGia> GetDanhSachLoaiTinhThueChuyenGia(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmLoaiTinhThueChuyenGia>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmLoaiTinhThueChuyenGia.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucLoaiTinhThueChuyenGia(HRM_dmLoaiTinhThueChuyenGia data, string idCurrentUser, ApplicationDbContext db = null)
        {

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmLoaiTinhThueChuyenGia.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucLoaiTinhThueChuyenGia: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucLoaiTinhThueChuyenGia(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_dmLoaiTinhThueChuyenGia> listDanhMuc = db.HRM_dmLoaiTinhThueChuyenGia.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucLoaiTinhThueChuyenGia: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục đóng thuế TNCN

        public List<HRM_dmMucDongThueTNCN> GetDanhSachDanhMucDongThueTNCN(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmMucDongThueTNCN>();

            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }

            result = aContext.HRM_dmMucDongThueTNCN.ToList();

            return result;
        }

        public MessageReturn SetDanhMucDongThueTNCN(HRM_dmMucDongThueTNCN aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục đóng thuế TNCN thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.IdGuid.IsEmpty())
            {
                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmMucDongThueTNCN.Add(aModel);
            }
            else
            {
                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật danh mục đóng thuế TNCN thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn DeleteDanhMucDongThueTNCN(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa danh mục đóng thuế TNCN thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id truyền nên");
            }

            var listModel = aContext.HRM_dmMucDongThueTNCN.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listModel.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            aContext.HRM_dmMucDongThueTNCN.RemoveRange(listModel);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #region Danh mục file đình kém hợp đồng

        public List<HRM_HoSoDinhKem> GetDanhSachHoSoDinhKem(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_HoSoDinhKem>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_HoSoDinhKem.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucHoSoDinhKem(HRM_HoSoDinhKem data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_HoSoDinhKem.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_HoSoDinhKem.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucHoSoDinhKem: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucHoSoDinhKem(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            int Check = db.HRM_HopDongHoSoDinhKem.Where(q => listId.Contains(q.IdHoSoDinhKem)).Count();
            if (Check > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi mã hồ sơ đã được sử dụng");


            List<HRM_HoSoDinhKem> listDanhMuc = db.HRM_HoSoDinhKem.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucTinh: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục tài sản bàn giao

        public List<HRM_dmTaiSanBanGiao> GetDanhMucTaiSanBanGiao(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmTaiSanBanGiao>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmTaiSanBanGiao.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucTaiSanBanGiao(HRM_dmTaiSanBanGiao data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmTaiSanBanGiao.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTaiSanBanGiao.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucTaiSanBanGiao: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucTaiSanBanGiao(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            int Check = db.HRM_HopDongTaiSanBanGiao.Where(q => listId.Contains(q.IdTaiSanBanGiao)).Count();
            if (Check > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi mã tài sản đã được sử dụng");


            List<HRM_dmTaiSanBanGiao> listDanhMuc = db.HRM_dmTaiSanBanGiao.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucTinh: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục phụ cấp

        public List<HRM_dmPhuCap> GetDanhMucPhuCap(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmPhuCap>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucPhuCap(HRM_dmPhuCap data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmPhuCap.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmPhuCap.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucPhuCap: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucPhuCap(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {
            int Check = db.HRM_HopDongPhuCap.Where(q => listId.Contains(q.IddmLoaiPhuCap)).Count();
            if (Check > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi mã tài sản đã được sử dụng");


            List<HRM_dmPhuCap> listDanhMuc = db.HRM_dmPhuCap.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucPhuCap: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục nhóm lương

        public List<HRM_dmNhomLuong> GetDanhNhomLuong(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmNhomLuong>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmNhomLuong.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdateDanhMucNhomLuong(HRM_dmNhomLuong data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmNhomLuong.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmNhomLuong.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNhomLuong: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucNhomLuong(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {


            List<HRM_dmNhomLuong> listDanhMuc = db.HRM_dmNhomLuong.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucNhomLuongp: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region Danh mục nhóm phúc lợi

        public List<HRM_dmNhomPhucLoi> GetDanhSachDanhMucNhomPhucLoi(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmNhomPhucLoi>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmNhomPhucLoi.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn SetDanhMucNhomPhucLoi(HRM_dmNhomPhucLoi aModel, ApplicationDbContext aContext = null)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục nhóm phúc lợi thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.Ma.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Vui lòng nhập mã");
            }

            var toDictId = aContext.HRM_dmNhomPhucLoi.ToList().ToDict(x => x.IdGuid);
            var toDictMa = aContext.HRM_dmNhomPhucLoi.ToList().ToDict(x => x.Ma);

            if (aModel.IdGuid.IsEmpty())
            {
                if (toDictMa.ContainsKey(aModel.Ma))
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Mã đã bị trùng. Vui lòng nhập mã khác");
                }

                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmNhomPhucLoi.Add(aModel);
            }
            else
            {
                if (toDictId.ContainsKey(aModel.IdGuid))
                {
                    if (toDictId[aModel.IdGuid].Ma != aModel.Ma)
                    {
                        return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không được thay đổi mã");
                    }
                }
                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu danh mục nhóm phúc lợi thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public MessageReturn DeleteDanhMucNhomPhucLoi(List<string> aListId, ApplicationDbContext aContext = null)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa thành công danh mục nhóm phúc lợi");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu Id truyền nên");
            }

            var listModel = aContext.HRM_dmNhomPhucLoi.Where(X => aListId.Contains(X.IdGuid)).ToList();
            if (listModel.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
            }

            aContext.HRM_dmNhomPhucLoi.RemoveRange(listModel);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #region Danh mục trạng thái Hợp đồng

        public List<HRM_dmTrangThaiHopDong> GetDanhMucTrangThaiHopDong(ApplicationDbContext aContext = null)
        {
            var result = new List<HRM_dmTrangThaiHopDong>();
            if (aContext.currentUser.Id.IsEmpty())
            {
                return result;
            }
            result = aContext.HRM_dmTrangThaiHopDong.OrderBy(x => x.SapXep).ToList();
            return result;
        }

        public MessageReturn UpdatetDanhMucTrangThaiHopDong(HRM_dmTrangThaiHopDong data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var checktrung = db.HRM_dmTrangThaiHopDong.Where(q => q.Ma == data.Ma && q.IdGuid != data.IdGuid).Count();
            if (checktrung > 0)
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Lỗi trùng mã");

            if (string.IsNullOrEmpty(data.IdGuid))
                db.HRM_dmTrangThaiHopDong.Add(data);
            else
                db.Entry(data).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucNhomLuong: {0} {1}", data.IdGuid, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeletetDanhMucTrangThaiHopDong(List<string> listId, string idCurrentUser, ApplicationDbContext db = null)
        {


            List<HRM_dmTrangThaiHopDong> listDanhMuc = db.HRM_dmTrangThaiHopDong.Where(q => listId.Contains(q.IdGuid)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DeleteDanhMucNhomLuongp: {0} {1}", JsonConvert.SerializeObject(listId), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        #endregion

        #region danh mục cơ sở

        //public List<BoPhan> GetListdmCoSo(ApplicationDbContext aContext)
        //{
        //    var result = new List<HRM.Api.Models.SmartEOEModel.BoPhan>();
        //    if (aContext.currentUser.Id.IsEmpty())
        //    {
        //        return result;
        //    }
        //    result = dbSmartEOS.BoPhans.Where(x => x.idDuAn > 0).OrderBy(x => x.Order).ToList();
        //    return result;
        //}

        #region Danh mục chức vụ công việc

        public MessageReturn SetDanhMucChucVuCongViec(HRM_dmChucVuCongViec aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục chức vụ công việc thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.IdGuid.IsEmpty())
            {
                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmChucVuCongViec.Add(aModel);
            }
            else
            {
                var model = aContext.HRM_dmChucVuCongViec.FirstOrDefault(x => x.IdGuid == aModel.IdGuid);
                if (model == null)
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
                }
                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật chức vụ công việc thành công");
            }

            aContext.SaveChanges();

            return result;
        }

        public List<HRM_dmChucVuCongViec> GetListDanhMucChucVuCongViec(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmChucVuCongViec>();

            var listModel = aContext.HRM_dmChucVuCongViec.ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            //   var toDictChucVu = aContext.HRM_dmChucVu.ToList().ToDict(x => x.IdGuid);
            result = listModel;
            //foreach (var item in listModel)
            //{
            //  //  item.TenChucVu = item.IdChucVu.GetValueFromDic(toDictChucVu, x => x.Ten);
            //    result.Add(item);
            //}

            return result;
        }

        public MessageReturn DeleteDanhMucChucVuCongViec(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu chức vụ công việc thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listDelete = aContext.HRM_dmChucVuCongViec.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listDelete.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            aContext.HRM_dmChucVuCongViec.RemoveRange(listDelete);
            aContext.SaveChanges();

            return result;
        }

        #endregion



        #region Danh mục trang thiết bị

        public MessageReturn SetDanhMucTrangThietBi(HRM_dmTrangThietBi aModel, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục trang thiết bị thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aModel == null)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu");
            }

            if (aModel.IdGuid.IsEmpty())
            {
                aModel.IdGuid = Guid.NewGuid().ToString();
                aContext.HRM_dmTrangThietBi.Add(aModel);
            }
            else
            {
                var model = aContext.HRM_dmTrangThietBi.AsNoTracking().FirstOrDefault(x => x.IdGuid == aModel.IdGuid);
                if (model == null)
                {
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Dữ liệu đã bị thay đổi hoặc đã bị xóa");
                }
                aContext.Entry(aModel).State = EntityState.Modified;
                result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu trang thiết bị thành công");
            }
            aContext.SaveChanges();

            return result;
        }

        public List<HRM_dmTrangThietBi> GetListDanhMucTrangThietBi(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmTrangThietBi>();

            var listModel = aContext.HRM_dmTrangThietBi.ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            return listModel;
        }

        public MessageReturn DeleteDanhMucTrangThietBi(List<string> aListId, ApplicationDbContext aContext)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu trang thiết bị thành công");

            if (aContext.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (aListId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listDelete = aContext.HRM_dmTrangThietBi.Where(x => aListId.Contains(x.IdGuid)).ToList();
            if (listDelete.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            aContext.HRM_dmTrangThietBi.RemoveRange(listDelete);
            aContext.SaveChanges();

            return result;
        }

        #endregion

        #endregion
        #region Danh mục noi dung đao tạo

        public MessageReturn SetdmNoiDungDaoTao(HRM_dmNoiDungDaoTao model, ApplicationDbContext db)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục thành công");

            HRM_dmNoiDungDaoTao danhMuc = db.HRM_dmNoiDungDaoTao.FirstOrDefault(r => r.Ma == model.Ma && r.IdGuid != model.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Mã đã tồn tại");
            }

            if (model.IdGuid.IsEmpty())
            {
                model.IdGuid = Guid.NewGuid().ToString();
                db.HRM_dmNoiDungDaoTao.Add(model);
            }
            else
            {
                var item = db.HRM_dmTrangThietBi.FirstOrDefault(x => x.IdGuid == model.IdGuid);

                db.Entry(model).State = EntityState.Modified;

            }
            db.SaveChanges();

            return result;
        }

        public List<HRM_dmNoiDungDaoTao> GetListdmNoiDungDaoTao(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmNoiDungDaoTao>();

            var listModel = aContext.HRM_dmNoiDungDaoTao.ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            return listModel;
        }

        public MessageReturn DeletedmNoiDungDaoTao(string Id, ApplicationDbContext db)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");

            if (db.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listDelete = db.HRM_dmNoiDungDaoTao.Where(x => x.IdGuid == Id).ToList();
            if (listDelete.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            db.HRM_dmNoiDungDaoTao.RemoveRange(listDelete);
            db.SaveChanges();

            return result;
        }

        #endregion

        #region Danh mục hình thức

        public MessageReturn SetdmHinhThucDaoTao(HRM_dmHinhThucDaoTao model, ApplicationDbContext db)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm mới danh mục thành công");

            HRM_dmHinhThucDaoTao danhMuc = db.HRM_dmHinhThucDaoTao.FirstOrDefault(r => r.Ma == model.Ma && r.IdGuid != model.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Mã đã tồn tại");
            }

            if (model.IdGuid.IsEmpty())
            {
                model.IdGuid = Guid.NewGuid().ToString();
                db.HRM_dmHinhThucDaoTao.Add(model);
            }
            else
            {
                var item = db.HRM_dmHinhThucDaoTao.FirstOrDefault(x => x.IdGuid == model.IdGuid);

                db.Entry(model).State = EntityState.Modified;

            }
            db.SaveChanges();

            return result;
        }

        public List<HRM_dmHinhThucDaoTao> GetListdmHinhThucDaoTao(ApplicationDbContext aContext)
        {
            var result = new List<HRM_dmHinhThucDaoTao>();

            var listModel = aContext.HRM_dmHinhThucDaoTao.ToList();
            if (listModel.IsEmpty())
            {
                return result;
            }

            return listModel;
        }

        public MessageReturn DeletedmHinhThucDaoTao(List<string> listId, ApplicationDbContext db)
        {
            var result = new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");

            if (db.currentUser.Id.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.ChuaDangNhap, "Không tìm thấy dữ liệu người dùng");
            }

            if (listId.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            var listDelete = db.HRM_dmHinhThucDaoTao.Where(x => listId.Contains(x.IdGuid)).ToList();
            if (listDelete.IsEmpty())
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu để xóa");
            }

            db.HRM_dmHinhThucDaoTao.RemoveRange(listDelete);
            db.SaveChanges();

            return result;
        }

        #endregion
        #region TieuChi

        public dynamic GetListTieuChi(ApplicationDbContext db, dynamic dnm)
        {
            if (dnm == null)
            {
                List<HRM_dmTieuChi> itemList = db.HRM_dmTieuChi.ToList();
                List<HRM_dmTieuChi_CauHoi> listCauHoi = db.HRM_dmTieuChi_CauHoi.ToList();
                foreach (HRM_dmTieuChi item in itemList)
                {
                    item.listCauHoi = listCauHoi.FindAll(x => x.IDTieuChi == item.IdGuid); ;
                }
                return itemList;
            }
            else
            {

                TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
                int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
                int PageSize = itemTimKiem.PageSize ?? 0;
                IQueryable<HRM_dmTieuChi> result = (from a in db.HRM_dmTieuChi
                                                     orderby a.Created descending
                                                     select a);
                if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                    result = result.Where(a => a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()));
                List<HRM_dmTieuChi_CauHoi> listCauHoi = db.HRM_dmTieuChi_CauHoi.ToList();
                var listItem = result.ToList();
                foreach (HRM_dmTieuChi item in listItem)
                {
                    List<HRM_dmTieuChi_CauHoi> CauHois = listCauHoi.FindAll(x => x.IDTieuChi == item.IdGuid);
                    item.listCauHoi = CauHois;
                }
                if (nCurrentPage == 0)
                    return listItem;
                int nSum = listItem.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                dynamic data = new DynamicObject();
                data.paging = paging;
                data.items = listItem;
                return data;
            }
        }

        public dynamic SetTieuChi(ApplicationDbContext db, HRM_dmTieuChi item)
        {

            try
            {

                if (item.IdGuid == null || item.IdGuid == "")
                {
                    item.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_dmTieuChi.Add(item);

                    List<HRM_dmTieuChi_CauHoi> cauHois = item.listCauHoi;
                    foreach (HRM_dmTieuChi_CauHoi CauHoi in cauHois)
                    {
                        CauHoi.IDTieuChi = item.IdGuid;
                        db.HRM_dmTieuChi_CauHoi.Add(CauHoi);

                    }
                }
                else
                {
                    db.Entry(item).State = EntityState.Modified;
                    List<HRM_dmTieuChi_CauHoi> cauHois = item.listCauHoi;
                    foreach (HRM_dmTieuChi_CauHoi CauHoi in cauHois)
                    {
                        if (CauHoi.isXoa == true)
                        {
                            if (CauHoi.IdGuid != null && CauHoi.IdGuid != "")
                                db.Entry(CauHoi).State = EntityState.Deleted;
                        }
                        else
                        {
                            CauHoi.IDTieuChi = item.IdGuid;
                            if (CauHoi.IdGuid != null && CauHoi.IdGuid != "")
                                db.Entry(CauHoi).State = EntityState.Modified;
                            else
                            {
                                CauHoi.IdGuid = Guid.NewGuid().ToString();
                                db.HRM_dmTieuChi_CauHoi.Add(CauHoi);
                            }
                        }

                    }
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công", item);
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Cập nhật không thành công " + e.Message);
            }
        }

        public dynamic DeleteTieuChi(ApplicationDbContext db, string Id)
        {

            try
            {
                HRM_dmTieuChi item = db.HRM_dmTieuChi.Find(Id);
                if (item.IdGuid != null || item.IdGuid != "")
                {
                    List<HRM_dmTieuChi_CauHoi> listCauHoi = db.HRM_dmTieuChi_CauHoi.Where(x => x.IDTieuChi == item.IdGuid).ToList();
                    foreach (HRM_dmTieuChi_CauHoi cauHoi in listCauHoi)
                    {
                        db.Entry(cauHoi).State = EntityState.Deleted;
                    }
                    db.Entry(item).State = EntityState.Deleted;
                }
                db.SaveChanges();
                return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu thành công");
            }
            catch (Exception e)
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, e.ToString());
            }

        }

        #endregion

        #region Danh mục đợt chấm công
        public dynamic GetListDanhMucDotChamCong(ApplicationDbContext db)
        {
            var lstResult = db.HRM_dmDotChamCongThang.AsNoTracking().OrderBy(q => q.Created).ToList();
            return lstResult;
        }
        #endregion

        #region Danh mục nhân viên thực hiện chấm công

        public dynamic UpdateUserThucHienChamCong(EntityHRM_DanhMuc_NhanVien_ChamCongBoPhan data, ApplicationDbContext db)
        {
            #region data
            //{EntityHRM_DanhMuc_NhanVien_ChamCongBoPhan}
            #endregion
            //xóa dữ liệu cũ
            var lstOld = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.AsNoTracking().Where(q => q.IdUserNhanVien == data.IdUserNhanVien && q.IdCongTyEOS == data.IdCongTyEOS).ToList();
            db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.RemoveRange(lstOld);
            db.SaveChanges();

            var lstNew = new List<HRM_DanhMuc_NhanVien_ChamCongBoPhan>();
            foreach (var IdBoPhanEOS in data.lstIdBoPhanEOS)
            {
                HRM_DanhMuc_NhanVien_ChamCongBoPhan newitem = new HRM_DanhMuc_NhanVien_ChamCongBoPhan();
                newitem.IdCongTyEOS = data.IdCongTyEOS;
                newitem.IdUserNhanVien = data.IdUserNhanVien;
                newitem.IdBoPhanEOS = IdBoPhanEOS;
                lstNew.Add(newitem);
            }
            db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.AddRange(lstNew);
            db.SaveChanges();
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }
        #endregion


        public dynamic SetDanhMucCauHinhChamCongHRM(HRM_DanhMuc_NhanVien_ChamCongBoPhan model, ApplicationDbContext db)
        {

            HRM_DanhMuc_NhanVien_ChamCongBoPhan NhanVienChamCong = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.FirstOrDefault(r => r.IdBoPhanEOS == model.IdBoPhanEOS);
            if (NhanVienChamCong != null)
            {
                db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Remove(NhanVienChamCong);
            }
            model.IdGuid = Guid.NewGuid().ToString();
            db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Add(model);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        public dynamic GetDanhMucCauHinhChamCongHRM(long IdCongTyEOS, ApplicationDbContext db)
        {
            List<HRM_DanhMuc_NhanVien_ChamCongBoPhan> NhanVienChamCongs = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Where(r => r.IdCongTyEOS == IdCongTyEOS).ToList();
            var listNhanVienEOS = _CallWebService.GetDanhSachNhanSuTheoBoPhan(db, IdCongTyEOS);
            foreach (var item in NhanVienChamCongs)
            {
                var itemNhanVien = listNhanVienEOS.Find(r => r.UserPkid == item.IdUserNhanVien);
                if (itemNhanVien != null)
                {
                    item.TenNhanVien = itemNhanVien.TenNhanVien;
                    item.TenBoPhan = itemNhanVien.TenBoPhan;
                }
            }
            return NhanVienChamCongs;
        }

        public dynamic DeleteDanhMucCauHinhChamCongHRM(string Id, ApplicationDbContext db)
        {
            HRM_DanhMuc_NhanVien_ChamCongBoPhan NhanVienChamCong = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Remove(NhanVienChamCong);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        public dynamic GetDanhSachBoPhanChamCongHRMByUser(long IdCongTyEOS, ApplicationDbContext db)
        {
            List<HRM_DanhMuc_NhanVien_ChamCongBoPhan> NhanVienChamCongs = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Where(r => r.IdCongTyEOS == IdCongTyEOS && r.IdUserNhanVien == db.IdUser).ToList();
            List<long> IdBoPhan = NhanVienChamCongs.Select(r => r.IdBoPhanEOS).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, IdBoPhan);
            return listBoPhan;

        }
        public dynamic GetDanhSachBoPhanChamCongByUser(long IdCongTyEOS, string IdUser, ApplicationDbContext db)
        {
            List<HRM_DanhMuc_NhanVien_ChamCongBoPhan> NhanVienChamCongs = db.HRM_DanhMuc_NhanVien_ChamCongBoPhan.Where(r => r.IdCongTyEOS == IdCongTyEOS && r.IdUserNhanVien == IdUser).ToList();
            List<long> IdBoPhan = NhanVienChamCongs.Select(r => r.IdBoPhanEOS).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(db, IdBoPhan);
            return listBoPhan;

        }

        #region danh mục thiết lập ký hiệu chấm công
        public dynamic GetListdmKyHieuChamCong(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            //string sSql = "";
            //string sWhere = "";
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmKyHieuChamCong
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (itemTimKiem.KeyWord.Contains(a.Ten) || (itemTimKiem.KeyWord.Contains(a.Ma)))))
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic DeletedmKyHieuChamCong(string Id, ApplicationDbContext db)
        {
            HRM_dmKyHieuChamCong NhanVienChamCong = db.HRM_dmKyHieuChamCong.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Xóa dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmKyHieuChamCong.Remove(NhanVienChamCong);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");
        }
        public dynamic SetdmKyHieuChamCong(HRM_dmKyHieuChamCong entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmKyHieuChamCong danhMuc = _dbContext.HRM_dmKyHieuChamCong.FirstOrDefault(r => r.Ma == entity.Ma && r.IdGuid != entity.IdGuid);
            if (danhMuc != null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Đã tồn tại mã ký hiệu");
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmKyHieuChamCong.Add(entity);
                _dbContext.SaveChanges();
                return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu  thành công", entity);


            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        #endregion
        #region danh mục thiết lập lịch nghỉ lể
        public dynamic GetListdmLichNghiLe(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            //string sSql = "";
            //string sWhere = "";
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmLichNghiLe
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && itemTimKiem.KeyWord.Contains(a.Ten)))
                         orderby a.isApDung, a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var lisDuAn = _CallWebService.GetDanhSachDuAn(db);
            foreach (var item in listItem)
            {
                item.TenCongTy = lisDuAn.Where(a => a.Id == item.IdCongTy).Select(a => a.TenBoPhan).FirstOrDefault();
            }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic DeletedmLichNghiLe(string Id, ApplicationDbContext db)
        {
            HRM_dmLichNghiLe NhanVienChamCong = db.HRM_dmLichNghiLe.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Xóa dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmLichNghiLe.Remove(NhanVienChamCong);
            List<HRM_dmLichNghiLe_Ngay> listItem = db.HRM_dmLichNghiLe_Ngay.Where(a => a.IdLichNghiLe == NhanVienChamCong.IdGuid).ToList();
            if (listItem != null && listItem.Count > 0)
                db.HRM_dmLichNghiLe_Ngay.RemoveRange(listItem);
            List<HRM_dmLichNghiLe_BoPhan> listBP = db.HRM_dmLichNghiLe_BoPhan.Where(a => a.IdLichNghiLe == NhanVienChamCong.IdGuid).ToList();
            if (listItem != null && listItem.Count > 0)
                db.HRM_dmLichNghiLe_BoPhan.RemoveRange(listBP);

            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");
        }
        public dynamic GetdmLichNghiLe(string Id, ApplicationDbContext db)
        {
            HRM_dmLichNghiLe NhanVienChamCong = db.HRM_dmLichNghiLe.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Lấy dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            var listBoPhan = db.HRM_dmLichNghiLe_BoPhan.Where(a => a.IdLichNghiLe == Id).ToList();
            NhanVienChamCong.listIdBoPhan = new List<long>();
            if (listBoPhan != null && listBoPhan.Count > 0)
            {
                NhanVienChamCong.listIdBoPhan = listBoPhan.Select(a => a.IdBoPhan).Distinct().ToList();
            }
            if (!string.IsNullOrEmpty(NhanVienChamCong.IdChucVu))
                NhanVienChamCong.listIdChucVu = NhanVienChamCong.IdChucVu.Split(',').Select(a=>long.Parse(a)).ToList();
            NhanVienChamCong.listNgay = db.HRM_dmLichNghiLe_Ngay.Where(a => a.IdLichNghiLe == NhanVienChamCong.IdGuid).ToList();
            if (NhanVienChamCong.listNgay != null && NhanVienChamCong.listNgay.Count > 0)
                foreach (var item in NhanVienChamCong.listNgay)
                {
                    if (item.TuNgay != null)
                        item.TuNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.TuNgay);
                    if (item.DenNgay != null)
                        item.DenNgayUnix = dungchung.DateTimeToUnixTimeStamp(item.DenNgay);

                }

            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "lấy dữ liệu  thành công", NhanVienChamCong);
        }

        public dynamic SetdmLichNghiLe(HRM_dmLichNghiLe entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmLichNghiLe entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmLichNghiLe)) as HRM_dmLichNghiLe;
            try
            {
                HRM_dmLichNghiLe entitydb = _dbContext.HRM_dmLichNghiLe.Where(a => a.IdCongTy == entity.IdCongTy && a.Nam == entity.Nam && a.IdGuid != entity.IdGuid).FirstOrDefault();
                if(entitydb!=null && Host=="PHT")
                {
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Đã thiết lập ngày nghỉ cho đơn vị");
                }    

                if (string.IsNullOrEmpty(entity.IdGuid))
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmLichNghiLe.Add(entity);

                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;

                List<HRM_dmLichNghiLe_Ngay> listItem = _dbContext.HRM_dmLichNghiLe_Ngay.Where(a => a.IdLichNghiLe == entity.IdGuid).ToList();
                if (listItem != null && listItem.Count > 0)
                    _dbContext.HRM_dmLichNghiLe_Ngay.RemoveRange(listItem);
                List<HRM_dmLichNghiLe_BoPhan> listBp = _dbContext.HRM_dmLichNghiLe_BoPhan.Where(a => a.IdLichNghiLe == entity.IdGuid).ToList();
                if (listBp != null && listBp.Count > 0)
                    _dbContext.HRM_dmLichNghiLe_BoPhan.RemoveRange(listBp);

                if (entity.listIdChucVu != null && entity.listIdChucVu.Count > 0)
                {
                    entity.IdChucVu = string.Join(",", entity.listIdChucVu);
                }    
                if (entity.listIdBoPhan != null && entity.listIdBoPhan.Count > 0)
                {
                    //var check = _dbContext.HRM_dmLichNghiLe_BoPhan.Where(a => a.IdLichNghiLe != entity.IdGuid && entity.listIdBoPhan.Contains(a.IdBoPhan)).ToList();
                    //if (check != null && check.Count > 0)
                    //{
                    //    List<long> listIdBoPhan = check.Select(a => a.IdBoPhan).Distinct().ToList();
                    //    var listBoPhan = _CallWebService.GetDanhSachBoPhanFromListId(_dbContext, listIdBoPhan);
                    //    string TenBoPhan = string.Join(", ", listBoPhan.Select(a => a.TenBoPhan));
                    //    return new MessageReturn(eErrorMessage.LoiCapNhat, "Bộ Phận : " + TenBoPhan + " đã được sử dụng ở lịch nghỉ lễ khác");

                    //}
                    List<HRM_dmLichNghiLe_BoPhan> listAdd = new List<HRM_dmLichNghiLe_BoPhan>();
                    foreach (var item in entity.listIdBoPhan)
                    {
                        HRM_dmLichNghiLe_BoPhan itemAdd = new HRM_dmLichNghiLe_BoPhan();
                        itemAdd.IdGuid = Guid.NewGuid().ToString();
                        itemAdd.IdBoPhan = item;
                        itemAdd.IdLichNghiLe = entity.IdGuid;
                        itemAdd.IdCongTy = entity.IdCongTy;
                        itemAdd.Nam = entity.Nam;
                        listAdd.Add(itemAdd);
                        _dbContext.HRM_dmLichNghiLe_BoPhan.AddRange(listAdd);
                    }
                }

                if (entity.listNgay != null && entity.listNgay.Count > 0)
                {
                    foreach (var item in entity.listNgay)
                    {
                        if (item.TuNgayUnix > 0)
                            item.TuNgay = dungchung.UnixTimeStampToDateTime(item.TuNgayUnix);
                        if (item.DenNgayUnix > 0)
                            item.DenNgay = dungchung.UnixTimeStampToDateTime(item.DenNgayUnix);
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdLichNghiLe = entity.IdGuid;
                        item.IdCongTy = entity.IdCongTy;
                        item.Nam = entity.Nam;
                    }
                    _dbContext.HRM_dmLichNghiLe_Ngay.AddRange(entity.listNgay);
                }
                //if (entity.isApDung == true)
                //{
                //    HRM_dmLichNghiLe entityDb = _dbContext.HRM_dmLichNghiLe.FirstOrDefault(a => a.isApDung == true && a.IdGuid != entity.IdGuid);
                //    if (entityDb != null)
                //    {
                //        entityDb.isApDung = false;
                //        _dbContext.Entry(entityDb).State = EntityState.Modified;
                //    }
                //}
                _dbContext.SaveChanges();
                return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        #endregion
        #region danh mục thiết lập Ngày làm việc
        public dynamic GetListdmLichLamViec(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            //string sSql = "";
            //string sWhere = "";
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            List<HRM_dmLichLamViec> result = (from a in db.HRM_dmLichLamViec

                                              orderby a.isApDung descending, a.Created descending
                                              select a).ToList();
            if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                result = result.Where(a => a.Ma.ToLower().Trim().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ten.ToLower().Trim().Contains(itemTimKiem.KeyWord.ToLower().Trim())).ToList();
            if (itemTimKiem.IdCongTy > 0)
            {
                var ThongTinDuAn = _CallWebService.GetThongTinCaLamViecDuAn(itemTimKiem.IdCongTy, db);
                if (ThongTinDuAn != null && ThongTinDuAn.itemThongTinDuAn != null && ThongTinDuAn.itemThongTinDuAn.listCaLamViec != null && ThongTinDuAn.itemThongTinDuAn.listCaLamViec.Count > 0)
                {
                    var listId = ThongTinDuAn.itemThongTinDuAn.listCaLamViec.Select(a => a.IddmCaLamViec).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid)).ToList();
                }
            }
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic DeletedmLichLamViec(string Id, ApplicationDbContext db)
        {
            HRM_dmLichLamViec NhanVienChamCong = db.HRM_dmLichLamViec.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Xóa dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmLichLamViec.Remove(NhanVienChamCong);
            List<HRM_dmLichLamViec_Ngay> listItemN = db.HRM_dmLichLamViec_Ngay.Where(a => a.IdLichLamViec == NhanVienChamCong.IdGuid).ToList();
            List<HRM_dmLichLamViec_Gio> listItemG = db.HRM_dmLichLamViec_Gio.Where(a => a.IdLichLamViec == NhanVienChamCong.IdGuid).ToList();
            if (listItemN != null && listItemN.Count > 0)
                db.HRM_dmLichLamViec_Ngay.RemoveRange(listItemN);
            if (listItemG != null && listItemG.Count > 0)
                db.HRM_dmLichLamViec_Gio.RemoveRange(listItemG);

            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");
        }
        public dynamic GetdmLichLamViecByListId(List<string> Id, ApplicationDbContext db)
        {
            var listGio = db.HRM_dmLichLamViec.Where(r => Id.Contains(r.IdGuid)).ToList();
            if (listGio == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Lấy dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            foreach (var item in listGio)
            {
                if (item.TuGio != null)
                    item.TuGioUnix = dungchung.DateTimeToUnixTimeStamp(item.TuGio);
                if (item.DenGio != null)
                    item.DenGioUnix = dungchung.DateTimeToUnixTimeStamp(item.DenGio);
            }
            return listGio;
        }
        public dynamic GetdmLichLamViec(string Id, ApplicationDbContext db)
        {
            HRM_dmLichLamViec NhanVienChamCong = db.HRM_dmLichLamViec.FirstOrDefault(r => r.IdGuid == Id);
            if (NhanVienChamCong == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Lấy dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            if (NhanVienChamCong.TuGio != null)
                NhanVienChamCong.TuGioUnix = dungchung.DateTimeToUnixTimeStamp(NhanVienChamCong.TuGio);
            if (NhanVienChamCong.DenGio != null)
                NhanVienChamCong.DenGioUnix = dungchung.DateTimeToUnixTimeStamp(NhanVienChamCong.DenGio);
            NhanVienChamCong.listNgay = db.HRM_dmLichLamViec_Ngay.Where(a => a.IdLichLamViec == NhanVienChamCong.IdGuid).OrderBy(a => a.Thu).ToList();
            List<HRM_dmLichLamViec_Gio> listItemG = db.HRM_dmLichLamViec_Gio.Where(a => a.IdLichLamViec == NhanVienChamCong.IdGuid).ToList();
            if (listItemG != null && listItemG.Count > 0)
                foreach (var item in NhanVienChamCong.listNgay)
                {
                    item.tenThu = DefineData.QuyDoiThu(item.Thu ?? 0);
                    item.listGio = new List<HRM_dmLichLamViec_Gio>();
                    item.listGio = listItemG.Where(a => a.IdLichLamViecNgay == item.IdGuid).ToList();
                    if (item.listGio != null && item.listGio.Count > 0)
                    {
                        foreach (var Child in item.listGio)
                        {
                            if (Child.TuGio != null)
                                Child.TuGioUnix = dungchung.DateTimeToUnixTimeStamp(Child.TuGio);
                            if (Child.DenGio != null)
                                Child.DenGioUnix = dungchung.DateTimeToUnixTimeStamp(Child.DenGio);

                        }
                    }
                }
            NhanVienChamCong.listNgay = NhanVienChamCong.listNgay.OrderBy(a => a.Thu).ToList(); ;
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "lấy dữ liệu  thành công", NhanVienChamCong);
        }
        public dynamic SetdmLichLamViec(HRM_dmLichLamViec entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmLichLamViec entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmLichLamViec)) as HRM_dmLichLamViec;
            if (entity.TuGioUnix > 0)
                entity.TuGio = dungchung.UnixTimeStampToDateTime(entity.TuGioUnix);
            if (entity.DenGioUnix > 0)
                entity.DenGio = dungchung.UnixTimeStampToDateTime(entity.DenGioUnix);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmLichLamViec.Add(entity);

            }
            else
                _dbContext.Entry(entity).State = EntityState.Modified;
            List<HRM_dmLichLamViec_Ngay> listItemN = _dbContext.HRM_dmLichLamViec_Ngay.Where(a => a.IdLichLamViec == entity.IdGuid).ToList();
            List<HRM_dmLichLamViec_Gio> listItemG = _dbContext.HRM_dmLichLamViec_Gio.Where(a => a.IdLichLamViec == entity.IdGuid).ToList();
            if (listItemN != null && listItemN.Count > 0)
                _dbContext.HRM_dmLichLamViec_Ngay.RemoveRange(listItemN);
            if (listItemG != null && listItemG.Count > 0)
                _dbContext.HRM_dmLichLamViec_Gio.RemoveRange(listItemG);
            List<HRM_dmLichLamViec_Gio> listItemAdd = new List<HRM_dmLichLamViec_Gio>();
            if (entity.listNgay != null && entity.listNgay.Count > 0)
            {
                foreach (var item in entity.listNgay)
                {

                    item.IdGuid = Guid.NewGuid().ToString();
                    item.IdLichLamViec = entity.IdGuid;
                    if (item.listGio != null && item.listGio.Count > 0)
                    {
                        foreach (var gio in item.listGio)
                        {
                            if (gio.TuGioUnix > 0)
                                gio.TuGio = dungchung.UnixTimeStampToDateTime(gio.TuGioUnix);
                            if (gio.DenGioUnix > 0)
                                gio.DenGio = dungchung.UnixTimeStampToDateTime(gio.DenGioUnix);
                            gio.IdGuid = Guid.NewGuid().ToString();
                            gio.IdLichLamViec = entity.IdGuid;
                            gio.IdLichLamViecNgay = item.IdGuid;
                        }
                        listItemAdd.AddRange(item.listGio);
                    }
                }
                _dbContext.HRM_dmLichLamViec_Ngay.AddRange(entity.listNgay);
                if (listItemAdd != null && listItemAdd.Count > 0)
                    _dbContext.HRM_dmLichLamViec_Gio.AddRange(listItemAdd);
            }
            if (entity.isApDung == true)
            {
                HRM_dmLichLamViec entityDb = _dbContext.HRM_dmLichLamViec.FirstOrDefault(a => a.isApDung == true && a.IdGuid != entity.IdGuid);
                if (entityDb != null)
                {
                    entityDb.isApDung = false;
                    _dbContext.Entry(entityDb).State = EntityState.Modified;
                }
            }
            _dbContext.SaveChanges();

            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }
        #endregion
        public dynamic GetListDanhMucHinhThucNghi(ApplicationDbContext db)
        {
            List<DanhMuc> listItem = _CallWebService.GetListDanhMucHinhThucNghi(db);
            return listItem;
        }


        public class DM_LoaiLuongModel
        {
            public int Id { set; get; }
            public string LoaiLuong { set; get; }
        }
        #region Danh mục chức vụ kiểm soát nội bộ 
        public dynamic GetListdmChucVuKiemSoatNoiBo(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmChucVuKiemSoatNoiBo
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && itemTimKiem.KeyWord.Contains(a.Ten)))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmChucVuKiemSoatNoiBo(HRM_dmChucVuKiemSoatNoiBo entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmChucVuKiemSoatNoiBo danhMuc = _dbContext.HRM_dmChucVuKiemSoatNoiBo.FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmChucVuKiemSoatNoiBo.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu  thành công", entity);
                }
                else
                {
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không  thành công");
                }

            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không  thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }

        public dynamic DeletedmChucVuKiemSoatNoiBo(string Id, ApplicationDbContext db)
        {
            HRM_dmChucVuKiemSoatNoiBo dmChucVuKiemSoatNoiBo = db.HRM_dmChucVuKiemSoatNoiBo.FirstOrDefault(r => r.IdGuid == Id);
            if (dmChucVuKiemSoatNoiBo == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmChucVuKiemSoatNoiBo.Remove(dmChucVuKiemSoatNoiBo);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        #endregion
        #region Danh mục Ngân Hàng
        public dynamic GetListdmNganHang(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmNganHang
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && itemTimKiem.KeyWord.Contains(a.Ten)))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.Where(a => a.LoaiTaiKhoan == 1).ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic GetListdmNganHangICT(ApplicationDbContext db)
        {

            var result = (from a in db.HRM_dmNganHang
                          where a.LoaiTaiKhoan == 2
                          orderby a.Created
                          select a).ToList();

            return result;
        }

        public dynamic SetdmNganHang(HRM_dmNganHang entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmNganHang danhMuc = _dbContext.HRM_dmNganHang.FirstOrDefault(r => r.Ma == entity.Ma && r.IdGuid != entity.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Mã ngân hàng đã tồn tại");
            }
            if (string.IsNullOrEmpty(entity.IdGuid))
            {

                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmNganHang.Add(entity);

            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }

        public dynamic DeletedmNganHang(string Id, ApplicationDbContext db)
        {
            var dmNganHang = db.HRM_dmNganHang.Where(r => Id == r.IdGuid).ToList();
            if (dmNganHang == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmNganHang.RemoveRange(dmNganHang);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        #endregion

        #region Thư viện đánh giá

        public dynamic GetListdmNhomDanhGia(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmNhomDanhGia
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmNhomDanhGia(HRM_dmNhomDanhGia entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmNhomDanhGia danhMuc = _dbContext.HRM_dmNhomDanhGia.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmNhomDanhGia.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmNhomDanhGia(ApplicationDbContext db, string Id)
        {
            HRM_dmNhomDanhGia dmNhomDanhGia = db.HRM_dmNhomDanhGia.FirstOrDefault(r => r.IdGuid == Id);
            if (dmNhomDanhGia == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmNhomDanhGia.Remove(dmNhomDanhGia);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        public ErrorMessage GetListCachDanhGia()
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                List<HRM_dmNhomDanhGia> listDanhMuc = new List<HRM_dmNhomDanhGia>();
                //HRM_dmNhomDanhGia itemDat = new HRM_dmNhomDanhGia();
                //itemDat.IdGuid = "1";
                //itemDat.Ten = "Đạt - Không đạt";
                //listDanhMuc.Add(itemDat);
                HRM_dmNhomDanhGia itemChamDiem = new HRM_dmNhomDanhGia();
                itemChamDiem.IdGuid = "0";
                itemChamDiem.Ten = "Chấm điểm";
                listDanhMuc.Add(itemChamDiem);
                err.Data = listDanhMuc;
                return err;
            }
            catch (Exception ex)
            {
                err.SetLoiVang("GetListCachDanhGia " + ex.ToString());
                return err;
            }
        }
        public ErrorMessage GetListThuVienDanhGia(ApplicationDbContext _dbContext, TimKiem timKiem)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                IQueryable<HRM_ThuVienDanhGia> result = from a in _dbContext.HRM_ThuVienDanhGia
                                                         where ((!string.IsNullOrEmpty(timKiem.Keyword) && (a.Ma.ToLower().Contains(timKiem.Keyword.Trim().ToLower()) || a.Ten.ToLower().Contains(timKiem.Keyword.Trim().ToLower()))) || string.IsNullOrEmpty(timKiem.Keyword))
                                                         orderby a.Created descending
                                                         select a;

                if (timKiem.IdBoPhan > 0)
                {
                    List<string> listId = new List<string>();
                    var listBp = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_ThuVienDanhGia" && a.IdBoPhan == timKiem.IdBoPhan).ToList();
                    if (listBp != null && listBp.Count > 0)
                        listId = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                    result = result.Where(a => listId.Contains(a.IdGuid));
                }


                if (timKiem.CurrentPage == 0)
                {
                    err.Data = result.ToList();
                    return err;
                }
                int total = result.Count();
                int current = timKiem.CurrentPage ?? 1;

                int pageSize = timKiem.PageSize ?? 25;
                result = result.Skip(pageSize * (current - 1)).Take(pageSize);
                var list = result.ToList();
                //var loaiGoiThaus = _dbContext.HRM_dmLoaiGoiThau.ToList();
                //foreach (var item in list)
                //{
                //    var loaiGoiThau = loaiGoiThaus.Find(x => x.Id == item.IddmLoaiGoiThau);
                //    if (loaiGoiThau != null)
                //    {
                //        item.MadmLoaiGoiThau = loaiGoiThau.Ma;
                //        item.TendmLoaiGoiThau = loaiGoiThau.Ten;
                //    }
                //}
                var paginationSet = new PaginationSet<HRM_ThuVienDanhGia>
                {

                    Items = list,
                    Page = current,
                    TotalCount = total,
                    PageSize = pageSize

                };
                err.Data = paginationSet;
                return err;
            }
            catch (Exception ex)
            {
                err.SetLoiVang("GetListThuVienDanhGia " + ex.ToString());
                return err;
            }
        }

        public ErrorMessage SetThuVienDanhGia(ApplicationDbContext _dbContext, HRM_ThuVienDanhGia entity)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.CapNhatThanhCong);
            {
                try
                {
                    if (string.IsNullOrEmpty(entity.IdGuid))
                    {
                        entity.IdGuid = Guid.NewGuid().ToString();
                        var danhGia = _dbContext.HRM_ThuVienDanhGia.FirstOrDefault(x => x.Ma == entity.Ma);
                        if (danhGia != null)
                        {
                            err.SetLoiKhac("Mã " + entity.Ma + " đã bị trùng!");
                            return err;
                        }
                        _dbContext.HRM_ThuVienDanhGia.Add(entity);
                        //err.Status = ErrorMessage.eState.ThemMoiDuLieu;
                        //_logModify.AddModify(DefineData.eModify.Them.ToString(), nameof (HRM_ThuVienDanhGia), "", JsonConvert.SerializeObject(entity), "", 0);

                    }
                    else
                    {
                        var danhGia = _dbContext.HRM_ThuVienDanhGia.FirstOrDefault(x => x.Ma == entity.Ma && x.IdGuid != entity.IdGuid);
                        if (danhGia != null)
                        {
                            err.SetLoiKhac("Mã " + entity.Ma + " đã bị trùng!");
                            return err;
                        }
                        _dbContext.Entry(entity).State = EntityState.Modified;
                        //_logModify.AddModify(DefineData.eModify.Sua.ToString(), DefineData.eTable.HRM_ThuVienDanhGia.ToString(), "", JsonConvert.SerializeObject(danhGia), JsonConvert.SerializeObject(entity), 0);

                    }
                    var ListBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(r => r.IdQuyTrinh == entity.IdGuid).ToList();
                    _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(ListBoPhan);
                    if (entity.listIdBoPhan != null && entity.listIdBoPhan.Count > 0)
                    {
                        foreach (var item in entity.listIdBoPhan)
                        {
                            HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                            bp.IdGuid = Guid.NewGuid().ToString();
                            bp.IdQuyTrinh = entity.IdGuid;
                            bp.IdBoPhan = item;
                            bp.TableName = nameof(HRM_ThuVienDanhGia);
                            _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                        }
                    }
                    List<HRM_ThuVienDanhGia_Item> listThuVien = new List<HRM_ThuVienDanhGia_Item>();

                    if (entity.listItem != null)
                    {
                        foreach (var item in entity.listItem)
                        {
                            ThuVienDanhGiaToList(item, listThuVien, null);
                        }
                        foreach (var item in listThuVien)
                        {
                            if (String.IsNullOrEmpty(item.Ma))
                            {
                                err.SetLoiKhac("Mã không được để trống");
                                return err;
                            }
                            var MaTrung = listThuVien.FindAll(x => x.Ma == item.Ma && x.IdGuid != item.IdGuid).Count();
                            if (MaTrung > 0)
                            {
                                err.SetLoiKhac("Mã " + item.Ma + " bị trùng");
                                return err;
                            }
                        }
                    }

                    List<HRM_ThuVienDanhGia_Item> listItem = _dbContext.HRM_ThuVienDanhGia_Item.Where(a => a.IdThuVienDanhGia == entity.IdGuid).ToList();
                    _dbContext.HRM_ThuVienDanhGia_Item.RemoveRange(listItem);

                    if (entity.listItem != null)
                    {
                        int SapXep = 1;
                        string errMsg = "Thư viện đánh giá có mã: ";
                        bool invalid = false;
                        foreach (var item in listThuVien)
                        {
                            if (item.DiemToiDa < item.DiemToiThieu)
                            {
                                invalid = true;
                                errMsg += item.Ma + "; ";
                            }
                            item.SapXep = SapXep;
                            item.IdThuVienDanhGia = entity.IdGuid;
                            SapXep++;
                        }
                        errMsg += " điểm tối thiểu lớn hơn điểm tối đa!";
                        if (invalid)
                        {
                            err.SetLoiKhac(errMsg);
                            return err;
                        }
                        _dbContext.HRM_ThuVienDanhGia_Item.AddRange(listThuVien);
                    }
                    if (listThuVien.Count() > 0)
                    {
                        List<HRM_ThuVienDanhGia_NhanSu> DGnhanSus = new List<HRM_ThuVienDanhGia_NhanSu>();
                        foreach (var item in listThuVien)
                        {
                            if (item.listNhanSu != null)
                            {
                                foreach (var nhanSu in item.listNhanSu)
                                {
                                    HRM_ThuVienDanhGia_NhanSu _nhanSu = new HRM_ThuVienDanhGia_NhanSu(nhanSu);
                                    _nhanSu.IdThuVienDanhGiaItem = item.IdGuid;
                                    _nhanSu.IdThuVienDanhGia = entity.IdGuid;
                                    DGnhanSus.Add(_nhanSu);
                                }
                            }
                            if (item.listPhongBan != null)
                            {
                                foreach (var phongBan in item.listPhongBan)
                                {
                                    HRM_ThuVienDanhGia_NhanSu _nhanSu = new HRM_ThuVienDanhGia_NhanSu(phongBan);
                                    _nhanSu.IdThuVienDanhGiaItem = item.IdGuid;
                                    _nhanSu.IdThuVienDanhGia = entity.IdGuid;
                                    DGnhanSus.Add(_nhanSu);
                                }
                            }
                        }


                        _dbContext.HRM_ThuVienDanhGia_NhanSu.AddRange(DGnhanSus);
                    }
                    _dbContext.SaveChanges();
                    err.Data = entity;
                    return err;
                }
                catch (Exception ex)
                {
                    err.SetLoiVang("SetThuVienDanhGia " + ex.ToString());
                    return err;
                }
            }
        }

        public ErrorMessage GetThuVienDanhGia(ApplicationDbContext _dbContext, string Id, bool CheckQuyen = false)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
            try
            {
                HRM_ThuVienDanhGia ThuVien = _dbContext.HRM_ThuVienDanhGia.FirstOrDefault(a => a.IdGuid == Id);
                if (ThuVien == null)
                {
                    err.SetLoiKhac("Không tồn tại");
                    return err;
                }
                List<HRM_ThuVienDanhGia_Item> listItem = _dbContext.HRM_ThuVienDanhGia_Item.Where(a => a.IdThuVienDanhGia == Id).OrderBy(x => x.SapXep).ToList();
                List<HRM_ThuVienDanhGia_NhanSu> danhGia_NhanSus = _dbContext.HRM_ThuVienDanhGia_NhanSu.Where(x => x.IdThuVienDanhGia == Id).ToList();
                var listPhongBanUser = _CallWebService.GetDanhSachBoPhanByUser(_dbContext);
                var nhanSus = _CallWebService.GetListNhanSu(_dbContext);
                var phongBans = _CallWebService.GetListPhongBan(_dbContext);

                ThuVien.listIdBoPhan = new List<long>();
                ThuVien.listIdBoPhan = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == ThuVien.IdGuid).Select(a => a.IdBoPhan).Distinct().ToList();


                foreach (var item in listItem)
                {
                    List<HRM_ThuVienDanhGia_NhanSu> listNhanSuFind = danhGia_NhanSus.FindAll(x => x.IdThuVienDanhGiaItem == item.IdGuid);
                    item.listNhanSu = new List<NhanSu>();
                    item.listPhongBan = new List<PhongBanView>();
                    if (listNhanSuFind == null || listNhanSuFind.Count == 0)
                    {
                        item.isCapNhat = true;
                        item.isShow = true;
                    }
                    else
                        foreach (var itemD in listNhanSuFind)
                        {
                            if (!string.IsNullOrEmpty(itemD.IdUser))
                            {
                                NhanSu itemFind = nhanSus.Find(x => x.IdUser == itemD.IdUser);
                                if (itemFind == null)
                                    continue;
                                NhanSu itemAdd = new NhanSu();
                                itemAdd = itemFind.Clone();
                                itemAdd.isCapNhat = itemD.isCapNhat;
                                itemAdd.isXem = itemD.isXem;
                                if (CheckQuyen == true)
                                {
                                    if (itemAdd.isCapNhat == true)
                                        itemAdd.isXem = true;
                                }
                                itemAdd.IdThuVienDanhGiaItem = itemD.IdGuid;
                                item.listNhanSu.Add(itemAdd);
                            }
                            else
                            {
                                PhongBanView itemFind = phongBans.Find(x => x.Id == itemD.IdPhongBan);
                                if (itemFind == null)
                                    continue;
                                PhongBanView itemAdd = new PhongBanView();
                                itemAdd = itemFind.Clone();
                                itemAdd.isCapNhat = itemD.isCapNhat;
                                itemAdd.isXem = itemD.isXem;
                                if (CheckQuyen == true)
                                {
                                    if (itemAdd.isCapNhat == true)
                                        itemAdd.isXem = true;
                                }
                                itemAdd.IdThuVienDanhGiaItem = itemD.IdGuid;
                                item.listPhongBan.Add(itemAdd);
                            }
                        }
                    foreach (var itemD in item.listNhanSu)
                    {
                        item.isCapNhat = itemD.isCapNhat;
                        item.isXem = itemD.isXem;

                        if (itemD.IdUser == _dbContext.currentUser.Id)
                        {
                            item.isShow = true;
                            break;
                        }
                    }
                    if (item.isShow != true)
                    {
                        foreach (var itemD in item.listPhongBan)
                        {
                            item.isCapNhat = itemD.isCapNhat;
                            item.isXem = itemD.isXem;

                            if (listPhongBanUser.Find(x => x.Id == itemD.Id) != null)
                            {
                                item.isShow = true;
                                break;
                            }
                        }
                    }
                }



                var parents = listItem.FindAll(a => a.IdThuVienDanhGia == Id && string.IsNullOrEmpty(a.IdParent)).Select(x => new ReturnData<HRM_ThuVienDanhGia_Item> { data = x }).ToList();
                foreach (var item in parents)
                {
                    ThuVienDanhGiaToTree(item, listItem);
                    item.data.isRoot = true;
                }
                ThuVien.listItem = parents;
                err.Data = ThuVien;
                return err;
            }
            catch (Exception ex)
            {
                err.SetLoiVang("GetThuVienDanhGia" + ex.ToString());
                return err;
            }
        }

        public ErrorMessage DeleteThuVienDanhGia(ApplicationDbContext _dbContext, List<string> listID)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.XoaThanhCong);
            try
            {
                if (listID.Count() == 0)
                {
                    err.SetLoiKhac("Chưa chọn danh mục nào!");
                    return err;
                }
                IQueryable<HRM_ThuVienDanhGia> ThuViens = _dbContext.HRM_ThuVienDanhGia.Where(a => listID.Contains(a.IdGuid));
                IQueryable<HRM_ThuVienDanhGia_Item> listItem = _dbContext.HRM_ThuVienDanhGia_Item.Where(a => listID.Contains(a.IdThuVienDanhGia));
                List<string> IdNhanSus = listItem.Select(x => x.IdGuid).ToList();
                var danhGia_NhanSus = _dbContext.HRM_ThuVienDanhGia_NhanSu.Where(x => IdNhanSus.Contains(x.IdThuVienDanhGiaItem)).ToList();
                if (ThuViens != null)
                    _dbContext.HRM_ThuVienDanhGia.RemoveRange(ThuViens);
                if (listItem != null)
                    _dbContext.HRM_ThuVienDanhGia_Item.RemoveRange(listItem);
                if (danhGia_NhanSus != null)
                    _dbContext.HRM_ThuVienDanhGia_NhanSu.RemoveRange(danhGia_NhanSus);
                //_logModify.AddModify(DefineData.eModify.Xoa.ToString(), DefineData.eTable.HRM_ThuVienDanhGia.ToString(), "", JsonConvert.SerializeObject(listID), "", 0);

                _dbContext.SaveChanges();
                return err;
            }
            catch (Exception ex)
            {
                err.SetLoiVang("DeleteThuVienDanhGia " + ex.ToString());
                return err;
            }

        }


        public ErrorMessage ExportThuVienDanhGia(ApplicationDbContext _dbContext, string Id)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/ThuVienDanhGia.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);
                if (!String.IsNullOrEmpty(Id))
                {
                    ExcelWorksheet sheetThuVien = package.Workbook.Worksheets[1];
                    ExcelWorksheet sheetChiTiet = package.Workbook.Worksheets[2];
                    HRM_ThuVienDanhGia danhmuc = GetThuVienDanhGia(_dbContext, Id).Data;
                    List<HRM_ThuVienDanhGia_Item> Items = _dbContext.HRM_ThuVienDanhGia_Item.Where(a => a.IdThuVienDanhGia == danhmuc.IdGuid).OrderBy(x => x.SapXep).ToList();
                    var nhomDanhGias = _dbContext.HRM_dmNhomDanhGia.ToList();
                    //var loaiGoiThaus = _dbContext.HRM_dmLoaiGoiThau.ToList();
                    int nRowBegin = 4;
                    int nRow = nRowBegin;
                    int stt = 1;
                    sheetThuVien.Cells[nRow, 1].Value = stt;
                    sheetThuVien.Cells[nRow, 2].Value = danhmuc.Ma;
                    sheetThuVien.Cells[nRow, 3].Value = danhmuc.Ten;
                    //string Iddm = danhmuc.IddmLoaiGoiThau;
                    //var loaiGoiThau = loaiGoiThaus.Find(x => x.Id == Iddm);
                    //if (loaiGoiThau != null)
                    //{
                    //    sheetThuVien.Cells[nRow, 4].Value = loaiGoiThau.Ma;
                    //    sheetThuVien.Cells[nRow, 5].Value = loaiGoiThau.Ten;
                    //}
                    sheetThuVien.Cells[nRow, 9].Value = danhmuc.GhiChu;
                    nRow++;
                    stt++;
                    if (nRow > nRowBegin)
                    {
                        nRow--;
                        sheetThuVien.Cells[nRowBegin, 2, nRow, 2].Style.WrapText = true;
                        sheetThuVien.Cells[nRowBegin, 4, nRow, 4].Style.WrapText = true;
                        sheetThuVien.Cells[nRowBegin, 6, nRow, 6].Style.WrapText = true;
                        DrawTableExcel(sheetThuVien, nRowBegin, nRow, 1, 6);
                    }
                    nRow = nRowBegin;
                    int i = 1;

                    foreach (HRM_ThuVienDanhGia_Item item in Items)
                    {
                        HRM_ThuVienDanhGia thuVienTaiLieu = danhmuc;
                        sheetChiTiet.Cells[nRow, 1].Value = i;
                        sheetChiTiet.Cells[nRow, 2].Value = item.Ma;
                        sheetChiTiet.Cells[nRow, 4].Value = item.Ten;
                        if (item.CachDanhGia == "0")
                            sheetChiTiet.Cells[nRow, 5].Value = "Chấm điểm";
                        else
                            sheetChiTiet.Cells[nRow, 5].Value = "Đạt - Không đạt";
                        sheetChiTiet.Cells[nRow, 6].Value = item.DiemToiDa;
                        sheetChiTiet.Cells[nRow, 7].Value = item.DiemToiThieu;
                        sheetChiTiet.Cells[nRow, 8].Value = item.YeuCau;
                        sheetChiTiet.Cells[nRow, 9].Value = item.GhiChu;
                        var nhomDanhGia = nhomDanhGias.Find(x => x.IdGuid == item.IddmNhomDanhGia);
                        if (nhomDanhGia != null)
                        {
                            item.MadmNhomDanhGia = nhomDanhGia.Ma;
                            item.TendmNhomDanhGia = nhomDanhGia.Ten;
                        }
                        sheetChiTiet.Cells[nRow, 10].Value = item.MadmNhomDanhGia;
                        sheetChiTiet.Cells[nRow, 11].Value = item.TendmNhomDanhGia;
                        var parent = Items.Find(x => x.IdGuid == item.IdParent);
                        if (parent != null)
                        {
                            item.MaCha = parent.Ma;
                        }
                        sheetChiTiet.Cells[nRow, 3].Value = item.MaCha;
                        sheetChiTiet.Cells[nRow, 12].Value = danhmuc.Ma;
                        nRow++;
                        i++;
                    }
                    if (nRow > nRowBegin)
                    {
                        nRow--;
                        sheetChiTiet.Cells[nRowBegin, 4, nRow, 4].Style.WrapText = true;
                        sheetChiTiet.Cells[nRowBegin, 8, nRow, 9].Style.WrapText = true;
                        sheetChiTiet.Cells[nRowBegin, 11, nRow, 11].Style.WrapText = true;
                        sheetChiTiet.Cells[nRowBegin, 6, nRow, 7].Style.Numberformat.Format = dungchung.m_FormatSoLuong0;
                        DrawTableExcel(sheetChiTiet, nRowBegin, nRow, 1, 12);
                    }
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("ThuVienDanhGia.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportThuVienDanhGia " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportThuVienDanhGia(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet ListThuVien = package.Workbook.Worksheets["Thư viện đánh giá"];
                ExcelWorksheet ListItem = package.Workbook.Worksheets["Đánh giá"];

                var listColumnName = new List<string>();
                String[] ColumnNames = new String[] { "STT", "Mã", "Tên", "Mã phòng ban", "Tên phòng ban", "Ghi chú" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(ListThuVien, err, listColumnName, "THƯ VIỆN ĐÁNH GIÁ"))
                {
                    //err.SetLoi("File không đúng định dạng!" + err.);
                    return err;
                }
                var listColumnNameItem = new List<string>();
                String[] ColumnNameItems = new String[] { "STT", "Mã", "Mã cha", "Tên", "Cách đánh giá", "Điểm tối đa", "Điểm tối thiểu", "Yêu cầu", "Ghi chú", "Mã nhóm đánh giá", "Tên nhóm đánh giá", "Mã thư viện đánh giá" };
                listColumnNameItem.AddRange(ColumnNameItems);
                if (!dungchung.KiemTraTempExcel(ListItem, err, listColumnNameItem, "THÔNG TIN ĐÁNH GIÁ"))
                {
                    //err.SetLoi("File không đúng định dạng");
                    return err;
                }

                //List<HRM_dmLoaiGoiThau> loaiGoiThaus = _dbContext.HRM_dmLoaiGoiThau.ToList();
                HRM_ThuVienDanhGia thuvien = new HRM_ThuVienDanhGia();
                for (int row = 4; row <= ListThuVien.Cells.End.Row; row++)
                {
                    string Ma = dungchung.GetCell(ListThuVien, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien.Ma = Ma;
                    thuvien.Ten = dungchung.GetCell(ListThuVien, row, 3);
                    //thuvien.MadmLoaiGoiThau = dungchung.GetCell(ListThuVien, row, 4);
                    //if (!string.IsNullOrEmpty(thuvien.MadmLoaiGoiThau))
                    //{
                    //    var LoaiGoiThau = loaiGoiThaus.Find(x => x.Ma == thuvien.MadmLoaiGoiThau);
                    //    if (LoaiGoiThau != null)
                    //    {
                    //        thuvien.IddmLoaiGoiThau = LoaiGoiThau.Id;
                    //    }
                    //}
                    thuvien.GhiChu = dungchung.GetCell(ListThuVien, row, 6);
                }
                string IdThuVien = "";
                List<HRM_ThuVienDanhGia> dmThuVien = _dbContext.HRM_ThuVienDanhGia.AsNoTracking().ToList();

                HRM_ThuVienDanhGia check = dmThuVien.Where(a => a.Ma == thuvien.Ma).FirstOrDefault();
                if (check != null)
                {
                    err.SetLoiKhac("Mã thư viện: " + thuvien.Ma + " đã tồn tại!");
                    return err;
                }
                //else
                //{
                //    item.Id = IdThuVien = Guid.NewGuid().ToString();
                //    _dbContext.HRM_ThuVienDanhGia.Add(item);
                //}
                var nhomDanhGias = _dbContext.HRM_dmNhomDanhGia.ToList();
                //var tieuChiDanhGias = _dbContext.HRM_dmTieuChiDanhGia.ToList();
                List<HRM_ThuVienDanhGia_Item> Items = new List<HRM_ThuVienDanhGia_Item>();
                for (int row = 4; row <= ListItem.Cells.End.Row; row++)
                {
                    HRM_ThuVienDanhGia_Item item = new HRM_ThuVienDanhGia_Item();
                    string Ma = dungchung.GetCell(ListItem, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    item.Ma = Ma;
                    item.Ten = dungchung.GetCell(ListItem, row, 4);
                    item.CachDanhGia = dungchung.GetCell(ListItem, row, 5) == "1" ? "1" : "0";
                    item.DiemToiDa = dungchung.GetCellToFloat(ListItem, row, 6);
                    item.DiemToiThieu = dungchung.GetCellToFloat(ListItem, row, 7);
                    item.YeuCau = dungchung.GetCell(ListItem, row, 8);
                    item.GhiChu = dungchung.GetCell(ListItem, row, 9);
                    //string danhGia = GetCell(ListItem, row, 8);
                    //bool isDanhGia = dungchung.IsValidDecimalNumber(danhGia);
                    //if (isDanhGia)
                    //else
                    //item.CachDanhGia = 0;
                    item.SapXep = row;
                    string MaNhomDanhGia = dungchung.GetCell(ListItem, row, 10);
                    if (!string.IsNullOrEmpty(MaNhomDanhGia.Trim()))
                    {
                        var nhomDanhGia = nhomDanhGias.Find(x => x.Ma == MaNhomDanhGia);
                        if (nhomDanhGia != null)
                        {
                            item.IddmNhomDanhGia = nhomDanhGia.IdGuid;
                            item.TendmNhomDanhGia = nhomDanhGia.Ten;
                        }
                        else
                        {
                            err.SetLoiKhac("Mã nhóm đánh giá không tồn tại :" + MaNhomDanhGia);
                            return err;
                        }
                    }
                    item.MaCha = dungchung.GetCell(ListItem, row, 3);
                    if (string.IsNullOrEmpty(item.MaCha))
                        item.isRoot = true;
                    item.IdThuVienDanhGia = IdThuVien;
                    Items.Add(item);
                }

                if (Items.Count > 0)
                {
                    //List<HRM_ThuVienDanhGia_Item> listItem = _dbContext.HRM_ThuVienDanhGia_Item.Where(x => x.IdThuVienDanhGia == IdThuVien).AsNoTracking().ToList();
                    //_dbContext.RemoveRange(listItem);
                    //foreach (var item in Items)
                    //{
                    //    item.Id = Guid.NewGuid().ToString();
                    //}
                    Items = Items.Select(x => { x.IdGuid = Guid.NewGuid().ToString(); return x; }).ToList();
                    foreach (var item in Items)
                    {
                        var parent = Items.Find(x => x.Ma == item.MaCha);
                        if (parent != null)
                            item.IdParent = parent.IdGuid;
                    }
                    var parents = Items.Where(x => string.IsNullOrEmpty(x.IdParent)).Select(x => new ReturnData<HRM_ThuVienDanhGia_Item>
                    {
                        data = x
                    }).ToList();
                    foreach (var item in parents)
                    {
                        ThuVienDanhGiaToTree(item, Items);
                    }
                    thuvien.listItem = parents;
                    //_dbContext.HRM_ThuVienDanhGia_Item.AddRange(Items);
                }
                s.Close();
                _dbContext.SaveChanges();
                err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThuVienDanhGia " + e.ToString());
            }
            return err;
        }

        public void UpdateThuVienTieuChi(ApplicationDbContext db)
        {
            var listTieuChi = db.HRM_ThuVienDanhGia_Item.Where(a => a.IdThuVienDanhGia != "de2713cb-cd45-4762-a2d1-19d7cd3d3db9" && a.IdThuVienDanhGia != "f8a80ee5-f10d-4dac-bce2-cdbc9a232b7e").ToList();
            var listPhanQuyen = db.HRM_ThuVienDanhGia_NhanSu.Where(a => a.IdThuVienDanhGiaItem == "adbfa18c-a2bf-4d49-82c2-b2e97905e35d").ToList();

            var listAdd = new List<HRM_ThuVienDanhGia_NhanSu>();
            foreach (var item in listTieuChi)
            {
                foreach (var PQ in listPhanQuyen)
                {
                    var itemAdd = PQ.Clone();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.IdThuVienDanhGia = item.IdThuVienDanhGia;
                    itemAdd.IdThuVienDanhGiaItem = item.IdGuid;
                    listAdd.Add(itemAdd);
                }
            }
            db.HRM_ThuVienDanhGia_NhanSu.AddRange(listAdd);
            db.SaveChanges();
        }

        public void ThuVienDanhGiaToList(ReturnData<HRM_ThuVienDanhGia_Item> data, List<HRM_ThuVienDanhGia_Item> ListAdd, ReturnData<HRM_ThuVienDanhGia_Item> parent)
        {
            data.data.IdGuid = Guid.NewGuid().ToString();
            if (parent != null)
            {
                data.data.IdParent = parent.data.IdGuid;
            }
            ListAdd.Add(data.data);
            if (data.children.Count() > 0)
            {
                foreach (var item in data.children)
                {
                    ThuVienDanhGiaToList(item, ListAdd, data);
                }
            }
        }
        public void ThuVienDanhGiaToTree(ReturnData<HRM_ThuVienDanhGia_Item> parent, List<HRM_ThuVienDanhGia_Item> ListAll)
        {
            parent.data.TenCachDanhGia = parent.data.CachDanhGia == null ? "" : (parent.data.CachDanhGia.Equals("1") ? "Đạt - Không Đạt" : "Chấm điểm");
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_ThuVienDanhGia_Item>
            {
                data = x
            }).ToList();
            if (parent.children.Count() > 0)
            {
                foreach (var item in parent.children)
                {
                    item.data.isShow = parent.data.isShow;
                    item.data.isXem = parent.data.isXem;
                    item.data.isCapNhat = parent.data.isCapNhat;
                    ThuVienDanhGiaToTree(item, ListAll);
                }
            }
        }
        #endregion

        public dynamic GetListdmCapBacLuong(TimKiem TimKiem, ApplicationDbContext db)
        {
            try
            {
                int nCurrentPage = TimKiem.CurrentPage ?? 0;
                int PageSize = TimKiem.PageSize ?? 25;
                IQueryable<HRM_dmCapBacLuong> result = from a in db.HRM_dmCapBacLuong
                                                        orderby a.SapXep
                                                        select a;
                if (TimKiem.KeyWord != null && TimKiem.KeyWord != "")
                    result = result.Where(a => a.Ma.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()) || a.Ten.ToLower().Contains(TimKiem.KeyWord.ToLower().Trim()));



                if (TimKiem.IddmChucDanh > 0)
                {
                    var idChucDanhToken = "," + TimKiem.IddmChucDanh.ToString().Trim() + ",";
                    result = result.Where(a => !string.IsNullOrEmpty(a.IdViTriLamViecs)
                        && ("," + a.IdViTriLamViecs + ",").Contains(idChucDanhToken));
                }
                if (nCurrentPage == 0)
                {
                    var itemLists = result.ToList();
                    var listPhuCap = db.HRM_dmPhuCap.ToList();
                    List<string> listId = itemLists.Select(a => a.IdGuid).Distinct().ToList();
                    var PhuCaps = db.HRM_dmCapBacLuong_PhuCap.Where(a => listId.Contains(a.IdCapBacLuong)).ToList();
                    if (TimKiem.IdCongTyEOS != null && TimKiem.IdCongTyEOS > 0)
                        PhuCaps = PhuCaps.Where(a => !string.IsNullOrEmpty(a.IddmCoSoApDungs)
                        && ((!a.IddmCoSoApDungs.Contains(",") && a.IddmCoSoApDungs == TimKiem.IdCongTyEOS.ToString().Trim())
                        || (a.IddmCoSoApDungs.Split(',').Contains(TimKiem.IdCongTyEOS.ToString().Trim())))).ToList();
                    foreach (var item in PhuCaps)
                    {
                        var Phucap = listPhuCap.FirstOrDefault(a => a.IdGuid == item.IdPhuCap);
                        if (Phucap != null)
                            item.TenPhuCap = Phucap.Ten;
                    }
                    foreach (var item in itemLists)
                    {
                        item.listPhuCap = new List<HRM_dmCapBacLuong_PhuCap>();
                        item.listPhuCap = PhuCaps.Where(a => a.IdCapBacLuong == item.IdGuid).ToList();
                    }
                    return itemLists;
                }
                var itemList = result.ToList();
                int nSum = itemList.Count;
                int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
                PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
                itemList = itemList.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
                var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);

                foreach (var item in itemList)
                {
                    if (!string.IsNullOrEmpty(item.IdViTriLamViecs))
                    {
                        item.listIdViTriLamViec = new List<long>();
                        string[] array = item.IdViTriLamViecs.Split(',');
                        foreach (var a in array)
                            item.listIdViTriLamViec.Add(long.Parse(a));
                        var ChucDanhs = listChucDanh.Where(a => item.listIdViTriLamViec.Contains(a.Id)).ToList();
                        if (ChucDanhs != null && ChucDanhs.Count > 0)
                        {
                            item.TenViTriLamViecs = string.Join(",", ChucDanhs.Select(a => a.Ten));
                        }
                    }
                }
                dynamic dnm = new DynamicObject();
                dnm.paging = paging;
                dnm.items = itemList;
                return dnm;
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, ex.ToString());
            }

        }
        public dynamic GetdmCapBacLuong(string Id, ApplicationDbContext db)
        {
            try
            {
                var quytrinh = db.HRM_dmCapBacLuong.FirstOrDefault(a => a.IdGuid == Id);
                quytrinh.listIdViTriLamViec = new List<long>();
                if (quytrinh.IdViTriLamViecs != null)
                {
                    if (quytrinh.IdViTriLamViecs.Contains(","))
                    {
                        string[] a = quytrinh.IdViTriLamViecs.Split(',');
                        foreach (var item in a)
                        {
                            quytrinh.listIdViTriLamViec.Add(long.Parse(item));
                        }
                    }
                    else
                        quytrinh.listIdViTriLamViec.Add(long.Parse(quytrinh.IdViTriLamViecs));
                }
                quytrinh.listPhuCap = db.HRM_dmCapBacLuong_PhuCap.Where(a => a.IdCapBacLuong == Id).OrderBy(a => a.SapXep).ToList();
                foreach (var item in quytrinh.listPhuCap)
                {
                    item.ListIdCoSo = new List<long>();
                    if (!string.IsNullOrEmpty(item.IddmCoSoApDungs))
                    {
                        if (item.IddmCoSoApDungs.Contains(","))
                        {
                            string[] listId = item.IddmCoSoApDungs.Split(',');
                            foreach (var a in listId)
                            {
                                item.ListIdCoSo.Add(long.Parse(a));
                            }
                        }
                        else
                            item.ListIdCoSo.Add(long.Parse(item.IddmCoSoApDungs));
                    }
                }
                return new MessageReturn(eErrorMessage.DangNhapThanhCong, "Lấy dữ liệu thành công", quytrinh);
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, ex.ToString());
            }
        }
        public dynamic DeletedmCapBacLuong(List<string> Ids, ApplicationDbContext db)
        {
            try
            {
                var check = db.HRM_TuyenDung_UngVien.Where(a => Ids.Contains(a.IdCapBacLuongDeXuat)).ToList();
                if (check != null && check.Count > 0)
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Cấp bậc lương đang được sử dụng");
                var checkLuong = db.HRM_TuyenDung_PheDuyetLuong_UngVien.Where(a => Ids.Contains(a.IdCapBacLuongChapNhan)).ToList();
                if (checkLuong != null && checkLuong.Count > 0)
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Cấp bậc lương đang được sử dụng");

                var quytrinh = db.HRM_dmCapBacLuong.FirstOrDefault(a => Ids.Contains(a.IdGuid));
                var listPhuCap = db.HRM_dmCapBacLuong_PhuCap.Where(a => Ids.Contains(a.IdCapBacLuong)).OrderBy(a => a.SapXep).ToList();
                db.HRM_dmCapBacLuong.Remove(quytrinh);
                if (listPhuCap != null && listPhuCap.Count > 0)
                    db.HRM_dmCapBacLuong_PhuCap.RemoveRange(listPhuCap);
                db.SaveChanges();
                return new MessageReturn(eErrorMessage.DangNhapThanhCong, "Xóa dữ liệu thành công");
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, ex.ToString());
            }
        }
        public dynamic SetdmCapBacLuong(HRM_dmCapBacLuong entity, ApplicationDbContext db)
        {
            try
            {
                var entitydb = db.HRM_dmCapBacLuong.Where(a => a.Ma == entity.Ma && a.IdGuid != entity.IdGuid).ToList();
                if (entitydb != null && entitydb.Count > 0)
                    return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Mã cấp bậc đã tồn tại");
                if (entity.listIdViTriLamViec != null && entity.listIdViTriLamViec.Count > 0)
                {
                    entity.IdViTriLamViecs = string.Join(",", entity.listIdViTriLamViec);
                }
                if (string.IsNullOrEmpty(entity.IdGuid))
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    db.HRM_dmCapBacLuong.Add(entity);

                }
                else
                    db.Entry(entity).State = EntityState.Modified;
                var listPhuCapOld = db.HRM_dmCapBacLuong_PhuCap.Where(a => a.IdCapBacLuong == entity.IdGuid).OrderBy(a => a.SapXep).ToList();
                if (listPhuCapOld != null && listPhuCapOld.Count > 0)
                    db.HRM_dmCapBacLuong_PhuCap.RemoveRange(listPhuCapOld);
                if (entity.listPhuCap != null && entity.listPhuCap.Count > 0)
                {
                    foreach (var item in entity.listPhuCap)
                    {
                        item.IdGuid = Guid.NewGuid().ToString();
                        item.IdCapBacLuong = entity.IdGuid;
                        if (item.ListIdCoSo != null && item.ListIdCoSo.Count > 0)
                        {
                            item.IddmCoSoApDungs = string.Join(",", item.ListIdCoSo);
                        }
                    }
                    db.HRM_dmCapBacLuong_PhuCap.AddRange(entity.listPhuCap);
                }
                db.SaveChanges();
                return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xử lý thành công", entity);
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiLayDuLieu, ex.ToString());
            }
        }

        public ErrorMessage ImportQUyTrinhXuLy(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet = package.Workbook.Worksheets.First();
                DateTime a = DateTime.Now;
                List<HRM_QuyTrinhXuLy> list = new List<HRM_QuyTrinhXuLy>();
                for (int row = 2; row <= sheet.Cells.End.Row; row++)
                {
                    var item = new HRM_QuyTrinhXuLy();
                    int ncol = 1;
                    item.Id = GetCell(sheet, row, ncol);
                    if (string.IsNullOrEmpty(item.Id))
                        break;
                    ncol++;
                    item.IdThietLapQuyTrinh = GetCell(sheet, row, ncol);
                    ncol++;
                    item.IdTrangThai = GetCell(sheet, row, ncol);
                    ncol++;
                    item.IdTable = GetCell(sheet, row, ncol);
                    ncol++;
                    item.TenTrangThai = GetCell(sheet, row, ncol);
                    ncol++;
                    item.GhiChu = GetCell(sheet, row, ncol);
                    ncol++;
                    item.isDaXuLy = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    string boolCheck = GetCell(sheet, row, ncol);
                    if (boolCheck == "FALSE")
                        item.isTrucTiepXuLy = false;
                    else
                        item.isTrucTiepXuLy = true;
                    ncol++;
                    item.IdUser = GetCell(sheet, row, ncol);
                    ncol++;
                    item.Created = GetCellToDateTime(sheet, row, ncol) ?? a;
                    ncol++;
                    item.CreatedBy = GetCell(sheet, row, ncol);
                    ncol++;
                    item.Modified = GetCellToDateTime(sheet, row, ncol) ?? a;
                    ncol++;
                    ncol++;
                    item.ThoiGian = GetCellToDouble(sheet, row, ncol);
                    ncol++;
                    item.Loai = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    item.NgayXuLy = GetCellToDateTime(sheet, row, ncol) ?? a;
                    ncol++;
                    item.IdUserUyQuyen = GetCell(sheet, row, ncol);
                    ncol++;
                    item.NgayCanhBao = GetCellToDateTime(sheet, row, ncol);
                    ncol++;
                    item.NgayHetHan = GetCellToDateTime(sheet, row, ncol);
                    ncol++;
                    item.IdDuAn = GetCellToLong(sheet, row, ncol);
                    ncol++;
                    item.SoQuyTrinh = GetCell(sheet, row, ncol);
                    ncol++;
                    item.NgayNhanViec = GetCellToDateTime(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianGiamTru = GetCellToFloat(sheet, row, ncol);
                    ncol++;
                    item.LyDoGiamTru = GetCell(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianXuLy_Ngay = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianXuLy_Gio = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianXuLy_Phut = GetCellToInt(sheet, row, ncol);

                    ncol++;
                    item.ThoiGianNhanhCham_Ngay = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianNhanhCham_Gio = GetCellToInt(sheet, row, ncol);
                    ncol++;
                    item.ThoiGianNhanhCham_Phut = GetCellToInt(sheet, row, ncol);


                    list.Add(item);

                }
                _dbContext.HRM_QuyTrinhXuLy.AddRange(list);
                s.Close();
                _dbContext.SaveChanges();
                err.Data = list;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportThuVienDanhGia " + e.ToString());
            }
            return err;
        }




        #region Bộ phận chức danh

        public List<HRM_BoPhan_ChucDanhViewModel> GetDanhSachBoPhan_ChucDanh(ApplicationDbContext db = null)
        {
            var result = new List<HRM_BoPhan_ChucDanhViewModel>();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db).OrderBy(a => a.MaBoPhan).ToList();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var BoPhanChucDanhs = db.HRM_BoPhan_ChucDanh.ToList();
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            foreach (var item in listBoPhan)
            {
                HRM_BoPhan_ChucDanhViewModel BoPhan = new HRM_BoPhan_ChucDanhViewModel();
                BoPhan.IdBoPhan = item.Id;
                BoPhan.TenBoPhan = item.TenBoPhan;
                var CongTy = listCongTy.FirstOrDefault(a => a.Id == item.BoPhanChaPkid);
                if (CongTy != null)
                {
                    BoPhan.IdCongTy = CongTy.Id;
                    BoPhan.TenCongTy = CongTy.TenBoPhan;
                }
                BoPhan.listIdChucDanh = new List<long>();
                if (BoPhanChucDanhs != null && BoPhanChucDanhs.Count > 0)
                {
                    BoPhan.listIdChucDanh = BoPhanChucDanhs.Where(a => a.IdBoPhan == item.Id).Select(a => a.IdChucDanh).Distinct().ToList();
                    if (BoPhan.listIdChucDanh != null && BoPhan.listIdChucDanh.Count > 0)
                    {
                        var ChucDanhs = listChucDanh.Where(a => BoPhan.listIdChucDanh.Contains(a.Id)).ToList();
                        BoPhan.TenChucDanh = string.Join(", ", ChucDanhs.Select(a => a.Ten).Distinct().ToList());
                    }
                }
                result.Add(BoPhan);
            }

            return result;
        }

        public MessageReturn UpdateDanhMucBoPhan_ChucDanh(HRM_BoPhan_ChucDanhViewModel data, string idCurrentUser, ApplicationDbContext db = null)
        {

            var BoPhanChucDanhold = db.HRM_BoPhan_ChucDanh.Where(a => a.IdBoPhan == data.IdBoPhan).ToList();
            db.HRM_BoPhan_ChucDanh.RemoveRange(BoPhanChucDanhold);
            if (data.listIdChucDanh != null && data.listIdChucDanh.Count > 0)
            {
                List<HRM_BoPhan_ChucDanh> listAdd = new List<HRM_BoPhan_ChucDanh>();
                foreach (var item in data.listIdChucDanh)
                {
                    HRM_BoPhan_ChucDanh Add = new HRM_BoPhan_ChucDanh();
                    Add.IdGuid = Guid.NewGuid().ToString();
                    Add.IdCongTy = data.IdCongTy;
                    Add.IdBoPhan = data.IdBoPhan;
                    Add.IdChucDanh = item;
                    listAdd.Add(Add);
                }
                db.HRM_BoPhan_ChucDanh.AddRange(listAdd);
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("UpdateDanhMucBoPhan_ChucDanh: {0} {1}", data.IdBoPhan, ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public MessageReturn DeleteDanhMucBoPhan_ChucDanh(List<long> listIdBoPhan, string idCurrentUser, ApplicationDbContext db = null)
        {
            List<HRM_BoPhan_ChucDanh> listDanhMuc = db.HRM_BoPhan_ChucDanh.Where(q => listIdBoPhan.Contains(q.IdBoPhan)).ToList();
            foreach (var edititem in listDanhMuc)
            {
                db.Entry(edititem).State = EntityState.Deleted;
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorFormat("DelDanhMucBoPhan_ChucDanh: {0} {1}", JsonConvert.SerializeObject(listIdBoPhan), ex.Message);
                return new MessageReturn(DefineData.eErrorMessage.LoiCapNhat, "Cập nhật lỗi: " + ex.Message);
            }
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Cập nhật thành công");
        }

        public dynamic GetDanhSachChucDanhTheoIdBoPhan(long IdBoPhan, ApplicationDbContext db = null)
        {
            var listChucDanh = _CallWebService.GetDanhSachChucDanh(db);
            List<long> listId = db.HRM_BoPhan_ChucDanh.Where(a => a.IdBoPhan == IdBoPhan).Select(a => a.IdChucDanh).Distinct().ToList();
            listChucDanh = listChucDanh.Where(a => listId.Contains(a.Id)).ToList();

            return listChucDanh;
        }

        #endregion

        #region Câu hỏi
        #region nhóm câu hỏi
        public dynamic GetListdmNhomCauHoi(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 20;
            var result = from a in db.HRM_dmNhomCauHoi
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()) || a.Ma.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmNhomCauHoi(HRM_dmNhomCauHoi entity, ApplicationDbContext _dbContext)
        {

            HRM_dmNhomCauHoi danhMuc = _dbContext.HRM_dmNhomCauHoi.FirstOrDefault(r => r.Ma == entity.Ma && r.IdGuid != entity.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Trùng mã nhóm câu hỏi");
            }
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmNhomCauHoi.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }

        public dynamic DeletedmNhomCauHoi(string Id, ApplicationDbContext db)
        {
            HRM_dmNhomCauHoi dmNhomCauHoi = db.HRM_dmNhomCauHoi.FirstOrDefault(r => r.IdGuid == Id);
            if (dmNhomCauHoi == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmNhomCauHoi.Remove(dmNhomCauHoi);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        public ErrorMessage ExportdmNhomCauHoi(ApplicationDbContext _dbContext, List<string> listId)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/dmNhomCauHoi.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);
                var listDanhMuc = _dbContext.HRM_dmNhomCauHoi.Where(a => listId.Contains(a.IdGuid)).ToList();
                if (listDanhMuc != null && listDanhMuc.Count > 0)
                {
                    ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                    int STT = 1;
                    int nRow = 4;
                    foreach (var item in listDanhMuc)
                    {
                        sheet1.Cells[nRow, 1].Value = STT;
                        sheet1.Cells[nRow, 2].Value = item.Ma;
                        sheet1.Cells[nRow, 3].Value = item.Ten;
                        sheet1.Cells[nRow, 4].Value = item.GhiChu;
                        nRow++;
                    }
                    dungchung.DrawTableExcel(sheet1, 4, nRow - 1, 1, 4);
                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("dmNhomCauHoi.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmNhomCauHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportdmNhomCauHoi(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets.First();
                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "Mã", "Tên", "Ghi chú" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "DANH SACH NHÓM CÂU HỎI"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_dmNhomCauHoi.AsNoTracking().ToList();
                List<HRM_dmNhomCauHoi> listAdd = new List<HRM_dmNhomCauHoi>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_dmNhomCauHoi thuvien = new HRM_dmNhomCauHoi();

                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    thuvien = listdaTa.FirstOrDefault(a => a.Ma == Ma);
                    if (thuvien == null)
                        thuvien = new HRM_dmNhomCauHoi();
                    thuvien.Ma = Ma;
                    thuvien.Ten = dungchung.GetCell(sheet1, row, 3);

                    thuvien.GhiChu = dungchung.GetCell(sheet1, row, 6);
                    if (!string.IsNullOrEmpty(thuvien.IdGuid))
                        _dbContext.Entry(thuvien).State = EntityState.Modified;
                    else
                    {
                        thuvien.IdGuid = Guid.NewGuid().ToString();
                        listAdd.Add(thuvien);
                    }
                }
                _dbContext.HRM_dmNhomCauHoi.AddRange(listAdd);
                s.Close();
                _dbContext.SaveChanges();
                //err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportdmNhomCauHoi " + e.ToString());
            }
            return err;
        }
        #endregion
        #region Câu hỏi

        public dynamic GetListdmCauHoi(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_dmCauHoi> result = from a in db.HRM_dmCauHoi
                                               where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.NoiDung.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()) || a.Ma.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()))))
                                               orderby a.Created
                                               select a;
            if (!string.IsNullOrEmpty(itemTimKiem.LoaiCauHoi))
                result = result.Where(a => a.Loai == itemTimKiem.LoaiCauHoi);
            if (!string.IsNullOrEmpty(itemTimKiem.IddmNhomCauHoi))
                result = result.Where(a => a.IddmNhomCauHoi == itemTimKiem.IddmNhomCauHoi);
            if (!string.IsNullOrEmpty(itemTimKiem.IdLoaiKhaoSat))
                result = result.Where(a => a.IdLoaiKhaoSat == itemTimKiem.IdLoaiKhaoSat);
            result = result.Where(a => a.isKhaoSat == itemTimKiem.isKhaoSat);

            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            List<string> listId = listItem.Select(a => a.IdGuid).ToList();
            var listCauHoi = db.HRM_dmCauHoi_Item.Where(a => listId.Contains(a.IddmCauHoi)).OrderBy(a => a.STT).ToList();
            List<string> listIdNhom = listItem.Select(a => a.IddmNhomCauHoi).Distinct().ToList();
            var NhomCauHois = db.HRM_dmNhomCauHoi.Where(a => listIdNhom.Contains(a.IdGuid)).ToList();
            Parallel.ForEach(listItem, item =>
            {
                var NhomCauHoi = NhomCauHois.FirstOrDefault(a => a.IdGuid == item.IddmNhomCauHoi);
                if (NhomCauHoi != null)
                    item.TendmNhomCauHoi = NhomCauHoi.Ten;
                item.TenLoai = DefineData.QuyDoiLoaiCauHoi(item.Loai);
                item.listCauTraLoi = listCauHoi.Where(a => a.IddmCauHoi == item.IdGuid).ToList();
                if (item.listCauTraLoi.Where(a => string.IsNullOrEmpty(a.Ten)).Count() > 0)
                {
                    int i = 1;
                    foreach (var tl in item.listCauTraLoi)
                    {
                        tl.Ten = dungchung.GetAlphabeticalOrder(i);
                        i++;
                    }
                }
            });
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmCauHoi(HRM_dmCauHoi entity, ApplicationDbContext _dbContext)
        {

            HRM_dmCauHoi danhMuc = _dbContext.HRM_dmCauHoi.FirstOrDefault(r => r.Ma == entity.Ma && r.IdGuid != entity.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Trùng mã nhóm câu hỏi");
            }
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmCauHoi.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            var CauTLOld = _dbContext.HRM_dmCauHoi_Item.Where(a => a.IddmCauHoi == entity.IdGuid).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                _dbContext.HRM_dmCauHoi_Item.RemoveRange(CauTLOld);
            if (entity.listCauTraLoi != null && entity.listCauTraLoi.Count > 0)
            {
                int STT = 1;
                foreach (var item in entity.listCauTraLoi)
                {
                    if (string.IsNullOrEmpty(item.IdGuid))
                        item.IdGuid = Guid.NewGuid().ToString();
                    item.IddmCauHoi = entity.IdGuid;
                    item.STT = STT;
                    STT++;
                }
                if (entity.Loai == DefineData.eLoaiCauhoi.SapXep.ToString())
                    entity.DapAn = string.Join(",", entity.listCauTraLoi.Select(a => a.SapXepDapAn).ToList());
                _dbContext.HRM_dmCauHoi_Item.AddRange(entity.listCauTraLoi);
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }

        public dynamic DeletedmCauHoi(string Id, ApplicationDbContext db)
        {
            HRM_dmCauHoi dmCauHoi = db.HRM_dmCauHoi.FirstOrDefault(r => r.IdGuid == Id);
            if (dmCauHoi == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            var CauTLOld = db.HRM_dmCauHoi_Item.Where(a => a.IddmCauHoi == Id).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                db.HRM_dmCauHoi_Item.RemoveRange(CauTLOld);
            db.HRM_dmCauHoi.Remove(dmCauHoi);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        public dynamic GetListLoaiCauHoi()
        {
            List<DanhMuc> list = new List<DanhMuc>();
            list.Add(new DanhMuc
            {
                Ma = DefineData.eLoaiCauhoi.ChonABCD.ToString(),
                Ten = "Chọn đáp án"
            });
            list.Add(new DanhMuc
            {
                Ma = DefineData.eLoaiCauhoi.NhapText.ToString(),
                Ten = "Nhập câu trả lời"
            });
            list.Add(new DanhMuc
            {
                Ma = DefineData.eLoaiCauhoi.SapXep.ToString(),
                Ten = "Sắp xếp"
            });
            list.Add(new DanhMuc
            {
                Ma = DefineData.eLoaiCauhoi.KyNang.ToString(),
                Ten = "Kỹ Năng"
            });


            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công", list);
        }

        public ErrorMessage ExportdmCauHoi(ApplicationDbContext _dbContext, List<HRM_dmCauHoi> listDanhMuc)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ExportDuLieuThanhCong);
            try
            {
                string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/dmCauHoi.xlsx");
                DateTime dt = DateTime.Now;
                string sFileNameCopy = HrmOptions.PathUploads + "ThuVienDanhGiaDownload_" + dt.ToOADate() + ".xlsx";
                File.Copy(sFileName, sFileNameCopy, true);
                using Stream s = File.OpenRead(sFileNameCopy);
                using ExcelPackage package = new ExcelPackage(s);

                if (listDanhMuc != null && listDanhMuc.Count > 0)
                {
                    var ListNhomCauHoi = _dbContext.HRM_dmNhomCauHoi.ToList();
                    ExcelWorksheet sheet1 = package.Workbook.Worksheets["Câu hỏi"];
                    ExcelWorksheet sheet2 = package.Workbook.Worksheets["Câu trả lời"];
                    int STT = 1;
                    int STTCauTL = 1;
                    int nRow = 4;
                    int nRowCauTL = 5;
                    foreach (var item in listDanhMuc)
                    {
                        sheet1.Cells[nRow, 1].Value = STT;
                        sheet1.Cells[nRow, 2].Value = item.Ma;
                        sheet1.Cells[nRow, 3].Value = item.NoiDung;
                        var nhomCauHoi = ListNhomCauHoi.FirstOrDefault(a => a.IdGuid == item.IddmNhomCauHoi);
                        if (nhomCauHoi != null)
                        {
                            sheet1.Cells[nRow, 4].Value = nhomCauHoi.Ma;
                            sheet1.Cells[nRow, 5].Value = nhomCauHoi.Ten;
                        }
                        sheet1.Cells[nRow, 6].Value = item.Loai;
                        sheet1.Cells[nRow, 7].Value = item.Diem;
                        if (item.Loai != DefineData.eLoaiCauhoi.SapXep.ToString())
                            sheet1.Cells[nRow, 8].Value = item.DapAn;

                        nRow++;
                        STT++;
                        if (item.listCauTraLoi != null)
                        {
                            foreach (var itemTl in item.listCauTraLoi)
                            {
                                sheet2.Cells[nRowCauTL, 1].Value = STTCauTL;
                                sheet2.Cells[nRowCauTL, 2].Value = item.Ma;
                                sheet2.Cells[nRowCauTL, 3].Value = item.NoiDung;
                                sheet2.Cells[nRowCauTL, 4].Value = itemTl.NoiDung;
                                sheet2.Cells[nRowCauTL, 5].Value = itemTl.SapXepDapAn;
                                nRowCauTL++;
                                STTCauTL++;
                            }
                        }
                    }
                    dungchung.DrawTableExcel(sheet1, 3, nRow - 1, 1, 8);
                    dungchung.DrawTableExcel(sheet2, 3, nRowCauTL - 1, 1, 5);

                }
                s.Close();
                byte[] bytee = package.GetAsByteArray();
                File.WriteAllBytes(sFileNameCopy, bytee);
                err.Data = "/uploader/api/hdfiles?filename=" + dungchung.Base64Encode("dmCauHoi.xlsx")
                                + "&path=" + dungchung.Base64Encode(sFileNameCopy);
            }
            catch (Exception e)
            {
                err.SetLoiVang("ExportdmCauHoi " + e.ToString());
            }
            return err;
        }

        public ErrorMessage ImportdmCauHoi(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Câu hỏi"];
                ExcelWorksheet sheet2 = package.Workbook.Worksheets["Câu trả lời"];
                var listColumnName = new List<string>();
                string[] ColumnNames = new string[] { "STT", "Mã", "Nội dung", "Mã nhóm câu hỏi", "Tên nhóm câu hỏi", "Loại câu hỏi", "Điểm", "Đáp án" };
                listColumnName.AddRange(ColumnNames);
                if (!dungchung.KiemTraTempExcel(sheet1, err, listColumnName, "DANH SÁCH CÂU HỎI"))
                {
                    return err;
                }
                var listdaTa = _dbContext.HRM_dmCauHoi.AsNoTracking().ToList();
                var ListNhomCauHoi = _dbContext.HRM_dmNhomCauHoi.ToList();
                List<HRM_dmCauHoi> listAdd = new List<HRM_dmCauHoi>();
                List<string> listMa = new List<string>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    HRM_dmCauHoi thuvien = new HRM_dmCauHoi();

                    string Ma = dungchung.GetCell(sheet1, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;

                    thuvien.Ma = Ma;
                    thuvien.NoiDung = dungchung.GetCell(sheet1, row, 3);
                    string MaNhomCauHoi = dungchung.GetCell(sheet1, row, 4);
                    var NhomCauHoi = ListNhomCauHoi.FirstOrDefault(a => a.Ma == MaNhomCauHoi);
                    if (NhomCauHoi != null)
                    {
                        thuvien.IddmNhomCauHoi = NhomCauHoi.IdGuid;
                    }
                    thuvien.Loai = dungchung.GetCell(sheet1, row, 6);
                    thuvien.Diem = dungchung.GetCellToDouble(sheet1, row, 7);
                    if (thuvien.Loai != DefineData.eLoaiCauhoi.SapXep.ToString())
                        thuvien.DapAn = dungchung.GetCell(sheet1, row, 8);
                    var thuviencheck = listdaTa.FirstOrDefault(a => a.Ma == Ma);
                    if (thuviencheck != null)
                        listMa.Add(Ma);
                    else
                    {
                        thuvien.IdGuid = Guid.NewGuid().ToString();
                        listAdd.Add(thuvien);
                    }
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("Mã câu hỏi đã tồn tại ở sheet câu hỏi " + string.Join(", ", listAdd));
                    return err;
                }

                List<HRM_dmCauHoi_Item> listItem = new List<HRM_dmCauHoi_Item>();
                listMa = new List<string>();
                for (int row = 5; row <= sheet2.Cells.End.Row; row++)
                {
                    string Ma = dungchung.GetCell(sheet2, row, 2);
                    if (string.IsNullOrEmpty(Ma))
                        break;
                    HRM_dmCauHoi thuvien = listAdd.FirstOrDefault(a => a.Ma == Ma);
                    if (thuvien == null)
                    {
                        listMa.Add(Ma);
                    }
                    HRM_dmCauHoi_Item itemAdd = new HRM_dmCauHoi_Item();
                    itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.IddmCauHoi = thuvien.IdGuid;
                    itemAdd.NoiDung = dungchung.GetCell(sheet2, row, 4);
                    itemAdd.SapXepDapAn = dungchung.GetCellToInt(sheet2, row, 5);
                    itemAdd.STT = row;
                    if (thuvien.Loai == DefineData.eLoaiCauhoi.SapXep.ToString())
                    {
                        if (string.IsNullOrEmpty(thuvien.DapAn))
                            thuvien.DapAn += itemAdd.SapXepDapAn;
                        else
                            thuvien.DapAn += "," + itemAdd.SapXepDapAn;
                    }
                    listItem.Add(itemAdd);
                }
                if (listMa.Count > 0)
                {
                    err.SetLoiKhac("Mã câu hỏi không tồn tại ở sheet câu trả lời " + string.Join(", ", listAdd));
                    return err;
                }

                _dbContext.HRM_dmCauHoi.AddRange(listAdd);
                _dbContext.HRM_dmCauHoi_Item.AddRange(listItem);
                s.Close();
                _dbContext.SaveChanges();
                //err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportdmCauHoi " + e.ToString());
            }
            return err;
        }

        #endregion
        public dynamic GetListdmCauTrucDe(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 20;
            var result = from a in db.HRM_dmCauTrucDe
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()) || a.Ma.ToUpper().Contains(itemTimKiem.KeyWord.Trim().ToUpper()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            List<string> listId = listItem.Select(a => a.IdGuid).ToList();
            var listCauHoi = db.HRM_dmCauTrucDe_Item.Where(a => listId.Contains(a.IddmCauTrucDe)).OrderBy(a => a.STT).ToList();
            var listnhomCauHoi = db.HRM_dmNhomCauHoi.ToList();
            Parallel.ForEach(listCauHoi, item =>
            {
                var nhomCauHoi = listnhomCauHoi.FirstOrDefault(a => a.IdGuid == item.IddmNhomCauHoi);
                if (nhomCauHoi != null)
                    item.TendmNhomCauHoi = nhomCauHoi.Ten;
            });
            Parallel.ForEach(listItem, item =>
            {
                item.listItem = listCauHoi.Where(a => a.IddmCauTrucDe == item.IdGuid).ToList();
            });
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmCauTrucDe(HRM_dmCauTrucDe entity, ApplicationDbContext _dbContext)
        {

            HRM_dmCauTrucDe danhMuc = _dbContext.HRM_dmCauTrucDe.FirstOrDefault(r => r.Ma == entity.Ma && r.IdGuid != entity.IdGuid);
            if (danhMuc != null)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Trùng mã cấu trúc đề");
            }
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmCauTrucDe.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            var CauTLOld = _dbContext.HRM_dmCauTrucDe_Item.Where(a => a.IddmCauTrucDe == entity.IdGuid).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                _dbContext.HRM_dmCauTrucDe_Item.RemoveRange(CauTLOld);
            if (entity.listItem != null && entity.listItem.Count > 0)
            {
                int STT = 1;
                foreach (var item in entity.listItem)
                {
                    if (string.IsNullOrEmpty(item.IdGuid))
                        item.IdGuid = Guid.NewGuid().ToString();
                    item.IddmCauTrucDe = entity.IdGuid;
                    item.STT = STT;
                    STT++;
                }
                _dbContext.HRM_dmCauTrucDe_Item.AddRange(entity.listItem);
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu  thành công", entity);
        }

        public dynamic DeletedmCauTrucDe(string Id, ApplicationDbContext db)
        {
            HRM_dmCauTrucDe dmCauTrucDe = db.HRM_dmCauTrucDe.FirstOrDefault(r => r.IdGuid == Id);
            var CauTLOld = db.HRM_dmCauTrucDe_Item.Where(a => a.IddmCauTrucDe == Id).ToList();
            if (CauTLOld != null && CauTLOld.Count > 0)
                db.HRM_dmCauTrucDe_Item.RemoveRange(CauTLOld);
            if (dmCauTrucDe == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không  thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmCauTrucDe.Remove(dmCauTrucDe);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        #region Danh Mục Loại Chi Phí Công Tác
        public dynamic GetListdmPhuongTien(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmPhuongTien
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmPhuongTien(HRM_dmPhuongTien entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmPhuongTien danhMuc = _dbContext.HRM_dmPhuongTien.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmPhuongTien.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmPhuongTien(ApplicationDbContext db, string Id)
        {
            HRM_dmPhuongTien dmPhuongTien = db.HRM_dmPhuongTien.FirstOrDefault(r => r.IdGuid == Id);
            if (dmPhuongTien == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmPhuongTien.Remove(dmPhuongTien);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        #endregion

        #region Danh Mục Phương Tiện
        public dynamic GetListdmLoaiChiPhiCongTac(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmLoaiChiPhiCongTac
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmLoaiChiPhiCongTac(HRM_dmLoaiChiPhiCongTac entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmLoaiChiPhiCongTac danhMuc = _dbContext.HRM_dmLoaiChiPhiCongTac.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmLoaiChiPhiCongTac.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmLoaiChiPhiCongTac(ApplicationDbContext db, string Id)
        {
            HRM_dmLoaiChiPhiCongTac dmLoaiChiPhiCongTac = db.HRM_dmLoaiChiPhiCongTac.FirstOrDefault(r => r.IdGuid == Id);
            if (dmLoaiChiPhiCongTac == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmLoaiChiPhiCongTac.Remove(dmLoaiChiPhiCongTac);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        #endregion
        #region Danh mục nơi cấp
        public dynamic GetListdmNoiCap(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmNoiCap
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.SapXep
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmNoiCap(HRM_dmNoiCap entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmNoiCap danhMuc = _dbContext.HRM_dmNoiCap.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmNoiCap.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmNoiCap(ApplicationDbContext db, string Id)
        {
            HRM_dmNoiCap dmNoiCap = db.HRM_dmNoiCap.FirstOrDefault(r => r.IdGuid == Id);
            if (dmNoiCap == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmNoiCap.Remove(dmNoiCap);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        #endregion
        #region Danh mục nơi cấp bảo hiểm
        public dynamic GetListdmNoiCapBaoHiem(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmNoiCapBaoHiem
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmNoiCapBaoHiem(HRM_dmNoiCapBaoHiem entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmNoiCapBaoHiem danhMuc = _dbContext.HRM_dmNoiCapBaoHiem.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmNoiCapBaoHiem.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmNoiCapBaoHiem(ApplicationDbContext db, string Id)
        {
            HRM_dmNoiCapBaoHiem dmNoiCap = db.HRM_dmNoiCapBaoHiem.FirstOrDefault(r => r.IdGuid == Id);
            if (dmNoiCap == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmNoiCapBaoHiem.Remove(dmNoiCap);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        #endregion

        #region Danh mục hệ số làm thêm giờ
        public dynamic GetListdmHeSoLamThemGio(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 0;
            int PageSize = itemTimKiem.PageSize ?? 0;
            var result = from a in db.HRM_dmHeSoLamThemGio
                         where (string.IsNullOrEmpty(itemTimKiem.KeyWord) || (!string.IsNullOrEmpty(itemTimKiem.KeyWord) && (a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()) || a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim()))))
                         orderby a.Created
                         select a;
            if (nCurrentPage == 0)
                return result.ToList();
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmHeSoLamThemGio(HRM_dmHeSoLamThemGio entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmKyHieuChamCong entity = JsonConvert.DeserializeObject(dyn.ToString(), typeof(HRM_dmKyHieuChamCong)) as HRM_dmKyHieuChamCong;
            HRM_dmHeSoLamThemGio danhMuc = _dbContext.HRM_dmHeSoLamThemGio.AsNoTracking().FirstOrDefault(r => r.Ma == entity.Ma);
            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                if (danhMuc == null)
                {
                    entity.IdGuid = Guid.NewGuid().ToString();
                    _dbContext.HRM_dmHeSoLamThemGio.Add(entity);
                    _dbContext.SaveChanges();
                    return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Thêm dữ liệu thành công", entity);
                }
                else
                    return new MessageReturn(eErrorMessage.LoiCapNhat, "Thêm dữ liệu không thành công");
            }
            else
            {
                if (danhMuc != null)
                {
                    if (danhMuc.IdGuid == entity.IdGuid)
                        _dbContext.Entry(entity).State = EntityState.Modified;
                    else
                        return new MessageReturn(eErrorMessage.LoiCapNhat, "cạp nhập dữ liệu không thành công");
                }
                else
                    _dbContext.Entry(entity).State = EntityState.Modified;
                _dbContext.SaveChanges();

            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmHeSoLamThemGio(ApplicationDbContext db, string Id)
        {
            HRM_dmHeSoLamThemGio dmHeSoLamThemGio = db.HRM_dmHeSoLamThemGio.FirstOrDefault(r => r.IdGuid == Id);
            if (dmHeSoLamThemGio == null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Cập nhật dữ liệu không thành công do bản ghi không tồn tại hoặc bị xóa trước đó");
            db.HRM_dmHeSoLamThemGio.Remove(dmHeSoLamThemGio);
            db.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }
        #endregion
        #region Danh mục hệ số làm thêm giờ
        public dynamic GetListdmQuyDinhCongTacPhi(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 1;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_dmQuyDinhCongTacPhi> result = from a in db.HRM_dmQuyDinhCongTacPhi
                                                          orderby a.Created
                                                          select a;
            if (itemTimKiem.IdBoPhan > 0)
                result = result.Where(a => a.IdBoPhan == itemTimKiem.IdBoPhan);
            if (itemTimKiem.IdChucVu > 0)
                result = result.Where(a => a.IdChucVu == itemTimKiem.IdChucVu);

            var listItem = result.ToList();
            var listresult = listItem.Select(a => a.IdChucVu).Distinct().Select(a => new HRM_DanhMucCongTacPhiViewModel
            {
                IdChucVu = a
            }).Distinct().ToList();
            int nSum = listresult.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listresult = listresult.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listBoPhan = _CallWebService.GetDanhSachBoPhan(db);
            var listChucVu = _CallWebService.GetDanhSachChucVu(db);
            var listdanhmucChiPhi = db.HRM_dmCongTacPhi.ToList();
            if (listresult != null && listresult.Count > 0)
                foreach (var item in listresult)
                {
                    var chucVu = listChucVu.FirstOrDefault(a => a.Id == item.IdChucVu);
                    if (chucVu != null)
                        item.TenChucVu = chucVu.TenChucVu;
                    item.listIdBoPhan = new List<long>();
                    item.listCongTacPhi = (from a in listItem
                                           where a.IdChucVu == item.IdChucVu
                                           group a by a.IddmLoaiChiPhi into g
                                           select g.OrderBy(a => a.Created).FirstOrDefault()
                                          ).ToList();

                    if (item.listCongTacPhi != null && item.listCongTacPhi.Count > 0)
                    {
                        item.MucPhi = item.listCongTacPhi.Sum(a => a.MucPhi);
                        {
                            List<string> listId = item.listCongTacPhi.Select(a => a.IddmLoaiChiPhi).Distinct().ToList();
                            item.TenLoaiChiPhi = string.Join(", ", listdanhmucChiPhi.Where(a => listId.Contains(a.IdGuid)).Select(a => a.Ten));
                        }
                        item.listIdBoPhan = item.listCongTacPhi.Select(a => a.IdBoPhan).Distinct().ToList();
                        if (item.listIdBoPhan != null && item.listIdBoPhan.Count > 0)
                        {
                            List<string> list = new List<string>();
                            foreach (long Id in item.listIdBoPhan)
                            {
                                list.Add(listBoPhan.Where(a => a.Id == Id).Select(a => a.TenBoPhan).FirstOrDefault());
                            }
                            item.TenBoPhan = string.Join(", ", list);
                        }
                        foreach (var Cp in item.listCongTacPhi)
                            Cp.NgayBatDauUnix = dungchung.DateTimeToUnixTimeStamp(Cp.NgayBatDau);
                        item.GhiChu = item.listCongTacPhi.FirstOrDefault().GhiChu;

                    }
                }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listresult;
            return data;
        }
        public dynamic SetdmQuyDinhCongTacPhi(HRM_DanhMucCongTacPhiViewModel entity, ApplicationDbContext _dbContext)
        {

            var listdbOld = _dbContext.HRM_dmQuyDinhCongTacPhi.Where(r => r.IdChucVu == entity.IdChucVu).ToList();
            if (listdbOld != null && listdbOld.Count > 0)
                _dbContext.HRM_dmQuyDinhCongTacPhi.RemoveRange(listdbOld);

            if (entity.listCongTacPhi != null && entity.listCongTacPhi.Count > 0)
            {
                List<HRM_dmQuyDinhCongTacPhi> listAdd = new List<HRM_dmQuyDinhCongTacPhi>();
                foreach (var item in entity.listCongTacPhi)
                {
                    DateTime Ngay = dungchung.UnixTimeStampToDateTime(item.NgayBatDauUnix);
                    foreach (var idBoPhan in entity.listIdBoPhan)
                    {
                        listAdd.Add(new HRM_dmQuyDinhCongTacPhi
                        {
                            IdGuid = Guid.NewGuid().ToString(),
                            GhiChu = entity.GhiChu,
                            IdChucVu = entity.IdChucVu,
                            IdBoPhan = idBoPhan,
                            IddmLoaiChiPhi = item.IddmLoaiChiPhi,
                            NgayBatDau = Ngay,
                            MucPhi = item.MucPhi
                        });
                    }
                }
                _dbContext.HRM_dmQuyDinhCongTacPhi.AddRange(listAdd);
            }
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmQuyDinhCongTacPhi(ApplicationDbContext _dbContext, long IdChucVu)
        {
            var listdbOld = _dbContext.HRM_dmQuyDinhCongTacPhi.Where(r => r.IdChucVu == IdChucVu).ToList();
            if (listdbOld != null && listdbOld.Count > 0)
                _dbContext.HRM_dmQuyDinhCongTacPhi.RemoveRange(listdbOld);
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhật dữ liệu  thành công");
        }

        #endregion

        #endregion


        #region thiết Lập danh mục bảng Lương


        public dynamic GetListdmThietLapBangLuong(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 1;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_dmThietLapBangLuong> result = from a in db.HRM_dmThietLapBangLuong
                                                          orderby a.Created
                                                          select a;

            if (itemTimKiem.IdCongTy > 0)
            {
                List<string> listIds = new List<string>();
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBangLuong_CONGTY" && a.IdBoPhan == itemTimKiem.IdCongTy).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                result = result.Where(a => listIds.Contains(a.IdGuid));
            }
            if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                result = result.Where(a => (!string.IsNullOrEmpty(a.Ma) && a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.Ten) && a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())));
            if (itemTimKiem.IdBoPhan > 0)
            {
                List<string> listIds = new List<string>();
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBangLuong" && a.IdBoPhan == itemTimKiem.IdBoPhan).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                result = result.Where(a => listIds.Contains(a.IdGuid));
            }
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var listId = listItem.Select(a => a.IdGuid).Distinct().ToList();
            //var listChiTiet = db.HRM_dmThietLapBangLuong_ChiTiet.Where(a => listId.Contains(a.IdDanhMuc)).ToList();
            //var listdanhmucChiPhi = db.HRM_dmLabelThietLapBangLuong.ToList();
            if (listItem != null && listItem.Count > 0)
                foreach (var item in listItem)
                {

                    if (item.IdCongTy != null)
                    {
                        item.listIdCongTy = new List<long>();
                        if (item.IdCongTy.Contains(","))
                        {
                            string[] list = item.IdCongTy.Split(',');
                            foreach (var bp in list)
                                item.listIdCongTy.Add(long.Parse(bp));
                        }
                        else item.listIdCongTy.Add(long.Parse(item.IdCongTy));
                        List<BoPhan> CongTy = listCongTy.FindAll(x => item.listIdCongTy.Contains(x.Id));
                        if (CongTy != null)
                            item.TenCongTy = string.Join(",", CongTy.Select(a => a.TenBoPhan));
                    }
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
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));
                    }
                }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmThietLapBangLuong(HRM_dmThietLapBangLuong entity, ApplicationDbContext _dbContext)
        {
            HRM_dmThietLapBangLuong check = _dbContext.HRM_dmThietLapBangLuong.FirstOrDefault(a => a.IdCongTy == entity.IdCongTy && a.IdGuid != entity.IdGuid);
            if (check != null)
                return new MessageReturn(eErrorMessage.LoiCapNhat, "Dự án đã được thiết lập bảng lương");

            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmThietLapBangLuong.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            var listdbOld = _dbContext.HRM_dmThietLapBangLuong_ChiTiet.Where(r => r.IdDanhMuc == entity.IdGuid).ToList();
            if (listdbOld != null && listdbOld.Count > 0)
                _dbContext.HRM_dmThietLapBangLuong_ChiTiet.RemoveRange(listdbOld);

            var listBp = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == entity.IdGuid).ToList();
            _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(listBp);
            if (entity.listIdBoPhan != null && entity.listIdBoPhan.Count > 0)
            {
                entity.IdBoPhan = string.Join(",", entity.listIdBoPhan);
                foreach (var item in entity.listIdBoPhan)
                {
                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                    bp.IdGuid = Guid.NewGuid().ToString();
                    bp.IdQuyTrinh = entity.IdGuid;
                    bp.IdBoPhan = item;
                    bp.TableName = nameof(HRM_dmThietLapBangLuong);
                    _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                }
            }
            if (entity.listIdCongTy != null && entity.listIdCongTy.Count > 0)
            {
                entity.IdCongTy = string.Join(",", entity.listIdCongTy);
                foreach (var item in entity.listIdCongTy)
                {
                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                    bp.IdGuid = Guid.NewGuid().ToString();
                    bp.IdQuyTrinh = entity.IdGuid;
                    bp.IdBoPhan = item;
                    bp.TableName = "HRM_dmThietLapBangLuong_CONGTY";
                    _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                }
            }

            if (entity.listChiTiet != null && entity.listChiTiet.Count > 0)
            {
                List<HRM_dmThietLapBangLuong_ChiTiet> listAdd = new List<HRM_dmThietLapBangLuong_ChiTiet>();
                int header = 1;
                ThietLapBangLuongToList(entity.listChiTiet, listAdd, header);
                int sapxep = 1;
                foreach (var item in listAdd)
                {
                    if (item.isChon == false)
                        continue;
                    item.IdDanhMuc = entity.IdGuid;
                    //item.IdCongTy = entity.IdCongTy;
                    item.SapXep = sapxep;
                    sapxep++;
                }
                _dbContext.HRM_dmThietLapBangLuong_ChiTiet.AddRange(listAdd);
            }

            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmThietLapBangLuong(ApplicationDbContext _dbContext, string Id)
        {
            try
            {
                var listdbOld = _dbContext.HRM_dmThietLapBangLuong_ChiTiet.Where(r => r.IdDanhMuc == Id).ToList();
                if (listdbOld != null && listdbOld.Count > 0)
                    _dbContext.HRM_dmThietLapBangLuong_ChiTiet.RemoveRange(listdbOld);
                HRM_dmThietLapBangLuong check = _dbContext.HRM_dmThietLapBangLuong.FirstOrDefault(a => a.IdGuid == Id);
                _dbContext.HRM_dmThietLapBangLuong.Remove(check);
                _dbContext.SaveChanges();
                return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic GetdmThietLapBangLuong(ApplicationDbContext _dbContext, string Id = null)
        {
            HRM_dmThietLapBangLuong entity = new HRM_dmThietLapBangLuong();
            if (!string.IsNullOrEmpty(Id))
                entity = _dbContext.HRM_dmThietLapBangLuong.FirstOrDefault(a => a.IdGuid == Id);
            var listdb = _dbContext.HRM_dmThietLapBangLuong_ChiTiet.Where(r => r.IdDanhMuc == Id).ToList();
            var listDanhMuc = _dbContext.HRM_dmLabelThietLapBangLuong.OrderBy(a => a.SapXep).ToList();
            var listPhuCap = _dbContext.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            if (entity.IdBoPhan != null)
            {
                entity.listIdBoPhan = new List<long>();
                if (entity.IdBoPhan.Contains(","))
                {
                    string[] list = entity.IdBoPhan.Split(',');
                    foreach (var bp in list)
                        entity.listIdBoPhan.Add(long.Parse(bp));
                }
                else entity.listIdBoPhan.Add(long.Parse(entity.IdBoPhan));
            }
            if (entity.IdCongTy != null)
            {
                entity.listIdCongTy = new List<long>();
                if (entity.IdCongTy.Contains(","))
                {
                    string[] list = entity.IdCongTy.Split(',');
                    foreach (var bp in list)
                        entity.listIdCongTy.Add(long.Parse(bp));
                }
                else entity.listIdCongTy.Add(long.Parse(entity.IdCongTy));
            }

            entity.listChiTiet = new List<ReturnData<HRM_dmThietLapBangLuong_ChiTiet>>();
            var listAdd = new List<HRM_dmThietLapBangLuong_ChiTiet>();
            foreach (var item in listDanhMuc)
            {
                HRM_dmThietLapBangLuong_ChiTiet itemAdd = new HRM_dmThietLapBangLuong_ChiTiet(item);
                if (!string.IsNullOrEmpty(Id) && listdb != null && listdb.Count > 0)
                {
                    var check = listdb.FirstOrDefault(a => a.IddmLabel == item.IdGuid);
                    if (check != null)
                    {
                        itemAdd.isChon = true;
                        itemAdd.SapXep = check.SapXep;
                        //itemAdd.IdGuid = check.IdGuid;
                        //itemAdd.IdParent = check.IdParent;
                    }
                    else
                    {
                        //itemAdd.IdGuid = Guid.NewGuid().ToString();

                        itemAdd.SapXep = item.SapXep;
                    }
                }
                else
                {
                    //itemAdd.IdGuid = Guid.NewGuid().ToString();
                    itemAdd.SapXep = item.SapXep;
                }
                listAdd.Add(itemAdd);
            }
            var HoTro = listAdd.Where(a => a.Ma == "HoTro").FirstOrDefault();
            foreach (var item in listPhuCap)
            {
                HRM_dmThietLapBangLuong_ChiTiet itemAdd = new HRM_dmThietLapBangLuong_ChiTiet(item);
                if (listdb != null && listdb.Count > 0)
                {
                    var check = listdb.FirstOrDefault(a => a.IddmLabel == item.IdGuid);
                    if (check != null)
                    {
                        itemAdd.isChon = true;
                        itemAdd.SapXep = check.SapXep;
                        itemAdd.IdGuid = check.IdGuid;
                    }
                    else
                    {
                        itemAdd.IdGuid = Guid.NewGuid().ToString();

                        itemAdd.SapXep = item.SapXep;
                    }

                }
                else
                {
                    itemAdd.IdGuid = Guid.NewGuid().ToString();

                    itemAdd.SapXep = item.SapXep;
                }
                itemAdd.IdParent = HoTro.IdGuid;
                listAdd.Add(itemAdd);
            }
            listAdd = listAdd.OrderBy(a => a.SapXep).ToList();
            entity.listChiTiet = listAdd.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_dmThietLapBangLuong_ChiTiet>
            {
                data = a,
                children = new List<ReturnData<HRM_dmThietLapBangLuong_ChiTiet>>()
            }).ToList();
            foreach (var item in entity.listChiTiet)
                ThietLapBangLuongToTree(listAdd, item);
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }
        public void ImPortDanhMuc(ApplicationDbContext _dbContext, string FileName)
        {

            var listDanhMuc = new List<HRM_dmLabelThietLapBangLuong>();
            string pathTemplate =PathUploads+ FileName;
            using Stream s = File.OpenRead(pathTemplate);
            using ExcelPackage package = new ExcelPackage(s);
            var sheet1 = package.Workbook.Worksheets["ICT"];
            for (int row = 3; row <= sheet1.Cells.End.Row; row++)
            {
                HRM_dmLabelThietLapBangLuong dm = new HRM_dmLabelThietLapBangLuong();
                dm.IdGuid = dungchung.GetCell(sheet1, row, 1);
                if (string.IsNullOrEmpty(dm.IdGuid))
                    dm.IdGuid = Guid.NewGuid().ToString();
                dm.Ma = dungchung.GetCell(sheet1, row, 2);
                if (dm.Ma == "")
                    break;
                dm.MaCha = dungchung.GetCell(sheet1, row, 3);
                dm.Ten = dungchung.GetCell(sheet1, row, 4);
                dm.DoRong = dungchung.GetCellToInt(sheet1, row, 5);
                dm.DinhDang = dungchung.GetCell(sheet1, row, 6);
                dm.SoThapPhan = dungchung.GetCellToInt(sheet1, row, 7);
                dm.SapXep = dungchung.GetCellToInt(sheet1, row, 8);
                listDanhMuc.Add(dm);
            }
            foreach (var Add in listDanhMuc)
            {
                var parent = listDanhMuc.FirstOrDefault(a => a.Ma == Add.MaCha);
                if (parent != null)
                    Add.IdParent = parent.IdGuid;
            }
            var parents = listDanhMuc.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_dmLabelThietLapBangLuong>
            {
                data = a,
                children = new List<ReturnData<HRM_dmLabelThietLapBangLuong>>()
            }).ToList();
            foreach (var item in parents)
                dmThietLapBangLuongToTree(listDanhMuc, item);
            listDanhMuc = new List<HRM_dmLabelThietLapBangLuong>();
            int Header = 1;
            dmThietLapBangLuongToList(parents, listDanhMuc, Header);
            _dbContext.HRM_dmLabelThietLapBangLuong.AddRange(listDanhMuc);
            _dbContext.SaveChanges();
        }
        public void ExportDanhMuc(ApplicationDbContext _dbContext)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("~/MauBaoCao/DanhSachAnHienCot.xlsx");
            var listDanhMuc = _dbContext.HRM_dmLabelThietLapBangLuong.OrderBy(a => a.SapXep).ToList(); ;
            var filenamegui = "DanhSachAnHienCot.xlsx";
            string sFileNameCopy =PathUploads+ filenamegui;
            File.Copy(sFileName, sFileNameCopy, true);
            using Stream s = File.OpenRead(sFileName);
            using ExcelPackage package = new ExcelPackage(s);
            ExcelWorksheet sheet1 = package.Workbook.Worksheets["ICT"];
            int row = 3;
            foreach (var dm in listDanhMuc)
            {

                sheet1.Cells[row, 1].Value = dm.IdGuid;
                sheet1.Cells[row, 2].Value = dm.Ma;
                var parent = listDanhMuc.FirstOrDefault(a => a.IdGuid == dm.IdParent);
                if (parent != null)
                    sheet1.Cells[row, 3].Value = parent.Ma;
                sheet1.Cells[row, 4].Value = dm.Ten;
                sheet1.Cells[row, 5].Value = dm.DoRong;
                sheet1.Cells[row, 6].Value = dm.DinhDang;
                sheet1.Cells[row, 7].Value = dm.SoThapPhan;
                sheet1.Cells[row, 8].Value = dm.SapXep;
                row++;
            }
            dynamic result = new DynamicObject();
            s.Close();

            byte[] bytee = package.GetAsByteArray();
            File.WriteAllBytes(sFileNameCopy, bytee);
            dynamic dnm = new DynamicObject();
            dnm.TenFile = "/uploader/api/hdfiles?filename=" + Base64Encode(filenamegui.ToString())
                            + "&path=" + Base64Encode(sFileNameCopy);
            //return dnm;
        }

        public void ThietLapBangLuongToList(List<ReturnData<HRM_dmThietLapBangLuong_ChiTiet>> listPhanKhu, List<HRM_dmThietLapBangLuong_ChiTiet> ListAdd, int header)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();
                item.data.Header = header;
                if (string.IsNullOrEmpty(item.data.IdKey))
                    item.data.IdKey = item.data.IdGuid;
                else
                    item.data.IdKey = item.data.IdKey + "_" + item.data.IdGuid;
                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                {
                    int Cap = header + 1;
                    foreach (var child in item.children)
                    {
                        child.data.IdParent = item.data.IdGuid;
                        child.data.IdKey = item.data.IdKey;
                    }

                    ThietLapBangLuongToList(item.children, ListAdd, Cap);
                }
            }
        }
        public void ThietLapBangLuongToTree(List<HRM_dmThietLapBangLuong_ChiTiet> ListAll, ReturnData<HRM_dmThietLapBangLuong_ChiTiet> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_dmThietLapBangLuong_ChiTiet>
            {
                data = x,
                children = new List<ReturnData<HRM_dmThietLapBangLuong_ChiTiet>>()
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    ThietLapBangLuongToTree(ListAll, item);
                }
            }

        }
        public void dmThietLapBangLuongToList(List<ReturnData<HRM_dmLabelThietLapBangLuong>> listPhanKhu, List<HRM_dmLabelThietLapBangLuong> ListAdd, int header)
        {
            foreach (var item in listPhanKhu)
            {
                //item.data.IdGuid = Guid.NewGuid().ToString();
                item.data.Header = header;
                if (string.IsNullOrEmpty(item.data.IdKey))
                    item.data.IdKey = item.data.IdGuid;
                else
                    item.data.IdKey = item.data.IdKey + "_" + item.data.IdGuid;
                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                {
                    int Cap = header + 1;
                    foreach (var child in item.children)
                    {
                        child.data.IdParent = item.data.IdGuid;
                        child.data.IdKey = item.data.IdKey;
                    }
                    dmThietLapBangLuongToList(item.children, ListAdd, Cap);
                }
            }
        }
        public void dmThietLapBangLuongToTree(List<HRM_dmLabelThietLapBangLuong> ListAll, ReturnData<HRM_dmLabelThietLapBangLuong> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_dmLabelThietLapBangLuong>
            {
                data = x,
                children = new List<ReturnData<HRM_dmLabelThietLapBangLuong>>()
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    dmThietLapBangLuongToTree(ListAll, item);
                }
            }

        }

        #endregion

        public ErrorMessage ImportdmTinhPhuongXas(ApplicationDbContext _dbContext, string FileName)
        {
            ErrorMessage err = new ErrorMessage(ErrorMessage.eState.ImportDuLieuThanhCong);
            try
            {
                string sFileName = HrmOptions.PathUploads + FileName;
                using Stream s = File.OpenRead(sFileName);
                using ExcelPackage package = new ExcelPackage(s);
                ExcelWorksheet sheet1 = package.Workbook.Worksheets["Danh mục địa chỉ"];
                var listColumnName = new List<string>();

                List<HRM_dmTinh> listTinh = new List<HRM_dmTinh>();
                List<HRM_dmQuanHuyen> listHuyen = new List<HRM_dmQuanHuyen>();
                List<HRM_dmPhuongXa> listXa = new List<HRM_dmPhuongXa>();
                for (int row = 4; row <= sheet1.Cells.End.Row; row++)
                {
                    // tỉnh 
                    string MaTinh = dungchung.GetCell(sheet1, row, 3);
                    if (!string.IsNullOrEmpty(MaTinh))
                    {
                        HRM_dmTinh itemTinh = new HRM_dmTinh();
                        itemTinh.IdGuid = Guid.NewGuid().ToString();
                        itemTinh.Ma = MaTinh;
                        itemTinh.Ten = dungchung.GetCell(sheet1, row, 1);
                        listTinh.Add(itemTinh);
                    }
                    string MaQuanHuyen = dungchung.GetCell(sheet1, row, 7);
                    if (!string.IsNullOrEmpty(MaQuanHuyen))
                    {
                        HRM_dmQuanHuyen itemQuanHuyen = new HRM_dmQuanHuyen();
                        itemQuanHuyen.IdGuid = Guid.NewGuid().ToString();
                        itemQuanHuyen.Ma = MaQuanHuyen;
                        itemQuanHuyen.Ten = dungchung.GetCell(sheet1, row, 5);
                        itemQuanHuyen.MaTinh = dungchung.GetCell(sheet1, row, 8);
                        listHuyen.Add(itemQuanHuyen);
                    }
                    string MaPhuongXa = dungchung.GetCell(sheet1, row, 12);
                    if (!string.IsNullOrEmpty(MaPhuongXa))
                    {
                        HRM_dmPhuongXa itemPhuongXa = new HRM_dmPhuongXa();
                        itemPhuongXa.IdGuid = Guid.NewGuid().ToString();
                        itemPhuongXa.Ma = MaPhuongXa;
                        itemPhuongXa.Ten = dungchung.GetCell(sheet1, row, 10);
                        itemPhuongXa.MaHuyen = dungchung.GetCell(sheet1, row, 13);
                        listXa.Add(itemPhuongXa);
                    }
                    else break;

                }
                foreach (var item in listHuyen)
                {
                    var tinh = listTinh.FirstOrDefault(a => a.Ma == item.MaTinh);
                    item.IdTinh = tinh.IdGuid;
                }
                foreach (var item in listXa)
                {
                    var tinh = listHuyen.FirstOrDefault(a => a.Ma == item.MaHuyen);
                    item.IdQuanHuyen = tinh.IdGuid;
                }
                _dbContext.HRM_dmTinh.AddRange(listTinh);
                _dbContext.HRM_dmQuanHuyen.AddRange(listHuyen);
                _dbContext.HRM_dmPhuongXa.AddRange(listXa);
                s.Close();
                _dbContext.SaveChanges();
                //err.Data = thuvien;
            }
            catch (Exception e)
            {
                err.SetLoiVang("ImportdmNhomCauHoi " + e.ToString());
            }
            return err;
        }

        #region Báo cáo công việc
        public dynamic GetListdmThietLapBaoCaoCongViec(ApplicationDbContext db, dynamic dnm)
        {
            TimKiem itemTimKiem = JsonConvert.DeserializeObject(dnm.ToString(), typeof(TimKiem)) as TimKiem;
            int nCurrentPage = itemTimKiem.CurrentPage ?? 1;
            int PageSize = itemTimKiem.PageSize ?? 20;
            IQueryable<HRM_dmThietLapBaoCaoCongViec> result = from a in db.HRM_dmThietLapBaoCaoCongViec
                                                               orderby a.Created
                                                               select a;

            if (itemTimKiem.IdCongTy > 0)
            {
                List<string> listIds = new List<string>();
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBaoCaoCongViec_CONGTY" && a.IdBoPhan == itemTimKiem.IdCongTy).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                result = result.Where(a => listIds.Contains(a.IdGuid));
            }
            if (!string.IsNullOrEmpty(itemTimKiem.KeyWord))
                result = result.Where(a => (!string.IsNullOrEmpty(a.Ma) && a.Ma.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())) || (!string.IsNullOrEmpty(a.Ten) && a.Ten.ToLower().Contains(itemTimKiem.KeyWord.ToLower().Trim())));
            if (itemTimKiem.IdBoPhan > 0)
            {
                List<string> listIds = new List<string>();
                var listBp = db.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.TableName == "HRM_dmThietLapBaoCaoCongViec" && a.IdBoPhan == itemTimKiem.IdBoPhan).ToList();
                if (listBp != null && listBp.Count > 0)
                    listIds = listBp.Select(a => a.IdQuyTrinh).Distinct().ToList();
                result = result.Where(a => listIds.Contains(a.IdGuid));
            }
            var listItem = result.ToList();
            int nSum = listItem.Count;
            int nTotalPage = PageSize > 0 ? (int)Math.Ceiling((double)nSum / PageSize) : (nSum > 0 ? 1 : 0);
            PagingInfor paging = new PagingInfor(nCurrentPage, nSum, nTotalPage);
            listItem = listItem.Skip(PageSize * (nCurrentPage - 1)).Take(PageSize).ToList();
            var listCongTy = _CallWebService.GetDanhSachCongTy(db);
            var BoPhans = _CallWebService.GetDanhSachBoPhan(db);
            var listId = listItem.Select(a => a.IdGuid).Distinct().ToList();
            //var listChiTiet = db.HRM_dmThietLapBaoCaoCongViec_ChiTiet.Where(a => listId.Contains(a.IdDanhMuc)).ToList();
            //var listdanhmucChiPhi = db.HRM_dmLabelThietLapBaoCaoCongViec.ToList();
            if (listItem != null && listItem.Count > 0)
                foreach (var item in listItem)
                {
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
                        List<BoPhan> BoPhan = BoPhans.FindAll(x => item.listIdBoPhan.Contains(x.Id));
                        if (BoPhan != null)
                            item.TenBoPhan = string.Join(",", BoPhan.Select(a => a.TenBoPhan));
                    }
                }
            dynamic data = new DynamicObject();
            data.paging = paging;
            data.items = listItem;
            return data;
        }
        public dynamic SetdmThietLapBaoCaoCongViec(HRM_dmThietLapBaoCaoCongViec entity, ApplicationDbContext _dbContext)
        {
            //HRM_dmThietLapBaoCaoCongViec check = _dbContext.HRM_dmThietLapBaoCaoCongViec.FirstOrDefault(a => a.IdGuid != entity.IdGuid);
            //if (check != null)
            //    return new MessageReturn(eErrorMessage.LoiCapNhat, "Dự án đã được thiết lập bảng lương");

            if (string.IsNullOrEmpty(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
                _dbContext.HRM_dmThietLapBaoCaoCongViec.Add(entity);
            }
            else
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            var listdbOld = _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.Where(r => r.IdDanhMuc == entity.IdGuid).ToList();
            if (listdbOld != null && listdbOld.Count > 0)
                _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.RemoveRange(listdbOld);

            var listBp = _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Where(a => a.IdQuyTrinh == entity.IdGuid).ToList();
            _dbContext.HRM_QuyTrinhBoPhan_BoPhan.RemoveRange(listBp);
            if (entity.listIdBoPhan != null && entity.listIdBoPhan.Count > 0)
            {
                entity.IdBoPhan = string.Join(",", entity.listIdBoPhan);
                foreach (var item in entity.listIdBoPhan)
                {
                    HRM_QuyTrinhBoPhan_BoPhan bp = new HRM_QuyTrinhBoPhan_BoPhan();
                    bp.IdGuid = Guid.NewGuid().ToString();
                    bp.IdQuyTrinh = entity.IdGuid;
                    bp.IdBoPhan = item;
                    bp.TableName = nameof(HRM_dmThietLapBaoCaoCongViec);
                    _dbContext.HRM_QuyTrinhBoPhan_BoPhan.Add(bp);
                }
            }


            if (entity.listChiTiet != null && entity.listChiTiet.Count > 0)
            {
                List<HRM_dmThietLapBaoCaoCongViec_ChiTiet> listAdd = new List<HRM_dmThietLapBaoCaoCongViec_ChiTiet>();
                int header = 1;
                ThietLapBaoCaoCongViecToList(entity.listChiTiet, listAdd, header);
                int sapxep = 1;
                foreach (var item in listAdd)
                {

                    item.IdDanhMuc = entity.IdGuid;
                    item.SapXep = sapxep;
                    sapxep++;
                }
                _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.AddRange(listAdd);
            }

            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }

        public dynamic DeletedmThietLapBaoCaoCongViec(ApplicationDbContext _dbContext, string Id)
        {
            try
            {
                var listdbOld = _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.Where(r => r.IdDanhMuc == Id).ToList();
                if (listdbOld != null && listdbOld.Count > 0)
                    _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.RemoveRange(listdbOld);
                HRM_dmThietLapBaoCaoCongViec check = _dbContext.HRM_dmThietLapBaoCaoCongViec.FirstOrDefault(a => a.IdGuid == Id);
                _dbContext.HRM_dmThietLapBaoCaoCongViec.Remove(check);
                _dbContext.SaveChanges();
                return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu  thành công");
            }
            catch (Exception ex)
            {
                return new MessageReturn(eErrorMessage.LoiCapNhat, ex.ToString());
            }
        }
        public dynamic GetdmThietLapBaoCaoCongViec(ApplicationDbContext _dbContext, string Id)
        {
            HRM_dmThietLapBaoCaoCongViec entity = new HRM_dmThietLapBaoCaoCongViec();
            if (!string.IsNullOrEmpty(Id))
                entity = _dbContext.HRM_dmThietLapBaoCaoCongViec.FirstOrDefault(a => a.IdGuid == Id);
            var listdb = _dbContext.HRM_dmThietLapBaoCaoCongViec_ChiTiet.Where(r => r.IdDanhMuc == Id).OrderBy(a => a.SapXep).ToList();
            var listPhuCap = _dbContext.HRM_dmPhuCap.OrderBy(a => a.SapXep).ToList();
            if (entity.IdBoPhan != null)
            {
                entity.listIdBoPhan = new List<long>();
                if (entity.IdBoPhan.Contains(","))
                {
                    string[] list = entity.IdBoPhan.Split(',');
                    foreach (var bp in list)
                        entity.listIdBoPhan.Add(long.Parse(bp));
                }
                else entity.listIdBoPhan.Add(long.Parse(entity.IdBoPhan));
            }


            entity.listChiTiet = new List<ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>>();
            entity.listChiTiet = listdb.Where(a => string.IsNullOrEmpty(a.IdParent)).Select(a => new ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>
            {
                data = a,
                children = new List<ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>>()
            }).ToList();
            foreach (var item in entity.listChiTiet)
                ThietLapBaoCaoCongViecToTree(listdb, item);
            _dbContext.SaveChanges();
            return new MessageReturn(eErrorMessage.CapNhatThanhCong, "Cập nhập dữ liệu thành công", entity);
        }
        public void ThietLapBaoCaoCongViecToList(List<ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>> listPhanKhu, List<HRM_dmThietLapBaoCaoCongViec_ChiTiet> ListAdd, int header)
        {
            foreach (var item in listPhanKhu)
            {
                item.data.IdGuid = Guid.NewGuid().ToString();
                item.data.Header = header;
                if (string.IsNullOrEmpty(item.data.IdKey))
                    item.data.IdKey = item.data.IdGuid;
                else
                    item.data.IdKey = item.data.IdKey + "_" + item.data.IdGuid;
                ListAdd.Add(item.data);
                if (item.children.Count() > 0)
                {
                    int Cap = header + 1;
                    foreach (var child in item.children)
                    {
                        child.data.IdParent = item.data.IdGuid;
                        child.data.IdKey = item.data.IdKey;
                    }

                    ThietLapBaoCaoCongViecToList(item.children, ListAdd, Cap);
                }
            }
        }
        public void ThietLapBaoCaoCongViecToTree(List<HRM_dmThietLapBaoCaoCongViec_ChiTiet> ListAll, ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet> parent)
        {
            parent.children = ListAll.Where(x => x.IdParent == parent.data.IdGuid).Select(x => new ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>
            {
                data = x,
                children = new List<ReturnData<HRM_dmThietLapBaoCaoCongViec_ChiTiet>>()
            }).ToList();
            if (parent.children.Count > 0)
            {
                foreach (var item in parent.children)
                {
                    ThietLapBaoCaoCongViecToTree(ListAll, item);
                }
            }

        }

        #endregion
    }



}



