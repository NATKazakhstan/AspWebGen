namespace Nat.Web.ReportManager.CustomExport
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    using Stimulsoft.Base;
    using Stimulsoft.Base.Drawing;
    using Stimulsoft.Base.Localization;
    using Stimulsoft.Base.Services;
    using Stimulsoft.Report;
    using Stimulsoft.Report.Components;
    using Stimulsoft.Report.Components.ShapeTypes;
    using Stimulsoft.Report.Export;

    using Encoder = System.Drawing.Imaging.Encoder;

    [StiServiceBitmap(typeof(StiExportService), "Stimulsoft.Report.Images.Exports.Rtf.png")]
    public class StiRtfExportService : StiExportService
    {
        // Fields
        #region Fields

        private readonly ImageCodecInfo _imageCodec = null;
        private int _baseFontNumber;
        private Hashtable _bookmarkList;
        private byte _charsetCount;
        private byte[] _codePageToFont;
        private ArrayList _colorList;
        private ArrayList _fontList;
        private int[] _fontToCodePages;
        private float _imageQuality = 0.75f;
        private float _imageResolution = 0.96f;
        private StiMatrix _matrix;
        private int _pageHeight;
        private int _pageLeftMargins;
        private int _pageWidth;
        private int _pageWidthWithMargins;
        private ArrayList _styleList;
        private StreamWriter _sw;
        private Stream _sw2;
        private int[] _unicodeMapArray;
        private bool _useStyles;

        #endregion

        #region Public Properties

        public override string DefaultExtension
        {
            get { return "rtf"; }
        }

        public override StiExportFormat ExportFormat
        {
            get { return StiExportFormat.Rtf; }
        }

        public override string ExportNameInMenu
        {
            get { return StiLocalization.Get("Export", "ExportTypeRtfFile"); }
        }

        public override string GroupCategory
        {
            get { return "Word"; }
        }

        public override bool MultipleFiles
        {
            get { return false; }
        }

        public override int Position
        {
            get { return 0x15; }
        }
        
        #endregion

        #region Properties

        internal StiMatrix Matrix
        {
            get { return _matrix; }
        }

        #endregion

        // Methods
        #region Public Methods and Operators

        public override void ExportTo(StiReport report, Stream stream, StiExportSettings settings)
        {
            ExportRtf(report, stream, (StiRtfExportSettings)settings);
        }

        public override void Export(StiReport report, string fileName, bool sendEMail, StiGuiMode guiMode)
        {
            throw new NotSupportedException();
            /*
            using (var runner = StiGuiOptions.GetExportFormRunner("StiRtfExportSetupForm", guiMode, OwnerWindow))
            {
                runner["CurrentPage"] = report.CurrentPrintPage;
                runner["OpenAfterExportEnabled"] = !sendEMail;
                runner.Complete += (sender, args)=>
                    {
                        if (!args.DialogResult)
                            return;
                        if ((fileName == null) || (fileName.Length == 0))
                            fileName = GetFileName(report);
                        if (fileName != null)
                        {
                            StiFileUtils.ProcessReadOnly(fileName);
                            var stream = new FileStream(fileName, FileMode.Create);
                            StartProgress(guiMode);
                            var settings = new StiRtfExportSettings();
                            settings.PageRange = runner["PagesRange"] as StiPagesRange;
                            settings.ExportMode = (StiRtfExportMode)runner["ExportMode"];
                            settings.UsePageHeadersAndFooters = (bool)runner["UsePageHeadersAndFooters"];
                            settings.ImageQuality = (float)runner["ImageQuality"];
                            settings.ImageResolution = (float)runner["Resolution"];
                            StartExport(
                                report,
                                stream,
                                settings,
                                sendEMail,
                                (bool)runner["OpenAfterExport"],
                                fileName,
                                guiMode);
                        }
                    };
                runner.ShowDialog();
            }*/
        }

        public void ExportRtf(StiReport report, Stream stream)
        {
            var settings = new StiRtfExportSettings();
            ExportRtf(report, stream, settings);
        }

        public void ExportRtf(StiReport report, string fileName)
        {
            StiFileUtils.ProcessReadOnly(fileName);
            var stream = new FileStream(fileName, FileMode.Create);
            ExportRtf(report, stream);
            stream.Flush();
            stream.Close();
        }

        public void ExportRtf(StiReport report, Stream stream, StiRtfExportMode exportMode)
        {
            var settings = new StiRtfExportSettings();
            settings.ExportMode = exportMode;
            ExportRtf(report, stream, settings);
        }

        public void ExportRtf(StiReport report, Stream stream, StiRtfExportSettings settings)
        {
            StiLogService.Write(GetType(), "Export report to Rtf format");
            if (settings == null)
                throw new ArgumentNullException("The 'settings' argument cannot be equal in null.");

            var pageRange = settings.PageRange;
            _imageResolution = settings.ImageResolution;
            _imageQuality = settings.ImageQuality;
            _unicodeMapArray = new int[0x10000];
            _codePageToFont = new byte[10];
            _fontToCodePages = new int[10];
            _charsetCount = 0;
            _useStyles = true;
            if (_imageQuality < 0f)
                _imageQuality = 0f;
            if (_imageQuality > 1f)
                _imageQuality = 1f;
            if (_imageResolution < 10f)
                _imageResolution = 10f;

            _imageResolution /= 100f;
            double num = 1.0;
            double num2 = 1.0;
            _colorList = new ArrayList();
            _fontList = new ArrayList();
            _styleList = new ArrayList();
            _sw = new StreamWriter(stream, Encoding.GetEncoding(0x4e4));
            _sw2 = stream;
            for (int i = 0; i < 0x10000; i++)
                _unicodeMapArray[i] = 0;

            _bookmarkList = new Hashtable();
            StiPagesCollection selectedPages = pageRange.GetSelectedPages(report.RenderedPages);
            foreach (StiPage page in selectedPages)
            {
                selectedPages.GetPage(page);
                foreach (StiComponent component in page.Components)
                {
                    if (!component.Enabled)
                        continue;

                    var brush = component as IStiTextBrush;
                    if (brush != null)
                        GetColorNumberInt(_colorList, StiBrush.ToColor(brush.TextBrush));
                    var brush2 = component as IStiBrush;
                    if (brush2 != null)
                        GetColorNumber(_colorList, StiBrush.ToColor(brush2.Brush));
                    var border = component as IStiBorder;
                    if ((border != null) && !(component is IStiIgnoreBorderWhenExport))
                    {
                        var border2 = border.Border as StiAdvancedBorder;
                        if (border2 != null)
                        {
                            GetColorNumber(_colorList, border2.LeftSide.Color);
                            GetColorNumber(_colorList, border2.RightSide.Color);
                            GetColorNumber(_colorList, border2.TopSide.Color);
                            GetColorNumber(_colorList, border2.BottomSide.Color);
                        }
                        else
                            GetColorNumber(_colorList, border.Border.Color);
                    }

                    var shape = component as StiShape;
                    if (shape != null)
                        GetColorNumber(_colorList, shape.BorderColor);
                    var font = component as IStiFont;
                    if (font != null)
                        GetFontNumber(_fontList, font.Font);

                    /*if (component is StiRichText)
                    {
                        GetRtfString((StiRichText)component);
                    }*/
                    var stiText = component as StiText;
                    if (stiText != null)
                    {
                        var outputString = new StringBuilder(stiText.Text);
                        if (stiText.AllowHtmlTags)
                            outputString = new StringBuilder(ConvertTextWithHtmlTagsToRtfText(stiText, stiText.Text));
                        var builder2 = CheckArabic(outputString);
                        try
                        {
                            for (int n = 0; n < builder2.Length; n++)
                            {
                                if (builder2[n] >= _unicodeMapArray.Length)
                                    throw new Exception("builder2[n] >= _unicodeMapArray.Length");
                                _unicodeMapArray[builder2[n]] = 1;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Place 1", e);
                        }

                        GetRtfStyleFromComponent(component);
                    }

                    if (StiOptions.Export.Rtf.UsePageRefField && (component.BookmarkValue != null))
                    {
                        string key = component.BookmarkValue.ToString();
                        if (((key == null) || (key.Length <= 0)) || _bookmarkList.ContainsKey(key))
                            continue;
                        _bookmarkList.Add(key, key);
                    }
                }
            }

            try
            {
                for (int j = 0; j < 0x10000; j++)
                {
                    if (_unicodeMapArray[j] != 0)
                    {
                        if (j >= StiEncode.unicodeToCodePageArray.Length)
                            throw new Exception("j >= StiEncode.unicodeToCodePageArray.Length");
                        if (StiEncode.unicodeToCodePageArray[j] >= _codePageToFont.Length)
                            throw new Exception("StiEncode.unicodeToCodePageArray[j] >= _codePageToFont.Length");

                        _codePageToFont[StiEncode.unicodeToCodePageArray[j]] = 1;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Place 2", e);
            }
            

            _codePageToFont[1] = 1;
            for (int k = 1; k < 10; k++)
            {
                if (_codePageToFont[k] != 0)
                {
                    _codePageToFont[k] = _charsetCount;
                    _fontToCodePages[_charsetCount] = k - 1;
                    _charsetCount = (byte)(_charsetCount + 1);
                }
            }

            _codePageToFont[0] = _codePageToFont[1];
            for (int m = 0; m < _styleList.Count; m++)
            {
                var info = (StiRtfStyleInfo)_styleList[m];
                info.FontNumber *= _charsetCount;
                _styleList[m] = info;
            }

            RenderStartDoc();
            StatusString = StiLocalization.Get("Export", "ExportingCreatingDocument");
            _sw.WriteLine(@"\nolead");
            var lastPageName = string.Empty;
            foreach (StiPage page3 in selectedPages)
            {
                selectedPages.GetPage(page3);
                InvokeExporting(page3, selectedPages, 0, 0);
                if (IsStoped)
                    return;
                if (page3 == selectedPages[0])
                    RenderPageHeader(page3, report.Pages[0]);
                if (lastPageName != page3.Name
                    && !string.IsNullOrEmpty(lastPageName)
                    && !page3.PrintOnPreviousPage)
                    _sw.WriteLine(@"\sect");
                _sw.WriteLine("{");

                // кажись был кастыль "2", сделал "20" :)
                int num13 = ((int)Math.Round((_pageHeight) / (12.0 / num2))) + 20;
                    
                var listArray = new List<SortedList>(num13);

                foreach (StiComponent component2 in page3.Components)
                {
                    if (component2.Enabled)
                    {
                        double num15 = page3.Unit.ConvertToHInches(component2.Left);
                        double num16 = page3.Unit.ConvertToHInches(component2.Top);
                        double num17 = page3.Unit.ConvertToHInches(component2.Right);
                        double num18 = page3.Unit.ConvertToHInches(component2.Bottom);
                        var data = new StiRtfData();
                        data.X = (int)Math.Round((num15 * 14.4) * num);
                        data.Y = (int)Math.Round((num16 * 14.4) * num2);
                        data.Width = ((int)Math.Round((num17 * 14.4) * num)) - data.X;
                        data.Height = ((int)Math.Round((num18 * 14.4) * num2)) - data.Y;
                        data.Component = component2;
                        data.X++;
                        data.Y++;
                        int x = data.X;
                        var num20 = (int)Math.Round(num16 / (12.0 / num2));
                        
                        while (listArray.Count <= num20)
                            listArray.Add(new SortedList());
                        
                        while (listArray[num20].IndexOfKey(x) != -1)
                        {
                            x++;
                        }

                        listArray[num20].Add(x, data);
                    }
                }

                int num21 = 0;
                try
                {
                    while (num21 < listArray.Count)
                    {
                        if (listArray[num21].Count > 0)
                        {
                            for (int num22 = 0; num22 < listArray[num21].Count; num22++)
                            {
                                var data = (StiRtfData)listArray[num21].GetByIndex(num22);
                                RenderComponent(data);
                            }
                        }

                        num21++;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Place 3", e);
                }

                _sw.WriteLine("}");
                if (page3 != selectedPages[selectedPages.Count - 1])
                    RenderPageFooter();
                lastPageName = page3.Name;
            }

            RenderEndDoc();
            _sw.Flush();
            if (_matrix != null)
            {
                _matrix.Clear();
                _matrix = null;
            }

            _colorList = null;
            _fontList = null;
            _styleList = null;
            _unicodeMapArray = null;
            _codePageToFont = null;
            _fontToCodePages = null;
            _bookmarkList = null;
        }

        public void ExportRtf(StiReport report, Stream stream, StiRtfExportMode exportMode, StiPagesRange pageRange)
        {
            var settings = new StiRtfExportSettings();
            settings.PageRange = pageRange;
            settings.ExportMode = exportMode;
            ExportRtf(report, stream, settings);
        }

        public void ExportRtf(
            StiReport report, 
            Stream stream, 
            StiRtfExportMode exportMode, 
            StiPagesRange pageRange, 
            bool usePageHeadersAndFooters)
        {
            var settings = new StiRtfExportSettings();
            settings.PageRange = pageRange;
            settings.ExportMode = exportMode;
            settings.UsePageHeadersAndFooters = usePageHeadersAndFooters;
            ExportRtf(report, stream, settings);
        }

        public void ExportRtf(
            StiReport report, 
            Stream stream, 
            int codePage, 
            StiRtfExportMode exportMode, 
            StiPagesRange pageRange)
        {
            var settings = new StiRtfExportSettings();
            settings.PageRange = pageRange;
            settings.CodePage = codePage;
            settings.ExportMode = exportMode;
            ExportRtf(report, stream, settings);
        }

        public void ExportRtf(
            StiReport report, 
            Stream stream, 
            int codePage, 
            StiRtfExportMode exportMode, 
            StiPagesRange pageRange, 
            bool usePageHeadersAndFooters)
        {
            var settings = new StiRtfExportSettings();
            settings.PageRange = pageRange;
            settings.CodePage = codePage;
            settings.ExportMode = exportMode;
            settings.UsePageHeadersAndFooters = usePageHeadersAndFooters;
            ExportRtf(report, stream, settings);
        }

        public override string GetFilter()
        {
            return StiLocalization.Get("FileFilters", "RtfFiles");
        }

        #endregion

        #region Methods

        private static string GetLineHeightInTwips(double lineHeightScale)
        {
            return string.Format("{0}sl{1}{0}slmult1", "\x0017", Math.Round(235.2 * lineHeightScale));
        }

        private StringBuilder CheckArabic(StringBuilder outputString)
        {
            if (StiOptions.Export.Rtf.ConvertDigitsToArabic)
                return StiExportUtils.ConvertDigitsToArabic(outputString, StiOptions.Export.Rtf.ArabicDigitsType);
            return outputString;
        }

        private bool CheckShape1(StiShape shape)
        {
            if (
                (shape.ShapeType is StiVerticalLineShapeType) ||
                (shape.ShapeType is StiHorizontalLineShapeType) ||
                (shape.ShapeType is StiTopAndBottomLineShapeType) ||
                (shape.ShapeType is StiLeftAndRightLineShapeType) ||
                (shape.ShapeType is StiRectangleShapeType)
                
                // 				(shape.ShapeType is StiRoundedRectangleShapeType) ||
                // 				(shape.ShapeType is StiDiagonalDownLineShapeType) ||
                // 				(shape.ShapeType is StiDiagonalUpLineShapeType) ||
                // 				(shape.ShapeType is StiTriangleShapeType) ||
                // 				(shape.ShapeType is StiOvalShapeType) ||
                // 				(shape.ShapeType is StiArrowShapeType)
                )
                return true;
            else
                return false;
        }

        private bool CheckShape2(StiShape shape)
        {
            return (shape.ShapeType is StiVerticalLineShapeType) ||
                   (shape.ShapeType is StiHorizontalLineShapeType) ||
                   (shape.ShapeType is StiTopAndBottomLineShapeType) ||
                   (shape.ShapeType is StiLeftAndRightLineShapeType) ||
                   (shape.ShapeType is StiRectangleShapeType) ||
                   // 				(shape.ShapeType is StiRoundedRectangleShapeType) ||
                   (shape.ShapeType is StiDiagonalDownLineShapeType) ||
                   (shape.ShapeType is StiDiagonalUpLineShapeType) ||
                   // 				(shape.ShapeType is StiTriangleShapeType) ||
                   (shape.ShapeType is StiOvalShapeType);
        }

        private string ConvertTextWithHtmlTagsToRtfText(StiText stiText, string text)
        {
            string inputHtml = text;
            var ts = new StiTextRenderer.StiHtmlTagsState(
                stiText.Font.Bold, 
                stiText.Font.Italic, 
                stiText.Font.Underline, 
                stiText.Font.Strikeout, 
                stiText.Font.SizeInPoints, 
                stiText.Font.Name, 
                StiBrush.ToColor(stiText.TextBrush), 
                StiBrush.ToColor(stiText.Brush), 
                false, 
                false, 
                0.0, 
                0.0, 
                1.0, 
                stiText.HorAlignment);
            var baseState = new StiTextRenderer.StiHtmlState(ts, 0);
            var list = StiTextRenderer.ParseHtmlToStates(inputHtml, baseState);
            var builder = new StringBuilder(GetLineHeightInTwips(1.0));
            StiTextRenderer.StiHtmlTagsState state3 = ts;
            for (int i = 0; i < list.Count; i++)
            {
                var state4 = (StiTextRenderer.StiHtmlState)list[i];
                StiTextRenderer.StiHtmlTagsState tS = state4.TS;
                var builder2 = new StringBuilder();
                if (tS.Bold != state3.Bold)
                    builder2.Append(tS.Bold ? @"\b" : @"\b0");
                if (tS.Italic != state3.Italic)
                    builder2.Append(tS.Italic ? @"\i" : @"\i0");
                if (tS.Underline != state3.Underline)
                    builder2.Append(tS.Underline ? @"\ul" : @"\ul0");
                if (tS.Strikeout != state3.Strikeout)
                    builder2.Append(tS.Strikeout ? @"\strike" : @"\strike0");
                if (tS.Superscript != state3.Superscript)
                    builder2.Append(tS.Superscript ? @"\super" : @"\nosupersub");
                if (tS.Subsript != state3.Subsript)
                    builder2.Append(tS.Subsript ? @"\sub" : @"\nosupersub");
                if (tS.FontColor != state3.FontColor)
                    builder2.Append(string.Format(@"\cf{0}", GetColorNumberInt(_colorList, tS.FontColor)));
                if (tS.BackColor != state3.BackColor)
                    builder2.Append(string.Format(@"\highlight{0}", GetColorNumber(_colorList, tS.BackColor)));
                if (tS.FontSize != state3.FontSize)
                    builder2.Append(string.Format(@"\fs{0}", (int)(tS.FontSize * 2f)));
                if (tS.LetterSpacing != state3.LetterSpacing)
                {
                    builder2.Append(
                        string.Format(
                            @"\expnd{0}", 
                            ((int)((tS.LetterSpacing * tS.FontSize) * 3.8))
                            + StiOptions.Export.Rtf.SpaceBetweenCharacters));
                }

                if (tS.LineHeight != state3.LineHeight)
                    builder2.Append(GetLineHeightInTwips(tS.LineHeight));
                if (tS.TextAlign != state3.TextAlign)
                {
                    string str2 = @"\ql";
                    if (tS.TextAlign == StiTextHorAlignment.Center)
                        str2 = @"\qc";
                    if (tS.TextAlign == StiTextHorAlignment.Right)
                        str2 = @"\qr";
                    if (tS.TextAlign == StiTextHorAlignment.Width)
                        str2 = @"\qj";
                    builder2.Append(str2);
                }

                if (tS.FontName != state3.FontName)
                {
                    builder2.Append(
                        string.Format(
                            "{0}{1}", 
                            '\x0010', 
                            (char)(0x100 + GetFontNumber(_fontList, tS.FontName))));
                }

                if (builder2.Length > 0)
                {
                    builder.Append(builder2.Replace(@"\", "\x0017"));
                    builder.Append(" ");
                }

                if (state4.Text.ToString() == "\n")
                    builder.Append("\n");
                else
                    builder.Append(state4.Text);
                state3 = tS;
            }

            return
                builder.ToString().Replace("&nbsp;", "\x00a0").Replace("&lt;", "<").Replace("&gt;", ">").Replace(
                    "&quot;", "\"").Replace("&amp;", "&");
        }

        private void DrawLine(int tX1, int tY1, int tX2, int tY2, Color tColor, string stBorderWidth)
        {
            if (tColor.A == 0)
                return;
            _sw.Write("{\\shp{\\*");

            _sw.Write(
                "\\shpinst\\shpleft{0}\\shptop{1}\\shpright{2}\\shpbottom{3}", 
                tX1 < tX2 ? tX1 : tX2, 
                tY1 < tY2 ? tY1 : tY2, 
                tX1 > tX2 ? tX1 : tX2, 
                tY1 > tY2 ? tY1 : tY2);
            _sw.Write("\\shpwr3");
            _sw.Write("{\\sp{\\sn shapeType}{\\sv 20}}");
            _sw.Write("{\\sp{\\sn fFlipH}{\\sv " + (tX1 < tX2 ? "0" : "1") + "}}");
            _sw.Write("{\\sp{\\sn fFlipV}{\\sv " + (tY1 < tY2 ? "0" : "1") + "}}");
            _sw.Write("{\\sp{\\sn fFilled}{\\sv 0}}");
            _sw.Write(
                "{\\sp{\\sn lineColor}{\\sv " + string.Format("{0}", tColor.B * 65536 + tColor.G * 256 + tColor.R)
                + "}}");
            _sw.Write("{\\sp{\\sn lineWidth}{\\sv " + stBorderWidth + "}}");
            _sw.Write("{\\sp{\\sn fLine}{\\sv 1}}");
            _sw.WriteLine("}}");
        }

        private void FillRect(StiRtfData pp, Color tColor)
        {
            if (tColor.A != 0)
            {
                _sw.Write("{\\shp{\\*");
                _sw.Write(
                    "\\shpinst\\shpleft{0}\\shptop{1}\\shpright{2}\\shpbottom{3}", 
                    pp.X, 
                    pp.Y, 
                    pp.X + pp.Width, 
                    pp.Y + pp.Height);
                _sw.Write("\\shpwr3");
                _sw.Write("{\\sp{\\sn shapeType}{\\sv 1}}");
                _sw.Write("{\\sp{\\sn fFlipH}{\\sv 0}}");
                _sw.Write("{\\sp{\\sn fFlipV}{\\sv 0}}");
                _sw.Write(
                    "{\\sp{\\sn fillColor}{\\sv " + string.Format("{0}", tColor.B * 65536 + tColor.G * 256 + tColor.R)
                    + "}}");
                _sw.Write("{\\sp{\\sn fFilled}{\\sv 1}}");
                _sw.Write("{\\sp{\\sn fLine}{\\sv 0}}");
                _sw.WriteLine("}}");
            }
        }

        private int GetCharsetIndex(int charset)
        {
            int index = 0;
            for (int i = 0; i < 9; i++)
            {
                if (StiEncode.codePagesTable[i, 2] == charset)
                {
                    index = i + 1;
                    break;
                }
            }

            if (_charsetCount == 0)
            {
                _codePageToFont[index] = 1;
                return 0;
            }

            return _codePageToFont[index];
        }

        private string GetColorNumber(ArrayList tmpColorList, Color incomingColor)
        {
            return GetColorNumberInt(tmpColorList, incomingColor).ToString();
        }

        private int GetColorNumberInt(ArrayList tmpColorList, Color incomingColor)
        {
            if (tmpColorList.Count > 0)
            {
                for (int i = 0; i < tmpColorList.Count; i++)
                {
                    if (((Color)tmpColorList[i]) == incomingColor)
                        return i;
                }
            }

            tmpColorList.Add(incomingColor);
            return tmpColorList.Count - 1;
        }

        private int GetFontNumber(ArrayList tmpFontList, Font incomingFont)
        {
            if (tmpFontList.Count > 0)
            {
                for (int i = 0; i < tmpFontList.Count; i++)
                {
                    var font = (Font)tmpFontList[i];
                    if (font.Name == incomingFont.Name)
                        return i * ((_charsetCount == 0) ? 1 : _charsetCount);
                }
            }

            tmpFontList.Add(incomingFont);
            int num2 = tmpFontList.Count - 1;
            return num2 * ((_charsetCount == 0) ? 1 : _charsetCount);
        }

        private int GetFontNumber(ArrayList tmpFontList, string fontName)
        {
            if (tmpFontList.Count > 0)
            {
                for (int i = 0; i < tmpFontList.Count; i++)
                {
                    var font = (Font)tmpFontList[i];
                    if (font.Name == fontName)
                        return i * ((_charsetCount == 0) ? 1 : _charsetCount);
                }
            }

            tmpFontList.Add(new Font(fontName, 8f));
            int num2 = tmpFontList.Count - 1;
            return num2 * ((_charsetCount == 0) ? 1 : _charsetCount);
        }

        private int GetFontNumber(ArrayList tmpFontList, Font incomingFont, int charset)
        {
            if (tmpFontList.Count > 0)
            {
                for (int i = 0; i < tmpFontList.Count; i++)
                {
                    var font = (Font)tmpFontList[i];
                    if (font.Name == incomingFont.Name)
                        return (i * ((_charsetCount == 0) ? 1 : _charsetCount)) + GetCharsetIndex(charset);
                }
            }

            tmpFontList.Add(incomingFont);
            int num2 = tmpFontList.Count - 1;
            return (num2 * ((_charsetCount == 0) ? 1 : _charsetCount)) + GetCharsetIndex(charset);
        }

        private string GetImageString(Image image, float zoom, int absw, int absh)
        {
            var memw = new MemoryStream();

            

            if (_imageCodec == null)
                image.Save(memw, ImageFormat.Jpeg);
                

                #region Save jpeg with quality parameter
            else
            {
                var imageEncoderParameters = new EncoderParameters(1);
                imageEncoderParameters.Param[0] =
                    new EncoderParameter(Encoder.Quality, (long)(_imageQuality * 100)); // <------------;
                image.Save(memw, _imageCodec, imageEncoderParameters);
            }

            #endregion

            byte[] bytes = memw.ToArray();
            memw.Close();
            var sb = new StringBuilder((bytes.Length * 2) + 200);

            float zoom2 = zoom / _imageResolution;

            sb.Append(
                "{" + string.Format(
                    "\\pict\\picscalex{0}\\picscaley{1}\\picwgoal{2}\\pichgoal{3}\\jpegblip ", 
                          // (int)(100 / zoom2),	//corrected 2008.03.03
                          // (int)(100 / zoom2),	//corrected 2008.03.03
                    100, 
                          // corrected 2008.03.03
                    100, 
                          // corrected 2008.03.03
                          // absw * zoom,
                          // absh * zoom));
                    absw, 
                    absh));

            for (int index = 0; index < bytes.Length; index++)
            {
                sb.Append(bytes[index].ToString("x").PadLeft(2, (char)48));
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string GetLineStyle(StiBorderSide border, ArrayList colorList)
        {
            var builder = new StringBuilder();
            if (border != null)
            {
                switch (border.Style)
                {
                    case StiPenStyle.Solid:
                        builder.Append(@"\brdrs");
                        break;

                    case StiPenStyle.Dash:
                        builder.Append(@"\brdrdash");
                        break;

                    case StiPenStyle.DashDot:
                        builder.Append(@"\brdrdashd");
                        break;

                    case StiPenStyle.DashDotDot:
                        builder.Append(@"\brdrdashdd");
                        break;

                    case StiPenStyle.Dot:
                        builder.Append(@"\brdrdot");
                        break;

                    case StiPenStyle.Double:
                        builder.Append(@"\brdrdb");
                        break;

                    case StiPenStyle.None:
                        return builder.ToString();
                }

                builder.Append(@"\brdrw");
                builder.Append((int)(border.Size * 15.0));
                builder.Append(string.Format(@"\brdrcf{0}", this.GetColorNumber(colorList, border.Color)));
            }

            return builder.ToString();
        }

        private StringBuilder GetRtfString(StiRichText text)
        {
            StringBuilder builder4;
            if (text == null || text.RtfText == string.Empty)
                return new StringBuilder();

            var list = new ArrayList();
            var list2 = new ArrayList();
            var list3 = new ArrayList();
            string inputString = text.RtfText;
            int index = inputString.IndexOf(@"{\fonttbl");
            if (index != -1)
            {
                index += 9;
                while ((index < inputString.Length) && (inputString[index] != '}'))
                {
                    while ((index < inputString.Length) && (inputString[index] != '{'))
                    {
                        index++;
                    }

                    int num2 = 0;
                    int num3 = index;
                    do
                    {
                        if (inputString[num3] == '{')
                            num2++;
                        if (inputString[num3] == '}')
                            num2--;
                        num3++;
                    }
                    while (num2 > 0);
                    list.Add(inputString.Substring(index, num3 - index));
                    index = num3;
                    while (((index < inputString.Length) && (inputString[index] != '{')) && (inputString[index] != '}'))
                    {
                        index++;
                    }
                }

                inputString = inputString.Remove(0, index + 1);
            }

            for (int i = 0; i < list.Count; i++)
            {
                int num5;
                var str2 = (string)list[i];
                do
                {
                    num5 = str2.IndexOf(@"{\*");
                    if (num5 != -1)
                    {
                        int num6 = 0;
                        int num7 = num5;
                        do
                        {
                            if (str2[num7] == '{')
                                num6++;
                            if (str2[num7] == '}')
                                num6--;
                            num7++;
                        }
                        while (num6 > 0);
                        str2 = str2.Remove(num5, num7 - num5);
                        if ((str2[num5 - 1] != ' ') && (str2[num5] != ' '))
                            str2 = str2.Insert(num5, " ");
                    }
                }
                while (num5 != -1);
                num5 = str2.IndexOf(" ");
                int length = (str2.Length - num5) - 2;
                if (str2[((num5 + 1) + length) - 1] == ';')
                    length--;
                var incomingFont = new Font(str2.Substring(num5 + 1, length), 10f);
                int charset = 0;
                num5 = str2.IndexOf(@"\fcharset");
                if (num5 > 0)
                {
                    num5 += 9;
                    var builder = new StringBuilder();
                    while (char.IsDigit(str2[num5]))
                    {
                        builder.Append(str2[num5]);
                        num5++;
                    }

                    try
                    {
                        charset = int.Parse(builder.ToString());
                    }
                    catch
                    {
                        charset = 0;
                    }
                }

                string str4 = GetFontNumber(_fontList, incomingFont, charset).ToString();
                list2.Add(str4);
                var builder2 = new StringBuilder();
                for (num5 = str2.IndexOf(@"\f") + 2; char.IsDigit(str2[num5]); num5++)
                {
                    builder2.Append(str2[num5]);
                }

                list[i] = builder2.ToString();
            }

            index = inputString.IndexOf(@"{\colortbl");
            if (index != -1)
            {
                index += 10;
                while ((index < inputString.Length) && (inputString[index] != '}'))
                {
                    while (((index < inputString.Length) && (inputString[index] != ';')) && (inputString[index] != '\\'))
                    {
                        index++;
                    }

                    int num10 = index;
                    while (((num10 < inputString.Length) && (inputString[num10] != ';')) && (inputString[num10] != '}'))
                    {
                        num10++;
                    }

                    list3.Add(inputString.Substring(index, (num10 + 1) - index));
                    if (inputString[num10] == ';')
                        num10++;
                    index = num10;
                    while (((index < inputString.Length) && (inputString[index] != ';'))
                           && ((inputString[index] != '\\') && (inputString[index] != '}')))
                    {
                        index++;
                    }
                }

                inputString = inputString.Remove(0, index + 1);
            }

            for (int j = 0; j < list3.Count; j++)
            {
                var str5 = (string)list3[j];
                if (str5 != ";")
                {
                    int num12 = str5.IndexOf(@"\red") + 4;
                    var builder3 = new StringBuilder();
                    while (char.IsDigit(str5[num12]))
                    {
                        builder3.Append(str5[num12]);
                        num12++;
                    }

                    string s = builder3.ToString();
                    num12 = str5.IndexOf(@"\green") + 6;
                    builder3 = new StringBuilder();
                    while (char.IsDigit(str5[num12]))
                    {
                        builder3.Append(str5[num12]);
                        num12++;
                    }

                    string str7 = builder3.ToString();
                    num12 = str5.IndexOf(@"\blue") + 5;
                    builder3 = new StringBuilder();
                    while (char.IsDigit(str5[num12]))
                    {
                        builder3.Append(str5[num12]);
                        num12++;
                    }

                    string str8 = builder3.ToString();
                    Color incomingColor = Color.FromArgb(int.Parse(s), int.Parse(str7), int.Parse(str8));
                    list3[j] = GetColorNumber(_colorList, incomingColor);
                }
                else
                    list3[j] = GetColorNumber(_colorList, Color.Transparent);
            }

            index = inputString.IndexOf(@"{\stylesheet");
            if (index != -1)
            {
                int num13 = 0;
                int count = index;
                do
                {
                    if (inputString[count] == '{')
                        num13++;
                    if (inputString[count] == '}')
                        num13--;
                    count++;
                }
                while (num13 > 0);
                inputString = inputString.Remove(0, count);
            }

            int startIndex = 0;
            do
            {
                startIndex = inputString.IndexOf(@"\f", startIndex);
                if (startIndex != -1)
                {
                    if (char.IsDigit(inputString[startIndex + 2]))
                    {
                        startIndex += 2;
                        int num16 = startIndex;
                        builder4 = new StringBuilder();
                        while (char.IsDigit(inputString[num16]))
                        {
                            builder4.Append(inputString[num16]);
                            num16++;
                        }

                        string str9 = builder4.ToString();
                        for (int m = 0; m < list.Count; m++)
                        {
                            if (((string)list[m]) == str9)
                            {
                                inputString = inputString.Remove(startIndex, num16 - startIndex).Insert(
                                    startIndex, 
                                    (string)
                                    list2[m]);
                            }
                        }

                        startIndex = num16;
                    }
                    else
                        startIndex += 2;
                }
            }
            while (startIndex != -1);
            startIndex = 0;
            int num18 = 3;
            do
            {
                startIndex = inputString.IndexOf(@"\cf", startIndex);
                if (startIndex != -1)
                {
                    if (char.IsDigit(inputString[startIndex + num18]))
                    {
                        startIndex += num18;
                        int num19 = startIndex;
                        builder4 = new StringBuilder();
                        while (char.IsDigit(inputString[num19]))
                        {
                            builder4.Append(inputString[num19]);
                            num19++;
                        }

                        int num20 = int.Parse(builder4.ToString());
                        inputString = inputString.Remove(startIndex, num19 - startIndex).Insert(
                            startIndex, 
                            (string)list3[num20]);
                        startIndex = num19;
                    }
                    else
                        startIndex += num18;
                }
            }
            while (startIndex != -1);
            startIndex = 0;
            num18 = 6;
            do
            {
                startIndex = inputString.IndexOf(@"\cbpat", startIndex);
                if (startIndex != -1)
                {
                    if (char.IsDigit(inputString[startIndex + num18]))
                    {
                        startIndex += num18;
                        int num21 = startIndex;
                        builder4 = new StringBuilder();
                        while (char.IsDigit(inputString[num21]))
                        {
                            builder4.Append(inputString[num21]);
                            num21++;
                        }

                        int num22 = int.Parse(builder4.ToString());
                        inputString = inputString.Remove(startIndex, num21 - startIndex).Insert(
                            startIndex, 
                            (string)list3[num22]);
                        startIndex = num21;
                    }
                    else
                        startIndex += num18;
                }
            }
            while (startIndex != -1);
            startIndex = 0;
            num18 = 10;
            do
            {
                startIndex = inputString.IndexOf(@"\highlight", startIndex);
                if (startIndex != -1)
                {
                    if (char.IsDigit(inputString[startIndex + num18]))
                    {
                        startIndex += num18;
                        int num23 = startIndex;
                        builder4 = new StringBuilder();
                        while (char.IsDigit(inputString[num23]))
                        {
                            builder4.Append(inputString[num23]);
                            num23++;
                        }

                        int num24 = int.Parse(builder4.ToString());
                        inputString = inputString.Remove(startIndex, num23 - startIndex).Insert(
                            startIndex, 
                            (string)list3[num24]);
                        startIndex = num23;
                    }
                    else
                        startIndex += num18;
                }
            }
            while (startIndex != -1);
            index = inputString.Length - 1;
            while (inputString[index] != '}')
            {
                index--;
            }

            index--;
            while (((inputString[index] == '\r') || (inputString[index] == '\n')) || (inputString[index] == ' '))
            {
                index--;
            }

            inputString = inputString.Substring(0, index + 1);
            if (inputString.EndsWith(@"\par"))
                inputString = inputString.Substring(0, inputString.Length - @"\par".Length);
            if (inputString.EndsWith(@"\par }"))
                inputString = inputString.Substring(0, inputString.Length - @"\par }".Length) + '}';
            inputString = deleteToken(inputString, @"\viewkind");
            inputString = deleteToken(inputString, @"\uc");
            while ((inputString[0] == '\r') || (inputString[0] == '\n'))
            {
                inputString = inputString.Remove(0, 1);
            }

            if (inputString.Substring(0, 5) == @"\pard")
                inputString = inputString.Remove(0, 5);
            inputString = deleteToken(inputString, @"\formprot");
            inputString = deleteToken(inputString, @"\pagebb");
            var builder5 = new StringBuilder();
            for (int k = 0; k < inputString.Length; k++)
            {
                int num26 = inputString[k];
                if (num26 > 0xff)
                    builder5.AppendFormat(@"\u{0}{1}", num26, "?");
                else
                    builder5.Append(inputString[k]);
            }

            return builder5;
        }

        private int GetRtfStyleFromComponent(StiComponent component)
        {
            if (component == null)
                return 0;
            var font = component as IStiFont;
            var brush = component as IStiTextBrush;
            var alignment = component as IStiTextHorAlignment;
            var options = component as IStiTextOptions;
            var styleInfo = new StiRtfStyleInfo();
            styleInfo.Name = component.Name;
            if ((component.ComponentStyle != null) && (component.ComponentStyle.Length > 0))
                styleInfo.Name = component.ComponentStyle;
            if (font != null)
            {
                styleInfo.FontNumber = GetFontNumber(_fontList, font.Font);
                styleInfo.FontSize = (int)Math.Round(font.Font.SizeInPoints * 2f, 0);
                styleInfo.Bold = font.Font.Bold;
                styleInfo.Italic = font.Font.Italic;
                styleInfo.Underline = font.Font.Underline;
            }

            if (brush != null)
                styleInfo.TextColor = GetColorNumberInt(_colorList, StiBrush.ToColor(brush.TextBrush));
            if (alignment != null)
                styleInfo.Alignment = alignment.HorAlignment;
            if (options != null)
                styleInfo.RightToLeft = options.TextOptions.RightToLeft;
            return GetStyleNumber(_styleList, styleInfo);
        }

        private int GetStyleNumber(ArrayList tmpStyleList, StiRtfStyleInfo styleInfo)
        {
            if (tmpStyleList.Count > 0)
            {
                for (int i = 0; i < tmpStyleList.Count; i++)
                {
                    var info = (StiRtfStyleInfo)tmpStyleList[i];
                    if (((((info.Alignment == styleInfo.Alignment) && (info.Name == styleInfo.Name))
                          && ((info.FontNumber == styleInfo.FontNumber) && (info.FontSize == styleInfo.FontSize)))
                         &&
                         (((info.Bold == styleInfo.Bold) && (info.Italic == styleInfo.Italic))
                          && ((info.Underline == styleInfo.Underline) && (info.TextColor == styleInfo.TextColor))))
                        && (info.RightToLeft == styleInfo.RightToLeft))
                        return i + 1;
                }
            }

            tmpStyleList.Add(styleInfo);
            int num2 = tmpStyleList.Count - 1;
            return num2 + 1;
        }

        private string MakeHorAlignString(StiTextHorAlignment alignment, bool rightToLeft)
        {
            int num = 0;
            string str = @"\ql" + ((num != 0) ? string.Format(@"\ri{0}", num) : string.Empty);
            if (((alignment == StiTextHorAlignment.Left) && rightToLeft)
                || ((alignment == StiTextHorAlignment.Right) && !rightToLeft))
                str = @"\qr" + ((num != 0) ? string.Format(@"\li{0}", num) : string.Empty);
            if (alignment == StiTextHorAlignment.Center)
                str = @"\qc" + ((num != 0) ? string.Format(@"\ri{0}", num) : string.Empty);
            if (alignment == StiTextHorAlignment.Width)
                str = @"\qj";
            return str;
        }

        private void RenderBorder1(StiComponent component)
        {
            var border = component as IStiBorder;
            if ((border != null) && !(component is IStiIgnoreBorderWhenExport))
            {
                if (border.Border is StiAdvancedBorder)
                {
                    var border2 = border.Border as StiAdvancedBorder;
                    if (border2.IsLeftBorderSidePresent && (border2.LeftSide.Color.A != 0))
                        _sw.Write(@"\brdrl" + this.GetLineStyle(border2.LeftSide, _colorList));
                    if (border2.IsRightBorderSidePresent && (border2.RightSide.Color.A != 0))
                        _sw.Write(@"\brdrr" + this.GetLineStyle(border2.RightSide, _colorList));
                    if (border2.IsTopBorderSidePresent && (border2.TopSide.Color.A != 0))
                        _sw.Write(@"\brdrt" + this.GetLineStyle(border2.TopSide, _colorList));
                    if (border2.IsBottomBorderSidePresent && (border2.BottomSide.Color.A != 0))
                        _sw.Write(@"\brdrb" + this.GetLineStyle(border2.BottomSide, _colorList));
                }
                else if (border.Border.Color.A != 0)
                {
                    var lineStyle =
                        this.GetLineStyle(
                            new StiBorderSide(border.Border.Color, border.Border.Size, border.Border.Style), _colorList);
                    if ((border.Border.Side & StiBorderSides.Left) > StiBorderSides.None)
                        _sw.Write(@"\brdrl" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Right) > StiBorderSides.None)
                        _sw.Write(@"\brdrr" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Top) > StiBorderSides.None)
                        _sw.Write(@"\brdrt" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Bottom) > StiBorderSides.None)
                        _sw.Write(@"\brdrb" + lineStyle);
                }
            }
        }

        private void RenderBorder2(StiComponent component)
        {
            var border = component as IStiBorder;
            if ((border != null) && !(component is IStiIgnoreBorderWhenExport))
            {
                if (border.Border is StiAdvancedBorder)
                {
                    var border2 = border.Border as StiAdvancedBorder;
                    if (border2.IsLeftBorderSidePresent && (border2.LeftSide.Color.A != 0))
                        _sw.Write(@"\clbrdrl" + this.GetLineStyle(border2.LeftSide, _colorList));
                    if (border2.IsRightBorderSidePresent && (border2.RightSide.Color.A != 0))
                        _sw.Write(@"\clbrdrr" + this.GetLineStyle(border2.RightSide, _colorList));
                    if (border2.IsTopBorderSidePresent && (border2.TopSide.Color.A != 0))
                        _sw.Write(@"\clbrdrt" + this.GetLineStyle(border2.TopSide, _colorList));
                    if (border2.IsBottomBorderSidePresent && (border2.BottomSide.Color.A != 0))
                        _sw.Write(@"\clbrdrb" + this.GetLineStyle(border2.BottomSide, _colorList));
                }
                else if (border.Border.Color.A != 0)
                {
                    var lineStyle =
                        this.GetLineStyle(
                            new StiBorderSide(border.Border.Color, border.Border.Size, border.Border.Style), _colorList);
                    if ((border.Border.Side & StiBorderSides.Left) > StiBorderSides.None)
                        _sw.Write(@"\clbrdrl" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Right) > StiBorderSides.None)
                        _sw.Write(@"\clbrdrr" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Top) > StiBorderSides.None)
                        _sw.Write(@"\clbrdrt" + lineStyle);
                    if ((border.Border.Side & StiBorderSides.Bottom) > StiBorderSides.None)
                        _sw.Write(@"\clbrdrb" + lineStyle);
                }
            }
        }

        private void RenderBrush1(StiComponent component)
        {
            var brush = component as IStiBrush;
            if ((brush == null) || component.IsExportAsImage(StiExportFormat.Rtf))
                return;
            Color incomingColor = StiBrush.ToColor(brush.Brush);
            if (incomingColor.A != 0)
            {
                string colorNumber = GetColorNumber(_colorList, incomingColor);
                _sw.Write(@"\cbpat{0}", colorNumber);
            }
        }

        private void RenderBrush2(StiComponent component)
        {
            var brush = component as IStiBrush;
            if ((brush != null) && !component.IsExportAsImage(StiExportFormat.Rtf))
            {
                Color color = StiBrush.ToColor(brush.Brush);
                _sw.Write(@"{\sp{\sn fillColor}{\sv ");
                _sw.Write("{0}", ((color.B * 0x10000) + (color.G * 0x100)) + color.R);
                _sw.Write("}}");
                _sw.Write(color.A != 0 ? @"{\sp{\sn fFilled}{\sv 1}} " : @"{\sp{\sn fFilled}{\sv 0}} ");
            }
        }

        private void RenderComponent(StiRtfData byIndex)
        {
            StiComponent component = byIndex.Component;

            var stiText = component as StiText;
            var stiRichText = component as StiRichText;
            var stiShape = component as StiShape;
            if ((stiText == null && stiRichText == null && stiShape == null) || !component.IsEnabled
                || byIndex.Height == 0)
                return;

            _sw.WriteLine();
            int correctX = 0;
            int correctW = 0;
            if (component.TagValue == "frame" || component.TagValue == "frameEndPar" || component.TagValue == "frameEndPar0")
            {
                _sw.Write(@"{{\posx{0}", byIndex.X);
                _sw.Write("\\absh-{0}", byIndex.Height);
                var border3 = component as IStiBorder;
                if ((border3 != null) && !(component is IStiIgnoreBorderWhenExport))
                {
                    if ((border3.Border.Side & StiBorderSides.Left) > StiBorderSides.None)
                    {
                        correctX += 0x26;
                        correctW += 0x26;
                    }

                    if ((border3.Border.Side & StiBorderSides.Right) > StiBorderSides.None)
                        correctW += 0x26;
                }

                RenderBorder1(component);
            }

            // _sw.Write(@"{{\posx{0}", byIndex.X <= 1 ? byIndex.X : byIndex.X + _pageLeftMargins);
            _sw.Write(@"{\rtlch\fcs1 \af0 \ltrch\fcs0");

            if (byIndex.X != 0 && (component.TagValue != "frame" && component.TagValue != "frameEndPar" && component.TagValue != "frameEndPar0"))
                _sw.Write(@"\li{0}\lin{0}", byIndex.X + correctX);
            if (component.TagValue == "frame" || component.TagValue == "frameEndPar" || component.TagValue == "frameEndPar0")
            {
                _sw.Write(
                    @"\ri{0}\rin0\absw{2}", 
                    _pageWidth - byIndex.Width - byIndex.X, 
                    _pageWidthWithMargins - byIndex.Width - byIndex.X + _pageLeftMargins, 
                    byIndex.Width - correctW);
            }
            else if (_pageWidthWithMargins - byIndex.Width - byIndex.X != 0)
                _sw.Write(@"\ri{0}\rin{0}", _pageWidthWithMargins - byIndex.Width - byIndex.X);

            RenderTextAngle1(component);
            RenderBrush1(component);
            RenderStyle12(component);
            RenderHorAlign12(component);
            RenderTextBrush12(component);
            RenderTextFont12(component);

            if (stiText != null)
                RenderText12(stiText);
            if (stiRichText != null)
                RenderRtf12(stiRichText);
            else
            {
                if (component is StiShape)
                {
                    RenderShape2(byIndex);

                    // if (component.TagValue == "frame" || component.TagValue == "frameEndPar")
                    // RenderShape1(byIndex, correctX, correctW);
                }

                /* else
                {
                    RenderImage12(component, byIndex.Width - correctW, byIndex.Height);
                }*/
            }

            _sw.Write("}");
            if (component.TagValue == "frame")
                _sw.Write(@"}");
            if (component.TagValue == "frameEndPar")
                _sw.Write(@"}\par ");
            if (component.TagValue == "frameEndPar0")
                _sw.Write(@"}{\rtlch\fcs1 \ab0\af0\afs2 \par }");
        }

        private void RenderEndDoc()
        {
            _sw.WriteLine(string.Empty);
            _sw.WriteLine("}");
        }

        private void RenderHorAlign12(StiComponent component)
        {
            var alignment = component as IStiTextHorAlignment;
            if (alignment != null)
            {
                var options = (IStiTextOptions)component;
                string str = MakeHorAlignString(alignment.HorAlignment, options.TextOptions.RightToLeft);
                _sw.Write("{0}", str);
            }
        }

        private void RenderImage12(StiComponent component, int absw, int absh)
        {
            var exportImage = component as IStiExportImage;
            if (exportImage != null)
            {
                float zoom = _imageResolution;

                var exportImageExtended = exportImage as IStiExportImageExtended;

                Image image = null;

                if (component.IsExportAsImage(StiExportFormat.Rtf))
                {
                    if (exportImageExtended != null && exportImageExtended.IsExportAsImage(StiExportFormat.Rtf))
                        image = exportImageExtended.GetImage(ref zoom, StiExportFormat.Rtf);
                    else
                        image = exportImage.GetImage(ref zoom);
                }

                if (image != null)
                {
                    _sw.Write(GetImageString(image, zoom, absw, absh));
                    image.Dispose();
                }
            }
        }

        private void RenderPageFooter()
        {
            // _sw.WriteLine(@"\sect");
            
        }

        private void RenderPageHeader(StiPage page, StiPage reportFirstPage)
        {
            _pageHeight =
                (int)Math.Round(14.4 * page.Unit.ConvertToHInches((page.PageHeight * page.SegmentPerHeight)));
            _pageWidth = (int)Math.Round(14.4 * page.Unit.ConvertToHInches((page.PageWidth * page.SegmentPerWidth)));
            var num = (int)Math.Round(14.4 * page.Unit.ConvertToHInches(page.Margins.Left));
            var num2 = (int)Math.Round(14.4 * page.Unit.ConvertToHInches(page.Margins.Right));
            var num3 = (int)Math.Round(14.4 * page.Unit.ConvertToHInches(page.Margins.Top));
            int num4 = ((int)Math.Round(14.4 * page.Unit.ConvertToHInches(page.Margins.Bottom))) - 0x2a;
            if (num4 < 0)
                num4 = 0;
            if (_pageWidth > 0x7b0c)
                _pageWidth = 0x7b0c;
            if (_pageHeight > 0x7b0c)
                _pageHeight = 0x7b0c;

            _sw.Write(@"\sectd");
            if (page.Orientation == StiPageOrientation.Landscape)
                _sw.Write(@"\lndscpsxn");
            _sw.Write(@"\paperw{0}\paperh{1}", _pageWidth, _pageHeight);
            
            // Другой колонтитул для первой страницы \titlepg для нумерации страниц без первой страницы
            var otherColontitulForFirstPage = reportFirstPage.TagValue == "pagenumber-skipfirst" ? "\\titlepg" : "";
            
            _sw.WriteLine(
                @"\margl{0}\margr{1}\margt{2}\margb{3}\headery{2}\footery{3}{4}", 
                new object[] { num, num2, num3, num4, otherColontitulForFirstPage });
            _pageLeftMargins = num;
            _pageWidthWithMargins = _pageWidth - num - num2;
            
            if ((string) reportFirstPage.TagValue == "pagenumber-skipfirst" || (string) reportFirstPage.TagValue == "pagenumber")
            {
                _sw.WriteLine(
                    @"{\headerr \ltrpar \pard\plain \ltrpar\s22\qc \li0\ri0\sa160\sl259\slmult1\widctlpar\tqc\tx4677\tqr\tx9355\wrapdefault\aspalpha\aspnum\faauto\adjustright\rin0\lin0\itap0\pararsid5205865 \rtlch\fcs1 \af0\afs22\alang1025 \ltrch\fcs0 
\f37\fs22\lang1049\langfe1049\cgrid\langnp1049\langfenp1049 {\field{\*\fldinst {\rtlch\fcs1 \af0 \ltrch\fcs0 \f0\fs28\insrsid5205865\charrsid4980740 PAGE   \\
* MERGEFORMAT}}{\fldrslt {\rtlch\fcs1 \af0 \ltrch\fcs0 \f0\fs28\lang1024\langfe1024\noproof\insrsid14840001 2}}}\sectd \ltrsect\linex0\endnhere\sectdefaultcl\sftnbj {\rtlch\fcs1 \af0 \ltrch\fcs0 \f0\fs28\lang1033\langfe1049\langnp1033\insrsid5205865\charrsid15432033 
\par }}");
            }
        }

        private static Regex _emptyRTF = new Regex(
            @"^(\\ltrpar)?\\qj(\\lang\d+)?\\f\d\\fs28(\\tab)?(\\b)?((\\f\d)?\\par[\r\n]+\\pard\\lang\d+(\\b\d)?\\f\d\\fs17)?$",
            RegexOptions.Compiled);

        private void RenderRtf12(StiRichText component)
        {
            // string str = GetRtfString(component).Replace(@"\pard", @"\par");
            // _sw.Write(str);
            StringBuilder sb = GetRtfString(component);
            string str = sb.ToString();
            
            // если пустое поле, то пропускаем его
            if (_emptyRTF.IsMatch(str))
                return;

            int pradIndex = str.LastIndexOf(@"\prad");
            string margingTop = @"\sb" + (int)Math.Round(component.Margins.Top * 14.4) + " ";
            string margingBottom = @"\sa" + (int)Math.Round(component.Margins.Bottom * 16) + " ";

            // if (component.Margins.Top > 0)
            // sb.Insert(3, margingTop);
            int index = str.IndexOf(@"\par");
            if (index > -1)
            {
                sb.Insert(index++, "{");
                if (component.Margins.Bottom > 0)
                {
                    sb.Insert(index, margingBottom);
                    index += margingBottom.Length;
                }

                if (component.Margins.Top > 0)
                {
                    sb.Insert(index, margingTop);
                    index += margingTop.Length;
                }

                sb[index + 4] = '}';
            }
            else
            {
                sb.Append("{");
                if (component.Margins.Top > 0)
                    sb.Append(margingTop);
                if (component.Margins.Bottom > 0)
                    sb.Append(margingBottom);
                sb.Append(@"\par}");
            }

            _sw.Write(pradIndex > -1 ? sb.ToString(0, index) : sb.ToString());
        }

        private void RenderShape1(StiRtfData pp, int correctX, int correctW)
        {
            StiComponent component = pp.Component;
            var shape = component as StiShape;
            if (shape == null)
                return;
            if (CheckShape1(shape))
            {
                

                var mBrush = component as IStiBrush;

                #region Fillcolor

                string stBrush = string.Empty;
                Color tempColor = Color.Transparent;
                if (mBrush != null)
                    tempColor = StiBrush.ToColor(mBrush.Brush);
                if (tempColor.A != 0)
                {
                    string tempColorSt = GetColorNumber(_colorList, tempColor);
                    stBrush = string.Format("\\cbpat{0}", tempColorSt);
                }

                #endregion

                #region stroke color

                string stBorder = string.Empty;
                Color tempColor2 = shape.BorderColor;
                if (tempColor2.A != 0)
                {
                    string tempColor2St = GetColorNumber(_colorList, tempColor2);
                    stBorder = string.Format("\\brdrcf{0}", tempColor2St);
                }

                #endregion

                var borderWidth = (int)(shape.Size * 14);
                string stBorderWidth = string.Format("\\brdrs\\brdrw{0}", borderWidth);

                _sw.Write("\\fs1");

                #region VerticalLine

                if (shape.ShapeType is StiVerticalLineShapeType)
                {
                    if (tempColor.A != 0)
                        _sw.Write(stBrush);
                    if (tempColor2.A != 0)
                    {
                        _sw.WriteLine(" \\par}");
                        _sw.Write("{");
                        _sw.Write(
                            "\\nowrap\\posx{0}\\posy{1}\\absw{2}\\absh{3}", 
                            pp.X + correctX, 
                            pp.Y, 
                            pp.Width - correctW, 
                            (pp.Height + borderWidth) / 2);
                        _sw.Write("\\brdrr" + stBorderWidth + stBorder + "\\fs1");
                    }
                }

                #endregion

                #region HorizontalLine

                if (shape.ShapeType is StiHorizontalLineShapeType)
                {
                    if (tempColor.A != 0)
                        _sw.Write(stBrush);
                    if (tempColor2.A != 0)
                    {
                        _sw.WriteLine(" \\par}");
                        _sw.Write("{");
                        _sw.Write(
                            "\\nowrap\\posx{0}\\posy{1}\\absw{2}\\absh{3}", 
                            pp.X + correctX, 
                            pp.Y, 
                            pp.Width - correctW, 
                            (pp.Height + borderWidth) / 2);
                        _sw.Write("\\brdrb" + stBorderWidth + stBorder + "\\fs1");
                    }
                }

                #endregion

                #region TopAndBottomLine

                if (shape.ShapeType is StiTopAndBottomLineShapeType)
                {
                    if (tempColor.A != 0)
                        _sw.Write(stBrush);
                    _sw.Write("\\brdrt" + stBorderWidth + stBorder);
                    _sw.Write("\\brdrb" + stBorderWidth + stBorder);
                    _sw.WriteLine("\\fs1");
                }

                #endregion

                #region LeftAndRightLine

                if (shape.ShapeType is StiLeftAndRightLineShapeType)
                {
                    if (tempColor.A != 0)
                        _sw.Write(stBrush);
                    _sw.Write("\\brdrl" + stBorderWidth + stBorder);
                    _sw.Write("\\brdrr" + stBorderWidth + stBorder);
                    _sw.WriteLine("\\fs1");
                }

                #endregion

                #region Rectangle

                if (shape.ShapeType is StiRectangleShapeType)
                {
                    if (tempColor.A != 0)
                        _sw.Write(stBrush);
                    _sw.Write("\\box" + stBorderWidth + stBorder);
                    _sw.WriteLine("\\fs1");
                }

                #endregion

                // 					_sw.WriteLine(" \\par}");
                
            }
            else
                RenderImage12(component, pp.Width, pp.Height);
        }

        private void RenderShape2(StiRtfData pp)
        {
            StiComponent component = pp.Component;
            var shape = component as StiShape;

            if (shape == null)
                return;

            if (CheckShape2(shape))
            {
                var mBrush = component as IStiBrush;

                // Fillcolor
                Color tempColor = Color.Transparent;
                if (mBrush != null)
                    tempColor = StiBrush.ToColor(mBrush.Brush);

                // stroke color
                Color tempColor2 = shape.BorderColor;

                var borderWidth = (int)(shape.Size * 8400);
                string stBorderWidth = string.Format("{0}", borderWidth);

                #region VerticalLine

                if (shape.ShapeType is StiVerticalLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(
                        pp.X + pp.Width / 2, pp.Y, pp.X + pp.Width / 2, pp.Y + pp.Height, tempColor2, stBorderWidth);
                }

                #endregion

                #region HorizontalLine

                if (shape.ShapeType is StiHorizontalLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(
                        pp.X, pp.Y + pp.Height / 2, pp.X + pp.Width, pp.Y + pp.Height / 2, tempColor2, stBorderWidth);
                }

                #endregion

                #region TopAndBottomLine

                if (shape.ShapeType is StiTopAndBottomLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(pp.X, pp.Y, pp.X + pp.Width, pp.Y, tempColor2, stBorderWidth);
                    DrawLine(pp.X, pp.Y + pp.Height, pp.X + pp.Width, pp.Y + pp.Height, tempColor2, stBorderWidth);
                }

                #endregion

                #region LeftAndRightLine

                if (shape.ShapeType is StiLeftAndRightLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(pp.X, pp.Y, pp.X, pp.Y + pp.Height, tempColor2, stBorderWidth);
                    DrawLine(pp.X + pp.Width, pp.Y, pp.X + pp.Width, pp.Y + pp.Height, tempColor2, stBorderWidth);
                }

                #endregion

                #region Rectangle

                if (shape.ShapeType is StiRectangleShapeType)
                {
                    _sw.Write("{\\shp{\\*");
                    _sw.Write(
                        "\\shpinst\\shpleft{0}\\shptop{1}\\shpright{2}\\shpbottom{3}", 
                        pp.X, 
                        pp.Y, 
                        pp.X + pp.Width, 
                        pp.Y + pp.Height);
                    _sw.Write("\\shpwr3");
                    _sw.Write("{\\sp{\\sn shapeType}{\\sv 1}}");
                    _sw.Write("{\\sp{\\sn fFlipH}{\\sv 0}}");
                    _sw.Write("{\\sp{\\sn fFlipV}{\\sv 0}}");
                    if (tempColor.A != 0)
                    {
                        _sw.Write(
                            "{\\sp{\\sn fillColor}{\\sv "
                            + string.Format("{0}", tempColor.B * 65536 + tempColor.G * 256 + tempColor.R) + "}}");
                        _sw.Write("{\\sp{\\sn fFilled}{\\sv 1}}");
                    }
                    else
                        _sw.Write("{\\sp{\\sn fFilled}{\\sv 0}}");
                    if (tempColor2.A != 0)
                    {
                        _sw.Write(
                            "{\\sp{\\sn lineColor}{\\sv "
                            + string.Format("{0}", tempColor2.B * 65536 + tempColor2.G * 256 + tempColor2.R) + "}}");
                        _sw.Write("{\\sp{\\sn lineWidth}{\\sv " + stBorderWidth + "}}");
                        _sw.Write("{\\sp{\\sn fLine}{\\sv 1}}");
                    }
                    else
                        _sw.Write("{\\sp{\\sn fLine}{\\sv 0}}");
                    _sw.WriteLine("}}");
                }

                #endregion

                #region Oval

                if (shape.ShapeType is StiOvalShapeType)
                {
                    _sw.Write("{\\shp{\\*");
                    _sw.Write(
                        "\\shpinst\\shpleft{0}\\shptop{1}\\shpright{2}\\shpbottom{3}", 
                        pp.X, 
                        pp.Y, 
                        pp.X + pp.Width, 
                        pp.Y + pp.Height);
                    _sw.Write("\\shpwr3");
                    _sw.Write("{\\sp{\\sn shapeType}{\\sv 3}}");
                    _sw.Write("{\\sp{\\sn fFlipH}{\\sv 0}}");
                    _sw.Write("{\\sp{\\sn fFlipV}{\\sv 0}}");
                    if (tempColor.A != 0)
                    {
                        _sw.Write(
                            "{\\sp{\\sn fillColor}{\\sv "
                            + string.Format("{0}", tempColor.B * 65536 + tempColor.G * 256 + tempColor.R) + "}}");
                        _sw.Write("{\\sp{\\sn fFilled}{\\sv 1}}");
                    }
                    else
                        _sw.Write("{\\sp{\\sn fFilled}{\\sv 0}}");
                    if (tempColor2.A != 0)
                    {
                        _sw.Write(
                            "{\\sp{\\sn lineColor}{\\sv "
                            + string.Format("{0}", tempColor2.B * 65536 + tempColor2.G * 256 + tempColor2.R) + "}}");
                        _sw.Write("{\\sp{\\sn lineWidth}{\\sv " + stBorderWidth + "}}");
                        _sw.Write("{\\sp{\\sn fLine}{\\sv 1}}");
                    }
                    else
                        _sw.Write("{\\sp{\\sn fLine}{\\sv 0}}");
                    _sw.WriteLine("}}");
                }

                #endregion

                #region DiagonalDownLine

                if (shape.ShapeType is StiDiagonalDownLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(pp.X, pp.Y, pp.X + pp.Width, pp.Y + pp.Height, tempColor2, stBorderWidth);
                }

                #endregion

                #region DiagonalUpLine

                if (shape.ShapeType is StiDiagonalUpLineShapeType)
                {
                    FillRect(pp, tempColor);
                    DrawLine(pp.X, pp.Y + pp.Height, pp.X + pp.Width, pp.Y, tempColor2, stBorderWidth);
                }

                #endregion
            }
            else
                RenderImage12(component, pp.Width, pp.Height);
        }

        private void RenderStartDoc()
        {
            _sw.Write(@"{\rtf1");
            _sw.WriteLine(@"\ansi\ansicpg{0}", 0x4e4);
            _sw.WriteLine(string.Empty);
            _sw.WriteLine(@"{\fonttbl");
            for (int i = 0; i < _fontList.Count; i++)
            {
                var font = (Font)_fontList[i];
                for (int k = 0; k < _charsetCount; k++)
                {
                    int num3 = StiEncode.codePagesTable[_fontToCodePages[k], 2];
                    _sw.Write("{");
                    _sw.Write(@"\f{0}\fcharset{1} {2};", (i * _charsetCount) + k, (num3 == 1) ? 204 : num3, font.Name);
                    _sw.WriteLine("}");
                }
            }

            _sw.WriteLine("}");
            _sw.WriteLine(string.Empty);
            _sw.WriteLine(@"{\colortbl");
            for (int j = 0; j < _colorList.Count; j++)
            {
                var color = (Color)_colorList[j];
                if (color == Color.Transparent)
                    _sw.WriteLine(";");
                else
                    _sw.WriteLine(@"\red{0}\green{1}\blue{2};", color.R, color.G, color.B);
            }

            _sw.WriteLine("}");
            _sw.WriteLine(string.Empty);
            if (_useStyles)
            {
                _sw.WriteLine(@"{\stylesheet");
                for (int m = 0; m < _styleList.Count; m++)
                {
                    _sw.Write("{");
                    var info = (StiRtfStyleInfo)_styleList[m];
                    _sw.Write(@"\s{0}\sbasedon0\snext{0} ", m + 1);
                    if (info.FontNumber != -1)
                    {
                        _sw.Write(@"\f{0}", info.FontNumber);
                        _sw.Write(@"\fs{0}", info.FontSize);
                        if (info.Bold)
                            _sw.Write(@"\b");
                        if (info.Italic)
                            _sw.Write(@"\i");
                        if (info.Underline)
                            _sw.Write(@"\ul");
                    }

                    if (info.TextColor != -1)
                        _sw.Write(@"\cf{0}", info.TextColor);
                    _sw.Write(MakeHorAlignString(info.Alignment, info.RightToLeft));
                    if (info.RightToLeft)
                        _sw.Write(@"\rtlpar");
                    else
                        _sw.Write(@"\ltrpar");
                    _sw.Write(" {0};", info.Name);
                    _sw.WriteLine("}");
                }

                _sw.WriteLine("}");
                _sw.WriteLine(string.Empty);
            }

            _sw.Write(@"\viewkind1");
        }

        private void RenderStyle12(StiComponent component)
        {
            if (_useStyles)
            {
                int rtfStyleFromComponent = GetRtfStyleFromComponent(component);
                _sw.Write(@"\s{0}", rtfStyleFromComponent);
            }
        }

        private void RenderText12(StiText text)
        {
            var options = text as IStiTextOptions;
            if ((text != null) && !text.IsExportAsImage(StiExportFormat.Rtf) && text.Text.Value != null
                && !string.IsNullOrEmpty(text.Text.Value.Trim(' ', '\t')))
            {
                var stInput = new StringBuilder(text.Text.Value);
                bool useRightToLeft = ((options != null) && (options.TextOptions != null))
                                      && options.TextOptions.RightToLeft;
                StringBuilder builder2 = UnicodeToRtfString(stInput, useRightToLeft);
                int index = text.Text.Value.IndexOf('\n');
                if (index > -1)
                {
                    if (text.Margins.Top > 0)
                    {
                        builder2.Insert(4, @"{\sb" + (int)Math.Round(text.Margins.Top * 14.4) + " ");

                        // builder2.Insert(4, @"{\sb" + (int)Math.Round((14.4 * text.Page.Unit.ConvertToHInches(text.Margins.Top))) + " ");
                        index = builder2.ToString().IndexOf(@"\par");
                        builder2.Replace(@"\par ", @"\par}", index, 5);
                    }
                }

                builder2.Append(@"{");
                if (text.Margins.Top > 0 && index == -1)
                    builder2.Insert(0, @"\sb" + (int)Math.Round(text.Margins.Top * 14.4) + " ");
                if (text.Margins.Bottom > 0)
                    builder2.Insert(0, @"\sa" + (int)Math.Round(text.Margins.Bottom * 14.4) + " ");

                builder2.Append(@" \par}");

                // builder2.Replace(@"\par ", @"\par");
                var buffer = new byte[builder2.Length];
                for (int i = 0; i < builder2.Length; i++)
                {
                    buffer[i] = (byte)builder2[i];
                }

                _sw.Flush();
                _sw2.Write(buffer, 0, buffer.Length);
            }
        }

        private void RenderTextAngle1(StiComponent component)
        {
            var options = component as IStiTextOptions;
            if (options != null)
            {
                float angle = options.TextOptions.Angle;
                int num2 = 0;
                if ((angle > 45f) && (angle < 135f))
                    num2 = 1;
                if ((angle > 225f) && (angle < 315f))
                    num2 = 2;
                switch (num2)
                {
                    case 1:
                        _sw.Write(@"\frmtxbtlr");
                        return;

                    case 2:
                        _sw.Write(@"\frmtxtbrl");
                        return;
                }

                _sw.Write(@"\frmtxlrtb");
            }
        }

        private void RenderTextAngle2(StiComponent component)
        {
            var options = component as IStiTextOptions;
            if (options != null)
            {
                float angle = options.TextOptions.Angle;
                int num2 = 0;
                if ((angle > 45f) && (angle < 135f))
                    num2 = 1;
                if ((angle > 225f) && (angle < 315f))
                    num2 = 2;
                switch (num2)
                {
                    case 1:
                        _sw.Write(@"\cltxbtlr");
                        return;

                    case 2:
                        _sw.Write(@"\cltxtbrl");
                        return;
                }

                _sw.Write(@"\cltxlrtb");
            }
        }

        private void RenderTextBrush12(StiComponent component)
        {
            var brush = component as IStiTextBrush;
            if (brush != null)
            {
                string colorNumber = GetColorNumber(_colorList, StiBrush.ToColor(brush.TextBrush));
                _sw.Write(@"\cf{0}", colorNumber);
            }
        }

        private void RenderTextFont12(StiComponent component)
        {
            var font = component as IStiFont;
            if (font != null)
            {
                _baseFontNumber = GetFontNumber(_fontList, font.Font);
                _sw.Write(@"\f{0}", _baseFontNumber);
                _sw.Write(@"\fs{0}", Math.Round(font.Font.SizeInPoints * 2f));
                if (font.Font.Bold)
                    _sw.Write(@"\b");
                if (font.Font.Italic)
                    _sw.Write(@"\i");
                if (font.Font.Underline)
                    _sw.Write(@"\ul");
                _sw.Write(string.Format(@"\sl{0}\slmult1", Math.Round(240.0 * StiOptions.Export.Rtf.LineSpacing)));
                if (StiOptions.Export.Rtf.SpaceBetweenCharacters != 0)
                    _sw.Write(string.Format(@"\expndtw{0}", StiOptions.Export.Rtf.SpaceBetweenCharacters));
            }
        }

        private void RenderVerAlign2(StiComponent component)
        {
            var alignment = component as IStiVertAlignment;
            if (alignment != null)
            {
                if (alignment.VertAlignment == StiVertAlignment.Top)
                    _sw.Write(@"\clvertalt");
                if (alignment.VertAlignment == StiVertAlignment.Center)
                    _sw.Write(@"\clvertalc");
                if (alignment.VertAlignment == StiVertAlignment.Bottom)
                    _sw.Write(@"\clvertalb");
            }
        }

        private StringBuilder UnicodeToRtfString(StringBuilder stInput, bool useRightToLeft)
        {
            if (useRightToLeft)
                stInput = CheckArabic(stInput);
            stInput.Replace(@"\", @"\\");
            stInput.Replace("{", @"\{");
            stInput.Replace("}", @"\}");
            stInput.Replace("\n", @"\par ");
            stInput.Replace("\x0017", @"\");
            stInput.Replace("\x0018", @"{\field{\*\fldinst {PAGE}}}");
            stInput.Replace("\x0019", @"{\field{\*\fldinst {NUMPAGES}}}");
            stInput.Replace("\x001a", @"{\field{\*\fldinst {");
            stInput.Replace("\x001b", @" }}{\fldrslt {");
            stInput.Replace("\x001c", @"{\*\bkmkstart ");
            stInput.Replace("\x001d", @"}{\*\bkmkend ");
            stInput.Replace("\x001e", "}");
            string str = stInput.ToString();
            string[] strArray = null;
            if (str.IndexOf('\x0010') == -1)
                strArray = new[] { str };
            else
                strArray = str.Split(new[] { '\x0010' });
            var builder = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                string str2 = strArray[i];
                if (i != 0)
                {
                    _baseFontNumber = str2[0] - 'Ā';
                    if (_baseFontNumber < 0)
                        _baseFontNumber = 0;
                    str2 = str2.Substring(2);
                }

                int num2 = 0;
                while (num2 < str2.Length)
                {
                    int num3 = num2;
                    var sb = new StringBuilder();
                    int codepage = 1;
                    while ((num2 < str2.Length) && (StiEncode.unicodeToCodePageArray[str2[num2]] == 0))
                    {
                        sb.Append(str2[num2]);
                        num2++;
                    }

                    if (num2 < str2.Length)
                    {
                        codepage = StiEncode.unicodeToCodePageArray[str2[num2]];
                        sb.Append(str2[num2]);
                        num2++;
                        while ((num2 < str2.Length)
                               &&
                               ((StiEncode.unicodeToCodePageArray[str2[num2]] == 0)
                                || (StiEncode.unicodeToCodePageArray[str2[num2]] == codepage)))
                        {
                            sb.Append(str2[num2]);
                            num2++;
                        }
                    }

                    StringBuilder builder3 = StiEncode.Encode(sb, codepage);
                    builder.Append(@"\f" + (_baseFontNumber + _codePageToFont[codepage]) + " ");
                    for (int j = 0; j < builder3.Length; j++)
                    {
                        int index = str2[num3 + j];
                        if ((builder3[j] == '?') && (StiEncode.unicodeToCodePageArray[index] < 2))
                            builder.Append(@"\u" + index + "?");
                        else
                            builder.Append(builder3[j]);
                    }
                }
            }

            return builder;
        }

        private string deleteToken(string inputString, string token)
        {
            int index;
            do
            {
                index = inputString.IndexOf(token);
                if (index > -1)
                {
                    int num2 = index + token.Length;
                    while (num2 < inputString.Length && char.IsDigit(inputString[num2]))
                    {
                        num2++;
                    }

                    inputString = inputString.Remove(index, num2 - index);
                }
            }
            while (index != -1);
            return inputString;
        }

        #endregion

        // Properties

        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        private struct StiRtfData
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public StiComponent Component;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StiRtfStyleInfo
        {
            public string Name;
            public StiTextHorAlignment Alignment;
            public bool RightToLeft;
            public int FontNumber;
            public int FontSize;
            public bool Bold;
            public bool Italic;
            public bool Underline;
            public int TextColor;
        }
    }
}