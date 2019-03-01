namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;

    public class BaseFilterParameterPeriods<TTable, TField> : BaseFilterParameter<TTable, TField>
        where TTable : class
        where TField : struct
    {
        public Expression<Func<TTable, TField?>> ValueStartExpression { get; }
        public Expression<Func<TTable, TField?>> ValueEndExpression { get; }

        public BaseFilterParameterPeriods(Expression<Func<TTable, TField?>> valueStartExpression, Expression<Func<TTable, TField?>> valueEndExpression)
            : base(valueStartExpression)
        {
            ValueStartExpression = valueStartExpression;
            ValueEndExpression = valueEndExpression;
        }

        protected override void BetweenExpression(string strValue1, string strValue2, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue1) || string.IsNullOrEmpty(strValue2))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value1 = ConvertToFieldType(strValue1);
            var value2 = ConvertToFieldTypeToEndDay(strValue2);
            var fieldEnd = Expression.Invoke(ValueEndExpression, param);
            var fieldStart = Expression.Invoke(ValueStartExpression, param);
            var valueExp1 = QueryParameters.GetExpression(fieldEnd + ".GreaterThanOrEqual", value1, fieldEnd.Type);
            var valueExp2 = QueryParameters.GetExpression(fieldStart + ".LessThanOrEqual", value2, fieldStart.Type);
            var exp1 = Expression.GreaterThanOrEqual(fieldEnd, valueExp1);
            var exp2 = Expression.LessThanOrEqual(fieldStart, valueExp2);
            SetWhereExpression(Expression.And(exp1, exp2), param);
        }

        protected override void EqualsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var fieldEnd = Expression.Invoke(ValueEndExpression, param);
            var fieldStart = Expression.Invoke(ValueStartExpression, param);
            var valueExp = QueryParameters.GetExpression(fieldEnd + ".GreaterThanOrEqual", value, fieldEnd.Type);
            var exp1 = Expression.GreaterThanOrEqual(fieldEnd, valueExp);
            var exp2 = Expression.LessThanOrEqual(fieldStart, valueExp);
            SetWhereExpression(Expression.And(exp1, exp2), param);
        }

        protected override void MoreOrEqualExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var fieldEnd = Expression.Invoke(ValueEndExpression, param);
            var valueExp = QueryParameters.GetExpression(fieldEnd + ".MoreOrEqual", value, fieldEnd.Type);
            var exp1 = Expression.GreaterThanOrEqual(fieldEnd, valueExp);
            SetWhereExpression(exp1, param);
        }
    }
}