#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Models.NotificationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HRM.Api.Services;

/// <summary>
/// Facade tÃ†Â°Ã†Â¡ng thÃƒÂ­ch ngÃ†Â°Ã¡Â»Â£c cho toÃƒÂ n bÃ¡Â»â„¢ lÃ¡Â»Âi gÃ¡Â»Âi _CallWebService trÃƒÂªn .NET 8:
/// Ã¡Â»Â§y quyÃ¡Â»Ân sang ISmartEosOrganizationClient, ISmartEosWorkflowClient, ISmartEosFaceClient vÃƒÂ  IWorkflowEngine Ã„â€˜Ã†Â°Ã¡Â»Â£c inject qua DI.
/// </summary>
public partial class CallWebService
{
    private const string RouterSmartEOS = "SmartEOSAPI/";
    private const string RouterQuyTrinh = "SmartEOSAPI/QuanTriQuyTrinh/";
    private const string RouterNhanSu = "SmartEOSAPI/CoCauNhanSu/";
    private const string RouterDanhMuc = "SmartEOSAPI/DanhMuc/";
    private const string RouterQuanTri = "SmartEOSAPI/QuanTri/";
    private const string RouterTuHoSo = "SmartEOSAPI/TuHoSo/";

    private readonly ISmartEosAuthClient _authClient;
    private readonly ISmartEosOrganizationClient _orgClient;
    private readonly ISmartEosWorkflowClient _workflowClient;
    private readonly ISmartEosFaceClient _faceClient;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly HrmOptions _options;
    private readonly ILogger<CallWebService> _logger;

    public CallWebService(
        ISmartEosAuthClient authClient,
        ISmartEosOrganizationClient orgClient,
        ISmartEosWorkflowClient workflowClient,
        ISmartEosFaceClient faceClient,
        IWorkflowEngine workflowEngine,
        IOptions<HrmOptions> options,
        ILogger<CallWebService> logger)
    {
        _authClient = authClient;
        _orgClient = orgClient;
        _workflowClient = workflowClient;
        _faceClient = faceClient;
        _workflowEngine = workflowEngine;
        _options = options.Value;
        _logger = logger;
    }

    public class LM_Modify : HRM.Api.Common.LM_Modify
    {
    }

    public class KiemTraTabview
    {
        public bool ThemMoi { get; set; }
    }

    public class TrangThaiQuyTrinhThietLap
    {
        public string IdTrangThai { get; set; }
        public string TenTrangThai { get; set; }
    }

    public class PSM_DuAn
    {
        public long Id { get; set; }
        public string SoPhieu { get; set; }
        public string TenDuAn { get; set; }
        public string MaDuAn { get; set; }
        public string idUserQuanLyDuAn { get; set; }
        public string IdTrangThai { get; set; }
        public string GhiChu { get; set; }
        public string DiaChi { get; set; }
    }

    public class HRM_TimKiemkeHoach
    {
        public long IdDuAn { get; set; }
        public double NgayUnix { get; set; }
    }

    private T Deserialize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return default;
        try { return JsonConvert.DeserializeObject<T>(content); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Deserialize failed for {Type}", typeof(T).Name);
            return default;
        }
    }

    public TrangThaiQuyTrinh GetTrangThaiChuyenTiep(ApplicationDbContext db, string eAction, string IdTrangThai, string IdTable, long IdDuAn = 0, string NoiDung = "")
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiChuyenTiep?IdUser={Uri.EscapeDataString(db.IdUser)}&IdTrangThai={Uri.EscapeDataString(IdTrangThai ?? "")}&IdTable={Uri.EscapeDataString(IdTable ?? "")}&NoiDung={Uri.EscapeDataString(NoiDung ?? "")}";
        var content = _orgClient.CallGetAsync(url).GetAwaiter().GetResult();
        var item = Deserialize<TrangThaiQuyTrinh>(content) ?? new TrangThaiQuyTrinh();
        if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(db, IdTable))
        {
            return new TrangThaiQuyTrinh();
        }
        return item;
    }

    public dynamic GetTrangThaiHuy(ApplicationDbContext db, string eAction, string IdTable, string NoiDung = "")
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiHuy?IdUser={Uri.EscapeDataString(db.IdUser)}&eAction={Uri.EscapeDataString(eAction ?? "")}&IdTable={Uri.EscapeDataString(IdTable ?? "")}&NoiDung={Uri.EscapeDataString(NoiDung ?? "")}";
        var content = _orgClient.CallGetAsync(url).GetAwaiter().GetResult();
        var item = Deserialize<TimKiem>(content) ?? new TimKiem();
        if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(db, IdTable))
        {
            return new TimKiem();
        }
        return item;
    }

    public TrangThaiQuyTrinh GetTrangThaiKhongDuyet(ApplicationDbContext db, string eAction, string IdTrangThai, string IdTable, long IdDuAn = 0, string NoiDung = "")
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiKhongDuyet?IdUser={Uri.EscapeDataString(db.IdUser)}&IdTrangThai={Uri.EscapeDataString(IdTrangThai ?? "")}&IdTable={Uri.EscapeDataString(IdTable ?? "")}&NoiDung={Uri.EscapeDataString(NoiDung ?? "")}";
        var content = _orgClient.CallGetAsync(url).GetAwaiter().GetResult();
        var item = Deserialize<TrangThaiQuyTrinh>(content) ?? new TrangThaiQuyTrinh();
        if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(db, IdTable))
        {
            return new TrangThaiQuyTrinh();
        }
        return item;
    }

    public List<HRMFaceReponeModel> GetChamCongFace(HRMFaceModel model)
    {
        var data = _faceClient.GetChamCongFaceAsync(model).GetAwaiter().GetResult();
        return data.Select(x => new HRMFaceReponeModel
        {
            IdUser = x.IdUser,
            IdFcaeNhanVien = x.IdFcaeNhanVien,
            IdNhanVienEOS = x.IdNhanVienEOS,
            NgayChamCong = x.NgayChamCong,
            ThoiGianChamCong = x.ThoiGianChamCong
        }).ToList();
    }

    public string AddNotification(AddNotificationNoUserModel model)
        => _orgClient.CallPostAsync("SmartEOSAPI/Notification/AddNotificationNoLogin", model).GetAwaiter().GetResult() ?? string.Empty;

    public List<HRM_hungGio> GetDanhSachKhungGioByListIdCaLamViec(ApplicationDbContext _dbContext, List<string> listId)
    {
        var content = _orgClient.CallPostAsync("SmartEOSAPI/DanhMuc/GetDanhSachKhungGioByListIdCaLamViec", listId).GetAwaiter().GetResult();
        return Deserialize<List<HRM_hungGio>>(content) ?? new List<HRM_hungGio>();
    }

    public List<QuyTrinhTrangThai> GetListTenTrangThai(List<string> listIdTrangThai)
    {
        var content = _orgClient.CallPostAsync($"{RouterQuyTrinh}GetListTenTrangThai", listIdTrangThai).GetAwaiter().GetResult();
        return Deserialize<List<QuyTrinhTrangThai>>(content) ?? new List<QuyTrinhTrangThai>();
    }

    public List<QT_ThietLapQuyTrinh> GetListdsThietLapQuyTrinh()
    {
        var content = _orgClient.CallPostAsync("SmartEOSAPI/HanhChinhVanPhong/GetListdsThietLapQuyTrinh", null).GetAwaiter().GetResult();
        return Deserialize<List<QT_ThietLapQuyTrinh>>(content) ?? new List<QT_ThietLapQuyTrinh>();
    }

    public List<QuyTrinhTrangThai> GetListQuyTrinhTrangThai()
    {
        var content = _orgClient.CallGetAsync($"{RouterQuyTrinh}GetListQuyTrinhTrangThai").GetAwaiter().GetResult();
        return Deserialize<List<QuyTrinhTrangThai>>(content) ?? new List<QuyTrinhTrangThai>();
    }

    public string GetTrangThaiCuoi(ApplicationDbContext db, string eAction)
    {
        var content = _orgClient.CallGetAsync($"{RouterQuyTrinh}GetTrangThaiCuoi?eAction={Uri.EscapeDataString(eAction ?? "")}").GetAwaiter().GetResult();
        return Deserialize<TrangThaiQuyTrinhThietLap>(content)?.IdTrangThai;
    }

    public clsButton KiemTraButton(ApplicationDbContext _dbContext, string IdTable, string IdTrangThai)
    {
        var url = $"{RouterQuyTrinh}KiemTraButton?IdTrangThai={Uri.EscapeDataString(IdTrangThai ?? "")}&IdTable={Uri.EscapeDataString(IdTable ?? "")}";
        var content = _orgClient.CallGetAsync(url).GetAwaiter().GetResult();
        return Deserialize<clsButton>(content) ?? new clsButton();
    }

    public dynamic GetDanhSachUser()
        => _orgClient.CallGetAsync($"{RouterSmartEOS}QuanTri/GetDanhSachUserForHopDong?isFull=true").GetAwaiter().GetResult() ?? string.Empty;

    public dynamic AddlistChucVuNhanSu(ApplicationDbContext _dbContext, List<HRM.Api.Models.Link_NhanVien_BoPhan_ChucVuNonSQL> entity)
        => _orgClient.CallPostAsync($"{RouterNhanSu}AddlistChucVuNhanSu", entity).GetAwaiter().GetResult() ?? string.Empty;

    public List<BoPhan> GetDanhSachBoPhanByUser(ApplicationDbContext _dbContext)
    {
        var url = $"{RouterNhanSu}GetDanhSachBoPhanByUser?IdUser={Uri.EscapeDataString(_dbContext.currentUser?.Id ?? _dbContext.IdUser)}";
        var content = _orgClient.CallGetAsync(url).GetAwaiter().GetResult();
        return (Deserialize<List<BoPhan>>(content) ?? new List<BoPhan>()).Where(a => a.LoaiBoPhanPkid != 3).ToList();
    }

    public List<PhongBanView> GetListPhongBan(ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachBoPhan").GetAwaiter().GetResult();
        return Deserialize<List<PhongBanView>>(content) ?? new List<PhongBanView>();
    }

    public List<NhanSu> GetListNhanSu(ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetBoPhanNhanSu").GetAwaiter().GetResult();
        return Deserialize<List<NhanSu>>(content) ?? new List<NhanSu>();
    }

    public List<clsNhanVien> AddDanhSachNhanSu(ApplicationDbContext _dbContext, List<HRM_NhanVien> listNhanVien)
    {
        var listOut = new List<clsNhanVien>();
        var batch = new List<clsNhanVien>();
        foreach (var nv in listNhanVien)
        {
            batch.Add(new clsNhanVien(nv));
            if (batch.Count == 20)
            {
                SendBatch(batch, listOut);
                batch.Clear();
            }
        }
        if (batch.Count > 0) SendBatch(batch, listOut);
        return listOut;

        void SendBatch(List<clsNhanVien> items, List<clsNhanVien> acc)
        {
            var content = _orgClient.CallPostAsync($"{RouterNhanSu}AddDanhSachNhanSu", items).GetAwaiter().GetResult();
            var msg = Deserialize<MessageReturn>(content);
            if (msg?.Error == eErrorMessage.CapNhatThanhCong && msg.Value != null)
            {
                var added = JsonConvert.DeserializeObject<List<clsNhanVien>>(msg.Value.ToString());
                if (added != null) acc.AddRange(added);
            }
        }
    }

    public clsNhanVien SetNhanSu(ApplicationDbContext _dbContext, HRM_NhanVien aModel)
    {
        var content = _orgClient.CallPostAsync($"{RouterNhanSu}SetNhanSu", new clsNhanVien(aModel)).GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        if (msg?.Error == eErrorMessage.CapNhatThanhCong && msg.Value != null)
            return JsonConvert.DeserializeObject<clsNhanVien>(msg.Value.ToString()) ?? new clsNhanVien();
        return new clsNhanVien();
    }

    public clsNhanVien SetNhanSuCreateUser(ApplicationDbContext _dbContext, HRM_NhanVien aModel)
    {
        var content = _orgClient.CallPostAsync($"{RouterNhanSu}SetNhanSuCreateUser", new clsNhanVien(aModel)).GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        if (msg?.Error == eErrorMessage.CapNhatThanhCong && msg.Value != null)
            return JsonConvert.DeserializeObject<clsNhanVien>(msg.Value.ToString()) ?? new clsNhanVien();
        return new clsNhanVien();
    }

    public void SetNhanSu_DuAn(ApplicationDbContext _dbContext, clsNhanSu_DuAn aModel)
        => _orgClient.CallPostAsync("SmartEOSAPI/DanhMuc/AddListUserNhanSuDuAnHopDong", aModel).GetAwaiter().GetResult();

    public bool KiemTraTab(ApplicationDbContext _dbContext, string eAction)
    {
        var content = _orgClient.CallGetAsync($"{RouterQuyTrinh}KiemTraTab?eAction={Uri.EscapeDataString(eAction ?? "")}").GetAwaiter().GetResult();
        return Deserialize<KiemTraTabview>(content)?.ThemMoi ?? false;
    }

    public void AddModify(ApplicationDbContext _dbContext, string TenChucNang, string TrangThai, string eTable, string NoiDung, string IdQuyTrinh, string DataTruoc, string DataSau, long? IdDuAn, string IdUser, string Username)
    {
        _workflowClient.AddModifyAsync(new HRM.Api.Common.LM_Modify
        {
            TenChucNang = TenChucNang,
            TrangThai = TrangThai,
            eTable = eTable,
            NoiDung = NoiDung,
            IdQuyTrinh = IdQuyTrinh,
            DataTruoc = DataTruoc,
            DataSau = DataSau,
            IdDuAn = IdDuAn ?? 0,
            IdUser = IdUser,
            UserName = Username
        }).GetAwaiter().GetResult();
    }

    public void CreateLog(ApplicationDbContext _dbContext, string FuntionName, string IdUser, string Username)
        => _workflowClient.CreateLogAsync(FuntionName, IdUser, Username).GetAwaiter().GetResult();

    public clsLink_NhanVien_BoPhan_ChucVu UpdateChucVuNhanSu(ApplicationDbContext _dbContext, HRM_HopDong aModel)
        => PostChucVuNhanSu(new clsLink_NhanVien_BoPhan_ChucVu(aModel));

    public clsLink_NhanVien_BoPhan_ChucVu UpdateChucVuNhanSuNhanVien(ApplicationDbContext _dbContext, long IdNhanVienEOS, long IdCongTyEOS, long IdBoPhanEOS, long IddmChucVu, DateTime NgayHieuLuc, DateTime? NgayHetHan)
        => PostChucVuNhanSu(new clsLink_NhanVien_BoPhan_ChucVu
        {
            NhanVienPkid = IdNhanVienEOS,
            IdCongTy = IdCongTyEOS,
            BoPhanPkid = IdBoPhanEOS,
            ChucVuPkid = IddmChucVu,
            NgayBatDau = NgayHieuLuc,
            NgayKetThuc = NgayHetHan
        });

    public clsLink_NhanVien_BoPhan_ChucVu AddChucVuNhanSu(ApplicationDbContext _dbContext, HRM_HopDong aModel)
        => PostChucVuNhanSu(new clsLink_NhanVien_BoPhan_ChucVu(aModel));

    private clsLink_NhanVien_BoPhan_ChucVu PostChucVuNhanSu(clsLink_NhanVien_BoPhan_ChucVu payload)
    {
        var content = _orgClient.CallPostAsync($"{RouterNhanSu}AddChucVuNhanSu", payload).GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        if (msg?.Error == eErrorMessage.CapNhatThanhCong && !string.IsNullOrEmpty(content))
            return Deserialize<clsLink_NhanVien_BoPhan_ChucVu>(content) ?? new clsLink_NhanVien_BoPhan_ChucVu();
        return new clsLink_NhanVien_BoPhan_ChucVu();
    }

    public void UpdateAnhDaiDienByMultiUser(ApplicationDbContext _dbContext, List<HRM_FileDinhKem> aModel)
        => _orgClient.CallPostAsync($"{RouterQuanTri}UpdateAnhDaiDienByMultiUser", aModel).GetAwaiter().GetResult();

    public List<clsLink_NhanVien_BoPhan_ChucVu> GetDanhSachChucVu_BoPhan(ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachChucVu_BoPhan").GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        if (msg?.Error == eErrorMessage.CapNhatThanhCong && !string.IsNullOrEmpty(content))
            return Deserialize<List<clsLink_NhanVien_BoPhan_ChucVu>>(content) ?? new();
        return new();
    }

    public List<Link_NhanVien_BoPhan_ChucVu> GetDanhSachChucVuTheoListIdNhanVien(ApplicationDbContext _dbContext, List<long> listIdNhanVienEOS)
    {
        var content = _orgClient.CallPostAsync($"{RouterNhanSu}GetDanhSachChucVuTheoListIdNhanVien", listIdNhanVienEOS).GetAwaiter().GetResult();
        return Deserialize<List<Link_NhanVien_BoPhan_ChucVu>>(content) ?? new();
    }

    public string GetTenChucVu_BoPhan(ApplicationDbContext _dbContext, long IdNhanVienEOS)
    {
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetTenChucVu_BoPhan?IdNhanVien={IdNhanVienEOS}").GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        return msg?.Error == eErrorMessage.CapNhatThanhCong ? (content ?? "") : "";
    }

    public List<clsNhanVien> GetBoPhanNhanSu(ApplicationDbContext _dbContext)
        => Deserialize<List<clsNhanVien>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetBoPhanNhanSu").GetAwaiter().GetResult()) ?? new();

    public List<clsNhanVien> GetDanhSachNhanSuTheoBoPhan(ApplicationDbContext _dbContext, long IdPhongBan)
        => Deserialize<List<clsNhanVien>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachNhanVienByIdBoPhan?IdBoPhan={IdPhongBan}").GetAwaiter().GetResult()) ?? new();

    public List<clsNhanVien> GetDanhSachNhanVien(ApplicationDbContext _dbContext)
        => Deserialize<List<clsNhanVien>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachNhanVien").GetAwaiter().GetResult()) ?? new();

    public BoPhan GetBoPhan(ApplicationDbContext _dbContext, long Id)
    {
        var item = Deserialize<BoPhan>(_orgClient.CallGetAsync($"{RouterNhanSu}GetBoPhanNoneUser?Id={Id}").GetAwaiter().GetResult()) ?? new BoPhan();
        if (_options.Host == "ICT")
        {
            item.MaSoThue = _options.MaSoThue;
            item.DiaChi = _options.DiaChi;
        }
        return item;
    }

    public List<BoPhan> GetDanhSachBoPhanFromListId(ApplicationDbContext _dbContext, List<long> listId)
        => Deserialize<List<BoPhan>>(_orgClient.CallPostAsync($"{RouterNhanSu}GetDanhSachBoPhanFromListId", listId).GetAwaiter().GetResult()) ?? new();

    public List<ChucDanh> GetDanhSachChucDanhFromListId(ApplicationDbContext _dbContext, List<long> listId)
        => Deserialize<List<ChucDanh>>(_orgClient.CallPostAsync($"{RouterNhanSu}GetDanhSachChucDanhFromListId", listId).GetAwaiter().GetResult()) ?? new();

    public List<clsUser> GetDanhSachUserTheoListIdUser(ApplicationDbContext _dbContext, List<string> listIdUser)
        => _orgClient.GetDanhSachUserByIdsAsync(listIdUser).GetAwaiter().GetResult();

    public List<BoPhan> GetDanhSachBoPhan(ApplicationDbContext _dbContext)
        => _orgClient.GetDanhSachBoPhanAsync(useCache: true).GetAwaiter().GetResult();

    public string GetJwtTokenForAPI()
        => _authClient.GetSystemJwtTokenAsync().GetAwaiter().GetResult() ?? string.Empty;

    public List<BoPhan> GetDanhSachDuAnDaDuyet(ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterDanhMuc}GetDanhSachDuAnDaDuyet").GetAwaiter().GetResult();
        var listData = Deserialize<List<HRM_DuAn>>(content) ?? new();
        return listData.Select(a => new BoPhan
        {
            Id = a.Id,
            MaBoPhan = a.MaDuAn,
            TenBoPhan = a.TenDuAn,
            DiaChi = a.DiaChi,
            IddmCaLamViec = a.IddmCaLamViec,
            isNhaMay = a.isNhaMay,
            IddmLoaiDuAn = a.IddmLoaiDuAn
        }).OrderBy(a => a.MaBoPhan).ToList();
    }

    public List<HRM.Api.Common.DanhMuc> GetListdmLoaiDuAn(ApplicationDbContext _dbContext)
        => Deserialize<List<HRM.Api.Common.DanhMuc>>(_orgClient.CallPostAsync($"{RouterDanhMuc}GetListdmLoaiDuAn", null).GetAwaiter().GetResult()) ?? new();

    public List<BoPhan> GetDanhSachDuAnQuanLyHoSoNhanVien(ApplicationDbContext _dbContext, string IdUser = "")
    {
        if (string.IsNullOrEmpty(IdUser)) IdUser = _dbContext.currentUser?.Id ?? _dbContext.IdUser;
        var content = _orgClient.CallGetAsync($"{RouterDanhMuc}GetDanhSachDuAnQuanLyHoSoNhanVien?IdUser={Uri.EscapeDataString(IdUser ?? "")}").GetAwaiter().GetResult();
        var listData = Deserialize<List<HRM_DuAn>>(content) ?? new();
        return listData.Select(a => new BoPhan
        {
            Id = a.Id,
            MaBoPhan = a.MaDuAn,
            TenBoPhan = a.TenDuAn,
            DiaChi = a.DiaChi,
            IddmCaLamViec = a.IddmCaLamViec,
            isNhaMay = a.isNhaMay,
            IddmLoaiDuAn = a.IddmLoaiDuAn
        }).ToList();
    }

    public List<BoPhan> GetDanhSachCongTy(ApplicationDbContext _dbContext, string IdUser = "")
        => _orgClient.GetDanhSachCongTyAsync(string.IsNullOrEmpty(IdUser) ? _dbContext.currentUser?.Id : IdUser, useCache: true).GetAwaiter().GetResult();

    public List<BoPhan> GetDanhSachDuAn(ApplicationDbContext _dbContext)
        => _orgClient.GetDanhSachDuAnAsync(useCache: true).GetAwaiter().GetResult();

    public BoPhan GetQuyTrinhDuAn(long? IdDuAn, ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterDanhMuc}GetQuyTrinhDuAn?Id={IdDuAn}").GetAwaiter().GetResult();
        var errR = Deserialize<HRM_DuAn>(content);
        if (errR == null) return null;
        return new BoPhan
        {
            Id = errR.Id,
            MaBoPhan = errR.MaDuAn,
            TenBoPhan = errR.TenDuAn,
            DiaChi = errR.DiaChi,
            IddmCaLamViec = errR.IddmCaLamViec,
            isNhaMay = errR.isNhaMay,
            itemThongTinDuAn = errR.itemThongTinDuAn
        };
    }

    public HRM_DuAn GetThongTinCaLamViecDuAn(long? IdDuAn, ApplicationDbContext _dbContext)
        => Deserialize<HRM_DuAn>(_orgClient.CallGetAsync($"{RouterDanhMuc}GetThongTinCaLamViecDuAn?IdDuAn={IdDuAn}").GetAwaiter().GetResult());

    public List<clsNhanVien> GetDanhSachNhanVienKiemNghiemByCongTy(ApplicationDbContext _dbContext, List<long> listIdBoPhan)
        => Deserialize<List<clsNhanVien>>(_orgClient.CallPostAsync($"{RouterNhanSu}GetDanhSachNhanVienKiemNghiemByCongTy", listIdBoPhan).GetAwaiter().GetResult()) ?? new();

    public List<BoPhan> GetCongTyKyHopDong(ApplicationDbContext _dbContext)
    {
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachCongTy").GetAwaiter().GetResult();
        var listItem = Deserialize<List<BoPhan>>(content) ?? new();
        return listItem.Where(a => a.BoPhanChaPkid == 0).ToList();
    }

    public List<BoPhan> GetDanhSachBoPhanTheoCongTy(ApplicationDbContext _dbContext, long IdBoPhan)
        => Deserialize<List<BoPhan>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachBoPhanTheoCongTy?IdCongTy={IdBoPhan}").GetAwaiter().GetResult()) ?? new();

    public List<BoPhan> GetPhongBanTheoUser(ApplicationDbContext _dbContext, string IdUser)
        => Deserialize<List<BoPhan>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachBoPhanByUserNoneSql?IdUser={Uri.EscapeDataString(IdUser ?? "")}").GetAwaiter().GetResult()) ?? new();

    public ChucVu GetChucVu(ApplicationDbContext _dbContext, long Id)
        => Deserialize<ChucVu>(_orgClient.CallGetAsync($"{RouterNhanSu}GetChucVu?Id={Id}").GetAwaiter().GetResult()) ?? new ChucVu();

    public clsNhanVien GetNhanSu(ApplicationDbContext _dbContext, long Id)
        => Deserialize<clsNhanVien>(_orgClient.CallGetAsync($"{RouterNhanSu}GetNhanSu?Id={Id}").GetAwaiter().GetResult()) ?? new clsNhanVien();

    public clsNhanVien GetNhanSuFromUserPkid(ApplicationDbContext _dbContext, string _IdUser = "")
    {
        var id = string.IsNullOrEmpty(_IdUser) ? _dbContext.IdUser : _IdUser;
        return Deserialize<clsNhanVien>(_orgClient.CallGetAsync($"{RouterNhanSu}GetNhanSuFromUserPkid?Id={Uri.EscapeDataString(id ?? "")}").GetAwaiter().GetResult()) ?? new clsNhanVien();
    }

    public PhongBan GetListBoPhanTheoIdUser(string IdUser, long IdBoPhan)
    {
        var item = new PhongBan { BoMon = new PhongBan() };
        var content = _orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachBoPhanByUserNoneSql?IdUser={Uri.EscapeDataString(IdUser ?? "")}").GetAwaiter().GetResult();
        var list = Deserialize<List<PhongBan>>(content) ?? new();
        if (list.Count > 1)
        {
            var phong = list.FirstOrDefault(a => a.Id == IdBoPhan);
            if (phong != null)
            {
                var bomon = list.FirstOrDefault(a => a.BoPhanChaPkid == phong.Id);
                if (bomon != null)
                {
                    item = phong;
                    item.BoMon = bomon;
                }
                else
                {
                    item = list.FirstOrDefault(a => a.Id == phong.BoPhanChaPkid);
                    if (item != null) item.BoMon = phong;
                    else item = phong;
                }
            }
        }
        else if (list.Count == 1)
        {
            item = list[0];
        }
        return item;
    }

    public string GetNhanSu_IdUserEOS(ApplicationDbContext _dbContext, long Id)
        => GetNhanSu(_dbContext, Id).UserPkid;

    public List<BoPhan> GetDanhSachCongViecTheoBoPhanChucVu(ApplicationDbContext _dbContext, long IdBoPhan, long IdChucVu)
        => Deserialize<List<BoPhan>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetCongViecChucTrachById?IdBoPhan={IdBoPhan}&IdChucVu={IdChucVu}").GetAwaiter().GetResult()) ?? new();

    public ChucDanh GetChucDanh(ApplicationDbContext _dbContext, long Id)
        => Deserialize<ChucDanh>(_orgClient.CallGetAsync($"{RouterNhanSu}GetChucDanh?Id={Id}").GetAwaiter().GetResult()) ?? new ChucDanh();

    public string GetChucDanh_Ten(ApplicationDbContext _dbContext, long Id)
        => GetChucDanh(_dbContext, Id).Ten;

    public List<ChucVu> GetDanhSachChucVu(ApplicationDbContext _dbContext)
        => _orgClient.GetDanhSachChucVuAsync(useCache: true).GetAwaiter().GetResult();

    public List<NguoiDaiDien> GetDanhSachNguoiDaiDienTheoCongTy(ApplicationDbContext _dbContext, long IdCongTy)
        => Deserialize<List<NguoiDaiDien>>(_orgClient.CallGetAsync($"{RouterNhanSu}GetDanhSachNguoiDaiDienTheoCongTy?IdCongTy={IdCongTy}").GetAwaiter().GetResult()) ?? new();

    public List<HRM.Api.Common.DanhMuc> GetDanhMucLoaiNghiPhep(ApplicationDbContext _dbContext)
        => Deserialize<List<HRM.Api.Common.DanhMuc>>(_orgClient.CallGetAsync("SmartEOSAPI/HanhChinhVanPhong/GetDanhMucLoaiNghiPhep").GetAwaiter().GetResult()) ?? new();

    public List<HRM.Api.Common.DanhMuc> GetListDanhMucHinhThucNghi(ApplicationDbContext _dbContext)
        => Deserialize<List<HRM.Api.Common.DanhMuc>>(_orgClient.CallGetAsync($"{RouterDanhMuc}GetListDanhMucHinhThucNghi").GetAwaiter().GetResult()) ?? new();

    public List<ChucDanh> GetDanhSachChucDanh(ApplicationDbContext _dbContext)
        => _orgClient.GetDanhSachChucDanhAsync(useCache: true).GetAwaiter().GetResult();

    public List<Link_DanhSachDuAn_NhanSu> GetListUserNhanSuDuAnCurrent(long IdDuAn, ApplicationDbContext _dbContext)
    {
        var list = Deserialize<List<Link_DanhSachDuAn_NhanSu>>(_orgClient.CallGetAsync($"{RouterDanhMuc}GetListUserNhanSuDuAn?IdDuAn={IdDuAn}").GetAwaiter().GetResult()) ?? new();
        return list.OrderBy(a => a.isDieuDong).ToList();
    }

    public List<Link_DanhSachDuAn_NhanSu> GetListUserNhanSuDuAnTheoThoiGian(long IdDuAn, double TuNgay, double DenNgay, ApplicationDbContext _dbContext)
        => _orgClient.GetListUserNhanSuDuAnTheoThoiGianAsync(IdDuAn, TuNgay, DenNgay).GetAwaiter().GetResult();

    public dynamic GetTrangThaiChuyenTiep_QuyTrinh(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => _workflowClient.GetTrangThaiChuyenTiepQuyTrinhAsync(entity.IdTrangThai).GetAwaiter().GetResult();

    public dynamic GetTrangThaiChuyenTiep_listUser(ApplicationDbContext _dbContext, long IdDuAn, string IdTrangThai)
        => _workflowClient.GetTrangThaiChuyenTiepUsersAsync(IdDuAn, IdTrangThai).GetAwaiter().GetResult();

    public ErrorMessage SetKiemTraQuyTrinh(ApplicationDbContext _dbContext, string IdTable, string IdTrangThai, bool isOpen)
    {
        var ok = _workflowClient.SetKiemTraQuyTrinhAsync(IdTable, IdTrangThai, isOpen).GetAwaiter().GetResult();
        var err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
        if (!ok) err.SetLoiKhac("KÃ¡ÂºÂ¿t nÃ¡Â»â€˜i khÃƒÂ´ng thÃƒÂ nh cÃƒÂ´ng!");
        return err;
    }

    public TrangThaiQuyTrinh GetTrangThaiKhoiTao(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => _workflowClient.GetTrangThaiKhoiTaoAsync(entity).GetAwaiter().GetResult();

    public TrangThaiQuyTrinh GetTrangThaiKhoiTao(ApplicationDbContext _dbContext, string eAction, string IdGuid)
    {
        var item = _workflowClient.GetTrangThaiKhoiTaoAsync(eAction, IdGuid).GetAwaiter().GetResult();
        if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(_dbContext, IdGuid))
            return new TrangThaiQuyTrinh();
        return item;
    }

    public TrangThaiQuyTrinh GetTrangThaiKhoiTaoKhongAddQuyTrinhXuLy(ApplicationDbContext _dbContext, string eAction, string IdTrangThai)
        => _workflowClient.GetTrangThaiKhoiTaoKhongAddQuyTrinhXuLyAsync(eAction, IdTrangThai).GetAwaiter().GetResult();

    public TrangThaiQuyTrinh DoiTrangThaiKhoiTao(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => _workflowClient.DoiTrangThaiKhoiTaoAsync(entity).GetAwaiter().GetResult();

    public bool KiemTraTrangThaiKhoiTao(string IdTrangThai)
        => _workflowClient.KiemTraTrangThaiKhoiTaoAsync(IdTrangThai).GetAwaiter().GetResult();

    public bool KhongDuyetBuocCho(ApplicationDbContext _dbContext, AudiQuyTrinh entity, ErrorMessage err)
        => _workflowClient.KhongDuyetBuocChoAsync(entity).GetAwaiter().GetResult();

    public TrangThaiQuyTrinh GetTrangThaiChuyenTiep(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => _workflowClient.GetTrangThaiChuyenTiepAsync(entity).GetAwaiter().GetResult();

    public TrangThaiQuyTrinh GetTrangThaiKhongDuyet(ApplicationDbContext _dbContext, AudiQuyTrinh entity)
        => _workflowClient.GetTrangThaiKhongDuyetAsync(entity).GetAwaiter().GetResult();

    public TrangThaiQuyTrinh SetHuyQuyTrinh(ApplicationDbContext _dbContext, List<string> listIdTable)
    {
        var item = _workflowClient.SetHuyQuyTrinhAsync(listIdTable).GetAwaiter().GetResult();
        if (!string.IsNullOrEmpty(item.IdTrangThai))
        {
            foreach (var id in listIdTable)
            {
                if (!DongBoQuyTrinhXuLy(_dbContext, id)) return item;
            }
        }
        return item;
    }

    public ErrorMessage XoaQuyTrinh(ApplicationDbContext _dbContext, string IdTable)
    {
        var err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
        var ok = _workflowClient.XoaQuyTrinhXuLyAsync(IdTable).GetAwaiter().GetResult();
        if (!ok) err.SetLoiKhac("XÃƒÂ³a quy trÃƒÂ¬nh khÃƒÂ´ng thÃƒÂ nh cÃƒÂ´ng!");
        else if (!DongBoQuyTrinhXuLy(_dbContext, IdTable)) err.SetLoiKhac("Ã„ÂÃ¡Â»â€œng bÃ¡Â»â„¢ khÃƒÂ´ng thÃƒÂ nh cÃƒÂ´ng!");
        return err;
    }

    public bool DongBoQuyTrinhXuLy(ApplicationDbContext _dbContext, string IdTable)
        => _workflowEngine.SyncWorkflowAsync(IdTable).GetAwaiter().GetResult();

    public List<TrangThaiQuyTrinhThietLap> GetTrangThaiQuyTrinhThietLapSmartEOS(ApplicationDbContext _dbContext, List<string> IdTrangThais)
        => Deserialize<List<TrangThaiQuyTrinhThietLap>>(_orgClient.CallPostAsync($"{RouterQuyTrinh}GetListTenTrangThai", IdTrangThais).GetAwaiter().GetResult()) ?? new();

    public List<PSM_DuAn> GetDataDuAn(ApplicationDbContext _dbContext)
        => Deserialize<List<PSM_DuAn>>(_orgClient.CallGetAsync($"{RouterDanhMuc}GetDanhSachDuAnByIdUser?IdUser={Uri.EscapeDataString(_dbContext.currentUser?.Id ?? _dbContext.IdUser)}").GetAwaiter().GetResult()) ?? new();

    public PSM_DuAn GetQuyTrinhDuAn(ApplicationDbContext _dbContext, long? IdDuAn)
        => Deserialize<PSM_DuAn>(_orgClient.CallGetAsync($"{RouterDanhMuc}GetQuyTrinhDuAn?Id={IdDuAn}").GetAwaiter().GetResult());

    public List<clsUser> GetDanhSachUserByListId(ApplicationDbContext _dbContext, List<string> listID)
        => _orgClient.GetDanhSachUserByIdsAsync(listID).GetAwaiter().GetResult();

    public ErrorMessage UploadTrucTiepFileLenFolder(ApplicationDbContext _dbContext, UploadFileLenFolder uploadFileLenFolder)
    {
        var err = new ErrorMessage(ErrorMessage.eState.ThanhCong);
        var content = _orgClient.CallPostAsync($"{RouterTuHoSo}UploadTrucTiepFileLenFolder", uploadFileLenFolder).GetAwaiter().GetResult();
        var msg = Deserialize<MessageReturn>(content);
        if (msg == null)
        {
            err.SetLoiKhac("KÃ¡ÂºÂ¿t nÃ¡Â»â€˜i khÃƒÂ´ng thÃƒÂ nh cÃƒÂ´ng!");
        }
        else if (msg.Error != eErrorMessage.CapNhatThanhCong)
        {
            err.Data = msg.Value;
            err.SetLoiKhac(msg.Detail);
        }
        return err;
    }

    public TrangThaiQuyTrinh GetTrangThaiChuyenTiep(ApplicationDbContext _dbContext, ChuyenTrangThai entity)
    {
        try
        {
            var userId = _dbContext.currentUser?.Id ?? _dbContext.IdUser;
            var payload = new clsGetTrangThai(userId, entity);
            var content = _orgClient.CallPostAsync($"{RouterQuyTrinh}GetTrangThaiChuyenTiep", payload).GetAwaiter().GetResult();
            var item = Deserialize<TrangThaiQuyTrinh>(content) ?? new TrangThaiQuyTrinh();
            if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(_dbContext, entity?.IdGuid))
            {
                return new TrangThaiQuyTrinh();
            }
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTrangThaiChuyenTiep(ChuyenTrangThai) failed for IdGuid {IdGuid}", entity?.IdGuid);
            return new TrangThaiQuyTrinh();
        }
    }

    public TrangThaiQuyTrinh DoiTrangThaiKhoiTao(ChuyenTrangThai entity, ApplicationDbContext _dbContext)
    {
        try
        {
            var userId = _dbContext.currentUser?.Id ?? _dbContext.IdUser;
            var payload = new clsGetTrangThai(userId, entity);
            var content = _orgClient.CallPostAsync($"{RouterQuyTrinh}DoiTrangThaiKhoiTao", payload).GetAwaiter().GetResult();
            return Deserialize<TrangThaiQuyTrinh>(content) ?? new TrangThaiQuyTrinh();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DoiTrangThaiKhoiTao(ChuyenTrangThai) failed for IdGuid {IdGuid}", entity?.IdGuid);
            return new TrangThaiQuyTrinh();
        }
    }

    public List<HRM_QuyTrinhLamThemGio_ChiTiet> GetKeHoachLamViecFromDieuHanh(ApplicationDbContext _dbContext, long IdDuAn, double Ngay)
    {
        try
        {
            var payload = new HRM_TimKiemkeHoach { IdDuAn = IdDuAn, NgayUnix = Ngay };
            var content = _orgClient.CallPostAsync("DieuHanhCongViec/ThucHienHopDong/GetQuyTrinhLamThemGioChiTiet", payload).GetAwaiter().GetResult();
            return Deserialize<List<HRM_QuyTrinhLamThemGio_ChiTiet>>(content) ?? new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetKeHoachLamViecFromDieuHanh failed for IdDuAn {IdDuAn}", IdDuAn);
            return new List<HRM_QuyTrinhLamThemGio_ChiTiet>();
        }
    }

    public TrangThaiQuyTrinh GetTrangThaiKhoiTaoKetThuc(ApplicationDbContext db, string eAction, string IdTable, string SoQuyTrinh, string NoiDung = "", long IdDuAn = 0)
    {
        try
        {
            var userId = db.currentUser?.Id ?? db.IdUser;
            var payload = new clsGetTrangThai(userId, eAction, IdTable, SoQuyTrinh, NoiDung, IdDuAn);
            var content = _orgClient.CallPostAsync($"{RouterQuyTrinh}GetTrangThaiKhoiTaoKetThuc", payload).GetAwaiter().GetResult();
            var item = Deserialize<TrangThaiQuyTrinh>(content) ?? new TrangThaiQuyTrinh();
            if (!string.IsNullOrEmpty(item.IdTrangThai) && !DongBoQuyTrinhXuLy(db, payload.IdTable))
            {
                return item;
            }
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTrangThaiKhoiTaoKetThuc failed for IdTable {IdTable}", IdTable);
            return new TrangThaiQuyTrinh();
        }
    }
}
