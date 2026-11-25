#nullable disable
using Microsoft.EntityFrameworkCore;
using HRM.Api.Entity;
using HRM.Api.Helper;
using HRM.Api.Models;
//using HRM.Api.Models.SmartEOEModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
namespace HRM.Api.Services
{
    public class FileUploadController
    {

        public MessageReturn DeleteFiles(List<HRM_FileDinhKem> aListFile, ApplicationDbContext aContext)
        {
            if (aListFile.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu tệp đính kèm để xóa");
            }

            var listId = aListFile.Select(x => x.IdGuid).ToList();
            listId = listId.ClearIsEmpty();

            var listFileDelete = aContext.HRM_FileDinhKem.Where(x => listId.Contains(x.IdGuid)).ToList();
            if (listFileDelete.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu tệp đính kèm để xóa");
            }

            aContext.HRM_FileDinhKem.RemoveRange(listFileDelete);
            return new MessageReturn(DefineData.eErrorMessage.CapNhatThanhCong, "Xóa dữ liệu tệp đính kèm thành công");
        }

        public MessageReturn DeleteFiles(List<HRM_FileDinhKem> aListFile, ApplicationDbContext aContext, MessageReturn result)
        {
            if (aListFile.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu tệp đính kèm để xóa");
            }

            var listId = aListFile.Select(x => x.IdGuid).ToList();
            listId = listId.ClearIsEmpty();

            var listFileDelete = aContext.HRM_FileDinhKem.Where(x => listId.Contains(x.IdGuid)).ToList();
            if (listFileDelete.IsEmpty())
            {
                return new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Không tìm thấy dữ liệu tệp đính kèm để xóa");
            }

            aContext.HRM_FileDinhKem.RemoveRange(listFileDelete);
            return result;
        }

        private static void PopulateFileLinks(IEnumerable<HRM_FileDinhKem> tepDinhKems)
        {
            foreach (HRM_FileDinhKem item in tepDinhKems)
            {
                string sFileName = Path.Combine(HostingEnvironmentCompat.PathUploads) + item.FileNameGUI;
                string duoi = Path.GetExtension(item.FileNameGUI)?.TrimStart('.') ?? string.Empty;
                string name = Guid.NewGuid().ToString();
                if (!String.IsNullOrEmpty(item.FileName))
                {
                    item.Link = "/uploader/DownloadFile?filename=" + HttpUtility.UrlEncode(dungchung.Base64Encode(item.FileName))
                            + "&path=" + dungchung.Base64Encode(sFileName);
                    item.LinkDocView = "docviewer/?filename=" + HttpUtility.UrlEncode(dungchung.Base64Encode(name + "." + duoi))
                           + "&path=" + dungchung.Base64Encode(sFileName);
                }
            }
        }

        public static List<HRM_FileDinhKem> GetListFileDinhKem(ApplicationDbContext db, string IdTable, string eTable)
        {
            List<HRM_FileDinhKem> TepDinhKems = db.HRM_FileDinhKem.AsNoTracking()
                .Where(x => x.idItem == IdTable && x.TableName == eTable)
                .OrderByDescending(x => x.Created)
                .ToList();
            PopulateFileLinks(TepDinhKems);
            return TepDinhKems;
        }

        public static ILookup<string, HRM_FileDinhKem> GetListFileDinhKemByIds(ApplicationDbContext db, IEnumerable<string> itemIds, string eTable)
        {
            var ids = itemIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids == null || ids.Count == 0)
                return Enumerable.Empty<HRM_FileDinhKem>().ToLookup(x => x.idItem);

            var files = db.HRM_FileDinhKem.AsNoTracking()
                .Where(x => x.TableName == eTable && ids.Contains(x.idItem))
                .OrderByDescending(x => x.Created)
                .ToList();
            PopulateFileLinks(files);
            return files.ToLookup(x => x.idItem);
        }
        public static void SetFileDinhKem(ApplicationDbContext db, List<HRM_FileDinhKem> listFile, string eTable, string IdTable)
        {
            //Giải quyết trường hợp để lấy thời gian up và người up
            List<HRM_FileDinhKem> TepDinhKemDBs = db.HRM_FileDinhKem.AsNoTracking().Where(x => x.TableName == eTable.ToString() && (string.IsNullOrEmpty(IdTable) || (!string.IsNullOrEmpty(IdTable) && x.idItem == IdTable))).ToList();

            if (listFile != null && listFile.Count > 0)
            {

                foreach (var item in listFile)
                {
                    string sFileName = HostingEnvironmentCompat.PathUploadsGoc + item.FileNameGUI;
                    if (File.Exists(sFileName) == true && HostingEnvironmentCompat.PathUploadsGoc!= HostingEnvironmentCompat.PathUploads)
                    {
                        string destFile = Path.Combine(HostingEnvironmentCompat.PathUploads + item.FileNameGUI);

                        // Nếu file đã tồn tại ở thư mục đích thì xóa hoặc ghi đè
                        if (File.Exists(destFile))
                        {
                            File.Delete(destFile);
                        }

                        // Move file
                        File.Move(sFileName, destFile);

                    }
                    if (!string.IsNullOrEmpty(IdTable))
                        item.idItem = IdTable;
                    item.TableName = eTable.ToString();
                    if (string.IsNullOrEmpty(item.IdGuid))
                    {
                        HRM_FileDinhKem itemFind = db.HRM_FileDinhKem.FirstOrDefault(x => x.TableName == item.TableName && x.idItem == IdTable && x.FileNameGUI == item.FileNameGUI && x.FileName == item.FileName);
                        if (itemFind != null)
                            continue;
                        item.IdGuid = Guid.NewGuid().ToString();

                        db.HRM_FileDinhKem.Add(item);

                    }
                    else
                    {
                        HRM_FileDinhKem fileDinhKemDB = TepDinhKemDBs.Find(x => x.IdGuid == item.IdGuid);
                        db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    }
                    //}
                }
            }


            List<string> listId = listFile.Select(a => a.IdGuid).Distinct().ToList();
            List<HRM_FileDinhKem> TepDinhKems = db.HRM_FileDinhKem.Where(x => x.TableName == eTable.ToString() && x.idItem == IdTable && !listId.Contains(x.IdGuid)).ToList();
            List<string> listIdRemove = new List<string>();
            foreach (var item in TepDinhKems)
            {
                var fileDK = listFile.Find(x => x.IdGuid == item.IdGuid);
                if (fileDK == null)
                {
                    db.HRM_FileDinhKem.Remove(item);

                    //string sFile = (Path.Combine(Directory.GetCurrentDirectory(), "uploaded") ?? "") + item.FileNameGUI;
                    //if (File.Exists(sFile))
                    //    File.Delete(sFile);
                }
            }
            db.SaveChanges();
            //List<BM_PhanCongCongViec_FileDinhKem> listPhanCongRemove = db.BM_PhanCongCongViec_FileDinhKem.Where(x => listIdRemove.Contains(x.IdFileDinhKem)).ToList();
            //if (listPhanCongRemove.Count > 0)
            //    db.BM_PhanCongCongViec_FileDinhKem.RemoveRange(listPhanCongRemove);
            if (listFile != null && listFile.Count > 0)
            {
                foreach (HRM_FileDinhKem item in listFile)
                {

                    //string sFileName = Path.Combine(Directory.GetCurrentDirectory(), "uploaded") + item.FileNameGUI;
                    string sFileName = HostingEnvironmentCompat.PathUploads + item.FileNameGUI;
                    if (File.Exists(sFileName) == false)
                        sFileName = HostingEnvironmentCompat.PathUploads + item.FileNameGUI;
                    if (!string.IsNullOrEmpty(item.FileName))
                    {
                        item.Link = "/uploader/DownloadFile?filename=" + dungchung.Base64Encode(item.FileName)
                                + "&path=" + dungchung.Base64Encode(sFileName);
                        //item.Link = "/uploader/api/hdfiles?filename=" + Dungchung.Base64Encode(item.FileName)
                        //        + "&path=" + Dungchung.Base64Encode(sFileName);
                    }
                }
            }

        }
    }
}
