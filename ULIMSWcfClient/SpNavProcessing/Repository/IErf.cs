using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface IErf
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ErfDat> GetRecordsByState(string localAuthority, int maximumRows, ObjectState state);
    }

    [DataContract]
    public class ErfDat
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public Guid GlobalId { get; set; }

        [DataMember]
        public Nullable<decimal> ComputedSize { get; set; }

        [DataMember]
        public string Zoning { get; set; }

        [DataMember]
        public string Density { get; set; }

        [DataMember]
        public string ErfNo { get; set; }

        [DataMember]
        public string Ownership { get; set; }

        [DataMember]
        public string Portion { get; set; }

        [DataMember]
        public string StandNo { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public Nullable<decimal> SurveySize { get; set; }

        [DataMember]
        public Nullable<System.DateTime> CreatedDate { get; set; }

        [DataMember]
        public Nullable<System.DateTime> LastEditDate { get; set; }

        [DataMember]
        public string LocalAuthority { get; set; }

        [DataMember]
        public string Township { get; set; }

        [DataMember]
        public ObjectState ItemState { get; set; }
    }
}