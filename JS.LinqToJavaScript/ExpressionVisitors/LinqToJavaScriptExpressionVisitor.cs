namespace JS.LinqToJavaScript.ExpressionVisitors
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Script.Serialization;

    using LinqToJavaScript.Attributes;
    using LinqToJavaScript.ExpressionVisitorsArgs;

    internal class LinqToJavaScriptExpressionVisitor : ExpressionVisitor
    {
        #region Constants

        private const string Dot = ".";
        private const string Tab1 = "    ";
        private const string Tab2 = "        ";
        private const string ThisDot = "this.";
        private const string This = "this";

        #endregion

        #region Fields

        private readonly Dictionary<Type, object> javaScriptChildrenClasses = new Dictionary<Type, object>();

        private JavaScriptClassAttribute javaScriptClassAttribute;
        
        private object javaScriptClassValue;

        private StringBuilder resultScript;

        private string context = This;

        #endregion

        #region Methods

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            resultScript.Append("(");
            Expression test = Visit(c.Test);
            resultScript.Append(" ? ");
            Expression ifTrue = Visit(c.IfTrue);
            resultScript.Append(" : ");
            Expression ifFalse = Visit(c.IfFalse);
            resultScript.Append(")");
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
                return Expression.Condition(test, ifTrue, ifFalse);
            return c;
        }

        internal string Translate(Expression expression)
        {
            resultScript = new StringBuilder();
            Visit(expression);
            return resultScript.ToString();
        }

        protected virtual void DeclareClassScript(string methodName)
        {
            if (javaScriptClassValue == null)
            {
                throw new NotSupportedException(
                    string.Format("The method '{0}' is not supported. _javaScriptClassValue == null", methodName));
            }

            var properties = GetJavaScriptProperties(javaScriptClassValue, javaScriptChildrenClasses);
            var methods = GetJavaScriptMethods(javaScriptClassValue);

            resultScript.Append("Type.registerNamespace('").Append(javaScriptClassAttribute.Namespace).AppendLine("');");

            resultScript.AppendLine();

            resultScript.Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .AppendLine(" = function() {");

            resultScript.Append(Tab1)
               .Append(javaScriptClassAttribute.Namespace)
               .Append(Dot)
               .Append(javaScriptClassAttribute.ClassName)
               .AppendLine(".initializeBase(this);");

            if (properties.Count > 0)
            {
                resultScript.AppendLine();

                SetJavaScriptValuesToNull(properties);
            }

            resultScript.AppendLine("}");
            resultScript.AppendLine();

            resultScript.Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .AppendLine(".prototype = {");

            resultScript.Append(Tab1).AppendLine("initialize: function() {");

            resultScript.Append(Tab2)
               .Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .AppendLine(".callBaseMethod(this, 'initialize');");

            resultScript.Append(Tab1).AppendLine("},");

            resultScript.AppendLine();

            resultScript.Append(Tab1).AppendLine("dispose: function() {");

            resultScript.Append(Tab2)
               .Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .AppendLine(".callBaseMethod(this, 'dispose');");

            if (properties.Count > 0 || methods.Count > 0)
                resultScript.Append(Tab1).AppendLine("},").AppendLine();
            else
                resultScript.Append(Tab1).AppendLine("}");

            AddJavaScriptProperties(properties, methods.Count > 0);
            AddJavaScriptMethods(methods, false);

            resultScript.AppendLine("}");

            resultScript.AppendLine();

            resultScript.Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .Append(".registerClass('")
               .Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .Append("', ")
               .Append(javaScriptClassAttribute.BaseClassName ?? "Sys.Component")
               .Append(");");
        }

        protected virtual List<JavaScriptMethodArgs> GetJavaScriptMethods(object obj)
        {
            return new List<JavaScriptMethodArgs>();
        }

        protected internal static List<JavaScriptPropertyArgs> GetJavaScriptProperties(object obj, Dictionary<Type, object> javaScriptChildrenClasses)
        {
            var properties = new List<JavaScriptPropertyArgs>();
            var descriptors = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor descriptor in descriptors)
            {
                var propertyAttribute = descriptor.Attributes.OfType<JavaScriptPropertyAttribute>().SingleOrDefault();
                if (propertyAttribute == null)
                    continue;

                var propertyTypeAttributes = TypeDescriptor.GetAttributes(descriptor.PropertyType);
                var childScript = propertyTypeAttributes != null
                                      ? propertyTypeAttributes.OfType<JavaScriptClassAttribute>().SingleOrDefault()
                                      : null;

                if (childScript != null && !javaScriptChildrenClasses.ContainsKey(descriptor.PropertyType))
                    javaScriptChildrenClasses[descriptor.PropertyType] = descriptor.GetValue(obj);

                var propertyName = string.IsNullOrEmpty(propertyAttribute.PropertyName)
                                       ? (descriptor.Name.Substring(0, 1).ToLower() + descriptor.Name.Substring(1))
                                       : propertyAttribute.PropertyName;
                var privateFieldName = string.IsNullOrEmpty(propertyAttribute.PrivateFieldName)
                                           ? "_" + propertyName
                                           : propertyAttribute.PrivateFieldName;

                var expressionGetProperty =
                    string.IsNullOrEmpty(propertyAttribute.ExpressionGetProperty)
                        ? null
                        : (Expression)descriptors[propertyAttribute.ExpressionGetProperty].GetValue(obj);

                var expressionSetProperty =
                    string.IsNullOrEmpty(propertyAttribute.ExpressionSetProperty)
                        ? null
                        : (Expression)descriptors[propertyAttribute.ExpressionSetProperty].GetValue(obj);

                properties.Add(
                    new JavaScriptPropertyArgs
                        {
                            Object = obj,
                            ExpressionGetProperty = expressionGetProperty,
                            ExpressionSetProperty = expressionSetProperty,
                            PropertyName = propertyName,
                            PrivateFieldName = privateFieldName,
                            ChildObject = childScript == null ? null : descriptor.GetValue(obj),
                            JavaScriptClassAttribute = childScript,
                            PropertyDescriptor = descriptor,
                            AddPropertyToClass = !propertyAttribute.DeclaredInBaseClass,
                            ContextProperty = string.IsNullOrEmpty(propertyAttribute.ContextOfGetProperty) ? This : propertyAttribute.ContextOfGetProperty,
                            ReadOnly = propertyAttribute.ReadOnly,
                        });
            }

            return properties.OrderBy(r => r.PropertyName).ToList();
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            resultScript.Append("(");
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    resultScript.Append(" & ");
                    break;
                case ExpressionType.AndAlso:
                    resultScript.Append(" && ");
                    break;
                case ExpressionType.Or:
                    resultScript.Append(" | ");
                    break;
                case ExpressionType.OrElse:
                    resultScript.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    resultScript.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    resultScript.Append(" != ");
                    break;
                case ExpressionType.LessThan:
                    resultScript.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    resultScript.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    resultScript.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    resultScript.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            Visit(b.Right);
            resultScript.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
            {
                resultScript.Append("null");
                return c;
            }
            
            var type = c.Value.GetType();
            switch (Type.GetTypeCode(type))
            {
                    // todo: сделать обработку других типов (классы, DateTime)
                case TypeCode.Boolean:
                    resultScript.Append(c.Value.ToString().ToLower());
                    break;
                case TypeCode.String:
                    resultScript.Append("'");
                    resultScript.Append(((string)c.Value).Replace("'", @"\'"));
                    resultScript.Append("'");
                    break;
                case TypeCode.DateTime:
                    resultScript.Append("new Date");
                    resultScript.Append(((DateTime)c.Value).ToString("(yyyy, M, d, H, m, s)"));
                    break;
                case TypeCode.Object:
                    var enumerable = c.Value as IEnumerable;

                    if (enumerable != null)
                    {
                        resultScript.Append("[");
                        foreach (var item in enumerable)
                        {
                            VisitConstant(Expression.Constant(item));
                            resultScript.Append(",");
                        }

                        if (resultScript.Length == 1)
                            resultScript.Append(']');
                        else
                            resultScript[resultScript.Length - 1] = ']';

                        return c;
                    }

                    var attr = type.GetCustomAttributes(typeof(JavaScriptClassAttribute), true)
                                   .Cast<JavaScriptClassAttribute>()
                                   .SingleOrDefault();
                    if (attr != null)
                    {
                        if (c.Value == null)
                        {
                            throw new NotSupportedException(
                                string.Format("The null constant for '{0}' is not supported", c.Value));
                        }

                        if (javaScriptClassValue == c.Value)
                        {
                            resultScript.Append(This);
                        }
                        else if (javaScriptClassValue != null)
                        {
                            throw new NotSupportedException(
                                string.Format("javaScriptClassValue can't be set twice. Second value: {0}", c.Value));
                        }
                        else
                        {
                            javaScriptClassAttribute = attr;
                            javaScriptClassValue = c.Value;
                        }

                        return c;
                    }

                    var regex = c.Value as Regex;
                    if (regex != null)
                    {
                        resultScript.Append('/')
                            .Append(regex)
                            .Append("/gi");
                        return c;                             
                    }
                    /*
                    if (c.Type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
                    {
                        var value = Expression.Lambda(c).Compile().DynamicInvoke();
                        return Visit(Expression.Constant(value));
                    }
                    */
                    throw new NotSupportedException(
                        string.Format("The constant for '{0}' is not supported", c.Value));
                default:
                    resultScript.Append(c.Value);
                    break;
            }

            return c;
        }

        protected override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            if (original.Count == 0)
            {
                resultScript.Append("[");
                var readOnlyList = base.VisitExpressionList(original);
                resultScript.Append("]");
                return readOnlyList;
            }

            List<Expression> list = null;
            if (original.Count == 1 && original[0].NodeType == ExpressionType.NewArrayInit)
                return base.VisitExpressionList(original);

            if (original.Count == 1 && original[0].NodeType == ExpressionType.Constant)
            {
                Expression p = Visit(original[0]);
                list = new List<Expression>(1);
                list.Add(p);
                return list.AsReadOnly();
            }

            resultScript.Append("[");
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = Visit(original[i]);
                resultScript.Append(",");
                if (list != null)
                    list.Add(p);
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }

                    list.Add(p);
                }
            }

            resultScript[resultScript.Length - 1] = ']';
            if (list != null)
                return list.AsReadOnly();
            return original;
        }

        protected override Expression VisitInvocation(InvocationExpression iv)
        {
            if (javaScriptClassValue == null)
                return base.VisitInvocation(iv);

            resultScript.Append("(");
            Visit(iv.Expression);
            resultScript.Append(").call(this, ");
            for (int i = 0; i < iv.Arguments.Count; i++)
            {
                if (i > 0) resultScript.Append(", ");
                Visit(iv.Arguments[i]);
            }

            resultScript.Append(")");
            return iv;
        }

        protected override Expression VisitLambda(LambdaExpression lambda)
        {
            if (context == null)
            {
                resultScript.Append("function(");

                for (int i = 0; i < lambda.Parameters.Count; i++)
                {
                    if (i > 0) resultScript.Append(", ");
                    resultScript.Append(lambda.Parameters[i].Name);
                }

                resultScript.AppendLine(") {");
                resultScript.Append(Tab2).Append("return ");
                Visit(lambda.Body);
                resultScript.AppendLine(";").Append(Tab1).Append("}");

                return lambda;
            }

            return base.VisitLambda(lambda);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (context == null)
                resultScript.Append(p.Name);
            else
                resultScript.Append(context);

            return p;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            var expression = m.Expression;
            if (expression != null)
            {
                if (expression.NodeType == ExpressionType.Convert)
                {
                    var operand = ((UnaryExpression)expression).Operand;
                    if (operand.NodeType == ExpressionType.Parameter || operand.NodeType == ExpressionType.MemberAccess)
                        expression = operand;
                }

                if (expression.NodeType == ExpressionType.Parameter || expression.NodeType == ExpressionType.MemberAccess)
                {
                    Visit(expression);

                    if (m.Member.DeclaringType == typeof(bool?))
                        return m;

                        resultScript.Append(".");
                }
                else if (expression != null && expression.NodeType == ExpressionType.Constant)
                {
                    var value = Expression.Lambda(m).Compile().DynamicInvoke();
                    return VisitConstant(Expression.Constant(value));
                }
            }

            if (m.Member.DeclaringType == typeof(string) && "Empty".Equals(m.Member.Name))
            {
                return VisitConstant(Expression.Constant(string.Empty));
            }

            if (m.Member.DeclaringType == typeof(string) && "Length".Equals(m.Member.Name))
            {
                resultScript.Append("length");
                return m;
            }

            var attrs = m.Member
                .GetCustomAttributes(typeof(JavaScriptPropertyAttribute), false)
                .OfType<JavaScriptPropertyAttribute>()
                .SingleOrDefault();
            if (attrs == null)
            {
                resultScript.Append(m.Member.Name);
            }
            else
            {
                resultScript.Append("get_");
                if (string.IsNullOrEmpty(attrs.PropertyName))
                    resultScript.Append(m.Member.Name.Substring(0, 1).ToLower() + m.Member.Name.Substring(1));
                else
                    resultScript.Append(attrs.PropertyName);
                resultScript.Append("()");
            }

            return m;
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Enumerable) && m.Method.Name == "Contains")
            {
                // $.inArray(5, [1,2,3])
                resultScript.Append("($.inArray(");
                Visit(m.Arguments[1]);
                resultScript.Append(", ");
                Visit(m.Arguments[0]);
                resultScript.Append(") > -1)");
                return m;
            }

            if (typeof(IEnumerable).IsAssignableFrom(m.Method.DeclaringType) && m.Method.Name == "Contains")
            {
                // $.inArray(5, [1,2,3])
                resultScript.Append("($.inArray(");
                Visit(m.Arguments[0]);
                resultScript.Append(", ");
                Visit(m.Object);
                resultScript.Append(") > -1)");
                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "JQueryGetValue")
            {
                Visit(m.Arguments[0]);

                // todo: добавить условие для различия ExtNet и JQuery
                // resultScript.Append(".val()");
                resultScript.Append(".getValue()");
                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "JQueryFindById")
            {
                // todo: добавить условие для различия ExtNet и JQuery
                /*resultScript.Append("$('#' + ");
                Visit(m.Arguments[0]);
                resultScript.Append(")");*/
                resultScript.Append("eval(");
                Visit(m.Arguments[0]);
                resultScript.Append(")");
                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "JQueryFind")
            {
                // todo: добавить условие для различия ExtNet и JQuery
                /*resultScript.Append("$(");
                Visit(m.Arguments[0]);
                resultScript.Append(")");*/
                resultScript.Append("eval(");
                Visit(m.Arguments[0]);
                resultScript.Append(")");
                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "DeclareClassScript")
            {
                DeclareClassScript(m.Method.Name);
                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "DeclareAllClassScript")
            {
                DeclareClassScript(m.Method.Name);
                foreach (var childrenClass in javaScriptChildrenClasses)
                {
                    resultScript.AppendLine().AppendLine();
                    javaScriptClassValue = childrenClass.Value;
                    javaScriptClassAttribute =
                        TypeDescriptor.GetAttributes(childrenClass.Key).OfType<JavaScriptClassAttribute>().Single();
                    DeclareClassScript(m.Method.Name);
                }

                return m;
            }

            if (typeof(LinqToJavaScriptExtender).IsAssignableFrom(m.Method.DeclaringType)
                && m.Method.Name == "CreateClassScript")
            {
                CreateClassScript(m);
                return m;
            }

            if (m.Method.DeclaringType == typeof(Regex) && m.Method.Name == "IsMatch")
            {
                // $.inArray(5, [1,2,3])
                Visit(m.Arguments[0]);
                resultScript.Append(".match(");
                Visit(m.Object);
                resultScript.Append(")");
                return m;
            }

            if (m.Method.DeclaringType == typeof(string) && m.Method.Name == "IsNullOrEmpty")
            {
                resultScript.Append("(");
                Visit(m.Arguments[0]);
                resultScript.Append(" == null || ");
                Visit(m.Arguments[0]);
                resultScript.Append(" == '')");
                return m;
            }

            var attr = m.Method.GetCustomAttributes(typeof(JavaScriptFunctionAttribute), true).OfType<JavaScriptFunctionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                var fnName = string.IsNullOrEmpty(attr.FunctionName) ? m.Method.Name[0].ToString().ToLower() + m.Method.Name.Substring(1) : attr.FunctionName;
                Visit(m.Object);
                resultScript.Append(Dot).Append(fnName).Append("(");
                foreach (var args in m.Arguments)
                    Visit(args);
                resultScript.Append(")");
                return m;
            }

            if (m.Method.Name == "Equals")
            {
                resultScript.Append("(");
                Visit(m.Object);
                resultScript.Append(" == ");
                Visit(m.Arguments[0]);
                resultScript.Append(")");
                return m;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(init.Bindings);
            resultScript.Append(" })");
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            var attr = nex.Type.GetCustomAttributes(typeof(JavaScriptClassAttribute), true).OfType<JavaScriptClassAttribute>().FirstOrDefault();
            if (attr == null)
            {
                if (typeof(IEnumerable).IsAssignableFrom(nex.Type))
                    VisitExpressionList(nex.Arguments);
                else
                    resultScript.Append("({ ");
            }
            else
            {
                resultScript.Append("$create(").Append(attr.Namespace).Append(Dot).Append(attr.ClassName).Append(", { ");
            }

            return nex;
        }

        protected override IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (i > 0) resultScript.Append(", ");

                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null)
                    list.Add(b);
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                        list.Add(original[j]);
                    list.Add(b);
                }
            }

            if (list != null)
                return list;
            return original;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            var memberAttr = assignment.Member.GetCustomAttributes(typeof(JavaScriptPropertyAttribute), false).OfType<JavaScriptPropertyAttribute>().FirstOrDefault();
            if (memberAttr == null)
            {
                var attr = assignment.Member.DeclaringType == null
                               ? null
                               : assignment.Member.DeclaringType.GetCustomAttributes(typeof(JavaScriptClassAttribute), true)
                                     .OfType<JavaScriptClassAttribute>()
                                     .FirstOrDefault();
                if (attr == null)
                    resultScript.Append(assignment.Member.Name);
                else
                    resultScript.Append(assignment.Member.Name.Substring(0, 1).ToLower() + assignment.Member.Name.Substring(1));
            }
            else if (memberAttr.PropertyName != null)
                resultScript.Append(memberAttr.PropertyName);
            else
                resultScript.Append(assignment.Member.Name.Substring(0, 1).ToLower() + assignment.Member.Name.Substring(1));

            resultScript.Append(": ");

            Visit(assignment.Expression);
            return assignment;
        }

        private void CreateClassScript(MethodCallExpression m)
        {
            if (javaScriptClassValue == null)
            {
                throw new NotSupportedException(
                    string.Format("The method '{0}' is not supported. _javaScriptClassValue == null", m.Method.Name));
            }

            var childrenClasses = new Dictionary<Type, object>();
            childrenClasses[javaScriptClassValue.GetType()] = javaScriptClassValue;
            GetJavaScriptProperties(javaScriptClassValue, childrenClasses);
            resultScript.Append(Tab1)
               .Append("$create(")
               .Append(javaScriptClassAttribute.Namespace).Append(Dot).Append(javaScriptClassAttribute.ClassName)
               .Append(", ");
            var ser = new JavaScriptSerializer();

            ser.RegisterConverters(
                new[]
                    {
                        new LinqToJavaScriptConvert
                            {
                                JavaScriptChildrenClasses = childrenClasses,
                            },
                    });
            ser.Serialize(javaScriptClassValue, resultScript);
            resultScript.AppendLine(");");

            // todo: убрать костыль. нужен другой сериализатор
            foreach (var childrenClass in childrenClasses)
            {
                var attr = childrenClass.Key.GetCustomAttributes(typeof(JavaScriptClassAttribute), true)
                             .Cast<JavaScriptClassAttribute>()
                             .Single();
                resultScript.Replace("__" + attr.ClassName + "__\":\"", "\":");
                resultScript.Replace("\",\"" + attr.ClassName + "__\":", string.Empty);
                resultScript.Replace(",\"" + attr.ClassName + "____\":\"\"", ")");
            }
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                // todo: сделать другие унарные операции
                case ExpressionType.Not:
                    resultScript.Append("!");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    if (u.IsLiftedToNull || u.Type == typeof(object) || true)
                        Visit(u.Operand);
                    else
                    {
                        throw new NotSupportedException(
                            string.Format(
                                "The unary operator '{0}' is not supported if IsLiftedToNull is false", u.NodeType));
                    }

                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        private static Expression StripQuotes(Expression e)
        {
            // note: для чего?
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private void AddJavaScriptMethods(List<JavaScriptMethodArgs> methods, bool finishWithComma)
        {
            if (methods.Count == 0)
                return;

            throw new NotImplementedException();
        }

        private void AddJavaScriptProperties(List<JavaScriptPropertyArgs> properties, bool finishWithComma)
        {
            properties = properties.Where(r => r.AddPropertyToClass).ToList();
            if (properties.Count == 0)
                return;

            for (int i = 0; i < properties.Count; i++)
            {
                var prop = properties[i];
                resultScript.Append(Tab1).Append("get_").Append(prop.PropertyName).AppendLine(": function() {");
                if (prop.ExpressionGetProperty == null)
                    resultScript.Append(Tab2).Append("return ").Append(ThisDot).Append(prop.PrivateFieldName).AppendLine(";");
                else
                {
                    var oldContext = context;
                    context = prop.ContextProperty;
                    if (prop.PropertyDescriptor.PropertyType == typeof(bool) || prop.PropertyDescriptor.PropertyType == typeof(bool?))
                    {
                        resultScript.Append(Tab2).Append("var resVal = ");
                        Visit(prop.ExpressionGetProperty);
                        resultScript.AppendLine(";");
                        resultScript.Append(Tab2).AppendLine("resVal = (typeof resVal === 'string' || resVal instanceof String) ? resVal.toLowerCase() === 'true' : resVal;");
                        resultScript.Append(Tab2).AppendLine("return resVal;");
                    }
                    else
                    {
                        resultScript.Append(Tab2).Append("return ");
                        Visit(prop.ExpressionGetProperty);
                        resultScript.AppendLine(";");
                    }

                    context = oldContext;
                }

                if (!prop.ReadOnly && (prop.ExpressionGetProperty == null || prop.ExpressionSetProperty != null))
                {
                    if (prop.ExpressionSetProperty != null)
                        throw new NotSupportedException();

                    resultScript.Append(Tab1).AppendLine("},");

                    resultScript.AppendLine();

                    resultScript.Append(Tab1).Append("set_").Append(prop.PropertyName).AppendLine(": function(value) {");
                    resultScript.Append(Tab2).Append(ThisDot).Append(prop.PrivateFieldName).AppendLine(" = value;");
                }

                if (i + 1 < properties.Count || finishWithComma)
                    resultScript.Append(Tab1).AppendLine("},").AppendLine();
                else
                    resultScript.Append(Tab1).AppendLine("}");
            }
        }

        private void SetJavaScriptValuesToNull(IEnumerable<JavaScriptPropertyArgs> properties)
        {
            var data = properties
                .Where(r => r.AddPropertyToClass)
                .Where(r => r.ExpressionGetProperty == null || r.ExpressionSetProperty != null);
            foreach (var property in data)
            {
                if (property.ExpressionGetProperty == null && typeof(LambdaExpression).IsAssignableFrom(property.PropertyDescriptor.PropertyType))
                {
                    var exp = (LambdaExpression)property.PropertyDescriptor.GetValue(property.Object);
                    if (exp != null)
                    {
                        context = null;
                        resultScript.Append(Tab1).Append(ThisDot).Append(property.PrivateFieldName).Append(" = ");
                        Visit(exp);
                        resultScript.AppendLine(";");
                        context = This;
                        continue;
                    }
                }
                else if (property.ExpressionGetProperty == null
                         && property.PropertyDescriptor.PropertyType.IsArray
                         && typeof(LambdaExpression).IsAssignableFrom(property.PropertyDescriptor.PropertyType.GetElementType()))
                {
                    var exps = (LambdaExpression[])property.PropertyDescriptor.GetValue(property.Object);
                    if (exps != null && exps.Length > 0)
                    {
                        context = null;
                        resultScript.Append(Tab1).Append(ThisDot).Append(property.PrivateFieldName).Append(" = [");

                        for (int i = 0; i < exps.Length; i++)
                        {
                            if (i > 0)
                                resultScript.Append(", ");
                            Visit(exps[i]);
                        }

                        resultScript.AppendLine("];");
                        context = This;
                        continue;
                    }
                }
                    
                resultScript.Append(Tab1).Append(ThisDot).Append(property.PrivateFieldName).AppendLine(" = null;");
            }
        }

        #endregion
    }
}