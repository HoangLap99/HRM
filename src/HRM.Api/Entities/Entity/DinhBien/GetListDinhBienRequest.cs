﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.DinhBien
{
    public class GetListDinhBienRequest:EntityRequest
    {
        public Nullable<long> IdCoSo { get; set; }
    }
}
