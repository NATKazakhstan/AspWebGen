/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 но€бр€ 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Linq;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls
{
    public class SelectedParameterNavigator
    {
        private static readonly object _lock = new object();
        private static IDictionary<string, XDocument> documents = new Dictionary<string, XDocument>();

        public static string GenerateHtml(IEnumerable<ItemNavigator> itemNavigators)
        {
            var sb = new StringBuilder();

            bool isFirst = true;
            foreach (var item in itemNavigators.Where(p => p.Visible))
            {
                if (!isFirst) sb.Append("<br/>");
                sb.Append("<span class=\"font13\"><b>");
                sb.Append(HttpUtility.HtmlAttributeEncode(item.TableCaption));
                var s = item.ToString();
                if(!string.IsNullOrEmpty(s))
                {
                    sb.Append(":</b> ");
                    sb.Append(HttpUtility.HtmlAttributeEncode(s));
                }
                else
                    sb.Append("</b>");
                sb.Append("&nbsp;<b>&gt;</b>");
                if (!string.IsNullOrEmpty(item.EditUrl))
                {
                    sb.Append("&nbsp;<a href=\"");
                    sb.Append(item.EditUrl);
                    sb.Append("\">");
                    sb.Append(HttpUtility.HtmlAttributeEncode(Resources.SView));
                    sb.Append("</a>&nbsp;-");
                }
                sb.Append("&nbsp;<a href=\"");
                sb.Append(item.JournalUrl);
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlAttributeEncode(Resources.SToJournal));
                sb.Append("</a></span>");

//                sb.AppendFormat(
//                    "<span class=\"font13\"><b>{1}:</b> {0}&nbsp;<b>&gt;</b>&nbsp;<a href=\"{2}\">{4}</a>&nbsp;-&nbsp;<a href=\"{3}\">{5}</a></span>",
//                    item, item.TableCaption, item.EditUrl, item.JournalUrl, Resources.SView, Resources.SToJournal);
                isFirst = false;
            }
            if (!isFirst)
            {
                sb.Append("<hr />");
                return sb.ToString();
            }
            return "";
        }

        public static string GenerateHtml(IDictionary<Type, ItemNavigator> itemNavigators)
        {
            return GenerateHtml(itemNavigators.Values.OrderByDescending(p => p.OrderField));
        }

        public static IEnumerable<ItemNavigator> GetItems(MainPageUrlBuilder UrlBuilder, Type type, Type dataContextType)
        {
            return GetItemNavigators(UrlBuilder, type, dataContextType).Values.Where(p => p.ValueNullable.HasValue).OrderByDescending(p => p.OrderField).ToArray();
        }

        public static IDictionary<Type, ItemNavigator> GetItemNavigators(MainPageUrlBuilder UrlBuilder, Type type, Type dataContextType)
        {
            return GetItemNavigators(UrlBuilder, type, dataContextType, "");
        }

        public static IDictionary<Type, ItemNavigator> GetItemNavigators(MainPageUrlBuilder UrlBuilder, Type type, Type dataContextType, string projectName)
        {
            var navigators = new Dictionary<Type, ItemNavigator>();
            if (typeof(IRow).IsAssignableFrom(type))
                type = type.GetProperty("Item").PropertyType;
            AddParentNavigators(navigators, type, UrlBuilder, dataContextType, projectName);
            return navigators;
        }

        private static void AddParentNavigators(IDictionary<Type, ItemNavigator> navigators, Type type, MainPageUrlBuilder url, Type dataContextType, string projectName)
        {
            var doc = GetDocumnet(projectName);
            var db = (DataContext) Activator.CreateInstance(dataContextType);
            int order = 1;
            var element = doc.Element("data").Elements().FirstOrDefault(e => e.Attribute("TableType").Value == type.Name);
            if (element == null)
                throw new Exception("Ќе найдена таблица '" + type.Name + "' дл€ построени€ св€зи (DataInformation_*).");
            var parentReferences = element.Attribute("ParentReferences").Value;
            var isMain = Convert.ToBoolean(element.Attribute("IsMain").Value);
            if (!isMain && !string.IsNullOrEmpty(parentReferences))
            {
                foreach (var parent in parentReferences.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var property = type.GetProperty(parent);
                    if (property != null)
                    {
                        AddParentNavigators(navigators, doc, db, property.PropertyType, parent, url, ref order);
                        if (property.PropertyType == type)
                            navigators[type].Visible = /*url.ShowHistory &&*/ !string.IsNullOrEmpty(navigators[type].EditUrl);
                    }
                }
            }
        }

        private static void AddParentNavigators(IDictionary<Type, ItemNavigator> navigators, XContainer doc, DataContext db, Type type, string removeInKey, MainPageUrlBuilder url, ref int order)
        {
            ItemNavigator navigator;
            if (!navigators.ContainsKey(type))
            {
                var element = doc.Element("data").Elements().FirstOrDefault(e => e.Attribute("TableType").Value == type.Name);
                if (element == null)
                    throw new Exception("Ќе найдена таблица '" + type.Name + "' дл€ построени€ св€зи (DataInformation_*).");
                navigator = new ItemNavigator();
                navigators.Add(type, navigator);
                navigator.TableName = element.Attribute("TableName").Value;
                navigator.TableType = type;
                navigator.RecordCaption = element.Attribute("RecordCaption").Value;
                navigator.TableCaption = element.Attribute("Title").Value;
                navigator.ParentReferences = element.Attribute("ParentReferences").Value;
                navigator.IsMain = Convert.ToBoolean(element.Attribute("IsMain").Value);
                navigator.ColumnName = element.Attribute("ColumnName").Value;
                navigator.AlternativeColumnName = element.Attribute("AlternativeColumnName").Value;
                SetUrls(removeInKey, navigator, url);
                if (navigator.ValueNullable.HasValue)
                    SetText(db, type, navigator);
            }
            else
            {
                navigator = navigators[type];
                if (string.IsNullOrEmpty(navigator.EditUrl) && !navigator.IsFiltered)
                {
                    SetUrls(removeInKey, navigator, url);
                    if (navigator.ValueNullable.HasValue)
                        SetText(db, type, navigator);
                }
            }
            navigator.OrderField = order++;
            if (!navigator.IsMain && !string.IsNullOrEmpty(navigator.ParentReferences))
            {
                foreach (var parent in navigator.ParentReferences.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var property = type.GetProperty(parent);
                    if (property != null && property.PropertyType != type)
                    {
                        AddParentNavigators(navigators, doc, db, property.PropertyType,
                                            removeInKey == null ? parent : removeInKey + "." + parent,
                                            url, ref order);
                    }
                }
            }            
        }

        private static void SetUrls(string removeInKey, ItemNavigator item, MainPageUrlBuilder url)
        {
            var builder = new MainPageUrlBuilder
                              {
                                  UserControl = item.TableName + "Edit",
                                  IsDataControl = true,
                                  IsRead = true,
                                  Page = url.Page,
                                  ShowHistory = url.ShowHistory,
                              };
            bool needLookLink = false;
            if (!string.IsNullOrEmpty(removeInKey))
            {
                string thisFilter = removeInKey + ".id";
                removeInKey += ".";
                var index = removeInKey.Length;
                foreach (var pair in url.QueryParameters)
                {
                    if (thisFilter.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        long value;
                        if (long.TryParse(pair.Value, out value))
                        {
                            item.ValueNullable = value;
                            builder.QueryParameters["ref" + item.TableName] = pair.Value;
                            needLookLink = true;
                        }
                    }
                    else if (pair.Key.StartsWith(removeInKey))
                        builder.QueryParameters[pair.Key.Substring(index)] = pair.Value;
                    else if (pair.Key.Equals("ref" + item.TableName))
                    {
                        long value;
                        if (long.TryParse(pair.Value, out value))
                            item.ValueNullable = value;
                        if (!string.IsNullOrEmpty(pair.Value))
                        {
                            needLookLink = true;
                            builder.QueryParameters[pair.Key] = pair.Value;
                        }
                    }
                    else if (!pair.Key.Contains('.'))
                        builder.QueryParameters[pair.Key] = pair.Value;
                }
            }
            foreach (var parameter in url.ControlFilterParameters)
            {
                //todo: надо както убирать фильтры дочерних журналов, при возврате вверх по ссылкам
//                if (item.TableName == parameter.Key)
//                    break;
                builder.ControlFilterParameters.Add(parameter);
            }
            if (needLookLink)
                item.EditUrl = builder.CreateUrl();
            builder.UserControl = item.TableName + "Journal";
            builder.IsRead = false;
            item.IsFiltered = builder.QueryParameters.Count > 0;
            item.JournalUrl = builder.CreateUrl();
        }

        public static XContainer GetDocumnet(string projectName)
        {
            lock(_lock)
            {
                if(!documents.ContainsKey(projectName))
                {
                    string file;
                    if(projectName == "")
                        file = HttpContext.Current.Request.MapPath("~/DataInformation.xml");
                    else
                        file = HttpContext.Current.Request.MapPath("~/DataInformation_" + projectName + ".xml");
                    var document = XDocument.Load(file);
                    documents.Add(projectName, document);
                }
            }
            return documents[projectName];
        }

        private static void SetText(DataContext db, Type tableType, ItemNavigator itemNavigator)
        {
            ParameterExpression param = Expression.Parameter(tableType, "c");
            Expression right = Expression.Constant(itemNavigator.ValueNullable.Value);
            Expression left = Expression.Property(param, tableType.GetProperty("id"));
            Expression filter = Expression.Equal(left, right);
            Expression pred = Expression.Lambda(filter, param);

            ITable table = db.GetTable(tableType);
            Expression expr = Expression.Call(typeof(Queryable), "Where", new[] { tableType },
                                              Expression.Constant(table), pred);
            IQueryable query = table.AsQueryable().Provider.CreateQuery(expr);
            IEnumerator enumerator = query.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                itemNavigator.Text = "[запись не найдена]";
                return;
            }
            var obj = enumerator.Current;
            itemNavigator.Text = GetText(obj, itemNavigator.ColumnName);
            if (!string.IsNullOrEmpty(itemNavigator.AlternativeColumnName))
                itemNavigator.AlternativeText = GetText(obj, itemNavigator.AlternativeColumnName);
        }

        private static string GetText(object obj, string columnName)
        {
            foreach (var field in columnName.Split('.'))
            {
                PropertyInfo property = obj.GetType().GetProperty(field);
                if(property == null)
                    throw new Exception("Ќе существует колонка " + field + " в таблице " + obj.GetType().FullName);
                var newObj = property.GetValue(obj, null);
                if (newObj == null) throw new Exception("—войство " + field + " таблицы " + obj.GetType().FullName + " должно быть об€зательным дл€ заполнени€");
                obj = newObj;
            }
            return (obj ?? "[нет значени€]").ToString();
        }

        public class ItemNavigator
        {
            public ItemNavigator()
            {
                Visible = true;
            }
            public string EditUrl { get; set; }
            public string JournalUrl { get; set; }
            public long Value { get { return ValueNullable ?? 0; } }
            public long? ValueNullable { get; set; }
            public string Text { get; set; }
            public string TableName { get; set; }
            public string AlternativeText { get; set; }
            public string RecordCaption { get; set; }
            public string TableCaption { get; set; }
            public Type TableType { get; set; }
            public string ParentReferences { get; set; }
            public bool IsMain { get; set; }
            public bool Visible { get; set; }
            public int OrderField { get; set; }
            public bool IsFiltered { get; set; }
            internal string ColumnName { get; set; }
            internal string AlternativeColumnName { get; set; }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(RecordCaption))
                {
                    if (string.IsNullOrEmpty(AlternativeText))
                        return Text;
                    return string.Format("{0} ({1})", Text, AlternativeText);
                }
                if (string.IsNullOrEmpty(AlternativeText))
                    return string.Format("{0}: {1}", RecordCaption, Text);
                return string.Format("{0}: {1} ({2})", RecordCaption, Text, AlternativeText);
            }
        }
    }
}