using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardApi.Models
{
    public class WidgetModel
    {
        private string _sqlTableName = "bfuFinalAllocation";
        //public string[] Dimension { get; set; }

        public Dimension[] Dimension { get; set; }

        //public string  Measure { get; set; }
        public Measure[] Measure { get; set; }

        //public Dictionary<string,string> FixedFilter { get; set; }
        public List<FixedFilter> FixedFilter { get; set; }

        public List<Filter> FilterList { get; set; }

        //public List<Filter> ORFilterList { get; set; }

        public List<ORFilter> ORFilterList { get; set; }

        public string Type { get; set; }  //Rename it to WidgetType

        public bool ShowTotal { get; set; }

        public bool IsForTotal { get; set; }

        public bool EnablePagination { get; set; }

        public bool IsRecordCountReq { get; set; }

        public int StartRowNum { get; set; }

        public int PageSize { get; set; }

        public bool ShowTotalRowCount { get; set; }

        public string OrderBy { get; set; }

        public List<Search> SearchList { get; set; }

        public Measure[] Aggr { get; set; }

        public List<FixedFilter> AggrFilter { get; set; }
       

        public string SqlTableName
        {
            get
            {
                return _sqlTableName;
            }
            set
            {
                _sqlTableName = value;
            }
        }

        public string[] AllColumns { get; set; }

        public string TablesKey { get; set; }
    }


    //public class GroupDimension
    //{
    //    public string[] Name { get; set; }

    //    public string CurrentDim { get; set; }
    //}

    public class Dimension
    {
        private string _dimType = "Simple";

        public string Name { get; set; }

        public string Type
        {
            get
            {
                return _dimType;
            }
            set
            {
                _dimType = value;
            }
        }

        public string CurrentDim { get; set; }

        public string Formula { get; set; }

        public bool IsPercentage { get; set; }

        public string TableName { get; set; }

        public int TableId { get; set; }

    }

    public class ORFilter
    {
        private string _operationType = "and";

        public string OperationType
        {
            get
            {
                return _operationType;
            }
            set
            {
                _operationType = value;
            }
        }

        public List<Filter> FilterList { get; set; }
    }

    public class Measure
    {
        public Measure()
        {
            this.TableNames = new List<string>();
            this.TableIds = new List<int>();
        }
        //private bool _isVisible = true;

        private bool _isExpression = true;

        public string Expression { get; set; }
        public string DisplayName { get; set; }

        public string Name { get; set; }

        //public bool IsVisible
        //{
        //    get
        //    {
        //        return _isVisible;
        //    }
        //    set
        //    {
        //        _isVisible = value;
        //    }
        //}

        public bool ShowTotal { get; set; }

        public bool IsExpression
        {
            get
            {
                return _isExpression;
            }
            set
            {
                _isExpression = value;
            }
        }

        public string Type { get; set; }

        public List<string> TableNames { get; set; }

        public List<int> TableIds { get; set; }
    }

    public class FixedFilter
    {
        private string _operType = "in";

        public string ColName { get; set; }

        public string[] Values { get; set; }

        public string Type { get; set; }

        public string OperationType
        {
            get
            {
                return _operType;
            }
            set
            {
                _operType = value;
            }
        }

        public string CrosssTabMeasure { get; set; }
    }

    public class Filter
    {
        private string _operType = "in";

        public string ColName { get; set; }

        public string[] Values { get; set; }

        public string OperationType
        {
            get
            {
                return _operType;
            }
            set
            {
                _operType = value;
            }
        }

        public string TableName { get; set; }

        public int TableId { get; set; }
    }

    public class Search
    {
        public string ColName { get; set; }

        public string Value { get; set; }
    }

    //public class Table
    //{
    //    public string MyProperty { get; set; }
    //    public bool IsAssociationExist { get; set; }
    //}

    public class AssociationModel
    {
        public Association Association { get; set; }
        public int AppId { get; set; }
    }
   
}
