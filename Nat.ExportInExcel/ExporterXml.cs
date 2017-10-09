using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Web.UI;
using System.IO;
using System.Xml;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using Nat.Web.Controls.GenerationClasses.Navigator;
using Nat.Web.Tools;

namespace Nat.ExportInExcel
{
    public class ExporterXml<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>
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
        private const string HeaderStyleId = "header";
        private const string HeaderVertiacalStyleId = "vHeader";
        private const string HeaderSheetStyleId = "sheetHeader";
        private const string DataStyleId = "data";
        private const string DataVerticalStyleId = "dataV";
        private const string DataGroupStyleId = "gData";
        private const string DataGroupVerticalStyleId = "gDataV";
        private const string FilterStyleId = "filter";
        
        
        private int[] _rowSpans;
        private readonly Dictionary<int, int> _addedRowSpans = new Dictionary<int, int>();
        private int _columnIndex;
        private bool _columnIndexMovedByRowSpan;
        
        private BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
            TNavigatorControl, TNavigatorValues, TFilter> _journalControl;
        private XmlTextWriter _writer;

        public ILogMonitor LogMonitor { get; set; }
        
        public Stream GetExcel(BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl, RvsSavedProperties properties)
        {
            var page = new Page();
            _journalControl = journalControl;
            _journalControl.StorageValues = properties.StorageValues;
            _journalControl.InitializeControls(properties);
            page.TemplateControl.Controls.Add(_journalControl);
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            _writer = new XmlTextWriter(streamWriter) {Formatting = Formatting.Indented};
            _writer.WriteProcessingInstruction("xml", "version=\"1.0\"");
            _writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
            _writer.WriteStartElement("Workbook");
            _writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:spreadsheet");
            _writer.WriteAttributeString("xmlns:o", "urn:schemas-microsoft-com:office:office");
            _writer.WriteAttributeString("xmlns:x", "urn:schemas-microsoft-com:office:excel");
            _writer.WriteAttributeString("xmlns:ss", "urn:schemas-microsoft-com:office:spreadsheet");
            _writer.WriteAttributeString("xmlns:html", "http://www.w3.org/TR/REC-html40");

            RenderDocumentProperties();
            RenderStyles();
            RenderWorksheet();

            _writer.WriteEndElement();
            _writer.Flush();
            streamWriter.Flush();
            stream.Position = 0;
            return stream;
        }

        #region Render Sheet

        private void RenderDocumentProperties()
        {
            _writer.WriteStartElement("DocumentProperties");
            _writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:office");

            #region Author
            _writer.WriteStartElement("Author");
            var identity = WindowsIdentity.GetCurrent();
            if (identity != null) _writer.WriteValue(identity.Name);
            _writer.WriteEndElement();
            #endregion

            #region Created
            _writer.WriteStartElement("Created");
            _writer.WriteValue(DateTime.Now.ToUniversalTime().ToString());
            _writer.WriteEndElement();
            #endregion

            #region Version
            _writer.WriteStartElement("Version");
            _writer.WriteValue("12");
            _writer.WriteEndElement();
            #endregion

            _writer.WriteEndElement();
        }

        private void RenderStyles()
        {
            _writer.WriteStartElement("Styles");
            AddStyle("Default", "Normal", Aligment.Left, Aligment.Center, false, false, 14, false, 0, "", 0);
            AddStyle(HeaderStyleId,
                "", Aligment.Center, Aligment.Center, true, false, 14, true, 1, "Continuous", 0);
            AddStyle(HeaderVertiacalStyleId,
                "", Aligment.Center, Aligment.Center, true, false, 14, true, 1, "Continuous", 90);
            AddStyle(HeaderSheetStyleId,
                "", Aligment.Center, Aligment.Center, true, false, 18, false, 1, "Continuous", 0);
            
            AddStyle(DataStyleId,
                "", Aligment.Left, Aligment.Center, false, false, 14, true, 1, "Continuous", 0);
            AddStyle(DataVerticalStyleId,
                "", Aligment.Left, Aligment.Center, false, false, 14, true, 1, "Continuous", 90);

            AddStyle(DataGroupStyleId,
                "", Aligment.Left, Aligment.Center, true, false, 14, true, 1, "Continuous", 0);
            AddStyle(DataGroupVerticalStyleId,
                "", Aligment.Left, Aligment.Center, true, false, 14, true, 1, "Continuous", 90);

            AddStyle(FilterStyleId,
                "", Aligment.Left, Aligment.Center, false, false, 14, false, 1, "Continuous", 0);

            foreach (var row in _journalControl.Journal.Rows)
                AddStyles(row);

            var items = _journalControl.Journal.InnerHeader.ColumnHierarchy.GetAllItems();
            foreach (var columnHierarchy in items)
                columnHierarchy.StyleId = AddStyle(columnHierarchy.BColor, columnHierarchy.PColor, columnHierarchy.IsVerticalHeader ? HeaderVertiacalStyleId : HeaderStyleId, "Solid");

            _writer.WriteEndElement();
        }

        private void RenderWorksheet()
        {
            _writer.WriteStartElement("Worksheet");
            _writer.WriteAttributeString("ss:Name", "Data");

            #region Table
            _writer.WriteStartElement("Table");
            _writer.WriteAttributeString("ss:ExpandedColumnCount", GetCountColumns().ToString());
            _writer.WriteAttributeString("ss:ExpandedRowCount", GetCountRows().ToString());
            _writer.WriteAttributeString("x:FullColumns", "1");
            _writer.WriteAttributeString("x:FullRows", "1");
            _writer.WriteAttributeString("ss:StyleID", "Default");
            //_writer.WriteAttributeString("ss:DefaultColumnWidth", "100");
            //_writer.WriteAttributeString("ss:DefaultRowHeight", "20");
            _writer.WriteAttributeString("ss:DefaultAutoFitWidth", "1");
            _writer.WriteAttributeString("ss:DefaultAutoFitHeight", "1");

            RenderContentTable();
            _writer.WriteEndElement();
            #endregion

            RenderWorksheetOptions();
            _writer.WriteEndElement();
        }

        private void RenderContentTable()
        {
            var fullColSpan = _journalControl.Journal.InnerHeader.GetFullColSpan();
            _rowSpans = new int[fullColSpan];

            RenderColumnsSettings();
            RenderSheetHeader(fullColSpan);
            RenderFilter();
            RenderHeader();
            RenderData();
        }

        private void RenderWorksheetOptions()
        {
            var header = _journalControl.Journal.InnerHeader;
            var fixedRowsCount = _journalControl.FixedHeader ? _journalControl.FixedRowsCount + GetCountRowsBeforeData() : 0;
            var fixedColumnsCount = _journalControl.FixedColumnsCount;

            _writer.WriteStartElement("WorksheetOptions");
            _writer.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:office:excel");

            #region PageSetup
            _writer.WriteStartElement("PageSetup");

            _writer.WriteStartElement(HeaderStyleId);
            _writer.WriteAttributeString("x:Margin", "0.3");
            _writer.WriteEndElement();

            _writer.WriteStartElement("Footer");
            _writer.WriteAttributeString("x:Margin", "0.3");
            _writer.WriteEndElement();

            _writer.WriteStartElement("PageMargins");
            _writer.WriteAttributeString("x:Bottom", "0.75");
            _writer.WriteAttributeString("x:Left", "0.7");
            _writer.WriteAttributeString("x:Right", "0.7");
            _writer.WriteAttributeString("x:Top", "0.75");
            _writer.WriteEndElement();

            _writer.WriteEndElement();
            #endregion

            #region Unsynced
            _writer.WriteStartElement("Unsynced");
            _writer.WriteEndElement();
            #endregion

            #region Zoom
            _writer.WriteStartElement("Zoom");
            _writer.WriteValue(70);
            _writer.WriteEndElement();
            #endregion

            #region Print
            _writer.WriteStartElement("Print");

            _writer.WriteStartElement("PaperSizeIndex");
            _writer.WriteValue(9);
            _writer.WriteEndElement();

            _writer.WriteStartElement("HorizontalResolution");
            _writer.WriteValue(600);
            _writer.WriteEndElement();

            _writer.WriteStartElement("VerticalResolution");
            _writer.WriteValue(600);
            _writer.WriteEndElement();

            _writer.WriteEndElement();
            #endregion

            #region Selected
            _writer.WriteStartElement("Selected");
            _writer.WriteEndElement();
            #endregion

            #region FreezePanes
            _writer.WriteStartElement("FreezePanes");
            _writer.WriteEndElement();
            #endregion

            #region FrozenNoSplit
            _writer.WriteStartElement("FrozenNoSplit");
            _writer.WriteEndElement();
            #endregion

            #region FixedRows
            if (fixedRowsCount > 0)
            {
                _writer.WriteStartElement("SplitHorizontal");
                _writer.WriteValue(fixedRowsCount);
                _writer.WriteEndElement();

                _writer.WriteStartElement("TopRowBottomPane");
                _writer.WriteValue(fixedRowsCount);
                _writer.WriteEndElement();
            }
            #endregion

            #region FixedColumns
            if (fixedColumnsCount > 0)
            {
                _writer.WriteStartElement("SplitVertical");
                _writer.WriteValue(fixedColumnsCount);
                _writer.WriteEndElement();

                _writer.WriteStartElement("LeftColumnRightPane");
                _writer.WriteValue(fixedColumnsCount);
                _writer.WriteEndElement();
            }
            #endregion

            #region Protection
            _writer.WriteStartElement("ProtectObjects");
            _writer.WriteValue(true);
            _writer.WriteEndElement();

            _writer.WriteStartElement("ProtectScenarios");
            _writer.WriteValue(true);
            _writer.WriteEndElement();
            #endregion

            #region ActivePane

            int activePane = -1;
            if (fixedRowsCount > 0 && fixedColumnsCount > 0)
                activePane = 0;
            else if (fixedRowsCount > 0)
                activePane = 2;
            else if (fixedColumnsCount > 0)
                activePane = 1;

            if (activePane > -1)
            {
                _writer.WriteStartElement("ActivePane");
                _writer.WriteValue(activePane);
                _writer.WriteEndElement();
            }

            #endregion

            #region Panes
            _writer.WriteStartElement("Panes");
            _writer.WriteStartElement("Pane");

            _writer.WriteStartElement("Number");
            _writer.WriteValue(0);
            _writer.WriteEndElement();
            /*
            _writer.WriteStartElement("ActiveRow");
            _writer.WriteValue(1);
            _writer.WriteEndElement();

            _writer.WriteStartElement("ActiveCol");
            _writer.WriteValue(1);
            _writer.WriteEndElement();
            */

            _writer.WriteEndElement();
            _writer.WriteEndElement();
            #endregion

            _writer.WriteEndElement();
        }

        #endregion

        #region Render Header

        private void RenderColumnsSettings()
        {
            var header = _journalControl.Journal.InnerHeader;
            foreach (var item in header.GetDataColumns())
            {
                _writer.WriteStartElement("Column");
                if (item.Width > 0)
                    _writer.WriteAttributeString("ss:Width", (item.Width + 10).ToString());
                _writer.WriteEndElement();
            }
        }

        private void RenderHeader()
        {
            var header = _journalControl.Journal.InnerHeader;
            int maxRowSpan = header.GetMaxRowSpan();
            _addedRowSpans.Clear();
            int level = 0;
            do
            {
                _writer.WriteStartElement("Row");
                if (level < header.RowsProperties.Count)
                    RenderRowsProperties(header.RowsProperties[level]);
                MoveRowIndex();
                foreach (var columnHierarchy in header.ColumnHierarchy.Where(r => r.IsVisibleColumn(header.ColumnsDic)))
                    RenderColumnHierarchy(columnHierarchy, _writer, level, 0, maxRowSpan, header.ColumnsDic);

                _writer.WriteEndElement();
            } while (++level < maxRowSpan);
        }

        private void RenderSheetHeader(int colSpan)
        {
            _columnIndex = 0;
            _writer.WriteStartElement("Row");
            _writer.WriteAttributeString("ss:Height", "40");
            RenderCell(_writer, _journalControl.TableHeader, 1, colSpan, HeaderSheetStyleId);
            _writer.WriteEndElement();
            _addedRowSpans.Clear();
        }

        private void RenderCrossHeader(IEnumerable<CrossColumnDataSourceItem> listItems, int level, int rowSpan)
        {
            if (listItems.Count() == 0) return;

            var maxCountData = 1;
            var maxLevel = listItems.Max(r => r.Level);
            foreach (var column in listItems)
            {
                var currentLevel = level - maxLevel + column.Level;
                if (column.Level > currentLevel && currentLevel > 0)
                    RenderCrossHeader(column.ChildItems,
                                      currentLevel, rowSpan + maxLevel - column.Level);
                else if (column.Level == currentLevel && !column.HideInHeader)
                {
                    if (column.CountColumnValues > maxCountData)
                        maxCountData = column.CountColumnValues;

                    if (column.Level == 1)
                    {
                        var cellRowSpan = rowSpan + maxLevel - column.Level;
                        RenderCell(_writer, column.Header, cellRowSpan, column.ColSpan, HeaderStyleId);
                    }
                    else
                        RenderCell(_writer, column.Header, 1, column.ColSpan, HeaderStyleId);
                }
            }
            for (int i = 1; i < maxCountData; i++)
            {
                foreach (var column in listItems)
                {
                    var currentLevel = level - maxLevel + column.Level;
                    if (column.Level == currentLevel && !column.HideInHeader)
                    {
                        if (column.Level == 1)
                        {
                            var cellRowSpan = rowSpan + maxLevel - column.Level;
                            RenderCell(_writer, column.Header, cellRowSpan, column.ColSpan, HeaderStyleId);
                        }
                        else
                            RenderCell(_writer, column.Header, 1, column.ColSpan, HeaderStyleId);
                    }
                }
            }
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
            RenderCell(writer, columnHierarchy.Header, columnHierarchy.RowSpan, columnHierarchy.ColSpan, columnHierarchy.StyleId ?? (columnHierarchy.IsVerticalHeader ? HeaderVertiacalStyleId : HeaderStyleId));
        }

        #endregion

        #region Render Filter

        private void RenderFilter()
        {
            var filters = _journalControl.BaseFilter.GetFilterStrings();
            var fullColSpan = _journalControl.Journal.InnerHeader.GetFullColSpan();
            foreach (var filter in filters)
            {
                _columnIndex = 0;
                _writer.WriteStartElement("Row");
                RenderCell(_writer, filter, 1, fullColSpan, FilterStyleId);
                _writer.WriteEndElement();
                _addedRowSpans.Clear();
            }
            _columnIndex = 0;
            _writer.WriteStartElement("Row");
            RenderCell(_writer, "", 1, fullColSpan, FilterStyleId);
            _writer.WriteEndElement();
            _addedRowSpans.Clear();
        }

        #endregion

        #region Render Data

        private void RenderData()
        {
            foreach (var row in _journalControl.Journal.Rows)
                RenderData(row);
        }

        private void RenderData(BaseJournalRow<TRow> row)
        {
            _writer.WriteStartElement("Row");
            if (_journalControl.Journal.RowsPropertiesDic.ContainsKey(row.RowKey))
                RenderRowsProperties(_journalControl.Journal.RowsPropertiesDic[row.RowKey]);

            MoveRowIndex();
            var cellsPropertiesDic = _journalControl.Journal.CellsPropertiesDic;
            foreach (var cell in row.AllCells)
            {
                var bCell = cell.Value as BaseJournalCell;
                string styleId = null;
                if (bCell != null && cellsPropertiesDic.ContainsKey(bCell.GetCellKey()))//достаем стиль
                    styleId = cellsPropertiesDic[bCell.GetCellKey()].StyleId;

                if (styleId == null)//если стиль не был найден используем стандартный
                {
                    if (cell.Key != null && cell.Key.UsingInGroup)
                        styleId = bCell != null && bCell.IsVertical() ? DataGroupVerticalStyleId : DataGroupStyleId;
                    else if (bCell != null && bCell.RenderContext.TotalGroupValues != null)
                        styleId = DataGroupStyleId;
                    else if (bCell != null && bCell.IsVertical())
                        styleId = DataVerticalStyleId;
                    else
                        styleId = DataStyleId;
                }
                if (bCell != null)
                    RenderCell(_writer, bCell.GetValue(), bCell.RowSpan, bCell.ColSpan, styleId);
                else
                    RenderCell(_writer, "", 1, 1, styleId);
            }
            _writer.WriteEndElement();
        }

        #endregion

        #region help functions, properties

        private int GetCountColumns()
        {
            return _journalControl.Journal.InnerHeader.GetFullColSpan();
        }

        private int GetCountRows()
        {
            return _journalControl.Journal.Rows.Count //количество строк журнала RenderData
                   + GetCountRowsBeforeData();
        }

        private int GetCountRowsBeforeData()
        {
            return 1 //Название заголовка и пустая строка RenderSheetHeader
                   + _journalControl.Journal.InnerHeader.GetMaxRowSpan() //количество строк в заголовке RenderHeader
                   + _journalControl.BaseFilter.GetFiltersCount() + 1 //Строки фильтров и одна пустая RenderFilter
                ;
        }

        private void AddStyle(string styleId, string styleName, Aligment horizontal, Aligment vertical, bool isBold, bool isItalic, float size,
            bool fullBorder, int weightBorder, string borderLineStyle, int Rotate)
        {
            _writer.WriteStartElement("Style");
            _writer.WriteAttributeString("ss:ID", styleId);
            if (!string.IsNullOrEmpty(styleName))
                _writer.WriteAttributeString("ss:Name", styleName);

            #region Alignment
            _writer.WriteStartElement("Alignment");
            _writer.WriteAttributeString("ss:Horizontal", horizontal.ToString());
            _writer.WriteAttributeString("ss:Vertical", vertical.ToString());
            _writer.WriteAttributeString("ss:WrapText", "1");
            if (Rotate != 0)
                _writer.WriteAttributeString("ss:Rotate", Rotate.ToString());
            _writer.WriteEndElement();
            #endregion

            #region Borders

            _writer.WriteStartElement("Borders");
            if (fullBorder)
            {
                #region Bottom
                _writer.WriteStartElement("Border");
                _writer.WriteAttributeString("ss:Position", "Bottom");
                _writer.WriteAttributeString("ss:LineStyle", borderLineStyle);
                _writer.WriteAttributeString("ss:Weight", weightBorder.ToString());
                _writer.WriteEndElement();
                #endregion
                #region Left
                _writer.WriteStartElement("Border");
                _writer.WriteAttributeString("ss:Position", "Left");
                _writer.WriteAttributeString("ss:LineStyle", borderLineStyle);
                _writer.WriteAttributeString("ss:Weight", weightBorder.ToString());
                _writer.WriteEndElement();
                #endregion
                #region Right
                _writer.WriteStartElement("Border");
                _writer.WriteAttributeString("ss:Position", "Right");
                _writer.WriteAttributeString("ss:LineStyle", borderLineStyle);
                _writer.WriteAttributeString("ss:Weight", weightBorder.ToString());
                _writer.WriteEndElement();
                #endregion
                #region Top
                _writer.WriteStartElement("Border");
                _writer.WriteAttributeString("ss:Position", "Top");
                _writer.WriteAttributeString("ss:LineStyle", borderLineStyle);
                _writer.WriteAttributeString("ss:Weight", weightBorder.ToString());
                _writer.WriteEndElement();
                #endregion
            }
            _writer.WriteEndElement();
            #endregion

            #region Font
            _writer.WriteStartElement("Font");
            _writer.WriteAttributeString("ss:FontName", "Times New Roman");
            _writer.WriteAttributeString("x:CharSet", "204");
            _writer.WriteAttributeString("x:Family", "Roman");
            _writer.WriteAttributeString("ss:Size", size.ToString());
            _writer.WriteAttributeString("ss:Color", "#000000");
            if (isBold) _writer.WriteAttributeString("ss:Bold", "1");
            _writer.WriteEndElement();
            #endregion

            _writer.WriteEndElement();
        }

        private int _styleId = 1;
        private Dictionary<string, string> _styleIdForCells = new Dictionary<string, string>();

        private string AddStyle(string bColor, string pColor, string parentStyleId, string pattern)
        {
            if (string.IsNullOrEmpty(bColor) && string.IsNullOrEmpty(pColor)) return parentStyleId;
            var key = string.Format("{0};{1};{2};{3}", parentStyleId, bColor, pattern, pColor);
            if (_styleIdForCells.ContainsKey(key))
                return _styleIdForCells[key];

            var styleId = "cs" + _styleId++;
            _styleIdForCells[key] = styleId;

            _writer.WriteStartElement("Style");
            _writer.WriteAttributeString("ss:ID", styleId);
            if (parentStyleId != null)
                _writer.WriteAttributeString("ss:Parent", parentStyleId);

            if (!string.IsNullOrEmpty(pColor))
            {
                _writer.WriteStartElement("Font");
                _writer.WriteAttributeString("ss:Color", pColor);
                _writer.WriteEndElement();
            }

            _writer.WriteStartElement("Interior");
            if (!string.IsNullOrEmpty(bColor))
            {
                _writer.WriteAttributeString("ss:Color", bColor);
                _writer.WriteAttributeString("ss:Pattern", pattern);
            }
            _writer.WriteEndElement();//Interior

            _writer.WriteEndElement();//Style
            return styleId;
        }

        private void AddStyles(BaseJournalRow<TRow> row)
        {
            var rowProperties = _journalControl.Journal.RowsPropertiesDic.ContainsKey(row.RowKey) 
                ? _journalControl.Journal.RowsPropertiesDic[row.RowKey]
                : null;
            var cellsProperties = _journalControl.Journal.CellsProperties;
            var cellsPropertiesDic = _journalControl.Journal.CellsPropertiesDic;
            var hierachyColumns = _journalControl.Journal.InnerHeader.GetDataColumns();
            int i = 0;
            foreach (var cell in row.AllCells)
            {
                var hierarchyItem = hierachyColumns[i++];
                var bCell = cell.Value as BaseJournalCell;
                string styleId;
                if (cell.Key != null && cell.Key.UsingInGroup)
                    styleId = bCell != null && bCell.IsVertical() ? DataGroupVerticalStyleId : DataGroupStyleId;
                else if (bCell != null && bCell.RenderContext.TotalGroupValues != null)
                    styleId = DataGroupStyleId;
                else if (bCell != null && bCell.IsVertical())
                    styleId = DataVerticalStyleId;
                else
                    styleId = DataStyleId;

                string cellKey = null;
                if (bCell != null)
                    cellKey = bCell.GetCellKey();
                else
                {
                    //todo: сделать эту часть
                }
                if (cellKey != null) //если есть у ячеки ключ
                {
                    CellProperties cellProps = null;
                    if (cellsPropertiesDic.ContainsKey(cellKey))
                        cellProps = cellsPropertiesDic[cellKey];
                    else if (rowProperties != null || hierarchyItem.BColor != null || hierarchyItem.PColor != null) //созадем свйоства для ячейки, если имеются настройки для строки или колонки
                    {
                        cellsPropertiesDic[cellKey] = cellProps = new CellProperties {Key = cellKey};
                        cellsProperties.Add(cellProps);
                    }
                    if (cellProps != null) //если стиль был найден или создан, то проставляем ключ стиля
                    {
                        if (hierarchyItem.BColor != null && cellProps.BColor == null)
                            cellProps.BColor = hierarchyItem.BColor;
                        if (hierarchyItem.PColor != null && cellProps.PColor == null)
                            cellProps.PColor = hierarchyItem.PColor;
                        if (rowProperties != null) //если есть настройки у строки то передаем их ячейки
                        {
                            if (cellProps.BColor == null)
                                cellProps.BColor = rowProperties.BColor;
                            if (cellProps.PColor == null)
                                cellProps.PColor = rowProperties.PColor;
                        }
                        cellProps.StyleId = AddStyle(cellProps.BColor, cellProps.PColor, styleId, "Solid");
                    }
                }
            }
        }

        private void MoveRowIndex()
        {
            _columnIndex = 0;
            _columnIndexMovedByRowSpan = false;
            foreach (var rowNextSpan in _addedRowSpans)
                _rowSpans[rowNextSpan.Key] = rowNextSpan.Value;
            _addedRowSpans.Clear();
            for (int i = 0; i < _rowSpans.Length; i++)
                _rowSpans[i]--;
            MoveColumnIndex(0);
        }

        private void MoveColumnIndex(int colSpan)
        {
            _columnIndexMovedByRowSpan = false;
            _columnIndex += colSpan;
            while (_columnIndex < _rowSpans.Length && _rowSpans[_columnIndex] > 0)
            {
                _columnIndexMovedByRowSpan = true;
                _columnIndex++;
            }
        }

        private void AddRowSpans(int rowSpan, int colSpan)
        {
            while (colSpan > 0)
                _addedRowSpans[_columnIndex + --colSpan] = rowSpan;
        }

        private void RenderCell(XmlTextWriter writer, string stringData, int rowSpan, int colSpan, string styleId)
        {
            writer.WriteStartElement("Cell");
            if (_columnIndexMovedByRowSpan)
                writer.WriteAttributeString("ss:Index", (_columnIndex + 1).ToString());
            if (rowSpan > 1)
                writer.WriteAttributeString("ss:MergeDown", (rowSpan - 1).ToString());
            if (colSpan > 1)
                writer.WriteAttributeString("ss:MergeAcross", (colSpan - 1).ToString());
            writer.WriteAttributeString("ss:StyleID", styleId);

            #region Data
            writer.WriteStartElement("Data");
            writer.WriteAttributeString("ss:Type", "String");
            if (!string.IsNullOrEmpty(stringData))
            {
                if (stringData.StartsWith(" ") || stringData.EndsWith(" "))
                    writer.WriteAttributeString("xml:space", "preserve");
                writer.WriteString(stringData);
            }
            writer.WriteEndElement();
            #endregion

            writer.WriteEndElement();
            AddRowSpans(rowSpan, colSpan);
            MoveColumnIndex(colSpan);
        }

        private void RenderRowsProperties(RowProperties rowProperties)
        {
            if (rowProperties.Height != null)
                _writer.WriteAttributeString("ss:Height", (rowProperties.Height.Value + 12).ToString());
        }

        #endregion

        private enum Aligment
        {
            Left,
            Right,
            Top,
            Bottom,
            Center,
        }
    }
}
