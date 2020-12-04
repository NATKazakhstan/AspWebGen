namespace Nat.ExportInExcel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nat.Web.Tools.Export;

    public class ExportColumn : IExportColumn
    {
        public List<ExportColumn> Children { get; } = new List<ExportColumn>();
        public string Header { get; set; }
        public string ColumnName { get; set; }
        public bool IsVerticalHeaderText { get; set; }
        public bool IsVerticalDataText { get; set; }
        public string NullItemText { get; set; }
        public bool Visible { get; set; } = true;
        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public bool HasChild => Children.Count > 0;
        public decimal Width { get; set; }
        public bool IsNumericColumn { get; set; }
        public Func<object, string> GetValueHandler { get; set; }
        public Func<object, string> GetHyperLinkHandler { get; set; }
        public Func<object, int> GetDataRowSpanHandler { get; set; }

        public virtual string GetValue(object row)
        {
            return GetValueHandler?.Invoke(row) ?? (row as ExportDataRow)?.GetValue(ColumnName);
        }

        public virtual string GetHyperLink(object row)
        {
            return GetHyperLinkHandler?.Invoke(row) ?? (row as ExportDataRow)?.GetHyperLink(ColumnName);
        }

        public virtual IEnumerable<IExportColumn> GetChilds()
        {
            return Children.Cast<IExportColumn>();
        }

        public virtual int? GetDataRowSpan(object row)
        {
            return GetDataRowSpanHandler?.Invoke(row);
        }
    }

    public class ExportColumn<TRow> : ExportColumn
    {
        public new Func<TRow, string> GetValueHandler { get; set; }
        public new Func<TRow, string> GetHyperLinkHandler { get; set; }

        public override string GetValue(object row)
        {
            return GetValueHandler?.Invoke((TRow)row) ?? (row as ExportDataRow)?.GetValue(ColumnName);
        }

        public override string GetHyperLink(object row)
        {
            return GetHyperLinkHandler?.Invoke((TRow)row) ?? (row as ExportDataRow)?.GetHyperLink(ColumnName);
        }
    }
}