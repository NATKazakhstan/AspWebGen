using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DataBinding.Tools;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    using System.Linq;

    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Controls.SelectValues;

    [ParseChildren(true)]
    [PersistChildren(false)]
    public class EnableBySelectedValue : IEnableControl
    {
        private static readonly MethodInfo methodInfo =
            typeof(DataBoundControl).GetMethod("GetData", BindingFlags.Instance | BindingFlags.NonPublic);

        private WebControl _control;

        public EnableBySelectedValue()
        {
            CodeField = "code";
            ValueField = "id";
        }

        [DefaultValue("id")]
        public string ValueField { get; set; }

        [DefaultValue("code")]
        public string CodeField { get; set; }

        [DefaultValue(null)]
        public string Code { get; set; }


        [ScriptIgnore]
        public DataBoundControl DataBoundControl
        {
            get
            {
                if (_control == null && EnableControls != null && !string.IsNullOrEmpty(ControlID))
                {
                    if (EnableControls.Owner != null)
                        _control = (DataBoundControl)WebUtils.FindControlRecursive(EnableControls.Owner, ControlID);
                    else
                        _control = (DataBoundControl)WebUtils.FindControlRecursive(EnableControls.Page, ControlID);
                }
                return _control as DataBoundControl;
            }
            set
            {
                _control = value;
                ControlID = value != null ? value.ID : null;
            }
        }

        [ScriptIgnore]
        public ListControl ListControl
        {
            get { return DataBoundControl as ListControl; }
            set
            {
                _control = value;
                ControlID = value != null ? value.ID : null;
            }
        }

        [ScriptIgnore]
        public BaseListDataBoundControl BaseListDataBoundControl
        {
            get { return DataBoundControl as BaseListDataBoundControl; }
            set
            {
                _control = value;
                ControlID = value != null ? value.ID : null;
            }
        }

        [ScriptIgnore]
        public WebControl Control
        {
            get
            {
                return _control;
            }
            set
            {
                _control = value;
                ControlID = value != null ? value.ID : null;
            }
        }

        public string Value { get; set; }

        public bool Checked { get; set; }

        #region IEnableControl Members

        [DefaultValue(false)]
        public bool Disable { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ScriptIgnore]
        public EnableControls EnableControls { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ScriptIgnore]
        public Control Owner { get; set; }

        public string GetClientID()
        {
            if (!string.IsNullOrEmpty(ClientID))
                return ClientID;

            if (DataBoundControl == null) return Control != null ? Control.ClientID : null;
            return DataBoundControl.ClientID;
        }

        public string ControlID { get; set; }

        [ScriptIgnore]
        public string ClientID { get; set; }

        public void RefreshValues()
        {
            if (!string.IsNullOrEmpty(Code) && ListControl != null)
            {
                var data = (DataSourceView)methodInfo.Invoke(ListControl, new object[0]);
                var view = data as TableDataSourceView;
                if (view == null)
                    throw new Exception(
                        string.Format("Конрол '{0}' не использует TableDataSourceView в виде источника", ListControl.ID));
                var dic = new Dictionary<string, string> {{CodeField, Code}};
                DataRowView row = FindHelper.FindRow(dic,
                                                     new DataView(view.Table, view.GetFilterString(), "",
                                                                  DataViewRowState.CurrentRows));
                if (row != null)
                    Value = row[ValueField].ToString();
                else
                    throw new Exception(string.Format("Table '{0}' not containt code in '{1}'", view.Table.TableName,
                                                      EnableControls.ID));
            }

            if (!string.IsNullOrEmpty(Value) && BaseListDataBoundControl != null)
            {
                ClientID = BaseListDataBoundControl.GetClientID(Value);
                ControlID = ClientID;
            }
        }

        public TypeComponent TypeComponent
        {
            get
            {
                if (BaseListDataBoundControl != null)
                    return BaseListDataBoundControl.GetTypeComponent();
                return TypeComponent.ValueControl;
            }
        }

        public bool IsEquals()
        {
            string value;
            if (ListControl != null) 
                value = ListControl.SelectedValue;
            else if (BaseListDataBoundControl != null)
                return !Checked ^ BaseListDataBoundControl.SelectedValues.Contains(Value);
            else if (Control != null) 
                value = Control.Attributes["value"];
            else 
                value = string.Empty;
            
            return string.Equals(Value, value);
        }

        public IEnableControl Clone()
        {
            return new EnableBySelectedValue
                       {
                           _control = _control,
                           Code = Code,
                           CodeField = CodeField,
                           ControlID = ControlID,
                           Disable = Disable,
                           EnableControls = EnableControls,
                           Owner = Owner,
                           Value = Value,
                           ValueField = ValueField,
                       };
        }

        #endregion
    }
}