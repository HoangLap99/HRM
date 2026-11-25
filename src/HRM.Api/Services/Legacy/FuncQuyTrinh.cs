#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using HRM.Api.Models.SmartEOEModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.QuyTrinh
{
    public class FuncQuyTrinh : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncQuyTrinh(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncQuyTrinh> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }
        //public dynamic GetPhongBanTheoUser(string IdUser)
        //{
        //    HRM.Api.Models.SmartEOEModel.BoPhan boPhan = new HRM.Api.Models.SmartEOEModel.BoPhan();
        //    var BoPhanUser = (from a in smdb.NhanViens
        //                      join b in smdb.Link_NhanVien_BoPhan_ChucVu on a.Id equals b.NhanVienPkid
        //                      join c in smdb.BoPhans on b.BoPhanPkid equals c.Id
        //                      where b.KiemNhiem == false && a.UserPkid == IdUser
        //                      select c).FirstOrDefault();

        //    if (BoPhanUser != null)
        //        return BoPhanUser;
        //    return boPhan;
        //}

        //public dynamic GetChucVuTheoUser(string IdUser)
        //{
        //    HRM.Api.Models.SmartEOEModel.ChucVu chucVu = new HRM.Api.Models.SmartEOEModel.ChucVu();
        //    var chucVuUser = (from a in smdb.NhanViens
        //                      join b in smdb.Link_NhanVien_BoPhan_ChucVu on a.Id equals b.NhanVienPkid
        //                      join c in smdb.ChucVus on b.ChucVuPkid equals c.Id
        //                      where b.KiemNhiem == false && a.UserPkid == IdUser
        //                      select c).FirstOrDefault();

        //    if (chucVuUser != null)
        //        return chucVuUser;
        //    return chucVu;
        //}
    }
}