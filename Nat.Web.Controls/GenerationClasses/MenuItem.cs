/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 4 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Collections.Generic;
using System.Text;
using System.Web;
using AjaxControlToolkit;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.UI.WebControls;

    public class MenuItem
    {
        public MenuItem()
        {
            Items = new List<MenuItem>();
            PopupPosition = HoverMenuPopupPosition.Bottom;
            HyperLinkCss = "menuHyperLink";
        }

        public IList<MenuItem> Items { get; private set; }
        public string Text { get; set; }
        public bool TextAsHtml { get; set; }
        public string Url { get; set; }
        public string AttributeUrl { get; set; }
        public string ToolTip { get; set; }
        public string Question { get; set; }
        public string Target { get; set; }
        public HoverMenuPopupPosition PopupPosition { get; set; }
        public string OnClick { get; set; }
        public string HyperLinkCss { get; set; }
        public Unit? Width { get; set; }

//        public bool IsButton { get; set; }
        //public string OnClick { get; set; }

        public void GenerateHtml(StringBuilder sb, string clientID, ExtenderAjaxControl extenderAjaxControl, bool isButton)
        {
            if (Items.Count <= 0)
            {
                RenderLink(sb);
                return;
            }

            if (isButton)
            {
                sb.Append("<span id=\"");
                sb.Append(clientID);
                sb.Append("\" class=\"menuMoreAsButton\"");
                sb.Append(" title=\"");
                sb.Append(ToolTip);
                sb.Append("\"><u>");
                if (!string.IsNullOrEmpty(Url))
                    RenderLink(sb);
                else
                    sb.Append(TextAsHtml ? Text : HttpUtility.HtmlAttributeEncode(Text));
                sb.Append("</u>&nbsp;<small>&#9660;</small></span>");
                sb.Append("<table id=\"mp_");
                sb.Append(clientID);
                sb.Append("\" class=\"menuPanel\">");
            }
            else
            {
                sb.Append("<tr id=\"");
                sb.Append(clientID);
                sb.Append("\" class=\"menuMore\"");
                sb.Append(" title=\"");
                sb.Append(ToolTip);
                sb.Append("\"><td><u>");
                if (!string.IsNullOrEmpty(Url))
                    RenderLink(sb);
                else
                    sb.Append(TextAsHtml ? Text : HttpUtility.HtmlAttributeEncode(Text));
                sb.Append("</u>&nbsp;</td><td width=\"10px\"><small>&#9660;</small>");
                sb.Append("<table id=\"mp_");
                sb.Append(clientID);
                sb.Append("\" class=\"menuPanel\">");
            }

            int i = 0;
            foreach (var item in Items)
            {
                if (item.Items.Count == 0)
                    sb.Append("<tr class=\"menuItem\"><td>");
                item.GenerateHtml(sb, clientID + i, extenderAjaxControl, false);
                sb.Append("</td></tr>");
                i++;
            }

            sb.Append("</table>");
            var extender = new HoverMenuExtender
                {
                    ID = "hme_" + clientID,
                    PopupControlID = "mp_" + clientID,
                    PopupPosition = PopupPosition,
                    PopDelay = 200,
                    OffsetX = 0,
                    OffsetY = 0,
                };
            extenderAjaxControl.AddExtender(extender, clientID);
        }

        private void RenderLink(StringBuilder sb)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" class=\"{3}\" title=\"{1}\" url=\"{2}\"",
                Url,
                HttpUtility.HtmlAttributeEncode(ToolTip),
                AttributeUrl,
                HyperLinkCss);

            if (Width != null)
                sb.AppendFormat(" style=\"width:{0};display:block;\"", Width);

            if (!string.IsNullOrEmpty(Question) && !string.IsNullOrEmpty(OnClick))
            {
                sb.AppendFormat(
                    " onclick=\"if(confirm('{0}')) {{ {1}; return true; }} else return false;\"",
                    HttpUtility.HtmlAttributeEncode(Question),
                    OnClick);
            }
            else if (!string.IsNullOrEmpty(Question))
            {
                sb.AppendFormat(
                    " onclick=\"return confirm('{0}');\"",
                    HttpUtility.HtmlAttributeEncode(Question));
            }
            else if (!string.IsNullOrEmpty(OnClick))
                sb.AppendFormat(" onclick=\"{0}\"", OnClick);

            if (!string.IsNullOrEmpty(Target))
                sb.AppendFormat(" target = '{0}'", HttpUtility.HtmlAttributeEncode(Target));

            sb.AppendFormat(">{0}</a>", TextAsHtml ? Text : HttpUtility.HtmlAttributeEncode(Text));
        }
    }
}