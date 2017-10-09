/*
 * Created by: Denis M. Silkov
 * Created: 20 �������� 2007 �.
 */

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools
{
    /// <summary>
    /// ControlIDConverter � ��������� ���� ���������, ������� ���������� ����������.
    /// </summary>
    /// <typeparam name="T">��� ���������.</typeparam>
    public class ControlIDConverter<T> : ControlIDConverter where T: Control
    {
        protected override bool FilterControl(Control control)
        {
            return control is T;
        }
    }
}