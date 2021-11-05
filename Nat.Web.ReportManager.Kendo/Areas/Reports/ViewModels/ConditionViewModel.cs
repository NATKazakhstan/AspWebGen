using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager.QueryGeneration;
using Nat.Tools;
using Nat.Tools.Constants;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Tools.ResourceTools;
using Nat.Web.Controls;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Core.System.Mvc;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class ConditionViewModel
    {
        public static ConditionViewModel From(IColumnFilter columnFilter)
        {
            var captions = (columnFilter as ColumnFilter)?.CustomColumnFilterTypeCaptions;
            var storage = columnFilter.GetStorage();
            var filterTypes = Enum.GetValues(typeof(ColumnFilterType))
                .Cast<ColumnFilterType>()
                .Where(r => (r & storage.AvailableFilters) != 0)
                .Select(r => new ColumnFilterTypeViewModel(r, captions))
                .ToList();

            var model = new ConditionViewModel
            {
                Key = storage.Name,
                Title = storage.Caption,
                FilterTypes = filterTypes,
                DefaultFilterType = storage.DefaultFilterType == null ? 0 : (int) storage.DefaultFilterType,
                DataType = storage.DataType?.Name ?? "Object",
                FilterType = (int) storage.FilterType,
                Value1 = storage.Value1,
                Value2 = storage.Value2,
                Values = storage.Values,
                VisibleValue1 = storage.FilterType.HasArgs(),
                VisibleValue2 = storage.FilterType.IsBinaryFilter(),
                AutoPostBack = (columnFilter as ColumnFilter)?.PostBack ?? false,
                RequireReload = storage.CustomConditions.Count > 0 || !string.IsNullOrEmpty((columnFilter as ColumnFilter)?.CascadeFromCondition),
                CheckedFilterCondition = (columnFilter as ICheckedFilterCondition)?.CheckedFilterCondition,
                CheckedFilterConditionTooltip = (columnFilter as ICheckedFilterCondition)?.CheckedFilterConditionTooltip,
                CascadeFromCondition = (columnFilter as ColumnFilter)?.CascadeFromCondition,
                CascadeFromParentField = storage.ParentColumn,
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
            if (inData == null)
                return null;

            if (!(inData is DataView dataView))
            {
                if (inData.OfType<BaseRow>().FirstOrDefault() != null)
                    return inData.Cast<BaseRow>().Select(r => new {r.id, r.Value, r.Name});
                return inData;
            }

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
            if (storage.RefDataSource is ReportDataSource reportDataSource)
            {
                TemplateValue1 = reportDataSource.GetTemplateValue("DynamicValue1");
                if ((storage.AvailableFilters & ColumnFilterType.Between) != 0)
                    TemplateValue2 = reportDataSource.GetTemplateValue("DynamicValue2");

                reportDataSource.Data = reportDataSource.GetStaticData(storage);
                storage.RefTableRolledIn = reportDataSource.Data == null;
                return reportDataSource.Data;
            }

            var filterRefTable = DataSourceHelper.GetDataTable(storage.RefDataSource);
            if (filterRefTable == null)
            {
                var data = storage.RefDataSource as IEnumerable;
                var ds = storage.RefDataSource as IDataSource;
                var dsView = ds?.GetView("Default") ?? storage.RefDataSource as DataSourceView;
                var isDSV = dsView is IDataSourceView;
                var arguments = isDSV ? new DataSourceSelectArguments() : new DataSourceSelectArguments(storage.DisplayColumn);
                dsView?.Select(arguments, r => data = r);
                return data;
            }

            var tableDataSource = GetTableDataSource(storage);
            var rolledIn =
                DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.ROLLED_IN, false);
            var isTree =
                DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.IS_TREE_REF, false);

            if (rolledIn || string.IsNullOrEmpty(DisplayColumn) || string.IsNullOrEmpty(ValueColumn))
            {
                if (string.IsNullOrEmpty(ValueColumn) && filterRefTable.PrimaryKey.Length > 0)
                    ValueColumn = filterRefTable.PrimaryKey[0].ColumnName;

                Columns = new List<ColumnViewModel>();
                foreach (DataColumn dc in filterRefTable.Columns)
                {
                    var column = ColumnViewModel.From(dc);
                    if (column != null)
                    {
                        Columns.Add(column);
                        if (string.IsNullOrEmpty(DisplayColumn))
                            DisplayColumn = column.field;
                    }
                }

                if (Columns.Count > 2)
                {
                    Columns[0].width = "330px";
                    Columns.Where(c => string.IsNullOrEmpty(c.width)).ForEach(c => c.width = "220px");
                }

                if (isTree)
                {
                    var relation = DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.TREE_REF_RELATION, "");
                    if (!string.IsNullOrEmpty(relation) && filterRefTable.ParentRelations[relation].ChildColumns.Length == 1)
                        ParentField = filterRefTable.ParentRelations[relation].ChildColumns[0].ColumnName;

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
        public bool AutoPostBack { get; set; }
        public bool RequireReload { get; set; }
        public bool AllowAddParameter { get; set; }
        public bool ParameterClone { get; set; }
        public bool Removed { get; set; }
        public string TemplateValue1 { get; set; }
        public string TemplateValue2 { get; set; }
        public string ParentField { get; set; }
        public List<ColumnViewModel> Columns { get; set; }
        public string CheckedFilterCondition { get; set; }
        public bool CheckedFilterConditionValue { get; set; }
        public bool ShowOnlySelectedValues { get; set; }
        public string CheckedFilterConditionTooltip { get; set; }
        public string CascadeFromCondition { get; set; }
        public string CascadeFromParentField { get; set; }

        public void InitStorage(ColumnFilterStorage storage)
        {
            storage.FilterType = (ColumnFilterType)FilterType;
            if (storage.DataType == null)
                return;
            ParseValues(storage.DataType);
            if (storage.FilterType == ColumnFilterType.In)
                storage.Values = Values ?? new object[0];
            else
            {
                storage.Value1 = Value1;
                storage.Value2 = Value2;
            }

            if (!string.IsNullOrEmpty(CheckedFilterCondition))
            {
                var condition = storage.CustomConditions.FirstOrDefault(r => r.ColumnName == CheckedFilterCondition);
                if (condition != null) condition.EmptyCondition = CheckedFilterConditionValue || ShowOnlySelectedValues;
            }

            if (ShowOnlySelectedValues)
            {
                var queryCondition = new QueryCondition("id", ColumnFilterType.In, storage.Values == null || storage.Values.Length == 0 ? new object[] {0L} : storage.Values);
                storage.CustomConditions.Add(queryCondition);
            }

            if (storage.RefDataSource is ReportDataSource)
                GetData(storage);
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
            try
            {
                string strValue;
                if (obj is object[] arr1)
                    strValue = (string) arr1[0];
                else if (obj is string str)
                    strValue = str;
                else if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                {
                    if (obj == null) return null;

                    if (obj is DateTime dateTime)
                        return dateTime.ToLocalTime();

                    strValue = Convert.ToString(obj);
                }
                else
                    return obj == null ? null : Convert.ChangeType(obj, dataType);

                if (string.IsNullOrEmpty(strValue))
                    return null;

                if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                {
                    if (strValue.StartsWith("/Date(")) return DateModelBinder.ParseDateTime(strValue);
                    return DateTime.Parse(strValue,
                            Thread.CurrentThread.CurrentCulture.DateTimeFormat,
                            DateTimeStyles.AssumeLocal)
                        .ToLocalTime();
                    //return ((DateTime) Convert.ChangeType(strValue, dataType)).ToLocalTime();
                }

                return Convert.ChangeType(strValue, dataType);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}