﻿namespace HRM.Api.Common.Interfaces;

/// <summary>
/// Interface DIP cung cấp dữ liệu chấm công tóm tắt cho SmartEOS/Nghỉ phép
/// (tách phụ thuộc khỏi FuncChamCong khi phân tách module).
/// </summary>
public interface IAttendanceSummaryProvider
{
    Task<List<HRM_BangChamCong_NhanVien>> GetChamCongFromMayChamCongAsync(
        long idCongTy,
        double tuNgayUnix,
        double denNgayUnix,
        List<string> listIdUser,
        CancellationToken ct = default);
}
