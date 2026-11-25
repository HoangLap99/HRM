using Newtonsoft.Json;

namespace HRM.Api.Common;

/// <summary>
/// Chuẩn response JSON trả về cho Frontend (giữ nguyên PascalCase: Error, StatusCode, sError, Detail, Value).
/// </summary>
public class MessageReturn
{
    public DefineData.eErrorMessage Error { get; set; }
    public int StatusCode { get; set; }

    public string sError => Error.ToString();

    public string? Detail { get; set; }
    public dynamic? Value { get; set; }

    public MessageReturn()
    {
    }

    public MessageReturn(DefineData.eErrorMessage error, string detail)
    {
        StatusCode = (int)error;
        Error = error;
        Detail = detail;
    }

    public MessageReturn(DefineData.eErrorMessage error, string detail, dynamic? value)
    {
        StatusCode = (int)error;
        Error = error;
        Detail = detail;
        Value = value;
    }

    public static MessageReturn Success(string detail = "Cập nhật thành công", dynamic? value = null)
        => new(DefineData.eErrorMessage.CapNhatThanhCong, detail, value);

    public static MessageReturn Fail(string detail, DefineData.eErrorMessage error = DefineData.eErrorMessage.LoiCapNhat, dynamic? value = null)
        => new(error, detail, value);

    public static MessageReturn Unauthenticated(string detail = "Chưa đăng nhập")
        => new(DefineData.eErrorMessage.ChuaDangNhap, detail);
}

/// <summary>
/// Model thông báo lỗi legacy dùng trong một số module nhập liệu và đồng bộ quy trình.
/// </summary>
public class ErrorMessage
{
    public enum eState : int
    {
        KhongThanhCong = 0,
        ThanhCong = 200,
        VanDangSuDung = 2,
        ChuoiRong = 3,
        CapNhatThanhCong = 4,
        XoaThanhCong = 6,
        ChuaNhapItem = 7,
        MaRong = 8,
        SoPhieuRong = 9,
        SoPhieuTrung = 10,
        MaBiTrung = 11,
        TongHopThanhCong = 12,
        ImportDuLieuThanhCong = 13,
        TruyCapDuLieu = 14,
        ExportDuLieuThanhCong = 15,
        ChuyenTiepThanhCong = 16,
        KichHoatTaiKhoanThanhCong = 17,
        ChuaDangNhap = 18,
        LoiKhac = 99,
        LoiVang = 100,
        TaoQuyTrinhThanhCong = 1,
        QuaSoLuongDangCo = 3,
        ThemMoiDuLieu = 7,
        QuyTrinhKhongTonTai = 20,
        QuyTrinhDaChuyenTruocDo = 21,
        KhongTimThayTrangThai = 22,
        KhongCoDuLieu = 23,
        KhongTheXoa = 24,
    }

    public dynamic? Data { get; set; }
    public dynamic? objectReturn { get; set; }
    public long ID { get; set; }
    public string? message { get; set; }

    [JsonProperty("StatusCode")]
    public int StatusCode { get; set; }

    private eState _state;
    public eState State
    {
        get => _state;
        set => (_state, message) = value switch
        {
            eState.MaBiTrung => (eState.KhongThanhCong, "Mã bị trùng!"),
            eState.VanDangSuDung => (eState.KhongThanhCong, "Danh mục vẫn đang sử dụng: " + message),
            eState.ChuoiRong => (eState.KhongThanhCong, "Chuỗi rỗng!"),
            eState.CapNhatThanhCong => (eState.ThanhCong, "Cập nhật dữ liệu thành công!"),
            eState.ImportDuLieuThanhCong => (eState.ThanhCong, "Nhận dữ liệu thành công!"),
            eState.ExportDuLieuThanhCong => (eState.ThanhCong, "Xuất dữ liệu thành công!"),
            eState.TongHopThanhCong => (eState.ThanhCong, "Tổng hợp dữ liệu thành công!"),
            eState.XoaThanhCong => (eState.ThanhCong, "Xóa dữ liệu thành công!"),
            eState.KichHoatTaiKhoanThanhCong => (eState.ThanhCong, "Kích hoạt tài khoản thành công!"),
            eState.ChuaDangNhap => (eState.ThanhCong, "Chưa đăng nhập!"),
            eState.ChuaNhapItem => (eState.KhongThanhCong, "Chưa nhập Item!"),
            eState.MaRong => (eState.KhongThanhCong, "Mã không được trống!"),
            eState.SoPhieuRong => (eState.KhongThanhCong, "Số phiếu không được rỗng!"),
            eState.SoPhieuTrung => (eState.KhongThanhCong, "Số phiếu bị trùng!"),
            eState.ChuyenTiepThanhCong => (eState.ThanhCong, "Chuyển tiếp thành công!"),
            eState.LoiVang => (eState.KhongThanhCong, "Lỗi văng của backend!" + message),
            eState.TruyCapDuLieu => (eState.KhongThanhCong, "Bạn không có quyền truy cập dữ liệu!"),
            eState.ThanhCong => (eState.ThanhCong, message),
            eState.LoiKhac => (eState.KhongThanhCong, message),
            _ => (value, message)
        };
    }

    public eState Status
    {
        set
        {
            var (statusCode, newMessage) = value switch
            {
                eState.QuaSoLuongDangCo => (eState.KhongThanhCong, "Nhiều hơn số lượng đang có"),
                eState.TaoQuyTrinhThanhCong => (eState.ThanhCong, "Tạo quy trình thành công"),
                eState.CapNhatThanhCong => (eState.ThanhCong, "Cập nhật dữ liệu thành công!"),
                eState.XoaThanhCong => (eState.ThanhCong, "Xóa dữ liệu thành công!"),
                eState.ThemMoiDuLieu => (eState.ThanhCong, "Thêm mới thành công"),
                eState.ChuyenTiepThanhCong => (eState.ThanhCong, "Chuyển tiếp thành công!"),
                eState.ImportDuLieuThanhCong => (eState.ThanhCong, "Nhận dữ liệu thành công!"),
                eState.ExportDuLieuThanhCong => (eState.ThanhCong, "Xuất dữ liệu thành công!"),
                eState.MaBiTrung => (eState.KhongThanhCong, "Mã bị trùng!"),
                eState.VanDangSuDung => (eState.KhongThanhCong, "Danh mục vẫn đang sử dụng!"),
                eState.ChuaDangNhap => (eState.KhongThanhCong, "Đăng nhập không thành công vui lòng kiểm tra lại!"),
                eState.QuyTrinhKhongTonTai => (eState.KhongThanhCong, "Quy trình không tồn tại vui lòng kiểm tra!"),
                eState.QuyTrinhDaChuyenTruocDo => (eState.KhongThanhCong, "Quy trình đã được chuyển trước đó vui lòng kiểm tra!"),
                eState.KhongTimThayTrangThai => (eState.KhongThanhCong, "Không tìm thấy trạng thái, vui lòng kiểm tra cài đặt quy trình!"),
                eState.KhongThanhCong => (eState.KhongThanhCong, "Có lỗi trong quá trình xử lý!" + message),
                eState.ThanhCong => (eState.ThanhCong, "Xử lý thành công!"),
                eState.LoiKhac => (eState.KhongThanhCong, message),
                eState.KhongCoDuLieu => (eState.ThanhCong, "Không có dữ liệu!"),
                eState.KhongTheXoa => (eState.KhongThanhCong, "Đang được sử dụng, không thể xóa!"),
                _ => (value, message)
            };
            StatusCode = (int)statusCode;
            message = newMessage;
        }
    }

    public ErrorMessage()
    {
        State = eState.KhongThanhCong;
    }

    public ErrorMessage(eState state)
    {
        State = state;
    }

    public void SetDanhMucDangSuDung(string mess)
    {
        message = mess;
        State = eState.VanDangSuDung;
    }

    public void SetLoiKhac(string mess)
    {
        message = mess;
        State = eState.LoiKhac;
    }

    public void SetLoiVang(string mess)
    {
        message = mess;
        State = eState.LoiVang;
    }

    public void MaBiTrung(string mess)
    {
        message = mess;
        State = eState.MaBiTrung;
    }

    public void SetThanhCong(string mess)
    {
        message = mess;
        State = eState.ThanhCong;
    }

    public void SetDangNhapKhongThanhCong(string mess)
    {
        message = mess;
        State = eState.KhongThanhCong;
    }
}

/// <summary>
/// Thông tin phân trang chuẩn trả về cho Frontend.
/// </summary>
public class PagingInfor
{
    public int TotalPage { get; set; }
    public int TotalItem { get; set; }
    public int CurrentPage { get; set; }

    public PagingInfor()
    {
    }

    public PagingInfor(int currentPage, int totalItem, int totalPage)
    {
        TotalPage = totalPage;
        TotalItem = totalItem;
        CurrentPage = currentPage;
    }
}
