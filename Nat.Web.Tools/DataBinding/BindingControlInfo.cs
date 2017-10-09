/*
 * Created by: Denis M. Silkov
 * Created: 14 ������� 2007 �.
 * Copyright � JSC New Age Technologies 2007
 */

using System;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools.DataBinding
{
    [Serializable]
    /// <summary>
    /// ���������� � �������� ���������.
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
        /// CSS �����.
        /// </summary>
        public string CssClass
        {
            get { return cssClass; }
            set { cssClass = value; }
        }

        /// <summary>
        /// ���������.
        /// </summary>
        public string ToolTip
        {
            get { return toolTip; }
            set { toolTip = value; }
        }

        /// <summary>
        /// ������������� ���, ���� info == null �������, �������� ��������.
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
        /// ������������� ��������������� �������� ��������.
        /// </summary>
        /// <param name="control"></param>
        public void SetProperties(WebControl control)
        {
            control.CssClass = CssClass;
            control.ToolTip = ToolTip;
        }
    }
}