using System.Drawing;

namespace Nat.Web.Tools.Report
{
    public interface IWatermark
    {
        string Font { get; set; }
        int FontSize { get; set; }
        Color TextColor { get; set; }

        string GetText();
        byte[] GetImage(string pluginFullName);
    }
}