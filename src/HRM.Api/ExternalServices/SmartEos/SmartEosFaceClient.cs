using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Interfaces.SmartEos;
using HRM.Api.Common.Options;
using HRM.Api.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HRM.Api.ExternalServices.SmartEos;

public sealed class SmartEosFaceClient : SmartEosHttpClientBase, ISmartEosFaceClient
{
    private const string RouterFace = "SmartEOSAPI/HRM_Face/";

    public SmartEosFaceClient(
        HttpClient httpClient,
        ICurrentUserContext currentUserContext,
        IOptions<HrmOptions> options,
        ILogger<SmartEosFaceClient> logger)
        : base(httpClient, currentUserContext, options, logger)
    {
    }

    public async Task<List<HRMFaceResponseModel>> GetChamCongFaceAsync(HRMFaceModel model, CancellationToken ct = default)
    {
        return await PostJsonAsync<List<HRMFaceResponseModel>>(
            $"{RouterFace}ChamCongFace/GetListChamCongFace",
            model,
            includeAuth: true,
            ct: ct) ?? new List<HRMFaceResponseModel>();
    }
}
