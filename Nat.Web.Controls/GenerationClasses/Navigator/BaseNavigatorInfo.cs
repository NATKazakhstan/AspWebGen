using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.UI;
using Nat.Web.Controls.Properties;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    using System.Web.Caching;

    public abstract class BaseNavigatorInfo
    {
        private PropertyInfo _referenceProperty;
        private static Dictionary<Type, BaseNavigatorInfo> _cacheNavigators = new Dictionary<Type, BaseNavigatorInfo>();
        private static Dictionary<Type, object> _cacheDataSource = new Dictionary<Type, object>();

        /// <summary>
        /// Ключ текущей записи таблицы.
        /// </summary>
        protected string TableNameKey;
        /// <summary>
        /// Историчный ключ записи таблицы.
        /// </summary>
        protected string TableNameHistoryKey;
        /// <summary>
        /// Родительский ключ текущей записи таблицы.
        /// </summary>
        protected string TableNameParentKey;

        /// <summary>
        /// Получить ключ навигатора для формирования кеша в разрезе референса.
        /// </summary>
        /// <returns></returns>
        protected string ReferenceCacheKey;
        /// <summary>
        /// Получить ключ навигатора для формирования кеша в разрезе таблицы.
        /// </summary>
        /// <returns></returns>
        protected string CacheKey;

        /// <summary>
        /// Достаточность установления фильтра этого навигатора. 
        /// Если true и фильтр данных этого навигатора установлен, то рекурсия вверх по иерархии не подымается.
        /// </summary>
        public virtual bool FilterDataSeterEnough
        {
            get { return true; }
        }

        public virtual bool DisableHtmlAttributeEncode
        {
            get { return false; }
        }

        public BaseNavigatorInfo()
        {
            ParentNavigators = new List<BaseNavigatorInfo>();
            ChildsNavigators = new List<BaseNavigatorInfo>();
        }

        /// <summary>
        /// Список связей к родительским навигаторам.
        /// </summary>
        public List<BaseNavigatorInfo> ParentNavigators { get; private set; }

        /// <summary>
        /// Список связей к родительским навигаторам.
        /// </summary>
        public List<BaseNavigatorInfo> ChildsNavigators { get; private set; }

        /// <summary>
        /// Тип таблицы.
        /// </summary>
        public abstract Type TableType { get; }

        /// <summary>
        /// Тип объекта предоставляющего данные
        /// </summary>
        public abstract Type DataSourceViewType { get; }

        /// <summary>
        /// Название таблицы.
        /// </summary>
        public abstract string TableName { get; }

        /// <summary>
        /// Заголовок таблицы.
        /// </summary>
        public abstract string Header { get; }

        /// <summary>
        /// Тескт ссылки на дочки.
        /// </summary>
        public abstract string HeaderToChilds { get; }

        /// <summary>
        /// Текст ссылки на родителя.
        /// </summary>
        public abstract string HeaderToParent { get; }

        public virtual string GetHeaderToParent(string rowKey)
        {
            return HeaderToParent;
        }

        /// <summary>
        /// Связь по которой производится переход от дочки к родителю.
        /// </summary>
        public abstract string ReferenceName { get; }

        public virtual string CustomNavigatorUserControl { get { return null; } }
        public virtual Type CustomNavigatorClassName { get { return null; } }

        public virtual bool HideIfEmpty { get { return false; } }

        /// <summary>
        /// Связь между записями 1-1
        /// </summary>
        public virtual bool IsOneToOne
        {
            get { return false; }
        }

        protected QueryParameters QueryParameters { get; set; }

        public void InitQueryParameters(QueryParameters qParams)
        {
            QueryParameters = qParams;
        }

        /// <summary>
        /// Получить вырожение для фильтрации данных по текущей ссылке.
        /// </summary>
        /// <param name="url">Текущая ссылка.</param>
        /// <param name="exp">Вырожение предоставляющее объект строки, типа таблицы навигатора.</param>
        /// <param name="referenceKey">Ключ ссылки в строке браузера.</param>
        /// <returns>Возвращет вырожение в виде результата boolean, если нет необходимости в фильтрации, то возвращается null.</returns>
        public virtual Expression FilterData(MainPageUrlBuilder url, Expression exp, string referenceKey)
        {
            var value = GetKeyValue(url.QueryParameters, referenceKey, false);
            if (string.IsNullOrEmpty(value)) return null;
            var key = (long)DataSourceViewGetName.GetKey(value);
            if (QueryParameters == null)
                return Expression.Equal(Expression.Property(exp, "id"), Expression.Constant(key));
            var parameterKey = string.IsNullOrEmpty(referenceKey) ? TableNameKey : referenceKey + ".id.Equals";
            return Expression.Equal(Expression.Property(exp, "id"), QueryParameters.GetExpression(parameterKey, key));
        }

        /// <summary>
        /// Метод для получения вырожения с текущим объектом типа таблицы навигатора.
        /// </summary>
        /// <param name="exp">Вырожение объекта дочерней таблицы</param>
        /// <returns></returns>
        public virtual Expression MoveToParent(Expression exp)
        {
            return Expression.Property(exp, ReferenceProperty);
        }

        /// <summary>
        /// Свойство для перехода по связи.
        /// </summary>
        public PropertyInfo ReferenceProperty 
        {
            get 
            {
                if (_referenceProperty == null)
                {
                    _referenceProperty = TableType.GetProperty(ReferenceName);
                    if (_referenceProperty == null)
                        throw new Exception("Not found reference '" + ReferenceName + "' in table '" + TableName + "'");
                }
                return _referenceProperty;
            }
        }

        /// <summary>
        /// Получить может ли создаться ссылка на родителя
        /// </summary>
        /// <returns></returns>
        public bool GetVisibleToParent(IDictionary<string, string> values, string referenceKey, BaseNavigatorValues navigatorValues)
        {
            var args = GetVisibleArgs(values, referenceKey, navigatorValues);
            return args.Visible;
        }

        /// <summary>
        /// Получить может ли создаться ссылка на родителя
        /// </summary>
        /// <returns></returns>
        public bool GetFilterByParent(IDictionary<string, string> values, string referenceKey, BaseNavigatorValues navigatorValues)
        {
            var args = GetVisibleArgs(values, referenceKey, navigatorValues);
            return args.Visible && args.FilterByParent;
        }

        /// <summary>
        /// Может ли создаться ссылка на дочку
        /// </summary>
        /// <returns></returns>
        public bool GetVisibleToChilds(IDictionary<string, string> values, string referenceKey, BaseNavigatorValues navigatorValues)
        {
            var args = GetVisibleArgs(values, referenceKey, navigatorValues);
            return args.Visible && (args.VisibleToJournalButton || (args.VisibleLookButton && IsOneToOne) || args.VisibleAddButton);
        }

        /// <summary>
        /// Получить аргументы для определения видимости ссылок.
        /// </summary>
        /// <returns></returns>
        protected BaseNavigatorInfoVisibleEventArgs GetVisibleArgs(IDictionary<string, string> values, string referenceKey, BaseNavigatorValues navigatorValues)
        {
            var keyCache = ReferenceCacheKey + ".Args." + referenceKey;
            BaseNavigatorInfoVisibleEventArgs args = null;
            if (HttpContext.Current != null)
                args = HttpContext.Current.Items[keyCache] as BaseNavigatorInfoVisibleEventArgs;
            if (args != null) return args;
            args = new BaseNavigatorInfoVisibleEventArgs
                       {
                           SelectedValue = GetKeyValue(values, referenceKey, false),
                           Values = navigatorValues
                       };
            InitializeNavigatorInfoVisible(args);
            if (HttpContext.Current != null)
                HttpContext.Current.Items[keyCache] = args;
            return args;
        }

        protected virtual void InitializeNavigatorInfoVisible(BaseNavigatorInfoVisibleEventArgs args)
        {
            if (HideIfEmpty && args.SelectedValue == null)
                args.Visible = false;
        }

        /// <summary>
        /// Удалить кеш сформированные в текущем запросе.
        /// </summary>
        public void ClearRequestCache()
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items.Remove(ReferenceCacheKey + ".Args");
        }

        /// <summary>
        /// Получить ссылки на родительскую таблицу.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="url"></param>
        /// <param name="urlClone"></param>
        /// <param name="referenceKey"></param>
        /// <param name="navigatorControl"></param>
        /// <returns></returns>
        public bool GetNavigatorToParent(HtmlTextWriter writer, MainPageUrlBuilder url, MainPageUrlBuilder urlClone, string referenceKey, BaseNavigatorControl navigatorControl)
        {
            var args = GetVisibleArgs(url.QueryParameters, referenceKey, navigatorControl.BaseNavigatorValues);
            if (!args.VisibleLookButton && !args.VisibleLookButton)
                return false;

            var value = GetKeyValue(url.QueryParameters, referenceKey, false);
            IDictionary<string, string> oldParameters;
            if (value != null && !string.IsNullOrEmpty(CustomNavigatorUserControl))
            {
                var control = navigatorControl.NavigatorCustomControls[this];
                control.SetValue(value);
                control.BaseNavigatorValues = navigatorControl.BaseNavigatorValues;

                oldParameters = MoveQueryParametersToUp(urlClone, referenceKey);
                control.LookUrl = GetLookUrl(urlClone);
                control.JournalUrl = GetJournalUrl(urlClone);
                urlClone.QueryParameters = oldParameters;

                //navigatorControl.AddControl(control);
                control.Initialize();
                control.RenderControl(writer);
                return true;
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "font13");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.RenderBeginTag(HtmlTextWriterTag.B);
            writer.Write(HttpUtility.HtmlAttributeEncode(GetHeaderToParent(value)));

            string name;
            if (!string.IsNullOrEmpty(value) && (name = GetRowName(value)) != null)
            {
                writer.Write(": ");
                writer.RenderEndTag(); //B
                if (DisableHtmlAttributeEncode)
                    writer.Write(name);
                else
                    writer.Write(HttpUtility.HtmlAttributeEncode(name));
            }
            else
            {
                writer.RenderEndTag(); //B
                args.VisibleLookButton = false;
            }
            writer.Write("&nbsp;<b>&gt;</b>");
            oldParameters = MoveQueryParametersToUp(urlClone, referenceKey);
            if (args.VisibleLookButton)
            {
                writer.Write("&nbsp;");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, GetLookUrl(urlClone));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(Resources.SView);
                writer.RenderEndTag();
                
                if (args.VisibleToJournalButton)
                    writer.Write("&nbsp;-");
            }
            if (args.VisibleToJournalButton)
            {
                writer.Write("&nbsp;");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, GetJournalUrl(urlClone));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(Resources.SToJournal);
                writer.RenderEndTag();
            }
            urlClone.QueryParameters = oldParameters;
            writer.RenderEndTag(); //span
            return true;
        }

        /// <summary>
        /// Получить выбранное значение таблицы навигатора
        /// </summary>
        /// <param name="values"></param>
        /// <param name="referenceKey"></param>
        /// <param name="allowGetTableKey"></param>
        /// <returns></returns>
        public virtual string GetKeyValue(IDictionary<string, string> values, string referenceKey, bool allowGetTableKey)
        {
            if (string.IsNullOrEmpty(referenceKey))
            {
                if (!values.ContainsKey(TableNameKey)) return null;
                return values[TableNameKey];
            }
            referenceKey += ".id";
            if (!values.ContainsKey(referenceKey))
                return allowGetTableKey && values.ContainsKey(TableNameKey) ? values[TableNameKey] : null;
            return values[referenceKey];
        }

        /// <summary>
        /// Получить неименование строки.
        /// </summary>
        /// <param name="key">Ключ записи</param>
        /// <returns></returns>
        public virtual string GetRowName(string key)
        {
            var cacheKey = CacheKey + ".Name:" + key;
            string name = null;
            lock (DataSourceViewGetName)
            {
                if (HttpContext.Current != null)
                    name = (string)HttpContext.Current.Cache[cacheKey];

                if (string.IsNullOrEmpty(name))
                {
                var view = DataSourceViewGetName;
                lock (view)
                    name = view.GetName(key);
                    if (name == null) return null;
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Cache.Add(
                            cacheKey,
                            name,
                            null,
                            DateTime.Now.AddMinutes(1),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Default,
                            null);
                    }
                }
            }

            return name;
        }

        public object GetKeyValue(MainPageUrlBuilder url)
        {
            var value = GetKeyValue(url.QueryParameters, null, true);
            if (string.IsNullOrEmpty(value)) return null;
            return DataSourceViewGetName.GetKey(value);
        }

        public object GetKeyValue(MainPageUrlBuilder url, string referenceKey, bool allowGetTableKey)
        {
            var value = GetKeyValue(url.QueryParameters, referenceKey, allowGetTableKey);
            if (string.IsNullOrEmpty(value)) return null;
            return DataSourceViewGetName.GetKey(value);
        }

        public static string GetRowName(object key, Type tableType)
        {
            var cacheKey = GetCacheKey(tableType) + ".Name:" + key;
            string name = null;
            if (HttpContext.Current != null)
                name = (string)HttpContext.Current.Cache[cacheKey];
            if (string.IsNullOrEmpty(name))
            {
                var view = GetDataSourceViewGetName(tableType);
                lock (view)
                    name = view.GetName(key);
                if (name == null) return null;
                if (HttpContext.Current != null)
                    HttpContext.Current.Cache[cacheKey] = name;
            }
            return name;
        }

        public static string GetRowCode(object key, Type tableType)
        {
            var cacheKey = GetCacheKey(tableType) + ".Code:" + key;
            string code = null;
            if (HttpContext.Current != null)
                code = (string)HttpContext.Current.Cache[cacheKey];
            if (string.IsNullOrEmpty(code))
            {
                var view = GetDataSourceViewGetName(tableType);
                lock (view)
                    code = view.GetCode(key);
                if (code != null && HttpContext.Current != null)
                    HttpContext.Current.Cache[cacheKey] = code;
            }
            return code;
        }

        protected internal IDataSourceViewGetName DataSourceViewGetName
        {
            get
            {
                return (IDataSourceViewGetName)DataSource;
            }
        }

        protected internal object DataSource
        {
            get
            {
                if (_cacheDataSource.ContainsKey(TableType))
                    return _cacheDataSource[TableType];
                lock (_cacheDataSource)
                    return _cacheDataSource[TableType] = Activator.CreateInstance(DataSourceViewType);
            }
        }

        protected string GetLookUrl(MainPageUrlBuilder urlClone)
        {
            urlClone.UserControl = TableName + "Edit";
            urlClone.IsRead = true;
            return urlClone.CreateUrl();
        }

        protected internal string GetJournalUrl(MainPageUrlBuilder urlClone)
        {
            urlClone.UserControl = TableName + "Journal";
            urlClone.IsRead = false;
            return urlClone.CreateUrl();
        }

        public static BaseNavigatorInfo GetNavigator(Type type)
        {
            BaseNavigatorInfo navigator;
            bool isNewNavigator = false;
            lock (_cacheNavigators)
            {
                if (!_cacheNavigators.ContainsKey(type))
                {
                    _cacheNavigators[type] = navigator = (BaseNavigatorInfo)Activator.CreateInstance(type);
                    isNewNavigator = true;
                }
                else
                    navigator = _cacheNavigators[type];
            }
            if (isNewNavigator) navigator.Initialize();
            return navigator;
        }

        public virtual void Initialize()
        {
            BaseInitialize();
        }

        public virtual void BaseInitialize()
        {
            TableNameKey = "ref" + TableName;
            TableNameHistoryKey = "ref" + TableName + "History";
            TableNameParentKey = "ref" + TableName + "Parent";
            ReferenceCacheKey = "BaseNavigatorInfo." + TableType.Name + "." + ReferenceName;
            CacheKey = GetCacheKey(TableType);
        }

        public static string GetCacheKey(Type tableType)
        {
            return "BaseNavigatorInfo." + tableType.Name;
        }

        public static IDataSourceViewGetName GetDataSourceViewGetName(Type tableType)
        {
            //Предпологаем что этот метод дергается только после того когда уже по типу таблици датасоурс есть в кеше
            return (IDataSourceViewGetName)_cacheDataSource[tableType];
        }

        /// <summary>
        /// Изменить наименования ссылок на родителей, режется по переданной ссылке.
        /// </summary>
        /// <param name="urlClone"></param>
        /// <param name="referenceKey"></param>
        /// <returns></returns>
        protected internal IDictionary<string, string> MoveQueryParametersToUp(MainPageUrlBuilder urlClone, string referenceKey)
        {
            var oldParameters = urlClone.QueryParameters;
            var newParams = new Dictionary<string, string>(oldParameters.Count);
            if (referenceKey == "")
            {
                foreach (var param in oldParameters)
                    newParams[param.Key] = param.Value;                
            }
            else
            {
                var thisKey = referenceKey + ".id";
                referenceKey += ".";
                var index = referenceKey.Length;
                foreach (var param in oldParameters)
                {
                    if (thisKey.Equals(param.Key, StringComparison.OrdinalIgnoreCase))
                        newParams[TableNameKey] = param.Value;
                    else if (param.Key.StartsWith(referenceKey))
                        newParams[param.Key.Substring(index)] = param.Value;
                    else if (param.Key.Equals(TableNameKey))
                    {
                        if (!newParams.ContainsKey(param.Key) && !string.IsNullOrEmpty(param.Value))
                            newParams[param.Key] = param.Value;
                    }
                    else if (!param.Key.Contains('.'))
                        newParams[param.Key] = param.Value;
                }
            }
            urlClone.QueryParameters = newParams;
            return oldParameters;
        }

        public virtual bool RenderLogInformation(StringBuilder sb, IDictionary<string, string> queryParameters, string referenceKey, bool allowGetTableKey)
        {
            string value = GetKeyValue(queryParameters, referenceKey, allowGetTableKey);
            string name;
            if (!string.IsNullOrEmpty(value))
            {
                name = GetRowName(value);
                sb.Append(HttpUtility.HtmlAttributeEncode(GetHeaderToParent(value)));
                sb.Append(": ");
                sb.Append(value);
                sb.Append(", ");
                sb.Append(HttpUtility.HtmlAttributeEncode(name ?? "[" + Resources.ENotExistsRowOrName + "]"));
                return true;
            }
            return false;
        }

        public MainPageUrlBuilder GetUrlToParentJournal(MainPageUrlBuilder url, MainPageUrlBuilder urlClone, string referenceKey, object values)
        {
            string value = GetKeyValue(url.QueryParameters, referenceKey, false);
            MoveQueryParametersToUp(urlClone, referenceKey);
            GetJournalUrl(urlClone);
            return urlClone;
        }

        public virtual string GetUrlForNavigateTo(string id, string destinationTable)
        {
            return null;
        }
    }
}
