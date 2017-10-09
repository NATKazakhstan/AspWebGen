using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Data.Linq;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses
{
    public abstract class BaseFilterExpression
    {
        /// <summary>
        /// Лямбда выражение, для фильтрации данных.
        /// </summary>
        protected internal Expression whereExpression { get; set; }

        protected internal Type FilterForType { get; set; }

        /// <summary>
        /// Сообщение описывающее фильтр.
        /// </summary>
        public string Message { get; set; }
    }

    internal class FilterExpressionInternal : BaseFilterExpression
    {
    }

    public class FilterExpression<TTable> : BaseFilterExpression
        where TTable : class
    {
        public FilterExpression()
        {
        }

        public FilterExpression(Expression<Func<TTable, bool>> where, string message)
        {
            Where = where;
            Message = message;
        }

        /// <summary>
        /// Лямбда выражение, для фильтрации данных.
        /// </summary>
        public Expression<Func<TTable, bool>> Where { get; set; }
    }

    public class FilterExpression<TDataContext, TTable> : BaseFilterExpression
        where TTable : class
        where TDataContext : DataContext
    {
        public QueryParameters QueryParameters { get; set; }

        public FilterExpression()
        {
        }

        public FilterExpression(Expression<Func<TDataContext, TTable, bool>> where, string message)
        {
            Where = where;
            Message = message;
        }

        /// <summary>
        /// Лямбда выражение, для фильтрации данных.
        /// </summary>
        public Expression<Func<TDataContext, TTable, bool>> Where { get; set; }
    }
}
