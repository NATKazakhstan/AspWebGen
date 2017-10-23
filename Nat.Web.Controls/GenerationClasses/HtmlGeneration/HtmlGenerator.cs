/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 26 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.DateTimeControls;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using System.ComponentModel;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.IO;

    public static class HtmlGenerator
    {
        public const string checkbox2 = "<input type=\"checkbox\" id=\"{0}\" checked=\"checked\" />";
        public const string checkbox3 = "<input type=\"checkbox\" id=\"{0}\" />";
        public const string dropDownListEnd = @"</select>";

        public const string dropDownListOption = @"<option value=""{0}"">{1}</option>";
        public const string dropDownListSelectedOption = @"<option selected=""selected"" value=""{0}"">{1}</option>";
        public const string dropDownListStart = @"<select id=""{0}"" style=""width:{1};"" onmouseover=""{2}"">";
        public const string dropDownListStartOnChange = @"<select id=""{0}"" style=""width:{1};"" onchange=""{2}"" onmouseover=""{3}"">";

        public const string targetBlank = "_blank";

        public const string dropDownListScript = "_onmouseoverDropDownListToSetTitle(this)";
        
        public const string textarea1 = "<textarea id=\"{0}\" rows=\"{1}\" cols=\"{2}\">{3}</textarea>";

        public const string textBox1 = "<input type=\"text\" id=\"{0}\" maxlength=\"{1}\" size=\"{2}\" value=\"{3}\" />";
        public const string textBox1_2 = "<input type=\"text\" id=\"{0}\" size=\"{1}\" value=\"{2}\" />";

        public const string textBox2 = "<input type=\"text\" id=\"{0}\" maxlength=\"{1}\" style=\"width:{2}\" value=\"{3}\" />";

        public const string textBox3 = "<input type=\"text\" id=\"{0}\" style=\"width:{1}\" value=\"{2}\" />";

        public static void AddCheckBox(this StringBuilder sb, string clientID, bool isChecked)
        {
            if (isChecked) sb.AppendFormat(checkbox2, clientID);
            else sb.AppendFormat(checkbox3, clientID);
        }

        public static void AddHyperLink(this StringBuilder sb, string url, string text, string toolTip)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\">{2}</a>", url, HttpUtility.HtmlAttributeEncode(toolTip), text);
        }

        public static void AddHyperLinkAsLink(this StringBuilder sb, string url, string text, string toolTip)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{1}\">{2}</a>", url, HttpUtility.HtmlAttributeEncode(toolTip), text);
        }


        public static void AddHyperLink(this StringBuilder sb, Action<StringBuilder> writeUrl, string text, string toolTip)
        {
            sb.Append("<a href=\"");
            writeUrl(sb);
            sb.AppendFormat("\" title=\"{0}\" class=\"linkAsButton\">{1}</a>", HttpUtility.HtmlAttributeEncode(toolTip), text);
        }

        public static void AddHyperLink(this StringBuilder sb, string url, string text, string toolTip, string question)
        {
            sb.AppendFormat(
                "<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\" onclick=\"return confirm('{3}')\">{2}</a>",
                url, HttpUtility.HtmlAttributeEncode(toolTip), text, HttpUtility.HtmlAttributeEncode(question));
        }

        public static void AddHyperLinkWithClick(this StringBuilder sb, string url, string text, string toolTip, string onclick)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{1}\" class=\"linkAsButton\" onclick=\"{3}\">{2}</a>", url,
                            HttpUtility.HtmlAttributeEncode(toolTip), text, HttpUtility.HtmlAttributeEncode(onclick));
        }

        public static void AddLinkButton(this StringBuilder sb, Page page, Control control, string argument, string text,
                                         string toolTip)
        {
            sb.AppendFormat("<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                            HttpUtility.HtmlAttributeEncode(toolTip),
                            page.ClientScript.GetPostBackEventReference(control, argument), text);
        }

        public static void AddLinkButton(this StringBuilder sb, Page page, Control control, string argument, string text,
                                         string toolTip, string question)
        {
            sb.AppendFormat(
                "<a href=\"javascript:if(confirm('{1}')) {2};\" title=\"{0}\" class=\"linkAsButton\" >{3}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                HttpUtility.HtmlAttributeEncode(question),
                page.ClientScript.GetPostBackEventReference(control, argument), text);
        }

        public static void AddLinkButtonForArray(StringBuilder sb, Page page, Control control, string argument,
                                                 string text, string toolTip, params string[] keys)
        {
            var jss = new JavaScriptSerializer();
            string serializedKeys = jss.Serialize(keys);
            sb.AppendFormat(
                "<a href=\"javascript:{1}\" title=\"{0}\" class=\"linkAsButton\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(toolTip),
                page.ClientScript.GetPostBackEventReference(control, "{0}").
                    Replace("'{0}'", string.Format("'{0}' + GetSerializedValues('{1}')", argument, serializedKeys)),
                text);
        }

        public static void AddDropDownList(StringBuilder sb, string clientID, object value, string width,
                                           IEnumerable<KeyValuePair<object, object>> dataSource)
        {
            sb.AppendFormat(dropDownListStart, clientID, width, dropDownListScript);
            if (dataSource != null)
                foreach (var row in dataSource)
                {
                    var optGroupDataSource = row.Value as IEnumerable<KeyValuePair<object, object>>;
                    if (optGroupDataSource != null && row.Key is string)
                        AddOptGroup(sb, value, (string)row.Key, optGroupDataSource);
                    else if (row.Key == null && value == null || row.Key != null && row.Key.Equals(value))
                        sb.AppendFormat(dropDownListSelectedOption, row.Key, row.Value);
                    else
                        sb.AppendFormat(dropDownListOption, row.Key, row.Value);
                }
            sb.Append(dropDownListEnd);
        }

        private static void AddOptGroup(StringBuilder sb, object value, string label, IEnumerable<KeyValuePair<object, object>> dataSource)
        {
            sb.AppendFormat("<optgroup label=\"{0}\">", label);

            foreach (var row in dataSource)
            {
                if (row.Key == null && value == null || (row.Key != null && row.Key.Equals(value)))
                    sb.AppendFormat(dropDownListSelectedOption, row.Key, row.Value);
                else
                    sb.AppendFormat(dropDownListOption, row.Key, row.Value);
            }

            sb.Append("</optgroup>");
        }

        private static void AddOptGroup(HtmlTextWriter writer, object value, string label, IEnumerable<KeyValuePair<object, object>> dataSource)
        {
            writer.AddAttribute("label", label);
            writer.RenderBeginTag("optgroup");

            foreach (var row in dataSource)
            {
                if (row.Key == null && value == null || row.Key != null && row.Key.Equals(value))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, (row.Key ?? "").ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Option);
                    writer.Write(row.Value);
                    writer.RenderEndTag();
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, (row.Key ?? "").ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Option);
                    writer.Write(row.Value);
                    writer.RenderEndTag();
                }
            }

            writer.RenderEndTag();
        }

        public static void AddDropDownList(StringBuilder sb, string clientID, object value, string width, string onChange,
                                           IEnumerable<KeyValuePair<object, object>> dataSource)
        {
            sb.AppendFormat(dropDownListStartOnChange, clientID, width, onChange, dropDownListScript);
            if (dataSource != null)
                foreach (var row in dataSource)
                {
                    if (row.Key == null && value == null || (row.Key != null && row.Key.Equals(value)))
                        sb.AppendFormat(dropDownListSelectedOption, row.Key, row.Value);
                    else
                        sb.AppendFormat(dropDownListOption, row.Key, row.Value);
                }
            sb.AppendFormat(dropDownListEnd);
        }


        public static void AddDropDownList(StringBuilder sb, string clientID, object value, string width, string onChange,
                                           IEnumerable<KeyValuePair<object, List<Pair>>> dataSource)
        {
            sb.AppendFormat(dropDownListStartOnChange, clientID, width, onChange, dropDownListScript);
            if (dataSource != null)
                foreach (var row in dataSource)
                {
                    sb.AppendFormat(@"<option value=""{0}""", row.Key);
                    var text = "";
                    if (row.Value != null)
                        foreach (var parameter in row.Value)
                        {
                            if ("text".Equals(parameter.First.ToString(), StringComparison.CurrentCulture))
                            {
                                text = parameter.Second.ToString();
                                continue;
                            }
                            sb.Append(' ');
                            sb.Append(parameter.First);
                            sb.Append("=\"");
                            sb.Append(HttpUtility.HtmlAttributeEncode(parameter.Second.ToString()));
                            sb.Append("\"");
                        }
                    if (row.Key == null && value == null || (row.Key != null && row.Key.Equals(value)))
                        sb.Append(@" selected=""selected""");
                    sb.Append(">");
                    sb.Append(text);
                    sb.Append("</option>");
                }
            sb.AppendFormat(dropDownListEnd);
        }

        public static void AddTextArea(StringBuilder sb, string clientID, object value, int rows, int columns)
        {
            sb.AppendFormat(textarea1, clientID, rows, columns, value);
        }

        public static void AddTextBox(StringBuilder sb, string clientID, object value, int maxLength, int columns,
                                      string width)
        {
            if (columns > 0)
            {
                if(maxLength > 0)
                    sb.AppendFormat(textBox1, clientID, maxLength, columns, value);
                else
                    sb.AppendFormat(textBox1_2, clientID, columns, value);
            }
            else if (maxLength > 0)
                sb.AppendFormat(textBox2, clientID, maxLength, width ?? "100%", value);
            else
                sb.AppendFormat(textBox3, clientID, width ?? "100%", value);
        }

        public static void AddTextBox(StringBuilder sb, string clientID, object value, string width)
        {
            sb.AppendFormat(textBox3, clientID, width, value);
        }

        public static void AddLookupTextBox(
            StringBuilder sb,
            string clientID,
            object value,
            string textValue,
            string tableName,
            string projectName,
            string mode,
            ExtenderAjaxControl extenderAjaxControl,
            int minimumPrefixLength,
            string width)
        {
            using (var textWriter = new StringWriter(sb))
            using (var writer = new HtmlTextWriter(textWriter))
            {
                var lookup = new BaseLookup
                    {
                        ID = clientID,
                        TableName = tableName,
                        SelectMode = mode,
                        Value = value,
                        Text = textValue,
                        ProjectName = projectName,
                        Width = new Unit(width),
                        IsMultipleSelect = false,
                    };
                lookup.Render(writer, extenderAjaxControl);
            }
        }

        public static void AddLookupTextBox(StringBuilder sb, string clientID, object value, string textValue,
                                    string tableName, string projectName, string mode, BrowseFilterParameters browseFilterParameters,
                                    ExtenderAjaxControl extenderAjaxControl, int minimumPrefixLength, string width)
        {
            AddLookupTextBox(sb, clientID, value, textValue, tableName, projectName, mode, browseFilterParameters,
                             extenderAjaxControl, minimumPrefixLength, width, false);
        }

        public static void AddLookupTextBox(
            StringBuilder sb,
            string clientID,
            object value,
            string textValue,
            string tableName,
            string projectName,
            string mode,
            BrowseFilterParameters browseFilterParameters,
            ExtenderAjaxControl extenderAjaxControl,
            int minimumPrefixLength,
            string width,
            bool isMultipleSelect)
        {
            var lookup = new BaseLookup
                {
                    ID = clientID,
                    TableName = tableName,
                    SelectMode = mode,
                    Value = value,
                    Text = textValue,
                    BrowseFilterParameters = browseFilterParameters,
                    ProjectName = projectName,
                    Width = new Unit(width),
                    IsMultipleSelect = isMultipleSelect,
                    MinimumPrefixLength = minimumPrefixLength,
                };
            lookup.Render(sb, extenderAjaxControl);
        }

        public static void AddLookupTextBox(
            StringBuilder sb,
            string clientID,
            object value,
            string textValue,
            string tableName,
            string projectName,
            string mode,
            string alternativeColumnWidth,
            object alternativeColumnValue,
            ExtenderAjaxControl extenderAjaxControl,
            int minimumPrefixLength,
            string width)
        {
            var lookup = new BaseLookup
                {
                    ID = clientID,
                    TableName = tableName,
                    SelectMode = mode,
                    Value = value,
                    Text = textValue,
                    AlternativeCellWidth = alternativeColumnWidth,
                    AlternativeColumnValue = alternativeColumnValue,
                    ProjectName = projectName,
                    Width = new Unit(width),
                    IsMultipleSelect = false,
                };
            lookup.Render(sb, extenderAjaxControl);
        }

        public static void AddLookupTextBox(StringBuilder sb, string clientID, object value, string textValue,
                                    string tableName, string projectName, string mode, string alternativeColumnWidth,
                                    object alternativeColumnValue, BrowseFilterParameters browseFilterParameters,
                                    ExtenderAjaxControl extenderAjaxControl, int minimumPrefixLength, string width)
        {
            AddLookupTextBox(sb, clientID, value, textValue, tableName, projectName, mode, alternativeColumnWidth,
                                    alternativeColumnValue, browseFilterParameters,
                                    extenderAjaxControl, minimumPrefixLength, width, false);
        }

        public static void AddLookupTextBox(
            StringBuilder sb,
            string clientID,
            object value,
            string textValue,
            string tableName,
            string projectName,
            string mode,
            string alternativeColumnWidth,
            object alternativeColumnValue,
            BrowseFilterParameters browseFilterParameters,
            ExtenderAjaxControl extenderAjaxControl,
            int minimumPrefixLength,
            string width,
            bool isMultipleSelect)
        {
            using (var textWriter = new StringWriter(sb))
            using (var writer = new HtmlTextWriter(textWriter))
            {
                var lookup = new BaseLookup
                    {
                        ID = clientID,
                        TableName = tableName,
                        SelectMode = mode,
                        Value = value,
                        Text = textValue,
                        AlternativeCellWidth = alternativeColumnWidth,
                        AlternativeColumnValue = alternativeColumnValue,
                        BrowseFilterParameters = browseFilterParameters,
                        ProjectName = projectName,
                        Width = new Unit(width),
                        IsMultipleSelect = isMultipleSelect,
                    };
                lookup.Render(writer, extenderAjaxControl);
            }
        }

        public static void AddAutoCompliteForLookup(ExtenderAjaxControl extenderAjaxControl, string clientID, int minimumPrefixLength)
        {
            AddAutoCompliteForLookup(extenderAjaxControl, clientID, minimumPrefixLength, null);
        }

        public static void AddAutoCompliteForLookup(ExtenderAjaxControl extenderAjaxControl, string clientID, int minimumPrefixLength, Control dependendControl)
        {
            var extender = new AutoCompleteExtender
               {
                   ID = "ac_" + clientID,
                   ServicePath = "/WebServiceAutoComplete.asmx",
                   ServiceMethod = "GetCompletionList",
                   BehaviorID = "acb_" + clientID,
                   MinimumPrefixLength = minimumPrefixLength,
                   CompletionInterval = 500,
                   EnableCaching = false,
                   CompletionSetCount = 10,
                   FirstRowSelected = true,
                   UseContextKey = true,
                   CompletionListCssClass = "autocomplete_completionListElement",
                   CompletionListItemCssClass = "autocomplete_listItem",
                   CompletionListHighlightedItemCssClass = "autocomplete_highlightedListItem",
                   OnClientPopulated = "OnLookupPopulated",
                   OnClientPopulating = "OnLookupPopulating",
                   OnClientItemSelected = "OnLookupItemSelected",
               };
            extenderAjaxControl.AddExtender(extender, clientID, dependendControl);
        }

        public static void AddCollapsiblePanel(ExtenderAjaxControl extenderAjaxControl, string clientID, string collapseControlID, string expandControlID)
        {
            AddCollapsiblePanel(extenderAjaxControl, clientID, collapseControlID, expandControlID, true);
        }

        public static void AddCollapsiblePanel(ExtenderAjaxControl extenderAjaxControl, string clientID, string collapseControlID, string expandControlID, bool collapsed)
        {
            var extender = new CollapsiblePanelExtender
                {
                    ID = "cp_" + clientID,
                    BehaviorID = "cpb_" + clientID,
                    Collapsed = collapsed,
                    ExpandDirection = CollapsiblePanelExpandDirection.Vertical,
                    CollapseControlID = collapseControlID,
                    ExpandControlID = expandControlID,
                };
            extenderAjaxControl.AddExtender(extender, clientID);
        }

        public static void AddDropDownList(StringBuilder sb, string clientID, long? value, string width,
                                           long? valueNotSet, string textNotSet, IDataSource dataSource)
        {
            DataSourceView view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                sb.AppendFormat(dropDownListStart, clientID, width, dropDownListScript);
                if (valueNotSet == value)
                    sb.AppendFormat(dropDownListSelectedOption, valueNotSet, textNotSet);
                else
                    sb.AppendFormat(dropDownListOption, valueNotSet, textNotSet);
                foreach (object row in dataSourceData)
                {
                    PropertyInfo propertyID = row.GetType().GetProperty("id");
                    PropertyInfo propertyName = row.GetType().GetProperty(fieldName);
                    var rowID = (long) propertyID.GetValue(row, null);
                    object rowName = propertyName.GetValue(row, null);
                    if (rowID == value)
                        sb.AppendFormat(dropDownListSelectedOption, rowID, rowName);
                    else
                        sb.AppendFormat(dropDownListOption, rowID, rowName);
                }
                sb.AppendFormat(dropDownListEnd);
            }
        }

        public static void AddDropDownList(StringBuilder sb, string clientID, long? value, string width, IDataSource dataSource)
        {
            DataSourceView view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                sb.AppendFormat(dropDownListStart, clientID, width, dropDownListScript);
                foreach (object row in dataSourceData)
                {
                    PropertyInfo propertyID = row.GetType().GetProperty("id");
                    PropertyInfo propertyName = row.GetType().GetProperty(fieldName);
                    var rowID = (long) propertyID.GetValue(row, null);
                    object rowName = propertyName.GetValue(row, null);
                    if (rowID == value)
                        sb.AppendFormat(dropDownListSelectedOption, rowID, rowName);
                    else
                        sb.AppendFormat(dropDownListOption, rowID, rowName);
                }
                sb.AppendFormat(dropDownListEnd);
            }
        }

        public static void AddDropDownList(StringBuilder sb, string clientID, string value, string width, IDataSource dataSource, string keyFieldName)
        {
            DataSourceView view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                sb.AppendFormat(dropDownListStart, clientID, width, dropDownListScript);
                foreach (object row in dataSourceData)
                {
                    object rowID;
                    if (row is IDataRow)
                        rowID = GetValue(row, "Item." + keyFieldName);
                    else
                        rowID = GetValue(row, keyFieldName);
                    var rowName = GetValue(row, fieldName);
                    if (value != null && value.Equals(rowID.ToString()))
                        sb.AppendFormat(dropDownListSelectedOption, rowID, rowName);
                    else
                        sb.AppendFormat(dropDownListOption, rowID, rowName);
                }

                sb.AppendFormat(dropDownListEnd);
            }
        }

        private static object GetValue(object obj, string keyFieldName)
        {
            foreach (var property in keyFieldName.Split('.'))
            {
                if (obj == null)
                    return null;
                var properties = TypeDescriptor.GetProperties(obj);
                var descriptor = properties[property];
             
                if (descriptor == null)
                {
                    var message = string.Format("Не найдено свойство {0} у типа {1}", property, obj.GetType().FullName);
                    throw new ArgumentException(message, keyFieldName);
                }

                obj = descriptor.GetValue(obj);
            }

            return obj;
        }

        public static void AddImage(this StringBuilder sb, string url, string imageUrl, string toolTip)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{2}\"><img style=\"border:0px\" src=\"{1}\" alt=\"{2}\"/></a>", url, imageUrl, HttpUtility.HtmlAttributeEncode(toolTip));
        }

        public static void AddImage(this StringBuilder sb, Action<StringBuilder> writeUrl, string imageUrl, string toolTip)
        {
            sb.Append("<a href=\"");
            writeUrl(sb);
            sb.AppendFormat("\" title=\"{1}\"><img style=\"border:0px\" src=\"{0}\" alt=\"{1}\"/></a>", imageUrl, HttpUtility.HtmlAttributeEncode(toolTip));
        }

        public static void AddImage(this StringBuilder sb, Control control, string argument, string imageUrl, string toolTip)
        {
            var url = control.Page.ClientScript.GetPostBackEventReference(control, argument);
            sb.AppendFormat("<a href=\"javascript:{0}\" title=\"{2}\"><img style=\"border:0px\" src=\"{1}\" alt=\"{2}\"/></a>", url, imageUrl, HttpUtility.HtmlAttributeEncode(toolTip));
        }

        public static void AddImage(this StringBuilder sb, string url, string imageUrl, string toolTip, string onclick)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{2}\" onclick=\"{3}\"><img style=\"border:0px\" src=\"{1}\" alt=\"{2}\"/></a>", url, imageUrl, HttpUtility.HtmlAttributeEncode(toolTip), HttpUtility.HtmlAttributeEncode(onclick));
        }

        public static void AddImage(this StringBuilder sb, string url, string imageUrl, string toolTip, string onclick, string id)
        {
            sb.AppendFormat("<a id=\"{4}\" href=\"{0}\" title=\"{2}\" onclick=\"{3}\"><img style=\"border:0px\" src=\"{1}\" alt=\"{2}\"/></a>", url, imageUrl, HttpUtility.HtmlAttributeEncode(toolTip), HttpUtility.HtmlAttributeEncode(onclick), id);
        }

        public static void AddImage(this StringBuilder sb, string imageUrl, string alt)
        {
            sb.AppendFormat("<img style=\"border:0px\" src=\"{0}\" alt=\"{1}\"/>", imageUrl, HttpUtility.HtmlAttributeEncode(alt));
        }

        public static void AddImageWithTarget(this StringBuilder sb, string url, string imageUrl, string toolTip, string target)
        {
            sb.AppendFormat("<a href=\"{0}\" title=\"{2}\" target=\"{3}\"><img style=\"border:0px\" src=\"{1}\" alt=\"{2}\"/></a>", url, imageUrl, HttpUtility.HtmlAttributeEncode(toolTip), target);
        }

        public static void AddDatePiker(StringBuilder sb, string clientID, DateTime? value, int widthPixel, 
                                        DatePickerMode mode, ExtenderAjaxControl extenderAjaxControl)
        {
            var dateTime = "";
            if (value.HasValue)
                switch (mode)
                {
                    case DatePickerMode.Date:
                        dateTime = value.Value.ToShortDateString();
                        break;
                    case DatePickerMode.Time:
                        dateTime = value.Value.ToShortTimeString();
                        break;
                    case DatePickerMode.DateTime:
                        dateTime = value.Value.ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
            sb.Append(
                $"<span style=\"white-space: nowrap;\" style=\"width:{widthPixel}\">"
                + $"<input type=\"text\" id=\"{clientID}\" style=\"width:{widthPixel - 23}px\" value=\"{dateTime}\" />"
                + $"<input type=\"image\" id=\"Img{clientID}\" style=\"cursor:hand\" src=\"{Themes.IconUrlCalendar}\" /></span>");

            var calendarExtender = new CalendarExtender
                               {
                                   ID = "ce_" + clientID,
                                   PopupButtonID = "Img" + clientID,
                               };
            extenderAjaxControl.AddExtender(calendarExtender, clientID);
            var maskedEditExtender = new MaskedEditExtender
            {
                ID = "mee_" + clientID,
                AcceptAMPM = true,
                ClearMaskOnLostFocus = true,
                CultureName = CultureInfo.CurrentCulture.Name,
            };
            extenderAjaxControl.AddExtender(maskedEditExtender, clientID);

            if (mode == DatePickerMode.Date)
            {
                calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                maskedEditExtender.MaskType = MaskedEditType.Date;
                String dateMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                dateMask = Regex.Replace(dateMask, "\\w", "9");
                dateMask = Regex.Replace(dateMask, "\\W", "/");
                maskedEditExtender.Mask = dateMask;
            }
            else if (mode == DatePickerMode.Time)
            {
                calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                maskedEditExtender.MaskType = MaskedEditType.Time;
                String timeMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

                timeMask = Regex.Replace(timeMask, @"(?<1>\w)(?<!\k<1>\k<1>)(?!\k<1>)", @"$1$1");
                timeMask = Regex.Replace(timeMask, "\\w", @"9");
                timeMask = Regex.Replace(timeMask, "\\W", @":");
                maskedEditExtender.Mask = timeMask;
            }
            else
            {
                calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                    CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                maskedEditExtender.MaskType = MaskedEditType.DateTime;

                String dateMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                dateMask = Regex.Replace(dateMask, "\\w", "9");
                dateMask = Regex.Replace(dateMask, "\\W", "/");

                String timeMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                timeMask = Regex.Replace(timeMask, @"(?<1>\w)(?<!\k<1>\k<1>)(?!\k<1>)", @"$1$1");
                timeMask = Regex.Replace(timeMask, "\\w", @"9");
                timeMask = Regex.Replace(timeMask, "\\W", @":");

                maskedEditExtender.Mask = String.Format("{0} {1}", dateMask, timeMask);
            }

        }

        public static string GetControlToSetHeight(string clientID)
        {
            return string.Format("<a href=\"minimize\" onclick=\"return _changeHeight(this,'{0}','{1}','{2}');\" title=\"{3}\" class=\"linkAsButton\">{1}</a>",
                clientID,
                HttpUtility.HtmlAttributeEncode(Resources.SChangeHieghtMore),
                HttpUtility.HtmlAttributeEncode(Resources.SChangeHieghtLess),
                HttpUtility.HtmlAttributeEncode(Resources.SChangeHieght));
        }

        public static void AddDropDownList(HtmlTextWriter writer, string clientId, long? value, string width,
                                           long? valueNotSet, string textNotSet, IDataSource dataSource)
        {
            DataSourceView view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
                writer.AddAttribute("onmouseover", dropDownListScript);
                writer.RenderBeginTag(HtmlTextWriterTag.Select);

                if (valueNotSet == value)
                    AddOptionInDropDownList(writer, valueNotSet.ToString(), textNotSet, true);
                else
                    AddOptionInDropDownList(writer, valueNotSet.ToString(), textNotSet, false);

                foreach (var row in dataSourceData)
                {
                    var propertyId = row.GetType().GetProperty("id");
                    var propertyName = row.GetType().GetProperty(fieldName);
                    var rowId = (long)propertyId.GetValue(row, null);
                    var rowName = propertyName.GetValue(row, null);
                    if (rowId == value)
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, true);
                    else
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, false);
                }
                writer.RenderEndTag();
            }
        }

        private static void AddOptionInDropDownList(HtmlTextWriter writer, string key, object value, bool isSelected)
        {
            if (isSelected)
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, key);
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(value == null ? "" : value.ToString());
            writer.RenderEndTag();
        }

        private static void AddOptionInDropDownList(HtmlTextWriter writer, string key, object value, object title, bool isSelected)
        {
            if (isSelected)
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, key);
            if (title != null)
                writer.AddAttribute(HtmlTextWriterAttribute.Title, title.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(value == null ? string.Empty : value.ToString());
            writer.RenderEndTag();
        }

        public static void AddDropDownList(HtmlTextWriter writer, string clientId, long? value, string width,
                                           IDataSource dataSource)
        {
            DataSourceView view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
                writer.AddAttribute("onmouseover", dropDownListScript);
                writer.RenderBeginTag(HtmlTextWriterTag.Select);

                foreach (var row in dataSourceData)
                {
                    var propertyId = row.GetType().GetProperty("id");
                    var propertyName = row.GetType().GetProperty(fieldName);
                    var rowId = (long)propertyId.GetValue(row, null);
                    var rowName = propertyName.GetValue(row, null);
                    if (rowId == value)
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, true);
                    else
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, false);
                }
                writer.RenderEndTag();
            }
        }

        public static void AddDropDownList(HtmlTextWriter writer, string clientId, string value, string width,
                                           IDataSource dataSource)
        {
            var view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
                writer.AddAttribute("onmouseover", dropDownListScript);
                writer.RenderBeginTag(HtmlTextWriterTag.Select);

                foreach (var row in dataSourceData)
                {
                    var irow = row as IDataRow;
                    if (irow != null)
                    {
                        AddOptionInDropDownList(writer, irow.Value, irow.Name,
                                                irow.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
                        continue;
                    }
                    var propertyId = row.GetType().GetProperty("id");
                    var propertyName = row.GetType().GetProperty(fieldName);
                    var rowId = Convert.ToString(propertyId.GetValue(row, null));
                    var rowName = propertyName.GetValue(row, null);
                    AddOptionInDropDownList(writer, rowId, rowName,
                                            rowId.Equals(value, StringComparison.OrdinalIgnoreCase));
                }
                writer.RenderEndTag();
            }
        }

        public static void AddDropDownList(HtmlTextWriter writer, string clientId, string value, string width,
                                           IDataSource dataSource, string keyFieldName)
        {
            var view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
            if (dataSourceData != null)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
                writer.AddAttribute("onmouseover", dropDownListScript);
                writer.RenderBeginTag(HtmlTextWriterTag.Select);

                foreach (var row in dataSourceData)
                {
                    var irow = row as IDataRow;
                    string rowId;
                    object rowName;
                    if (irow != null)
                    {
                        rowId = Convert.ToString(GetValue(row, "Item." + keyFieldName));
                        rowName = irow.Name;
                    }
                    else
                    {
                        rowId = Convert.ToString(GetValue(row, keyFieldName));
                        rowName = GetValue(row, fieldName);
                    }

                    AddOptionInDropDownList(writer, rowId, rowName, rowId.Equals(value, StringComparison.OrdinalIgnoreCase));
                }

                writer.RenderEndTag();
            }
        }

        public static void AddDropDownList(HtmlTextWriter writer, string clientId, object value, string width, string onChange,
                                           IEnumerable<KeyValuePair<object, object>> dataSource)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
            if (!string.IsNullOrEmpty(onChange))
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, onChange);
            writer.AddAttribute("onmouseover", dropDownListScript);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            if (dataSource != null)
                foreach (var row in dataSource)
                {
                    var optGroupDataSource = row.Value as IEnumerable<KeyValuePair<object, object>>;
                    if (optGroupDataSource != null && row.Key is string)
                        AddOptGroup(writer, value, (string)row.Key, optGroupDataSource);
                    else if (row.Key == null && value == null || (row.Key != null && row.Key.Equals(value)))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, (row.Key ?? "").ToString());
                        writer.RenderBeginTag(HtmlTextWriterTag.Option);
                        writer.Write(row.Value);
                        writer.RenderEndTag();
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, (row.Key ?? "").ToString());
                        writer.RenderBeginTag(HtmlTextWriterTag.Option);
                        writer.Write(row.Value);
                        writer.RenderEndTag();
                    }
                }
            writer.RenderEndTag();
        }

        public static void AddTextBox(HtmlTextWriter writer, string clientId, object value, int maxLength, int columns, string width)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, clientId);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value == null ? "" : value.ToString());
            if (maxLength > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, maxLength.ToString());
            if (columns > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Size, columns.ToString());
            else
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width ?? "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        public static void RenderDropDownList(this HtmlTextWriter writer, IDropDownList dropDownList, ExtenderAjaxControl extenderAjaxControl)
        {
            IEnumerable dataSourceData = dropDownList.GetData();
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, dropDownList.Width.ToString());
            writer.AddAttribute(HtmlTextWriterAttribute.Id, dropDownList.ClientID);
            if (!string.IsNullOrEmpty(dropDownList.UniqueID))
                writer.AddAttribute(HtmlTextWriterAttribute.Name, dropDownList.UniqueID);
            if (!dropDownList.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled"); 
            writer.AddAttribute("onmouseover", dropDownListScript);

            var control = dropDownList as WebControl;
            if (control != null && control.HasAttributes)
            {
                foreach (string attributeKey in control.Attributes.Keys)
                    writer.AddAttribute(attributeKey, control.Attributes[attributeKey]);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            if (dropDownList.AllowValueNotSet)
            {
                AddOptionInDropDownList(
                    writer,
                    null,
                    dropDownList.TextOfValueNotSet ?? Resources.SNotSpecified,
                    Resources.SNotSpecified,
                    dropDownList.SelectedValue == null || string.Empty.Equals(dropDownList.SelectedValue));
            }

            if (dataSourceData != null)
            {
                string selectedValue = Convert.ToString(dropDownList.SelectedValue);
                foreach (var row in dataSourceData)
                {
                    object rowId;
                    object rowName;
                    var dataRow = row as IDataRow;
                    if (dataRow != null && ("id".Equals(dropDownList.IDPropertyName) || "Value".Equals(dropDownList.IDPropertyName)))
                    {
                        rowId = dataRow.Value;
                    }
                    else
                    {
                        string keyFieldName;

                        if ("id".Equals(dropDownList.IDPropertyName))
                            keyFieldName = "id";
                        else
                        {
                            keyFieldName = dataRow != null
                                               ? "Item." + dropDownList.IDPropertyName
                                               : dropDownList.IDPropertyName;
                        }

                        rowId = GetValue(row, keyFieldName);
                    }

                    if (dataRow != null
                        && ("Name".Equals(dropDownList.IDPropertyName) || "nameRu".Equals(dropDownList.IDPropertyName)
                            || "nameKz".Equals(dropDownList.IDPropertyName)))
                    {
                        rowName = dataRow.Name;
                    }
                    else
                    {
                        string keyFieldName;
                        if ("Name".Equals(dropDownList.NamePropertyName))
                            keyFieldName = "Name";
                        else
                        {
                            keyFieldName = dataRow != null
                                               ? "Item." + dropDownList.NamePropertyName
                                               : dropDownList.NamePropertyName;
                        }

                        rowName = GetValue(row, keyFieldName);
                    }

                    object rowTitle = null;
                    if (dropDownList.TitlePropertyName != null)
                        rowTitle = GetValue(row, dropDownList.TitlePropertyName);

                    if (Convert.ToString(rowId).Equals(selectedValue))
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, rowTitle, true);
                    else
                        AddOptionInDropDownList(writer, rowId.ToString(), rowName, rowTitle, false);
                }
            }

            writer.RenderEndTag();
        }

        public static void RenderLookup(this HtmlTextWriter writer, ILookup lookup, ExtenderAjaxControl extenderAjaxControl)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, lookup.Width.ToString());
            writer.AddStyleAttribute("float", "left");
            writer.AddStyleAttribute("cellPadding", "0");
            writer.AddStyleAttribute("cellSpacing", "0");
            writer.AddStyleAttribute("border-collapse", "collapse");
            writer.AddAttribute("isLookup", "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            
            #region tr content

            #region td alternate

            if (string.IsNullOrEmpty(lookup.AlternativeCellWidth))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
                
                writer.RenderEndTag();
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, lookup.AlternativeCellWidth);
                writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "tbA_" + lookup.ClientID);
                if (!string.IsNullOrEmpty(lookup.UniqueID))
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, lookup.UniqueID + "$tbA");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, (lookup.AlternativeColumnValue ?? "").ToString());
                writer.AddAttribute("isCode", "true");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, lookup.AlternativeCellWidth);
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            #endregion

            #region td text value

            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "tbT_" + lookup.ClientID);
            if (!string.IsNullOrEmpty(lookup.UniqueID))
                writer.AddAttribute(HtmlTextWriterAttribute.Name, lookup.UniqueID + "$tbT" );
            writer.AddAttribute(HtmlTextWriterAttribute.Value, lookup.Text);
            writer.AddAttribute("isKz", LocalizationHelper.IsCultureKZ.ToString());
            writer.AddAttribute("mode", lookup.SelectMode);
            writer.AddAttribute("viewmode", lookup.ViewMode);
            writer.AddAttribute("dataSource", lookup.ProjectName + "." + lookup.TableName + "JournalDataSourceView");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "98%");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            writer.RenderEndTag();

            #endregion

            #region td id value

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "30px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            #region browse

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "ibBrowse_" + lookup.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SSelectText);
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SSelectText);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlBrowse);
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                string.Format("SetLookup(this); ReadValues('/MainPage.aspx/data/{0}Edit/read?{0}='); window.showModalDialog('/MainPage.aspx/data/{0}Journal/{3}?mode={1}&viewmode={5}&__SKVColumn={2}&{4}=' + valueID + GetLookupFilters(this), window, sFeatures); return false;",
                lookup.TableName, //0
                lookup.SelectMode, //1
                lookup.SelectKeyValueColumn, //2
                lookup.IsMultipleSelect ? "multipleselect" : "Select", //3
                lookup.IsMultipleSelect ? "__selected" : "ref" + lookup.TableName, //4
                lookup.ViewMode //5
                ));
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            #endregion

            #region value

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, lookup.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, (lookup.Value ?? "").ToString());
            writer.AddAttribute("isIgnoreVisible", "true");
            if (!string.IsNullOrEmpty(lookup.UniqueID))
                writer.AddAttribute(HtmlTextWriterAttribute.Name, lookup.UniqueID);
            if (!string.IsNullOrEmpty(lookup.OnChangedValue))
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, lookup.OnChangedValue);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            #endregion

            #region other values

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "tbValues_" + lookup.ClientID);
            writer.AddAttribute("isIgnoreVisible", "true");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            #endregion

            #region browseParameters

            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (lookup.BrowseFilterParameters != null)
                writer.Write(lookup.BrowseFilterParameters.GetClientParameters());
            writer.RenderEndTag();

            #endregion

            #region selectInfo

            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(lookup.SelectInfo == null ? "{}" : new JavaScriptSerializer().Serialize(lookup.SelectInfo));
            writer.RenderEndTag();

            #endregion

            writer.RenderEndTag();

            #endregion

            #region td clear value

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            #region set null value

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "image");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "bNull_" + lookup.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SClearText);
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "X");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlClearBrowseValue);
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "SetLookup(this); NullValues(); return false;");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            #endregion

            writer.RenderEndTag();

            #endregion

            #region td info
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "20px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Padding, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            #region A
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "hlInfo_" + lookup.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SInformationText);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format("/MainPage.aspx/data/{0}Edit/read?ref{0}=", lookup.TableName));
            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            
            #region image

            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SInformationText);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlBrowseInfo);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            #endregion
            
            writer.RenderEndTag();
            #endregion

            writer.RenderEndTag();

            #endregion

            #endregion

            writer.RenderEndTag();
            writer.RenderEndTag();

            if (extenderAjaxControl != null)
                AddAutoCompliteForLookup(extenderAjaxControl, "tbT_" + lookup.ClientID, lookup.MinimumPrefixLength);
        }

        public static void RenderTextBox(this HtmlTextWriter writer, ITextBox textBox, ExtenderAjaxControl extenderAjaxControl)
        {
            if (!textBox.Width.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, textBox.Width.ToString());
            if (!textBox.Height.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, textBox.Height.ToString());
            if (!string.IsNullOrEmpty(textBox.ToolTip))
                writer.AddAttribute(HtmlTextWriterAttribute.Title, textBox.ToolTip);

            if (!string.IsNullOrEmpty(textBox.UniqueID))
                writer.AddAttribute(HtmlTextWriterAttribute.Name, textBox.UniqueID);
            if (textBox.Rows != null)
                writer.AddAttribute(HtmlTextWriterAttribute.Rows, textBox.Rows.ToString());
            if (textBox.Columns != null)
                writer.AddAttribute(HtmlTextWriterAttribute.Cols, textBox.Columns.ToString());
            if (textBox.MaxLength != null)
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, textBox.MaxLength.ToString());
            if (!textBox.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, textBox.ClientID);
            if (textBox.IsMultipleLines)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
                writer.Write(textBox.Value);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, textBox.TextValue);
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
            }
            writer.RenderEndTag();
        }

        public static void RenderCheckBox(this HtmlTextWriter writer, ICheckBox checkBox, ExtenderAjaxControl extenderAjaxControl, Action<HtmlTextWriter> addInputAttributes = null)
        {
            if (!string.IsNullOrEmpty(checkBox.UniqueID))
                writer.AddAttribute(HtmlTextWriterAttribute.Name, checkBox.UniqueID);
            if (!checkBox.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            if (!string.IsNullOrEmpty(checkBox.ClientID))
                writer.AddAttribute(HtmlTextWriterAttribute.Id, checkBox.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if (!string.IsNullOrEmpty(checkBox.OnChange))
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, checkBox.OnChange);
            if (!string.IsNullOrEmpty(checkBox.OnClick))
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, checkBox.OnClick);
            if (checkBox.Checked)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, "on");
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            if (addInputAttributes != null)
                addInputAttributes(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            if (!string.IsNullOrEmpty(checkBox.Label))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.For, checkBox.ClientID);
                if (!checkBox.Enabled)
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                if (!string.IsNullOrEmpty(checkBox.ToolTip))
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, checkBox.ToolTip);
                writer.RenderBeginTag(HtmlTextWriterTag.Label);
                writer.Write(HttpUtility.HtmlEncode(checkBox.Label).Replace("\r\n", "<br />"));
                writer.RenderEndTag();
            }

            if (!string.IsNullOrEmpty(checkBox.FalseText) || !string.IsNullOrEmpty(checkBox.TrueText))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.For, checkBox.ClientID);
                writer.AddAttribute("checkedAs", "false");
                if (!checkBox.Enabled)
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                if (!string.IsNullOrEmpty(checkBox.ToolTip))
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, checkBox.ToolTip);
                if (checkBox.Checked)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Label);
                writer.Write(HttpUtility.HtmlEncode(checkBox.FalseText).Replace("\r\n", "<br />"));
                writer.RenderEndTag();
           
                writer.AddAttribute(HtmlTextWriterAttribute.For, checkBox.ClientID);
                writer.AddAttribute("checkedAs", "true");
                if (!checkBox.Enabled)
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                if (!string.IsNullOrEmpty(checkBox.ToolTip))
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, checkBox.ToolTip);
                if (!checkBox.Checked)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Label);
                writer.Write(HttpUtility.HtmlEncode(checkBox.TrueText).Replace("\r\n", "<br />"));
                writer.RenderEndTag();
                writer.RenderBeginTag(HtmlTextWriterTag.Script);
                writer.Write(
                    string.Format(
                        @"
$(function () {{
    $('#{0}').change(function () {{
        var no = $('[for={0}][checkedAs=false]');
        var yes = $('[for={0}][checkedAs=true]');
        no.toggle(!this.checked);
        yes.toggle(this.checked);
    }});
}})",
                        checkBox.ClientID));
                writer.RenderEndTag();
            }
        }

        public static void RenderHyperLink(this HtmlTextWriter writer, IHyperLink hyperLink)
        {
            writer.AddAttribute(
                HtmlTextWriterAttribute.Href,
                string.IsNullOrEmpty(hyperLink.Url) ? "javascript:void(0);" : hyperLink.Url);
            if (!hyperLink.Enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            if (!string.IsNullOrEmpty(hyperLink.ToolTip))
                writer.AddAttribute(HtmlTextWriterAttribute.Title, hyperLink.ToolTip);

            if (string.IsNullOrEmpty(hyperLink.ImgUrl) && hyperLink.RenderAsButton)
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "linkAsButton");
            else if (string.IsNullOrEmpty(hyperLink.ToolTip) && !string.IsNullOrEmpty(hyperLink.Text))
                writer.AddAttribute(HtmlTextWriterAttribute.Title, hyperLink.Text);

            if (!string.IsNullOrEmpty(hyperLink.ValueOfLink))
                writer.AddAttribute(HtmlTextWriterAttribute.Value, hyperLink.ValueOfLink);
            if (!string.IsNullOrEmpty(hyperLink.Target))
                writer.AddAttribute(HtmlTextWriterAttribute.Target, hyperLink.Target);

            if (!string.IsNullOrEmpty(hyperLink.OnClick))
            {
                var onclick = string.IsNullOrEmpty(hyperLink.OnClickQuestion)
                                  ? hyperLink.OnClick
                                  : string.Format(
                                      "if (confirm('{0}')) {1};",
                                      HttpUtility.HtmlAttributeEncode(hyperLink.OnClickQuestion),
                                      hyperLink.OnClick);
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onclick);
            }
            else if (!string.IsNullOrEmpty(hyperLink.OnClickQuestion))
            {
                var question = string.Format("return confirm('{0}')", HttpUtility.HtmlAttributeEncode(hyperLink.OnClickQuestion));
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, question);
            }


            var control = hyperLink as WebControl;
            if (control != null)
            {
                foreach (string attributeKey in control.Style.Keys)
                    writer.AddStyleAttribute(attributeKey, control.Style[attributeKey]);
            }

            if (string.IsNullOrEmpty(hyperLink.ImgUrl))
            {
                if (!hyperLink.Width.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, hyperLink.Width.ToString());
                if (!hyperLink.Height.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, hyperLink.Height.ToString()); 
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(hyperLink.Text);
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                if (!hyperLink.Width.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, hyperLink.Width.ToString());
                if (!hyperLink.Height.IsEmpty)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, hyperLink.Height.ToString());
                writer.AddAttribute(HtmlTextWriterAttribute.Src, hyperLink.ImgUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddAttribute(
                    HtmlTextWriterAttribute.Alt,
                    string.IsNullOrEmpty(hyperLink.Text) ? hyperLink.ToolTip : hyperLink.Text);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }
    }
}