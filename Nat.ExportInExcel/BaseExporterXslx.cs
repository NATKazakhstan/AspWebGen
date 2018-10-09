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
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DocumentFormat.OpenXml.Packaging;

    using Nat.Tools.Classes;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.GenerationClasses.HierarchyFields;
    using Nat.Web.Tools;
    using Nat.Web.Tools.Export.Formatting;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1215:InstanceReadonlyElementsMustAppearBeforeInstanceNonReadonlyElements", Justification = "Reviewed. Suppression is OK here.")]
    public abstract class BaseExporterXslx
    {
        protected const string DefaultStyleId = "Default";
        protected const string HeaderStyleId = "header";
        protected const string HeaderVertiacalStyleId = "vHeader";
        protected const string HeaderSheetStyleId = "sheetHeader";
        protected const string DataStyleId = "data";
        protected const string DataStyleCenterId = "dataCenter";
        protected const string DataVerticalStyleId = "dataV";
        protected const string DataGroupStyleId = "gData";
        protected const string DataGroupStyleCenterId = "gDataCenter";
        protected const string DataGroupVerticalStyleId = "gDataV";
        protected const string FilterStyleId = "filter";

        // ReSharper disable InconsistentNaming
        private int[] _rowSpans;
        private string[] _rowStyles;
        private readonly Dictionary<string, int> _sharedValues = new Dictionary<string, int>();
        private int _sharedValuesCount;
        private readonly List<KeyValuePair<Point, Point>> _mergedCells = new List<KeyValuePair<Point, Point>>();
        protected readonly Dictionary<int, int> _addedRowSpans = new Dictionary<int, int>();
        private readonly Dictionary<string, string> _hyperlinkKeys = new Dictionary<string, string>();
        private readonly Dictionary<string, Pair<string, string>> _hyperlinkRanges = new Dictionary<string, Pair<string, string>>();
        protected int _columnIndex;
        protected int _rowIndex;
        protected readonly Dictionary<string, StringBuilder> _columnsAddresses = new Dictionary<string, StringBuilder>();

        private static Regex _convertSharedStringRegex =
            new Regex(
                "(?<Tab>\t)|(?<BR><br\\s*/>)|(?<I><i>)|(?<B><b>)|(?<IClose></i>)|(?<BClose></b>)|(?<Text>(.|\\n)*?)((?<TabEnd>\t)|(?<BREnd><br\\s*/>)|(?<IEnd><i>)|(?<BEnd><b>)|(?<ICloseEnd></i>)|(?<BCloseEnd></b>)|(?<End>$))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private Stream _tempStream;
        protected XmlTextWriter _writer;
        private StreamWriter _streamWriter;

        private SpreadsheetDocument _doc;
        protected WorksheetPart _worksheetPart;
        //// ReSharper restore InconsistentNaming

        public ILogMonitor LogMonitor { get; set; }


        protected abstract int GetFixedColumnsCount();
        
        protected abstract int GetFixedRowsCount();

        protected abstract int GetCountRows();

        protected abstract int GetCountColumns();

        protected abstract void RenderColumnsSettings();

        protected abstract string GetHeader();

        protected abstract IEnumerable<string> GetFilterStrings();

        public Stream GetExcel()
        {
            var stream = new MemoryStream();
            _doc = SpreadsheetDocument.Create(stream, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            using (_tempStream = new MemoryStream())
            {
                WriteDocProps();

                WriteWorkBook();
                RenderStyles();
                RenderWorksheet();
                RenderSharedStrings();
                using (var themeStream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("Nat.ExportInExcel.theme1.xml"))
                {
                    var themePart = _doc.WorkbookPart.AddNewPart<ThemePart>("rId2");
                    themePart.FeedData(themeStream);
                }

                _doc.Close();
                stream.Position = 0;
                return stream;
            }
        }

        #region BaseOfDocument

        private void RenderSharedStrings()
        {
            StartWrtie(_doc.WorkbookPart.AddNewPart<SharedStringTablePart>("rId4"));
            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");

            _writer.WriteStartElementExt("sst",
                                         "xmlns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main",
                                         "count", _sharedValuesCount.ToString(),
                                         "uniqueCount", _sharedValues.Count.ToString());
            foreach (var value in _sharedValues.OrderBy(r => r.Value).Select(r => r.Key))
                WriteSharedValue(value);

            _writer.WriteEndElement();
            PutInFile();
        }

        private void WriteSharedValue(string value)
        {
            var match = _convertSharedStringRegex.Match(value);
            if (!match.Success || match.Groups["Text"].Length == value.Length)
            {
                _writer.WriteDoubleElementStringExt("si", string.Empty, string.Empty, "t", value, "xml:space", "preserve");
                return;
            }

            _writer.WriteStartElement("si");

            var bold = 0;
            var italic = 0;
            while (match.Success)
            {
                if (match.Groups["I"].Success) italic++;
                if (match.Groups["B"].Success) bold++;
                if (match.Groups["IClose"].Success) italic--;
                if (match.Groups["BClose"].Success) bold--;

                if (match.Groups["Tab"].Success)
                    _writer.WriteDoubleElementStringExt("r", string.Empty, string.Empty, "t", "     ", "xml:space", "preserve");

                if (match.Groups["BR"].Success)
                    _writer.WriteDoubleElementStringExt("r", string.Empty, string.Empty, "t", "\r\n", "xml:space", "preserve");

                if (match.Groups["Text"].Success && !string.IsNullOrEmpty(match.Groups["Text"].Value))
                {
                    _writer.WriteStartElement("r");
                    
                    _writer.WriteStartElement("rPr");
                    if (bold > 0) _writer.WriteEmptyElementExt("b");
                    if (italic > 0) _writer.WriteEmptyElementExt("i");
                    _writer.WriteEndElement(); //rPr

                    _writer.WriteStartElementExt("t", "xml:space", "preserve");
                    _writer.WriteString(match.Groups["Text"].Value);
                    _writer.WriteEndElement(); //t

                    _writer.WriteEndElement(); //r
                }

                if (match.Groups["TabEnd"].Success)
                    _writer.WriteDoubleElementStringExt("r", string.Empty, string.Empty, "t", "     ", "xml:space", "preserve");

                if (match.Groups["BREnd"].Success)
                    _writer.WriteDoubleElementStringExt("r", string.Empty, string.Empty, "t", "\r\n", "xml:space", "preserve");

                if (match.Groups["IEnd"].Success) italic++;
                if (match.Groups["BEnd"].Success) bold++;
                if (match.Groups["ICloseEnd"].Success) italic--;
                if (match.Groups["BCloseEnd"].Success) bold--;

                match = match.NextMatch();
            }

            _writer.WriteEndElement(); //si
        }

        private void StartWrtie(OpenXmlPart part)
        {
            _tempStream = part.GetStream(FileMode.Create, FileAccess.Write);
            _streamWriter = new StreamWriter(_tempStream);
            _writer = new XmlTextWriter(_streamWriter) { Formatting = Formatting.Indented };
            _writer.Indentation = 0;
            _writer.Formatting = Formatting.None;
        }

        private void WriteDocProps()
        {
            StartWrtie(_doc.AddExtendedFilePropertiesPart());

            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");
            _writer.WriteStartElement("Properties");
            _writer.WriteAttributeString("xmlns", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties");
            _writer.WriteAttributeString("xmlns:vt", "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes");
            _writer.WriteElementString("DocSecurity", "1");
            _writer.WriteElementString("ScaleCrop", "false");

            #region HeadingPairs
            _writer.WriteStartElement("HeadingPairs");
            _writer.WriteStartElement("vt:vector");
            _writer.WriteAttributeString("size", "2");
            _writer.WriteAttributeString("baseType", "variant");

            _writer.WriteStartElement("vt:variant");
            _writer.WriteElementString("vt:lpstr", "Worksheets");
            _writer.WriteEndElement(); // vt:variant

            _writer.WriteStartElement("vt:variant");
            _writer.WriteElementString("vt:i4", "1");
            _writer.WriteEndElement(); // vt:variant

            _writer.WriteEndElement(); // vt:vector
            _writer.WriteEndElement(); // HeadingPairs
            #endregion

            #region TitlesOfParts
            _writer.WriteStartElement("TitlesOfParts");
            _writer.WriteStartElement("vt:vector");
            _writer.WriteAttributeString("size", "1");
            _writer.WriteAttributeString("baseType", "lpstr");

            _writer.WriteElementString("vt:lpstr", "Data");

            _writer.WriteEndElement(); // vt:vector
            _writer.WriteEndElement(); // TitlesOfParts
            #endregion

            _writer.WriteElementString("LinksUpToDate", "false");
            _writer.WriteElementString("SharedDoc", "false");
            _writer.WriteElementString("HyperlinksChanged", "false");
            _writer.WriteElementString("AppVersion", "12.0000");
            _writer.WriteEndElement(); // Properties
            PutInFile();

            StartWrtie(_doc.AddCoreFilePropertiesPart());

            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");
            _writer.WriteStartElement("cp:coreProperties");
            _writer.WriteAttributeString("xmlns:cp", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties");
            _writer.WriteAttributeString("xmlns:dc", "http://purl.org/dc/elements/1.1/");
            _writer.WriteAttributeString("xmlns:dcterms", "http://purl.org/dc/terms/");
            _writer.WriteAttributeString("xmlns:dcmitype", "http://purl.org/dc/dcmitype/");
            _writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            _writer.WriteElementString("dc:creator", Thread.CurrentPrincipal.Identity.Name);
            _writer.WriteElementString("cp:lastModifiedBy", Thread.CurrentPrincipal.Identity.Name);
            var date = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            _writer.WriteElementStringExt("dcterms:created", date, "xsi:type", "dcterms:W3CDTF");
            _writer.WriteElementStringExt("dcterms:modified", date, "xsi:type", "dcterms:W3CDTF");

            _writer.WriteEndElement(); // cp:coreProperties
            PutInFile();
        }

        private void PutInFile()
        {
            _writer.Flush();
            _streamWriter.Flush();
            _streamWriter.Close();
            _streamWriter.Dispose();
            _tempStream.Close();
            _tempStream.Dispose();
        }

        private void WriteWorkBook()
        {
            // AddNewPart<WorkbookPart>("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", "rId1")
            StartWrtie(_doc.AddWorkbookPart());
            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");
            _writer.WriteStartElement("workbook");
            _writer.WriteAttributeString("xmlns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            _writer.WriteAttributeString("xmlns:r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            _writer.WriteElementStringExt("fileVersion", "",
                                          "appName", "xl",
                                          "lastEdited", "4",
                                          "lowestEdited", "4",
                                          "rupBuild", "4506");
            _writer.WriteElementStringExt("workbookPr", "", "defaultThemeVersion", "124226");
            _writer.WriteStartElement("bookViews");
            _writer.WriteElementStringExt("workbookView", "",
                                          "xWindow", "120",
                                          "yWindow", "135",
                                          "windowWidth", "10005",
                                          "windowHeight", "10005");
            _writer.WriteEndElement(); // bookViews

            _writer.WriteStartElement("sheets");
            _writer.WriteElementStringExt("sheet", "",
                                          "name", "Data",
                                          "sheetId", "1",
                                          "r:id", "rId1");
            _writer.WriteEndElement(); // sheets
            _writer.WriteElementStringExt("calcPr", "", "calcId", "125725", "refMode", "R1C1");

            _writer.WriteEndElement(); // workbook
            PutInFile();
        }

        #endregion


        #region Render Sheet

        private readonly Dictionary<Style, int> _styles = new Dictionary<Style, int>();
        private readonly Dictionary<StyleFont, int> _fonts = new Dictionary<StyleFont, int>();
        private readonly Dictionary<StyleBorder, int> _borders = new Dictionary<StyleBorder, int>();
        private readonly Dictionary<StyleFill, int> _fills = new Dictionary<StyleFill, int>();
        private readonly Dictionary<string, Style> _baseStyles = new Dictionary<string, Style>();

        private void RenderStyles()
        {
            #region Create Styles

            _fonts.Add(new StyleFont { Size = 14 }, 0);
            _fonts.Add(new StyleFont { Size = 14, Bold = true }, 1);
            _fonts.Add(new StyleFont { Size = 18, Bold = true }, 2);
            _borders.Add(new StyleBorder { Style = string.Empty, }, 0);
            _borders.Add(new StyleBorder { Style = "thin", }, 1);
            _fills.Add(new StyleFill { Pattern = "none" }, 0);
            _fills.Add(new StyleFill { Pattern = "gray125" }, 1);
            _baseStyles[DefaultStyleId] =
                new Style
                {
                    FontId = 0,
                    HorizontalAlignment = Aligment.Left,
                    VerticalAlignment = Aligment.Center,
                    WrapText = true,
                };
            _baseStyles[HeaderStyleId] =
                new Style
                {
                    FontId = 1,
                    BorderId = 1,
                    HorizontalAlignment = Aligment.Center,
                    VerticalAlignment = Aligment.Center,
                    WrapText = true,
                };
            _baseStyles[HeaderVertiacalStyleId] = _baseStyles[HeaderStyleId].Clone(90, Aligment.Left);

            _baseStyles[HeaderSheetStyleId] =
                new Style
                {
                    FontId = 2,
                    HorizontalAlignment = Aligment.Center,
                    VerticalAlignment = Aligment.Center,
                    WrapText = true,
                };

            _baseStyles[DataStyleId] =
                new Style
                {
                    FontId = 0,
                    BorderId = 1,
                    HorizontalAlignment = Aligment.Left,
                    VerticalAlignment = Aligment.Center,
                    WrapText = true,
                };
            _baseStyles[DataVerticalStyleId] = _baseStyles[DataStyleId].Clone(90, Aligment.Center);
            _baseStyles[DataStyleCenterId] = _baseStyles[DataStyleId].Clone(Aligment.Center);

            _baseStyles[DataGroupStyleId] =
                new Style
                {
                    FontId = 1,
                    BorderId = 1,
                    HorizontalAlignment = Aligment.Left,
                    VerticalAlignment = Aligment.Center,
                    WrapText = true,
                };
            _baseStyles[DataGroupStyleCenterId] = _baseStyles[DataGroupStyleId].Clone(Aligment.Center);
            _baseStyles[DataGroupVerticalStyleId] = _baseStyles[DataGroupStyleId].Clone(90, Aligment.Center);

            _baseStyles[FilterStyleId] =
                new Style
                {
                    FontId = 1,
                    HorizontalAlignment = Aligment.Left,
                    VerticalAlignment = Aligment.Center,
                };

            foreach (var style in _baseStyles.Where(style => !_styles.ContainsKey(style.Value)))
            {
                _styles[style.Value] = _styles.Count;
            }

            AddRowsStyles();
            if (RenderFooterTable != null) AddStyles(RenderFooterTable);
            if (RenderFirstHeaderTable != null) AddStyles(RenderFirstHeaderTable);

            #endregion

            #region Write Styles

            StartWrtie(_doc.WorkbookPart.AddNewPart<WorkbookStylesPart>("rId3"));
            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");
            _writer.WriteStartElement("styleSheet");
            _writer.WriteAttributeString("xmlns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

            #region Fonts
            _writer.WriteStartElement("fonts");
            _writer.WriteAttributeString("count", _fonts.Count.ToString());
            foreach (var font in _fonts.OrderBy(r => r.Value).Select(r => r.Key))
            {
                _writer.WriteStartElement("font");
                if (font.Bold) _writer.WriteEmptyElementExt("b");
                if (font.Italic) _writer.WriteEmptyElementExt("i");
                _writer.WriteElementStringExt("sz", string.Empty, "val", font.Size.ToString());
                _writer.WriteElementStringExt("color", string.Empty, "rgb", GetColor(font.Color ?? Color.Black));
                _writer.WriteElementStringExt("name", string.Empty, "val", "Times New Roman");
                _writer.WriteElementStringExt("family", string.Empty, "val", "1");
                _writer.WriteElementStringExt("charset", string.Empty, "val", "204");
                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            #endregion

            #region fills
            _writer.WriteStartElement("fills");
            _writer.WriteAttributeString("count", _fills.Count.ToString());
            foreach (var fill in _fills.OrderBy(r => r.Value).Select(r => r.Key))
            {
                _writer.WriteStartElement("fill");
                if (string.IsNullOrEmpty(fill.Pattern) || fill.Pattern.ToLower() == "none")
                    _writer.WriteElementStringExt("patternFill", string.Empty, "patternType", "none");
                else
                {
                    _writer.WriteStartElement("patternFill");
                    _writer.WriteAttributeString("patternType", fill.Pattern.ToLower());
                    _writer.WriteElementStringExt("fgColor", string.Empty, "rgb", GetColor(fill.Color));
                    _writer.WriteElementStringExt("bgColor", string.Empty, "indexed", "64");
                    _writer.WriteEndElement();
                }

                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            #endregion

            #region borders
            _writer.WriteStartElementExt("borders", "count", _borders.Count.ToString());
            foreach (var border in _borders.OrderBy(r => r.Value).Select(r => r.Key))
            {
                _writer.WriteStartElement("border");
                if (border.Style == string.Empty)
                {
                    _writer.WriteEmptyElementExt("left");
                    _writer.WriteEmptyElementExt("right");
                    _writer.WriteEmptyElementExt("top");
                    _writer.WriteEmptyElementExt("bottom");
                }
                else
                {
                    var color = GetColor(border.Color ?? Color.Black);
                    _writer.WriteDoubleElementStringExt("left", "style", border.Style, "color", string.Empty, "rgb", color);
                    _writer.WriteDoubleElementStringExt("right", "style", border.Style, "color", string.Empty, "rgb", color);
                    _writer.WriteDoubleElementStringExt("top", "style", border.Style, "color", string.Empty, "rgb", color);
                    _writer.WriteDoubleElementStringExt("bottom", "style", border.Style, "color", string.Empty, "rgb", color);
                }

                _writer.WriteEndElement();
            }

            _writer.WriteEndElement();
            #endregion

            #region cellStyleXfs
            _writer.WriteStartElementExt("cellStyleXfs", "count", "1");
            _writer.WriteElementStringExt("xf", string.Empty, "numFmtId", "0", "fontId", "0", "fillId", "0", "borderId", "0");
            _writer.WriteEndElement();
            #endregion

            #region cellXfs
            _writer.WriteStartElementExt("cellXfs", "count", _styles.Count.ToString());

            foreach (var style in _styles.OrderBy(r => r.Value).Select(r => r.Key))
            {
                // numFmtId="14" - формат даты
                _writer.WriteStartElementExt("xf", "numFmtId", "0",
                                             "fontId", (style.FontId ?? 0).ToString(),
                                             "fillId", (style.FillId ?? 0).ToString(),
                                             "borderId", (style.BorderId ?? 0).ToString());
                if (style.FontId != null) _writer.WriteAttributeString("applyFont", "1");
                if (style.FillId != null) _writer.WriteAttributeString("applyFill", "1");
                if (style.BorderId != null) _writer.WriteAttributeString("applyBorder", "1");
                _writer.WriteAttributeString("applyAlignment", "1");
                _writer.WriteElementStringExt("alignment", string.Empty,
                                           "horizontal", style.HorizontalAlignment.ToString().ToLower(),
                                           "vertical", style.VerticalAlignment.ToString().ToLower(),
                                           "wrapText", style.WrapText ? "1" : "0",
                                           "textRotation", style.TextRotation.ToString());
                _writer.WriteEndElement(); // xf
            }

            _writer.WriteEndElement();
            #endregion

            #region cellStyles
            _writer.WriteStartElementExt("cellStyles", "count", "1");
            _writer.WriteElementStringExt("cellStyle", string.Empty, "name", "Normal", "xfId", "0", "builtinId", "0");
            _writer.WriteEndElement();
            #endregion

            #region dxfs
            _writer.WriteStartElementExt("dxfs", "count", "0");
            _writer.WriteEndElement();
            #endregion

            #region tableStyles
            _writer.WriteElementStringExt("tableStyles", string.Empty,
                                          "count", "0",
                                          "defaultTableStyle", "TableStyleMedium9",
                                          "defaultPivotStyle", "PivotStyleLight16");
            #endregion

            _writer.WriteEndElement(); // styleSheet
            PutInFile();

            #endregion
        }

        protected abstract void AddRowsStyles();

        private string GetColor(Color color)
        {
            return Color.FromArgb(color.ToArgb()).Name.ToUpper();
        }

        private void RenderWorksheet()
        {
            _worksheetPart = _doc.WorkbookPart.AddNewPart<WorksheetPart>("rId1");
            StartWrtie(_worksheetPart);
            _writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");
            _writer.WriteWhitespace("\r\n");

            _writer.WriteStartElementExt("worksheet",
                                         "xmlns", "http://schemas.openxmlformats.org/spreadsheetml/2006/main",
                                         "xmlns:r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

            _writer.WriteStartElement("sheetPr");
            _writer.WriteElementStringExt("pageSetUpPr", string.Empty, "fitToPage", "1");
            _writer.WriteEndElement(); // sheetPr
            
            _writer.WriteElementStringExt("dimension", string.Empty, "ref", "A1:" + GetLaterByInt(GetCountColumns() - 1) + GetCountRows());

            #region sheetViews

            _writer.WriteStartElement("sheetViews");
            _writer.WriteStartElementExt("sheetView", "tabSelected", "1", "zoomScale", "70", "workbookViewId", "0");
            var fixedRowsCount = GetFixedRowsCount();
            var fixedColumnsCount = GetFixedColumnsCount();
            if (fixedRowsCount > 0 || fixedColumnsCount > 0)
            {
                _writer.WriteElementStringExt("pane", string.Empty,
                                              "xSplit", fixedColumnsCount.ToString(),
                                              "ySplit", fixedRowsCount.ToString(),
                                              "state", "frozen",
                                              "topLeftCell", GetLaterByInt(fixedColumnsCount) + (fixedRowsCount + 1));
            }

            _writer.WriteElementStringExt("selection", string.Empty, "activeCell", "A1", "sqref", "A1");
            _writer.WriteEndElement(); // sheetView
            _writer.WriteEndElement(); // sheetViews
            _writer.WriteElementStringExt("sheetFormatPr", string.Empty, "defaultRowHeight", "30");
            #endregion

            RenderContentTable();

            _writer.WriteElementStringExt("pageSetup", string.Empty, "fitToHeight", "0", "orientation", "landscape");
            _writer.WriteEndElement(); // worksheet
            
            PutInFile();
        }

        private void RenderContentTable()
        {
            var fullColSpan = GetCountColumns();
            _rowSpans = new int[fullColSpan];
            _rowStyles = new string[fullColSpan];

            RenderColumnsSettings();

            _writer.WriteStartElement("sheetData");
            RenderFirstHeader();
            RenderSheetHeader(fullColSpan);
            RenderFilter();
            RenderHeader();
            RenderData();
            RenderFooter();
            _writer.WriteEndElement();

            if (_mergedCells.Count > 0)
            {
                _writer.WriteStartElementExt("mergeCells", "count", _mergedCells.Count.ToString());
                var mergedCellsData = _mergedCells
                    .Select(r => GetLaterByInt(r.Key.X) + r.Key.Y + ":" + GetLaterByInt(r.Value.X) + r.Value.Y)
                    .OrderBy(r => r);
                foreach (var mergedCell in mergedCellsData)
                    _writer.WriteElementStringExt("mergeCell", string.Empty, "ref", mergedCell);
                _writer.WriteEndElement();
            }

            var formating = GetConditionalFormatting();
            if (formating != null && formating.Count > 0)
                RenderConditionalFormatting(formating);

            #region hyperlinks
            if (_hyperlinkRanges.Count > 0)
            {
                _writer.WriteStartElement("hyperlinks");
                foreach (var hyperlinkRange in _hyperlinkRanges)
                {
                    _writer.WriteElementStringExt("hyperlink", string.Empty,
                                                  "ref", hyperlinkRange.Key,
                                                  "r:id", hyperlinkRange.Value.First,
                                                  "display", hyperlinkRange.Value.Second);
                }

                _writer.WriteEndElement();
            }
            #endregion

            _writer.WriteElementStringExt("pageMargins", string.Empty,
                                          "left", "0.7", "right", "0.7",
                                          "top", "0.75", "bottom", "0.75",
                                          "header", "0.3", "footer", "0.3",
                                          string.Empty, string.Empty, string.Empty, string.Empty,
                                          string.Empty, string.Empty, string.Empty, string.Empty);
        }

        protected abstract List<ConditionalFormatting> GetConditionalFormatting();

        protected abstract void RenderData();

        protected abstract Table RenderFooterTable { get; }
        protected abstract Table RenderFirstHeaderTable { get; }

        protected abstract void RenderHeader();

        #endregion

        #region

        private void RenderSheetHeader(int colSpan)
        {
            WriteStartRow(28);
            RenderCell(_writer, GetHeader(), 1, colSpan, HeaderSheetStyleId, ColumnType.Other, string.Empty);
            _writer.WriteEndElement();
            _addedRowSpans.Clear();
        }
        
        #endregion

        #region RenderFooter RenderFirstHeader
        
        protected void RenderFooter()
        {
            if (RenderFooterTable == null)
                return;

            RenderTable(RenderFooterTable);
        }

        protected void RenderFirstHeader()
        {
            if (RenderFirstHeaderTable == null)
                return;

            RenderTable(RenderFirstHeaderTable);
        }

        protected void RenderTable(Table table)
        {
            foreach (TableRow row in table.Rows)
            {
                WriteStartRow(row.Height.IsEmpty || row.Height.Type != UnitType.Pixel ? null : (int?)row.Height.Value);
                MoveRowIndex();

                foreach (TableCell cell in row.Cells)
                {
                    RenderCell(
                        _writer,
                        cell.Text,
                        cell.RowSpan == 0 ? 1 : cell.RowSpan,
                        cell.ColumnSpan == 0 ? 1 : cell.ColumnSpan,
                        cell.Attributes["StyleID"],
                        ColumnType.Other,
                        string.Empty);
                }

                _writer.WriteEndElement();
            }
            _addedRowSpans.Clear();
        }

        #endregion

        #region Render Filter

        private void RenderFilter()
        {
            var filters = GetFilterStrings();
            var fullColSpan = GetCountColumns();
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    WriteStartRow(null);
                    RenderCell(_writer, filter, 1, fullColSpan, FilterStyleId, ColumnType.Other, string.Empty);
                    _writer.WriteEndElement();
                    _addedRowSpans.Clear();
                }
            }

            WriteStartRow(null);
            RenderCell(_writer, string.Empty, 1, fullColSpan, FilterStyleId, ColumnType.Other, string.Empty);
            _writer.WriteEndElement();
            _addedRowSpans.Clear();
        }

        #endregion

        #region Render ConditionalFormatting

        private void RenderConditionalFormatting(IEnumerable<ConditionalFormatting> conditionalFormattings)
        {
            foreach (var conditionalFormatting in conditionalFormattings)
                RenderConditionalFormatting(conditionalFormatting);
        }

        private void RenderConditionalFormatting(ConditionalFormatting conditionalFormattings)
        {
            var columnsAddresses = GetColumnsAddresses(conditionalFormattings.Columns);
            if (string.IsNullOrEmpty(columnsAddresses))
                return;

            _writer.WriteStartElementExt("conditionalFormatting", "sqref", columnsAddresses);
            RenderConditionalFormatting(conditionalFormattings.Rule);
            _writer.WriteEndElement();
        }

        private void RenderConditionalFormatting(ConditionalFormattingRule rule)
        {
            string type;
            switch (rule.Type)
            {
                case ConditionalFormattingRuleType.ColorScale:
                    type = "colorScale";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _writer.WriteStartElementExt("cfRule", "type", type, "priority", rule.Priority.ToString());

            switch (rule.Type)
            {
                case ConditionalFormattingRuleType.ColorScale:
                    RenderConditionalFormattingColorScale(rule);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _writer.WriteEndElement();
        }

        private void RenderConditionalFormattingColorScale(ConditionalFormattingRule rule)
        {
            var cs = rule.Items.OfType<ConditionalFormattingColorScale>().FirstOrDefault();
            if (cs == null)
                throw new NullReferenceException("ConditionalFormattingColorScale not found");

            _writer.WriteStartElement("colorScale");

            WriteConditionalFormattingVO(cs.Min);
            WriteConditionalFormattingVO(cs.Avg);
            WriteConditionalFormattingVO(cs.Max);
            WriteConditionalFormattingColorRGB(cs.ColorMin);
            WriteConditionalFormattingColorRGB(cs.ColorAvg);
            WriteConditionalFormattingColorRGB(cs.ColorMax);

            _writer.WriteEndElement();
        }

        private void WriteConditionalFormattingColorRGB(Color color)
        {
            _writer.WriteElementStringExt("color", string.Empty, "rgb", GetColor(color));
        }

        private void WriteConditionalFormattingVO(ConditionalFormattingVO cfvo)
        {
            string type;
            switch (cfvo.Type)
            {
                case ConditionalFormattingVOType.Num:
                case ConditionalFormattingVOType.Min:
                case ConditionalFormattingVOType.Max:
                case ConditionalFormattingVOType.Percentile:
                case ConditionalFormattingVOType.Percent:
                    type = cfvo.Type.ToString().ToLower();
                    break;
                case ConditionalFormattingVOType.AutoMin:
                    type = "autoMin";
                    break;
                case ConditionalFormattingVOType.AutoMax:
                    type = "autoMax";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrEmpty(cfvo.Value))
                _writer.WriteElementStringExt("cfvo", string.Empty, "type", type);
            else
                _writer.WriteElementStringExt("cfvo", string.Empty, "type", type, "val", cfvo.Value.Replace(",", "."));
        }

        private string GetColumnsAddresses(IEnumerable<string> columns)
        {
            var address = columns
                .Where(c => _columnsAddresses.ContainsKey(c))
                .Select(c => _columnsAddresses[c])
                .Aggregate(
                new StringBuilder(),
                (sb1, sb2) =>
                {
                    if (sb2.Length == 0)
                        return sb1;
                    if (sb1.Length > 0)
                        sb1.Append(" ");
                    sb1.Append(sb2);
                    return sb1;
                });
            return address.ToString();
        }

        #endregion

        #region help functions, properties

        protected void AddStyles(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                foreach (TableCell tableCell in tableRow.Cells)
                {
                    tableCell.Attributes["StyleID"] = AddStyle(
                        null,
                        null,
                        string.IsNullOrEmpty(tableCell.Style[HtmlTextWriterStyle.FontSize])
                            ? (int?)null
                            : Convert.ToInt32(tableCell.Style[HtmlTextWriterStyle.FontSize]),
                        "bold".Equals(tableCell.Style[HtmlTextWriterStyle.FontWeight], StringComparison.OrdinalIgnoreCase),
                        "italic".Equals(tableCell.Style[HtmlTextWriterStyle.FontStyle], StringComparison.OrdinalIgnoreCase),
                        string.IsNullOrEmpty(tableCell.Style[HtmlTextWriterStyle.TextAlign])
                            ? (Aligment?)null
                            : (Aligment)Enum.Parse(typeof(Aligment), tableCell.Style[HtmlTextWriterStyle.TextAlign]),
                        string.IsNullOrEmpty(tableCell.Style[HtmlTextWriterStyle.VerticalAlign])
                            ? (Aligment?)null
                            : (Aligment)Enum.Parse(typeof(Aligment), tableCell.Style[HtmlTextWriterStyle.VerticalAlign]),
                        DefaultStyleId,
                        "solid").ToString();
                }
            }
        }

        protected void WriteStartRow(int? height)
        {
            _columnIndex = 0;
            _writer.WriteStartElementExt("row", "r", (++_rowIndex).ToString(), "spans", "1:" + _rowSpans.Length);
            if (height != null)
            {
                _writer.WriteAttributeString("ht", ((double)height * 1.8).ToString("0"));
                _writer.WriteAttributeString("customHeight", "1");
            }
        }

        protected int AddStyle(CellProperties cellProps, string parentStyleId, string pattern)
        {
            return AddStyle(
                    string.IsNullOrEmpty(cellProps.BColor) ? null : (Color?)ColorTranslator.FromHtml(cellProps.BColor),
                    string.IsNullOrEmpty(cellProps.PColor) ? null : (Color?)ColorTranslator.FromHtml(cellProps.PColor),
                    cellProps.Size,
                    cellProps.Bold,
                    cellProps.Italic,
                    cellProps.HAligment,
                    null,
                    parentStyleId,
                    pattern);
        }

        protected int AddStyle(Color? bColor, Color? pColor, int? size, bool? bold, bool? italic, Aligment? hAligment, Aligment? vAligment, string parentStyleId, string pattern)
        {
            if (bColor == null && pColor == null && size == null && bold == null && italic == null && hAligment == null) return _styles[_baseStyles[parentStyleId]];
            var style = _baseStyles[parentStyleId];
            if (bColor != null)
            {
                var sFill = new StyleFill { Color = bColor.Value, Pattern = pattern };
                if (_fills.ContainsKey(sFill))
                    style.FillId = _fills[sFill];
                else
                    style.FillId = _fills[sFill] = _fills.Count;
            }
            else
                style.FillId = null;
            if (pColor != null || size != null || bold != null || italic != null)
            {
                var sFont = _fonts.First(r => r.Value == _baseStyles[parentStyleId].FontId).Key;
                if (pColor != null) sFont.Color = pColor;
                if (size != null) sFont.Size = size.Value;
                if (bold != null) sFont.Bold = bold.Value;
                if (italic != null) sFont.Italic = italic.Value;

                if (_fonts.ContainsKey(sFont))
                    style.FontId = _fonts[sFont];
                else
                    style.FontId = _fonts[sFont] = _fonts.Count;
            }

            if (hAligment != null) style.HorizontalAlignment = hAligment.Value;
            if (vAligment != null) style.VerticalAlignment = vAligment.Value;

            if (_styles.ContainsKey(style))
                return _styles[style];

            return _styles[style] = _styles.Count;
        }

        protected void MoveRowIndex()
        {
            _columnIndex = 0;
            foreach (var rowNextSpan in _addedRowSpans)
                _rowSpans[rowNextSpan.Key] = rowNextSpan.Value;
            _addedRowSpans.Clear();
            for (int i = 0; i < _rowSpans.Length; i++)
                _rowSpans[i]--;
            MoveColumnIndex(0, null);
        }

        private void MoveColumnIndex(int colSpan, string styleId)
        {
            var addCells = 1;
            while (addCells < colSpan)
                _writer.WriteElementStringExt("c", string.Empty, "r", GetLaterByInt(_columnIndex + addCells++) + _rowIndex, "s", styleId);

            _columnIndex += colSpan;
            while (_columnIndex < _rowSpans.Length && _rowSpans[_columnIndex] > 0)
            {
                _writer.WriteElementStringExt("c", string.Empty, "r", GetLaterByInt(_columnIndex) + _rowIndex, "s", _rowStyles[_columnIndex]);
                _columnIndex++;
            }
        }

        private void AddRowSpans(int rowSpan, int colSpan, string styleId)
        {
            while (colSpan > 0)
            {
                _addedRowSpans[_columnIndex + --colSpan] = rowSpan;
                _rowStyles[_columnIndex + colSpan] = styleId;
            }
        }

        protected string RenderCell(XmlTextWriter writer, string stringData, int rowSpan, int colSpan, string styleId, ColumnType columnType, string formula)
        {
            var startCell = GetLaterByInt(_columnIndex) + _rowIndex;
            if (_baseStyles.ContainsKey(styleId))
                styleId = _styles[_baseStyles[styleId]].ToString();
            writer.WriteStartElementExt("c", "r", startCell, "s", styleId);
            if (!string.IsNullOrEmpty(stringData))
            {
                if (!string.IsNullOrEmpty(formula))
                    writer.WriteElementString("f", formula);

                decimal decimalValue;
                if (columnType == ColumnType.Numeric && decimal.TryParse(stringData, out decimalValue))
                {
                    writer.WriteElementString("v", stringData.Replace(",", "."));
                }
                else
                {
                    writer.WriteAttributeString("t", "s");
                    writer.WriteElementString("v", GetSharedStringIndex(stringData).ToString());
                }
            }
            else if (!string.IsNullOrEmpty(formula))
                writer.WriteElementString("f", formula);

            writer.WriteEndElement();

            if (rowSpan > 1 || colSpan > 1)
            {
                _mergedCells.Add(
                    new KeyValuePair<Point, Point>(
                        new Point(_columnIndex, _rowIndex),
                        new Point(_columnIndex + colSpan - 1, _rowIndex + rowSpan - 1)));
            }

            AddRowSpans(rowSpan, colSpan, styleId);
            MoveColumnIndex(colSpan, styleId);
            return startCell + ":" + GetLaterByInt(_columnIndex - 1) + (_rowIndex + rowSpan - 1);
        }

        private int GetSharedStringIndex(string stringData)
        {
            _sharedValuesCount++;
            if (_sharedValues.ContainsKey(stringData))
                return _sharedValues[stringData];
            return _sharedValues[stringData] = _sharedValues.Count;
        }

        protected static string GetLaterByInt(int value)
        {
            if (value == 0) return "A";
            string result = string.Empty;
            int mod = value % 26;
            result = (char)('A' + mod) + result;
            value /= 26;
            while (value > 0)
            {
                mod = value % 26 - 1;
                result = (char)('A' + mod) + result;
                value /= 26;
            }

            return result;
        }

        protected void AddHyperLink(string rangeOfCell, string cellData, string href)
        {
            if (href.StartsWith("javascript:")) return;

            string keyHref;
            if (_hyperlinkKeys.ContainsKey(href))
            {
                keyHref = _hyperlinkKeys[href];
            }
            else
            {
                keyHref = _hyperlinkKeys[href] = "rId" + (_hyperlinkKeys.Count + 1);
                Uri url = HttpContext.Current.Request.Url;
                var uri = new Uri(new Uri(url.Scheme + "://" + url.Host + ":" + url.Port), href);
                _worksheetPart.AddHyperlinkRelationship(uri, true, keyHref);
            }

            _hyperlinkRanges[rangeOfCell] = new Pair<string, string>(keyHref, cellData);
        }

        #endregion
    }
}
