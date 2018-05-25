using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses
{
    public abstract class BaseFilterEventArgs : EventArgs
    {
        //public virtual DataContext DataContext { get; set; }
    }

    public class BaseFilterEventArgs<TTable> : BaseFilterEventArgs
        where TTable : class
    {
        internal Expression _where;
        internal MainPageUrlBuilder _url;
        private List<FilterExpression<TTable>> _wheres = new List<FilterExpression<TTable>>();
        private List<JoinFilter> _joinFilters = new List<JoinFilter>(1);

        public BaseFilterEventArgs(MainPageUrlBuilder url)
        {
            _url = url;
            TableParameterExpression = Expression.Parameter(typeof(TTable), "r");
        }

        public QueryParameters QueryParameters { get; protected set; }
        public ParameterExpression TableParameterExpression { get; private set; }
        public Func<FilterDataArgs, Expression> Filters { get; set; }

        /// <summary>
        /// Запрет кеширование фильтра. 
        /// Кеширование используется в рамках одного запроса клиента.
        /// </summary>
        public bool DenyCache { get; set; }

        /// <summary>
        /// Тип таблицы, данные которой будут фильтроваться.
        /// </summary>
        public Type TypeOfData { get; set; }

        public bool CancelTreeUse { get; set; }

        public Type GetTTable()
        {
            return typeof(TTable);
        }

        public bool HasFilter()
        {
            return _where != null || _wheres.Count > 0 || _joinFilters.Count > 0 || Filters != null;
        }

        #region добавление фильтров по лямбда выражению

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter(Expression<Func<TTable, bool>> where)
        {
            if (QueryParameters != null)
                QueryParameters.AddUniqueParameter(where);
            if (_where == null)
                _where = Expression.Invoke(where, TableParameterExpression);
            else
                _where = Expression.And(Expression.Invoke(where, TableParameterExpression), _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        /// <param name="wheres">Выражения фильтрации данных, которые объединяться через Or.</param>
        public void AddFiltersByOr(params Expression<Func<TTable, bool>>[] wheres)
        {
            Expression exp = null;
            var param = Expression.Parameter(typeof(TTable), "r");
            foreach (var expression in wheres)
            {
                if (exp == null)
                    exp = Expression.Invoke(expression, param);
                else
                    exp = Expression.Or(exp, Expression.Invoke(expression, param));
            }

            if (exp == null)
                throw new ArgumentNullException("wheres");

            AddFilter(Expression.Lambda<Func<TTable, bool>>(exp, param));
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        /// <param name="exludeTypes">Список типов таблиц, к которым не применяется фильтр</param>
        public void AddFilterExcludeTables(Expression<Func<TTable, bool>> where, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            AddFilter(where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        /// <param name="onlyTypes">Список типов таблиц, к которым применяется фильтр</param>
        public void AddFilterForTables(Expression<Func<TTable, bool>> where, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            AddFilter(where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, Expression<Func<TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    value = (TValue)Convert.ChangeType(_url.QueryParameters[queryParameterName], typeof(TValue));
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, TValue?, Expression<Func<TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value1 = null;
                TValue? value2 = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                {
                    var split = _url.QueryParameters[queryParameterName].Split(',');
                    value1 = (TValue)Convert.ChangeType(split[0], typeof(TValue));
                    value2 = (TValue)Convert.ChangeType(split[1], typeof(TValue));
                }
                var where = filterHandler(value1, value2);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<List<TValue>, Expression<Func<TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                var value = LinqFilterGenerator.GetValueCollection<TValue>(_url.QueryParameters[queryParameterName]);
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        #endregion

        #region добавление фильтров по классу FilterExpression<TTable>

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        public void AddFilter(FilterExpression<TTable> filterExpression)
        {
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        /// <param name="exludeTypes">Список типов таблиц, к которым не применяется фильтр</param>
        public void AddFilterExcludeTables(FilterExpression<TTable> filterExpression, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        /// <param name="onlyTypes">Список типов таблиц, к которым применяется фильтр</param>
        public void AddFilterForTables(FilterExpression<TTable> filterExpression, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Класс фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, FilterExpression<TTable>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    value = (TValue)Convert.ChangeType(_url.QueryParameters[queryParameterName], typeof(TValue));
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Класс фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<List<TValue>, FilterExpression<TTable>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                var value = LinqFilterGenerator.GetValueCollection<TValue>(_url.QueryParameters[queryParameterName]);
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        #endregion

        /// <summary>
        /// Добавление выражения фильтрации данных объединению с таблицей (inner join).
        /// </summary>
        /// <typeparam name="TInner">Тип таблицы с которой производится объединение</typeparam>
        /// <typeparam name="TKey">Тип ключа, для объединения</typeparam>
        /// <param name="inner">Таблица с которой объединяется текущий запрос</param>
        /// <param name="outerKeySelector">Ключ текущей таблицы, для объединения</param>
        /// <param name="innerKeySelector">Ключ таблицы с которой производится объединение</param>
        public void AddFilter<TInner, TKey>(IQueryable<TInner> inner, 
            Expression<Func<TTable, TKey>> outerKeySelector, 
            Expression<Func<TInner, TKey>> innerKeySelector)
        {
            _joinFilters.Add(new JoinFilter<TKey, TInner>
            {
                Inner = inner,
                InnerKeySelector = innerKeySelector,
                OuterKeySelector = outerKeySelector
            });
        }

        /// <summary>
        /// Добавление выражения фильтрации данных объединению с таблицей (inner join).
        /// </summary>
        /// <typeparam name="TInner">Тип таблицы с которой производится объединение</typeparam>
        /// <typeparam name="TKey">Тип ключа, для объединения</typeparam>
        /// <param name="inner">Таблица с которой объединяется текущий запрос</param>
        /// <param name="outerKeySelector">Ключ текущей таблицы, для объединения</param>
        /// <param name="innerKeySelector">Ключ таблицы с которой производится объединение</param>
        public void AddFilter<TInner, TKey>(IEnumerable<TInner> inner,
            Expression<Func<TTable, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector)
        {
            AddFilter(inner.AsQueryable(), outerKeySelector, innerKeySelector);
        }

        public virtual Expression FilterData<T>(Expression source, Expression upToTable, ParameterExpression param, params Expression[] fieldsToCheckReference)
            where T : class
        {
            return FilterData<T>(source, upToTable, param, (IEnumerable<Expression>)fieldsToCheckReference);
        }

        public virtual Expression FilterData<T>(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
            where T : class
        {
            return FilterData<T>(new FilterDataArgs
                                     {
                                         FieldsToCheckReference = fieldsToCheckReference,
                                         QueryParameters = QueryParameters,
                                         Source = source,
                                         TableParam = param,
                                         UpToTable = upToTable,
                                     });
        }

        public virtual Expression FilterData<T>(FilterDataArgs args)
           where T : class
        {
            if (args.TableParam == null)
                args.TableParam = Expression.Parameter(typeof(T), "c");
            var thisTableExp = args.UpToTable ?? args.TableParam;

            if (_where != null || Filters != null)
            {
                Expression filter = GetExpression(args);
                foreach (var item in args.FieldsToCheckReference)
                {
                    if (LinqFilterGenerator.IsNullableType(item.Type))
                        filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                }
                Expression pred = Expression.Lambda(filter, args.TableParam);
                args.Source = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, args.Source, pred);
            }
            foreach (var joinFilter in _joinFilters)
                args.Source = joinFilter.FilterData<T>(args.Source, args.TableParam, thisTableExp);
            return args.Source;
        }

        public virtual Expression GetExpression(FilterDataArgs args)
        {
            if (args.UpToTable == null) args.UpToTable = args.TableParam;
            Expression filter = null;
            if (_where != null)
                filter = Expression.Invoke(Expression.Lambda(_where, TableParameterExpression), args.UpToTable);

            //если фильтр пустой, то его инициализируем первым значением и добавляем через И остальные, иначе все добавляем через И
            IEnumerable<FilterExpression<TTable>> wheres = _wheres;
            if (filter == null)
            {
                foreach (var item in _wheres.Take(1))
                    filter = Expression.Invoke(item.Where, args.UpToTable);
                wheres = _wheres.Skip(1);
            }
            foreach (var item in wheres)
                filter = Expression.And(filter, Expression.Invoke(item.Where, args.UpToTable));
            if (Filters != null)
            {
                filter = filter == null
                             ? Filters(args)
                             : Expression.And(filter, Filters(args));
            }
            return filter;
        }

        public virtual void GetExpressionCollection(List<BaseFilterExpression> list, Type getForType, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference)
        {
            if (_where != null)
            {
                Expression filter = Expression.Invoke(Expression.Lambda(_where, TableParameterExpression), upToTable);
                if (fieldsToCheckReference != null)
                    foreach (var item in fieldsToCheckReference)
                    {
                        if (LinqFilterGenerator.IsNullableType(item.Type))
                            filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                    }
                list.Add(new FilterExpressionInternal
                {
                    whereExpression = filter,
                    FilterForType = getForType,
                    Message = string.Empty,
                });
            }

            foreach (var filterExpression in _wheres)
            {
                Expression filter = Expression.Invoke(filterExpression.Where, upToTable);
                if (fieldsToCheckReference != null)
                    foreach (var item in fieldsToCheckReference)
                    {
                        if (LinqFilterGenerator.IsNullableType(item.Type))
                            filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                    }
                filterExpression.whereExpression = filter;
                filterExpression.FilterForType = getForType;
                list.Add(filterExpression);
            }

            if (Filters != null)
            {
                var filterDataArgs = new FilterDataArgs
                    {
                        FieldsToCheckReference = fieldsToCheckReference,
                        QueryParameters = QueryParameters,
                        TableParam = TableParameterExpression,
                        UpToTable = upToTable,
                    };
                list.Add(
                    new FilterExpressionInternal
                        {
                            whereExpression = Filters(filterDataArgs),
                            FilterForType = getForType,
                            Message = string.Empty,
                        });
            }
        }

        public virtual Expression GetFilterExpression(Expression source, Expression upToTable, ParameterExpression param, IEnumerable<Expression> fieldsToCheckReference)
        {
            if (HasFilter())
            {
                Expression filter = GetExpression(new FilterDataArgs
                                                      {
                                                          FieldsToCheckReference = fieldsToCheckReference,
                                                          QueryParameters = QueryParameters,
                                                          Source = source,
                                                          UpToTable = upToTable,
                                                          TableParam = param,
                                                      });
                if (fieldsToCheckReference != null)
                    foreach (var item in fieldsToCheckReference)
                    {
                        if (LinqFilterGenerator.IsNullableType(item.Type))
                            filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                    }
                source = source == null 
                    ? filter 
                    : Expression.And(source, filter);
            }
            return source;
        }

        private abstract class JoinFilter
        {
            public abstract Expression FilterData<T>(Expression source, ParameterExpression param, Expression thisTableExp);
        }

        private class JoinFilter<TKey, TInner> : JoinFilter
        {
            public IQueryable<TInner> Inner;
            public Expression<Func<TTable, TKey>> OuterKeySelector;
            public Expression<Func<TInner, TKey>> InnerKeySelector;

            public override Expression FilterData<T>(Expression source, ParameterExpression param, Expression thisTableExp)
            {
                var filterOuterKeySelector = Expression.Invoke(OuterKeySelector, thisTableExp);
                Expression outerKeySelector = Expression.Lambda(filterOuterKeySelector, param);
                Expression<Func<TTable, TInner, TTable>> resultSelector = (t, it) => t;
                return Expression.Call(typeof(Queryable), "Join"
                    , new[] { typeof(T), typeof(TInner), typeof(TKey), typeof(T) }
                    , source, Inner.Expression, outerKeySelector, InnerKeySelector, resultSelector);
                //return source.Provider.CreateQuery<T>(expr);
            }
        }
    }
    
    public class BaseFilterEventArgs<TTable, TDataContext> : BaseFilterEventArgs<TTable>
        where TTable : class
        where TDataContext : DataContext
    {
        private List<FilterExpression<TDataContext, TTable>> _wheres = new List<FilterExpression<TDataContext, TTable>>();

        public BaseFilterEventArgs(MainPageUrlBuilder url, QueryParameters qParams)
            : base(url)
        {
            if (qParams != null)
                DBParameterExpression = qParams.DBParameterExpression;
            QueryParameters = qParams;
        }

        public ParameterExpression DBParameterExpression { get; private set; }

        #region добавление фильтров по лямбда выражению

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter(Expression<Func<TDataContext, TTable, bool>> where)
        {
            Expression exp = Expression.Invoke(where, QueryParameters.GetDBExpression<TDataContext>(), TableParameterExpression);
            if (QueryParameters != null) QueryParameters.AddUniqueParameter(where);
            _where = _where == null ? exp : Expression.And(exp, _where);
        } 
        
        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey>(Expression<Func<TTable, TKey, bool>> where, TKey value)
        {
            Expression exp = Expression.Invoke(where,
                                               TableParameterExpression,
                                               QueryParameters.GetExpression(where.ToString(), value));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }


        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        public void AddFiltersByOr<TKey>(Expression<Func<TTable, TKey, bool>> where, TKey[] values)
        {
            Expression exp = null;
            var param = Expression.Parameter(typeof(TTable), "r");
            
            foreach (var value in values)
            {
                var valueParam = Expression.Constant(value, typeof(TKey));
                if (exp == null)
                    exp = Expression.Invoke(where, param, valueParam);
                else
                    exp = Expression.Or(exp, Expression.Invoke(where, param, valueParam));
            }

            if (exp == null)
                exp = Expression.Constant(false);

            AddFilter(Expression.Lambda<Func<TTable, bool>>(exp, param));
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        public void AddFiltersByOr<TKey>(Expression<Func<TDataContext, TTable, TKey, bool>> where, TKey[] values)
        {
            Expression exp = null;
            var dbparam = Expression.Parameter(typeof(TDataContext), "db");
            var param = Expression.Parameter(typeof(TTable), "r");

            foreach (var value in values)
            {
                var valueParam = Expression.Constant(value, typeof(TKey));
                if (exp == null)
                    exp = Expression.Invoke(where, dbparam, param, valueParam);
                else
                    exp = Expression.Or(exp, Expression.Invoke(where, dbparam, param, valueParam));
            }

            if (exp == null)
                exp = Expression.Constant(false);

            AddFilter(Expression.Lambda<Func<TDataContext, TTable, bool>>(exp, dbparam, param));
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        /// <param name="wheres">Выражения фильтрации данных, которые объединяться через Or.</param>
        public void AddFiltersByOr(params Expression<Func<TDataContext, TTable, bool>>[] wheres)
        {
            Expression exp = null;
            var dbparam = Expression.Parameter(typeof(TDataContext), "db");
            var param = Expression.Parameter(typeof(TTable), "r");
            foreach (var expression in wheres)
            {
                if (exp == null)
                    exp = Expression.Invoke(expression, dbparam, param);
                else
                    exp = Expression.Or(exp, Expression.Invoke(expression, dbparam, param));
            }

            if (exp == null)
                throw new ArgumentNullException("wheres");

            AddFilter(Expression.Lambda<Func<TDataContext, TTable, bool>>(exp, dbparam, param));
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        /// <param name="wheres">Выражения фильтрации данных, которые объединяться через Or.</param>
        public void AddFiltersByOrExcludeTables(Expression<Func<TDataContext, TTable, bool>>[] wheres, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            AddFiltersByOr(wheres);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        /// <param name="wheres">Выражения фильтрации данных, которые объединяться через Or.</param>
        public void AddFiltersByOrForTables(Expression<Func<TDataContext, TTable, bool>>[] wheres, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            AddFiltersByOr(wheres);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        /// <param name="wheres">Выражения фильтрации данных, которые объединяться через Or.</param>
        public void AddFiltersByOr(params IBaseFilterOrItem[] wheres)
        {
            Expression exp = null;
            var dbparam = Expression.Parameter(typeof(TDataContext), "db");
            var param = Expression.Parameter(typeof(TTable), "r");
            foreach (var expression in wheres)
            {
                var valueParam = Expression.Constant(expression.FilterValue, expression.FilterValueType);
                if (exp == null)
                    exp = Expression.Invoke(expression.FilterExpression, dbparam, param, valueParam);
                else
                    exp = Expression.Or(exp, Expression.Invoke(expression.FilterExpression, dbparam, param, valueParam));
            }

            if (exp == null)
                throw new ArgumentNullException("wheres");

            AddFilter(Expression.Lambda<Func<TDataContext, TTable, bool>>(exp, dbparam, param));
        }

        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        public void AddFiltersByOrExcludeTables(IBaseFilterOrItem[] wheres, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            AddFiltersByOr(wheres);
        }
        
        /// <summary>
        /// Добавление выражения фильтрации данных с объединением фильтров через Or.
        /// </summary>
        public void AddFiltersByOrForTables(IBaseFilterOrItem[] wheres, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            AddFiltersByOr(wheres);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey1, TKey2>(Expression<Func<TTable, TKey1, TKey2, bool>> where1,
                                            TKey1 value1, TKey2 value2)
        {
            Expression exp = Expression.Invoke(where1, TableParameterExpression,
                QueryParameters.GetExpression(where1.ToString(), value1),
                QueryParameters.GetExpression(where1.ToString(), value2));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey>(Expression<Func<TDataContext, TTable, TKey, bool>> where, TKey value)
        {
            Expression exp = Expression.Invoke(where,
                                               QueryParameters.GetDBExpression<TDataContext>(),
                                               TableParameterExpression,
                                               QueryParameters.GetExpression(where.ToString(), value));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey1, TKey2>(Expression<Func<TDataContext, TTable, TKey1, TKey2, bool>> where1, 
                                            TKey1 value1, TKey2 value2)
        {
            Expression exp = Expression.Invoke(where1, QueryParameters.GetDBExpression<TDataContext>(), TableParameterExpression,
                QueryParameters.GetExpression(where1.ToString(), value1),
                QueryParameters.GetExpression(where1.ToString(), value2));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey1, TKey2, TKey3>(Expression<Func<TDataContext, TTable, TKey1, TKey2, bool>> where1,
                                            TKey1 value1, TKey2 value2, TKey3 value3)
        {
            Expression exp = Expression.Invoke(where1, QueryParameters.GetDBExpression<TDataContext>(), TableParameterExpression,
                QueryParameters.GetExpression(where1.ToString(), value1),
                QueryParameters.GetExpression(where1.ToString(), value2),
                QueryParameters.GetExpression(where1.ToString(), value3));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        public void AddFilter<TKey1, TKey2, TKey3, TKey4>(Expression<Func<TDataContext, TTable, TKey1, TKey2, bool>> where1,
                                            TKey1 value1, TKey2 value2, TKey3 value3, TKey4 value4, string filterNameKey)
        {
            Expression exp = Expression.Invoke(where1, QueryParameters.GetDBExpression<TDataContext>(), TableParameterExpression,
                QueryParameters.GetExpression(where1.ToString(), value1),
                QueryParameters.GetExpression(where1.ToString(), value2),
                QueryParameters.GetExpression(where1.ToString(), value3),
                QueryParameters.GetExpression(where1.ToString(), value4));
            _where = _where == null ? exp : Expression.And(exp, _where);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        /// <param name="exludeTypes">Список типов таблиц, к которым не применяется фильтр</param>
        public void AddFilterExcludeTables<TKey>(Expression<Func<TDataContext, TTable, TKey, bool>> where, TKey value, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            AddFilter(where, value);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных.
        /// </summary>
        /// <param name="where">Выражение фильтрации данных</param>
        /// <param name="onlyTypes">Список типов таблиц, к которым применяется фильтр</param>
        public void AddFilterForTables<TKey>(Expression<Func<TDataContext, TTable, TKey, bool>> where, TKey value, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            AddFilter(where, value);
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, Expression<Func<TDataContext, TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    value = (TValue)Convert.ChangeType(_url.QueryParameters[queryParameterName], typeof(TValue));
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, TValue?, Expression<Func<TDataContext, TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value1 = null;
                TValue? value2 = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                {
                    var split = _url.QueryParameters[queryParameterName].Split(',');
                    value1 = (TValue)Convert.ChangeType(split[0], typeof(TValue));
                    value2 = (TValue)Convert.ChangeType(split[1], typeof(TValue));
                }
                var where = filterHandler(value1, value2);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление выражения фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Выражение фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<List<TValue>, Expression<Func<TDataContext, TTable, bool>>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                var value = LinqFilterGenerator.GetValueCollection<TValue>(_url.QueryParameters[queryParameterName]);
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        #endregion

        #region добавление фильтров по классу FilterExpression<TTable>

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        public void AddFilter(FilterExpression<TDataContext, TTable> filterExpression)
        {
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        /// <param name="exludeTypes">Список типов таблиц, к которым не применяется фильтр</param>
        public void AddFilterExcludeTables(FilterExpression<TDataContext, TTable> filterExpression, params Type[] exludeTypes)
        {
            if (exludeTypes.Contains(TypeOfData)) return;
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных.
        /// </summary>
        /// <param name="where">Класс фильтрации данных</param>
        /// <param name="onlyTypes">Список типов таблиц, к которым применяется фильтр</param>
        public void AddFilterForTables(FilterExpression<TDataContext, TTable> filterExpression, params Type[] forTypes)
        {
            if (!forTypes.Contains(TypeOfData)) return;
            _wheres.Add(filterExpression);
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Класс фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<TValue?, FilterExpression<TDataContext, TTable>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                TValue? value = null;
                if (!string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    value = (TValue)Convert.ChangeType(_url.QueryParameters[queryParameterName], typeof(TValue));
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра.</typeparam>
        /// <param name="queryParameterName">Наименование параметра.</param>
        public void AddFilter<TValue>(string queryParameterName, Action<TValue?, BaseFilterEventArgs<TTable, TDataContext>> addFilter)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                // TODO: добавлена обработка множественных значений фильтров(они приходят через зпт). Maxat
                // уточнить у Сергея правильно ли такое решение в данном контексте
                if (string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    addFilter(null, this);
                else
                {
                    string[] parameters = _url.QueryParameters[queryParameterName].Split(',');
                    if (!parameters.Any())
                    {
                        var value =
                            (TValue?)Convert.ChangeType(_url.QueryParameters[queryParameterName], typeof(TValue));
                        addFilter(value, this);
                    }
                    else
                    {
                        foreach (var parameter in parameters)
                        {
                            addFilter(
                                (TValue?)Convert.ChangeType(parameter, typeof(TValue)),
                                this);
                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра.</typeparam>
        /// <param name="queryParameterName">Наименование параметра.</param>
        public void AddFilter<TValue>(Enum queryParameterName, Action<TValue?, BaseFilterEventArgs<TTable, TDataContext>> addFilter)
            where TValue : struct
        {
            AddFilter(queryParameterName.ToString(), addFilter);
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра.</typeparam>
        /// <param name="queryParameterName">Наименование параметра.</param>
        /// <param name="addFilter">Метод для добавления фильтров.</param>
        public void AddFilter<TValue>(Enum queryParameterName, Action<List<TValue>, BaseFilterEventArgs<TTable, TDataContext>> addFilter)
            where TValue : struct
        {
            AddFilter(queryParameterName.ToString(), addFilter);
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра.</typeparam>
        /// <param name="queryParameterName">Наименование параметра.</param>
        /// <param name="addFilter">Метод для добавления фильтров.</param>
        public void AddFilter<TValue>(string queryParameterName, Action<List<TValue>, BaseFilterEventArgs<TTable, TDataContext>> addFilter)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                if (string.IsNullOrEmpty(_url.QueryParameters[queryParameterName]))
                    addFilter(new List<TValue>(0), this);
                else
                {
                    var value = LinqFilterGenerator.GetValueCollection<TValue>(_url.QueryParameters[queryParameterName]);
                    addFilter(value, this);
                }
            }
        }

        /// <summary>
        /// Добавление класса фильтрации данных по входному параметру, который содержит список значений.
        /// Если параметр не присутствует, то фильтр не применяется.
        /// </summary>
        /// <typeparam name="TValue">Тип параметра</typeparam>
        /// <param name="queryParameterName">Наименование параметра</param>
        /// <param name="filterHandler">Класс фильтрации по входному параметру</param>
        public void AddFilter<TValue>(string queryParameterName, Func<List<TValue>, FilterExpression<TDataContext, TTable>> filterHandler)
            where TValue : struct
        {
            if (_url.QueryParameters.ContainsKey(queryParameterName))
            {
                var value = LinqFilterGenerator.GetValueCollection<TValue>(_url.QueryParameters[queryParameterName]);
                var where = filterHandler(value);
                if (where != null) AddFilter(where);
            }
        }

        #endregion

        #region SetKSP Filter

        /// <summary>
        /// Добавить фильтр по KSP
        /// </summary>
        /// <param name="kspExpression"></param>
        public void AddFilterByKSP(Expression<Func<TTable, string, bool>> kspExpression)
        {
            var ksp = User.GetSubdivisionKSP();
            if (string.IsNullOrEmpty(ksp)) return;
            AddFilter(kspExpression, ksp);
        }

        /// <summary>
        /// Добавить фильтр по KSP
        /// </summary>
        /// <param name="kspExpression"></param>
        public void AddFiltersByKSP(params Expression<Func<TTable, string, bool>>[] kspExpressions)
        {
            var ksp = User.GetSubdivisionKSP();
            if (string.IsNullOrEmpty(ksp)) return;

            Expression exp = null;
            var param = Expression.Parameter(typeof(TTable), "r");
            var value = Expression.Parameter(typeof(string), "value");
            foreach (var expression in kspExpressions)
            {
                if (exp == null)
                    exp = Expression.Invoke(expression, param, value);
                else
                    exp = Expression.Or(exp, Expression.Invoke(expression, param, value));
            }

            if (exp == null)
                throw new ArgumentNullException("kspExpressions");

            AddFilter(Expression.Lambda<Func<TTable, string, bool>>(exp, param, value), ksp);
        }

        /// <summary>
        /// Добавить фильтр по KSP
        /// </summary>
        /// <param name="kspExpression"></param>
        public void AddFiltersByKSP(params Expression<Func<TDataContext, TTable, string, bool>>[] kspExpressions)
        {
            var ksp = User.GetSubdivisionKSP();
            if (string.IsNullOrEmpty(ksp)) return;

            Expression exp = null;
            var dbparam = Expression.Parameter(typeof(TDataContext), "db");
            var param = Expression.Parameter(typeof(TTable), "r");
            var value = Expression.Parameter(typeof(string), "value");
            foreach (var expression in kspExpressions)
            {
                if (exp == null)
                    exp = Expression.Invoke(expression, dbparam, param, value);
                else
                    exp = Expression.Or(exp, Expression.Invoke(expression, dbparam, param, value));
            }

            if (exp == null)
                throw new ArgumentNullException("kspExpressions");

            AddFilter(Expression.Lambda<Func<TDataContext, TTable, string, bool>>(exp, dbparam, param, value), ksp);
        }

        /// <summary>
        /// Добавить фильтр по KSP
        /// </summary>
        /// <param name="kspExpression"></param>
        public void AddFilterByKSPForTables(Expression<Func<TDataContext, TTable, string, bool>> kspExpression, params Type[] forTypes)
        {
            var ksp = User.GetSubdivisionKSP();
            if (string.IsNullOrEmpty(ksp)) return;
            AddFilterForTables(kspExpression, ksp, forTypes);
        }

        /// <summary>
        /// Добавить фильтр по KSP
        /// </summary>
        /// <param name="kspExpression"></param>
        public void AddFilterByKSPExcludeTables(Expression<Func<TDataContext, TTable, string, bool>> kspExpression, params Type[] exludeTypes)
        {
            var ksp = User.GetSubdivisionKSP();
            if (string.IsNullOrEmpty(ksp)) return;
            AddFilterExcludeTables(kspExpression, ksp, exludeTypes);
        }

        #endregion
        
        public override Expression GetExpression(FilterDataArgs args)
        {
            if (args.UpToTable == null) args.UpToTable = args.TableParam;
            Expression filter = base.GetExpression(args);

            //если фильтр пустой, то его инициализируем первым значением и добавляем через И остальные, иначе все добавляем через И
            IEnumerable<FilterExpression<TDataContext, TTable>> wheres = _wheres;
            if (filter == null)
            {
                foreach (var item in _wheres.Take(1))
                    filter = Expression.Invoke(item.Where, DBParameterExpression, args.UpToTable);
                wheres = _wheres.Skip(1);
            }

            foreach (var item in wheres)
                filter = Expression.And(filter, Expression.Invoke(item.Where, DBParameterExpression, args.UpToTable));

            if (Filters != null)
            {
                filter = filter == null
                             ? Filters(args)
                             : Expression.And(filter, Filters(args));
            }

            return filter;
        }

        public override void GetExpressionCollection(List<BaseFilterExpression> list, Type getForType, Expression upToTable, IEnumerable<Expression> fieldsToCheckReference)
        {
            foreach (var filterExpression in _wheres)
            {
                Expression filter = Expression.Invoke(filterExpression.Where, DBParameterExpression, upToTable);
                if (fieldsToCheckReference != null)
                    foreach (var item in fieldsToCheckReference)
                    {
                        if (LinqFilterGenerator.IsNullableType(item.Type))
                            filter = Expression.Or(filter, Expression.Equal(item, Expression.Constant(null)));
                    }
                filterExpression.whereExpression = filter;
                filterExpression.FilterForType = getForType;
                list.Add(filterExpression);
            }
            base.GetExpressionCollection(list, getForType, upToTable, fieldsToCheckReference);
        }
    }
}
