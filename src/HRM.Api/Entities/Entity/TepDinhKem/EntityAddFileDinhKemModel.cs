﻿#nullable disable
using HRM.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HRM.Api.Entity
{
    public class EntityAddFileDinhKemModel
    {
        public EntityAddFileDinhKemModel(List<HDFile> aListFile,string aIdTable,string aNameTable, DefineData.eFlagDinhKem aFlagDinhKem)
        {
            this.ListFile = aListFile;
            this.IdTable = aIdTable;
            this.NameTable = aNameTable;
            this.FlagDinhKem = aFlagDinhKem.ToString();
        }

        public EntityAddFileDinhKemModel(HDFile aFile, string aIdTable, string aNameTable, DefineData.eFlagDinhKem aFlagDinhKem)
        {
            this.ListFile = new List<HDFile>() { aFile };
            this.IdTable = aIdTable;
            this.NameTable = aNameTable;
            this.FlagDinhKem = aFlagDinhKem.ToString();
        }
        public List<HDFile> ListFile { get; set; }
        public string IdTable { get; set; }
        public string NameTable { get; set; }
        public string FlagDinhKem { get; set; }
    }

    public class EntityAddHRMFileDinhKemModel
    {
        public EntityAddHRMFileDinhKemModel(HRM_FileDinhKem aFile, string aIdTable, string aNameTable, string aFlagDinhKem)
        {
            this.ListFile = new List<HRM_FileDinhKem>() { aFile };
            this.IdTable = aIdTable;
            this.NameTable = aNameTable;
            this.FlagDinhKem = aFlagDinhKem;
        }
        public EntityAddHRMFileDinhKemModel(List<HRM_FileDinhKem> aListFile,string aIdTable,string aNameTable,string aFlagDinhKem)
        {
            this.ListFile = aListFile;
            this.IdTable = aIdTable;
            this.NameTable = aNameTable;
            this.FlagDinhKem = aFlagDinhKem;
        }
        public List<HRM_FileDinhKem> ListFile { get; set; }
        public string IdTable { get; set; }
        public string NameTable { get; set; }
        public string FlagDinhKem { get; set; }
    }
}
