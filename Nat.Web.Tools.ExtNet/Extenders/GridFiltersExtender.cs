/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.26
* Copyright © JSC NAT Kazakhstan 2012
*/

using System.Threading;

namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.UI;

    using Ext.Net;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.Filter;

    using Newtonsoft.Json.Linq;

    using IGridColumn = Nat.Web.Tools.ExtNet.IGridColumn;

    public static class GridFiltersExtender
    {
        public static void InitializeColumns(this GridFilters gridFilter, IEnumerable<IGridColumn> columns)
        {
            foreach (var column in columns)
            {
                if (column.HasFilter)
                    gridFilter.Filters.Add(column.CreateFilter());

                if (column.HasChildren)
                    gridFilter.InitializeColumns(column.Children);
            }
        }

        public static void SetGridFilters<TKey, TTable, TDataContext, TRow>(
            this GridFilters gridFilter, 
            string gridFilterStr,
            BaseFilterControl<TKey, TTable, TDataContext> filter,
            BaseGridColumns columns)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            var conditions = GetFilterValues(gridFilterStr);
            if (conditions == null)
                return;

            filter.Filter += (sender, filterArgs) =>
                {
                    var filterColumns = columns.Columns.OfType<GridColumn>().ToDictionary(r => r.ColumnNameIndex);
                    foreach (var condition in conditions.Conditions)
                    {
                        var filterColumn = filterColumns[condition.Field];
                        if (filterColumn.SetGridFilterHandler != null)
                        {
                            filterColumn.SetGridFilterHandler(filterArgs, condition);
                            continue;
                        }

                        var property = filterColumn.FilterColumnMapping ?? condition.Field;
                        var param = Expression.Parameter(typeof(TTable), "filterExtNet");
                        Type fieldType;
                        var getValueExp = LinqFilterGenerator.GetProperty(
                            typeof(TTable), property, param, out fieldType);
                        var values = GetValue(condition, fieldType);
                        DateTime? dateEnd = null;
                        if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
                        {
                            values[0] = ((DateTime)values[0]).Date;
                            dateEnd = ((DateTime)values[0]).AddDays(1).AddSeconds(-1);
                        }

                        if (filterColumn.IsForeignKey && filterColumn.DataSource != null && !filterColumn.IsLookup
                            && (fieldType == typeof(long) || fieldType == typeof(long?))
                            && condition.Type == FilterType.List)
                        {
                            IEnumerable data = null;
                            var strValues = values.Cast<string>().ToDictionary(r => r.ToLower());
                            filterColumn.DataSource.GetView("").Select(new DataSourceSelectArguments(), c => data = c);
                            values = data.Cast<IDataRow>()
                                .Where(r => strValues.ContainsKey(r.Name.ToLower()))
                                .Select(r => Convert.ChangeType(r.Value, fieldType))
                                .ToArray();
                        }

                        Expression expression = null;
                        foreach (var value in values)
                        {
                            var exp3 = dateEnd != null
                                           ? filterArgs.QueryParameters.GetExpression(
                                               dateEnd.ToString(), dateEnd, fieldType)
                                           : null;
                            var tempExp = GetExpression(
                                getValueExp,
                                filterArgs.QueryParameters.GetExpression(value.ToString(), value, fieldType),
                                exp3,
                                condition);
                            expression = expression == null ? tempExp : Expression.Or(expression, tempExp);
                        }

                        if (expression != null)
                        {
                            var lambda = Expression.Lambda<Func<TTable, bool>>(expression, param);
                            filterArgs.AddFilter(lambda);
                        }
                    }
                };
        }

        public static string GetFilterValuesFromRequest(string filterParams, string filter)
        {
            var submitDirectEventConfig = HttpContext.Current.Request["submitDirectEventConfig"];
            if (!string.IsNullOrEmpty(submitDirectEventConfig))
            {
                var directConfig = JSON.Deserialize<JObject>(submitDirectEventConfig);
                var token = directConfig["config"];
                
                if (token != null)
                    token = token["extraParams"];
         
                if (token != null)
                    token = token[filterParams];

                var filtersStr = token == null ? null : token.Value<string>();

                return filtersStr;
            }

            return filter;
        }

        private static FilterConditions GetFilterValues(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
                return new FilterConditions(filter);

            return null;
        }

        private static object[] GetValue(FilterCondition condition, Type fieldType)
        {
            switch (condition.Type)
            {
                case FilterType.Boolean:
                    return new object[] {condition.Value<bool>()};
                case FilterType.Date:
                    return new object[] {condition.Value<DateTime>()};
                case FilterType.Numeric:
                    var castType = fieldType;
                    if (castType.IsGenericType
                        && castType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                        castType = castType.GetGenericArguments()[0];
                    var strValue = condition.Value<string>();
                    var decimalSeparator = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
                    var newstrValue = decimalSeparator == "."
                        ? strValue.Replace(",", ".")
                        : strValue.Replace(".", ",");
                    return new[] {Convert.ChangeType(newstrValue, castType)};
                case FilterType.String:
                    return new object[] {condition.Value<string>()};
                case FilterType.List:
                    return condition.List.Cast<object>().ToArray();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Expression GetExpression(
            Expression exp1, Expression exp2, Expression exp3, FilterCondition condition)
        {
            switch (condition.Comparison)
            {
                case Comparison.Eq:
                    switch (condition.Type)
                    {
                        case FilterType.Boolean:
                        case FilterType.Numeric:
                        case FilterType.List:
                            return Expression.Equal(exp1, exp2);
                        case FilterType.String:
                            return Expression.Call(exp1, "Contains", new Type[0], exp2);
                        case FilterType.Date:
                            return Expression.And(
                                Expression.GreaterThanOrEqual(exp1, exp2),
                                Expression.LessThanOrEqual(exp1, exp3));
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case Comparison.Gt:
                    if (condition.Type == FilterType.Date)
                        return Expression.GreaterThan(exp1, exp3);
                    return Expression.GreaterThan(exp1, exp2);
                case Comparison.Lt:
                    return Expression.LessThan(exp1, exp2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}