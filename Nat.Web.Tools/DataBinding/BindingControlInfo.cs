/*
 * Created by: Denis M. Silkov
 * Created: 14 декабря 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

using System;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools.DataBinding
{
    [Serializable]
    /// <summary>
    /// Информация о контроле байндинга.
    /// </summary>
    public class BindingControlInfo
    {
        private string cssClass;
        private string toolTip;

        public BindingControlInfo() {}

        public BindingControlInfo(string cssClass, string toolTip) : this()
        {
            this.cssClass = cssClass;
            this.toolTip = toolTip;
        }

        /// <summary>
        /// CSS класс.
        /// </summary>
        public string CssClass
        {
            get { return cssClass; }
            set { cssClass = value; }
        }

        /// <summary>
        /// Подсказка.
        /// </summary>
        public string ToolTip
        {
            get { return toolTip; }
            set { toolTip = value; }
        }

        /// <summary>
        /// Устанавливает или, если info == null очищает, свойства контрола.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="info"></param>
        public static void SetOrClearProperties(WebControl control, BindingControlInfo info)
        {
            if(info != null)
                info.SetProperties(control);
            else
            {
                control.CssClass = "";
                control.ToolTip = "";
            }
        }

        /// <summary>
        /// Устанавливает соответствующие свойства контрола.
        /// </summary>
        /// <param name="control"></param>
        public void SetProperties(WebControl control)
        {
            control.CssClass = CssClass;
            control.ToolTip = ToolTip;
        }
    }
}