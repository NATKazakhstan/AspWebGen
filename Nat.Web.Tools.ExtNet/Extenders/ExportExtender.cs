namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Tools.Export;

    public static class ExportExtender
    {
        public static void Group(this JournalExportEventArgs args, IEnumerable<GridColumn> columns, Store store)
        {
            if (string.IsNullOrEmpty(store.GroupField))
                return;

            var column = columns.FirstOrDefault(r => r.ColumnName == store.GroupField);
            if (column == null)
                return;

            var properties = column.ServerMapping.Split('.');
            args.GetGroupText = row =>
                {
                    var value = row;
                    for (var i = 0; i < properties.Length && value != null; i++)
                    {
                        var propInfo = TypeDescriptor.GetProperties(value.GetType());
                        var property = propInfo.Find(properties[i], false);
                        value = property?.GetValue(value);
                    }

                    return value?.ToString();
                };
        }

        public static ConfigTotalValues Total(this JournalExportEventArgs args, IEnumerable<GridColumn> columns)
        {
            var agg = new AggregateDataSimple();
            var configTotalValues = new ConfigTotalValues { Aggregate = agg };
            args.ComputeTotalValue = (r, group, column) =>
                {
                    if (configTotalValues.ComputeTotalValue(r, group, column))
                        return;

                    if (!column.IsNumericColumn)
                        return;

                    var aggregateType = GetAggType(column as GridColumn);
                    if (aggregateType == ColumnAggregateType.None)
                        return;

                    var row = (BaseRow)r;
                    var strValue = column.GetValue(row);
                    if (string.IsNullOrEmpty(strValue))
                        return;

                    var value = Convert.ToDouble(strValue);
                    var groupAgg = agg.GetChildGroup(column.ColumnName);
                    groupAgg.StrValue = column.ColumnName;
                    groupAgg.AggregateType = aggregateType;
                    Aggregate(groupAgg, value);

                    groupAgg = groupAgg.GetChildGroup(group);
                    groupAgg.AggregateType = aggregateType;
                    groupAgg.StrValue = group;
                    Aggregate(groupAgg, value);

                    var rowAgg = groupAgg.GetChildGroup(row.Value);
                    rowAgg.StrValue = strValue;
                    Aggregate(rowAgg, value);
                    rowAgg.Value = value;
                };
            args.GetTotalValue = (group, column) =>
                {
                    if (configTotalValues.HasTotalValue(column))
                        return configTotalValues.GetTotalValue(group, column);

                    if (!column.IsNumericColumn)
                        return null;

                    var gridColumn = (GridHtmlGenerator.Column)column;
                    var value = configTotalValues.GetTotalValue(group, column.ColumnName);
                    return string.Format(gridColumn.Format ?? "{0}", value);
                };

            return configTotalValues;
        }

        private static void Aggregate(AggregateData agg, double value)
        {
            switch (agg.AggregateType)
            {
                case ColumnAggregateType.Sum:
                    agg.Value = (agg.Value ?? 0) + value;
                    break;
                case ColumnAggregateType.Count:
                    agg.Count++;
                    break;
                case ColumnAggregateType.Avg:
                    agg.Value = (agg.Value ?? 0) + value;
                    agg.Count++;
                    break;
                case ColumnAggregateType.Max:
                    agg.Value = agg.Value == null ? value : Math.Max(agg.Value.Value, value);
                    break;
                case ColumnAggregateType.Min:
                    agg.Value = agg.Value == null ? value : Math.Min(agg.Value.Value, value);
                    break;
            }
        }

        private static ColumnAggregateType GetAggType(GridColumn column)
        {
            if (column == null)
                return ColumnAggregateType.Sum;

            switch (column.SummaryType)
            {
                case SummaryType.None:
                    return ColumnAggregateType.None;
                case SummaryType.Average:
                    return ColumnAggregateType.Avg;
                case SummaryType.Count:
                    return ColumnAggregateType.Count;
                case SummaryType.Max:
                    return ColumnAggregateType.Max;
                case SummaryType.Min:
                    return ColumnAggregateType.Min;
                case SummaryType.Sum:
                    return ColumnAggregateType.Sum;
                default:
                    return ColumnAggregateType.None;
            }
        }

        public class ConfigTotalValues
        {
            private readonly Dictionary<string, Action<ComputeTotalValuesEventArgs>> _configCompute =
                new Dictionary<string, Action<ComputeTotalValuesEventArgs>>();

            private readonly Dictionary<string, Func<TotalValuesEventArgs, string>> _configGetTotal =
                new Dictionary<string, Func<TotalValuesEventArgs, string>>();

            internal AggregateDataSimple Aggregate { get; set; }

            internal double? GetTotalValue(string group, string column)
            {
                var groupAgg = Aggregate.GetChildGroup(column);
                if (group != null)
                    groupAgg = groupAgg.GetChildGroup(group);

                switch (groupAgg.AggregateType)
                {
                    case ColumnAggregateType.Min:
                    case ColumnAggregateType.Max:
                    case ColumnAggregateType.Sum:
                        return groupAgg.Value;
                    case ColumnAggregateType.Count:
                        return groupAgg.Count;
                    case ColumnAggregateType.Avg:
                        if (groupAgg.Count == 0)
                            return null;
                        return groupAgg.Value / groupAgg.Count;
                }

                return 0;
            }

            internal bool ComputeTotalValue(object row, string group, IExportColumn column)
            {
                if (!_configCompute.ContainsKey(column.ColumnName))
                    return false;

                _configCompute[column.ColumnName](new ComputeTotalValuesEventArgs(this, row, group, column));
                return true;
            }

            internal bool HasTotalValue(IExportColumn column)
            {
                return _configGetTotal.ContainsKey(column.ColumnName);
            }

            internal string GetTotalValue(string group, IExportColumn column)
            {
                return _configGetTotal[column.ColumnName](new TotalValuesEventArgs(this, group, column));
            }

            public ConfigTotalValues GetTotalValue(string columnName, Func<TotalValuesEventArgs, string> getTotalValue)
            {
                _configGetTotal[columnName] = getTotalValue;
                return this;
            }

            public ConfigTotalValues ComputeTotalValue(string columnName, Action<ComputeTotalValuesEventArgs> computeTotalValue)
            {
                _configCompute[columnName] = computeTotalValue;
                return this;
            }
        }

        public class TotalValuesEventArgs
        {
            public TotalValuesEventArgs(ConfigTotalValues configTotalValues, string group, IExportColumn column)
            {
                ConfigTotalValues = configTotalValues;
                Group = group;
                Column = column;
            }

            internal ConfigTotalValues ConfigTotalValues { get; set; }
            public string Group { get; }
            public IExportColumn Column { get; }

            public double? GetTotalValue(string column)
            {
                return ConfigTotalValues.GetTotalValue(Group, column);
            }

            public double? GetTotalValue(IExportColumn column)
            {
                return ConfigTotalValues.GetTotalValue(Group, column.ColumnName);
            }

            public double? GetTotalValue(string group, string column)
            {
                return ConfigTotalValues.GetTotalValue(group, column);
            }

            public double? GetTotalValue(string group, IExportColumn column)
            {
                return ConfigTotalValues.GetTotalValue(group, column.ColumnName);
            }
        }

        public class ComputeTotalValuesEventArgs
        {
            public ComputeTotalValuesEventArgs(ConfigTotalValues configTotalValues, object row, string group, IExportColumn column)
            {
                ConfigTotalValues = configTotalValues;
                Row = row;
                Group = group;
                Column = column;
            }

            internal ConfigTotalValues ConfigTotalValues { get; set; }

            public object Row { get; }
            public string Group { get; }
            public IExportColumn Column { get; }

            public double? GetTotalValue(string group, string column)
            {
                return ConfigTotalValues.GetTotalValue(group, column);
            }
        }
    }
}