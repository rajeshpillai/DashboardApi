using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public class OdbcModel
    {
        public string ConnectionString { get; set; }

        public string TableName { get; set; }

        public string NewTableName { get; set; }

        public List<DBColumn> ColumnNames { get; set; }
    }

    public class DBColumn
    {
        public string column_name { get; set; }

        public string data_type { get; set; }

    }
}