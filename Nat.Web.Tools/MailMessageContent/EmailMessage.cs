/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.05.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.MailMessageContent
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    public class EmailMessage : IDisposable
    {
        public HtmlTextWriter HtmlWriter { get; }

        private readonly Stack<int> tableColumns = new Stack<int>();

        public EmailMessage(HtmlTextWriter writer)
        {
            HtmlWriter = writer;
        }

        public static Action<TSource, HtmlTextWriter> WriteDate<TSource>(Func<TSource, DateTime?> getValue)
        {
            return (source, textWriter) =>
                {
                    var date = getValue(source);
                    if (date != null) textWriter.Write(date.Value.ToShortDateString());
                };
        }

        public static Action<TSource, HtmlTextWriter> WriteDateTime<TSource>(Func<TSource, DateTime?> getValue)
        {
            return (source, textWriter) =>
                {
                    var dateTime = getValue(source);
                    if (dateTime != null) textWriter.Write(dateTime.Value.ToString("dd.MM.yyyy HH:mm"));
                };
        }

        public static Action<TSource, HtmlTextWriter> Write<TSource, TValue>(Func<TSource, TValue?> getValue)
            where TValue : struct
        {
            return (source, textWriter) =>
                {
                    var value = getValue(source);
                    if (value != null) textWriter.Write(value.Value);
                };
        }

        public static Action<TSource, HtmlTextWriter> WriteDate<TSource>(Func<TSource, DateTime?> getValue1, Func<TSource, DateTime?> getValue2)
        {
            return (source, textWriter) =>
                {
                    var date1 = getValue1(source);
                    var date2 = getValue2(source);
                    if (date1 == date2)
                    {
                        if (date1 != null) textWriter.Write(date1.Value.ToShortDateString());
                    }
                    else
                    {
                        if (date1 != null)
                        {
                            textWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "5px");
                            textWriter.RenderBeginTag(HtmlTextWriterTag.Strike);
                            textWriter.Write(date1.Value.ToShortDateString());
                            textWriter.RenderEndTag();
                        }

                        if (date2 != null)
                        {
                            if (date1 == null)
                            {
                                textWriter.AddStyleAttribute(
                                    HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(Color.LightGreen));
                            }
                            else
                                textWriter.WriteBreak();
                            textWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                            textWriter.Write(date2.Value.ToShortDateString());
                            textWriter.RenderEndTag();
                        }
                    }
                };
        }

        public static Action<TSource, HtmlTextWriter> WriteDateTime<TSource>(Func<TSource, DateTime?> getValue1, Func<TSource, DateTime?> getValue2)
        {
            return (source, textWriter) =>
            {
                var dateTime1 = getValue1(source);
                var dateTime2 = getValue2(source);
                if (dateTime1 == dateTime2)
                {
                    if (dateTime1 != null) textWriter.Write(dateTime1.Value.ToString("dd.MM.yyyy HH:mm"));
                }
                else
                {
                    if (dateTime1 != null)
                    {
                        textWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "5px");
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Strike);
                        textWriter.Write(dateTime1.Value.ToString("dd.MM.yyyy HH:mm"));
                        textWriter.RenderEndTag();
                    }

                    if (dateTime2 != null)
                    {
                        if (dateTime1 == null)
                        {
                            textWriter.AddStyleAttribute(
                                HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(Color.LightGreen));
                        }
                        else
                            textWriter.WriteBreak();
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        textWriter.Write(dateTime2.Value.ToString("dd.MM.yyyy HH:mm"));
                        textWriter.RenderEndTag();
                    }
                }
            };
        }

        public static Action<TSource, HtmlTextWriter> Write<TSource, TValue>(Func<TSource, TValue?> getValue1, Func<TSource, TValue?> getValue2)
            where TValue : struct
        {
            return (source, textWriter) =>
                {
                    var value1 = getValue1(source);
                    var value2 = getValue2(source);
                    Write(value1, value2, textWriter);
                };
        }

        public static void Write<TValue>(TValue value1, TValue value2, HtmlTextWriter textWriter, string format = "{0}")
        {
            Write(value1, value2, value1, value2, textWriter, format);
        }

        public static void Write(object value1, object value2, object name1, object name2, HtmlTextWriter textWriter, string format = "{0}")
        {
            if (value1 != null && value2 != null && value1.Equals(value2))
            {
                textWriter.Write(format, name1);
            }
            else if (value1 != null || value2 != null)
            {
                if (value1 != null)
                {
                    textWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "5px");
                    textWriter.RenderBeginTag(HtmlTextWriterTag.Strike);
                    textWriter.Write(format, name1);
                    textWriter.RenderEndTag();
                }

                if (value2 != null)
                {
                    if (value1 == null)
                    {
                        textWriter.AddStyleAttribute(
                            HtmlTextWriterStyle.BackgroundColor,
                            ColorTranslator.ToHtml(Color.LightGreen));
                    }
                    else
                        textWriter.WriteBreak();

                    textWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                    textWriter.Write(format, name2);
                    textWriter.RenderEndTag();
                }
            }
        }

        public static Action<TSource, HtmlTextWriter> Write<TSource>(Func<TSource, string> getValue1, Func<TSource, string> getValue2)
        {
            return (source, textWriter) =>
            {
                var value1 = getValue1(source);
                var value2 = getValue2(source);
                if (value1 != null && value1.Equals(value2))
                {
                    textWriter.Write(value1);
                }
                else if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
                {
                    if (!string.IsNullOrEmpty(value1))
                    {
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Strike);
                        textWriter.Write(value1);
                        textWriter.RenderEndTag();
                    }

                    if (!string.IsNullOrEmpty(value2))
                    {
                        if (string.IsNullOrEmpty(value1))
                        {
                            textWriter.AddStyleAttribute(
                                HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(Color.LightGreen));
                        }
                        else
                            textWriter.WriteBreak();
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        textWriter.Write(value2);
                        textWriter.RenderEndTag();
                    }
                }
            };
        }

        public static Action<TSource, HtmlTextWriter> Write<TSource, TValue, TResultValue>(Func<TSource, TValue?> getValue1, Func<TSource, TValue?> getValue2, Func<TValue, TResultValue> getName)
            where TValue : struct
        {
            return (source, textWriter) =>
            {
                var value1 = getValue1(source);
                var value2 = getValue2(source);
                if (value1.HasValue && value2.HasValue && value1.Value.Equals(value2.Value))
                {
                    textWriter.Write(getName(value1.Value));
                }
                else if (value1.HasValue || value2.HasValue)
                {
                    if (value1 != null)
                    {
                        textWriter.AddStyleAttribute(HtmlTextWriterStyle.PaddingRight, "5px");
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Strike);
                        textWriter.Write(getName(value1.Value));
                        textWriter.RenderEndTag();
                    }

                    if (value2 != null)
                    {
                        if (value1 == null)
                        {
                            textWriter.AddStyleAttribute(
                            HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(Color.LightGreen));
                        }
                        else 
                            textWriter.WriteBreak();
                        textWriter.RenderBeginTag(HtmlTextWriterTag.Span);
                        textWriter.Write(getName(value2.Value));
                        textWriter.RenderEndTag();
                    }
                }
            };
        }

        public void BeginMessage()
        {
            HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Html);
            HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Body);
        }

        public void EndMessage()
        {
            while (tableColumns.Count > 0)
            {
                EndTable();
                tableColumns.Pop();
            }
            
            HtmlWriter.RenderEndTag();
            HtmlWriter.RenderEndTag();
        }

        public void BeginTable(string title, Action<HtmlTextWriter> addTableAttributes, Action<HtmlTextWriter> addHeaderThAttributes, params string[] headers)
        {
            // Если уже была начата таблица, то рендерим заголовок в ней
            if (tableColumns.Count > 0)
            {
                // Если текст заголовка не пустой, то рендерим его в новую строку текущей таблицы
                if (!string.IsNullOrEmpty(title))
                {
                    HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr); //// tr 3

                    addHeaderThAttributes(HtmlWriter);
                    HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, tableColumns.Peek().ToString());
                    HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td); //// td 3

                    HtmlWriter.WriteEncodedText(title);

                    HtmlWriter.RenderEndTag(); //// td 3
                    HtmlWriter.RenderEndTag(); //// tr 3
                }

                // Создаем строку и ячейку для таблицы
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr); //// tr for table
                HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, tableColumns.Peek().ToString());
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td); //// td for table

                // Создаем новую таблицу
                addTableAttributes(HtmlWriter);
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Table); //// table
            }
            else
            {
                addTableAttributes(HtmlWriter);
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Table); //// table

                // Если текст заголовка не пустой, то рендерим его в новую строку
                if (!string.IsNullOrEmpty(title))
                {
                    HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr); //// tr 1

                    addHeaderThAttributes(HtmlWriter);
                    HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Colspan, headers.Length.ToString());
                    HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td); //// td 1

                    HtmlWriter.WriteEncodedText(title);

                    HtmlWriter.RenderEndTag(); //// td 1
                    HtmlWriter.RenderEndTag(); //// tr 1
                }
            }

            // Если есть текст в заголовках тогда рендерим строку с заголовками
            if (!headers.All(string.IsNullOrEmpty))
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr); ////tr 2

                foreach (var header in headers)
                {
                    HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Th); ////td 2
                    HtmlWriter.WriteEncodedText(header);
                    HtmlWriter.RenderEndTag(); //// td 2
                }

                HtmlWriter.RenderEndTag(); //// tr 2
            }

            tableColumns.Push(headers.Length);
        }

        public void AddRows<T>(IEnumerable<T> data, params Action<T, HtmlTextWriter>[] renderCellValues)
        {
            foreach (T row in data) 
                AddRow(row, renderCellValues);
        }

        public void AddRow<T>(T row, params Action<T, HtmlTextWriter>[] renderCellValues)
        {
            var dif = tableColumns.Peek() - renderCellValues.Length;
            if (dif < 0)
                throw new ArgumentOutOfRangeException(
                    "renderCellValues", "Длина колекции больше чем количество колонок в таблице");

            HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);

            foreach (var renderCellValue in renderCellValues)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                renderCellValue(row, HtmlWriter);
                HtmlWriter.RenderEndTag();
            }

            for (int i = 0; i < dif; i++)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                HtmlWriter.RenderEndTag();                
            }

            HtmlWriter.RenderEndTag();
        } 
        
        public void AddRow(params string[] renderCellValues)
        {
            var dif = tableColumns.Peek() - renderCellValues.Length;
            if (dif < 0)
                throw new ArgumentOutOfRangeException(
                    "renderCellValues", "Длина колекции больше чем количество колонок в таблице");

            HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);

            foreach (var renderCellValue in renderCellValues)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                HtmlWriter.Write(renderCellValue);
                HtmlWriter.RenderEndTag();
            }

            for (int i = 0; i < dif; i++)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                HtmlWriter.RenderEndTag();                
            }

            HtmlWriter.RenderEndTag();
        }

        public void AddRowHtmlEncode(params string[] renderCellValues)
        {
            var dif = tableColumns.Peek() - renderCellValues.Length;
            if (dif < 0)
                throw new ArgumentOutOfRangeException(
                    "renderCellValues", "Длина колекции больше чем количество колонок в таблице");

            HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);

            foreach (var renderCellValue in renderCellValues)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                HtmlWriter.Write(HttpUtility.HtmlEncode(renderCellValue));
                HtmlWriter.RenderEndTag();
            }

            for (int i = 0; i < dif; i++)
            {
                HtmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                HtmlWriter.RenderEndTag();                
            }

            HtmlWriter.RenderEndTag();
        }

        public void EndTable()
        {
            HtmlWriter.RenderEndTag(); //// table
            tableColumns.Pop();
            if (tableColumns.Count > 0)
            {
                HtmlWriter.RenderEndTag(); //// td for table
                HtmlWriter.RenderEndTag(); //// tr for table
            }
        }
        
        public void Dispose()
        {
            HtmlWriter.Dispose();
        }

        public int ColsCount()
        {
            if (tableColumns.Count == 0)
                return 1;
            return tableColumns.Peek();
        }
    }
}