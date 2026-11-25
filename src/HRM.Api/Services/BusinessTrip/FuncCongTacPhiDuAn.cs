#nullable disable
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRM.Api.Services.CongTacPhi
{
    public class FuncCongTacPhiDuAn : FuncBase
    {
        private string Host => HrmOptions.Host ?? "0";

        public FuncCongTacPhiDuAn(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<FuncCongTacPhiDuAn> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

    }
}