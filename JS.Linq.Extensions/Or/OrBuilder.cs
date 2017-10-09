namespace JS.Linq.Extensions.Or
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class OrBuilder<TSource> : IOrBuilder<TSource>
    {
        private readonly IQueryable<TSource> source;

        private readonly List<Expression<Func<TSource, bool>>> predicates = new List<Expression<Func<TSource, bool>>>();

        public OrBuilder(IQueryable<TSource> source)
        {
            this.source = source;
        }

        public IOrBuilder<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            predicates.Add(predicate);
            return this;
        }

        public IQueryable<TSource> EndOr()
        {
            if (predicates.Count == 0)
                return source;

            var parameter = Expression.Parameter(typeof(TSource), "orBuilder");
            Expression exp = Expression.Invoke(predicates[0], parameter);
            foreach (var expression in predicates.Skip(1))
                exp = Expression.Or(exp, Expression.Invoke(expression, parameter));
            var lambda = Expression.Lambda<Func<TSource, bool>>(exp, parameter);
            return source.Where(lambda);
        }
    }
}