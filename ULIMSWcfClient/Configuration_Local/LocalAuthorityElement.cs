using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration_Local
{
    public class LocalAuthorityElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "",
         IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return ((string)(base["name"]));
            }
            set
            {
                base["name"] = value;
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
    }
}