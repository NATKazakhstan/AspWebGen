using System.Drawing;

namespace Nat.Web.ReportManager
{
    public interface IQrCodeTextFormat
    {
        string GetUserText(long logId);
        string GetQrCodeData(long logId);
        Image GetHorizontalImage();
        Image GetVerticalImage();
    }
}