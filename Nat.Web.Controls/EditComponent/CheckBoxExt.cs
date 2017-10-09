using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class CheckBoxExt : CheckBox
    {
        /// <summary>
        /// Замена DBNull.Value и null на это значение.
        /// </summary>
        [DefaultValue(false)]
        public bool NullValue
        {
            get { return (bool?)ViewState["_nullValue"] ?? false; }
            set { ViewState["_nullValue"] = value; }
        }

        [DefaultValue(false)]
        [Bindable(true, BindingDirection.TwoWay)]
        [Themeable(false)]
        public new object Checked
        {
            get { return base.Checked; }
            set
            {
                if(value != null && value != DBNull.Value)
                {
                    base.Checked = Convert.ToBoolean(value);
                }
                else
                {
                    base.Checked = NullValue;
                }
            }
        }
    }
}