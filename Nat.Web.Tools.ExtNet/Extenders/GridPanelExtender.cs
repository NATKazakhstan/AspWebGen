/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.26
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;

    using Ext.Net;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;

    using IGridColumn = Nat.Web.Tools.ExtNet.IGridColumn;

    public static class GridPanelExtender
    {
        public static void ReadData<TKey, TTable, TDataContext, TRow>(
            this GridPanel gridPanel, 
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
            gridPanel.GetStore().ReadData<TKey, TTable, TDataContext, TRow>(view, e, filter, gridFilter, columns);
        }

        public static void InitializeColumns(this TablePanel gridPanel, BaseGridColumns gridColumns, GridFilters gridFilter, AbstractStore store)
        {
            var columns = gridColumns.GetExtNetGridColumns();
            store.InitializeStoreModel(columns);
            gridPanel.InitializeColumns(columns, gridPanel.ColumnModel.Columns);
            if (gridFilter != null)
                gridFilter.InitializeColumns(columns);
        }

        public static void InitializeColumns(this GridPanel gridPanel, BaseGridColumns gridColumns, GridFilters gridFilter)
        {
            InitializeColumns(gridPanel, gridColumns, gridFilter, gridPanel.GetStore());
        }

        public static void InitializeColumns(this TreePanel treePanel, BaseGridColumns gridColumns, GridFilters gridFilter)
        {
            var firstColumn = (IGridColumn)gridColumns.Columns.FirstOrDefault(r => r.Visible && !r.ColumnName.StartsWith("__"));
            if (firstColumn != null)
                firstColumn.IsTreeColumn = true;

            // todo: убрать этот код, т.к. кнопки нужны
            var buttons = (IGridColumn)gridColumns.Columns.FirstOrDefault(r => r.Visible && r.ColumnName.Equals("__buttons"));
            if (buttons != null)
                buttons.IsTreeColumn = true;

            InitializeColumns(treePanel, gridColumns, gridFilter, treePanel.GetStore());
        }

        public static void InitializeColumns(this TablePanel gridPanel, IEnumerable<IGridColumn> columns, ItemsCollection<ColumnBase> columnModel)
        {
            foreach (var column in columns.Where(r => r.ShowInGrid))
            {
                var extColumn = column.CreateColumn();
                columnModel.Add(extColumn);
                if (column.HasChildren)
                    gridPanel.InitializeColumns(column.Children, extColumn.Columns);
            }
        }

        public static void InitializeSelectMode(this TablePanel tablePanel, MainPageUrlBuilder url)
        {
            InitializeSelectMode(tablePanel, url, null);
        }

        public static void InitializeSelectMode(this TablePanel tablePanel, MainPageUrlBuilder url, string storeLoadHandler)
        {
            var gridPanel = tablePanel as GridPanel;
            var treePanel = tablePanel as TreePanel;
            if (gridPanel != null)
            {
                gridPanel.GetStore().Listeners.Load.Handler =
                    (string.IsNullOrEmpty(storeLoadHandler) ? string.Empty : storeLoadHandler + "; ")
                    + "GridStoreLoadHandler(#{grid}, store, #{SelectIDHidden});";
            }

            if (url.IsSelect && !url.IsMultipleSelect)
            {
                var selectionModel = (RowSelectionModel)tablePanel.GetSelectionModel();
                selectionModel.Mode = SelectionMode.Single;
                selectionModel.Visible = false;

                var handler = " if (e.target.className == null || e.target.className.indexOf('x-action') == -1) window.frameElement.addSelectedValues(record.data);";
                if (gridPanel != null)
                    gridPanel.Listeners.ItemClick.Handler = handler;

                if (treePanel != null)
                    treePanel.Listeners.ItemClick.Handler = handler;

                ResourceManager.AddInstanceScript(string.Format("setFrameElementSelectedRecords({0})", tablePanel.ClientID));
            }

            if (url.IsMultipleSelect)
            {
                var selectionModel = (RowSelectionModel)tablePanel.GetSelectionModel();
                selectionModel.Mode = SelectionMode.Single;
                selectionModel.Visible = false;
                string handler = @"
    var userGrid = #{selectedUserValues};
    var hasSameRow = userGrid.store.getById(record.data.id) != undefined;
    if (!hasSameRow){
        userGrid.store.add({id: record.data.id, RecordName: record.data.RowName});
    }";
                if (gridPanel != null)
                    gridPanel.Listeners.ItemClick.Handler = handler;

                if (treePanel != null)
                    treePanel.Listeners.ItemClick.Handler = handler;

                ResourceManager.AddInstanceScript(
                    string.Format(
                        "setFrameElementSelectedRecordsForUserGrid({0});",
                        tablePanel.Parent.NamingContainer.FindControl("selectedUserValues").ClientID));
            }
        }
    }
}
