namespace Nat.Web.ReportManager
{
    using Nat.ReportManager.QueryGeneration;

    public interface IRememberReports
    {
        void CreateReport(string path, IReportPlugin plugin);
    }
}