namespace Nat.Web.Tools.Security
{
    public interface IReportAccess
    {
        bool DoesHaveUserPermission(string pluginName);
    }
}