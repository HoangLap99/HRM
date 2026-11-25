using HRM.Api.Common.Interfaces;
using HRM.Api.Persistence;

namespace HRM.Api.Services.Attendance;

/// <summary>
/// Provider thÃƒÂ¡Ã‚Â»Ã‚Â±c tÃƒÂ¡Ã‚ÂºÃ‚Â¿ triÃƒÂ¡Ã‚Â»Ã†â€™n khai IAttendanceSummaryProvider cho SmartEOS / Xin nghÃƒÂ¡Ã‚Â»Ã¢â‚¬Â° phÃƒÆ’Ã‚Â©p
/// thÃƒÆ’Ã‚Â´ng qua FuncChamCong.GetChamCongFromMayChamCongPHT (thay thÃƒÂ¡Ã‚ÂºÃ‚Â¿ NoOpAttendanceSummaryProvider tÃƒÂ¡Ã‚Â»Ã‚Â« Giai Ãƒâ€žÃ¢â‚¬ËœoÃƒÂ¡Ã‚ÂºÃ‚Â¡n 3).
/// </summary>
public sealed class AttendanceSummaryProvider : IAttendanceSummaryProvider
{
    private readonly HrmDbContext _db;
    private readonly HRM.Api.Services.FuncChamCong _funcChamCong;

    public AttendanceSummaryProvider(
        HrmDbContext db,
        HRM.Api.Services.FuncChamCong funcChamCong)
    {
        _db = db;
        _funcChamCong = funcChamCong;
    }

    public Task<List<HRM_BangChamCong_NhanVien>> GetChamCongFromMayChamCongAsync(
        long idCongTy,
        double tuNgayUnix,
        double denNgayUnix,
        List<string> listIdUser,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var result = _funcChamCong.GetChamCongFromMayChamCongPHT(
            idCongTy,
            tuNgayUnix,
            denNgayUnix,
            _db,
            listIdUser);
        return Task.FromResult(result ?? new List<HRM_BangChamCong_NhanVien>());
    }
}