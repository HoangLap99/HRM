﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class CodeName
    {
        public class StringCodeName
        {
            public StringCodeName()
            {

            }
            public StringCodeName(string aCode,string aName)
            {
                this.Code = aCode;
                this.Name = aName;
            }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class NumberCodeName
        {
            public NumberCodeName()
            {

            }
            public NumberCodeName(long aCode,string aName)
            {
                this.Code = aCode;
                this.Name = aName;
            }
            public long Code { get; set; }
            public string Name { get; set; }
        }

        public class InterCodeName
        {
            public InterCodeName()
            {

            }
            public InterCodeName(int aCode,string aName)
            {
                this.Code = aCode;
                this.Name = aName;
            }
            public int Code { get; set; }
            public string Name { get; set; }
        }
    }
}
