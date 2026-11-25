#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRM.Api.Common;
using HRM.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.Api.Services.Payroll;

/// <summary>
/// Pre-indexed, modular, parallel-safe PHT payroll calculation engine.
/// Consolidates the duplicated PHT calculation logic from GetTinhLuongNhanVienTheoKyChamCongPHT
/// and GetTinhLuongNhanVienTheoChamCongPHTNew, replacing O(N^2) linear scans with O(1) lookups
/// and AsNoTracking() queries while preserving 100% identical formulas and output ordering.
/// </summary>
public static class PhtPayrollCalculator
{
    public sealed class PhtPayrollBatchResult
    {
        public List<HRM_QuyTrinhTinhLuong_NhanVien> SalaryLines { get; init; } = new();
        public List<HRM_NhanVien> Employees { get; init; } = new();
        public List<HRM_HopDong> Contracts { get; init; } = new();
    }

    private sealed class PhtLookupContext
    {
        public DateTime NgayBD { get; init; }
        public DateTime NgayKT { get; init; }
        public ILookup<string, HRM_BangChamCong_NhanVien_ThoiGian> DataByEmp { get; init; }
        public ILookup<string, HRM_Luong_NhanVien> LuongByEmp { get; init; }
        public ILookup<string, HRM_HopDong> HopDongByEmp { get; init; }
        public ILookup<(string EmpId, string LuongId), HRM_Luong_PhuCap_NhanVien> PhuCapByEmpLuong { get; init; }
        public ILookup<string, HRM_ThoiGianLamViecKeHoach> LichByEmp { get; init; }
        public ILookup<string, long> QtBpByEmp { get; init; }
        public Dictionary<string, int> DependentCountByEmp { get; init; }
        public ILookup<string, HRM_TheoDoiXinNghiPhep> NghiPhepByEmp { get; init; }
        public ILookup<string, HRM_CapPhatThuHoiTaiSan_Item> CapPhatByEmp { get; init; }
        public Dictionary<long, string> LichNghiIdByBoPhan { get; init; }
        public ILookup<string, HRM_dmLichNghiLe_Ngay> LichNghiChiTietById { get; init; }
        public List<HRM_dmLichLamViec> CaLamViecs { get; init; }
        public Dictionary<string, HRM_dmLichLamViec> CaLamViecById { get; init; }
        public Dictionary<string, HRM_dmLoaiHopDong> LoaiHopDongById { get; init; }
        public List<HRM_dmPhuCap> PhuCapDefs { get; init; }
        public List<HRM_dmCauHinhThueThuNhapCaNhan> TaxBrackets { get; init; }
        public Dictionary<long, BoPhan> BoPhanById { get; init; }
        public Dictionary<long, BoPhan> DuAnById { get; init; }
        public Dictionary<long, ChucDanh> ChucDanhById { get; init; }
        public Dictionary<long, ChucVu> ChucVuById { get; init; }
        public Func<double, List<HRM_dmCauHinhThueThuNhapCaNhan>, double> CalculateTax { get; init; }
    }

    public static PhtPayrollBatchResult Calculate(
        long idCongTy,
        List<long> idBoPhan,
        DateTime ngayBD,
        DateTime ngayKT,
        List<HRM_BangChamCong_NhanVien_ThoiGian> listData,
        ApplicationDbContext db,
        CallWebService callWebService,
        Func<double, List<HRM_dmCauHinhThueThuNhapCaNhan>, double> tinhThue)
    {
        var listIdNhanVien = listData.Select(x => x.IdNhanVien).Distinct().ToList();
        var employeeQuery = db.HRM_NhanVien.AsNoTracking().Where(x => listIdNhanVien.Contains(x.IdGuid));
        if (idBoPhan != null && idBoPhan.Count > 0)
            employeeQuery = employeeQuery.Where(x => idBoPhan.Contains(x.IdBoPhanEOS));

        var listNhanVien = employeeQuery
            .OrderBy(a => a.NgayBatDauLamViec)
            .ThenBy(a => a.TenDayDu)
            .ToList();

        var listIdBoPhan = listNhanVien.Select(a => a.IdBoPhanEOS)
            .Concat(listNhanVien.Select(a => a.IdCongTyEOS))
            .Distinct()
            .ToList();

        var listBoPhan = callWebService.GetDanhSachBoPhanFromListId(db, listIdBoPhan) ?? new List<BoPhan>();
        var listDuAn = callWebService.GetDanhSachDuAn(db) ?? new List<BoPhan>();
        var listChucDanh = callWebService.GetDanhSachChucDanh(db) ?? new List<ChucDanh>();
        var listChucVu = callWebService.GetDanhSachChucVu(db) ?? new List<ChucVu>();

        var listHopDong = db.HRM_HopDong.AsNoTracking()
            .Where(a => listIdNhanVien.Contains(a.IdNhanVien))
            .OrderByDescending(a => a.NgayHieuLuc)
            .ToList();

        var caLamViecs = db.HRM_dmLichLamViec.AsNoTracking().ToList();
        var loaiHopDongs = db.HRM_dmLoaiHopDong.AsNoTracking().ToList();
        var listPhuCapdb = db.HRM_dmPhuCap.AsNoTracking().OrderBy(a => a.SapXep).ToList();

        var listPhuCap = db.HRM_Luong_PhuCap_NhanVien.AsNoTracking()
            .Where(a => listIdNhanVien.Contains(a.IdNhanVien)
                && a.NgayBatDauHieuLuc <= ngayKT
                && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= ngayBD))
            .ToList();

        var listLuong = db.HRM_Luong_NhanVien.AsNoTracking()
            .Where(a => listIdNhanVien.Contains(a.IdNhanVien)
                && a.NgayBatDauHieuLuc <= ngayKT
                && (a.NgayHetHieuLuc == null || a.NgayHetHieuLuc >= ngayBD))
            .OrderByDescending(a => a.NgayBatDauHieuLuc)
            .ToList();

        var coveredEmpIds = listLuong.Select(a => a.IdNhanVien).Distinct().ToList();
        if (coveredEmpIds.Count < listIdNhanVien.Count)
        {
            var expiredLuong = (from a in db.HRM_Luong_NhanVien.AsNoTracking()
                                where listIdNhanVien.Contains(a.IdNhanVien)
                                    && !coveredEmpIds.Contains(a.IdNhanVien)
                                    && a.NgayHetHieuLuc != null
                                    && a.NgayHetHieuLuc.Value.Year > 2000
                                    && a.NgayHetHieuLuc <= ngayBD
                                group a by a.IdNhanVien into g
                                select g.OrderByDescending(x => x.NgayBatDauHieuLuc).FirstOrDefault())
                                .ToList();
            foreach (var l in expiredLuong)
            {
                if (l != null) listLuong.Add(l);
            }
        }

        var dependentCountByEmp = db.HRM_NhanVienQuanHeGiaDinh.AsNoTracking()
            .Where(a => listIdNhanVien.Contains(a.IdNhanVien) && a.isLaNguoiPhuThuoc == true)
            .GroupBy(a => a.IdNhanVien)
            .ToDictionary(g => g.Key, g => g.Count());

        var validLeaveCodes = new HashSet<string>(StringComparer.Ordinal)
        {
            DefineData.eKyHieuChamCong.PN.ToString(),
            DefineData.eKyHieuChamCong.NP.ToString(),
            DefineData.eKyHieuChamCong.NC.ToString(),
            DefineData.eKyHieuChamCong.NTL.ToString()
        };
        var idLoaiPhep = (callWebService.GetListDanhMucHinhThucNghi(db) ?? new List<DanhMuc>())
            .Where(a => validLeaveCodes.Contains(a.Ma))
            .Select(a => a.Id)
            .ToList();

        var listXinNghiPhep = db.HRM_TheoDoiXinNghiPhep.AsNoTracking()
            .Where(a => a.IdCongTy == idCongTy
                && listIdNhanVien.Contains(a.IdNhanVien)
                && a.NgayNghi >= ngayBD
                && a.NgayNghi <= ngayKT
                && idLoaiPhep.Contains(a.IdHinhThucNghi))
            .ToList();

        var listCapPhatTaiSan = db.HRM_CapPhatThuHoiTaiSan_Item.AsNoTracking()
            .Where(a => listIdNhanVien.Contains(a.IdNhanVien)
                && a.TinhLuong == true
                && ((a.NgayCapPhat != null && a.NgayCapPhat >= ngayBD && a.NgayCapPhat <= ngayKT)
                    || (a.NgayThuHoi != null && a.NgayThuHoi >= ngayBD && a.NgayThuHoi <= ngayKT)))
            .ToList();

        var listLichNghi = db.HRM_dmLichNghiLe_BoPhan.AsNoTracking()
            .Where(a => listIdBoPhan.Contains(a.IdBoPhan)
                && a.IdCongTy == idCongTy
                && (a.Nam == ngayBD.Year || a.Nam == ngayKT.Year))
            .ToList();

        var listIdLichNghi = listLichNghi.Select(a => a.IdLichNghiLe).Distinct().ToList();
        var listLichNghiChiTiet = listIdLichNghi.Count > 0
            ? db.HRM_dmLichNghiLe_Ngay.AsNoTracking()
                .Where(a => listIdLichNghi.Contains(a.IdLichNghiLe) && a.TuNgay <= ngayKT && a.DenNgay >= ngayBD)
                .ToList()
            : new List<HRM_dmLichNghiLe_Ngay>();

        var listCauHinh = db.HRM_dmCauHinhThueThuNhapCaNhan.AsNoTracking()
            .OrderByDescending(a => a.STT)
            .ToList();

        var listLichLamViec = db.HRM_ThoiGianLamViecKeHoach.AsNoTracking()
            .Where(x => listIdNhanVien.Contains(x.IdNhanVien)
                && x.IdCongTy == idCongTy
                && x.Ngay >= ngayBD
                && x.Ngay <= ngayKT)
            .ToList();

        var listQtBp = db.HRM_QuyTrinhBoPhan_BoPhan.AsNoTracking()
            .Where(x => listIdNhanVien.Contains(x.IdQuyTrinh))
            .Select(x => new { x.IdQuyTrinh, x.IdBoPhan })
            .ToList();

        foreach (var d in listData)
            d.isNgayLe = false;

        var ctx = new PhtLookupContext
        {
            NgayBD = ngayBD,
            NgayKT = ngayKT,
            DataByEmp = listData.ToLookup(a => a.IdNhanVien),
            LuongByEmp = listLuong.ToLookup(a => a.IdNhanVien),
            HopDongByEmp = listHopDong.ToLookup(a => a.IdNhanVien),
            PhuCapByEmpLuong = listPhuCap.ToLookup(a => (a.IdNhanVien, a.IdLuong_NhanVien)),
            LichByEmp = listLichLamViec.ToLookup(a => a.IdNhanVien),
            QtBpByEmp = listQtBp.ToLookup(x => x.IdQuyTrinh, x => x.IdBoPhan),
            DependentCountByEmp = dependentCountByEmp,
            NghiPhepByEmp = listXinNghiPhep.ToLookup(a => a.IdNhanVien),
            CapPhatByEmp = listCapPhatTaiSan.ToLookup(a => a.IdNhanVien),
            LichNghiIdByBoPhan = listLichNghi
                .GroupBy(a => a.IdBoPhan)
                .ToDictionary(g => g.Key, g => g.First().IdLichNghiLe),
            LichNghiChiTietById = listLichNghiChiTiet.ToLookup(a => a.IdLichNghiLe),
            CaLamViecs = caLamViecs,
            CaLamViecById = caLamViecs
                .Where(a => !string.IsNullOrEmpty(a.IdGuid))
                .GroupBy(a => a.IdGuid)
                .ToDictionary(g => g.Key, g => g.First()),
            LoaiHopDongById = loaiHopDongs
                .Where(a => !string.IsNullOrEmpty(a.IdGuid))
                .GroupBy(a => a.IdGuid)
                .ToDictionary(g => g.Key, g => g.First()),
            PhuCapDefs = listPhuCapdb,
            TaxBrackets = listCauHinh,
            BoPhanById = listBoPhan.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.First()),
            DuAnById = listDuAn.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.First()),
            ChucDanhById = listChucDanh.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.First()),
            ChucVuById = listChucVu.GroupBy(a => a.Id).ToDictionary(g => g.Key, g => g.First()),
            CalculateTax = tinhThue
        };

        // Parallel per-employee calculation with deterministic output ordering
        var perEmployeeResults = new List<HRM_QuyTrinhTinhLuong_NhanVien>[listNhanVien.Count];
        Parallel.For(0, listNhanVien.Count, idx =>
        {
            perEmployeeResults[idx] = CalculateEmployee(listNhanVien[idx], ctx);
        });

        var listResult = new List<HRM_QuyTrinhTinhLuong_NhanVien>(listNhanVien.Count);
        for (int i = 0; i < perEmployeeResults.Length; i++)
        {
            if (perEmployeeResults[i] != null && perEmployeeResults[i].Count > 0)
                listResult.AddRange(perEmployeeResults[i]);
        }

        return new PhtPayrollBatchResult
        {
            SalaryLines = listResult,
            Employees = listNhanVien,
            Contracts = listHopDong
        };
    }

    private static List<HRM_QuyTrinhTinhLuong_NhanVien> CalculateEmployee(HRM_NhanVien item, PhtLookupContext ctx)
    {
        var results = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
        var listLuongNV = ctx.LuongByEmp[item.IdGuid].OrderBy(a => a.NgayBatDauHieuLuc).ToList();
        if (listLuongNV.Count == 0)
            return results;

        var listdatas = ctx.DataByEmp[item.IdGuid].ToList();
        if (listdatas.Count == 0)
            return results;

        var empContracts = ctx.HopDongByEmp[item.IdGuid].ToList();
        var empShifts = ctx.LichByEmp[item.IdGuid].ToList();
        var empLeaves = ctx.NghiPhepByEmp[item.IdGuid].ToList();

        var listLuongCheck = new List<HRM_QuyTrinhTinhLuong_NhanVien>();
        int stt = 1;
        bool isDaTinhBHXH = false;

        foreach (var luong in listLuongNV)
        {
            var datas = listdatas
                .Where(a => a.NgayChamCong >= luong.NgayBatDauHieuLuc
                    && (luong.NgayHetHieuLuc == null || (luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayChamCong <= luong.NgayHetHieuLuc)))
                .OrderBy(a => a.NgayChamCong)
                .ToList();

            if (datas.Count == 0)
                continue;

            var itemAdd = new HRM_QuyTrinhTinhLuong_NhanVien
            {
                LoaiLuong = luong.LoaiLuong,
                LuongDongBHXH = luong.LuongHachToan,
                LuongCoBan = luong.Luong,
                NgayApDung = luong.NgayBatDauHieuLuc,
                listPhuCap = new List<HRM_QuyTrinhTinhLuong_PhuCap>(),
                PhuCapCongChinh = 0,
                PhuCapKhongTheoCongChinh = 0
            };

            var hopDong = empContracts.FirstOrDefault(a => a.IdGuid == luong.IdNguonNhapLuong);
            ResolveShift(item, luong, hopDong, empShifts, ctx, itemAdd);

            // Workdays & overtime days
            if (itemAdd.SoGioMotNgay > 0)
            {
                if (itemAdd.LoaiLuong == 3)
                {
                    itemAdd.NgayCong = datas.Sum(a => a.SoGioLamViec ?? 0);
                    itemAdd.NgayCongCaGay = datas.Sum(a => a.SoGioLamViecCaGay ?? 0);
                    itemAdd.NgayTangCa = datas.Sum(a => a.SoGioLamViecTangCa ?? 0);
                }
                else
                {
                    itemAdd.NgayCong = datas.Sum(a => (a.SoGioLamViec ?? 0) / (a.SoGioTieuChuan ?? 8));
                    itemAdd.NgayCongCaGay = datas.Sum(a => (a.SoGioLamViecCaGay ?? 0) / (a.SoGioTieuChuan ?? 8));
                    itemAdd.NgayTangCa = datas.Sum(a => (a.SoGioLamViecTangCa ?? 0) / (a.SoGioTieuChuan ?? 8));
                }
            }

            PopulateEmployeeProfile(item, ctx, itemAdd);
            CalculateAllowances(item, luong, datas, ctx, itemAdd);
            CalculateHolidays(item, luong, datas, empShifts, ctx, itemAdd);

            // Base salary, leave pay, overtime pay
            double cong1Ngay = itemAdd.LuongCoBan / itemAdd.NgayCongTieuChuan;
            itemAdd.LuongTheoCong = 0;

            if (empLeaves.Count > 0)
            {
                itemAdd.NghiPhep = empLeaves
                    .Where(a => a.NgayNghi >= luong.NgayBatDauHieuLuc
                        && (luong.NgayHetHieuLuc == null || (luong.NgayHetHieuLuc.Value.Year > 2000 && a.NgayNghi <= luong.NgayHetHieuLuc)))
                    .Sum(a => a.ThoiGianNghi);
                itemAdd.LuongPhep = itemAdd.LoaiLuong == 3
                    ? itemAdd.NghiPhep * itemAdd.SoGioMotNgay * itemAdd.LuongCoBan
                    : itemAdd.NghiPhep * cong1Ngay;
            }

            itemAdd.LuongTheoCong = itemAdd.LoaiLuong == 3
                ? (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * itemAdd.LuongCoBan
                : (itemAdd.NgayCong + itemAdd.NgayCongCaGay) * cong1Ngay;

            itemAdd.LuongTangCa = itemAdd.LoaiLuong == 3
                ? itemAdd.NgayTangCa * itemAdd.LuongCoBan
                : itemAdd.NgayTangCa * cong1Ngay;

            if (itemAdd.NgayTangCa < 0) itemAdd.NgayTangCa = 0;
            if (itemAdd.LuongTangCa < 0) itemAdd.LuongTangCa = 0;
            if (itemAdd.NgayCong < 0) itemAdd.NgayCong = 0;
            if (itemAdd.NgayCongCaGay < 0) itemAdd.NgayCongCaGay = 0;
            if (itemAdd.Le_TetKDL < 0.1)
            {
                itemAdd.Le_TetKDL = 0;
                itemAdd.LuongLeKDL = 0;
            }

            itemAdd.TongLuong = itemAdd.LuongTheoCong + itemAdd.LuongTangCa + itemAdd.LuongPhep
                + itemAdd.LuongNghiBu + itemAdd.LuongLe + itemAdd.LuongLeKDL
                + itemAdd.PhuCapCongChinh + itemAdd.PhuCapKhongTheoCongChinh;

            if ((item.ThangTruyThuBHXH != null && item.ThangTruyThuBHXH >= ctx.NgayBD && item.ThangTruyThuBHXH <= ctx.NgayBD)
                || (item.ThangTruyThuBHYT != null && item.ThangTruyThuBHYT >= ctx.NgayBD && item.ThangTruyThuBHYT <= ctx.NgayBD))
            {
                itemAdd.TruyThu = itemAdd.LuongDongBHXH * ((item.PhanTramTruyThuBHYT ?? 0) + (item.PhanTramTruyThuBHXH ?? 0)) / 100;
            }

            if (luong.LuongHachToan > 0 && luong.LoaiLuong == 1 && !isDaTinhBHXH)
            {
                CalculateInsurance(listLuongNV, listdatas, itemAdd);
                isDaTinhBHXH = true;
            }

            listLuongCheck.Add(itemAdd);
            if (stt == listLuongNV.Count)
            {
                CalculateTaxAndAssets(item, listLuongCheck, ctx, itemAdd);
            }

            stt++;
            itemAdd.TienThucNhan = itemAdd.TongLuong - itemAdd.ThueTNCNTheoLuyTien - itemAdd.TongNguoiNLD
                - (itemAdd.TongGiamThueTNCN ?? 0) - (itemAdd.TongGiamThueTNCN ?? 0);
            results.Add(itemAdd);
        }

        return results;
    }

    private static void ResolveShift(
        HRM_NhanVien item,
        HRM_Luong_NhanVien luong,
        HRM_HopDong hopDong,
        List<HRM_ThoiGianLamViecKeHoach> empShifts,
        PhtLookupContext ctx,
        HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        HRM_dmLichLamViec caLamViec = null;

        if (hopDong != null)
        {
            if (!string.IsNullOrEmpty(hopDong.IdLoaiHopDong) && ctx.LoaiHopDongById.TryGetValue(hopDong.IdLoaiHopDong, out var loaiHopDong))
            {
                itemAdd.IdLoaiHopDong = hopDong.IdLoaiHopDong;
                itemAdd.TenLoaiHopDong = loaiHopDong.Ten;
            }

            if (!string.IsNullOrEmpty(hopDong.IdThoiGianLamViec))
                ctx.CaLamViecById.TryGetValue(hopDong.IdThoiGianLamViec, out caLamViec);

            if (caLamViec == null && !string.IsNullOrEmpty(item.IdCaLamViec))
                caLamViec = ctx.CaLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));

            if (caLamViec == null)
            {
                string idCa = empShifts
                    .Where(a => a.Loai == 0 && !string.IsNullOrEmpty(a.IddmCaLamViec)
                        && a.Ngay >= luong.NgayBatDauHieuLuc
                        && (luong.NgayHetHieuLuc == null || (luong.NgayHetHieuLuc.Value.Year > 2000 && a.Ngay <= luong.NgayHetHieuLuc)))
                    .Select(a => a.IddmCaLamViec)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(idCa))
                    ctx.CaLamViecById.TryGetValue(idCa, out caLamViec);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(item.IdCaLamViec))
                caLamViec = ctx.CaLamViecs.FirstOrDefault(a => item.IdCaLamViec.Contains(a.IdGuid));
            else
            {
                string idCa = empShifts
                    .Where(a => a.Loai == 0 && !string.IsNullOrEmpty(a.IddmCaLamViec))
                    .Select(a => a.IddmCaLamViec)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(idCa))
                    ctx.CaLamViecById.TryGetValue(idCa, out caLamViec);
            }
        }

        caLamViec ??= new HRM_dmLichLamViec { SoGioTrongNgay = 8, SoNgayCongTieuChuan = 26 };
        itemAdd.NgayCongTieuChuan = caLamViec.SoNgayCongTieuChuan ?? 1;
        itemAdd.CaLamViec = caLamViec.Ten ?? "";
        itemAdd.SoGioMotNgay = caLamViec.SoGioTrongNgay ?? 0;

        if (itemAdd.LoaiLuong == 3)
        {
            itemAdd.SoGioMotNgay = 1;
            itemAdd.NgayCongTieuChuan = 1;
        }
    }

    private static void PopulateEmployeeProfile(HRM_NhanVien item, PhtLookupContext ctx, HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        itemAdd.IdNhanVien = item.IdGuid;
        itemAdd.TenNhanVien = item.TenDayDu;
        itemAdd.MaNhanVien = item.MaNhanVien;
        if (item.NgayNghiViec != null && item.NgayNghiViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            itemAdd.NgayNghiViec = item.NgayNghiViec.Value.ToString("dd/MM/yyyy");
        if (item.NgayBatDauLamViec != null && item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy") != "01/01/1970")
            itemAdd.NgayVaoLam = item.NgayBatDauLamViec.Value.ToString("dd/MM/yyyy");

        var coSoNames = ctx.QtBpByEmp[item.IdGuid]
            .Select(id => ctx.DuAnById.TryGetValue(id, out var da) ? da.TenBoPhan : null)
            .Where(name => name != null)
            .ToList();
        itemAdd.MaCoSo = string.Join(",", coSoNames);

        if (ctx.BoPhanById.TryGetValue(item.IdBoPhanEOS, out var boPhan))
        {
            itemAdd.MaBoPhan = boPhan.MaBoPhan;
            itemAdd.TenBoPhan = boPhan.TenBoPhan;
        }
        if (ctx.ChucDanhById.TryGetValue(item.IdChucDanh, out var chucDanh))
            itemAdd.TenChucDanh = chucDanh.Ten;
        if (ctx.ChucVuById.TryGetValue(item.IdChucVu, out var chucVu))
            itemAdd.TenChucVu = chucVu.TenChucVu;
    }

    private static void CalculateAllowances(
        HRM_NhanVien item,
        HRM_Luong_NhanVien luong,
        List<HRM_BangChamCong_NhanVien_ThoiGian> datas,
        PhtLookupContext ctx,
        HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        var phuCaps = ctx.PhuCapByEmpLuong[(item.IdGuid, luong.IdGuid)].ToList();
        Dictionary<string, HRM_Luong_PhuCap_NhanVien> phuCapByType = null;
        if (phuCaps.Count > 0)
        {
            phuCapByType = phuCaps
                .Where(a => !string.IsNullOrEmpty(a.IddmLoaiPhuCap))
                .GroupBy(a => a.IddmLoaiPhuCap)
                .ToDictionary(g => g.Key, g => g.First());
        }

        foreach (var phucap in ctx.PhuCapDefs)
        {
            var itemPC = new HRM_QuyTrinhTinhLuong_PhuCap { IdNhanVien = item.IdGuid };
            if (phuCapByType != null && phuCapByType.TryGetValue(phucap.IdGuid, out var pc))
            {
                double ngayCong = 0;
                double ngayCongCaGay = 0;
                foreach (var d in datas)
                {
                    if (d.NgayChamCong < pc.NgayBatDauHieuLuc) continue;
                    if (itemAdd.LoaiLuong == 3)
                    {
                        ngayCong += d.SoGioLamViec ?? 0;
                        ngayCongCaGay += d.SoGioLamViecCaGay ?? 0;
                    }
                    else
                    {
                        double std = d.SoGioTieuChuan ?? 8;
                        ngayCong += (d.SoGioLamViec ?? 0) / std;
                        ngayCongCaGay += (d.SoGioLamViecCaGay ?? 0) / std;
                    }
                }

                double tongSoCong = ngayCong + ngayCongCaGay;
                itemPC.Ma = phucap.Ma;
                itemPC.IdLoaiThoiGian = pc.LoaiPhuCap;
                itemPC.IddmPhuCap = pc.IddmLoaiPhuCap;
                itemPC.SoTienPhuCap = pc.SoTien;

                double phuCapAmt = itemAdd.NgayCongTieuChuan > 0
                    ? (tongSoCong / itemAdd.NgayCongTieuChuan) * pc.SoTien
                    : 0;

                if (phucap.Ma == "PCK-KCT")
                {
                    itemAdd.PhuCapKhongTheoCongChinh += pc.SoTien;
                }
                else if (phucap.Ma == "PC.CG" && pc.LoaiPhuCap != 5)
                {
                    phuCapAmt = (ngayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                    itemAdd.PhuCapCongChinh += phuCapAmt;
                }
                else if (pc.LoaiPhuCap == 5)
                {
                    if (phucap.Ma == "PC.NN")
                    {
                        if (tongSoCong >= 15)
                            phuCapAmt = pc.SoTien;
                        itemAdd.PhuCapCongChinh += phuCapAmt;
                    }
                    else if (phucap.Ma == "PC.CG")
                    {
                        phuCapAmt = itemAdd.NgayCongCaGay >= 15
                            ? pc.SoTien
                            : (ngayCongCaGay / itemAdd.NgayCongTieuChuan) * pc.SoTien;
                        itemAdd.PhuCapCongChinh += phuCapAmt;
                    }
                }
                else
                {
                    if (tongSoCong >= itemAdd.NgayCongTieuChuan || pc.LoaiPhuCap == 1)
                        phuCapAmt = pc.SoTien;
                    itemAdd.PhuCapCongChinh += phuCapAmt;
                }
            }

            itemAdd.listPhuCap.Add(itemPC);
        }
    }

    private static void CalculateHolidays(
        HRM_NhanVien item,
        HRM_Luong_NhanVien luong,
        List<HRM_BangChamCong_NhanVien_ThoiGian> datas,
        List<HRM_ThoiGianLamViecKeHoach> empShifts,
        PhtLookupContext ctx,
        HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        if (!ctx.LichNghiIdByBoPhan.TryGetValue(item.IdBoPhanEOS, out var idLichNghiLe))
            return;

        var nghiLeChiTiets = ctx.LichNghiChiTietById[idLichNghiLe].Distinct().ToList();
        if (nghiLeChiTiets.Count == 0)
            return;

        var holidayDays = new HashSet<int>();
        foreach (var ngay in nghiLeChiTiets)
        {
            for (DateTime t = ngay.TuNgay; t <= ngay.DenNgay; t = t.AddDays(1))
            {
                if (t >= ctx.NgayBD && t <= ctx.NgayKT)
                    holidayDays.Add(t.Day);
            }
        }

        var workedHolidayData = datas
            .Where(a => (a.SoGioLamViec > 0 || a.SoGioLamViecCaGay > 0 || a.SoGioLamViecTangCa > 0)
                && a.NgayChamCong.HasValue && holidayDays.Contains(a.NgayChamCong.Value.Day))
            .ToList();

        if (workedHolidayData.Count > 0)
        {
            itemAdd.Le_Tet = 0;
            itemAdd.Le_TetKDL = 0;
            itemAdd.LuongLe = 0;
            itemAdd.LuongLeKDL = 0;

            var workedByDay = workedHolidayData
                .GroupBy(a => a.NgayChamCong.Value.Day)
                .ToDictionary(g => g.Key, g => g.First());

            var workedDaysSet = new HashSet<int>(workedByDay.Keys);
            bool hasPlannedShiftOnWorkedDays = empShifts.Any(x => workedDaysSet.Contains(x.Ngay.Day));

            foreach (var ngay in nghiLeChiTiets)
            {
                if (ngay.TyLeLuong <= 1) continue;

                for (DateTime t = ngay.TuNgay; t <= ngay.DenNgay; t = t.AddDays(1))
                {
                    double ngayCongRate = luong.LoaiLuong == 3
                        ? luong.Luong
                        : luong.Luong / itemAdd.NgayCongTieuChuan;

                    if (workedByDay.TryGetValue(t.Day, out var ngayC))
                    {
                        double std = ngayC.SoGioTieuChuan ?? 8;
                        double soGLV = (ngayC.SoGioLamViec ?? 0) + (ngayC.SoGioLamViecCaGay ?? 0) + (ngayC.SoGioLamViecTangCa ?? 0);
                        double tongCongN = soGLV / std;
                        double tyLe = ngay.TyLeLuong / 100;

                        if (itemAdd.LoaiLuong == 3)
                        {
                            itemAdd.Le_Tet += soGLV;
                            itemAdd.LuongLe += soGLV * itemAdd.LuongCoBan * 2;
                        }
                        else
                        {
                            itemAdd.Le_Tet += tongCongN;
                            itemAdd.LuongLe += ngayCongRate * tongCongN * tyLe;
                            if (tongCongN < 1)
                            {
                                itemAdd.Le_TetKDL += 1 - tongCongN;
                                itemAdd.LuongLeKDL += ngayCongRate * (1 - tongCongN);
                            }
                        }

                        itemAdd.NgayCong -= (ngayC.SoGioLamViec ?? 0) / std;
                        itemAdd.NgayCongCaGay -= (ngayC.SoGioLamViecCaGay ?? 0) / std;
                        itemAdd.NgayTangCa -= (ngayC.SoGioLamViecTangCa ?? 0) / std;
                        ngayC.isNgayLe = true;
                    }
                    else if (hasPlannedShiftOnWorkedDays && itemAdd.LoaiLuong != 3)
                    {
                        itemAdd.Le_TetKDL += 1;
                        itemAdd.LuongLeKDL += ngayCongRate;
                    }
                }
            }
        }
        else if (itemAdd.LoaiLuong != 3)
        {
            int distinctPlannedHolidayDays = empShifts
                .Where(x => holidayDays.Contains(x.Ngay.Day))
                .Select(a => a.Ngay)
                .Distinct()
                .Count();

            if (distinctPlannedHolidayDays > 0)
            {
                double ngayCongRate = luong.Luong / itemAdd.NgayCongTieuChuan;
                itemAdd.Le_TetKDL += distinctPlannedHolidayDays;
                itemAdd.LuongLeKDL += distinctPlannedHolidayDays * ngayCongRate;
            }
        }
    }

    private static void CalculateInsurance(
        List<HRM_Luong_NhanVien> listLuongNV,
        List<HRM_BangChamCong_NhanVien_ThoiGian> listdatas,
        HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        var eligible = listLuongNV.Where(a => a.LoaiLuong == 1 && a.LuongHachToan > 0).ToList();
        DateTime tuNgay = eligible.OrderBy(a => a.NgayBatDauHieuLuc).Select(a => a.NgayBatDauHieuLuc).FirstOrDefault();
        DateTime? denNgay = eligible.OrderByDescending(a => a.NgayBatDauHieuLuc).Select(a => a.NgayHetHieuLuc).FirstOrDefault();

        double totalHours = listdatas
            .Where(a => a.NgayChamCong >= tuNgay && (denNgay == null || (denNgay.Value.Year > 2000 && denNgay >= a.NgayChamCong)))
            .Sum(a => (a.SoGioLamViec ?? 0) + (a.SoGioLamViecCaGay ?? 0) + (a.SoGioLamViecTangCa ?? 0));

        double divisor = itemAdd.SoGioMotNgay == 1 ? 8 : itemAdd.SoGioMotNgay;
        if (divisor > 0 && (totalHours / divisor) > 14)
        {
            double baseSalary = itemAdd.LuongDongBHXH;
            itemAdd.MucDongBaoHiemXaHoi = baseSalary;
            itemAdd.SDLDBHXH = baseSalary * 17.5 / 100;
            itemAdd.SDLDBHYT = baseSalary * 3 / 100;
            itemAdd.SDLDBHTN = baseSalary * 1 / 100;
            itemAdd.TongNguoiSDLD = itemAdd.SDLDBHXH + itemAdd.SDLDBHYT + itemAdd.SDLDBHTN;
            itemAdd.NLDBHXH = baseSalary * 8 / 100;
            itemAdd.NLDBHYT = baseSalary * 1.5 / 100;
            itemAdd.NLDBHTN = baseSalary * 1 / 100;
            itemAdd.TongNguoiNLD = itemAdd.NLDBHXH + itemAdd.NLDBHYT + itemAdd.NLDBHTN;
        }
    }

    private static void CalculateTaxAndAssets(
        HRM_NhanVien item,
        List<HRM_QuyTrinhTinhLuong_NhanVien> listLuongCheck,
        PhtLookupContext ctx,
        HRM_QuyTrinhTinhLuong_NhanVien itemAdd)
    {
        ctx.DependentCountByEmp.TryGetValue(item.IdGuid, out int dependentCount);
        itemAdd.SoGiamTruBanThan = 11000000;
        itemAdd.SpNguoiDangKyNPT = dependentCount;
        itemAdd.TongTienGiamTruGiaCanh = itemAdd.SoGiamTruBanThan + itemAdd.SpNguoiDangKyNPT * 4400000;
        itemAdd.ThuNhapMienThue = listLuongCheck.Sum(a => a.LuongLe + a.LuongLeKDL + a.TongNguoiNLD);

        double totalGross = listLuongCheck.Sum(a => a.TongLuong);
        double taxable = totalGross - itemAdd.TongTienGiamTruGiaCanh - itemAdd.ThuNhapMienThue;
        itemAdd.ThuNhapTinhThueTNCN = taxable > 0 ? taxable : 0;
        itemAdd.ThueTNCNTheoLuyTien = ctx.CalculateTax(itemAdd.ThuNhapTinhThueTNCN, ctx.TaxBrackets);

        var assets = ctx.CapPhatByEmp[item.IdGuid].Where(a => a.Loai == 1).ToList();
        if (assets.Count > 0)
        {
            itemAdd.DongPhuc = assets.Sum(a => (a.SoLuongCapPhat ?? 0) * (a.GiaTriCapPhat ?? 0));
            itemAdd.TraTienDongPhuc = assets.Sum(a => (a.SoLuongThuHoi ?? 0) * (a.GiaTriThuHoi ?? 0));
        }

        itemAdd.KhauTruTaiNguon = 0;
        itemAdd.isCamKet = item.isCamKet;
        if (listLuongCheck.Sum(a => a.MucDongBaoHiemXaHoi) == 0 && item.isCamKet == false)
            itemAdd.KhauTruTaiNguon = itemAdd.TongLuong * 0.1;

        itemAdd.TongGiamThueTNCN = itemAdd.ThueTNCNTheoLuyTien + itemAdd.KhauTruTaiNguon;
        if (itemAdd.TongGiamThueTNCN < 50000)
            itemAdd.TongGiamThueTNCN = 0;
    }
}
