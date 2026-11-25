#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Common;
using HRM.Api.Models;

namespace HRM.Api.Services.Attendance;

/// <summary>
/// Pre-indexed read-only lookup structures for attendance calculation loops,
/// reducing O(N * D * M) linear scans to O(1) dictionary/lookup access.
/// </summary>
public sealed class AttendanceLookupContext
{
    public ILookup<(string EmpId, DateTime Date), HRM_ThoiGianLamViecKeHoach> LichLamViecByEmpDate { get; }
    public ILookup<string, HRM_ThoiGianLamViecKeHoach> LichLamViecByEmp { get; }
    public ILookup<(string EmpId, DateTime Date), HRM_CheckInOut> CheckInOutByEmpDate { get; }
    public ILookup<(string EmpId, DateTime Date), HRM_TheoDoiXinNghiPhep> NghiPhepByEmpDate { get; }
    public ILookup<(string EmpId, DateTime Date), HRM_TheoDoiDiMuonVeSom> DiMuonVeSomByEmpDate { get; }
    public Dictionary<string, HRM_dmLichLamViec> CaLamViecById { get; }
    public Dictionary<double, HRM_dmLichLamViec> CaLamViecBySoGio { get; }
    public Dictionary<long, BoPhan> BoPhanById { get; }
    public Dictionary<long, ChucVu> ChucVuById { get; }

    public AttendanceLookupContext(
        IEnumerable<HRM_ThoiGianLamViecKeHoach> listLichLamViec,
        IEnumerable<HRM_CheckInOut> listCheckInOut,
        IEnumerable<HRM_TheoDoiXinNghiPhep> listXinNghiPhep,
        IEnumerable<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom,
        IEnumerable<HRM_dmLichLamViec> listCaLamViec,
        IEnumerable<BoPhan> listBoPhan = null,
        IEnumerable<ChucVu> listChucVu = null)
    {
        var lichList = listLichLamViec ?? Enumerable.Empty<HRM_ThoiGianLamViecKeHoach>();
        LichLamViecByEmpDate = lichList.ToLookup(x => (x.IdNhanVien, x.Ngay.Date));
        LichLamViecByEmp = lichList.ToLookup(x => x.IdNhanVien);

        CheckInOutByEmpDate = (listCheckInOut ?? Enumerable.Empty<HRM_CheckInOut>())
            .ToLookup(x => (x.IdNhanVien, x.Ngay.Date));

        NghiPhepByEmpDate = (listXinNghiPhep ?? Enumerable.Empty<HRM_TheoDoiXinNghiPhep>())
            .ToLookup(x => (x.IdNhanVien, x.NgayNghi.Date));

        DiMuonVeSomByEmpDate = (listDiMuonVeSom ?? Enumerable.Empty<HRM_TheoDoiDiMuonVeSom>())
            .Where(x => x.NgayDangKy.HasValue)
            .ToLookup(x => (x.IdNhanVien, x.NgayDangKy.Value.Date));

        var caList = (listCaLamViec ?? Enumerable.Empty<HRM_dmLichLamViec>()).ToList();
        CaLamViecById = caList
            .Where(a => !string.IsNullOrEmpty(a.IdGuid))
            .GroupBy(a => a.IdGuid)
            .ToDictionary(g => g.Key, g => g.First());
        CaLamViecBySoGio = caList
            .Where(a => a.SoGioTrongNgay.HasValue)
            .GroupBy(a => a.SoGioTrongNgay.Value)
            .ToDictionary(g => g.Key, g => g.First());

        BoPhanById = (listBoPhan ?? Enumerable.Empty<BoPhan>())
            .GroupBy(a => a.Id)
            .ToDictionary(g => g.Key, g => g.First());

        ChucVuById = (listChucVu ?? Enumerable.Empty<ChucVu>())
            .GroupBy(a => a.Id)
            .ToDictionary(g => g.Key, g => g.First());
    }

    public List<HRM_CheckInOut> GetCheckInOut(string empId, DateTime dt, DateTime? checkDate)
    {
        var day1 = CheckInOutByEmpDate[(empId, dt.Date)];
        if (checkDate.HasValue && checkDate.Value.Date != dt.Date)
        {
            return day1.Concat(CheckInOutByEmpDate[(empId, checkDate.Value.Date)])
                .OrderBy(a => a.Gio)
                .ToList();
        }
        return day1.OrderBy(a => a.Gio).ToList();
    }
}

/// <summary>
/// Pure, thread-safe attendance late/early/mid-shift calculation engine shared across
/// FuncKeHoachLamViec, FuncChamCong, and FuncDieuHanhNhanSu.
/// </summary>
public static class AttendanceTimeCalculator
{
    private static readonly string CodeIn = DefineData.eKyHieuChamCong.I.ToString();
    private static readonly string CodeOut = DefineData.eKyHieuChamCong.O.ToString();
    private static readonly string CodeCT = DefineData.eKyHieuChamCong.CT.ToString();
    private static readonly string CodeNTD = DefineData.eKyHieuChamCong.NTD.ToString();
    private static readonly string CodeM = DefineData.eKyHieuChamCong.M.ToString();
    private static readonly string CodeS = DefineData.eKyHieuChamCong.S.ToString();
    private static readonly string CodeG = DefineData.eKyHieuChamCong.G.ToString();

    public static List<clsDiMuonVeSom> TinhSomMuon(
        string host,
        string IdNhanVien,
        List<HRM_dmKyHieuChamCong> dmKyHieuChamCongs,
        List<HRM_TheoDoiDiMuonVeSom> listDiMuonVeSom,
        List<HRM_ThoiGianLamViecKeHoach> listThoiGianLamViec,
        List<HRM_CheckInOut> listCheckInOut,
        DateTime Ngay,
        double SoPhutDuocDiMuon,
        double SoPhutDuocVeSom,
        string MaCongTy,
        List<HRM_TheoDoiXinNghiPhep> listNghiPhep = null,
        bool XacNhan = false,
        long IdDuAn = 0,
        HRM_dmLichLamViec Ca = null)
    {
        Ngay = Ngay.Date;
        var sortedShifts = listThoiGianLamViec.OrderBy(x => x.Ngay).ThenBy(x => x.GioVao).ToList();
        var sortedLogs = listCheckInOut.OrderBy(x => x.Gio).ToList();
        var listOut = new List<clsDiMuonVeSom>();

        var listInOutCD = BuildActualInOutIntervals(sortedLogs, SoPhutDuocDiMuon, SoPhutDuocVeSom, out double soGioLamViecTT);
        if (listInOutCD.Count == 0 || sortedLogs.Count == 1)
            return listOut;

        var listInOutNgay = MatchShiftsWithActualIntervals(sortedShifts, listInOutCD, listNghiPhep, Ca, out double soGioLamViec);

        if (soGioLamViecTT >= soGioLamViec && MaCongTy == "VPCT" && host == "PHT")
            return listOut;

        sortedShifts = sortedShifts.OrderBy(a => a.GioVao).ToList();

        // Build O(1) symbol lookup
        var kyHieuByMa = new Dictionary<string, HRM_dmKyHieuChamCong>(StringComparer.Ordinal);
        if (dmKyHieuChamCongs != null)
        {
            foreach (var kh in dmKyHieuChamCongs)
            {
                if (!string.IsNullOrEmpty(kh.Ma) && !kyHieuByMa.ContainsKey(kh.Ma))
                    kyHieuByMa[kh.Ma] = kh;
            }
        }

        int soPhutNghi = CalculateLeaveOffsetMinutes(listNghiPhep, listInOutNgay, Ngay, soGioLamViec, out double heSo, out string maHinhThucNghi);

        int soPhutDiMuon = 0;
        int soPhutVeSom = 0;
        if (listDiMuonVeSom != null && listDiMuonVeSom.Count > 0)
        {
            double gioMuon = 0;
            double gioSom = 0;
            foreach (var d in listDiMuonVeSom)
            {
                if (d.IdNhanVien != IdNhanVien || d.NgayDangKy != Ngay) continue;
                if (d.IdLoai == 1) gioMuon += d.TongGio;
                else if (d.IdLoai == 2) gioSom += d.TongGio;
            }
            soPhutDiMuon = (int)Math.Round(gioMuon * 60, 0);
            soPhutVeSom = (int)Math.Round(gioSom * 60, 0);
        }

        if (soPhutNghi > 0 && !XacNhan)
        {
            var itemOut = new clsDiMuonVeSom();
            if (!string.IsNullOrEmpty(maHinhThucNghi) && kyHieuByMa.TryGetValue(maHinhThucNghi, out var nghiPhepKh))
            {
                itemOut.IddmKyHieuChamCong = nghiPhepKh.IdGuid;
                itemOut.KyHieuChamCong = heSo < 1 ? nghiPhepKh.Ma + "/2" : nghiPhepKh.Ma;
            }
            else
            {
                itemOut.KyHieuChamCong = maHinhThucNghi;
            }
            itemOut.SoPhut = soPhutNghi;
            listOut.Add(itemOut);
        }

        kyHieuByMa.TryGetValue(CodeCT, out var khCT);
        kyHieuByMa.TryGetValue(CodeNTD, out var khNTD);
        kyHieuByMa.TryGetValue(CodeM, out var khM);
        kyHieuByMa.TryGetValue(CodeS, out var khS);
        kyHieuByMa.TryGetValue(CodeG, out var khG);
        HRM_dmKyHieuChamCong khNghiPhep = !string.IsNullOrEmpty(maHinhThucNghi) && kyHieuByMa.TryGetValue(maHinhThucNghi, out var tmpNp) ? tmpNp : null;

        bool hasCT = false;
        foreach (var item in sortedShifts)
        {
            if (item.isCongTac)
            {
                if (hasCT) continue;
                if (khCT != null)
                {
                    listOut.Add(new clsDiMuonVeSom
                    {
                        IddmKyHieuChamCong = khCT.IdGuid,
                        KyHieuChamCong = khCT.Ma,
                        SoPhut = 0
                    });
                    hasCT = true;
                }
                continue;
            }

            if (item.GioVaoTT == null && item.GioRaTT == null)
            {
                var itemOut = new clsDiMuonVeSom(item)
                {
                    SoPhut = item.TongSoGioGiao == 0
                        ? (int)(item.GioRa - item.GioVao).TotalMinutes
                        : (int)(item.TongSoGioGiao * 60),
                    isCaGay = item.isCaGay,
                    Loai = item.Loai
                };

                if (soPhutNghi > 0)
                {
                    if (!XacNhan)
                    {
                        if (khNghiPhep != null)
                        {
                            itemOut.IddmKyHieuChamCong = khNghiPhep.IdGuid;
                            itemOut.KyHieuChamCong = khNghiPhep.Ma;
                        }
                        listOut.Add(itemOut);
                    }

                    if (soPhutNghi >= itemOut.SoPhut)
                        soPhutNghi -= itemOut.SoPhut;
                    else
                        itemOut.SoPhut -= soPhutNghi;
                    continue;
                }

                if (khNTD != null)
                {
                    itemOut.IddmKyHieuChamCong = khNTD.IdGuid;
                    itemOut.KyHieuChamCong = khNTD.Ma;
                }
                listOut.Add(itemOut);
                continue;
            }

            item.GioVaoTT ??= item.GioVao;
            item.GioRaTT ??= item.GioRa;

            // Late arrival (M)
            double soGiay = (item.GioVaoTT.Value - item.GioVao).TotalSeconds;
            if (soGiay > 0)
            {
                int mins = (int)(soGiay / 60);
                if (mins == 0) continue;

                var itemOut = new clsDiMuonVeSom(item)
                {
                    GioVao = item.GioVao,
                    isCaGay = item.isCaGay,
                    Loai = item.Loai,
                    SoPhut = mins
                };

                if (khM != null)
                {
                    itemOut.IddmKyHieuChamCong = khM.IdGuid;
                    itemOut.KyHieuChamCong = khM.Ma;
                    if (item.Loai == 0)
                        itemOut.SoPhut = DeductMinutes(itemOut.SoPhut, ref soPhutNghi);
                    itemOut.SoPhut = DeductMinutes(itemOut.SoPhut, ref soPhutDiMuon);
                }

                if (itemOut.SoPhut != 0)
                    listOut.Add(itemOut);
            }

            // Early departure (S)
            soGiay = (item.GioRa - item.GioRaTT.Value).TotalSeconds;
            if (soGiay > 0)
            {
                int mins = (int)(soGiay / 60);
                if (mins == 0) continue;

                var itemOut = new clsDiMuonVeSom(item)
                {
                    isCaGay = item.isCaGay,
                    Loai = item.Loai,
                    SoPhut = mins
                };

                if (khS != null)
                {
                    itemOut.IddmKyHieuChamCong = khS.IdGuid;
                    itemOut.KyHieuChamCong = khS.Ma;
                    if (item.Loai == 0)
                        itemOut.SoPhut = DeductMinutes(itemOut.SoPhut, ref soPhutNghi);
                    itemOut.SoPhut = DeductMinutes(itemOut.SoPhut, ref soPhutVeSom);
                }

                if (itemOut.SoPhut != 0)
                    listOut.Add(itemOut);
            }

            // Mid-shift exit (G)
            if (item.GioVaoTiep < item.GioRa)
            {
                foreach (var itemInOut in listInOutCD)
                {
                    if (itemInOut.GioVao > item.GioRa || itemInOut.GioRa < item.GioVao) continue;
                    if (itemInOut.GioVaoTiep == null || itemInOut.GioVaoTiep >= item.GioRa) continue;
                    if (itemInOut.GioRa >= item.GioVao && itemInOut.GioRa <= item.GioRa)
                    {
                        soGiay = (itemInOut.GioVaoTiep.Value - itemInOut.GioRa).TotalSeconds;
                        if (soGiay > 0)
                        {
                            var itemOut = new clsDiMuonVeSom(item)
                            {
                                isCaGay = item.isCaGay,
                                Loai = item.Loai,
                                SoPhut = (int)(soGiay / 60)
                            };
                            if (khG != null)
                            {
                                itemOut.IddmKyHieuChamCong = khG.IdGuid;
                                itemOut.KyHieuChamCong = khG.Ma;
                                var checkXinPhep = listDiMuonVeSom?.FirstOrDefault(a =>
                                    a.IdLoai == 3 && a.IdNhanVien == item.IdNhanVien && a.NgayDangKy == item.Ngay);
                                if (checkXinPhep != null)
                                {
                                    itemOut.SoPhut = Math.Max(0, itemOut.SoPhut - (int)(checkXinPhep.TongGio * 60));
                                }
                            }
                            if (itemOut.SoPhut != 0)
                                listOut.Add(itemOut);
                        }
                    }
                }
            }
        }

        return listOut;
    }

    private static List<HRM_ThoiGianLamViecKeHoach> BuildActualInOutIntervals(
        List<HRM_CheckInOut> sortedLogs,
        double soPhutDuocDiMuon,
        double soPhutDuocVeSom,
        out double soGioLamViecTT)
    {
        var listInOutCD = new List<HRM_ThoiGianLamViecKeHoach>();
        soGioLamViecTT = 0;
        int nCount = sortedLogs.Count;

        for (int i = 0; i < nCount; i++)
        {
            var itemIn = sortedLogs[i];
            if (itemIn.Loai == CodeOut) continue;

            var inOutCD = new HRM_ThoiGianLamViecKeHoach
            {
                IdCongTy = itemIn.IdCongTy,
                GioVao = itemIn.Gio.AddMinutes(-soPhutDuocDiMuon)
            };

            if (nCount > i + 1)
            {
                HRM_CheckInOut firstOut = null;
                for (int j = 0; j < nCount; j++)
                {
                    if (sortedLogs[j].Loai == CodeOut && sortedLogs[j].Gio > inOutCD.GioVao)
                    {
                        firstOut = sortedLogs[j];
                        break;
                    }
                }
                if (firstOut == null) continue;
                inOutCD.GioRa = firstOut.Gio.AddMinutes(soPhutDuocVeSom);
            }

            if (i + 2 < nCount && inOutCD.GioRa.Year > 2024)
            {
                HRM_CheckInOut nextIn = null;
                for (int j = 0; j < nCount; j++)
                {
                    if (sortedLogs[j].Loai == CodeIn && sortedLogs[j].Gio > inOutCD.GioRa)
                    {
                        nextIn = sortedLogs[j];
                        break;
                    }
                }

                if (nextIn != null)
                {
                    inOutCD.GioVaoTiep = nextIn.Gio.AddMinutes(-soPhutDuocDiMuon);
                    for (int j = nCount - 1; j >= 0; j--)
                    {
                        if (sortedLogs[j].Loai == CodeOut && sortedLogs[j].Gio > inOutCD.GioVao && sortedLogs[j].Gio <= inOutCD.GioVaoTiep)
                        {
                            inOutCD.GioRa = sortedLogs[j].Gio.AddMinutes(soPhutDuocVeSom);
                            break;
                        }
                    }
                }
                else
                {
                    for (int j = nCount - 1; j >= 0; j--)
                    {
                        if (sortedLogs[j].Loai == CodeOut && sortedLogs[j].Gio > inOutCD.GioVao)
                        {
                            inOutCD.GioRa = sortedLogs[j].Gio.AddMinutes(soPhutDuocVeSom);
                            break;
                        }
                    }
                }
            }

            soGioLamViecTT += (inOutCD.GioRa - inOutCD.GioVao).TotalMinutes;
            listInOutCD.Add(inOutCD);
        }

        if (listInOutCD.Count > 1)
            listInOutCD.Sort((a, b) => a.GioVao.CompareTo(b.GioVao));

        return listInOutCD;
    }

    private static List<HRM_ThoiGianLamViecKeHoach> MatchShiftsWithActualIntervals(
        List<HRM_ThoiGianLamViecKeHoach> sortedShifts,
        List<HRM_ThoiGianLamViecKeHoach> listInOutCD,
        List<HRM_TheoDoiXinNghiPhep> listNghiPhep,
        HRM_dmLichLamViec ca,
        out double soGioLamViec)
    {
        soGioLamViec = 0;
        var listInOutNgay = new List<HRM_ThoiGianLamViecKeHoach>();
        bool hasLeave = listNghiPhep != null && listNghiPhep.Count > 0;

        foreach (var item in sortedShifts)
        {
            if (hasLeave)
                item.SoGioNghi = 0;

            if (item.Loai == 0)
            {
                if (item.TongSoGioGiao == 0)
                    soGioLamViec += (item.GioRa - item.GioVao).TotalMinutes - (item.SoGioNghi ?? 0) * 60;
                else
                    soGioLamViec += item.TongSoGioGiao * 60 - (item.SoGioNghi ?? 0) * 60;

                if (item.GioRaTT.HasValue && item.GioVaoTT.HasValue)
                {
                    double gioCheck = (item.GioRa - item.GioVao).TotalMinutes / 60;
                    if (gioCheck != 0)
                    {
                        double gioCheckTT = (item.GioRaTT.Value - item.GioVaoTT.Value).TotalMinutes / 60;
                        if (gioCheckTT / gioCheck <= 0.5)
                            soGioLamViec += (item.SoGioNghi ?? 0) * 60;
                    }
                }
            }

            bool matchedAny = false;
            foreach (var itemInOut in listInOutCD)
            {
                if (!(itemInOut.GioVao < item.GioRa && itemInOut.GioRa > item.GioVao))
                    continue;
                matchedAny = true;

                if (itemInOut.GioRa < item.GioVao) continue;
                if (itemInOut.GioVao > item.GioRa && item.GioVao > item.GioRa) continue;

                if (item.GioVaoTT == null || item.GioVaoTT > itemInOut.GioVao)
                {
                    if (item.GioVao >= itemInOut.GioVao && item.GioVao < itemInOut.GioRa)
                        item.GioVaoTT = item.GioVao;
                    else if (item.GioVao <= itemInOut.GioVao && item.GioRa > itemInOut.GioVao)
                        item.GioVaoTT = itemInOut.GioVao;
                }

                if (item.GioRaTT == null || item.GioRaTT <= itemInOut.GioRa)
                    item.GioRaTT = itemInOut.GioRa;

                if (itemInOut.GioVaoTiep != null)
                    item.GioVaoTiep = itemInOut.GioVaoTiep;
            }

            if (matchedAny && item.GioRaTT.HasValue && item.GioVaoTT.HasValue)
            {
                DateTime gioVao = item.GioVaoTT.Value < item.GioVao ? item.GioVao : item.GioVaoTT.Value;
                DateTime gioRa = item.GioRaTT.Value > item.GioRa ? item.GioRa : item.GioRaTT.Value;

                if (ca != null && ca.LoaiNgayNghi == 4 && (int)item.Ngay.DayOfWeek == 5 && gioRa.Hour > 12)
                    gioRa = new DateTime(gioRa.Year, gioRa.Month, gioRa.Day, 12, 0, 0);

                listInOutNgay.Add(new HRM_ThoiGianLamViecKeHoach { GioVao = gioVao, GioRa = gioRa });
            }
        }

        return listInOutNgay;
    }

    private static int CalculateLeaveOffsetMinutes(
        List<HRM_TheoDoiXinNghiPhep> listNghiPhep,
        List<HRM_ThoiGianLamViecKeHoach> listInOutNgay,
        DateTime ngay,
        double soGioLamViec,
        out double heSo,
        out string maHinhThucNghi)
    {
        heSo = 0;
        maHinhThucNghi = string.Empty;
        if (listNghiPhep == null || listNghiPhep.Count == 0)
            return 0;

        heSo = listNghiPhep.Sum(a => a.ThoiGianNghi);
        int soPhutNghi = (int)(heSo * soGioLamViec);
        maHinhThucNghi = listNghiPhep[0].HinhThucNghi;
        string loaiNghi = string.Join(",", listNghiPhep.Select(a => a.MaLoaiNghi).Distinct());

        if (!loaiNghi.Contains("CANGAY"))
        {
            DateTime time12 = ngay.Date.AddHours(12);
            DateTime nextDay = ngay.AddDays(1);
            bool isSang = loaiNghi.Contains("SANG");

            bool anyOverlap = false;
            foreach (var item in listInOutNgay)
            {
                bool matches = isSang
                    ? (item.GioVao <= time12 && item.GioRa >= ngay)
                    : (item.GioVao <= nextDay && item.GioRa >= time12);
                if (!matches) continue;

                anyOverlap = true;
                DateTime gioVe = item.GioRa > time12 ? time12 : item.GioRa;
                soPhutNghi -= (int)(gioVe - item.GioVao).TotalMinutes;
            }

            if (!anyOverlap)
                soPhutNghi = 0;
        }

        return soPhutNghi;
    }

    private static int DeductMinutes(int minutes, ref int allowance)
    {
        if (minutes >= allowance)
        {
            minutes -= allowance;
            allowance = 0;
            return minutes;
        }

        allowance -= minutes;
        return 0;
    }
}
