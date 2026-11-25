#nullable disable
using System.Collections.Frozen;
using HRM.Api.Common.Interfaces;
using HRM.Api.Features.Payroll;
using Microsoft.EntityFrameworkCore;

namespace HRM.Api.Services.Payroll;

/// <summary>
/// Dựng bảng tính lương động cho cả mẫu Chuẩn (ICT/chung) và mẫu PHT
/// bằng FrozenDictionary O(1) column evaluator thay thế 900+ dòng else-if lặp lại.
/// </summary>
public sealed class DynamicPayrollGridBuilder : IPayrollGridBuilder
{
    private sealed record RowBuildContext(
        HRM_QuyTrinhTinhLuong_NhanVien Data,
        HRM_NhanVien Employee,
        string PhoneNumber,
        string ContractNumber,
        int SeniorityYears,
        int SeniorityMonths);

    private static readonly FrozenDictionary<string, Action<clsBangTinhLuong_Header, List<HRM_QuyTrinhTinhLuong_NhanVien>, clsBangTinhLuong_GiaTri>> StandardTotalEvaluators =
        new Dictionary<string, Action<clsBangTinhLuong_Header, List<HRM_QuyTrinhTinhLuong_NhanVien>, clsBangTinhLuong_GiaTri>>(StringComparer.Ordinal)
        {
            ["TenNhanVien"] = (_, _, c) => c.NoiDung = "Tổng Cộng",
            ["KyNhan"] = (_, _, c) => c.NoiDung = "",
            ["ThuCoc"] = (_, list, c) => c.GiaTri = list.Sum(a => a.DongPhuc),
            ["SoTienPCDiLai"] = (_, list, c) => c.GiaTri = list.Sum(a => a.SoTienPCDiLai),
            ["ThanhTienPCDiLai"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongThamNien),
            ["TienBienChe"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienBienChe),
            ["LuongCoBan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongCoBan),
            ["TA_DonGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TA_DonGia),
            ["TA_ThanhTien"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TA_ThanhTien),
            ["TA_SoCong"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TA_SoCong),
            ["TK_DonGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TK_DonGia),
            ["TK_TruAnCa"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TK_TruAnCa),
            ["TK_XuatAn"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TK_XuatAn),
            ["TongLuongCB"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongCoBan + a.listPhuCap.Sum(x => x.SoTienPhuCapHienThi)),
            ["Luong50"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Luong50 ?? 0),
            ["Luong100"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Luong100 ?? 0),
            ["Cong100"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Cong100 ?? 0),
            ["Cong50"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Cong50 ?? 0),
            ["DanhGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongDanhGia),
            ["TongLuong"] = (_, list, c) => c.GiaTri = list.Sum(d => d.listGioTieuChuan.Sum(a => a.Luong) + d.PhuCapCongChinh + d.LuongLeKDL),
            ["TongLuongDanhGia"] = (_, list, c) => c.GiaTri = list.Sum(d => d.LuongCoBan + d.PhuCapCongChinh + d.LuongDanhGia),
            ["TongKhauTru"] = (_, list, c) => c.GiaTri = list.Sum(ComputeStandardDeductions),
            ["Thuong"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Thuong),
            ["LuongDongBHXH"] = (_, list, c) => c.GiaTri = list.Sum(a => a.MucDongBaoHiemXaHoi),
            ["LuongBHXH"] = (_, list, c) => c.GiaTri = list.Sum(a => a.NLDBHYT + a.NLDBHXH + a.NLDBHTN),
            ["CongDoan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienCongDoan),
            ["TruyThu"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TruyThu),
            ["LuongLe"] = (_, list, c) => c.GiaTri = list.Sum(d => d.LuongLe),
            ["LuongLeKDL"] = (_, list, c) => c.GiaTri = list.Sum(d => d.LuongLeKDL),
            ["LeTet"] = (_, list, c) => c.GiaTri = list.Sum(d => d.Le_Tet),
            ["LeTetKDL"] = (_, list, c) => c.GiaTri = list.Sum(d => d.Le_TetKDL),
            ["TruyHoan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienTruyHoan ?? 0),
            ["ThucNhan"] = (_, list, c) => c.GiaTri = list.Sum(ComputeStandardNetPay),
            ["TongLuongTinhThue"] = (_, list, c) => c.GiaTri = Math.Max(0, list.Sum(a => a.TongLuong - a.ThuNhapMienThue)),
            ["GiamTruBanThan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.SoGiamTruBanThan),
            ["SoNguoiGTGC"] = (_, list, c) => c.GiaTri = list.Sum(a => a.SpNguoiDangKyNPT),
            ["SoTienGTGC"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TongTienGiamTruGiaCanh),
            ["ThuNhapTinhThue"] = (_, list, c) => c.GiaTri = list.Sum(a => a.ThuNhapTinhThueTNCN),
            ["CongNghiKeHoach"] = (_, list, c) => c.GiaTri = list.Sum(a => a.CongNghiKeHoach),
            ["LuongNghiKeHoach"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongNghiKeHoach),
            ["HoTroPhongSach"] = (_, list, c) => c.GiaTri = list.Sum(a => a.HoTroPhongSach),
            ["HoTroBHYT"] = (_, list, c) => c.GiaTri = list.Sum(a => a.HoTroBHYT),
            ["TruTienBHCN"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TruTienBHCN),
            ["ThanhToanBHCN"] = (_, list, c) => c.GiaTri = list.Sum(a => a.ThanhToanBHCN),
            ["GiamTruDoViPham"] = (_, list, c) => c.GiaTri = list.Sum(a => a.GiamTruDoViPham),
            ["HoTroNhatCuaRoi"] = (_, list, c) => c.GiaTri = list.Sum(a => a.HoTroNhatCuaRoi),
            ["CacKhoanCongTyThanhToan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.CacKhoanCongTyThanhToan),
            ["HoTroCapPhatBHLD"] = (_, list, c) => c.GiaTri = list.Sum(a => a.HoTroCapPhatBHLD),
            ["CongOm"] = (_, list, c) => c.GiaTri = list.Sum(a => a.CongOm),
            ["GiamDanhGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.GiamDanhGia),
            ["LiXiTrucTet"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LiXiTrucTet),
            ["SoTienDongThue"] = (_, list, c) => c.GiaTri = list.Sum(a => a.ThueTNCNTheoLuyTien),
            ["HoTro"] = (h, list, c) =>
            {
                var allowances = list.SelectMany(b => b.listPhuCap.Where(a => a.IddmPhuCap == h.IddmLabel)).ToList();
                c.GiaTri = allowances.Sum(a => a.SoTienPhuCapHienThi);
                c.IddmLabel = h.IddmLabel;
            },
            ["CongPhep"] = (_, list, c) => c.GiaTri = list.Sum(a => a.NghiPhep),
            ["CongDem"] = (_, list, c) => c.GiaTri = list.Sum(a => a.CongDem),
            ["CongGioTieuChuan"] = (h, list, c) =>
            {
                var label = h.name;
                var items = list.SelectMany(b => b.listGioTieuChuan.Where(a => $"Công ({a.GioTieuChuan}h)" == label)).ToList();
                c.GiaTri = items.Sum(a => a.TongSoGio / a.GioTieuChuan);
                if (c.GiaTri > 0)
                {
                    c.GiaTri += list
                        .Where(b => b.listGioTieuChuan.Where(a => $"Công ({a.GioTieuChuan}h)" == label).Sum(a => a.TongSoGio / a.GioTieuChuan) > 0)
                        .Sum(a => a.Le_TetKDL);
                }
                if (items.Count > 0)
                {
                    c.heso = items[0].GioTieuChuan;
                }
                c.Name = h.name;
            },
            ["SoCongThucTe"] = (_, list, c) => c.GiaTri = list.Sum(b => b.listGioTieuChuan.Sum(a => a.TongSoGio / a.GioTieuChuan) + b.Le_TetKDL),
            ["SoTienConLai"] = (_, list, c) => c.GiaTri = list.Sum(ComputeStandardRemainingPay),
            ["CongTangCa"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listTangCa.Where(a => $"Công ({a.HeSo}%)" == h.name)).ToList();
                c.GiaTri = items.Sum(a => a.Cong);
                if (items.Count > 0)
                {
                    c.heso = items[0].HeSo;
                }
            },
            ["LuongPhep"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongPhep),
            ["LuongDem"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongTheoCongDem),
            ["LuongGioTieuChuan"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listGioTieuChuan.Where(a => a.GioTieuChuan.ToString() == h.name)).ToList();
                c.GiaTri = items.Sum(a => a.Luong);
                if (items.Count > 0)
                {
                    c.heso = items[0].GioTieuChuan;
                }
            },
            ["LuongNgoaiGio"] = (_, list, c) => c.GiaTri = list.Sum(b => b.listTangCa.Sum(a => a.Luong)),
            ["BaoHiemNgoai"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listBaoHiemNgoai.Where(a => a.IdNoiCap == h.IddmLabel)).ToList();
                c.GiaTri = items.Sum(a => a.SoTien);
            }
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, Action<clsBangTinhLuong_Header, RowBuildContext, clsBangTinhLuong_GiaTri>> StandardRowEvaluators =
        new Dictionary<string, Action<clsBangTinhLuong_Header, RowBuildContext, clsBangTinhLuong_GiaTri>>(StringComparer.Ordinal)
        {
            ["MaSoThue"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.MaSoThueTNCN,
            ["MaNhanVien"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.MaNhanVien,
            ["KyNhan"] = (_, _, c) => c.NoiDung = "",
            ["TenNhanVien"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.TenDayDu,
            ["NgayVaoThang"] = (_, ctx, c) =>
            {
                if (ctx.Employee?.NgayBatDauLamViec != null)
                    c.NoiDung = ctx.Employee.NgayBatDauLamViec.Value.Month.ToString();
            },
            ["NgayVaoNam"] = (_, ctx, c) =>
            {
                if (ctx.Employee?.NgayBatDauLamViec != null)
                    c.NoiDung = ctx.Employee.NgayBatDauLamViec.Value.Year.ToString();
            },
            ["ThamNienNam"] = (_, ctx, c) => c.NoiDung = ctx.SeniorityYears.ToString(),
            ["ThamNienThang"] = (_, ctx, c) => c.NoiDung = ctx.SeniorityMonths.ToString(),
            ["GioiTinh"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.IdGioiTinh,
            ["TLCB_HT"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan + ctx.Data.PhuCapCongChinh,
            ["NgaySinh"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.NgaySinh.ToString("dd/MM/yyyy"),
            ["SDT"] = (_, ctx, c) => c.NoiDung = ctx.PhoneNumber,
            ["QueQuan"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.ChoOHienNayTruDiaChi,
            ["SoHopDong"] = (_, ctx, c) => c.NoiDung = ctx.ContractNumber,
            ["LuongCoBan"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan,
            ["TongLuongCB"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan + ctx.Data.listPhuCap.Sum(a => a.SoTienPhuCapHienThi),
            ["DanhGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongDanhGia,
            ["Thuong"] = (_, ctx, c) => c.GiaTri = ctx.Data.Thuong,
            ["Luong100"] = (_, ctx, c) => c.GiaTri = ctx.Data.Luong100,
            ["Cong100"] = (_, ctx, c) => c.GiaTri = ctx.Data.Cong100,
            ["Luong50"] = (_, ctx, c) => c.GiaTri = ctx.Data.Luong50,
            ["Cong50"] = (_, ctx, c) => c.GiaTri = ctx.Data.Cong50,
            ["HoTroBHYT"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroBHYT,
            ["TruTienBHCN"] = (_, ctx, c) => c.GiaTri = ctx.Data.TruTienBHCN,
            ["ThanhToanBHCN"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThanhToanBHCN,
            ["GiamTruDoViPham"] = (_, ctx, c) => c.GiaTri = ctx.Data.GiamTruDoViPham,
            ["HoTroNhatCuaRoi"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroNhatCuaRoi,
            ["SoTienPCDiLai"] = (_, ctx, c) => c.GiaTri = ctx.Data.SoTienPCDiLai,
            ["ThanhTienPCDiLai"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongThamNien,
            ["TienBienChe"] = (_, ctx, c) => c.GiaTri = ctx.Data.TienBienChe,
            ["LeTet"] = (_, ctx, c) => c.GiaTri = ctx.Data.Le_Tet,
            ["LeTetKDL"] = (_, ctx, c) => c.GiaTri = ctx.Data.Le_TetKDL,
            ["LuongLe"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongLe,
            ["LuongLeKDL"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongLeKDL,
            ["LuongDongBHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.MucDongBaoHiemXaHoi,
            ["LuongBHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHYT + ctx.Data.NLDBHXH + ctx.Data.NLDBHTN,
            ["CongDoan"] = (_, ctx, c) => c.GiaTri = ctx.Data.TienCongDoan,
            ["TruyThu"] = (_, ctx, c) => c.GiaTri = ctx.Data.TruyThu,
            ["TruyHoan"] = (_, ctx, c) => c.GiaTri = ctx.Data.TienTruyHoan,
            ["ThuCoc"] = (_, ctx, c) => c.GiaTri = ctx.Data.DongPhuc,
            ["SoTKNganHang"] = (_, ctx, c) => c.NoiDung = ctx.Data.SoTaiKhoan,
            ["NganHang"] = (_, ctx, c) => c.NoiDung = ctx.Data.NganHang,
            ["TaiKhoanNguon"] = (_, ctx, c) => c.NoiDung = ctx.Data.NganHangNguon,
            ["CongNghiKeHoach"] = (_, ctx, c) => c.GiaTri = ctx.Data.CongNghiKeHoach,
            ["LuongNghiKeHoach"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongNghiKeHoach,
            ["HoTroPhongSach"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroPhongSach,
            ["TA_ThanhTien"] = (_, ctx, c) => c.GiaTri = ctx.Data.TA_ThanhTien,
            ["GiamDanhGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.GiamDanhGia,
            ["TK_TruAnCa"] = (_, ctx, c) => c.GiaTri = ctx.Data.TK_TruAnCa,
            ["TK_XuatAn"] = (_, ctx, c) => c.GiaTri = ctx.Data.TK_XuatAn,
            ["CacKhoanCongTyThanhToan"] = (_, ctx, c) => c.GiaTri = ctx.Data.CacKhoanCongTyThanhToan,
            ["CongOm"] = (_, ctx, c) => c.GiaTri = ctx.Data.CongOm,
            ["HoTroCapPhatBHLD"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroCapPhatBHLD,
            ["TK_DonGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.TK_DonGia,
            ["TA_SoCong"] = (_, ctx, c) => c.GiaTri = ctx.Data.TA_SoCong,
            ["TA_DonGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.TA_DonGia,
            ["LiXiTrucTet"] = (_, ctx, c) => c.GiaTri = ctx.Data.LiXiTrucTet,
            ["TongLuongTinhThue"] = (_, ctx, c) => c.GiaTri = Math.Max(0, ctx.Data.TongLuong - ctx.Data.ThuNhapMienThue),
            ["GiamTruBanThan"] = (_, ctx, c) => c.GiaTri = ctx.Data.SoGiamTruBanThan,
            ["SoNguoiGTGC"] = (_, ctx, c) => c.GiaTri = ctx.Data.SpNguoiDangKyNPT,
            ["SoTienGTGC"] = (_, ctx, c) => c.GiaTri = ctx.Data.TongTienGiamTruGiaCanh,
            ["ThuNhapTinhThue"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThuNhapTinhThueTNCN,
            ["SoTienDongThue"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThueTNCNTheoLuyTien,
            ["ThucNhan"] = (_, ctx, c) => c.GiaTri = ComputeStandardNetPay(ctx.Data),
            ["SoTienConLai"] = (_, ctx, c) => c.GiaTri = ComputeStandardRemainingPay(ctx.Data),
            ["HoTro"] = (h, ctx, c) =>
            {
                var pc = ctx.Data.listPhuCap.FirstOrDefault(a => a.IddmPhuCap == h.IddmLabel);
                if (pc != null)
                {
                    c.GiaTri = pc.SoTienPhuCapHienThi;
                    c.LoaiPhuCap = pc.IdLoaiThoiGian;
                    c.IddmLabel = pc.IddmPhuCap;
                }
            },
            ["CongPhep"] = (_, ctx, c) => c.GiaTri = ctx.Data.NghiPhep,
            ["CongDem"] = (_, ctx, c) => c.GiaTri = ctx.Data.CongDem,
            ["CongGioTieuChuan"] = (h, ctx, c) =>
            {
                var pc = ctx.Data.listGioTieuChuan.FirstOrDefault(a => $"Công ({a.GioTieuChuan}h)" == h.name);
                if (pc != null)
                {
                    c.GiaTri = pc.TongSoGio / pc.GioTieuChuan;
                    if (c.GiaTri > 0)
                        c.GiaTri += ctx.Data.Le_TetKDL;
                    c.heso = pc.GioTieuChuan;
                    c.Name = h.name;
                }
            },
            ["SoCongThucTe"] = (_, ctx, c) => c.GiaTri = ctx.Data.listGioTieuChuan.Sum(a => a.TongSoGio / a.GioTieuChuan) + ctx.Data.Le_TetKDL,
            ["CongTangCa"] = (h, ctx, c) =>
            {
                var pc = ctx.Data.listTangCa.FirstOrDefault(a => $"Công ({a.HeSo}%)" == h.name);
                if (pc != null)
                {
                    c.GiaTri = pc.Cong;
                    c.heso = pc.HeSo;
                }
            },
            ["LuongPhep"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongPhep,
            ["LuongDem"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongTheoCongDem,
            ["TongLuong"] = (_, ctx, c) => c.GiaTri = ctx.Data.listGioTieuChuan.Sum(a => a.Luong) + ctx.Data.PhuCapCongChinh + ctx.Data.LuongLeKDL,
            ["TongLuongDanhGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan + ctx.Data.PhuCapCongChinh + ctx.Data.LuongDanhGia,
            ["TongKhauTru"] = (_, ctx, c) => c.GiaTri = ComputeStandardDeductions(ctx.Data),
            ["LuongNgoaiGio"] = (_, ctx, c) => c.GiaTri = ctx.Data.listTangCa.Sum(pc => pc.Luong),
            ["BaoHiemNgoai"] = (h, ctx, c) =>
            {
                var pc = ctx.Data.listBaoHiemNgoai.FirstOrDefault(a => a.IdNoiCap == h.IddmLabel);
                if (pc != null)
                    c.GiaTri = pc.SoTien;
            }
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, Action<clsBangTinhLuong_Header, List<HRM_QuyTrinhTinhLuong_NhanVien>, clsBangTinhLuong_GiaTri>> PhtTotalEvaluators =
        new Dictionary<string, Action<clsBangTinhLuong_Header, List<HRM_QuyTrinhTinhLuong_NhanVien>, clsBangTinhLuong_GiaTri>>(StringComparer.Ordinal)
        {
            ["TenNhanVien"] = (_, _, c) => c.NoiDung = "Tổng Cộng",
            ["ThuCoc"] = (_, list, c) => c.GiaTri = list.Sum(a => a.DongPhuc),
            ["LuongCoBan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongCoBan + a.LuongThamNien),
            ["DanhGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongDanhGia),
            ["TongLuong"] = (_, list, c) => c.GiaTri = list.Sum(d => d.listGioTieuChuan.Sum(a => a.Luong) + d.PhuCapCongChinh + d.PhuCapKhongTheoCongChinh),
            ["TongLuongDanhGia"] = (_, list, c) => c.GiaTri = list.Sum(d => d.LuongCoBan + d.PhuCapCongChinh + d.LuongDanhGia),
            ["TongKhauTru"] = (_, list, c) => c.GiaTri = list.Sum(d => (d.TienCongDoan ?? 0) + d.TruyThu + (d.DongPhuc ?? 0) + d.NLDBHYT + d.NLDBHXH + d.NLDBHTN + (d.TK_TruAnCa ?? 0) + (d.GiamDanhGia ?? 0)),
            ["PhuCapKhac"] = (_, list, c) => c.GiaTri = list.Sum(d => d.PhuCapKhongTheoCongChinh),
            ["TienThucNhan"] = (_, list, c) => c.GiaTri = list.Sum(ComputePhtNetPay),
            ["SoTienThucChi"] = (_, list, c) => c.GiaTri = list.Sum(ComputePhtActualPaid),
            ["TongCong"] = (_, list, c) => c.GiaTri = list.Sum(d => d.NgayCong + d.NgayCongCaGay),
            ["Thuong"] = (_, list, c) => c.GiaTri = list.Sum(a => a.Thuong),
            ["LuongChinh"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongTheoCong),
            ["LuongDongBHXH"] = (_, list, c) => c.GiaTri = list.Sum(a => a.MucDongBaoHiemXaHoi),
            ["LuongBHXH"] = (_, list, c) => c.GiaTri = list.Sum(a => a.NLDBHYT + a.NLDBHXH + a.NLDBHTN),
            ["CongDoan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienCongDoan),
            ["TruyThu"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TruyThu),
            ["TruyHoan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienTruyHoan ?? 0),
            ["ThucNhan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TienThucNhan) + list.Sum(a => a.TienTruyHoan ?? 0),
            ["TongLuongTinhThue"] = (_, list, c) => c.GiaTri = Math.Max(0, list.Sum(a => a.TongLuong - a.ThuNhapMienThue)),
            ["GiamTruBanThan"] = (_, list, c) => c.GiaTri = list.Sum(a => a.SoGiamTruBanThan),
            ["SoNguoiGTGC"] = (_, list, c) => c.GiaTri = list.Sum(a => a.SpNguoiDangKyNPT),
            ["SoTienGTGC"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TongTienGiamTruGiaCanh),
            ["ThuNhapTinhThue"] = (_, list, c) => c.GiaTri = list.Sum(a => a.ThuNhapTinhThueTNCN),
            ["SoTienDongThue"] = (_, list, c) => c.GiaTri = list.Sum(a => a.ThueTNCNTheoLuyTien),
            ["HoTro"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listPhuCap.Where(a => a.IddmPhuCap == h.IddmLabel)).ToList();
                c.IddmLabel = h.IddmLabel;
                c.GiaTri = items.Sum(a => a.SoTienPhuCap);
            },
            ["CongPhep"] = (_, list, c) => c.GiaTri = list.Sum(a => a.NghiPhep),
            ["CongDem"] = (_, list, c) => c.GiaTri = list.Sum(a => a.CongDem),
            ["CongGioTieuChuan"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listGioTieuChuan.Where(a => $"Công ({a.GioTieuChuan}h)" == h.name)).ToList();
                c.GiaTri = items.Sum(a => a.TongSoGio / a.GioTieuChuan);
                if (items.Count > 0)
                    c.heso = items[0].GioTieuChuan;
            },
            ["CongTangCa"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listTangCa.Where(a => $"Công ({a.HeSo}%)" == h.name)).ToList();
                c.GiaTri = items.Sum(a => a.Cong);
                if (items.Count > 0)
                    c.heso = items[0].HeSo;
            },
            ["LuongPhep"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongPhep),
            ["LuongTangCa"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongTangCa),
            ["LuongDem"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongTheoCongDem),
            ["LuongGioTieuChuan"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listGioTieuChuan.Where(a => a.GioTieuChuan.ToString() == h.name)).ToList();
                c.GiaTri = items.Sum(a => a.Luong);
                if (items.Count > 0)
                    c.heso = items[0].GioTieuChuan;
            },
            ["LuongNgoaiGio"] = (_, list, c) => c.GiaTri = list.Sum(b => b.listTangCa.Sum(a => a.Luong)),
            ["TongLuongDuyet"] = (_, list, c) => c.GiaTri = list.Sum(ComputePhtApprovedSalary),
            ["TA_ThanhTien"] = (_, list, c) => c.GiaTri = list.Sum(a => a.TA_ThanhTien),
            ["NgayCongDem"] = (_, list, c) => c.GiaTri = list.Sum(a => a.NgayCongDem),
            ["LuongDanhGia"] = (_, list, c) => c.GiaTri = list.Sum(a => a.LuongDanhGia),
            ["BaoHiemNgoai"] = (h, list, c) =>
            {
                var items = list.SelectMany(b => b.listBaoHiemNgoai.Where(a => a.IdNoiCap == h.IddmLabel)).ToList();
                c.GiaTri = items.Sum(a => a.SoTien);
            }
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, Action<clsBangTinhLuong_Header, RowBuildContext, clsBangTinhLuong_GiaTri>> PhtRowEvaluators =
        new Dictionary<string, Action<clsBangTinhLuong_Header, RowBuildContext, clsBangTinhLuong_GiaTri>>(StringComparer.Ordinal)
        {
            ["MaSoThue"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.MaSoThueTNCN,
            ["CoSo"] = (_, ctx, c) => c.NoiDung = ctx.Data.MaCoSo,
            ["MaNhanVien"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.MaNhanVien,
            ["TenNhanVien"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.TenDayDu,
            ["BoPhan"] = (_, ctx, c) => c.NoiDung = ctx.Data.MaBoPhan,
            ["ChucDanh"] = (_, ctx, c) => c.NoiDung = ctx.Data.TenChucDanh,
            ["CaLamViec"] = (_, ctx, c) => c.NoiDung = ctx.Data.CaLamViec,
            ["NgayCongCoSo"] = (_, ctx, c) => c.GiaTri = ctx.Data.NgayCongTieuChuan,
            ["NgayVaoThang"] = (_, ctx, c) =>
            {
                if (ctx.Employee?.NgayBatDauLamViec != null)
                    c.NoiDung = ctx.Employee.NgayBatDauLamViec.Value.Month.ToString();
            },
            ["NgayVaoNam"] = (_, ctx, c) =>
            {
                if (ctx.Employee?.NgayBatDauLamViec != null)
                    c.NoiDung = ctx.Employee.NgayBatDauLamViec.Value.Year.ToString();
            },
            ["LuongCoBan"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan,
            ["ThamNienNam"] = (_, ctx, c) => c.NoiDung = ctx.SeniorityYears.ToString(),
            ["ThamNienThang"] = (_, ctx, c) => c.NoiDung = ctx.SeniorityMonths.ToString(),
            ["GioiTinh"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.IdGioiTinh,
            ["NgaySinh"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.NgaySinh.ToString("dd/MM/yyyy"),
            ["SDT"] = (_, ctx, c) => c.NoiDung = ctx.PhoneNumber,
            ["QueQuan"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.ChoOHienNayTruDiaChi,
            ["SoHopDong"] = (_, ctx, c) => c.NoiDung = ctx.ContractNumber,
            ["DanhGia"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongDanhGia,
            ["TongPhuCapCC"] = (_, ctx, c) => c.GiaTri = ctx.Data.listPhuCap.Where(a => a.IdLoaiThoiGian != 4 && a.Ma != "PCK-KCT").Sum(a => a.SoTienPhuCap),
            ["PhuCapKhac"] = (_, ctx, c) => c.GiaTri = ctx.Data.PhuCapKhongTheoCongChinh,
            ["TongLuong"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongCoBan + ctx.Data.PhuCapCongChinh + ctx.Data.PhuCapKhongTheoCongChinh,
            ["TongCong"] = (_, ctx, c) => c.GiaTri = ctx.Data.NgayCong + ctx.Data.NgayCongCaGay,
            ["CaF"] = (_, ctx, c) => c.GiaTri = ctx.Data.NgayCong,
            ["CaP"] = (_, ctx, c) => c.GiaTri = ctx.Data.NgayCongCaGay,
            ["TangCa"] = (_, ctx, c) => c.GiaTri = ctx.Data.NgayTangCa,
            ["CongPhep"] = (_, ctx, c) => c.GiaTri = ctx.Data.NghiPhep,
            ["LeTet"] = (_, ctx, c) => c.GiaTri = ctx.Data.Le_Tet,
            ["LeTetKDL"] = (_, ctx, c) => c.GiaTri = ctx.Data.Le_TetKDL,
            ["NghiBu"] = (_, ctx, c) => c.GiaTri = ctx.Data.NghiBu,
            ["LuongChinh"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongTheoCong,
            ["LuongPhep"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongPhep,
            ["LuongLe"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongLe,
            ["LuongLeKDL"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongLeKDL,
            ["LuongNB"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongNghiBu,
            ["PCCongChinh"] = (_, ctx, c) => c.GiaTri = ctx.Data.PhuCapCongChinh,
            ["PCCongPhu"] = (_, ctx, c) => c.GiaTri = ctx.Data.PhuCapKhongTheoCongChinh,
            ["TruyThu"] = (_, ctx, c) => c.GiaTri = ctx.Data.TamUng,
            ["HoTroKhac"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroKhac,
            ["LuongTangCa"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongTangCa,
            ["DCKhac"] = (_, ctx, c) => c.GiaTri = ctx.Data.DCKhac,
            ["HoTroNghiViec"] = (_, ctx, c) => c.GiaTri = ctx.Data.HoTroNghiViec,
            ["TongLuongDuyet"] = (_, ctx, c) => c.GiaTri = ComputePhtApprovedSalary(ctx.Data),
            ["ThuDongPhuc"] = (_, ctx, c) => c.GiaTri = ctx.Data.DongPhuc,
            ["CamKet"] = (_, ctx, c) => c.NoiDung = ctx.Employee?.isCamKet == true ? "Có" : "Không",
            ["GiamTruBanThan"] = (_, ctx, c) => c.GiaTri = ctx.Data.SoGiamTruBanThan,
            ["SoNguoiGTGC"] = (_, ctx, c) => c.GiaTri = ctx.Data.SpNguoiDangKyNPT,
            ["SoTienGTGC"] = (_, ctx, c) => c.GiaTri = ctx.Data.TongTienGiamTruGiaCanh,
            ["TongGiamTru"] = (_, ctx, c) => c.GiaTri = ctx.Data.TongTienGiamTruGiaCanh,
            ["ThuNhapMienThue"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThuNhapMienThue,
            ["KhauTruTaiNguon"] = (_, ctx, c) => c.GiaTri = ctx.Data.KhauTruTaiNguon,
            ["TongGiamTruTNCN"] = (_, ctx, c) => c.GiaTri = ctx.Data.TongGiamThueTNCN,
            ["ThuNhapTinhThue"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThuNhapTinhThueTNCN,
            ["SoTienDongThue"] = (_, ctx, c) => c.GiaTri = ctx.Data.ThueTNCNTheoLuyTien,
            ["SD_BHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.SDLDBHXH,
            ["SD_BHYT"] = (_, ctx, c) => c.GiaTri = ctx.Data.SDLDBHYT,
            ["SD_BHTN"] = (_, ctx, c) => c.GiaTri = ctx.Data.SDLDBHTN,
            ["SD_Tong"] = (_, ctx, c) => c.GiaTri = ctx.Data.SDLDBHXH + ctx.Data.SDLDBHYT + ctx.Data.SDLDBHTN,
            ["LD_BHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHXH,
            ["LD_BHYT"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHYT,
            ["LD_BHTN"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHTN,
            ["LoaiHopDong"] = (_, ctx, c) => c.NoiDung = ctx.Data.TenLoaiHopDong,
            ["TongGT"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHYT + ctx.Data.NLDBHXH + ctx.Data.NLDBHTN + ctx.Data.TongGiamThueTNCN + ctx.Data.DongPhuc + ctx.Data.TruyThu,
            ["TraTienDP"] = (_, ctx, c) => c.GiaTri = ctx.Data.TraTienDongPhuc,
            ["TienThucNhan"] = (_, ctx, c) => c.GiaTri = ComputePhtNetPay(ctx.Data),
            ["SoTienThucChi"] = (_, ctx, c) => c.GiaTri = ComputePhtActualPaid(ctx.Data),
            ["TruyThuBHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.TruyThu,
            ["HoTro"] = (h, ctx, c) =>
            {
                var pc = ctx.Data.listPhuCap.FirstOrDefault(a => a.IddmPhuCap == h.IddmLabel);
                if (pc != null)
                {
                    c.IddmLabel = h.IddmLabel;
                    c.GiaTri = pc.SoTienPhuCap;
                }
            },
            ["TA_ThanhTien"] = (_, ctx, c) => c.GiaTri = ctx.Data.TA_ThanhTien,
            ["Thuong"] = (_, ctx, c) => c.GiaTri = ctx.Data.Thuong,
            ["LuongDongBHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.LuongDongBHXH,
            ["LuongBHXH"] = (_, ctx, c) => c.GiaTri = ctx.Data.NLDBHYT + ctx.Data.NLDBHXH + ctx.Data.NLDBHTN
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public List<clsBangTinhLuong_NhanVien> BuildStandardGrid(
        List<clsBangTinhLuong_Header> headers,
        List<HRM_QuyTrinhTinhLuong_NhanVien> data,
        List<HRM_NhanVien> employees,
        List<HRM_HopDong> contracts,
        IHrmDbContext db)
        => BuildGridCore(headers, data, employees, contracts, db, isPhtMode: false);

    public List<clsBangTinhLuong_NhanVien> BuildPhtGrid(
        List<clsBangTinhLuong_Header> headers,
        List<HRM_QuyTrinhTinhLuong_NhanVien> data,
        List<HRM_NhanVien> employees,
        List<HRM_HopDong> contracts,
        IHrmDbContext db)
        => BuildGridCore(headers, data, employees, contracts, db, isPhtMode: true);

    private static List<clsBangTinhLuong_NhanVien> BuildGridCore(
        List<clsBangTinhLuong_Header> headers,
        List<HRM_QuyTrinhTinhLuong_NhanVien> data,
        List<HRM_NhanVien> employees,
        List<HRM_HopDong> contracts,
        IHrmDbContext db,
        bool isPhtMode)
    {
        var listResult = new List<clsBangTinhLuong_NhanVien>();
        var empIds = employees.Select(a => a.IdGuid).ToList();

        var phoneLookup = db.HRM_NhanVien_SoDienThoai
            .Where(a => empIds.Contains(a.IdNhanVien) && a.isApDung == true)
            .AsNoTracking()
            .ToList()
            .GroupBy(a => a.IdNhanVien)
            .ToDictionary(g => g.Key, g => g.First().SoDienThoai);

        var empLookup = employees.GroupBy(e => e.IdGuid).ToDictionary(g => g.Key, g => g.First());
        var contractLookup = contracts.GroupBy(c => c.IdNhanVien).ToDictionary(g => g.Key, g => g.First().SoHopDong);

        var totalEvaluators = isPhtMode ? PhtTotalEvaluators : StandardTotalEvaluators;
        var rowEvaluators = isPhtMode ? PhtRowEvaluators : StandardRowEvaluators;

        var totalRow = new clsBangTinhLuong_NhanVien
        {
            listGiaTri = new List<clsBangTinhLuong_GiaTri>(headers.Count)
        };

        foreach (var header in headers)
        {
            var cell = new clsBangTinhLuong_GiaTri
            {
                SoThapPhan = header.SoThapPhan,
                isDuocNhap = false,
                Ma = "TONG"
            };
            if (header.Ma != null && totalEvaluators.TryGetValue(header.Ma, out var eval))
            {
                eval(header, data, cell);
            }
            totalRow.listGiaTri.Add(cell);
        }
        listResult.Add(totalRow);

        var now = DateTime.Now;
        var orderedRows = isPhtMode ? data : data.OrderBy(a => a.NgayVaoLam).ToList();

        foreach (var row in orderedRows)
        {
            empLookup.TryGetValue(row.IdNhanVien ?? string.Empty, out var emp);
            phoneLookup.TryGetValue(row.IdNhanVien ?? string.Empty, out var phone);
            contractLookup.TryGetValue(row.IdNhanVien ?? string.Empty, out var contractNo);

            int seniorityYears = 0;
            int seniorityMonths = 0;
            if (emp?.NgayBatDauLamViec != null)
            {
                int totalMonths = ((now.Year - emp.NgayBatDauLamViec.Value.Year) * 12) + now.Month - emp.NgayBatDauLamViec.Value.Month + 1;
                seniorityYears = totalMonths / 12;
                seniorityMonths = totalMonths % 12;
            }

            var itemNv = new clsBangTinhLuong_NhanVien
            {
                IdNhanVien = row.IdNhanVien,
                TenBoPhan = row.TenBoPhan,
                NgayVaoLam = emp?.NgayBatDauLamViec,
                listGiaTri = new List<clsBangTinhLuong_GiaTri>(headers.Count)
            };

            if (isPhtMode)
            {
                itemNv.TenChucDanh = row.TenChucVu;
                itemNv.IdCongTy = row.IdCongTyEOS;
                itemNv.TenCongTy = row.MaCoSo;
            }
            else
            {
                itemNv.isDieuDong = row.isDieuDong;
            }

            var ctx = new RowBuildContext(row, emp, phone, contractNo, seniorityYears, seniorityMonths);
            foreach (var header in headers)
            {
                var cell = new clsBangTinhLuong_GiaTri
                {
                    Ma = header.Ma,
                    SoThapPhan = header.SoThapPhan,
                    isDuocNhap = header.isDuocNhap
                };
                if (header.Ma != null && rowEvaluators.TryGetValue(header.Ma, out var eval))
                {
                    eval(header, ctx, cell);
                }
                itemNv.listGiaTri.Add(cell);
            }

            listResult.Add(itemNv);
        }

        return listResult.OrderBy(a => a.NgayVaoLam).ToList();
    }

    private static double ComputeStandardDeductions(HRM_QuyTrinhTinhLuong_NhanVien d)
        => (d.TienCongDoan ?? 0) + d.TruyThu + (d.DongPhuc ?? 0) + d.NLDBHYT + d.NLDBHXH + d.NLDBHTN
           + (d.TK_TruAnCa ?? 0) + (d.GiamDanhGia ?? 0) + (d.TruTienBHCN ?? 0);

    private static double ComputeStandardNetPay(HRM_QuyTrinhTinhLuong_NhanVien d)
        => d.listGioTieuChuan.Sum(a => a.Luong)
           + d.listPhuCap.Sum(a => a.SoTienPhuCap)
           + (d.LuongDanhGia ?? 0) + d.LuongLe + d.LuongLeKDL + d.LuongTheoCongDem + (d.Thuong ?? 0)
           + (d.LuongNghiKeHoach ?? 0) + d.LuongPhep + (d.Luong50 ?? 0) + (d.Luong100 ?? 0)
           + (d.TA_ThanhTien ?? 0) + (d.HoTroCapPhatBHLD ?? 0) + (d.HoTroBHYT ?? 0) + (d.HoTroNhatCuaRoi ?? 0)
           - (d.GiamTruDoViPham ?? 0)
           - ComputeStandardDeductions(d);

    private static double ComputeStandardRemainingPay(HRM_QuyTrinhTinhLuong_NhanVien d)
        => d.listGioTieuChuan.Sum(a => a.Luong)
           + d.listPhuCap.Sum(a => a.SoTienPhuCap)
           + (d.LuongDanhGia ?? 0) + d.LuongLe + d.LuongLeKDL + d.LuongTheoCongDem + (d.Thuong ?? 0)
           + (d.LuongNghiKeHoach ?? 0) + d.LuongPhep + (d.Luong50 ?? 0) + (d.Luong100 ?? 0)
           + (d.TA_ThanhTien ?? 0) + (d.HoTroCapPhatBHLD ?? 0) + (d.HoTroBHYT ?? 0) + (d.HoTroNhatCuaRoi ?? 0)
           - (d.GiamTruDoViPham ?? 0) + (d.ThanhToanBHCN ?? 0)
           - (d.TruyThu + (d.DongPhuc ?? 0) + d.NLDBHYT + d.NLDBHXH + d.NLDBHTN
              + (d.TK_TruAnCa ?? 0) + (d.GiamDanhGia ?? 0) + (d.TruTienBHCN ?? 0));

    private static double ComputePhtNetPay(HRM_QuyTrinhTinhLuong_NhanVien d)
        => d.TienThucNhan - (d.DongPhuc ?? 0) - d.TruyThu + (d.TraTienDongPhuc ?? 0)
           + d.HoTroKhac + d.HoTroNghiViec + d.DCKhac;

    private static double ComputePhtActualPaid(HRM_QuyTrinhTinhLuong_NhanVien d)
        => d.TienThucNhan - (d.DongPhuc ?? 0) - d.TruyThu - d.TamUng + (d.TraTienDongPhuc ?? 0)
           + d.HoTroKhac + d.HoTroNghiViec + d.DCKhac;

    private static double ComputePhtApprovedSalary(HRM_QuyTrinhTinhLuong_NhanVien d)
        => d.LuongTheoCong + d.LuongPhep + d.LuongLe + d.LuongNghiBu + d.LuongTangCa
           + d.PhuCapCongChinh + d.PhuCapKhongTheoCongChinh + d.HoTroKhac + d.HoTroNghiViec + d.DCKhac;
}