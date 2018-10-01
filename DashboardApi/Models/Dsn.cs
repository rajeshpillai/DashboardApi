using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public enum VizDsnType
    {
        USER,
        SYSTEM
    }

    public class VizDsn
    {
        public string Name { get; set; }
        public string Driver { get; set; }
        public string DsnType { get; set; }
        public string VizDBType { get; set; }
    }
}