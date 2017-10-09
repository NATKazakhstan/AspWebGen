namespace Nat.Web.Controls.GenerationClasses
{
    public static class BaseClientManagmentControlExtensions
    {
        public static void SetEnableMode(this BaseClientManagmentControl source, EnableMode mode, params EnableItem[] items)
        {
            foreach (var enableItem in items)
                enableItem.EnableMode = mode;
        }
    }
}