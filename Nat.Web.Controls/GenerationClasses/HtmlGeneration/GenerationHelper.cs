/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 θώνÿ 2009 γ.
 * Copyright © JSC New Age Technologies 2009
 */

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Text;
    using System.Web.UI;

    public static class GenerationHelper
    {
        public static string GetHeaderName(string header)
        {
            if (string.IsNullOrEmpty(header))
                return header;

            var split = header.Split(new[] { '#', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(split.Length * 30);
            foreach (var value in split)
            {
                if (value.StartsWith("_") || value.StartsWith("..._"))
                    continue;

                sb.Append(value.StartsWith("...") ? value.Substring(3) : value);
                sb.Append("/");
            }

            return sb.ToString(0, sb.Length - 1);
        }

        public static string GetHeaderName(string groupName, string header)
        {
            if (string.IsNullOrEmpty(groupName)) return header;
            var split = groupName.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(split.Length * 30);
            foreach (var value in split)
            {
                if (value.StartsWith("_") || value.StartsWith("..._"))
                    continue;
                
                sb.Append(value.StartsWith("...") ? value.Substring(3) : value);
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