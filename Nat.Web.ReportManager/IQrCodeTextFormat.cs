namespace Nat.Web.ReportManager
{
    public interface IQrCodeTextFormat
    {
        string GetUserText(long logId);
        string GetQrCodeData(long logId);
    }
}