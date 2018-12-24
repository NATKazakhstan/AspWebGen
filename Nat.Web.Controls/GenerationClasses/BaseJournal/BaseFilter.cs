/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.31
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.UI.WebControls;

    using Nat.Tools.Filtering;
    using Nat.Web.Controls.GenerationClasses.Filter;
    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools;

    public abstract class BaseFilter
    {
        private List<string> _filters;

        private IDictionary<string, List<string>> _filtersDic;

        public virtual string FilterHeader
        {
            get { return FilterProjectHeader + " -&gt; " + FilterTableHeader; }
        }

        public abstract string FilterProjectHeader { get; }

        public abstract string FilterTableHeader { get; }

        public abstract string TableName { get; }

        public List<string> RequiredFilters { get; set; }

        protected bool IsFilterStringsCreated { get; set; }

        public abstract QueryParameters QueryParameters { get; }

        public static List<string> GetFilterStrings(Dictionary<string, List<FilterItem>> filterValues, IEnumerable<FilterHtmlGenerator.Filter> allFilters)
        {
            var filters = new List<string>();
            foreach (var filter in allFilters)
            {
                if (string.IsNullOrEmpty(filter.FilterName) || !filter.Visible)
                    continue;

                var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                var filterItemValues = filterValues.ContainsKey(filter.FilterName)
                                           ? filterValues[filter.FilterName]
                                           : (filterValues.ContainsKey(filterName) ? filterValues[filterName] : null);

                if (filterItemValues == null)
                    continue;

                foreach (var filterItem in filterItemValues)
                {
                    if ("Non".Equals(filterItem.FilterType))
                        continue;

                    var values = filter.GetValuesString(filterItem).ToList();
                    if (values.Count == 2)
                        filters.Add(filter.Header + " " + values[0] + " " + values[1]);
                    else if (values.Count > 0)
                    {
                        filters.Add(filter.Header + " " + values[0]);
                        if (values.Count > 1)
                            filters.AddRange(values.Skip(1).Select(r => "        " + r));
                    }
                }
            }

            return filters;
        }

        public static IDictionary<string, List<string>> GetDictionaryFilterStrings(Dictionary<string, List<FilterItem>> filterValues, IEnumerable<FilterHtmlGenerator.Filter> allFilters)
        {
            var filters = new Dictionary<string, List<string>>();
            foreach (var filter in allFilters)
            {
                if (string.IsNullOrEmpty(filter.FilterName))
                    continue;

                var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                var filterItemValues = filterValues.ContainsKey(filter.FilterName)
                                           ? filterValues[filter.FilterName]
                                           : (filterValues.ContainsKey(filterName) ? filterValues[filterName] : null);

                if (filterItemValues == null)
                    continue;

                foreach (var filterItem in filterItemValues)
                {
                    if ("Non".Equals(filterItem.FilterType))
                        continue;

                    var values = filter.GetValuesString(filterItem).ToList();
                    if (values.Count > 0)
                    {
                        if (!filters.ContainsKey(filterName))
                            filters[filterName] = new List<string>();
                        filters[filterName].AddRange(values);
                    }
                }
            }

            return filters;
        }

        internal static IDictionary<string, IList> GetForPublicationStrings(Dictionary<string, List<FilterItem>> filterValues, IEnumerable<FilterHtmlGenerator.Filter> allFilters)
        {
            var filters = new Dictionary<string, IList>();
            foreach (var filter in allFilters)
            {
                if (string.IsNullOrEmpty(filter.FilterName))
                    continue;

                var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                var filterItemValues = filterValues.ContainsKey(filter.FilterName)
                                           ? filterValues[filter.FilterName]
                                           : (filterValues.ContainsKey(filterName) ? filterValues[filterName] : null);

                if (filterItemValues == null)
                    continue;

                foreach (var filterItem in filterItemValues)
                {
                    if ("Non".Equals(filterItem.FilterType))
                        continue;

                    var values = filter.GetListValuesString(filterItem).Cast<object>().ToList();
                    if (values.Count > 0)
                    {
                        if (!filters.ContainsKey(filterName))
                            filters[filterName] = values;
                        else
                        {
                            var list = filters[filterName] as List<List<object>>;
                            if (list != null)
                                list.Add(values);
                            else
                            {
                                var previousList = (List<object>)filters[filterName];
                                list = new List<List<object>> { previousList, values };
                                filters[filterName] = list;
                            }
                        }
                    }
                }
            }

            return filters;
        }
        
        public abstract IQueryable FilterHeaderData(IQueryable data, CrossColumnDataSource headerDataSource);

        public abstract IQueryable FilterCrossData(IQueryable data, BaseJournalCrossTable crossTable);

        public abstract Expression FilterCrossData(Expression data, BaseJournalCrossTable crossTable);

        public abstract void SetDB(DataContext db);

        public abstract int GetMaxRecursion<THeaderTable>(CrossColumnDataSource dataSource)
            where THeaderTable : class;

        public abstract List<string> GetStartTreeKeys<THeaderTable>(CrossColumnDataSource dataSource)
            where THeaderTable : class;

        public virtual string GetStartTreeKey<THeaderTable>(CrossColumnDataSource dataSource)
            where THeaderTable : class
        {
            var keys = GetStartTreeKeys<THeaderTable>(dataSource);
            if (keys == null) return null;
            return keys.FirstOrDefault();
        }

        public abstract List<THeaderKey> GetStartTreeKeys<THeaderTable, THeaderKey>(CrossColumnDataSource dataSource)
            where THeaderTable : class
            where THeaderKey : struct;

        public virtual int GetFiltersCount()
        {
            return GetFilterStrings().Count();
        }

        public virtual IList<string> GetFilterStringsByName(string filterName)
        {
            if (_filtersDic == null)
                _filtersDic = GetDictionaryFilterStrings(GetFilterValues(), GetAllFilters());

            if (!_filtersDic.ContainsKey(filterName))
                return new string[0];

            return _filtersDic[filterName];
        }

        public virtual IEnumerable<string> GetFilterStrings()
        {
            EnsureFilterStrings();
            return _filters;
        }

        internal IDictionary<string, IList> GetForPublicationStrings()
        {
            var filterValues = GetFilterValues();
            var allFilters = GetAllFilters();
            return GetForPublicationStrings(filterValues, allFilters);
        }

        protected virtual void EnsureFilterStrings()
        {
            if (!IsFilterStringsCreated)
            {
                IsFilterStringsCreated = true;
                CreateFilterStrings();
            }
        }

        protected virtual void CreateFilterStrings()
        {
            var filterValues = GetFilterValues();
            var allFilters = GetAllFilters();
            _filters = GetFilterStrings(filterValues, allFilters);
        }

        public virtual Dictionary<string, List<FilterItem>> GetFilterValues()
        {
            return new Dictionary<string, List<FilterItem>>();
        }

        public virtual List<FilterItem> GetFilterValuesByName(string filterName)
        {
            var filterValues = GetFilterValues();
            if (filterValues.ContainsKey(filterName) && filterValues[filterName].Count > 0)
                return filterValues[filterName];
            var originalFilterName = LinqFilterGenerator.GetFilterName(filterName);
            if (filterValues.ContainsKey(originalFilterName) && filterValues[originalFilterName].Count > 0)
                return filterValues[originalFilterName];
            return null;
        }

        protected abstract IEnumerable<FilterHtmlGenerator.Filter> GetAllFilters();

        public virtual void SetFiltersByStorageValues(StorageValues storageValues, MainPageUrlBuilder urlBuilder)
        {
            var filterValues = GetFilterValues();
            foreach (var item in storageValues.GetStorageNames())
            {
                var columnFilterType = storageValues.GetStorageFilterType(item);
                if (columnFilterType == null || columnFilterType == ColumnFilterType.None) continue;
                
                var values = storageValues.GetStorageValues(item);
                if (!filterValues.ContainsKey(item))
                    filterValues[item] = new List<FilterItem>();
                else
                    filterValues[item].Clear();
                if (columnFilterType == ColumnFilterType.In || columnFilterType == ColumnFilterType.OutOf)
                    filterValues[item].AddRange(values.Select(r => new FilterItem(item, columnFilterType, new[] { r })));
                else
                {
                    var filterItem = new FilterItem(item, columnFilterType, values);
                    filterValues[item].Add(filterItem);
                }
            }
            for (int i = 0; i < storageValues.CountListValues; i++)
            {
                foreach (var item in storageValues.GetCircleStorageNames(i))
                {
                    var columnFilterType = storageValues.GetCircleStorageFilterType(item, i);
                    if (columnFilterType == null || columnFilterType == ColumnFilterType.None) continue;

                    var values = storageValues.GetCircleStorageValues(item, i);
                    var filterItem = new FilterItem(item, columnFilterType, values);
                    if (!filterValues.ContainsKey(item))
                        filterValues[item] = new List<FilterItem>();
                    else if (i == 0)
                        filterValues[item].Clear();

                    filterValues[item].Add(filterItem);
                }
            }
            urlBuilder.SetFilter(TableName, filterValues);
        }

        public void SetFiltersToStorageValues(StorageValues storageValues)
        {
            var filterValues = GetFilterValues();
            foreach (var item in storageValues.GetStorageNames())
            {
                if (!filterValues.ContainsKey(item)) continue;
                if (filterValues[item].Count == 0)
                    storageValues.SetStorageValues(item, ColumnFilterType.None, new object[2]);
                else
                {
                    var filterItem = filterValues[item][0];
                    storageValues.SetStorageValues(item, filterItem.ColumnFilterType.Value, filterItem.Value1, filterItem.Value2);
                }
            }
            for (int i = 0; i < storageValues.CountListValues; i++)
            {
                foreach (var item in storageValues.GetCircleStorageNames(i))
                {
                    if (!filterValues.ContainsKey(item) || filterValues[item].Count <= i) continue;
                    var filterItem = filterValues[item][i];
                    storageValues.SetCircleStorageValues(item, i, filterItem.Value1, filterItem.Value2);
                }
            }
        }

        public abstract void SetFilterControl(IFilterControl filterControl);
    }

    public abstract class BaseFilter<TKey, TTable> : BaseFilter
        where TKey : struct
        where TTable : class
    {
        public abstract void InitializeFilter(
            BaseFilterControl<TKey, TTable> filterControl, IList<FilterHtmlGenerator.Filter> filters);
    }

    public abstract class BaseFilter<TKey, TTable, TDataContext> : BaseFilter<TKey, TTable>
        where TKey : struct
        where TTable : class
        where TDataContext : DataContext
    {
        private IList<FilterHtmlGenerator.Filter> _filters;
        private BaseFilterControl<TKey, TTable> _filterControl;

        protected BaseFilter()
        {
            HeaderFilterArgs = new List<BaseJournalFilterEventArgs>();
            JournalFilterArgs = new List<BaseJournalFilterEventArgs>();
            CrossDataFilterArgs = new List<BaseJournalFilterEventArgs>();
        }

        protected List<BaseJournalFilterEventArgs> HeaderFilterArgs { get; set; }

        protected List<BaseJournalFilterEventArgs> JournalFilterArgs { get; set; }

        protected List<BaseJournalFilterEventArgs> CrossDataFilterArgs { get; set; }

        protected abstract void InitializeFilters();

        public TDataContext DB { get; set; }

        public BaseFilterControl<TKey, TTable> FilterControl
        {
            get { return _filterControl; }
        }

        public override QueryParameters QueryParameters
        {
            get { return FilterControl.QueryParameters; }
        }
        
        public override void SetFilterControl(IFilterControl filterControl)
        {
            _filterControl = (BaseFilterControl<TKey, TTable>) filterControl;
        }

        public abstract void Initialize(BaseJournalControl journalControl);

        public override void InitializeFilter(BaseFilterControl<TKey, TTable> filterControl, IList<FilterHtmlGenerator.Filter> filters)
        {
            _filterControl = filterControl;
            InitializeFilters();
            var dependedFilters = GetDependedFilters();
            if (JournalFilterArgs.Count > 0)
            {
                foreach (var filter in JournalFilterArgs.SelectMany(r => r.GetFilterParameters()))
                    filter.AddFilter(filters);
            }
            if (HeaderFilterArgs.Count > 0)
            {
                var groupFilter = new FilterHtmlGenerator.Filter
                                      {
                                          Header = Resources.SHeaderFilters,
                                          Type = FilterHtmlGenerator.FilterType.Group,
                                      };
                filters.Add(groupFilter);
                foreach (var filter in HeaderFilterArgs.SelectMany(r => r.GetFilterParameters()))
                    filter.AddFilter(groupFilter.Children);
            }
            if (CrossDataFilterArgs.Count > 0)
            {
                var groupFilter = new FilterHtmlGenerator.Filter
                                      {
                                          Header = Resources.SDataFilters,
                                          Type = FilterHtmlGenerator.FilterType.Group,
                                      };
                filters.Add(groupFilter);
                foreach (var filter in CrossDataFilterArgs.SelectMany(r => r.GetFilterParameters()))
                    filter.AddFilter(groupFilter.Children);
            }
            _filters = filters;

            var allFilters = filters.
                Union(filters.SelectMany(f => f.AllChildren)).
                Where(f => !string.IsNullOrEmpty(f.FilterName));
            var requiredFilters = RequiredFilters == null ? null : RequiredFilters.ToDictionary(r => r);
            if (dependedFilters != null && requiredFilters != null)
                foreach (var filter in allFilters)
                {
                    var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                    if (requiredFilters.ContainsKey(filterName))
                        filter.RequiredFilter = true;
                    if (dependedFilters.ContainsKey(filterName))
                    {
                        var f = (BaseFilterParameter) filter;
                        if (f.DependedFilters == null)
                            f.DependedFilters = dependedFilters[filterName];
                        else
                            f.DependedFilters.AddRange(dependedFilters[filterName]);
                    }
                }
            else if (dependedFilters != null)
                foreach (var filter in allFilters)
                {
                    var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                    if (dependedFilters.ContainsKey(filterName))
                    {
                        var f = (BaseFilterParameter)filter;
                        if (f.DependedFilters == null)
                            f.DependedFilters = dependedFilters[filterName];
                        else
                            f.DependedFilters.AddRange(dependedFilters[filterName]);
                    }
                }
            else if (requiredFilters != null)
                foreach (var filter in allFilters)
                {
                    var filterName = LinqFilterGenerator.GetFilterName(filter.FilterName);
                    if (requiredFilters.ContainsKey(filterName))
                        filter.RequiredFilter = true;
                }
        }

        protected virtual Dictionary<string, List<BaseFilterParameter>> GetDependedFilters()
        {
            return null;
        }

        protected override IEnumerable<FilterHtmlGenerator.Filter> GetAllFilters()
        {
            return _filters.Union(_filters.SelectMany(r => r.AllChildren));
        }

        public override Dictionary<string, List<FilterItem>> GetFilterValues()
        {
            return _filterControl.GetFilterItemsIternal();
        }

        public override IQueryable FilterHeaderData(IQueryable data, CrossColumnDataSource headerDataSource)
        {
            Expression filter = null;
            var param = Expression.Parameter(headerDataSource.HeaderType, "fDataHeader");

            foreach (var filterArgs in HeaderFilterArgs.Where(r => r.CrossHeader == headerDataSource))
            {
                var filterItems = filterArgs
                    .GetFilterParameters()
                    .Where(
                        r => r.ColumnAggregateType == ColumnAggregateType.None
                             && r.WhereExpression != null);

                foreach (var filterItem in filterItems)
                {
                    if (filter == null)
                        filter = Expression.Invoke(filterItem.WhereExpression, param);
                    else
                        filter = Expression.And(filter, Expression.Invoke(filterItem.WhereExpression, param));
                }
            }

            if (filter != null)
            {
                var lambda = Expression.Lambda(filter, param);
                var resExp = Expression.Call(typeof(Queryable), "Where", new[] { headerDataSource.HeaderType },
                                             data.Expression, lambda);
                if (FilterControl.QueryParameters != null)
                    return FilterControl.QueryParameters.CreateQuery(resExp);
                return data.Provider.CreateQuery(resExp);
            }
            
            return data;
        }

        public override IQueryable FilterCrossData(IQueryable data, BaseJournalCrossTable crossTable)
        {
            var expression = FilterCrossData(data.Expression, crossTable);
            if (FilterControl.QueryParameters != null)
                return FilterControl.QueryParameters.CreateQuery(expression);
            return data.Provider.CreateQuery(expression);
        }

        public override Expression FilterCrossData(Expression query, BaseJournalCrossTable crossTable)
        {
            foreach (var filterArgs in CrossDataFilterArgs.Where(r => r.CrossTable == crossTable))
            {
                var filterItems =
                    filterArgs.GetFilterParameters()
                              .Where(
                                  r => r.ColumnAggregateType == ColumnAggregateType.None
                                       && r.WhereExpression != null);
                foreach (var filterItem in filterItems)
                {
                    var whereExpression = filterItem.GetWhereExpression(filterItem);
                    if (whereExpression != null)
                    {
                        query = Expression.Call(
                            typeof(Queryable),
                            "Where",
                            new[] { crossTable.CrossTableType },
                            query,
                            whereExpression);
                    }
                }
            }

            foreach (var headerFilterArg in HeaderFilterArgs)
            {
                var exp = SetFilterCrossData(crossTable, headerFilterArg.CrossHeader);
                if (exp != null)
                    query = Expression.Call(typeof (Queryable), "Where", new[] {crossTable.CrossTableType}, query, exp);
            }

            return query;
        }

        protected Expression SetFilterCrossData(BaseJournalCrossTable crossTable, CrossColumnDataSource headerDataSource)
        {
            Expression crossFilter = null;
            var dbExp = Expression.Constant(DB);
            var crossParam = Expression.Parameter(crossTable.CrossTableType, "crossParam");

            foreach (var filterArgs in HeaderFilterArgs.Where(r => r.CrossHeader == headerDataSource))
            {
                var groupedFilterItems =
                    from f in filterArgs.GetFilterParameters()
                    group f by f.CorssDataToHeader;
                foreach (var groupedFilterItem in groupedFilterItems)
                {
                    if (groupedFilterItem.Key == null)
                    {
                        foreach (var filterItem in groupedFilterItem)
                        {
                            if (filterItem.CrossTable != crossTable ||
                                filterItem.FieldReferenceFromCrossToHeader == null) continue;
                            var list = headerDataSource.GetKeys();
                            var listExp = Expression.Constant(list);
                            Expression fieldExp = Expression.Invoke(filterItem.FieldReferenceFromCrossToHeader,
                                                                    crossParam);
                            if (fieldExp.Type.IsGenericType &&
                                fieldExp.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof (Nullable<>)))
                                fieldExp = Expression.Property(fieldExp, "Value");
                            Expression exp = Expression.Call(listExp, "Contains", new Type[] {}, fieldExp);
                            crossFilter = crossFilter == null ? exp : Expression.And(crossFilter, exp);
                        }
                    }
                    else
                    {
                        var param = Expression.Parameter(headerDataSource.HeaderType, "fDataHeaderInC");
                        Expression filter = null;
                        foreach (var filterItem in groupedFilterItem)
                        {
                            if (filterItem.CrossTable != crossTable) continue;

                            if (filterItem.WhereExpression != null)
                            {
                                if (filter == null)
                                    filter = Expression.Invoke(filterItem.WhereExpression, param);
                                else
                                    filter = Expression.And(filter, Expression.Invoke(filterItem.WhereExpression, param));
                            }
                        }
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, param);
                            var crossExp = Expression.Invoke(groupedFilterItem.Key, crossParam, dbExp);
                            var resExp = Expression.Call(typeof (Queryable), "FirstOrDefault",
                                                         new[] {headerDataSource.HeaderType}, crossExp, lambda);
                            var addFilter = Expression.NotEqual(resExp,
                                                                Expression.Constant(null, headerDataSource.HeaderType));
                            crossFilter = crossFilter == null ? addFilter : Expression.And(crossFilter, addFilter);
                        }
                    }
                }
            }
            if (crossFilter != null)
                return Expression.Lambda(crossFilter, crossParam);
            return null;
        }

        public override void SetDB(DataContext db)
        {
            if (db != null)
                DB = (TDataContext) db;
        }

        public override int GetMaxRecursion<THeaderTable>(CrossColumnDataSource dataSource)
        {
            foreach (var headerFilterArg in HeaderFilterArgs.Where(r => r.CrossHeader == dataSource))
            {
                foreach (var parameter in headerFilterArg.GetFilterParameters())
                {
                    var treeHeader = parameter as TreeHeaderFilterParameter<THeaderTable>;
                    if (treeHeader != null && treeHeader.IsMaxRecursion)
                    {
                        if (treeHeader.MaxRecursion == 0)
                            return 3;
                        return treeHeader.MaxRecursion;
                    }
                }
            }
            return 3;
        }

        public override List<string> GetStartTreeKeys<THeaderTable>(CrossColumnDataSource dataSource)
        {
            foreach (var headerFilterArg in HeaderFilterArgs.Where(r => r.CrossHeader == dataSource))
            {
                foreach (var parameter in headerFilterArg.GetFilterParameters())
                {
                    var treeHeader = parameter as TreeHeaderFilterParameter<THeaderTable>;
                    if (treeHeader != null && treeHeader.IsStartLevel)
                        return treeHeader.StartHeaderValues;
                }
            }
            return null;
        }

        public override List<THeaderKey> GetStartTreeKeys<THeaderTable, THeaderKey>(CrossColumnDataSource dataSource)
        {
            foreach (var headerFilterArg in HeaderFilterArgs.Where(r => r.CrossHeader == dataSource))
            {
                foreach (var parameter in headerFilterArg.GetFilterParameters())
                {
                    var treeHeader = parameter as TreeHeaderFilterParameter<THeaderTable>;
                    if (treeHeader != null && treeHeader.IsStartLevel)
                        return treeHeader.StartHeaderValues.Select(r => (THeaderKey)Convert.ChangeType(r, typeof(THeaderKey))).ToList();
                }
            }
            return null;
        }
    }
}
