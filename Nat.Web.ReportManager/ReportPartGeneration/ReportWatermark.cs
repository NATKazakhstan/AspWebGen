using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Nat.ExportInExcel;
using Nat.Web.Tools.Report;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using XDrawing = DocumentFormat.OpenXml.Spreadsheet.Drawing;
using WHeader = DocumentFormat.OpenXml.Wordprocessing.Header;
using WParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WPicture = DocumentFormat.OpenXml.Wordprocessing.Picture;
using Vml = DocumentFormat.OpenXml.Vml;
using System.Xml.Linq;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using DocumentFormat.OpenXml.Office2010.Word;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.Objects;
using TelerikRgbColor = Telerik.Windows.Documents.Fixed.Model.ColorSpaces.RgbColor;
using System.Windows;

namespace Nat.Web.ReportManager.ReportPartGeneration
{
    public class ReportWatermark : IReportWatermark
    {
        private static string _pluginFullName = string.Empty;
        public void AddToExcelStream( Stream stream, string pluginFullName )
        {
            try
            {
                var rWatermark = DependencyResolver.Current.GetService<IWatermark>();
                var watermarkImage = rWatermark.GetImage( pluginFullName );
                if (watermarkImage == null)
                    return;

                _pluginFullName = pluginFullName;
                using (var doc = SpreadsheetDocument.Open( stream, true, new OpenSettings() ))
                {
                    var sheetPart = doc.WorkbookPart.WorksheetParts.First();

                    AddSheetView( sheetPart );
                    AddPrinterSettings( sheetPart );

                    var dwg = sheetPart.Worksheet.Elements<XDrawing>().FirstOrDefault();
                    if (dwg != null)
                    {
                        sheetPart.Worksheet.RemoveChild( dwg );
                    }

                    AddHeader( sheetPart );
                    AddWatermarkDrawing( sheetPart, watermarkImage, dwg );
                }
            }
            finally
            {
                stream.Position = 0;
            }
        }

        #region excel

        private void AddWatermarkDrawing( WorksheetPart sheetPart, byte[] watermarkImage, XDrawing dwg )
        {
            var vmlDwgPart = sheetPart.VmlDrawingParts.FirstOrDefault();
            if (vmlDwgPart == null)
            {
                vmlDwgPart = sheetPart.AddNewPart<VmlDrawingPart>();
                WriteVmlDrwPart( vmlDwgPart );
            }

            ImagePart imagePart = vmlDwgPart.AddImagePart( ImagePartType.Png );
            using (var imgStream = new MemoryStream( watermarkImage ))
            {
                imagePart.FeedData( imgStream );
            }

            string imgPrtId = imagePart == null ? string.Empty : vmlDwgPart.GetIdOfPart( imagePart );
            FIllVmlDwgPart( vmlDwgPart, imgPrtId );

            SetPostElements( sheetPart, dwg, vmlDwgPart );
        }

        private static void SetPostElements( WorksheetPart sheetPart, XDrawing dwg, VmlDrawingPart vmlDwgPart )
        {
            var ldhf = sheetPart.Worksheet.Elements<LegacyDrawingHeaderFooter>().FirstOrDefault();

            if (ldhf == null)
            {
                ldhf = new LegacyDrawingHeaderFooter();
            }
            else
            {
                sheetPart.Worksheet.RemoveChild( ldhf );
            }

            ldhf.Id = sheetPart.GetIdOfPart( vmlDwgPart );
            if (dwg != null)
            {
                sheetPart.Worksheet.Append( dwg );
            }

            sheetPart.Worksheet.Append( ldhf );
        }

        private void FIllVmlDwgPart( VmlDrawingPart drawingPart, string imgPartId )
        {
            using (var tempStream = drawingPart.GetStream( FileMode.Open, FileAccess.ReadWrite ))
            {
                XDocument xd = XDocument.Load( tempStream );

                var root = xd.Root;
                var nsV = root.GetNamespaceOfPrefix( "v" );
                var nsO = root.GetNamespaceOfPrefix( "o" );
                AddShapeIntoRoot( imgPartId, root, nsV, nsO );
                tempStream.Position = 0;

                using (var xw = XmlWriter.Create( tempStream, GetXmlWriterSettings() ))
                {
                    xd.Save( xw );
                    xw.Flush();
                }
            }
        }

        private static void AddShapeIntoRoot( string imgPartId, XElement root, XNamespace nsV, XNamespace nsO )
        {
            root.Add(
                new XElement( nsV + "shape",
                    new XAttribute( "id", "CH" ),
                    new XAttribute( nsO + "spid", "_x0000_s1025" ),
                    new XAttribute( "type", "#_x0000_t75" ),
                    new XAttribute( "style",
                        "position:absolute;margin-left:0;margin-top:0;width:1038pt;height:1038pt;z-index:1" ),
                    new XElement( nsV + "imagedata",
                        new XAttribute( nsO + "relid", imgPartId ),
                        new XAttribute( nsO + "title", $"img_{DateTime.Now.Ticks}" )
                    ),
                    new XElement( nsO + "lock",
                        new XAttribute( nsV + "ext", "edit" ),
                        new XAttribute( "rotation", "t" )
                    )
                )
            );
        }

        private static XmlWriterSettings GetXmlWriterSettings()
        {
            return new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
            };
        }

        private static void AddHeader( WorksheetPart sheetPart )
        {
            var headerFooter = sheetPart.Worksheet.Elements<HeaderFooter>().FirstOrDefault();
            bool isNewHf = false;
            if (headerFooter == null)
            {
                headerFooter = new HeaderFooter();
                isNewHf = true;
            }

            bool isCrossTable = _pluginFullName.Contains( "CrossTableViews" );
            headerFooter.ScaleWithDoc = !isCrossTable;
            AddOddHeader( headerFooter, isNewHf );
            MoveOddFooter( headerFooter, isNewHf );

            if (isNewHf)
                sheetPart.Worksheet.Append( headerFooter );
        }

        private static void MoveOddFooter( HeaderFooter headerFooter, bool isNewHf )
        {
            if (isNewHf)
                return;
            var oddF = headerFooter.Elements<OddFooter>().FirstOrDefault();
            if (oddF != null)
            {
                headerFooter.RemoveChild( oddF );
                headerFooter.Append( oddF );
            }
        }

        private static void AddOddHeader( HeaderFooter headerFooter, bool isNewHf )
        {
            OddHeader oddH = null;
            if (!isNewHf)
            {
                oddH = headerFooter.Elements<OddHeader>().FirstOrDefault();
                if (oddH != null)
                    headerFooter.RemoveChild( oddH );
            }
            oddH = new OddHeader();
            oddH.Text = "&C&G";
            headerFooter.Append( oddH );
        }

        private static void AddSheetView( WorksheetPart sheetPart )
        {
            var shViews = sheetPart.Worksheet.Elements<SheetViews>().FirstOrDefault();
            var shView = shViews?.Elements<SheetView>().FirstOrDefault();
            if (shView == null)
                return;
            shView.View = new EnumValue<SheetViewValues>( SheetViewValues.PageLayout );
            shView.ZoomScaleNormal = 100;
            shView.ZoomScale = 50;
            shView.ZoomScalePageLayoutView = 50;
            shView.WorkbookViewId = 0;
            if (_pluginFullName.Contains( "CrossTableViews" ))
            {
                if (shView.Pane?.State != null)
                    shView.Pane.State.Value = PaneStateValues.Split;
            }
        }

        private static void AddPrinterSettings( WorksheetPart sheetPart )
        {
            var printerStngsPart = sheetPart.SpreadsheetPrinterSettingsParts.FirstOrDefault();
            if (printerStngsPart == null)
            {
                printerStngsPart = sheetPart.AddNewPart<SpreadsheetPrinterSettingsPart>();
                FeedPrinterSettings( printerStngsPart );
            }

            var pageSetup = sheetPart.Worksheet.Elements<PageSetup>().FirstOrDefault();
            if (pageSetup == null)
                return;
            pageSetup.Id = sheetPart.GetIdOfPart( printerStngsPart );
            SetPageScale( pageSetup );
            pageSetup.PaperSize = 9;
        }

        private static void SetPageScale( PageSetup pageSetup )
        {
            if (_pluginFullName.Contains( "CrossTableViews.ReportPlugins.PositionAppointmentCommanders" )
                || _pluginFullName.Contains( "CrossTableViews.ReportPlugins.PositionAppointment" ))
            {
                pageSetup.Scale = 36;
            }
        }

        private static void FeedPrinterSettings( SpreadsheetPrinterSettingsPart printerStngsPart )
        {
            string manifestRes = "Nat.Web.ReportManager.printerSettings1.bin";
            using (var prntrStream = Assembly.GetAssembly( typeof( ReportResultPage ) )
                       .GetManifestResourceStream( manifestRes ))
            {
                printerStngsPart.FeedData( prntrStream );
            }
        }

        private static void WriteVmlDrwPart( VmlDrawingPart vmlDrwPart )
        {
            var tempStream = vmlDrwPart.GetStream( FileMode.Create, FileAccess.Write );
            var streamWriter = new StreamWriter( tempStream );
            var writer = new XmlTextWriter( streamWriter ) { Formatting = Formatting.Indented };
            writer.Indentation = 0;
            writer.Formatting = Formatting.None;

            writer.WriteStartElementExt( "xml",
                "xmlns:v", "urn:schemas-microsoft-com:vml",
                "xmlns:o", "urn:schemas-microsoft-com:office:office",
                "xmlns:x", "urn:schemas-microsoft-com:office:excel" );
            writer.WriteStartElementExt( "o:shapelayout",
                "v:ext", "edit" );
            writer.WriteStartElementExt( "o:idmap",
                "v:ext", "edit",
                "data", "1" );
            writer.WriteEndElement();//o:idmap
            writer.WriteEndElement();//o:shapelayout

            writer.WriteStartElementExt( "v:shapetype",
                "id", "_x0000_t75",
                "coordsize", "21600,21600" );
            writer.WriteAttributeString( "o:spt", null, "75" );
            writer.WriteAttributeString( "o:preferrelative", null, "t" );
            writer.WriteAttributeString( "path", null, "m@4@5l@4@11@9@11@9@5xe" );
            writer.WriteAttributeString( "filled", null, "f" );
            writer.WriteAttributeString( "stroked", null, "f" );

            writer.WriteStartElementExt( "v:stroke", "joinstyle", "miter" );
            writer.WriteEndElement();//v:stroke

            writer.WriteStartElement( "v:formulas" );
            writer.WriteStartElementExt( "v:f", "eqn", "if lineDrawn pixelLineWidth 0" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "sum @0 1 0" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "sum 0 0 @1" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @2 1 2" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @3 21600 pixelWidth" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @3 21600 pixelHeight" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "sum @0 0 1" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @6 1 2" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @7 21600 pixelWidth" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "sum @8 21600 0" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "prod @7 21600 pixelHeight" );
            writer.WriteEndElement();//v:f
            writer.WriteStartElementExt( "v:f", "eqn", "sum @10 21600 0" );
            writer.WriteEndElement();//v:f
            writer.WriteEndElement();//v:formulas

            writer.WriteStartElementExt( "v:path", "o:extrusionok", "f",
                "gradientshapeok", "t",
                "o:connecttype", "rect" );
            writer.WriteEndElement();//v:path

            writer.WriteStartElementExt( "o:lock", "v:ext", "edit",
                "aspectratio", "t" );
            writer.WriteEndElement();//o:lock

            writer.WriteEndElement();//v:shapetype

            writer.WriteEndElement();//xml

            writer.Flush();
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();
            tempStream.Close();
            tempStream.Dispose();
        }

        #endregion

        public void AddToWordStream( Stream stream, string pluginFullName )
        {
            try
            {
                var rWatermark = DependencyResolver.Current.GetService<IWatermark>();
                var watermarkImage = rWatermark.GetImage( pluginFullName );
                if (watermarkImage == null)
                    return;

                using (var doc = WordprocessingDocument.Open( stream, true, new OpenSettings() ))
                {
                    if (doc.MainDocumentPart == null)
                        return;
                    ConfigTable( doc );
                    doc.MainDocumentPart.DeleteParts( doc.MainDocumentPart.HeaderParts );

                    var headerPart = doc.MainDocumentPart.AddNewPart<HeaderPart>();
                    var imgPart = headerPart.AddImagePart( ImagePartType.Png );
                    using (var imgStream = new MemoryStream( watermarkImage ))
                    {
                        imgPart.FeedData( imgStream );
                    }

                    AddHeaderPartContent( headerPart, headerPart.GetIdOfPart( imgPart ) );
                    var secProps = doc.MainDocumentPart.Document.Body.Elements<SectionProperties>();
                    foreach (var secProp in secProps)
                    {
                        secProp.RemoveAllChildren<HeaderReference>();
                        secProp.PrependChild<HeaderReference>( new HeaderReference()
                        {
                            Id = doc.MainDocumentPart.GetIdOfPart( headerPart )
                        } );
                    }
                }
            }
            finally
            {
                stream.Position = 0;
            }
        }

        #region word
        private static void ConfigTable( WordprocessingDocument doc )
        {
            var tbls = doc.MainDocumentPart.Document.Body.Elements<Table>();
            foreach (var tbl in tbls)
            {
                var trs = tbl.Elements<TableRow>();
                foreach (var tr in trs)
                {
                    var tcs = tr.Elements<TableCell>();
                    foreach (var tc in tcs)
                    {
                        var tcPrs = tc.Elements<TableCellProperties>();
                        foreach (var tcPr in tcPrs)
                        {
                            if (tcPr.Shading == null)
                                continue;
                            tcPr.Shading.Fill = "auto";
                        }
                    }
                }
            }
        }

        private void AddHeaderPartContent( HeaderPart headerPart, string imgPartId )
        {
            var header = new WHeader();
            var paragraph = new WParagraph();
            var run = new WRun();
            var picture = new WPicture();
            var shape = new Vml.Shape()
            {
                Id = $"WordPictureWatermark{DateTime.Now.Ticks}",
                Style = "position:absolute;margin-left:0;margin-top:0;width:1410.95pt;height:1410.95pt;z-index:-251657216;mso-position-horizontal:center;mso-position-horizontal-relative:margin;mso-position-vertical:center;mso-position-vertical-relative:margin",
                OptionalString = "_x0000_s2050",
                AllowInCell = false,
                Type = "#_x0000_t75"
            };
            var imgData = new Vml.ImageData()
            {
                RelationshipId = imgPartId,
                Title = $"img_{DateTime.Now.Ticks}"
            };
            shape.Append( imgData );
            picture.Append( shape );
            run.Append( picture );
            paragraph.Append( run );
            header.Append( paragraph );
            headerPart.Header = header;
        }
        #endregion

        public void AddToPdfStream(Stream stream, string pluginFullName )
        {
            try
            {
                var watermark = DependencyResolver.Current.GetService<IWatermark>();
                if (watermark == null)
                    return;
                AddWatermark( stream, watermark );
            }
            finally
            {
                stream.Position = 0;
            }
        }

        #region pdf
        private void AddWatermark( Stream stream, IWatermark watermark )
        {
            RadFixedDocument doc = ImportDocument( stream );
            for (int i = 0; i < doc.Pages.Count; i++)
            {
                RadFixedPage page = doc.Pages[ i ];
                InsertWatermark( watermark, page );
            }
            PdfFormatProvider provider = new PdfFormatProvider();
            stream.Position = 0;
            provider.ExportSettings = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export.PdfExportSettings()
            {                
                ImageQuality = Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export.ImageQuality.High,
                ShouldEmbedFonts = false,
                IsEncrypted = false
            };
            provider.Export( doc, stream );
        }

        private void InsertWatermark( IWatermark watermark, RadFixedPage page )
        {            
            Block block = GetBlock( watermark );

            var splitted = watermark.GetText().Split( '\n' );
            var txt = splitted[ 0 ].Length > splitted[ 1 ].Length ? splitted[ 0 ] : splitted[ 1 ];
            block.InsertText( txt );
            var blockSize = block.Measure();
            block.Clear();

            DrawWatermarkOnPage( page, block, splitted, blockSize );
        }

        private void DrawWatermarkOnPage( RadFixedPage page, Block block, string[] splitted, Size bsize )
        {
            FixedContentEditor editor = new FixedContentEditor( page );

            const double angle = -45;
            const double rad = Math.PI / 4;
            const int padding = 40;
            editor.Position.Rotate( angle );
            double blockWidth = bsize.Width * Math.Abs( Math.Sin( rad ) ) + padding;

            int x = GetCounter( page.Size.Width / blockWidth );
            int y = GetCounter( page.Size.Height / blockWidth );
            var offset = bsize.Height * Math.Abs( Math.Cos( rad ) );

            for (int i = 0; i <= x; i++)
            {
                for (int j = 1; j <= y; j++)
                {
                    double pageTopOffset = -padding * 1.3;
                    
                    editor.Position.Translate( blockWidth * i, blockWidth * j + pageTopOffset );
                    block.InsertText( splitted[ 0 ] );
                    editor.DrawBlock( block );

                    editor.Position.Translate( blockWidth * i + offset, blockWidth * j + offset + pageTopOffset );
                    block = block.Split();
                    block.InsertText( splitted[ 1 ] );
                    editor.DrawBlock( block );

                    block = block.Split();
                }
            }
        }

        private int GetCounter( double value )
        {
            if (value == 0)
                return 0;
            int valueBase = (int)value;
            double valueRest = value - valueBase;
            if (valueRest > 0.1)
            {
                valueBase++;
            }
            return valueBase;
        }

        private static Block GetBlock( IWatermark watermark )
        {
            var block = new Block();
            block.TextProperties.FontSize = watermark.FontSize;
            block.TextProperties.TrySetFont( new System.Windows.Media.FontFamily( watermark.Font )
                , FontStyles.Normal, FontWeights.Normal );

            block.HorizontalAlignment = Telerik.Windows.Documents.Fixed.Model.Editing.Flow.HorizontalAlignment.Center;
            block.VerticalAlignment = Telerik.Windows.Documents.Fixed.Model.Editing.Flow.VerticalAlignment.Center;
            block.GraphicProperties.FillColor = new TelerikRgbColor(
                watermark.TextColor.A,
                watermark.TextColor.R,
                watermark.TextColor.G,
                watermark.TextColor.B );
            return block;
        }

        private RadFixedDocument ImportDocument( Stream stream )
        {
            PdfFormatProvider provider = new PdfFormatProvider();
            RadFixedDocument document = provider.Import( stream );

            return document;
        }

        #endregion
    }
}