using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools.Classes;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [SupportsEventValidation]
    [ValidationProperty("SelectedValue")]
    [ControlValueProperty("SelectedValue")]
    public class CodeDropDownListExt : DataBoundControl, IEditableTextControl, IClientElementProvider
    {

        #region Fields

        private Panel panel;
        private DropDownListExt dropDownListExtCode;
        private DropDownListExt dropDownListExtText;
        private bool dropDownListExtIncludeNullItem = true;
        private string dataValueField;
        private string dataCodeField;
        private string dataTextField;

        #endregion Fields

        public event EventHandler SelectedIndexChanged;
        public event EventHandler TextChanged;

        #region Implementation of IClientElementProvider

        public ICollection<string> GetInputElements()
        {
            return new[]
                       {
                           dropDownListExtCode.ClientID,
                           dropDownListExtText.ClientID,
                       };
        }

        public ICollection<Control> GetInputControls()
        {
            return new Control[]
                       {
                           dropDownListExtCode,
                           dropDownListExtText
                       };
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            return new[]
                {
                    new Pair<string, IFormatProvider>(dropDownListExtCode.ClientID, null),
                    new Pair<string, IFormatProvider>(dropDownListExtText.ClientID, null),

                };
        }

        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            dropDownListExtCode = new SortDropDownListExt
                                      {
                                          ID = (ID + "_code"),
                                          DataSourceID = DataSourceID,
                                          DataValueField = dataValueField,
                                          DataTextField = dataCodeField,
                                          IncludeNullItem = dropDownListExtIncludeNullItem,
                                          DataMember = "DefaultView",
                                          AppendDataBoundItems = true,
                                          Width = Unit.Percentage(26)
                                      };
            dropDownListExtCode.SelectedIndexChanged += DropDownListExtCode_OnSelectedIndexChanged;

            dropDownListExtText = new SortDropDownListExt();
            dropDownListExtText.ID = ID + "_text";
            dropDownListExtText.DataSourceID = DataSourceID;
            dropDownListExtText.DataValueField = dataValueField;
            dropDownListExtText.DataTextField = dataTextField;
            dropDownListExtText.IncludeNullItem = dropDownListExtIncludeNullItem;
            dropDownListExtText.DataMember = "DefaultView";
            dropDownListExtText.Width = Unit.Percentage(74);
            dropDownListExtText.AppendDataBoundItems = true;
            dropDownListExtText.TextChanged += DropDownListExtText_TextChanged;
            dropDownListExtText.Attributes.Add("onchange", "CodeDropDownListExtSynchronizeCode(this)");
            dropDownListExtText.Attributes.Add("role", "text");
            dropDownListExtCode.Attributes.Add("onchange", "CodeDropDownListExtSynchronizeText(this)");
            dropDownListExtCode.Attributes.Add("role", "code");

            Controls.Add(panel = new Panel());
            panel.Style[HtmlTextWriterStyle.Overflow] = "hidden";
            panel.Style[HtmlTextWriterStyle.PaddingRight] = "3px";
            panel.Style[HtmlTextWriterStyle.WhiteSpace] = "nowrap";
            panel.Controls.Add(dropDownListExtCode);
            panel.Controls.Add(dropDownListExtText);
        }

        private void DropDownListExtText_TextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
                TextChanged(this, e);
        }

        private void DropDownListExtCode_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, e);
        }

//        protected override int CreateChildControls(System.Collections.IEnumerable dataSource, bool dataBinding)
//        {
//            return Controls.Count;
//        }

        #endregion Methods

        #region Properties

        [DefaultValue(""), Bindable(true, BindingDirection.OneWay), Category("Data")]
        public string DataValueField
        {
            get
            {
                if (dropDownListExtCode != null)
                    return dropDownListExtCode.DataValueField;
                return dataValueField;
            }
            set
            {
                if (dropDownListExtCode != null)
                    dropDownListExtCode.DataValueField = value;
                if (dropDownListExtText != null)
                    dropDownListExtText.DataValueField = value;
                dataValueField = value;
            }
        }

        [DefaultValue(""), Bindable(true, BindingDirection.TwoWay), Category("Data")]
        public string DataCodeField
        {
            get
            {
                if (dropDownListExtCode != null)
                    return dropDownListExtCode.DataTextField;
                return dataCodeField;
            }
            set
            {
                if (dropDownListExtCode != null)
                    dropDownListExtCode.DataTextField = value;
                dataCodeField = value;
            }
        }

        [DefaultValue(""), Bindable(true, BindingDirection.TwoWay), Category("Data")]
        public string DataTextField
        {
            get
            {
                if (dropDownListExtText != null)
                    return dropDownListExtText.DataTextField;
                return dataTextField;
            }
            set
            {
                if (dropDownListExtText != null)
                    dropDownListExtText.DataTextField = value;
                dataTextField = value;
            }
        }

        [Category("Behavior"), DefaultValue(false), Description("Включать в список null-элемент.")]
        public bool DropDownListExtIncludeNullItem
        {
            get { return dropDownListExtIncludeNullItem;  }
            set { dropDownListExtIncludeNullItem = value; }
        }

        public string GetCode
        {
            get { return dropDownListExtCode.SelectedItem.Text; }
        }

        public string SelectedValue
        {
            get
            {
                if (dropDownListExtCode.SelectedValue == null) return null;
                return dropDownListExtCode.SelectedValue.ToString();
            }
            set { dropDownListExtCode.SelectedValue = dropDownListExtText.SelectedValue = value; }
        }

        public DropDownListExt DropDownListExtCode
        { 
            get { return dropDownListExtCode; }
        }

        public DropDownListExt DropDownListExtText
        {
            get { return dropDownListExtText; }
        }

        public string Text
        {
            get { return dropDownListExtText.Text; }
            set { }
        }

        #endregion Properties

        internal class SortDropDownListExt : DropDownListExt
        {
            protected override void OnDataBinding(EventArgs e)
            {
                var data = GetData();
                var tableData = data as TableDataSourceView;
                if (tableData != null) tableData.SortExpression = DataTextField;
                var linqData = data as LinqDataSourceView;
//                if(linqData != null) linqData.OrderBy = DataTextField;
                Items.Clear();
                base.OnDataBinding(e);
            }
        }
    }
}