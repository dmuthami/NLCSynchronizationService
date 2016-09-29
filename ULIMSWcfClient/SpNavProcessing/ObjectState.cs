using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing
{
    [DataContract]
    public enum ObjectState
    {
        [EnumMember]
        New = 0,

        [EnumMember]
        Unchanged = 1,

        [EnumMember]
        Update = 2,

        [EnumMember]
        Delete = 3,

        [EnumMember]
        Ignore = 4
    }
}