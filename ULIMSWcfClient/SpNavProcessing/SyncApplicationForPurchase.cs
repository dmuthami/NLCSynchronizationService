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
using ULIMSWcfClient.svcNavApplicationForPurchase;

namespace ULIMSWcfClient.SpNavProcessing
{
    public class SyncApplicationForPurchase : ISyncASpToNav<ErfApplicationData>
    {
        #region Navision Data Execution Code
        public SyncApplicationForPurchase()
        {
        }
        public SyncApplicationForPurchase(string localAuthority, string localAuthCode)
        {
            LocalAuthority = localAuthority;
            LocalAuthorityCode = localAuthCode;
        }
        public string LocalAuthority { get; set; }
        public string LocalAuthorityCode { get; set; }
        public async Task Synchronize()
        {
            Console.WriteLine("Sync Application Purchase");
            try
            {
                bool noMoreRecords = false;

                do
                {
                    ErfApplicationData[] getErfApplicationsTask = GetUnsynchronizedItems();
                    ErfApplicationData[] erfApplications = getErfApplicationsTask;
                    if (erfApplications.Count() < ConfigHelperWeb.GetPageSize)
                    {
                        noMoreRecords = true;
                        if (erfApplications.Count() == 0)
                            break;
                    }

                    Task<bool> sendItemsTask = SendUnsynchronizedItems(erfApplications);
                    bool isSendSuccess = await sendItemsTask;
                    if (isSendSuccess)
                    {
                        //Synchronize the charges then flag synchronized erfapplications
                        SyncCharges chargesSync = new SyncCharges(LocalAuthority, LocalAuthorityCode);
                        //ChargesData[] chargesData = erfApplications
                        //                                 .SelectMany(e => e.ErfCharges)
                        //                                 .ToArray();
                        List<ChargesData> chargesData = new List<ChargesData>();


                        foreach (var erfApp in erfApplications)
                        {
                            if (erfApp.ErfCharges != null)
                            {
                                foreach (var cd in erfApp.ErfCharges)
                                    chargesData.Add(cd);
                            }
                        }

                        await chargesSync.Synchronize(chargesData.ToArray());

                        bool flagItemsTask = FlagSynchronizedItems(erfApplications);
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
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public async Task<bool> SendUnsynchronizedItems(ErfApplicationData[] erfApplications)
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

            string endpointname = "Application_For_Purchase_Port";
            string wcfaddress = ConfigHelperWeb.GetClientAddressByName(endpointname);
            wcfaddress = wcfaddress.Replace("ReplaceWithAPercentEncodedCompanyName", Uri.EscapeDataString(LocalAuthority));

            Application_For_Purchase_PortClient client = new Application_For_Purchase_PortClient(navWSBinding, new EndpointAddress(wcfaddress));

            client.ClientCredentials.Windows.AllowedImpersonationLevel =
                System.Security.Principal.TokenImpersonationLevel.Delegation;

            client.ClientCredentials.Windows.ClientCredential = _cred;

            try
            {
                CreateMultiple createMultiple = new CreateMultiple();
                Application_For_Purchase[] appPurchases = MapApplicationPurchase(erfApplications);
                createMultiple.Application_For_Purchase_List = appPurchases;
                Task<CreateMultiple_Result> resultsTask = client.CreateMultipleAsync(createMultiple);

                CreateMultiple_Result result = await resultsTask;
                appPurchases = result.Application_For_Purchase_List;

                ((ICommunicationObject)client).Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("Item: Application Purchase\n Function: {0}\nError:{1}\n", "SendUnsynchronizedItems", ex.Message));
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
        public ErfApplicationData[] GetUnsynchronizedItems()
        {
            try
            {
                int pageSize = ConfigHelperWeb.GetPageSize;

                ErfApplicationData[] erfdata = GetErfApplicationWithCharges(LocalAuthority, LocalAuthorityCode, pageSize).ToArray();
                ErfApplicationData[] erfApplications = erfdata;
                return erfApplications;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool FlagSynchronizedItems(ErfApplicationData[] erfApplications)
        {
            try
            {
                List<int> ids = new List<int>();
                foreach (var erfApp in erfApplications)
                    ids.Add(erfApp.ID);

                bool syncrhonizeTask = FlagSynchronizedRecords(LocalAuthorityCode, ids.ToArray());
                bool synched = syncrhonizeTask;
                return true;
            }
            catch (FaultException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private Application_For_Purchase[] MapApplicationPurchase(ErfApplicationData[] spAppPurchasesArray)
        {
            List<Application_For_Purchase> navAppPurchases = new List<Application_For_Purchase>();

            Application_For_Purchase navAppPurchase;
            foreach (ErfApplicationData ap in spAppPurchasesArray)
            {
                navAppPurchase = new Application_For_Purchase();

                navAppPurchase.ID_No = ap.ApplicantIdNumber;
                navAppPurchase.Applicant_Name = ap.ApplicantName;
                navAppPurchase.Phone_Nod = ap.ApplicantPhoneNumber;
                navAppPurchase.Physical_Address = ap.ApplicantPhysicalAddress;
                navAppPurchase.Postal_Address = ap.ApplicantPostalAddress;
                navAppPurchase.Application_ID = ap.ApplicationID;
                navAppPurchase.Application_Type = ap.ApplicationType;
                navAppPurchase.Email = ap.Email;
                navAppPurchase.Stand_No = ap.StandNo;
                navAppPurchase.User_ID = ap.UserID;
                navAppPurchase.Fax = ap.ApplicantFax;
                navAppPurchase.Passport_No = ap.ApplicantPassportNumber;
                navAppPurchase.Website = ap.ApplicantWebsite;
                navAppPurchase.Minimum_Building_Value = ap.MinimumBuildingValue;
                navAppPurchase.Minimum_Building_ValueSpecified = true;
                navAppPurchase.Purchase_Price = Convert.ToDecimal(ap.PurchasePrice);
                navAppPurchase.Purchase_PriceSpecified = true;
                navAppPurchase.Deposit = Convert.ToDecimal(ap.Deposit);
                navAppPurchase.DepositSpecified = true;
                navAppPurchase.Interest_Rate = Convert.ToInt32(ap.InterestRate);
                navAppPurchase.Interest_RateSpecified = true;

                if (DateTime.Compare(ap.EndDate, DateTime.MinValue) == 0)
                    navAppPurchase.End_DateSpecified = false;
                else
                {
                    navAppPurchase.End_Date = Convert.ToDateTime(ap.EndDate);
                    navAppPurchase.End_DateSpecified = true;
                }

                if (DateTime.Compare(ap.StartDate, DateTime.MinValue) == 0)
                    navAppPurchase.Start_DateSpecified = false;
                else
                {
                    navAppPurchase.Start_Date = Convert.ToDateTime(ap.StartDate);
                    navAppPurchase.Start_DateSpecified = true;
                }

                string paymentMethod = ap.MethodOfPayment;
                if (string.IsNullOrEmpty(paymentMethod))
                    navAppPurchase.Payment_Method = Payment_Method._blank_;
                if (paymentMethod == "Installment")
                    navAppPurchase.Payment_Method = Payment_Method.Installment;
                else if (paymentMethod == "Upfront")
                    navAppPurchase.Payment_Method = Payment_Method.Cash;
                navAppPurchase.Payment_MethodSpecified = true;

                if (DateTime.Compare(ap.InstallmentStartDate, DateTime.MinValue) == 0)
                    navAppPurchase.Installment_StartSpecified = false;
                else
                {
                    navAppPurchase.Installment_Start = Convert.ToDateTime(ap.InstallmentStartDate);
                    navAppPurchase.Installment_StartSpecified = true;
                }

                if (DateTime.Compare(ap.PurchaseDate, DateTime.MinValue) == 0)
                    navAppPurchase.Purchase_DateSpecified = false;
                else
                {
                    navAppPurchase.Purchase_Date = Convert.ToDateTime(ap.PurchaseDate);
                    navAppPurchase.Purchase_DateSpecified = true;
                }

                navAppPurchases.Add(navAppPurchase);
            }
            return navAppPurchases.ToArray();
        }
        #endregion

        #region SharePoint Data Access Code

        public List<ErfApplicationData> GetErfApplicationWithCharges(string localAuthority, string localAuthCode, int maximumRows)
        {
            List<ErfApplicationData> erfApplications = GetRecords(localAuthority, localAuthCode, maximumRows);
            string siteUrl = ConfigHelperWeb.GetSiteUrlByLocalAuthorityCode(localAuthCode);

            System.Net.WebRequest request = System.Net.HttpWebRequest.Create(siteUrl);
            request.UseDefaultCredentials = true;
            request.PreAuthenticate = true;
            CredentialCache.DefaultNetworkCredentials.Domain = ConfigHelperWeb.GetSPDomain;
            CredentialCache.DefaultNetworkCredentials.UserName = ConfigHelperWeb.GetSPUsername;
            CredentialCache.DefaultNetworkCredentials.Password = ConfigHelperWeb.GetSPPassword;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;

            System.Net.WebResponse response = request.GetResponse();

            if (erfApplications.Count > 0)
            {
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
                                                <FieldRef Name='Title' />
                                                <Values>
                                                    {1}
                                                </Values>
                                            </In>" +
                                    @"</Where>
                                </Query>
                            </View>";

                        StringBuilder sb = new StringBuilder();
                        foreach (var erf in erfApplications)
                        {
                            sb.Append(string.Format(@"<Value Type='Text'>{0}</Value>", erf.ApplicationID));
                        }
                        queryString = string.Format(queryString, localAuthority, sb.ToString());

                        CamlQuery query = new CamlQuery();
                        query.ViewXml = queryString;

                        ListItemCollection listItems = list.GetItems(query);
                        context.Load(listItems);
                        context.ExecuteQuery();

                        var charges = MapChargesRecords(listItems);

                        for (int i = 0; i < erfApplications.Count; i++)
                        {
                            erfApplications[i].ErfCharges = charges
                                .Where(c => c.ApplicationID == erfApplications[i].ApplicationID).ToList();
                        }
                    }
                }
            }
            return erfApplications;
        }
        public List<ErfApplicationData> GetRecords(string localAuthority, string localAuthCode, int maximumRows)
        {
            try
            {
                List<ErfApplicationData> applicationPurchases = new List<ErfApplicationData>();
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
                        List list = web.Lists.GetByTitle("Application for Purchase");

                        CamlQuery query = new CamlQuery();
                        query.ViewXml = string.Format(@"
                                    <View>
	                                    <Query>
		                                    <Where>" +
                                                    @"<Eq>
                                                        <FieldRef Name='Update_x0020_to_x0020_Finance' />
                                                        <Value Type='Number'>1</Value>
                                                    </Eq>" +
                                           @" </Where>
	                                    </Query>
	                                    <ViewFields> 
                                            <FieldRef Name='Applicant_x0020_ID_x0020_Number' />
                                            <FieldRef Name='Applicant_x0020_Name' />
                                            <FieldRef Name='Applicant_x0020_Passport_x0020_N' />
                                            <FieldRef Name='Applicant_x0020_Postal_x0020_Add' />
                                            <FieldRef Name='Applicant_x0020_Phone_x0020_Numb' />
                                            <FieldRef Name='Applicant_x0020_Physical_x0020_A' />
                                            <FieldRef Name='Title' />
                                            <FieldRef Name='Application_x0020_Type' />
                                            <FieldRef Name='Applicant_x0020_Email' />
                                            <FieldRef Name='End_x0020_Date' />
                                            <FieldRef Name='ID' />
                                            <FieldRef Name='Local_x0020_Authority' />
                                            <FieldRef Name='Method_x0020_of_x0020_Payment' />
                                            <FieldRef Name='Minimum_x0020_Building_x0020_Val' />
                                            <FieldRef Name='Purchase_x0020_Date' />
                                            <FieldRef Name='Purchase_x0020_Price' />
                                            <FieldRef Name='Stand_x0020_No' />
                                            <FieldRef Name='Start_x0020_Date' />
                                            <FieldRef Name='UserID' />
                                            <FieldRef Name='Township' />
                                            <FieldRef Name='Zoning' />
                                            <FieldRef Name='Applicant_x0020_Gender' />
                                            <FieldRef Name='Applicant_x0020_Cellphone' />
                                            <FieldRef Name='Applicant_x0020_Website' />
                                            <FieldRef Name='Applicant_x0020_Fax' />
                                            <FieldRef Name='Applicant_x0020_VAT_x0020_Reg_x0' />
                                            <FieldRef Name='Deposit' />
                                            <FieldRef Name='Interest_x0020_Rate' />
                                            <FieldRef Name='Installment_x0020_Start_x0020_Da' />
                                            <FieldRef Name='Repayment_x0020_Period' />
                                            <FieldRef Name='Update_x0020_to_x0020_Finance' />
	                                    </ViewFields>
	                                    <QueryOptions />
                                        <RowLimit>{0}</RowLimit>
                                    </View>", maximumRows);

                        var items = list.GetItems(query);
                        context.Load(items);
                        context.ExecuteQuery();
                        var erfapplications = MapRecords(items);
                        return erfapplications;
                    }
                    else
                        return applicationPurchases;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
        public List<ChargesData> MapChargesRecords(ListItemCollection items)
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
                    ChargeType = Common.Common.ConvertTo<string>(listitem["Charge_Type"]),
                    //ChargeTypeID = Common.ConvertTo<int>(listitem["Charge_x0020_Type_x003a_ID"]),
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
        public List<ErfApplicationData> MapRecords(ListItemCollection items)
        {
            List<ErfApplicationData> applicationData = new List<ErfApplicationData>();
            ListItemCollection lists = items as ListItemCollection;
            ErfApplicationData erfApplication;
            
            foreach (ListItem listitem in lists)
            {
                erfApplication = new ErfApplicationData()
                {
                    ApplicantFax = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Fax"]),
                    ApplicantGender = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Gender"]),
                    ApplicantIdNumber = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_ID_x0020_Number"]),
                    ApplicantName = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Name"]),
                    ApplicantPassportNumber = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Passport_x0020_N"]),
                    ApplicantPhoneNumber = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Phone_x0020_Numb"]),
                    ApplicantPhysicalAddress = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Physical_x0020_A"]),
                    ApplicantPostalAddress = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Postal_x0020_Add"]),
                    ApplicantWebsite = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Website"]),
                    ApplicationID = Common.Common.ConvertTo<string>(listitem["Title"]),
                    ApplicationType = Common.Common.ConvertTo<string>(listitem["Application_x0020_Type"]),
                    Deposit = Common.Common.ConvertTo<decimal>(listitem["Deposit"]),
                    Email = Common.Common.ConvertTo<string>(listitem["Applicant_x0020_Email"]),
                    EndDate = Common.Common.ConvertTo<DateTime>(listitem["End_x0020_Date"]),
                    ID = Common.Common.ConvertTo<int>(listitem["ID"]),
                    InstallmentStartDate = Common.Common.ConvertTo<DateTime>(listitem["Installment_x0020_Start_x0020_Da"]),
                    InterestRate = Common.Common.ConvertTo<int>(listitem["Interest_x0020_Rate"]),
                    LocalAuthority = Common.Common.ConvertTo<string>(listitem["Local_x0020_Authority"]),
                    MethodOfPayment = Common.Common.ConvertTo<string>(listitem["Method_x0020_of_x0020_Payment"]),
                    MinimumBuildingValue = Common.Common.ConvertTo<decimal>(listitem["Minimum_x0020_Building_x0020_Val"]),
                    PurchaseDate = Common.Common.ConvertTo<DateTime>(listitem["Purchase_x0020_Date"]),
                    PurchasePrice = Common.Common.ConvertTo<decimal>(listitem["Purchase_x0020_Price"]),
                    StandNo = Common.Common.ConvertTo<string>(listitem["Stand_x0020_No"]),
                    StartDate = Common.Common.ConvertTo<DateTime>(listitem["Start_x0020_Date"]),
                    Township = Common.Common.ConvertTo<string>(listitem["Township"]),
                    UserID = Common.Common.ConvertTo<string>(listitem["UserID"]),
                    Zoning = Common.Common.ConvertTo<string>(listitem["Zoning"])
                };

                int status = Common.Common.ConvertTo<int>(listitem["Update_x0020_to_x0020_Finance"]);
                erfApplication.ItemState = status == 1 ? ObjectState.New
                                         : status == 2 ? ObjectState.Unchanged
                                         : status == 3 ? ObjectState.Update
                                         : status == 4 ? ObjectState.Delete
                                         : ObjectState.Ignore;
                applicationData.Add(erfApplication);
            }
            return applicationData;
        }
        public bool FlagSynchronizedRecords(string localAuthCode, int[] ids)
        {
            try
            {
                List<ErfApplicationData> applicationPurchases = new List<ErfApplicationData>();
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
                        List list = web.Lists.GetByTitle("Application for Purchase");

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