/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 16 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Web.UI;
using Nat.Web.Controls.DataBinding.Tools;

namespace Nat.Web.Controls
{
    public class EnableByEmpty : IEnableControl
    {
        #region IEnableControl Members

        /// <summary>
        /// Дизаблить контрол
        /// По умолчанию false.
        /// </summary>
        [DefaultValue(false)]
        [Description("Дизаблить контрол.")]
        public bool Disable { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ScriptIgnore]
        public EnableControls EnableControls { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ScriptIgnore]
        public Control Owner { get; set; }

        public string GetClientID()
        {
            return "";
        }

        public string ControlID { get; set; }

        public void RefreshValues()
        {
        }

        public TypeComponent TypeComponent
        {
            get { return TypeComponent.Disable; }
        }

        public bool IsEquals()
        {
            return true;
        }

        public IEnableControl Clone()
        {
            return new EnableByEmpty
                       {
                           ControlID = ControlID,
                           Disable = Disable,
                           EnableControls = EnableControls,
                           Owner = Owner,
                       };
        }

        #endregion

    }
}