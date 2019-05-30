/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Classes;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Web.Tools;

#region Resources

[assembly : WebResource("Nat.Web.Controls.Lookup.AutoCompleteTextBox.css", "text/css", PerformSubstitution = true)]
[assembly : WebResource("Nat.Web.Controls.Lookup.AutoCompleteTextBox.png", "image/png")]
[assembly : WebResource("Nat.Web.Controls.Lookup.AutoCompleteTextBox.js", "text/javascript")]

#endregion

namespace Nat.Web.Controls
{
    [ClientCssResource("Nat.Web.Controls.Lookup.AutoCompleteTextBox.css")]
    [ClientScriptResource("Nat.Web.Controls.AutoCompleteTextBox", "Nat.Web.Controls.Lookup.AutoCompleteTextBox.js")]
    public class AutoCompleteTextBox : DataBoundControl, IClientElementProvider,
                                       IScriptControl, INamingContainer, IEditableTextControl
    {
        #region Fields

        private AutoCompleteExtender autoCompleteExtender;
        private HiddenField hiddenField;
        private TextBoxExt textBox;

        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            hiddenField = new HiddenField();
            hiddenField.ID = "hiddenFieldID";
            Controls.Add(hiddenField);
            hiddenField.ValueChanged += HiddenField_OnValueChanged;

            textBox = new TextBoxExt();
            textBox.ID = "textBoxID";
            Controls.Add(textBox);

            autoCompleteExtender = new AutoCompleteExtender();
            autoCompleteExtender.ID = "autoCompleteExtenderID";
            Controls.Add(autoCompleteExtender);
            autoCompleteExtender.TargetControlID = textBox.ID;

            autoCompleteExtender.ServicePath = "~/autocomplete.asmx";
            autoCompleteExtender.ServiceMethod = "GetCompletionList";
            autoCompleteExtender.UseContextKey = true;
            autoCompleteExtender.MinimumPrefixLength = 1;
            autoCompleteExtender.CompletionListCssClass = "autocomplete_completionListElement";
            autoCompleteExtender.CompletionListHighlightedItemCssClass = "autocomplete_highlightedListItem";
            autoCompleteExtender.CompletionListItemCssClass = "autocomplete_listItem";
            autoCompleteExtender.FirstRowSelected = true;
            autoCompleteExtender.EnableCaching = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            autoCompleteExtender.ContextKey = WebMethodSessionKey;

            base.OnLoad(e);
        }

        private void HiddenField_OnValueChanged(object sender, EventArgs e)
        {
            if (hiddenField.Value != "")
            {
                ViewState["Value"] = hiddenField.Value;
            }
            else
                ViewState["Value"] = null;
            OnItemChanged(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            textBox.Width = Width;


            if (!DesignMode)
            {
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
                ScriptObjectBuilder.RegisterCssReferences(this);
            }

            FillProps();

            RequiresDataBinding = false;
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);

            base.Render(writer);
        }

        private void FillProps()
        {
            TableDataSourceView _data = (TableDataSourceView) GetData();

            if (!DesignMode &&
                _data != null &&
                !String.IsNullOrEmpty(DataTextField) && !String.IsNullOrEmpty(DataValueField))
            {
                Hashtable props = new Hashtable();
                props["ShowHistoricalData"] = _data.ShowHistoricalData;
                props["LoadAllHistoricalData"] = _data.LoadAllHistoricalData;
                props["EndDateField"] = _data.EndDateField;
                props["StartDateField"] = _data.StartDateField;
                props["HistolicalPoint"] = _data.HistolicalPoint;
                props["QueryConditionsList"] = _data.CustomConditions;
                props["FilterParameters"] = _data.GetFilterParameters();
                props["SelectMethod"] = _data.SelectMethod;
                props["TypeName"] = _data.TypeName;
                props["SelectParameters"] = _data.GetSelectParameters();
                props["DataTextField"] = DataTextField;
                props["DataCodeField"] = DataCodeField;
                props["DataValueField"] = DataValueField;
                props["DataDisableRowField"] = DataDisableRowField;
                props["ConditionValue"] = ConditionValue;
                props["FilterExpression"] = _data.FilterExpression;
                props["SetFilterByCustomConditions"] = _data.SetFilterByCustomConditions;

                Page.Session[WebMethodSessionKey] = props;
            }
        }

        public void EnsureTextByValue(Object value)
        {
            if (!String.IsNullOrEmpty(Convert.ToString(value)))
            {
                TableDataSourceView _data = (TableDataSourceView) GetData();
                TableDataSourceView tableDataSourceView = new TableDataSourceView(
                    null,
                    _data.ShowHistoricalData,
                    _data.LoadAllHistoricalData,
                    _data.EndDateField,
                    _data.StartDateField,
                    _data.HistolicalPoint,
                    _data.CustomConditions,
                    _data.SelectMethod,
                    _data.TypeName,
                    _data.GetSelectParameters(),
                    _data.GetFilterParameters(),
                    _data.FilterExpression,
                    _data.SetFilterByCustomConditions);
                if (_data.HistoricalCountKeys == 1)
                {
                    tableDataSourceView.HistoricalKey = _data.HistoricalKey;
                    tableDataSourceView.HistoricalSelectMethod = _data.HistoricalSelectMethod;
                    tableDataSourceView.HistoricalCountKeys = 1;
                    tableDataSourceView.HistoricalValues[0] = value;
                }
                QueryCondition queryCondition = new QueryCondition(DataValueField, ColumnFilterType.Equal, value, null);
                tableDataSourceView.CustomConditions.Add(queryCondition);
                DataView dataView;
                try
                {
                    DataSourceSelectArguments arguments = new DataSourceSelectArguments();
                    dataView =
                        (DataView) tableDataSourceView.Select(true, arguments, new DataSourceSelectExtArguments(1));
                }
                finally
                {
                    tableDataSourceView.CustomConditions.Remove(queryCondition);
                }

                //выполнение запроса должно помещать 1 запись в таблицу
                if (dataView.Table.Rows.Count == 1)
                {
                    Text = dataView.Table.Rows[0][DataTextField].ToString();
                }
                else if (dataView.Count == 1)
                {
                    Text = dataView[0][DataTextField].ToString();
                }
                else
                {
                    Text = "";
                }
            }
            else
            {
                Text = "";
            }
            textBox.Text = Text;
        }

        #endregion

        #region Events

        public event EventHandler ItemChanged;

        protected virtual void OnItemChanged(EventArgs e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);

            if (TextChanged != null)
                TextChanged(this, e);
        }

        #endregion

        #region Properties

        private string WebMethodSessionKey
        {
            get
            {
                if (ViewState["WebMethodSessionKey"] == null)
                    ViewState["WebMethodSessionKey"] = Guid.NewGuid().ToString();
                return (string)ViewState["WebMethodSessionKey"];
            }
            set
            {
                ViewState["WebMethodSessionKey"] = value;
            }
        }

        [Browsable(false)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Value
        {
            get
            {
                if (hiddenField != null &&
                    !hiddenField.Value.Equals(Convert.ToString(ViewState["Value"])))
                {
                    if (hiddenField.Value != "")
                        ViewState["Value"] = hiddenField.Value;
                    else
                        ViewState["Value"] = null;
                    Text = textBox.Text;
                }
                return ViewState["Value"] ?? "";
            }
            set
            {
                if (ViewState["Value"] != value)
                {
                    ViewState["Value"] = value;
                    EnsureTextByValue(value);
                }
                hiddenField.Value = Convert.ToString(value);
            }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataTextField
        {
            get { return (String) (ViewState["DataTextField"] ?? ""); }
            set { ViewState["DataTextField"] = value; }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataValueField
        {
            get { return (String) (ViewState["DataValueField"] ?? ""); }
            set { ViewState["DataValueField"] = value; }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataCodeField
        {
            get { return (String) (ViewState["DataCodeField"] ?? ""); }
            set { ViewState["DataCodeField"] = value; }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataDisableRowField
        {
            get { return (String) (ViewState["DataDisableRowField"] ?? ""); }
            set { ViewState["DataDisableRowField"] = value; }
        }

        [DefaultValue(1)]
        [Category("Data")]
        public Int32 ConditionValue
        {
            get { return (Int32) (ViewState["ConditionValue"] ?? 1); }
            set { ViewState["ConditionValue"] = value; }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public Boolean AutoPostBack
        {
            get { return textBox.AutoPostBack; }
            set
            {
                if (textBox == null) OnInit(EventArgs.Empty);
                textBox.AutoPostBack = value;
            }
        }

        [Browsable(false)]
        [Bindable(true, BindingDirection.OneWay)]
        public String Text
        {
            get { return (String) ViewState["Text"]; }
            set { ViewState["Text"] = value; }
        }

        #endregion

        #region IClientElementProvider Members

        public ICollection<string> GetInputElements()
        {
            return new String[] {textBox.ClientID};
        }

        public ICollection<Control> GetInputControls()
        {
            return new Control[] {textBox};
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            return new Pair<string, IFormatProvider>[]
                {
                    new Pair<string, IFormatProvider>(textBox.ClientID, null)
                };
        }

        #endregion

        #region IEditableTextControl Members

        public event EventHandler TextChanged;

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.AutoCompleteTextBox", ClientID);

                desc.AddProperty("hiddenFieldID", hiddenField.ClientID);
                desc.AddProperty("autoCompleteBehaviorID", autoCompleteExtender.BehaviorID);

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
    }
}