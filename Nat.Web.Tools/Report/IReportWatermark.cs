using System.IO;

namespace Nat.Web.Tools.Report
{
    public interface IReportWatermark
    {
        void AddToExcelStream(Stream stream, string pluginFullName);
        void AddToWordStream(Stream stream, string pluginFullName);
    }
}