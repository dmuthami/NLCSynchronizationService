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
using ULIMSWcfClient.svcNavCharges;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class SyncCharges
    {
        #region Navision Data Execution Code
        public SyncCharges(string localAuthority, string localAuthCode)
        {
            this.LocalAuthority = localAuthority;
            this.LocalAuthorityCode = localAuthCode;
        }
        public string LocalAuthority { get; set; }
        public string LocalAuthorityCode { get; set; }
        public async Task Synchronize(ChargesData[] chargesData)
        {
            try
            {
                bool noMoreRecords = false;
                int index = 0;
                ChargesData[] recsToSend;
                do
                {
                    recsToSend = chargesData
                        .Skip(index)
                        .Take(ConfigHelperWeb.GetPageSize)
                        .ToArray();

                    if (recsToSend.Count() < ConfigHelperWeb.GetPageSize)
                    {
                        noMoreRecords = true;
                        if (recsToSend.Count() == 0)
                            break;
                    }

                    Task<bool> sendItemsTask = SendUnsynchronizedItems(recsToSend);
                    bool isSendSuccess = await sendItemsTask;
                    if (isSendSuccess)
                    {
                        bool flagItemsTask = FlagSynchronizedItems(recsToSend);
                        bool isFlagSuccess = flagItemsTask;
                    }
                    index += ConfigHelper.GetPageSize;
                } while (!noMoreRecords);
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public bool FlagSynchronizedItems(ChargesData[] items)
        {
            try
            {
                int[] ids = items.Select(c => c.ID).ToArray();

                bool syncrhonizeTask = FlagSynchronizedRecords(LocalAuthorityCode, ids);
                bool synched = syncrhonizeTask;
                return true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> SendUnsynchronizedItems(ChargesData[] items)
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

            string endpointname = "Charges_Port";
            string wcfaddress = ConfigHelperWeb.GetClientAddressByName(endpointname);
            wcfaddress = wcfaddress.Replace("ReplaceWithAPercentEncodedCompanyName", Uri.EscapeDataString(LocalAuthority));

            Charges_PortClient client = new Charges_PortClient(navWSBinding, new EndpointAddress(wcfaddress));

            client.ClientCredentials.Windows.AllowedImpersonationLevel =
                System.Security.Principal.TokenImpersonationLevel.Delegation;

            client.ClientCredentials.Windows.ClientCredential = _cred;

            try
            {
                CreateMultiple createMultiple = new CreateMultiple();
                Charges[] charges = MapCharges(items);
                createMultiple.Charges_List = charges;
                Task<CreateMultiple_Result> resultsTask = client.CreateMultipleAsync(createMultiple);

                CreateMultiple_Result result = await resultsTask;
                charges = result.Charges_List;

                ((ICommunicationObject)client).Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
                throw ex;
            }
            finally
            {
                if (((ICommunicationObject)client).State != CommunicationState.Closed)
                {
                    ((ICommunicationObject)client).Abort();
                }
            }
        }
        private Charges[] MapCharges(ChargesData[] chargesData)
        {
            List<Charges> chargesList = new List<Charges>();

            Charges charge;
            foreach (ChargesData cdata in chargesData)
            {
                charge = new Charges()
                {

                    Stand_No = Convert.ToString(cdata.StandNo),
                    //Charge_Type = Convert.ToString(cdata.ChargeType),
                    Amount_Exclusive_VAT = Convert.ToDecimal(cdata.Amount),
                    Amount_Exclusive_VATSpecified = true,
                    Application_ID = cdata.ApplicationID,
                    Description = cdata.Description
                };
                chargesList.Add(charge);
            }
            return chargesList.ToArray();
        }
        #endregion

        #region Sharepoint Data Access Code

        public List<ChargesData> GetRecords(string localAuthority, string localAuthCode, int maximumRows)
        {
            try
            {
                List<ChargesData> chargesData = new List<ChargesData>();
                string siteUrl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthCode);

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
                        List list = web.Lists.GetByTitle("Applied Charges-Application");

                        CamlQuery query = new CamlQuery();
                        query.ViewXml = string.Format(@"
                                    <View>
	                                    <Query>
		                                    <Where>" +
                                                    @"<Eq>
                                                        <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                                        <Value Type='Number'>0</Value>
                                                    </Eq>" +
                                            @"</Where>
	                                    </Query>
	                                    <ViewFields> 
                                            <FieldRef Name='Title' />
                                            <FieldRef Name='Charge_x0020_Type' />
                                            <FieldRef Name='Charge_x0020_Type_x003a_ID' />
                                            <FieldRef Name='ID' />
                                            <FieldRef Name='Stand_x0020_No' />
                                            <FieldRef Name='Local_x0020_Authority' />
                                            <FieldRef Name='Amount' />
                                            <FieldRef Name='Application_x0020_GUID' />
                                            <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                            <FieldRef Name='Description' />
	                                    </ViewFields>
	                                    <QueryOptions />
                                        <RowLimit>{0}</RowLimit>
                                    </View>", maximumRows);

                        ListItemCollection items = list.GetItems(query);
                        context.Load(items);
                        context.ExecuteQuery();
                        var erfapplications = MapRecords(items);

                        return erfapplications;
                    }
                    else
                        return chargesData;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public List<ChargesData> MapRecords(ListItemCollection items)
        {
            List<ChargesData> chargesData = new List<ChargesData>();
            ListItemCollection lists = items as ListItemCollection;
            ChargesData chargeData;
            foreach (ListItem listitem in lists)
            {
                chargeData = new ChargesData()
                {
                    Amount = Common.Common.ConvertTo<decimal>(listitem["Amount"]),
                    ApplicationGuid = Common.Common.ConvertTo<string>(listitem["Application_x0020_GUID"]),
                    ApplicationID = Common.Common.ConvertTo<string>(listitem["Title"]),
                    ChargeType = Common.Common.ConvertTo<string>(listitem["Charge_x0020_Type"]),
                    ChargeTypeID = Common.Common.ConvertTo<int>(listitem["Charge_x0020_Type_x003a_ID"]),
                    ID = Common.Common.ConvertTo<int>(listitem["ID"]),
                    LocalAuthority = Common.Common.ConvertTo<string>(listitem["Local_x0020_Authority"]),
                    StandNo = Common.Common.ConvertTo<string>(listitem["Stand_x0020_No"]),
                    Description = Common.Common.ConvertTo<string>(listitem["Description"]),
                };

                int status = Common.Common.ConvertTo<int>(listitem["Update_x0020_to_x0020_Finance"]);
                chargeData.ItemState = status == 1 ? ObjectState.New
                                     : status == 2 ? ObjectState.Unchanged
                                     : status == 3 ? ObjectState.Update
                                     : status == 4 ? ObjectState.Delete
                                     : ObjectState.Ignore;
                chargesData.Add(chargeData);
            }
            return chargesData;
        }
        public bool FlagSynchronizedRecords(string localAuthCode, int[] ids)
        {
            try
            {
                List<ChargesData> chargesDatas = new List<ChargesData>();
                string siteUrl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthCode);

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
                        List list = web.Lists.GetByTitle("Applied Charges-Application");

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

                        foreach (var item in listItems)
                        {
                            item["Update_x0020_to_x0020_Finance"] = 2;
                            item.Update();
                        }
                        context.ExecuteQuery();
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