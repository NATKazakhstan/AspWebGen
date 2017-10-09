/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 θώνÿ 2009 γ.
 * Copyright © JSC New Age Technologies 2009
 */
using System;
using System.Linq;
using System.Text;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.UI;

    public static class GenerationHelper
    {
        public static string GetHeaderName(string groupName, string header)
        {
            if (string.IsNullOrEmpty(groupName)) return header;
            var split = groupName.Split(new[] {'#'}, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(split.Length*30);
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith("_") || split[i].StartsWith("..._"))
                    continue;
                if (split[i].StartsWith("..."))
                    sb.Append(split[i].Substring(3));
                else
                    sb.Append(split[i]);
                sb.Append("/");
            }
            sb.Append(header);
            return sb.ToString();
        }

        public static StringBuilder AppendSortLink(
            this StringBuilder sb,
            Control control,
            GridHtmlGenerator.Column column,
            string header = null)
        {
            return sb.AppendFormat(
               "<a href=\"javascript:{0}\">{1}</a>",
               control.Page.ClientScript.GetPostBackEventReference(control, "sort:" + column.Sort),
               header ?? column.Header);
        }


        public static StringBuilder Content(this StringBuilder sb, GridHtmlGenerator.Column column)
        {
            column.ColumnContentHandler(sb);
            return sb;
        }
    }
}