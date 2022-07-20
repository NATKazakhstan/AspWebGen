using System.Drawing;
using System.IO;

namespace Nat.Web.ReportManager
{
    public interface IQrCodeTextFormat
    {
        string GetUserText(long logId);
        string GetQrCodeData(long logId);
        Image GetHorizontalImage();
        Image GetVerticalImage();
        Stream GetHorizontalImageStream();
        Stream GetVerticalImageStream();
        Stream GetQrCodeImageStream(long logId);
    }
}