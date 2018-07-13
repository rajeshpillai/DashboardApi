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
        public Association()
        {
            this.Relations = new List<Relation>();
        }
        public string TableName { get; set; }

        public List<Relation> Relations { get; set; }
    }


    public class DerivedAssociation
    {
        public DerivedAssociation()
        {
            this.Relations = new List<Relation>();
        }
        public string TableName1 { get; set; }
        public List<Relation> Relations { get; set; }
    }


    public class Table
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public int Order { get; set; }
    }


    public class Dashboard
    {
        private List<Table> _tables = new List<Table>();

        public Dashboard()
        {
            _tables = new List<Table>();
        }

        public List<Table> Tables
        {
            get
            {
                return _tables;
            }
        }

        public List<Association> Associations { get; set; }

        public Dictionary<string, DerivedAssociation> TableAssociationHash { get; set; }

        public void AddTable(Table table)
        {
            _tables.Add(table);

            table.Id = _tables.Count() >= 1 ? _tables.Count() - 1 : 0;

            table.Order = table.Id;

        }
    }
}