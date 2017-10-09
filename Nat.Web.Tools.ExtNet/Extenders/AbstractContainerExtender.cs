using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Ext.Net;

namespace Nat.Web.Tools.ExtNet.Extenders
{
    public static class AbstractContainerExtender
    {
        public static void SetHiddenControlContainer(AbstractComponent control)
        {
            (control.Parent as AbstractContainer).SetHiddenControlContainer();
        }

        private static void SetHiddenControlContainer(this AbstractContainer container)
        {
            if (container == null) return;

            var hidden = container.Items.Where(c => c.GetType() != typeof(Hidden)).All(c => c.Hidden);
            container.Hidden = hidden;
            (container.Parent as AbstractContainer).SetHiddenControlContainer();
        }
    }
}
