using System.IO;

namespace Nat.Web.ReportManager.ReportGeneration
{
    public interface ICustomStreamReport
    {
        Stream Export(ExportFormat format, out string fileName, out string fileNameExtension);
        string BarCodeLogText { get; set; }
        string BarCodeLogData { get; set; }
        Stream BarCodeLogDataStream { get; set; }
        Stream BarCodeLogImageH { get; set; }
        Stream BarCodeLogImageV { get; set; }
    }
}
