using System.Data;
using System.Data.OleDb;

namespace DashboardApi.DbProviders
{
    public class OledbDBProvider : IVizDbProvider
    {
        public IDbConnection GetConnection(string connString)
        {
            var conn = new OleDbConnection(connString);
            return conn;
        }

        public IDbCommand CreateDBCommand(string query, IDbConnection conn)
        {
            var cmd = new OleDbCommand(query, (OleDbConnection)conn);
            return cmd;
        }

        public IDbDataAdapter CreateDBDataAdapter(IDbCommand cmd)
        {
            var da = new OleDbDataAdapter((OleDbCommand)cmd);
            return da;
        }
    }
}