namespace Nat.Web.Tools
{
    public interface ILogReportGenerateCode
    {
        long GetCodeFor(string reportPlugin, string title, long logCode, LogReportGenerateCodeType type);
    }
}