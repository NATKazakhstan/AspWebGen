/*
 * Created by: Roman V. Kurbangaliev
 * Created: 06.11.2008
 * Copyright © JSC NAT Kazakhstan 2008
 */

using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DataBinding.Tools;

namespace Nat.Web.Controls
{
    using System;

    public class EnableByDropDownList : IEnableControl
    {
        private WebControl _control;

        private StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// При совпадении с TextBox-ом выставляется значение активности по свойству Disable.
        /// По умолчанию false.
        /// </summary>
        [DefaultValue(null)]
        [Description("При совпадении с DropDownList-ом выставляется значение активности по свойству Disable.")]
        public string Value { get; set; }

        [ScriptIgnore]
        public DropDownList DropDownList
        {
            get
            {
                if (_control == null && EnableControls != null)
                {
                    if (EnableControls.Owner != null)
                        _control = (DropDownList)WebUtils.FindControlRecursive(EnableControls.Owner, ControlID);
                    else
                        _control = (DropDownList)WebUtils.FindControlRecursive(EnableControls.Page, ControlID);
                }
                return _control as DropDownList;
            }
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

        #region IEnableControl Members

        /// <summary>
        /// Дизаблить контрол при выполнении условий.
        /// По умолчанию false.
        /// </summary>
        [DefaultValue(false)]
        [Description("Дизаблить контрол при выполнении условий.")]
        public bool Disable { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ScriptIgnore]
        public EnableControls EnableControls { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ScriptIgnore]
        public Control Owner { get; set; }

        public string GetClientID()
        {
            if (DropDownList == null) return Control != null ? Control.ClientID : null;
            return DropDownList.ClientID;
        }

        public string ControlID { get; set; }

        public void RefreshValues()
        {
        }

        public TypeComponent TypeComponent
        {
            get { return TypeComponent.ValueControl; }
        }

        public bool IsEquals()
        {
            var value = DropDownList != null ? DropDownList.SelectedValue : (Control != null ? Control.Attributes["value"] : "");
            return string.Equals(Value, value, ComparisonType);
        }

        public StringComparison ComparisonType
        {
            get { return comparisonType; }
            set { comparisonType = value; }
        }

        public IEnableControl Clone()
        {
            return new EnableByDropDownList
                {
                    _control = _control,
                    ControlID = ControlID,
                    Disable = Disable,
                    EnableControls = EnableControls,
                    Owner = Owner,
                    Value = Value,
                    ComparisonType = ComparisonType,
                };
        }

        #endregion
    }
}