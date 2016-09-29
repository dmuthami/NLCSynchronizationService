using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.ConfigurationWeb
{
    [ConfigurationCollection(typeof(LocalAuthorityElement))]
    public class LocalAuthorityCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalAuthorityElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalAuthorityElement)(element)).LocalAuthority;
        }

        public LocalAuthorityElement this[int idx]
        {
            get
            {
                return (LocalAuthorityElement)BaseGet(idx);
            }
        }
    }
}