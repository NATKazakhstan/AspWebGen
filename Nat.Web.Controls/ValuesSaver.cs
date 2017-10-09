using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class ValuesSaver
    {
        public ValuesSaver()
        {
            InnerValues = new Dictionary<string, object>();
            TargetValues = new Dictionary<string, object>();
        }

        public Control Target { get; set;}

        protected Dictionary<string, object> InnerValues { get; private set; }

        protected Dictionary<string, object> TargetValues { get; private set; }

        public void SaveValues()
        {
            var values = InnerValues;
            values.Clear();
            if (Target == null) return;

            SaveValuesRecursive(Target, values);
        }

        private static void SaveValuesRecursive(Control target, IDictionary<string, object> values)
        {
            foreach (Control control in target.Controls)
            {
                if (control.HasControls()) SaveValuesRecursive(control, values);
                if (string.IsNullOrEmpty(control.ID)) continue;

                var tb = control as TextBox;
                if (tb != null)
                {
                    values.Add(tb.ID, tb.Text);
                    continue;
                }
                var lc = control as ListControl;
                if (lc != null)
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
                if (rb != null)
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
            if (Target == null) return;
            var dv = Target as DetailsView;
            if (dv != null)
            {
                values.Add("CM", dv.CurrentMode);
                if (dv.AllowPaging) values.Add("PI", dv.PageIndex);
                return;
            }
            var gv = Target as GridView;
            if (gv != null)
            {
                values.Add("SI", gv.SelectedIndex);
                if (gv.AllowPaging) values.Add("PI", gv.PageIndex);
                if (gv.AllowSorting)
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
            if (Target == null) return;

            LoadValuesRecursive(Target, values);
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
            if (Target == null || TargetValues.Count == 0) return;
            var dv = Target as DetailsView;
            if (dv != null)
            {
                dv.ChangeMode((DetailsViewMode)values["CM"]);
                if (dv.AllowPaging) dv.PageIndex = (int)values["PI"];
                dv.DataBind();
                return;
            }
            var gv = Target as GridView;
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