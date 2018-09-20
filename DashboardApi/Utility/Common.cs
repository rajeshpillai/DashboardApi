using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace DashboardApi.Utility
{
    public enum dataType
    {
        System_Boolean = 0,
        System_Int32 = 1,
        System_Int64 = 2,
        System_Double = 3,
        System_DateTime = 4,
        System_String = 5
    }

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
       

        public static dataType ParseString(string str)
        {

            bool boolValue;
            Int32 intValue;
            Int64 bigintValue;
            double doubleValue;
            DateTime dateValue;

            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(str, out boolValue))
                return dataType.System_Boolean;
            else if (Int32.TryParse(str, out intValue))
                return dataType.System_Int32;
            else if (Int64.TryParse(str, out bigintValue))
                return dataType.System_Int64;
            else if (double.TryParse(str, out doubleValue))
                return dataType.System_Double;
            else if (DateTime.TryParse(str, out dateValue))
                return dataType.System_DateTime;
            else return dataType.System_String;

        }
    }
}