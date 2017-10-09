namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using JS.Linq.Extensions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterStringSearch<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        private readonly List<BaseFilterParameterString<TTable>> innerFilters;

        public BaseFilterParameterStringSearch(params Expression<Func<TTable, string>>[] fields)
        {
            innerFilters = fields.Select(r => new BaseFilterParameterString<TTable>(r) { IsJournalFilter = true }).ToList();
        }

        protected override Type FieldType => typeof(string);

        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            QueryParameters = qParams;

            InnerWhereExpression = innerFilters
                .Select(r => r.OnFilter(filtertype, filterItem, qParams))
                .Where(r => r != null)
                .ToOrLambdaExpression<TTable>();

            var param = Expression.Parameter(typeof(TTable), "onFilterSearch");
            Expression expression;
            if (WhereExpression != null)
                expression = Expression.Or(Expression.Invoke(InnerWhereExpression, param), Expression.Invoke(WhereExpression, param));
            else
                expression = Expression.Invoke(InnerWhereExpression, param);
            WhereExpression = Expression.Lambda(expression, param);

            return GetJournalFilter(qParams, TableType);
        }

        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            throw new NotSupportedException();
        }

        public static void ReplaceFilter(string filterName, IList<FilterHtmlGenerator.Filter> filters, params Expression<Func<TTable, string>>[] fields)
        {
            var filter = filters.OfType<BaseFilterParameter>().Select((r, ind) => new {ind, r}).FirstOrDefault(r => r.r.FilterName == filterName);
            if (filter == null)
            {
                foreach (var childFilter in filters)
                {
                    filter = childFilter.Children.OfType<BaseFilterParameter>().Select((r, ind) => new { ind, r }).FirstOrDefault(r => r.r.FilterName == filterName);
                    if (filter != null)
                    {
                        filters = childFilter.Children;
                        break;
                    }
                }
            }

            if (filter == null)
                throw new ArgumentOutOfRangeException(nameof(filterName));

            filters.RemoveAt(filter.ind);
            var newFilter = new BaseFilterParameterStringSearch<TTable>(fields)
                {
                    AllowAddFilter = filter.r.AllowAddFilter,
                    AllowedFilterTypes = filter.r.AllowedFilterTypes,
                    Enabled = filter.r.Enabled,
                    FilterName = filter.r.FilterName,
                    IsJournalFilter = filter.r.IsJournalFilter,
                    Header = filter.r.Header,
                    MainGroup = filter.r.MainGroup,
                    Mandatory = filter.r.Mandatory,
                    MaxLength = filter.r.MaxLength,
                    Type = filter.r.Type,
                    Visible = filter.r.Visible,
                    VisiblePermissions = filter.r.VisiblePermissions,
                    Width = filter.r.Width,
                    Height = filter.r.Height
                };
            filters.Insert(filter.ind, newFilter);
            newFilter.SetStandartHandler();
        }
    }
}