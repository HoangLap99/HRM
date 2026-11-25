﻿#nullable disable
using HRM.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntitySingleResponse<T> where T:class
    {
        public MessageReturn Error { get; set; }
        public T Item { get; set; }
    }
}
