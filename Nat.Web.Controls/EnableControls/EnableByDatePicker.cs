/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 октября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools.Specific;
using Nat.Web.Controls.DataBinding.Tools;
using Nat.Web.Controls.DateTimeControls;

namespace Nat.Web.Controls
{
    public class EnableByDatePicker : IEnableControl
    {
        public EnableByDatePicker()
        {
            DatePickerMode = DatePickerMode.Date;
        }

        private WebControl _control;

        /// <summary>
        /// При совпадении с DatePicker-ом выставляется значение активности по свойству Disable.
        /// По умолчанию false.
        /// </summary>
        [DefaultValue(null)]
        [Description("При совпадении с DatePicker-ом выставляется значение активности по свойству Disable.")]
        public string Value { get; set; }

        [DefaultValue(DatePickerMode.Date)]
        public DatePickerMode DatePickerMode { get; set; }

        [ScriptIgnore]
        public DatePicker DatePicker
        {
            get
            {
                if (_control == null && EnableControls != null)
                {
                    if (EnableControls.Owner != null)
                        _control = (DatePicker) WebUtils.FindControlRecursive(EnableControls.Owner, ControlID);
                    else
                        _control = (DatePicker) WebUtils.FindControlRecursive(EnableControls.Page, ControlID);
                }
                return _control as DatePicker;
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
            if (DatePicker == null) return Control != null ? Control.ClientID : null;
            return DatePicker.FindControl("textBoxID").ClientID;
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
            DateTime? date = DatePicker != null
                                 ? (DateTime?) DatePicker.Date
                                 : (Control != null && !string.IsNullOrEmpty(Control.Attributes["value"])
                                        ? (DateTime?) Convert.ToDateTime(Control.Attributes["value"])
                                        : null);
            if (string.IsNullOrEmpty(Value))
                return date == null;
            if (DatePickerMode == DatePickerMode.Time)
                return date == DateTime.Parse(SpecificInstances.DbConstants.MinDate.ToString("dd.MM.yyyy ") + Value);
            return date == DateTime.Parse(Value);
        }

        public IEnableControl Clone()
        {
            return new EnableByDatePicker
                       {
                           _control = _control,
                           ControlID = ControlID,
                           DatePickerMode = DatePickerMode,
                           Disable = Disable,
                           EnableControls = EnableControls,
                           Owner = Owner,
                           Value = Value,
                       };
        }

        #endregion
    }
}