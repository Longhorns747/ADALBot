using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace TestBot
{
    public static class Constants
    {
        internal static string ADClientId = ConfigurationManager.AppSettings["ADClientId"];

        internal static string ADClientSecret = ConfigurationManager.AppSettings["ADClientSecret"];

        internal static string apiBasePath = ConfigurationManager.AppSettings["apiBasePath"].ToLower();

        internal static string botId = ConfigurationManager.AppSettings["AppId"];

        internal static string botSecret = ConfigurationManager.AppSettings["AppSecret"];

        internal static string botName = ConfigurationManager.AppSettings["botName"];
    }
}