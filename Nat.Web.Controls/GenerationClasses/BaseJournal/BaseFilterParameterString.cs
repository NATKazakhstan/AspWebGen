namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterString<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public BaseFilterParameterString()
        {
        }

        public BaseFilterParameterString(Expression<Func<TTable, string>> valueExpression)
        {
            ValueExpression = valueExpression;
        }

        public BaseFilterParameterString(
            Expression<Func<TTable, string>> valueExpression,
            Expression<Func<TTable, string>> valueTextExpressionRu,
            Expression<Func<TTable, string>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;
            ValueTextExpressionRu = valueTextExpressionRu;
            ValueTextExpressionKz = valueTextExpressionKz;
        }

        public Expression<Func<TTable, string>> ValueExpression { get; set; }

        public Expression<Func<TTable, string>> ValueExpressionSecond { get; set; }

        public Expression<Func<TTable, string>> ValueTextExpressionRu { get; set; }

        public Expression<Func<TTable, string>> ValueTextExpressionKz { get; set; }

        public override Expression OValueExpression => ValueExpression;

        public override Expression OValueExpressionSecond => ValueExpressionSecond;

        public override Expression OTextValueExpressionKz => ValueTextExpressionKz ?? base.OTextValueExpressionKz;

        public override Expression OTextValueExpressionRu => ValueTextExpressionRu ?? base.OTextValueExpressionRu;

        protected override Type FieldType => typeof(string);

        protected override void IsNotNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.NotEqual(field, Expression.Constant(null, field.Type));
            expression = Expression.And(expression, Expression.NotEqual(field, Expression.Constant("", field.Type)));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected override void IsNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.Equal(field, Expression.Constant(null, field.Type));
            expression = Expression.Or(expression, Expression.Equal(field, Expression.Constant("", field.Type)));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }
    }
}