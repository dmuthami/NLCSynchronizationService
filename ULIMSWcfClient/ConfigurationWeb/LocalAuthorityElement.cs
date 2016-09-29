using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.ConfigurationWeb
{
    public class LocalAuthorityElement : ConfigurationElement
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

        [ConfigurationProperty("siteUrl",
          DefaultValue = "", IsKey = false, IsRequired = true)]
        public string SiteUrl
        {
            get
            {
                return ((string)(base["siteUrl"]));
            }
            set
            {
                base["siteUrl"] = value;
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