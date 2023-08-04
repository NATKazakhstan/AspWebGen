using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Nat.ExportInExcel;
using Nat.Web.Tools.Report;
using XDrawing = DocumentFormat.OpenXml.Spreadsheet.Drawing;

namespace Nat.Web.ReportManager.ReportPartGeneration
{
    public class ReportQr: IReportQr
    {
        private string _pluginFullName = string.Empty;
        public void AddToExcel(Stream stream, long logId, string pluginFullName)
        {
            try
            {
                _pluginFullName = pluginFullName;
                var qrCodeTextFormat = DependencyResolver.Current.GetService<IQrCodeTextFormat>();
                using (var qrImgStream = qrCodeTextFormat.GetQrCodeImageStream(logId))
                {
                    if (qrImgStream == null) return;
                    using (var doc = SpreadsheetDocument.Open(stream, true, new OpenSettings()))
                    {
                        var sheetPart = doc.WorkbookPart.WorksheetParts.First();

                        PrepareSheetPart(sheetPart);
                        var dwg = sheetPart.Worksheet.Elements<XDrawing>().FirstOrDefault();
                        if (dwg != null)
                        {
                            sheetPart.Worksheet.RemoveChild(dwg);
                        }
                        SetPageMargins(sheetPart);
                        SetHeaderFooter(sheetPart);
                        var drawingPart = GetVmlDrawingPart(sheetPart);
                        var imagePart = drawingPart.AddImagePart(ImagePartType.Png);
                        imagePart.FeedData(qrImgStream);
                        FIllVmlDwgPart(drawingPart, drawingPart.GetIdOfPart(imagePart));
                        SetPostElements(sheetPart, drawingPart, dwg);
                    }
                }
            }
            finally
            {
                stream.Position = 0;
            }
        }

        private static void SetPostElements(WorksheetPart sheetPart, VmlDrawingPart drawingPart, Drawing dwg)
        {
            var ldhf = sheetPart.Worksheet.Elements<LegacyDrawingHeaderFooter>().FirstOrDefault();
            if (ldhf == null)
            {
                ldhf = new LegacyDrawingHeaderFooter();
            }
            else
            {
                sheetPart.Worksheet.RemoveChild(ldhf);
            }

            ldhf.Id = sheetPart.GetIdOfPart(drawingPart);
            if (dwg != null)
            {
                sheetPart.Worksheet.Append(dwg);
            }

            sheetPart.Worksheet.Append(ldhf);
        }

        private static VmlDrawingPart GetVmlDrawingPart(WorksheetPart sheetPart)
        {
            var r = sheetPart.VmlDrawingParts.FirstOrDefault();
            if (r == null)
            {
                r = sheetPart.AddNewPart<VmlDrawingPart>();
                WriteVmlPart(r);
            }
            
            return r;
        }

        private static void WriteVmlPart(VmlDrawingPart vmlDrawingPart)
        {
            var tempStream = vmlDrawingPart.GetStream(FileMode.Create, FileAccess.Write);
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
            
            writer.WriteEndElement();//xml
            
            writer.Flush();
            streamWriter.Flush();
            streamWriter.Close();
            streamWriter.Dispose();
            tempStream.Close();
            tempStream.Dispose();
        }

        private void PrepareSheetPart(WorksheetPart sheetPart)
        {
            AddSheetView(sheetPart);
            AddPrinterSettings(sheetPart);
        }

         private void AddSheetView(WorksheetPart sheetPart)
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

        private void AddPrinterSettings(WorksheetPart sheetPart)
        {
            var printerStngsPart = sheetPart.SpreadsheetPrinterSettingsParts.FirstOrDefault();
            if (printerStngsPart == null)
            {
                printerStngsPart = sheetPart.AddNewPart<SpreadsheetPrinterSettingsPart>();
                FeedPrinterSettings(printerStngsPart);
            }
            var pageSetup = sheetPart.Worksheet.Elements<PageSetup>().FirstOrDefault();
            bool isNewPgSetup = false;
            if (pageSetup == null)
            {
                pageSetup = new PageSetup();
                isNewPgSetup = true;
            }
            pageSetup.Id = sheetPart.GetIdOfPart(printerStngsPart);
            SetPageScale(pageSetup);
            pageSetup.PaperSize = 9;
            if(isNewPgSetup)
                sheetPart.Worksheet.Append(pageSetup);
        }

        private void SetPageScale(PageSetup pageSetup)
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
        
        private void SetHeaderFooter(WorksheetPart sheetPart)
        {
            var hf = sheetPart.Worksheet.Elements<HeaderFooter>().FirstOrDefault();
            bool isNewHF = false;
            if (hf == null)
            {
                hf = new HeaderFooter();
                isNewHF = true;
            }
            bool isCrossTable = _pluginFullName.Contains( "CrossTableViews" );
            hf.ScaleWithDoc = !isCrossTable;
            hf.AlignWithMargins = false;
            hf.Append(new OddFooter("&R&G"));
            if(isNewHF)
                sheetPart.Worksheet.Append(hf);
        }

        private static void SetPageMargins(WorksheetPart sheetPart)
        {
            var pgMargins = sheetPart.Worksheet.Elements<PageMargins>().FirstOrDefault();
            if (pgMargins == null) return;
            pgMargins.Top = 0.55118;
            pgMargins.Bottom = 1.25985;
            pgMargins.Header = 0;
            pgMargins.Footer = 0.19685;
        }

        private void FIllVmlDwgPart(VmlDrawingPart drawingPart, string imgPartId)
        {
            using (var tempStream = drawingPart.GetStream(FileMode.Open, FileAccess.ReadWrite))
            {
                XDocument xd = XDocument.Load(tempStream);
            
                var root = xd.Root;
                var nsV = root.GetNamespaceOfPrefix("v");
                var nsO = root.GetNamespaceOfPrefix("o");
                AddShapeIntoRoot(imgPartId, root, nsV, nsO);
                tempStream.Position = 0;
                
                using (var xw = XmlWriter.Create(tempStream, GetXmlWriterSettings()))
                {
                    xd.Save(xw);
                    xw.Flush();
                }
            }
        }

        private static void AddShapeIntoRoot(string imgPartId, XElement root, XNamespace nsV, XNamespace nsO)
        {
            root.Add(
                new XElement(nsV + "shape",
                    new XAttribute("id", "RF"),
                    new XAttribute(nsO + "spid", "_x0000_s1030"),
                    new XAttribute("type", "#_x0000_t75"),
                    new XAttribute("style",
                        "position:absolute;margin-left:0;margin-top:0;width:70.5pt;height:70.5pt;z-index:2"),
                    new XElement(nsV + "imagedata",
                        new XAttribute(nsO + "relid", imgPartId),
                        new XAttribute(nsO + "title", $"qr_img_{DateTime.Now.Ticks}")
                    ),
                    new XElement(nsO + "lock",
                        new XAttribute(nsV + "ext", "edit"),
                        new XAttribute("rotation", "t")
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

    }
}