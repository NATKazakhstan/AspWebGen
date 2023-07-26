using System.IO;

namespace Nat.Web.Tools.Report
{
    public interface IReportQr
    {
        void AddToExcel(Stream stream, long logId, string pluginFullName);
    }
}