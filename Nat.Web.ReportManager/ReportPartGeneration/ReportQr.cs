using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Nat.Web.Tools.Report;

namespace Nat.Web.ReportManager.ReportPartGeneration
{
    public class ReportQr: IReportQr
    {
        public void AddToExcel(Stream stream, long logId)
        {
            var qrCodeTextFormat = DependencyResolver.Current.GetService<IQrCodeTextFormat>();
            var qrImgStream = qrCodeTextFormat.GetQrCodeImageStream(logId);
            if (qrImgStream == null) return;
            using (var doc = SpreadsheetDocument.Open( stream, true, new OpenSettings()))
            {
                var sheetPart = doc.WorkbookPart.WorksheetParts.First();

                SetPageMargins(sheetPart);
                SetHeaderFooter(sheetPart);
                var drawingPart = sheetPart.VmlDrawingParts.FirstOrDefault();
                if (drawingPart == null) return;
                var imagePart = drawingPart.AddImagePart(ImagePartType.Png);
                imagePart.FeedData(qrImgStream);
                FIllVmlDwgPart(drawingPart, drawingPart.GetIdOfPart(imagePart));            
            }

            stream.Position = 0;
        }

        private static void SetHeaderFooter(WorksheetPart sheetPart)
        {
            var hf = sheetPart.Worksheet.Elements<HeaderFooter>().FirstOrDefault();
            if (hf == null) return;
            hf.AlignWithMargins = false;
            hf.Append(new OddFooter("&R&G"));
        }

        private static void SetPageMargins(WorksheetPart sheetPart)
        {
            var pgMargins = sheetPart.Worksheet.Elements<PageMargins>().FirstOrDefault();
            if (pgMargins == null) return;
            pgMargins.Top = 0.55118;
            pgMargins.Bottom = 1.25985;
            pgMargins.Header = 0;
            pgMargins.Footer = 0;
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