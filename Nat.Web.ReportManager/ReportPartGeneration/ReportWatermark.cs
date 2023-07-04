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
using XDrawing = DocumentFormat.OpenXml.Spreadsheet.Drawing;
using WHeader = DocumentFormat.OpenXml.Wordprocessing.Header;
using WParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WPicture = DocumentFormat.OpenXml.Wordprocessing.Picture;
using Vml = DocumentFormat.OpenXml.Vml;

namespace Nat.Web.ReportManager.ReportPartGeneration
{
    public class ReportWatermark:IReportWatermark
    {
        private static string _pluginFullName = string.Empty;
        public void AddToExcelStream(Stream stream, string pluginFullName)
        {
            var rWatermark = DependencyResolver.Current.GetService<IWatermark>();
            var watermarkImage = rWatermark.GetImage(pluginFullName);
            if (watermarkImage == null) return;
            
            _pluginFullName = pluginFullName;
            using (var doc = SpreadsheetDocument.Open( stream, true, new OpenSettings()))
            {
                var sheetPart = doc.WorkbookPart.WorksheetParts.First();

                AddSheetView(sheetPart);
                AddPrinterSettings(sheetPart);

                var dwg = sheetPart.Worksheet.Elements<XDrawing>().FirstOrDefault();
                if (dwg != null)
                {
                    sheetPart.Worksheet.RemoveChild(dwg);
                }
                AddHeader(sheetPart);
                AddWatermarkDrawing(sheetPart, watermarkImage, dwg);
            }

            stream.Position = 0;
        }

        #region excel
        
        private static void AddWatermarkDrawing(WorksheetPart sheetPart, byte[] watermarkImage, XDrawing dwg)
        {
            var vmlDrwPart = sheetPart.AddNewPart<VmlDrawingPart>();
            var imagePart = vmlDrwPart.AddImagePart(ImagePartType.Png);
            using (var imgStream = new MemoryStream(watermarkImage))
            {
                imagePart.FeedData(imgStream);
            }

            FillVmlDrwPart(vmlDrwPart, vmlDrwPart.GetIdOfPart(imagePart));
            var ldhf = new LegacyDrawingHeaderFooter()
            {
                Id = sheetPart.GetIdOfPart(vmlDrwPart)
            };
            sheetPart.Worksheet.Append(dwg);
            sheetPart.Worksheet.Append(ldhf);
        }

        private static void AddHeader(WorksheetPart sheetPart)
        {
            var headerFooter = sheetPart.Worksheet.Elements<HeaderFooter>().FirstOrDefault();
            if (headerFooter == null)
            {
                bool isCrossTable = _pluginFullName.Contains( "CrossTableViews" );
                headerFooter = new HeaderFooter()
                {
                    ScaleWithDoc = !isCrossTable
                };
            }

            var oddH = new OddHeader("&C&G");
            headerFooter.Append(oddH);
            sheetPart.Worksheet.Append(headerFooter);
        }

        private static void AddSheetView(WorksheetPart sheetPart)
        {
            var shViews = sheetPart.Worksheet.Elements<SheetViews>().FirstOrDefault();
            var shView = shViews?.Elements<SheetView>().FirstOrDefault();
            if(shView == null)return;
            shView.View = new EnumValue<SheetViewValues>(SheetViewValues.PageLayout);
            shView.ZoomScaleNormal = 100;
            shView.ZoomScale = 50;
            shView.ZoomScalePageLayoutView = 50;
            shView.WorkbookViewId = 0;
            if(_pluginFullName.Contains( "CrossTableViews" ))
            {
                if (shView.Pane?.State != null)
                    shView.Pane.State.Value = PaneStateValues.Split;
            }            
        }

        private static void AddPrinterSettings(WorksheetPart sheetPart)
        {
            var printerStngsPart = sheetPart.AddNewPart<SpreadsheetPrinterSettingsPart>();
            FeedPrinterSettings(printerStngsPart);

            var pageSetup = sheetPart.Worksheet.Elements<PageSetup>().FirstOrDefault();
            if (pageSetup == null) return;
            pageSetup.Id = sheetPart.GetIdOfPart(printerStngsPart);
            SetPageScale(pageSetup);
            pageSetup.PaperSize = 9;
        }

        private static void SetPageScale(PageSetup pageSetup)
        {
            if (_pluginFullName.Contains("CrossTableViews.ReportPlugins.PositionAppointmentCommanders")
                || _pluginFullName.Contains("CrossTableViews.ReportPlugins.PositionAppointment"))
            {
                pageSetup.Scale = 36;
            }
        }

        private static void FeedPrinterSettings(SpreadsheetPrinterSettingsPart printerStngsPart)
        {
            string manifestRes = "Nat.Web.ReportManager.printerSettings1.bin";
            using (var prntrStream = Assembly.GetAssembly(typeof(ReportResultPage))
                       .GetManifestResourceStream(manifestRes))
            {
                printerStngsPart.FeedData(prntrStream);
            }
        }

        private static void FillVmlDrwPart(VmlDrawingPart vmlDrwPart, string imgPartId)
        {
            var tempStream = vmlDrwPart.GetStream(FileMode.Create, FileAccess.Write);
            var streamWriter = new StreamWriter(tempStream);
            var writer = new XmlTextWriter(streamWriter) { Formatting = Formatting.Indented };
            writer.Indentation = 0;
            writer.Formatting = Formatting.None;
            
            writer.WriteStartElementExt("xml",
                "xmlns:v","urn:schemas-microsoft-com:vml",
                "xmlns:o","urn:schemas-microsoft-com:office:office",
                "xmlns:x","urn:schemas-microsoft-com:office:excel");
            writer.WriteStartElementExt("o:shapelayout",
                "v:ext", "edit");
            writer.WriteStartElementExt("o:idmap",
                "v:ext", "edit",
                "data","1");
            writer.WriteEndElement();//o:idmap
            writer.WriteEndElement();//o:shapelayout

            writer.WriteStartElementExt("v:shapetype",
                "id", "_x0000_t75",
                "coordsize", "21600,21600");
            writer.WriteAttributeString("o:spt", null, "75");
            writer.WriteAttributeString("o:preferrelative", null, "t");
            writer.WriteAttributeString("path", null, "m@4@5l@4@11@9@11@9@5xe");
            writer.WriteAttributeString("filled", null, "f");
            writer.WriteAttributeString("stroked", null, "f");
            
            writer.WriteStartElementExt("v:stroke", "joinstyle", "miter"); writer.WriteEndElement();//v:stroke
            
            writer.WriteStartElement("v:formulas");
            writer.WriteStartElementExt("v:f", "eqn", "if lineDrawn pixelLineWidth 0"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "sum @0 1 0"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "sum 0 0 @1"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @2 1 2"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @3 21600 pixelWidth"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @3 21600 pixelHeight"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "sum @0 0 1"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @6 1 2"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @7 21600 pixelWidth"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "sum @8 21600 0"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "prod @7 21600 pixelHeight"); writer.WriteEndElement();//v:f
            writer.WriteStartElementExt("v:f", "eqn", "sum @10 21600 0"); writer.WriteEndElement();//v:f
            writer.WriteEndElement();//v:formulas
            
            writer.WriteStartElementExt("v:path", "o:extrusionok", "f",
                "gradientshapeok", "t",
                "o:connecttype", "rect");
            writer.WriteEndElement();//v:path
            
            writer.WriteStartElementExt("o:lock", "v:ext", "edit",
                "aspectratio", "t");
            writer.WriteEndElement();//o:lock
            
            writer.WriteEndElement();//v:shapetype
            
            writer.WriteStartElement("v:shape");
            writer.WriteAttributeString("id", null, "CH");
            writer.WriteAttributeString("o:spid", null, "_x0000_s1025");
            writer.WriteAttributeString("type", null, "#_x0000_t75");
            writer.WriteAttributeString("style", null, "position:absolute;margin-left:0;margin-top:0;width:1038pt;height:1038pt;z-index:1");
            
            writer.WriteStartElementExt("v:imagedata","o:relid", imgPartId,
                "o:title", $"img_{DateTime.Now.Ticks}");
            writer.WriteEndElement();//v:imagedata
            
            writer.WriteStartElementExt("o:lock",  "v:ext", "edit",
                "rotation", "t");
            writer.WriteEndElement();//o:lock
            
            writer.WriteEndElement();//v:shape
            writer.WriteEndElement();//xml
            
            writer.Flush();
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();
            tempStream.Close();
            tempStream.Dispose();
        }

        #endregion
        
        public void AddToWordStream(Stream stream, string pluginFullName)
        {
            var rWatermark = DependencyResolver.Current.GetService<IWatermark>();
            var watermarkImage = rWatermark.GetImage(pluginFullName);
            if (watermarkImage == null) return;

            using (var doc = WordprocessingDocument.Open(stream, true, new OpenSettings()))
            {
                if (doc.MainDocumentPart == null) return;
                doc.MainDocumentPart.DeleteParts(doc.MainDocumentPart.HeaderParts);
                var headerPart = doc.MainDocumentPart.AddNewPart<HeaderPart>();
                var imgPart = headerPart.AddImagePart(ImagePartType.Png);
                using (var imgStream = new MemoryStream(watermarkImage))
                {
                    imgPart.FeedData(imgStream);
                }
                AddHeaderPartContent(headerPart, headerPart.GetIdOfPart(imgPart));
                var secProps = doc.MainDocumentPart.Document.Body.Elements<SectionProperties>();
                foreach (var secProp in secProps)
                {
                    secProp.RemoveAllChildren<HeaderReference>();
                    secProp.PrependChild<HeaderReference>(new HeaderReference()
                    {
                        Id = doc.MainDocumentPart.GetIdOfPart(headerPart)
                    });
                }
            }

            stream.Position = 0;
        }

        private void AddHeaderPartContent(HeaderPart headerPart, string imgPartId)
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
            shape.Append(imgData);
            picture.Append(shape);
            run.Append(picture);
            paragraph.Append(run);
            header.Append(paragraph);
            headerPart.Header = header;
        }
    }
}