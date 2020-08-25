using System.Linq;
using Ext.Net;

namespace Nat.Web.Tools.ExtNet.Extenders
{
    public static class AbstractContainerExtender
    {
        public static void SetHiddenControlContainer(AbstractComponent control)
        {
            var abstractContainer = control.Parent as AbstractContainer ?? control.ParentComponent as AbstractContainer;
            abstractContainer.SetHiddenControlContainer(control.Hidden);
        }

        private static void SetHiddenControlContainer(this AbstractContainer container, bool newHidden)
        {
            if (container == null) return;

            var abstractContainer = container.Parent as AbstractContainer;

            if (container.ContentControls.Count > 0 && !container.Page.IsPostBack)
            {
                return;
            }
            var components = container.Items.Where(c => c.GetType() != typeof(Hidden)).ToArray();
            if (components.Length == 0)
                container.Hidden = newHidden;
            else if (components.All(c => c.Hidden) && newHidden)
                container.Hidden = true;
            else
                container.Hidden = false;

            abstractContainer.SetHiddenControlContainer(container.Hidden);
        }
    }
}