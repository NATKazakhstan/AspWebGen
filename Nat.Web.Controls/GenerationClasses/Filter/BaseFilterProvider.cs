/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 14 июля 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace Nat.Web.Controls.GenerationClasses
{
    public abstract class BaseFilterProvider : IFilterProvider
    {
        private static readonly Dictionary<Type, Dictionary<Type, Expression>> TypeExpressions = new Dictionary<Type, Dictionary<Type, Expression>>();
        private Type _type;
        private ParameterExpression _param;

        public MainPageUrlBuilder Url { get; set; }
        public bool IsSelect { get; set; }
        public string ProjectName { get; set; }

        public virtual void SetFilters(ref IQueryable enumerable)
        {
            if (!TypeExpressions.ContainsKey(_type)) return;
            var expression = TypeExpressions[_type];
            if (expression.Count == 0) return;
            SetFilters(enumerable, expression);
        }

        public abstract void SetFilters(IQueryable enumerable, IDictionary<Type, Expression> expressions);

        public virtual void Init(Type tableType)
        {
            if (_type != null) return;
            _param = Expression.Parameter(tableType, "c");
            _type = tableType;
            lock (TypeExpressions)
                if (!TypeExpressions.ContainsKey(tableType))
                {
                    var doc = SelectedParameterNavigator.GetDocumnet(ProjectName ?? "");
                    var pairs = new Dictionary<Type, Expression>();
                    CreateExpression(doc, tableType, _param, pairs);
                    TypeExpressions.Add(tableType, pairs);
                }
        }

        private static void CreateExpression(XContainer doc, Type tableType, Expression expression, IDictionary<Type, Expression> expressions)
        {
            var element = doc.Element("data").Elements().FirstOrDefault(e => e.Attribute("TableType").Value == tableType.Name);
            if (element == null)
                throw new Exception("Не найдена таблица '" + tableType.Name + "' для построения связи (DataInformation_*).");

            var parentReferences = element.Attribute("ParentReferences").Value;
            var isMain = Convert.ToBoolean(element.Attribute("IsMain").Value);
            expressions[tableType] = expression;
            if (isMain || string.IsNullOrEmpty(parentReferences)) return;
            foreach (var parent in parentReferences.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var property = tableType.GetProperty(parent);
                if (property != null && property.PropertyType != tableType)
                {
                    Expression exp = Expression.Property(expression, property);
                    CreateExpression(doc, property.PropertyType, exp, expressions);
                }
            }
        }
    }
}