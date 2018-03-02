namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseJoinFilterParameterContainer<TTable> : BaseFilterParameterContainer<TTable>
        where TTable : class
    {
        private readonly Dictionary<string, BaseFilterParameter<TTable>> filters = new Dictionary<string, BaseFilterParameter<TTable>>();
        private readonly Dictionary<string, List<FilterItem>> filterItems = new Dictionary<string, List<FilterItem>>();

        public BaseJoinFilterParameterContainer(string filterName)
        {
            this.FilterName = filterName;
        }

        public void Initialize(IList<FilterHtmlGenerator.Filter> intializeFilters, params string[] joinFilterNames)
        {
            var filtersDic =
                intializeFilters.Union(intializeFilters.SelectMany(f => f.AllChildren))
                    .Where(r => r.FilterName != null)
                    .ToDictionary(r => r.FilterName);
            foreach (var filterName in joinFilterNames)
            {
                var filter = (BaseFilterParameter<TTable>)filtersDic[filterName];
                filter.Container = this;
                filter.IsJournalFilter = true;
                this.filters.Add(filterName, filter);
                this.filterItems.Add(filterName, new List<FilterItem>());
            }

            this.GetChildsFromJournal = this.filters.Values.First().GetChildsFromJournal;

            this.AddFilter(intializeFilters);
        }

        public override void ClearState()
        {
            base.ClearState();

            foreach (var filterItem in this.filterItems.Values)
                filterItem.Clear();
        }

        public override void AddFilterValue(
            BaseFilterParameter<TTable> baseFilterParameter,
            Enum filtertype,
            FilterItem filterItem,
            QueryParameters qParams)
        {
            this.filterItems[filterItem.FilterName].Add(filterItem);
        }

        public override Expression GetFilter(QueryParameters qParams)
        {
            var expressions = new List<Expression>();
            foreach (var filter in this.filters)
            {
                AddExpressions(qParams, this.filterItems[filter.Key], filter.Value, expressions);
            }

            if (expressions.Count == 0) return null;

            var param = Expression.Parameter(typeof(TTable), "aggRow");
            var andExpressions = expressions.Select(e => (Expression)Expression.Invoke(e, param)).Aggregate(Expression.And);
            var filterByChilds = Expression.Lambda(andExpressions, param);
            return this.GetFilterExpressionByChilds(qParams, typeof(TTable), filterByChilds);
        }
        
        private void AddExpressions(QueryParameters qParams, List<FilterItem> filterItems, BaseFilterParameter<TTable> filter, List<Expression> expressions)
        {
            expressions.AddRange(
                filterItems.Where(f => f.FilterType == "NotEquals")
                    .Select(f => filter.CreateFilterExpression(f, qParams))
                    .Where(f => f != null));

            var items = filterItems
                .Where(f => f.FilterType != "NotEquals" && f.FilterType != "Non")
                .Select(f => filter.CreateFilterExpression(f, qParams))
                .Where(f => f != null)
                .ToList();
            if (items.Count == 1)
            {
                expressions.Add(items[0]);
                AppliedFilters.Add(filter);
            }
            else if (items.Count > 1)
            {
                var param = Expression.Parameter(typeof(TTable), "orRow");
                var orExpressions =
                    items.Select(e => (Expression)Expression.Invoke(e, param)).Aggregate(Expression.Or);
                expressions.Add(Expression.Lambda(orExpressions, param));
                AppliedFilters.Add(filter);
            }
        }
    }
}