using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web;
using ULIMSWcfClient.ConfigurationWeb;
using ULIMSWcfClient.svcSpErf;

namespace ULIMSWcfClient.GisProcessing
{
    public class gistospExecution
    {
        public bool SaveRecords(ErfData[] items, string localAuthorityCode, ObjectState state)
        {
            try
            {
                bool isSuccess = false;
                LocalAuthority la = ConfigHelperWeb.GetLocalAuthority(localAuthorityCode);
                if (string.IsNullOrEmpty(la.SubSiteUrl))
                {
                    Exception ex = new Exception("Local Authority Name is Empty");
                    throw new FaultException<Exception>(ex, new FaultReason(new FaultReasonText("Local Authority name is Empty")));
                }

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

                        if (state == ObjectState.New)
                        {
                            List list = web.Lists.GetByTitle("Erf");

                            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                            ListItem oListItem;
                            foreach (ErfData erfData in items)
                            {
                                oListItem = list.AddItem(itemCreateInfo);
                                oListItem["ErfNo"] = erfData.ErfNo;
                                oListItem["Local_x0020_Authority"] = la.LocalAuthorityName;
                                oListItem["Township"] = erfData.Township;
                                oListItem["Zoning"] = erfData.Zoning;
                                oListItem["Title"] = erfData.StandNo;
                                oListItem["Survey_x0020_Size"] = erfData.SurveySize;
                                oListItem["Computed_x0020_Size"] = erfData.ComputedSize;
                                oListItem["Ownership"] = erfData.Ownership;
                                oListItem["Density"] = erfData.Density;
                                oListItem["Update_x0020_to_x0020_Finance"] = 1;
                                oListItem.Update();
                            }
                            context.ExecuteQuery();
                            isSuccess = true;
                        }
                        else if (state == ObjectState.Update)
                        {
                            if (items.Count() > 0)
                            {
                                List list = web.Lists.GetByTitle("Erf");
                                string queryString = @"
                                        <View>
                                            <Query>
                                                <Where>
                                                    <In>
                                                        <FieldRef Name='Title' />
                                                        <Values>
                                                            {0}
                                                        </Values>
                                                    </In>
                                                </Where>
                                            </Query>
                                        </View>";

                                StringBuilder sb = new StringBuilder();
                                Dictionary<string, ErfData> erfByStandNo = new Dictionary<string, ErfData>();
                                foreach (var ed in items)
                                {
                                    sb.Append(string.Format(@"<Value Type='Text'>{0}</Value>", ed.StandNo));
                                    erfByStandNo[ed.StandNo] = ed;
                                }
                                queryString = string.Format(queryString, sb.ToString());

                                CamlQuery query = new CamlQuery();
                                query.ViewXml = queryString;

                                ListItemCollection listItems = list.GetItems(query);
                                context.Load(listItems);
                                context.ExecuteQuery();


                                ErfData erf;
                                string standNo = string.Empty;
                                Dictionary<string, ListItem> listItemDict = new Dictionary<string, ListItem>();
                                foreach (var litem in listItems)
                                {
                                    standNo = Common.Common.ConvertTo<string>(litem["Title"]);
                                    listItemDict[standNo] = litem;
                                }


                                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                                ListItem item;
                                bool isupdate = false;
                                foreach (KeyValuePair<string, ErfData> erfD in erfByStandNo)
                                {
                                    erf = erfD.Value;
                                    if (listItemDict.Keys.Contains(erfD.Key))
                                    {
                                        item = listItemDict[erfD.Key];
                                        if (erf != null)
                                        {
                                            item["ErfNo"] = erf.ErfNo;
                                            item["Local_x0020_Authority"] = la.LocalAuthorityName;
                                            item["Township"] = erf.Township;
                                            item["Zoning"] = erf.Zoning;
                                            item["Title"] = erf.StandNo;
                                            item["Survey_x0020_Size"] = erf.SurveySize;
                                            item["Computed_x0020_Size"] = erf.ComputedSize;
                                            item["Ownership"] = erf.Ownership;
                                            item["Density"] = erf.Density;
                                            item["Update_x0020_to_x0020_Finance"] = 3;
                                            item.Update();
                                            isupdate = true;
                                        }
                                    }
                                    else
                                    {
                                        item = list.AddItem(itemCreateInfo);
                                        item["ErfNo"] = erf.ErfNo;
                                        item["Local_x0020_Authority"] = la.LocalAuthorityName;
                                        item["Township"] = erf.Township;
                                        item["Zoning"] = erf.Zoning;
                                        item["Title"] = erf.StandNo;
                                        item["Survey_x0020_Size"] = erf.SurveySize;
                                        item["Computed_x0020_Size"] = erf.ComputedSize;
                                        item["Ownership"] = erf.Ownership;
                                        item["Density"] = erf.Density;
                                        item["Update_x0020_to_x0020_Finance"] = 1;
                                        item.Update();
                                        isupdate = true;
                                    }
                                }
                                if (isupdate)
                                    context.ExecuteQuery();
                                isSuccess = isupdate;
                            }
                        }
                        else if (state == ObjectState.Delete)
                        {
                            if (items.Count() > 0)
                            {
                                List list = web.Lists.GetByTitle("Erf");
                                string queryString = @"
                                        <View>
                                            <Query>
                                                <Where>
                                                    <In>
                                                        <FieldRef Name='Title' />
                                                        <Values>
                                                            {0}
                                                        </Values>
                                                    </In>
                                                </Where>
                                            </Query>
                                        </View>";

                                StringBuilder sb = new StringBuilder();
                                Dictionary<string, ErfData> erfByStandNo = new Dictionary<string, ErfData>();
                                foreach (var ed in items)
                                {
                                    sb.Append(string.Format(@"<Value Type='Text'>{0}</Value>", ed.StandNo));
                                    erfByStandNo[ed.StandNo] = ed;
                                }
                                queryString = string.Format(queryString, sb.ToString());

                                CamlQuery query = new CamlQuery();
                                query.ViewXml = queryString;

                                ListItemCollection listItems = list.GetItems(query);
                                context.Load(listItems);
                                context.ExecuteQuery();

                                ErfData erf;
                                string standNo = string.Empty;
                                Dictionary<string, ListItem> listItemDict = new Dictionary<string, ListItem>();
                                foreach (var litem in listItems)
                                {
                                    standNo = Common.Common.ConvertTo<string>(litem["Title"]);
                                    listItemDict[standNo] = litem;
                                }

                                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                                ListItem item;
                                bool isupdate = false;
                                foreach (KeyValuePair<string, ErfData> erfD in erfByStandNo)
                                {
                                    erf = erfD.Value;
                                    if (listItemDict.Keys.Contains(erfD.Key))
                                    {
                                        item = listItemDict[erfD.Key];
                                        if (erf != null)
                                        {
                                            item["ErfNo"] = erf.ErfNo;
                                            item["Local_x0020_Authority"] = la.LocalAuthorityName;
                                            item["Township"] = erf.Township;
                                            item["Zoning"] = erf.Zoning;
                                            item["Title"] = erf.StandNo;
                                            item["Survey_x0020_Size"] = erf.SurveySize;
                                            item["Computed_x0020_Size"] = erf.ComputedSize;
                                            item["Ownership"] = erf.Ownership;
                                            item["Density"] = erf.Density;
                                            item["Update_x0020_to_x0020_Finance"] = 3;
                                            item["Deleted_x0020_Status"] = "Yes";
                                            item.Update();
                                            isupdate = true;
                                        }
                                    }
                                }
                                if (isupdate)
                                    context.ExecuteQuery();
                                isSuccess = isupdate;
                            }
                        }
                    }
                    return isSuccess;
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<Exception>(ex, new FaultReason(ex.Message));
            }
        }
    }
}