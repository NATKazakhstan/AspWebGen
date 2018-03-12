namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Data.Linq.SqlClient;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterString<TTable> : BaseFilterParameter<TTable>
        where TTable : class
    {
        public bool AllowEqualsOperations { get; set; } = true;

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
            if (AllowEqualsOperations)
                expression = Expression.And(expression, Expression.NotEqual(field, Expression.Constant("", field.Type)));
            else
            {
                expression = Expression.And(
                    expression,
                    Expression.Not(Expression.Call(typeof(SqlMethods), "Like", new Type[0], field, Expression.Constant(""))));
            }
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected override void IsNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.Equal(field, Expression.Constant(null, field.Type));
            if (AllowEqualsOperations)
                expression = Expression.Or(expression, Expression.Equal(field, Expression.Constant("", field.Type)));
            else
            {
                expression = Expression.Or(
                    expression,
                    Expression.Call(typeof(SqlMethods), "Like", new Type[0], field, Expression.Constant("")));
            }

            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected override void EqualsExpression(string strValue, Type tableType)
        {
            if (AllowEqualsOperations)
                base.EqualsExpression(strValue, tableType);
            else if (!string.IsNullOrEmpty(strValue))
            {
                var param = Expression.Parameter(tableType, "bFilter");
                var value = ConvertToFieldType(strValue);
                var field = InvokeValueExpression(param);
                var valueExp = QueryParameters.GetExpression(field + ".Equals", value, field.Type);
                SetWhereExpression(Expression.Call(typeof(SqlMethods), "Like", new Type[0], field, valueExp), param);
            }
        }

        protected override void NotEqualsExpression(string strValue, Type tableType)
        {
            if (AllowEqualsOperations)
                base.NotEqualsExpression(strValue, tableType);
            else if (!string.IsNullOrEmpty(strValue))
            {
                var param = Expression.Parameter(tableType, "bFilter");
                var value = ConvertToFieldType(strValue);
                var field = InvokeValueExpression(param);
                var valueExp = QueryParameters.GetExpression(field + ".Equals", value, field.Type);
                SetWhereExpression(Expression.Not(Expression.Call(typeof(SqlMethods), "Like", new Type[0], field, valueExp)), param);
            }
        }
    }
}