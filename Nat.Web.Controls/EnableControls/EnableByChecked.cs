/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 7 ������ 2008 �.
 * Copyright � JSC New Age Technologies 2008
 */

using System;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DataBinding.Tools;

namespace Nat.Web.Controls
{
    public class EnableByChecked : IEnableControl
    {
        private WebControl _control;

        /// <summary>
        /// ��� ���������� � CheckBox-�� ������������ �������� ���������� �� �������� Disable.
        /// �� ��������� false.
        /// </summary>
        [DefaultValue(false)]
        [Description("��� ���������� � CheckBox-�� ������������ �������� ���������� �� �������� Disable.")]
        public bool Checked { get; set; }

        [ScriptIgnore]
        public CheckBox CheckBox
        {
            get
            {
                if (_control == null && EnableControls != null)
                {
                    if (EnableControls.Owner != null)
                        _control = WebUtils.FindControlRecursive(EnableControls.Owner, ControlID) as CheckBox;
                    else
                        _control = WebUtils.FindControlRecursive(EnableControls.Page, ControlID) as CheckBox;
                }

                return _control as CheckBox;
            }

            set
            {
                _control = value;
                ControlID = value != null ? value.ID : null;
            }
        }

        [ScriptIgnore]
        public RadioButtonList RadioButtonList
        {
            get
            {
                if (_control == null && EnableControls != null)
                {
                    if (EnableControls.Owner != null)
                        _control = WebUtils.FindControlRecursive(EnableControls.Owner, ControlID) as RadioButtonList;
                    else
                        _control = WebUtils.FindControlRecursive(EnableControls.Page, ControlID) as RadioButtonList;
                }

                return _control as RadioButtonList;
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
                ControlID = value != null ? value.ID: null;
            }
        }

        #region IEnableControl Members

        /// <summary>
        /// ��������� ������� ��� ���������� �������.
        /// �� ��������� false.
        /// </summary>
        [DefaultValue(false)]
        [Description("��������� ������� ��� ���������� �������.")]
        public bool Disable { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ScriptIgnore]
        public EnableControls EnableControls { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ScriptIgnore]
        public Control Owner { get; set; }

        public string GetClientID()
        {
            if (CheckBox == null) return Control != null ? Control.ClientID : null;
            return CheckBox.ClientID;
        }

        public string ControlID { get; set; }

        public void RefreshValues()
        {
        }

        public TypeComponent TypeComponent
        {
            get { return TypeComponent.CheckedControl; }
        }

        public bool IsEquals()
        {
            if (CheckBox != null)
                return CheckBox.Checked == Checked;

            if (RadioButtonList != null)
            {
                if (string.IsNullOrEmpty(RadioButtonList.SelectedValue)) return false;
                return Convert.ToBoolean(RadioButtonList.SelectedValue) == Checked;
            }

            if (string.IsNullOrEmpty(Control.Attributes["checked"])) return false;
            return Convert.ToBoolean(Control.Attributes["checked"]) == Checked;
        }

        public IEnableControl Clone()
        {
            return new EnableByChecked
                       {
                           _control = _control,
                           ControlID = ControlID,
                           Checked = Checked,
                           Disable = Disable,
                           EnableControls = EnableControls,
                           Owner = Owner,
                       };
        }

        #endregion
    }
}