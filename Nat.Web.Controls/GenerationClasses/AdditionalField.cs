/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Text;
using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web;

    public class AdditionalField
    {
        public AdditionalField()
        {
            HtmlEncode = true;
        }

        public string Caption { get; set; }
        public string Text { get; set; }
        public bool HtmlEncode { get; set; }
        public bool HtmlEncodeCaption { get; set; }

        /// <summary>
        /// Если true, при рендере не создаются HTML теги, все необходимые теги предусматривает разработчик в полях Caption и Text. 
        /// HtmlEncode и HtmlEncodeCaption всегда считаются false.
        /// </summary>
        public bool CreateWithoutHtmlTags { get; set; }

        public void AddRowHtml(StringBuilder sb)
        {
            sb.AppendFormat("<tr>");
            if (string.IsNullOrEmpty(Caption))
            {
                sb.Append("<td class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
            else
            {
                sb.Append("<td class=\"detailsTableLableTD\">");
                sb.Append(HtmlEncodeCaption ? HttpUtility.HtmlEncode(Caption) : Caption);
                sb.Append("</td>");
                sb.Append("<td class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
            sb.Append("</tr>");
        }

        public void AddRowHtml(StringBuilder sb, string controlID, EnableItem enableItem)
        {
            sb.Append("<tr id=\"");
            sb.Append(controlID);

            if (enableItem != null)
            {
                enableItem.aditinalTargetID.Add(controlID);
                if ((enableItem.EnableMode & EnableMode.Hide) == EnableMode.Hide && !enableItem.Result)
                    sb.Append("\" style=\"display:none>");
            }

            sb.Append("\">");

            if (string.IsNullOrEmpty(Caption))
            {
                sb.Append("<td ");
                sb.Append("class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
            else
            {
                sb.Append("<td ");
                sb.Append("class=\"detailsTableLableTD\">");
                sb.Append(HtmlEncodeCaption ? HttpUtility.HtmlEncode(Caption) : Caption);
                sb.Append("</td>");
                sb.Append("<td class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
            sb.Append("</tr>");
        }

        public void AddCellHtml(StringBuilder sb, string controlID, EnableItem enableItem)
        {
            if (enableItem != null)
                enableItem.aditinalTargetID.Add(controlID);

            if (string.IsNullOrEmpty(Caption))
            {
                sb.Append("<td id=\"");
                sb.Append(controlID);
                sb.Append("\" ");
                sb.Append("class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
            else
            {
                sb.Append("<td id=\"");
                sb.Append(controlID);
                sb.Append("\" ");
                sb.Append("class=\"detailsTableLableTD\">");
                sb.Append(HtmlEncodeCaption ? HttpUtility.HtmlEncode(Caption) : Caption);
                sb.Append("</td>");
                sb.Append("<td class=\"detailsTableDataTD\">");
                sb.Append(HtmlEncode ? HttpUtility.HtmlEncode(Text) : Text);
                sb.Append("</td>");
            }
        }

        public static string CreateString(List<AdditionalField> fields, EnableItem enableItem, int commandGroup, string formatID)
        {
            if (fields.Count == 0) return "";
            var sb = new StringBuilder(256 * fields.Count);
            int i = 0;
            if (commandGroup == 0 || commandGroup == 1)
            {
                foreach (var field in fields)
                {
                    if (field.CreateWithoutHtmlTags)
                        sb.Append(field.Caption).Append(field.Text);
                    else
                    {
                        var controlID = string.Format(formatID, ++i);
                        field.AddRowHtml(sb, controlID, enableItem);
                    }
                }
            }
            else
            {
                foreach (var field in fields)
                {
                    if (field.CreateWithoutHtmlTags)
                        sb.Append(field.Caption).Append(field.Text);
                    else
                    {
                        var controlID = string.Format(formatID, ++i);
                        field.AddCellHtml(sb, controlID, enableItem);
                    }
                }
            }
            return sb.ToString();
        }
    }
}