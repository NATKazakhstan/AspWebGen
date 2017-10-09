namespace JS.Linq.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using JS.Linq.Extensions.Or;

    public static class Queryable
    {
        public static IOrBuilder<TSource> BeginOr<TSource>(this IQueryable<TSource> source)
        {
            return new OrBuilder<TSource>(source);
        }

        public static Expression<Func<TTable, bool>> ToOrLambdaExpression<TTable>(this IEnumerable<Expression> expressions)
        {
            var param = Expression.Parameter(typeof(TTable), "orRow");
            var exp = expressions
                .Select(r => (Expression)Expression.Invoke(r, param))
                .Aggregate(Expression.Or);
            return Expression.Lambda<Func<TTable, bool>>(exp, param);
        }
    }
}