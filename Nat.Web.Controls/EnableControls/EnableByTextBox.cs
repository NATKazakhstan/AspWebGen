/*
 * Created by: Evgeny P. Kolesnikov
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
    public class EnableByTextBox : IEnableControl
    {
        private WebControl _control;

        /// <summary>
        /// При совпадении с TextBox-ом выставляется значение активности по свойству Disable.
        /// По умолчанию false.
        /// </summary>
        [DefaultValue(null)]
        [Description("При совпадении с TextBox-ом выставляется значение активности по свойству Disable.")]
        public string Value { get; set; }

        [ScriptIgnore]
        public TextBox TextBox
        {
            get
            {
                if (_control == null && EnableControls != null)
                {
                    if (EnableControls.Owner != null)
                        _control = (TextBox)WebUtils.FindControlRecursive(EnableControls.Owner, ControlID);
                    else
                        _control = (TextBox)WebUtils.FindControlRecursive(EnableControls.Page, ControlID);
                }
                return _control as TextBox;
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
            if (TextBox == null) return Control != null ? Control.ClientID : null;
            return TextBox.ClientID;
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
            var value = TextBox != null ? TextBox.Text : (Control != null ? Control.Attributes["value"] : "");
            return string.Equals(value ?? "", Value ?? "");
        }

        public IEnableControl Clone()
        {
            return new EnableByTextBox
                       {
                           _control = _control,
                           ControlID = ControlID,
                           Disable = Disable,
                           EnableControls = EnableControls,
                           Owner = Owner,
                           Value = Value,
                       };
        }

        #endregion
    }
}