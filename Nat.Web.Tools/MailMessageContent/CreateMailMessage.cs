/*
 * Created by: Eugene.Kolesnikov
 * Created: 10 ноября 2010 г.
 * Copyright © JSC NAT Kazakhstan 2010
 */
using System;
using System.IO;
using System.Web.UI;
using System.Drawing;

namespace Nat.Web.Tools.MailMessageContent
{
    using System.Web;

    public static class CreateMailMessage
    {
        #region Formatting message

        #region Begin of message

        public static HtmlTextWriter GetMessageTitle(string comment)
        {
            TextWriter tw = new StringWriter();
            var html = new HtmlTextWriter(tw);

            html.RenderBeginTag(HtmlTextWriterTag.Html);
            html.RenderBeginTag(HtmlTextWriterTag.Body);

            GetMessageTitle(html, comment);
            return html;
        }

        public static HtmlTextWriter GetMessageTitle(string comment, string colSpan)
        {
            TextWriter tw = new StringWriter();
            var html = new HtmlTextWriter(tw);

            html.RenderBeginTag(HtmlTextWriterTag.Html);
            html.RenderBeginTag(HtmlTextWriterTag.Body);

            GetMessageTitle(html, comment, colSpan);
            return html;
        }

        public static void GetMessageTitle(HtmlTextWriter html, string comment)
        {
            GetMessageTitle(html, comment, "2");
        }

        public static void GetMessageTitle(HtmlTextWriter html, string comment, string colSpan)
        {
            html.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            html.RenderBeginTag(HtmlTextWriterTag.Table);

            html.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14pt");
            html.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            html.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            html.AddAttribute(HtmlTextWriterAttribute.Colspan, colSpan);
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            html.WriteEncodedText(comment);
            html.RenderEndTag();
        }

        public static void BeginTable(HtmlTextWriter html, params string[] headers)
        {
            html.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            html.RenderBeginTag(HtmlTextWriterTag.Table);

            html.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "14pt");
            html.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            html.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            foreach (var header in headers)
            {
                html.RenderBeginTag(HtmlTextWriterTag.Th);
                html.Write(GetHtmlEncode(header, true));
                html.RenderEndTag();                
            }
        }

        #endregion

        #region Groups in message

        public static void SetMessageHeader(HtmlTextWriter html, string textHeader, string colSpan)
        {
            html.RenderBeginTag(HtmlTextWriterTag.Tr);
            html.AddAttribute(HtmlTextWriterAttribute.Colspan, colSpan);
            html.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            html.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "bold");
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            html.WriteLine(textHeader);
            html.RenderEndTag();
            html.RenderEndTag();
        }

        public static void SetMessageHeader(HtmlTextWriter html, string textHeader)
        {
            SetMessageHeader(html, textHeader, "2");
        }

        #endregion

        #region Set Values

        public static void AddCell(HtmlTextWriter html, object cellValue)
        {
            AddCell(html, cellValue, true);
        }

        public static void AddCell(HtmlTextWriter html, object cellValue, bool htmlEncode)
        {
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            html.WriteLine(GetHtmlEncode(cellValue, htmlEncode));
            html.RenderEndTag();
        }

        private static string GetHtmlEncode(object cellValue, bool htmlEncode)
        {
            if (cellValue == null)
                return null;

            return htmlEncode ? HttpUtility.HtmlEncode(cellValue.ToString()) : cellValue.ToString();
        }

        public static void SetCellValue(HtmlTextWriter html, string textHeader, object cellValue, bool isCnanged, Color color)
        {
            SetCellValue(html, textHeader, cellValue, isCnanged, color, true);
        }

        public static void SetCellValue(HtmlTextWriter html, string textHeader, object cellValue, bool isCnanged, Color color, bool htmlEncode)
        {
            html.RenderBeginTag(HtmlTextWriterTag.Tr);
            html.AddStyleAttribute(HtmlTextWriterStyle.Width, "30%");
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            html.WriteLine(GetHtmlEncode(textHeader, true));
            html.RenderEndTag();
            if (isCnanged)
            {
                html.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(color));
//                isCnanged = false;
            }
            
            html.AddStyleAttribute(HtmlTextWriterStyle.Width, "70%");
            html.RenderBeginTag(HtmlTextWriterTag.Td);

            if (cellValue != null)
            {
                if (cellValue.GetType() == typeof(DateTime))
                    html.WriteLine(((DateTime)cellValue).ToShortDateString());
                else if (cellValue.GetType() == typeof(DateTime?))
                    html.WriteLine(((DateTime?)cellValue).Value.ToShortDateString());
                else if (cellValue.GetType() == typeof(bool))
                    html.WriteLine((bool)cellValue ? "Да" : "Нет");
                else if (cellValue.GetType() == typeof(bool?))
                    html.WriteLine(cellValue == null ? "" : (bool)cellValue ? "Да" : "Нет");
                else
                    html.WriteLine(GetHtmlEncode(cellValue, htmlEncode));
            }
            else
                html.WriteLine("");
            html.RenderEndTag();
            html.RenderEndTag();

        }
        
        public static void SetCellValue(HtmlTextWriter html, string textHeader, object cellValue)
        {
            var isChanged = false;
            SetCellValue(html, textHeader, cellValue, ref isChanged);
        }
        
        public static void SetCellValue(HtmlTextWriter html, string textHeader, object cellValue, bool isColored)
        {
            SetCellValue(html, textHeader, cellValue, isColored,  Color.LightGreen);
        }

        public static void SetCellValue(HtmlTextWriter html, string textHeader, object cellValue, ref bool isChanged)
        {
            SetCellValue(html, textHeader, cellValue, isChanged, Color.LightGreen);
        }
        
        #region Update Cell Value

        public static void SetCellValueUpdate(HtmlTextWriter html, string textHeader, object originalCellValue, object newCellValue)
        {
            var isChanged = false;
            SetCellValue(html, textHeader, ref isChanged, originalCellValue, newCellValue);
        }

        public static void SetCellValue(HtmlTextWriter html, string textHeader, ref bool isChanged, object originalCellValue, object newCellValue)
        {
            SetCellValue(html, textHeader, ref isChanged, originalCellValue, newCellValue, true);
        }

        public static void SetCellValue(HtmlTextWriter html, string textHeader, ref bool isChanged, object originalCellValue, object newCellValue, bool htmlEncode)
        {
            html.RenderBeginTag(HtmlTextWriterTag.Tr);
            html.AddStyleAttribute(HtmlTextWriterStyle.Width, "20%");
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            html.WriteLine(GetHtmlEncode(textHeader, true));
            html.RenderEndTag();
            html.AddStyleAttribute(HtmlTextWriterStyle.Width, "40%");
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            //html.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            if (originalCellValue != null)
            {
                if (originalCellValue.GetType() == typeof(DateTime))
                    html.WriteLine(((DateTime)originalCellValue).ToShortDateString());
                else if (originalCellValue.GetType() == typeof(DateTime?))
                    html.WriteLine(((DateTime?)originalCellValue).Value.ToShortDateString());
                else
                    html.WriteLine(GetHtmlEncode(originalCellValue, htmlEncode));
            }else
                html.WriteLine();
            
            html.RenderEndTag();
            html.AddStyleAttribute(HtmlTextWriterStyle.Width, "40%");
            if (isChanged)
            {
                html.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "lightgreen");
                isChanged = false;
            }
            html.RenderBeginTag(HtmlTextWriterTag.Td);
            //html.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
            if(newCellValue == null)
                html.WriteLine();
            else
            {
                if (newCellValue.GetType() == typeof(DateTime))
                    html.WriteLine(((DateTime)newCellValue).ToShortDateString());
                else if (newCellValue.GetType() == typeof(DateTime?))
                    html.WriteLine(((DateTime?)newCellValue).Value.ToShortDateString());
                else
                    html.WriteLine(GetHtmlEncode(newCellValue, htmlEncode));
            }
            html.RenderEndTag();
            html.RenderEndTag();
        }

        #endregion
        
        #endregion

        #endregion
    }
}