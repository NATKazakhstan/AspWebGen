using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Microsoft.JScript;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.Filter;
using Convert = System.Convert;

namespace Nat.Web.Controls
{
    public class MainPageUrlBuilder : ICloneable
    {
        public const string CustomParameterPrefix = "__p__";

        public const string ReferencIDPrefix = "ref";
        public const string UserControlTypeEdit = "Edit";
        public const string UserControlTypeJournal = "Journal";
        public const string UserControlTypeFilter = "Filter";
        public const string QueryParameterManagerType = "ManagerType";
        public const string NavigateToDestinationParentTableName = "NavigateToDestinationParentTableName";

        private static readonly Regex _queryRegex = new Regex(@"(?<=\?|&)(?!&)(?<field>.+?)=(?<value>.*?)(?=&|$)",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _queryRegexBackUrl =
            new Regex(@"(?<=\?|&)(?<field>__backurl)=(?<value>.*?)(?=&|$)",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _regex =
            new Regex(
                @"/(?<page>\w+?)(\.aspx|\.asmx|\.ashx|\.\w+)?/(?<type>(data|download|execute|custom|navigateto)/)?(?<usercontrol>[\w\d_]+)?(?<param1>/[\w\d_]+)?(?<param2>/[\w\d_]+)?(?<param3>/[\w\d_]+)?(?<param4>/[\w\d_]+)?(?<param5>/[\w\d_]+)?(?<query>\?.*)?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private bool _isSelect;

        public MainPageUrlBuilder()
        {
            QueryParameters = new Dictionary<string, string>();
            ParameterConverter = new Dictionary<string, string>();
            ControlFilterParameters = new List<FilterParameter>();
            CustomQueryParameters = new CustomQueryParametersDic();
            LookHistoryValues = new Dictionary<string, string>();
            CurrentValues = new Dictionary<string, string>();
        }

        public MainPageUrlBuilder(string url) : this()
        {
            Url = url;
            ParseUrl(true);
        }

        public MainPageUrlBuilder(string url, bool throwException)
            : this()
        {
            Url = url;
            ParseUrl(throwException);
        }

        public string Url { get; private set; }
        public string UserControl { get; set; }
        public bool IsDataControl { get; set; }
        public bool NavigateTo { get; set; }
        public bool ShowHistory { get; set; }
        public bool ShowFilter { get; set; }
        public bool IsMultipleSelect { get; set; }
        public string Culture { get; set; }
        public string StorageValuesSessionKey { get; set; }
        public string ReportPluginName { get; set; }
        public bool TimeoutInSQL { get; set; }

        public bool IsSelect
        {
            get { return _isSelect; }
            set
            {
                _isSelect = value;
                if (!value) IsMultipleSelect = false;
            }
        }

        public string SelectForAddType { get; set; }

        public bool IsNew { get; set; }
        public bool IsRead { get; set; }
        public bool IsFilterWindow { get; set; }
        public bool IsCustomUserControl { get; set; }
        public string SelectMode { get; set; }
        public string ViewMode { get; set; }
        public bool IsDownload { get; set; }
        public bool IsExecute { get; set; }
        public IDictionary<string, string> QueryParameters { get; internal set; }

        /// <summary>
        /// Колекция параметров для использования в логике входов.
        /// </summary>
        public CustomQueryParametersDic CustomQueryParameters { get; private set; }

        public IDictionary<string, string> ParameterConverter { get; set; }
        public List<FilterParameter> ControlFilterParameters { get; set; }
        public SelectParameters SelectParameters { get; set; }
        public string QueryParametersPrefix { get; set; }
        public string Query { get; private set; }
        public string FilterQuery { get; set; }
        public string BackUrl { get; set; }
        public bool SetSelectedValues { get; set; }
        public string SelectedValues { get; set; }
        public bool CreateBackUrlByRequest { get; set; }
        public string Page { get; set; }
        public bool RemoveQueryParameterPrefix { get; set; }
        public DateTime? HistoryOnDate { get; set; }
        public string CustomFilterClassName { get; set; }
        public bool DeniedUseShortFilters { get; set; }

        /// <summary>
        /// Изменить возврат наименования записи. Используется при выборе записи из журнала.
        /// </summary>
        public string SelectNameColumn { get; set; }

        /// <summary>
        /// Изменить возврат ключа записи. Используется при выборе записи из журнала для фильтрации, приемущественно по коду.
        /// </summary>
        public string SelectKeyValueColumn { get; set; }

        /// <summary>
        /// Текущие выбранные записи, словарь значений по таблице. Только для четния.
        /// </summary>
        public Dictionary<string, string> CurrentValues { get; set; }

        /// <summary>
        /// Просмотр историчных записей, словарь значений по табицам. Только для четния.
        /// </summary>
        public Dictionary<string, string> LookHistoryValues { get; set; }

        private void ParseUrl(bool throwException)
        {
            Match match = _regex.Match(Url);
            UserControl = "";
            IsDataControl = false;
            NavigateTo = false;
            ShowHistory = false;
            IsSelect = false;
            ShowFilter = false;
            IsMultipleSelect = false;
            IsNew = false;
            IsRead = false;
            IsDownload = false;
            IsExecute = false;
            IsFilterWindow = false;
            IsCustomUserControl = false;
            QueryParameters.Clear();
            ParameterConverter.Clear();
            ControlFilterParameters.Clear();
            SelectParameters = null;
            BackUrl = "";
            Query = "";
            SelectedValues = "";
            FilterQuery = null;
            if (!match.Success)
                return;

            Page = match.Groups["page"].Value;
            if (match.Groups["type"].Success)
            {
                if (match.Groups["type"].Value.Equals("data/"))
                    IsDataControl = true;
                else if (match.Groups["type"].Value.Equals("navigateto/"))
                    NavigateTo = true;
                else if (match.Groups["type"].Value.Equals("download/"))
                    IsDownload = true;
                else if (match.Groups["type"].Value.Equals("execute/"))
                    IsExecute = true;
                else if (match.Groups["type"].Value.Equals("custom/"))
                    IsCustomUserControl = true;
            }

            if ((IsDataControl || NavigateTo) && !match.Groups["usercontrol"].Success)
            {
                if (throwException)
                    throw new Exception("Not set UserControls in url");
                IsDataControl = false;
                NavigateTo = false;
                return;
            }

            if (match.Groups["usercontrol"].Success)
                UserControl = HttpUtility.UrlDecode(match.Groups["usercontrol"].Value);

            var dic = new Dictionary<string, string>();
            if (IsCustomUserControl)
                UserControl += match.Groups["param1"].Success ? HttpUtility.UrlDecode(match.Groups["param1"].Value) : "";
            else
                AddParam(match, dic, "param1");

            AddParam(match, dic, "param2");
            AddParam(match, dic, "param3");
            AddParam(match, dic, "param4");
            AddParam(match, dic, "param5");

            ShowHistory = dic.ContainsKey("showhistory");
            IsMultipleSelect = dic.ContainsKey("multipleselect");
            IsSelect = dic.ContainsKey("select") || IsMultipleSelect;
            ShowFilter = dic.ContainsKey("showfilter");
            IsNew = dic.ContainsKey("new");
            IsRead = dic.ContainsKey("read");
            IsFilterWindow = dic.ContainsKey("filter");

            if (match.Groups["query"].Success)
            {
                Query = match.Groups["query"].Value;
                ParseQuery();
            }
        }

        private void ParseQuery()
        {
            if (!string.IsNullOrEmpty(Query))
            {
                Match match = _queryRegex.Match(Query);
                while (match.Success)
                {
                    if (match.Groups["field"].Success && match.Groups["value"].Success)
                    {
                        var fieldValue = HttpUtility.UrlDecode(match.Groups["value"].Value);
                        if (match.Groups["field"].Value == "mode")
                            SelectMode = fieldValue;
                        else if(match.Groups["field"].Value == "viewmode")
                            ViewMode = fieldValue;
                        else if (match.Groups["field"].Value == "culture")
                            Culture = fieldValue;
                        else if (match.Groups["field"].Value == "storageValues")
                            StorageValuesSessionKey = fieldValue;
                        else if (match.Groups["field"].Value == "reportPluginName")
                            ReportPluginName = fieldValue;
                        else if (match.Groups["field"].Value == "__backurl")
                            BackUrl = HttpUtility.UrlDecode(Query.Substring(match.Groups["value"].Index));
                        else if (match.Groups["field"].Value == "__filter")
                            FilterQuery = fieldValue;
                        else if (match.Groups["field"].Value == "__historyOnDate")
                            HistoryOnDate = System.Convert.ToDateTime(fieldValue);
                        else if (match.Groups["field"].Value == "__selected")
                            SelectedValues = fieldValue;
                        else if (match.Groups["field"].Value == "__customFCN")
                            CustomFilterClassName = fieldValue;
                        else if (match.Groups["field"].Value.Equals("__SNColumn", StringComparison.OrdinalIgnoreCase))
                            SelectNameColumn = fieldValue;
                        else if (match.Groups["field"].Value.Equals("__SKVColumn", StringComparison.OrdinalIgnoreCase))
                            SelectKeyValueColumn = fieldValue;
                        else if (match.Groups["field"].Value.Equals("__SFAddType", StringComparison.OrdinalIgnoreCase))
                            SelectForAddType = fieldValue;
                        else if (match.Groups["field"].Value.Equals("__deniedShortFilters", StringComparison.OrdinalIgnoreCase) && fieldValue == "on")
                            DeniedUseShortFilters = true;
                        else if (match.Groups["field"].Value.Equals("__timeoutInSQL", StringComparison.OrdinalIgnoreCase) && fieldValue == "on")
                            TimeoutInSQL = true;
                        else if (match.Groups["field"].Value == "__filters")
                        {
                            string component = fieldValue;
                            var jss = new JavaScriptSerializer();
                            if (string.IsNullOrEmpty(component))
                                ControlFilterParameters.Clear();
                            else
                                ControlFilterParameters = jss.Deserialize<List<FilterParameter>>(component);
                        }
                        else if (match.Groups["field"].Value == "__selParams")
                        {
                            string component = fieldValue;
                            var jss = new JavaScriptSerializer();
                            SelectParameters = jss.Deserialize<SelectParameters>(component);
                        }
                        else if (match.Groups["field"].Value.StartsWith(CustomParameterPrefix))
                        {
                            string key = match.Groups["field"].Value.Substring(CustomParameterPrefix.Length);
                            CustomQueryParameters[key] = new QueryParameter { Value = fieldValue };
                        }
                        else// if (!QueryParameters.ContainsKey(match.Groups["field"].Value))
                        {
                            QueryParameters[match.Groups["field"].Value] = fieldValue;
                            if (match.Groups["field"].Value.StartsWith("refHistory"))
                                LookHistoryValues[match.Groups["field"].Value] = fieldValue;
                            else if (match.Groups["field"].Value.StartsWith("ref"))
                                LookHistoryValues[match.Groups["field"].Value] = fieldValue;
                        }
                    }

                    match = match.NextMatch();
                }
            }
        }

        private static void AddParam(Match match, IDictionary<string, string> dic, string paramName)
        {
            if (!match.Groups[paramName].Success)
                return;

            var value = HttpUtility.UrlDecode(match.Groups[paramName].Value);
            if (!dic.ContainsKey(value))
                dic.Add(value.Substring(1).ToLower(), null);
        }

        public string CreateUrl(bool setFilterQuery)
        {
            return CreateUrl(setFilterQuery, false);
        }

        public string CreateUrl(bool setFilterQuery, bool addAllCustomParameters)
        {
            var sb = new StringBuilder();
            CreateUrl(sb, setFilterQuery, addAllCustomParameters, false);
            Url = sb.ToString();
            return Url;
        }

        public string CreateUrl(bool setFilterQuery, bool addAllCustomParameters, bool removeSession)
        {
            var sb = new StringBuilder();
            CreateUrl(sb, setFilterQuery, addAllCustomParameters, removeSession);
            Url = sb.ToString();
            return Url;
        }

        public string CreateUrl()
        {
            var sb = new StringBuilder();
            CreateUrl(sb);
            Url = sb.ToString();
            return Url;
        }

        public string CreateQueryParameters()
        {
            var sb = new StringBuilder();
            CreateUrl(sb);
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != '?') 
                    continue;
                sb.Remove(0, i + 1);
                break;
            }

            return sb.ToString();
        }

        public void CreateUrl(StringBuilder sb)
        {
            CreateUrl(sb, false);
        }

        public void CreateUrl(StringBuilder sb, bool setFilterQuery)
        {
            CreateUrl(sb, setFilterQuery, false, false);
        }

        public void CreateUrl(StringBuilder sb, bool setFilterQuery, bool addAllCustomParameters, bool removeSession)
        {
            sb.Append("/");
            sb.Append(Page ?? "MainPage");
            //sb.Append(".aspx");
            if (IsDataControl) sb.Append("/data");
            if (NavigateTo) sb.Append("/navigateto");
            if (IsDownload) sb.Append("/download");
            if (IsExecute) sb.Append("/execute");
            if (IsCustomUserControl) sb.Append("/custom");
            if (new[] { IsDataControl, IsDownload, IsExecute, NavigateTo }.Where(r => r).Count() > 1)
                throw new Exception("заданы взаимоисключающие параметры IsDataControl, IsDownload, IsExecute, NavigateTo");

            if (UserControl != null && !UserControl.Equals(""))
                sb.Append("/" + UserControl);
            if (ShowHistory) sb.Append("/showhistory");
            if (IsNew) sb.Append("/new");
            if (IsMultipleSelect) sb.Append("/multipleselect");
            if (IsSelect && !IsMultipleSelect) sb.Append("/select");
            if (ShowFilter) sb.Append("/showfilter");
            if (IsRead) sb.Append("/read");
            if (IsFilterWindow) sb.Append("/filter");
            bool hasQuery = false;
            if (QueryParameters.Count > 0)
            {
                sb.Append("?");
                hasQuery = true;
                foreach (var pair in QueryParameters)
                {
                    if (ParameterConverter.ContainsKey(pair.Key))
                        sb.Append(ParameterConverter[pair.Key]);
                    else if (!string.IsNullOrEmpty(QueryParametersPrefix))
                    {
                        if (RemoveQueryParameterPrefix)
                        {
                            if (pair.Key.StartsWith(QueryParametersPrefix))
                                sb.Append(pair.Key.Substring(QueryParametersPrefix.Length));
                            else
                                sb.Append(pair.Key);
                        }
                        else
                        {
                            if (!pair.Key.StartsWith("ref"))
                                sb.Append(QueryParametersPrefix);
                            sb.Append(pair.Key);
                        }
                    }
                    else
                        sb.Append(pair.Key);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(pair.Value));
                    sb.Append("&");
                }
                sb.Remove(sb.Length - 1, 1); //убираем симол "&"  конца строки
            }
            
                foreach (var pair in CustomQueryParameters)
                {
                    if (!pair.Value.UseInUrl && !addAllCustomParameters) continue;
                    if (!hasQuery)
                    {
                        sb.Append("?");
                        hasQuery = true;
                    }
                    else
                        sb.Append("&");
                    sb.Append(CustomParameterPrefix);
                    sb.Append(pair.Key);
                    sb.Append("=");
                    sb.Append(HttpUtility.UrlEncode(pair.Value.Value));
                }

            AddParameter(sb, "culture", Culture, ref hasQuery);
            AddParameter(sb, "reportPluginName", HttpUtility.UrlEncode(ReportPluginName), ref hasQuery);
            AddParameter(sb, "mode", HttpUtility.UrlEncode(SelectMode), ref hasQuery);
            AddParameter(sb, "viewmode", HttpUtility.UrlEncode(ViewMode), ref hasQuery);
            AddParameter(sb, "__historyOnDate", HistoryOnDate.ToString(), ref hasQuery);
            AddParameter(sb, "__SNColumn", HttpUtility.UrlEncode(SelectNameColumn), ref hasQuery);
            AddParameter(sb, "__SKVColumn", HttpUtility.UrlEncode(SelectKeyValueColumn), ref hasQuery);
            AddParameter(sb, "__SFAddType", HttpUtility.UrlEncode(SelectForAddType), ref hasQuery);
            AddParameter(sb, "__customFCN", HttpUtility.UrlEncode(CustomFilterClassName), ref hasQuery);
            if (DeniedUseShortFilters)
                AddParameter(sb, "__deniedShortFilters", "on", ref hasQuery);
            if (TimeoutInSQL)
                AddParameter(sb, "__timeoutInSQL", "on", ref hasQuery);
            
            if (SetSelectedValues)
                AddParameter(sb, "__selected", HttpUtility.UrlEncode(SelectedValues), ref hasQuery);
           
            if (FilterQuery != null && setFilterQuery)
            {
                if (hasQuery) sb.Append("&");
                else
                {
                    sb.Append("?");
                    hasQuery = true;
                }
                sb.AppendFormat("__filter={0}", HttpUtility.UrlEncode(FilterQuery));
            }
            if (ControlFilterParameters.Count > 0)
            {
                if (hasQuery) sb.Append("&");
                else
                {
                    sb.Append("?");
                    hasQuery = true;
                }
                List<FilterParameter> filters;
                if (removeSession)
                {
                    filters = new List<FilterParameter>(ControlFilterParameters.Count);
                    foreach (var filterParameter in ControlFilterParameters)
                    {
                        var value = string.IsNullOrEmpty(filterParameter.SessionKey)
                                           ? filterParameter.Value
                                           : (string) HttpContext.Current.Session[filterParameter.SessionKey];
                        filters.Add(new FilterParameter
                                        {
                                            Key = filterParameter.Key,
                                            Value = value,
                                        });
                    }
                }
                else
                    filters = ControlFilterParameters;

                string serialize = new JavaScriptSerializer().Serialize(filters);
                sb.AppendFormat("__filters={0}", HttpUtility.UrlEncode(serialize));
            }
            if (SelectParameters != null)
            {
                if (hasQuery) sb.Append("&");
                else
                {
                    sb.Append("?");
                    hasQuery = true;
                }
                string serialize = new JavaScriptSerializer().Serialize(SelectParameters);
                sb.AppendFormat("__selParams={0}", HttpUtility.UrlEncode(serialize));
            }
            if (CreateBackUrlByRequest)
            {
                if (hasQuery) sb.Append("&");
                else
                {
                    sb.Append("?");
                    hasQuery = true;
                }
                string url = HttpContext.Current.Request.Url.PathAndQuery;
                url = _queryRegexBackUrl.Replace(url, "").Replace("&&", "&").Replace("?&", "?");
                sb.AppendFormat("__backurl={0}", HttpUtility.UrlEncode(url));
            }
            else if (!string.IsNullOrEmpty(BackUrl))
            {
                if (hasQuery) sb.Append("&");
                else
                {
                    sb.Append("?");
                    hasQuery = true;
                }
                sb.AppendFormat("__backurl={0}", HttpUtility.UrlEncode(BackUrl));
            }
        }

        private void AddParameter(StringBuilder sb, string parameterName, string value, ref bool hasQuery)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (hasQuery) sb.Append("&");
            else
            {
                sb.Append("?");
                hasQuery = true;
            }
            sb.Append(parameterName);
            sb.Append("=");
            sb.Append(value);
        }

        public static MainPageUrlBuilder FromUrlToOtherControl(string url)
        {
            return new MainPageUrlBuilder(url)
                       {
                           IsFilterWindow = false,
                           IsSelect = false,
                           ShowFilter = false,
                           IsNew = false,
                           IsRead = false,
                           SelectMode = "",
                           ViewMode = "",
                           SelectedValues = "",
                       };
        }

        public string GetFilter(string controlName)
        {
            var filter = ControlFilterParameters.
                Where(p => p.Key.Equals(controlName, StringComparison.OrdinalIgnoreCase)).
                FirstOrDefault();
            if (!string.IsNullOrEmpty(filter.SessionKey))
                return (string)HttpContext.Current.Session[filter.SessionKey];
            if (string.IsNullOrEmpty(filter.Value)) return filter.Value;
            return filter.Value;
        }

        public Dictionary<string, string[]> GetFilterDic(string controlName)
        {
            var filter = GetFilter(controlName);
            if (string.IsNullOrEmpty(filter)) return null;

            var jss = new JavaScriptSerializer();
            var lists = jss.Deserialize<List<List<string>>>(filter);
            var filterValues = new Dictionary<string, string[]>();
            foreach (var list in lists)
                filterValues.Add(list[0], list.Skip(1).ToArray());
            return filterValues;
        }

        public Dictionary<string, List<FilterItem>> GetFilterItemsDic(string controlName)
        {
            return GetFilterItemsDicByFilterContent(GetFilter(controlName));
        }

        public static Dictionary<string, List<FilterItem>> GetFilterItemsDicByFilterContent(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return new Dictionary<string, List<FilterItem>>();

            var jss = new JavaScriptSerializer();
            var filterValues = new Dictionary<string, List<FilterItem>>();
            List<FilterItem> lists;
            try
            {
                lists = jss.Deserialize<List<FilterItem>>(filter);
            }
            catch (InvalidOperationException)
            {
                return filterValues;
            }
            foreach (var item in lists)
            {
                List<FilterItem> list;
                if (filterValues.ContainsKey(item.FilterName))
                    list = filterValues[item.FilterName];
                else
                    list = filterValues[item.FilterName] = new List<FilterItem>();
                list.Add(item);
            }
            return filterValues;
        }

        public static List<FilterItem> GetFilterItemsByFilterContent(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return new List<FilterItem>();

            var jss = new JavaScriptSerializer();
            List<FilterItem> lists;
            try
            {
                lists = jss.Deserialize<List<FilterItem>>(filter);
            }
            catch (InvalidOperationException)
            {
                return new List<FilterItem>();
            }

            return lists;
        }

        public void CheckUseSession()
        {
            if (!ControlFilterParameters.All(r => string.IsNullOrEmpty(r.SessionKey)))
                return;
            var filters = new List<FilterParameter>(ControlFilterParameters);
            ControlFilterParameters = new List<FilterParameter>();
            foreach (var filterParameter in filters)
                SetFilter(filterParameter.Key, filterParameter.Value);
        }

        public void SetFilter(string controlName, string filter)
        {
            var sessionKey = RemoveFilterGetSession(controlName);
            while (ControlFilterParameters.Count > 4) ControlFilterParameters.RemoveAt(0);
            if (filter != null && filter.Length > 100 && string.IsNullOrEmpty(sessionKey) && HttpContext.Current != null && HttpContext.Current.Session != null)
                sessionKey = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(sessionKey) && !string.IsNullOrEmpty(filter) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session[sessionKey] != null && (string)HttpContext.Current.Session[sessionKey] != filter)
                    sessionKey = Guid.NewGuid().ToString();
                HttpContext.Current.Session[sessionKey] = filter;
                ControlFilterParameters.Add(new FilterParameter { Key = controlName, SessionKey = sessionKey });
            }
            else
                ControlFilterParameters.Add(new FilterParameter { Key = controlName, Value = filter });
        }

        public string SetFilterBySessionKey(string controlName, string filter)
        {
            RemoveFilter(controlName);
            var sessionKey = Guid.NewGuid().ToString();

            // note: возмножно вместо удаления параметров нужно использовать сессию
            while (ControlFilterParameters.Count > 2) ControlFilterParameters.RemoveAt(0);
            if (!string.IsNullOrEmpty(sessionKey) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[sessionKey] = filter;
                ControlFilterParameters.Add(new FilterParameter { Key = controlName, SessionKey = sessionKey });
            }
            else
                ControlFilterParameters.Add(new FilterParameter { Key = controlName, Value = filter });
            return sessionKey;
        }

        public void SetFilter(string controlName, Dictionary<string, string[]> filters)
        {
            var jss = new JavaScriptSerializer();
            var values = filters.Select(p => new[] {p.Key, p.Value[0], p.Value[1], p.Value[2]}).ToList();
            if (values.Count == 0)
                SetFilter(controlName, "");
            else
                SetFilter(controlName, jss.Serialize(values));
        }

        public void SetFilter(string controlName, Dictionary<string, List<FilterItem>> filters)
        {
            SetFilter(controlName, GetFilterContentByFilterItemsDic(filters));
        }

        public string SetFilterBySessionKey(string controlName, Dictionary<string, List<FilterItem>> filters)
        {
            return SetFilterBySessionKey(controlName, GetFilterContentByFilterItemsDic(filters));
        }

        public void SetFilter(string controlName, List<FilterItem> filters)
        {
            SetFilter(controlName, GetFilterContentByFilterItems(filters));
        }

        public string SetFilterBySessionKey(string controlName, List<FilterItem> filters)
        {
            return SetFilterBySessionKey(controlName, GetFilterContentByFilterItems(filters));
        }

        public static string GetFilterContentByFilterItemsDic(Dictionary<string, List<FilterItem>> filterItems)
        {
            var jss = new JavaScriptSerializer();
            var values = filterItems.SelectMany(p => p.Value).ToList();
            if (values.Count == 0)
                return "";
            return jss.Serialize(values);
        }

        public static string GetFilterContentByFilterItems(List<FilterItem> filterItems)
        {
            var jss = new JavaScriptSerializer();
            if (filterItems.Count == 0)
                return "";
            return jss.Serialize(filterItems);
        }

        public static Dictionary<string, List<FilterItem>> ConvertFilters(
            Dictionary<string, List<FilterItem>> filters,
            params KeyValuePair<string, string>[] keyFromToValue)
        {
            return keyFromToValue
                .Where(r => filters.ContainsKey(r.Key))
                .Select(r => new {FilterName = r.Value, FilterItems = filters[r.Key]})
                .Select(r => r.FilterItems
                    .Select(filterItem =>
                        new FilterItem
                        {
                            FilterName = r.FilterName,
                            FilterType = filterItem.FilterType,
                            Value1 = filterItem.Value1,
                            Value2 = filterItem.Value2,
                        }).ToList())
                .ToDictionary(r => r[0].FilterName);
        }

        public void RemoveFilter(string controlName)
        {
            RemoveFilterGetSession(controlName);
        }

        public string RemoveFilterGetSession(string controlName)
        {
            var row = ControlFilterParameters.
                Select((r, ind) => new {r.Key, r.SessionKey, ind}).
                Where(r => r.Key.Equals(controlName, StringComparison.OrdinalIgnoreCase)).
                Select(r => new {r.SessionKey, r.ind}).
                FirstOrDefault();
            if (row != null)
            {
                ControlFilterParameters.RemoveAt(row.ind);
                return row.SessionKey;
            }
            return null; 
        }

        public static string GetSelectMode(Page page)
        {
            return page.Request.QueryString["mode"];
        }

        public static string GetViewMode(Page page)
        {
            return page.Request.QueryString["viewmode"];
        }

        private static MainPageUrlBuilder _customCacheCurrent;
        /// <summary>
        /// Текущий урл. Может изменяться в течении изменений на странице.
        /// </summary>
        public static MainPageUrlBuilder Current
        {
            get 
            {
                if (HttpContext.Current == null) return _customCacheCurrent;
                var url = (MainPageUrlBuilder)HttpContext.Current.Items["MainPageUrlBuilder"];
                if (url == null)
                {
                    url = new MainPageUrlBuilder(HttpContext.Current.Request.Url.PathAndQuery, false);
                    HttpContext.Current.Items["MainPageUrlBuilder"] = url;
                }
                return url;
            }
            set
            {
                if (HttpContext.Current == null) 
                    _customCacheCurrent = value;
                else
                    HttpContext.Current.Items["MainPageUrlBuilder"] = value;
            }
        }

        public static void ChangedUrl()
        {
            ChangedUrl(true);
        }

        public static void ChangedUrl(bool checkCorrect)
        {
            if (!checkCorrect || Current.IsDataControl)
                HttpContext.Current.RewritePath(Current.CreateUrl(true, true), true);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public MainPageUrlBuilder Clone()
        {
            return new MainPageUrlBuilder
            {
                Url = Url,
                UserControl = UserControl,
                ShowHistory = ShowHistory,
                SetSelectedValues = SetSelectedValues,
                SelectMode = SelectMode,
                ViewMode = ViewMode,
                SelectedValues = SelectedValues,
                RemoveQueryParameterPrefix = RemoveQueryParameterPrefix,
                QueryParametersPrefix = QueryParametersPrefix,
                Query = Query,
                Page = Page,
                IsSelect = IsSelect,
                ShowFilter = ShowFilter,
                IsRead = IsRead,
                IsNew = IsNew,
                IsMultipleSelect = IsMultipleSelect,
                IsFilterWindow = IsFilterWindow,
                IsExecute = IsExecute,
                IsCustomUserControl = IsCustomUserControl,
                IsDownload = IsDownload,
                IsDataControl = IsDataControl,
                NavigateTo = NavigateTo,
                FilterQuery = FilterQuery,
                CreateBackUrlByRequest = CreateBackUrlByRequest,
                HistoryOnDate = HistoryOnDate,
                BackUrl = BackUrl,
                ParameterConverter = new Dictionary<string, string>(ParameterConverter),
                QueryParameters = new Dictionary<string, string>(QueryParameters),
                CustomQueryParameters = new CustomQueryParametersDic(CustomQueryParameters),
                ControlFilterParameters = new List<FilterParameter>(ControlFilterParameters),
            };
        }

        public MainPageUrlBuilder Clone(string userControl, bool isNew, bool isRead, string selectMode, bool isSelect)
        {
            return Clone(userControl, isNew, isRead, selectMode, "", isSelect);
        }

        public MainPageUrlBuilder Clone(string userControl, bool isNew, bool isRead, string selectMode, string viewMode, bool isSelect)
        {
            return new MainPageUrlBuilder
            {
                Url = Url,
                UserControl = userControl,
                ShowHistory = ShowHistory,
                SetSelectedValues = SetSelectedValues,
                SelectMode = selectMode,
                ViewMode = viewMode,
                SelectedValues = SelectedValues,
                RemoveQueryParameterPrefix = RemoveQueryParameterPrefix,
                QueryParametersPrefix = QueryParametersPrefix,
                Query = Query,
                Page = Page,
                IsSelect = isSelect,
                IsRead = isRead,
                IsNew = isNew,
                IsMultipleSelect = IsMultipleSelect && isSelect,
                IsFilterWindow = IsFilterWindow,
                IsExecute = IsExecute,
                IsCustomUserControl = IsCustomUserControl,
                IsDownload = IsDownload,
                IsDataControl = IsDataControl,
                NavigateTo = NavigateTo,
                FilterQuery = FilterQuery,
                CreateBackUrlByRequest = CreateBackUrlByRequest,
                BackUrl = BackUrl,
                HistoryOnDate = HistoryOnDate,
                ParameterConverter = new Dictionary<string, string>(ParameterConverter),
                QueryParameters = new Dictionary<string, string>(QueryParameters),
                CustomQueryParameters = new CustomQueryParametersDic(CustomQueryParameters),
                ControlFilterParameters = new List<FilterParameter>(ControlFilterParameters),
            };
        }

        public MainPageUrlBuilder Clone(string userControl, bool isNew, string selectMode, bool isSelect)
        {
            return Clone(userControl, isNew, selectMode, "", isSelect);
        }

        public MainPageUrlBuilder Clone(string userControl, bool isNew, string selectMode, string viewMode, bool isSelect)
        {
            return new MainPageUrlBuilder
            {
                Url = Url,
                UserControl = userControl,
                ShowHistory = ShowHistory,
                SetSelectedValues = SetSelectedValues,
                SelectMode = selectMode,
                ViewMode = viewMode,
                SelectedValues = SelectedValues,
                RemoveQueryParameterPrefix = RemoveQueryParameterPrefix,
                QueryParametersPrefix = QueryParametersPrefix,
                Query = Query,
                Page = Page,
                IsSelect = isSelect,
                IsRead = IsRead,
                IsNew = isNew,
                IsMultipleSelect = IsMultipleSelect && isSelect,
                IsFilterWindow = IsFilterWindow,
                IsExecute = IsExecute,
                IsCustomUserControl = IsCustomUserControl,
                IsDownload = IsDownload,
                IsDataControl = IsDataControl,
                NavigateTo = NavigateTo,
                FilterQuery = FilterQuery,
                CreateBackUrlByRequest = CreateBackUrlByRequest,
                HistoryOnDate = HistoryOnDate,
                BackUrl = BackUrl,
                ParameterConverter = new Dictionary<string, string>(ParameterConverter),
                QueryParameters = new Dictionary<string, string>(QueryParameters),
                CustomQueryParameters = new CustomQueryParametersDic(CustomQueryParameters),
                ControlFilterParameters = new List<FilterParameter>(ControlFilterParameters),
            };
        }

        #region Nested type: CustomQueryParametersDic

        public class CustomQueryParametersDic : Dictionary<string, QueryParameter>
        {
            public CustomQueryParametersDic()
            {
            }

            public CustomQueryParametersDic(CustomQueryParametersDic dic) : base (dic)
            {
            }

            /// <summary>
            /// добавление элемента в словарь
            /// </summary>
            /// <param name="key">наименование ключа</param>
            /// <param name="value">знаение</param>
            /// <remarks>метод добавляет элемент в словарь с параметром UseInUrl=true</remarks>
            public void Add(string key, string value)
            {
                this[key] = new QueryParameter {UseInUrl = true, Value = value};
            }
        }

        #endregion

        #region Nested type: FilterParameter

        public struct FilterParameter
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string SessionKey { get; set; }

            public FilterParameter Clone()
            {
                return new FilterParameter {Key = Key, Value = Value, SessionKey = SessionKey,};
            }
        }

        #endregion

        #region Nested type: QueryParameter

        public struct QueryParameter
        {
            /// <summary>
            /// Значение параметра
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Передовать ли данное значение параметра в url, при его создании.
            /// </summary>
            public bool UseInUrl { get; set; }
        }

        #endregion
    }
}