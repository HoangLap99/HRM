#nullable disable
using HRM.Api.Features.Attendance;

namespace HRM.Api.Services.Attendance;

/// <summary>
/// Triển khai chuẩn thuật toán ghép cặp In/Out và ánh xạ giờ thực tế vào lịch làm việc kế hoạch
/// với độ phức tạp O(N) thông qua ILookup theo IdNhanVien / Ngay.
/// </summary>
public sealed class AttendancePairingService : IAttendancePairingService
{
    private static readonly string InCode = eKyHieuChamCong.I.ToString();
    private static readonly string OutCode = eKyHieuChamCong.O.ToString();

    public IReadOnlyList<HRM_ThoiGianLamViecKeHoach> PairCheckInOuts(
        IReadOnlyList<HRM_CheckInOut> checkInOuts,
        IReadOnlyList<HRM_ThoiGianLamViecKeHoach> plannedSchedules,
        double allowedLateMinutes = 0,
        double allowedEarlyMinutes = 0,
        bool includeCheckOutOnly = false)
    {
        if (checkInOuts == null || checkInOuts.Count == 0)
        {
            return Array.Empty<HRM_ThoiGianLamViecKeHoach>();
        }

        var orderedLogs = checkInOuts.OrderBy(a => a.Gio).ToList();
        var logsByEmployee = orderedLogs.ToLookup(a => a.IdNhanVien);
        var schedulesByEmpDay = (plannedSchedules ?? Array.Empty<HRM_ThoiGianLamViecKeHoach>())
            .ToLookup(a => (a.IdNhanVien, Ngay: a.Ngay.Date));

        var result = new List<HRM_ThoiGianLamViecKeHoach>(orderedLogs.Count);
        int count = orderedLogs.Count;

        for (int i = 0; i < count; i++)
        {
            var item = orderedLogs[i];
            if (item.Loai == OutCode)
            {
                continue;
            }

            var windowEnd = item.Ngay.Date;
            var firstShift = schedulesByEmpDay[(item.IdNhanVien, windowEnd)].FirstOrDefault();
            if (firstShift != null && firstShift.GioRa.Date > windowEnd)
            {
                windowEnd = firstShift.GioRa.Date;
            }

            var empLogs = logsByEmployee[item.IdNhanVien]
                .Where(a => a.Gio >= item.Gio && a.Ngay >= item.Ngay && a.Ngay <= windowEnd)
                .ToList();

            var paired = new HRM_ThoiGianLamViecKeHoach
            {
                IdCongTy = item.IdCongTy,
                Ngay = item.Ngay,
                IdNhanVien = item.IdNhanVien,
                GioVao = allowedLateMinutes != 0 ? item.Gio.AddMinutes(-allowedLateMinutes) : item.Gio
            };

            if (empLogs.Count >= 2)
            {
                var firstOut = empLogs.FirstOrDefault(a => a.Loai == OutCode);
                if (firstOut != null)
                {
                    paired.GioRa = allowedEarlyMinutes != 0 ? firstOut.Gio.AddMinutes(allowedEarlyMinutes) : firstOut.Gio;
                    paired.IdCongTyVe = firstOut.IdCongTy;
                }
            }

            if (i + 2 < count && paired.GioRa.Year > 2024)
            {
                var nextIn = empLogs
                    .Where(a => a.Loai == InCode && a.Gio > paired.GioRa)
                    .OrderBy(a => a.Gio)
                    .FirstOrDefault();

                if (nextIn != null)
                {
                    paired.GioVaoTiep = allowedLateMinutes != 0 ? nextIn.Gio.AddMinutes(-allowedLateMinutes) : nextIn.Gio;
                    var lastOutBeforeNextIn = empLogs
                        .Where(a => a.Loai == OutCode && a.Gio > paired.GioVao && a.Gio <= paired.GioVaoTiep)
                        .OrderByDescending(a => a.Gio)
                        .FirstOrDefault();
                    if (lastOutBeforeNextIn != null)
                    {
                        paired.GioRa = allowedEarlyMinutes != 0
                            ? lastOutBeforeNextIn.Gio.AddMinutes(allowedEarlyMinutes)
                            : lastOutBeforeNextIn.Gio;
                        paired.IdCongTyVe = lastOutBeforeNextIn.IdCongTy;
                    }
                }
                else
                {
                    var lastOut = empLogs
                        .Where(a => a.Loai == OutCode && a.Gio > paired.GioVao)
                        .OrderByDescending(a => a.Gio)
                        .FirstOrDefault();
                    if (lastOut != null)
                    {
                        paired.GioRa = allowedEarlyMinutes != 0
                            ? lastOut.Gio.AddMinutes(allowedEarlyMinutes)
                            : lastOut.Gio;
                        paired.IdCongTyVe = lastOut.IdCongTy;
                    }
                }
            }

            result.Add(paired);
        }

        if (includeCheckOutOnly)
        {
            var matchedKeys = result.Select(x => (x.IdNhanVien, Ngay: x.Ngay.Date)).ToHashSet();
            foreach (var outOnly in orderedLogs.Where(x => x.Loai == OutCode))
            {
                if (matchedKeys.Contains((outOnly.IdNhanVien, outOnly.Ngay.Date)))
                {
                    continue;
                }

                result.Add(new HRM_ThoiGianLamViecKeHoach
                {
                    IdCongTy = outOnly.IdCongTy,
                    IdCongTyVe = outOnly.IdCongTy,
                    Ngay = outOnly.Ngay,
                    IdNhanVien = outOnly.IdNhanVien,
                    GioRa = allowedEarlyMinutes != 0 ? outOnly.Gio.AddMinutes(allowedEarlyMinutes) : outOnly.Gio
                });
            }
        }

        return result;
    }

    public void MapActualTimesToSchedules(
        IList<HRM_ThoiGianLamViecKeHoach> plannedSchedules,
        IReadOnlyList<HRM_ThoiGianLamViecKeHoach> pairedInOuts,
        bool snapToShiftStartWhenEarly = false)
    {
        if (plannedSchedules == null || plannedSchedules.Count == 0 || pairedInOuts == null || pairedInOuts.Count == 0)
        {
            return;
        }

        var inOutsByEmp = pairedInOuts.ToLookup(a => a.IdNhanVien);

        foreach (var shift in plannedSchedules)
        {
            var candidates = inOutsByEmp[shift.IdNhanVien]
                .Where(a => a.Ngay >= shift.Ngay &&
                            ((a.GioVao <= shift.GioRa && a.GioRa >= shift.GioVao)
                             || (a.GioVao.Year > 2000 && a.GioVao < shift.GioRa && a.GioRa.Year != a.GioVao.Year)
                             || (a.GioRa.Year > 2000 && a.GioRa > shift.GioVao && a.GioRa.Year != a.GioVao.Year)))
                .OrderBy(a => a.GioVao)
                .ToList();

            foreach (var candidate in candidates)
            {
                bool hasValidIn = candidate.GioVao.Year > 2000;
                bool hasValidOut = candidate.GioRa.Year > 2020;

                if (hasValidOut && candidate.GioRa < shift.GioVao)
                {
                    continue;
                }
                if (hasValidIn && candidate.GioVao > shift.GioRa && shift.GioVao > shift.GioRa)
                {
                    continue;
                }

                // Sửa triệt để bug so sánh với GioVaoTT == null của bản cũ (so sánh đúng với candidate.GioVao)
                if (hasValidIn && (shift.GioVaoTT == null || shift.GioVaoTT > candidate.GioVao))
                {
                    shift.GioVaoTT = snapToShiftStartWhenEarly && shift.GioVao >= candidate.GioVao && candidate.GioRa > shift.GioVao
                        ? shift.GioVao
                        : candidate.GioVao;
                }

                if (hasValidOut && (shift.GioRaTT == null || (shift.GioRaTT <= candidate.GioRa && (!hasValidIn || shift.GioRa >= candidate.GioVao))))
                {
                    shift.GioRaTT = candidate.GioRa;
                }

                if (candidate.GioVaoTiep != null)
                {
                    shift.GioVaoTiep = candidate.GioVaoTiep;
                }
            }
        }
    }
}