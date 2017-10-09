/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Classes;
using Nat.Tools.Constants;
using Nat.Tools.ResourceTools;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;


#region Resources

[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTextBox.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTextBox.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTextBox.js", "text/javascript")]
[assembly: ScriptResource("Nat.Web.Controls.Lookup.LookupTextBox.js", "Nat.Web.Controls.ScriptResources.ScriptResources.resources", "Nat.Web.Controls.Resources")]

#endregion


namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.LookupTextBox", "Nat.Web.Controls.Lookup.LookupTextBox.js")]
    public class LookupTextBox : DataBoundControl, IScriptControl, 
        INamingContainer, IClientElementProvider, IEditableTextControl, INotTypeValidator
    {
        #region Fields

        private Button applyButton;
        private Button cancelButton;
        private String dataCodeField;
        private String dataTextField;
        private String dataValueField;
        private Boolean gridTreeMode;
        private HiddenField hiddenField;
        private LookupTable lookupTable;
        private Button nullButton;
        private String nullButtonID;
        private Button okButton;
        private String okButtonID;
        private PopupControl popupControl;
        private Button showModalButton;
        private AutoCompleteTextBoxPair textBox;
        private Boolean autoPostBack;
        private String dataDisableRowField;
        private Int32 conditionValue = 1;
        private String filterControlType;
        private Point popupPosition = new Point(0, 0);
        private Boolean fillGridViewFirstTime;
        private bool allowTreeAndPaging;

        #endregion


        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            hiddenField = new HiddenField();
            hiddenField.ID = "hiddenFieldID";
            Controls.Add(hiddenField);
            hiddenField.ValueChanged += HiddenField_OnValueChanged;

            Table table = new Table();
            table.ID = "tableID";
            Controls.Add(table);

            table.Width = Width;
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell {Width = Unit.Percentage(100)});
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());

            //table.Rows[0].Cells[0].BackColor = Color.Red;
            //table.Rows[0].Cells[1].BackColor = Color.Green;
            //table.Rows[0].Cells[2].BackColor = Color.Blue;

            textBox = new AutoCompleteTextBoxPair();
            textBox.ID = "textBoxID";
            table.Rows[0].Cells[0].Controls.Add(textBox);
            textBox.DataSource = DataSource;
            textBox.DataSourceID = DataSourceID;
            textBox.DataTextField = dataTextField;
            textBox.DataValueField = dataValueField;
            textBox.DataCodeField = dataCodeField;
            textBox.DataDisableRowField = dataDisableRowField;
            textBox.ConditionValue = conditionValue;
            textBox.AutoPostBack = autoPostBack;
            textBox.ItemChanged += TextBox_OnItemChanged;

            showModalButton = new Button();
            showModalButton.ID = "showModalButtonID";
            table.Rows[0].Cells[1].Controls.Add(showModalButton);
            showModalButton.Text = "...";
            showModalButton.OnClientClick = "return false;";

            nullButton = new Button();
            nullButtonID = ClientID + "_nullButtonID";
            nullButton.ID = nullButtonID;
            table.Rows[0].Cells[2].Controls.Add(nullButton);
            nullButton.Text = "x";
            nullButton.OnClientClick = "return false;";

            if(Width != Unit.Empty)
            {
                textBox.Width = Unit.Percentage(100);

                table.Rows[0].Cells[0].Style["padding-right"] = "6px";
                table.Rows[0].Cells[1].Width = 1;
                table.Rows[0].Cells[2].Width = 1;
            }

            {
                popupControl = new PopupControl();
                popupControl.ID = "popupControlID";
                Controls.Add(popupControl);
//#if LOCAL
                popupControl.Width = Unit.Pixel(1000);
//#else
//                popupControl.Width = Unit.Percentage(199);
//#endif
                popupControl.Position = popupPosition;
                if(DesignMode)
                    popupControl.Visible = false;

                lookupTable = new LookupTable();
                lookupTable.ID = "lookupTableID";
                lookupTable.FilterControlType = filterControlType;
                lookupTable.DataSource = DataSource;
                lookupTable.DataSourceID = DataSourceID;
                popupControl.Controls.Add(lookupTable);
                lookupTable.ShowFullBriefViewButton = true;
                lookupTable.GridTreeMode = gridTreeMode;
                lookupTable.AllowTreeAndPaging = allowTreeAndPaging;
                lookupTable.DataDisableRowField = dataDisableRowField;
                lookupTable.ConditionValue = conditionValue;
                lookupTable.FillGridViewFirstTime = fillGridViewFirstTime;
                lookupTable.GridHeight = Unit.Percentage(99);
                UpdateDataKeyNames();

                cancelButton = new Button();
                cancelButton.ID = "cancelButtonID";
                popupControl.Controls.Add(cancelButton);
                cancelButton.Text = LookupControlsResources.SCancel;
                cancelButton.Width = Unit.Pixel(85);
                cancelButton.Style["float"] = "right";
                cancelButton.OnClientClick = "return false;";

                okButton = new Button();
                okButtonID = ClientID + "_okButtonID";
                okButton.ID = okButtonID;
                popupControl.Controls.Add(okButton);
                okButton.Text = LookupControlsResources.SSelect;
                okButton.Width = Unit.Pixel(85);
                okButton.Style["float"] = "right";
                okButton.OnClientClick = "return false;";

                applyButton = new Button();
                applyButton.ID = "applyButton";
                popupControl.Controls.Add(applyButton);
                applyButton.Text = LookupControlsResources.SApplyFilter;
                applyButton.Style["float"] = "left";
                applyButton.OnClientClick = "return false;";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            TableDataSourceView view = (TableDataSourceView)GetData();
            view.FillType = TableDataSourceFillType.Never;

            String tableCaption = (String)(DataSetResourceManager.GetTableExtProperty(view.Table, TableExtProperties.CAPTION) ?? view.Table.TableName);
            String SSelectFromDBDirectory = LookupControlsResources.SSelectFromDBDirectory;
            popupControl.HeaderText = String.Format("{0} \"{1}\"", SSelectFromDBDirectory, tableCaption);

            if(AllowNullButton)
                nullButton.Style["display"] = "";
            else
                nullButton.Style["display"] = "none";
            
            // Because we don't use ViewState in GridView of LookupTable we have to
            // regenerate DataKeyNames from conrols where we stored this values earlier
            dataTextField = DataTextField;
            dataValueField = DataValueField;
            dataCodeField = DataCodeField;
            dataDisableRowField = DataDisableRowField;
            UpdateDataKeyNames();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(!DesignMode)
            {
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            }
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        private void UpdateDataKeyNames()
        {
            List<String> items = new List<String>();

            items.Add(dataValueField);
            items.Add(dataTextField);

            if (!String.IsNullOrEmpty(dataCodeField))
                items.Add(dataCodeField);

            if (!String.IsNullOrEmpty(dataDisableRowField))
                items.Add(dataDisableRowField);

            lookupTable.DataKeyNames = items.ToArray();
        }

        private void TextBox_OnItemChanged(object sender, EventArgs e)
        {
            OnItemChanged(e);
        }

        private void HiddenField_OnValueChanged(object sender, EventArgs e)
        {
            //if (hiddenField.Value == "1")
            //    popupControl.Show();
            //else if (hiddenField.Value == "0")
            //    popupControl.Hide();
        }


        public override void DataBind()
        {
            base.DataBind();
//            if (lookupTable.GridDataInitialized)
//                Refresh();
        }

        public void Refresh()
        {
            lookupTable.Refresh();
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

        [Browsable(false)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Value
        {
            get { return textBox.Value; }
            set { textBox.Value = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true, BindingDirection.TwoWay)]
        public String Text
        {
            get { return textBox.Text; }
            set { }
        }

        public override object DataSource
        {
            get { return base.DataSource; }
            set
            {
                base.DataSource = value;
                if(textBox != null)
                    textBox.DataSource = value;
                if(lookupTable != null)
                    lookupTable.DataSource = value;
            }
        }

        public override string DataSourceID
        {
            get { return base.DataSourceID; }
            set
            {
                base.DataSourceID = value;
                if(textBox != null)
                    textBox.DataSourceID = value;
                if(lookupTable != null)
                    lookupTable.DataSource = value;
            }
        }

        [Category("Data")]
        [DefaultValue("")]
        [Localizable(true)]
        public String DataTextField
        {
            get
            {
                if (textBox == null)
                    return dataTextField;
                return textBox.DataTextField;
            }
            set
            {
                if(textBox != null)
                    textBox.DataTextField = value;
                dataTextField = value;
                if (lookupTable != null)
                    UpdateDataKeyNames();
            }
        }

        [Category("Data")]
        [DefaultValue("")]
        [Localizable(false)]
        public String DataCodeField
        {
            get
            {
                if (textBox == null)
                    return dataCodeField;
                return textBox.DataCodeField;
            }
            set
            {
                if(textBox != null)
                    textBox.DataCodeField = value;
                dataCodeField = value;
                if (lookupTable != null)
                    UpdateDataKeyNames();
            }
        }

        [Category("Data")]
        [DefaultValue("")]
        [Localizable(false)]
        public String DataValueField
        {
            get
            {
                if (textBox == null)
                    return dataValueField;
                return textBox.DataValueField;
            }
            set
            {
                if(textBox != null)
                    textBox.DataValueField = value;
                dataValueField = value;
                if (lookupTable != null)
                    UpdateDataKeyNames();
            }
        }

        [Category("Data")]
        [DefaultValue("")]
        [Localizable(false)]
        public String DataDisableRowField
        {
            get
            {
                if (lookupTable != null)
                    return lookupTable.DataDisableRowField;
                return dataDisableRowField;
            }
            set
            {
                dataDisableRowField = value;
                if (lookupTable != null)
                {
                    lookupTable.DataDisableRowField = value;
                    UpdateDataKeyNames();
                }
                if (textBox != null)
                    textBox.DataDisableRowField = value;
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public Boolean FillGridViewFirstTime
        {
            get
            {
                if (lookupTable != null)
                    return lookupTable.FillGridViewFirstTime;
                else
                    return fillGridViewFirstTime;
            }
            set
            {
                fillGridViewFirstTime = value;
                if (lookupTable != null)
                    lookupTable.FillGridViewFirstTime = value;
            }
        }

        [Category("Behavior")]
        [DefaultValue("")]
        public String FilterControlType
        {
            get { return lookupTable.FilterControlType; }
            set
            {
                filterControlType = value;
                if (lookupTable != null)
                    lookupTable.FilterControlType = value;
            }
        }

        [Category("Data")]
        [DefaultValue(1)]
        [Localizable(false)]
        public Int32 ConditionValue
        {
            get { return lookupTable.ConditionValue; }
            set
            {
                conditionValue = value;
                if (lookupTable != null)
                    lookupTable.ConditionValue = value;
                if (textBox != null)
                    textBox.ConditionValue = value;
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Localizable(false)]
        public Boolean AutoPostBack
        {
            get { return textBox.AutoPostBack; }
            set
            {
                if (textBox != null)
                    textBox.AutoPostBack = value;
                autoPostBack = value;
            }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public Boolean AllowNullButton
        {
            get { return (Boolean)(ViewState["AllowNullButton"] ?? true); }
            set { ViewState["AllowNullButton"] = value; }
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        [Localizable(false)]
        public Boolean GridTreeMode
        {
            get { return lookupTable.GridTreeMode; }
            set
            {
                if(lookupTable != null)
                    lookupTable.GridTreeMode = value;
                gridTreeMode = value;
            }
        }

        [DefaultValue("")]
        public Point PopupPosition
        {
            get
            {
                if (popupControl != null)
                    return popupControl.Position;
                else
                    return popupPosition;
            }
            set
            {
                popupPosition = value;
                if (popupControl != null)
                    popupControl.Position = value;
            }
        }

        [DefaultValue(false)]
        public bool AllowTreeAndPaging
        {
            get { return lookupTable.AllowTreeAndPaging; }
            set
            {
                if (lookupTable != null)
                    lookupTable.AllowTreeAndPaging = value;
                allowTreeAndPaging = value;
            }
        }

        #endregion


        #region IClientElementProvider Members

        public ICollection<string> GetInputElements()
        {
            List<String> list = new List<string>();
            list.AddRange(textBox.GetInputElements());
            return list.ToArray();
        }

        public ICollection<Control> GetInputControls()
        {
            List<Control> list = new List<Control>();
            list.AddRange(textBox.GetInputControls());
            return list.ToArray();
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            List<Pair<string, IFormatProvider>> list =
                new List<Pair<string, IFormatProvider>>();
            list.AddRange(textBox.GetInputFormatProviders());
            return list.ToArray();
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                ScriptControlDescriptor desc = new ScriptControlDescriptor("Nat.Web.Controls.LookupTextBox", ClientID);

                desc.AddProperty("hiddenFieldID", hiddenField.ClientID);

                desc.AddProperty("okButtonID", okButton.ClientID);
                desc.AddProperty("cancelButtonID", cancelButton.ClientID);
                desc.AddProperty("applyButtonID", applyButton.ClientID);
                desc.AddProperty("showModalButtonID", showModalButton.ClientID);
                desc.AddProperty("nullButtonID", nullButton.ClientID);

                desc.AddProperty("textBoxID", textBox.ClientID);
                desc.AddProperty("lookupTableID", lookupTable.ClientID);

                desc.AddProperty("popupControlID", popupControl.ClientID);

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