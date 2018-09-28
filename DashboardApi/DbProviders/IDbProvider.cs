using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DashboardApi.DbProviders
{
    public interface IDbProvider
    {
        IDbConnection GetConnection(string connString);

        IDbCommand CreateDBCommand(string query, IDbConnection conn);

        IDbDataAdapter CreateDBDataAdapter(IDbCommand cmd);

    }
}
