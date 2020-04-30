namespace Nat.Web.ReportManager
{
    public interface IExportPermission
    {
        string[] GetWordRoles();
        string[] GetExcelRoles();
        string[] GetPdfRoles();
    }
}