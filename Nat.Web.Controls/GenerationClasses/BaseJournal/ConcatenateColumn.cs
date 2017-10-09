using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using System.Web.UI;
using System.Web.Script.Serialization;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    [Serializable]
    public class ConcatenateColumn : BaseColumn
    {
        public const string ColumnNamePrefix = "_concat_";
        private const string СolumnNameSplitter = "_";
        private const string СolumnHeaderSplitter = "; ";
        private const string СolumnValueSplitter = "\r\n";

        private List<BaseColumn> _concatenatedColumns;

        public ConcatenateColumnType ConcatenateColumnType { get; set; }
        public bool IsNew { get; set; }
        public bool MarkAsDeleted { get; set; }

        public List<BaseColumn> ConcatenatedColumns
        {
            get
            {
                if (_concatenatedColumns == null)
                    _concatenatedColumns = new List<BaseColumn>();
                return _concatenatedColumns;
            }
        }

        private void CheckConcatenateColumnType()
        {
            bool isSumm = false;
            foreach (var column in ConcatenatedColumns)
            {
                isSumm = column.ColumnType == ColumnType.Numeric;
                if (!isSumm)
                    break;
            }
            ConcatenateColumnType = isSumm ? ConcatenateColumnType.Summ : ConcatenateColumnType.Concatenate;
        }

        private void CheckAllowAggregate()
        {
            if (ConcatenatedColumns.FirstOrDefault(r => r.ColumnType != ColumnType.Numeric) == null)
            {
                AllowAggregate = true;
                AggregateType = ColumnAggregateType.Sum;
            }
        }

        public void BuildColumn()
        {
            ColumnName = ColumnNamePrefix;
            Header = "";
            HideIfValueEquals = ConcatenatedColumns.Select(r => r.HideIfValueEquals).FirstOrDefault();
            foreach (var column in ConcatenatedColumns)
            {
                ColumnName += column.ColumnName + СolumnNameSplitter;
                Header += column.Header + СolumnHeaderSplitter;
                if (string.IsNullOrEmpty(column.HideIfValueEquals) || HideIfValueEquals != column.HideIfValueEquals)
                    HideIfValueEquals = null;
            }
            if (ColumnName.Length > 0)
                ColumnName = ColumnName.Substring(0, ColumnName.Length - СolumnNameSplitter.Length);
            if (Header.Length > 0)
                Header = Header.Substring(0, Header.Length - СolumnHeaderSplitter.Length);
            // TODO: Copy needed properties
        }

        public void AddColumn(BaseColumn column)
        {
            if (column == null) throw new ArgumentNullException("column", "Gived column is null");
            ConcatenatedColumns.Add(column);
            BuildColumn();
            CheckConcatenateColumnType();
            CheckAllowAggregate();
        }

        public List<string> GetConcatenateColumnsNames()
        {
            return ConcatenatedColumns.Select(r => r.ColumnName).ToList();
        }

        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (ConcatenateColumnType == ConcatenateColumnType.Concatenate)
            {
                if (groupValues != null)
                    return "";
                string value = "";
                foreach (var column in ConcatenatedColumns)
                {
                    var temp = column.GetName(row, item, crossColumnId, null);
                    if (!string.IsNullOrEmpty(temp))
                        value += temp + СolumnValueSplitter;
                }
                return value.TrimEnd(СolumnValueSplitter.ToCharArray());
            }
            else
            {
                if (groupValues != null)
                    return GetGroupValue(crossColumnId, groupValues);
                double value = 0;
                foreach (var column in ConcatenatedColumns)
                    value = value + Convert.ToDouble(column.GetValue(row, item, crossColumnId, null));
                return value;
            }
        }

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            var value = GetValue(row, item, crossColumnId, groupValues);
            return value != null ? value.ToString() : null;
        }
        
        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            if (ConcatenatedColumns.Count != 0)
                ConcatenatedColumns[0].GetContent(row, item, writer, crossColumnId, groupValues);
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            return "";
        }

        public override object GetValue(RenderContext context)
        {
            if (ConcatenateColumnType == ConcatenateColumnType.Concatenate)
            {
                if (context.GroupValues != null) return "";
                string value = ConcatenatedColumns.
                    Select(column => column.GetName(context.OtherColumns[column.ColumnName])).
                    Where(temp => !string.IsNullOrEmpty(temp)).
                    Aggregate("", (current, temp) => current + temp + СolumnValueSplitter);
                return value.TrimEnd(СolumnValueSplitter.ToCharArray());
            }
            if (context.GroupValues != null) return GetGroupValue(context);

            DependenceColumns(context, ConcatenatedColumns);
            return ConcatenatedColumns.Aggregate<BaseColumn, double>(
                0, (current, column) =>
                   current + Convert.ToDouble(column.GetValue(context.OtherColumns[column.ColumnName])));
        }

        public override int GetRowsCount(RenderContext context)
        {
            if (ConcatenatedColumns.Count != 0)
                return ConcatenatedColumns[0].GetRowsCount(context);
            return 1;
        }

        public override bool ShowAsTemplate(RenderContext context)
        {
            return false;
        }

        public override string GetRowNumber(RenderContext context)
        {
            return null;
        }
    }

    [Serializable]
    public class ConcatenateColumnTransporter
    {
        public ConcatenateColumnTransporter()
        {
        }

        public ConcatenateColumnTransporter(ConcatenateColumn column)
        {
            _concatenatedColumnsNames = column.GetConcatenateColumnsNames();
        }

        private List<string> _concatenatedColumnsNames;
        public List<string> ConcatenatedColumnsNames
        {
            get
            {
                if (_concatenatedColumnsNames == null)
                    _concatenatedColumnsNames = new List<string>();
                return _concatenatedColumnsNames;
            }
            set { _concatenatedColumnsNames = value; }
        }
    }

    public static class ConcatenateColumnMaker
    {
        private const string ColumnListSplitter = ";";

        public static List<ConcatenateColumn> GetConcatenateColumns(List<ConcatenateColumnTransporter> transporters, Dictionary<string, BaseColumn> columns)
        {
            var result = new List<ConcatenateColumn>();
            if (transporters == null) return result;
            foreach (var transporter in transporters)
                result.Add(MakeConcatenateColumn(transporter, columns));

            return result;
        }

        private static ConcatenateColumn MakeConcatenateColumn(ConcatenateColumnTransporter transporter, Dictionary<string, BaseColumn> columns)
        {
            var result = new ConcatenateColumn();
            foreach (var columnName in transporter.ConcatenatedColumnsNames)
            {
                result.AddColumn(columns[columnName]);
            }
            return result;
        }

        public static List<ConcatenateColumnTransporter> GetConcatenateColumnTransporters(List<ConcatenateColumn> concatenateColumns)
        {
            var result = new List<ConcatenateColumnTransporter>();
            foreach(var column in concatenateColumns)
                result.Add(new ConcatenateColumnTransporter(column));
            return result;
        }

        public static void AddNewConcatenateColumn(string columnList, List<ConcatenateColumn> concatenateColumns, Dictionary<string, BaseColumn> columns)
        {
            if (!string.IsNullOrEmpty(columnList))
            {
                string[] concList = columnList.Split(new[] { ColumnListSplitter }, StringSplitOptions.RemoveEmptyEntries);
                var concCol = new ConcatenateColumn();
                concCol.Visible = true;
                concCol.IsNew = true;
                foreach (var columnName in concList)
                    concCol.AddColumn(columns[columnName]);
                if (!concatenateColumns.Exists(r => r.ColumnName == concCol.ColumnName))
                    concatenateColumns.Add(concCol);
            }
        }

        public static void RemoveConcatenateColumns(string columnNames, List<ConcatenateColumn> concatenateColumns)
        {
            if (!string.IsNullOrEmpty(columnNames))
            {
                string[] concList = columnNames.Split(new[] { ColumnListSplitter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var columnName in concList)
                {
                    var column = concatenateColumns.SingleOrDefault(r => r.ColumnName == columnName);
                    column.MarkAsDeleted = true;
                }
            }
        }
    }

    public enum ConcatenateColumnType
    {
        Summ,
        Concatenate
    }
}
