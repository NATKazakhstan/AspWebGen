using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.EnableController
{
    public class ControllerControlItem
    {
        public ControllerControlItem()
        {
            Enabled = new Dictionary<ControllerActiveType, bool?>();
        }

        public ControllerControlItem(string clientID)
        {
            ClientID = clientID;
            Enabled = new Dictionary<ControllerActiveType, bool?>();
        }

        public ControllerControlItem(Control control)
        {
            ClientID = control.ClientID;
            Control = control;
            Enabled = new Dictionary<ControllerActiveType, bool?>();
        }

        public string ClientID { get; set; }
        public Control Control { get; set; }
        public Dictionary<ControllerActiveType, bool?> Enabled { get; set; }
        public bool JoinControllersAsOr { get; set; }

        public void ChangeActiveControls()
        {
            if (Control == null) return;
            foreach (var item in Enabled)
            {
                var webControl = Control as WebControl;
                switch (item.Key)
                {
                    case ControllerActiveType.None:
                        break;
                    case ControllerActiveType.Enabled:
                        if (webControl != null && item.Value != null)
                            webControl.Enabled = item.Value.Value;
                        break;
                    case ControllerActiveType.ReadOnly:
                        if (webControl != null && item.Value != null)
                            webControl.Attributes["readonly"] = item.Value.Value.ToString().ToLower();
                        break;
                    case ControllerActiveType.ValidationDisabled:
                        if (webControl != null && item.Value != null)
                            webControl.Attributes["EnableMode.ChangeValidateGroup"] = item.Value.Value.ToString().ToLower();
                        break;
                    case ControllerActiveType.Hide:
                        if (webControl != null && item.Value != null)
                            webControl.Style["display"] = item.Value.Value ? "" : "none";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
