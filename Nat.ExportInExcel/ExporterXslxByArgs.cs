/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.ExportInExcel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls;

    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Tools.Export;
    using Nat.Web.Tools.Export.Formatting;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1215:InstanceReadonlyElementsMustAppearBeforeInstanceNonReadonlyElements", Justification = "Reviewed. Suppression is OK here.")]
    public class ExporterXslxByArgs : BaseExporterXslx
    {
// ReSharper disable InconsistentNaming
        private JournalExportEventArgs _args;
//// ReSharper restore InconsistentNaming

        public Stream GetExcel(JournalExportEventArgs args)
        {
            _args = args;
            LogMonitor = args.LogMonitor;
            _args.FileNameExtention = "xlsx";
            return GetExcel();
        }

        protected override int GetFixedColumnsCount()
        {
            return _args.FixedColumnsCount;
        }

        protected override int GetFixedRowsCount()
        {
            if (_args.FixedRowsCount > 0)
                return _args.FixedRowsCount + (RenderFirstHeaderTable?.Rows.Count ?? 0);
            return 0;
        }

        protected override int GetCountRows()
        {
            return _args.Data.Count // количество данных
                + _args.Columns.Max(GetLevel) // количество строк в заголовке таблицы
                + (_args.FilterValues?.Count ?? 0) + 1 // количество фильтров + пустая строка
                   + (RenderFooterTable?.Rows.Count ?? 0)
                   + (RenderFirstHeaderTable?.Rows.Count ?? 0)
                + 1; // строка заголовка
        }

        protected override int GetCountColumns()
        {
            var footerCount = RenderFooterTable?.Rows.Cast<TableRow>()
                                  .Max(r => r.Cells.Cast<TableCell>().Max(c => c.ColumnSpan == 0 ? 1 : c.ColumnSpan)) ?? 0;
            var headerCount = RenderFirstHeaderTable?.Rows.Cast<TableRow>()
                                  .Max(r => r.Cells.Cast<TableCell>().Max(c => c.ColumnSpan == 0 ? 1 : c.ColumnSpan)) ?? 0;
            return Math.Max(Math.Max(_args.Columns.Sum((Func<IExportColumn, int>)CountColumns), footerCount), headerCount);
        }

        protected int GetRowsInHeader()
        {
            return _args.Columns.Max((Func<IExportColumn, int>)GetLevel);
        }

        private int CountColumns(IExportColumn column)
        {
            if (column.HasChild) 
                return column.GetChilds().Sum((Func<IExportColumn, int>)CountColumns);
            return column.Visible ? 1 : 0;
        }
        
        protected override string GetHeader()
        {
            return _args.Header;
        }

        protected override IEnumerable<string> GetFilterStrings()
        {
            return _args.FilterValues;
        }

        protected override void AddRowsStyles()
        {
        }

        protected override List<ConditionalFormatting> GetConditionalFormatting()
        {
            return _args.ConditionalFormatting;
        }

        protected override void RenderColumnsSettings()
        {
            _writer.WriteStartElement("cols");
            var columns = _args.Columns.SelectMany(GetColumns).ToList();
            for (int i = 0; i < columns.Count;)
            {
                var item = columns[i];
                var maxI = ++i;
                decimal width = item.Width;
                while (columns.Count > maxI && columns[maxI].Width == width)
                    maxI++;
                _writer.WriteStartElementExt("col", "min", i.ToString(), "max", maxI.ToString());
                if (width > 0)
                {
                    _writer.WriteAttributeString("width", width.ToString().Replace(',', '.'));
                    _writer.WriteAttributeString("customWidth", "1");
                }

                _writer.WriteEndElement();
                i = maxI;
            }

            _writer.WriteEndElement();
        }

        private IEnumerable<IExportColumn> GetColumns(IExportColumn column)
        {
            if (!column.Visible || column.RowSpan < 1)
                return new IExportColumn[0];
            if (column.HasChild)
                return column.GetChilds().SelectMany(GetColumns).Where(r => r.RowSpan > 0);
            return new[] { column };
        }

        protected override void RenderData()
        {
            var columns = _args.Columns.SelectMany(GetColumns).ToArray();
            if (_args.GetGroupText == null)
                foreach (var row in _args.Data)
                    RenderData(row, columns);
            else
            {
                var groupText = string.Empty;
                var rowIndex = -1;
                foreach (var row in _args.Data)
                {
                    rowIndex++;
                    var newGroupText = _args.GetGroupText(row);
                    if (rowIndex != 0 && _args.GetTotalValue != null && newGroupText != groupText)
                        RenderTotalRow(columns, groupText);

                    if (rowIndex == 0 || newGroupText != groupText)
                    {
                        _args.StartRenderRow?.Invoke(row);
                        WriteStartRow(null);
                        MoveRowIndex();
                        RenderCell(_writer, newGroupText, 1, columns.Length, DataGroupStyleId, ColumnType.Other, null);
                        _writer.WriteEndElement();
                    }

                    RenderData(row, columns);

                    if (_args.ComputeTotalValue != null)
                    {
                        foreach (var column in columns)
                            _args.ComputeTotalValue(row, newGroupText, column);
                    }

                    groupText = newGroupText;
                }

                if (rowIndex != 0 && _args.GetTotalValue != null)
                    RenderTotalRow(columns, groupText);

                if (rowIndex != 0 && _args.GetTotalValue != null)
                    RenderTotalRow(columns, null);
            }

            _addedRowSpans.Clear();
        }

        private void RenderTotalRow(IExportColumn[] columns, string groupText)
        {
            _args.StartRenderRow?.Invoke(null);
            WriteStartRow(null);

            MoveRowIndex();
            foreach (var column in columns)
            {
                string styleId;
                if (column.IsVerticalDataText) styleId = DataGroupVerticalStyleId;
                else if (column.IsNumericColumn) styleId = DataGroupStyleCenterId;
                else styleId = DataGroupStyleId;

                var totalValue = _args.GetTotalValue(groupText, column);
                RenderCell(
                    _writer,
                    totalValue,
                    column.RowSpan,
                    column.ColSpan,
                    styleId,
                    column.IsNumericColumn ? ColumnType.Numeric : ColumnType.Other,
                    null);
            }
         
            _writer.WriteEndElement();
        }

        private void RenderData(object row, IEnumerable<IExportColumn> columns)
        {
            _args.StartRenderRow?.Invoke(row);
            WriteStartRow(null);

            MoveRowIndex();
            foreach (var column in columns)
            {
                string styleId;

                if (column.IsVerticalDataText) styleId = DataVerticalStyleId;
                else if (column.IsNumericColumn) styleId = DataStyleCenterId;
                else styleId = DataStyleId;

                string cellData = column.GetValue(row);
                if (!string.IsNullOrEmpty(column.ColumnName))
                {
                    StringBuilder sb;
                    if (_columnsAddresses.ContainsKey(column.ColumnName)) sb = _columnsAddresses[column.ColumnName].Append(" ");
                    else _columnsAddresses[column.ColumnName] = sb = new StringBuilder();
                    sb.Append(GetLaterByInt(_columnIndex)).Append(_rowIndex);
                }

                var rangeOfCell = RenderCell(
                    _writer,
                    cellData,
                    column.RowSpan,
                    column.ColSpan,
                    styleId,
                    column.IsNumericColumn ? ColumnType.Numeric : ColumnType.Other,
                    string.Empty);
                
                var href = column.GetHyperLink(row);
                if (!string.IsNullOrEmpty(href)) AddHyperLink(rangeOfCell, cellData, href);
            }

            _writer.WriteEndElement();
        }

        protected override void RenderHeader()
        {
            int maxRowSpan = GetRowsInHeader();
            _addedRowSpans.Clear();
            int level = 0;
            do
            {
                WriteStartRow(null);
                MoveRowIndex();
                RenderHeader(_args.Columns.Where(r => r.Visible), level, 0, maxRowSpan);
                _writer.WriteEndElement();
            }
            while (++level < maxRowSpan);
        }

        protected override Table RenderFooterTable { get; }
        protected override Table RenderFirstHeaderTable { get; }
        protected override Table[] RenderAdditionalSheetsTable { get; }

        private void RenderHeader(IEnumerable<IExportColumn> columns, int renderLevel, int currentLevel, int maxRowSpan)
        {
            foreach (var column in columns)
            {
                if (renderLevel == currentLevel)
                {
                    var header = column.Header?.Replace("<br>", "\r\n").Replace("<br/>", "\r\n").Replace("<br />", "\r\n").Replace("</br>", "\r\n");
                    RenderCell(
                        _writer,
                        header,
                        column.HasChild ? 1 : maxRowSpan - currentLevel,
                        column.HasChild ? column.GetChilds().Sum((Func<IExportColumn, int>)GetColSpan) : 1,
                        column.IsVerticalHeaderText ? HeaderVertiacalStyleId : HeaderStyleId,
                        ColumnType.Other,
                        string.Empty);
                }
                else if (column.HasChild)
                    RenderHeader(column.GetChilds(), renderLevel, currentLevel + 1, maxRowSpan);
            }
        }

        private int GetLevel(IExportColumn column)
        {
            if (!column.Visible)
                return 0;
            if (column.HasChild)
                return column.GetChilds().Max((Func<IExportColumn, int>)GetLevel) + 1;
            return 1;
        }

        private int GetColSpan(IExportColumn column)
        {
            if (!column.Visible)
                return 0;
            if (column.HasChild)
                return column.GetChilds().Sum((Func<IExportColumn, int>)GetColSpan);
            return 1;
        }
    }
}
