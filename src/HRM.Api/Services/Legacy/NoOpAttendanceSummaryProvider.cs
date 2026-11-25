﻿using HRM.Api.Common.Interfaces;

namespace HRM.Api.Services.Legacy;

/// <summary>
/// Provider mặc định cho Giai đoạn 2 (sẽ được thay thế bởi AttendanceSummaryProvider thực ở Giai đoạn 3 khi migrate FuncChamCong).
/// </summary>
public sealed class NoOpAttendanceSummaryProvider : IAttendanceSummaryProvider
{
    public Task<List<HRM_BangChamCong_NhanVien>> GetChamCongFromMayChamCongAsync(
        long idCongTy,
        double tuNgayUnix,
        double denNgayUnix,
        List<string> listIdUser,
        CancellationToken ct = default)
        => Task.FromResult(new List<HRM_BangChamCong_NhanVien>());
}
