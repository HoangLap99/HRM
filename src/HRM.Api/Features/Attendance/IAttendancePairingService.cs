namespace HRM.Api.Features.Attendance;

/// <summary>
/// Service chuẩn hóa thuật toán ghép cặp dữ liệu máy chấm công / FaceID (HRM_CheckInOut)
/// với lịch làm việc kế hoạch, hỗ trợ đủ vào-ra, chỉ chấm vào, chỉ chấm về (Loai == "O"), ca đêm và ca gãy.
/// </summary>
public interface IAttendancePairingService
{
    IReadOnlyList<HRM_ThoiGianLamViecKeHoach> PairCheckInOuts(
        IReadOnlyList<HRM_CheckInOut> checkInOuts,
        IReadOnlyList<HRM_ThoiGianLamViecKeHoach> plannedSchedules,
        double allowedLateMinutes = 0,
        double allowedEarlyMinutes = 0,
        bool includeCheckOutOnly = false);

    void MapActualTimesToSchedules(
        IList<HRM_ThoiGianLamViecKeHoach> plannedSchedules,
        IReadOnlyList<HRM_ThoiGianLamViecKeHoach> pairedInOuts,
        bool snapToShiftStartWhenEarly = false);
}