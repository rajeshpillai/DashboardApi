using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public class ImportedFileModel
    {
        public List<ColumnDetail> Columns { get; set; }

        public string FileName { get; set; }

        public string Delemiter { get; set; }

        public string TableName { get; set; }
    }

   
}