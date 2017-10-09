using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Web.Controls.GenerationClasses.BaseJournal;

namespace Nat.Web.Controls.GenerationClasses.HierarchyFields
{
    public static class ColumnHierarchyExtensions
    {
        public static IEnumerable<string> GetVisibleColumnNames(this List<ColumnHierarchy> columnHierarchy, Dictionary<string, BaseColumn> columnsDic)
        {
            return columnHierarchy.
                Where(r => r.IsVisibleColumn(columnsDic)).
                SelectMany(r => r.GetVisibleColumnNames(columnsDic));
        }

        public static IEnumerable<ColumnHierarchy> GetVisibleColumns(this List<ColumnHierarchy> columnHierarchy, Dictionary<string, BaseColumn> columnsDic)
        {
            return columnHierarchy.
                Where(r => r.IsVisibleColumn(columnsDic)).
                SelectMany(r => r.GetVisibleColumns(columnsDic));
        }

        public static IEnumerable<ColumnHierarchy> GetAllItems(this List<ColumnHierarchy> columnHierarchy)
        {
            return columnHierarchy.SelectMany(r => r.SelectAll());
        }

        public static void SetOrders(this List<ColumnHierarchy> columnHierarchy)
        {
            var i = 1;
            foreach (var item in columnHierarchy)
            {
                item.Order = i++;
                item.Childs.SetOrders();
            }
        }

        public static void Order(this List<ColumnHierarchy> columnHierarchy)
        {
            if(columnHierarchy.Count == 0) return;
            /*var i = columnHierarchy.Max(r => r.Order) + 1;
            foreach (var item in columnHierarchy.Where(r => r.Order == 0))
                item.Order = i++;*/
            if (columnHierarchy.FirstOrDefault(r => r.Order == 0) == null)
            	columnHierarchy.Sort(new Comparison<ColumnHierarchy>((r1, r2) => r1.Order.CompareTo(r2.Order)));
            foreach (var item in columnHierarchy)
                item.Childs.Order();
        }
        /*
        public static void EnsureColumnHierarchyExistsInDic(this List<ColumnHierarchy> columnHierarchy, Dictionary<string, BaseColumn> columnsDic)
        {
            foreach (var item in columnHierarchy)
            {
                if (item.ColumnName != null && !columnsDic.ContainsKey(item.ColumnName))
                    columnsDic[item.ColumnName] = item.
            }
        }*/
    }
}
