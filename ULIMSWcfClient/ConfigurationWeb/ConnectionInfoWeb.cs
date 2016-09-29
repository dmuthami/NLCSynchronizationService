using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.ConfigurationWeb
{
    public class ConnectionInfoWeb
    {
        private static string connectionString;
        private static string providerName;
        private const string defaultKeyName = "DefaultConnectionString";

        public static readonly ConnectionInfoWeb Default = new ConnectionInfoWeb();

        public ConnectionInfoWeb()
            : this(ConfigHelperWeb.GetSetting(defaultKeyName))
        {
        }
        public ConnectionInfoWeb(string connectionStringName)
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