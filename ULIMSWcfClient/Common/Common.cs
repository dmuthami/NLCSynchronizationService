using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Common
{
    public class Common
    {
        public static T ConvertTo<T>(object value)
        {
            T returnValue = default(T);
            try
            {
                if (value != null)
                    returnValue = (T)Convert.ChangeType(value, typeof(T));
            }
            catch { }
            return returnValue;
        }
    }
}