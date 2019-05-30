/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Classes;
using Nat.Web.Tools;


#region Resources

[assembly: WebResource("Nat.Web.Controls.Lookup.AutoCompleteTextBoxPair.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Lookup.AutoCompleteTextBoxPair.js", "text/javascript")]

#endregion


namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.AutoCompleteTextBoxPair", "Nat.Web.Controls.Lookup.AutoCompleteTextBoxPair.js")]
    public class AutoCompleteTextBoxPair : DataBoundControl, IScriptControl, 
        INamingContainer, IClientElementProvider, IEditableTextControl
    {
        #region Fields

        private AutoCompleteTextBox codeTextBox;
        private AutoCompleteTextBox mainTextBox;
        private Table table;

        #endregion


        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            table = new Table();
            table.ID = "tableID";
            Controls.Add(table);
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());

            codeTextBox = new AutoCompleteTextBox();
            codeTextBox.ID = "codeTextBoxID";
            table.Rows[0].Cells[0].Controls.Add(codeTextBox);

            mainTextBox = new AutoCompleteTextBox();
            mainTextBox.ID = "mainTextBoxID";
            table.Rows[0].Cells[1].Controls.Add(mainTextBox);
            mainTextBox.ItemChanged += MainTextBox_OnItemChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            table.Width = Width == Unit.Empty ? Unit.Percentage(100) : Width;
            Boolean IsDataCodeField = !String.IsNullOrEmpty(DataCodeField);

            table.CellPadding = 0;
            table.CellSpacing = 0;

            table.Rows[0].Cells[0].Visible = IsDataCodeField;
            table.Rows[0].Cells[0].Width = CodeWidth == Unit.Empty ? Unit.Percentage(30) : CodeWidth;
            codeTextBox.Width = Unit.Percentage(100);

            table.Rows[0].Cells[1].Style["padding-right"] = "6px";
            mainTextBox.Width = Unit.Percentage(100);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);

            RequiresDataBinding = false;
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);

            base.Render(writer);
        }

        private void MainTextBox_OnItemChanged(object sender, EventArgs e)
        {
            OnItemChanged(e);
        }

        #endregion


        #region Events

        public event EventHandler ItemChanged;

        protected virtual void OnItemChanged(EventArgs e)
        {
            if(ItemChanged != null)
                ItemChanged(this, e);

            if (TextChanged != null)
                TextChanged(this, e);
        }

        #endregion


        #region Properties

        public override object DataSource
        {
            get { return base.DataSource; }
            set
            {
                base.DataSource = value;
                if (mainTextBox == null) OnInit(EventArgs.Empty);
                mainTextBox.DataSource = value;
                codeTextBox.DataSource = value;
            }
        }

        public override string DataSourceID
        {
            get { return base.DataSourceID; }
            set
            {
                base.DataSourceID = value;
                mainTextBox.DataSourceID = value;
                codeTextBox.DataSourceID = value;
            }
        }

        [Browsable(false)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Value
        {
            get { return mainTextBox.Value; }
            set
            {
                mainTextBox.Value = value;
                if(!String.IsNullOrEmpty(DataCodeField))
                    codeTextBox.Value = value;
            }
        }

        [Browsable(false)]
        [Bindable(true, BindingDirection.OneWay)]
        public String Text
        {
            get { return mainTextBox.Text; }
            set { }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataTextField
        {
            get { return mainTextBox.DataTextField; }
            set
            {
                mainTextBox.DataTextField = value;
                codeTextBox.DataCodeField = value;
            }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataCodeField
        {
            get { return mainTextBox.DataCodeField; }
            set
            {
                mainTextBox.DataCodeField = value;
                codeTextBox.DataTextField = value;
            }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataValueField
        {
            get { return mainTextBox.DataValueField; }
            set
            {
                mainTextBox.DataValueField = value;
                codeTextBox.DataValueField = value;
            }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataDisableRowField
        {
            get { return mainTextBox.DataDisableRowField; }
            set
            {
                mainTextBox.DataDisableRowField = value;
                codeTextBox.DataDisableRowField = value;
            }
        }

        [DefaultValue(1)]
        [Category("Data")]
        public Int32 ConditionValue
        {
            get { return mainTextBox.ConditionValue; }
            set
            {
                mainTextBox.ConditionValue = value;
                codeTextBox.ConditionValue = value;
            }
        }

        [DefaultValue("")]
        [Category("Appearance")]
        public Unit CodeWidth
        {
            get { return (Unit)(ViewState["CodeWidth"] ?? Unit.Empty); }
            set { ViewState["CodeWidth"] = value; }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public Boolean AutoPostBack
        {
            get { return mainTextBox.AutoPostBack; }
            set { mainTextBox.AutoPostBack = value; }
        }

        #endregion


        #region IClientElementProvider Members

        public ICollection<string> GetInputElements()
        {
            List<String> list = new List<string>();
            list.AddRange(mainTextBox.GetInputElements());
            if(codeTextBox != null)
                list.AddRange(codeTextBox.GetInputElements());
            return list.ToArray();
        }

        public ICollection<Control> GetInputControls()
        {
            List<Control> list = new List<Control>();
            list.AddRange(mainTextBox.GetInputControls());
            if(codeTextBox != null)
                list.AddRange(codeTextBox.GetInputControls());
            return list.ToArray();
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            List<Pair<string, IFormatProvider>> list = new List<Pair<string, IFormatProvider>>();
            list.AddRange(mainTextBox.GetInputFormatProviders());
            if(codeTextBox != null)
                list.AddRange(codeTextBox.GetInputFormatProviders());
            return list.ToArray();
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                ScriptControlDescriptor desc = new ScriptControlDescriptor("Nat.Web.Controls.AutoCompleteTextBoxPair", ClientID);

                desc.AddProperty("mainBehaviorID", mainTextBox.ClientID);
                desc.AddProperty("codeBehaviorID", codeTextBox.ClientID);

                desc.AddProperty("dataValueField", DataValueField);
                desc.AddProperty("dataTextField", DataTextField);
                desc.AddProperty("dataCodeField", DataCodeField);

                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion


        #region IEditableTextControl Members

        public event EventHandler TextChanged;

        #endregion
    }
}