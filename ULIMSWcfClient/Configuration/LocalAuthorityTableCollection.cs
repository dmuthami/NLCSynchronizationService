using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    [ConfigurationCollection(typeof(LocalAuthorityTableElement))]
    public class LocalAuthorityTableCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalAuthorityTableElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalAuthorityTableElement)(element)).LocalAuthority;
        }

        public LocalAuthorityTableElement this[int id]
        {
            get
            {
                return (LocalAuthorityTableElement)BaseGet(id);
            }
        }
    }
}