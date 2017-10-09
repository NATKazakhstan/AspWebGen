using System;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq;
using System.Linq.Expressions;

using Nat.Web.Controls.Trace;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    /*public class CustomCollection<TKey> : ICollection
        where TKey : struct
    {
        object[] _array;

        public CustomCollection(object[] array)
        {
            _array = array;
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            _array.CopyTo(array, index);
        }

        public int Count
        {
            get { return _array.Length; }
        }

        public bool IsSynchronized
        {
            get { return _array.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return _array.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        #endregion

        public TKey? this[int index]
        {
            get
            {
                return (TKey?)_array[index];
            }
        }
    }*/

    public abstract class QueryParameters
    {
        protected QueryParameters(ParameterExpression parameterExpression, ParameterExpression dbParameterExpression)
        {
            ParameterExpression = parameterExpression;
            DBParameterExpression = dbParameterExpression;
            ParametersNames = new List<string>(8);
            ParametersValues = new List<object>(8);
            Messages = new List<string>();
            //SupportParameterExpression = true;
        }
        
        public List<string> ParametersNames { get; set; }
        public List<object> ParametersValues { get; set; }
        public bool AllowPaging { get; set; }
        public bool AllowFileterByRefParent { get; set; }
        public bool AllowFileterByRefParentIsNull { get; set; }
        public bool SelectJournalRow { get; set; }
        public bool SupportParameterExpression { get; set; }

        public int StartRowIndex { get; set; }
        public int MaximumRows { get; set; }
        public string OrderBy { get; set; }
        public string TableName { get; set; }
        public abstract Type QParamTableType { get; }
        public object RefParent { get; set; }
        public string Action { get; set; }
        public string SelectColumnName { get; set; }
        public List<string> Messages { get; private set; }

        public ParameterExpression ParameterExpression { get; private set; }
        public ParameterExpression DBParameterExpression { get; private set; }
        public Expression CurrentExpression { get; set; }

        public DataContext InternalDB { get; set; }

        public abstract string GetCacheKey<TResult>();

        public StructQueryParameters GetExecuteParameter()
        {
            return new StructQueryParameters
            {
                ParametersValues = ParametersValues.ToArray(),
                StartRowIndex = StartRowIndex,
                MaximumRows = MaximumRows,
                RefParent = RefParent,
            };
        }

        public void AddUniqueParameter(Expression expression)
        {
            ParametersNames.Add(expression.ToString());
        }

        public void AddUniqueParameter(string parameterKey)
        {
            ParametersNames.Add(parameterKey);
        }

        public void AddMessage(string message)
        {
            Messages.Add(message);
        }

        public abstract IQueryable<TResult> CreateQuery<TResult>();
        public abstract IQueryable<TResult> CreateQuery<TResult>(Expression expression);
        public abstract IQueryable CreateQuery(Expression expression);
        public abstract IQueryable<TResult> GetCompiled<TResult>(bool supportGlobalCache);
        public abstract IQueryable<TResult> GetCompiled<TResult>(Expression expression, bool supportGlobalCache);

        private readonly Dictionary<Type, bool> _isFiltered = new Dictionary<Type, bool>();

        public bool IsFiltered(Type type)
        {
            return _isFiltered.ContainsKey(type);
        }

        public void Filtered(Type type)
        {
            _isFiltered[type] = true;
        }
    }

    public class QueryParameters<TDataContext, TTable> : QueryParameters
        where TDataContext : DataContext
        where TTable : class
    {
        public QueryParameters(TDataContext db)
            : base(Expression.Parameter(typeof(StructQueryParameters), "qParam"), Expression.Parameter(typeof(TDataContext), "dbParam"))
        {
            DB = db;
        }

        public QueryParameters(ParameterExpression parameterExpression, ParameterExpression dbParameterExpression) : base(parameterExpression, dbParameterExpression)
        {
        }

        public override Type QParamTableType
        {
            get { return typeof(TTable); }
        }

        public override string GetCacheKey<TResult>()
        {
            var sb = new StringBuilder();
            sb.Append(typeof(TDataContext).FullName);
            sb.Append(";");
            sb.Append(typeof(TTable).FullName);
            sb.Append(";");
            sb.Append(typeof(TResult).FullName);
            sb.Append(";");
            sb.Append(TableName);
            sb.Append(";");
            sb.Append(Action);
            sb.Append(";");
            sb.Append(SelectColumnName);
            sb.Append(";");
            if (SelectJournalRow)
                sb.Append("Row;");
            if (AllowPaging)
                sb.Append("Paging;");
            if (AllowFileterByRefParent)
                sb.Append("FilterByRefParent;");
            if (AllowFileterByRefParentIsNull)
                sb.Append("FilterByRefParentIsNull;");
            sb.Append("OrderBy:");
            sb.Append(OrderBy);
            sb.Append(";");
            sb.Append("Filter:");
            sb.Append(string.Join(",", ParametersNames.ToArray()));
            return sb.ToString();
        }

        public Expression GetTable()
        {
            return Expression.Call(DBParameterExpression, "GetTable", new[] {typeof (TTable)});
        }

        public TDataContext DB { get; internal set; }

        public override IQueryable<TResult> CreateQuery<TResult>()
        {
            return CreateQuery<TResult>(CurrentExpression);
        }

        public override IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TResult>>>(
                expression, DBParameterExpression, ParameterExpression);
            var resultExpression = Expression.Invoke(lambda, Expression.Constant(DB), Expression.Constant(GetExecuteParameter()));
            return ((IQueryable)DB.GetTable<TTable>()).Provider.CreateQuery<TResult>(resultExpression);
        }

        public override IQueryable CreateQuery(Expression expression)
        {
            var lambda = Expression.Lambda(expression, DBParameterExpression, ParameterExpression);
            var resultExpression = Expression.Invoke(lambda, Expression.Constant(DB), Expression.Constant(GetExecuteParameter()));
            return ((IQueryable)DB.GetTable<TTable>()).Provider.CreateQuery(resultExpression);
        }

        public override IQueryable<TResult> GetCompiled<TResult>(bool supportGlobalCache)
        {
            return GetCompiled<TResult>(CurrentExpression, supportGlobalCache);
        }

        public override IQueryable<TResult> GetCompiled<TResult>(Expression expression, bool supportGlobalCache)
        {
            var query = GetCachedQuery<IQueryable<TResult>>(supportGlobalCache);
            if (query == null)
            {
                var lambda = Expression.Lambda<Func<TDataContext, StructQueryParameters, IQueryable<TResult>>>(
                    expression, DBParameterExpression, ParameterExpression);
                HttpContext.Current.Trace.WriteExt("Query", "BeginCompile");
                query = CompiledQuery.Compile(lambda);
                HttpContext.Current.Trace.WriteExt("Query", "EndCompile");
                SetCachedQuery(query, supportGlobalCache);
            }
            HttpContext.Current.Trace.WriteExt("Query", "BeginExecute");
            var results = query(DB, GetExecuteParameter());
            HttpContext.Current.Trace.WriteExt("Query", "EndExecute");
            return results;
        }

        public Func<TDataContext, StructQueryParameters, TResult> GetCachedQuery<TResult>(bool allowGlobalCache)
        {
            return GetQueryCache<TResult>(this, allowGlobalCache);
        }

        public void SetCachedQuery<TResult>(Func<TDataContext, StructQueryParameters, TResult> value, bool allowGlobalCache)
        {
            SetQueryCache(this, value, allowGlobalCache);
        }

        public static Func<TDataContext, StructQueryParameters, TResult> GetQueryCache<TResult>(QueryParameters parameters, bool allowGlobalCache)
        {
            HttpContext.Current.Trace.WriteExt("Query", "BeginGetCachedQuery");
            try
            {
                var key = parameters.GetCacheKey<TResult>();
                var cacheParameters = allowGlobalCache ? CurrentGlobalCacheParameters : CurrentCacheParameters;
                var cache = cacheParameters.GetCache<TResult>(key);
                if (cache == null) return null;
                cache.LastExecution = DateTime.Now;
                return cache.Func;
            }
            finally 
            {
                HttpContext.Current.Trace.WriteExt("Query", "EndGetCachedQuery");
            }
        }

        public static void SetQueryCache<TResult>(QueryParameters parameters, Func<TDataContext, StructQueryParameters, TResult> func, bool allowGlobalCache)
        {
            var key = parameters.GetCacheKey<TResult>();
            var cacheParameters = allowGlobalCache ? CurrentGlobalCacheParameters : CurrentCacheParameters;
            cacheParameters.AddCache(key, func);
        }

        private static CacheParameters CurrentCacheParameters
        {
            get
            {
                var cacheKey = typeof (QueryParameters<TDataContext, TTable>).FullName;
                //todo: use session for cache
                /*var parameters = (CacheParameters)HttpContext.Current.Session["QueryParameters"];
                if (parameters == null)
                    HttpContext.Current.Session["QueryParameters"] = parameters = new CacheParameters();*/
                var parameters = (CacheParameters)HttpContext.Current.Items[cacheKey];
                if (parameters == null)
                    HttpContext.Current.Items[cacheKey] = parameters = new CacheParameters();
                return parameters;
            }
        }

        private static CacheParameters CurrentGlobalCacheParameters
        {
            get
            {
                var cacheKey = typeof(QueryParameters<TDataContext, TTable>).FullName;
                var parameters = (CacheParameters)HttpContext.Current.Cache[cacheKey];
                if (parameters == null)
                    HttpContext.Current.Cache[cacheKey] = parameters = new CacheParameters();
                return parameters;
            }
        }

        private class CacheParameters
        {
            public CacheParameters()
            {
                Cache = new Dictionary<string, CacheParameter>();
                LastExecution = DateTime.Now;
            }

            private Dictionary<string, CacheParameter> Cache { get; set; }

            private DateTime LastExecution { get; set; }

            private object _lock = new object();

            private void ClearCache()
            {
                // что бы не было частого обхода по колекции
                if (LastExecution.AddMinutes(2) > DateTime.Now)
                    return;
                var datetime = DateTime.Now.AddMinutes(-20);
                LastExecution = datetime;
                foreach (var item in Cache.Where(r => r.Value.LastExecution < datetime).ToArray())
                    Cache.Remove(item.Key);
            }

            public void AddCache<TResult>(string key, Func<TDataContext, StructQueryParameters, TResult> func)
            {
                lock (_lock)
                {
                    Cache[key] = new CacheParameter<TResult>
                        {
                            Func = func,
                            LastExecution = DateTime.Now,
                        };
                    ClearCache();
                }
            }

            public CacheParameter<TResult> GetCache<TResult>(string key)
            {
                CacheParameter<TResult> cache = null;
                lock (_lock)
                {
                    if (Cache.ContainsKey(key))
                        cache = (CacheParameter<TResult>)Cache[key];
                    ClearCache();
                }
                return cache;
            }
        }

        private class CacheParameter
        {
            public DateTime LastExecution { get; set; }
        }

        private class CacheParameter<TResult> : CacheParameter
        {
            public Func<TDataContext, StructQueryParameters, TResult> Func { get; set; }
        }
    }

    public class NullQueryParameters<TDataContext, TTable> : QueryParameters<TDataContext, TTable>
        where TDataContext : DataContext
        where TTable : class
    {
        public NullQueryParameters(TDataContext db)
            : base(null, null)
        {
            InternalDB = db;
            DB = db;
        }
    }
}
