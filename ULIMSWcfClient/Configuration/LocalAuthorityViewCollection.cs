using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    [ConfigurationCollection(typeof(LocalAuthorityViewElement))]
    public class LocalAuthorityViewCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalAuthorityViewElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalAuthorityViewElement)(element)).LocalAuthority;
        }

        public LocalAuthorityViewElement this[int idx]
        {
            get
            {
                return (LocalAuthorityViewElement)BaseGet(idx);
            }
        }
    }    
}