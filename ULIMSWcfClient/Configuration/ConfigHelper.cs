using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ULIMSWcfClient.Configuration
{
    public class ConfigHelper
    {
        public static Dictionary<string, string> Settings { get; set; }

        public static int GetPageSize
        {
            get { return GetSetting<int>("PageSize"); }
        }
        public static int GetMinuteInterval
        {
            get { return GetSetting<int>("MinuteInterval"); }
        }

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