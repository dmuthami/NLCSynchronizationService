using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface ICharges : ISyncSpToNav<ChargesData>
    {
    }

    [DataContract]
    public class ChargesData
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string ApplicationID { get; set; }

        [DataMember]
        public string ChargeType { get; set; }

        [DataMember]
        public int ChargeTypeID { get; set; }

        [DataMember]
        public string ApplicationGuid { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public string StandNo { get; set; }

        [DataMember]
        public string LocalAuthority { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public ObjectState ItemState { get; set; }
    }
}