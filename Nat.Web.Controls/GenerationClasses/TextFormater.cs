using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;

namespace Nat.Web.Controls.GenerationClasses
{
    public class TextFormater
    {
        private static Regex _regexDetectUrls = new Regex(@"^\s*(?<url>.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static string FormatToHtml(object text)
        {
            if (text == null) return null;
            var t = text as string ?? text.ToString();
            return t.Replace("\r\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
        }

        public static string FormatKSP(object text)
        {
            if (text == null) return null;
            string t;
            t = text as string;
            if (t == null) t = text.ToString();
            if (t.Length != 16) return t;
            var sb = new StringBuilder(t);
            sb.Insert(14, '.');
            sb.Insert(12, '.');
            sb.Insert(10, '.');
            sb.Insert(8, '.');
            sb.Insert(7, '.');
            sb.Insert(5, '.');
            sb.Insert(4, '.');
            sb.Insert(2, '.');
            sb.Append(" (");
            sb.Append(t);
            sb.Append(")");
            return sb.ToString();
        }

        public static string CreateLink(object text)
        {
            if (text == null) return null;
            var t = text as string ?? text.ToString();
            var match = _regexDetectUrls.Match(t);
            if (!match.Success) return "";
            var sb = new StringBuilder();
            while (match.Success)
            {
                string url = match.Groups["url"].Value.Replace("\"", "");
                sb.Append("<a href=\"");
                sb.Append(url);
                sb.Append("\">");
                sb.Append(url);
                sb.Append("</a>");
                sb.Append("<br/>");
                match = match.NextMatch();
            }
            return sb.ToString();
        }
    }
}
