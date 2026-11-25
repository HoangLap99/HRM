using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HRM.Api.Workflow;

/// <summary>
/// Engine xử lý vòng đời quy trình phê duyệt SmartEOS an toàn kiểu cho mọi thực thể AudiQuyTrinh.
/// </summary>
public sealed class WorkflowEngine : IWorkflowEngine
{
    private static readonly JsonSerializerSettings SafeJsonSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly IHrmDbContext _db;
    private readonly ISmartEosWorkflowClient _workflowClient;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IHrmDbContext db,
        ISmartEosWorkflowClient workflowClient,
        ICurrentUserContext currentUserContext,
        ILogger<WorkflowEngine> logger)
    {
        _db = db;
        _workflowClient = workflowClient;
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public async Task<MessageReturn> InitializeWorkflowAsync<TEntity>(
        TEntity entity,
        string? maQuyTrinh = null,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entity.IdGuid))
            {
                entity.IdGuid = Guid.NewGuid().ToString();
            }

            if (!string.IsNullOrWhiteSpace(maQuyTrinh))
            {
                entity.eAction = maQuyTrinh;
            }

            var trangThai = await _workflowClient.GetTrangThaiKhoiTaoAsync(entity, ct);
            if (string.IsNullOrWhiteSpace(trangThai.IdTrangThai))
            {
                return MessageReturn.Fail("Không tìm thấy trạng thái");
            }

            entity.IdTrangThai = trangThai.IdTrangThai;
            entity.TenTrangThai = trangThai.TenTrangThai;
            entity.IdTrangThaiThamGia = entity.IdTrangThai;
            entity.IdCongViec = string.Empty;

            return MessageReturn.Success("Khởi tạo quy trình thành công", entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing workflow for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> BeforeSaveAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            if (entityDb is null)
            {
                return MessageReturn.Success();
            }

            entity.TenTrangThai = entityDb.TenTrangThai;
            if (entityDb.IdTrangThai == entity.IdTrangThai)
            {
                return MessageReturn.Success();
            }

            entity.IdTrangThaiTruocKhiDoi = entityDb.IdTrangThai;
            entityDb.IdTrangThaiTruocKhiDoi = entityDb.IdTrangThai;
            entityDb.IdTrangThai = entity.IdTrangThai;

            await _workflowClient.DoiTrangThaiKhoiTaoAsync(entity, ct);
            entity.IdTrangThaiThamGia = entity.IdTrangThai;
            entityDb.IdTrangThaiThamGia = entity.IdTrangThai;

            await SaveNextStepConfigurationAsync(entity, ct);
            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BeforeSaveAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> AfterSaveAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            await SaveNextStepConfigurationAsync(entity, ct);

            var actionType = entityDb is not null
                ? DefineData.eModify.Sua.ToString()
                : DefineData.eModify.Them.ToString();

            var dataBefore = entityDb is not null ? JsonConvert.SerializeObject(entityDb, SafeJsonSettings) : string.Empty;
            var dataAfter = JsonConvert.SerializeObject(entity, SafeJsonSettings);

            await _workflowClient.AddModifyAsync(new LM_Modify
            {
                TenChucNang = DefineData.QuyDoiTenQuyTrinh(entity.eAction),
                TrangThai = actionType,
                eTable = entity.eAction,
                NoiDung = entity.NoiDung,
                IdQuyTrinh = entity.IdGuid,
                DataTruoc = dataBefore,
                DataSau = dataAfter,
                IdDuAn = entity.IdDuAn ?? 0,
                IdUser = _currentUserContext.UserId,
                UserName = _currentUserContext.FullName
            }, ct);

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AfterSaveAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> BeforeForwardAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        long idDuAn = 0,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            if (entityDb is null)
            {
                return MessageReturn.Fail("Không tồn tại quy trình");
            }

            if (entityDb.IdTrangThai != entity.IdTrangThai)
            {
                return MessageReturn.Fail("Quy trình đã chuyển trước đó");
            }

            entity.IdDuAn = idDuAn;
            await SaveNextStepConfigurationAsync(entity, ct);

            entity.IdTrangThaiTruocKhiChuyenBuoc = entity.IdTrangThai;
            var trangThai = await _workflowClient.GetTrangThaiChuyenTiepAsync(entity, ct);

            if (string.IsNullOrWhiteSpace(trangThai.IdTrangThai))
            {
                return MessageReturn.Fail(trangThai.TenTrangThai ?? "Không tìm thấy trạng thái chuyển tiếp");
            }

            entity.LoaiTrangThai = trangThai.LoaiTrangThai;
            if (trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.KetThuc ||
                trangThai.LoaiTrangThai == (int)DefineData.eQuyTrinh_LoaiTrangThai.XuLyDacBiet)
            {
                entity.isKetThuc = true;
                entityDb.isKetThuc = true;
            }

            await _workflowClient.SetKiemTraQuyTrinhAsync(entity.IdGuid, entity.IdTrangThai, isOpen: false, ct);

            entity.IdTrangThai = trangThai.IdTrangThai;
            entity.TenTrangThai = trangThai.TenTrangThai;
            entity.listKhoiTao = trangThai.listKhoiTao;

            entityDb.IdTrangThai = trangThai.IdTrangThai;
            entityDb.TenTrangThai = trangThai.TenTrangThai;

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BeforeForwardAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> AfterForwardAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            await CompletePreviousStepAsync(entity, ct);

            await _workflowClient.AddModifyAsync(new LM_Modify
            {
                TenChucNang = DefineData.QuyDoiTenQuyTrinh(entity.eAction),
                TrangThai = DefineData.eModify.ChuyenTiep.ToString(),
                eTable = entity.eAction,
                NoiDung = entity.NoiDung,
                IdQuyTrinh = entity.IdGuid,
                DataTruoc = entityDb is not null ? JsonConvert.SerializeObject(entityDb, SafeJsonSettings) : string.Empty,
                DataSau = JsonConvert.SerializeObject(entity, SafeJsonSettings),
                IdDuAn = entity.IdDuAn ?? 0,
                IdUser = _currentUserContext.UserId,
                UserName = _currentUserContext.FullName
            }, ct);

            if (entity.listKhoiTao is { Count: > 0 })
            {
                await InitializeFollowUpWorkflowsAsync(entity, ct);
            }

            return MessageReturn.Success("Chuyển tiếp thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AfterForwardAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> BeforeRejectAsync<TEntity>(
        TEntity? entityDb,
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            if (entityDb is null)
            {
                return MessageReturn.Fail("Không tồn tại quy trình");
            }

            if (entityDb.IdTrangThai != entity.IdTrangThai)
            {
                return MessageReturn.Fail("Quy trình đã chuyển trước đó");
            }

            entity.IdTrangThaiTruocKhiChuyenBuoc = entity.IdTrangThai;
            var trangThai = await _workflowClient.GetTrangThaiKhongDuyetAsync(entity, ct);
            if (string.IsNullOrWhiteSpace(trangThai.IdTrangThai))
            {
                return MessageReturn.Fail(trangThai.TenTrangThai ?? "Không tìm thấy trạng thái không duyệt");
            }

            await _workflowClient.SetKiemTraQuyTrinhAsync(entity.IdGuid, entity.IdTrangThai, isOpen: false, ct);

            entity.IdTrangThai = trangThai.IdTrangThai;
            entity.TenTrangThai = trangThai.TenTrangThai;
            entityDb.IdTrangThai = trangThai.IdTrangThai;
            entityDb.TenTrangThai = trangThai.TenTrangThai;

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in BeforeRejectAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> AfterRejectAsync<TEntity>(
        TEntity entity,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            await CompletePreviousStepAsync(entity, ct);

            await _workflowClient.AddModifyAsync(new LM_Modify
            {
                TenChucNang = DefineData.QuyDoiTenQuyTrinh(entity.eAction),
                TrangThai = DefineData.eModify.KhongDuyet.ToString(),
                eTable = entity.eAction,
                NoiDung = entity.NoiDung,
                IdQuyTrinh = entity.IdGuid,
                DataTruoc = string.Empty,
                DataSau = JsonConvert.SerializeObject(entity, SafeJsonSettings),
                IdDuAn = entity.IdDuAn ?? 0,
                IdUser = _currentUserContext.UserId,
                UserName = _currentUserContext.FullName
            }, ct);

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AfterRejectAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<MessageReturn> LoadWorkflowStateAsync<TEntity>(
        TEntity? entity,
        bool isXemQuyTrinh = false,
        CancellationToken ct = default) where TEntity : AudiQuyTrinh
    {
        try
        {
            if (entity is null)
            {
                return MessageReturn.Fail("Quy trình không tồn tại", DefineData.eErrorMessage.LoiLayDuLieu);
            }

            await PopulateNextStepSelectionsAsync(entity, ct);

            if (isXemQuyTrinh)
            {
                await _workflowClient.SetKiemTraQuyTrinhAsync(entity.IdGuid, entity.IdTrangThai, isOpen: true, ct);
            }

            entity.IdTrangThaiThamGia = entity.IdTrangThai;

            var userId = _currentUserContext.UserId;
            var pendingTask = await _db.HRM_QuyTrinhXuLy
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdUser == userId && x.IdTable == entity.IdGuid && x.isDaXuLy == 0, ct);

            if (pendingTask is not null && IsDelegatedOrWaitingStep(pendingTask.Loai))
            {
                entity.IdTrangThaiThamGia = pendingTask.IdTrangThai;
                entity.TenTrangThai = pendingTask.TenTrangThai;
            }

            var currentStepTask = await _db.HRM_QuyTrinhXuLy
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.IdUser == userId &&
                    x.IdTable == entity.IdGuid &&
                    x.IdTrangThai == entity.IdTrangThaiThamGia &&
                    (x.Loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.PhanCong ||
                     x.Loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.Cho ||
                     x.Loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.UyQuyen), ct);

            if (currentStepTask is not null)
            {
                entity.isDisableTrangThaiTiepTheo = true;
            }

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadWorkflowStateAsync for {EntityType} ({IdGuid})", typeof(TEntity).Name, entity?.IdGuid);
            return MessageReturn.Fail(ex.Message);
        }
    }

    public async Task<bool> SyncWorkflowAsync(string idTable, CancellationToken ct = default)
    {
        try
        {
            var latestTasks = await _workflowClient.GetDongBoQuyTrinhXuLyAsync(idTable, ct);
            var existingTasks = await _db.HRM_QuyTrinhXuLy
                .Where(x => x.IdTable == idTable)
                .ToListAsync(ct);

            if (existingTasks.Count > 0)
            {
                _db.HRM_QuyTrinhXuLy.RemoveRange(existingTasks);
            }

            if (latestTasks.Count > 0)
            {
                _db.HRM_QuyTrinhXuLy.AddRange(latestTasks);
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing workflow tasks for table {IdTable}", idTable);
            return false;
        }
    }

    public async Task<MessageReturn> DeleteWorkflowAsync(string idTable, CancellationToken ct = default)
    {
        try
        {
            var attachments = await _db.HRM_FileDinhKem
                .Where(x => x.idItem == idTable)
                .ToListAsync(ct);

            if (attachments.Count > 0)
            {
                _db.HRM_FileDinhKem.RemoveRange(attachments);
                await _db.SaveChangesAsync(ct);
            }

            var deleted = await _workflowClient.XoaQuyTrinhXuLyAsync(idTable, ct);
            if (deleted)
            {
                await SyncWorkflowAsync(idTable, ct);
            }

            return MessageReturn.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {IdTable}", idTable);
            return MessageReturn.Fail(ex.Message);
        }
    }

    private async Task PopulateNextStepSelectionsAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : AudiQuyTrinh
    {
        string idQtTrangThaiTiepTheo = string.Empty;
        entity.listTrangThaiTiepTheo = await _workflowClient.GetTrangThaiChuyenTiepQuyTrinhAsync(entity.IdTrangThai, ct);

        var tttt = await _db.HRM_QT_TrangThaiTiepTheo
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdTrangThai == entity.IdTrangThai && x.IdTable == entity.IdGuid && !x.isKetThuc, ct)
            ?? await _db.HRM_QT_TrangThaiTiepTheo
                .AsNoTracking()
                .Where(x => x.IdTrangThai == entity.IdTrangThai && x.IdTable == entity.IdGuid && x.isKetThuc)
                .OrderByDescending(x => x.Created)
                .FirstOrDefaultAsync(ct);

        if (tttt is not null)
        {
            var foundState = entity.listTrangThaiTiepTheo.FirstOrDefault(x => x.IdTrangThai == tttt.IdTrangThaiTiepTheo);
            if (foundState is not null)
            {
                foundState.isChon = true;
            }
            idQtTrangThaiTiepTheo = tttt.Id;
        }

        var selectedState = entity.listTrangThaiTiepTheo.FirstOrDefault(x => x.isChon);
        if (selectedState is null && entity.listTrangThaiTiepTheo.Count > 0)
        {
            entity.listTrangThaiTiepTheo[0].isChon = true;
            selectedState = entity.listTrangThaiTiepTheo[0];
        }

        if (selectedState is null)
        {
            return;
        }

        long duAnId = entity.eAction == DefineData.eAction.QUYTRINHDUAN.ToString() ? (entity.IdDuAn ?? 0) : 0;
        entity.listUserTiepTheo = await _workflowClient.GetTrangThaiChuyenTiepUsersAsync(duAnId, selectedState.IdTrangThai, ct);

        var savedUsers = await _db.HRM_QT_UserTiepTheo
            .AsNoTracking()
            .Where(x => x.IdQTTrangThaiTiepTheo == idQtTrangThaiTiepTheo && x.IdTable == entity.IdGuid && !x.isKetThuc)
            .ToListAsync(ct);

        if (savedUsers.Count == 0)
        {
            savedUsers = await _db.HRM_QT_UserTiepTheo
                .AsNoTracking()
                .Where(x => x.IdQTTrangThaiTiepTheo == idQtTrangThaiTiepTheo && x.IdTable == entity.IdGuid && x.isKetThuc)
                .ToListAsync(ct);
        }

        if (savedUsers.Count == 0)
        {
            savedUsers = await _db.HRM_QT_UserTiepTheo
                .AsNoTracking()
                .Where(x => x.IdTrangThai == entity.IdTrangThai && x.IdTable == entity.IdGuid && !x.isKetThuc)
                .ToListAsync(ct);
        }

        if (savedUsers.Count > 0)
        {
            foreach (var u in entity.listUserTiepTheo)
            {
                u.isChon = false;
            }
        }

        var selectedUserIds = savedUsers.Select(u => u.IdUser).ToHashSet();
        foreach (var candidate in entity.listUserTiepTheo.Where(u => selectedUserIds.Contains(u.IdUser)))
        {
            candidate.isChon = true;
        }
    }

    private async Task SaveNextStepConfigurationAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : AudiQuyTrinh
    {
        if (entity.listTrangThaiTiepTheo is null)
        {
            await _db.SaveChangesAsync(ct);
            return;
        }

        var existingState = await _db.HRM_QT_TrangThaiTiepTheo
            .FirstOrDefaultAsync(x => x.IdTrangThai == entity.IdTrangThai && x.IdTable == entity.IdGuid && !x.isKetThuc, ct);

        var chosenState = entity.listTrangThaiTiepTheo.FirstOrDefault(x => x.isChon);
        if (chosenState is not null)
        {
            if (existingState is null)
            {
                existingState = new HRM_QT_TrangThaiTiepTheo
                {
                    Id = Guid.NewGuid().ToString(),
                    IdTable = entity.IdGuid,
                    IdTrangThai = entity.IdTrangThai,
                    IdTrangThaiTiepTheo = chosenState.IdTrangThai
                };
                _db.HRM_QT_TrangThaiTiepTheo.Add(existingState);
            }
            else
            {
                existingState.IdTrangThaiTiepTheo = chosenState.IdTrangThai;
            }
        }
        else if (existingState is not null)
        {
            _db.HRM_QT_TrangThaiTiepTheo.Remove(existingState);
        }

        if (entity.listUserTiepTheo is not null)
        {
            var existingUsers = await _db.HRM_QT_UserTiepTheo
                .Where(x => x.IdTrangThai == entity.IdTrangThai && x.IdTable == entity.IdGuid && !x.isKetThuc)
                .ToListAsync(ct);

            if (existingUsers.Count > 0)
            {
                _db.HRM_QT_UserTiepTheo.RemoveRange(existingUsers);
            }

            var newUsers = entity.listUserTiepTheo
                .Where(x => x.isChon)
                .Select(x => new HRM_QT_UserTiepTheo(entity, existingState, x.IdUser))
                .ToList();

            if (newUsers.Count > 0)
            {
                _db.HRM_QT_UserTiepTheo.AddRange(newUsers);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task CompletePreviousStepAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : AudiQuyTrinh
    {
        var previousStatusId = entity.IdTrangThaiTruocKhiChuyenBuoc;

        if (entity.listTrangThaiTiepTheo is not null)
        {
            var previousState = await _db.HRM_QT_TrangThaiTiepTheo
                .FirstOrDefaultAsync(x => x.IdTrangThai == previousStatusId && x.IdTable == entity.IdGuid && !x.isKetThuc, ct);

            if (previousState is not null)
            {
                previousState.isKetThuc = true;
            }
        }

        if (entity.listUserTiepTheo is not null)
        {
            var previousUsers = await _db.HRM_QT_UserTiepTheo
                .Where(x => x.IdTrangThai == previousStatusId && x.IdTable == entity.IdGuid && !x.isKetThuc)
                .ToListAsync(ct);

            foreach (var item in previousUsers)
            {
                item.isKetThuc = true;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task InitializeFollowUpWorkflowsAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : AudiQuyTrinh
    {
        if (entity.listKhoiTao is null || entity.listKhoiTao.Count == 0)
        {
            return;
        }

        var existingRelated = await _db.HRM_QuyTrinhLienQuan
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdQuyTrinhLienQuan == entity.IdGuid, ct);

        foreach (var item in entity.listKhoiTao)
        {
            if (item.Ngay == 0)
            {
                var related = new HRM_QuyTrinhLienQuan
                {
                    Id = Guid.NewGuid().ToString(),
                    IdQuyTrinhGoc = entity.IdGuid,
                    eActionGoc = entity.eAction,
                    eActionLienQuan = item.eAction
                };

                related.IdQuyTrinhKey = existingRelated is null
                    ? $"{related.IdQuyTrinhGoc}_{related.IdQuyTrinhLienQuan}"
                    : $"{existingRelated.IdQuyTrinhKey}_{related.IdQuyTrinhLienQuan}";

                _db.HRM_QuyTrinhLienQuan.Add(related);
            }
            else
            {
                _db.HRM_QuyTrinhKhoiTao.Add(new HRM_QuyTrinhKhoiTao(item, entity));
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool IsDelegatedOrWaitingStep(int? loai) =>
        loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.Cho ||
        loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.PhanCong ||
        loai == (int)DefineData.eQuyTrinh_LoaiTrangThai.UyQuyen;
}
