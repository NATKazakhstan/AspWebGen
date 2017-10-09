/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 7 октября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Reflection;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Tools;
using System.Collections;
using System.Web;

namespace Nat.Web.Controls
{
    public class LinqFilterGenerator
    {
        internal delegate void GenerateFilterHandler(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams);

        internal static readonly Dictionary<string, GenerateFilterHandler> filterHandlers = new Dictionary<string, GenerateFilterHandler>();

        public const string ParameterEqualsCollection = "EqualsCollection";
        public const string ParameterEquals = "Equals";
        public const string ParameterNotEquals = "NotEquals";
        public const string ParameterNotEqualsCollection = "NotEqualsCollection";
        public const string ParameterNon = "Non";

        static LinqFilterGenerator()
        {
            filterHandlers.Add(ParameterNon, delegate { });
            filterHandlers.Add(ParameterEquals, delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 == null) return;
                if (castType == typeof(DateTime))
                {
                    var date = Convert.ToDateTime(value1);
                    if (date == date.Date)
                    {
                        var dateExpLeft = left1;
                        var dateExpRight = right1;
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            dateExpLeft = Expression.Property(left1, "Value");

                        if (right1.Type.IsGenericType && right1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            dateExpRight = Expression.Property(right1, "Value");
                        filter = Expression.Equal(Expression.Property(dateExpLeft, "Year"), Expression.Property(dateExpRight, "Year"));
                        filter = Expression.And(filter, Expression.Equal(Expression.Property(dateExpLeft, "Month"), Expression.Property(dateExpRight, "Month")));
                        filter = Expression.And(filter, Expression.Equal(Expression.Property(dateExpLeft, "Day"), Expression.Property(dateExpRight, "Day")));
                    }
                }
                if(filter == null)
                {
                    if (!string.IsNullOrEmpty(value1) && castType == typeof(long) && value1.Contains(","))
                    {
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            left1 = Expression.Property(left1, "Value");
                        filter = Expression.Call(right1, "Contains", new Type[] { }, left1);
                    }
                    else
                        filter = Expression.Equal(left1, right1);
                }
            });
            filterHandlers.Add(ParameterEqualsCollection, delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                {
                    if (!string.IsNullOrEmpty(value1) && value1.Contains(","))
                    {
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            left1 = Expression.Property(left1, "Value");
                        filter = Expression.Call(right1, "Contains", new Type[] { }, left1);
                    }
                    else
                    {
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            left1 = Expression.Property(left1, "Value");
                        filter = Expression.Equal(left1, right1);
                    }
                }
            });
            filterHandlers.Add(ParameterNotEquals, delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 == null) return;
                if (castType == typeof(DateTime))
                {
                    var date = Convert.ToDateTime(value1);
                    if (date == date.Date)
                    {
                        var dateExp = left1;
                        var dateExpRight = right1;
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            dateExp = Expression.Property(left1, "Value");

                        if (right1.Type.IsGenericType && right1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            dateExpRight = Expression.Property(right1, "Value");

                        filter = Expression.NotEqual(Expression.Property(dateExp, "Year"), Expression.Property(dateExpRight, "Year"));
                        filter = Expression.Or(filter, Expression.NotEqual(Expression.Property(dateExp, "Month"), Expression.Property(dateExpRight, "Month")));
                        filter = Expression.Or(filter, Expression.NotEqual(Expression.Property(dateExp, "Day"), Expression.Property(dateExpRight, "Day")));
                    }
                }
                if (filter == null)
                {
                    if (!string.IsNullOrEmpty(value1) && castType == typeof(long) && value1.Contains(","))
                    {
                        if (left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
                            left1 = Expression.Property(left1, "Value");
                        filter = Expression.Not(Expression.Call(right1, "Contains", new Type[] { }, left1));
                    }
                    else
                        filter = Expression.NotEqual(left1, right1);
                }
            });
            filterHandlers.Add(ParameterNotEqualsCollection, delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                {
                    var nullable = left1.Type.IsGenericType && left1.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>));
                    var canBeNull = nullable || left1.Type == typeof(string);
                    if (!string.IsNullOrEmpty(value1) && value1.Contains(","))
                    {
                        if (canBeNull)
                        {
                            var isNull = Expression.Equal(left1, Expression.Constant(null, fieldType));
                            if (nullable)
                                left1 = Expression.Property(left1, "Value");
                            filter = Expression.Not(Expression.Call(right1, "Contains", new Type[] { }, left1));
                            filter = Expression.Or(filter, isNull);
                        }
                        else
                            filter = Expression.Not(Expression.Call(right1, "Contains", new Type[] { }, left1));
                    }
                    else
                    {
                        if (canBeNull)
                        {
                            var isNull = Expression.Equal(left1, Expression.Constant(null, fieldType));
                            if (nullable)
                                left1 = Expression.Property(left1, "Value"); 
                            filter = Expression.NotEqual(left1, right1);
                            filter = Expression.Or(filter, isNull);
                        }
                        else
                            filter = Expression.NotEqual(left1, right1);
                    }
                }
            });
            filterHandlers.Add("Contains", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                //if (right1 != null)
                //    filter = Expression.Call(typeof(SqlMethods), "Like", new Type[0], left1, right1);
                if (right1 != null)
                    filter = Expression.Call(left1, "Contains", new Type[] { }, right1);
            });
            filterHandlers.Add("NotContains", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                //if (right1 != null)
                //    filter = Expression.Call(typeof(SqlMethods), "Like", new Type[0], left1, right1);
                if (right1 != null)
                    filter = Expression.Not(Expression.Call(left1, "Contains", new Type[] { }, right1));
            });
            filterHandlers.Add("ContainsWords", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                var split = value1.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < split.Length; i++)
                {
                    if (qParams != null)
                        right1 = qParams.GetExpression(fieldName + ".ContainsWords", Convert.ChangeType(split[i], castType), fieldType);
                    else
                        right1 = Expression.Constant(Convert.ChangeType(split[i], castType), fieldType);
                    filter = filter == null
                                 ? (Expression)Expression.Call(left1, "Contains", new Type[] { }, right1)
                                 : Expression.And(filter, Expression.Call(left1, "Contains", new Type[] { }, right1));
                }
            });
            filterHandlers.Add("IsNull", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (!IsNullableType(fieldType))
                    filter = Expression.Constant(false);
                else
                {
                    right1 = Expression.Constant(null, fieldType);
                    filter = Expression.Equal(left1, right1);
                }
            });
            filterHandlers.Add("IsNotNull", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                right1 = Expression.Constant(null, fieldType);
                filter = Expression.NotEqual(left1, right1);
            });
            filterHandlers.Add("More", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.GreaterThan(left1, right1);
            });
            filterHandlers.Add("MoreOrEqual", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.GreaterThanOrEqual(left1, right1);
            });
            filterHandlers.Add("MoreOrEqualOrNull", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Or(Expression.GreaterThanOrEqual(left1, right1),
                                           Expression.Equal(left1, Expression.Constant(null, fieldType)));
            });
            filterHandlers.Add("MoreOrNull", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Or(Expression.GreaterThan(left1, right1),
                                           Expression.Equal(left1, Expression.Constant(null, fieldType)));
            });
            filterHandlers.Add("Less", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.LessThan(left1, right1);
            });
            filterHandlers.Add("LessOrEqual", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.LessThanOrEqual(left1, right1);
            });
            filterHandlers.Add("LessOrEqualOrNull", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Or(Expression.LessThanOrEqual(left1, right1),
                                           Expression.Equal(left1, Expression.Constant(null, fieldType)));
            });
            filterHandlers.Add("Between", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                {
                    Expression right2 = Expression.Constant(Convert.ChangeType(value2, castType), fieldType);
                    Expression left2 = GetProperty(tableType, fieldName, param);
                    filter = Expression.And(Expression.GreaterThanOrEqual(left1, right1),
                                            Expression.LessThanOrEqual(left2, right2));
                }
            });
            filterHandlers.Add("StartsWith", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Call(left1, "StartsWith", new Type[] { }, right1);
            });
            filterHandlers.Add("StartsWithCode", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Call(left1, "StartsWith", new Type[] { }, right1);
            });
            filterHandlers.Add("NotStartsWithCode", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Not(Expression.Call(left1, "StartsWith", new Type[] { }, right1));
            });
            filterHandlers.Add("EndsWith", delegate(Expression left1, Expression right1, string value1, string value2, Type fieldType, Type castType, Type tableType, string fieldName, ParameterExpression param, ref Expression filter, QueryParameters qParams)
            {
                if (right1 != null)
                    filter = Expression.Call(left1, "EndsWith", new Type[] { }, right1);
            });
        }


        public static string RegisterFilterName(string filterName, string tableName)
        {
            if (MainPageUrlBuilder.Current.DeniedUseShortFilters) return filterName;
            var dic = (Dictionary<string, Dictionary<int, string>>)HttpContext.Current.Items["RegisteredFilterName"];
            var dicBack = (Dictionary<string, Dictionary<string, string>>)HttpContext.Current.Items["RegisteredFilterNameBack"];
            if (dic == null)
            {
                HttpContext.Current.Items["RegisteredFilterName"] = dic = new Dictionary<string, Dictionary<int, string>>();
                HttpContext.Current.Items["RegisteredFilterNameBack"] = dicBack = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!dic.ContainsKey(tableName))
            {
                dic[tableName] = new Dictionary<int, string>();
                dicBack[tableName] = new Dictionary<string, string>();
            }
            var dicItem = dic[tableName];
            var dicItemBack = dicBack[tableName];
            if (dicItemBack.ContainsKey(filterName))
                return dicItemBack[filterName];
            var index = dicItem.Count;
            dicItem.Add(index, filterName);
            return dicItemBack[filterName] = "." + tableName + "." + index;
        }

        public static string GetFilterName(string filterName)
        {
            if (!filterName.StartsWith("."))
                return filterName;
            var dic = (Dictionary<string, Dictionary<int, string>>)HttpContext.Current.Items["RegisteredFilterName"];
            if (dic == null) return filterName;
            var dotIndex = filterName.IndexOf('.', 2);
            var tableName = filterName.Substring(1, dotIndex - 1);
            var index = Convert.ToInt32(filterName.Substring(dotIndex + 1));
            return dic[tableName][index];
        }

        public static void GenerateFilter(StringBuilder where, IDictionary<string, Parameter> parameters, string filterType, TypeCode typeCode, string columnName, string parameterName, string value1, string value2)
        {
            switch (filterType)
            {
                case "Non":
                    break;
                case "Equal":
                    GenerateFilter(where, parameters, typeCode, "{0} == @{1}", columnName, parameterName, value1);
                    break;
                case "NotEqual":
                    GenerateFilter(where, parameters, typeCode, "{0} != @{1}", columnName, parameterName, value1);
                    break;
                case "Contains":
                    GenerateFilter(where, parameters, typeCode, "{0}.Contains(@{1})", columnName, parameterName, value1);
                    break;
                case "ContainsWords":
                    var split = value1.Split(new [] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < split.Length; i++)
                    {
                        GenerateFilter(where, parameters, typeCode, "{0}.Contains(@{1})", columnName, parameterName, split[i]);
                    }
                    break;
                case "IsNull":
                    if (where.Length > 0) where.Append(" && ");
                    where.AppendFormat("{0} == null", columnName);
                    break;
                case "IsNotNull":
                    if (where.Length > 0) where.Append(" && ");
                    where.AppendFormat("{0} != null", columnName);
                    break;
                case "More":
                    GenerateFilter(where, parameters, typeCode, "{0} > @{1}", columnName, parameterName, value1);
                    break;
                case "Less":
                    GenerateFilter(where, parameters, typeCode, "{0} < @{1}", columnName, parameterName, value1);
                    break;
                case "Between":
                    GenerateFilter(where, parameters, typeCode, "({0} >= @{1} && {0} <= @{2})", columnName, parameterName, value1, value2);
                    break;
                case "StartWith":
                    GenerateFilter(where, parameters, typeCode, "{0}.StartsWith(@{1})", columnName, parameterName, value1);
                    break;
                case "EndWith":
                    GenerateFilter(where, parameters, typeCode, "{0}.EndsWith(@{1})", columnName, parameterName, value1);
                    break;
            }
        }

        public static void GenerateFilter(StringBuilder where, IDictionary<string, Parameter> parameters,
                                          IDictionary<string, string> queryParameters)
        {
            foreach (var parameter in queryParameters)
            {
                GenerateFilter(where, parameters, "Equal", TypeCode.Int64, parameter.Key, "queryParam", parameter.Value, null);
            }
        }

        private static void GenerateFilter(StringBuilder where, IDictionary<string, Parameter> parameters, TypeCode typeCode, string format, string columnName, string parameterName, string value)
        {
            if (value == null) return;

            if (where.Length > 0) where.Append(" && ");
            var paramName = string.Format("F{0}", parameterName);
            int index = 0;
            while (parameters.ContainsKey(paramName))
                paramName = string.Format("F{0}{1}", parameterName, index++);
            where.AppendFormat(format, columnName, paramName);
            parameters.Add(paramName, new Parameter(paramName, typeCode, value));
        }

        private static void GenerateFilter(StringBuilder where, IDictionary<string, Parameter> parameters, TypeCode typeCode, string format, string columnName, string parameterName, string value1, string value2)
        {
            if (value1 == null || value2 == null) return;

            if (where.Length > 0) where.Append(" && ");
            var paramName1 = string.Format("F{0}1", parameterName);
            var paramName2 = string.Format("F{0}2", parameterName);
            int index = 0;
            while (parameters.ContainsKey(paramName1))
                paramName1 = string.Format("F{0}{1}", parameterName, index++);

            while (parameters.ContainsKey(paramName2))
                paramName2 = string.Format("F{0}{1}", parameterName, index++);

            where.AppendFormat(format, columnName, paramName1, paramName2);
            parameters.Add(paramName1, new Parameter(paramName1, typeCode, value1));
            parameters.Add(paramName2, new Parameter(paramName2, typeCode, value2));
        }

        public static Expression GenerateFilter(Expression source, Type tableType, string filterType, string fieldName, string value1, string value2)
        {
            var filter = GenerateFilter(tableType, filterType, fieldName, value1, value2);
            if (filter == null) return source;
            return Expression.Call(typeof(Queryable), "Where", new[] { tableType }, source, filter);
        }

        public static Expression GenerateFilter(Expression source, Type tableType, string filterType, string fieldName, string value1, string value2, QueryParameters qParams)
        {
            var filter = GenerateFilter(tableType, filterType, fieldName, value1, value2, qParams);
            if (filter == null) return source;
            return Expression.Call(typeof(Queryable), "Where", new[] { tableType }, source, filter);
        }

        public static IQueryable GenerateFilter(IQueryable source, Type tableType, string filterType, string fieldName, string value1, string value2)
        {
            var filter = GenerateFilter(tableType, filterType, fieldName, value1, value2);
            if (filter == null) return source;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new[] { tableType }, source.Expression, filter);
            return source.Provider.CreateQuery(expr);
        }

        public static IQueryable<T> GenerateFilter<T>(IQueryable<T> source, string filterType, string fieldName, string value1, string value2)
            where T: class
        {
            var filter = GenerateFilter(typeof(T), filterType, fieldName, value1, value2);
            if (filter == null) return source;
            Expression expr = Expression.Call(typeof(Queryable), "Where", new[] { typeof(T) }, source.Expression, filter);
            return source.Provider.CreateQuery<T>(expr);
        }

        public static Expression GenerateFilter(Type tableType, string filterType, string fieldName, string value1, string value2)
        {
            return GenerateFilter(tableType, filterType, fieldName, value1, value2, null);
        }

        public static Expression GenerateFilter(Type tableType, string filterType, string fieldName, string value1, string value2, QueryParameters qParams)
        {
            ParameterExpression param = Expression.Parameter(tableType, "c");
            Expression filter = null;
            fieldName = GetFilterName(fieldName);
            var fieldSplit = fieldName.Split(',');
            if (fieldSplit.Length > 1)
            {
                var textValues = fieldSplit[fieldSplit.Length - 1].Split(':');
                if (textValues.Length > 1)
                {
                    var textValue = ":" + fieldSplit[1] + (fieldSplit.Length > 2 ? ":" + fieldSplit[2] : "");
                    for (int i = 0; i < fieldSplit.Length - 1; i++)
                        fieldSplit[i] = fieldSplit[i] + textValue;
                }
                string[] value1Split = null;
                string[] value2Split = null;
                if (value1 != null)
                    value1Split = value1.Split(',');
                if (value2 != null)
                    value2Split = value2.Split(',');
                Expression filter2 = null;
                for (int i = 0; i < fieldSplit.Length; i++)
                {
                    CreateFilter(tableType, filterType, fieldSplit[i],
                        value1Split != null && value1Split.Length > i ? value1Split[i] : null,
                        value2Split != null && value2Split.Length > i ? value2Split[i] : null,
                        param, ref filter2, qParams);
                    if (filter != null)
                    {
                        if (filterType == ParameterEquals)
                            filter2 = Expression.And(filter, filter2);
                        else if (filterType == ParameterNotEquals)
                            filter2 = Expression.Or(filter, filter2);
                        else
                            throw new NotSupportedException("Other FilterTypes Not supported");
                    }
                    filter = filter2;
                }
            }
            else
                CreateFilter(tableType, filterType, fieldName, value1, value2, param, ref filter, qParams);
            if (filter == null) return null;
            return Expression.Lambda(filter, param);
        }

        private static void CreateFilter(Type tableType, string filterType, string fieldName, string value1, string value2, ParameterExpression param, ref Expression filter, QueryParameters qParams)
        {
            filter = null;
            GenerateFilterHandler filterHandler;
            fieldName = GetFilterName(fieldName);
            var textValues = fieldName.Split(':');
            if (textValues.Length > 1) fieldName = textValues[0];
            if (filterType.EndsWith("ByRef"))
            {
                filterType = filterType.Substring(0, filterType.Length - 5);
                if (textValues.Length > 2)
                {
                    if (LocalizationHelper.IsCultureKZ)
                        fieldName = textValues[2];
                    else
                        fieldName = textValues[1];
                }
                else
                    fieldName = textValues[1];
                value1 = value2;
            }
            else if (filterType.Equals("BetweenColumns"))
            {
                if (textValues.Length < 2)
                    throw new Exception("Не указана вторая колонка для фильтрации по периоду для фльтра " + fieldName);
                Expression filterLess = null;
                Expression filterMore = null;
                CreateFilter(tableType, "Less", textValues[0], value1, value2, param, ref filterLess, qParams);
                CreateFilter(tableType, "MoreOrNull", textValues[1], value1, value2, param, ref filterMore, qParams);
                if (filterLess != null && filterMore != null)
                    filter = Expression.And(filterLess, filterMore);
                return;
            }
            var index = fieldName.LastIndexOf('.');
            if (index > -1 && filterHandlers.ContainsKey(fieldName.Substring(index + 1)))
            {
                filterType = fieldName.Substring(index + 1);
                filterHandler = filterHandlers[filterType];
                fieldName = fieldName.Substring(0, index);
            }
            else
            {
                switch (filterType)
                {
                    case "Non":
                        break;
                    case "Equal":
                        filterType = ParameterEquals;
                        break;
                    case "NotEqual":
                        filterType = ParameterNotEquals;
                        break;
                    case "StartWith":
                        filterType = "StartsWith";
                        break;
                    case "EndWith":
                        filterType = "EndsWith";
                        break;
                }
                filterHandler = filterHandlers[filterType];
            }
            Type fieldType;
            Expression left1 = GetProperty(tableType, fieldName, param, out fieldType);
            Expression right1 = null;
            var castType = fieldType;
            try
            {
                if (castType.IsGenericType && castType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>))) castType = castType.GetGenericArguments()[0];
                if (!string.IsNullOrEmpty(value1)
                    && !("IsNull".Equals(filterType, StringComparison.OrdinalIgnoreCase)
                         || "IsNotNull".Equals(filterType, StringComparison.OrdinalIgnoreCase)
                        )
                   )
                {
                    //если значение дробное, то нужно проверить соответствие раздилителя, если не верно, то заменить на нужный разделитель
                    CheckNumber(ref value1, ref value2, castType);
                    if (castType == typeof (long) && value1.Contains(","))
                    {
                        var collection = GetValueCollection<long>(value1);
                        if (qParams != null)
                            right1 = qParams.GetExpression(fieldName + "." + filterType, collection);
                        else
                            right1 = Expression.Constant(collection);
                    }
                    else if (filterType.EndsWith("Collection", StringComparison.OrdinalIgnoreCase) &&
                             value1.Contains(","))
                    {
                        right1 = GetValueCollectionExpression(value1, castType);
                        qParams.RegisterExpression(right1, value1);
                    }
                    else if (qParams != null)
                        right1 = qParams.GetExpression(fieldName + "." + filterType,
                                                       Convert.ChangeType(value1, castType), fieldType);
                    else
                        right1 = Expression.Constant(Convert.ChangeType(value1, castType), fieldType);
                }
            }
            catch
            {
            }

            qParams.RegisterExpression(left1);
            qParams.RegisterParameter(filterType);
            filterHandler(left1, right1, value1, value2, fieldType, castType, tableType, fieldName, param, ref filter, qParams);
        }

        private static void CheckNumber(ref string value1, ref string value2, Type castType)
        {
            if ((castType != typeof (double) && castType != typeof (decimal)) && castType != typeof (float))
                return;

            if (value1.Contains("."))
            {
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                    value1 = value1.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
            else if (value1.Contains(","))
            {
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ",")
                    value1 = value1.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
            if (!string.IsNullOrEmpty(value2))
            {
                if (value2.Contains("."))
                {
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                        value2 = value2.Replace(".", CultureInfo.CurrentCulture.NumberFormat.
                                                         NumberDecimalSeparator);
                }
                else if (value2.Contains(","))
                {
                    if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ",")
                        value2 = value2.Replace(",", CultureInfo.CurrentCulture.NumberFormat.
                                                         NumberDecimalSeparator);
                }
            }
        }

        public static IList GetValueCollection(string value, Type castType)
        {
            IList collection = null;
            if (typeof(int) == castType)
                collection = GetValueCollection<int>(value);
            else if (typeof(string) == castType)
                collection = GetValueCollection<string>(value);
            else if (typeof(byte) == castType)
                collection = GetValueCollection<byte>(value);
            else if (typeof(long) == castType)
                collection = GetValueCollection<long>(value);
            else if (typeof(DateTime) == castType)
                collection = GetValueCollection<DateTime>(value);
            else if (typeof(short) == castType)
                collection = GetValueCollection<short>(value);
            else if (typeof(decimal) == castType)
                collection = GetValueCollection<decimal>(value);
            else if (typeof(float) == castType)
                collection = GetValueCollection<float>(value);
            else if (typeof(double) == castType)
                collection = GetValueCollection<double>(value);
            else if (typeof(char) == castType)
                collection = GetValueCollection<char>(value);
            return collection;
        }


        public static Expression GetValueCollectionExpression(string value, Type castType)
        {
            Expression expression = null;
            if (typeof(int) == castType)
                expression = Expression.Constant(GetValueCollection<int>(value));
            else if (typeof(string) == castType)
                expression = Expression.Constant(GetValueCollection<string>(value));
            else if (typeof(byte) == castType)
                expression = Expression.Constant(GetValueCollection<byte>(value));
            else if (typeof(long) == castType)
                expression = Expression.Constant(GetValueCollection<long>(value));
            else if (typeof(DateTime) == castType)
                expression = Expression.Constant(GetValueCollection<DateTime>(value));
            else if (typeof(short) == castType)
                expression = Expression.Constant(GetValueCollection<short>(value));
            else if (typeof(decimal) == castType)
                expression = Expression.Constant(GetValueCollection<decimal>(value));
            else if (typeof(float) == castType)
                expression = Expression.Constant(GetValueCollection<float>(value));
            else if (typeof(double) == castType)
                expression = Expression.Constant(GetValueCollection<double>(value));
            else if (typeof(char) == castType)
                expression = Expression.Constant(GetValueCollection<char>(value));
            return expression;
        }

        public static List<long> GetValueCollection(string value)
        {
            return GetValueCollection<long>(value);
        }

        public static List<T> GetValueCollection<T>(string value)
        {
            var split = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (typeof(T) == typeof(string))
            {
                var collection = new List<T>(split.Length);
                for (int i = 0; i < split.Length; i++)
                    collection.Add((T)(object)split[i]);
                return collection;
            }
            else
            {
                var collection = new List<T>(split.Length);
                for (int i = 0; i < split.Length; i++)
                    collection.Add((T)Convert.ChangeType(split[i], typeof(T)));
                return collection;
            }
        }

        public static List<TResult> GetValueCollection<TCast, TResult>(string value)
        {
            var split = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var collection = new List<TResult>(split.Length);
            if (typeof(TCast) == typeof(string))
            {
                for (int i = 0; i < split.Length; i++)
                    collection.Add((TResult)(object)split[i]);
            }
            else
            {
                for (int i = 0; i < split.Length; i++)
                    collection.Add((TResult)Convert.ChangeType(split[i], typeof(TCast)));
            }
            return collection;
        }

        public static List<string> GetValueCollectionString(string value)
        {
            var split = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var collection = new List<string>(split.Length);
            for (int i = 0; i < split.Length; i++)
                collection.Add(split[i]);
            return collection;
        }

        public static Expression GetProperty(Type type, string fieldName, Expression expression)
        {
            if (typeof(IRow).IsAssignableFrom(type))
                GetMemberItem(ref type, ref expression);
            foreach (var propertyName in fieldName.Split('.'))
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new Exception("не существует свойство " + propertyName + " у таблицы " + type.FullName);
                type = property.PropertyType;
//                if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Nullable<>)))
//                    expression = Expression.Property(Expression.Property(expression, property), "Value");
//                else
                    expression = Expression.Property(expression, property);
            }
            return expression;
        }

        public static Expression GetProperty(Type type, string fieldName, Expression expression, out string fieldNameLast)
        {
            fieldNameLast = null;
            if (typeof(IRow).IsAssignableFrom(type))
                GetMemberItem(ref type, ref expression);
            foreach (var propertyName in fieldName.Split('.'))
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new Exception("не существует свойство " + propertyName + " у таблицы " + type.FullName);
                type = property.PropertyType;
                expression = Expression.Property(expression, property);
                fieldNameLast = propertyName;
            }
            return expression;
        }

        private static void GetMemberItem(ref Type type, ref Expression expression)
        {
            /*
            var field = type.GetField("Item");
            if (field != null)
            {
                expression = Expression.Field(expression, field);
                type = field.FieldType;
            }
            else*/
            {
                var property = type.GetProperty("Item");
                expression = Expression.Property(expression, property);
                type = property.PropertyType;
            }
        }

        public static Expression GetProperty(Type type, string fieldName, Expression expression, out Type fieldType)
        {
            return GetProperty(type, fieldName, expression, true, out fieldType);
        }

        public static Expression GetProperty(Type type, string fieldName, Expression expression, bool detectItem, out Type fieldType)
        {
            if (typeof(IRow).IsAssignableFrom(type) && detectItem)
                GetMemberItem(ref type, ref expression);

            foreach (var propertyName in fieldName.Split('.'))
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                    throw new Exception("не существует свойство " + propertyName + " у таблицы " + type.FullName);

                type = property.PropertyType;
                expression = Expression.Property(expression, property);
            }

            fieldType = type;
            return expression;
        }

        private static MethodInfo _methodEqualsOfObject = typeof(object).GetMethod(ParameterEquals, new[] { typeof(object) });

        //todo: подумать почему, то не всегда проставляется проверка на null
        public static int GetIndexBySortExpression<TableType>(string sortExpression, IQueryable<TableType> dt, string primaryKeys, IQueryable<TableType> selectedValue) 
            where TableType : class
        {
            var data = dt.Select(r => new { Item = r, Selected = selectedValue.FirstOrDefault()});
            //var primaryKeysArray = primaryKeys.Split(',');
            ParameterExpression param = Expression.Parameter(data.ElementType, "c");
            Expression filter = null;
            Expression currentFilter = null;
            
            foreach (string orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);
                
                string orderByColumn = split[0];
                Expression exp;
                Type fieldType;
                Expression property1;
                Expression property2;
                if (ascending)
                {
                    property2 = LinqFilterGenerator.GetProperty(data.ElementType, "Selected." + orderByColumn, param, out fieldType);
                    property1 = LinqFilterGenerator.GetProperty(data.ElementType, "Item." + orderByColumn, param);
                }
                else
                {
                    property1 = LinqFilterGenerator.GetProperty(data.ElementType, "Selected." + orderByColumn, param, out fieldType);
                    property2 = LinqFilterGenerator.GetProperty(data.ElementType, "Item." + orderByColumn, param);
                }
                var compare = Compare(property1, property2, fieldType);
                var equalsNull1 = Expression.Call(Expression.Constant(null), _methodEqualsOfObject, Expression.Convert(property1, typeof(object)));
                var equalsNull2 = Expression.Call(Expression.Constant(null), _methodEqualsOfObject, Expression.Convert(property2, typeof(object)));

                //if (primaryKeysArray.Contains(orderByColumn))
                //    exp = Expression.LessThanOrEqual(compare, Expression.Constant(0));
                //else
                exp = Expression.LessThan(compare, Expression.Constant(0));
                exp = Expression.Or(exp, Expression.And(equalsNull1, Expression.Not(equalsNull2)));

                if (filter == null)
                    filter = exp;
                else
                    filter = Expression.Or(filter, Expression.And(currentFilter, exp));

                exp = Expression.Or(Expression.Equal(property1, property2),
                      Expression.And(equalsNull1, equalsNull2));

                if (currentFilter == null)
                    currentFilter = exp;
                else 
                    currentFilter = Expression.And(currentFilter, exp);
            }
            Expression pred = Expression.Lambda(filter, param);
            Expression expr = Expression.Call(typeof(Queryable), "Count", new[] { data.ElementType }, data.Expression, pred);
            return data.Provider.Execute<int>(expr);
        }

        private static Expression Compare(Expression expression1, Expression expression2, Type elementType)
        {
            if (!elementType.IsGenericType)
            {
                var method = elementType.GetMethod("CompareTo", new Type[] { elementType });
                return Expression.Call(expression1, method, expression2);
            }
            else
            {
                elementType = elementType.GetGenericArguments()[0];
                var method = elementType.GetMethod("CompareTo", new Type[] { elementType });
                return Expression.Call(Expression.Property(expression1, "Value"), method, Expression.Property(expression2, "Value"));
            }
        }


        public static IQueryable<T> OrderBy<T>(IQueryable<T> data, string sortExpression)
            where T : class
        {
            var query = data.Expression;
            string str = "OrderBy";
            string str2 = "OrderByDescending";
            foreach (string orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);

                string orderByColumn = split[0];
                ParameterExpression param = Expression.Parameter(typeof(T), "c");
                Type fieldType;
                var property = GetProperty(typeof(T), orderByColumn, param, out fieldType);
                query = Expression.Call(typeof(Queryable),
                                        ascending ? str : str2,
                                        new[] { typeof(T), fieldType },
                                        query,
                                        Expression.Lambda(property, param));
                str = "ThenBy";
                str2 = "ThenByDescending";
            }
            return data.Provider.CreateQuery<T>(query);
        }

        public static Expression GetSortedExpression<T>(Expression query, string sortExpression)
            where T : class
        {
            string str = "OrderBy";
            string str2 = "OrderByDescending";
            foreach (string orderBy in sortExpression.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var split = orderBy.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool ascending = true;
                if (split.Length == 2)
                    ascending = split[1].Equals("Asc", StringComparison.OrdinalIgnoreCase);

                string orderByColumn = split[0];
                ParameterExpression param = Expression.Parameter(typeof(T), "c");
                Type fieldType;
                var property = GetProperty(typeof(T), orderByColumn, param, out fieldType);
                query = Expression.Call(typeof(Queryable),
                                        ascending ? str : str2,
                                        new[] { typeof(T), fieldType },
                                        query,
                                        Expression.Lambda(property, param));
                str = "ThenBy";
                str2 = "ThenByDescending";
            }
            return query;
        }

        #region FilterHandler Get-Methods

        public static FilterHandler GetTextFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData>> expGetField)
            where TTable : class
            where TChildTable : class
        {
            FilterHandler handler =
                delegate(IQueryable query, Enum type, string value1, string value2)
                    {
                        ParameterExpression paramChild;
                        Expression filter;
                        InvocationExpression expChilds;
                        ParameterExpression param = GetTextFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, out filter, out expChilds);
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, paramChild);
                            var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                            new[] { typeof(TChildTable) }, expChilds, lambda);
                            filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                            lambda = Expression.Lambda(filter, param);
                            var resExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, query.Expression, lambda);
                            return query.Provider.CreateQuery(resExp);
                        }
                        return query;
                    };
            return handler;
        }


        public static ExpressionFilterHandler GetTextExpressionFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData>> expGetField)
            where TTable : class
            where TChildTable : class
        {
            ExpressionFilterHandler handler =
                delegate(Enum type, string value1, string value2, QueryParameters @params)
                {
                    ParameterExpression paramChild;
                    Expression filter;
                    InvocationExpression expChilds;
                    ParameterExpression param = GetTextFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, out filter, out expChilds);
                    if (filter != null)
                    {
                        var lambda = Expression.Lambda(filter, paramChild);
                        var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                        new[] { typeof(TChildTable) }, expChilds, lambda);
                        filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                        lambda = Expression.Lambda(filter, param);
                        return lambda;
                    }
                    return null;
                };
            return handler;
        }

        public static FilterHandler GetNumericFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {
            FilterHandler handler =
                delegate(IQueryable query, Enum type, string value1, string value2)
                    {
                        ParameterExpression paramChild;
                        Expression filter;
                        InvocationExpression expChilds;
                        ParameterExpression param = GetNumericFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, value2, out filter, out expChilds);
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, paramChild);
                            var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                            new[] { typeof(TChildTable) }, expChilds, lambda);
                            filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                            lambda = Expression.Lambda(filter, param);
                            var resExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, query.Expression, lambda);
                            return query.Provider.CreateQuery(resExp);
                        }
                        return query;
                    };
            return handler;
        }

        public static ExpressionFilterHandler GetNumericExpressionFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {
            ExpressionFilterHandler handler =
                delegate(Enum type, string value1, string value2, QueryParameters @params)
                    {
                        ParameterExpression paramChild;
                        Expression filter;
                        InvocationExpression expChilds;
                        ParameterExpression param = GetNumericFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, value2, out filter, out expChilds);
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, paramChild);
                            var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                            new[] { typeof(TChildTable) }, expChilds, lambda);
                            filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                            lambda = Expression.Lambda(filter, param);
                            return lambda;
                        }
                        return null;
                    };
            return handler;
        }


        public static FilterHandler GetReferenceFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {

            FilterHandler handler =
                delegate(IQueryable query, Enum type, string value1, string value2)
                    {
                        ParameterExpression paramChild;
                        Expression filter;
                        InvocationExpression expChilds;
                        ParameterExpression param =
                           GetRefFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, out filter, out expChilds);
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, paramChild);
                            var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                            new[] { typeof(TChildTable) }, expChilds, lambda);
                            filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                            lambda = Expression.Lambda(filter, param);
                            var resExp = Expression.Call(typeof(Queryable), "Where", new[] { typeof(TTable) }, query.Expression, lambda);
                            return query.Provider.CreateQuery(resExp);
                        }
                        return query;
                    };
            return handler;
        }

        public static ExpressionFilterHandler GetReferenceExpressionFilterHandler<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {

            ExpressionFilterHandler handler =
                delegate(Enum type, string value1, string value2, QueryParameters @params)
                    {
                        ParameterExpression paramChild;
                        Expression filter;
                        InvocationExpression expChilds;
                        ParameterExpression param = 
                            GetRefFilterParams(expFromTableToChild, expGetField, out paramChild, type, value1, out filter, out expChilds);
                        if (filter != null)
                        {
                            var lambda = Expression.Lambda(filter, paramChild);
                            var firstItem = Expression.Call(typeof(Queryable), "FirstOrDefault",
                                                            new[] { typeof(TChildTable) }, expChilds, lambda);
                            filter = Expression.NotEqual(firstItem, Expression.Constant(null, typeof(TChildTable)));
                            lambda = Expression.Lambda(filter, param);
                            return lambda;
                        }
                        return null;
                    };
            return handler;
        }

        #region Filter params

        private static ParameterExpression GetNumericFilterParams<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField, out ParameterExpression paramChild, Enum type, string value1, string value2, out Expression filter, out InvocationExpression expChilds)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {
            var param = Expression.Parameter(typeof(TTable), "mt");
            expChilds = Expression.Invoke(expFromTableToChild, param);
            paramChild = Expression.Parameter(typeof(TChildTable), "cht");
            var expField = Expression.Invoke(expGetField, paramChild);
            filter = null;
            var filterType = (DefaultFilters.NumericFilter)type;
            ConstantExpression expValue = null;
            if (!string.IsNullOrEmpty(value1))
            {
                var value = (TTypeData)Convert.ChangeType(value1, typeof(TTypeData));
                expValue = Expression.Constant(value, typeof(TTypeData?));
            }
            switch (filterType)
            {
                case DefaultFilters.NumericFilter.Non:
                    break;
                case DefaultFilters.NumericFilter.Equals:
                    if (expValue != null)
                        filter = Expression.Equal(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.NotEquals:
                    if (expValue != null)
                        filter = Expression.NotEqual(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.IsNotNull:
                    expValue = Expression.Constant(null, typeof(TTypeData?));
                    filter = Expression.NotEqual(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.IsNull:
                    expValue = Expression.Constant(null, typeof(TTypeData?));
                    filter = Expression.Equal(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.More:
                    if (expValue != null)
                        filter = Expression.GreaterThan(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.Less:
                    if (expValue != null)
                        filter = Expression.LessThan(expField, expValue);
                    break;
                case DefaultFilters.NumericFilter.Between:
                    if (!string.IsNullOrEmpty(value2) && expValue != null)
                    {
                        var value = (TTypeData)Convert.ChangeType(value2, typeof(TTypeData));
                        var expValue2 = Expression.Constant(value);
                        filter = Expression.GreaterThanOrEqual(expField, expValue);
                        filter = Expression.And(filter, Expression.LessThanOrEqual(expField, expValue2));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return param;
        }
        
        private static ParameterExpression GetTextFilterParams<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData>> expGetField, out ParameterExpression paramChild, Enum type, string value1, out Expression filter, out InvocationExpression expChilds)
            where TTable : class
            where TChildTable : class
           // where TTypeData : struct
        {
            var param = Expression.Parameter(typeof(TTable), "mt");
            expChilds = Expression.Invoke(expFromTableToChild, param);
            paramChild = Expression.Parameter(typeof(TChildTable), "cht");
            var expField = Expression.Invoke(expGetField, paramChild);
            filter = null;
            var filterType = (DefaultFilters.TextFilter)type;
            ConstantExpression expValue = null;
            if (!string.IsNullOrEmpty(value1))
            {
                var value = (TTypeData)Convert.ChangeType(value1, typeof(TTypeData));
                expValue = Expression.Constant(value, typeof(TTypeData));
            }
            switch (filterType)
            {
                case DefaultFilters.TextFilter.Non:
                    break;
                case DefaultFilters.TextFilter.Equals:
                    if (expValue != null)
                        filter = Expression.Equal(expField, expValue);
                    break;
                case DefaultFilters.TextFilter.NotEquals:
                    if (expValue != null)
                        filter = Expression.NotEqual(expField, expValue);
                    break;
                case DefaultFilters.TextFilter.IsNotNull:
                    expValue = Expression.Constant(null, typeof(TTypeData));
                    filter = Expression.NotEqual(expField, expValue);
                    break;
                case DefaultFilters.TextFilter.IsNull:
                    expValue = Expression.Constant(null, typeof(TTypeData));
                    filter = Expression.Equal(expField, expValue);
                    break;
                case DefaultFilters.TextFilter.StartsWith:
                    if (expValue != null)
                        filter = Expression.Call(expField, "StartsWith", new Type[] { }, expValue);
                    break;
                case DefaultFilters.TextFilter.Contains:
                    if (expValue != null)
                        filter = Expression.Call(expField, "Contains", new Type[] { }, expValue);
                    break;
                case DefaultFilters.TextFilter.EndsWith:
                    if (expValue != null)
                        filter = Expression.Call(expField, "EndsWith", new Type[] { }, expValue);
                    break;
                case DefaultFilters.TextFilter.ContainsWords:
                    filter = ContainsWordsExpression(expField, expValue);
                    break;
                case DefaultFilters.TextFilter.ContainsAnyWord:
                    filter = ContainsAnyWordExpression(expField, expValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return param;
        }

        private static ParameterExpression GetRefFilterParams<TTable, TChildTable, TTypeData>(Expression<Func<TTable, IQueryable<TChildTable>>> expFromTableToChild, Expression<Func<TChildTable, TTypeData?>> expGetField, out ParameterExpression paramChild, Enum type, string value1, out Expression filter, out InvocationExpression expChilds)
            where TTable : class
            where TChildTable : class
            where TTypeData : struct
        {
            var param = Expression.Parameter(typeof(TTable), "mt");
            expChilds = Expression.Invoke(expFromTableToChild, param);
            paramChild = Expression.Parameter(typeof(TChildTable), "cht");

            var expField = Expression.Invoke(expGetField, paramChild);

            filter = null;
            var filterType = (DefaultFilters.ReferenceFilter)type;
            ConstantExpression expValue = null;
            if (!string.IsNullOrEmpty(value1))
            {
                if (typeof(TTypeData) == typeof(long) && value1.Contains(","))
                {
                    var collection = GetValueCollection(value1);
                    expValue = Expression.Constant(collection);
                }
                else
                {
                    var value = (TTypeData)Convert.ChangeType(value1, typeof(TTypeData));
                    expValue = Expression.Constant(value, typeof(TTypeData?));
                }
            }
            switch (filterType)
            {
                case DefaultFilters.ReferenceFilter.Non:
                    break;
                case DefaultFilters.ReferenceFilter.Equals:
                    if (expValue != null)
                    {
                        if (typeof(TTypeData) == typeof(long) && value1.Contains(","))
                            filter = Expression.Call(expValue, "Contains", new Type[] { }, expField);
                        else
                            filter = Expression.Equal(expField, expValue);
                    }
                    break;
                case DefaultFilters.ReferenceFilter.NotEquals:
                    if (expValue != null)
                    {
                        if (typeof(TTypeData) == typeof(long) && value1.Contains(","))
                            filter = Expression.Not(Expression.Call(expValue, "Contains", new Type[] { }, expField));
                        else
                            filter = Expression.NotEqual(expField, expValue);

                    }
                    break;
                case DefaultFilters.ReferenceFilter.IsNotNull:
                    expValue = Expression.Constant(null, typeof(TTypeData?));
                    filter = Expression.NotEqual(expField, expValue);
                    break;
                case DefaultFilters.ReferenceFilter.IsNull:
                    expValue = Expression.Constant(null, typeof(TTypeData?));
                    filter = Expression.Equal(expField, expValue);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsByRef:
                    break;
                case DefaultFilters.ReferenceFilter.StartsWithByRef:
                    if (expValue != null)
                        filter = Expression.Call(expField, "StartsWith", new Type[] { }, expValue);
                    break;
                case DefaultFilters.ReferenceFilter.EndsWithByRef:
                    if (expValue != null)
                        filter = Expression.Call(expField, "EndsWith", new Type[] { }, expValue);
                    break;
                case DefaultFilters.ReferenceFilter.ContainsWordsByRef:
                    break;
                case DefaultFilters.ReferenceFilter.ContainsAnyWordByRef:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return param;
        }

        private static Expression ContainsWordsExpression(InvocationExpression field, ConstantExpression strValue)
        {
            Expression filter = null;

            var split = strValue.Value.ToString().Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                var right = Expression.Constant(Convert.ChangeType(split[i], field.Type));
                filter = filter == null
                             ? (Expression)Expression.Call(field, "Contains", new Type[] { }, right)
                             : Expression.And(filter, Expression.Call(field, "Contains", new Type[] { }, right));
            }
            return filter;
        }

        private static Expression ContainsAnyWordExpression(InvocationExpression field, ConstantExpression strValue)
        {
            Expression filter = null;

            var split = strValue.Value.ToString().Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < split.Length; i++)
            {
                var right = Expression.Constant(Convert.ChangeType(split[i], field.Type));
                filter = filter == null
                             ? (Expression)Expression.Call(field, "Contains", new Type[] { }, right)
                             : Expression.Or(filter, Expression.Call(field, "Contains", new Type[] { }, right));
            }
            return filter;
        }

        #endregion

        #endregion

        public static bool IsNullableType(Type type)
        {
            if (!type.IsGenericType) return false;
            var definition = type.GetGenericTypeDefinition();
            if (definition == null) return false;
            return definition.IsAssignableFrom(typeof (Nullable<>));
        }
    }
}