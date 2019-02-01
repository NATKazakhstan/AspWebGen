/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.ExportInExcel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    using Nat.Tools.System;
    using Nat.Web.Controls.Data;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.GenerationClasses.HierarchyFields;
    using Nat.Web.Controls.GenerationClasses.Navigator;
    using Nat.Web.Tools.Export.Formatting;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1215:InstanceReadonlyElementsMustAppearBeforeInstanceNonReadonlyElements", Justification = "Reviewed. Suppression is OK here.")]
    public class ExporterXslx<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> : BaseExporterXslx
        where TDataContext : DataContext, new()
        where TKey : struct
        where TTable : class
        where TFilterControl : BaseFilterControl<TKey, TTable>, new()
        where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
        where TRow : BaseRow, new()
        where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
        where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
        where TNavigatorValues : BaseNavigatorValues, new()
        where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
    {
        // ReSharper disable InconsistentNaming
        private BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
            TNavigatorControl, TNavigatorValues, TFilter> _journalControl;
        //// ReSharper restore InconsistentNaming

        public Stream GetExcel(BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl, RvsSavedProperties properties)
        {
            if (HttpContext.Current != null)
            {
                foreach (var key in HttpContext.Current.Items.Keys.OfType<string>().ToList().Where(r => r.EndsWith(".FiltersCache")))
                    HttpContext.Current.Items.Remove(key);
            }

            var page = new Page();
            _journalControl = journalControl;
            _journalControl.StorageValues = properties.StorageValues;
            _journalControl.InitializeControls(properties);
            page.TemplateControl.Controls.Add(_journalControl);
            return GetExcel();
        }

        protected override void AddRowsStyles()
        {
            foreach (var row in _journalControl.Journal.Rows.Where(r => r.Visible)) 
                AddStyles(row);

            var items = _journalControl.Journal.InnerHeader.ColumnHierarchy.GetAllItems();
            foreach (var columnHierarchy in items)
            {
                columnHierarchy.StyleId =
                    AddStyle(
                        new CellProperties { BColor = columnHierarchy.BColor, PColor = columnHierarchy.PColor },
                        columnHierarchy.IsVerticalHeader ? HeaderVertiacalStyleId : HeaderStyleId,
                        "solid").ToString();
            }
        }

        protected override int GetFixedColumnsCount()
        {
            return _journalControl.FixedColumnsCount;
        }

        protected override int GetFixedRowsCount()
        {
            return _journalControl.FixedHeader ? _journalControl.FixedRowsCount + GetCountRowsBeforeData() + (RenderFirstHeaderTable?.Rows.Count ?? 0) : 0;
        }

        #region Render Header

        protected override void RenderColumnsSettings()
        {
            var header = _journalControl.Journal.InnerHeader;
            _writer.WriteStartElement("cols");
            var columns = header.GetAllDataColumns()
                .Where(r => r.IsVisibleColumn(header.ColumnsDic))
                .Where(r => r.Childs.Count == 0 || string.IsNullOrEmpty(r.CrossColumnKey))
                .ToList();
            for (int i = 0; i < columns.Count;)
            {
                var item = columns[i];
                var maxI = ++i;
                int width = item.Width;
                if (item.HideInHeader && !string.IsNullOrEmpty(item.ColumnName))
                    width = item.Parent.Width;
                while (columns.Count > maxI && columns[maxI].Width == width)
                    maxI++;
                _writer.WriteStartElementExt("col", "min", i.ToString(), "max", maxI.ToString());
                if (width > 0)
                {
                    _writer.WriteAttributeString("width", (width * 0.139 + 1.8).ToString().Replace(',', '.'));
                    _writer.WriteAttributeString("customWidth", "1");
                }

                _writer.WriteEndElement();
                i = maxI;
            }

            _writer.WriteEndElement();
        }

        protected override void RenderHeader()
        {
            var header = _journalControl.Journal.InnerHeader;
            int maxRowSpan = header.GetMaxRowSpan();
            _addedRowSpans.Clear();
            int level = 0;
            do
            {
                if (level < header.RowsProperties.Count)
                    WriteStartRow(header.RowsProperties[level].Height);
                else
                    WriteStartRow(null);
                MoveRowIndex();
                foreach (var columnHierarchy in header.ColumnHierarchy.Where(r => r.IsVisibleColumn(header.ColumnsDic)))
                    RenderColumnHierarchy(columnHierarchy, _writer, level, 0, maxRowSpan, header.ColumnsDic);

                _writer.WriteEndElement();
            }
            while (++level < maxRowSpan);
        }
        
        public void RenderColumnHierarchy(ColumnHierarchy columnHierarchy, XmlTextWriter writer, int level, int inLevel, int maxRowSpan, Dictionary<string, BaseColumn> columnsDic)
        {
            if (level == 0 && !columnHierarchy.HideInHeader)
                RenderColumnHierarchy(columnHierarchy, writer);

            foreach (var child in columnHierarchy.GetChilds().Where(r => r.IsVisibleColumn(columnsDic)))
                RenderColumnHierarchy(child, writer, level - columnHierarchy.RowSpan, inLevel + columnHierarchy.RowSpan, maxRowSpan - columnHierarchy.RowSpan, columnsDic);
        }

        private void RenderColumnHierarchy(ColumnHierarchy columnHierarchy, XmlTextWriter writer)
        {
            RenderCell(writer, columnHierarchy.Header, columnHierarchy.RowSpan, columnHierarchy.ColSpan, columnHierarchy.StyleId ?? (columnHierarchy.IsVerticalHeader ? HeaderVertiacalStyleId : HeaderStyleId), ColumnType.Other, string.Empty);
        }

        protected override IEnumerable<string> GetFilterStrings()
        {
            return _journalControl.BaseFilter.GetFilterStrings();
        }

        protected override string GetHeader()
        {
            return _journalControl.TableHeader;
        }

        protected override string GetSheetName()
        {
            return _journalControl.Journal.GetExportSheetName() ?? base.GetSheetName();
        }

        protected override List<ConditionalFormatting> GetConditionalFormatting()
        {
            return _journalControl.Journal.GetConditionalFormatting();
        }

        #endregion

        #region Render Data

        protected override void RenderData()
        {
            foreach (var row in _journalControl.Journal.Rows.Where(r => r.Visible))
                RenderData(row);
            _addedRowSpans.Clear();
        }

        private void RenderData(BaseJournalRow<TRow> row)
        {
            if (row.RowKey != null && _journalControl.Journal.RowsPropertiesDic.ContainsKey(row.RowKey))
                WriteStartRow(_journalControl.Journal.RowsPropertiesDic[row.RowKey].Height);
            else
                WriteStartRow(null);

            MoveRowIndex();
            var cellsPropertiesDic = _journalControl.Journal.CellsPropertiesDic;
            foreach (var cell in row.AllCells.Where(r => r.Value.Visible))
            {
                var bCell = cell.Value as BaseJournalCell;
                string styleId = null;
                if (bCell != null && cellsPropertiesDic.ContainsKey(bCell.GetCellKey())) // достаем стиль
                    styleId = cellsPropertiesDic[bCell.GetCellKey()].StyleId;

                // если стиль не был найден используем стандартный
                if (styleId == null) 
                {
                    if (cell.Key != null && cell.Key.UsingInGroup)
                        styleId = bCell != null && bCell.IsVertical() ? DataGroupVerticalStyleId : DataGroupStyleId;
                    else if (bCell != null && bCell.RenderContext.TotalGroupValues != null)
                        styleId = bCell.Column.ColumnType == ColumnType.Numeric ? DataGroupStyleCenterId : DataGroupStyleId;
                    else if (bCell != null && bCell.IsVertical())
                        styleId = DataVerticalStyleId;
                    else if (bCell != null && bCell.Column != null && bCell.Column.ColumnType == ColumnType.Numeric)
                        styleId = DataStyleCenterId;
                    else
                        styleId = DataStyleId;
                }

                if (bCell != null)
                {
                    string cellData = bCell.GetValue().Replace("&nbsp;", " ");
                    if (!string.IsNullOrEmpty(bCell.RenderContext.ColumnHierarchy.ColumnName) && bCell.RenderContext.GroupValues == null)
                    {
                        StringBuilder sb;
                        if (_columnsAddresses.ContainsKey(bCell.RenderContext.ColumnHierarchy.ColumnName))
                            sb = _columnsAddresses[bCell.RenderContext.ColumnHierarchy.ColumnName].Append(" ");
                        else 
                            _columnsAddresses[bCell.RenderContext.ColumnHierarchy.ColumnName] = sb = new StringBuilder();
                        sb.Append(GetLaterByInt(_columnIndex)).Append(_rowIndex);
                    }

                    var formula = bCell.RenderContext.GetFormula()?.ToString(_columnIndex, _rowIndex) ?? string.Empty;
                    var rangeOfCell = RenderCell(_writer, cellData, bCell.RowSpan, bCell.ColSpan, styleId, bCell.Column == null ? ColumnType.Other : bCell.Column.ColumnType, formula);
                    if (cell.Key != null && cell.Key.TypeCell == BaseJournalTypeCell.HyperLink && bCell.RenderContext.TotalGroupValues == null)
                    {
                        var href = cell.Key.GetHyperLink(bCell.RenderContext);
                        if (!string.IsNullOrEmpty(href)) 
                            AddHyperLink(rangeOfCell, cellData, href);
                    }
                }
                else
                    RenderCell(_writer, string.Empty, 1, 1, styleId, ColumnType.Other, string.Empty);
            }

            _writer.WriteEndElement();
        }

        #endregion

        #region Render Footer

        protected override Table RenderFooterTable => _journalControl.Journal.ExportFooter;
        protected override Table RenderFirstHeaderTable => _journalControl.Journal.ExportHeader;
        protected override Table[] RenderAdditionalSheetsTable => _journalControl.Journal.AdditionalSheetsTable;

        #endregion

        #region help functions, properties

        private void AddStyles(BaseJournalRow<TRow> row)
        {
            var rowProperties = row.RowKey != null && _journalControl.Journal.RowsPropertiesDic.ContainsKey(row.RowKey)
                                    ? _journalControl.Journal.RowsPropertiesDic[row.RowKey]
                                    : null;
            var cellsProperties = _journalControl.Journal.CellsProperties;
            var cellsPropertiesDic = _journalControl.Journal.CellsPropertiesDic;
            foreach (var cell in row.AllCells)
            {
                var bCell = (BaseJournalCell)cell.Value;
                var hierarchyItem = bCell.RenderContext.ColumnHierarchy;
                string styleId;
                if (cell.Key != null && cell.Key.UsingInGroup)
                    styleId = bCell != null && bCell.IsVertical() ? DataGroupVerticalStyleId : DataGroupStyleId;
                else if (bCell != null && bCell.RenderContext.TotalGroupValues != null)
                    styleId = bCell.Column.ColumnType == ColumnType.Numeric ? DataGroupStyleCenterId : DataGroupStyleId;
                else if (bCell != null && bCell.IsVertical())
                    styleId = DataVerticalStyleId;
                else if (bCell != null && bCell.Column != null && bCell.Column.ColumnType == ColumnType.Numeric)
                    styleId = DataStyleCenterId;
                else
                    styleId = DataStyleId;

                string cellKey = null;
                //todo: реализовать else
                if (bCell != null)
                    cellKey = bCell.GetCellKey();

                // если есть у ячеки ключ
                if (cellKey != null)
                {
                    CellProperties cellProps = null;
                    if (cellsPropertiesDic.ContainsKey(cellKey))
                    {
                        cellProps = cellsPropertiesDic[cellKey];
                    }
                    else if (rowProperties != null || (hierarchyItem != null 
                                                       && (hierarchyItem.BColor != null 
                                                           || hierarchyItem.PColor != null
                                                           || hierarchyItem.HAligment != null)))
                    {
                        // созадем свйоства для ячейки, если имеются настройки для строки или колонки
                        cellsPropertiesDic[cellKey] = cellProps = new CellProperties { Key = cellKey };
                        cellsProperties.Add(cellProps);
                    }

                    if (cellProps != null)
                    {
                        if (hierarchyItem != null && bCell.ColSpan <= 1)
                        {
                            if (hierarchyItem.BColor != null && cellProps.BColor == null)
                                cellProps.BColor = hierarchyItem.BColor;
                            if (hierarchyItem.PColor != null && cellProps.PColor == null)
                                cellProps.PColor = hierarchyItem.PColor;
                            if (hierarchyItem.HAligment != null && cellProps.HAligment == null)
                                cellProps.HAligment = hierarchyItem.HAligment;
                        }

                        // если есть настройки у строки то передаем их ячейки
                        if (rowProperties != null)
                        {
                            if (cellProps.BColor == null)
                                cellProps.BColor = rowProperties.BColor;
                            if (cellProps.PColor == null)
                                cellProps.PColor = rowProperties.PColor;
                            if (cellProps.Size == null)
                                cellProps.Size = rowProperties.Size;
                            if (cellProps.Bold == null)
                                cellProps.Bold = rowProperties.Bold;
                            if (cellProps.Italic == null)
                                cellProps.Italic = rowProperties.Italic;
                            if (cellProps.HAligment == null)
                                cellProps.HAligment = rowProperties.HAligment;
                        }

                        // если стиль был найден или создан, то проставляем ключ стиля
                        cellProps.StyleId = AddStyle(cellProps, styleId, "Solid").ToString();
                    }
                }
            }
        }

        protected override int GetCountColumns()
        {
            var footerCount = GetCountColumns(_journalControl.Journal.ExportFooter);
            var headerCount = GetCountColumns(_journalControl.Journal.ExportHeader);
            return Math.Max(Math.Max(_journalControl.Journal.InnerHeader.GetFullColSpan(), footerCount), headerCount);
        }

        protected override int GetCountRows()
        {
            // количество строк журнала RenderData
            return _journalControl.Journal.Rows.Count(r => r.Visible)
                   + GetCountRowsBeforeData()
                   + (_journalControl.Journal.ExportFooter?.Rows.Count ?? 0)
                   + (_journalControl.Journal.ExportHeader?.Rows.Count ?? 0);
        }

        private int GetCountRowsBeforeData()
        {
            return 1 // Название заголовка и пустая строка RenderSheetHeader
                   + _journalControl.Journal.InnerHeader.GetMaxRowSpan() // количество строк в заголовке RenderHeader
                   + _journalControl.BaseFilter.GetFiltersCount() + 1; // Строки фильтров и одна пустая RenderFilter
        }

        #endregion
    }
}
