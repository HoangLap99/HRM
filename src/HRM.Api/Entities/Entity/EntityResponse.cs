﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using HRM.Api.Models;

namespace HRM.Api.Entity
{
    public class EntityResponse<T> where T:class
    {
        public PagingInfor paging { get; set; }
        public List<T> items { get; set; }
        public Object value { get; set; }
    }
}
