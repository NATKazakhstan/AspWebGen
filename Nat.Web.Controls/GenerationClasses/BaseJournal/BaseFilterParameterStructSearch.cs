namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using JS.Linq.Extensions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterStructSearch<TTable, TField> : BaseFilterParameter<TTable>
        where TTable : class
        where TField : struct
    {
        private readonly List<BaseFilterParameter<TTable, TField>> innerFilters;

        public BaseFilterParameterStructSearch(params Expression<Func<TTable, TField?>>[] fields)
        {
            innerFilters = fields.Select(r => new BaseFilterParameter<TTable, TField>(r) { IsJournalFilter = true }).ToList();
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
    }
}