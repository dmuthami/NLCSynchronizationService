using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;

namespace ULIMSWcfClient.ConfigurationWeb
{
    public class ConfigHelperWeb
    {
        public static int GetPageSize
        {
            get { return GetSetting<int>("PageSize"); }
        }

        public static int GetMinuteInterval
        {
            get { return GetSetting<int>("MinuteInterval"); }
        }

        public static string GetSPUsername
        {
            get { return GetSetting<string>("SPUsername"); }
        }

        public static string GetSPPassword
        {
            get { return GetSetting<string>("SPPassword"); }
        }

        public static string GetSPDomain
        {
            get { return GetSetting<string>("SPDomain"); }
        }

        public static string GetNavUsername
        {
            get { return GetSetting<string>("NavUsername"); }
        }

        public static string GetNavPassword
        {
            get { return GetSetting<string>("NavPassword"); }
        }

        public static string GetNavDomain
        {
            get { return GetSetting<string>("NavDomain"); }
        }

        public static Dictionary<string, LocalAuthority> LocalAuthoritySettings { get; set; }

        public static LocalAuthority GetLocalAuthority(string code)
        {
            LocalAuthority la = new LocalAuthority();
            if (LocalAuthoritySettings == null)
                LocalAuthoritySettings = new Dictionary<string, LocalAuthority>();
            if (LocalAuthoritySettings.Keys.Contains(code))
                la = LocalAuthoritySettings[code];
            else
            {
                LocalAuthoritySection section = (LocalAuthoritySection)ConfigurationManager.GetSection("LocalAuthoritySection");
                if (section != null)
                {
                    foreach (LocalAuthorityElement element in section.LocalAuthoritiesKeys)
                    {
                        if (string.Compare(element.Code, code, false) == 0)
                        {
                            la = new LocalAuthority()
                            {
                                LocalAuthorityCode = element.Code,
                                LocalAuthorityName = element.LocalAuthority,
                                SubSiteUrl = element.SiteUrl
                            };
                            LocalAuthoritySettings[code] = la;
                            break;
                        }
                    }
                }
            }
            return la;
        }

        public static Dictionary<string, string> SiteUrlByLocalAuthorityCode { get; set; }

        public static string GetSiteUrlByLocalAuthorityCode(string code)
        {
            if (SiteUrlByLocalAuthorityCode == null)
                SiteUrlByLocalAuthorityCode = new Dictionary<string, string>();
            string siteUrl = string.Empty;
            if (SiteUrlByLocalAuthorityCode.Keys.Contains(code))
                siteUrl = SiteUrlByLocalAuthorityCode[code];
            else
            {
                LocalAuthoritySection section = (LocalAuthoritySection)ConfigurationManager.GetSection("LocalAuthoritySection");
                if (section != null)
                {
                    foreach (LocalAuthorityElement element in section.LocalAuthoritiesKeys)
                    {
                        if (string.Compare(element.Code, code, false) == 0)
                        {
                            siteUrl = element.SiteUrl;
                            SiteUrlByLocalAuthorityCode[code] = siteUrl;
                            break;
                        }
                    }
                }
            }
            return siteUrl;
        }

        public static string GetSiteUrl(string name)
        {
            if (Settings == null)
                Settings = new Dictionary<string, string>();
            string siteUrl = string.Empty;
            if (Settings.Keys.Contains(name))
                siteUrl = Settings[name];
            else
            {
                LocalAuthoritySection section = (LocalAuthoritySection)ConfigurationManager.GetSection("LocalAuthoritySection");
                if (section != null)
                {
                    foreach (LocalAuthorityElement element in section.LocalAuthoritiesKeys)
                    {
                        if (string.Compare(element.LocalAuthority, name, false) == 0)
                        {
                            siteUrl = element.SiteUrl;
                            Settings[name] = siteUrl;
                            break;
                        }
                    }
                }
            }
            return siteUrl;
        }

        public static string GetClientAddressByName(string name)
        {
            string address = string.Empty;
            ClientSection clientSection = (ClientSection)ConfigurationManager.GetSection("system.serviceModel/client");
            for (int i = 0; i < clientSection.Endpoints.Count; i++)
            {
                if (string.Compare(clientSection.Endpoints[i].Name, name, false) == 0)
                {
                    address = clientSection.Endpoints[i].Address.ToString();
                    break;
                }
            }
            return address;
        }

        public static Dictionary<string, string> Settings { get; set; }

        public static string GetSetting(string settingName)
        {
            if (Settings == null)
                Settings = new Dictionary<string, string>();
            string value;

            if (Settings.Keys.Contains(settingName))
                value = Settings[settingName];
            else
            {
                value = ConfigurationManager.AppSettings[settingName];
                Settings[settingName] = value;
            }
            return value;
        }

        public static T GetSetting<T>(string settingName)
        {
            return Common.Common.ConvertTo<T>(GetSetting(settingName));
        }
    }
}