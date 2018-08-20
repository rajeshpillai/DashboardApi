using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DashboardApi.Models;

namespace DashboardApi
{
    public class QueryEngine
    {
        public Dashboard dashboard = null;

        public QueryEngine(Dashboard dshBoard)
        {
            this.dashboard = dshBoard;
        }

        public string BuildQuery(WidgetModel widgetModel)
        {
            string query = null;
            widgetModel.IsForTotal = false;

            widgetModel.TablesKey = GetTablesInvolved(widgetModel);
            
            query = GetSelectQuery(widgetModel);
            query += GetTablesAssociationQuery(widgetModel);

            var tableQuery = query;

            query = "Select D.* ";

            if (null != widgetModel.Measure && widgetModel.Measure.Count() > 0)
            {
                query += "," + GetMeasureString(widgetModel);
            }
            query += " from (" + tableQuery + ") D ";

            if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0 && null != widgetModel.Measure && widgetModel.Measure.Length > 0) // && !widgetModel.IsForTotal)
            {
                query += " group by "; // + String.Join(",", "D." + widgetModel.Dimension.Select(d => d.Name));    
                
                foreach(var dim in widgetModel.Dimension)
                {
                    query += "D.\"" + dim.Name  + "\",";
                }
                query = query.Remove(query.LastIndexOf(','), 1);              
            }
            if (widgetModel.EnablePagination && widgetModel.PageSize > 0 && !widgetModel.IsRecordCountReq && !widgetModel.IsForTotal)
            {
                query += " limit " + widgetModel.PageSize + " offset " + widgetModel.StartRowNum;
            }

            if (widgetModel.Type == "datagrid" && widgetModel.ShowTotal && widgetModel.StartRowNum == 0 && null != widgetModel.Measure && widgetModel.Measure.Count() > 0)
            {
                widgetModel.IsForTotal = true;
                var totalQuery = GetSelectQuery(widgetModel);
                totalQuery += GetTablesAssociationQuery(widgetModel);
                var totalSelectQuery = "select ";
                if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0 && null != widgetModel.Measure && widgetModel.Measure.Length > 0) // && !widgetModel.IsForTotal)
                {                   
                    foreach (var dim in widgetModel.Dimension)
                    {
                        totalSelectQuery += "null as \"" + dim.Name + "\",";
                    }

                    foreach (var m in widgetModel.Measure)
                    {
                        
                        totalSelectQuery += ReplaceTableNameOfMeasure(widgetModel, m.Expression, "D") + " as \"" + m.Expression + "\",";
                    }

                    totalSelectQuery = totalSelectQuery.Remove(totalSelectQuery.LastIndexOf(','), 1);
                }


                totalQuery = totalSelectQuery + " from (" + totalQuery + " ) D";

                query = totalQuery + " union all " + query;
            }

            
            return query;
        }

        public string BuildTotalRecordsCountQuery(WidgetModel widgetModel)
        {
            string query = null;

            widgetModel.TablesKey = GetTablesInvolved(widgetModel);

            query = GetSelectQuery(widgetModel);
            query += GetTablesAssociationQuery(widgetModel);

            //query = "Select D.*," + GetMeasureString(widgetModel) + " from (" + query + ") D ";
            query = "Select D.* from (" + query + ") D ";

            if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0 && null != widgetModel.Measure && widgetModel.Measure.Length > 0 ) //&& !widgetModel.IsForTotal)
            {
                query += " group by "; // + String.Join(",", "D." + widgetModel.Dimension.Select(d => d.Name));    

                foreach (var dim in widgetModel.Dimension)
                {
                    query += "D.\"" + dim.Name + "\",";
                }
                query = query.Remove(query.LastIndexOf(','), 1);
            }

            query = "Select count(*) as totalRowsCount from (" + query + ") as t1";

            return query;
        }

        //private string GetDimStringForGroupBy(WidgetModel widgetModel)
        //{

        //}

        private string GetMeasureString(WidgetModel widgetModel)
        {            
            var measures = new System.Text.StringBuilder();
            if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length == 0)))
            {
                return null;
            }
            if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
            {
                return null;
            }

            var measuresString = string.Empty;
            if (null != widgetModel.Measure)
            {
                foreach (var measure in widgetModel.Measure)
                {
                    if (string.IsNullOrWhiteSpace(measure.Expression)) { continue; }
                    var expression = ReplaceTableNameOfMeasure(widgetModel,measure.Expression.Trim(), "D");

                    if (!string.IsNullOrWhiteSpace(measure.DisplayName))
                    {
                        measures.Append(string.Format("{0} as \"{1}\" ", expression, measure.DisplayName.Trim()) + ", ");
                    }
                    else
                    {
                        measures.Append(string.Format("{0} as \"{1}\"", expression, measure.Expression.Trim()) + ", ");
                    }
                }

                measuresString = measures.ToString();
                if (string.IsNullOrWhiteSpace(measuresString))
                {
                    return null;
                }
                measuresString = measuresString.Remove(measuresString.LastIndexOf(','), 1);
            }

            return measuresString;
        }

        private string GetSelectQuery(WidgetModel widgetModel)
        {
            string selectQuery = null;
            var measures = new System.Text.StringBuilder();
            if (widgetModel.Type == "kpi" && (null == widgetModel.Measure || (null != widgetModel.Measure && widgetModel.Measure.Length == 0)))
            {
                return null;
            }
            if (widgetModel.Type == "filter" && (null == widgetModel.Dimension || (null != widgetModel.Dimension && widgetModel.Dimension.Length == 0)))
            {
                return null;
            }

            var measuresString = string.Empty;
            var dimString = string.Empty;


            DerivedAssociation derivedAssociation = null;
            if (dashboard.TableAssociationHash.ContainsKey(widgetModel.TablesKey))
            {
                derivedAssociation = dashboard.TableAssociationHash[widgetModel.TablesKey];
            }
            if (null != widgetModel.Measure)
            {
                foreach (var measure in widgetModel.Measure)
                {
                    if (string.IsNullOrWhiteSpace(measure.Expression)) { continue; }

                    var mTableName = GetTableNameOfMeasure(measure.Expression);
                    var expression = measure.Expression.Trim();

                    if (null != derivedAssociation && expression.IndexOf("sum") != -1) //Only for sum
                    {
                        if (derivedAssociation.TableName1 == mTableName)
                        {
                            if (derivedAssociation.Relations[0].Cardinality == Cardinality.OneToMany)
                            {
                                expression = expression + "/count(" + derivedAssociation.Relations[0].Keys[0][0] + ")"; //divide by count
                            }
                        }
                        else
                        {
                            var matchedRel = derivedAssociation.Relations.Where(r => r.TableName2 == mTableName).FirstOrDefault();
                            if (matchedRel.Cardinality == Cardinality.ManyToOne)
                            {
                                expression += expression + "/count(" + matchedRel.Keys[0][1] + ")"; //divide by count
                            }
                        }
                    } 
                    else if (null != derivedAssociation && expression.IndexOf("count") != -1) //Only for count
                    {
                        bool isCountToSum = IsCountToSum(widgetModel, expression);
                        if (!isCountToSum)
                        {
                            //add null condition i count is zero
                            //CASE WHEN count(orders.ordernumber) = 0 THEN null ELSE count(orders.ordernumber) END
                            expression = "CASE WHEN " + expression + "= 0 THEN null ELSE " + expression + " END ";
                        }
                    }

                    //measures.Append(string.Format("{0} ", measure.Expression.Trim()) + ", ");
                    if (!string.IsNullOrWhiteSpace(measure.DisplayName))
                    {
                        measures.Append(string.Format("{0} as \"{1}\" ", expression, measure.DisplayName.Trim()) + ", ");
                    }
                    else
                    {
                        measures.Append(string.Format("{0} as \"{1}\"", expression, measure.Expression.Trim()) + ", ");
                    }
                }

                measuresString = measures.ToString();
                if (string.IsNullOrWhiteSpace(measuresString))
                {
                    return null;
                }
                measuresString = measuresString.Remove(measuresString.LastIndexOf(','), 1);
            }
            var dims = new System.Text.StringBuilder();

            if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0)
            {
                foreach (var dim in widgetModel.Dimension)
                {
                    if (widgetModel.IsForTotal)
                    {
                        dims.Append(" null as \"" + dim.Name.Trim() + "\",");
                    }
                    else if (!string.IsNullOrWhiteSpace(dim.Name))
                    {
                        // var expDisplayName = !string.IsNullOrWhiteSpace(measure.DisplayName) ? measure.DisplayName : '[' + measure.Expression + ']';                        
                        dims.Append(dim.Name.Trim() + " as \"" + dim.Name.Trim() + "\",");
                        //dims.Append(dim.Name.Trim() + ",");                        
                    }                   
                }
                dimString = dims.ToString();
                dimString = dimString.Remove(dimString.LastIndexOf(','), 1);
            }
            var selectColumnsString = ((!string.IsNullOrWhiteSpace(dimString)) ? dimString : string.Empty) + ((!string.IsNullOrWhiteSpace(measuresString)) ? ((!string.IsNullOrWhiteSpace(dimString)) ? "," + measuresString : measuresString) : string.Empty);

            widgetModel.AllColumns = selectColumnsString.Split(',');
            //Todo: widgetModel.SqlTableName
            if (widgetModel.Type == "filter")
            {
                //query = query.Select(dimString).Distinct();
                selectQuery = "Select distinct " + dimString ;

            }
            else //if (widgetModel.Type == "kpi")
            {
               
                if (!string.IsNullOrWhiteSpace(selectColumnsString))
                {
                    //query = query.SelectRaw(dimString);
                    //selectQuery = "Select distinct " + selectColumnsString;
                    selectQuery = "Select " + selectColumnsString;
                }               

                //if (!string.IsNullOrWhiteSpace(dimString) && !string.IsNullOrWhiteSpace(measuresString))
                //{

                //    //query = query.GroupBy(widgetModel.Dimension.Select(d => d.Name).ToArray<string>());
                //}

                //data = db.Query("employee").SelectRaw(measuresString).Get();
            }

            return selectQuery;
        }

        private string GetTablesAssociationQuery(WidgetModel widgetModel)
        {
            string query = null;
            var tablesKey = (null != widgetModel.TablesKey)? widgetModel.TablesKey : GetTablesInvolved(widgetModel);
            DerivedAssociation derivedAssociation = null;
            string groupByAssociationKeys = null;

            if (dashboard.TableAssociationHash.ContainsKey(tablesKey))
            {
                derivedAssociation = dashboard.TableAssociationHash[tablesKey];

                if (null != derivedAssociation)
                {
                    query = " from  " + derivedAssociation.TableName1; // db.Query(derivedAssociation.TableName1);
                    foreach (var rel in derivedAssociation.Relations)
                    {
                        //var tableId = dashboard.Tables.Where(t => t.Name == rel.TableName2).FirstOrDefault().Id.ToString();
                        //if(!tablesKey.Contains(tableId)) { continue; }
                        //Todo: determine the joins based on filters on tables.
                        query += " left outer join " + rel.TableName2 + " on ";// + rel.Keys[0] + rel.Operation + rel.Keys[1] + " ";
                        //query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
                        foreach (var keys in rel.Keys)
                        {
                            query += keys[0] + rel.Operation + keys[1] + " and ";
                            groupByAssociationKeys += keys[0] + ",";
                        }
                        query = query.Substring(0, query.LastIndexOf("and"));

                    }
                    if (null != groupByAssociationKeys)
                    {
                        groupByAssociationKeys = groupByAssociationKeys.Substring(0, groupByAssociationKeys.LastIndexOf(","));
                    }
                }
            }
            else
            {
                if (tablesKey.Split(',').Length == 1)
                {
                    var tableId = tablesKey.Split(',')[0];
                    var tableName = dashboard.Tables.Where(t => t.Id.ToString() == tableId).First().Name;
                    //query = db.Query(tableName);
                    query = " from  " + tableName;
                }
            }

            if (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0)
            {
                query += (query.Contains("where"))? " " : " where ";
                foreach (var filter in widgetModel.FilterList)
                {
                    //query = query.WhereIn(filter.ColName, filter.Values);
                    var values = string.Empty;
                    foreach(var fValue in filter.Values)
                    {
                        values += "'" + fValue + "',";
                    }
                    values = values.Substring(0, values.LastIndexOf(","));
                    query += filter.ColName + " in (" + values + ") and ";
                }
                query = query.Substring(0, query.LastIndexOf("and"));
            }

            if (null != widgetModel.SearchList && widgetModel.SearchList.Count() > 0)
            {
                query += (null != widgetModel.FilterList && widgetModel.FilterList.Count() > 0) ? " and " : " where ";                
                foreach (var search in widgetModel.SearchList)
                {
                    query += search.ColName + " like '%" + search.Value + "%' and ";
                }
                query = query.Substring(0, query.LastIndexOf("and"));
            }

                if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0 && null != widgetModel.Measure && widgetModel.Measure.Length > 0) // && !widgetModel.IsForTotal)
            {
                //query = query.GroupBy(widgetModel.Dimension.Select(d => d.Name).ToArray<string>());

                query += " group by " + String.Join(",", widgetModel.Dimension.Select(d => d.Name));
                if(null != groupByAssociationKeys)
                {
                    query += "," + groupByAssociationKeys;
                }
              
            }

            //if(widgetModel.EnablePagination && widgetModel.PageSize > 0 && !widgetModel.IsRecordCountReq && !widgetModel.IsForTotal)
            //{
            //    query += " limit " + widgetModel.PageSize + " offset " + widgetModel.StartRowNum;
            //}

            //query += " limit 1000" + " offset 0";

            return query;
        }

        private string GetTablesInvolved(WidgetModel widgetModel)
        {
            List<string> tables = new List<string>();

            if (null != widgetModel.Dimension && widgetModel.Dimension.Count() > 0)
            {
                foreach (var dim in widgetModel.Dimension)
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
                    foreach (var r in replacementStrings)
                    {
                        expression = expression.Replace(r, "");
                    }
                    expression = expression.Trim();
                    measure.Name = expression;
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

            if(tables.Count == 0)
            {
                return string.Empty;
            }
            foreach (var t in tables)
            {
                var table = dashboard.Tables.Where(tbl => tbl.Name == t).FirstOrDefault();
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

        private string GetTableNameOfMeasure(string measureExp)
        {
            var replacementStrings = new string[5] { "sum", "count", "avg", "(", ")" };
            
            var expression = measureExp;
            //if (string.IsNullOrWhiteSpace(expression)) { continue; }
            foreach (var r in replacementStrings)
            {
                expression = expression.Replace(r, "");
            }
            expression = expression.Trim();               
            return( expression.Substring(0, expression.IndexOf(".")));                            
        }

        private string ReplaceTableNameOfMeasure(WidgetModel widgetModel, string measureExp, string replaceByTableName)
        {
            var replacementStrings = new string[5] { "sum", "count", "avg", "(", ")" };
            var mTableName = GetTableNameOfMeasure(measureExp);
            var expression = measureExp;
            //DerivedAssociation derivedAssociation = null;
            //if (dashboard.TableAssociationHash.ContainsKey(widgetModel.TablesKey))
            //{
            //    derivedAssociation = dashboard.TableAssociationHash[widgetModel.TablesKey];
            //}
            ////if (string.IsNullOrWhiteSpace(expression)) { continue; }
            foreach (var r in replacementStrings)
            {
                expression = expression.Replace(r, "");
            }
            expression = expression.Trim();
            bool isCountToSum = IsCountToSum(widgetModel, measureExp);

            //if (null != derivedAssociation && measureExp.IndexOf("count") != -1) //Only for count
            //{
            //    if(derivedAssociation.Relations.Where(r => r.Keys.Where(k=>k.Contains(expression)).Count() > 0).Count() > 0)
            //    {
            //        var relation = derivedAssociation.Relations.Where(r => r.Keys.Where(k => k.Contains(expression)).Count() > 0).First();
            //        if(relation.TableName2 == mTableName)
            //        {
            //            if (relation.Cardinality == Cardinality.OneToMany)
            //            {
            //                isCountToSum = true;
            //            }
            //        } else if (derivedAssociation.TableName1 == mTableName)
            //        {
            //            if (derivedAssociation.Relations[0].Cardinality == Cardinality.ManyToOne)
            //            {
            //                isCountToSum = true;
            //            }
            //        }
            //        //isCountToSum = false;
            //    } else if (derivedAssociation.TableName1 == mTableName)
            //    {
            //        if (derivedAssociation.Relations[0].Cardinality == Cardinality.ManyToOne)
            //        {
            //            isCountToSum = true;
            //        }
            //    }
            //    else
            //    {
            //        var matchedRel = derivedAssociation.Relations.Where(r => r.TableName2 == mTableName).FirstOrDefault();
            //        if (matchedRel.Cardinality == Cardinality.OneToMany)
            //        {
            //            isCountToSum = true;
            //        }
            //    }
            //}


            expression = measureExp.Replace(expression, replaceByTableName + ".\"" + measureExp + "\"");

            if (isCountToSum)
            {
                expression =  expression.Replace("count(D","sum(D");
            }

            //return (measureExp.Replace(tableName, replaceByTableName));
            //return (replaceByTableName + ".\"" + measureExp + "\"");
            return expression;
        }


        private bool IsCountToSum(WidgetModel widgetModel, string measureExp)
        {
            var replacementStrings = new string[5] { "sum", "count", "avg", "(", ")" };
            var mTableName = GetTableNameOfMeasure(measureExp);
            var expression = measureExp;
            DerivedAssociation derivedAssociation = null;
            if (dashboard.TableAssociationHash.ContainsKey(widgetModel.TablesKey))
            {
                derivedAssociation = dashboard.TableAssociationHash[widgetModel.TablesKey];
            }
            //if (string.IsNullOrWhiteSpace(expression)) { continue; }
            foreach (var r in replacementStrings)
            {
                expression = expression.Replace(r, "");
            }
            expression = expression.Trim();
            bool isCountToSum = false;

            if (null != derivedAssociation && measureExp.IndexOf("count") != -1) //Only for count
            {
                if (derivedAssociation.Relations.Where(r => r.Keys.Where(k => k.Contains(expression)).Count() > 0).Count() > 0)
                {
                    var relation = derivedAssociation.Relations.Where(r => r.Keys.Where(k => k.Contains(expression)).Count() > 0).First();
                    if (relation.TableName2 == mTableName)
                    {
                        if (relation.Cardinality == Cardinality.OneToMany)
                        {
                            isCountToSum = true;
                        }
                    }
                    else if (derivedAssociation.TableName1 == mTableName)
                    {
                        if (derivedAssociation.Relations[0].Cardinality == Cardinality.ManyToOne)
                        {
                            isCountToSum = true;
                        }
                    }
                    //isCountToSum = false;
                }
                else if (derivedAssociation.TableName1 == mTableName)
                {
                    if (derivedAssociation.Relations[0].Cardinality == Cardinality.ManyToOne)
                    {
                        isCountToSum = true;
                    }
                }
                else
                {
                    var matchedRel = derivedAssociation.Relations.Where(r => r.TableName2 == mTableName).FirstOrDefault();
                    if (matchedRel.Cardinality == Cardinality.OneToMany)
                    {
                        isCountToSum = true;
                    }
                }
            }

            //return (measureExp.Replace(tableName, replaceByTableName));
            //return (replaceByTableName + ".\"" + measureExp + "\"");
            return isCountToSum;
        }


    }
}