#nullable disable
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace HRM.Api.Models
{
    #pragma warning disable CS8981
    public partial class dungchung
    {
        public static string m_FormatSoLuong0 = "_(* #,##0_);_(* (#,##0);_(* \"-\"??_);_(@_)";
        public static string m_FormatSoLuong2 = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
        public static string m_FormatSoLuong1 = "_(* #,##0.0_);_(* (#,##0.0);_(* \"-\"??_);_(@_)";
        public static string m_FormatSoLuong3 = "_(* #,##0.000_);_(* (#,##0.000);_(* \"-\"??_);_(@_)";
        public static string m_FormatSoLuong8 = "_(* #,##0.00000000_);_(* (#,##0.00000000);_(* \"-\"??_);_(@_)";
        public static string sDuongDanUpload = "PathUploads";

        public static int SetRichTextFromHtml(ExcelRangeBase range, string html, string defaultFontName, short defaultFontSize)
        {
            int soDong = 1;
            range.Value = string.Empty;

            if (string.IsNullOrEmpty(html))
            {
                range.IsRichText = false;
                return soDong;
            }

            html = Regex.Replace(html, @"<br[^>]*>", "\r\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            OfficeOpenXml.Style.ExcelRichText thisrt = null;
            bool isFirst = true;

            var matches = Regex.Matches(html, @"<(/?(?:color#[0-9A-F]{6}|[a-z]+))[^<>]*>([\s\S]*?)(?=</?[a-z]+[^<>]*>|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            int totalMatches = matches.Count;
            int dem = 1;

            foreach (Match m in matches)
            {
                if (isFirst)
                {
                    range.IsRichText = true;
                    thisrt = range.RichText.Add(CleanText(html[..m.Index]));
                    thisrt.Size = defaultFontSize;
                    thisrt.FontName = defaultFontName;
                    isFirst = false;
                }

                var tag = m.Groups[1].Captures[0].Value;
                var text = CleanText(m.Groups[2].Captures[0].Value);
                string breakRow = "";
                if (tag.Contains("/p", StringComparison.OrdinalIgnoreCase) && dem < totalMatches)
                {
                    breakRow = "\r\n";
                    soDong++;
                }

                if (string.IsNullOrEmpty(thisrt!.Text))
                {
                    thisrt.Text = text + breakRow;
                }
                else
                {
                    thisrt = range.RichText.Add(text + breakRow);
                }

                SetStyleFromTag(tag, thisrt);
                dem++;
            }

            if (thisrt == null)
            {
                int a = 0;
                while ((a = html.IndexOf('\n', a)) != -1)
                {
                    a++;
                    soDong++;
                    if (a > 100 * soDong)
                    {
                        soDong += 2;
                    }
                }
                if (soDong == 1)
                {
                    soDong = html.Length / 100 + 1;
                }
                range.IsRichText = false;
                range.Value = CleanText(html);
            }
            else if (string.IsNullOrEmpty(thisrt.Text))
            {
                range.RichText.Remove(thisrt);
                if (range.RichText.Count == 0)
                {
                    range.IsRichText = false;
                    range.Value = string.Empty;
                }
            }

            return soDong;
        }

        private static void SetStyleFromTag(string tag, OfficeOpenXml.Style.ExcelRichText settings)
        {
            switch (tag.ToLowerInvariant())
            {
                case "ins":
                    settings.Bold = true;
                    settings.Color = System.Drawing.ColorTranslator.FromHtml("#0000FF");
                    break;
                case "del":
                    settings.Strike = true;
                    settings.Color = System.Drawing.ColorTranslator.FromHtml("#CC0000");
                    break;
                case "b":
                case "strong":
                    settings.Bold = true;
                    break;
                case "i":
                case "em":
                    settings.Italic = true;
                    break;
                case "u":
                    settings.UnderLine = true;
                    break;
                case "s":
                case "strike":
                    settings.Strike = true;
                    break;
                case "sup":
                    settings.VerticalAlign = OfficeOpenXml.Style.ExcelVerticalAlignmentFont.Superscript;
                    break;
                case "sub":
                    settings.VerticalAlign = OfficeOpenXml.Style.ExcelVerticalAlignmentFont.Subscript;
                    break;
                case "/b":
                case "/strong":
                    settings.Bold = false;
                    break;
                case "/i":
                case "/em":
                    settings.Italic = false;
                    break;
                case "/u":
                    settings.UnderLine = false;
                    break;
                case "/s":
                case "/strike":
                    settings.Strike = false;
                    break;
                case "/sup":
                case "/sub":
                    settings.VerticalAlign = OfficeOpenXml.Style.ExcelVerticalAlignmentFont.None;
                    break;
                case "/color":
                case "/ins":
                case "/del":
                    if (tag.Equals("/ins", StringComparison.OrdinalIgnoreCase)) settings.Bold = false;
                    if (tag.Equals("/del", StringComparison.OrdinalIgnoreCase)) settings.Strike = false;
                    settings.Color = System.Drawing.Color.FromArgb(0);
                    break;
                case "/p":
                    settings.PreserveSpace = true;
                    break;
                default:
                    if (tag.StartsWith("color", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.Color = System.Drawing.ColorTranslator.FromHtml(tag[5..]);
                    }
                    break;
            }
        }

        private static string CleanText(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = WebUtility.HtmlDecode(s);
            return s.Replace('\u00A0', ' ');
        }

        public static string GetAlphabeticalOrder(int number)
        {
            if (number < 1) return string.Empty;
            string result = string.Empty;
            while (number > 0)
            {
                number--;
                result = (char)('A' + number % 26) + result;
                number /= 26;
            }
            return result;
        }

        public static string QuyDoiLaMa(int num)
        {
            if (num < 1 || num > 3999) return "Invalid number";
            string[] thousands = { "", "M", "MM", "MMM" };
            string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] units = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            return thousands[num / 1000] + hundreds[num % 1000 / 100] + tens[num % 100 / 10] + units[num % 10];
        }

        public static string NumberToText(double inputNumber, bool suffix = true)
        {
            string[] unitNumbers = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] placeValues = { "", "nghìn", "triệu", "tỷ" };
            bool isNegative = false;
            string result = " ";

            string sNumber = inputNumber.ToString("#");
            if (!string.IsNullOrEmpty(sNumber))
            {
                double number = Convert.ToDouble(sNumber);
                if (number < 0)
                {
                    number = -number;
                    sNumber = number.ToString(CultureInfo.InvariantCulture);
                    isNegative = true;
                }

                int positionDigit = sNumber.Length;
                if (positionDigit == 0)
                {
                    result = unitNumbers[0] + result;
                }
                else
                {
                    int placeValue = 0;
                    while (positionDigit > 0)
                    {
                        int tens = -1, hundreds = -1;
                        int ones = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                        positionDigit--;
                        if (positionDigit > 0)
                        {
                            tens = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                            positionDigit--;
                            if (positionDigit > 0)
                            {
                                hundreds = Convert.ToInt32(sNumber.Substring(positionDigit - 1, 1));
                                positionDigit--;
                            }
                        }

                        if (ones > 0 || tens > 0 || hundreds > 0 || placeValue == 3)
                            result = placeValues[placeValue] + result;

                        placeValue++;
                        if (placeValue > 3) placeValue = 1;

                        if (ones == 1 && tens > 1)
                            result = "một " + result;
                        else if (ones == 5 && tens > 0)
                            result = "lăm " + result;
                        else if (ones > 0)
                            result = unitNumbers[ones] + " " + result;

                        if (tens < 0) break;
                        if (tens == 0 && ones > 0) result = "lẻ " + result;
                        if (tens == 1) result = "mười " + result;
                        if (tens > 1) result = unitNumbers[tens] + " mươi " + result;

                        if (hundreds < 0) break;
                        if (hundreds > 0 || tens > 0 || ones > 0)
                            result = unitNumbers[hundreds] + " trăm " + result;

                        result = " " + result;
                    }
                }

                result = result.Trim();
                if (isNegative) result = "Âm " + result;
            }
            return result;
        }

        public static void DrawTableExcel(ExcelWorksheet sheet1, int nRowBegin, int nRowEnd, int nColBegin, int nColEnd)
        {
            var border = sheet1.Cells[nRowBegin, nColBegin, nRowEnd, nColEnd].Style.Border;
            border.Right.Style = border.Left.Style = border.Top.Style = border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        private static string FindDuplicateColumnValues(ExcelWorksheet sheet1, int nRow, int nCol)
        {
            var seen = new HashSet<string>();
            var duplicates = new StringBuilder();
            for (int row = nRow; row <= sheet1.Cells.End.Row; row++)
            {
                string sMa = GetCell(sheet1, row, nCol);
                if (sMa == "") break;
                if (!seen.Add(sMa)) duplicates.Append(sMa).Append("; ");
            }
            return duplicates.ToString();
        }

        public static bool KiemTraMaTrungExcel(ExcelWorksheet sheet1, ErrorMessage err, int nRow = 4, int nCol = 2)
        {
            string sMaTrung = FindDuplicateColumnValues(sheet1, nRow, nCol);
            if (sMaTrung == "") return true;
            err.SetLoiVang("Mã trong tệp excel bị trùng: " + sMaTrung);
            return false;
        }

        public static bool KiemTraSDTTrungExcel(ExcelWorksheet sheet1, ErrorMessage err, int nRow = 4, int nCol = 2)
        {
            string sMaTrung = FindDuplicateColumnValues(sheet1, nRow, nCol);
            if (sMaTrung == "") return true;
            err.SetLoiVang("Số điện thoại trong tệp excel bị trùng: " + sMaTrung);
            return false;
        }

        public static MessageReturn KiemTraMaTrungExcel(ExcelWorksheet sheet1, MessageReturn err, int nRow = 4, int nCol = 2)
        {
            string sMaTrung = FindDuplicateColumnValues(sheet1, nRow, nCol);
            return sMaTrung != ""
                ? new MessageReturn(DefineData.eErrorMessage.LoiLayDuLieu, "Mã trong tệp excel bị trùng: " + sMaTrung)
                : err;
        }

        public static bool KiemTraTempExcel(ExcelWorksheet sheet1, ErrorMessage err, List<string> listColumnName, string tieuDe, int nRow = 3)
        {
            if (sheet1 == null) return false;
            string tieuDeExcel = GetCell(sheet1, 1, 1);
            if (tieuDeExcel.Trim() != tieuDe.Trim())
            {
                err.SetLoiKhac("Tiêu đề sai định dạng: " + tieuDeExcel);
                return false;
            }
            string columnSai = "";
            int totalCol = listColumnName.Count;
            for (int col = 1; col <= totalCol; col++)
            {
                var colName = GetCell(sheet1, nRow, col);
                if (!listColumnName.Contains(colName.Trim()) && colName != "")
                {
                    columnSai += colName + ',';
                }
            }
            if (columnSai != "")
            {
                err.SetLoiKhac("Sai định dạng cột: " + columnSai);
                return false;
            }
            return true;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
            => DateTimeHelper.UnixTimeStampToDateTime(unixTimeStamp);

        public static DateTime? UnixTimeStampToDateTime(double? unixTimeStamp)
            => DateTimeHelper.UnixTimeStampToDateTime(unixTimeStamp);

        public static double DateTimeToUnixTimeStamp(DateTime? time)
            => DateTimeHelper.DateTimeToUnixTimeStamp(time);

        public static DateTime ToDateTime(string input)
            => DateTimeHelper.ToDateTime(input);

        public static dynamic CalculateAge(DateTime birthDate, DateTime currentDate)
        {
            if (birthDate > currentDate)
                throw new ArgumentException("Ngày sinh không được lớn hơn ngày hiện tại");

            int years = currentDate.Year - birthDate.Year;
            if (currentDate < birthDate.AddYears(years)) years--;

            int months = currentDate.Month - birthDate.Month;
            if (currentDate.Day < birthDate.Day) months--;
            if (months < 0) months += 12;

            int days = (currentDate - birthDate.AddYears(years).AddMonths(months)).Days;
            return new { Years = years, Months = months, Days = days };
        }

        public static string Base64Encode(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        public static string Base64Decode(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData)) return string.Empty;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetCellTangCol(ExcelWorksheet sheet, int nRow, ref int nCol)
        {
            string sValue = sheet.Cells[nRow, nCol].Value?.ToString()?.Trim() ?? "";
            nCol++;
            return sValue;
        }

        public static string GetCell(ExcelWorksheet sheet, int nRow, int nCol)
            => sheet.Cells[nRow, nCol].Value?.ToString()?.Trim() ?? "";

        public static float? GetCellToFloat(ExcelWorksheet sheet, int nRow, int nCol)
        {
            string sValue = sheet.Cells[nRow, nCol].Value?.ToString() ?? "";
            if (string.IsNullOrEmpty(sValue)) return 0;
            return float.TryParse(sValue, out float dValue) ? dValue : null;
        }

        public static int GetCellToInt(ExcelWorksheet sheet, int nRow, int nCol)
        {
            var a = sheet.Cells[nRow, nCol].Value;
            if (a != null && !string.IsNullOrEmpty(a.ToString()))
                return int.Parse(a.ToString()!.Trim());
            return 0;
        }

        public static double GetCellToDouble(ExcelWorksheet sheet, int nRow, int nCol)
        {
            var a = sheet.Cells[nRow, nCol].Value;
            if (a != null && !string.IsNullOrEmpty(a.ToString()))
            {
                try { return double.Parse(a.ToString()!.Trim()); } catch { return 0; }
            }
            return 0;
        }

        public static long GetCellToLong(ExcelWorksheet sheet, int nRow, int nCol)
        {
            var a = sheet.Cells[nRow, nCol].Value?.ToString();
            return a != null ? long.Parse(a) : 0;
        }

        private static DateTime ParseDayMonthYear(string sValue)
        {
            var sep = sValue.Contains('/') ? '/' : '-';
            var dValue = sValue.Split(sep);
            return new DateTime(int.Parse(dValue[2][..4]), int.Parse(dValue[1]), int.Parse(dValue[0]));
        }

        public static DateTime? GetCellToDateTime(ExcelWorksheet sheet, int nRow, int nCol)
        {
            string sValue = GetCell(sheet, nRow, nCol);
            if (sValue == "") return null;

            string format = sheet.Cells[nRow, nCol].Style.Numberformat.Format ?? "";
            try
            {
                if (format.Contains("yy"))
                {
                    string[] formats = { "dd/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yyyy", "dd/MM/yy", "dd/M/yy", "d/M/yy", "d/MM/yy", "MM/dd/yyyy", "MM/dd/yy" };
                    if (DateTime.TryParseExact(sValue, formats, new CultureInfo("vi-VN"), DateTimeStyles.None, out var dtNgay))
                        return dtNgay;

                    try { return Convert.ToDateTime(sValue); }
                    catch { return DateTime.FromOADate(Convert.ToDouble(sValue)); }
                }
                if (format.Contains('h'))
                {
                    try { return Convert.ToDateTime(sValue); }
                    catch { return DateTime.FromOADate(Convert.ToDouble(sValue)); }
                }

                return ParseDayMonthYear(sValue);
            }
            catch
            {
                try { return ParseDayMonthYear(sValue); }
                catch
                {
                    try { return Convert.ToDateTime(sValue); }
                    catch
                    {
                        double raw = Math.Round(Convert.ToDouble(sValue) * 24 * 60) / (24 * 60);
                        return DateTime.FromOADate(raw);
                    }
                }
            }
        }

        public static bool KiemTraInsertOrUpdate(string Id)
            => string.IsNullOrEmpty(Id) || Id.Length <= 5;

        public static int LamTronTinhLuong(float giaTri)
            => (int)Math.Floor(giaTri + 0.5);

        public static char ChuyenTuSoSangChu(int so)
            => (char)(so + 64);

        private static (DateTime NgayDauKy, DateTime NgayCuoiKy) ResolveKyChamCongRange(
            int nam,
            int thang,
            string idDotChamCong,
            List<HRM_dmDotChamCongThang> lstDotChamCong)
        {
            var ngayDauThang = new DateTime(nam, thang, 1);
            var ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);
            var itemDot = lstDotChamCong.FirstOrDefault(q => q.IdGuid == idDotChamCong);
            if (itemDot == null) return (ngayDauThang, ngayCuoiThang);

            if (itemDot.NgayBatDau == 0) itemDot.NgayBatDau = 1;
            if (itemDot.NgayKetThuc == 0 || itemDot.NgayKetThuc >= ngayCuoiThang.Day)
                itemDot.NgayKetThuc = ngayCuoiThang.Day;

            return (new DateTime(nam, thang, itemDot.NgayBatDau), new DateTime(nam, thang, itemDot.NgayKetThuc));
        }

        public static List<NgayChamCong> GetDanhSachNgayCongTrongThang(
            int Nam,
            int Thang,
            string IdDanhMucDotChamCongThang,
            List<HRM_dmDotChamCongThang> lstDotChamCong,
            List<HRM.Api.Models.SmartEOEModel.CaiDatHeThong> lstCaiDatNghiLe)
        {
            var lstNgayChamCongTrongKy = new List<NgayChamCong>();
            var (ngayDauKy, ngayCuoiKy) = ResolveKyChamCongRange(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong);

            var ngayCuoiKyWithTime = ngayCuoiKy.AddDays(1).AddMilliseconds(-1);
            lstCaiDatNghiLe = (lstCaiDatNghiLe ?? new List<HRM.Api.Models.SmartEOEModel.CaiDatHeThong>())
                .Where(q => (q.ValueStartDateTime >= ngayDauKy && q.ValueStartDateTime <= ngayCuoiKyWithTime)
                         || (q.ValueEndDateTime >= ngayDauKy && q.ValueEndDateTime <= ngayCuoiKyWithTime))
                .ToList();

            var lstNgayNghiLe = new HashSet<int>();
            foreach (var itemCaiDatNghiLe in lstCaiDatNghiLe)
            {
                if (itemCaiDatNghiLe.Ma != DefineData.eMaSetting.NgayNghiLe.ToString()) continue;
                var start = itemCaiDatNghiLe.ValueStartDateTime > ngayDauKy ? itemCaiDatNghiLe.ValueStartDateTime.Date : ngayDauKy;
                var end = itemCaiDatNghiLe.ValueEndDateTime < ngayCuoiKy ? itemCaiDatNghiLe.ValueEndDateTime.Date : ngayCuoiKy;
                for (int d = start.Day; d <= end.Day; d++) lstNgayNghiLe.Add(d);
            }

            for (var current = ngayDauKy; current <= ngayCuoiKy; current = current.AddDays(1))
            {
                var nc = new NgayChamCong(current) { isNghiLe = lstNgayNghiLe.Contains(current.Day) };
                lstNgayChamCongTrongKy.Add(nc);
            }

            return lstNgayChamCongTrongKy;
        }

        public static List<NgayChamCong> GetDanhSachNgayCongTrongThang(
            int Nam,
            int Thang,
            string IdDanhMucDotChamCongThang,
            List<HRM_dmDotChamCongThang> lstDotChamCong,
            HRM_dmLichLamViec LichLamViec,
            HRM_dmLichNghiLe LichNghiLe)
        {
            var lstNgayChamCongTrongKy = new List<NgayChamCong>();
            var (ngayDauKy, ngayCuoiKy) = ResolveKyChamCongRange(Nam, Thang, IdDanhMucDotChamCongThang, lstDotChamCong);

            for (var current = ngayDauKy; current <= ngayCuoiKy; current = current.AddDays(1))
            {
                var nc = new NgayChamCong(current);
                if (LichNghiLe?.listNgay?.Find(x => x.TuNgay >= current && x.DenNgay <= current) != null)
                    nc.isNghiLe = true;

                var itemNgay = LichLamViec?.listNgay?.Find(x => DefineData.QuyDoiThuTA(x.Thu ?? 0) == current.DayOfWeek.ToString());
                nc.isNghi = itemNgay == null || !itemNgay.isNgayLamViec;

                lstNgayChamCongTrongKy.Add(nc);
            }

            return lstNgayChamCongTrongKy;
        }

        public static double ConvertGioPhutSangGio(string input)
        {
            if (string.IsNullOrEmpty(input) || !input.Contains(':')) return 0;
            var parts = input.Split(':');
            int hour = Convert.ToInt32(parts[0]);
            int phut = Convert.ToInt32(parts[1]);
            return Math.Round((phut / 60.0) + hour, 2);
        }

        public static double ConvertGioPhut(string input, ref int hour, ref int phut)
        {
            if (string.IsNullOrEmpty(input) || !input.Contains(':')) return 0;
            var parts = input.Split(':');
            hour = Convert.ToInt32(parts[0]);
            phut = Convert.ToInt32(parts[1]);
            return hour;
        }
    }
}

#pragma warning restore CS8981

