﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity.DinhBien
{
    public class GetListQuyTrinhDinhBienRequest:EntityRequest
    {
        public string SoQuyTrinh { get; set; }
    }
}
