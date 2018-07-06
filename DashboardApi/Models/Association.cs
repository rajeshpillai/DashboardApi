using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public class Relation
    {       
        public string TableName2 { get; set; }

        public string Type { get; set; }

        public string Operation { get; set; }

        public List<string> Keys { get; set; }

    }


    public class Association
    {
        public string TableName { get; set; }

        public List<Relation> Relations { get; set; }
    }


}