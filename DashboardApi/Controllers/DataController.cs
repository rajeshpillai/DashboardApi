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
using System.Runtime.Caching;
using System.Data.Odbc;
using MonetDB.Driver.Data;
using MonetDB.Driver;
using MonetDB.Driver.Extensions;
using System.Web;
using DashboardApi.Utility;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Data.OleDb;


namespace DashboardApi.Controllers
{
    

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DataController : ApiController
    {
        //static object PageData = null;

        static List<Association> TableAssociations = null;

        //static Dashboard dashboard = null;

        // GET: api/Data
        [Route("api/data/getsalary")]
        [HttpGet]
        public dynamic GetSalary()
        {
            // var connection = new System.Data.Odbc.OdbcConnection("Driver={MonetDB ODBC Driver};HOST=127.0.0.1;PORT=50000; Database=demo;UID=monetdb; PWD=monetdb");

            // var connection = new System.Data.Odbc.OdbcConnection("Dsn=testmonetdb;HOST=127.0.0.1;PORT=50000; Database=demo;UID=monetdb; PWD=monetdb");
            //System.Data.Odbc.OdbcException
            //            HResult = 0x80131937
            //  Message = ERROR[IM002][Microsoft][ODBC Driver Manager] Data source name not found and no default driver specified
            //  Source =
            //  StackTrace:
            //< Cannot evaluate the exception stack trace >

            // var driver = new MonetDB.Driver.MonetDbDriver("database=demo;host=127.0.0.1;");
            // var ConnectionInfo = driver.Database.ConnectionInfo;
            //var result = MonetDB.Driver.Helpers.MonetDbHelper.ExecuteScalar(ConnectionInfo.ToString(), "select count(*) from skills");


            ////var connection = new MySqlConnection(
            //// "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
            ////);

            //working start *********************
            System.Data.DataSet ds = new System.Data.DataSet();
            var dt = new System.Data.DataTable("Test");
            dt.Columns.Add("city");
            dt.Columns.Add("sum(salary)");
            using (var connection = new System.Data.MonetDb.MonetDbConnection("database=demo;host=127.0.0.1;PORT=50000;username=monetdb;password=monetdb"))
            {



                connection.Open();
                
                
                using (var command = connection.CreateCommand())
                {
                    // create table
                    command.CommandText = "SELECT city, sum(salary) FROM employee group by city";

                    using (var reader = command.ExecuteReader())
                    {
                        //ds = reader.ToDateSet();
                        while (reader.Read())
                        {
                            var values = new object[reader.FieldCount];

                            reader.GetValues(values);
                            dt.Rows.Add(values);
                            

                            //var ename = reader[0];
                            //var salary = reader[1];
                            //var schema = reader.GetSchemaTable();
                            //var t1 = schema.Columns.Contains("ename");
                            //var t2 = schema.Columns.Contains("value");
                            //var t3 = schema.Columns["ename"].DataType;
                            //var t4 = schema.Columns["value"].DataType;
                        }
                    }
                    ds.Tables.Add(dt);
                }


                //using (var command = new System.Data.MonetDb.MonetDbCommand(connection)
                //{
                //    CommandText = "SELECT ename, salary FROM employee WHERE empid = 1"
                //})
                //{
                //    using (var reader = command.ExecuteReader())
                //    {
                //        ds = reader.ToDateSet();
                //    }
                //}

                connection.Close();
            }

            //ds.Tables[0].ToString();

            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            //var dt = ds.Tables[0];
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            //var tt = Json(serializer.Serialize(rows));
            return Json(rows);
            //working end *********************

            //return ds;

            //using (OdbcConnection con = new OdbcConnection("database=demo;host=127.0.0.1;"))
            //{

            //    con.Open();
            //    // We are now connected. Now we can use OdbcCommand objects
            //    // to actually accomplish things.
            //    using (OdbcCommand com = new OdbcCommand("SELECT ename, salary FROM employee WHERE empid = 1", con))
            //    {
            //        using (OdbcDataReader reader = com.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                string word = reader.GetString(0);
            //                // Word is from the database. Do something with it.
            //            }
            //        }
            //    }
            //}




            //var connection = new System.Data.MonetDb.MonetDbConnection("database=demo;host=127.0.0.1;PORT=50000;username=monetdb;password=monetdb");
            //var db = new QueryFactory(connection, new MySqlCompiler());

            //var employees = db.Query("employee").Where("empid", 1).Select("ename", "salary").Get();

            //////SqlResult result = compiler.Compile(query);

            //////var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();

           // return new List<string>();

            //return employees;
        }

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

        private void BuildTableMetatdata(string appId)
        {
            var appTitle = "app_" + appId;
            var dashboard = new Dashboard();

            if (null != MemoryCache.Default.Get("dashboard_" + appTitle,null)){
                dashboard = MemoryCache.Default.Get("dashboard_" + appTitle, null) as Dashboard;
                return;
            }

            //dashboard.AddTable(new Table() { Name = "Product" });
            //dashboard.AddTable(new Table() { Name = "ProductInventory" });
            //dashboard.AddTable(new Table() { Name = "ProductVendor" });
            //dashboard.AddTable(new Table() { Name = "PurchaseOrderDetail" });
            //dashboard.AddTable(new Table() { Name = "PurchaseOrderHeader" });

            //dashboard.AddTable(new Table() { Name = "employee1" });
            //dashboard.AddTable(new Table() { Name = "skills1" });


            //dashboard.Tables.AddRange(MemoryCache.Default.Get("tables") as List<Table>);

            //Get App Header and tables for that App

            var app = GetAppById(appId);

            var tables = app.Tables;
            //var tables = GetTables();
            //var tablesObject = tables.ToObject<List<object>>();
            //dashboard.Tables = new List<Table>();
            foreach (var o in tables)
            {
                dashboard.AddTable(new Table() { Name = o.Name });
            }
           
            dashboard.Associations = GetAllTableAssociations(appTitle);

            var tablesCount = dashboard.Tables.Count();

            int[,] graph = new int[tablesCount, tablesCount];

            for (var u = 0; u < tablesCount; u++)
            {
                for (var v = 0; v < tablesCount; v++)
                {
                    //graph[u, v] = IsEdgeExist(u, v, associations, tables);
                    graph[u, v] = IsEdgeExist(u, v, dashboard.Associations, dashboard);
                }
            }
            var combinations = GetAllCombinations(dashboard);

            //Get tables involed for each combination
            var tablesInvolvedForCombi = new Dictionary<string, List<string>>();
            foreach (var combi in combinations)
            {
                var tablesInvoled = combi.Split(',');
                if (tablesInvoled.Length == 1)
                {
                    continue;
                }
                var tInvolvedForCombi = new List<string>();

                for (int k = 0; k < tablesInvoled.Count() - 1; k++)
                {
                    var t = DashboardApi.Utility.Dijkstra.DijkstraAlgo(graph, Convert.ToInt32(tablesInvoled[k]), Convert.ToInt32(tablesInvoled[k + 1]), tablesCount);
                    if (null == t) { continue; }
                    foreach (var tIndex in t.Split(','))
                    {
                        tInvolvedForCombi.Add(tIndex);
                        //if (tInvolvedForCombi.Contains(tIndex))
                        //{
                        //    tInvolvedForCombi.Remove(tIndex);
                        //    tInvolvedForCombi.Add(tIndex);
                        //}
                        //else
                        //{
                        //    tInvolvedForCombi.Add(tIndex);
                        //}
                    }
                }
                if (!tInvolvedForCombi.Contains(tablesInvoled[tablesInvoled.Count() - 1]))
                {
                    tInvolvedForCombi.Add((tablesInvoled[tablesInvoled.Count() - 1]).ToString());  //Insert last table if not exist in list
                }


                tablesInvolvedForCombi.Add(combi, tInvolvedForCombi);
            }


            var TableAssociationHash = new Dictionary<string, DerivedAssociation>();
            foreach (var combi in combinations)
            {
                var da = new DerivedAssociation();

                if (!tablesInvolvedForCombi.ContainsKey(combi))
                {
                    continue;
                }
                var tablesInvoled = tablesInvolvedForCombi[combi];//combi.Split(',');  //Get Tables involved for that combination
                if (tablesInvoled.Count() == 1)
                {
                    continue;
                }
                //

                //List<DerivedAssociation> dAssociations = new List<DerivedAssociation>();
                var derivedAssociation = new DerivedAssociation();
                var tablesConsidered = new List<string>();

                derivedAssociation.TableName1 = dashboard.Tables.Where(tbl => tbl.Id == Convert.ToInt32(tablesInvoled[0])).First().Name;
                tablesConsidered.Add(tablesInvoled[0]);
                for (int j = 1; j < tablesInvoled.Count(); j++)//s (var t in tablesInvoled)
                {
                    //var curTableName = tables[Convert.ToInt32(tablesInvoled[j])];
                    //var prevTableName = tables[Convert.ToInt32(tablesInvoled[j - 1])];

                    var curTableName = dashboard.Tables.Where(tbl => tbl.Id == Convert.ToInt16(tablesInvoled[j])).First().Name;
                    if (tablesConsidered.Contains(tablesInvoled[j]))
                    {
                        continue;
                    }
                    var prevTableName = dashboard.Tables.Where(tbl => tbl.Id == Convert.ToInt32(tablesInvoled[j - 1])).First().Name;

                    var tableAssociation = dashboard.Associations.Where(a => a.TableName == prevTableName && a.Relations.Where(r => r.TableName2 == curTableName).FirstOrDefault() != null).FirstOrDefault();

                    if (null != tableAssociation)
                    {
                        var relation = tableAssociation.Relations.Where(r => r.TableName2 == curTableName).FirstOrDefault();
                        derivedAssociation.Relations.Add(relation);

                        //dAssociations.Add(derivedAssociation);
                    }
                    tablesConsidered.Add(tablesInvoled[j]);

                    //var tableAssociation= associations.Where(a => a.TableName == tables[Convert.ToInt32(tablesInvoled[j - 1])] && a.Relations.Where(r => r.TableName2 == tables[Convert.ToInt32(tablesInvoled[j])]).FirstOrDefault() != null ||
                    //                     (a.TableName == tables[Convert.ToInt32(tablesInvoled[j])] && a.Relations.Where(r => r.TableName2 == tables[Convert.ToInt32(tablesInvoled[j - 1])]).FirstOrDefault() != null)).FirstOrDefault();

                    //if (null != tableAssociation)
                    //{
                    //    derivedAssociation.Relations.Add(tableAssociation.Relations.Where(r=>r.TableName2 == tables[Convert.ToInt32(tablesInvoled[j - 1])] || r.TableName2 == tables[Convert.ToInt32(tablesInvoled[j])]).FirstOrDefault());

                    //    dAssociations.Add(derivedAssociation);
                    //}


                }
                TableAssociationHash.Add(combi, derivedAssociation);

            }

            dashboard.TableAssociationHash = TableAssociationHash;

            MemoryCache.Default.Set("dashboard_" + appTitle, dashboard, null,null);

            //Save TableAssociationHash in file
            var associationsPath = Common.GetAppPath(appTitle);
            associationsPath += @"\associationHash.assocHash";
            var compressionHelper = new CompressionHelper<Dictionary<string, DerivedAssociation>>();
            compressionHelper.CompressAndSaveLZ4(TableAssociationHash, associationsPath);

        }

        //// GET: api/Data
        //[Route("api/data/getall")]
        //[HttpGet]
        //[EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
        //public IEnumerable<dynamic> GetAll()
        //{
           
        //    var connection = new MySqlConnection(
        //     "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
        //    );
        //    var db = new QueryFactory(connection, new MySqlCompiler());

        //    var associations = GetAllTableAssociations();


        //    int count = 0;
        //    Query query = null;
        //    foreach (var a in associations)
        //    {
        //        if (count == 0)
        //        {
        //            query = db.Query(a.TableName);
        //        }

        //        foreach (var relation in a.Relations)
        //        {
        //            foreach(var keys in relation.Keys)
        //            {
        //                query = query.Join(relation.TableName2, keys[0], keys[1], relation.Operation, relation.Type);
        //            }
                    
        //        }

        //        count++;
        //    }

        //    ///db.Query("Product").Join("ProductInventory", "Product.ProductId", "ProductInventory.ProductId", "=", "inner").Select("Product.Name");

        //    var data = query.Get();

        //    //var data =  db.Query("Product").Join("ProductInventory", "Product.ProductId", "ProductInventory.ProductId", "=", "inner").Select("Product.Name").Get();

        //    var employees = db.Query("employee").Get();

        //    //SqlResult result = compiler.Compile(query);

        //    //var users = new XQuery(connection, compiler).From("Users").Limit(10).Get();


        //    return employees;
        //}


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

        //public Query GetTablesAssociationQuery(WidgetModel widgetModel)
        //{

        //    var connection = new MySqlConnection(
        //     "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
        //    );
        //    var db = new QueryFactory(connection, new MySqlCompiler());
        //    Query query = null;

        //    //var tables = GetTablesInvolved(widgetModel);

        //    var tablesKey = GetTablesInvolved(widgetModel);
        //    DerivedAssociation derivedAssociation = null;

        //    if (dashboard.TableAssociationHash.ContainsKey(tablesKey))
        //    {
        //        derivedAssociation = dashboard.TableAssociationHash[tablesKey];

        //        if (null != derivedAssociation)
        //        {
        //            query = db.Query(derivedAssociation.TableName1);
        //            foreach (var rel in derivedAssociation.Relations)
        //            {
        //                foreach (var keys in rel.Keys)
        //                {
        //                    query = query.Join(rel.TableName2, keys[0], keys[1], rel.Operation, rel.Type);
        //                }
                        
        //            }
        //        }
        //    } else
        //    {
        //        if(tablesKey.Split(',').Length == 1)
        //        {
        //            var tableId = tablesKey.Split(',')[0];
        //            var tableName = dashboard.Tables.Where(t => t.Id.ToString() == tableId).First().Name;
        //            query = db.Query(tableName);
        //        }
        //    }

        //    //if (null != tables && tables.Count > 0)
        //    //{
        //    //    var allAssociations = dashboard.Associations; //GetAllTableAssociations();

        //    //    var tablesConsidered = new List<string>();
        //    //    tablesConsidered.Add(tables[0]);
        //    //    query = db.Query(tables[0]);
        //    //    for (var i = 0; i < tables.Count; i++)
        //    //    {
        //    //        var association = allAssociations.Where(a => a.TableName == tables[i]).FirstOrDefault();
        //    //        if(null != association)
        //    //        {
        //    //            foreach (var rel in association.Relations)
        //    //            {
        //    //                if (tables.Contains(rel.TableName2) && !tablesConsidered.Contains(rel.TableName2))
        //    //                {
        //    //                    //consider it
        //    //                    query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
        //    //                    tablesConsidered.Add(rel.TableName2);
        //    //                }
        //    //            }
        //    //        }

        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    if (widgetModel.Type == "filter" && widgetModel.FilterList.Count == 0)
        //    //    {
        //    //        //Do not joing on any other table.
        //    //        //Select from table of dimention only
        //    //        var dimName = widgetModel.Dimension[0].Name;
        //    //        var tableName = dimName.Substring(0, dimName.IndexOf("."));
        //    //        query = db.Query(tableName);
        //    //    }                
        //    //}


        //    return query;
        //}

        ////private List<string> GetTablesInvolved(WidgetModel widgetModel)
        //private string GetTablesInvolved(WidgetModel widgetModel)
        //{
        //    List<string> tables = new List<string>();

        //    if(null != widgetModel.Dimension && widgetModel.Dimension.Count() > 0)
        //    {
        //        foreach(var dim in widgetModel.Dimension)
        //        {
        //            var dimName = dim.Name;
        //            var tableName = dimName.Substring(0, dimName.IndexOf("."));
        //            if (!tables.Contains(tableName))
        //            {
        //                tables.Add(tableName);
        //            }
        //        }
        //    }

        //    if (null != widgetModel.Measure && widgetModel.Measure.Count() > 0)
        //    {
        //        var replacementStrings = new string[5] { "sum", "count", "avg", "(", ")" };
        //        foreach (var measure in widgetModel.Measure)
        //        {
        //            var expression = measure.Expression;
        //            if (string.IsNullOrWhiteSpace(expression)) { continue; }
        //            foreach(var r in replacementStrings)
        //            {
        //                expression =  expression.Replace(r, "");
        //            }
        //            expression = expression.Trim();
        //            //var measure = dim.Name;
        //            var tableName = expression.Substring(0, expression.IndexOf("."));
        //            if (!tables.Contains(tableName))
        //            {
        //                tables.Add(tableName);
        //            }
        //        }
        //    }

        //    if (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0)
        //    {
        //        foreach (var filter in widgetModel.FilterList)
        //        {
        //            var colName = filter.ColName;
        //            var tableName = colName.Substring(0, colName.IndexOf("."));
        //            filter.TableName = tableName;
        //            if (!tables.Contains(tableName))
        //            {
        //                tables.Add(tableName);
        //            }
        //        }
        //    }

        //    //Get Table Objects and string for hash key
        //    var tableList = new List<Table>();
        //    var tableKey = string.Empty;
        //    //tables = tables.Select(t=>t.).OrderBy
        //    foreach ( var t in tables)
        //    {
        //       var table =  dashboard.Tables.Where(tbl => tbl.Name == t).FirstOrDefault();
        //        if (null != table)
        //        {
        //            tableList.Add(table);
        //            //
        //        }
        //    }
        //    tableList = tableList.OrderBy(t => t.Id).ToList();
        //    foreach (var t in tableList)
        //    {
        //        tableKey += t.Id + ",";
        //    }
        //        tableKey = tableKey.Substring(0, tableKey.LastIndexOf(","));

        //    //return tables;
        //    return tableKey;
        //}


        // GET: api/Data
        [Route("api/data/getData")]
        [HttpPost]
        public dynamic GetData(WidgetModel widgetModel)
        {
            //BuildTableMetatdata(widgetModel);

            var dashboard = new Dashboard();

            if (null != MemoryCache.Default.Get("dashboard_" + "app_" + widgetModel.AppId, null))
            {
                dashboard = MemoryCache.Default.Get("dashboard_" + "app_" + widgetModel.AppId, null) as Dashboard;
            }

            if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length == 0)))
            {
                return null;
            }
            if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
            {
                return null;
            }

            var queryEngine = new QueryEngine(dashboard);
            var query = queryEngine.BuildQuery(widgetModel);

            if(null == query)
            {
                return null;
            }

            return GetDataFromDB(query);

            //using (var client = new WebClient())
            //{
            //    var values = new System.Collections.Specialized.NameValueCollection();
            //    values["equery"] = query;

            //    var response = client.UploadValues("http://localhost:4000/getdata","POST", values);

            //    var responseString = System.Text.Encoding.Default.GetString(response);

            //    return Newtonsoft.Json.Linq.JArray.Parse(responseString);
            //   //var t  =Json<string>(responseString);
            //   // t.Content
            //}

            //var dt = QueryExecuteEngine.ExecuteQuery(query, widgetModel.AllColumns);

            //var data = GetJsonObjectFromTable(dt);

            //return data;
            //return "";
        }

        private dynamic GetDataFromDB(string query)
        {
            using (var client = new WebClient())
            {
                var values = new System.Collections.Specialized.NameValueCollection();
                values["equery"] = query;

                var response = client.UploadValues(ConfigurationManager.AppSettings.Get("nodemonetdbappurl") +  "getdata", "POST", values);

                var responseString = System.Text.Encoding.Default.GetString(response);

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Response resp = serializer.Deserialize<Response>(responseString);

                return resp;
                //return Newtonsoft.Json.Linq.JArray.Parse(responseString);

            }
        }

        private dynamic ExecuteQueryFromDB(string query)
        {
            using (var client = new WebClient())
            {
                var values = new System.Collections.Specialized.NameValueCollection();
                values["equery"] = query;

                var response = client.UploadValues(ConfigurationManager.AppSettings.Get("nodemonetdbappurl") + "getdata", "POST", values);

                var responseString = System.Text.Encoding.Default.GetString(response);

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Response resp = serializer.Deserialize<Response>(responseString);

                //return JsonConvert.SerializeObject(resp, Settings);

                return resp;
                //return item; // Newtonsoft.Json.Linq.JObject.Parse(responseString);
            }
        }


        [Route("api/data/getTotalRecordsCount")]
        [HttpPost]
        public dynamic GetTotalRecordsCount(WidgetModel widgetModel)
        {
            //var recordsCount = 0;
            //BuildTableMetatdata(widgetModel);
            var dashboard = new Dashboard();

            if (null != MemoryCache.Default.Get("dashboard_" + "app_" + widgetModel.AppId, null))
            {
                dashboard = MemoryCache.Default.Get("dashboard_" + "app_" + widgetModel.AppId, null) as Dashboard;               
            }

            if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length == 0)))
            {
                return null;
            }
            if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
            {
                return null;
            }

            widgetModel.IsRecordCountReq = true;

            var queryEngine = new QueryEngine(dashboard);
            var query = queryEngine.BuildTotalRecordsCountQuery(widgetModel);

            if (null == query)
            {
                return null;
            }
            //using (var client = new WebClient())
            //{
            //    var values = new System.Collections.Specialized.NameValueCollection();
            //    values["equery"] = query;

            //    var response = client.UploadValues("http://localhost:4000/getdata", "POST", values);

            //    var responseString = System.Text.Encoding.Default.GetString(response);

            //    return Newtonsoft.Json.Linq.JArray.Parse(responseString);
            //    //var t  =Json<string>(responseString);
            //    // t.Content
            //}

            return GetDataFromDB(query);

            //return recordsCount;
        }


        private dynamic GetJsonObjectFromTable(System.Data.DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            //var dt = ds.Tables[0];
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName.Trim(), dr[col].ToString().Trim('"'));
                }
                rows.Add(row);
            }
            //var tt = Json(serializer.Serialize(rows));
            return Json(rows);
        }


        //// GET: api/Data
        //[Route("api/data/getDataWorkingWithMaridDB")]
        //[HttpPost]
        //public IEnumerable<dynamic> GetDataWorkingWithMariaDB(WidgetModel widgetModel)
        //{
        //    //BuildTableMetatdata(widgetModel);

        //    IEnumerable<dynamic> data = null;
        //    Query query = null;
        //    var connection = new MySqlConnection(
        //     "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
        //    );
        //    var db = new QueryFactory(connection, new MySqlCompiler());

        //    //if (widgetModel.Type == "kpi"){
        //    var measures = new System.Text.StringBuilder();
        //    if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length ==0)))
        //    {
        //        return null;
        //    }
        //    if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
        //    {
        //        return null;
        //    }

        //    var measuresString = string.Empty;
        //    var dimString = string.Empty;

        //    if (null != widgetModel.Measure)
        //    {
        //        foreach (var measure in widgetModel.Measure)
        //        {
        //            if (string.IsNullOrWhiteSpace(measure.Expression)) { continue; }

        //            // var expDisplayName = !string.IsNullOrWhiteSpace(measure.DisplayName) ? measure.DisplayName : '[' + measure.Expression + ']';
        //            if (!string.IsNullOrWhiteSpace(measure.DisplayName))
        //            {
        //                measures.Append(string.Format("{0} as {1} ", measure.Expression.Trim(), measure.DisplayName.Trim()) + ", ");
        //            }
        //            else
        //            {
        //                measures.Append(string.Format("{0} ", measure.Expression.Trim()) + ", ");
        //            }

        //        }

        //        measuresString = measures.ToString();
        //        measuresString = measuresString.Remove(measuresString.LastIndexOf(','), 1);
        //    }
            

        //    var dims = new System.Text.StringBuilder();

        //    if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0)
        //    {
        //        foreach (var dim in widgetModel.Dimension)
        //        {
        //            // var expDisplayName = !string.IsNullOrWhiteSpace(measure.DisplayName) ? measure.DisplayName : '[' + measure.Expression + ']';
        //            if (!string.IsNullOrWhiteSpace(dim.Name))
        //            {
        //                dims.Append(dim.Name.Trim() + " as '" + dim.Name.Trim() + "',");
        //            }                    
        //        }
        //        dimString = dims.ToString();
        //        dimString = dimString.Remove(dimString.LastIndexOf(','), 1);
        //    }

        //    query = GetTablesAssociationQuery(widgetModel);

        //    //Todo: widgetModel.SqlTableName
        //    if (widgetModel.Type == "filter")
        //        {
        //            query = query.Select(dimString).Distinct();
        //            //data = db.Query("employee").Select(dimString).Distinct().Get();
                   
        //        } else //if (widgetModel.Type == "kpi")
        //        {
        //            if (!string.IsNullOrWhiteSpace(dimString))
        //            {
        //                query = query.SelectRaw(dimString);
        //            }
        //            if (!string.IsNullOrWhiteSpace(measuresString))
        //            {
        //                query = query.SelectRaw(measuresString);
        //            }

        //            if(!string.IsNullOrWhiteSpace(dimString)  && !string.IsNullOrWhiteSpace(measuresString))
        //            {
                    
        //                query = query.GroupBy( widgetModel.Dimension.Select(d => d.Name).ToArray<string>());
        //            }
                
        //                //data = db.Query("employee").SelectRaw(measuresString).Get();
        //        }
        //    var constraints = new Dictionary<string, object>();
            

        //    if (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0)
        //    {
        //        foreach(var filter in widgetModel.FilterList)
        //        {
        //           // constraints.Add(filter.ColName, filter.Values.to);
        //            query = query.WhereIn(filter.ColName, filter.Values);
        //        }
                
        //    }

        //    data = query.Get();
            
        //    //query.comp

        //    return data;
        //}



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
        public void SavePageLayout(PageLayoutModel pageLayoutModel)
        {

            //PageData = pageLayoutModel.Layout;

            var associationsPath = Common.GetAppPath("app_" + pageLayoutModel.AppId);
            associationsPath += @"\page_" + pageLayoutModel.PageId.ToString() + ".pgl";
            var compressionHelper = new CompressionHelper<object>();
            compressionHelper.CompressAndSaveJsonLZ4(pageLayoutModel.Layout, associationsPath);
        }

        [Route("api/data/getPageData")]
        [HttpPost]
        public object GetPageData(dynamic criteria)
        {
            var appId = criteria.appId.Value;
            var pageId = criteria.pageId.Value;
            var appTitle = criteria.appTitle.Value;
            //if (null != PageData)
            //{
            //    PageData.Where(p=>p.pageId == pageId)
            //}

            //BuildTableMetatdata("app_" + appId);
            BuildTableMetatdata(appId);

            var pagePath = Common.GetAppPath("app_"+ appId);
            pagePath += @"\page_" + pageId.ToString() + ".pgl";

            if (File.Exists(pagePath))
            {
                var compressionHelper = new CompressionHelper<object>();
                var pageLayout = compressionHelper.ReadCompressJsonLZ4(pagePath);                
                return pageLayout;
            }
            return null;

            //return PageData;
        }


        [Route("api/data/getPageDataWithMetaData")]
        [HttpPost]
        public object GetPageDataWithMetaData(dynamic criteria)
        {
            var appId = criteria.appId.Value;
            var pageId = criteria.pageId.Value;
            var appTitle = criteria.appTitle.Value;
            //if (null != PageData)
            //{
            //    PageData.Where(p=>p.pageId == pageId)
            //}

            //BuildTableMetatdata("app_" + appId);
            BuildTableMetatdata(appId);

            var pagePath = Common.GetAppPath("app_" + appId);
            pagePath += @"\page_" + pageId.ToString() + ".pgl";

            if (File.Exists(pagePath))
            {
                var compressionHelper = new CompressionHelper<object>();
                var pageLayout = compressionHelper.ReadCompressJsonLZ4(pagePath);

                AppModel app = GetAppById(appId);
                var page = app.Pages.Where(p => p.Id.ToString() == pageId).FirstOrDefault();
                if (null != page)
                {
                    pageLayout.page = page;
                }
                return pageLayout;
            }
            return null;

            //return PageData;
        }


        [Route("api/data/getPageMetaData")]
        [HttpPost]
        public Page GetPageMetaData(dynamic criteria)
        {
            var appId = criteria.appId.Value;
            var pageId = criteria.pageId.Value;
            var appTitle = criteria.appTitle.Value;
            //if (null != PageData)
            //{
            //    PageData.Where(p=>p.pageId == pageId)
            //}

            //BuildTableMetatdata("app_" + appId);
            BuildTableMetatdata(appId);

            var pagePath = Common.GetAppPath("app_" + appId);

            AppModel app = GetAppById(appId);
            var page = app.Pages.Where(p => p.Id.ToString() == pageId).FirstOrDefault();

            return page;

            
        }

        //[Route("api/data/getAllPagesOfApp")]
        //[HttpGet]
        //public List<Page> GetAllPagesOfApp(string appId)
        //{
        //    var pages = new List<Page>();
        //    var appPath = Common.GetAppPath("app_" + appId);
        //    if (Directory.Exists(appPath))
        //    {
        //        var pageFiles = Directory.GetFiles(appPath, "*.pgl");
        //        foreach(var pagePath in pageFiles)
        //        {
        //            var page = new Page();
        //            page.Id  = pagePath
        //        }

        //    }

        //    var associationsPath = Common.GetAppPath("app_" + appId);
        //    associationsPath += @"\page_" + pageId.ToString() + ".pgl";

        //    //if (File.Exists(associationsPath))
        //    //{
        //    //    var compressionHelper = new CompressionHelper<object>();
        //    //    var pageLayout = compressionHelper.ReadCompressJsonLZ4(associationsPath);
        //    //    BuildTableMetatdata("app_" + appId);
        //    //    return pageLayout;
        //    //}
        //    //return null;

        //    return pages;
        //}

        [Route("api/data/getTables")]
        [HttpPost]
        public dynamic GetTables()
        {
            var tables = GetDataFromDB("select tables.id, tables.name from tables where tables.system=false");
            //if (null != MemoryCache.Default.Get("dashboard", null))
            //{
            //    dashboard = MemoryCache.Default.Get("dashboard", null) as Dashboard;               
            //}

            //if (null ==  dashboard) { dashboard = new Dashboard(); }
            //if(null == dashboard.Tables || (null != dashboard.Tables && dashboard.Tables.Count() == 0))
            //{
            //    var tablesObject = tables.ToObject<List<object>>();
            //    foreach(var o in tablesObject)
            //    {
            //        dashboard.AddTable(new Table() { Name = o.name });
            //    }
            //}

            //MemoryCache.Default.Set("tables", dashboard.Tables, null, null);
            ////dashboard.AddTable(new Table() { Name = "employee1" });
            ////dashboard.AddTable(new Table() { Name = "skills1" });
            return tables;
        }

        [Route("api/data/getColumns")]
        [HttpGet]
        public dynamic GetColumns(string tableName)
        {
            return GetDataFromDB("select * from sys.columns where table_id = (select id from tables where name ='" + tableName + "')");
        }

        [Route("api/data/getExistingTableAssociation")]
        [HttpGet]
        public dynamic GetExistingTableAssociation(string appTitle,string tableName1,string tableName2)
        {
            var associations = new List<Association>();
            if (null != TableAssociations)
            {
                associations = TableAssociations;
            } else
            {
                associations = GetAllTableAssociations(appTitle);
            }
            if(null  != tableName1 && null != tableName2)
            {
               var association = associations.Where(a => a.TableName == tableName1).FirstOrDefault();
                if(null != association)
                {
                    var newAssociation = new Association();
                    association.TableName = tableName1;
                    association.Relations =  association.Relations.Where(r => r.TableName2 == tableName2).ToList();
                    return association;
                }
            } else if (null != tableName1)
            {
                return associations.Where(a => a.TableName == tableName1).FirstOrDefault();
            }
            else if (null != tableName2)
            {
                return associations.Where(a => a.TableName == tableName2).FirstOrDefault();
            }
            return null;
        }

        [Route("api/data/saveTableAssociation")]
        [HttpPost]
        public void SaveTableAssociation(AssociationModel associationModel)
        {           
            var dashboard = new Dashboard();

            if (null != MemoryCache.Default.Get("dashboard_" + "app_" + associationModel.AppId, null))
            {
                dashboard = MemoryCache.Default.Get("dashboard_" + "app_" + associationModel.AppId, null) as Dashboard;
            } else
            {
                //Build dashboard
                BuildTableMetatdata(associationModel.AppId.ToString());
                if (null != MemoryCache.Default.Get("dashboard_" + "app_" + associationModel.AppId, null))
                {
                    dashboard = MemoryCache.Default.Get("dashboard_" + "app_" + associationModel.AppId, null) as Dashboard;
                }
            }

            
            var associations = dashboard.Associations;
            if(null == associations)
            {
                associations = new List<Association>();
            }
            //var 
            var association = associationModel.Association;
            var existingAssociation = associations.Where(a => a.TableName == association.TableName).FirstOrDefault();
            if(null != existingAssociation)
            {
               var existRel = existingAssociation.Relations.Where(r => r.TableName2 == association.Relations[0].TableName2).FirstOrDefault();
                if(null != existRel)
                {
                    existRel.Keys = association.Relations[0].Keys;
                } else
                {
                    existingAssociation.Relations.Add(association.Relations[0]);                  
                }
            } else
            {
                associations.Add(association);
            }

            

            if (null != association.Relations && association.Relations.Count() > 0)
            {
                var newAssociation = new Association();
                newAssociation.TableName = association.Relations[0].TableName2;
                var rel = new Relation();
                rel.TableName2 = association.TableName;
                rel.Keys = association.Relations[0].Keys;
                newAssociation.Relations.Add(rel); //clone it 
                existingAssociation = associations.Where(a => a.TableName == newAssociation.TableName).FirstOrDefault();
                if (null != existingAssociation)
                {
                    var existRel = existingAssociation.Relations.Where(r => r.TableName2 == newAssociation.Relations[0].TableName2).FirstOrDefault();
                    if (null != existRel)
                    {
                        existRel.Keys = newAssociation.Relations[0].Keys;
                    }
                    else
                    {
                        existingAssociation.Relations.Add(newAssociation.Relations[0]);
                    }
                }
                else
                {
                    associations.Add(newAssociation);
                }

                //associations.Add(newAssociation);
            }
            //TableAssociations = associations;

            //Save Associations in file.
            var associationsPath = Common.GetAppPath("app_" + associationModel.AppId);
            associationsPath += @"\association.assoc";
            var compressionHelper = new CompressionHelper<List<Association>>();
            compressionHelper.CompressAndSaveLZ4(associations, associationsPath);

            MemoryCache.Default.Remove("dashboard_" + "app_" + associationModel.AppId, null);
        }

        [Route("api/data/saveApp")]
        [HttpPost]
        public void SaveApp(AppModel app)//, AssociationModel newAssociationModel)
        {
           // MemoryCache.Default.Remove("dashboard", null);

            MemoryCache.Default.Remove("dashboard_" + "app_" + app.Id, null);

            //var associations = app.Associations; //TableAssociations;
            //if (null == associations)
            //{
            //    associations = new List<Association>();
            //}
            ////var 
            //var association = newAssociationModel.Association;
            //var existingAssociation = associations.Where(a => a.TableName == association.TableName).FirstOrDefault();
            //if (null != existingAssociation)
            //{
            //    var existRel = existingAssociation.Relations.Where(r => r.TableName2 == association.Relations[0].TableName2).FirstOrDefault();
            //    if (null != existRel)
            //    {
            //        existRel.Keys = association.Relations[0].Keys;
            //    }
            //    else
            //    {
            //        existingAssociation.Relations.Add(association.Relations[0]);
            //    }
            //}
            //else
            //{
            //    associations.Add(association);
            //}



            //if (null != association.Relations && association.Relations.Count() > 0)
            //{
            //    var newAssociation = new Association();
            //    newAssociation.TableName = association.Relations[0].TableName2;
            //    var rel = new Relation();
            //    rel.TableName2 = association.TableName;
            //    rel.Keys = association.Relations[0].Keys;
            //    newAssociation.Relations.Add(rel); //clone it 
            //    existingAssociation = associations.Where(a => a.TableName == newAssociation.TableName).FirstOrDefault();
            //    if (null != existingAssociation)
            //    {
            //        var existRel = existingAssociation.Relations.Where(r => r.TableName2 == newAssociation.Relations[0].TableName2).FirstOrDefault();
            //        if (null != existRel)
            //        {
            //            existRel.Keys = newAssociation.Relations[0].Keys;
            //        }
            //        else
            //        {
            //            existingAssociation.Relations.Add(newAssociation.Relations[0]);
            //        }
            //    }
            //    else
            //    {
            //        associations.Add(newAssociation);
            //    }

            //    //associations.Add(newAssociation);
            //}
            ////TableAssociations = associations;

            //Get appHeader
            var appHeader = new AppHeader();
            appHeader.Id = app.Id;
            appHeader.Title = app.Title;
            appHeader.Tables = app.Tables;
            appHeader.Pages = app.Pages;

            //Save app Header in file.
            var appTitle = "app_" + app.Id;
            var apPath = Common.GetAppPath(appTitle);
            apPath += @"\"  + appTitle + ".header";
            var compressionHelper = new CompressionHelper<AppHeader>();
            compressionHelper.CompressAndSaveLZ4(appHeader, apPath);

            ////Save Associations in file.
            //var associationsPath = Common.GetAppPath(appTitle);
            //associationsPath += @"\association.assoc";
            //var assocCompressionHelper = new CompressionHelper<List<Association>>();
            //assocCompressionHelper.CompressAndSaveLZ4(associations, associationsPath);
        }


        [Route("api/data/createNewApp")]
        [HttpPost]
        public void CreateNewApp(AppModel app)
        {
            var appFileName = "app_" + app.Id;
            var appPath = Common.GetAppPath(appFileName);
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }

            //Save App header File
            var appHeaderPath = appPath + @"\" + appFileName +".header";            
            var compressionHelper = new CompressionHelper<AppModel>();
            compressionHelper.CompressAndSaveLZ4(app, appHeaderPath);

        }

        [Route("api/data/createNewPage")]
        [HttpPost]
        public string CreateNewPage(dynamic page)
        {
            var appId = page.appId.Value;
            var pageId = page.id.Value;
            var pageTitle = page.title.Value;
            var appFileName = "app_" + appId;
            var appPath = Common.GetAppPath(appFileName);
            if (Directory.Exists(appPath))
            {
                AppModel app = GetAppById(Convert.ToString(appId));
                if(null != app)
                {
                    var pg = new Page();
                    pg.Id = Convert.ToInt32(pageId);
                    pg.Title = Convert.ToString(pageTitle);

                    app.Pages.Add(pg);
                }
                //Save App header File
                var appHeaderPath = appPath + @"\" + appFileName + ".header";
                var compressionHelper = new CompressionHelper<AppModel>();
                compressionHelper.CompressAndSaveLZ4(app, appHeaderPath);

                return JsonConvert.SerializeObject(app, Settings);
                //return app;
            }
            return null;
        }

        [Route("api/data/getAppById")]
        [HttpGet]
        public AppModel GetAppById(string appId)
        {
            var dashboardPath = Common.GetFilePath();
            if (Directory.Exists(dashboardPath))
            {
                var appPath = dashboardPath + @"\" + "app_" + appId;
                //Read App header File
                var appHeaderPath = appPath + @"\" + "app_" + appId + ".header";
                var compressionHelper = new CompressionHelper<AppModel>();
                var app = compressionHelper.ReadCompressSharpLZ4(appHeaderPath);
                //return JsonConvert.SerializeObject(app, Settings);
                return app;                
            }
            return null;
        }

        [Route("api/data/deleteApp")]
        [HttpPost]
        public string DeleteApp(string appId)
        {
            var dashboardPath = Common.GetFilePath();
            if (Directory.Exists(dashboardPath))
            {
                var appPath = dashboardPath + @"\" + "app_" + appId;
                if (Directory.Exists(appPath))
                {
                    Directory.Delete(appPath,true);
                }
            }

            return LoadApps();
        }

        [Route("api/data/deletePage")]
        [HttpPost]
        public string DeletePage(string appId, string pageId)
        {
            var dashboardPath = Common.GetFilePath();
            if (Directory.Exists(dashboardPath))
            {
                var appPath = dashboardPath + @"\" + "app_" + appId;
                var pagePath = dashboardPath + @"\" + "app_" + appId + @"\" + "page_" + pageId + ".pgl";

                if (Directory.Exists(appPath) && File.Exists(pagePath))
                {
                    File.Delete(pagePath);                   
                }
                 //Get App header and updte it by removing this page data.
                    var app = GetAppById(appId);
                    app.Pages = app.Pages.Where(p => p.Id.ToString() != pageId).ToList();
                    //Save app header back on file.
                    SaveApp(app);
            }

            return LoadApps();
        }

        [Route("api/data/getAppByIdAsString")]
        [HttpGet]
        public string GetAppByIdAsString(string appId)
        {
            var dashboardPath = Common.GetFilePath();
            if (Directory.Exists(dashboardPath))
            {
                var appPath = dashboardPath + @"\" + "app_" + appId;
                //Read App header File
                var appHeaderPath = appPath + @"\" + "app_" + appId + ".header";
                var compressionHelper = new CompressionHelper<AppModel>();
                var app = compressionHelper.ReadCompressSharpLZ4(appHeaderPath);
                //return JsonConvert.SerializeObject(app, Settings);
                //return app;

                return JsonConvert.SerializeObject(app, Settings);
            }
            return null;
        }

        [Route("api/data/loadApps")]
        [HttpGet]
        public string LoadApps()
        {
            var appList = GetApps();
            return JsonConvert.SerializeObject(appList, Settings);

        }

        private List<AppModel> GetApps()
        {
            var appList = new List<AppModel>();
            var dashboardPath = Common.GetFilePath();
            if (Directory.Exists(dashboardPath))
            {
                var directories = Directory.EnumerateDirectories(dashboardPath);
                foreach (var dir in directories)
                {
                    var appName = dir.Substring(dir.LastIndexOf(@"\"));
                    //Read App header File
                    var appHeaderPath = dir + @"\" + appName + ".header";
                    var compressionHelper = new CompressionHelper<AppModel>();
                    appList.Add(compressionHelper.ReadCompressSharpLZ4(appHeaderPath));
                }
            }
            return appList;
        }

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        [Route("api/data/getColumnsDatatype")]
        [HttpPost]
        public List<ColumnDetail> GetColumnsDataType() //System.Web.HttpPostedFileBase file
        {
            List<ColumnDetail> columnDetailList = new List<ColumnDetail>();
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (null != file)
            {
                char delimiter = Convert.ToChar(HttpContext.Current.Request.Params["delimiter"]);
                var tableName = HttpContext.Current.Request.Params["tablename"];
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    tableName = file.FileName.Replace(" ", "_").Replace(".", "_");
                }                
                var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/"),
                                       System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);

                var directory = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/");
                DataTable dt;

                switch (delimiter)
                {
                    case ',':
                        dt = ReadCommaSeparatedFile(path);
                        break;
                    default:
                        dt = ReadCustomDelimiterFile(path, delimiter);
                        break;
                }
                dt.TableName = tableName;
                            
                foreach (DataColumn col in dt.Columns)
                {
                    var colDtl = new ColumnDetail();
                    colDtl.Name = col.ColumnName;
                    colDtl.CType = SQLGetType(col);
                    columnDetailList.Add(colDtl);
                }
            }
            return columnDetailList;
        }

        [Route("api/data/addData")]
        [HttpPost]
        public Response AddData(ImportedFileModel importedFileModel)
        {
            char delimiter = Convert.ToChar(importedFileModel.Delemiter);
            var tableName = importedFileModel.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = importedFileModel.FileName.Replace(" ", "_").Replace(".", "_");
            }

            var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/"), importedFileModel.FileName);
            var directory = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/");
            DataTable dt;

            switch (delimiter)
            {
                case ',':
                    dt = ReadCommaSeparatedFile(path);
                    break;
                default:
                    dt = ReadCustomDelimiterFile(path, delimiter);
                    break;
            }
          
            var query = CreateTableFromColumns(importedFileModel.Columns, tableName);

            //Create table
            Response result =  ExecuteQueryFromDB(query);

            if(result.Status.ToLower() == "failed")
            {
                return result;
            }

            var importDataQuery = "COPY OFFSET 2 INTO \"" + tableName + "\" FROM '" + path + "' USING DELIMITERS '" + delimiter + "','\n','\"' NULL AS '';";
            //var importDataQuery = "COPY OFFSET 2 INTO " + tableName + " FROM '" + path + "' USING DELIMITERS '\t';";
            importDataQuery = importDataQuery.Replace("\\", "/");
            //Import Data
            Response importResult = ExecuteQueryFromDB(importDataQuery);

            return importResult;

        }

        [Route("api/data/importTable")]
        [HttpPost]
        public void ImportTable() //System.Web.HttpPostedFileBase file
        {           
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (null != file)
            {
                char delimiter = Convert.ToChar(HttpContext.Current.Request.Params["delimiter"]);
                var tableName = HttpContext.Current.Request.Params["tablename"];
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    tableName = file.FileName.Replace(" ", "_").Replace(".", "_");
                }
                //System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/ImportElements/");
                var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/"),
                                       System.IO.Path.GetFileName(file.FileName));

                //var path = "";// Path.Combine(_env.WebRootPath,"upload", file.FileName);


                file.SaveAs(path);

                var directory = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/");
                DataTable dt;

                switch (delimiter)
                {
                    case ',':
                        dt = ReadCommaSeparatedFile(path);
                        break;                    
                    default:
                        dt = ReadCustomDelimiterFile(path, delimiter);
                        break;
                }

                ////DataTable dt = ReadCustomDelimiterFile(path);

                //System.Data.DataSet ds = new System.Data.DataSet();
                //string strConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + directory + "\";Extended Properties = 'text;HDR=Yes;FMT=Delimited(,)'; ";
                ////string strConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + directory + "\";Extended Properties = 'text;HDR=Yes;FMT=TabDelimited'; ";
                ////"Driver={Microsoft Text Driver (*.txt; *.csv)}; Dbq=" + path + "; Extensions=asc,csv,tab,txt;Persist Security Info=False";
                //string sql_select;

                //System.Data.OleDb.OleDbConnection conn;
                //conn = new System.Data.OleDb.OleDbConnection(strConnString.Trim());
                //conn.Open();

                ////Creates the select command text

                //sql_select = "select top 200 * from [" + file.FileName + "]";

                ////Creates the data adapter
                //System.Data.OleDb.OleDbDataAdapter obj_oledb_da = new System.Data.OleDb.OleDbDataAdapter(sql_select, conn);

                ////Fills dataset with the records from CSV file
                //obj_oledb_da.Fill(ds, file.FileName.Replace(string.Empty, "_").Replace(".", "_"));
                dt.TableName = tableName;
                //var tableName = dt.TableName;
                var query = CreateTableFromDataTable(tableName, dt);

                //Create table
                ExecuteQueryFromDB(query);

                var importDataQuery = "COPY OFFSET 2 INTO " + tableName + " FROM '" + path + "' USING DELIMITERS '" + delimiter + "';";
                //var importDataQuery = "COPY OFFSET 2 INTO " + tableName + " FROM '" + path + "' USING DELIMITERS '\t';";
                importDataQuery = importDataQuery.Replace("\\","/");
                //Import Data
                ExecuteQueryFromDB(importDataQuery);
                

            }
        }

        private DataTable ReadCommaSeparatedFile(string filePath)
        {
            DataTable dt = new DataTable();
            var directory = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/");
            var fileName = filePath.Substring(filePath.LastIndexOf(@"\")+1);
            System.Data.DataSet ds = new System.Data.DataSet();
            string strConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + directory + "\";Extended Properties = 'text;HDR=Yes;FMT=Delimited(,)'; ";
            //string strConnString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"" + directory + "\";Extended Properties = 'text;HDR=Yes;FMT=TabDelimited'; ";
            //"Driver={Microsoft Text Driver (*.txt; *.csv)}; Dbq=" + path + "; Extensions=asc,csv,tab,txt;Persist Security Info=False";
            string sql_select;

            System.Data.OleDb.OleDbConnection conn;
            conn = new System.Data.OleDb.OleDbConnection(strConnString.Trim());
            conn.Open();

            //Creates the select command text

            sql_select = "select top 200 * from [" + fileName + "]";

            //Creates the data adapter
            System.Data.OleDb.OleDbDataAdapter obj_oledb_da = new System.Data.OleDb.OleDbDataAdapter(sql_select, conn);

            //Fills dataset with the records from CSV file
            obj_oledb_da.Fill(ds, "csv");

            return ds.Tables[0];
        }

        private DataTable ReadCustomDelimiterFile(string filePath, char delimiter)
        {
            DataTable dt = new DataTable();
            //DataTable dtCloned = null;
            string[] lines = System.IO.File.ReadAllLines(filePath);
            var linesCountToDetermineDataType = lines.Length;
            if(linesCountToDetermineDataType > 200)
            {
                linesCountToDetermineDataType = 200;
            }
            for (int i = 0; i < linesCountToDetermineDataType; i++)
            {
                DataRow toInsert = dt.NewRow();
                if (i==0)
                {
                    string[] columnarray = lines[i].Trim().Split(delimiter);
                    for (int j = 0; j < columnarray.Length; j++)
                    {
                        dt.Columns.Add(columnarray[j]);
                    }
                }
                else if (i < lines.Length && i > 0)
                {
                    var h = i;
                    string[] rowarray = lines[i].Trim().Split(delimiter);
                    for (int k = 0; k < rowarray.Length; k++)
                    {
                        //System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(rowarray[k], @"\p{C}");

                        toInsert[k] = rowarray[k];
                    }
                    dt.Rows.InsertAt(toInsert, h);
                    h++;
                }
                else if (lines[i].Trim() == "END - OF - DATA")
                {
                    break;
                }
                else
                {
                    continue;
                }
            }


            DataTable dtCloned = dt.Clone();
            //Determine datatType of each column
            foreach (DataColumn  col in dt.Columns)
            {
                List<dataType> dataTypes = new List<dataType>();
                foreach(DataRow row in dt.Rows)
                {
                    var data = row[col.ColumnName];
                    if(null != data)
                    {
                        var type = Common.ParseString(data.ToString());
                        if (!dataTypes.Contains(type))
                        {
                            dataTypes.Add(type);
                        }
                    }
                    if (dataTypes.Count() > 1)
                    {
                        break;
                    }
                }

                if(dataTypes.Count() == 1)
                {                    
                    var clonedCol = dtCloned.Columns[col.ColumnName];
                    switch (dataTypes[0])
                    {
                        case dataType.System_Boolean:
                            clonedCol.DataType = typeof(bool);
                            break;
                        case dataType.System_DateTime:
                            clonedCol.DataType = typeof(DateTime);
                            break;
                        case dataType.System_Double:
                            clonedCol.DataType = typeof(Double);
                            break;
                        case dataType.System_Int32:
                            clonedCol.DataType = typeof(Int32);
                            break;
                        case dataType.System_Int64:
                            clonedCol.DataType = typeof(Int64);
                            break;
                        default:
                            clonedCol.DataType = typeof(String);
                            break;
                    }                    
                }
            }

            return dtCloned;
        }



        public static string CreateTableFromDataTable(string tableName, DataTable table)
        {
            string sql = "CREATE TABLE \"" + tableName + "\" ("; //\n";
            // columns
            foreach (DataColumn column in table.Columns)
            {
                //sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
                sql += "\"" + column.ColumnName.ToLower() + "\" " + SQLGetType(column) + " NULL ,";
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' });// + "\n";
            //sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";
            // primary keys
            //if (table.PrimaryKey.Length > 0)
            //{
            //    sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
            //    foreach (DataColumn column in table.PrimaryKey)
            //    {
            //        sql += "[" + column.ColumnName + "],";
            //    }
            //    sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
            //}
            //else
            sql += ")";
            return sql;
        }


        public static string CreateTableFromColumns(List<ColumnDetail> columns, string tableName)
        {
            string sql = "CREATE TABLE \"" + tableName + "\" ("; //\n";
            // columns
            foreach (var column in columns)
            {
                //sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
                sql += "\"" + column.Name.ToLower() + "\" " + column.CType + " NULL ,";
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' });// + "\n";
            //sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";
            // primary keys
            //if (table.PrimaryKey.Length > 0)
            //{
            //    sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
            //    foreach (DataColumn column in table.PrimaryKey)
            //    {
            //        sql += "[" + column.ColumnName + "],";
            //    }
            //    sql = sql.TrimEnd(new char[] { ',' }) + "))\n";
            //}
            //else
            sql += ")";
            return sql;
        }

        public static string SQLGetType(DataColumn column)
        {
            return GetSqlType(column.DataType, column.MaxLength, 10, 2);
        }

        public static string GetSqlType(object type, int columnSize, int numericPrecision, int numericScale)
        {           
            switch (type.ToString())
            {
                case "System.Byte[]":
                case "varchar":
                case "mediumtext":
                case "longtext":
                case "varbinary":
                case "character varying":
                    return "STRING";

                case "bytea":
                    return "BLOB";



                case "System.Boolean":
                case "bit":
                case "boolean":
                    return "BOOL";



                case "System.DateTime":
                case "timestamp":
                case "timestamp without time zone":
                    return "timestamp";
                    //return "STRING";



                case "System.DateTimeOffset":
                    return "DATETIMEOFFSET";



                case "System.Decimal":
                case "decimal":
                    if (numericPrecision != -1 && numericScale != -1)
                        return "DECIMAL(" + numericPrecision + "," + numericScale + ")";
                    else
                        return "DECIMAL";



                case "System.Double":
                    return "DOUBLE";



                case "System.Single":
                    return "REAL";



                case "System.Int64":
                    return "BIGINT";



                case "System.Int32":
                case "int": //mysql
                case "integer": //postgres
                    return "INT";



                case "System.Int16":
                case "smallint":
                    return "SMALLINT";



                case "System.String":
                    //return "NVARCHAR(" + ((columnSize == -1 || columnSize > 8000) ? "MAX" : columnSize.ToString()) + ")";
                    return "STRING";



                case "System.Byte":
                    return "TINYINT";



                case "System.Guid":
                    return "UNIQUEIDENTIFIER";



                default:
                    throw new Exception(type.ToString() + " not implemented.");
            }
        }

        private static List<Association> GetAllTableAssociations(string appTitle)
        {            
            //if(null != TableAssociations)
            //{
            //    return TableAssociations;
            //} else
            //{
                //Read from File
                var associationsPath = Common.GetAppPath(appTitle);
                associationsPath += @"\association.assoc";
                var compressionHelper = new CompressionHelper<List<Association>>();
                if (File.Exists(associationsPath))
                {
                    return compressionHelper.ReadCompressSharpLZ4(associationsPath);
                }
                return new List<Association>();
            //}
            //var associations = new List<Association>();
            //var association = new Association();
            
            //association.TableName = "employee1";
            //var relations = new List<Relation>();
            //var rel = new Relation() { TableName2 = "skills1", Type = "left" };
            //rel.Keys = new List<List<string>>();
            //var keys = new List<string>();
            //keys.Add("employee1.empid");
            //keys.Add("skills1.empid");
            //rel.Keys.Add(keys);
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            //association = new Association();
            //association.TableName = "Skills1";
            //relations = new List<Relation>();
            //rel = new Relation() { TableName2 = "employee1", Type = "left" , Keys = new List<List<string>>() };
            //keys = new List<string>();
            //keys.Add("employee1.empid");
            //keys.Add("skills1.empid");
            //rel.Keys.Add(keys);
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            ////association.TableName = "Product";
            ////var relations = new List<Relation>();
            ////var rel = new Relation() { TableName2 = "ProductInventory", Type = "left" };          
            ////var keys = new List<string>();
            ////keys.Add("Product.ProductId");
            ////keys.Add("ProductInventory.ProductId");
            ////rel.Keys = keys;
            ////rel.Operation = "=";
            ////relations.Add(rel);
            ////var rel2 = new Relation() { TableName2 = "ProductVendor", Type = "left" };
            ////var keys2 = new List<string>();
            ////keys2.Add("Product.ProductId");
            ////keys2.Add("ProductVendor.ProductId");
            ////rel2.Keys = keys2;
            ////rel2.Operation = "=";
            ////relations.Add(rel2);
            ////rel2 = new Relation() { TableName2 = "PurchaseOrderDetail", Type = "left" };
            ////keys2 = new List<string>();
            ////keys2.Add("Product.ProductId");
            ////keys2.Add("PurchaseOrderDetail.ProductId");
            ////rel2.Keys = keys2;
            ////rel2.Operation = "=";
            ////relations.Add(rel2);
            ////association.Relations = relations;
            ////associations.Add(association);

            ////association = new Association();
            ////association.TableName = "ProductInventory";
            ////relations = new List<Relation>();
            ////rel = new Relation() { TableName2 = "Product", Type = "left" };
            ////keys = new List<string>();
            ////keys.Add("Product.ProductId");
            ////keys.Add("ProductInventory.ProductId");
            ////rel.Keys = keys;
            ////rel.Operation = "=";
            ////relations.Add(rel);
            ////association.Relations = relations;
            ////associations.Add(association);

            ////association = new Association();
            ////association.TableName = "ProductVendor";
            ////relations = new List<Relation>();
            ////rel = new Relation() { TableName2 = "Product", Type = "left" };
            ////keys = new List<string>();
            ////keys.Add("Product.ProductId");
            ////keys.Add("ProductVendor.ProductId");
            ////rel.Keys = keys;
            ////rel.Operation = "=";
            ////relations.Add(rel);
            ////association.Relations = relations;
            ////associations.Add(association);

            ////association = new Association();
            ////association.TableName = "PurchaseOrderDetail";
            ////relations = new List<Relation>();
            ////rel = new Relation() { TableName2 = "PurchaseOrderHeader", Type = "left" };
            ////keys = new List<string>();
            ////keys.Add("PurchaseOrderDetail.PurchaseOrderId");
            ////keys.Add("PurchaseOrderHeader.PurchaseOrderId");
            ////rel.Keys = keys;
            ////rel.Operation = "=";
            ////relations.Add(rel);
            ////association.Relations = relations;
            ////associations.Add(association);

            ////association = new Association();
            ////association.TableName = "PurchaseOrderHeader";
            ////relations = new List<Relation>();
            ////rel = new Relation() { TableName2 = "PurchaseOrderDetail", Type = "left" };
            ////keys = new List<string>();
            ////keys.Add("PurchaseOrderDetail.PurchaseOrderId");
            ////keys.Add("PurchaseOrderHeader.PurchaseOrderId");
            ////rel.Keys = keys;
            ////rel.Operation = "=";
            ////relations.Add(rel);
            ////association.Relations = relations;
            ////associations.Add(association);

            //return associations;

        }

        public List<string> GetAllCombinations(Dashboard dashboard)
        {
            //var tables = new List<string>();
            //tables.Add("0");
            //tables.Add("1");
            //tables.Add("2");
            //tables.Add("3");


            IList<string> list = dashboard.Tables.Select(t => t.Id.ToString()).ToList<string>(); ;
            List<string> allCombinations = new List<String>();
            for (int i = 1; i <= list.Count; i++)
            {
                var combis = new Facet.Combinatorics.Combinations<string>(
                    list, i, Facet.Combinatorics.GenerateOption.WithoutRepetition);
                allCombinations.AddRange(combis.Select(c => string.Join(",", c)));
            }

            foreach (var combi in allCombinations)
                Console.WriteLine(combi);

            return allCombinations;
        }


        [Route("api/data/connectOledb")]
        [HttpGet]
        public Response ConnectOLEDB(OdbcModel odbcModel)
        {
            //OdbcModel odbcModel = new OdbcModel();
            ////odbcModel.ConnectionString = "Provider=MariaDB Provider;Data Source=classicmodels;User Id=root;Password=root123;";
            //odbcModel.ConnectionString = "Provider=MariaDB Provider;Data Source=localhost,3306; Initial Catalog=classicmodels;User ID=root; Password=root123;Activation=SJNF-W6LE-W22Z-DRPV"; 

            Response resp = new Response();
            try
            {
                string connString = odbcModel.ConnectionString; // "DSN=vizmysql;Database=classicmodels;Uid=root;Pwd=root123;";
                var databaseName = connString.Substring(connString.IndexOf("Initial Catalog"));
                databaseName = databaseName.Substring(databaseName.IndexOf("=") + 1, databaseName.IndexOf(";") - 16);
                //var dsn = connString.Substring(connString.IndexOf("DSN"));
                //dsn = dsn.Substring(dsn.IndexOf("=") + 1, dsn.IndexOf(";") - 4);

                //var reader = OleDbEnumerator.GetRootEnumerator();
                //string test = "";
                //var list = new List<String>();
                //while (reader.Read())
                //{
                //    for (var i = 0; i < reader.FieldCount; i++)
                //    {
                //        if (reader.GetName(i) == "SOURCES_NAME")
                //        {
                //            list.Add(reader.GetValue(i).ToString());
                //        }
                //    }
                //    test += reader.GetName(0) + "  " + reader.GetValue(0);
                //}
                //reader.Close();

                ////OleDbEnumerator enumerator = new OleDbEnumerator();
                ////var data = enumerator.GetElements();
                ////foreach(OleDbDataReader prov in OleDbEnumerator.GetRootEnumerator())
                ////{
                ////    //prov.GetName()
                ////}

                var dsn = "mysql";
                var tableListQuery = "";
                if (dsn.Contains("mysql"))
                {
                    tableListQuery = "SELECT table_name FROM information_schema.tables where table_schema = '" + databaseName + "'; ";
                }
                else if (dsn.Contains("postgres"))
                {
                    tableListQuery = "SELECT tablename as table_name FROM pg_catalog.pg_tables  where schemaname = 'public';";
                }

                using (OleDbConnection con = new OleDbConnection(connString))
                {
                    OleDbCommand cmd = new OleDbCommand(tableListQuery, con);
                    //Postgresql = SELECT tablename FROM pg_catalog.pg_tables  where schemaname = 'public';
                    OleDbDataAdapter da = new OleDbDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    DataTable dataTable = ds.Tables[0];
                    resp.Data = dataTable;
                    resp.Status = "success";
                }
            }
            catch (Exception ex)
            {
                resp.Status = "failed";
                resp.Error = ex.Message;
            }

            return resp;

        }


        [Route("api/data/connectOdbc")]
        [HttpPost]
        public Response ConnectODBC(OdbcModel odbcModel)
        {
            Response resp = new Response();
            try
            {
                string connString = odbcModel.ConnectionString; // "DSN=vizmysql;Database=classicmodels;Uid=root;Pwd=root123;";
                var databaseName = connString.Substring(connString.IndexOf("Database"));
                databaseName = databaseName.Substring(databaseName.IndexOf("=") + 1, databaseName.IndexOf(";") - 9);
                var dsn = connString.Substring(connString.IndexOf("DSN"));
                dsn = dsn.Substring(dsn.IndexOf("=") + 1, dsn.IndexOf(";") - 4);
                DBType dbType = DBType.MySql;
                if (dsn.Contains("mysql"))
                {
                    dbType = DBType.MySql;
                }
                else if (dsn.Contains("postgres"))
                {
                    dbType = DBType.PostgreSql;
                }

                var tableListQuery = DbQueryFactory.GetTableListQuery(dbType,databaseName);
                //if (dsn.Contains("mysql"))
                //{
                //    tableListQuery = "SELECT table_name FROM information_schema.tables where table_schema = '" + databaseName + "'; ";
                //} else if (dsn.Contains("postgres"))
                //{
                //    tableListQuery = "SELECT tablename as table_name FROM pg_catalog.pg_tables  where schemaname = 'public';";
                //}

                    using (OdbcConnection con = new OdbcConnection(connString))
                {
                    OdbcCommand cmd = new OdbcCommand(tableListQuery, con);
                    //Postgresql = SELECT tablename FROM pg_catalog.pg_tables  where schemaname = 'public';
                    OdbcDataAdapter da = new OdbcDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    DataTable dataTable = ds.Tables[0];
                    resp.Data = dataTable;
                    resp.Status = "success";
                }
            }
            catch (Exception ex)
            {
                resp.Status = "failed";
                resp.Error = ex.Message;
            }

            return resp;

        }

        [Route("api/data/getColumnsForTableOdbc")]
        [HttpPost]
        public Response GetColumnsForTableOdbc (OdbcModel odbcModel)
        {
            Response resp = new Response();
            try
            {
                string connString = odbcModel.ConnectionString; // "DSN=vizmysql;Database=classicmodels;Uid=root;Pwd=root123;";
                var databaseName = connString.Substring(connString.IndexOf("Database"));
                databaseName = databaseName.Substring(databaseName.IndexOf("=") + 1, databaseName.IndexOf(";") - 9);
                var dsn = connString.Substring(connString.IndexOf("DSN"));
                dsn = dsn.Substring(dsn.IndexOf("=") + 1, dsn.IndexOf(";") - 4);

                DBType dbType = DBType.MySql;
                if (dsn.Contains("mysql"))
                {
                    dbType = DBType.MySql;
                }
                else if (dsn.Contains("postgres"))
                {
                    dbType = DBType.PostgreSql;
                }

                var columnListQuery = DbQueryFactory.GetColumsnForTableQuery(dbType, databaseName, odbcModel.TableName);
                
                using (OdbcConnection con = new OdbcConnection(connString))
                {
                    OdbcCommand cmd = new OdbcCommand(columnListQuery, con);
                    OdbcDataAdapter da = new OdbcDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    DataTable dataTable = ds.Tables[0];
                    resp.Data = dataTable;
                    resp.Status = "success";
                    //return dataTable;
                }
            }
            catch (Exception ex)
            {
                resp.Status = "failed";
                resp.Error = ex.Message;
            }
            
            return resp;
        }

        [Route("api/data/importDataFromMySql")]
        [HttpPost]
        public Response ImportDataFromMySql(OdbcModel odbcModel)
        {
            var tableName = odbcModel.TableName;
            var delimiter = "|";
            var databaseName = odbcModel.ConnectionString.Substring(odbcModel.ConnectionString.IndexOf("Database"));
            databaseName = databaseName.Substring(databaseName.IndexOf("=") + 1, databaseName.IndexOf(";") - 9);
            var dsn = odbcModel.ConnectionString.Substring(odbcModel.ConnectionString.IndexOf("DSN"));
            dsn = dsn.Substring(dsn.IndexOf("=") + 1, dsn.IndexOf(";") - 4);
            //string connString = "Driver={ODBC 5.1 Driver};Server=127.0.0.1;Database=classicmodels; User=root;Password=root123;Option=3;";
            //string connString = "DSN=mysql32test;Database=classicmodels;Uid=root;Pwd=root123;";
            //string connString = "DSN=vizmysql;Database=classicmodels;Uid=root;Pwd=root123;";
            using (OdbcConnection con = new OdbcConnection(odbcModel.ConnectionString))
            {
               
                var selectQuery = "";

                //Create table for MonetDB
                var columnDetailList = new List<ColumnDetail>();
                //var columnNames = odbcModel.ColumnNames.Select(c=>c.column_name)
                //bool isDateTimeColExist = false;
                foreach (var dbCol in odbcModel.ColumnNames)
                {
                    if(dbCol.data_type == "bytea" || dbCol.data_type == "varbinary")
                    {
                        continue;
                    }
                    var col = new ColumnDetail();
                    col.Name = dbCol.column_name;
                    col.CType = GetSqlType(dbCol.data_type, 10, 10, 2);
                    //isDateTimeColExist = col.CType == "timestamp";
                    columnDetailList.Add(col);
                    if(col.CType == "timestamp")
                    {
                        //Postgresql= to_char
                        if (dsn.Contains("postgres"))
                        {
                            selectQuery += "to_char( " + col.Name + ", 'YYYY-MM-DD HH:mm:ss') " + col.Name + "  ,";
                        }
                        else
                        {                            
                            selectQuery += " DATE_FORMAT( " + col.Name + ", '%Y-%m-%d %H:%m:%s') " + col.Name + "  ,";
                        }
                            

                    } else
                    {
                        selectQuery += col.Name + ",";
                    }
                    
                }

                selectQuery = selectQuery.Remove(selectQuery.LastIndexOf(','), 1);


                OdbcCommand cmd = new OdbcCommand("select " + selectQuery + " from " + tableName, con);
                OdbcDataAdapter da = new OdbcDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dataTable = ds.Tables[0];
                
                var lines = new List<string>();

                string[] columnNames = dataTable.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName).
                                                  ToArray();

                var header = string.Join(",", columnNames);
                lines.Add(header);

                var valueLines = dataTable.AsEnumerable()
                                   .Select(row => string.Join("|", row.ItemArray));
                lines.AddRange(valueLines);

                var path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Upload/"), odbcModel.NewTableName + ".csv");
                File.WriteAllLines(path, lines);

                ////SELECT COLUMN_NAME, DATA_TYPE,is_NULLABLE FROM information_schema.columns WHERE table_schema='classicmodels' AND table_name='customers'
                
                //var columnListQuery = "";
                //if (dsn.Contains("mysql"))
                //{
                //    columnListQuery = "SELECT COLUMN_NAME column_name, DATA_TYPE data_type,IS_NULLABLE is_nullable FROM information_schema.columns WHERE table_schema='" + databaseName + "' AND table_name='" + odbcModel.TableName + "'";
                //}
                //else if (dsn.Contains("postgres"))
                //{
                //    columnListQuery = "select column_name column_name, data_type data_type, is_nullable is_nullable, character_maximum_length, numeric_precision, numeric_scale from information_schema.columns where table_name = '" + odbcModel.TableName + "';";
                //}

                //cmd = new OdbcCommand(columnListQuery, con);
                //da = new OdbcDataAdapter(cmd);
                //ds = new DataSet();
                //da.Fill(ds);
                //dataTable = ds.Tables[0];

                ////Create table for MonetDB
                //var columnDetailList = new List<ColumnDetail>();
                ////var columnNames = odbcModel.ColumnNames.Select(c=>c.column_name)
                //foreach (DataRow row in dataTable.Rows)
                //{
                //    var dbCol =  odbcModel.ColumnNames.Where(c => c.column_name == row["column_name"].ToString()).FirstOrDefault();
                //    if (null != dbCol)
                //    {
                //        var col = new ColumnDetail();
                //        col.Name = row["column_name"].ToString();
                //        col.CType = GetSqlType(row["data_type"], 10, 10, 2);
                //        columnDetailList.Add(col);
                //    }                    
                //}
                //tableName = "customersnew";
                var query = CreateTableFromColumns(columnDetailList, odbcModel.NewTableName);
                //Create table
                Response result = ExecuteQueryFromDB(query);

                if (result.Status.ToLower() == "failed")
                {
                    return result;
                }

                var importDataQuery = "COPY OFFSET 2 INTO \"" + odbcModel.NewTableName + "\" FROM '" + path + "' USING DELIMITERS '" + delimiter + "','\n','\"' NULL AS '';";
                //var importDataQuery = "COPY OFFSET 2 INTO " + tableName + " FROM '" + path + "' USING DELIMITERS '\t';";
                importDataQuery = importDataQuery.Replace("\\", "/");
                //Import Data
                Response importResult = ExecuteQueryFromDB(importDataQuery);

                return importResult;
            }
        }


        private static int IsEdgeExist(int u, int v, List<Association> associations, Dashboard dashboard)
        {
            var value = 0;

            if (associations.Where(a => a.TableName == dashboard.Tables[u].Name && a.Relations.Where(r => r.TableName2 == dashboard.Tables[v].Name).FirstOrDefault() != null ||
                                        (a.TableName == dashboard.Tables[v].Name && a.Relations.Where(r => r.TableName2 == dashboard.Tables[u].Name).FirstOrDefault() != null)).FirstOrDefault() != null)
            {
                value = 1;
            }


            return value;
        }

    }
}
