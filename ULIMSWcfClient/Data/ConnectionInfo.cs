using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using ULIMSWcfClient.Configuration;

namespace ULIMSWcfClient.Data
{
    public class ConnectionInfo
    {
        private static string connectionString;
        private static string providerName;
        private const string defaultKeyName = "DefaultConnectionString";

        public static readonly ConnectionInfo Default = new ConnectionInfo();

        public ConnectionInfo()
            : this(ConfigHelper.GetSetting(defaultKeyName))
        {
        }
        public ConnectionInfo(string connectionStringName)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            ConnectionString = settings.ConnectionString;
            ProviderName = settings.ProviderName;
        }

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }
        public string ProviderName
        {
            get { return providerName; }
            set { providerName = value; }
        }
    }
}