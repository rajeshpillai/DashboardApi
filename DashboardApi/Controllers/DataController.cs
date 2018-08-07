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
using System.Net.Http;

namespace DashboardApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DataController : ApiController
    {
        static object PageData = null;

        Dashboard dashboard = null;


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

        private void BuildTableMetatdata()
        {
            dashboard = new Dashboard();

            if (null != MemoryCache.Default.Get("dashboard",null)){
                dashboard = MemoryCache.Default.Get("dashboard", null) as Dashboard;
                return;
            }

            //dashboard.AddTable(new Table() { Name = "Product" });
            //dashboard.AddTable(new Table() { Name = "ProductInventory" });
            //dashboard.AddTable(new Table() { Name = "ProductVendor" });
            //dashboard.AddTable(new Table() { Name = "PurchaseOrderDetail" });
            //dashboard.AddTable(new Table() { Name = "PurchaseOrderHeader" });

            dashboard.AddTable(new Table() { Name = "employee1" });
            dashboard.AddTable(new Table() { Name = "skills1" });            

            dashboard.Associations = GetAllTableAssociations();

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

            MemoryCache.Default.Set("dashboard", dashboard, null,null);

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

            var associations = GetAllTableAssociations();


            int count = 0;
            Query query = null;
            foreach (var a in associations)
            {
                if (count == 0)
                {
                    query = db.Query(a.TableName);
                }

                foreach (var relation in a.Relations)
                {
                    query = query.Join(relation.TableName2, relation.Keys[0], relation.Keys[1], relation.Operation, relation.Type);
                }

                count++;
            }

            ///db.Query("Product").Join("ProductInventory", "Product.ProductId", "ProductInventory.ProductId", "=", "inner").Select("Product.Name");

            var data = query.Get();

            //var data =  db.Query("Product").Join("ProductInventory", "Product.ProductId", "ProductInventory.ProductId", "=", "inner").Select("Product.Name").Get();

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

        public Query GetTablesAssociationQuery(WidgetModel widgetModel)
        {

            var connection = new MySqlConnection(
             "Host=localhost;Port=3306;User=root;Password=root123;Database=adventureworks;SslMode=None"
            );
            var db = new QueryFactory(connection, new MySqlCompiler());
            Query query = null;

            //var tables = GetTablesInvolved(widgetModel);

            var tablesKey = GetTablesInvolved(widgetModel);
            DerivedAssociation derivedAssociation = null;

            if (dashboard.TableAssociationHash.ContainsKey(tablesKey))
            {
                derivedAssociation = dashboard.TableAssociationHash[tablesKey];

                if (null != derivedAssociation)
                {
                    query = db.Query(derivedAssociation.TableName1);
                    foreach (var rel in derivedAssociation.Relations)
                    {
                        query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
                    }
                }
            } else
            {
                if(tablesKey.Split(',').Length == 1)
                {
                    var tableId = tablesKey.Split(',')[0];
                    var tableName = dashboard.Tables.Where(t => t.Id.ToString() == tableId).First().Name;
                    query = db.Query(tableName);
                }
            }

            //if (null != tables && tables.Count > 0)
            //{
            //    var allAssociations = dashboard.Associations; //GetAllTableAssociations();

            //    var tablesConsidered = new List<string>();
            //    tablesConsidered.Add(tables[0]);
            //    query = db.Query(tables[0]);
            //    for (var i = 0; i < tables.Count; i++)
            //    {
            //        var association = allAssociations.Where(a => a.TableName == tables[i]).FirstOrDefault();
            //        if(null != association)
            //        {
            //            foreach (var rel in association.Relations)
            //            {
            //                if (tables.Contains(rel.TableName2) && !tablesConsidered.Contains(rel.TableName2))
            //                {
            //                    //consider it
            //                    query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
            //                    tablesConsidered.Add(rel.TableName2);
            //                }
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    if (widgetModel.Type == "filter" && widgetModel.FilterList.Count == 0)
            //    {
            //        //Do not joing on any other table.
            //        //Select from table of dimention only
            //        var dimName = widgetModel.Dimension[0].Name;
            //        var tableName = dimName.Substring(0, dimName.IndexOf("."));
            //        query = db.Query(tableName);
            //    }                
            //}


            return query;
        }

        //private List<string> GetTablesInvolved(WidgetModel widgetModel)
        private string GetTablesInvolved(WidgetModel widgetModel)
        {
            List<string> tables = new List<string>();

            if(null != widgetModel.Dimension && widgetModel.Dimension.Count() > 0)
            {
                foreach(var dim in widgetModel.Dimension)
                {
                    var dimName = dim.Name;
                    var tableName = dimName.Substring(0, dimName.IndexOf("."));
                    if (!tables.Contains(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }

            if (null != widgetModel.Measure && widgetModel.Measure.Count() > 0)
            {
                var replacementStrings = new string[5] { "sum", "count", "avg", "(", ")" };
                foreach (var measure in widgetModel.Measure)
                {
                    var expression = measure.Expression;
                    if (string.IsNullOrWhiteSpace(expression)) { continue; }
                    foreach(var r in replacementStrings)
                    {
                        expression =  expression.Replace(r, "");
                    }
                    expression = expression.Trim();
                    //var measure = dim.Name;
                    var tableName = expression.Substring(0, expression.IndexOf("."));
                    if (!tables.Contains(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }

            if (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0)
            {
                foreach (var filter in widgetModel.FilterList)
                {
                    var colName = filter.ColName;
                    var tableName = colName.Substring(0, colName.IndexOf("."));
                    filter.TableName = tableName;
                    if (!tables.Contains(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }

            //Get Table Objects and string for hash key
            var tableList = new List<Table>();
            var tableKey = string.Empty;
            //tables = tables.Select(t=>t.).OrderBy
            foreach ( var t in tables)
            {
               var table =  dashboard.Tables.Where(tbl => tbl.Name == t).FirstOrDefault();
                if (null != table)
                {
                    tableList.Add(table);
                    //
                }
            }
            tableList = tableList.OrderBy(t => t.Id).ToList();
            foreach (var t in tableList)
            {
                tableKey += t.Id + ",";
            }
                tableKey = tableKey.Substring(0, tableKey.LastIndexOf(","));

            //return tables;
            return tableKey;
        }


        // GET: api/Data
        [Route("api/data/getData")]
        [HttpPost]
        public dynamic GetData(WidgetModel widgetModel)
        {
            BuildTableMetatdata();

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


            using (var client = new WebClient())
            {
                var values = new System.Collections.Specialized.NameValueCollection();
                values["equery"] = query;
               
                var response = client.UploadValues("http://localhost:4000/getdata","POST", values);

                var responseString = System.Text.Encoding.Default.GetString(response);

                return Newtonsoft.Json.Linq.JArray.Parse(responseString);
               //var t  =Json<string>(responseString);
               // t.Content
            }

            //var dt = QueryExecuteEngine.ExecuteQuery(query, widgetModel.AllColumns);

            //var data = GetJsonObjectFromTable(dt);

            //return data;
            //return "";
        }

        [Route("api/data/getTotalRecordsCount")]
        [HttpPost]
        public dynamic GetTotalRecordsCount(WidgetModel widgetModel)
        {
            var recordsCount = 0;
            BuildTableMetatdata();

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
            using (var client = new WebClient())
            {
                var values = new System.Collections.Specialized.NameValueCollection();
                values["equery"] = query;

                var response = client.UploadValues("http://localhost:4000/getdata", "POST", values);

                var responseString = System.Text.Encoding.Default.GetString(response);

                return Newtonsoft.Json.Linq.JArray.Parse(responseString);
                //var t  =Json<string>(responseString);
                // t.Content
            }

            return recordsCount;
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


        // GET: api/Data
        [Route("api/data/getDataWorkingWithMaridDB")]
        [HttpPost]
        public IEnumerable<dynamic> GetDataWorkingWithMariaDB(WidgetModel widgetModel)
        {
            BuildTableMetatdata();

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
                    if (string.IsNullOrWhiteSpace(measure.Expression)) { continue; }

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
                        dims.Append(dim.Name.Trim() + " as '" + dim.Name.Trim() + "',");
                    }                    
                }
                dimString = dims.ToString();
                dimString = dimString.Remove(dimString.LastIndexOf(','), 1);
            }

            query = GetTablesAssociationQuery(widgetModel);

            //Todo: widgetModel.SqlTableName
            if (widgetModel.Type == "filter")
                {
                    query = query.Select(dimString).Distinct();
                    //data = db.Query("employee").Select(dimString).Distinct().Get();
                   
                } else //if (widgetModel.Type == "kpi")
                {
                    if (!string.IsNullOrWhiteSpace(dimString))
                    {
                        query = query.SelectRaw(dimString);
                    }
                    if (!string.IsNullOrWhiteSpace(measuresString))
                    {
                        query = query.SelectRaw(measuresString);
                    }

                    if(!string.IsNullOrWhiteSpace(dimString)  && !string.IsNullOrWhiteSpace(measuresString))
                    {
                    
                        query = query.GroupBy( widgetModel.Dimension.Select(d => d.Name).ToArray<string>());
                    }
                
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
            
            //query.comp

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


        //private List<string> GetTables(WidgetModel widgetModel)
        //{
        //    var tables = new List<string>();
        //    if(null != widgetModel.Measure && widgetModel.Measure.Length > 0)
        //    {
        //        foreach(var measure in widgetModel.Measure)
        //        {
        //            var tableName = 
        //        }
        //    }

        //    return tables;
        //}

        private static List<Association> GetAllTableAssociations()
        {            
            var associations = new List<Association>();
            var association = new Association();
            
            association.TableName = "employee1";
            var relations = new List<Relation>();
            var rel = new Relation() { TableName2 = "skills1", Type = "left" };
            var keys = new List<string>();
            keys.Add("employee1.empid");
            keys.Add("skills1.empid");
            rel.Keys = keys;
            rel.Operation = "=";
            relations.Add(rel);
            association.Relations = relations;
            associations.Add(association);

            association = new Association();
            association.TableName = "Skills1";
            relations = new List<Relation>();
            rel = new Relation() { TableName2 = "employee1", Type = "left" };
            keys = new List<string>();
            keys.Add("employee1.empid");
            keys.Add("skills1.empid");
            rel.Keys = keys;
            rel.Operation = "=";
            relations.Add(rel);
            association.Relations = relations;
            associations.Add(association);

            //association.TableName = "Product";
            //var relations = new List<Relation>();
            //var rel = new Relation() { TableName2 = "ProductInventory", Type = "left" };          
            //var keys = new List<string>();
            //keys.Add("Product.ProductId");
            //keys.Add("ProductInventory.ProductId");
            //rel.Keys = keys;
            //rel.Operation = "=";
            //relations.Add(rel);
            //var rel2 = new Relation() { TableName2 = "ProductVendor", Type = "left" };
            //var keys2 = new List<string>();
            //keys2.Add("Product.ProductId");
            //keys2.Add("ProductVendor.ProductId");
            //rel2.Keys = keys2;
            //rel2.Operation = "=";
            //relations.Add(rel2);
            //rel2 = new Relation() { TableName2 = "PurchaseOrderDetail", Type = "left" };
            //keys2 = new List<string>();
            //keys2.Add("Product.ProductId");
            //keys2.Add("PurchaseOrderDetail.ProductId");
            //rel2.Keys = keys2;
            //rel2.Operation = "=";
            //relations.Add(rel2);
            //association.Relations = relations;
            //associations.Add(association);

            //association = new Association();
            //association.TableName = "ProductInventory";
            //relations = new List<Relation>();
            //rel = new Relation() { TableName2 = "Product", Type = "left" };
            //keys = new List<string>();
            //keys.Add("Product.ProductId");
            //keys.Add("ProductInventory.ProductId");
            //rel.Keys = keys;
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            //association = new Association();
            //association.TableName = "ProductVendor";
            //relations = new List<Relation>();
            //rel = new Relation() { TableName2 = "Product", Type = "left" };
            //keys = new List<string>();
            //keys.Add("Product.ProductId");
            //keys.Add("ProductVendor.ProductId");
            //rel.Keys = keys;
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            //association = new Association();
            //association.TableName = "PurchaseOrderDetail";
            //relations = new List<Relation>();
            //rel = new Relation() { TableName2 = "PurchaseOrderHeader", Type = "left" };
            //keys = new List<string>();
            //keys.Add("PurchaseOrderDetail.PurchaseOrderId");
            //keys.Add("PurchaseOrderHeader.PurchaseOrderId");
            //rel.Keys = keys;
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            //association = new Association();
            //association.TableName = "PurchaseOrderHeader";
            //relations = new List<Relation>();
            //rel = new Relation() { TableName2 = "PurchaseOrderDetail", Type = "left" };
            //keys = new List<string>();
            //keys.Add("PurchaseOrderDetail.PurchaseOrderId");
            //keys.Add("PurchaseOrderHeader.PurchaseOrderId");
            //rel.Keys = keys;
            //rel.Operation = "=";
            //relations.Add(rel);
            //association.Relations = relations;
            //associations.Add(association);

            return associations;

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
