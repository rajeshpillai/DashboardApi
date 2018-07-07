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

            var tables = GetTablesInvolved(widgetModel);

            if (null != tables && tables.Count > 0)
            {

                var allAssociations = GetAllTableAssociations();

                var tablesConsidered = new List<string>();
                tablesConsidered.Add(tables[0]);
                query = db.Query(tables[0]);
                for (var i = 0; i < tables.Count; i++)
                {
                    var association = allAssociations.Where(a => a.TableName == tables[i]).FirstOrDefault();
                    if(null != association)
                    {
                        foreach (var rel in association.Relations)
                        {
                            if (tables.Contains(rel.TableName2) && !tablesConsidered.Contains(rel.TableName2))
                            {
                                //consider it
                                query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
                                tablesConsidered.Add(rel.TableName2);
                            }
                        }
                    }
                   
                }
            }
            else
            {
                if (widgetModel.Type == "filter" && widgetModel.FilterList.Count == 0)
                {
                    //Do not joing on any other table.
                    //Select from table of dimention only
                    var dimName = widgetModel.Dimension[0].Name;
                    var tableName = dimName.Substring(0, dimName.IndexOf("."));
                    query = db.Query(tableName);
                }
                //else
                //{

                //    int count = 0;

                //    foreach (var a in allAssociations)
                //    {
                //        if (count == 0)
                //        {
                //            query = db.Query(a.TableName);
                //        }

                //        foreach (var relation in a.Relations)
                //        {
                //            query = query.Join(relation.TableName2, relation.Keys[0], relation.Keys[1], relation.Operation, relation.Type);
                //        }

                //        count++;
                //    }
                //}
            }
            

            return query;
        }

        private List<string> GetTablesInvolved(WidgetModel widgetModel)
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

            return tables;
        }

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

            association.TableName = "Product";
            var relations = new List<Relation>();
            var rel = new Relation() { TableName2 = "ProductInventory", Type = "left" };          
            var keys = new List<string>();
            keys.Add("Product.ProductId");
            keys.Add("ProductInventory.ProductId");
            rel.Keys = keys;
            rel.Operation = "=";
            relations.Add(rel);
            var rel2 = new Relation() { TableName2 = "ProductVendor", Type = "left" };
            var keys2 = new List<string>();
            keys2.Add("Product.ProductId");
            keys2.Add("ProductVendor.ProductId");
            rel2.Keys = keys2;
            rel2.Operation = "=";
            relations.Add(rel2);
            association.Relations = relations;
            associations.Add(association);

            association = new Association();

            association.TableName = "ProductInventory";
            relations = new List<Relation>();
            rel = new Relation() { TableName2 = "Product", Type = "left" };
            keys = new List<string>();
            keys.Add("Product.ProductId");
            keys.Add("ProductInventory.ProductId");
            rel.Keys = keys;
            rel.Operation = "=";
            relations.Add(rel);
            association.Relations = relations;
            associations.Add(association);


            return associations;

        }
    }
}
