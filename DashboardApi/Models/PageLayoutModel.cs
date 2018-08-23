using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public class PageLayoutModel
    {
        public Object Layout { get; set; }

        public int AppId { get; set; }

        public int PageId { get; set; }

        public string AppTitle { get; set; }
    }
}