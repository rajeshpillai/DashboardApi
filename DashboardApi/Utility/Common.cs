using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace DashboardApi.Utility
{
    public class Common
    {
        public static string GetFilePath()
        {
            return ConfigurationManager.AppSettings.Get("FilePath");
        }

        public static string GetAppPath(string appTitle)
        {
            if (string.IsNullOrEmpty(appTitle)) { appTitle = "App 1"; }
            var path  = ConfigurationManager.AppSettings.Get("FilePath"); ;
            path += @"\" + appTitle;
            return path;
        }
    }
}