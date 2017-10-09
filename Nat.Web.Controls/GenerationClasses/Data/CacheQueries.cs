/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.06.01
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.GenerationClasses.Data
{
    using System;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;

    using Nat.Web.Controls.HistoryFilters;

    public static class CacheQueries
    {
        public static SimpleCache SimpleCache { get; set; }

        public static TResult GetNameCached<TResult>(Func<TResult> getValue, string key)
        {
            if (HttpContext.Current.Cache[key] != null)
            {
                if (HttpContext.Current.Cache[key] == DBNull.Value)
                    return default(TResult);
                return (TResult)HttpContext.Current.Cache[key];
            }

            var result = getValue();
            if (HttpContext.Current != null)
                HttpContext.Current.Cache[key] = (object)result ?? DBNull.Value;
            return result;
        }

        public static void ClearCache<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
            where TTableType : class
        {
            if (selectExpForCultures != null)
            {
                foreach (var selectExpForCulture in selectExpForCultures)
                    ClearCache(dataContext, value, whereExp, selectExpForCulture.Expression);
            }

            string keyQuery;
            string keyResult;
            GetCacheKey(dataContext, value, whereExp, selectExp, out keyQuery, out keyResult);

            if (HttpContext.Current != null)
                HttpContext.Current.Cache.Remove(keyResult);
            else
                SimpleCache?.Remove(keyResult);
        }

        public static TResult GetNameCached<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
            where TTableType : class
        {
            string keyQuery;
            string keyResult;
            var result = GetValue(
                dataContext, value, whereExp, selectExp, true, out keyQuery, out keyResult, selectExpForCultures);

            if (HttpContext.Current != null)
                HttpContext.Current.Cache[keyResult] = (object)result ?? DBNull.Value;
            else if (SimpleCache != null)
                SimpleCache[keyResult] = (object)result ?? DBNull.Value;

            return result;
        }

        public static TResult ExecuteOrCachedAbsolute30S<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
        where TTableType : class
        {
            string keyQuery;
            string keyResult;
            var result = GetValue(
                dataContext,
                value,
                whereExp,
                selectExp,
                true,
                out keyQuery,
                out keyResult,
                selectExpForCultures);

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    DateTime.Now.AddSeconds(30),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }

            return result;
        }
        
        public static TResult ExecuteOrCachedAbsolute30SWithHistory<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
        where TTableType : class, IFilterPeriodHistory
        {
            string keyQuery;
            string keyResult;
            var result = GetValueWithHistory(
                dataContext,
                value,
                whereExp,
                selectExp,
                true,
                out keyQuery,
                out keyResult,
                selectExpForCultures);

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    DateTime.Now.AddSeconds(30),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }

            return result;
        }

        public static TResult ExecuteOrCachedAbsolute30SWithHistory2<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
        where TTableType : class, IFilterPeriodHistory2
        {
            string keyQuery;
            string keyResult;
            var result = GetValueWithHistory2(
                dataContext,
                value,
                whereExp,
                selectExp,
                true,
                out keyQuery,
                out keyResult,
                selectExpForCultures);

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    DateTime.Now.AddSeconds(30),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Normal,
                    null);
            }

            return result;
        }

        public static TResult ExecuteOrCachedSliding1S<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
        where TTableType : class
        {
            string keyQuery;
            string keyResult;
            var result = GetValue(
                dataContext,
                value,
                whereExp,
                selectExp,
                true,
                out keyQuery,
                out keyResult,
                selectExpForCultures);

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, 0, 1),
                    CacheItemPriority.Normal,
                    null);
            }

            return result;
        }

        public static TResult GetNameCached<TTableType, TKey, TResult>(
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration,
            CacheItemPriority priority,
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
            where TTableType : class
        {
            // cache of query: dataContext.GetTable<TTableType>().Where(whereT).Select(selectExp).FirstOrDefault();
            string keyQuery;
            string keyResult;
            var result = GetValue(dataContext, value, whereExp, selectExp, true, out keyQuery, out keyResult, selectExpForCultures);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    absoluteExpiration,
                    slidingExpiration,
                    priority,
                    null);
            }

            return result;
        }

        public static TResult ExecuteFunction<TTableType, TKey, TResult>(
             DataContext dataContext,
             TKey value,
             Expression<Func<TTableType, TKey, bool>> whereExp,
             Expression<Func<TTableType, TResult>> selectExp,
             params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
             where TTableType : class
        {
            // cache of query: dataContext.GetTable<TTableType>().Where(whereT).Select(selectExp).FirstOrDefault();
            string keyQuery;
            string keyResult;
            var result = GetValue(dataContext, value, whereExp, selectExp, false, out keyQuery, out keyResult, selectExpForCultures);
            return
                result;
        }

        public static TResult ExecuteFunction<TDataContext, TTableType, TKey, TResult>(
            TDataContext dataContext,
            TKey value,
            Expression<Func<TDataContext, TTableType, TKey, bool>> whereExp,
            Expression<Func<TDataContext, TTableType, TKey, TResult>> selectExp,
            params ExpressionCulture<Func<TDataContext, TTableType, TKey, TResult>>[] selectExpForCultures)
            where TTableType : class
            where TDataContext : DataContext
        {
            // cache of query: dataContext.GetTable<TTableType>().Where(whereT).Select(selectExp).FirstOrDefault();
            string keyQuery;
            string keyResult;
            var result = GetValue(
                dataContext, value, whereExp, selectExp, false, out keyQuery, out keyResult, selectExpForCultures);
            return result;
        }

        /// <summary>
        /// Метод для получения в запросах файлов, сейчас использует кэш прописаный в коде на 5 секунд от последнего обращения
        /// В будущем предпологается настройка в конфиге
        /// </summary>
        /// <typeparam name="TTableType"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dataContext"></param>
        /// <param name="value"></param>
        /// <param name="whereExp"></param>
        /// <param name="selectExp"></param>
        /// <param name="selectExpForCultures"></param>
        /// <returns></returns>
        public static TResult GetFile<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
            where TTableType : class
        {
            string keyQuery;
            string keyResult;
            var result = GetValue(
                dataContext, value, whereExp, selectExp, true, out keyQuery, out keyResult, selectExpForCultures);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Cache.Add(
                    keyResult,
                    (object)result ?? DBNull.Value,
                    null,
                    Cache.NoAbsoluteExpiration,
                    new TimeSpan(0, 0, 5),
                    CacheItemPriority.Normal,
                    null);
            }

            return result;
        }

        private static TResult GetValue<TTableType, TKey, TResult>(
           DataContext dataContext,
           TKey value,
           Expression<Func<TTableType, TKey, bool>> whereExp,
           Expression<Func<TTableType, TResult>> selectExp,
           bool useCacheValue,
           out string keyQuery,
           out string keyResult,
           params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
           where TTableType : class
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            selectExp = selectExpForCultures
                .Where(r => r.CultureName.Equals(currentCulture, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Expression)
                .FirstOrDefault() ?? selectExp;

            GetCacheKey(dataContext, value, whereExp, selectExp, out keyQuery, out keyResult);

            if (value == null) return default(TResult);

            Func<DataContext, TKey, TResult> func;

            if (HttpContext.Current != null || SimpleCache != null)
            {
                if (useCacheValue)
                {
                    var obj = HttpContext.Current != null ? HttpContext.Current.Cache[keyResult] : SimpleCache[keyResult];
                    if (obj == DBNull.Value) return default(TResult);
                    if (obj != null) return (TResult)obj;
                }

                func = (Func<DataContext, TKey, TResult>)
                       (HttpContext.Current != null ? HttpContext.Current.Cache[keyQuery] : SimpleCache[keyQuery]);
                if (func != null)
                    return func(dataContext, value);
            }

            var paramT = Expression.Parameter(typeof(TTableType), "tnc");
            var paramKey = Expression.Parameter(typeof(TKey), "tnk");
            var exp = Expression.Invoke(whereExp, paramT, paramKey);
            var whereT = Expression.Lambda<Func<TTableType, bool>>(exp, paramT);

            Expression<Func<DataContext, IQueryable<TTableType>>> getTable =
                dc => (IQueryable<TTableType>)dc.GetTable<TTableType>();

            var paramDc = Expression.Parameter(typeof(DataContext), "dcnc");
            Expression resultExp = Expression.Invoke(getTable, paramDc);
            resultExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTableType) }, resultExp, whereT);
            resultExp = Expression.Call(
                typeof(Queryable), "Select", new[] { typeof(TTableType), typeof(TResult) }, resultExp, selectExp);
            resultExp = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { typeof(TResult) }, resultExp);

            var resultLambda = Expression.Lambda<Func<DataContext, TKey, TResult>>(resultExp, paramDc, paramKey);
            func = CompiledQuery.Compile(resultLambda);
            var result = func(dataContext, value);
            if (HttpContext.Current != null)
                HttpContext.Current.Cache[keyQuery] = func;
            return result;
        }

        private static void GetCacheKey<TTableType, TKey, TResult>(
            DataContext dataContext,
            TKey value,
            Expression<Func<TTableType, TKey, bool>> whereExp,
            Expression<Func<TTableType, TResult>> selectExp,
            out string keyQuery,
            out string keyResult)
            where TTableType : class
        {
            keyQuery = dataContext.GetType().FullName
                       + "#" + typeof(TTableType).FullName
                       + "#" + typeof(TResult).FullName
                       + "#" + typeof(TKey).FullName
                       + "#" + whereExp
                       + "#" + selectExp;
            keyResult = keyQuery + "#" + value;
        }

        private static TResult GetValue<TDataContext, TTableType, TKey, TResult>(
           TDataContext dataContext,
           TKey value,
           Expression<Func<TDataContext, TTableType, TKey, bool>> whereExp,
           Expression<Func<TDataContext, TTableType, TKey, TResult>> selectExp,
           bool useCacheValue,
           out string keyQuery,
           out string keyResult,
           params ExpressionCulture<Func<TDataContext, TTableType, TKey, TResult>>[] selectExpForCultures)
           where TTableType : class
           where TDataContext : DataContext
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            selectExp = selectExpForCultures
                .Where(r => r.CultureName.Equals(currentCulture, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Expression)
                .FirstOrDefault() ?? selectExp;

            keyQuery = dataContext.GetType().FullName
                        + "#" + typeof(TTableType).FullName
                        + "#" + typeof(TResult).FullName 
                        + "#" + typeof(TKey).FullName
                        + "#" + typeof(TDataContext).FullName 
                        + "#" + whereExp 
                        + "#" + selectExp;
            keyResult = keyQuery + "#" + value;

            if (value == null) return default(TResult);

            Func<TDataContext, TKey, TResult> func;

            if (HttpContext.Current != null || SimpleCache != null)
            {
                if (useCacheValue)
                {
                    var obj = HttpContext.Current != null ? HttpContext.Current.Cache[keyResult] : SimpleCache[keyResult];
                    if (obj == DBNull.Value) return default(TResult);
                    if (obj != null) return (TResult)obj;
                }

                func = (Func<TDataContext, TKey, TResult>)
                       (HttpContext.Current != null ? HttpContext.Current.Cache[keyQuery] : SimpleCache[keyQuery]);
                if (func != null)
                    return func(dataContext, value);
            }

            var paramDc = Expression.Parameter(typeof(TDataContext), "dcnc");
            var paramT = Expression.Parameter(typeof(TTableType), "tnc");
            var paramKey = Expression.Parameter(typeof(TKey), "tnk");
            var exp = Expression.Invoke(whereExp, paramDc, paramT, paramKey);
            var whereT = Expression.Lambda<Func<TTableType, bool>>(exp, paramT);
            exp = Expression.Invoke(selectExp, paramDc, paramT, paramKey);
            var selectT = Expression.Lambda<Func<TTableType, TResult>>(exp, paramT);

            Expression<Func<TDataContext, IQueryable<TTableType>>> getTable =
                dc => (IQueryable<TTableType>)dc.GetTable<TTableType>();

            Expression resultExp = Expression.Invoke(getTable, paramDc);
            resultExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTableType) }, resultExp, whereT);
            resultExp = Expression.Call(
                typeof(Queryable), "Select", new[] { typeof(TTableType), typeof(TResult) }, resultExp, selectT);
            resultExp = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { typeof(TResult) }, resultExp);

            var resultLambda = Expression.Lambda<Func<TDataContext, TKey, TResult>>(resultExp, paramDc, paramKey);
            func = CompiledQuery.Compile(resultLambda);
            var result = func(dataContext, value);
            if (HttpContext.Current != null)
                HttpContext.Current.Cache[keyQuery] = func;
            return result;
        }

        private static TResult GetValueWithHistory<TTableType, TKey, TResult>(
           DataContext dataContext,
           TKey value,
           Expression<Func<TTableType, TKey, bool>> whereExp,
           Expression<Func<TTableType, TResult>> selectExp,
           bool useCacheValue,
           out string keyQuery,
           out string keyResult,
           params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
           where TTableType : class, IFilterPeriodHistory
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            selectExp = selectExpForCultures
                .Where(r => r.CultureName.Equals(currentCulture, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Expression)
                .FirstOrDefault() ?? selectExp;

            keyQuery = dataContext.GetType().FullName
                        + "#" + typeof(TTableType).FullName
                        + "#" + typeof(TResult).FullName
                        + "#" + typeof(TKey).FullName
                        + "#" + whereExp
                        + "#" + selectExp
                        + "#WithPeriodHistory";
            keyResult = keyQuery + "#" + value;

            if (value == null) return default(TResult);

            Func<DataContext, TKey, DateTime, TResult> func;

            if (HttpContext.Current != null)
            {
                if (useCacheValue)
                {
                    var obj = HttpContext.Current.Cache[keyResult];
                    if (obj == DBNull.Value) return default(TResult);
                    if (obj != null) return (TResult)obj;
                }

                func = (Func<DataContext, TKey, DateTime, TResult>)HttpContext.Current.Cache[keyQuery];
                if (func != null)
                    return func(dataContext, value, DateTime.Now);
            }

            var paramT = Expression.Parameter(typeof(TTableType), "tnc");
            var paramKey = Expression.Parameter(typeof(TKey), "tnk");
            var exp = Expression.Invoke(whereExp, paramT, paramKey);
            var whereT = Expression.Lambda<Func<TTableType, bool>>(exp, paramT);

            Expression<Func<DataContext, DateTime, IQueryable<TTableType>>> getTable =
                (dc, datetime) =>
                dc.GetTable<TTableType>().Where(
                    r => r.dateStart < datetime && (r.dateEnd == null || r.dateEnd >= datetime));

            var paramDc = Expression.Parameter(typeof(DataContext), "dcnc");
            var paramDateTime = Expression.Parameter(typeof(DateTime), "datetime");
            Expression resultExp = Expression.Invoke(getTable, paramDc, paramDateTime);
            resultExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTableType) }, resultExp, whereT);
            resultExp = Expression.Call(
                typeof(Queryable), "Select", new[] { typeof(TTableType), typeof(TResult) }, resultExp, selectExp);
            resultExp = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { typeof(TResult) }, resultExp);

            var resultLambda = Expression.Lambda<Func<DataContext, TKey, DateTime, TResult>>(resultExp, paramDc, paramKey, paramDateTime);
            func = CompiledQuery.Compile(resultLambda);
            var result = func(dataContext, value, DateTime.Now);
            if (HttpContext.Current != null)
                HttpContext.Current.Cache[keyQuery] = func;
            return result;
        }

        private static TResult GetValueWithHistory2<TTableType, TKey, TResult>(
           DataContext dataContext,
           TKey value,
           Expression<Func<TTableType, TKey, bool>> whereExp,
           Expression<Func<TTableType, TResult>> selectExp,
           bool useCacheValue,
           out string keyQuery,
           out string keyResult,
           params ExpressionCulture<Func<TTableType, TResult>>[] selectExpForCultures)
           where TTableType : class, IFilterPeriodHistory2
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture.Name;
            selectExp = selectExpForCultures
                .Where(r => r.CultureName.Equals(currentCulture, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Expression)
                .FirstOrDefault() ?? selectExp;

            keyQuery = dataContext.GetType().FullName
                        + "#" + typeof(TTableType).FullName
                        + "#" + typeof(TResult).FullName
                        + "#" + typeof(TKey).FullName
                        + "#" + whereExp
                        + "#" + selectExp
                        + "#WithPeriodHistory2";
            keyResult = keyQuery + "#" + value;

            if (value == null) return default(TResult);

            Func<DataContext, TKey, DateTime, TResult> func;

            if (HttpContext.Current != null)
            {
                if (useCacheValue)
                {
                    var obj = HttpContext.Current.Cache[keyResult];
                    if (obj == DBNull.Value) return default(TResult);
                    if (obj != null) return (TResult)obj;
                }

                func = (Func<DataContext, TKey, DateTime, TResult>)HttpContext.Current.Cache[keyQuery];
                if (func != null)
                    return func(dataContext, value, DateTime.Now);
            }

            var paramT = Expression.Parameter(typeof(TTableType), "tnc");
            var paramKey = Expression.Parameter(typeof(TKey), "tnk");
            var exp = Expression.Invoke(whereExp, paramT, paramKey);
            var whereT = Expression.Lambda<Func<TTableType, bool>>(exp, paramT);

            Expression<Func<DataContext, DateTime, IQueryable<TTableType>>> getTable =
                (dc, datetime) =>
                dc.GetTable<TTableType>().Where(
                    r => r.DateStart < datetime && (r.DateEnd == null || r.DateEnd >= datetime));

            var paramDc = Expression.Parameter(typeof(DataContext), "dcnc");
            var paramDateTime = Expression.Parameter(typeof(DateTime), "datetime");
            Expression resultExp = Expression.Invoke(getTable, paramDc, paramDateTime);
            resultExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTableType) }, resultExp, whereT);
            resultExp = Expression.Call(
                typeof(Queryable), "Select", new[] { typeof(TTableType), typeof(TResult) }, resultExp, selectExp);
            resultExp = Expression.Call(typeof(Queryable), "FirstOrDefault", new[] { typeof(TResult) }, resultExp);

            var resultLambda = Expression.Lambda<Func<DataContext, TKey, DateTime, TResult>>(resultExp, paramDc, paramKey, paramDateTime);
            func = CompiledQuery.Compile(resultLambda);
            var result = func(dataContext, value, DateTime.Now);
            if (HttpContext.Current != null)
                HttpContext.Current.Cache[keyQuery] = func;
            return result;
        }
    }

    public class ExpressionCulture
    {
        public ExpressionCulture()
        {
        }

        public ExpressionCulture(string cultureName, Expression expression)
        {
            CultureName = cultureName;
            Expression = expression;
        }

        public string CultureName { get; set; }
        public Expression Expression { get; set; }
    }

    public class ExpressionCulture<TDelegate>
    {
        public ExpressionCulture()
        {
        }

        public ExpressionCulture(string cultureName, Expression<TDelegate> expression)
        {
            CultureName = cultureName;
            Expression = expression;
        }

        public string CultureName { get; set; }
        public Expression<TDelegate> Expression { get; set; }
    }
}