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
using ULIMSWcfClient.svcNavValuationPeriods;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class SyncValuationPeriod : ISyncVpSpToNav<ValuationPeriodData>
    {
        #region Navision Data Execution Code
        public SyncValuationPeriod(string localAuthority, string localauthcode)
        {
            LocalAuthority = localAuthority;
            LocalAuthorityCode = localauthcode;
        }
        public SyncValuationPeriod(string localAuthority, CrudState state)
        {
            LocalAuthority = localAuthority;
            CurrentCrudState = state;
        }
        public string LocalAuthority { get; set; }
        public string LocalAuthorityCode { get; set; }
        public CrudState CurrentCrudState { get; set; }
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
        public async Task Synchronize(CrudState state)
        {
            Console.WriteLine(string.Format("Synchronize Valuation period:{0}", state == CrudState.Edited ? "Edit" : state == CrudState.New ? "New" : "Synchronized"));
            CurrentCrudState = state;
            try
            {
                bool noMoreRecords = false;
                //int index = 1;
                do
                {
                    ValuationPeriodData[] getValPeriodsTask = GetUnsynchronizedItems();
                    ValuationPeriodData[] valuationperiods = getValPeriodsTask;
                    if (valuationperiods == null)
                        break;

                    if (valuationperiods.Count() < ConfigHelperWeb.GetPageSize)
                    {
                        noMoreRecords = true;
                        if (valuationperiods.Count() == 0)
                            break;
                    }

                    Task<bool> sendItemsTask = SendUnsynchronizedItems(valuationperiods);
                    bool isSendSuccess = await sendItemsTask;
                    if (isSendSuccess)
                    {
                        bool flagItemsTask = FlagSynchronizedItems(valuationperiods);
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
                Console.Write(string.Format("Item: Erfs\n Function: {0}\nError:{1}\n", "Synchronize", ex.Message));

                //pass
            }
        }
        public ValuationPeriodData[] GetUnsynchronizedItems()
        {
            try
            {
                int pageSize = ConfigHelperWeb.GetPageSize;

                ObjectState state = CurrentCrudState == CrudState.New ? ObjectState.New
                                        : CurrentCrudState == CrudState.Edited ? ObjectState.Update
                                        : CurrentCrudState == CrudState.Synchonized ? ObjectState.Unchanged
                                        : ObjectState.Ignore;

                var valPeriodData = GetVpRecordsByState(LocalAuthority, LocalAuthorityCode, pageSize, state).ToArray();
                ValuationPeriodData[] valuationperiods = valPeriodData;
                return valuationperiods;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Erfs\n Function: {0}\nError:{1}\n", "GetUnsynchronizedItems", ex.Message));

                return null;
            }
        }
        public bool FlagSynchronizedItems(ValuationPeriodData[] items)
        {
            try
            {
                List<int> ids = new List<int>();
                foreach (var erf in items)
                    ids.Add(erf.ID);

                bool syncrhonizeTask = FlagSynchronizedRecords(LocalAuthorityCode, ids.ToArray());
                bool isValuationPeriodFlagged = syncrhonizeTask;
                return isValuationPeriodFlagged;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Valuation\n Function: {0}\nError:{1}\n", "FlagSynchronizedItems", ex.Message));

                return false;
            }
        }
        public async Task<bool> SendUnsynchronizedItems(ValuationPeriodData[] items)
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

            string endpointname = "Valuationperiods_Port";
            string wcfaddress = ConfigHelperWeb.GetClientAddressByName(endpointname);
            wcfaddress = wcfaddress.Replace("ReplaceWithAPercentEncodedCompanyName", Uri.EscapeDataString(LocalAuthority));

            Valuationperiods_PortClient client = new Valuationperiods_PortClient(navWSBinding, new EndpointAddress(wcfaddress));

            client.ClientCredentials.Windows.AllowedImpersonationLevel =
                System.Security.Principal.TokenImpersonationLevel.Delegation;

            client.ClientCredentials.Windows.ClientCredential = _cred;

            try
            {
                Valuationperiods[] valuationperiods = MapValuationPeriods(items);

                if (CurrentCrudState == CrudState.New)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Valuationperiods_Filter[] { new Valuationperiods_Filter { Field = Valuationperiods_Fields.Request_ID, Criteria = criteria } };
                    request.bookmarkKey = null;
                    request.setSize = items.Count();

                    ReadMultiple_Result readResult = await client.ReadMultipleAsync(request.filter, null, request.setSize);
                    var readTask = readResult.ReadMultiple_Result1;

                    if (readTask.Count() > 0)
                    {
                        foreach (var item in readTask)
                        {
                            var exists = readTask.FirstOrDefault(o => o.Request_ID == item.Request_ID);
                            var getRID = (exists != null) ? exists.Request_ID : null;

                            if (item.Request_ID == getRID)
                            {
                                Update _update = new Update();
                                _update.Valuationperiods = exists;

                                Update_Result upTask = await client.UpdateAsync(_update);
                            }
                            else if (item.Request_ID != getRID)
                            {
                                Create _create = new Create();
                                _create.Valuationperiods = item;

                                Create_Result crTask = await client.CreateAsync(_create);
                            }
                        }
                    }
                    else
                    {
                        CreateMultiple createMultiple = new CreateMultiple();
                        createMultiple.Valuationperiods_List = valuationperiods;
                        Task<CreateMultiple_Result> resultsTask = client.CreateMultipleAsync(createMultiple);
                        CreateMultiple_Result result = await resultsTask;
                        valuationperiods = result.Valuationperiods_List;
                    }
                }
                else if (CurrentCrudState == CrudState.Edited)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Valuationperiods_Filter[] { new Valuationperiods_Filter { Field = Valuationperiods_Fields.Request_ID, Criteria = criteria } };
                    request.bookmarkKey = Valuationperiods_Fields.Request_ID.ToString();
                    request.setSize = items.Count();

                    ReadMultiple_Result result = await client.ReadMultipleAsync(request.filter, request.bookmarkKey, request.setSize);
                    Valuationperiods[] valPeriodsToUpdate = result.ReadMultiple_Result1;

                    if (valPeriodsToUpdate.Length > 0)
                    {
                        UpdateMultiple updateMultiple = new UpdateMultiple();
                        Valuationperiods[] editedValuationperiods = GetEditedErfs(valuationperiods, valPeriodsToUpdate);
                        updateMultiple.Valuationperiods_List = editedValuationperiods;
                        Task<UpdateMultiple_Result> updateResultsTask = client.UpdateMultipleAsync(updateMultiple);
                        UpdateMultiple_Result updateResult = await updateResultsTask;
                        valuationperiods = updateResult.Valuationperiods_List;
                    }
                }

                ((ICommunicationObject)client).Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Erfs\n Function: {0}\nError:{1}\n", "SendUnsynchronizedItems", ex.Message));

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
        private Valuationperiods[] GetEditedErfs(Valuationperiods[] valuationperiods, Valuationperiods[] valPeriodsToUpdate)
        {
            int cnt = valPeriodsToUpdate.Count();

            List<Valuationperiods> lerfs = valuationperiods.ToList();
            Valuationperiods e, eu;
            for (int i = 0; i < cnt; i++)
            {
                eu = valPeriodsToUpdate[i];
                e = lerfs.FirstOrDefault(l => string.Compare(l.Request_ID, eu.Request_ID) == 0);
                if (e != null)
                {
                    valPeriodsToUpdate[i].Period_Name = e.Period_Name;
                    valPeriodsToUpdate[i].Request_ID = e.Request_ID;
                    valPeriodsToUpdate[i].Valuation_Type = e.Valuation_Type;
                    valPeriodsToUpdate[i].Key = e.Key;
                }
            }

            return valPeriodsToUpdate;
        }
        private string GetFilter(ValuationPeriodData[] items)
        {
            StringBuilder sb = new StringBuilder();
            int cnt = 0;
            string appendor = string.Empty;
            foreach (ValuationPeriodData vpd in items)
            {
                appendor = cnt++ == 0 ? "" : "|";
                sb.Append(string.Format("{0}={1}", appendor, vpd.ID));
            }
            return sb.ToString();
        }
        private Valuationperiods[] MapValuationPeriods(ValuationPeriodData[] items)
        {
            List<Valuationperiods> vperiods = new List<Valuationperiods>();
            Valuationperiods valuationperiod;
            foreach (ValuationPeriodData vperiod in items)
            {
                valuationperiod = new Valuationperiods()
                {
                    Key = Convert.ToString(vperiod.ID),
                    Period_Name = vperiod.PeriodName,
                    Request_ID = vperiod.RequestID,
                    Valuation_Type = vperiod.ValuationType
                };
                vperiods.Add(valuationperiod);
            }

            return vperiods.ToArray();
        }
        #endregion

        #region SharePoint Data Access Code

        public List<ValuationPeriodData> GetVpRecordsByState(string localAuthority, string localAuthCode, int maximumRows, ObjectState state)
        {
            try
            {
                List<ValuationPeriodData> valuationperiods = new List<ValuationPeriodData>();
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
                        List list = web.Lists.GetByTitle("Valuation Period");

                        CamlQuery query = new CamlQuery();
                        string neworupdate = state == ObjectState.New ? "1"
                                            : state == ObjectState.Update ? "3"
                                            : "0";
                        query.ViewXml = string.Format(@"
                                    <View>
                                        <Query>
                                           <Where>" +
                                                 @"<Eq>
                                                     <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                                     <Value Type='Number'>{0}</Value>
                                                  </Eq>" +
                                           @"</Where>
                                        </Query>
                                        <ViewFields>
                                           <FieldRef Name='ID' />
                                           <FieldRef Name='Title' />
                                           <FieldRef Name='RequestID' />
                                           <FieldRef Name='Valuation_x0020_Type' />
                                            <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                        </ViewFields>
                                        <QueryOptions />
                                        <RowLimit>{1}</RowLimit>
                                    </View>", neworupdate, maximumRows);


                        ListItemCollection items = list.GetItems(query);

                        context.Load(items);
                        context.ExecuteQuery();

                        valuationperiods = MapRecords(items);
                    }
                    return valuationperiods;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public List<ValuationPeriodData> MapRecords(Microsoft.SharePoint.Client.ListItemCollection items)
        {
            List<ValuationPeriodData> valData = new List<ValuationPeriodData>();
            ListItemCollection lists = items as ListItemCollection;
            ValuationPeriodData val;
            string standno = string.Empty;
            foreach (ListItem listitem in lists)
            {
                val = new ValuationPeriodData()
                {
                    ID = Common.Common.ConvertTo<int>(listitem["ID"]),
                    PeriodName = Common.Common.ConvertTo<string>(listitem["Title"]),
                    RequestID = Common.Common.ConvertTo<string>(listitem["RequestID"]),
                    ValuationType = Common.Common.ConvertTo<string>(listitem["Valuation_x0020_Type"]),
                };

                int status = Common.Common.ConvertTo<int>(listitem["Update_x0020_to_x0020_Finance"]);
                val.ItemState = status == 1 ? ObjectState.New
                                : status == 2 ? ObjectState.Unchanged
                                : status == 3 ? ObjectState.Update
                                : status == 4 ? ObjectState.Delete
                                : ObjectState.Ignore;
                valData.Add(val);
            }

            return valData;
        }
        public bool FlagSynchronizedRecords(string localAuthorityCode, int[] ids)
        {
            try
            {
                List<ErfDat> erfs = new List<ErfDat>();
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
                        List list = web.Lists.GetByTitle("Valuation Period");

                        string queryString = @"
                            <View>
                                <Query>
                                    <Where>" +
                                            @"<In>
                                                <FieldRef Name='ID' />
                                                <Values>
                                                    {0}
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
        public List<ValuationPeriodData> GetRecords(string localAuthority, string localAuthCode, int maximumRows)
        {
            return GetVpRecordsByState(localAuthority, localAuthCode, maximumRows, ObjectState.New);
        }

        #endregion
    }
}