using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Data
{
    public class Database
    {
        protected DbProviderFactory factory;
        private ConnectionInfo connectionInfo;
        private DbSettings dbSettings;

        public Database()
            : this(ConnectionInfo.Default)
        {

        }

        public Database(ConnectionInfo _connectionInfo)
            : this(new DbSettings(_connectionInfo))
        {
            this.connectionInfo = _connectionInfo;
            factory = DbProviderFactories.GetFactory(_connectionInfo.ProviderName);
        }

        public Database(DbSettings dbSettings)
        {
            this.dbSettings = dbSettings;
        }

        public ConnectionInfo ConnectionInfo { get { return connectionInfo; } }
        public DbSettings DbSettings { get { return dbSettings; } }

        public DbConnection DbConnection
        {
            get
            {
                DbConnection connection = factory.CreateConnection();
                connection.ConnectionString = connectionInfo.ConnectionString;
                return connection;
            }
        }

        public DbParameter DbParameter
        {
            get
            {
                return factory.CreateParameter();
            }
        }

        public DbDataAdapter DbDataAdapter
        {
            get
            {
                return factory.CreateDataAdapter();
            }
        }
    }
}