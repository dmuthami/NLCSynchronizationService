using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface IValuationPeriod
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ValuationPeriodData> GetVpRecordsByState(string localAuthority, string localAuthCode, int maximumRows, ObjectState state);
    }

    [DataContract]
    public class ValuationPeriodData
    {

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string RequestID { get; set; }

        [DataMember]
        public string PeriodName { get; set; }

        [DataMember]
        public string ValuationType { get; set; }

        [DataMember]
        public ObjectState ItemState { get; set; }
    }
}