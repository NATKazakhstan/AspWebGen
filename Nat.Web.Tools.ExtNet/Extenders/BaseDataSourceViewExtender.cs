/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.10.01
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Web.UI;

    using Ext.Net;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;

    public static class BaseDataSourceViewExtender
    {
        public static IEnumerable<TRow> Select<TKey, TTable, TDataContext, TRow>(
            this BaseDataSourceView<TKey, TTable, TDataContext, TRow> view,
            StoreReadDataEventArgs e,
            BaseFilterControl<TKey, TTable, TDataContext> filter,
            GridFilters gridFilter,
            BaseGridColumns columns,
            bool sort)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            var dataSourceSelectArguments = new DataSourceSelectArguments(e.Start, e.Limit);
            dataSourceSelectArguments.RetrieveTotalRowCount = true;

            if (sort && e.Sort != null && e.Sort.Length > 0)
                dataSourceSelectArguments.SortExpression = GetSortExpression(columns, e.Sort);

            IEnumerable result = null;
            view.Select(dataSourceSelectArguments, data => { result = data; });
            e.Total = dataSourceSelectArguments.TotalRowCount;

            return result.Cast<TRow>();
        }

        public static IEnumerable<TRow> Select<TKey, TTable, TDataContext, TRow>(
            this BaseDataSourceView<TKey, TTable, TDataContext, TRow> view,
            StoreReadDataEventArgs e,
            BaseFilterControl<TKey, TTable, TDataContext> filter,
            GridFilters gridFilter,
            BaseGridColumns columns)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            return Select(view, e, filter, gridFilter, columns, true);
        }

        public static string GetSortExpression(BaseGridColumns columns, DataSorter[] sort)
        {
            var dictionary = columns.Columns.OfType<GridColumn>().ToDictionary(r => r.ColumnNameIndex);
            var sortExpression =
                sort.Select(r => dictionary[r.Property].Sort + (r.Direction == SortDirection.DESC ? " desc" : string.Empty));
            return string.Join(",", sortExpression);
        }

        /// <summary>
        /// Сортировка данных на стороне SQL сервера.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="columns"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IQueryable<T> SortBy<T>(this IQueryable<T> data, BaseGridColumns columns, ICollection<DataSorter> sort)
            where T : class
        {
            var dictionary = columns.Columns.OfType<GridColumn>().ToDictionary(r => r.ColumnNameIndex);
            var sortExpression = string.Join(
                ",",
                sort
                    .Where(r => dictionary.ContainsKey(r.Property) && !string.IsNullOrEmpty(dictionary[r.Property].Sort))
                    .Select(r => dictionary[r.Property].Sort + (r.Direction == SortDirection.DESC ? " desc" : string.Empty)));

            if (string.IsNullOrEmpty(sortExpression))
                return data;

            return LinqFilterGenerator.OrderBy(data, sortExpression);
        }

        /// <summary>
        /// Сортировка данных после выполнения SQL запроса.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="columns"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static IEnumerable<T> SortBy<T>(this IEnumerable<T> data, BaseGridColumns columns, ICollection<DataSorter> sort)
            where T : class
        {
            var dictionary = columns.Columns.OfType<GridColumn>().ToDictionary(r => r.ColumnNameIndex);
            var sortExpression = string.Join(
                ",",
                sort
                    .Select(
                        r => (dictionary.ContainsKey(r.Property)
                                  ? (string.IsNullOrEmpty(dictionary[r.Property].Sort)
                                         ? dictionary[r.Property].ServerMapping
                                         : dictionary[r.Property].Sort)
                                  : r.Property)
                             + (r.Direction == SortDirection.DESC ? " desc" : string.Empty)));

            if (string.IsNullOrEmpty(sortExpression))
                return data;

            return LinqFilterGenerator.OrderBy(data.AsQueryable(), sortExpression);
        }
    }
}