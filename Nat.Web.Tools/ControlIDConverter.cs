/*
 * Created by: Denis M. Silkov
 * Created: 20 сент€бр€ 2007 г.
 */

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools
{
    /// <summary>
    /// ControlIDConverter с указанием типа контролов, которые необходимо показывать.
    /// </summary>
    /// <typeparam name="T">“ип контролов.</typeparam>
    public class ControlIDConverter<T> : ControlIDConverter where T: Control
    {
        protected override bool FilterControl(Control control)
        {
            return control is T;
        }
    }
}