using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DashboardApi.Models
{
    public class Response
    {
        public Response()
        {
            this.Status = "success";
        }
        public string Status { get; set; }
        public object Data { get; set; }

        public object Error { get; set; }
    }
}