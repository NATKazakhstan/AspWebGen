using System.IO;

namespace Nat.Web.Tools.Report
{
    public interface IReportSideInfo
    {
        void AddToExcel(Stream stream, long logId);
    }
}