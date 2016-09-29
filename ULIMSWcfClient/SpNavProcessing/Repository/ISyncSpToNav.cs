using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;

namespace ULIMSWcfClient.SpNavProcessing.Repository
{
    [ServiceContract]
    public interface ISyncSpToNav<T>
    {
        T[] GetUnsynchronizedItems();
        Task<bool> SendUnsynchronizedItems(T[] items);

        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ErfDat> GetRecordsByState(string localAuthority, string localAuthorityCode, int maximumRows, ObjectState state);
    }

    [ServiceContract]
    public interface ISyncVSpToNav<T>
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ValuationData> GetVRecordsByState(string localAuthority, string localAuthorityCode, int maximumRows, ObjectState state);
    }

    [ServiceContract]
    public interface ISyncVpSpToNav<T>
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ValuationPeriodData> GetVpRecordsByState(string localAuthority, string localAuthCode, int maximumRows, ObjectState state);
    }

    [ServiceContract]
    public interface ISyncASpToNav<T>
    {
        [OperationContract]
        [FaultContract(typeof(Exception))]
        List<ErfApplicationData> GetErfApplicationWithCharges(string localAuthority, string localAuthCode, int maximumRows);
    }
}