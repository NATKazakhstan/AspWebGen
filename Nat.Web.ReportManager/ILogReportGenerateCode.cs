namespace Nat.Web.ReportManager
{
    public interface ILogReportGenerateCode
    {
        long GetCodeFor(string reportPlugin, string title, long logCode, LogReportGenerateCodeType type);
    }
}