/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 19 января 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Nat.Web.Controls.Properties;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.Filter;
using System.Web.Compilation;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web;

    using System.Globalization;
    using System.Windows.Forms;

    using AjaxControlToolkit;

    using Nat.Web.Controls.DateTimeControls;
    using Nat.Web.Tools.Initialization;

    using Control = System.Web.UI.Control;

    public delegate void GetFilterContent(StringBuilder sb);
    public delegate IQueryable FilterHandler(IQueryable enumerable, Enum filterType, string value1, string value2);
    public delegate Expression ExpressionFilterHandler(Enum filterType, string value1, string value2, QueryParameters qParams);
    public delegate Expression ExpressionFilterHandlerV2(Enum filterType, FilterItem filterItem, QueryParameters qParams);

    public class FilterHtmlGenerator
    {
        private static int id;

        #region FilterType enum

        public enum FilterType
        {
            Group,
            Reference,
            Numeric,
            Boolean,
            Text,
            FullTextSearch
        }

        #endregion

        private static IEnumerable<KeyValuePair<object, object>> booleanList
        {
            get
            {
                return new[] 
                    {
                        new KeyValuePair<object, object>("Non", InitializerSection.StaticFilterNamesResources.SSelectCondition),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> notMadatoryList
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.NotMadatoryFilterTypes;
                return list
                       ?? new[]
                    {
                        new KeyValuePair<object, object>("IsNotNull", InitializerSection.StaticFilterNamesResources.SIsFilled),
                        new KeyValuePair<object, object>("IsNull", InitializerSection.StaticFilterNamesResources.SIsNotFilled),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> numericList
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.NumericFilterTypes;
                return list
                       ?? new[]
                    {
                        new KeyValuePair<object, object>("Non", InitializerSection.StaticFilterNamesResources.SSelectCondition),
                        new KeyValuePair<object, object>("Equals", InitializerSection.StaticFilterNamesResources.SEquals),
                        new KeyValuePair<object, object>("NotEquals", InitializerSection.StaticFilterNamesResources.SNotEquals),
                        new KeyValuePair<object, object>("More", InitializerSection.StaticFilterNamesResources.SMore),
                        new KeyValuePair<object, object>("Less", InitializerSection.StaticFilterNamesResources.SLess),
                        new KeyValuePair<object, object>("Between", InitializerSection.StaticFilterNamesResources.SBetween),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> datetimeList
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.DatetimeFilterTypes;
                return list
                       ?? new[]
                    {
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.Non.ToString(), InitializerSection.StaticFilterNamesResources.SSelectCondition),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.Equals.ToString(), InitializerSection.StaticFilterNamesResources.SEquals),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.NotEquals.ToString(), InitializerSection.StaticFilterNamesResources.SNotEquals),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.More.ToString(), InitializerSection.StaticFilterNamesResources.SDateMore),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.Less.ToString(), InitializerSection.StaticFilterNamesResources.SDateLess),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.Between.ToString(), InitializerSection.StaticFilterNamesResources.SDateBetween),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.DaysAgoAndMore.ToString(), InitializerSection.StaticFilterNamesResources.SDaysAgoAndMore),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.DaysLeftAndMore.ToString(), InitializerSection.StaticFilterNamesResources.SDaysLeftAndMore),
                        new KeyValuePair<object, object>(DefaultFilters.NumericFilter.ToDay.ToString(), InitializerSection.StaticFilterNamesResources.SToDay),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> referenceList
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.ReferenceFilterTypes;
                return list
                       ?? new[]
                    {
                        new KeyValuePair<object, object>("Non", InitializerSection.StaticFilterNamesResources.SSelectCondition),
                        new KeyValuePair<object, object>("Equals", InitializerSection.StaticFilterNamesResources.SEquals),
                        new KeyValuePair<object, object>("NotEquals", InitializerSection.StaticFilterNamesResources.SNotEquals),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> referenceListWithTextFilter
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.ReferencesFilterTypes;
                return list
                       ?? new[]
                           {
                               new KeyValuePair<object, object>(
                                   "Non",
                                   InitializerSection.StaticFilterNamesResources.SSelectCondition),
                               new KeyValuePair<object, object>(
                                   "Equals", InitializerSection.StaticFilterNamesResources.SEquals),
                               new KeyValuePair<object, object>(
                                   "NotEquals",
                                   InitializerSection.StaticFilterNamesResources.SNotEquals),
                               new KeyValuePair<object, object>(
                                   "StartsWithByRef",
                                   InitializerSection.StaticFilterNamesResources.SStartsWith),
                               new KeyValuePair<object, object>(
                                   "EndsWithByRef",
                                   InitializerSection.StaticFilterNamesResources.SEndsWith),
                               new KeyValuePair<object, object>(
                                   "ContainsByRef",
                                   InitializerSection.StaticFilterNamesResources.SContains),
                               new KeyValuePair<object, object>(
                                   "ContainsWordsByRef",
                                   InitializerSection.StaticFilterNamesResources.SContainsWords),
                               new KeyValuePair<object, object>(
                                   "ContainsAnyWordByRef",
                                   InitializerSection.StaticFilterNamesResources.SContainsAnyWord),
                           };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> referenceListWithTextFilterForCode
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.ReferenceListWithTextFilterForCodeTypes;
                return list
                       ?? new[]
                    {
                        new KeyValuePair<object, object>("Non", InitializerSection.StaticFilterNamesResources.SSelectCondition),
                        new KeyValuePair<object, object>("StartsWithCode", InitializerSection.StaticFilterNamesResources.SEquals),
                        new KeyValuePair<object, object>("NotStartsWithCode", InitializerSection.StaticFilterNamesResources.SNotEquals),
                    };
            }
        }

        private static IEnumerable<KeyValuePair<object, object>> textList
        {
            get
            {
                var list = InitializerSection.StaticFilterNamesResources.TextFilterTypes;
                return list
                       ?? new[]
                        {
                            new KeyValuePair<object, object>("Non", InitializerSection.StaticFilterNamesResources.SSelectCondition),
                            new KeyValuePair<object, object>("Equals", InitializerSection.StaticFilterNamesResources.SEquals),
                            new KeyValuePair<object, object>("NotEquals", InitializerSection.StaticFilterNamesResources.SNotEquals),
                            new KeyValuePair<object, object>("StartsWith", InitializerSection.StaticFilterNamesResources.SStartsWith),
                            new KeyValuePair<object, object>("EndsWith", InitializerSection.StaticFilterNamesResources.SEndsWith),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.Contains.ToString(), InitializerSection.StaticFilterNamesResources.SContains),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.NotContains.ToString(), InitializerSection.StaticFilterNamesResources.SNotContains),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.ContainsWords.ToString(), InitializerSection.StaticFilterNamesResources.SContainsWords),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.NotContainsWords.ToString(), InitializerSection.StaticFilterNamesResources.SNotContainsWords),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.LengthMore.ToString(), InitializerSection.StaticFilterNamesResources.SLengthMore),
                            new KeyValuePair<object, object>(DefaultFilters.TextFilter.LengthLess.ToString(), InitializerSection.StaticFilterNamesResources.SLengthLess),
                        };
            }
        }

        public static void GenerateHtml(StringBuilder sb, IEnumerable<Filter> filters,
                                        Dictionary<string, string[]> filterValues, 
                                        ExtenderAjaxControl extenderAjaxControl)
        {
            if (filterValues == null)
                filterValues = new Dictionary<string, string[]>();
            foreach (Filter filter in filters.Where(f => f.Visible && (f.VisiblePermissions == null || UserRoles.IsInAnyRoles(f.VisiblePermissions))))
            {
                if (filter.Type == FilterType.Group)
                {
                    //если нет видимых дочерних элементов, то пропустить.
                    if (filter.Children.FirstOrDefault(p => p.Visible) == null) continue;
                    if (filter.Children.FirstOrDefault(p => p.MainGroup) != null)
                        sb.Append("<tr mainGroup=\"true\">");
                    else
                        sb.Append("<tr>");
                    sb.Append("<td colspan=\"4\">\r\n<DIV width=\"100%\"><FIELDSET><LEGEND>");
                    sb.Append(filter.Header);
                    sb.Append("</LEGEND><table width=\"100%\">");
                    GenerateHtml(sb, filter.Children, filterValues, extenderAjaxControl);
                    sb.Append("</table></FIELDSET></DIV>\r\n</td></tr>\r\n");
                    continue;
                }
                sb.Append("<tr ftype=\"");
                sb.Append(filter.Type);
                if (filter.MainGroup)
                    sb.Append("\" mainGroup=\"true");
                sb.Append("\">\r\n\t<td style=\"width:25%\">\r\n\t\t");
                sb.Append(filter.Header);
                sb.Append("\r\n\t</td>\r\n\t<td field=\"");
                sb.Append(filter.FilterName);
                sb.Append("\">\r\n\t\t");
                string filterValue = null;
                string value1 = null;
                string value2 = null;
                if (filterValues.ContainsKey(filter.FilterName))
                {
                    filterValue = filterValues[filter.FilterName][0];
                    if (filterValues[filter.FilterName].Length > 1)
                        value1 = filterValues[filter.FilterName][1];
                    if (filterValues[filter.FilterName].Length > 2)
                        value2 = filterValues[filter.FilterName][2];
                }
                switch (filter.Type)
                {
                    case FilterType.Boolean:
                        var booleans = booleanList.Union(
                            new[]
                                {
                                    new KeyValuePair<object, object>("Equals", filter.TrueBooleanText),
                                    new KeyValuePair<object, object>("NotEquals", filter.FalseBooleanText),
                                });
                        if (filter.Mandatory)
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeClassNameFieldsOfFilter(this)", 
                                                          booleans);
                        else
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeClassNameFieldsOfFilter(this)",
                                                          booleans.Union(notMadatoryList));
                        sb.Append("\r\n\t</td>\r\n\t<td colspan=\"2\" style=\"width:75%\"></td>\r\n");
                        break;
                    case FilterType.Text:
                    case FilterType.FullTextSearch:
                        if (filter.Mandatory)
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)", textList);
                        else
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)",
                                                          textList.Union(notMadatoryList));
                        sb.Append("\r\n\t</td>\r\n\t<td colspan=\"2\" style=\"width:75%\" >\r\n\t\t");
                        HtmlGenerator.AddTextBox(sb, filter.FilterName, value1, filter.MaxLength, filter.Columns,
                                                 filter.Width);
                        sb.Append("\r\n\t</td>\r\n\t<td colspan=\"2\" style=\"display:none;width:75%\"></td>\r\n");
                        break;
                    case FilterType.Reference:
                        if (filter.Mandatory)
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)", 
                                                          filter.FilterName.Contains(':') ? referenceListWithTextFilter : referenceList);
                        else
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)",
                                                          (filter.FilterName.Contains(':') ? referenceListWithTextFilter : referenceList).Union(notMadatoryList));
                        sb.Append("\r\n\t</td>\r\n\t<td colspan=\"2\" style=\"width:75%\">\r\n");
                        if (filter.Lookup)
                        {
                            if (string.IsNullOrEmpty(filter.AlternativeCellWidth))
                                HtmlGenerator.AddLookupTextBox(sb, filter.FilterName, value1, string.IsNullOrEmpty(value1) ? "" : value2, filter.TableName, filter.ProjectName,
                                                               filter.SelectMode, filter.BrowseFilterParameters, extenderAjaxControl, 
                                                               filter.MinimumPrefixLength, "100%", filter.IsMultipleSelect);
                            else
                                HtmlGenerator.AddLookupTextBox(sb, filter.FilterName, value1, string.IsNullOrEmpty(value1) ? "" : value2, filter.TableName, filter.ProjectName,
                                                               filter.SelectMode, filter.AlternativeCellWidth, "",
                                                               filter.BrowseFilterParameters, extenderAjaxControl,
                                                               filter.MinimumPrefixLength, "100%", filter.IsMultipleSelect);
                        }
                        else
                        {
                            if (filter.DataSource != null)
                            {
                                if (!string.IsNullOrEmpty(filter.SelectKeyValueColumn))
                                {
                                    HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, value1, filter.Width, filter.DataSource, filter.SelectKeyValueColumn);
                                }
                                else
                                {
                                    long? value = string.IsNullOrEmpty(value1) ? null : (long?)Convert.ToInt64(value1);
                                    HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, value, filter.Width, filter.DataSource);
                                }
                            }
                            else
                                HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, value1, filter.Width, filter.DataSourceOther);
                        }
                        sb.Append("\r\n\t</td>\r\n\t<td>");
                        HtmlGenerator.AddTextBox(sb, filter.FilterName, value2, filter.Width);
                        sb.Append("</td>\r\n\t<td colspan=\"2\" style=\"display:none;width:75%\"></td>\r\n");
                        break;
                    case FilterType.Numeric:
                        if (filter.Mandatory)
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)", numericList);
                        else
                            HtmlGenerator.AddDropDownList(sb, "ddl" + filter.FilterName, filterValue, "", "ChangeActiveFieldsOfFilter(this)",
                                                          numericList.Union(notMadatoryList));
                        sb.Append("\r\n\t</td>\r\n\t<td style=\"width:10%\" >\r\n\t\t");
                        HtmlGenerator.AddTextBox(sb, filter.FilterName + "__1", value1, filter.Width);
                        sb.Append("\r\n\t</td>\r\n\t<td style=\"width:60%\" >\r\n\t\t");
                        HtmlGenerator.AddTextBox(sb, filter.FilterName + "__2", value2, filter.Width);
                        sb.Append("\r\n\t</td>\r\n\t<td colspan=\"2\" style=\"display:none;width:75%\"></td>\r\n");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sb.Append("\r\n</tr>\r\n");
            }
        }

        public static string GenerateHtml(IEnumerable<Filter> filters,
                                Dictionary<string, List<FilterItem>> filterValues,
                                ExtenderAjaxControl extenderAjaxControl, string filterTable)
        {
            var textWriter = new StringWriter();
            var writer = new HtmlTextWriter(textWriter);
            try
            {
                GenerateHtml(writer, filters, filterValues, extenderAjaxControl, filterTable);
                return textWriter.ToString();
            }
            finally
            {
                writer.Close();
                textWriter.Dispose();
            }
        }

        private static void GenerateHtml(HtmlTextWriter writer, IEnumerable<Filter> filters,
                               Dictionary<string, List<FilterItem>> filterValues,
                               ExtenderAjaxControl extenderAjaxControl, string filterTable)
        {
            RenderColTags(writer);

            if (filterValues == null)
                filterValues = new Dictionary<string, List<FilterItem>>();
            var genFilters = filters.Where(f => f.Visible && (f.VisiblePermissions == null || UserRoles.IsInAnyRoles(f.VisiblePermissions)));
            foreach (Filter filter in genFilters)
            {
                if (filter.Type == FilterType.Group)
                {
                    //если нет видимых дочерних элементов, то пропустить.
                    if (filter.Children.FirstOrDefault(p => p.Visible) == null) continue;

                    #region TR
                    writer.WriteLine();
                    if (filter.Children.FirstOrDefault(p => p.MainGroup) != null)
                        writer.AddAttribute("mainGroup", "true");
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    #region TD
                    writer.WriteLine();
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "5");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    #region Div, Fieldset
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

                    var cliendID = "childRefsGroup" + id++;

                    #region legend
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "l_" + cliendID);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "Pointer");
                    writer.RenderBeginTag(HtmlTextWriterTag.Legend);
                    writer.Write(filter.Header);
                    writer.RenderEndTag();
                    #endregion

                    #region Div                    
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, cliendID);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");                    
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);                  

                    #region Table
                    writer.WriteLine();
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTable");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    GenerateHtml(writer, filter.Children, filterValues, extenderAjaxControl, filterTable);
                    writer.RenderEndTag();
                    #endregion

                    writer.RenderEndTag();
                    #endregion

                    HtmlGenerator.AddCollapsiblePanel(
                        extenderAjaxControl,
                        cliendID,
                        "l_" + cliendID,
                        "l_" + cliendID,
                        !filter.DefaultGroupExpanded);

                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    #endregion

                    writer.RenderEndTag();
                    #endregion

                    writer.RenderEndTag();
                    #endregion

                    continue;
                }

                var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                var currentFilterValues =
                    filterValues.ContainsKey(filter.FilterName)
                        ? filterValues[filter.FilterName]
                        : (filterValues.ContainsKey(filterName) ? filterValues[filterName] : null);
                var filterCount = currentFilterValues != null ? currentFilterValues.Count : 0;
                if (currentFilterValues != null && filterCount > 0)
                {
                    var index = 0;
                    foreach (var filterItem in currentFilterValues)
                    {
                        GenerateFilterItemHtml(filterItem, filter, writer, extenderAjaxControl, index, filterCount, filterTable);
                        index++;
                    }
                }
                else
                {
                    var filterItem = new FilterItem {FilterType = "Non"};
                    GenerateFilterItemHtml(filterItem, filter, writer, extenderAjaxControl, 0, 1, filterTable);
                }
            }
        }

        private static void RenderColTags(HtmlTextWriter writer)
        {
            RenderColTag(writer, "25%");
            RenderColTag(writer, "19%");
            RenderColTag(writer, "20%");
            RenderColTag(writer, "35%");
            RenderColTag(writer, "1%");
        }

        private static void RenderColTag(HtmlTextWriter writer, string value)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Width, value);
            writer.RenderBeginTag("col");
            writer.RenderEndTag();
        }

        private static void GenerateFilterItemHtml(FilterItem filterItem, Filter filter, HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl, int index, int filterCount, string filterTable)
        {
            writer.WriteLine();
            writer.AddAttribute("ftype", filter.Type.ToString());
            var defaultFType = filter.DefaultFilterType != null ? filter.DefaultFilterType.ToString() : null;
            writer.AddAttribute("defaultFType", defaultFType);
            if (filter.MainGroup)
                writer.AddAttribute("mainGroup", "true");

            writer.AddAttribute(HtmlTextWriterAttribute.Class, index == 0 ? "FilterTR" : "FilterTR FilterTRDublicate");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);


            #region TD of Header

            //if (index == 0)
            {
                writer.WriteLine();
                if (filterCount > 1 && index == 0)
                    writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, filterCount.ToString());
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTDHeader");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(filter.Header);
                if (filter.RequiredFilter)
                    writer.Write("*");
                writer.RenderEndTag();
            }

            #endregion

            #region TD FilterType
            writer.WriteLine();
            writer.AddAttribute("field", filter.FilterName);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTDType");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            IEnumerable<KeyValuePair<object, object>> filterTypeValues;
            switch (filter.Type)
            {
                case FilterType.Boolean:
                    filterTypeValues = booleanList.Union(
                        new[]
                            {
                                new KeyValuePair<object, object>("Equals", filter.TrueBooleanText),
                                new KeyValuePair<object, object>("NotEquals", filter.FalseBooleanText),
                            });
                    if (!string.IsNullOrEmpty(filterItem.Value1) && filterItem.Value1.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        filterItem.FilterType = filterItem.FilterType == "Equals" ? "NotEquals" : "Equals";
                        filterItem.Value1 = "true";
                    }
                    break;
                case FilterType.Text:
                    filterTypeValues = textList;
                    break;
                case FilterType.Reference:
                    if ("EqualsCollection".Equals(filterItem.FilterType, StringComparison.OrdinalIgnoreCase))
                        filterItem.FilterType = nameof(DefaultFilters.ReferenceFilter.Equals);
                    var baseFilterParameter = filter as BaseFilterParameter;
                    if (filter.FilterByStartsWithCode)
                    {
                        filterTypeValues = referenceListWithTextFilterForCode;
                    }
                    else if (LinqFilterGenerator.GetFilterName(filter.FilterName).Contains(':')
                        || (baseFilterParameter != null
                            && baseFilterParameter.OTextValueExpressionRu != null
                            && baseFilterParameter.OTextValueExpressionRu != baseFilterParameter.OValueExpression))
                    {
                        filterTypeValues = referenceListWithTextFilter;
                    }
                    else
                    {
                        filterTypeValues = referenceList;
                    }
                    break;
                case FilterType.Numeric:
                    if (filter.CustomFilterType is DefaultFilters.NumericFilter
                        && (DefaultFilters.NumericFilter)filter.CustomFilterType == DefaultFilters.NumericFilter.BetweenColumns)
                    {
                        //if (filterItem.FilterType != "Non")
                        //    filterItem.FilterType = DefaultFilters.NumericFilter.BetweenColumns.ToString();
                        filterTypeValues =
                            new[]
                                {
                                    new KeyValuePair<object, object>("Non", Resources.SSelectCondition),
                                    new KeyValuePair<object, object>("BetweenColumns", Resources.SFilterByPeriod),
                                };
                    }
                    else if (filter.IsDateTime)
                        filterTypeValues = datetimeList;
                    else
                        filterTypeValues = numericList;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var onchange = filter.Type == FilterType.Boolean
                               ? "ChangeClassNameFieldsOfFilter(this)"
                               : string.Format("ChangeActiveFieldsOfFilter(this, $get('filter{0}'))", filterTable);
            
            var filterType = filterItem.FilterType;
            if (filter.DefaultFilterType != null && (filterType == "Non" || string.IsNullOrEmpty(filterType)))
                filterType = filter.DefaultFilterType.ToString();

            if (filterItem.IsDisabled && filterType != "Non")
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            var width = InitializerSection.StaticFilterNamesResources.SelectFilterTypeWidth.ToString();
            filterTypeValues = filterTypeValues.Where(r => r.Value != null);
            if (!filter.Mandatory)
                filterTypeValues = filterTypeValues.Union(notMadatoryList);
            if (filter.AllowedFilterTypes != null)
                filterTypeValues = filterTypeValues.Where(r => filter.AllowedFilterTypes.Contains(r.Key));

            HtmlGenerator.AddDropDownList(
                writer,
                "ddlType" + filter.FilterName + "_" + index,
                filterType,
                width,
                onchange,
                filterTypeValues);
            
            writer.RenderEndTag();
            #endregion

            writer.WriteLine();
            switch (filter.Type)
            {
                case FilterType.Boolean:
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDNoneValue");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                    break;
                case FilterType.Text:
                case FilterType.FullTextSearch:
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDSingleValue");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    HtmlGenerator.AddTextBox(
                        writer,
                        filter.FilterName + "_" + index,
                        filterItem.Value1,
                        filter.MaxLength,
                        filter.Columns,
                        "100%");
                    writer.RenderEndTag();
                    break;
                case FilterType.Reference:
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDSingleValue");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.WriteLine();
                    if (filter.Lookup)
                    {
                        var lookup = (ILookup)filter;
                        lookup.Value = filterItem.Value1;
                        lookup.Text = string.IsNullOrEmpty(filterItem.Value1)
                                          ? ""
                                          : (string.IsNullOrEmpty(filterItem.Value2)
                                                 ? filter.GetNameByValue(filterItem.Value1)
                                                 : filterItem.Value2);
                        lookup.Width = new Unit("100%");
                        lookup.ClientID = filter.FilterName + "_" + index;
                        if (filterItem.IsDisabled)
                            lookup.IsMultipleSelect = false;

                        HtmlGenerator.RenderLookup(writer, lookup, extenderAjaxControl);

                        /*var sb = new StringBuilder();
                        if (string.IsNullOrEmpty(filter.AlternativeCellWidth))
                            HtmlGenerator.AddLookupTextBox(sb, filter.FilterName, filterItem.Value1,
                                                           string.IsNullOrEmpty(filterItem.Value1) ? "" : filterItem.Value2,
                                                           filter.TableName, filter.ProjectName,
                                                           filter.SelectMode, filter.BrowseFilterParameters,
                                                           extenderAjaxControl,
                                                           filter.MinimumPrefixLength, "100%",
                                                           filter.IsMultipleSelect);
                        else
                            HtmlGenerator.AddLookupTextBox(sb, filter.FilterName, filterItem.Value1,
                                                           string.IsNullOrEmpty(filterItem.Value1) ? "" : filterItem.Value2,
                                                           filter.TableName, filter.ProjectName,
                                                           filter.SelectMode, filter.AlternativeCellWidth, "",
                                                           filter.BrowseFilterParameters, extenderAjaxControl,
                                                           filter.MinimumPrefixLength, "100%",
                                                           filter.IsMultipleSelect);
                        writer.Write(sb);*/
                    }
                    else
                    {
                        if (filter.DataSource != null)
                        {
                            var ddl = new BaseDropDownList
                                {
                                    ID = "ddl" + filter.FilterName + "_" + index,
                                    Value = filterItem.Value1,
                                    DataSource = filter.DataSource,
                                    IDPropertyName = string.IsNullOrEmpty(filter.SelectKeyValueColumn) ? "id" : filter.SelectKeyValueColumn,
                                    NamePropertyName = "Name",
                                    Width = new Unit("100%"),
                                };
                            if (filter.DefaultFilterType != null)
                            {
                                ddl.AllowValueNotSet = true;
                                ddl.TextOfValueNotSet = string.Empty;
                            }

                            HtmlGenerator.RenderDropDownList(writer, ddl, extenderAjaxControl);
                        }
                        else
                            HtmlGenerator.AddDropDownList(
                                writer,
                                "ddl" + filter.FilterName + "_" + index,
                                filterItem.Value1,
                                "100%",
                                "",
                                filter.DataSourceOther);
                    }
                    writer.RenderEndTag();
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDSingleValue2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    HtmlGenerator.AddTextBox(writer, filter.FilterName, filterItem.Value2, 0, 0, "100%");
                    writer.RenderEndTag();
                    break;
                case FilterType.Numeric:
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDDoubleValue1");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    var clientId = filter.FilterName + "__1" + "_" + index;
                    HtmlGenerator.AddTextBox(writer, clientId, filterItem.Value1, 0, 0, "100%");
                    if (filter.IsDateTime)
                        AddDatePicker(extenderAjaxControl, clientId);
                    writer.RenderEndTag();

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDDoubleValue2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    clientId = filter.FilterName + "__2" + "_" + index;
                    HtmlGenerator.AddTextBox(writer, clientId, filterItem.Value2, 0, 0, "100%");
                    if (filter.IsDateTime)
                        AddDatePicker(extenderAjaxControl, clientId);
                    writer.RenderEndTag();
                    
                    // третье поля используется для указания количества дней.
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDSingleValue2");
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    clientId = filter.FilterName + "__3" + "_" + index;
                    HtmlGenerator.AddTextBox(writer, clientId, filterItem.Value1, 0, 0, "100%");
                    writer.RenderEndTag();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (filter.Type != FilterType.Boolean)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTD FilterTDNoneValue");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
            }

            #region TD Link For Add new Filter
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "FilterTDAddFilter");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (filter.AllowAddFilter && !filterItem.IsDisabled)
            {
                #region link

                writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(666);");
                if (index == 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                                        string.Format("javascript:AddFilter(this, $get('filter{0}'));return false;",
                                                      filterTable));
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SAddFilter);
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:RemoveFilter(this);return false;");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SRemoveFilter);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.A);

                #region image

                if (index == 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlAdd);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SAddFilter);
                    writer.AddAttribute("altRemove", Resources.SRemoveFilter);
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlRemove);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SRemoveFilter);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();

                #endregion

                writer.RenderEndTag();

                #endregion
            }

            writer.RenderEndTag();
            #endregion

            writer.RenderEndTag();
        }

        private static void AddDatePicker(ExtenderAjaxControl extenderAjaxControl, string clientId)
        {
            var maskedEditExtender = new MaskedEditExtender
                {
                    ID = clientId + "_mask",
                };
            var calendarExtender = new CalendarExtender
                {
                    ID = clientId + "_calendar",
                };
            DatePicker.InitializeExtenders(maskedEditExtender, calendarExtender, string.Empty, DatePickerMode.DateTime, string.Empty);
            extenderAjaxControl.AddExtender(calendarExtender, clientId);
            extenderAjaxControl.AddExtender(maskedEditExtender, clientId);
        }

        #region Nested type: Filter

        public class Filter : ILookup
        {
            public Filter()
            {
                Visible = true;
                MainGroup = true;
                AllowAddFilter = true;
                IsMultipleSelect = true;
                Enabled = true;
                Children = new List<Filter>();
            }

            /// <summary>
            /// Поле фильтрации.
            /// </summary>
            public string FilterName { get; set; }

            /// <summary>
            /// Для FilterType.Reference, определяет что поле лукапное (для выбора из диалогового окна).
            /// </summary>
            public bool Lookup { get; set; }

            /// <summary>
            /// Фильтр по типу DateTime
            /// </summary>
            public bool IsDateTime { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, определяет какой таблицы открыть журнал.
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, определяет заголовок таблицы открыть журнал.
            /// </summary>
            public string TableHeader
            {
                get => _tableHeader ?? Header;
                set => _tableHeader = value;
            }

            /// <summary>
            /// Для FilterType.Reference и !Lookup, данные для выбора в выподающем списке.
            /// </summary>
            public IDataSource DataSource { get; set; }

            /// <summary>
            /// Для FilterType.Reference и !Lookup, данные для выбора в выподающем списке.
            /// Если не указан DataSource используется DataSourceOther.
            /// </summary>
            public IEnumerable<KeyValuePair<object, object>> DataSourceOther { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, настраевает мод выбора из справочника.
            /// </summary>
            public string SelectMode { get; set; }

            public string ViewMode { get; set; }

            /// <summary>
            /// Заголовок группы или поля.
            /// </summary>
            public string Header { get; set; }

            /// <summary>
            /// Находится ли фильтр в основной группе полей. 
            /// Если true, то показывается во основном списке фильтров, иначе показывается при раскрытии полного списка фильтров.
            /// </summary>
            public bool MainGroup { get; set; }

            /// <summary>
            /// Настройка видимости.
            /// </summary>
            public bool Visible { get; set; }

            /// <summary>
            /// Список прав для видимости колонки.
            /// </summary>
            public string[] VisiblePermissions { get; set; }

            /// <summary>
            /// Являеться ли поле обязательным для заполнения, т.е. если нет то в списке фильтров будут дополнительные виды фильтрации.
            /// </summary>
            public bool Mandatory { get; set; }

            /// <summary>
            /// Фильтр по строковому полю, по начинается с. Предполагается поле код в древовидных таблицах, где выбрав родителя доступны все дочки.
            /// </summary>
            public bool FilterByStartsWithCode { get; set; }

            /// <summary>
            /// Для FilterType.Text и FilterType.Numeric, настраевает максимальное количество введённых символов.
            /// </summary>
            public int MaxLength { get; set; }

            /// <summary>
            /// Максимальное значение для даты или числа.
            /// </summary>
            public object MaxValue { get; set; }

            /// <summary>
            /// Для FilterType.Numeric, настраевает  максимальное количество символов на дробную часть
            /// </summary>
            public int DecimalPrecision { get; set; }

            /// <summary>
            /// Для FilterType.Text и FilterType.Numeric, настраевает ширину контрола в символах.
            /// </summary>
            public int Columns { get; set; }

            /// <summary>
            /// Для FilterType.Text и FilterType.Numeric, настраевает ширину контрола.
            /// </summary>
            public string Width { get; set; }

            Unit IRenderComponent.Width
            {
                get { return new Unit(Width); }
            }

            public Unit Height { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, настраевает ширину доп. поля.
            /// </summary>
            public string AlternativeCellWidth { get; set; }

            /// <summary>
            /// Тип фильтра.
            /// </summary>
            public FilterType Type { get; set; }

            /// <summary>
            /// Тип фильтра.
            /// </summary>
            public bool DefaultGroupExpanded { get; set; }

            public Enum CustomFilterType { get; set; }

            /// <summary>
            /// Для FilterType.Group, список дочерних полей группы.
            /// </summary>
            public IList<Filter> Children { get; private set; }

            /// <summary>
            /// Для FilterType.Boolean. Текст значения true.
            /// </summary>
            public string TrueBooleanText { get; set; }

            /// <summary>
            /// Для FilterType.Boolean. Текст значения false.
            /// </summary>
            public string FalseBooleanText { get; set; }

            /// <summary>
            /// Кастомная фильтрация
            /// </summary>
            public FilterHandler FilterHandler { get; set; }

            /// <summary>
            /// Кастомная фильтрация
            /// </summary>
            public ExpressionFilterHandler ExpressionFilterHandler { get; set; }
            public ExpressionFilterHandlerV2 ExpressionFilterHandlerV2 { get; set; }

            /// <summary>
            /// Фильтрация журнала по параметрам. Для FilterType.Reference и Lookup
            /// </summary>
            public BrowseFilterParameters BrowseFilterParameters { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, настраевает минисальное количество символов для автокамплита.
            /// </summary>
            public int MinimumPrefixLength { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, включает возможность выбора нескольких записей для фильтрации.
            /// </summary>
            public bool IsMultipleSelect { get; set; }

            public string OnChangedValue { get; set; }
            public string[] DependedFilters { get; set; }
            public string DependedHandler { get; set; }

            public bool Enabled { get; set; }

            /// <summary>
            /// Не разрешается добавлять пользователем фильтр
            /// </summary>
            public bool AllowAddFilter { get; set; }

            /// <summary>
            /// Для FilterType.Reference и Lookup, указывает на наименование проекта.
            /// </summary>
            public string ProjectName { get; set; }
            
            /// <summary>
            /// Фильтр является обязательным при генерации страницы к названию ставиться символ "*"
            /// </summary>
            public bool RequiredFilter { get; set; }

            /// <summary>
            /// Изменить возврат ключа записи. Используется при выборе записи из журнала для фильтрации, приемущественно по коду.
            /// </summary>
            public string SelectKeyValueColumn { get; set; }

            public SelectColumnParameters SelectInfo { get; set; }

            public object ComboBoxView { get; set; }

            public Enum ParseEnum(string value)
            {
                switch (Type)
                {
                    case FilterType.Reference:
                        if ("EqualsCollection".Equals(value, StringComparison.OrdinalIgnoreCase))
                            return DefaultFilters.ReferenceFilter.Equals;
                        return (Enum)Enum.Parse(typeof (DefaultFilters.ReferenceFilter), value);
                    case FilterType.Numeric:
                        return (Enum)Enum.Parse(typeof(DefaultFilters.NumericFilter), value);
                    case FilterType.Boolean:
                        return (Enum)Enum.Parse(typeof(DefaultFilters.BooleanFilter), value);
                    case FilterType.Text:
                        return (Enum)Enum.Parse(typeof(DefaultFilters.TextFilter), value);
                    case FilterType.FullTextSearch:
                        return (Enum)Enum.Parse(typeof(DefaultFilters.FullTextSearchFilter), value);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public Enum DefaultFilterType { get; set; }

            public string[] AllowedFilterTypes { get; set; }

            public IEnumerable<Filter> AllChildren
            {
                get
                {
                    if (Children == null)
                        return new Filter[0];
                    return Children.Union(Children.SelectMany(r => r.AllChildren));
                }
            }

            public Func<string, string> GetNameByValueHandler;

            public IEnumerable<string> GetNamesByValue(string value)
            {
                if (DataSource != null)
                {
                    if (GetNameByValueHandler != null)
                        return value.Split(',').Select(r => GetNameByValueHandler(r));
                        
                    var viewLong = DataSource.GetView("default") as BaseDataSourceView<long>;
                    if (viewLong != null)
                        return value.Split(',').Select(viewLong.GetName);

                    var view = DataSource.GetView("default") as IDataSourceViewGetName;
                    if (view != null) return new[] { view.GetName(value) };
                }
                else if (DataSourceOther != null)
                {
                    var found = this.DataSourceOther.FirstOrDefault(r => r.Key.Equals(Convert.ChangeType(value, r.Key.GetType())));
                    return new[] {found.Value == null ? null : found.Value.ToString()};
                }
                else if (Lookup || !string.IsNullOrEmpty(DataSourceViewTypeName))
                {
                    var dataSourceView = GetDataSourceView();
                    if (dataSourceView != null)
                    {
                        var viewLong = dataSourceView as BaseDataSourceView<long>;
                        if (viewLong != null)
                            return value.Split(',').Select(viewLong.GetName);

                        var view = dataSourceView as IDataSourceViewGetName;
                        if (view != null) return new[] { view.GetName(value) };
                    }
                }
                return null;
            }

            private string _dataSourceViewTypeName;
            private string _tableHeader;

            public string DataSourceViewTypeName
            {
                get
                {
                    if (!string.IsNullOrEmpty(_dataSourceViewTypeName))
                        return _dataSourceViewTypeName;

                    if (!string.IsNullOrEmpty(ProjectName) && !string.IsNullOrEmpty(TableName))
                        return ProjectName + "." + TableName + "JournalDataSourceView";
                    
                    return null;
                }

                set { _dataSourceViewTypeName = value; }
            }

            public DataSourceView GetDataSourceView()
            {
                var type = BuildManager.GetType(DataSourceViewTypeName, false, true);
                return type == null ? null : (DataSourceView)Activator.CreateInstance(type);
            }

            public string GetNameByValue(string value)
            {
                if (DataSource != null)
                {
                    var view = DataSource.GetView("default") as IDataSourceViewGetName;
                    if (view != null) return view.GetName(value);
                }
                else if(DataSourceOther != null)
                {
                    var found = DataSourceOther.
                        Where(r => r.Key.Equals(Convert.ChangeType(value, r.Key.GetType()))).
                        FirstOrDefault();
                    return found.Value == null ? null : found.Value.ToString();
                }
                else if(Lookup)
                {
                    var type = BuildManager.GetType(ProjectName + "." + TableName + "JournalDataSourceView", false, true);
                    if (type != null)
                    {
                        var view = Activator.CreateInstance(type) as IDataSourceViewGetName;
                        if (view != null) return view.GetName(value);
                    }
                }
                return null;
            }

            public IEnumerable<string> GetValuesString(FilterItem filterItem)
            {
                var value1 = filterItem.Value1;
                var value2 = filterItem.Value2;
                switch (Type)
                {
                    case FilterType.Reference:
                        
                        DefaultFilters.ReferenceFilter referenceType;
                        if ("EqualsCollection".Equals(filterItem.FilterType, StringComparison.OrdinalIgnoreCase))
                            referenceType = DefaultFilters.ReferenceFilter.Equals;
                        else
                            referenceType = (DefaultFilters.ReferenceFilter)Enum.Parse(typeof(DefaultFilters.ReferenceFilter), filterItem.FilterType);
                        
                        List<string> values;
                        switch (referenceType)
                        {
                            case DefaultFilters.ReferenceFilter.Non:
                                break;
                            case DefaultFilters.ReferenceFilter.Equals:
                                if (string.IsNullOrEmpty(value1)) break;                                
                                values = GetNamesByValue(value1).ToList();
                                if (values.Count == 0)
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SEquals.ToFilterTypeString();
                                    yield return value2;
                                }
                                else
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SEquals.ToFilterTypeString();
                                    foreach (var value in values) yield return value;
                                }

                                break;
                            case DefaultFilters.ReferenceFilter.NotEquals:
                                if (string.IsNullOrEmpty(value1)) break;
                                values = GetNamesByValue(value1).ToList();
                                if (values.Count == 0)
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SNotEquals.ToFilterTypeString();
                                    yield return value2;
                                }
                                else
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SNotEquals.ToFilterTypeString();
                                    foreach (var value in values) yield return value;
                                }

                                break;
                            case DefaultFilters.ReferenceFilter.IsNotNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.ReferenceFilter.IsNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsNotFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.ReferenceFilter.ContainsByRef:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContains.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.ReferenceFilter.StartsWithByRef:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SStartsWith.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.ReferenceFilter.EndsWithByRef:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SEndsWith.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.ReferenceFilter.ContainsWordsByRef:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContainsWords.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.ReferenceFilter.ContainsAnyWordByRef:
                                if (string.IsNullOrEmpty(filterItem.Value2)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContainsAnyWord.ToFilterTypeString() + filterItem.Value2;
                                break;
                            case DefaultFilters.ReferenceFilter.StartsWithCode:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                values = GetNamesByValue(filterItem.Value1).ToList();
                                if (values.Count > 0)
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SEquals.ToFilterTypeString();
                                    foreach (var value in values) yield return value;
                                }

                                break;
                            case DefaultFilters.ReferenceFilter.NotStartsWithCode:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                values = GetNamesByValue(filterItem.Value1).ToList();
                                if (values.Count > 0)
                                {
                                    yield return InitializerSection.StaticFilterNamesResources.SNotEquals.ToFilterTypeString();
                                    foreach (var value in values) yield return value;
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Numeric:
                        var numericType = (DefaultFilters.NumericFilter)Enum.Parse(typeof(DefaultFilters.NumericFilter), filterItem.FilterType);
                        DateTime date;
                        if (IsDateTime && !string.IsNullOrEmpty(value1) && DateTime.TryParse(value1, out date) 
                            && date.Hour == 0 
                            && date.Minute == 0
                            && date.Second == 0)
                        {
                            value1 = date.ToShortDateString();
                        }

                        if (IsDateTime && !string.IsNullOrEmpty(value2) && DateTime.TryParse(value2, out date) 
                            && date.Hour == 0 
                            && date.Minute == 0
                            && date.Second == 0)
                        {
                            value2 = date.ToShortDateString();
                        }

                        switch (numericType)
                        {
                            case DefaultFilters.NumericFilter.Non:
                                break;
                            case DefaultFilters.NumericFilter.Equals:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SEquals.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.NotEquals:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SNotEquals.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.IsNotNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.NumericFilter.IsNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsNotFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.NumericFilter.More:
                                if (string.IsNullOrEmpty(value1)) break;
                                if (IsDateTime)
                                    yield return InitializerSection.StaticFilterNamesResources.SDateMore.ToFilterTypeString() + value1;
                                else
                                    yield return InitializerSection.StaticFilterNamesResources.SMore.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.Less:
                                if (string.IsNullOrEmpty(value1)) break;
                                if (IsDateTime)
                                    yield return InitializerSection.StaticFilterNamesResources.SDateLess.ToFilterTypeString() + value1;
                                else
                                    yield return InitializerSection.StaticFilterNamesResources.SLess.ToFilterTypeString() + value1;
                                yield return InitializerSection.StaticFilterNamesResources.SMore.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.NumericFilter.LessOrEqual:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SLess.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.NumericFilter.MoreOrEqual:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SMore.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.NumericFilter.Between:
                                if (string.IsNullOrEmpty(value1)) break;
                                if (IsDateTime)
                                    yield return InitializerSection.StaticFilterNamesResources.SDateBetween.ToFilterTypeString() + value1 + " - " + value2;
                                else
                                    yield return InitializerSection.StaticFilterNamesResources.SBetween.ToFilterTypeString() + value1 + " - " + value2;
                                break;
                            case DefaultFilters.NumericFilter.BetweenColumns:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SFilterByPeriod.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.DaysAgoAndMore:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SDaysAgoAndMore.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.DaysLeftAndMore:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SDaysLeftAndMore.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.NumericFilter.ToDay:
                                yield return InitializerSection.StaticFilterNamesResources.SToDay.ToFilterTypeStringBit();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Boolean:
                        var boolType = (DefaultFilters.BooleanFilter)
                                       Enum.Parse(typeof (DefaultFilters.BooleanFilter), filterItem.FilterType);
                        switch (boolType)
                        {
                            case DefaultFilters.BooleanFilter.Non:
                                break;
                            case DefaultFilters.BooleanFilter.Equals:
                                yield return TrueBooleanText.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.BooleanFilter.NotEquals:
                                yield return FalseBooleanText.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.BooleanFilter.IsNotNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.BooleanFilter.IsNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsNotFilled.ToFilterTypeStringBit();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Text:
                        var textType = (DefaultFilters.TextFilter)Enum.Parse(typeof(DefaultFilters.TextFilter), filterItem.FilterType);
                        switch (textType)
                        {
                            case DefaultFilters.TextFilter.Non:
                                break;
                            case DefaultFilters.TextFilter.Equals:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SEquals.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.NotEquals:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SNotEquals.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.IsNotNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.TextFilter.IsNull:
                                yield return InitializerSection.StaticFilterNamesResources.SIsNotFilled.ToFilterTypeStringBit();
                                break;
                            case DefaultFilters.TextFilter.Contains:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContains.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.StartsWith:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SStartsWith.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.EndsWith:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SEndsWith.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.ContainsWords:
                                if (string.IsNullOrEmpty(value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContainsWords.ToFilterTypeString() + value1;
                                break;
                            case DefaultFilters.TextFilter.ContainsAnyWord:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContainsAnyWord.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.TextFilter.NotContains:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SNotContains.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.TextFilter.NotContainsWords:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SNotContainsWords.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.TextFilter.LengthMore:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SLengthMore.ToFilterTypeString() + filterItem.Value1;
                                break;
                            case DefaultFilters.TextFilter.LengthLess:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SLengthLess.ToFilterTypeString() + filterItem.Value1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.FullTextSearch:
                        var fullTextType = (DefaultFilters.FullTextSearchFilter)Enum.Parse(typeof(DefaultFilters.FullTextSearchFilter), filterItem.FilterType);
                        switch (fullTextType)
                        {
                            case DefaultFilters.FullTextSearchFilter.Contains:
                                if (string.IsNullOrEmpty(filterItem.Value1)) break;
                                yield return InitializerSection.StaticFilterNamesResources.SContains.ToFilterTypeString() + filterItem.Value1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public IEnumerable<string> GetListValuesString(FilterItem filterItem)
            {
                switch (Type)
                {
                    case FilterType.Reference:
                        var referenceType = (DefaultFilters.ReferenceFilter)Enum.Parse(typeof(DefaultFilters.ReferenceFilter), filterItem.FilterType);
                        List<string> values;
                        switch (referenceType)
                        {
                            case DefaultFilters.ReferenceFilter.Non:
                                break;
                            case DefaultFilters.ReferenceFilter.Equals:
                            case DefaultFilters.ReferenceFilter.NotEquals:
                                values = GetNamesByValue(filterItem.Value1).ToList();
                                if (values == null || values.Count == 0)
                                {
                                    yield return filterItem.Value2;
                                }
                                else
                                {
                                    foreach (var value in values)
                                        yield return value;
                                }

                                break;
                            case DefaultFilters.ReferenceFilter.IsNotNull:
                                yield return Resources.SIsFilled.ToLower();
                                break;
                            case DefaultFilters.ReferenceFilter.IsNull:
                                yield return Resources.SIsNotFilled.ToLower();
                                break;
                            case DefaultFilters.ReferenceFilter.ContainsByRef:
                            case DefaultFilters.ReferenceFilter.StartsWithByRef:
                            case DefaultFilters.ReferenceFilter.EndsWithByRef:
                            case DefaultFilters.ReferenceFilter.ContainsWordsByRef:
                            case DefaultFilters.ReferenceFilter.ContainsAnyWordByRef:
                                yield return filterItem.Value1;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Numeric:
                        var numericType = (DefaultFilters.NumericFilter)Enum.Parse(typeof(DefaultFilters.NumericFilter), filterItem.FilterType);
                        switch (numericType)
                        {
                            case DefaultFilters.NumericFilter.Non:
                                break;
                            case DefaultFilters.NumericFilter.Equals:
                            case DefaultFilters.NumericFilter.NotEquals:
                            case DefaultFilters.NumericFilter.More:
                            case DefaultFilters.NumericFilter.Less:
                            case DefaultFilters.NumericFilter.BetweenColumns:
                                yield return filterItem.Value1;
                                break;
                            case DefaultFilters.NumericFilter.IsNotNull:
                                yield return Resources.SIsFilled.ToLower();
                                break;
                            case DefaultFilters.NumericFilter.IsNull:
                                yield return Resources.SIsNotFilled.ToLower();
                                break;
                            case DefaultFilters.NumericFilter.Between:
                                yield return filterItem.Value1;
                                yield return filterItem.Value2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Boolean:
                        var boolType = (DefaultFilters.BooleanFilter)
                                       Enum.Parse(typeof(DefaultFilters.BooleanFilter), filterItem.FilterType);
                        switch (boolType)
                        {
                            case DefaultFilters.BooleanFilter.Non:
                                break;
                            case DefaultFilters.BooleanFilter.Equals:
                                yield return TrueBooleanText.ToLower();
                                break;
                            case DefaultFilters.BooleanFilter.NotEquals:
                                yield return FalseBooleanText.ToLower();
                                break;
                            case DefaultFilters.BooleanFilter.IsNotNull:
                                yield return Resources.SIsFilled.ToLower();
                                break;
                            case DefaultFilters.BooleanFilter.IsNull:
                                yield return Resources.SIsNotFilled.ToLower();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.Text:
                        var textType = (DefaultFilters.TextFilter)Enum.Parse(typeof(DefaultFilters.TextFilter), filterItem.FilterType);
                        switch (textType)
                        {
                            case DefaultFilters.TextFilter.Non:
                                break;
                            case DefaultFilters.TextFilter.Equals:
                            case DefaultFilters.TextFilter.NotEquals:
                            case DefaultFilters.TextFilter.Contains:
                            case DefaultFilters.TextFilter.StartsWith:
                            case DefaultFilters.TextFilter.EndsWith:
                            case DefaultFilters.TextFilter.ContainsWords:
                                yield return filterItem.Value1;
                                break;
                            case DefaultFilters.TextFilter.IsNotNull:
                                yield return Resources.SIsFilled.ToLower();
                                break;
                            case DefaultFilters.TextFilter.IsNull:
                                yield return Resources.SIsNotFilled.ToLower();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case FilterType.FullTextSearch:
                        var fullTextType = (DefaultFilters.FullTextSearchFilter)Enum.Parse(typeof(DefaultFilters.FullTextSearchFilter), filterItem.FilterType);
                        switch (fullTextType)
                        {
                            case DefaultFilters.FullTextSearchFilter.Contains:
                                yield return filterItem.Value1;
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <summary>
            /// Задать зависимость для фильтров. Т.е. доступность текущего фильтра определяется обработчиком, где values будут значения.
            /// </summary>
            /// <param name="handler">Обработчик, например: return values[0] != null; или просто имя функции GetVisibleFilter1</param>
            /// <param name="filters">Фильтры, значения которых должно прийти.</param>
            public void DependendVisible(string handler, params Filter[] filters)
            {
                foreach (var filter in filters)
                {
                    filter.OnChangedValue +=
                        $" if (window.filterRepository && window.filterRepository.ToggleVisibleDependedControl) window.filterRepository.ToggleVisibleDependedControl(window.filterRepository.FindControlByFilterName('{FilterName}')); ";
                }

                DependedFilters = filters.Select(r => r.FilterName).ToArray();
                DependedHandler = handler;
            }

            /// <summary>
            /// Задать зависимость для фильтров. Т.е. доступность текущего фильтра определяется обработчиком, где values будут значения.
            /// </summary>
            /// <param name="handler">Обработчик, например: return values[0] != null; или просто имя функции GetVisibleFilter1</param>
            /// <param name="filtersList">Список фильтров на форме.</param>
            /// <param name="filters">Фильтры, значения которых должно прийти.</param>
            public void DependendVisible(string handler, IList<Filter> filtersList, params string[] filters)
            {
                var key = "Filter.DependendVisible." + filtersList.GetHashCode();
                var filterDic = (Dictionary<string, Filter>)HttpContext.Current?.Items[key];
                if (filterDic == null)
                {
                    filterDic = filtersList.Union(filtersList.SelectMany(r => r.AllChildren)).ToDictionary(r => r.FilterName);
                    if (HttpContext.Current != null)
                        HttpContext.Current.Items[key] = filterDic;
                }

                var searchedFiltes = filters.Select(r => filterDic[r]).ToArray();
                DependendVisible(handler, searchedFiltes);
            }

            /// <summary>
            /// Задать зависимость для фильтров. Т.е. доступность текущего фильтра определяется обработчиком, где values будут значения.
            /// </summary>
            /// <param name="handler">Обработчик, например: return values[0] != null; или просто имя функции GetVisibleFilter1</param>
            /// <param name="filtersList">Список фильтров на форме.</param>
            /// <param name="filters">Фильтры, значения которых должно прийти.</param>
            public static void DependendVisible(IList<Filter> filtersList, string filter, string handler,  params string[] filters)
            {
                var key = "Filter.DependendVisible." + filtersList.GetHashCode();
                var filterDic = (Dictionary<string, Filter>)HttpContext.Current?.Items[key];
                if (filterDic == null)
                {
                    filterDic = filtersList.Union(filtersList.SelectMany(r => r.AllChildren)).ToDictionary(r => r.FilterName);
                    if (HttpContext.Current != null)
                        HttpContext.Current.Items[key] = filterDic;
                }

                var searchedFiltes = filters.Select(r => filterDic[r]).ToArray();
                filterDic[filter].DependendVisible(handler, searchedFiltes);
            }

            #region ILookup Members

            string IRenderComponent.UniqueID { get; set; }
            string IRenderComponent.ClientID { get; set; }
            object IRenderComponent.Value { get; set; }
            string IRenderComponent.Text { get; set; }
            object ILookup.AlternativeColumnValue { get; set; }
            string ILookup.AlternateText { get; set; }
            string IRenderComponent.ValidationGroup { get; set; }
            Unit ILookup.Width
            {
                get { return new Unit(Width); }
                set { Width = value.ToString(); }
            }

            bool IRenderComponent.ValidateValue(string value, RenderContext renderContext)
            {
                throw new NotImplementedException();                
            }

            void IRenderComponent.Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
            {
                throw new NotImplementedException();
            }

            void IRenderComponent.InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
            {
                throw new NotImplementedException();
            }

            void IRenderComponent.AddValidators(IEnumerable<ValidatorProperties> validators)
            {
                throw new NotImplementedException();
            }

            void IRenderComponent.AddValidator(ValidatorProperties validator)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}