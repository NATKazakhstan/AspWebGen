using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.DateTimeControls;

namespace Nat.Web.Controls
{
    [ClientCssResource("Nat.Web.Controls.Lookup.AutoCompleteTextBox.css")]
    [ClientCssResource("Nat.Web.Controls.PopupControl.css")]
    [ClientCssResource("Nat.Web.Controls.CommandFieldExt.Styles.ButtonStyle.css")]
    [RequiredScript(typeof(DatePicker))]
    [RequiredScript(typeof(CollapsiblePanelExtender))]
    [RequiredScript(typeof(AutoCompleteExtender))]
    [RequiredScript(typeof(EnableControls))]
    [RequiredScript(typeof(CopyValue))]
    [RequiredScript(typeof(DropDownListExt))]
    [RequiredScript(typeof(PopupControl))]
    [RequiredScript(typeof(AutoCompleteTextBox))]
    [RequiredScript(typeof(AutoCompleteTextBoxPair))]
    [RequiredScript(typeof(LookupTable))]
    [RequiredScript(typeof(LookupTextBox))]
    [RequiredScript(typeof(ColumnFilter))]
    [RequiredScript(typeof(ColumnFilterList))]
    public class LoaderCssScript : WebControl, IScriptControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //CreateCalendarExtender();
        }

        private void CreateCalendarExtender() 
        {
            TextBox textBox = new TextBox();
            textBox.ID = "txt";
            textBox.Visible = false;
            Controls.Add(textBox);
            CalendarExtender extender = new CalendarExtender();
            extender.TargetControlID = textBox.ID;
            Controls.Add(extender);
        }

        private ScriptManager CurrentScriptManager
        {
            get { return ScriptManager.GetCurrent(Page); }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode) writer.Write("LoaderCssScript - '{0}'", ID);
            base.Render(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!DesignMode)
            {
                if (CurrentScriptManager != null)
                    CurrentScriptManager.RegisterScriptControl(this);
                ScriptObjectBuilder.RegisterCssReferences(this);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptObjectBuilder.RegisterCssReferences(this);
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            /*references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType(), false));
            references = references.Distinct(new ScriptReferencesComparer()).ToList();
            references.Add(new ScriptReference("Nat.Necl.Web.Validating.ClientValidator.js", "Nat.Necl.Web"));
            references.Add(new ScriptReference("Nat.Necl.Web.Validating.Validator.js", "Nat.Necl.Web"));
            references.Add(new ScriptReference("Nat.Necl.Web.Validating.ValidatedControl.js", "Nat.Necl.Web"));
            references.Add(new ScriptReference("Nat.Necl.Web.Validating.TypeExt.js", "Nat.Necl.Web"));
            //references.Add(new ScriptReference("AjaxControlToolkit.Common.Common.js", "AjaxControlToolkit, Version=1.0.11119.20010, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e"));
            references.Add(new ScriptReference("Nat.Web.Controls.DateTimeControls.DatePicker.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.ScrollSaver.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.Lookup.LookupTextBox.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.Lookup.AutoCompleteTextBox.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.Lookup.AutoCompleteTextBoxPair.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.TextBoxExt.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.DropDownListExtBehavior.js", "Nat.Web.Controls"));

            references.Add(new ScriptReference("Nat.Web.Controls.Filters.ColumnFilter.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.Filters.ColumnFilterList.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.Lookup.LookupTable.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.PopupControl.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.GridViewBehavior.js", "Nat.Web.Controls"));
            references.Add(new ScriptReference("Nat.Web.Controls.GridView.CheckedField.js", "Nat.Web.Controls"));

            references.Add(new ScriptReference("Nat.Web.Controls.EnableControls.EnableControlsBehavior.js", "Nat.Web.Controls"));

            foreach (ScriptReference reference in references)
                reference.NotifyScriptLoaded = false;*/
            return references;
        }

        private class ScriptReferencesComparer : IEqualityComparer<ScriptReference>
        {
            public bool Equals(ScriptReference x, ScriptReference y)
            {
                return string.IsNullOrEmpty(x.Name)
                           ? x.Path.Equals(y.Path)
                           : x.Name.Equals(y.Name);
            }

            public int GetHashCode(ScriptReference obj)
            {
                return string.IsNullOrEmpty(obj.Name) ? obj.Path.GetHashCode() : obj.Name.GetHashCode();
            }
        }
    }
}