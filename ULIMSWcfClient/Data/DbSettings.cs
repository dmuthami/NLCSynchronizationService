using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Data
{
    public class DbSettings
    {
        public DbSettings(ConnectionInfo connectionInfo)
        {
            switch (connectionInfo.ProviderName)
            {
                case "System.Data.SqlClient":
                case "System.Data.Odbc":
                case "System.Data.SqlServerCe.3.5":
                    ParameterPrefix = "@"; break;
                case "System.Data.OracleClient":
                    ParameterPrefix = "V_"; break;
                case "System.Data.OleDb":
                    ParameterPrefix = ""; break;
            }
        }

        public string ParameterPrefix { get; set; }
        public bool EnableTransactions { get; set; }
    }
}