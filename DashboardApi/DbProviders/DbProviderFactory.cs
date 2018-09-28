using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.DbProviders
{
    enum DBProviderType
    {
        Odbc,
        Oledb
    };

    public class DbProviderFactory
    {
        static IDbProvider GetDbProvider(DBProviderType dbProviderType)
        {
            switch (dbProviderType)
            {
                case DBProviderType.Odbc:
                    return new OdbcDBProvider();                    

                case DBProviderType.Oledb:
                    return new OledbDBProvider();
                default:
                    return null;
            }
        }
    }
}