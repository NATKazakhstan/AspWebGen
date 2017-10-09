using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public interface IEnableControl
    {
//        bool Enabled { get; }
        /// <summary>
        /// Дизаблить контрол при выполнении условий.
        /// </summary>
        bool Disable { get; set; }
        string ControlID { get; set; }
        string GetClientID();
//        Control Control { get; }
        EnableControls EnableControls { get; set; }
        Control Owner { set; get; }
        void RefreshValues();
        TypeComponent TypeComponent { get; }
        bool IsEquals();
        IEnableControl Clone();
    }
}