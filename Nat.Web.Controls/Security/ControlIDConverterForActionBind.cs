/*
 * Created by:   Sergey V. Shpakovskiy
 * Copyright (c) New Age Technologies
 * Created:      27.08.2007
 */

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.Security
{
    internal class ControlIDConverterForActionBind : ControlIDConverter
    {
        # region Private methods

        private string[] GetControls(IDesignerHost host, object instance)
        {
            IContainer container = host.Container;
            IComponent component = instance as IComponent;
            if ((component != null) && (component.Site != null))
            {
                container = component.Site.Container;
            }
            if (container == null)
            {
                return null;
            }
            ComponentCollection components = container.Components;
            ArrayList list = new ArrayList();
            foreach (IComponent comp in components)
            {
                Control control = comp as Control;
                if (control != null) RecurseChilds(control, ref list, instance, host);
            }
            list.Sort(Comparer.Default);
            return (string[])list.ToArray(typeof(string));
        }

        private void RecurseChilds(Control control, ref ArrayList list, object instance, IDesignerHost host)
        {
            if (control.HasControls())
            {
                foreach (Control ctrl in control.Controls)
                {
                    RecurseChilds(ctrl, ref list, instance, host);
                }
            }
            if (((control != instance) && ((control != host.RootComponent) && (control.ID != null))) &&
                ((control.ID.Length > 0) && FilterControl(control)) && !list.Contains(control.ID))
            {
                if (control is WebControl)
                    list.Add(control.ID);
            }
        }

        # endregion

        # region Public methods

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null) return null;

            IDesignerHost service = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            if (service == null) return null;

            string[] controls = GetControls(service, context.Instance);
            if (controls == null) return null;

            return new StandardValuesCollection(controls);
        }

        # endregion
    }
}