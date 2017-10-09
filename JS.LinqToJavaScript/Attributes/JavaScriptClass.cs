namespace JS.LinqToJavaScript.Attributes
{
    using System;

    public class JavaScriptClassAttribute : Attribute
    {
        #region Public Properties

        public string BaseClassName { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }

        #endregion
    }
}