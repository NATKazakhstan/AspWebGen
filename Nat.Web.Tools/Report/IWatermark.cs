namespace Nat.Web.Tools.Report
{
    public interface IWatermark
    {
        string GetText();
        byte[] GetImage(string pluginFullName);
    }
}