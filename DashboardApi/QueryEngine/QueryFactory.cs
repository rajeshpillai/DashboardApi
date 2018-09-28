using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi
{
    public enum DBType
    {
        MySql,
        PostgreSql
    }
    public class DbQueryFactory
    {
        public static string GetTableListQuery(DBType dBType, string databaseName)
        {
            string query = string.Empty;

            switch (dBType)
            {
                case DBType.MySql:
                    query = "SELECT table_name FROM information_schema.tables where table_schema = '" + databaseName + "'; ";
                    break;

                case DBType.PostgreSql:
                    query = "SELECT tablename as table_name FROM pg_catalog.pg_tables  where schemaname = 'public';";
                    break;

            }

            return query;
        }


        public static string GetColumsnForTableQuery(DBType dBType, string databaseName, string tableName)
        {
            string query = string.Empty;

            switch (dBType)
            {
                case DBType.MySql:
                    query = "SELECT COLUMN_NAME column_name, DATA_TYPE data_type,IS_NULLABLE is_nullable FROM information_schema.columns WHERE table_schema='" + databaseName + "' AND table_name='" + tableName + "'";
                    break;

                case DBType.PostgreSql:
                    query = "select column_name column_name, data_type data_type, is_nullable is_nullable, character_maximum_length, numeric_precision, numeric_scale from information_schema.columns where table_name = '" + tableName + "';";
                    break;

            }

            return query;
        }
    }
}