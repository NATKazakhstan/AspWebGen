using System.Linq;
using Ext.Net;

namespace Nat.Web.Tools.ExtNet.Extenders
{
    public static class AbstractContainerExtender
    {
        public static void SetHiddenControlContainer(AbstractComponent control)
        {
            var abstractContainer = control.Parent as AbstractContainer ?? control.ParentComponent as AbstractContainer;
            abstractContainer.SetHiddenControlContainer();
        }

        private static void SetHiddenControlContainer(this AbstractContainer container)
        {
            if (container == null) return;

            var hidden = container.Items.Where(c => c.GetType() != typeof(Hidden)).All(c => c.Hidden);
            container.Hidden = hidden;
            var abstractContainer = container.Parent as AbstractContainer ?? container.ParentComponent as AbstractContainer;
            abstractContainer.SetHiddenControlContainer();
        }
    }
}