using HRM.Api.Common;

namespace HRM.Api.Common.Interfaces.SmartEos;

/// <summary>
/// Giao tiếp đồng bộ dữ liệu chấm công khuôn mặt từ SmartEOS.
/// </summary>
public interface ISmartEosFaceClient
{
    Task<List<HRMFaceResponseModel>> GetChamCongFaceAsync(HRMFaceModel model, CancellationToken ct = default);
}
