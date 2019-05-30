using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager.QueryGeneration;
using Nat.Tools.Constants;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.ResourceTools;
using Nat.Web.Controls;
using Nat.Web.Core.System.Mvc;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class ConditionViewModel
    {
        public static ConditionViewModel From(ColumnFilterStorage storage)
        {
            var filterTypes = Enum.GetValues(typeof(ColumnFilterType))
                .Cast<ColumnFilterType>()
                .Where(r => (r & storage.AvailableFilters) != 0)
                .Select(r => new ColumnFilterTypeViewModel(r))
                .ToList();

            var model = new ConditionViewModel
            {
                Key = storage.Name,
                Name = storage.Name.Replace(":", "_").Replace(".", "_"),
                Title = storage.Caption,
                FilterTypes = filterTypes,
                DefaultFilterType = storage.DefaultFilterType == null ? 0 : (int)storage.DefaultFilterType,
                DataType = storage.DataType.Name,
                FilterType = (int)storage.FilterType,
                Value1 = storage.Value1,
                Value2 = storage.Value2,
                Values = storage.Values,
                VisibleValue1 = storage.FilterType.HasArgs(),
                VisibleValue2 = storage.FilterType.IsBinaryFilter(),
            };

            model.InitDataSource(storage);

            return model;
        }

        internal void InitDataSource(ColumnFilterStorage storage)
        {
            DisplayColumn = storage.DisplayColumn;
            ValueColumn = storage.ValueColumn;

            if (!storage.IsRefBound)
                return;

            Data = ParseDataView(GetData(storage));
        }

        public static IEnumerable ParseDataView(IEnumerable inData)
        {
            if (!(inData is DataView dataView))
                return inData;

            var data = new List<object>();
            var columns = dataView.Table.Columns.Cast<DataColumn>().Select((r, index) => new { r.ColumnName, index }).ToArray();
            foreach (DataRowView row in dataView)
                data.Add(columns.ToDictionary(r => r.ColumnName, r =>
                {
                    var value = row.Row.ItemArray[r.index];
                    if (value is DateTime dt)
                        return dt.ToShortDateString();

                    return value == DBNull.Value ? "" : value;
                }));

            return data;
        }

        private IEnumerable GetData(ColumnFilterStorage storage)
        {
            var filterRefTable = DataSourceHelper.GetDataTable(storage.RefDataSource);
            if (filterRefTable == null)
            {
                IEnumerable data = null;
                var ds = storage.RefDataSource as IDataSource;
                var dsView = ds?.GetView("Default") ?? storage.RefDataSource as DataSourceView;
                dsView?.Select(new DataSourceSelectArguments(storage.DisplayColumn), r => data = r);
                return data;
            }

            var tableDataSource = GetTableDataSource(storage);
            var rolledIn =
                DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.ROLLED_IN, false);

            if (rolledIn)
            {
                Columns = new List<ColumnViewModel>();
                foreach (DataColumn dc in filterRefTable.Columns)
                {
                    var column = ColumnViewModel.From(dc);
                    if (column != null)
                        Columns.Add(column);
                }

                return null;
            }

            tableDataSource.SortExpression = storage.DisplayColumn;
            return tableDataSource.Select();
        }

        public static TableDataSource GetTableDataSource(ColumnFilterStorage storage)
        {
            var filterRefTable = DataSourceHelper.GetDataTable(storage.RefDataSource);
            if (filterRefTable == null)
                return null;

            var tableAdapterType = TableAdapterTools.GetTableAdapterType(filterRefTable.GetType());
            var tableDataSource = new TableDataSource
            {
                TypeName = tableAdapterType.FullName,
                SelectMethod = storage.SelectMethod,
                FillType = TableDataSourceFillType.Always,
                SetFilterByCustomConditions = false
            };
            tableDataSource.View.CustomConditions.AddRange(storage.CustomConditions);
            tableDataSource.HistoricalCountKeys = 0;

            if (filterRefTable.Columns.IndexOf("dateEnd") != -1 &&
                filterRefTable.Columns.IndexOf("dateStart") != -1)
                tableDataSource.ShowHistoricalData = true;

            return tableDataSource;
        }

        public string Name { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public int DefaultFilterType { get; set; }
        public string DataType { get; set; }
        public int FilterType { get; set; }
        public object Value1 { get; set; }
        public object Value2 { get; set; }
        public object[] Values { get; set; }
        public bool VisibleValue1 { get; set; }
        public bool VisibleValue2 { get; set; }
        public List<ColumnFilterTypeViewModel> FilterTypes { get; set; }
        public string ValueColumn { get; set; }
        public string DisplayColumn { get; set; }
        public IEnumerable Data { get; set; }
        public bool Visible { get; set; }
        public List<ColumnViewModel> Columns { get; set; }

        public void SetToStorageValues(ColumnFilterStorage storage, StorageValues storageValues)
        {
            storage.FilterType = (ColumnFilterType)FilterType;
            ParseValues(storage.DataType);
            if (storage.FilterType == ColumnFilterType.In)
                storage.Values = Values;
            else
            {
                storage.Value1 = Value1;
                storage.Value2 = Value2;
            }

            storageValues.AddStorage(storage);
        }

        private void ParseValues(Type dataType)
        {
            Value1 = ParseValue(Value1, dataType);
            Value2 = ParseValue(Value2, dataType);
            
            if (FilterType == (int)ColumnFilterType.In)
                Values = Values?.Select(r => ParseValue(r, dataType)).ToArray();
        }

        private object ParseValue(object obj, Type dataType)
        {
            string strValue;
            if (obj is object[] arr1)
                strValue = (string) arr1[0];
            else if (obj is string str)
                strValue = str;
            else
                return obj == null ? null : Convert.ChangeType(obj, dataType);

            if (strValue.StartsWith("/Date(") && (dataType == typeof(DateTime) || dataType == typeof(DateTime?)))
                return DateModelBinder.ParseDateTime(strValue);
            return string.IsNullOrEmpty(strValue) ? null : Convert.ChangeType(strValue, dataType);
        }
    }
}