using System.Data;
using System.Data.Odbc;

namespace DashboardApi.DbProviders
{
    public class OdbcDBProvider: IVizDbProvider
    {
        public IDbConnection GetConnection(string connString)
        {
            var conn = new OdbcConnection(connString);
            return conn;
        }

        public IDbCommand CreateDBCommand(string query, IDbConnection conn)
        {
            var cmd = new OdbcCommand(query, (OdbcConnection)conn);
            return cmd;
        }

        public IDbDataAdapter CreateDBDataAdapter(IDbCommand cmd)
        {
            var da =  new OdbcDataAdapter((OdbcCommand)cmd);
            return da;
        }
    }
}