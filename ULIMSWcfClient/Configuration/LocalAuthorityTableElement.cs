using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    public class LocalAuthorityTableElement : ConfigurationElement
    {
        [ConfigurationProperty("localAuthority", DefaultValue = "",
         IsKey = true, IsRequired = true)]
        public string LocalAuthority
        {
            get
            {
                return ((string)(base["localAuthority"]));
            }
            set
            {
                base["localAuthority"] = value;
            }
        }

        [ConfigurationProperty("table",
          DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Table
        {
            get
            {
                return ((string)(base["table"]));
            }
            set
            {
                base["table"] = value;
            }
        }

        [ConfigurationProperty("code",
          DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Code
        {
            get
            {
                return ((string)(base["code"]));
            }
            set
            {
                base["code"] = value;
            }
        }

        [ConfigurationProperty("db",
         DefaultValue = "", IsKey = false, IsRequired = true)]
        public string DB
        {
            get
            {
                return ((string)(base["db"]));
            }
            set
            {
                base["db"] = value;
            }
        }
    }
}