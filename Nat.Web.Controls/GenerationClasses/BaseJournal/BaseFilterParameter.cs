using System.Globalization;
using System.Threading;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Controls.GenerationClasses.Filter;
    using Nat.Web.Tools;

    using System.Data.Linq;
    using System.Data.Linq.SqlClient;

    public abstract class BaseFilterParameter : FilterHtmlGenerator.Filter
    {
        public const string TreeStartLevelFilterName = "TreeStartLevel";
        public const string TreeMaxLevelFilterName = "TreeMaxLevel";

        protected BaseFilterParameter()
        {
            DependedFilters = new List<BaseFilterParameter>();
        }

        protected Expression DependendFilterExp { get; set; }

        public abstract Expression OValueExpression { get; }

        public abstract Expression CustomWhereExpression { get; }

        public Expression WhereExpression { get; set; }
        public Func<BaseFilterParameter, Expression> GetWhereExpression { get; set; } = f => f.WhereExpression;
        protected Expression InnerWhereExpression { get; set; }

        public Expression CorssDataToHeader { get; set; }

        public Expression GetChildsFromJournal { get; set; }

        public Expression FieldReferenceFromCrossToHeader { get; set; }

        public BaseJournalCrossTable CrossTable { get; set; }

        public List<BaseFilterParameter> DependedFilters { get; set; }

        /// <summary>
        /// Используется, если необходимо фильтровать данные по результату агрегации дочерних данных.
        /// Указывается тип агрегации поля дочернего журнала.
        /// </summary>
        public ColumnAggregateType ColumnAggregateType { get; set; }

        /// <summary>
        /// Фильтр основного журнала - true. Для кросс-журналов возможна фильтрация только дочерних журналов.
        /// </summary>
        public bool IsJournalFilter { get; set; }

        protected QueryParameters QueryParameters { get; set; }

        public virtual Expression OValueExpressionSecond
        {
            get { return null; }
        }

        public virtual Expression OTextValueExpressionRu
        {
            get
            {
                if (OValueExpression == null)
                    return null;
                if (OValueExpression.Type == typeof(string))
                    return OValueExpression;
                var lexp = OValueExpression as LambdaExpression;
                if (lexp != null && lexp.Body.Type == typeof(string))
                    return OValueExpression;
                return null;
            }
        }

        public virtual Expression OTextValueExpressionKz
        {
            get
            {
                if (OValueExpression == null)
                    return null;
                if (OValueExpression.Type == typeof(string))
                    return OValueExpression;
                var lexp = OValueExpression as LambdaExpression;
                if (lexp != null && lexp.Body.Type == typeof(string))
                    return OValueExpression;
                return null;
            }
        }
        
        public Func<object, object> ChangeFilterValueHandler { get; set; }
        public Func<object, IEnumerable> GetHierarchyValuesListHandler { get; set; }

        protected virtual Expression GetTextValueExpression(ParameterExpression param)
        {
            var exp = LocalizationHelper.IsCultureKZ ? OTextValueExpressionKz : OTextValueExpressionRu;

            if (((LambdaExpression)exp).Parameters.Count == 1)
                return Expression.Invoke(exp, param);

            return Expression.Invoke(
                exp,
                (Expression)QueryParameters.DBParameterExpression ?? Expression.Constant(QueryParameters.InternalDB),
                param);
        }

        protected virtual Expression InvokeValueExpression(ParameterExpression param)
        {
            switch (ColumnAggregateType)
            {
                case ColumnAggregateType.None:
                    if (((LambdaExpression)OValueExpression).Parameters.Count == 1)
                        return Expression.Invoke(OValueExpression, param);
                    return Expression.Invoke(
                        OValueExpression,
                        (Expression)QueryParameters.DBParameterExpression ?? Expression.Constant(QueryParameters.InternalDB),
                        param);
                case ColumnAggregateType.Sum:
                case ColumnAggregateType.Avg:
                case ColumnAggregateType.Max:
                case ColumnAggregateType.Min:
                case ColumnAggregateType.Count:
                    return param;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Expression InvokeValueExpressionSecond(ParameterExpression param)
        {
            if (((LambdaExpression)OValueExpressionSecond).Parameters.Count == 1)
                return Expression.Invoke(OValueExpressionSecond, param);
            return Expression.Invoke(
                OValueExpressionSecond,
                (Expression)QueryParameters.DBParameterExpression ?? Expression.Constant(QueryParameters.InternalDB),
                param);
        }

        public virtual void AddFilter(IList<FilterHtmlGenerator.Filter> filters)
        {
            filters.Add(this);
            SetStandartHandler();
        }

        public virtual void InsertFilter(IList<FilterHtmlGenerator.Filter> filters, int index)
        {
            filters.Insert(index, this);
            SetStandartHandler();
        }

        public virtual void SetStandartHandler()
        {
            FilterHandler = OnFilter;
            ExpressionFilterHandlerV2 = OnFilter;
        }

        protected abstract IQueryable OnFilter(IQueryable enumerable, Enum filtertype, string value1, string value2);

        protected internal abstract Expression OnFilter(Enum filtertype, FilterItem filterItem,
            QueryParameters queryParameters);

        public virtual Expression GetFilterExpression(FilterItem filterItem, QueryParameters queryParameters)
        {
            Enum emnum;
            switch (Type)
            {
                case FilterHtmlGenerator.FilterType.Reference:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.ReferenceFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Numeric:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.NumericFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Boolean:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.BooleanFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Text:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.TextFilter), filterItem.FilterType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return OnFilter(emnum, filterItem, queryParameters);
        }

        public virtual Expression GetJournalFilter(QueryParameters qParams, Type tableType)
        {
            if (IsJournalFilter)
                return InnerWhereExpression;
            if (GetChildsFromJournal == null || InnerWhereExpression == null)
            {
                if ((InnerWhereExpression == null && WhereExpression == null) || qParams == null)
                    return null;

                var t = Expression.Parameter(qParams.QParamTableType, "r");
                return Expression.Lambda(Expression.Constant(true), t);
            }
            // todo: нужно подумть как сделать чтобы фильтр по Exists отрабатывал на одну таблицу однажды
            // т.е. сейчас если два фильтра на кросс таблицу, то будет вызван дважды Exists, а нужно сделать один вызов
            // мысыль сделать накопление в qParams
            return this.GetFilterExpressionByChilds(qParams, tableType, this.InnerWhereExpression);
        }

        protected Expression GetFilterExpressionByChilds(QueryParameters qParams, Type tableType, Expression expression)
        {
            var journalTableType = ((LambdaExpression) this.GetChildsFromJournal).Parameters[1].Type;
            var param = Expression.Parameter(journalTableType, "fJByChild");
            Expression exp = Expression.Invoke(this.GetChildsFromJournal, qParams.GetDBExpression(qParams.InternalDB), param);
            exp = GetWhereExpressionByChildrenData(tableType, exp, expression);
            if (DependendFilterExp != null)
            {
                exp = Expression.Or(exp, Expression.Invoke(DependendFilterExp, param));
            }
            qParams.RegisterExpression(exp, "GetJournalFilter");
            return Expression.Lambda(exp, param);
        }

        public virtual void ClearState()
        {
            WhereExpression = null;
            InnerWhereExpression = null;
        }

        /// <summary>
        /// Получить фильтр основного журнала, по данным дочерней таблицы.
        /// </summary>
        /// <param name="tableType">Тип дочерней таблицы.</param>
        /// <param name="exp">Выражение возвращающее набор дочерних данных (Queryable).</param>
        /// <returns>Выражение фильтра по дочернему журналу. Выражение возвращает логический результат.</returns>
        protected virtual Expression GetWhereExpressionByChildrenData(Type tableType, Expression exp,
            Expression expression)
        {
            switch (ColumnAggregateType)
            {
                case ColumnAggregateType.None:
                    exp = Expression.Call(typeof(Queryable), "Any", new[] {tableType}, exp, expression);
                    break;
                case ColumnAggregateType.Sum:
                    exp = Expression.Call(typeof(Queryable), "Sum", new[] {tableType}, exp, OValueExpression);
                    exp = Expression.Invoke(expression, exp);
                    break;
                case ColumnAggregateType.Avg:
                    exp = Expression.Call(typeof(Queryable), "Avg", new[] {tableType}, exp, OValueExpression);
                    exp = Expression.Invoke(expression, exp);
                    break;
                case ColumnAggregateType.Max:
                    exp = Expression.Call(typeof(Queryable), "Max", new[] {tableType}, exp, OValueExpression);
                    exp = Expression.Invoke(expression, exp);
                    break;
                case ColumnAggregateType.Min:
                    exp = Expression.Call(typeof(Queryable), "Min", new[] {tableType}, exp, OValueExpression);
                    exp = Expression.Invoke(expression, exp);
                    break;
                case ColumnAggregateType.Count:
                    exp = Expression.Call(typeof(Queryable), "Count", new[] {tableType}, exp);
                    exp = Expression.Invoke(expression, exp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return exp;
        }
    }

    public abstract class BaseFilterParameter<TTable> : BaseFilterParameter
        where TTable : class
    {
        protected BaseFilterParameter()
        {
        }

        protected BaseFilterParameter(Expression<Func<TTable, bool>> where)
        {
            Where = where;
        }

        public Expression<Func<TTable, bool>> Where { get; set; }

        public Expression<Func<TTable, bool, bool>> CustomWhere { get; set; }

        protected virtual Type FieldType
        {
            get { return OValueExpression.Type; }
        }

        protected virtual Type TableType
        {
            get { return typeof(TTable); }
        }

        public BaseFilterParameterContainer<TTable> Container { get; set; }

        public override Expression OValueExpression
        {
            get { return Where; }
        }

        public override Expression CustomWhereExpression
        {
            get { return CustomWhere; }
        }

        protected override IQueryable OnFilter(IQueryable query, Enum filtertype, string value1, string value2)
        {
            if (Where != null)
            {
                var data = (IQueryable<TTable>) query;
                var resExp = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new[] {typeof(TTable)},
                    data.Expression,
                    Where);
                return data.Provider.CreateQuery(resExp);
            }

            if (OValueExpression != null)
            {
                var data = query;
                if (filtertype is DefaultFilters.NumericFilter)
                    return OnFilter(data, (DefaultFilters.NumericFilter) filtertype, value1, value2, TableType);
                if (filtertype is DefaultFilters.ReferenceFilter)
                    return OnFilter(data, (DefaultFilters.ReferenceFilter) filtertype, value1, value2, TableType);
                if (filtertype is DefaultFilters.BooleanFilter)
                    return OnFilter(data, (DefaultFilters.BooleanFilter) filtertype, value1, value2, TableType);
                if (filtertype is DefaultFilters.TextFilter)
                    return OnFilter(data, (DefaultFilters.TextFilter) filtertype, value1, value2, TableType);
            }

            return query;
        }

        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            if (Container != null)
            {
                Container.AddFilterValue(this, filtertype, filterItem, qParams);
                return null;
            }

            QueryParameters = qParams;
            if (Where != null)
            {
                this.ProcessDependedFilters(filtertype, filterItem, qParams);
                return Where;
            }

            return this.CreateFilterExpression(filtertype, filterItem, qParams);
        }

        internal Expression CreateFilterExpression(FilterItem filterItem, QueryParameters qParams)
        {
            Enum emnum;
            switch (Type)
            {
                case FilterHtmlGenerator.FilterType.Reference:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.ReferenceFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Numeric:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.NumericFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Boolean:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.BooleanFilter), filterItem.FilterType);
                    break;
                case FilterHtmlGenerator.FilterType.Text:
                    emnum = (Enum) Enum.Parse(typeof(DefaultFilters.TextFilter), filterItem.FilterType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CreateFilterExpression(emnum, filterItem, qParams);
        }

        private Expression CreateFilterExpression(
            Enum filtertype,
            FilterItem filterItem,
            QueryParameters qParams)
        {
            this.ProcessDependedFilters(filtertype, filterItem, qParams);
            if (this.OValueExpression != null)

            {
                Type tableType;
                switch (ColumnAggregateType)
                {
                    case ColumnAggregateType.None:
                        tableType = TableType;
                        break;
                    case ColumnAggregateType.Sum:
                    case ColumnAggregateType.Avg:
                    case ColumnAggregateType.Max:
                    case ColumnAggregateType.Min:
                    case ColumnAggregateType.Count:
                        tableType = ((LambdaExpression) OValueExpression).Body.Type;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (filtertype is DefaultFilters.NumericFilter)
                    return this.OnFilter((DefaultFilters.NumericFilter) filtertype, filterItem, tableType, qParams);
                if (filtertype is DefaultFilters.ReferenceFilter)
                    return this.OnFilter((DefaultFilters.ReferenceFilter) filtertype, filterItem, tableType, qParams);
                if (filtertype is DefaultFilters.BooleanFilter)
                    return this.OnFilter((DefaultFilters.BooleanFilter) filtertype, filterItem, tableType, qParams);
                if (filtertype is DefaultFilters.TextFilter)
                    return this.OnFilter((DefaultFilters.TextFilter) filtertype, filterItem, tableType, qParams);
            }

            return null;
        }
        
        protected void ProcessDependedFilters(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            if (this.DependedFilters != null && this.DependedFilters.Count > 0)
            {
                foreach (var dependedFilter in this.DependedFilters)
                {
                    Expression exp = dependedFilter.OnFilter(filtertype, filterItem, qParams);
                    if (this.DependendFilterExp == null)
                    {
                        this.DependendFilterExp = exp;
                    }
                    else if (exp != null)
                    {
                        var param = Expression.Parameter(qParams.QParamTableType, "fJByChild");
                        this.DependendFilterExp =
                            Expression.Lambda(
                                Expression.Or(Expression.Invoke(this.DependendFilterExp, param),
                                    Expression.Invoke(exp, param)),
                                param);
                    }
                }
            }
        }

        #region Методы создания вырожений

        protected object ConvertToFieldType(string strValue)
        {
            if (Type == FilterHtmlGenerator.FilterType.Numeric)
            {
                var decimalSeparator = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
                var newstrValue = decimalSeparator == "."
                    ? strValue.Replace(",", ".")
                    : strValue.Replace(".", ",");
                return Convert.ChangeType(newstrValue /*.Replace(".", ",")*/, FieldType);

            }

            if (ChangeFilterValueHandler != null)
                return ChangeFilterValueHandler(Convert.ChangeType(strValue, FieldType));

            return Convert.ChangeType(strValue, FieldType);
        }

        protected object ConvertToFieldTypeToEndDay(string strValue)
        {
            if (FieldType == typeof(DateTime))
            {
                var t = Convert.ToDateTime(ConvertToFieldType(strValue));
                return t.AddDays(1).AddSeconds(-1);

            }
            return ConvertToFieldType(strValue);
        }

        protected virtual void EqualsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            if (FieldType == typeof(DateTime) && Convert.ToDateTime(strValue).ToString(CultureInfo.CurrentCulture) !=
                strValue)
            {
                BetweenExpression(strValue, strValue, tableType);
                return;
            }

            var param = Expression.Parameter(tableType, "bFilter");
            if ( /*FieldType == typeof(long) || */IsMultipleSelect)
            {
                var field = InvokeValueExpression(param);
                Expression filter = null;
                foreach (var oneValue in strValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var value = ConvertToFieldType(oneValue);
                    var fieldList = GetHierarchyValuesListHandler?.Invoke(value);
                    if (fieldList != null)
                    {
                        foreach (var childValue in fieldList)
                        {
                            var childValueExp =
                                QueryParameters.GetExpression(field + ".Equals", childValue, field.Type);
                            filter = filter == null
                                ? Expression.Equal(field, childValueExp)
                                : Expression.Or(filter, Expression.Equal(field, childValueExp));
                        }
                    }

                    var valueExp = QueryParameters.GetExpression(field + ".Equals", value, field.Type);
                    filter = filter == null
                        ? Expression.Equal(field, valueExp)
                        : Expression.Or(filter, Expression.Equal(field, valueExp));
                }

                if (filter != null)
                    SetWhereExpression(filter, param);
            }
            else
            {
                if (!string.IsNullOrEmpty(strValue))
                {
                    var value = ConvertToFieldType(strValue);
                    var field = InvokeValueExpression(param);
                    var valueExp = QueryParameters.GetExpression(
                        field + ".Equals", value, field.Type);
                    SetWhereExpression(Expression.Equal(field, valueExp), param);
                }
            }
        }
        
        protected virtual void NotEqualsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            if (FieldType == typeof(DateTime) && Convert.ToDateTime(strValue).ToString(CultureInfo.CurrentCulture) !=
                strValue)
            {
                NotEqualsBetweenExpression(strValue, tableType);
                return;
            }

            var param = Expression.Parameter(tableType, "bFilter");
            if ( /*FieldType == typeof(long) || */IsMultipleSelect)
            {
                var field = InvokeValueExpression(param);
                Expression filter = null;
                foreach (var oneValue in strValue.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var value = ConvertToFieldType(oneValue);

                    var fieldList = GetHierarchyValuesListHandler?.Invoke(value);
                    if (fieldList != null)
                    {
                        foreach (var childValue in fieldList)
                        {
                            var childValueExp =
                                QueryParameters.GetExpression(field + ".NotEquals", childValue, field.Type);
                            filter = filter == null
                                ? Expression.NotEqual(field, childValueExp)
                                : Expression.And(filter, Expression.NotEqual(field, childValueExp));
                        }
                    }

                    var valueExp = QueryParameters.GetExpression(field + ".NotEquals", value, field.Type);
                    filter = filter == null
                        ? Expression.NotEqual(field, valueExp)
                        : Expression.And(filter, Expression.NotEqual(field, valueExp));
                }

                if (filter != null)
                    SetWhereExpression(filter, param);
            }
            else
            {
                var value = ConvertToFieldType(strValue);
                var field = InvokeValueExpression(param);
                var valueExp = QueryParameters.GetExpression(field + ".NotEquals", value, field.Type);
                SetWhereExpression(Expression.NotEqual(field, valueExp), param);
            }
        }

        protected virtual void IsNotNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.NotEqual(field, Expression.Constant(null, field.Type));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected virtual void IsNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.Equal(field, Expression.Constant(null, field.Type));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected virtual void MoreExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".More", value, field.Type);
            SetWhereExpression(Expression.GreaterThan(field, valueExp), param);
        }

        protected void LengthMoreExpression(string strValue, Type tableType)
        {
            int value;
            if (string.IsNullOrEmpty(strValue) || !int.TryParse(strValue, out value))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = Expression.Property(InvokeValueExpression(param), "Length");
            var valueExp = QueryParameters.GetExpression(field + ".More", value, field.Type);

            SetWhereExpression(Expression.GreaterThan(field, valueExp), param);
        }

        protected void LengthLessExpression(string strValue, Type tableType)
        {
            int value;
            if (string.IsNullOrEmpty(strValue) || !int.TryParse(strValue, out value))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = Expression.Property(InvokeValueExpression(param), "Length");
            var valueExp = QueryParameters.GetExpression(field + ".More", value, field.Type);
            
            SetWhereExpression(Expression.LessThan(field, valueExp), param);
        }

        protected virtual void MoreOrEqualExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".MoreOrEqual", value, field.Type);
            SetWhereExpression(Expression.GreaterThanOrEqual(field, valueExp), param);
        }

        protected void DaysAgoAndMoreExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var value = Convert.ToInt32(strValue);
            LessExpression(DateTime.Now.Date.AddDays(-value + 1).AddSeconds(-1).ToString(), tableType);
            /*
            if (tableType != typeof(DateTime) && tableType != typeof(DateTime?))
                throw new ArgumentException(string.Format("Тип поля {0} не соответствует дате, наименование фильтра: {1}.", tableType.Name, FilterName), "tableType");

            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            
            var valueExp = QueryParameters.GetExpression(field + ".DaysAgoAndMore", value, typeof(int));
            var currentDateExp = QueryParameters.GetExpression(field + ".DaysAgoAndMore", DateTime.Now.Date, field.Type);
            var diffDaysExp = Expression.Call(typeof(SqlMethods), "DateDiffDay", new Type[0], field, currentDateExp);
            SetWhereExpression(Expression.GreaterThanOrEqual(diffDaysExp, valueExp), param);
            */
        }

        protected void DaysLeftAndMoreExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var value = Convert.ToInt32(strValue);
            MoreOrEqualExpression(DateTime.Now.Date.AddDays(value).ToString(), tableType);

            /*
            if (tableType != typeof(DateTime) && tableType != typeof(DateTime?))
                throw new ArgumentException(string.Format("Тип поля {0} не соответствует дате, наименование фильтра: {1}.", tableType.Name, FilterName), "tableType");

            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            
            var valueExp = QueryParameters.GetExpression(field + ".DaysAgoAndMore", value, typeof(int));
            var currentDateExp = QueryParameters.GetExpression(field + ".DaysAgoAndMore", DateTime.Now.Date, field.Type);
            var diffDaysExp = Expression.Call(typeof(SqlMethods), "DateDiffDay", new Type[0], currentDateExp, field);
            SetWhereExpression(Expression.GreaterThanOrEqual(diffDaysExp, valueExp), param);
            */
        }

        protected void ToDayExpression(Type tableType)
        {
            BetweenExpression(DateTime.Now.Date.ToString(), DateTime.Now.Date.AddDays(1).AddSeconds(-1).ToString(), tableType);
        }

        protected virtual void LessExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = Convert.ChangeType(strValue, FieldType);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".Less", value, field.Type);
            SetWhereExpression(Expression.LessThan(field, valueExp), param);
        }
        
        protected virtual void LessOrEqualExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = FieldType == typeof(DateTime) &&
                        Convert.ToDateTime(strValue).ToString(CultureInfo.CurrentCulture) != strValue
                ? ConvertToFieldTypeToEndDay(strValue)
                : ConvertToFieldType(strValue);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".LessOrEqual", value, field.Type);
            SetWhereExpression(Expression.LessThanOrEqual(field, valueExp), param);
        }

        protected virtual void BetweenExpression(string strValue1, string strValue2, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue1) || string.IsNullOrEmpty(strValue2))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value1 = ConvertToFieldType(strValue1);
            var value2 = ConvertToFieldTypeToEndDay(strValue2);
            var field = InvokeValueExpression(param);
            var valueExp1 = QueryParameters.GetExpression(field + ".GreaterThanOrEqual", value1, field.Type);
            var valueExp2 = QueryParameters.GetExpression(field + ".LessThanOrEqual", value2, field.Type);
            var exp1 = Expression.GreaterThanOrEqual(field, valueExp1);
            var exp2 = Expression.LessThanOrEqual(field, valueExp2);
            SetWhereExpression(Expression.And(exp1, exp2), param);
        }

        protected virtual void NotEqualsBetweenExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value1 = ConvertToFieldType(strValue);
            var value2 = ConvertToFieldTypeToEndDay(strValue);
            var field = InvokeValueExpression(param);
            var valueExp1 = QueryParameters.GetExpression(field + ".LessThan", value1, field.Type);
            var valueExp2 = QueryParameters.GetExpression(field + ".GreaterThan", value2, field.Type);
            var exp1 = Expression.LessThan(field, valueExp1);
            var exp2 = Expression.GreaterThan(field, valueExp2);
            SetWhereExpression(Expression.Or(exp1, exp2), param);
        }

        protected virtual void PeriodExpression(FilterItem filterItem, Type tableType)
        {
            if (OValueExpressionSecond == null)
            {
                throw new Exception(
                    "OValueExpressionSecond не инициализирован для фильтрации по PeriodExpression у фильтра " +
                    filterItem.FilterName);
            }

            var param = Expression.Parameter(tableType, "bFilter");
            var value1 = ConvertToFieldType(filterItem.Value1);
            var field = InvokeValueExpression(param);
            var field2 = InvokeValueExpressionSecond(param);
            var valueExp1 = QueryParameters.GetExpression(field + ".LessThan", value1, field.Type);
            var valueExp2 = QueryParameters.GetExpression(field2 + ".GreaterThan", value1, field.Type);
            var exp1 = Expression.LessThan(field, valueExp1);
            var exp2 = Expression.GreaterThan(field2, valueExp2);
            exp2 = Expression.Or(exp2, Expression.Equal(field2, Expression.Constant(null, field.Type)));
            var expression = Expression.And(exp1, exp2);
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected virtual void ContainsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");

            var valueExp = QueryParameters.GetExpression(field + ".Contains", strValue, field.Type);
            var exp = Expression.Call(field, "Contains", new Type[] { }, valueExp);
            SetWhereExpression(exp, param);
        }

        protected virtual void NotContainsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");

            var valueExp = QueryParameters.GetExpression(field + ".NotContains", strValue, field.Type);
            var exp = Expression.Not(Expression.Call(field, "Contains", new Type[] { }, valueExp));
            SetWhereExpression(exp, param);
        }

        protected virtual void StartsWithExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");
            var valueExp = QueryParameters.GetExpression(field + ".StartsWith", strValue, field.Type);
            var exp = Expression.Call(field, "StartsWith", new Type[] { }, valueExp);
            SetWhereExpression(exp, param);
        }

        protected virtual void EndsWithExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");
            var valueExp = QueryParameters.GetExpression(field + ".EndsWith", strValue, field.Type);
            var exp = Expression.Call(field, "EndsWith", new Type[] { }, valueExp);
            SetWhereExpression(exp, param);
        }

        protected virtual void ContainsWordsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");

            Expression filter = null;
            var split = strValue.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                var valueExp = QueryParameters.GetExpression(field + ".Contains", split[i], field.Type);
                filter = filter == null
                    ? (Expression) Expression.Call(field, "Contains", new Type[] { }, valueExp)
                    : Expression.And(filter, Expression.Call(field, "Contains", new Type[] { }, valueExp));
            }

            if (filter != null)
                SetWhereExpression(filter, param);
        }

        protected virtual void NotContainsWordsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");

            Expression filter = null;
            var split = strValue.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                var valueExp = QueryParameters.GetExpression(field + ".NotContains", split[i], field.Type);
                Expression filterExpression =
                    Expression.Not(Expression.Call(field, "Contains", new Type[] { }, valueExp));
                filter = filter == null ? filterExpression : Expression.And(filter, filterExpression);
            }

            if (filter != null)
                SetWhereExpression(filter, param);
        }

        protected virtual void ContainsAnyWordExpression(string strValue, Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = GetTextValueExpression(param);
            if (field == null)
                throw new Exception("Нет выражения позволяющего фильтровать по текстовому условию");
            Expression filter = null;
            var split = strValue.Split(new[] {' ', '\t', ','}, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                var valueExp = QueryParameters.GetExpression(field + ".OrContains", split[i], field.Type);
                filter = filter == null
                    ? (Expression) Expression.Call(field, "Contains", new Type[] { }, valueExp)
                    : Expression.Or(filter, Expression.Call(field, "Contains", new Type[] { }, valueExp));
            }

            if (filter != null)
                SetWhereExpression(filter, param);
        }

        protected virtual void StartsWithCodeExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".StartsWith", value, field.Type);
            var exp = Expression.Call(field, "StartsWith", new Type[] { }, valueExp);
            SetWhereExpression(exp, param);
        }

        protected virtual void NotStartsWithCodeExpression(string strValue, Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var value = ConvertToFieldType(strValue);
            var field = InvokeValueExpression(param);
            var valueExp = QueryParameters.GetExpression(field + ".NotStartsWith", value, field.Type);
            var exp = Expression.Not(Expression.Call(field, "StartsWith", new Type[] { }, valueExp));
            SetWhereExpression(exp, param);
        }

        protected void SetWhereExpression(Expression expression, ParameterExpression param)
        {
            if (CustomWhereExpression != null)
                expression = Expression.Invoke(CustomWhereExpression, param, expression);
            InnerWhereExpression = Expression.Lambda(expression, param);
            if (WhereExpression != null)
                expression = Expression.Or(expression, Expression.Invoke(WhereExpression, param));
            WhereExpression = Expression.Lambda(expression, param);
        }

        #endregion

        #region OnFilter

        protected virtual IQueryable OnFilter(
            IQueryable query, DefaultFilters.NumericFilter filterType, string value1, string value2, Type dataType)
        {
            switch (filterType)
            {
                case DefaultFilters.NumericFilter.Non:
                    break;
                case DefaultFilters.NumericFilter.Equals:
                    EqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.NotEquals:
                    NotEqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.NumericFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.NumericFilter.More:
                    MoreExpression(value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.Less:
                    LessExpression(value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.Between:
                    BetweenExpression(value1, value2, dataType);
                    break;
                case DefaultFilters.NumericFilter.DaysAgoAndMore:
                    DaysAgoAndMoreExpression(value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.DaysLeftAndMore:
                    DaysLeftAndMoreExpression(value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return query;
        }

        protected virtual IQueryable OnFilter(
            IQueryable query, DefaultFilters.ReferenceFilter filterType, string value1, string value2, Type dataType)
        {
            switch (filterType)
            {
                case DefaultFilters.ReferenceFilter.Non:
                    break;
                case DefaultFilters.ReferenceFilter.Equals:
                    EqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.NotEquals:
                    NotEqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.ReferenceFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsByRef:
                    ContainsExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.StartsWithByRef:
                    StartsWithExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.EndsWithByRef:
                    EndsWithExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsWordsByRef:
                    ContainsWordsExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsAnyWordByRef:
                    ContainsAnyWordExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.StartsWithCode:
                    StartsWithCodeExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.NotStartsWithCode:
                    NotStartsWithCodeExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.MoreOrEqual:
                    MoreOrEqualExpression(value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.LessOrEqual:
                    LessOrEqualExpression(value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return query;
        }

        protected virtual IQueryable OnFilter(
            IQueryable query, DefaultFilters.BooleanFilter filterType, string value1, string value2, Type dataType)
        {
            switch (filterType)
            {
                case DefaultFilters.BooleanFilter.Non:
                    break;
                case DefaultFilters.BooleanFilter.Equals:
                    EqualsExpression(string.IsNullOrEmpty(value1) ? "true" : value1, dataType);
                    break;
                case DefaultFilters.BooleanFilter.NotEquals:
                    NotEqualsExpression(string.IsNullOrEmpty(value1) ? "true" : value1, dataType);
                    break;
                case DefaultFilters.BooleanFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.BooleanFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return query;
        }

        protected virtual IQueryable OnFilter(
            IQueryable query, DefaultFilters.TextFilter filterType, string value1, string value2, Type dataType)
        {
            switch (filterType)
            {
                case DefaultFilters.TextFilter.Non:
                    break;
                case DefaultFilters.TextFilter.Equals:
                    EqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotEquals:
                    NotEqualsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.TextFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.TextFilter.Contains:
                    ContainsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.StartsWith:
                    StartsWithExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.EndsWith:
                    EndsWithExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.ContainsWords:
                    ContainsWordsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.ContainsAnyWord:
                    ContainsAnyWordExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotContains:
                    NotContainsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotContainsWords:
                    NotContainsWordsExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.LengthMore:
                    LengthMoreExpression(value1, dataType);
                    break;
                case DefaultFilters.TextFilter.LengthLess:
                    LengthLessExpression(value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return query;
        }

        protected virtual Expression OnFilter(
            DefaultFilters.NumericFilter filterType,
            FilterItem filterItem,
            Type dataType,
            QueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
            switch (filterType)
            {
                case DefaultFilters.NumericFilter.Non:
                    break;
                case DefaultFilters.NumericFilter.Equals:
                    EqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.NotEquals:
                    NotEqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.NumericFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.NumericFilter.More:
                    MoreExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.Less:
                    LessExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.Between:
                    BetweenExpression(filterItem.Value1, filterItem.Value2, dataType);
                    break;
                case DefaultFilters.NumericFilter.BetweenColumns:
                    PeriodExpression(filterItem, dataType);
                    break;
                case DefaultFilters.NumericFilter.DaysAgoAndMore:
                    DaysAgoAndMoreExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.DaysLeftAndMore:
                    DaysLeftAndMoreExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.ToDay:
                    ToDayExpression(dataType);
                    break;
                case DefaultFilters.NumericFilter.LessOrEqual:
                    LessOrEqualExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.NumericFilter.MoreOrEqual:
                    MoreOrEqualExpression(filterItem.Value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return GetJournalFilter(queryParameters, TableType);
        }

        protected virtual Expression OnFilter(
            DefaultFilters.ReferenceFilter filterType,
            FilterItem filterItem,
            Type dataType,
            QueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
            switch (filterType)
            {
                case DefaultFilters.ReferenceFilter.Non:
                    break;
                case DefaultFilters.ReferenceFilter.Equals:
                    EqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.NotEquals:
                    NotEqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.ReferenceFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsByRef:
                    ContainsExpression(filterItem.Value2, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.StartsWithByRef:
                    StartsWithExpression(filterItem.Value2, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.EndsWithByRef:
                    EndsWithExpression(filterItem.Value2, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsWordsByRef:
                    ContainsWordsExpression(filterItem.Value2, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsAnyWordByRef:
                    ContainsAnyWordExpression(filterItem.Value2, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.StartsWithCode:
                    StartsWithCodeExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.NotStartsWithCode:
                    NotStartsWithCodeExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.MoreOrEqual:
                    MoreOrEqualExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.ReferenceFilter.LessOrEqual:
                    LessOrEqualExpression(filterItem.Value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return GetJournalFilter(queryParameters, TableType);
        }

        protected virtual Expression OnFilter(
            DefaultFilters.BooleanFilter filterType,
            FilterItem filterItem,
            Type dataType,
            QueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
            switch (filterType)
            {
                case DefaultFilters.BooleanFilter.Non:
                    break;
                case DefaultFilters.BooleanFilter.Equals:
                    EqualsExpression(string.IsNullOrEmpty(filterItem.Value1) ? "true" : filterItem.Value1, dataType);
                    break;
                case DefaultFilters.BooleanFilter.NotEquals:
                    NotEqualsExpression(string.IsNullOrEmpty(filterItem.Value1) ? "true" : filterItem.Value1, dataType);
                    break;
                case DefaultFilters.BooleanFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.BooleanFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return GetJournalFilter(queryParameters, TableType);
        }

        protected virtual Expression OnFilter(
            DefaultFilters.TextFilter filterType, FilterItem filterItem, Type dataType, QueryParameters queryParameters)
        {
            QueryParameters = queryParameters;
            switch (filterType)
            {
                case DefaultFilters.TextFilter.Non:
                    break;
                case DefaultFilters.TextFilter.Equals:
                    EqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotEquals:
                    NotEqualsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.IsNotNull:
                    IsNotNullExpression(dataType);
                    break;
                case DefaultFilters.TextFilter.IsNull:
                    IsNullExpression(dataType);
                    break;
                case DefaultFilters.TextFilter.Contains:
                    ContainsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.StartsWith:
                    StartsWithExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.EndsWith:
                    EndsWithExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.ContainsWords:
                    ContainsWordsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.ContainsAnyWord:
                    ContainsAnyWordExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotContains:
                    NotContainsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.NotContainsWords:
                    NotContainsWordsExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.LengthMore:
                    LengthMoreExpression(filterItem.Value1, dataType);
                    break;
                case DefaultFilters.TextFilter.LengthLess:
                    LengthLessExpression(filterItem.Value1, dataType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("filterType");
            }

            return GetJournalFilter(queryParameters, TableType);
        }

        #endregion
    }

    public class BaseFilterParameter<TTable, TField> : BaseFilterParameter<TTable>
        where TTable : class
        where TField : struct
    {
        public BaseFilterParameter()
        {
        }

        public BaseFilterParameter(Expression<Func<TTable, TField?>> valueExpression)
        {
            ValueExpression = valueExpression;
        }

        public BaseFilterParameter(Expression<Func<TTable, TField?>> valueExpression,
            Expression<Func<TTable, string>> valueTextExpressionRu,
            Expression<Func<TTable, string>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;
            ValueTextExpressionRu = valueTextExpressionRu;
            ValueTextExpressionKz = valueTextExpressionKz;
        }

        public BaseFilterParameter(Expression<Func<TTable, TField?>> valueExpression, Expression<Func<TTable, char>> valueTextExpressionRu, Expression<Func<TTable, char>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;
            
            var param = Expression.Parameter(typeof(TTable), "r");
            Expression<Func<char, string>> eee = c => c.ToString();
            ValueTextExpressionRu =
                Expression.Lambda<Func<TTable, string>>(Expression.Invoke(eee, Expression.Invoke(valueTextExpressionRu, param)), param);
            ValueTextExpressionKz =
                Expression.Lambda<Func<TTable, string>>(Expression.Invoke(eee, Expression.Invoke(valueTextExpressionKz, param)), param);
        }
        public Expression<Func<TTable, string>> ValueTextExpressionRu { get; set; }
        public Expression<Func<TTable, string>> ValueTextExpressionKz { get; set; }

        public override Expression OValueExpression => ValueExpression;

        public override Expression OValueExpressionSecond => ValueExpressionSecond;

        public override Expression OTextValueExpressionKz => ValueTextExpressionKz ?? base.OTextValueExpressionKz;

        public override Expression OTextValueExpressionRu => ValueTextExpressionRu ?? base.OTextValueExpressionRu;

        protected override Type FieldType => typeof(TField);

        public Expression<Func<TTable, TField?>> ValueExpression { get; set; }

        public Expression<Func<TTable, TField?>> ValueExpressionSecond { get; set; }

        protected override void EqualsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            if (typeof(IStructInit).IsAssignableFrom(FieldType))
            {
                var multiValue = ((IStructInit)((IStructInit)new TField()).StructInit(strValue)).GetDic();
                var lambda = (LambdaExpression)OValueExpression;
                var param = lambda.Parameters[0];
                var bindings = ((MemberInitExpression)((UnaryExpression)lambda.Body).Operand).Bindings;
                Expression filter = null;

                foreach (var memberBinding in bindings)
                {
                    var binding = (MemberAssignment)memberBinding;
                    var field = binding.Expression;
                    var value = multiValue[binding.Member.Name];
                    var valueExp = QueryParameters.GetExpression(field + ".Equals", value, field.Type);

                    filter = filter == null
                                ? Expression.Equal(field, valueExp)
                                : Expression.And(filter, Expression.Equal(field, valueExp));
                }

                if (filter != null)
                    SetWhereExpression(filter, param);

                return;
            }

            base.EqualsExpression(strValue, tableType);
        }

        protected override void NotEqualsExpression(string strValue, Type tableType)
        {
            if (string.IsNullOrEmpty(strValue))
                return;

            if (typeof(IStructInit).IsAssignableFrom(FieldType))
            {
                var multiValue = ((IStructInit)((IStructInit)new TField()).StructInit(strValue)).GetDic();
                var lambda = (LambdaExpression)OValueExpression;
                var param = lambda.Parameters[0];
                var bindings = ((MemberInitExpression)((UnaryExpression)lambda.Body).Operand).Bindings;
                Expression filter = null;

                foreach (var memberBinding in bindings)
                {
                    var binding = (MemberAssignment)memberBinding;
                    var field = binding.Expression;
                    var value = multiValue[binding.Member.Name];
                    var valueExp = QueryParameters.GetExpression(field + ".Equals", value, field.Type);

                    filter = filter == null
                                ? Expression.NotEqual(field, valueExp)
                                : Expression.Or(filter, Expression.NotEqual(field, valueExp));
                }

                if (filter != null)
                    SetWhereExpression(filter, param);

                return;
            }

            base.NotEqualsExpression(strValue, tableType);
        }

        protected override void IsNullExpression(Type tableType)
        {
            if (typeof(IStructInit).IsAssignableFrom(FieldType))
            {
                var lambda = (LambdaExpression)OValueExpression;
                var param = lambda.Parameters[0];
                var bindings = ((MemberInitExpression)((UnaryExpression)lambda.Body).Operand).Bindings;
                Expression filter = null;

                foreach (var memberBinding in bindings)
                {
                    var binding = (MemberAssignment)memberBinding;
                    var field = binding.Expression;
                    var emptyExp = Expression.Constant(string.Empty);
                    var nullExp = Expression.Constant(null, field.Type);

                    Expression equal = Expression.Equal(field, nullExp);
                    if (field.Type == typeof(string))
                        equal = Expression.Or(equal, Expression.Equal(field, emptyExp));

                    filter = filter == null
                                 ? equal
                                 : Expression.Or(filter, equal);
                }

                if (filter != null)
                {
                    QueryParameters.RegisterExpression(filter);
                    SetWhereExpression(filter, param);
                }

                return;
            }

            base.IsNullExpression(tableType);
        }

        protected override void IsNotNullExpression(Type tableType)
        {
            if (typeof(IStructInit).IsAssignableFrom(FieldType))
            {
                var lambda = (LambdaExpression)OValueExpression;
                var param = lambda.Parameters[0];
                var bindings = ((MemberInitExpression)((UnaryExpression)lambda.Body).Operand).Bindings;
                Expression filter = null;

                foreach (var memberBinding in bindings)
                {
                    var binding = (MemberAssignment)memberBinding;
                    var field = binding.Expression;
                    var emptyExp = Expression.Constant(string.Empty);
                    var nullExp = Expression.Constant(null, field.Type);

                    Expression equal = Expression.NotEqual(field, nullExp);
                    if (field.Type == typeof(string))
                        equal = Expression.And(equal, Expression.NotEqual(field, emptyExp));

                    filter = filter == null
                                 ? equal
                                 : Expression.And(filter, equal);
                }

                if (filter != null)
                {
                    QueryParameters.RegisterExpression(filter);
                    SetWhereExpression(filter, param);
                }

                return;
            }

            base.IsNotNullExpression(tableType);
        }
    }

    public class BaseFilterParameter<TDataContext, TTable, TField> : BaseFilterParameter<TTable>
        where TDataContext : DataContext
        where TTable : class
        where TField : struct
    {
        public BaseFilterParameter()
        {
        }

        public BaseFilterParameter(Expression<Func<TDataContext, TTable, TField?>> valueExpression)
        {
            ValueExpression = valueExpression;
        }

        public BaseFilterParameter(
            Expression<Func<TDataContext, TTable, TField?>> valueExpression,
            Expression<Func<TDataContext, TTable, string>> valueTextExpressionRu,
            Expression<Func<TDataContext, TTable, string>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;
            ValueTextExpressionRu = valueTextExpressionRu;
            ValueTextExpressionKz = valueTextExpressionKz;
        }

        public BaseFilterParameter(
            Expression<Func<TDataContext, TTable, TField?>> valueExpression,
            Expression<Func<TDataContext, TTable, char>> valueTextExpressionRu,
            Expression<Func<TDataContext, TTable, char>> valueTextExpressionKz)
        {
            ValueExpression = valueExpression;

            var param = Expression.Parameter(typeof(TTable), "r");
            var dbParam = Expression.Parameter(typeof(TDataContext), "r");
            Expression<Func<char, string>> eee = c => c.ToString();
            ValueTextExpressionRu =
                Expression.Lambda<Func<TDataContext, TTable, string>>(Expression.Invoke(eee, Expression.Invoke(valueTextExpressionRu, param)), dbParam, param);
            ValueTextExpressionKz =
                Expression.Lambda<Func<TDataContext, TTable, string>>(Expression.Invoke(eee, Expression.Invoke(valueTextExpressionKz, param)), dbParam, param);
        }

        public Expression<Func<TDataContext, TTable, TField?>> ValueExpression { get; set; }
        public Expression<Func<TDataContext, TTable, TField?>> ValueExpressionSecond { get; set; }
        public Expression<Func<TDataContext, TTable, string>> ValueTextExpressionRu { get; set; }
        public Expression<Func<TDataContext, TTable, string>> ValueTextExpressionKz { get; set; }

        public override Expression OValueExpression => ValueExpression;

        public override Expression OValueExpressionSecond => ValueExpressionSecond;

        public override Expression OTextValueExpressionKz => ValueTextExpressionKz ?? base.OTextValueExpressionKz;

        public override Expression OTextValueExpressionRu => ValueTextExpressionRu ?? base.OTextValueExpressionRu;

        protected override Type FieldType => typeof(TField);
        
        protected override void IsNotNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.NotEqual(field, Expression.Constant(null, field.Type));
            expression = Expression.And(expression, Expression.NotEqual(field, Expression.Constant(string.Empty)));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }

        protected override void IsNullExpression(Type tableType)
        {
            var param = Expression.Parameter(tableType, "bFilter");
            var field = InvokeValueExpression(param);
            var expression = Expression.Equal(field, Expression.Constant(null, field.Type));
            expression = Expression.Or(expression, Expression.Equal(field, Expression.Constant(string.Empty)));
            QueryParameters.RegisterExpression(expression);
            SetWhereExpression(expression, param);
        }
    }
}