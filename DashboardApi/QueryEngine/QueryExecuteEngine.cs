using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi
{
    public class QueryExecuteEngine
    {
        private static string connectionString = "database=demo;host=127.0.0.1;PORT=50000;username=monetdb;password=monetdb";

        public static System.Data.DataTable ExecuteQuery(string query, string[] columns)
        {
            //System.Data.DataSet ds = new System.Data.DataSet();
            var dt = new System.Data.DataTable("Data");
            foreach(var col in columns)
            {
                if (!string.IsNullOrWhiteSpace(col))
                {
                    dt.Columns.Add(col);
                }                
            }
            using (var connection = new System.Data.MonetDb.MonetDbConnection("database=demo;host=127.0.0.1;PORT=50000;username=monetdb;password=monetdb"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    using (var reader = command.ExecuteReader())
                    {                       
                        while (reader.Read())
                        {
                            var values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            dt.Rows.Add(values);
                        }
                    }
                    //ds.Tables.Add(dt);
                }
                connection.Close();
            }
            return dt;           
        }
    }
}