using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    public class LocalAuthorityViewSection : ConfigurationSection
    {
        [ConfigurationProperty("Views")]
        public LocalAuthorityViewCollection LocalAuthoritiesKeys
        {
            get { return ((LocalAuthorityViewCollection)(base["Views"])); }
        }
    }  
}