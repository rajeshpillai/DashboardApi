using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi
{
    public enum VizDBType
    {
        mysql,
        postgresql
    }
    public class DbQueryFactory
    {
        public static string GetTableListQuery(VizDBType dBType, string databaseName)
        {
            string query = string.Empty;

            switch (dBType)
            {
                case VizDBType.mysql:
                    query = "SELECT table_name FROM information_schema.tables where table_schema = '" + databaseName + "'; ";
                    break;

                case VizDBType.postgresql:
                    query = "SELECT tablename as table_name FROM pg_catalog.pg_tables  where schemaname = 'public';";
                    break;

            }

            return query;
        }


        public static string GetColumsnForTableQuery(VizDBType dBType, string databaseName, string tableName)
        {
            string query = string.Empty;

            switch (dBType)
            {
                case VizDBType.mysql:
                    query = "SELECT COLUMN_NAME column_name, DATA_TYPE data_type,IS_NULLABLE is_nullable FROM information_schema.columns WHERE table_schema='" + databaseName + "' AND table_name='" + tableName + "'";
                    break;

                case VizDBType.postgresql:
                    query = "select column_name column_name, data_type data_type, is_nullable is_nullable, character_maximum_length, numeric_precision, numeric_scale from information_schema.columns where table_name = '" + tableName + "';";
                    break;

            }

            return query;
        }

        public static string GetAllDatabasesQuery(VizDBType dBType)
        {
            string query = string.Empty;

            switch (dBType)
            {
                case VizDBType.mysql:
                    query = "SELECT schema_name dbname FROM information_schema.schemata WHERE schema_name not LIKE '%_schema' ";
                    break;

                case VizDBType.postgresql:
                    query = "SELECT pg_database.datname as dbname FROM pg_database, pg_user WHERE pg_database.datdba = pg_user.usesysid";
                    break;

            }

            return query;
        }
    }
}