using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ULIMSWcfClient.Configuration;
using ULIMSWcfClient.ConfigurationWeb;
using ULIMSWcfClient.SpNavProcessing.Repository;
using ULIMSWcfClient.svcNavValuation;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class SyncValuation : ISyncVSpToNav<ValuationData>
    {
        #region Navision Data Execution Code
        public SyncValuation(string localAuthority, string localauthcode)
        {
            LocalAuthority = localAuthority;
            LocalAuthorityCode = localauthcode;
        }
        public SyncValuation(string localAuthority, CrudState state)
        {
            LocalAuthority = localAuthority;
            CurrentCrudState = state;
        }
        public string LocalAuthority { get; set; }
        public string LocalAuthorityCode { get; set; }
        public CrudState CurrentCrudState { get; set; }
        public async Task Synchronize(CrudState state)
        {
            Console.WriteLine(string.Format("Synchronize Valutation:{0}", state == CrudState.Edited ? "Edit" : state == CrudState.New ? "New" : "Synchronized"));
            CurrentCrudState = state;
            try
            {
                bool noMoreRecords = false;

                do
                {
                    ValuationData[] getValuationsTask = GetUnsynchronizedItems();
                    ValuationData[] valuationData = getValuationsTask;

                    if (valuationData == null)
                        break;

                    if (valuationData.Count() < ConfigHelperWeb.GetPageSize)
                    {
                        noMoreRecords = true;
                        if (valuationData.Count() == 0)
                            break;
                    }

                    Task<bool> sendItemsTask = SendUnsynchronizedItems(valuationData);
                    bool isSendSuccess = await sendItemsTask;
                    if (isSendSuccess)
                    {
                        bool flagItemsTask = FlagSynchronizedItems(valuationData);
                        bool isFlagSuccess = flagItemsTask;
                    }
                    else
                    {
                        break;
                    }
                } while (!noMoreRecords);
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Valuation\n Function: {0}\nError:{1}\n", "Synchronize", ex.Message));
            }
        }

        /// <summary>
        /// Main Method that initiates Execution of Erf-Valuations
        /// </summary>
        /// <returns></returns>
        public async Task Synchronize()
        {
            try
            {
                await Synchronize(CrudState.New);
                await Synchronize(CrudState.Edited);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public ValuationData[] GetUnsynchronizedItems()
        {
            try
            {
                int pageSize = ConfigHelperWeb.GetPageSize;

                ObjectState state = CurrentCrudState == CrudState.New ? ObjectState.New
                                      : CurrentCrudState == CrudState.Edited ? ObjectState.Update
                                      : CurrentCrudState == CrudState.Synchonized ? ObjectState.Unchanged
                                      : ObjectState.Ignore;

                var val_list = GetVRecordsByState(LocalAuthority, LocalAuthorityCode, pageSize, state).ToArray();
                ValuationData[] valuations = val_list;

                return valuations;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Valuation\n Function: {0}\nError:{1}\n", "GetUnsynchronizedItems", ex.Message));

                return null;
            }
        }
        public bool FlagSynchronizedItems(ValuationData[] items)
        {
            try
            {
                List<int> ids = new List<int>();
                foreach (var erf in items)
                    ids.Add(erf.ID);

                bool syncrhonizeTask = FlagSynchronizedRecords(LocalAuthorityCode, ids.ToArray());
                bool isValuationFlagged = syncrhonizeTask;
                return syncrhonizeTask;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Valuation\n Function: {0}\nError:{1}\n", "FlagSynchronizedItems", ex.Message));

                return false;
            }
        }
        public async Task<bool> SendUnsynchronizedItems(ValuationData[] items)
        {
            BasicHttpBinding navWSBinding = new BasicHttpBinding();
            navWSBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            navWSBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

            NetworkCredential _cred = new NetworkCredential()
            {
                Domain = ConfigHelperWeb.GetNavDomain,
                UserName = ConfigHelperWeb.GetNavUsername,
                Password = ConfigHelperWeb.GetNavPassword
            };

            string endpointname = "Valuation_Port";
            string wcfaddress = ConfigHelperWeb.GetClientAddressByName(endpointname);
            wcfaddress = wcfaddress.Replace("ReplaceWithAPercentEncodedCompanyName", Uri.EscapeDataString(LocalAuthority));

            Valuation_PortClient client = new Valuation_PortClient(navWSBinding, new EndpointAddress(wcfaddress));

            client.ClientCredentials.Windows.AllowedImpersonationLevel =
                System.Security.Principal.TokenImpersonationLevel.Delegation;

            client.ClientCredentials.Windows.ClientCredential = _cred;

            try
            {
                Valuation[] vals = MapApplicationPurchase(items);

                if (CurrentCrudState == CrudState.New)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Valuation_Filter[] { new Valuation_Filter { Field = Valuation_Fields.Stand_No, Criteria = criteria } };
                    request.bookmarkKey = null;
                    request.setSize = items.Count();

                    ReadMultiple_Result readResult = await client.ReadMultipleAsync(request.filter, null, request.setSize);
                    var readTask = readResult.ReadMultiple_Result1;

                    if (readTask.Count() > 0)
                    {
                        foreach (var item in readTask)
                        {
                            var exists = readTask.FirstOrDefault(o => o.Stand_No == item.Stand_No);
                            var getStan = (exists != null) ? exists.Stand_No : null;

                            if (item.Stand_No == getStan)
                            {
                                Update _update = new Update();
                                _update.Valuation = exists;

                                Update_Result upTask = await client.UpdateAsync(_update);
                            }
                            else if (item.Stand_No != getStan)
                            {
                                Create _create = new Create();
                                _create.Valuation = item;

                                Create_Result crTask = await client.CreateAsync(_create);
                            }
                        }
                    }
                    else
                    {
                        CreateMultiple createMultiple = new CreateMultiple();
                        createMultiple.Valuation_List = vals;
                        CreateMultiple_Result resultTask = await client.CreateMultipleAsync(createMultiple);
                        var result = resultTask;
                        vals = result.Valuation_List;
                    }
                }
                else if (CurrentCrudState == CrudState.Edited)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Valuation_Filter[] { new Valuation_Filter { Field = Valuation_Fields.Stand_No, Criteria = criteria } };
                    request.bookmarkKey = Valuation_Fields.Stand_No.ToString();
                    request.setSize = items.Count();

                    ReadMultiple_Result result = await client.ReadMultipleAsync(request.filter, request.bookmarkKey, request.setSize);
                    Valuation[] valsToUpdate = result.ReadMultiple_Result1;

                    if (valsToUpdate.Count() > 0)
                    {
                        UpdateMultiple updateMultiple = new UpdateMultiple();
                        Valuation[] editedErfs = GetEditedErfs(vals, valsToUpdate);
                        updateMultiple.Valuation_List = editedErfs;
                        Task<UpdateMultiple_Result> updateResultsTask = client.UpdateMultipleAsync(updateMultiple);
                        UpdateMultiple_Result updateResult = await updateResultsTask;
                        vals = updateResult.Valuation_List;
                    }
                }

                ((ICommunicationObject)client).Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Valuation\n Function: {0}\nError:{1}\n", "SendUnsynchronizedItems", ex.Message));

                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
                return false;
            }
            finally
            {
                if (((ICommunicationObject)client).State != CommunicationState.Closed)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
        }
        private Valuation[] GetEditedErfs(Valuation[] vals, Valuation[] valsToUpdate)
        {
            int cnt = valsToUpdate.Count();

            List<Valuation> lvals = vals.ToList();
            Valuation v, vu;
            for (int i = 0; i < cnt; i++)
            {
                vu = valsToUpdate[i];
                v = lvals.FirstOrDefault(l => string.Compare(l.Stand_No, vu.Stand_No) == 0);
                if (v != null)
                {
                    valsToUpdate[i].Description = v.Description;
                    valsToUpdate[i].Improvement_Value = v.Improvement_Value;
                    valsToUpdate[i].Land_Value = v.Land_Value;
                    valsToUpdate[i].Land_ValueSpecified = v.Land_ValueSpecified;
                    valsToUpdate[i].Improvement_ValueSpecified = v.Improvement_ValueSpecified;
                    valsToUpdate[i].Stand_No = v.Stand_No;
                    valsToUpdate[i].Effective_Date = v.Effective_Date;
                    valsToUpdate[i].Effective_DateSpecified = v.Effective_DateSpecified;
                }
            }

            return valsToUpdate;
        }
        private string GetFilter(ValuationData[] items)
        {
            StringBuilder sb = new StringBuilder();
            int cnt = 0;
            string appendor = string.Empty;
            foreach (ValuationData ed in items)
            {
                appendor = cnt++ == 0 ? "" : "|";
                sb.Append(string.Format("{0}={1}", appendor, ed.ID));
            }
            return sb.ToString();
        }
        private Valuation[] MapApplicationPurchase(ValuationData[] items)
        {
            List<Valuation> valuations = new List<Valuation>();
            Valuation val;
            foreach (ValuationData v in items)
            {
                val = new Valuation();
                val.Description = v.DescriptionOfImprovement;
                val.Improvement_Value = v.ImprovementValue;
                val.Land_Value = v.GroundValue;
                val.Land_ValueSpecified = true;
                val.Improvement_ValueSpecified = true;
                val.Stand_No = v.StandNo;

                if (DateTime.Compare(v.Date, DateTime.MinValue) == 0)
                    val.Effective_DateSpecified = false;
                else
                {
                    val.Effective_Date = v.Date;
                    val.Effective_DateSpecified = true;
                }

                valuations.Add(val);
            }
            return valuations.ToArray();
        }
        #endregion

        #region SharePoint Data Access Code
        public List<ValuationData> GetVRecordsByState(string localAuthority, string localAuthCode, int maximumRows, ObjectState state)
        {
            try
            {
                List<ValuationData> valuations = new List<ValuationData>();
                string siteurl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthCode);

                System.Net.WebRequest request = System.Net.HttpWebRequest.Create(siteurl);
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                CredentialCache.DefaultNetworkCredentials.Domain = ConfigHelperWeb.GetSPDomain;
                CredentialCache.DefaultNetworkCredentials.UserName = ConfigHelperWeb.GetSPUsername;
                CredentialCache.DefaultNetworkCredentials.Password = ConfigHelperWeb.GetSPPassword;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                System.Net.WebResponse response = request.GetResponse();

                using (ClientContext context = new ClientContext(siteurl))
                {
                    if (context != null)
                    {
                        context.Credentials = request.Credentials;

                        Web web = context.Web;
                        List list = web.Lists.GetByTitle("Valuation");

                        CamlQuery query = new CamlQuery();
                        string neworupdate = state == ObjectState.New ? "1"
                                       : state == ObjectState.Update ? "3"
                                       : "0";
                        query.ViewXml = string.Format(@"
                                    <View>
                                        <Query>" +
                                                 @"<Where>
                                                      <Eq>
                                                         <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                                         <Value Type='Number'>{0}</Value>
                                                      </Eq>
                                                   </Where>" +
                                        @"</Query>
                                        <ViewFields>
                                           <FieldRef Name='Title' />
                                           <FieldRef Name='Stand_x0020_No' />
                                           <FieldRef Name='Erf_x0020_No' />
                                           <FieldRef Name='Date' />
                                           <FieldRef Name='Local_x0020_Authority' />
                                           <FieldRef Name='Township' />
                                           <FieldRef Name='Ground_x0020_Value' />
                                           <FieldRef Name='Improvement_x0020_Value' />
                                           <FieldRef Name='Price' />
                                           <FieldRef Name='Total_x0020_Value' />
                                           <FieldRef Name='Erf_x0020_Area' />
                                           <FieldRef Name='Description_x0020_of_x0020_Impro' />
                                           <FieldRef Name='Registered_x0020_Owner' />
                                           <FieldRef Name='Zoning' />
                                           <FieldRef Name='Deed_x0020_No' />
                                           <FieldRef Name='ID' />
                                           <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                        </ViewFields>
                                        <QueryOptions />
                                        <RowLimit>{1}</RowLimit>
                                    </View>", neworupdate, maximumRows);

                        ListItemCollection items = list.GetItems(query);
                        context.Load(items);
                        context.ExecuteQuery();
                        valuations = MapRecords(items);
                    }
                    return valuations;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public List<ValuationData> GetRecords(string localAuthority, string localAuthorityCode, int maximumRows)
        {
            return GetVRecordsByState(localAuthority, localAuthorityCode, maximumRows, ObjectState.New);
        }
        public List<ValuationData> MapRecords(ListItemCollection items)
        {
            List<ValuationData> valData = new List<ValuationData>();
            ListItemCollection lists = items as ListItemCollection;
            ValuationData val;
            string standno = string.Empty;
            foreach (ListItem listitem in lists)
            {
                standno = Common.Common.ConvertTo<string>(listitem["Stand_x0020_No"]);
                if (!string.IsNullOrEmpty(standno))
                {
                    val = new ValuationData()
                    {
                        Date = Common.Common.ConvertTo<DateTime>(listitem["Date"]),
                        DeedNo = Common.Common.ConvertTo<string>(listitem["Deed_x0020_No"]),
                        DescriptionOfImprovement = Common.Common.ConvertTo<string>(listitem["Description_x0020_of_x0020_Impro"]),
                        ErfArea = Common.Common.ConvertTo<decimal>(listitem["Erf_x0020_Area"]),
                        ErfNo = Common.Common.ConvertTo<string>(listitem["Erf_x0020_No"]),
                        GroundValue = Common.Common.ConvertTo<decimal>(listitem["Ground_x0020_Value"]),
                        ImprovementValue = Common.Common.ConvertTo<decimal>(listitem["Improvement_x0020_Value"]),
                        LocalAuthority = Common.Common.ConvertTo<string>(listitem["Local_x0020_Authority"]),
                        Price = Common.Common.ConvertTo<decimal>(listitem["Price"]),
                        RegisteredOwner = Common.Common.ConvertTo<string>(listitem["Registered_x0020_Owner"]),
                        StandNo = Common.Common.ConvertTo<string>(listitem["Stand_x0020_No"]),
                        TotalValue = Common.Common.ConvertTo<decimal>(listitem["Total_x0020_Value"]),
                        Township = Common.Common.ConvertTo<string>(listitem["Township"]),
                        ValuationCode = Common.Common.ConvertTo<string>(listitem["Title"]),
                        Zoning = Common.Common.ConvertTo<string>(listitem["Zoning"]),
                        ID = Common.Common.ConvertTo<int>(listitem["ID"]),
                    };

                    int status = Common.Common.ConvertTo<int>(listitem["Update_x0020_to_x0020_Finance"]);
                    val.ItemState = status == 1 ? ObjectState.New
                                    : status == 2 ? ObjectState.Unchanged
                                    : status == 3 ? ObjectState.Update
                                    : status == 4 ? ObjectState.Delete
                                    : ObjectState.Ignore;
                    valData.Add(val);

                }
            }
            return valData;
        }
        public bool FlagSynchronizedRecords(string localAuthorityCode, int[] ids)
        {
            try
            {
                List<ValuationData> erfs = new List<ValuationData>();
                string siteUrl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthorityCode);

                System.Net.WebRequest request = System.Net.HttpWebRequest.Create(siteUrl);
                request.UseDefaultCredentials = true;
                request.PreAuthenticate = true;
                CredentialCache.DefaultNetworkCredentials.Domain = ConfigHelperWeb.GetSPDomain;
                CredentialCache.DefaultNetworkCredentials.UserName = ConfigHelperWeb.GetSPUsername;
                CredentialCache.DefaultNetworkCredentials.Password = ConfigHelperWeb.GetSPPassword;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                System.Net.WebResponse response = request.GetResponse();

                using (ClientContext context = new ClientContext(siteUrl))
                {
                    if (context != null)
                    {
                        context.Credentials = request.Credentials;

                        Web web = context.Web;
                        List list = web.Lists.GetByTitle("Valuation");

                        string queryString = @"
                            <View>
                                <Query>
                                    <Where>" +
                                            @"<In>
                                                <FieldRef Name='ID' />
                                                <Values>
                                                    {1}
                                                </Values>
                                            </In>" +
                                    @"</Where>
                                </Query>
                            </View>";

                        StringBuilder sb = new StringBuilder();
                        foreach (var id in ids)
                        {
                            sb.Append(string.Format(@"<Value Type='Counter'>{0}</Value>", id));
                        }
                        queryString = string.Format(queryString, sb.ToString());

                        CamlQuery query = new CamlQuery();
                        query.ViewXml = queryString;

                        ListItemCollection listItems = list.GetItems(query);
                        context.Load(listItems);
                        context.ExecuteQuery();

                        if (listItems.Count > 0)
                        {
                            foreach (var item in listItems)
                            {
                                item["Update_x0020_to_x0020_Finance"] = 2;
                                item.Update();
                            }
                            context.ExecuteQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }

        #endregion
    }
}