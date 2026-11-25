#nullable disable
using System.Globalization;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using HRM.Api.Models.NotificationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.Services;

/// <summary>
/// Lớp nền nghiệp vụ .NET 8 cho các module Service, inject CallWebService, IWorkflowEngine và HrmOptions qua DI.
/// </summary>
public partial class FuncBase : dungchung
{
    public CallWebService _CallWebService { get; set; }
    protected readonly IWorkflowEngine WorkflowEngine;
    protected readonly HrmOptions HrmOptions;
    protected readonly ILogger Logger;

    public FuncBase(
        CallWebService callWebService,
        IWorkflowEngine workflowEngine,
        IOptions<HrmOptions> options,
        ILogger logger = null)
    {
        _CallWebService = callWebService;
        WorkflowEngine = workflowEngine;
        HrmOptions = options.Value;
        Logger = logger;
    }

    protected string PathUploads => HrmOptions.PathUploads;

    protected void LogErrorFormat(string format, params object[] args)
    {
        Logger?.LogError(string.Format(format, args));
    }

    protected void LogError(string message, Exception ex = null)
    {
        if (ex != null) Logger?.LogError(ex, message);
        else Logger?.LogError(message);
    }

    public int nPageSize = 25;
    public int nPageSizeBaoCao100 = 100;
    public int nPageSizeBaoCao50 = 50;
    public string m_sShortDateTime = "MM/dd/yyyy";

    public bool CheckID(string input) => !string.IsNullOrEmpty(input) && input.Length > 5;

    public List<HRMFaceReponeModel> GetChamCongFace(HRMFaceModel model)
        => _CallWebService.GetChamCongFace(model);

    public string AddNotification(AddNotificationNoUserModel model)
        => _CallWebService.AddNotification(model);

    public List<long> DequyPhongBan(long Id, List<BoPhan> LstBoPhanAll)
    {
        var tempListFolderId = new List<long>();
        var lstBoPhan = LstBoPhanAll.Where(r => r.BoPhanChaPkid == Id).ToList();
        if (lstBoPhan.Count > 0)
        {
            var lstIdBoPhan = lstBoPhan.Select(r => r.Id).ToList();
            foreach (var item in lstIdBoPhan)
            {
                tempListFolderId.AddRange(DequyPhongBan(item, LstBoPhanAll));
            }
            tempListFolderId.AddRange(lstIdBoPhan);
        }
        return tempListFolderId;
    }

    public string ReadNum(double value, string phancachphannghin = ".")
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "";
        nfi.NumberDecimalSeparator = ",";

        string number = value.ToString(nfi);
        if (number.Contains(','))
        {
            var parts = number.Split(',');
            return ReadNum(Convert.ToDouble(parts[0])) + " phẩy " + ReadNum(Convert.ToDouble(parts[1]));
        }

        string[] dv = { "", "mươi", "trăm", "nghìn", "triệu", "tỉ" };
        string[] cs = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

        int len = number.Length;
        number += "ss";
        string doc = "";
        int rd = 0, i = 0;

        while (i < len)
        {
            int n = (len - i + 2) % 3 + 1;
            int found = 0;
            for (int j = 0; j < n; j++)
            {
                if (number[i + j] != '0')
                {
                    found = 1;
                    break;
                }
            }

            if (found == 1)
            {
                rd = 1;
                for (int j = 0; j < n; j++)
                {
                    int ddv = 1;
                    switch (number[i + j])
                    {
                        case '0':
                            if (n - j == 3) doc += cs[0] + " ";
                            if (n - j == 2)
                            {
                                if (number[i + j + 1] != '0') doc += "linh ";
                                ddv = 0;
                            }
                            break;
                        case '1':
                            if (n - j == 3) doc += cs[1] + " ";
                            if (n - j == 2)
                            {
                                doc += "mười ";
                                ddv = 0;
                            }
                            if (n - j == 1)
                            {
                                int k = (i + j == 0) ? 0 : i + j - 1;
                                doc += (number[k] != '1' && number[k] != '0') ? "mốt " : cs[1] + " ";
                            }
                            break;
                        case '5':
                            doc += ((i + j == len - 1) || (i + j + 3 == len - 1)) ? "lăm " : cs[5] + " ";
                            break;
                        default:
                            doc += cs[number[i + j] - 48] + " ";
                            break;
                    }

                    if (ddv == 1)
                    {
                        doc += ((n - j) != 1) ? dv[n - j - 1] + " " : dv[n - j - 1];
                    }
                }
            }

            if (len - i - n > 0)
            {
                if ((len - i - n) % 9 == 0)
                {
                    if (rd == 1)
                        for (int k = 0; k < (len - i - n) / 9; k++)
                            doc += "tỉ ";
                    rd = 0;
                }
                else if (found != 0)
                {
                    doc += dv[((len - i - n + 1) % 9) / 3 + 2] + " ";
                }
            }

            i += n;
        }

        if (len == 1 && (number[0] == '0' || number[0] == '5'))
            return cs[number[0] - 48];

        return doc.TrimEnd();
    }

    protected static IQueryable<T> FilterWorkflowByTab<T>(
        IQueryable<T> query,
        ApplicationDbContext db,
        int nTabTrangThai)
        where T : IGuidKeyEntity
    {
        if (nTabTrangThai == 3)
            return query;

        var qtQuery = db.HRM_QuyTrinhXuLy.Where(qt => qt.IdUser == db.IdUser);
        if (nTabTrangThai == 1)
            qtQuery = qtQuery.Where(qt => qt.isDaXuLy == 0);
        else if (nTabTrangThai == 2)
            qtQuery = qtQuery.Where(qt => qt.isDaXuLy == 1);

        return from dm in query
               join qt in qtQuery on dm.IdGuid equals qt.IdTable
               select dm;
    }

    protected static List<T> Paginate<T>(
        IQueryable<T> query,
        int currentPage,
        int pageSize,
        out PagingInfor paging)
    {
        int effectivePageSize = pageSize > 0 ? pageSize : 25;
        int totalItem = query.Count();
        int totalPage = (int)Math.Ceiling((double)totalItem / effectivePageSize);
        paging = new PagingInfor(currentPage, totalItem, totalPage);

        int skip = currentPage > 1 ? (currentPage - 1) * effectivePageSize : 0;
        return query.Skip(skip).Take(effectivePageSize).ToList();
    }

    protected static List<T> GetPagedWorkflowList<T>(
        IQueryable<T> query,
        ApplicationDbContext db,
        int tabTrangThai,
        int currentPage,
        int pageSize,
        out PagingInfor paging)
        where T : class, IAuditableEntity, IGuidKeyEntity
    {
        var filtered = FilterWorkflowByTab(query, db, tabTrangThai);
        if (tabTrangThai == -1)
        {
            paging = new PagingInfor(currentPage, filtered.Count(), 0);
            return new List<T>();
        }

        return Paginate(filtered.OrderByDescending(x => x.Created), currentPage, pageSize, out paging);
    }

    protected static string GenerateNextCodeWithPrefix(
        IQueryable<string> existingCodes,
        string prefixWithDate,
        int parseDigits,
        int padDigits)
    {
        int maxNumber = existingCodes
            .Where(code => code != null && code.StartsWith(prefixWithDate))
            .AsEnumerable()
            .Select(code => int.TryParse(code.Length >= parseDigits ? code[^parseDigits..] : code, out int n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefixWithDate}{(maxNumber + 1).ToString($"D{padDigits}")}";
    }

    protected static string GenerateNextCode(IQueryable<string> existingCodes, string prefix, int digits = 4)
        => GenerateNextCodeWithPrefix(existingCodes, $"{prefix}{DateTime.Now:MMyyyy}_", digits, digits);

    protected static string GenerateNextCodeFromLast(string prefix, string lastCode, int digits = 6)
    {
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var parts = lastCode.Split('_');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }
        return $"{prefix}{DateTime.Now:MMyyyy}_{nextNumber.ToString($"D{digits}")}";
    }

    protected static dynamic CreateSoPhieuResult(string code)
    {
        dynamic result = new DynamicObject();
        result.SoPhieu = code;
        return result;
    }

    protected MessageReturn ExecuteInTransaction(ApplicationDbContext db, Func<MessageReturn> action)
    {
        using var tx = db.Database.BeginTransaction();
        try
        {
            var result = action();
            if (result.Error == eErrorMessage.CapNhatThanhCong)
                tx.Commit();
            else
                tx.Rollback();
            return result;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            LogError("Transaction error", ex);
            return MessageReturn.Fail($"Có lỗi xảy ra trong quá trình xử lý: {ex.Message}");
        }
    }

    public dynamic GetListFileDinhKem(ApplicationDbContext db, string TableName, string IdTable)
    {
        var tepDinhKems = db.HRM_FileDinhKem.Where(x => x.TableName == TableName && x.idItem == IdTable).ToList();
        foreach (var item in tepDinhKems)
        {
            string sFileName = HostingEnvironmentCompat.MapPath("/uploaded/") + item.FileNameGUI;
            if (item.FileNameGUI != null)
            {
                item.Link = "/uploader/api/hdfiles?filename=" + Base64Encode(item.FileName) + "&path=" + Base64Encode(sFileName);
            }
        }
        return tepDinhKems;
    }

    private static bool ApplyWorkflowResult(MessageReturn result, MessageReturn err)
    {
        if (result == null)
        {
            if (err != null) err.Error = eErrorMessage.LoiCapNhat;
            return false;
        }
        bool ok = result.Error == eErrorMessage.CapNhatThanhCong;
        if (!ok && err != null)
        {
            err.Error = result.Error;
            err.StatusCode = result.StatusCode;
            err.Detail = result.Detail;
            err.Value = result.Value;
        }
        return ok;
    }

    public TrangThaiQuyTrinh GetTrangThaiChuyenTiep(
        ApplicationDbContext db,
        string eAction,
        string IdTrangThai,
        string IdTable,
        long IdDuAn = 0,
        string NoiDung = "")
        => _CallWebService.GetTrangThaiChuyenTiep(db, eAction, IdTrangThai, IdTable, IdDuAn, NoiDung);

    public dynamic GetTrangThaiHuy(ApplicationDbContext db, string eAction, string IdTable, string NoiDung = "")
        => _CallWebService.GetTrangThaiHuy(db, eAction, IdTable, NoiDung);

    public bool GetQuyTrinh(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entity, MessageReturn err, bool isXemQuyTrinh)
        => ApplyWorkflowResult(WorkflowEngine.LoadWorkflowStateAsync(entity, isXemQuyTrinh).GetAwaiter().GetResult(), err);

    public bool KhoiTaoQuyTrinh(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entity, MessageReturn err)
    {
        if (entity != null) entity.IdGuid = Guid.NewGuid().ToString();
        return ApplyWorkflowResult(WorkflowEngine.InitializeWorkflowAsync(entity).GetAwaiter().GetResult(), err);
    }

    public bool SetTruocHamUpdate(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entityDB, AudiQuyTrinh entity, MessageReturn err)
        => ApplyWorkflowResult(WorkflowEngine.BeforeSaveAsync(entityDB, entity).GetAwaiter().GetResult(), err);

    public bool SetSauHamUpdate(ApplicationDbContext _dbContext, AudiQuyTrinh entityDB, AudiQuyTrinh entity, MessageReturn err)
        => ApplyWorkflowResult(WorkflowEngine.AfterSaveAsync(entityDB, entity).GetAwaiter().GetResult(), err);

    public bool SetQuyTrinh(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entity, eQuyTrinh_LoaiHam eLoaiHam, MessageReturn err)
        => entity != null && WorkflowEngine.SyncWorkflowAsync(entity.IdGuid).GetAwaiter().GetResult();

    public bool ChuyenTiepTruocHamUpdate(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entityDB, AudiQuyTrinh entity, MessageReturn err, long IdDuAn)
        => ApplyWorkflowResult(WorkflowEngine.BeforeForwardAsync(entityDB, entity, IdDuAn).GetAwaiter().GetResult(), err);

    public bool ChuyenTiepSauHamUpdate(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entityDB, AudiQuyTrinh entity, string IdHopDong, MessageReturn err)
        => ApplyWorkflowResult(WorkflowEngine.AfterForwardAsync(entityDB, entity).GetAwaiter().GetResult(), err);

    public bool KhongDuyetTruocHamUpdate(ApplicationDbContext _dbContext, CallWebService _callWebService, AudiQuyTrinh entityDB, AudiQuyTrinh entity, MessageReturn err)
        => ApplyWorkflowResult(WorkflowEngine.BeforeRejectAsync(entityDB, entity).GetAwaiter().GetResult(), err);

    public bool KhongDuyetSauHamUpdate(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => WorkflowEngine.AfterRejectAsync(entity).GetAwaiter().GetResult().Error == eErrorMessage.CapNhatThanhCong;

    public bool XoaQuyTrinh(MessageReturn msg, CallWebService _callWebService, ApplicationDbContext _dbContext, string Id)
        => ApplyWorkflowResult(WorkflowEngine.DeleteWorkflowAsync(Id).GetAwaiter().GetResult(), msg);

    private static void MoveUploadedFile(string fileNameGui, bool createDestDir = false)
    {
        var src = Path.Combine(HostingEnvironmentCompat.PathUploads ?? "", fileNameGui ?? "");
        var dest = Path.Combine(HostingEnvironmentCompat.MapPath("/uploaded/") ?? "", fileNameGui ?? "");
        if (!File.Exists(src) || string.Equals(src, dest, StringComparison.OrdinalIgnoreCase))
            return;

        if (createDestDir)
        {
            var destDir = Path.GetDirectoryName(dest);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
        }
        File.Move(src, dest, overwrite: true);
    }

    private static void DeleteUploadedFile(string fileNameGui)
    {
        var sFile = Path.Combine(HostingEnvironmentCompat.MapPath("/uploaded/") ?? "", fileNameGui ?? "");
        if (File.Exists(sFile)) File.Delete(sFile);
    }

    private static void ApplyAttachmentChange(ApplicationDbContext db, HRM_FileDinhKem item)
    {
        if (item.IdGuid.IsEmpty())
        {
            item.IdGuid = Guid.NewGuid().ToString();
            db.HRM_FileDinhKem.Add(item);
            MoveUploadedFile(item.FileNameGUI);
        }
        else if (!item.isDeleted)
        {
            db.Entry(item).State = EntityState.Modified;
        }
        else
        {
            db.Entry(item).State = EntityState.Deleted;
            DeleteUploadedFile(item.FileNameGUI);
        }
    }

    public void SetFileDinhKem(ApplicationDbContext db, HRM_FileDinhKem item, eTable eTable, string nIDBang, MessageReturn err = null)
    {
        try
        {
            if (item == null) return;
            item.TableName = eTable.ToString();
            item.idItem = nIDBang;
            if (item.isDeleted)
            {
                if (!KiemTraInsertOrUpdate(item.IdGuid))
                    db.Entry(item).State = EntityState.Deleted;
            }
            else if (KiemTraInsertOrUpdate(item.IdGuid))
            {
                var itemFind = db.HRM_FileDinhKem.FirstOrDefault(x => x.TableName == item.TableName && x.idItem == nIDBang && x.FileNameGUI == item.FileNameGUI);
                if (itemFind != null) return;
                db.HRM_FileDinhKem.Add(item);
                MoveUploadedFile(item.FileNameGUI, createDestDir: true);
            }
            else
            {
                db.Entry(item).State = EntityState.Modified;
            }
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            LogError("SetFileDinhKem error", ex);
            if (err != null)
            {
                err.Error = eErrorMessage.LoiCapNhat;
                err.Detail = ex.ToString();
            }
        }
    }

    public MessageReturn SetFileDinhKem(ApplicationDbContext aContext, HRM_FileDinhKem aItem, string aTableName, string aIdTable, MessageReturn aError)
    {
        if (aIdTable.IsEmpty())
            return new MessageReturn(eErrorMessage.LoiLayDuLieu, "Thiếu dữ liệu Id bảng của File đính kèm");
        aItem.idItem = aIdTable;
        aItem.TableName = aTableName;
        ApplyAttachmentChange(aContext, aItem);
        return aError;
    }

    public MessageReturn SetFileDinhKem(ApplicationDbContext aContext, List<HRM_FileDinhKem> aListValue, MessageReturn aError)
    {
        if (aListValue.IsNotEmpty())
        {
            foreach (var item in aListValue)
                ApplyAttachmentChange(aContext, item);
        }
        return aError;
    }

    public dynamic LogLichSuNhanVien(ApplicationDbContext db, string IdNhanVien, string IdTable, string TableName, string QuyTrinh, double ThoiGianUnix = 0, string NoiDung = "")
    {
        try
        {
            string tenQuyTrinh = !string.IsNullOrEmpty(NoiDung) ? NoiDung : QuyTrinh switch
            {
                "QUYTRINHHOPDONGNHANSU" => "Ký kết hợp đồng",
                "QUYTRINHQUYETDINHKYLUAT" => "Quyết định kỷ luật",
                "QUYTRINHQUYETDINHSATHAI" => "Quyết định sa thải",
                "QUYTRINHDIEUCHINHLUONG" => "Quyết định điều chỉnh lương",
                "QUYTRINHQUYETDINHKHENTHUONG" => "Quyết định khen thưởng",
                "QUYTRINHBONHIEMCHUCVU" => "Bổ nhiệm chức vụ",
                "QUYTRINHDIMUONVESOM" => "Xin đi muôn về sớm",
                "QUYTRINHBONHIEMCHUCVUDUAN" => "Bổ nhiệm điều động dự án",
                "QUYTRINHXINNGHIPHEP" => "Xin nghỉ phép",
                "QUYTRINHLAMTHEMGIO" => "Đăng ký làm thêm giờ",
                "QUYTRINHQUENCHAMCONG" => "Quên chấm công",
                "QUYTRINHNGHIVIEC" => "Xin nghỉ việc",
                "CHUONGTRINHDAOTAO" => "Chương trình đào tạo",
                "XACNHANTANGGIAMBAOHIEM" => "Xác nhận tăng giảm bảo hiểm",
                "QUYTRINHBONHIEMDIEUDONG" => "Bổ nhiệm điều động",
                _ => string.Empty
            };

            db.HRM_NhatKyNhanVien.Add(new HRM_NhatKyNhanVien
            {
                IdGuid = Guid.NewGuid().ToString(),
                IdNhanVien = IdNhanVien,
                IdTable = IdTable,
                TableName = TableName,
                eAction = QuyTrinh,
                QuyTrinh = tenQuyTrinh,
                ThoiGian = dungchung.UnixTimeStampToDateTime(ThoiGianUnix),
            });
            db.SaveChanges();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
