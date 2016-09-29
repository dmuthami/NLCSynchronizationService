using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.ConfigurationWeb
{
    public class LocalAuthoritySection : ConfigurationSection
    {
        [ConfigurationProperty("LocalAuthorities")]
        public LocalAuthorityCollection LocalAuthoritiesKeys
        {
            get { return ((LocalAuthorityCollection)(base["LocalAuthorities"])); }
        }
    }
}