namespace JS.Linq.Extensions.Or
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IOrBuilder<TSource>
    {
        IOrBuilder<TSource> Where(Expression<Func<TSource, bool>> predicate);

        IQueryable<TSource> EndOr();
    }
}