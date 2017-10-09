namespace JS.LinqToJavaScript
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using LinqToJavaScript.ExpressionVisitors;

    public class LinqToJavaScriptProvider : IQueryProvider
    {
        #region Public Methods and Operators

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new LinqToJavaScriptQuery<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return
                    (IQueryable)Activator.CreateInstance(
                        typeof(LinqToJavaScriptQuery<>).MakeGenericType(elementType),
                        new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public string GetCreateClassScript<T>(T obj)
        {
            Expression<Func<T, T>> expression = r => r.CreateClassScript();
            var visitor = new LinqToJavaScriptExpressionVisitor();
            var exp = Expression.Invoke(expression, Expression.Constant(obj));
            return visitor.Translate(exp);
        }

        public string GetDeclareAllClassScript<T>(T obj)
        {
            Expression<Func<T, T>> expression = r => r.DeclareAllClassScript();
            var visitor = new LinqToJavaScriptExpressionVisitor();
            var exp = Expression.Invoke(expression, Expression.Constant(obj));
            return visitor.Translate(exp);
        }

        public string GetDeclareClassScript<T>(T obj)
        {
            Expression<Func<T, T>> expression = r => r.DeclareClassScript();
            var visitor = new LinqToJavaScriptExpressionVisitor();
            var exp = Expression.Invoke(expression, Expression.Constant(obj));
            return visitor.Translate(exp);
        }

        public string GetQueryText(Expression expression)
        {
            return new LinqToJavaScriptExpressionVisitor().Translate(expression);
        }

        #endregion
    }
}