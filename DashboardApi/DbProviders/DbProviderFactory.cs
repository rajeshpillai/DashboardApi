using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.DbProviders
{
    public class DbProviderFactory
    {
        public static IVizDbProvider GetDbProvider(VizDBProviderType dbProviderType)
        {
            switch (dbProviderType)
            {
                case VizDBProviderType.odbc:
                    return new OdbcDBProvider();                    

                case VizDBProviderType.oledb:
                    return new OledbDBProvider();
                default:
                    return null;
            }
        }
    }
}