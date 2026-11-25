#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Common.Interfaces;
using HRM.Api.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Models;
using Newtonsoft.Json;

namespace HRM.Api.Services.ChamCong
{

    // quy trình đã chuyển qua smartEOS
        public class funcChamCongDuAn : FuncBase
    {
        public funcChamCongDuAn(
            CallWebService callWebService,
            IWorkflowEngine workflowEngine,
            IOptions<HrmOptions> options,
            ILogger<funcChamCongDuAn> logger)
            : base(callWebService, workflowEngine, options, logger)
        {
        }

    }

}
