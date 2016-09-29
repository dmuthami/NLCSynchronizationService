using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    public class LocalAuthorityTableSection : ConfigurationSection
    {
        [ConfigurationProperty("Tables")]
        public LocalAuthorityTableCollection LocalAuthoritiesKeys
        {
            get { return ((LocalAuthorityTableCollection)(base["Tables"])); }
        }
    }
}