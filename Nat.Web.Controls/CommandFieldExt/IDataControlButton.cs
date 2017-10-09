using System.Web.UI.WebControls;

namespace Nat.Web.Controls.CommandFieldExt
{
    public interface IDataControlButton : IButtonControl
    {
        string OnClientClick { get; set; }
    }
}
