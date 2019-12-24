using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Script.Serialization;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.Filter;
using System.Web.Compilation;
using Nat.Web.Controls.GenerationClasses.Navigator;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using System.Web;
using Nat.Web.Tools.WorkFlow;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.UI;

    using Nat.Tools.Filtering;

    public abstract class BaseFilterControl<TKey> : BaseHeaderControl, IBaseFilterControl, IFilterControl where TKey : struct
    {
        private MainPageUrlBuilder _url;
        private IWorkFlow[] _workFlows;
        private Control postBackFilterControl;
        private string postBackFilterArguments;

        protected BaseFilterControl()
        {
            Errors = new List<string>();
        }

        public event EventHandler FilterApply;
        
        public virtual IQueryable GetWhereForParentFilter(string parent, IQueryable query)
        {
            throw new NotSupportedException();
        }

        public abstract IQueryable SetFilters(IQueryable query);

        public virtual IQueryable SetFilters(IQueryable query, Type tsourceType, Expression upToTable)
        {
            return (IQueryable)GetType().GetMethod("ExecFilterData").MakeGenericMethod(tsourceType).Invoke(this, new object[] { query, upToTable });
        }

        public virtual IQueryable ExecFilterData<TSource>(IQueryable query, Expression upToTable) 
            where TSource : class
        {
            throw new NotImplementedException();
        }

        public abstract TKey? SelectedID { get; set; }
        public abstract void SetDB(DataContext db);

        protected IWorkFlow[] WorkFlows
        {
            get { return _workFlows ?? (_workFlows = CreateWorkFlows()); }
        }

        public Control PostBackFilterControl
        {
            get { return postBackFilterControl ?? this; }
            set { postBackFilterControl = value; }
        }

        public string PostBackFilterArguments
        {
            get { return postBackFilterArguments ?? string.Empty; }
            set { postBackFilterArguments = value; }
        }

        public List<string> Errors { get; set; }

        protected virtual IWorkFlow[] CreateWorkFlows()
        {
            return new IWorkFlow[0];
        }

        protected virtual void OnFilterApply(EventArgs args)
        {
            if (FilterApply != null) FilterApply(this, args);
        }

        public virtual void SetUrl(MainPageUrlBuilder url)
        {
            _url = url;
        }
        
        public MainPageUrlBuilder Url
        {
            get 
            {
                if (_url == null) _url = MainPageUrlBuilder.Current;
                return _url;
            }
        }

        public List<string> RequiredFilters { get; set; }

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

        private bool? _HasAjax;

        public bool HasAjax
        {
            get
            {
                if (_HasAjax == null)
                {
                    var updatePanel = ControlHelper.FindControl<UpdatePanel>(this);
                    _HasAjax = ScriptManager != null && updatePanel != null;
                }
                return _HasAjax.Value;
            }
        }

        protected bool ShowFilterGroup(IList<FilterHtmlGenerator.Filter> filters, string filterName)
        {
            var row = filters.FirstOrDefault(r => r.FilterName == filterName);
            if (row != null)
                return true;

            foreach (var group in filters.Where(r => r.Type == FilterHtmlGenerator.FilterType.Group))
            {
                if (this.ShowFilterGroup(group.Children, filterName))
                {
                    group.DefaultGroupExpanded = true;
                    return true;
                }
            }

            return false;
        }

        protected static void MoveFilterAfter(IList<FilterHtmlGenerator.Filter> filters, string filterName, string filterNameAfter)
        {
            var search = new SearchFilter(filters);
            if (!search.Find(filterName))
                throw new ArgumentOutOfRangeException("filterName", @"Не найден фильтр с заданным значением " + filterName);

            var searchAfter = new SearchFilter(filters);
            if (!searchAfter.Find(filterNameAfter))
                throw new ArgumentOutOfRangeException("filterNameAfter", @"Не найден фильтр с заданным значением " + filterNameAfter);

            search.FiltersCollection.Remove(search.Filter);
            searchAfter.FiltersCollection.Insert(searchAfter.FilterIndex + 1, search.Filter);
        }

        protected static void MoveFilterBefore(IList<FilterHtmlGenerator.Filter> filters, string filterName, string filterNameAfter)
        {
            var search = new SearchFilter(filters);
            if (!search.Find(filterName))
                throw new ArgumentOutOfRangeException("filterName", @"Не найден фильтр с заданным значением " + filterName);

            var searchAfter = new SearchFilter(filters);
            if (!searchAfter.Find(filterNameAfter))
                throw new ArgumentOutOfRangeException("filterNameAfter", @"Не найден фильтр с заданным значением " + filterNameAfter);

            search.FiltersCollection.Remove(search.Filter);
            searchAfter.FiltersCollection.Insert(searchAfter.FilterIndex, search.Filter);
        }

        protected void SetDefaultFilterType(
            IList<FilterHtmlGenerator.Filter> filters,            
            Enum defaultFilterType,
            params string[] filterNames)
        {
            var filterObjects = filters.Union(filters.SelectMany(f => f.AllChildren))
                .Where(r => filterNames.Contains(r.FilterName));
            foreach (var filterObject in filterObjects)
            {
                filterObject.DefaultFilterType = defaultFilterType;
            }
        }

        protected void SetDefaultFilterTypeExclude(
            IList<FilterHtmlGenerator.Filter> filters,
            Enum defaultFilterType,
            params string[] filterNames)
        {
            var filterObjects = filters.Union(filters.SelectMany(f => f.AllChildren))
                .Where(r => !filterNames.Contains(r.FilterName));
            foreach (var filterObject in filterObjects)
            {
                filterObject.DefaultFilterType = defaultFilterType;                
            }
        }

        protected void SetDefaultFilterType(IList<FilterHtmlGenerator.Filter> filters)
        {
            InitializeDefaultFilterType(filters);
        }

        public static void InitializeDefaultFilterType(IList<FilterHtmlGenerator.Filter> filters)
        {
            var filterObjects =
                filters.Union(filters.SelectMany(f => f.AllChildren)).Where(
                    r => r.Type != FilterHtmlGenerator.FilterType.Group);
            foreach (var filterObject in filterObjects)
            {
                switch (filterObject.Type)
                {
                    case FilterHtmlGenerator.FilterType.Numeric:
                        filterObject.DefaultFilterType = DefaultFilters.NumericFilter.Equals;
                        break;
                    case FilterHtmlGenerator.FilterType.Reference:
                        filterObject.DefaultFilterType = filterObject.FilterByStartsWithCode ? DefaultFilters.ReferenceFilter.StartsWithCode : DefaultFilters.ReferenceFilter.Equals;
                        break;
                    case FilterHtmlGenerator.FilterType.Text:
                        filterObject.DefaultFilterType = DefaultFilters.TextFilter.ContainsAnyWord;
                        break;
                    case FilterHtmlGenerator.FilterType.FullTextSearch:
                        filterObject.DefaultFilterType = DefaultFilters.FullTextSearchFilter.Contains;
                        break;
                }
            }
        }

        public virtual string GetTableName()
        {
            return null;
        }
    }
    
    public abstract class BaseFilterControl<TKey, TTable> : BaseFilterControl<TKey> 
        where TKey : struct
        where TTable : class
    {
        private Dictionary<string, int> _selectedValues;
        private string _SelectedValuesString;

        protected BaseFilterControl()
        {
            UseHistoryFilter = true;
            FilterExpressions = new List<Expression<Func<TTable, bool>>>();
            CancelTreeUseExceptionsInternal = new[] { "id.NotEqualsCollection" };
        }

        public virtual bool HasHistory { get { return false; } }
        public virtual bool UseHistoryFilter { get; set; }
        public bool CancelTreeUse { get; protected set; }
        public string[] CancelTreeUseExceptions { get; set; }
        public string[] CancelTreeUseExceptionsInternal { get; private set; }
        public virtual string SelectedValues 
        {
            get
            {
                return _SelectedValuesString ?? Url.SelectedValues;
            }
            set
            {
                _SelectedValuesString = value;
            }
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
                foreach (var item in (SelectedValues ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    _selectedValues[item] = 1;
            }
        }

        protected virtual void SetSelectedValue(object value)
        {
            if (value != null)
            {
                EnsureSelectedValuesCreated();
                _selectedValues[value.ToString()] = 1;
            }
        }

        protected virtual void SetSelectedValue(object[] values)
        {
            if (values != null && values.Length > 0)
            {
                EnsureSelectedValuesCreated();
                foreach (var value in values)
                    _selectedValues[value.ToString()] = 1;
            }
        }

        /*
        private Dictionary<Type, Dictionary<string, int>> _selectedValues;
        protected bool IsSelectedValue<T>(T value)
        {
            if (SelectedValues == null || value == null) return false;
            if (_selectedValues == null) _selectedValues = new Dictionary<Type, Dictionary<string, int>>();
            Dictionary<string, int> dic;
            if (!_selectedValues.ContainsKey(typeof(T)))
            {
                dic = _selectedValues[typeof(T)] = new Dictionary<string, int>();
                foreach(var item in SelectedValues.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries))
                    dic[Convert.ChangeType(item, typeof(T)).ToString()] = 1;
            }
            dic = _selectedValues[typeof(T)];
            return dic.ContainsKey(value.ToString());
        }*/
       

        private BaseFilter<TKey, TTable> _customFilter;
        public virtual BaseFilter<TKey, TTable> CustomFilter
        {
            get
            {
                if (_customFilter == null && !string.IsNullOrEmpty(Url.CustomFilterClassName))
                {
                    var type = BuildManager.GetType(Url.CustomFilterClassName, false);
                    if (type != null)
                        CustomFilter = Activator.CreateInstance(type, null) as BaseFilter<TKey, TTable>;
                }
                return _customFilter;
            }
            set
            {
                var notEquals = _customFilter != value;
                _customFilter = value;
                if (_customFilter != null && notEquals)
                {
                    if (savedUserFilterValues != null)
                    {
                        savedUserFilterValues.TableName = _customFilter.TableName;
                        savedUserFilterValues.FilterControlName = "filter" + GetTableName();
                    }

                    if (value.RequiredFilters != null)
                    {
                        if (RequiredFilters == null)
                            RequiredFilters = value.RequiredFilters.ToList();
                        else if (value.RequiredFilters != null)
                            RequiredFilters.AddRange(value.RequiredFilters.Except(RequiredFilters));
                    }
                }
            }
        }

        internal protected QueryParameters QueryParameters { get; set; }

        public virtual void EnsureFilterInitialize()
        {
        }

        public Expression FilterData(Expression source, QueryParameters qParams, params Expression[] fieldsToCheckReference)
        {
            return FilterData(source, qParams, (IEnumerable<Expression>)fieldsToCheckReference);
        }

        public Expression FilterData(Expression source, QueryParameters qParams, bool filterByParents, params Expression[] fieldsToCheckReference)
        {
            return FilterData<TTable>(source, qParams, null, null, fieldsToCheckReference, filterByParents);
        }

        public Expression FilterData(Expression source, params Expression[] fieldsToCheckReference)
        {
            return FilterData(source, (IEnumerable<Expression>)fieldsToCheckReference);
        }

        public Expression FilterData(Expression source, QueryParameters qParams, IEnumerable<Expression> fieldsToCheckReference)
        {
            return FilterData<TTable>(source, qParams, null, null, fieldsToCheckReference, true);
        }

        public Expression FilterData(Expression source, IEnumerable<Expression> fieldsToCheckReference)
        {
            return FilterData<TTable>(source, null, null, fieldsToCheckReference, true);
        }
        
        public Expression FilterData<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference, bool filterByParents)
            where T : class
        {
            return FilterData<T>(source, null, upToTable, param, fieldsToCheckReference, filterByParents);
        }

        protected bool DontSkipIsFiltered { get; set; }

        public Expression FilterData<T>(Expression source, QueryParameters qParams, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference, bool filterByParents)
            where T : class
        {
            QueryParameters = qParams ?? GetNullQueryParameters();
            var filterArgs = GetFilter(typeof(T));
            if (!DontSkipIsFiltered && (QueryParameters?.IsFiltered(filterArgs.GetTTable()) ?? false))
                return source;
            QueryParameters?.Filtered(filterArgs.GetTTable());

            foreach (var workFlow in WorkFlows)
            {
                workFlow.Url = Url;
                workFlow.SetFilters(filterArgs);
            }

            source = filterArgs.FilterData<T>(source, upToTable, param, fieldsToCheckReference);
            if (filterArgs.CancelTreeUse) CancelTreeUse = true;

            if (UseHistoryFilter)
                source = FilterDataByHistory<T>(source, upToTable, param, fieldsToCheckReference);
            source = FilterDataByFilterValues<T>(source, upToTable, param, fieldsToCheckReference);
            if (filterByParents)
            {
                ParameterExpression paramT = param;
                if (paramT == null)
                {
                    paramT = Expression.Parameter(typeof(TTable), "t");
                    upToTable = paramT;
                }
                source = FilterDataByParents<T>(source, paramT, upToTable, fieldsToCheckReference);
            }
            return source;
        }

        protected virtual QueryParameters GetNullQueryParameters()
        {
            return null;
        }

        protected Expression FilterDataByFilterValues<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            if (param == null)
                param = Expression.Parameter(typeof(T), "c");
            var thisTableExp = upToTable ?? param;

            var filterItems = GetFilterItems();
            var filterHandlers = GetFilterHandlers();
            Expression filter = null;
            if (filterItems == null)
            {
                var filterValues = GetFilterValues();
                if (filterValues != null && filterValues.Count > 0)
                    filter = GetExpressionByFilterValues(thisTableExp, param, filterValues, filterHandlers);
                else if (RequiredFilters != null && RequiredFilters.Count > 0)
                {
                    filter = Expression.Constant(false);
                    if (QueryParameters != null)
                        QueryParameters.AddMessage(Resources.SRequiredFilters);
                }
            }
            else
                filter = GetExpressionByFilterValues(thisTableExp, param, filterItems, filterHandlers);
            if (filter != null)
            {
                filter = fieldsToCheckReference.
                    Where(item => LinqFilterGenerator.IsNullableType(item.Type)).
                    Aggregate(filter, (current, item) =>
                                      Expression.Or(current, Expression.Equal(item, Expression.Constant(null))));
                Expression pred = Expression.Lambda(filter, param);
                source = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source, pred);
            }
            return source;
        }

        protected virtual Dictionary<string, string[]> GetFilterValues()
        {
            return null;
        }

        protected virtual Dictionary<string, List<FilterItem>> GetFilterItems()
        {
            return null;
        }

        internal Dictionary<string, List<FilterItem>> GetFilterItemsIternal()
        {
            return GetFilterItems();
        }

        internal protected virtual IDictionary<string, FilterHtmlGenerator.Filter> GetFilterHandlers()
        {
            return new Dictionary<string, FilterHtmlGenerator.Filter>();
        }

        protected Expression GetExpressionByFilterValues(Expression upToTable, ParameterExpression param, Dictionary<string, string[]> filterValues, IDictionary<string, FilterHtmlGenerator.Filter> filterHandlers)
        {
            if (upToTable == null) upToTable = param;
            Expression where = null;

            foreach (var filterValue in filterValues)
            {
                if (filterValue.Key.StartsWith("__") && filterValue.Value.Length == 1)
                    continue;
                Expression exp = null;
                if (filterHandlers.ContainsKey(filterValue.Key))
                {
                    var filterHandler = filterHandlers[filterValue.Key];
                    if (filterHandler.ExpressionFilterHandler != null)
                        exp = filterHandler.ExpressionFilterHandler(filterHandler.ParseEnum(filterValue.Value[0]), filterValue.Value[1], filterValue.Value[2], QueryParameters);
                    else if (filterHandler.FilterName == MultipleSelectedValues && typeof(TKey) == typeof(long))
                    {
                        if (string.IsNullOrEmpty(Url.SelectKeyValueColumn))
                            exp = LinqFilterGenerator.GenerateFilter(upToTable.Type, "Equals", "id", SelectedValues, null, QueryParameters);
                        else
                            exp = LinqFilterGenerator.GenerateFilter(upToTable.Type, "EqualsCollection", Url.SelectKeyValueColumn, SelectedValues, null, QueryParameters);
                        if (exp != null && ((CancelTreeUseExceptions == null || !CancelTreeUseExceptions.Contains(filterValue.Key)) && !CancelTreeUseExceptionsInternal.Contains(filterValue.Key)))
                            CancelTreeUse = true;
                    }
                }
                else
                {
                    exp = LinqFilterGenerator.GenerateFilter(upToTable.Type, filterValue.Value[0], filterValue.Key, filterValue.Value[1], filterValue.Value[2], QueryParameters);
                    if ((CancelTreeUseExceptions == null || !CancelTreeUseExceptions.Contains(filterValue.Key)) && !CancelTreeUseExceptionsInternal.Contains(filterValue.Key))
                        CancelTreeUse = true;
                }
                if (exp != null)
                {
                    exp = Expression.Invoke(exp, upToTable);
                    where = where == null ? exp : Expression.And(where, exp);
                }
            }

            return where;
        }

        protected Expression GetExpressionByFilterValues(Expression upToTable, ParameterExpression param, Dictionary<string, List<FilterItem>> filterValues, IDictionary<string, FilterHtmlGenerator.Filter> filterHandlers)
        {
            if (upToTable == null) upToTable = param;
            Expression where = null;

            foreach (var filter in filterHandlers.Values.OfType<BaseFilterParameter>())
                filter.ClearState();

            var requiredFiltersSeted = new Dictionary<string, bool>();
            if (RequiredFilters != null)
                foreach (var filter in RequiredFilters)
                    requiredFiltersSeted[filter] = false;

            foreach (var items in filterValues.Values)
            {
                Expression innerWhere = null;
                Expression innerWhere2 = null;
                foreach (var filterItem in items)
                {
                    var filterName = LinqFilterGenerator.GetFilterName(filterItem.FilterName);
                    if (string.IsNullOrEmpty(filterName) || filterName.StartsWith("."))
                        continue;
                    //todo: что это?
                    if (filterName.StartsWith("__")/* && filterValue.Value.Length == 1*/)
                        continue;
                    Expression exp = null;
                    exp = GetExpressionByFilterItem(upToTable, requiredFiltersSeted, filterItem, filterItem.FilterName, filterName, filterHandlers, exp);
                    if (exp != null)
                    {
                        exp = Expression.Invoke(exp, upToTable);
                        //QueryParameters.RegisterExpression(exp, "FilterValues");
                        if (filterItem.FilterType.StartsWith("NotEqual", StringComparison.OrdinalIgnoreCase)
                            || filterItem.FilterType.StartsWith("NotContains", StringComparison.OrdinalIgnoreCase)
                            || filterItem.FilterType.StartsWith("NotContainsWords", StringComparison.OrdinalIgnoreCase)
                            || filterItem.FilterType.Equals("LengthMore", StringComparison.OrdinalIgnoreCase)
                            || filterItem.FilterType.Equals("LengthLess", StringComparison.OrdinalIgnoreCase))
                        {
                            where = where == null ? exp : Expression.And(where, exp);
                        }
                        else if (filterItem.FilterType.Equals("DaysAgoAndMore", StringComparison.OrdinalIgnoreCase)
                                 || filterItem.FilterType.Equals("DaysLeftAndMore", StringComparison.OrdinalIgnoreCase))
                        {
                            innerWhere2 = innerWhere2 == null ? exp : Expression.And(innerWhere2, exp);
                        }
                        else
                            innerWhere = innerWhere == null ? exp : Expression.Or(innerWhere, exp);
                    }
                }

                if (innerWhere2 != null)
                    innerWhere = innerWhere == null ? innerWhere2 : Expression.Or(innerWhere, innerWhere2);

                if (innerWhere != null)
                    where = where == null ? innerWhere : Expression.And(where, innerWhere);
            }

            where = filterHandlers.Values.OfType<IBaseFilterParameterContainer>()
                .Select(filter =>
                    {
                        var expression = filter.GetFilter(QueryParameters);
                        if (expression != null)
                        {
                            foreach (var containerFilter in filter.GetAppliedFilters().Select(r => r.FilterName).Where(requiredFiltersSeted.ContainsKey))
                                requiredFiltersSeted[containerFilter] = true;
                        }

                        return expression;
                    })
                .Where(exp => exp != null)
                .Select(exp => (Expression)Expression.Invoke(exp, upToTable))
                .Aggregate(where, (current, exp) => current == null ? exp : Expression.And(current, exp));

            if (requiredFiltersSeted.Values.Contains(false))
            {
                where = Expression.Constant(false);
                if (QueryParameters != null)
                    QueryParameters.AddMessage(Resources.SRequiredFilters);
            }

            return where;
        }

        private Expression GetExpressionByFilterItem(Expression upToTable, Dictionary<string, bool> requiredFiltersSeted, FilterItem filterItem, string filterName, string filterNameOriginal, IDictionary<string, FilterHtmlGenerator.Filter> filterHandlers, Expression exp)
        {
            if (filterHandlers.ContainsKey(filterName) || filterHandlers.ContainsKey(filterNameOriginal))
            {
                var filterHandler = filterHandlers.ContainsKey(filterName)
                                        ? filterHandlers[filterName]
                                        : filterHandlers[filterNameOriginal];
                if (filterHandler.ExpressionFilterHandlerV2 != null)
                {
                    try
                    {
                        exp = filterHandler.ExpressionFilterHandlerV2(
                            filterHandler.ParseEnum(filterItem.FilterType),
                            filterItem,
                            QueryParameters);
                    }
                    catch (FormatException e)
                    {
                        if (filterHandler.IsDateTime)
                            Errors.Add(string.Format(Resources.SInvalidDateFormatInFilter, filterHandler.Header));
                        else if (filterHandler.Type == FilterHtmlGenerator.FilterType.Numeric)
                            Errors.Add(string.Format(Resources.SInvalidNumericFormatInFilter, filterHandler.Header));
                        else
                            Errors.Add(string.Format(Resources.SInvalidFormatInFilter, filterHandler.Header));
                        return null;
                    }
                }
                else if (filterHandler.ExpressionFilterHandler != null)
                {
                    exp = filterHandler.ExpressionFilterHandler(
                        filterHandler.ParseEnum(filterItem.FilterType),
                        filterItem.Value1,
                        filterItem.Value2,
                        QueryParameters);
                }
                else if (filterHandler.FilterName == MultipleSelectedValues && typeof(TKey) == typeof(long))
                {
                    if (string.IsNullOrEmpty(Url.SelectKeyValueColumn))
                    {
                        exp = LinqFilterGenerator.GenerateFilter(
                            upToTable.Type,
                            filterItem.FilterType,
                            "id",
                            SelectedValues,
                            null,
                            QueryParameters);
                    }
                    else
                    {
                        exp = LinqFilterGenerator.GenerateFilter(
                            upToTable.Type,
                            filterItem.FilterType + "Collection",
                            Url.SelectKeyValueColumn,
                            SelectedValues,
                            null,
                            QueryParameters);
                    }
                }
            }
            else
            {
                if (filterItem.FilterName == MultipleSelectedValues && typeof(TKey) == typeof(long))
                {
                    if (string.IsNullOrEmpty(Url.SelectKeyValueColumn))
                    {
                        exp = LinqFilterGenerator.GenerateFilter(
                            upToTable.Type,
                            filterItem.FilterType,
                            "id",
                            SelectedValues,
                            null,
                            QueryParameters);
                    }
                    else
                    {
                        exp = LinqFilterGenerator.GenerateFilter(
                            upToTable.Type,
                            filterItem.FilterType + "Collection",
                            Url.SelectKeyValueColumn,
                            SelectedValues,
                            null,
                            QueryParameters);
                    }
                }
                else
                {
                    exp = LinqFilterGenerator.GenerateFilter(
                        upToTable.Type,
                        filterItem.FilterType,
                        filterName,
                        filterItem.Value1,
                        filterItem.Value2,
                        QueryParameters);
                }
            }

            if (exp != null && (requiredFiltersSeted.ContainsKey(filterName) || requiredFiltersSeted.ContainsKey(filterNameOriginal)))
            {
                requiredFiltersSeted[filterName] = true;
                requiredFiltersSeted[filterNameOriginal] = true;
            }

            if (exp != null && ((CancelTreeUseExceptions == null || !CancelTreeUseExceptions.Contains(filterNameOriginal)) && !CancelTreeUseExceptionsInternal.Contains(filterNameOriginal)))
                CancelTreeUse = true;

            return exp;
        }

        public virtual Expression FilterDataByHistory<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            var filterByHistoryExp = GetExpressionFilterDataByHistory();
            if (filterByHistoryExp != null)
            {
                if (upToTable == null)
                {
                    source = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source, filterByHistoryExp);
                    QueryParameters.RegisterExpression(filterByHistoryExp, "History");
                }
                else
                {
                    Expression filter = Expression.Invoke(filterByHistoryExp, upToTable);
                    foreach (var item in fieldsToCheckReference)
                    {
                        if (LinqFilterGenerator.IsNullableType(item.Type))
                            filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                    }
                    source = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source, Expression.Lambda(filter, param));
                    QueryParameters.RegisterExpression(filter, "History");
                }
            }
            return source;
        }

        /// <summary>
        /// Описание уловия, фильтрующего данные для скрытия историчной информации.
        /// </summary>
        /// <returns></returns>
        public virtual Expression<Func<TTable, bool>> GetExpressionFilterDataByHistory()
        {
            return null;
        }

        public virtual Expression FilterDataShowHistory(Expression source)
        {
            var filterByHistoryExp = GetExpressionFilterDataForShowHistory();
            if (filterByHistoryExp != null)
            {
                var param = Expression.Parameter(typeof(TTable), "sh");
                source = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, source, Expression.Lambda(filterByHistoryExp, param));
            }
            return source;
        }

        /// <summary>
        /// Описание условия, фильтрующего текущие данные для показа историчной информации.
        /// </summary>
        /// <returns></returns>
        public virtual Expression<Func<TTable, bool>> GetExpressionFilterDataForShowHistory()
        {
            return null;
        }

        public IQueryable<TTable> FilterData(IQueryable<TTable> source, params Expression[] fieldsToCheckReference)
        {
            return FilterData(source, (IEnumerable<Expression>)fieldsToCheckReference);
        }

        public virtual IQueryable<TTable> FilterData(IQueryable<TTable> source, IEnumerable<Expression> fieldsToCheckReference)
        {
            var exp = FilterData(source.Expression, fieldsToCheckReference);
            return source.Provider.CreateQuery<TTable>(exp);
        }

        public abstract Expression FilterDataByParents<T>(Expression source, ParameterExpression param, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference)
            where T : class;

        public virtual BaseFilterEventArgs<TTable> GetFilter(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(Url) { TypeOfData = typeOfData };
        }


        public event EventHandler InitializeFilterExpressions;

        public string FilterByParentControl { get; set; }
        public ISelectedValue ParentControl { get; set; }
        public bool ShowHistory { get; set; }
        public bool ShowHistoryOnlyForThis { get; set; }
        protected SavedUserFilterValues savedUserFilterValues;
        protected const string MultipleSelectedValues = "MultipleSelectedValues";

        public string GetDefaultFilter()
        {
            if ((string.IsNullOrEmpty(MainPageUrlBuilder.Current.UserControl)
                || !MainPageUrlBuilder.Current.UserControl.StartsWith(GetFilterTableName(), StringComparison.OrdinalIgnoreCase))
                && ParentControl == null && string.IsNullOrEmpty(FilterByParentControl))
                return "";
            return GetDefaultFilter(null);
        }

        public string GetDefaultFilter(StorageValues storageValues)
        {
            var jss = new JavaScriptSerializer();
            if (savedUserFilterValues == null)
                savedUserFilterValues = new SavedUserFilterValues { TableName = GetFilterTableName() };
            var defFilter = savedUserFilterValues.GetDefaultFilter();
            var defaultFilters = new DefaultFilters();
            if (GetFilterItems() == null)
            {
                if (!string.IsNullOrEmpty(defFilter))
                    defaultFilters.FilterValues = GetFilterValues(defFilter);
                else
                    SetDefaultFilterIntranal(defaultFilters);
                if (Url.IsMultipleSelect && !string.IsNullOrEmpty(Url.SelectedValues))
                    defaultFilters.SetFilter(MultipleSelectedValues, DefaultFilters.BooleanFilter.Equals);
                var values = defaultFilters.FilterValues.Select(p => new[] { p.Key, p.Value[0], p.Value[1], p.Value[2] }).ToList();
                if (values.Count == 0) return "";
                var lists = jss.Serialize(values);
                return lists;
            }

            if (!string.IsNullOrEmpty(defFilter))
                defaultFilters.FilterItems = MainPageUrlBuilder.GetFilterItemsDicByFilterContent(defFilter);
            else
                SetDefaultFilterIntranal(defaultFilters);

            if (Url.IsMultipleSelect && !string.IsNullOrEmpty(Url.SelectedValues))
                defaultFilters.SetFilter(MultipleSelectedValues, DefaultFilters.BooleanFilter.Equals);

            return GetFilterByStorageValues(storageValues, defaultFilters);
        }

        internal string GetFilterByStorageValues(
            StorageValues storageValues,
            string defFilter)
        {
            var defaultFilters = new DefaultFilters();
            defaultFilters.FilterItems = MainPageUrlBuilder.GetFilterItemsDicByFilterContent(defFilter);
            return GetFilterByStorageValues(storageValues, defaultFilters);
        }

        private string GetFilterByStorageValues(
            StorageValues storageValues,
            DefaultFilters defaultFilters)
        {
            var jss = new JavaScriptSerializer();
            var filterItems = defaultFilters.FilterItems;
            if (storageValues != null)
            {
                var removeFilterNames = storageValues.GetStorageNames()
                    .Union(Enumerable.Range(0, storageValues.CountListValues)
                    .SelectMany(storageValues.GetCircleStorageNames))
                    .Distinct();
                defaultFilters.RemoveFilters(removeFilterNames);
                foreach (var item in storageValues.GetStorageNames())
                {
                    var values = storageValues.GetStorageValues(item);
                    var textValues = storageValues.GetStorageTextValues(item);
                    var filterType = storageValues.GetStorageFilterType(item);
                    SetFilter(defaultFilters, item, filterType, values, textValues);
                }

                for (int i = 0; i < storageValues.CountListValues; i++)
                {
                    foreach (var item in storageValues.GetCircleStorageNames(i))
                    {
                        var values = storageValues.GetCircleStorageValues(item, i);
                        var textValues = storageValues.GetCircleStorageTextValues(item, i);
                        defaultFilters.SetFilter(
                            item,
                            storageValues.GetCircleStorageFilterType(item, i),
                            GetValue1(values),
                            GetValue2(values, textValues));
                    }
                }
            }

            if (filterItems.Count == 0)
                return string.Empty;

            var lists = jss.Serialize(filterItems.SelectMany(r => r.Value).ToList());
            return lists;
        }

        private static void SetFilter(DefaultFilters defaultFilters, string filterName, ColumnFilterType? filterType, object[] values, string[] textValues)
        {
            switch (filterType)
            {
                case ColumnFilterType.In:
                case ColumnFilterType.OutOf:
                    for (var index = 0; index < values.Length; index++)
                    {
                        defaultFilters.SetFilter(
                            filterName,
                            filterType,
                            Convert.ToString(values[index]),
                            textValues?[index]);
                    }
                    break;
                default:
                    defaultFilters.SetFilter(
                        filterName,
                        filterType,
                        GetValue1(values),
                        GetValue2(values, textValues));
                    break;
            }
        }

        private static string GetValue1(object[] values)
        {
            if (values == null || values.Length < 1 || values[0] == null) 
                return null;
            if (values[0] is DateTime)
            {
                var date = (DateTime)values[0];
                return date.ToString("dd.MM.yyyy HH:mm");
            }

            return values[0].ToString();
        }

        private static string GetValue2(object[] values, string[] textValues)
        {
            if (values == null || values.Length < 2 || values[1] == null) 
                return textValues == null || textValues.Length == 0 ? null : textValues[0];

            if (values[1] is DateTime)
            {
                var date = (DateTime)values[1];
                return date.ToString("dd.MM.yyyy HH:mm");
            }

            return values[1].ToString();
        }

        public override string GetTableName()
        {
            return null;
        }

        public virtual string GetFilterTableName()
        {
            if (CustomFilter != null) return CustomFilter.TableName;
            return GetTableName();
        }

        protected static Dictionary<string, string[]> GetFilterValues(string serializedFilterValues)
        {
            var jss = new JavaScriptSerializer();
            var lists = jss.Deserialize<List<List<string>>>(serializedFilterValues);
            var values = new Dictionary<string, string[]>();
            if (lists != null)
                foreach (var list in lists)
                    values.Add(list[0], list.Skip(1).ToArray());
            return values;
        }

        protected virtual void SetDefaultFilterIntranal(DefaultFilters defaultFilters)
        {
            foreach (var workFlow in WorkFlows)
                workFlow.SetDefaultFilter(defaultFilters);
        }

        public List<Expression<Func<TTable, bool>>> FilterExpressions { get; set; }

        protected virtual IQueryable<TTable> FilterByCustomExpressions(IQueryable<TTable> data)
        {
            OnInitializeFilterExpressions();
            if (FilterExpressions.Count == 0) return data;
            var param = Expression.Parameter(typeof (TTable), "custExp");
            Expression filter = Expression.Invoke(FilterExpressions.First(), param);
            foreach (var expression in FilterExpressions.Skip(1))
                filter = Expression.And(filter, Expression.Invoke(expression, param));
            var lambda = Expression.Lambda(filter, param);
            var resExp = Expression.Call(typeof (Queryable), "Where", new[] {typeof (TTable)}, data.Expression, lambda);
            return data.Provider.CreateQuery<TTable>(resExp);
        }

        protected virtual Expression FilterByCustomExpressions(Expression data)
        {
            OnInitializeFilterExpressions();
            if (FilterExpressions.Count == 0) return data;
            var param = Expression.Parameter(typeof(TTable), "custExp");
            Expression filter = Expression.Invoke(FilterExpressions.First(), param);
            foreach (var expression in FilterExpressions.Skip(1))
                filter = Expression.And(filter, Expression.Invoke(expression, param));
            var lambda = Expression.Lambda(filter, param);
            var resExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, data, lambda);
            return resExp;
        }

        protected virtual void OnInitializeFilterExpressions()
        {
            if (InitializeFilterExpressions != null)
                InitializeFilterExpressions(this, EventArgs.Empty);
        }

        protected virtual void InitializeCustomFilters(IList<FilterHtmlGenerator.Filter> filters)
        {
            if (CustomFilter != null)
                CustomFilter.InitializeFilter(this, filters);
        }

        public override string Header
        {
            get
            {
                if (CustomFilter != null) return CustomFilter.FilterHeader;
                return base.Header;
            }
            protected set
            {
                base.Header = value;
            }
        }

        public virtual void ReParseFilterParameters()
        {}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ("on".Equals(HttpContext.Current.Request.Params["__loadFilterFromParam"], StringComparison.OrdinalIgnoreCase))
                Page.ClientScript.RegisterStartupScript(
                    GetType(), "__loadFilterFromParam", string.Format(@"
$(function() {{
    if (window.dialogArguments == null) {{
        window.InitialLoadFilterFromParam = function() {{
            SetFilters($get('filter{0}'), Sys.Serialization.JavaScriptSerializer.deserialize(window.dialogArguments.filterValue));
        }};
    }}
    else
        SetFilters($get('filter{0}'), Sys.Serialization.JavaScriptSerializer.deserialize(window.dialogArguments.filterValue));
}});
", GetTableName()), true);
        }
    }

    public abstract class BaseTreeFilterControl<TKey, TTable> : BaseFilterControl<TKey, TTable>
        where TKey : struct
        where TTable : class
    {
        public TKey? ParentID { get; set; }

        public IQueryable<TTable> FilterDataOfParents(IQueryable<TTable> source)
        {
            var filterArgs = GetFilterOfParents(typeof(TTable));
            var exp = filterArgs.FilterData<TTable>(source.Expression, null, null);
            return source.Provider.CreateQuery<TTable>(exp);
        }

        public IQueryable<TTable> FilterDataOfChilds(IQueryable<TTable> source)
        {
            var filterArgs = GetFilterOfChilds(typeof(TTable));
            var exp = filterArgs.FilterData<TTable>(source.Expression, null, null);
            return source.Provider.CreateQuery<TTable>(exp);
        }
        
        public virtual BaseFilterEventArgs<TTable> GetFilterOfParents(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(Url) { TypeOfData = typeOfData };
        }

        public virtual BaseFilterEventArgs<TTable> GetFilterOfChilds(Type typeOfData)
        {
            return new BaseFilterEventArgs<TTable>(Url) { TypeOfData = typeOfData };
        }
    }
    
    public abstract class BaseFilterControl<TKey, TTable, TDataContext> : BaseFilterControl<TKey, TTable>
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext, new()
    {
        public event EventHandler<BaseFilterEventArgs<TTable, TDataContext>> Filter;

        protected void OnFilter(BaseFilterEventArgs<TTable, TDataContext> e)
        {
            if (Filter != null) Filter(this, e);
        }

        protected abstract void FilterInitialize();
  
        public override void EnsureFilterInitialize()
        {
            FilterInitialize();
        }

        public virtual Expression GetFilteredExpression(Expression query, QueryParameters qParams)
        {
            FilterInitialize();

            QueryParameters = qParams;

            query = FilterByCustomExpressions(query);
            query = FilterData(query, qParams);

            if (ParentControl != null)
            {
                var param = Expression.Parameter(typeof(TTable), "r");
                var filterExp = ParentControl.GetExpression(FilterByParentControl, param, QueryParameters);
                if (filterExp != null)
                {
                    Expression pred = Expression.Lambda(filterExp, param);
                    query = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, query, pred);
                }
                else
                {
                    var filterValue = ParentControl.SelectedValue == null
                                          ? "0"
                                          : ParentControl.SelectedValueLong.ToString();
                    query = LinqFilterGenerator.GenerateFilter(query, typeof (TTable),
                                                               "Equal", FilterByParentControl,
                                                               filterValue, null, qParams);
                }
            }
            else
            {
                BaseNavigatorControl navigator = GetNavigatorControl();
                navigator.InitQueryParameters(qParams);
                query = navigator.FilterData(query);
                if (Url.UserControl != null && Url.UserControl.StartsWith(GetTableName()))
                {
                    foreach (var queryParameter in Url.QueryParameters)
                    {
                        if (queryParameter.Key.Contains(".") && !string.IsNullOrEmpty(queryParameter.Value) && !queryParameter.Key.EndsWith(".id"))
                            query = GetFilteredByQueryParameter(query, queryParameter, qParams);
                    }
                }
            }
            return query;
        }

        protected virtual Expression GetFilteredByQueryParameter(Expression query, KeyValuePair<string, string> queryParameter, QueryParameters qParams)
        {
            return LinqFilterGenerator.GenerateFilter(query, typeof(TTable), "Equal",
                                                      queryParameter.Key, queryParameter.Value, null, qParams);
        }

        protected abstract BaseNavigatorControl GetNavigatorControl();

        public override BaseFilterEventArgs<TTable> GetFilter(Type typeOfData)
        {
            var filter = new BaseFilterEventArgs<TTable, TDataContext>(Url, QueryParameters) { TypeOfData = typeOfData };
            OnFilter(filter);
            return filter;
        }

        public override IQueryable ExecFilterData<TSource>(IQueryable source, Expression upToTable) 
        {
            FilterInitialize();
            var exp = FilterData<TSource>(source.Expression, upToTable, null, new Expression[0], true);
            return source.Provider.CreateQuery<TSource>(exp);
        }
    }

    public abstract class BaseTreeFilterControl<TKey, TTable, TDataContext> : BaseFilterControl<TKey, TTable, TDataContext>
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext, new()
    {
        public TKey? ParentID { get; set; }
        public bool UseTree { get; set; }
        public bool IsSelectParentRows { get; set; }

        public Expression FilterDataOfParents(Expression source, QueryParameters qParams)
        {
            var filterArgs = GetFilterOfParents(typeof(TTable), qParams);
            return filterArgs.FilterData<TTable>(source, null, null);
        }

        public Expression FilterDataOfChilds(Expression source, QueryParameters qParams)
        {
            var filterArgs = GetFilterOfChilds(typeof(TTable), qParams);
            return filterArgs.FilterData<TTable>(source, null, null);
        }

        public IQueryable<TTable> FilterDataOfParents(IQueryable<TTable> source)
        {
            var filterArgs = GetFilterOfParents(typeof(TTable), null);
            var exp = filterArgs.FilterData<TTable>(source.Expression, null, null);
            return source.Provider.CreateQuery<TTable>(exp);
        }

        public IQueryable<TTable> FilterDataOfChilds(IQueryable<TTable> source)
        {
            var filterArgs = GetFilterOfChilds(typeof(TTable), null);
            var exp = filterArgs.FilterData<TTable>(source.Expression, null, null);
            return source.Provider.CreateQuery<TTable>(exp);
        }

        public virtual BaseFilterEventArgs<TTable, TDataContext> GetFilterOfParents(Type typeOfData, QueryParameters qParams)
        {
            return new BaseFilterEventArgs<TTable, TDataContext>(Url, qParams) { TypeOfData = typeOfData };
        }

        public virtual BaseFilterEventArgs<TTable, TDataContext> GetFilterOfChilds(Type typeOfData, QueryParameters qParams)
        {
            return new BaseFilterEventArgs<TTable, TDataContext>(Url, qParams) { TypeOfData = typeOfData };
        }

        public override Expression GetFilteredExpression(Expression query, QueryParameters qParams)
        {
            CancelTreeUse = !UseTree;
            return base.GetFilteredExpression(query, qParams);
        }

        protected override Expression GetFilteredByQueryParameter(Expression query, KeyValuePair<string, string> queryParameter, QueryParameters qParams)
        {
            if ((CancelTreeUseExceptions == null || !CancelTreeUseExceptions.Contains(queryParameter.Key)) && !CancelTreeUseExceptionsInternal.Contains(queryParameter.Key))
                CancelTreeUse = true;
            return base.GetFilteredByQueryParameter(query, queryParameter, qParams);
        }
    }
}