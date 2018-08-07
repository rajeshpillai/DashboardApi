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

            query = GetSelectQuery(widgetModel);
            query += GetTablesAssociationQuery(widgetModel);

            if (widgetModel.Type == "datagrid" && widgetModel.ShowTotal && widgetModel.StartRowNum == 0)
            {
                widgetModel.IsForTotal = true;
                var totalQuery = GetSelectQuery(widgetModel);
                totalQuery += GetTablesAssociationQuery(widgetModel);

                query = totalQuery + " union all " + query;
            }

            
            return query;
        }

        public string BuildTotalRecordsCountQuery(WidgetModel widgetModel)
        {
            string query = null;

            query = GetSelectQuery(widgetModel);
            query += GetTablesAssociationQuery(widgetModel);

            query = "Select count(*) as totalRowsCount from (" + query + ") as t1";

            return query;
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

            if (null != widgetModel.Measure)
            {
                foreach (var measure in widgetModel.Measure)
                {
                    if (string.IsNullOrWhiteSpace(measure.Expression)) { continue; }

                    //measures.Append(string.Format("{0} ", measure.Expression.Trim()) + ", ");
                    if (!string.IsNullOrWhiteSpace(measure.DisplayName))
                    {
                        measures.Append(string.Format("{0} as \"{1}\" ", measure.Expression.Trim(), measure.DisplayName.Trim()) + ", ");
                    }
                    else
                    {
                        measures.Append(string.Format("{0} as \"{1}\"", measure.Expression.Trim(), measure.Expression.Trim()) + ", ");
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
            var tablesKey = GetTablesInvolved(widgetModel);
            DerivedAssociation derivedAssociation = null;

            if (dashboard.TableAssociationHash.ContainsKey(tablesKey))
            {
                derivedAssociation = dashboard.TableAssociationHash[tablesKey];

                if (null != derivedAssociation)
                {
                    query = " from  " + derivedAssociation.TableName1; // db.Query(derivedAssociation.TableName1);
                    foreach (var rel in derivedAssociation.Relations)
                    {
                        //Todo: determine the joins based on filters on tables.
                        query += " left outer join " + rel.TableName2 + " on " + rel.Keys[0] + rel.Operation + rel.Keys[1] + " ";
                        //query = query.Join(rel.TableName2, rel.Keys[0], rel.Keys[1], rel.Operation, rel.Type);
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

                if (null != widgetModel.Dimension && widgetModel.Dimension.Length > 0 && null != widgetModel.Measure && widgetModel.Measure.Length > 0 && !widgetModel.IsForTotal)
            {
                //query = query.GroupBy(widgetModel.Dimension.Select(d => d.Name).ToArray<string>());

                query += " group by " + String.Join(",", widgetModel.Dimension.Select(d => d.Name));
            }

            if(widgetModel.EnablePagination && widgetModel.PageSize > 0 && !widgetModel.IsRecordCountReq && !widgetModel.IsForTotal)
            {
                query += " limit " + widgetModel.PageSize + " offset " + widgetModel.StartRowNum;
            }

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
    }
}