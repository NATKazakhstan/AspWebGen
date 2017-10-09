namespace JS.LinqToJavaScript.Attributes
{
    using System;

    public class JavaScriptPropertyAttribute : Attribute
    {
        #region Public Properties

        public string ExpressionGetProperty { get; set; }

        public string ExpressionSetProperty { get; set; }

        public string PropertyName { get; set; }

        public string PrivateFieldName { get; set; }

        public bool ReadOnly { get; set; }

        public bool WriteOnly { get; set; }

        public bool DeclaredInBaseClass { get; set; }

        public string ContextOfGetProperty { get; set; }

        #endregion
    }
}