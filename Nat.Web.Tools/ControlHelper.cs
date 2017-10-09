/*
 * Created by: Denis M. Silkov
 * Created: 12 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools
{
    /// <summary>
    /// Хелпер для работы с контролами.
    /// </summary>
    public static class ControlHelper
    {
        /// <summary>
        /// Находит рекурсивно контрол, начиная поиск с корневого контрола
        /// (корневой контрол тоже проверяется).
        /// </summary>
        /// <param name="root">Корневой контрол.</param>
        /// <param name="id">Имя контрола.</param>
        /// <returns>Найденный контрол либо null.</returns>
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
        /// Рекурсивный поиск контрола "вверх", заданного класса
        /// </summary>
        /// <typeparam name="T">Искомый класс</typeparam>
        /// <param name="control">контрол с которго начинается поиск</param>
        /// <returns></returns>
        public static T FindControl<T>(Control control) where T : class
        {
            T ctrl = control as T;
            if(ctrl != null) return ctrl;
            if (control != null) ctrl = FindControl<T>(control.Parent);
            return ctrl;
        }

        /// <summary>
        /// Рекурсивный поиск контрола "вверх"
        /// </summary>
        /// <param name="control">контрол с которго начинается поиск</param>
        /// <param name="controlID">Поиск контрола по ID</param>
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
        /// Возвращает true, если контрол отображается.
        /// Проверяет родителей на видимость, обрабатывает вложенность в DetailsView.
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