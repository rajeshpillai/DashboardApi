using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SqlKata;
using SqlKata.Compilers;
using MySql.Data.MySqlClient;
using SqlKata.Execution;
using System.Web.Http.Cors;
using DashboardApi.Models;

namespace DashboardApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DataController : ApiController
    {
        static object PageData = null;

        // GET: api/Data       
        [HttpGet]
        public IEnumerable<dynamic> Get()
        {
           
            var connection = new MySqlConnection(
             "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
            );
            var db = new QueryFactory(connection, new MySqlCompiler());

       
            var employees = db.Query("employee").Where("EmployeeId", 1).Select("EmployeeId","LoginId").Get();

            //SqlResult result = compiler.Compile(query);

            //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


            return employees;
        }

        // GET: api/Data
        [Route("api/data/getall")]
        [HttpGet]
        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
        public IEnumerable<dynamic> GetAll()
        {

            var connection = new MySqlConnection(
             "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
            );
            var db = new QueryFactory(connection, new MySqlCompiler());


            var employees = db.Query("employee").Get();

            //SqlResult result = compiler.Compile(query);

            //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


            return employees;
        }


        //// GET: api/Data
        //[Route("api/data/getfilters")]
        //[HttpPost]
        //public IEnumerable<dynamic> GetFilters(WidgetModel widgetModel)
        //{

        //    var connection = new MySqlConnection(
        //     "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
        //    );
        //    var db = new QueryFactory(connection, new MySqlCompiler());


        //    var employees = db.Query("employee").Select(widgetModel.Dimension).Distinct().Get();

        //    //SqlResult result = compiler.Compile(query);

        //    //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


        //    return employees;
        //}

        //// GET: api/Data
        //[Route("api/data/evalexp")]
        //[HttpPost]
        //public IEnumerable<dynamic> Evalexp(WidgetModel widgetModel)
        //{

        //    var connection = new MySqlConnection(
        //     "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
        //    );
        //    var db = new QueryFactory(connection, new MySqlCompiler());


        //    var evaluatedExp = db.Query("employee").SelectRaw(widgetModel.Measure + " as " + widgetModel.Dimension).Get();

        //    //SqlResult result = compiler.Compile(query);

        //    //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


        //    return evaluatedExp;
        //}


        // GET: api/Data
        [Route("api/data/getData")]
        [HttpPost]
        public IEnumerable<dynamic> GetData(WidgetModel widgetModel)
        {
            IEnumerable<dynamic> data = null;
            Query query = null;
            var connection = new MySqlConnection(
             "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
            );
            var db = new QueryFactory(connection, new MySqlCompiler());

            //if (widgetModel.Type == "kpi"){
            var measures = new System.Text.StringBuilder();
            if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length ==0)))
            {
                return null;
            }
            if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
            {
                return null;
            }

            var measuresString = string.Empty;
            var dimString = string.Empty;

            if (null != widgetModel.Measure)
            {
                foreach (var measure in widgetModel.Measure)
                {
                    // var expDisplayName = !string.IsNullOrWhiteSpace(measure.DisplayName) ? measure.DisplayName : '[' + measure.Expression + ']';
                    if (!string.IsNullOrWhiteSpace(measure.DisplayName))
                    {
                        measures.Append(string.Format("{0} as {1} ", measure.Expression.Trim(), measure.DisplayName.Trim()) + ", ");
                    }
                    else
                    {
                        measures.Append(string.Format("{0} ", measure.Expression.Trim()) + ", ");
                    }

                }

                measuresString = measures.ToString();
                measuresString = measuresString.Remove(measuresString.LastIndexOf(','), 1);
            }
            

            var dims = new System.Text.StringBuilder();

            if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0)
            {
                foreach (var dim in widgetModel.Dimension)
                {
                    // var expDisplayName = !string.IsNullOrWhiteSpace(measure.DisplayName) ? measure.DisplayName : '[' + measure.Expression + ']';
                    if (!string.IsNullOrWhiteSpace(dim.Name))
                    {
                        dims.Append(dim.Name.Trim() + ",");
                    }                    
                }
                dimString = dims.ToString();
                dimString = dimString.Remove(dimString.LastIndexOf(','), 1);
            }

         
            //Todo: widgetModel.SqlTableName
            if (widgetModel.Type == "filter")
                {
                    query = db.Query("employee").Select(dimString).Distinct();
                    //data = db.Query("employee").Select(dimString).Distinct().Get();
                   
                } else
                    {
                        query = db.Query("employee").SelectRaw(measuresString);
                        //data = db.Query("employee").SelectRaw(measuresString).Get();
                }
            var constraints = new Dictionary<string, object>();
            

            if (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0)
            {
                foreach(var filter in widgetModel.FilterList)
                {
                   // constraints.Add(filter.ColName, filter.Values.to);
                    query = query.WhereIn(filter.ColName, filter.Values);
                }
                
            }

            data = query.Get();

            //var constraints = new Dictionary<string, object> {
            //                    { "Year", 2017 },
            //                    { "CategoryId", 198 },
            //                    { "IsPublished", true }
            //                };


            //}

            //SqlResult result = compiler.Compile(query);

            //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


            return data;
        }

        // GET: api/Data/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Data
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Data/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Data/5
        public void Delete(int id)
        {
        }

        [Route("api/data/savePageData")]
        [HttpPost]
        public void SavePageData(object data)
        {
            PageData = data;
        }

        [Route("api/data/getPageData")]
        [HttpPost]
        public object GetPageData()
        {
           return PageData;
        }
    }
}
