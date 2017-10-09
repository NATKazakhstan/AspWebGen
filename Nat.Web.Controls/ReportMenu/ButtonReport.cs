/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 16 мая 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.ReportMenu
{
    public class ButtonReport : ITemplate
    {
        private Button btnMenu;

        public void InstantiateIn(Control container)
        {
            btnMenu = new Button();
            btnMenu.Text = Resources.SReport;
            btnMenu.OnClientClick = "return false;";
            container.Controls.Add(btnMenu);
        }
    }
}