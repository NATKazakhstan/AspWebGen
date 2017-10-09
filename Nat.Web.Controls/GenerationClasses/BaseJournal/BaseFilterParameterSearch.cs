namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterSearch<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public const string DefaultFilterName = "_Search";
        public const string SearchQueryParameter = "__search";

        public BaseFilterParameterSearch()
        {
            Visible = false;
            AllowAddFilter = false;
            Type = FilterHtmlGenerator.FilterType.Text;
            FilterName = DefaultFilterName;
        }

        public BaseFilterParameterSearch(params Expression<Func<TTable, string>>[] valueExpressions)
            : this((IEnumerable<Expression<Func<TTable, string>>>)valueExpressions)
        {
        }

        public BaseFilterParameterSearch(IEnumerable<Expression<Func<TTable, string>>> valueExpressions)
            : this()
        {
            ValueExpressions = valueExpressions.ToArray();
        }

        public BaseFilterParameterSearch(params Expression<Func<TTable, string, bool>>[] valueExpressions)
            : this((IEnumerable<Expression<Func<TTable, string, bool>>>)valueExpressions)
        {
        }

        public BaseFilterParameterSearch(IEnumerable<Expression<Func<TTable, string, bool>>> valueExpressions)
            : this()
        {
            ValueInvokeExpressions = valueExpressions.ToArray();
        }

        public Expression<Func<TTable, string>>[] ValueExpressions { get; private set; }

        public Expression<Func<TTable, string, bool>>[] ValueInvokeExpressions { get; private set; }

        protected override Type FieldType
        {
            get { return typeof(string); }
        }

        protected internal override Expression OnFilter(Enum filtertype, Filter.FilterItem filterItem, Filter.QueryParameters queryParameters)
        {
            QueryParameters = queryParameters;

            var textFilterType = (DefaultFilters.TextFilter)filtertype;
            switch (textFilterType)
            {
                case DefaultFilters.TextFilter.Non:
                    break;
                case DefaultFilters.TextFilter.ContainsWords:

                    ContainsWordsExpression(filterItem.Value1, TableType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return InnerWhereExpression;
        }
        
        protected override void ContainsWordsExpression(string strValue, Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            if ((ValueExpressions == null || ValueExpressions.Length == 0)
                && (ValueInvokeExpressions == null || ValueInvokeExpressions.Length == 0))
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");

            Expression filter = null;
            var split = strValue.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                Expression expression = null;
                if (ValueExpressions != null)
                {
                    foreach (var valueExpression in ValueExpressions)
                    {
                        var field = Expression.Invoke(valueExpression, param);
                        var valueExp = QueryParameters.GetExpression(field + ".Contains", split[i], field.Type);
                        var exp = (Expression)Expression.Call(field, "Contains", new Type[] { }, valueExp);
                        expression = expression == null ? exp : Expression.Or(expression, exp);
                    }
                }

                if (ValueInvokeExpressions != null)
                {
                    foreach (var invokeExpression in ValueInvokeExpressions)
                    {
                        var valueExp = QueryParameters.GetExpression("Constant", split[i], typeof(string));
                        Expression exp = Expression.Invoke(invokeExpression, param, valueExp);
                        expression = expression == null ? exp : Expression.Or(expression, exp);
                    }
                }

                if (expression != null)
                {
                    filter = filter == null
                                 ? expression
                                 : Expression.And(filter, expression);
                }
            }

            if (filter != null)
                SetWhereExpression(filter, param);
        }

        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            throw new NotSupportedException();
        }
    }
}