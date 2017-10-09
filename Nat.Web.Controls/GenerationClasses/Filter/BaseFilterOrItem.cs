namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Data.Linq;
    using System.Linq.Expressions;

    public class BaseFilterOrItem<TDataContext, TTable, TKey> : IBaseFilterOrItem
        where TDataContext : DataContext
        where TTable : class
    {
        public BaseFilterOrItem(Expression<Func<TDataContext, TTable, TKey, bool>> expression, TKey value)
        {
            Value = value;
            Expression = expression;
        }

        public TKey Value { get; set; }
        public Expression<Func<TDataContext, TTable, TKey, bool>> Expression { get; set; }

        object IBaseFilterOrItem.FilterValue
        {
            get { return Value; }
        }

        Type IBaseFilterOrItem.FilterValueType
        {
            get { return typeof(TKey); }
        }

        Expression IBaseFilterOrItem.FilterExpression
        {
            get { return Expression; }
        }
    }
}