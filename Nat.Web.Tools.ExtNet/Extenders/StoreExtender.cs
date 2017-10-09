/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.26
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Data.SqlClient;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;

    public static class StoreExtender
    {
        public static void InitializeStoreModel(this AbstractStore store, IEnumerable<IGridColumn> columns)
        {
            foreach (var column in columns)
            {
                var fields = column.CreateModelFields();
                if (fields != null)
                {
                    foreach (var field in fields)
                        store.Model[0].Fields.Add(field);
                }

                if (column.HasChildren)
                    store.InitializeStoreModel(column.Children);
            }
        }

        public static void ReadData<TKey, TTable, TDataContext, TRow>(
            this Store store,
            BaseDataSourceView<TKey, TTable, TDataContext, TRow> view,
            StoreReadDataEventArgs e,
            BaseFilterControl<TKey, TTable, TDataContext> filter,
            GridFilters gridFilter,
            BaseGridColumns columns)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            if (filter.Url.UserControl == null)
                filter.Url.UserControl = filter.GetTableName() + "Journal";
            try
            {
                var data = view.Select(e, filter, gridFilter, columns, store.RemoteSort);
                if (!string.IsNullOrEmpty(store.DataSourceID))
                    store.DataSourceID = null;
                store.DataSource = data;
            }
            catch (SqlException exception)
            {
                if (exception.Number != -2)
                    throw;

                store.DataSource = new TRow[0];
                X.Msg.Show(new MessageBoxConfig
                {
                    Title = Properties.Resources.SInformation,
                    Message = Properties.Resources.STimeOut,
                    Buttons = MessageBox.Button.OK,
                    Icon = MessageBox.Icon.WARNING,
                });
            }
        }

    }
}
