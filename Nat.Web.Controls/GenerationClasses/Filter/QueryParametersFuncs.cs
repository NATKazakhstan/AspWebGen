namespace Nat.Web.Controls.GenerationClasses.Filter
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Linq.Expressions;
    using System.Text;

    public static class QueryParametersFuncs
    {
        public static Expression GetDBExpression<TDataContext>(this QueryParameters qParams)
            where TDataContext : DataContext
        {
            if (qParams == null) return Expression.Constant(null, typeof(TDataContext));
            return (Expression)qParams.DBParameterExpression ?? Expression.Constant(qParams.InternalDB);
        }

        public static Expression GetDBExpression(this QueryParameters qParams, DataContext db)
        {
            if (qParams?.DBParameterExpression == null) return Expression.Constant(db);
            return qParams.DBParameterExpression;
        }

        public static void RegisterExpression(this QueryParameters qParams, Expression exp, string value)
        {
            if (qParams != null)
            {
                qParams.ParametersNames.Add(exp.ToString());
                qParams.ParametersNames.Add(value);
            }
        }

        public static void RegisterExpression(this QueryParameters qParams, Expression exp)
        {
            if (qParams != null)
                qParams.ParametersNames.Add(exp.ToString());
        }

        public static void RegisterParameter(this QueryParameters qParams, string value)
        {
            if (qParams != null)
                qParams.ParametersNames.Add(value);
        }

        private static void RegisterParameterName(this QueryParameters qParams, string parameterKey, object value)
        {
            if (qParams != null)
            {
                var collection = value as ICollection;
                string valueString = string.Empty;
                if (value != null)
                    valueString = collection == null ? value.ToString() : GetString(collection);
                qParams.ParametersNames.Add(parameterKey + ":" + valueString);
            }
        }

        private static void RegisterParameterValue(this QueryParameters qParams, object value)
        {
            if (qParams != null)
            {
                var collection = value as ICollection;
                string valueString = string.Empty;
                if (value != null)
                    valueString = collection == null ? value.ToString() : GetString(collection);
                qParams.ParametersValues.Add(valueString);
            }
        }

        public static Expression GetExpression<T>(this QueryParameters qParams, string parameterKey, T value)
        {
            return GetExpression(qParams, parameterKey, value, typeof (T));
        }

        public static Expression GetExpression<T>(this QueryParameters qParams, string parameterKey, T? value)
            where T : struct 
        {
            return GetExpression(qParams, parameterKey, value, typeof (T));
        }

        public static Expression GetExpression<T>(this QueryParameters qParams, Expression expression, T value)
        {
            return GetExpression(qParams, expression.ToString(), value, typeof(T));
        }

        public static Expression GetExpression<T>(this QueryParameters qParams, string parameterKey, T value, Type typeExpression)
        {
            return GetExpressionInternal(qParams, parameterKey, value, typeExpression);
        }

        internal static Expression GetExpressionInternal(this QueryParameters qParams, string parameterKey, object value, Type typeExpression)
        {
            if (qParams == null)
                return Expression.Constant(value, typeExpression);
            if (!qParams.SupportParameterExpression || qParams.ParametersValues.Count >= 20)
            {
                RegisterParameterName(qParams, parameterKey, value);
                return Expression.Constant(value, typeExpression);
            }
            
            var ending = LinqFilterGenerator.IsNullableType(typeExpression)
                             ? typeExpression.GetGenericArguments()[0].Name + "N"
                             : typeExpression.Name;
            if (TypeDescriptor.GetProperties(typeof(StructQueryParameters.Item)).Find(ending, false) == null)
            {
                RegisterParameterName(qParams, parameterKey, value);
                return Expression.Constant(value, typeExpression);
            }

            qParams.ParametersNames.Add(parameterKey);
            qParams.ParametersValues.Add(value);
            var index = qParams.ParametersValues.Count - 1;
            var valueExp = Expression.Property(qParams.ParameterExpression, "Parameter" + index);
            return Expression.Property(valueExp, ending);
        }

        private static string GetString(ICollection coll)
        {
            var sb = new StringBuilder();
            foreach (var value in coll)
                sb.Append(value).Append(";");
            return sb.ToString();
        }

        public static Expression GetExpression(this QueryParameters qParams, string parameterKey, long? value)
        {
            return GetExpression(qParams, parameterKey, value, typeof(long));
        }

        public static Expression GetExpression(this QueryParameters qParams, string parameterKey, object value, Type fieldType)
        {
            return GetExpressionInternal(qParams, parameterKey, value, fieldType);
        }

        public static Expression GetExpression(this QueryParameters qParams, Expression expression, object value, Type fieldType)
        {
            return GetExpressionInternal(qParams, expression.ToString(), value, fieldType);
        }
    }
}
