/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 12 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class DisabledTextBox : TextBox
    {
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public string Value
        {
            get
            {
                if (base.Text == "") return null;
                return base.Text;
            }
            set { base.Text = value; }
        }

        public DisabledTextBox()
        {
            Style.Add("display", "none");
            Style.Add("visibility", "hidden");
            Attributes.Add("isIgnoreVisible", "true");
//            Style.Add("float", "right");
//            Width = Unit.Pixel(0);
//            Attributes["onchangedisplay"] = "if (this.style.display != 'none') { this.style.display = 'none'; this.fireEvent('onchangedisplay'); } ";
        }

        public string OnClientChange { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!string.IsNullOrEmpty(OnClientChange))
                Attributes.Add("onchange", OnClientChange);
        }
    }
}