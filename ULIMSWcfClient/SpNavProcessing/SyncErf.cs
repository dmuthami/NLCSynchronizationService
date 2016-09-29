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
using ULIMSWcfClient.svcNavErfs;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class SyncErf : ISyncSpToNav<ErfDat>
    {
        #region Navision Data Execution Code
        public SyncErf(string localAuthority, string localauthcode)
        {
            LocalAuthority = localAuthority;
            LocalAuthorityCode = localauthcode;
        }
        public SyncErf(string localAuthority, CrudState state)
        {
            LocalAuthority = localAuthority;
            CurrentCrudState = state;
        }
        public string LocalAuthority { get; set; }
        public string LocalAuthorityCode { get; set; }
        public CrudState CurrentCrudState { get; set; }
        public async Task Synchronize(CrudState state)
        {
            Console.WriteLine(string.Format("Synchronize Erf:{0}", state == CrudState.Edited ? "Edit" : state == CrudState.New ? "New" : "Synchronized"));
            CurrentCrudState = state;
            try
            {
                bool noMoreRecords = false;
                //int index = 1;
                do
                {
                    ErfDat[] getErfsTask = GetUnsynchronizedItems();
                    ErfDat[] erfs = getErfsTask;
                    if (erfs == null)
                        break;

                    if (erfs.Count() < ConfigHelperWeb.GetPageSize)
                    {
                        noMoreRecords = true;
                        if (erfs.Count() == 0)
                            break;
                    }

                    Task<bool> sendItemsTask = SendUnsynchronizedItems(erfs);
                    bool isSendSuccess = await sendItemsTask;
                    if (isSendSuccess)
                    {
                        bool flagItemsTask = FlagSynchronizedItems(erfs);
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
        public ErfDat[] GetUnsynchronizedItems()
        {
            try
            {
                int pageSize = ConfigHelperWeb.GetPageSize;

                ObjectState state = CurrentCrudState == CrudState.New ? ObjectState.New
                                            : CurrentCrudState == CrudState.Edited ? ObjectState.Update
                                            : CurrentCrudState == CrudState.Synchonized ? ObjectState.Unchanged
                                            : ObjectState.Ignore;

                var erflist = GetRecordsByState(LocalAuthority, LocalAuthorityCode, pageSize, state).ToArray();

                ErfDat[] erfs = erflist;

                return erfs;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Erfs\n Function: {0}\nError:{1}\n", "GetUnsynchronizedItems", ex.Message));

                return null;
            }
        }
        public bool FlagSynchronizedItems(ErfDat[] items)
        {
            try
            {
                List<int> ids = new List<int>();
                foreach (var erf in items)
                    ids.Add(erf.ID);

                bool syncrhonizeTask = FlagSynchronizedRecords(LocalAuthorityCode, ids.ToArray());
                bool isErfFlagged = syncrhonizeTask;
                return isErfFlagged;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Erfs\n Function: {0}\nError:{1}\n", "FlagSynchronizedItems", ex.Message));

                return false;
            }
        }
        public async Task<bool> SendUnsynchronizedItems(ErfDat[] items)
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

            string endpointname = "Erfs_Port";
            string wcfaddress = ConfigHelperWeb.GetClientAddressByName(endpointname);
            wcfaddress = wcfaddress.Replace("ReplaceWithAPercentEncodedCompanyName", Uri.EscapeDataString(LocalAuthority));

            Erfs_PortClient client = new Erfs_PortClient(navWSBinding, new EndpointAddress(wcfaddress));

            client.ClientCredentials.Windows.AllowedImpersonationLevel =
                System.Security.Principal.TokenImpersonationLevel.Delegation;

            client.ClientCredentials.Windows.ClientCredential = _cred;

            try
            {
                Erfs[] erfs = MapApplicationPurchase(items);

                if (CurrentCrudState == CrudState.New)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Erfs_Filter[] { new Erfs_Filter { Field = Erfs_Fields.Stand_No, Criteria = criteria } };
                    request.bookmarkKey = null;
                    request.setSize = items.Count();

                    ReadMultiple_Result readResult = await client.ReadMultipleAsync(request.filter, request.bookmarkKey, request.setSize);
                    var readTask = readResult.ReadMultiple_Result1;

                    if (readTask.Count() > 0)
                    {
                        foreach (var item in readTask)
                        {
                            var exists = readTask.FirstOrDefault(o => o.Stand_No == item.Stand_No);
                            var getStan = (exists != null) ? exists.Stand_No : null;

                            if (item.Stand_No == getStan)
                            {
                                Update _upD = new Update();
                                _upD.Erfs = exists;
                                Update_Result upTask = await client.UpdateAsync(_upD);
                            }
                            else if (item.Stand_No != getStan)
                            {
                                Create _create = new Create();
                                _create.Erfs = item;

                                Create_Result crTask = await client.CreateAsync(_create);
                            }
                        }
                    }
                    else
                    {
                        CreateMultiple createMultiple = new CreateMultiple();
                        createMultiple.Erfs_List = erfs;
                        CreateMultiple_Result resultsTask = await client.CreateMultipleAsync(createMultiple);
                        var result = resultsTask;
                        erfs = result.Erfs_List;
                    }
                }
                else if (CurrentCrudState == CrudState.Edited)
                {
                    string criteria = GetFilter(items);
                    ReadMultiple request = new ReadMultiple();
                    request.filter = new Erfs_Filter[] { new Erfs_Filter { Field = Erfs_Fields.Stand_No, Criteria = criteria } };
                    request.bookmarkKey = null;
                    request.setSize = items.Count();

                    ReadMultiple_Result resultTask = await client.ReadMultipleAsync(request.filter, request.bookmarkKey, request.setSize);
                    Erfs[] erfsToUpdate = resultTask.ReadMultiple_Result1;

                    if (erfsToUpdate.Count() > 0)
                    {
                        UpdateMultiple updateMultiple = new UpdateMultiple();
                        Erfs[] editedErfs = GetEditedErfs(erfs, erfsToUpdate);
                        updateMultiple.Erfs_List = editedErfs;
                        Task<UpdateMultiple_Result> updateResultsTask = client.UpdateMultipleAsync(updateMultiple);
                        UpdateMultiple_Result updateResult = await updateResultsTask;
                        erfs = updateResult.Erfs_List;
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
        private string GetFilter(ErfDat[] items)
        {
            StringBuilder sb = new StringBuilder();
            int cnt = 0;
            string appendor = string.Empty;
            foreach (ErfDat ed in items)
            {
                appendor = cnt++ == 0 ? "" : "|";
                sb.Append(string.Format("{0}={1}", appendor, ed.StandNo));
            }
            return sb.ToString();
        }
        private Erfs[] GetEditedErfs(Erfs[] erfs, Erfs[] erfsToUpdate)
        {
            int cnt = erfsToUpdate.Count();

            List<Erfs> lerfs = erfs.ToList();
            Erfs e, eu;
            for (int i = 0; i < cnt; i++)
            {
                eu = erfsToUpdate[i];
                e = lerfs.FirstOrDefault(l => string.Compare(l.Stand_No, eu.Stand_No) == 0);
                if (e != null)
                {
                    erfsToUpdate[i].Size = e.Size;
                    erfsToUpdate[i].SizeSpecified = e.SizeSpecified;
                    erfsToUpdate[i].Street_Address = e.Street_Address;
                    erfsToUpdate[i].Township = e.Township;
                    erfsToUpdate[i].Zoning = e.Township;
                }
            }

            return erfsToUpdate;
        }
        private Erfs[] MapApplicationPurchase(ErfDat[] items)
        {
            List<Erfs> erfs = new List<Erfs>();
            Erfs erf;
            foreach (ErfDat e in items)
            {
                erf = new Erfs()
                {
                    Erf_No = e.ErfNo,
                    Stand_No = e.StandNo,
                    Township = e.Township,
                    Zoning = e.Zoning,
                    Size = Common.Common.ConvertTo<decimal>(e.SurveySize),
                    SizeSpecified = true
                };
                erfs.Add(erf);
            }
            return erfs.ToArray();
        }

        #endregion

        #region Sharepoint Data Access Code
        public List<ErfDat> GetRecordsByState(string localAuthority, string localAuthorityCode, int maximumRows, ObjectState state)
        {
            try
            {
                List<ErfDat> erfs = new List<ErfDat>();
                string siteurl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthorityCode);

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
                        List list = web.Lists.GetByTitle("Erf");

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
                                           <FieldRef Name='ErfNo' />
                                           <FieldRef Name='Local_x0020_Authority' />
                                           <FieldRef Name='Township' />
                                           <FieldRef Name='Zoning' />
                                           <FieldRef Name='Survey_x0020_Size' />
                                           <FieldRef Name='Computed_x0020_Size' />
                                           <FieldRef Name='Ownership' />
                                           <FieldRef Name='Density' />
                                           <FieldRef Name='ID' />
                                           <FieldRef Name='Title' />
                                            <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                        </ViewFields>
                                        <QueryOptions />
                                        <RowLimit>{1}</RowLimit>
                                    </View>", neworupdate, maximumRows);


                        ListItemCollection items = list.GetItems(query);

                        context.Load(items);
                        context.ExecuteQuery();

                        erfs = MapRecords(items);
                    }
                    return erfs;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public List<ErfDat> GetRecords(string localAuthority, string localAuthorityCode, int maximumRows)
        {
            return GetRecordsByState(localAuthority, localAuthorityCode, maximumRows, ObjectState.New);
        }
        public List<ErfDat> MapRecords(ListItemCollection items)
        {
            List<ErfDat> erfData = new List<ErfDat>();
            ListItemCollection lists = items;
            ErfDat erf;
            //int i = 0;
            foreach (ListItem listitem in lists)
            {
                erf = new ErfDat()
                {
                    ErfNo = Common.Common.ConvertTo<string>(listitem["ErfNo"]),
                    LocalAuthority = Common.Common.ConvertTo<string>(listitem["Local_x0020_Authority"]),
                    Township = Common.Common.ConvertTo<string>(listitem["Township"]),
                    Zoning = Common.Common.ConvertTo<string>(listitem["Zoning"]),
                    StandNo = Common.Common.ConvertTo<string>(listitem["Title"]),
                    SurveySize = Common.Common.ConvertTo<decimal>(listitem["Survey_x0020_Size"]),
                    ComputedSize = Common.Common.ConvertTo<decimal>(listitem["Computed_x0020_Size"]),
                    Ownership = Common.Common.ConvertTo<string>(listitem["Ownership"]),
                    Density = Common.Common.ConvertTo<string>(listitem["Density"]),
                    ID = Common.Common.ConvertTo<int>(listitem["ID"])
                };

                int status = Common.Common.ConvertTo<int>(listitem["Update_x0020_to_x0020_Finance"]);
                erf.ItemState = status == 1 ? ObjectState.New
                                : status == 2 ? ObjectState.Unchanged
                                : status == 3 ? ObjectState.Update
                                : status == 4 ? ObjectState.Delete
                                : ObjectState.Ignore;

                erfData.Add(erf);
            }
            return erfData;
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
                        List list = web.Lists.GetByTitle("Erf");

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
        #endregion
    }
}