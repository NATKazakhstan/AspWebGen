namespace Nat.Web.ReportManager.ReportGeneration
{
    using Nat.Web.Tools;

    public interface IRedirectReportPlugin
    {
        bool LogViewReport { get; }
        string RedirectUrl { get; }

        string GetReportUrl(string sessionGuid, string culture);

        void OpenReport(
            WebReportManager webReportManager,
            StorageValues storageValues,
            string format,
            string culture,
            string backPath,
            string textOfBackPath,
            string command);
    }
}