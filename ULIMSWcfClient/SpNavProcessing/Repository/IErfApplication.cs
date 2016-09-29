using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface IErfApplication : ISyncASpToNav<ErfApplicationData>
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ErfApplicationData> GetErfApplicationWithCharges(string localAuthority, int maximumRows);
    }

    [DataContract]
    public class ErfApplicationData
    {
        [DataMember]
        public IList<ChargesData> ErfCharges { get; set; }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string ApplicationID { get; set; }

        [DataMember]
        public Decimal MinimumBuildingValue { get; set; }

        [DataMember]
        public Decimal PurchasePrice { get; set; }

        [DataMember]
        public string ApplicantName { get; set; }

        [DataMember]
        public string ApplicantPostalAddress { get; set; }

        [DataMember]
        public string ApplicantPhysicalAddress { get; set; }

        [DataMember]
        public string ApplicantPassportNumber { get; set; }

        [DataMember]
        public string ApplicantPhoneNumber { get; set; }

        [DataMember]
        public string ApplicantIdNumber { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string StandNo { get; set; }

        [DataMember]
        public DateTime PurchaseDate { get; set; }

        [DataMember]
        public string MethodOfPayment { get; set; }

        [DataMember]
        public string LocalAuthority { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public string ApplicationType { get; set; }

        [DataMember]
        public string UserID { get; set; }

        [DataMember]
        public string Township { get; set; }

        [DataMember]
        public string Zoning { get; set; }

        [DataMember]
        public string ApplicantGender { get; set; }


        [DataMember]
        public string ApplicantWebsite { get; set; }

        [DataMember]
        public string ApplicantFax { get; set; }

        [DataMember]
        public Decimal Deposit { get; set; }

        [DataMember]
        public int InterestRate { get; set; }

        [DataMember]
        public DateTime InstallmentStartDate { get; set; }

        [DataMember]
        public ObjectState ItemState { get; set; }
    }
}