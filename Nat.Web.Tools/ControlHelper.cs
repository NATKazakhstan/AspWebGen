/*
 * Created by: Denis M. Silkov
 * Created: 12 ������� 2007 �.
 * Copyright � JSC New Age Technologies 2007
 */

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools
{
    /// <summary>
    /// ������ ��� ������ � ����������.
    /// </summary>
    public static class ControlHelper
    {
        /// <summary>
        /// ������� ���������� �������, ������� ����� � ��������� ��������
        /// (�������� ������� ���� �����������).
        /// </summary>
        /// <param name="root">�������� �������.</param>
        /// <param name="id">��� ��������.</param>
        /// <returns>��������� ������� ���� null.</returns>
        public static Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            Control control = root.FindControl(id);
            if (control != null && control.ID == id)
                return control;

            foreach (Control subControl in root.Controls)
            {
                Control foundControl = FindControlRecursive(subControl, id);
                if (foundControl != null)
                    return foundControl;
            }

            return null;
        }

        /// <summary>
        /// ����������� ����� �������� "�����", ��������� ������
        /// </summary>
        /// <typeparam name="T">������� �����</typeparam>
        /// <param name="control">������� � ������� ���������� �����</param>
        /// <returns></returns>
        public static T FindControl<T>(Control control) where T : class
        {
            T ctrl = control as T;
            if(ctrl != null) return ctrl;
            if (control != null) ctrl = FindControl<T>(control.Parent);
            return ctrl;
        }

        /// <summary>
        /// ����������� ����� �������� "�����"
        /// </summary>
        /// <param name="control">������� � ������� ���������� �����</param>
        /// <param name="controlID">����� �������� �� ID</param>
        /// <returns></returns>
        public static Control FindControl(Control control, string controlID)
        {
            var namingContainer = control;
            Control foundControl = null;
            if (control != control.Page)
            {
                while ((foundControl == null) && (namingContainer != control.Page))
                {
                    namingContainer = namingContainer.NamingContainer;
                    if (namingContainer == null)
                        throw new HttpException(
                            string.Format(
                                "The {0} control '{1}' does not have a naming container.  Ensure that the control is added to the page before calling DataBind",
                                control.GetType().Name, control.ID));

                    foundControl = namingContainer.FindControl(controlID);
                }

                return foundControl;
            }
            return control.FindControl(controlID);

        }


        /// <summary>
        /// ���������� true, ���� ������� ������������.
        /// ��������� ��������� �� ���������, ������������ ����������� � DetailsView.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static bool IsControlDisplayed(Control control)
        {
            if (!control.Visible || control.Parent == null)
                return control.Visible;

            DetailsViewRow detailsViewRow = control as DetailsViewRow;
            if (detailsViewRow != null)
            {
                DetailsView detailsView = GetDetailsView(detailsViewRow);
                if (detailsView != null && !detailsView.Fields[detailsViewRow.RowIndex].Visible)
                    return false;
            }

            return IsControlDisplayed(control.Parent);
        }

        private static DetailsView GetDetailsView(Control childControl)
        {
            Control parent = childControl.Parent;
            while (parent != null)
            {
                DetailsView detailsView = parent as DetailsView;
                if (detailsView != null)
                    return detailsView;

                parent = parent.Parent;
            }
            return null;
        }
    }
}