using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class SaveValuesControl : Control
    {
        private bool savedValues;
        private bool savedTargetValues;

        [DefaultValue(null)]
        [IDReferenceProperty]
        [TypeConverter(typeof(ControlIDConverter))]
        public string TargetID { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if(!DesignMode) base.Render(writer);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.PreLoad += Page_OnPreLoad;
        }

        private void Page_OnPreLoad(object sender, EventArgs e)
        {
            LoadTargetValues();
            LoadValues();
        }

        protected Dictionary<string, object> InnerValues
        {
            get 
            {
                if (ViewState["InnerState"] == null)
                    ViewState["InnerState"] = new Dictionary<string, object>();
                return (Dictionary<string, object>) ViewState["InnerState"];
            }
        }

        protected Dictionary<string, object> TargetValues
        {
            get 
            {
                if (ViewState["TargetState"] == null)
                    ViewState["TargetState"] = new Dictionary<string, object>();
                return (Dictionary<string, object>) ViewState["TargetState"];
            }
        }

        protected override object SaveViewState()
        {
            if(!savedValues) SaveValues();
            if(!savedTargetValues) SaveTargetValues();
            return base.SaveViewState();
        }

        public void SaveValues()
        {
            var values = InnerValues;
            values.Clear();
            var target = Parent.FindControl(TargetID);
            if (target == null) return;

            SaveValuesRecursive(target, values);
            savedValues = true;
        }

        private static void SaveValuesRecursive(Control target, IDictionary<string, object> values)
        {
            foreach (Control control in target.Controls)
            {
                if (control.HasControls()) SaveValuesRecursive(control, values);
                if (string.IsNullOrEmpty(control.ID)) continue;

                var tb = control as TextBox;
                if(tb != null)
                {
                    values.Add(tb.ID, tb.Text); 
                    continue;
                }
                var lc = control as ListControl;
                if(lc != null)
                {
                    values.Add(lc.ID, lc.SelectedIndex);
                    continue;
                }
                var cb = control as CheckBox;
                if (cb != null)
                {
                    values.Add(cb.ID, cb.Checked);
                    continue;
                }
                var rb = control as RadioButton;
                if(rb != null)
                {
                    values.Add(rb.ID, rb.Checked);
                    continue;
                }
            }
        }

        public void SaveTargetValues()
        {
            var values = TargetValues;
            values.Clear();
            var target = Parent.FindControl(TargetID);
            if(target == null) return;
            savedTargetValues = true;
            var dv = target as DetailsView;
            if (dv != null)
            {
                values.Add("CM", dv.CurrentMode);
                if (dv.AllowPaging) values.Add("PI", dv.PageIndex);
                return;
            }
            var gv = target as GridView;
            if(gv != null)
            {
                values.Add("SI", gv.SelectedIndex);
                if(gv.AllowPaging) values.Add("PI", gv.PageIndex);
                if(gv.AllowSorting)
                {
                    values.Add("SE", gv.SortExpression);
                    values.Add("SD", gv.SortDirection);
                }
                values.Add("EI", gv.EditIndex);
                return;
            }
        }

        public void LoadValues()
        {
            var values = InnerValues;
            var target = Parent.FindControl(TargetID);
            if (target == null) return;

            LoadValuesRecursive(target, values);
        }

        private static void LoadValuesRecursive(Control target, IDictionary<string, object> values)
        {
            foreach (Control control in target.Controls)
            {
                if (control.HasControls()) LoadValuesRecursive(control, values);
                if (string.IsNullOrEmpty(control.ID) || !values.ContainsKey(control.ID)) continue;
                var tb = control as TextBox;
                if (tb != null)
                {
                    tb.Text = (string)values[tb.ID];
                    continue;
                }
                var lc = control as ListControl;
                if (lc != null)
                {
                    lc.SelectedIndex = (int)values[lc.ID];
                    continue;
                }
                var cb = control as CheckBox;
                if (cb != null)
                {
                    cb.Checked = (bool)values[cb.ID];
                    continue;
                }
                var rb = control as RadioButton;
                if (rb != null)
                {
                    rb.Checked = (bool)values[rb.ID];
                    continue;
                }
            }
        }

        public void LoadTargetValues()
        {
            var values = TargetValues;
            var target = Parent.FindControl(TargetID);
            if (target == null || TargetValues.Count == 0) return;
            var dv = target as DetailsView;
            if (dv != null)
            {
                dv.ChangeMode((DetailsViewMode)values["CM"]);
                if (dv.AllowPaging) dv.PageIndex = (int) values["PI"];
                dv.DataBind();
                return;
            }
            var gv = target as GridView;
            if (gv != null)
            {
                gv.SelectedIndex = (int)values["SI"];
                if (gv.AllowPaging) gv.PageIndex = (int)values["PI"];
                if (gv.AllowSorting && !string.IsNullOrEmpty((string)values["SE"]))
                {
                    gv.Sort((string)values["SE"], (SortDirection)values["SD"]);
                }
                gv.EditIndex = (int)values["EI"];
                gv.DataBind();
                return;
            }  
        }
    }
}