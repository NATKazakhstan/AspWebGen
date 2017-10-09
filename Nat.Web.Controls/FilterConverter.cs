using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class FilterConverter : ControlIDConverter
    {
        protected override bool FilterControl(Control control)
        {
            if (!base.FilterControl(control))
                return false;
            return control is IFilterControl;
        }
    }
}