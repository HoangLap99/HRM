﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntityRequest
    {
        public int CurrentPage { get; set; }
        public int NumperPage { get; set; }
        public int? PageSize { get; set; }
        public string KeyWord { get; set; }
    }
}
