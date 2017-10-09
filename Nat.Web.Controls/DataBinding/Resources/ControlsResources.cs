using System.Web.UI;

[assembly: WebResource("Nat.Web.Controls.DataBinding.Resources.HoverPanel.js", "text/javascript")]
[assembly: WebResource("Nat.Web.Controls.DataBinding.Resources.warning.gif", "image/gif")]
[assembly: WebResource("Nat.Web.Controls.DataBinding.Resources.info.gif", "image/gif")]

namespace Nat.Web.Controls.DataBinding.Resources
{
    /// <summary>
    /// Class is used as to consolidate access to resources
    /// </summary>
    public class ControlsResources
    {
        public const string HOVERPANEL_SCRIPT_RESOURCE = "Nat.Web.Controls.DataBinding.Resources.HoverPanel.js";
        public const string INFO_ICON_RESOURCE = "Nat.Web.Controls.DataBinding.Resources.info.gif";
        public const string WARNING_ICON_RESOURCE = "Nat.Web.Controls.DataBinding.Resources.warning.gif";
    }
}