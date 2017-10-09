namespace JS.LinqToJavaScript.ExpressionVisitorsArgs
{
    using System.ComponentModel;
    using System.Linq.Expressions;

    using JS.LinqToJavaScript.Attributes;

    public class JavaScriptPropertyArgs
    {
        #region Public Properties

        public Expression ExpressionGetProperty { get; set; }

        public Expression ExpressionSetProperty { get; set; }

        public string PropertyName { get; set; }

        public string PrivateFieldName { get; set; }

        public object ChildObject { get; set; }

        public PropertyDescriptor PropertyDescriptor { get; set; }

        public bool AddPropertyToClass { get; set; }

        public string ContextProperty { get; set; }

        public JavaScriptClassAttribute JavaScriptClassAttribute { get; set; }

        public object Object { get; set; }

        public bool ReadOnly { get; set; }

        #endregion
    }
}