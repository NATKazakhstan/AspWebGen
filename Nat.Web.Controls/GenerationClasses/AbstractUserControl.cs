/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 февраля 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Linq;
using System.Web.Caching;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using System.Collections.Generic;
using System.Data.SqlClient;
using Nat.Web.Tools;
using System.Text;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Controls.Properties;
using System.Reflection.Emit;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using Nat.Web.Tools.WorkFlow;

namespace Nat.Web.Controls.GenerationClasses
{
    using Microsoft.JScript;
    using Nat.Tools.Filtering;

    using Nat.Web.Tools.Security;

    using Convert = System.Convert;

    public abstract class AbstractUserControl : BaseHeaderControl, ISelectedValue, IActionControl
    {
        public const string SetFilters = "SetFilters:";
        public const string FilterByColumn = "FilterBy:";
        public const string FilterEqualsByColumn = "FilterBy:{0}:Equals:{1}:{2}";
        public const string ClearFilter = "ClearFilter";
        public const string SelectAllRecords = "SelectAllRecords";
        public const string UnselectAllRecords = "UnselectAllRecords";

        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string InfoMessageCustomParameter = "InfoMessage";
        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string InfoMessageUrlParameter = MainPageUrlBuilder.CustomParameterPrefix + InfoMessageCustomParameter;

        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string InfoMessageInSessionCustomParameter = "InfoMessageInSession";
        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string InfoMessageInSessionUrlParameter = MainPageUrlBuilder.CustomParameterPrefix + InfoMessageInSessionCustomParameter;

        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string ErrorMessageCustomParameter = "ErrorMessage";
        /// <summary>
        /// Имя параметра для добавления сообщений в журнал.
        /// </summary>
        public const string ErrorMessageUrlParameter = MainPageUrlBuilder.CustomParameterPrefix + ErrorMessageCustomParameter;
       
        public event EventHandler FilterChanged;

        private MainPageUrlBuilder _url;
        private IWorkFlow[] _workFlows;

        protected IWorkFlow[] WorkFlows
        {
            get { return _workFlows ?? (_workFlows = CreateWorkFlows()); }
        }

        protected virtual IWorkFlow[] CreateWorkFlows()
        {
            return new IWorkFlow[0];
        }

        protected bool HideSelectButton { get; set; }

        protected AbstractUserControl()
        {
            ParentControl = "";
        }

        #region ISelectedValue Members

        public event EventHandler SelectedIndexChanged;
        public abstract long SelectedValueLong { get; }
        public abstract object SelectedValue { get; }
        public abstract DataKey SelectedDataKey { get; }
        public abstract string[] DataKeyNames { get; set; }
        public abstract Type TableType { get; }
        public virtual bool ShowHistory { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual bool IsNew { get; set; }
        public virtual bool IsSelect { get; set; }

        [DefaultValue("")]
        [IDReferenceProperty]
        [TypeConverter(typeof (ControlIDConverter))]
        [Themeable(false)]
        public virtual string ParentControl { get; set; }

        public virtual void SetParentValue(object value)
        {
        }

        public virtual MainPageUrlBuilder Url
        {
            get
            {
                if (_url == null)
                {
                    _url = MainPageUrlBuilder.Current;
                    InitializeUrl();
                    return _url;
                }
                return _url;
            }
            set
            {
                _url = value;
                InitializeUrl();
            }
        }

        protected virtual void InitializeUrl()
        {
        }

        public virtual Expression GetExpression(string reference, Expression param)
        {
            return null;
        }

        public virtual Expression GetExpression(string reference, Expression param, QueryParameters qParams)
        {
            return GetExpression(reference, param);
        }

        #endregion

        protected virtual string GetSelectAllHeaderHtml()
        {
            return string.Format(
                @"
<a href=""#"" onclick=""{4}; return false;"" title=""{6}""><img style=""border:0px"" src=""{5}""/></a>
<a href=""#"" onclick=""{7}; return false;"" title=""{9}""><img style=""border:0px"" src=""{8}""/></a>
<input type=""checkbox"" onclick=""AddOrRemoveSelectedValue(this)"" value=""all"" {2}/>
<a href=""#"" style=""display:{10}"" onclick=""SetSelectToDialogArgumentsAndClose('{1}'); return false;"" title=""{0}""><img style=""border:0px"" src=""{3}""/></a>",
                Resources.SSelectText,
                Resources.SMultipleSelectedMessage,
                string.Empty,
                Themes.IconUrlSelectAll,
                HttpUtility.HtmlAttributeEncode(Page.ClientScript.GetPostBackEventReference(new PostBackOptions(this, SelectAllRecords))),
                Themes.IconUrlSelectAllRecords,
                Resources.SSelectAllFromAllPages,
                HttpUtility.HtmlAttributeEncode(Page.ClientScript.GetPostBackEventReference(new PostBackOptions(this, UnselectAllRecords))),
                Themes.IconUrlUnselectAllRecords,
                Resources.SUnselectAllFromAllPages,
                HideSelectButton ? "none" : string.Empty);
        }

        public virtual string TableName { get { return null; } }

        public virtual string ProjectName { get { return null; } }

        protected string SessionKey
        {
            get { return (string)(ViewState["AUC.SessionKey"] ?? (ViewState["AUC.SessionKey"] = Guid.NewGuid().ToString())); }
        }

        internal Dictionary<string, int> _selectedValues;

        public virtual string SelectedValues { get { return null; } set { } }

        public virtual IEnumerable<string> GetSelectedValues()
        {
            EnsureSelectedValuesCreated();
            return _selectedValues.Keys;
        }

        protected virtual bool IsSelectedValue(object value)
        {
            if (value == null) return false;
            EnsureSelectedValuesCreated();
            return _selectedValues.ContainsKey(value.ToString());
        }

        protected virtual void EnsureSelectedValuesCreated()
        {
            if (_selectedValues == null)
            {
                _selectedValues = new Dictionary<string, int>();
                foreach (var item in (SelectedValues ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    _selectedValues[item] = 1;
            }
        }

        protected virtual void ClearSelectedValues()
        {
            if (_selectedValues != null)
                _selectedValues.Clear();
            if (!string.IsNullOrEmpty(SelectedValues))
                SelectedValues = string.Empty;
        }

        protected virtual void SetSelectedValue(object value)
        {
            if (value != null)
            {
                EnsureSelectedValuesCreated();
                _selectedValues[value.ToString()] = 1;
                UpdateSelectedValues(_selectedValues.Keys);
            }
        }

        protected virtual void SetSelectedValue(object[] values)
        {
            if (values != null && values.Length > 0)
            {
                EnsureSelectedValuesCreated();
                foreach (var value in values)
                    _selectedValues[value.ToString()] = 1;
                UpdateSelectedValues(_selectedValues.Keys);
            }
        }

        protected virtual void RemoveSelectedValues(ICollection<object> values)
        {
            if (values != null && values.Count > 0)
            {
                EnsureSelectedValuesCreated();
                foreach (var value in values.Select(r => r.ToString()).Where(_selectedValues.ContainsKey))
                    _selectedValues.Remove(value);

                UpdateSelectedValues(_selectedValues.Keys);
            }
        }

        protected virtual void UpdateSelectedValues(IEnumerable<string> selectedValues)
        {
            var sb = new StringBuilder();
            foreach (var item in selectedValues)
            {
                sb.Append(item);
                sb.Append(",");
            }

            if (sb.Length > 0)
                SelectedValues = sb.ToString(0, sb.Length - 1);
            else
                SelectedValues = null;
        }

        protected virtual void OnSelectedIndexChanged(EventArgs args)
        {
            if (SelectedIndexChanged != null) SelectedIndexChanged(this, args);
        }

        protected virtual void AddErrorsFromFilter()
        {
        }
        
        protected void SetSearchText(string search)
        {
            var filters = Url.GetFilterItemsDic(TableName);
            if (string.IsNullOrEmpty(search))
            {
                if (filters.ContainsKey(BaseFilterParameterSearch<object>.DefaultFilterName))
                    filters.Remove(BaseFilterParameterSearch<object>.DefaultFilterName);
            }
            else
                filters[BaseFilterParameterSearch<object>.DefaultFilterName] = new List<FilterItem>
                    {
                        new FilterItem(
                            BaseFilterParameterSearch<object>.DefaultFilterName,
                            ColumnFilterType.ContainsWords,
                            new object[] { search })
                    };
            Url.SetFilter(TableName, filters);
        }

        protected void SetFilterText(string search, string filter)
        {
            var filters = MainPageUrlBuilder.GetFilterItemsDicByFilterContent(filter);
            if (string.IsNullOrEmpty(search))
            {
                if (filters.ContainsKey(BaseFilterParameterSearch<object>.DefaultFilterName))
                    filters.Remove(BaseFilterParameterSearch<object>.DefaultFilterName);
            }
            else
            {
                filters[BaseFilterParameterSearch<object>.DefaultFilterName] = new List<FilterItem>
                    {
                        new FilterItem(
                            BaseFilterParameterSearch<object>.DefaultFilterName,
                            ColumnFilterType.ContainsWords,
                            new object[] { search })
                    };
            }

            if (Url.GetFilter(TableName) == null && filters.Count == 0)
                return;

            Url.SetFilter(TableName, filters);
        }

        /// <summary>
        /// Метод для получения индекса записи с учетом сортировки.
        /// </summary>
        /// <param name="db">Контекст данных</param>
        /// <param name="data">запрос данных в которых нужно найти индекс</param>
        /// <param name="orderBy">сортировка журнала</param>
        /// <param name="where">условие выборки записи</param>
        /// <returns></returns>
        protected static long? GetRowIndexOld(IQueryable data, DataContext db, string orderBy, string where)
        {
            if (string.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            if (string.IsNullOrEmpty(where)) throw new ArgumentNullException("where");
            if (data == null) throw new ArgumentNullException("data");
            if (db == null) throw new ArgumentNullException("db");
            var command = db.GetCommand(data);
            var format = "select top 1 RowNumber from (select *, ROW_NUMBER() over(order by {1}) as RowNumber from ({0}) as MyT) as MyT where {2}";
            command.CommandText = string.Format(format, command.CommandText, orderBy, where);
            try
            {
                command.Connection.Open();
                var value = (long?)command.ExecuteScalar();
                return value == null ? null : (long?)(value.Value - 1);
            }
            finally
            {
                command.Connection.Close();
            }
        }

        private static Regex _regexGetFieldsFromWhere = new Regex(@"\b(\w+)=", RegexOptions.Compiled);

        /// <summary>
        /// Метод для получения индекса записи с учетом сортировки.
        /// </summary>
        /// <param name="db">Контекст данных</param>
        /// <param name="data">запрос данных в которых нужно найти индекс</param>
        /// <param name="orderBy">сортировка журнала</param>
        /// <param name="where">условие выборки записи</param>
        /// <returns></returns>
        protected static long? GetRowIndex(IQueryable data, DataContext db, string orderBy, string where)
        {
            if (string.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            if (string.IsNullOrEmpty(where)) throw new ArgumentNullException("where");
            if (data == null) throw new ArgumentNullException("data");
            if (db == null) throw new ArgumentNullException("db");

            var orderFields = orderBy.Split(',');
            var whereFields = _regexGetFieldsFromWhere.Matches(where).Cast<Match>().Select(r => r.Groups[1].Value).ToArray();
            orderBy = GetOrderSelect(ref data, orderFields, whereFields);

            var command = db.GetCommand(data);
            var format = "select top 1 RowNumber from (select *, ROW_NUMBER() over(order by {1}) as RowNumber from ({0}) as MyT) as MyT where {2}";
            command.CommandText = string.Format(format, command.CommandText, orderBy, where);
            try
            {
                command.Connection.Open();
                var value = (long?)command.ExecuteScalar();
                return value == null ? null : (long?)(value.Value - 1);
            }
            finally
            {
                command.Connection.Close();
            }
        }

        private static string GetOrderSelect(ref IQueryable data, string[] orderFields, string[] whereFields)
        {
            var orderExpressions = new Expression[orderFields.Length];
            var whereExpressions = new Expression[whereFields.Length];
            var query = data.Expression;
            var param = Expression.Parameter(query.Type.GetGenericArguments()[0], "pRowIndex");
            var item = Expression.Property(param, "Item");
            var orderFieldsDesc = new bool[orderFields.Length];//порядок сортировки
            var orderFieldsFullNames = new List<string>(orderFields.Length);

            //инициализируем колекции выражений для сортировки и имена полей класса
            for (int i = 0; i < orderFields.Length; i++)
            {
                string fieldName;
                var split = orderFields[i].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                orderExpressions[i] = LinqFilterGenerator.GetProperty(item.Type, split[0], item, out fieldName);
                orderFieldsDesc[i] = split.Length > 1 && split[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

                if (orderFieldsFullNames.Take(i).Contains(split[0]))
                {
                    orderFields[i] = orderFields[orderFieldsFullNames.IndexOf(split[0])];
                }
                else
                {
                    var newFieldName = fieldName;
                    var increment = 2;
                    //если такое имя уже существует, то увеличиваем индекс, в соответствии с тем как это делает LINQ TO SQL
                    while (orderFields.Take(i).Contains(newFieldName))
                        newFieldName = fieldName + increment++;
                    orderFields[i] = newFieldName;
                }
                orderFieldsFullNames.Add(split[0]);
            }

            //инициализируем колекции выражений для фильтрации
            for (int i = 0; i < whereFields.Length; i++)
                whereExpressions[i] = LinqFilterGenerator.GetProperty(item.Type, whereFields[i], item);

            //получаем колекцию всех выражений, с учетом повторов в колекциях
            var allExpressions = new List<Expression>();
            foreach (var expression in whereExpressions.Union(orderExpressions).Where(r => !allExpressions.Select(t => t.ToString()).Contains(r.ToString())))
                allExpressions.Add(expression);

            //список результирующих типов вырожений
            var types = allExpressions.Select(r => r.Type).ToArray();
            
            //список имен свойств для класса
            var fieldNames = orderFields.Union(whereFields).ToArray();
            
            //получаем необходимый класс вызываем конструктор и изменяем выборку
            var classType = GetOrderType(types, fieldNames);
            var ctor = classType.GetConstructor(types);
            var members = fieldNames.Select(r => classType.GetMember(r).First());
            var newExp = Expression.New(ctor, allExpressions, members);
            var lambda = Expression.Lambda(newExp, param);
            query = Expression.Call(typeof(Queryable), "Select", new [] {param.Type, classType}, query, lambda);            
            data = data.Provider.CreateQuery(query);

            //полям добавляем направление сортировки
            for (int index = 0; index < orderFieldsDesc.Length; index++)
                if (orderFieldsDesc[index]) orderFields[index] += " desc";
            return string.Join(",", orderFields);
        }

        private static Dictionary<string, Type> _orderTypes;
        private static object _lockGetOrderType = new object();
        private static AppDomain _orderClassesDomain;
        private static AssemblyName _orderClassesAsmName;
        private static AssemblyBuilder _orderClassesAsmBuilder;
        private static ModuleBuilder _orderClassesModule;

        private static Type GetOrderType(Type[] ctorParams, string[] fieldNames)
        {
            lock (_lockGetOrderType)
            {
                if (_orderClassesDomain == null)
                {
                    _orderClassesDomain = Thread.GetDomain();
                    _orderClassesAsmName = new AssemblyName {Name = "Nat.Web.Controls.OrderClasses"};
                    _orderClassesAsmBuilder = _orderClassesDomain.DefineDynamicAssembly(_orderClassesAsmName, AssemblyBuilderAccess.Run);
                    _orderClassesModule = _orderClassesAsmBuilder.DefineDynamicModule("OrderClassesModule");
                    _orderTypes = new Dictionary<string, Type>();
                }
                var stringKey = string.Join(";", ctorParams.Select(r => r.Name).Union(fieldNames).ToArray());
                if (_orderTypes.ContainsKey(stringKey))
                    return _orderTypes[stringKey];

                var pointTypeBld = _orderClassesModule.DefineType("OC" + Guid.NewGuid().ToString("N"), TypeAttributes.Public);
                var fields = new FieldBuilder[ctorParams.Length];
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = pointTypeBld.DefineField(fieldNames[i], ctorParams[i], FieldAttributes.Public);

                var objType = typeof (Object);
                var objCtor = objType.GetConstructor(new Type[0]);

                var pointCtor = pointTypeBld.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorParams);
                var ctorIL = pointCtor.GetILGenerator();

                // NOTE: ldarg.0 holds the "this" reference - ldarg.1, ldarg.2, and ldarg.3
                // hold the actual passed parameters. ldarg.0 is used by instance methods
                // to hold a reference to the current calling object instance. Static methods
                // do not use arg.0, since they are not instantiated and hence no reference
                // is needed to distinguish them. 

                ctorIL.Emit(OpCodes.Ldarg_0);

                // Here, we wish to create an instance of System.Object by invoking its
                // constructor, as specified above.

                ctorIL.Emit(OpCodes.Call, objCtor);

                // Now, we'll load the current instance ref in arg 0, along
                for (int i = 0; i < fields.Length; i++)
                {
                    ctorIL.Emit(OpCodes.Ldarg_0);
                    if (i == 0)
                        ctorIL.Emit(OpCodes.Ldarg_1);
                    else if (i == 1)
                        ctorIL.Emit(OpCodes.Ldarg_2);
                    else if (i == 2)
                        ctorIL.Emit(OpCodes.Ldarg_3);
                    else
                        ctorIL.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                    ctorIL.Emit(OpCodes.Stfld, fields[i]);
                }

                // Our work complete, we return.
                ctorIL.Emit(OpCodes.Ret);

                // Finally, we create the type.
                Type pointType = pointTypeBld.CreateType();
                _orderTypes[stringKey] = pointType;
                return pointType;
            }
        }

        /// <summary>
        /// Получить значение по входному параметру inValue, где кешеруется по строке cacheName и входному параметру inValue.
        /// Если кеш не содержит значения то обращаемся к функции function для получения значения, затем оно кешируется.
        /// По умолчанию кешируется на 2 минуты.
        /// </summary>
        /// <typeparam name="T">Тип входного параметра</typeparam>
        /// <typeparam name="TResult">Тип результрирующего параметра, если значение может быть null, то нужно использовать класс Nullable</typeparam>
        /// <param name="inValue">Входной параметр.</param>
        /// <param name="cacheName">Строка под которой кешируется значение.</param>
        /// <param name="function">Функция определяющая значение по вохдному параметру</param>
        /// <returns>Возвращеат значение либо из кеша либо по результату функции</returns>
        public static TResult GetValue<T, TResult>(T inValue, string cacheName, Func<T, TResult> function)
        {
            return GetValue(inValue, cacheName, function, null, DateTime.Now.AddMinutes(2), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        /// <summary>
        /// Получить значение по входному параметру inValue, где кешеруется по строке cacheName и входному параметру inValue.
        /// Если кеш не содержит значения то обращаемся к функции function для получения значения, затем оно кешируется.
        /// </summary>
        /// <typeparam name="T">Тип входного параметра</typeparam>
        /// <typeparam name="TResult">Тип результрирующего параметра, если значение может быть null, то нужно использовать класс Nullable</typeparam>
        /// <param name="inValue">Входной параметр.</param>
        /// <param name="cacheName">Строка под которой кешируется значение.</param>
        /// <param name="function">Функция определяющая значение по вохдному параметру</param>
        /// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter contains null.</param>
        /// <param name="absoluteExpiration">The time at which the added object expires and is removed from the cache. If you are using sliding expiration, the absoluteExpiration parameter must be System.Web.Caching.Cache.NoAbsoluteExpiration.</param>
        /// <param name="slidingExpiration">The interval between the time the added object was last accessed and the time at which that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed. If you are using absolute expiration, the slidingExpiration parameter must be System.Web.Caching.Cache.NoSlidingExpiration.</param>
        /// <param name="priority">The relative cost of the object, as expressed by the System.Web.Caching.CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
        /// <param name="onRemoveCallback">A delegate that, if provided, is called when an object is removed from the cache. You can use this to notify applications when their objects are deleted from the cache.</param>
        /// <returns>Возвращеат значение либо из кеша либо по результату функции</returns>
        public static TResult GetValue<T, TResult>(T inValue, string cacheName, Func<T, TResult> function, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            return GetValue(inValue, cacheName, function, null, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
        }

        /// <summary>
        /// Получить значение по входному параметру inValue, где кешеруется по строке cacheName и входному параметру inValue.
        /// Если кеш не содержит значения то обращаемся к функции function для получения значения, затем оно кешируется.
        /// </summary>
        /// <typeparam name="T">Тип входного параметра</typeparam>
        /// <typeparam name="TResult">Тип результрирующего параметра, если значение может быть null, то нужно использовать класс Nullable</typeparam>
        /// <param name="inValue">Входной параметр.</param>
        /// <param name="cacheName">Строка под которой кешируется значение.</param>
        /// <param name="function">Функция определяющая значение по вохдному параметру</param>
        /// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter contains null.</param>
        /// <param name="absoluteExpiration">The time at which the added object expires and is removed from the cache. If you are using sliding expiration, the absoluteExpiration parameter must be System.Web.Caching.Cache.NoAbsoluteExpiration.</param>
        /// <param name="slidingExpiration">The interval between the time the added object was last accessed and the time at which that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed. If you are using absolute expiration, the slidingExpiration parameter must be System.Web.Caching.Cache.NoSlidingExpiration.</param>
        /// <param name="priority">The relative cost of the object, as expressed by the System.Web.Caching.CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
        /// <param name="onRemoveCallback">A delegate that, if provided, is called when an object is removed from the cache. You can use this to notify applications when their objects are deleted from the cache.</param>
        /// <returns>Возвращеат значение либо из кеша либо по результату функции</returns>
        public static TResult GetValue<T, TResult>(T inValue, string cacheName, Func<T, TResult> function, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority)
        {
            return GetValue(inValue, cacheName, function, null, absoluteExpiration, slidingExpiration, priority, null);
        }

        /// <summary>
        /// Получить значение по входному параметру inValue, где кешеруется по строке cacheName и входному параметру inValue.
        /// Если кеш не содержит значения то обращаемся к функции function для получения значения, затем оно кешируется.
        /// </summary>
        /// <typeparam name="T">Тип входного параметра</typeparam>
        /// <typeparam name="TResult">Тип результрирующего параметра, если значение может быть null, то нужно использовать класс Nullable</typeparam>
        /// <param name="inValue">Входной параметр.</param>
        /// <param name="cacheName">Строка под которой кешируется значение.</param>
        /// <param name="function">Функция определяющая значение по вохдному параметру</param>
        /// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache. If there are no dependencies, this parameter contains null.</param>
        /// <param name="absoluteExpiration">The time at which the added object expires and is removed from the cache. If you are using sliding expiration, the absoluteExpiration parameter must be System.Web.Caching.Cache.NoAbsoluteExpiration.</param>
        /// <param name="slidingExpiration">The interval between the time the added object was last accessed and the time at which that object expires. If this value is the equivalent of 20 minutes, the object expires and is removed from the cache 20 minutes after it is last accessed. If you are using absolute expiration, the slidingExpiration parameter must be System.Web.Caching.Cache.NoSlidingExpiration.</param>
        /// <param name="priority">The relative cost of the object, as expressed by the System.Web.Caching.CacheItemPriority enumeration. The cache uses this value when it evicts objects; objects with a lower cost are removed from the cache before objects with a higher cost.</param>
        /// <param name="onRemoveCallback">A delegate that, if provided, is called when an object is removed from the cache. You can use this to notify applications when their objects are deleted from the cache.</param>
        /// <returns>Возвращеат значение либо из кеша либо по результату функции</returns>
        public static TResult GetValue<T, TResult>(T inValue, string cacheName, Func<T, TResult> function, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            var cacheKey = cacheName + inValue.ToString();
            if (HttpContext.Current.Cache[cacheKey] == null)
            {
                var value = function(inValue);
                if (value != null)
                    HttpContext.Current.Cache.Add(cacheKey, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
                return value;
            }
            return (TResult)HttpContext.Current.Cache[cacheKey];
        }

        private bool? _HasAjax;
        public bool HasAjax
        {
            get
            {
                if(_HasAjax == null)
                {
                    var updatePanel = ControlHelper.FindControl<UpdatePanel>(this);
                    _HasAjax = ScriptManager != null && updatePanel != null;
                }
                return _HasAjax.Value;
            }
        }

        private bool? _UseAjaxAction;
        public bool UseAjaxAction
        {
            get
            {
                if (_UseAjaxAction == null)
                {
                    var updatePanel = ControlHelper.FindControl<UpdatePanel>(this);
                    _UseAjaxAction = ScriptManager != null && updatePanel != null && (MainPageUrlBuilder.Current.IsSelect || IsSelect || AsActionControl);
                }
                return _UseAjaxAction.Value;
            }
        }

        private ScriptManager _ScriptManager;
        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }

        protected BaseMainPage MainPage { get { return (BaseMainPage)Page; } }

        private class RowNumberClass
        {
            public long RowNumber { get; set; }
        }

        public static Action<string, string> ShowWarningMessage { get; set; }

        protected virtual void AddInfoMessage(string message)
        {
        }

        protected virtual void AddErrorMessage(string errorMessage)
        {
        }

        protected virtual void AddErrorMessage(IEnumerable<string> errorMessages)
        {
            foreach (var errorMessage in errorMessages)
                AddErrorMessage(errorMessage);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Url.CustomQueryParameters.ContainsKey(InfoMessageCustomParameter))
                AddInfoMessage(Url.CustomQueryParameters[InfoMessageCustomParameter].Value);

            if (Url.CustomQueryParameters.ContainsKey(InfoMessageInSessionCustomParameter))
            {
                var value = Session[Url.CustomQueryParameters[InfoMessageInSessionCustomParameter].Value];
                var message = value as string;
                if (message != null) 
                    AddInfoMessage(message);

                var messages = value as IEnumerable<string>;
                if (messages != null)
                {
                    foreach (var messageValue in messages)
                        AddInfoMessage(messageValue);
                }
            }

            if (Url.CustomQueryParameters.ContainsKey(ErrorMessageCustomParameter))
                AddErrorMessage(Url.CustomQueryParameters[ErrorMessageCustomParameter].Value);
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage.
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="argument">аргумент вызвавший событие</param>
        /// <param name="prefixArgument">аргумент на который должно работать событие</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции</param>
        /// <param name="function">функция выполняющее действие (событие)</param>
        /// <returns>Результат true если была вызвана функция иначе false</returns>
        protected bool ExecuteClickButton<T>(string argument, string prefixArgument, string successfulMessage, Func<T?, bool> function)
            where T : struct
        {
            return ExecuteClickButton(argument, prefixArgument, successfulMessage, null, function);
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage, иначе выводится информация об ошибке failedMessage.
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="argument">аргумент вызвавший событие</param>
        /// <param name="prefixArgument">аргумент на который должно работать событие</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции</param>
        /// <param name="failedMessage">Сообщение при не удачном исполнении фунции</param>
        /// <param name="function">функция выполняющее действие (событие)</param>
        /// <returns>Результат true если была вызвана функция иначе false</returns>
        protected bool ExecuteClickButton<T>(string argument, string prefixArgument, string successfulMessage, string failedMessage, Func<T?, bool> function)
            where T : struct
        {
            bool? result = null;
            try
            {
                if (argument.Equals(prefixArgument, StringComparison.OrdinalIgnoreCase))
                    result = function(null);
                else if (argument.StartsWith(prefixArgument, StringComparison.OrdinalIgnoreCase))
                {
                    var strValue = argument.Substring(prefixArgument.Length);
                    T value = (T)Convert.ChangeType(strValue, typeof(T));
                    result = function(value);
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                AddErrorMessage(e.Message);
                AddErrorMessage(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddErrorMessage(e.InnerException.Message);
                    AddErrorMessage(e.InnerException.StackTrace);
                }
                return false;
            }
            if (result != null)
            {
                if (result.Value)
                    AddInfoMessage(successfulMessage);
                else if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                return result.Value;
            }
            return false;
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage, иначе выводится информация об ошибке failedMessage.
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="argument">аргумент вызвавший событие</param>
        /// <param name="prefixArgument">аргумент на который должно работать событие</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции</param>
        /// <param name="failedMessage">Сообщение при не удачном исполнении фунции</param>
        /// <param name="function">функция выполняющее действие (событие)</param>
        /// <returns>Результат true если была вызвана функция иначе false</returns>
        protected bool ExecuteClickButton<T>(string argument, string prefixArgument, string successfulMessage, string failedMessage, Func<T?, bool> function, params KeyValuePair<int, string>[] errorSqlNumberMessages)
            where T : struct
        {
            bool? result = null;
            try
            {
                if (argument.Equals(prefixArgument, StringComparison.OrdinalIgnoreCase))
                    result = function(null);
                else if (argument.StartsWith(prefixArgument, StringComparison.OrdinalIgnoreCase))
                {
                    var strValue = argument.Substring(prefixArgument.Length);
                    T value = (T)Convert.ChangeType(strValue, typeof(T));
                    result = function(value);
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                var sqlException = e as SqlException;
                if (sqlException != null && errorSqlNumberMessages != null && errorSqlNumberMessages.Length > 0)
                {
                    bool errorFind = false;
                    var dicAddedErrors = new Dictionary<int, bool>();
                    foreach (SqlError error in sqlException.Errors)
                    {
                        if (dicAddedErrors.ContainsKey(error.Number))
                            continue;
                        var message = errorSqlNumberMessages.FirstOrDefault(n => n.Key == error.Number).Value;
                        if (!string.IsNullOrEmpty(message))
                        {
                            errorFind = true;
                            AddErrorMessage(message);
                            dicAddedErrors[error.Number] = true;
                        }
                        else if (error.Number >= 50000)
                        {
                            errorFind = true;
                            AddErrorMessage(error.Message);
                            dicAddedErrors[error.Number] = true;
                        }
                    }

                    if (errorFind) return false;
                }

                AddErrorMessage(e.Message);
                AddErrorMessage(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddErrorMessage(e.InnerException.Message);
                    AddErrorMessage(e.InnerException.StackTrace);
                }
                return false;
            }
            if (result != null)
            {
                if (result.Value)
                    AddInfoMessage(successfulMessage);
                else if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                return result.Value;
            }
            return false;
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage, иначе выводится информация об ошибке failedMessage.
        /// </summary>
        /// <typeparam name="T">Тип аргумента</typeparam>
        /// <param name="argument">аргумент вызвавший событие</param>
        /// <param name="prefixArgument">аргумент на который должно работать событие</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции</param>
        /// <param name="failedMessage">Сообщение при не удачном исполнении фунции</param>
        /// <param name="function">функция выполняющее действие (событие)</param>
        /// <returns>Результат true если была вызвана функция иначе false</returns>
        protected bool ExecuteClickButtonInTransaction<T>(string argument, string prefixArgument, string successfulMessage, string failedMessage, Func<T?, bool> function, DataContext db, params KeyValuePair<int, string>[] errorSqlNumberMessages)
            where T : struct
        {
            try
            {
                db.Connection.Open();
                try
                {
                    db.Transaction = db.Connection.BeginTransaction();
                    var result = ExecuteClickButton(
                        argument, prefixArgument, successfulMessage, failedMessage, function, errorSqlNumberMessages);
                    
                    if (result)
                        db.Transaction.Commit();
                    else
                        db.Transaction.Rollback();
                    
                    return result;
                }
                catch
                {
                    if (db.Transaction != null)
                    {
                        db.Transaction.Rollback();
                        db.Transaction = null;
                    }

                    throw;
                }
            }
            finally
            {
                db.Connection.Close();
                db.Transaction = null;
            }
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage.
        /// </summary>
        /// <typeparam name="T1">Тип аргумента 1.</typeparam>
        /// <typeparam name="T2">Тип аргумента 2.</typeparam>
        /// <param name="argument">Аргумент вызвавший событие.</param>
        /// <param name="prefixArgument">Аргумент на который должно работать событие.</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции.</param>
        /// <param name="function">Функция выполняющее действие (событие).</param>
        /// <returns>Результат true если была вызвана функция иначе false.</returns>
        protected bool ExecuteClickButton<T1, T2>(string argument, string prefixArgument, string successfulMessage, Func<T1?, T2?, bool> function)
            where T1 : struct
            where T2 : struct
        {
            return ExecuteClickButton<T1, T2>(argument, prefixArgument, successfulMessage, null, function);
        }

        /// <summary>
        /// Метод для упрощения кода.
        /// Позволяет выполнить функцию function, 
        /// если параметр argument равен параметру prefixArgument, то вызов функции function со значением null, функция ExecuteClickButton возвращает true.
        /// если параметр argument начинается с параметра prefixArgument, то в функцию function передается приведенное к типу T значение из argument, функция ExecuteClickButton возвращает true.
        /// иначе фунция function не вызывается и функция ExecuteClickButton возвращает false.
        /// Если функция function возвращает true, то в информационное поле выводится сообщение successfulMessage, иначе выводится информация об ошибке failedMessage.
        /// </summary>
        /// <typeparam name="T1">Тип аргумента 1.</typeparam>
        /// <typeparam name="T2">Тип аргумента 2.</typeparam>
        /// <param name="argument">Аргумент вызвавший событие.</param>
        /// <param name="prefixArgument">Аргумент на который должно работать событие.</param>
        /// <param name="successfulMessage">Сообщение при удачном исполнении фунции.</param>
        /// <param name="failedMessage">Сообщение при не удачном исполнении фунции.</param>
        /// <param name="function">Функция выполняющее действие (событие).</param>
        /// <returns>Результат true если была вызвана функция иначе false.</returns>
        protected bool ExecuteClickButton<T1, T2>(string argument, string prefixArgument, string successfulMessage, string failedMessage, Func<T1?, T2?, bool> function)
            where T1 : struct
            where T2 : struct
        {
            bool? result = null;
            try
            {
                if (argument.Equals(prefixArgument, StringComparison.OrdinalIgnoreCase))
                    result = function(null, null);
                else if (argument.StartsWith(prefixArgument, StringComparison.OrdinalIgnoreCase))
                {
                    var strValue = argument.Substring(prefixArgument.Length);
                    var split = strValue.Split(',');
                    T1 value1 = (T1)Convert.ChangeType(split[0], typeof(T1));
                    T2 value2 = (T2)Convert.ChangeType(split[1], typeof(T2)); ;
                    result = function(value1, value2);
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                AddErrorMessage(e.Message);
                AddErrorMessage(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddErrorMessage(e.InnerException.Message);
                    AddErrorMessage(e.InnerException.StackTrace);
                }
                return false;
            }
            if (result != null)
            {
                if (result.Value)
                    AddInfoMessage(successfulMessage);
                else if (!string.IsNullOrEmpty(failedMessage))
                    AddErrorMessage(failedMessage);
                return result.Value;
            }
            return false;
        }

        #region IActionControl Members

        public bool AsActionControl { get; set; }
        public bool IsFirstCreation { get; set; }

        public virtual void ResultActionValues(ActionControlResults result)
        {
        }

        #endregion
        
        protected void SetFilterUrl(HyperLink hlFilter, HyperLink hlCancelFilter, string selectedValue, string tableName)
        {
            var filterUrl = new MainPageUrlBuilder(Url.Url)
                {
                    UserControl = tableName + "Filter",
                    IsFilterWindow = true,
                    SelectMode = "none",
                };
            var thisUrl = Url.Clone();

            thisUrl.SetFilter(tableName, "");
            thisUrl.QueryParameters["ref" + tableName] = selectedValue;
            if (HasAjax)
            {
                hlFilter.Attributes["onclick"] = string.Format("return OpenFilterPostBack(this,'{0}','{1}');",
                                                               filterUrl.CreateUrl(true), thisUrl.CreateUrl(false));
                hlFilter.NavigateUrl = Page.ClientScript.GetPostBackClientHyperlink(this, SetFilters);
            }
            else
            {
                hlFilter.Attributes["onclick"] = string.Format("return OpenFilter(this,'{0}','{1}');",
                                                               filterUrl.CreateUrl(true), thisUrl.CreateUrl(false));
            }
            if (!string.IsNullOrEmpty(Url.GetFilter(tableName)))
            {
                if (HasAjax)
                {
                    var postBack = Page.ClientScript.GetPostBackClientHyperlink(this, ClearFilter);
                    hlCancelFilter.NavigateUrl = postBack;
                }
                else
                    hlCancelFilter.NavigateUrl = thisUrl.CreateUrl(true);
                hlCancelFilter.Visible = true;
            }
        }

        protected bool RaisePostBackEventFilter(string eventArgument, string tableName)
        {
            if (eventArgument.StartsWith(FilterByColumn, StringComparison.OrdinalIgnoreCase))
            {
                var dic = Url.GetFilterItemsDic(tableName) ?? new Dictionary<string, List<FilterItem>>();
                var split = eventArgument.Split(':');
                var filterName = split[1];
                dic[filterName] = new List<FilterItem>
                                  {
                                      new FilterItem
                                      {
                                          FilterName = filterName,
                                          FilterType = split[2],
                                          Value1 = split.Length > 3 ? split[3] : string.Empty,
                                          Value2 = split.Length > 4 ? split[4] : string.Empty,
                                      }
                                  };
                Url.SetFilter(tableName, dic);
                if (Url != MainPageUrlBuilder.Current)
                    MainPageUrlBuilder.Current.SetFilter(tableName, dic);
                MainPageUrlBuilder.ChangedUrl();
                OnFilterChanged(EventArgs.Empty);
                return true;
            }
            if (eventArgument.Equals(ClearFilter, StringComparison.OrdinalIgnoreCase))
            {
                Url.SetFilter(tableName, "");
                if (Url != MainPageUrlBuilder.Current)
                    MainPageUrlBuilder.Current.SetFilter(tableName, "");
                MainPageUrlBuilder.ChangedUrl();
                OnFilterChanged(EventArgs.Empty);
                return true;
            }
            if (eventArgument.StartsWith(SetFilters, StringComparison.OrdinalIgnoreCase))
            {
                var filterValues = eventArgument.Substring(SetFilters.Length);
                Url.SetFilter(tableName, filterValues);
                if (Url != MainPageUrlBuilder.Current)
                    MainPageUrlBuilder.Current.SetFilter(tableName, filterValues);
                MainPageUrlBuilder.ChangedUrl();
                OnFilterChanged(EventArgs.Empty);
                return true;
            }
            return false;
        }

        protected virtual void OnFilterChanged(EventArgs e)
        {
            if (FilterChanged != null)
                FilterChanged(this, e);
        }
    }

    public abstract class AbstractUserControl<TKey> : AbstractUserControl
        where TKey : struct
    {
        [Browsable(false)]
        public abstract TKey? SelectedValueKey { get; }

        public override string TableName
        {
            get { return null; }
        }
        
        protected void GetToChildsHtml(StringBuilder sb, TKey key, TKey? parentKey, bool usePostBack, string name, int countChildRows)
        {
            if (usePostBack)
            {
                var url = Url.Clone();
                url.QueryParameters["ref" + TableName] = "";
                url.QueryParameters["ref" + TableName + "Parent"] = key.ToString();
                sb.Append("<a class=\"linkAsButton\" href=\"");
                url.CreateUrl(sb);
                sb.Append("\" onclick=\"");
                sb.Append(HttpUtility.HtmlAttributeEncode(Page.ClientScript.GetPostBackEventReference(new PostBackOptions(this, "childs:" + key))));
                sb.Append("; return false;\">+</a>");
            }
            else
            {
                sb.AppendFormat(
                    "<a class=\"linkAsButton\" href=\"javascript:ExpandTree('{0}', '{1}', '{2}', '{3}', '{4}&nbsp;<a disabled=disabled><<</a>&nbsp;1&nbsp;<a disabled=disabled>>></a>&nbsp;{5}')\">+</a>",
                    key, parentKey, TableName, 
                    HttpUtility.HtmlAttributeEncode(name), 
                    string.Format(Resources.SCountPages, 1), 
                    string.Format(Resources.SCountData, countChildRows));
            }
        }

        public IEnumerable<TKey> GetSelectedValues()
        {
            EnsureSelectedValuesCreated();
            return _selectedValues.Keys.Select(r => (TKey)Convert.ChangeType(r, typeof(TKey)));
        }

        protected IEnumerable<string> GetStringSelectedValues()
        {
            EnsureSelectedValuesCreated();
            return _selectedValues.Keys;
        }

        protected virtual void GeneateReadHtml(BaseGridColumns gridColumns, StringBuilder sb, string cssClass)
        {
            gridColumns.GeneateReadHtml(sb, cssClass);
        }

        protected virtual void GeneateReadHtml(BaseGridColumns gridColumns, StringBuilder sb, string cssClass, string cssClassNotSelected)
        {
            gridColumns.GeneateReadHtml(sb, cssClass, cssClassNotSelected);
        }

        protected virtual string GetStartTableRow()
        {
            return "<tr>";
        }

        protected virtual string GetEndTableRow()
        {
            return "</tr>";
        }

        protected virtual string GetGroupRowHeader(
            List<List<GridHtmlGenerator.Column>> groupColumns, 
            Dictionary<GridHtmlGenerator.Column, object> groupColumnsValues)
        {
            var isEquals = true;
            var sb = new StringBuilder();
            int colSpan;
            foreach (var items in groupColumns)
            {
                if (isEquals)
                {
                    if (items.Any(column => !Equals(groupColumnsValues[column], column.ColumnValueHandler()))) 
                        isEquals = false;
                    if (isEquals) continue;
                }

                if (!items.Any(r => r.GroupVisible))
                    continue;

                sb.Append("<tr class=\"groupH\">");

                foreach (var column in items.Where(r => r.GroupVisible))
                {
                    groupColumnsValues[column] = column.ColumnValueHandler();
                    colSpan = column.ColGroupSpan;
                    if (colSpan > 1)
                    {
                        sb.Append("<td colspan=\"");
                        sb.Append(colSpan);
                        sb.Append("\">");
                    }
                    else sb.Append("<td>");
                    column.ColumnContentHandler(sb);
                    sb.Append("</td>");
                }

                sb.Append("</tr>");
            }

            if (isEquals) return string.Empty;
            return sb.ToString();
        }

        protected virtual string GetFooterRows<TTotalInfo>(IEnumerable<TTotalInfo> totals, BaseGridColumns gridColumns, Action<TTotalInfo> renderTotalInfo)
            where TTotalInfo : class
        {
            var sb = new StringBuilder();
            foreach (var total in totals)
            {
                renderTotalInfo(total);
                sb.Append("<tr class=\"ms-vb\">");
                int colSpan = 0;
                var columns = gridColumns.Columns.Where(p => p.Visible && (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions)));
                foreach (var column in columns)
                {
                    colSpan--;
                    if (colSpan > 0 || column.TotalColSpan == 0) continue;
                    colSpan = column.TotalColSpan;
                    if (colSpan > 1)
                    {
                        sb.Append("<td colspan=\"");
                        sb.Append(colSpan);
                        sb.Append("\">");
                    }
                    else sb.Append("<td>");

                    if (column.ColumnContentTotalHandler != null) column.ColumnContentTotalHandler(sb);
                    sb.Append("</td>");
                }

                sb.Append("</tr>");
            }

            renderTotalInfo(null);
            return sb.ToString();
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            AddErrorsFromFilter();
        }

        protected override void AddErrorsFromFilter()
        {
            var thisType = GetType();
            var descriptor = thisType.GetProperty("FilterControlInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            BaseFilterControl<TKey> filter = null;
            if (descriptor != null && typeof(BaseFilterControl<TKey>).IsAssignableFrom(descriptor.PropertyType))
            {
                filter = (BaseFilterControl<TKey>)descriptor.GetValue(this, null);
                if (filter != null)
                {
                    foreach (var error in filter.Errors.Distinct())
                        AddErrorMessage(error);
                }
            }

            if (descriptor != null && filter == null && thisType.BaseType != null)
            {
                var fieldInfo = thisType.BaseType.GetField("internalfilterControl", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null && typeof(BaseFilterControl<TKey>).IsAssignableFrom(fieldInfo.FieldType))
                {
                    filter = (BaseFilterControl<TKey>)fieldInfo.GetValue(this);
                    if (filter != null)
                    {
                        foreach (var error in filter.Errors.Distinct())
                            AddErrorMessage(error);
                    }
                }
            }
        }
    }
}