using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration_Local
{
    public class LocalAuthoritySection : ConfigurationSection
    {
        [ConfigurationProperty("LocalAuthority")]
        public LocalAuthorityCollection LocalAuthoritiesKeys
        {
            get { return ((LocalAuthorityCollection)(base["LocalAuthority"])); }
        }
    }
}