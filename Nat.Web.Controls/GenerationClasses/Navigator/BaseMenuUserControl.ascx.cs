using System.ComponentModel;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    public partial class BaseMenuUserControl : UserControl
    {
        public void SetMenu(BaseMenu menu)
        {
            ppc.HeaderText = menu.Header;
            ph.Controls.Add(menu);
        }

        protected override void OnInit(System.EventArgs e)
        {
            base.OnInit(e);
            ppc.CancelControlID = ClientID + "_Cancel";
        }
    }
}