using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface IValuation
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ValuationData> GetVRecordsByState(string localAuthority, string localAuthorityCode, int maximumRows, ObjectState state);
    }

    [DataContract]
    public class ValuationData
    {

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string ValuationCode { get; set; }

        [DataMember]
        public string StandNo { get; set; }

        [DataMember]
        public string ErfNo { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public string LocalAuthority { get; set; }

        [DataMember]
        public string Township { get; set; }

        [DataMember]
        public decimal GroundValue { get; set; }

        [DataMember]
        public decimal ImprovementValue { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal TotalValue { get; set; }

        [DataMember]
        public decimal ErfArea { get; set; }

        [DataMember]
        public string DescriptionOfImprovement { get; set; }

        [DataMember]
        public string RegisteredOwner { get; set; }

        [DataMember]
        public string Zoning { get; set; }

        [DataMember]
        public string DeedNo { get; set; }

        [DataMember]
        public ObjectState ItemState { get; set; }
    }
}