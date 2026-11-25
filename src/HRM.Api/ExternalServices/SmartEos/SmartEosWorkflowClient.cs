using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.ExternalServices.SmartEos;

public sealed class SmartEosWorkflowClient : SmartEosHttpClientBase, ISmartEosWorkflowClient
{
    private const string RouterQuyTrinh = "SmartEOSAPI/QuanTriQuyTrinh/";

    public SmartEosWorkflowClient(
        HttpClient httpClient,
        ICurrentUserContext currentUserContext,
        IOptions<HrmOptions> options,
        ILogger<SmartEosWorkflowClient> logger)
        : base(httpClient, currentUserContext, options, logger)
    {
    }

    public async Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoAsync(AudiQuyTrinh entity, CancellationToken ct = default)
    {
        var noiDung = !string.IsNullOrEmpty(entity.NoiDungKhac) ? entity.NoiDungKhac : entity.NoiDung;
        var thoiGianT = entity.ThoiGianT ?? 0;

        var url = $"{RouterQuyTrinh}GetTrangThaiKhoiTao?IdUser={Uri.EscapeDataString(CurrentUserContext.UserId)}" +
                  $"&eAction={Uri.EscapeDataString(entity.eAction ?? string.Empty)}" +
                  $"&IdTable={Uri.EscapeDataString(entity.IdGuid)}" +
                  $"&NoiDung={Uri.EscapeDataString(noiDung ?? string.Empty)}" +
                  $"&IdDuAn={entity.IdDuAn ?? 0}" +
                  $"&ThoiGianT={thoiGianT}" +
                  $"&SoQuyTrinh={Uri.EscapeDataString(entity.SoQuyTrinh ?? string.Empty)}" +
                  $"&IddmPhanKhu=";

        return await GetJsonAsync<TrangThaiQuyTrinh>(url, includeAuth: false, ct: ct) ?? new TrangThaiQuyTrinh();
    }

    public async Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoAsync(string eAction, string idTable, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiKhoiTao?IdUser={Uri.EscapeDataString(CurrentUserContext.UserId)}" +
                  $"&eAction={Uri.EscapeDataString(eAction)}" +
                  $"&IdTable={Uri.EscapeDataString(idTable)}" +
                  $"&NoiDung=&IdDuAn=0&ThoiGianT=0&SoQuyTrinh=";

        return await GetJsonAsync<TrangThaiQuyTrinh>(url, includeAuth: false, ct: ct) ?? new TrangThaiQuyTrinh();
    }

    public async Task<TrangThaiQuyTrinh> GetTrangThaiKhoiTaoKhongAddQuyTrinhXuLyAsync(
        string eAction,
        string? idTrangThai = null,
        CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiKhoiTaoKhongAddQuyTrinhXuLy?IdUser={Uri.EscapeDataString(CurrentUserContext.UserId)}" +
                  $"&eAction={Uri.EscapeDataString(eAction)}&IdTrangThai={Uri.EscapeDataString(idTrangThai ?? string.Empty)}";

        return await GetJsonAsync<TrangThaiQuyTrinh>(url, includeAuth: false, ct: ct) ?? new TrangThaiQuyTrinh();
    }

    public async Task<TrangThaiQuyTrinh> DoiTrangThaiKhoiTaoAsync(AudiQuyTrinh entity, CancellationToken ct = default)
    {
        var payload = new clsGetTrangThai(CurrentUserContext.UserId, entity);
        return await PostJsonAsync<TrangThaiQuyTrinh>($"{RouterQuyTrinh}DoiTrangThaiKhoiTao", payload, ct: ct)
            ?? new TrangThaiQuyTrinh();
    }

    public async Task<TrangThaiQuyTrinh> GetTrangThaiChuyenTiepAsync(AudiQuyTrinh entity, CancellationToken ct = default)
    {
        var payload = new clsGetTrangThai(CurrentUserContext.UserId, entity);
        return await PostJsonAsync<TrangThaiQuyTrinh>($"{RouterQuyTrinh}GetTrangThaiChuyenTiep", payload, includeAuth: false, ct: ct)
            ?? new TrangThaiQuyTrinh();
    }

    public async Task<TrangThaiQuyTrinh> GetTrangThaiKhongDuyetAsync(AudiQuyTrinh entity, CancellationToken ct = default)
    {
        var payload = new clsGetTrangThai(CurrentUserContext.UserId, entity)
        {
            NoiDung = !string.IsNullOrEmpty(entity.LyDoKhongDuyet) ? entity.LyDoKhongDuyet : entity.NoiDung
        };

        return await PostJsonAsync<TrangThaiQuyTrinh>($"{RouterQuyTrinh}GetTrangThaiKhongDuyet", payload, includeAuth: false, ct: ct)
            ?? new TrangThaiQuyTrinh();
    }

    public async Task<bool> KhongDuyetBuocChoAsync(AudiQuyTrinh entity, CancellationToken ct = default)
    {
        var payload = new clsGetTrangThai(CurrentUserContext.UserId, entity);
        using var request = CreateRequest(HttpMethod.Post, $"{RouterQuyTrinh}KhongDuyetBuocCho", payload, includeAuth: false);
        var content = await SendAndReadStringAsync(request, ct);
        return content is not null;
    }

    public async Task<TrangThaiQuyTrinh> SetHuyQuyTrinhAsync(IReadOnlyCollection<string> idTables, CancellationToken ct = default)
    {
        var payload = new clsHuyQuyTrinh
        {
            IdUser = CurrentUserContext.UserId,
            listIdTable = idTables.ToList()
        };

        return await PostJsonAsync<TrangThaiQuyTrinh>($"{RouterQuyTrinh}SetHuyQuyTrinh", payload, includeAuth: false, ct: ct)
            ?? new TrangThaiQuyTrinh();
    }

    public async Task<List<HRM_QT_TrangThaiTiepTheo>> GetTrangThaiChuyenTiepQuyTrinhAsync(string? idTrangThai, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiChuyenTiep_QuyTrinh?IdTrangThai={Uri.EscapeDataString(idTrangThai ?? string.Empty)}";
        return await GetJsonAsync<List<HRM_QT_TrangThaiTiepTheo>>(url, ct: ct) ?? new List<HRM_QT_TrangThaiTiepTheo>();
    }

    public async Task<List<HRM_QT_UserTiepTheo>> GetTrangThaiChuyenTiepUsersAsync(long idDuAn, string? idTrangThai, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}GetTrangThaiChuyenTiep_listUser?IdDuAn={idDuAn}&IdTrangThai={Uri.EscapeDataString(idTrangThai ?? string.Empty)}";
        return await GetJsonAsync<List<HRM_QT_UserTiepTheo>>(url, ct: ct) ?? new List<HRM_QT_UserTiepTheo>();
    }

    public async Task<List<HRM_QuyTrinhXuLy>> GetDongBoQuyTrinhXuLyAsync(string idTable, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}DongBoQuyTrinhXuLy?IdUser={Uri.EscapeDataString(CurrentUserContext.UserId)}&IdTable={Uri.EscapeDataString(idTable)}";
        return await GetJsonAsync<List<HRM_QuyTrinhXuLy>>(url, includeAuth: false, ct: ct) ?? new List<HRM_QuyTrinhXuLy>();
    }

    public async Task<bool> SetKiemTraQuyTrinhAsync(string idTable, string? idTrangThai, bool isOpen, CancellationToken ct = default)
    {
        var url = $"SmartEOSAPI/QuyTrinh/SetKiemTraQuyTrinh?IdTable={Uri.EscapeDataString(idTable)}&IdTrangThai={Uri.EscapeDataString(idTrangThai ?? string.Empty)}&isOpen={isOpen}";
        using var request = CreateRequest(HttpMethod.Get, url, includeAuth: true);
        var content = await SendAndReadStringAsync(request, ct);
        return content is not null;
    }

    public async Task<bool> KiemTraTrangThaiKhoiTaoAsync(string? idTrangThai, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}KiemTraTrangThaiKhoiTao?IdTrangThai={Uri.EscapeDataString(idTrangThai ?? string.Empty)}";
        var msg = await GetJsonAsync<MessageReturn>(url, includeAuth: false, ct: ct);
        if (msg?.Error == DefineData.eErrorMessage.CapNhatThanhCong && msg.Value is bool b)
        {
            return b;
        }
        return false;
    }

    public async Task<bool> XoaQuyTrinhXuLyAsync(string idTable, CancellationToken ct = default)
    {
        var url = $"{RouterQuyTrinh}XoaQuyTrinhXuLy?IdTable={Uri.EscapeDataString(idTable)}";
        var msg = await GetJsonAsync<MessageReturn>(url, includeAuth: true, ct: ct);
        return msg?.Error == DefineData.eErrorMessage.CapNhatThanhCong;
    }

    public async Task AddModifyAsync(LM_Modify modify, CancellationToken ct = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "Log_Modify/Modify/AddModify", modify, includeAuth: true);
        await SendAndReadStringAsync(request, ct);
    }

    public async Task CreateLogAsync(string functionName, string? idUser, string? userName, CancellationToken ct = default)
    {
        var url = $"CreateLog/AddModify?FuntionName={Uri.EscapeDataString(functionName)}" +
                  $"&IdUser={Uri.EscapeDataString(idUser ?? string.Empty)}" +
                  $"&Username={Uri.EscapeDataString(userName ?? string.Empty)}";

        using var request = CreateRequest(HttpMethod.Get, url, includeAuth: true);
        await SendAndReadStringAsync(request, ct);
    }
}
